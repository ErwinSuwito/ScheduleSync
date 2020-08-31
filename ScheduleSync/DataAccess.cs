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
using Windows.Storage;
using Windows.Storage.Streams;

namespace ScheduleSync
{
    public class DataAccess
    {
        StorageFile jsonFile;
        StorageFolder tempFolder = ApplicationData.Current.LocalFolder;
        private string scheduleJson;

        private async Task<bool> GetSchedule()
        {
            try
            {
                StorageFile scheduleZip = await tempFolder.CreateFileAsync("schedule.gz", CreationCollisionOption.ReplaceExisting);
                WebClient wc = new WebClient();
                wc.DownloadFile("https://s3-ap-southeast-1.amazonaws.com/open-ws/weektimetable", scheduleZip.Path);

                jsonFile = await tempFolder.CreateFileAsync("schedule.json", CreationCollisionOption.ReplaceExisting);
                var zipStream = await scheduleZip.OpenStreamForWriteAsync();

                using (var fileStream = await jsonFile.OpenStreamForWriteAsync())
                {
                    using (GZipStream decompressionStream = new GZipStream(zipStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(fileStream);
                    }
                    fileStream.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                jsonFile = await tempFolder.GetFileAsync("schedule.json");
                scheduleJson = await File.ReadAllTextAsync(jsonFile.Path);
            }

            // Modifies original JSON so that its item is an array. To make it easier to parse
            string modifiedJson = scheduleJson.Replace("[", "{\"schedules\":[");
            scheduleJson = modifiedJson.Replace("]", "]}");

            return !string.IsNullOrEmpty(scheduleJson);
        }

        private List<Schedule> ParseTimetable()
        {
            var result = JsonConvert.DeserializeObject<Root>(scheduleJson);

            return result.schedules;
        }

        public async Task<List<Schedule>> FilterTimetable(string intakeCode, string tutGroup, bool isLocalStudent)
        {
            bool IsSuccess = await GetSchedule();

            if (!IsSuccess)
            {
                return null;
            }

            List<Schedule> items = ParseTimetable();
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

            foreach (Schedule item in items)
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

                    filteredItems.Add(item);
                }
            }

            return filteredItems;
        }
    }
}
