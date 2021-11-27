using CommunityToolkit.Authentication;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using ScheduleSync.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ScheduleSync.Views.Setup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IgnoredModulesSetupPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public IgnoredModulesSetupPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var dataAccess = new DataAccess();

            string IntakeCode = localSettings.Values["IntakeCode"].ToString();
            string TutorialGroup = localSettings.Values["TutorialGroup"].ToString();
            bool.TryParse(localSettings.Values["IsFsStudent"].ToString(), out bool isForeignStudent);

            var schedule = await dataAccess.GetTimetable(IntakeCode, TutorialGroup, isForeignStudent);

            var filteredSchedule = schedule.GroupBy(x => x.MODULE_NAME)
                .Select(x => x.FirstOrDefault()).ToList();

            ModulesListView.ItemsSource = filteredSchedule;

            LoadingStackPanel.Visibility = Visibility.Collapsed;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            OSVersion OperatingSystemVersion = SystemInformation.Instance.OperatingSystemVersion;
            if (OperatingSystemVersion.Build >= 22000)
            {
                var parentFrame = Window.Current.Content as Frame;
                parentFrame.Navigate(typeof(Shell.MainShell), null);
            }
            else
            {
                var parentFrame = Window.Current.Content as Frame;
                parentFrame.Navigate(typeof(Shell.AcrylicMainShell), null);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
