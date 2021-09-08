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

        public HomePage()
        {
            this.InitializeComponent();
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            StartSyncingAnimation();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string IntakeCode = localSettings.Values["IntakeCode"].ToString();
            string TutorialGroup = localSettings.Values["TutorialGroup"].ToString();
            bool.TryParse(localSettings.Values["IsFsStudent"].ToString(), out bool isForeignStudent);

            var schedule = await data.GetTimetable(IntakeCode, TutorialGroup, isForeignStudent);
            var result = await syncService.SyncEventsAsync(schedule);

            if (result != SyncResult.Success)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Unable to sync",
                    Content = "We're encountering an error while syncing your timetable. Please try again later.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }

            localSettings.Values["LastSyncedDate"] = DateTime.Now.ToShortDateString();
            StopSyncingAnimation();
        }

        private void StartSyncingAnimation()
        {
            SyncButton.IsEnabled = false;
            ProgressRing.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
        }

        private void StopSyncingAnimation()
        {
            SyncButton.IsEnabled = true;
            ProgressRing.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
            UpdateDates();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDates();
        }

        private void UpdateDates()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string syncedUntilDate = (localSettings.Values["LastScheduleDate"] == null) ? "Never" : localSettings.Values["LastScheduleDate"].ToString();
            string lastSyncDate = (localSettings.Values["LastSyncedDate"] == null) ? "Never" : localSettings.Values["LastSyncedDate"].ToString();
            SyncUntilDate.Text = syncedUntilDate;
            LastSyncDate.Text = lastSyncDate;
        }
    }
}
