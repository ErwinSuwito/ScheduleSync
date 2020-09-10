using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using ScheduleSync;
using Windows.Storage;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Composition;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    class DownloadScheduleBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        DataAccess da = new DataAccess();
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            DateTime syncedUntil = new DateTime();

            if (localSettings.Values["SyncedUntilDate"] != null)
            {
                DateTime.TryParse(localSettings.Values["SyncedUntilDate"].ToString(), out syncedUntil);
            }
            else
            {
                await UpdateSchedule();
            }

            if (DateTime.Today >= syncedUntil && (DateTime.Today.DayOfWeek == DayOfWeek.Friday || DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday))
            {
                bool IsSuccess = await UpdateSchedule();
                if (IsSuccess)
                {
                    var toastContent = new ToastContent()
                    {
                        Visual = new ToastVisual()
                        {
                            BindingGeneric = new ToastBindingGeneric()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = "You have new schedule!"
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = "Next week's timetable is ready to be imported to your Calendar."
                                    }
                                }
                            }
                        },
                        Actions = new ToastActionsCustom()
                        {
                            Buttons =
                            {
                                new ToastButton("Add now", "sync")
                                {
                                    ActivationType = ToastActivationType.Background,
                                    ActivationOptions = new ToastActivationOptions()
                                    {
                                        AfterActivationBehavior = ToastAfterActivationBehavior.PendingUpdate
                                    }
                                },
                                new ToastButtonDismiss("Dismiss")
                            }
                        }
                    };

                    // Create the toast notification
                    var toastNotif = new ToastNotification(toastContent.GetXml());

                    // And send the notification
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                }
            }

            _deferral.Complete();
        }

        private async Task<bool> UpdateSchedule()
        {
            bool IsSuccess = await da.GetSchedule();
            if (IsSuccess)
            {
                IsSuccess = await da.ExtractGZip();

                if (IsSuccess)
                {
                    var schedule = await da.ReadAndParseSchedule();

                    IsSuccess = (schedule == null) ? false : true;
                }
            }

            return IsSuccess;
        }
    }
}
