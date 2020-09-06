using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace ScheduleSync
{
    class DownloadScheduleBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral; 
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            


            _deferral.Complete();
        }
    }
}
