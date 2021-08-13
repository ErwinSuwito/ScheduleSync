﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        private void IntakeSettings_Loaded(object sender, RoutedEventArgs e)
        {
            string studentType = (IntakeSettings.IsFsStudent == true) ? "(FS)" : "(LS)";
            IntakeCode.Text = IntakeSettings.IntakeCode + studentType + ", " + IntakeSettings.TutorialGroup;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IntakeSettings.SaveIntakeSettings();
            base.OnNavigatingFrom(e);
        }
    }
}
