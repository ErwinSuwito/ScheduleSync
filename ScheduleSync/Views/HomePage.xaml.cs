using Microsoft.Toolkit.Uwp.UI.Animations;
using ScheduleSync.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduleSync.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        SyncService syncService = new SyncService();
        DataAccess data = new DataAccess();
        bool IsLoading = false;
        SyncResult result;
        DispatcherTimer dt = new DispatcherTimer();

        public HomePage()
        {
            this.InitializeComponent();
            dt.Tick += Dt_Tick;
            dt.Interval = new TimeSpan(0, 0, 3);
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoading)
            {
                StartSyncingAnimation();

                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                string IntakeCode = localSettings.Values["IntakeCode"].ToString();
                string TutorialGroup = localSettings.Values["TutorialGroup"].ToString();
                bool.TryParse(localSettings.Values["IsFsStudent"].ToString(), out bool isForeignStudent);

                var schedule = await data.GetTimetable(IntakeCode, TutorialGroup, isForeignStudent);
                result = await syncService.SyncEventsAsync(schedule);

                ContentDialog contentDialog;

                switch (result)
                {
                    case SyncResult.Failed:
                        contentDialog = new ContentDialog()
                        {
                            Title = "Unable to sync",
                            Content = "We're encountering an error while syncing your timetable. Please try again later.",
                            CloseButtonText = "Ok"
                        };

                        await contentDialog.ShowAsync();
                        break;

                    case SyncResult.NoSchedule:
                        contentDialog = new ContentDialog()
                        {
                            Title = "No schedule found",
                            Content = "We didn't found any schedule for your intake. If there is supposed to be a schedule, make sure the intake code in Settings is correct.",
                            CloseButtonText = "Ok"
                        };

                        await contentDialog.ShowAsync();
                        break;
                }

                localSettings.Values["LastSyncedDate"] = DateTime.Now.ToShortDateString();
                StopSyncingAnimation();
            }
        }

        private void StartSyncingAnimation()
        {
            AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromSeconds(0.5)).StartAsync(SyncNowText);
            IsLoading = true;
            ProgressRing.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
        }

        private void StopSyncingAnimation()
        {
            IsLoading = false;
            ProgressRing.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;

            if (result == SyncResult.Success || result == SyncResult.NoSchedule)
            {
                dt.Start();
                SyncSuccessIcon.Visibility = Visibility.Visible;
            }
            else
            {
                AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromSeconds(0.5)).StartAsync(SyncNowText);
            }

            UpdateDates();
        }

        private void Dt_Tick(object sender, object e)
        {
            dt.Stop();
            SyncSuccessIcon.Visibility = Visibility.Collapsed;
            AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromSeconds(0.5)).StartAsync(SyncNowText);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDates();
        }

        private void UpdateDates()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string syncedUntilDate = (localSettings.Values["LastScheduleDate"] == null) ? "Never" : (localSettings.Values["LastScheduleDate"].ToString() == DateTime.Today.ToShortDateString() ? "Today" : localSettings.Values["LastScheduleDate"].ToString());
            string lastSyncDate = (localSettings.Values["LastSyncedDate"] == null) ? "Never" : (localSettings.Values["LastSyncedDate"].ToString() == DateTime.Today.ToShortDateString()) ? "Today" : localSettings.Values["LastSyncedDate"].ToString();
            SyncUntilDate.Text = syncedUntilDate;
            LastSyncDate.Text = lastSyncDate;
        }
    }
}
