using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduleSync.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Shows the appview back button and adds event to handle when
            // the back button is clicked
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            // Loads settings and update controls
            intakeCodeBox.Text = localSettings.Values["IntakeCode"].ToString();
            tutorialGroupBox.Text = localSettings.Values["TutorialGroup"].ToString();

            bool.TryParse(localSettings.Values["IsLocalStudent"].ToString(), out bool IsLocalStudent);
            if (IsLocalStudent)
            {
                lsRadBtn.IsChecked = true;
            }
            else
            {
                fsRadBtn.IsChecked = true;
            }

            if (Debugger.IsAttached)
            {
                debugPanel.Visibility = Visibility.Visible;
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values["IntakeCode"] = intakeCodeBox.Text.Trim();
            localSettings.Values["TutorialGroup"] = tutorialGroupBox.Text.Trim();
            localSettings.Values["IsLocalStudent"] = lsRadBtn.IsChecked;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(@"https://github.com/ErwinSuwito/ScheduleSync/issues");
            await Launcher.LaunchUriAsync(uri);
        }

        private void deregisterBgTask_Click(object sender, RoutedEventArgs e)
        {

        }

        private void openStorageFolder_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
