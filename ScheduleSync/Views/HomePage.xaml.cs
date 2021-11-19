using Humanizer;
using Microsoft.Toolkit.Uwp.UI.Animations;
using ScheduleSync.Controls;
using ScheduleSync.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduleSync.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        SyncService syncService = new SyncService();
        DataAccess data = new DataAccess();
        private bool isLoading = false;
        private bool showCheckIcon = false;
        SyncResult result;
        DispatcherTimer dt = new DispatcherTimer();
        private string syncUntilDate;
        private string lastSyncDate;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        #region Getter Setter methods
        public string SyncUntilDate
        {
            get { return this.syncUntilDate; }
            set
            {
                this.syncUntilDate = value;
                this.OnPropertyChanged();
            }
        }

        public string LastSyncDate
        {
            get { return this.lastSyncDate; }
            set
            {
                this.lastSyncDate = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                this.isLoading = value;
                this.OnPropertyChanged();
            }
        }

        public bool ShowCheckIcon
        {
            get { return this.showCheckIcon; }
            set
            {
                this.showCheckIcon = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        public HomePage()
        {
            this.InitializeComponent();
            dt.Tick += Dt_Tick;
            dt.Interval = new TimeSpan(0, 0, 3);
        }

        private async Task<bool> SyncSchedule(string intakeCode, string tutorialGroup, bool isForeignStudent, bool saveLastSyncDate)
        {
            var schedule = await data.GetTimetable(intakeCode, tutorialGroup, isForeignStudent);
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

            if (saveLastSyncDate)
                localSettings.Values["LastSyncedDate"] = DateTimeOffset.Now;

            return true;
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoading)
            {
                StartSyncingAnimation();
                
                string IntakeCode = localSettings.Values["IntakeCode"].ToString();
                string TutorialGroup = localSettings.Values["TutorialGroup"].ToString();
                bool.TryParse(localSettings.Values["IsFsStudent"].ToString(), out bool isForeignStudent);

                await SyncSchedule(IntakeCode, TutorialGroup, isForeignStudent, true);

                StopSyncingAnimation();
            }
        }

        private void StartSyncingAnimation()
        {
            AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromSeconds(0.5)).StartAsync(SyncNowText);
            IsLoading = true;
        }

        private void StopSyncingAnimation()
        {
            IsLoading = false;

            if (result == SyncResult.Success || result == SyncResult.NoSchedule)
            {
                dt.Start();
                ShowCheckIcon = true;
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
            ShowCheckIcon = false;
            AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromSeconds(0.5)).StartAsync(SyncNowText);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDates();
        }

        private void UpdateDates()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            DateTimeOffset syncedUntilDateTimeOffset, lastSyncDateTimeOffset;

            if (localSettings.Values["LastScheduleDate"] != null)
                DateTimeOffset.TryParse(localSettings.Values["LastScheduleDate"].ToString(), out syncedUntilDateTimeOffset);

            if (localSettings.Values["LastSyncedDate"] != null)
                DateTimeOffset.TryParse(localSettings.Values["LastSyncedDate"].ToString(), out lastSyncDateTimeOffset);

            LastSyncDate = (lastSyncDateTimeOffset != DateTimeOffset.MinValue) ? lastSyncDateTimeOffset.Humanize() : "Never";

            if (syncedUntilDateTimeOffset.DateTime < DateTime.Now)
            {
                SyncUntilDate = (syncedUntilDateTimeOffset != DateTimeOffset.MinValue) ? syncedUntilDateTimeOffset.Humanize() : "Never";
            }
            else
            {
                SyncUntilDate = syncedUntilDateTimeOffset.DateTime.ToShortDateString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var otherIntakeDialog = new SyncOtherIntakeContentDialog();
            await otherIntakeDialog.ShowAsync();

            if (!string.IsNullOrEmpty(otherIntakeDialog.intake) && !string.IsNullOrEmpty(otherIntakeDialog.tutorialGroup))
            {
                StartSyncingAnimation();
                await SyncSchedule(otherIntakeDialog.intake, otherIntakeDialog.tutorialGroup, otherIntakeDialog.isForeignStudent, false);
                StopSyncingAnimation();
            }
        }
    }
}
