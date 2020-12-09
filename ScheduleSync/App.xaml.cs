using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ScheduleSync
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Windows.UI.Xaml.Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        DataAccess da = new DataAccess();

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            // Registers background task
            RegisterBackgroundTask();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public async void RegisterBackgroundTask()
        {
            string taskName = "Background Schedule Downloader";
            foreach (var _task in BackgroundTaskRegistration.AllTasks)
            {
                if (_task.Value.Name == taskName)
                {
                    return;
                }
            }

            await BackgroundExecutionManager.RequestAccessAsync();
            var builder = new BackgroundTaskBuilder();
            builder.Name = "Background Schedule Downloader";
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.InternetAvailable, false));
            builder.IsNetworkRequested = true;

            BackgroundTaskRegistration task = builder.Register();
            Debug.WriteLine("Task registered");
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            var deferral = args.TaskInstance.GetDeferral();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            // Background task to sync schedule from notification
            if (args.TaskInstance.TriggerDetails is ToastNotificationActionTriggerDetail)
            {
                var toastArgs = args.TaskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;

                if (toastArgs.Argument == "sync")
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
                                        Text = "We're adding your schedule..."
                                    }
                                }
                            }
                        }
                    };

                    // Create the toast notification
                    var toastNotif = new ToastNotification(toastContent.GetXml());

                    // And send the notification
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

                    try
                    {
                        List<Schedule> schedules = await da.ReadAndParseSchedule();
                        string intakeCode = localSettings.Values["IntakeCode"].ToString();
                        string tutorialGroup = localSettings.Values["TutorialGroup"].ToString();
                        bool.TryParse(localSettings.Values["IsLocalStudent"].ToString(), out bool isLocalStudent);

                        List<Schedule> filteredSchedule = await da.FilterTimetablev2(schedules, intakeCode, tutorialGroup, isLocalStudent);

                        if (filteredSchedule.Count > 0)
                        {
                            foreach (Schedule schedule in filteredSchedule)
                            {
                                Event @event = new Event()
                                {
                                    Subject = schedule.MODID,
                                    Start = new DateTimeTimeZone
                                    {
                                        DateTime = schedule.DATESTAMP_ISO + " " + schedule.TIME_FROM,
                                        TimeZone = "Singapore Standard Time"
                                    },
                                    End = new DateTimeTimeZone
                                    {
                                        DateTime = schedule.DATESTAMP_ISO + " " + schedule.TIME_TO,
                                        TimeZone = "Singapore Standard Time"
                                    },
                                    Location = new Location
                                    {
                                        DisplayName = schedule.ROOM
                                    },
                                    Body = new ItemBody
                                    {
                                        ContentType = BodyType.Html,
                                        Content = "Your lecturer is <a href=\"mailto:" + schedule.SAMACCOUNTNAME + "@staffemail.apu.edu.my\">" + schedule.NAME + "</a><br><br>Added by ScheduleSync"
                                    }
                                };

                                var provider = ProviderManager.Instance.GlobalProvider;

                                if (provider != null && provider.State == ProviderState.SignedIn)
                                {
                                    await provider.Graph.Me.Events.Request().AddAsync(@event);
                                }
                            }

                            toastContent = new ToastContent()
                            {
                                Visual = new ToastVisual()
                                {
                                    BindingGeneric = new ToastBindingGeneric()
                                    {
                                        Children =
                                    {
                                        new AdaptiveText()
                                        {
                                            Text = "We've added your schedule"
                                        }
                                    }
                                    }
                                }
                            };

                            // Create the toast notification
                            toastNotif = new ToastNotification(toastContent.GetXml());

                            // And send the notification
                            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                        }
                    }
                    catch (Exception ex)
                    {
                        toastContent = new ToastContent()
                        {
                            Visual = new ToastVisual()
                            {
                                BindingGeneric = new ToastBindingGeneric()
                                {
                                    Children =
                                    {
                                        new AdaptiveText()
                                        {
                                            Text = "We can't add your schedule now. Please try again later."
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = "Make sure that you are still logged in from the app settings."
                                        }
                                    }
                                }
                            }
                        };

                        // Create the toast notification
                        toastNotif = new ToastNotification(toastContent.GetXml());

                        // And send the notification
                        ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                    }
                }
            }
            else
            {
                #region "OldCode"
                /*
                // Downloads new schedule
                bool isLatestScheduleSynced = false;

                if (localSettings.Values["SyncedUntilDate"] != null)
                {
                    string syncedUntil = localSettings.Values["SyncedUntilDate"].ToString();
                    bool.TryParse(localSettings.Values[syncedUntil].ToString(), out isLatestScheduleSynced);
                }
                else
                {
                    await UpdateSchedule();
                }

                if (!isLatestScheduleSynced && (DateTime.Today.DayOfWeek == System.DayOfWeek.Friday || DateTime.Today.DayOfWeek == System.DayOfWeek.Saturday || DateTime.Today.DayOfWeek == System.DayOfWeek.Sunday))
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
                                        AfterActivationBehavior = ToastAfterActivationBehavior.Default
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
                */
                #endregion

                var dayOfWeek = DateTime.Today.DayOfWeek;

                if (dayOfWeek == System.DayOfWeek.Friday || dayOfWeek == System.DayOfWeek.Saturday || dayOfWeek == System.DayOfWeek.Sunday)
                {
                    if (localSettings.Values["SyncedUntilDate"] != null)
                    {
                        string syncedUntilDate = localSettings.Values["SyncedUntilDate"].ToString();
                        DateTime.TryParse(syncedUntilDate, out DateTime SyncedUntilDateTime);

                        if (SyncedUntilDateTime < DateTime.Today)
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
                                                    AfterActivationBehavior = ToastAfterActivationBehavior.Default
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
                    }
                    else
                    {
                        Debug.WriteLine("It's not friday, saturday, or sunday yet.");
                    }
                }
            }
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
