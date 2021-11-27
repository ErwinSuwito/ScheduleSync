using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ScheduleSync.Data
{
    public class DataAccess
    {
        public Exception Exception { get; private set; }
        StorageFolder tempFolder = ApplicationData.Current.LocalFolder;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        List<string> ignoredModules = new List<string>();

        public DataAccess()
        {
            if (localSettings.Containers.ContainsKey("IgnoredModulesName") == false)
            {
                localSettings.CreateContainer("IgnoredModulesName", ApplicationDataCreateDisposition.Always);
            }

            for (int i = 0; i < localSettings.Containers["IgnoredModulesName"].Values.Count; i++)
            {
                ignoredModules.Add(localSettings.Containers["IgnoredModulesName"].Values.ElementAt(i).Value.ToString());
            }
        }

        private async Task<bool> DownloadSchedule()
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
                this.Exception = ex;
                return false;
            }
        }

        private async Task<bool> ExtractGZip()
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
                this.Exception = ex;
                return false;
            }
        }

        private async Task<bool> GetSchedule()
        {
            bool result = await DownloadSchedule();
            if (result == true)
            {
                result = await ExtractGZip();
            }

            return result;
        }

        private async Task<List<Schedule>> DeserializeSchedule()
        {
            if (!File.Exists(tempFolder.Path + "schedule.json"))
            {
                await GetSchedule();
            }

            StorageFile jsonFile = await tempFolder.GetFileAsync("schedule.json");

            string scheduleJson = await File.ReadAllTextAsync(jsonFile.Path);

            // Modifies original JSON so that its item is an array. To make it easier to parse
            string modifiedJson = scheduleJson.Replace("[", "{\"schedules\":[");
            scheduleJson = modifiedJson.Replace("]", "]}");

            var result = JsonConvert.DeserializeObject<ScheduleRoot>(scheduleJson);

            Schedule lastItem = result.schedules.LastOrDefault();
            DateTimeOffset.TryParse(lastItem.DATESTAMP_ISO, out DateTimeOffset dt);
            localSettings.Values["LastScheduleDate"] = dt;

            return result.schedules;
        }

        public async Task<List<Schedule>> GetTimetable(string intakeCode, string tutorialGroup, bool isFsStudent)
        {
            string studentType = (isFsStudent == true) ? "(FS)" : "(LS)";
            var allSchedule = await DeserializeSchedule();

            if (allSchedule == null)
                return null;

            List<Schedule> filteredItems = new List<Schedule>();

            foreach (Schedule item in allSchedule)
            {
                if (item.INTAKE == intakeCode && item.GROUPING == tutorialGroup)
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
                        var isIgnoredModuleFound = ignoredModules.Find(x => item.MODID.ToLower().Contains(x.ToLower()));
                        if (isIgnoredModuleFound == null)
                        {
                            filteredItems.Add(item);
                        }
                    }
                }
            }

            return filteredItems;
        }
    }
}