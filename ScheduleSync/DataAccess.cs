using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ScheduleSync
{
    public class DataAccess
    {
        public Exception exception;
        StorageFolder tempFolder = ApplicationData.Current.LocalFolder;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public async Task<bool> GetSchedule()
        {
            try
            {
                StorageFile scheduleZip = await tempFolder.CreateFileAsync("schedule.gz", CreationCollisionOption.ReplaceExisting);
                WebClient wc = new WebClient();
                await wc.DownloadFileTaskAsync("https://s3-ap-southeast-1.amazonaws.com/open-ws/weektimetable", scheduleZip.Path);

                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public async Task<bool> ExtractGZip()
        {
            try
            {
                StorageFile scheduleZip = await tempFolder.GetFileAsync("schedule.gz");
                StorageFile jsonFile = await tempFolder.CreateFileAsync("schedule.json", CreationCollisionOption.ReplaceExisting);
                var zipStream = await scheduleZip.OpenStreamForWriteAsync();

                using (var fileStream = await jsonFile.OpenStreamForWriteAsync())
                {
                    using (GZipStream decompressionStream = new GZipStream(zipStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(fileStream);
                    }
                    fileStream.Flush();

                    return true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public async Task<List<Schedule>> ReadAndParseSchedule()
        {
            StorageFile jsonFile = await tempFolder.GetFileAsync("schedule.json");
            string scheduleJson = await File.ReadAllTextAsync(jsonFile.Path);

            // Modifies original JSON so that its item is an array. To make it easier to parse
            string modifiedJson = scheduleJson.Replace("[", "{\"schedules\":[");
            scheduleJson = modifiedJson.Replace("]", "]}");
            
            var result = JsonConvert.DeserializeObject<Root>(scheduleJson);
            

            Schedule lastItem = result.schedules.LastOrDefault();

            if (lastItem != null)
            {
                // Checks and remove old data
                if (localSettings.Values["SyncedUntilDate"] != null)
                {
                    localSettings.Values.Remove(localSettings.Values["SyncedUntilDate"].ToString());
                }

                string syncedUntilDate = lastItem.DATESTAMP_ISO;
                localSettings.Values["SyncedUntilDate"] = syncedUntilDate;
                localSettings.Values[syncedUntilDate] = true;
            }
            else
            {
                localSettings.Values["SyncedUntilDate"] = null;
            }

            localSettings.Values["LastSync"] = DateTime.Now.ToString();

            return result.schedules;
        }

        public async Task<List<Schedule>> FilterTimetablev2(List<Schedule> scheduleList, string intakeCode, string tutGroup, bool isLocalStudent)
        {
            List<Schedule> filteredItems = new List<Schedule>();
            string studentType;

            if (isLocalStudent == true)
            {
                studentType = "(LS)";
            }
            else
            {
                studentType = "(FS)";
            }

            foreach (Schedule item in scheduleList)
            {
                if (item.INTAKE == intakeCode && item.GROUPING == tutGroup)
                {
                    if (item.MODID.Contains("(LS)") || item.MODID.Contains("(FS)"))
                    {
                        if (!item.MODID.Contains(studentType))
                        {
                            continue;
                        }
                    }

                    DateTime dt = new DateTime();
                    DateTime.TryParse(item.DATESTAMP_ISO, out dt);
                    if (dt.DayOfYear >= DateTime.Today.DayOfYear)
                    {
                        filteredItems.Add(item);
                    }
                }
            }

            return filteredItems;
        }

    }
}
