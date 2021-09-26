﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IntakeSettings.SaveIntakeSettings();
            base.OnNavigatingFrom(e);
        }

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as StackPanel;

            switch (item.Tag)
            {
                case "Feedback":
                    await Launcher.LaunchUriAsync(new Uri(@"https://github.com/ErwinSuwito/ScheduleSync/issues"));
                    break;

                case "PrivacyPolicy":
                    await Launcher.LaunchUriAsync(new Uri(@"http://suwito.codes/privacypolicy/"));
                    break;

                case "Github":
                    await Launcher.LaunchUriAsync(new Uri(@"https://github.com/ErwinSuwito/ScheduleSync"));
                    break;

                default:
                    break;
            }
        }
    }
}
