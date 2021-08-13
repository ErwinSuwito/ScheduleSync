﻿using CommunityToolkit.Authentication;
using System;
using System.Collections.Generic;
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

namespace ScheduleSync
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string clientId = ClientSecret.ClientId;
            string[] scopes = new string[] { "User.Read", "Calendars.ReadWrite" };

            ProviderManager.Instance.GlobalProvider = new MsalProvider(clientId, scopes);

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["RequireFirstRun"] != null)
            {
                this.Frame.Navigate(typeof(Shell.MainShell), null);
            }
            else
            {
                this.Frame.Navigate(typeof(Shell.SetupPage), null);
            }
        }
    }
}
