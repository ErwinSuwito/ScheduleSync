using Microsoft.Toolkit.Graph.Providers;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ScheduleSync
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        IProvider provider = ProviderManager.Instance.GlobalProvider;


        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            graphProvider.ClientId = ClientSecret.GraphApiClientId;

            provider.StateChanged += Provider_StateChanged;

            base.OnNavigatedTo(e);
        }

        private async void Provider_StateChanged(object sender, StateChangedEventArgs e)
        {
            if (provider != null || provider.State == ProviderState.SignedOut)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "You are signed out",
                    Content = "You are signed out of your account. You'll need to login again before continuing.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();

                rootFrame.Navigate(typeof(Views.SignInPage), null);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["RequireFirstRun"] != null)
            {
                rootFrame.Navigate(typeof(Views.HomePage), null);
            }
            else
            {
                rootFrame.Navigate(typeof(Views.SignInPage), null);
            }
        }
    }
}
