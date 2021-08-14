using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        IProvider provider = ProviderManager.Instance.GlobalProvider;
        GraphServiceClient graphClient;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void IntakeSettings_Loaded(object sender, RoutedEventArgs e)
        {
            // Gets the intake code of the user (with their student type and tutorial group)
            // and shows it in a TextBlock inside the expander control header.
            IntakeCode.Text = IntakeSettings.GetIntakeCode();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IntakeSettings.SaveIntakeSettings();
            base.OnNavigatingFrom(e);
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (provider.State == ProviderState.SignedIn)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Are you sure you want to logout?",
                    Content = "We won't be able to sync your calendar after you logout. You will be asked to sign in again the next time you open the app.",
                    PrimaryButtonText = "Logout",
                    CloseButtonText = "Cancel"
                };

                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (provider != null)
                    {
                        await provider.SignOutAsync();
                    }
                }
            }
            else if(provider.State == ProviderState.SignedOut)
            {
                await provider.SignInAsync();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (provider != null)
            {
                provider.StateChanged += Provider_StateChanged;
                await RefreshUserInfoAsync();
            }
        }

        private async Task RefreshUserInfoAsync()
        {
            if (provider.State == ProviderState.SignedIn)
            {
                LogoutButton.Content = "Logout";
                graphClient = provider.GetClient();
                var me = await graphClient.Me.Request().GetAsync();
                UserEmail.Text = me.UserPrincipalName;
            }
            else if (provider.State == ProviderState.SignedOut)
            {
                LogoutButton.Content = "Login";
                UserEmail.Text = "You are not signed in";
            }
        }

        private async void Provider_StateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            await  RefreshUserInfoAsync();
        }
    }
}
