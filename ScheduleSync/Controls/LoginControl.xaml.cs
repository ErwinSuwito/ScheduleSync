using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ScheduleSync.Controls
{
    public sealed partial class LoginControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        IProvider provider = ProviderManager.Instance.GlobalProvider;
        GraphServiceClient graphClient;
        private string loginButtonText;
        private string signedInAsEmailText;

        public string LoginButtonText
        {
            get { return this.loginButtonText; }
            set
            {
                this.loginButtonText = value;
                this.OnPropertyChanged();
            }
        }
        public string SignedInAsEmailText
        {
            get { return this.signedInAsEmailText; }
            set
            {
                this.signedInAsEmailText = value;
                this.OnPropertyChanged();
            }
        }

        public LoginControl()
        {
            this.InitializeComponent();
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
            else if (provider.State == ProviderState.SignedOut)
            {
                await provider.SignInAsync();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (provider != null)
            {
                provider.StateChanged += Provider_StateChanged;
                await RefreshUserInfoAsync();
            }
        }

        private async void Provider_StateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            await RefreshUserInfoAsync();
        }

        private async Task RefreshUserInfoAsync()
        {
            if (provider.State == ProviderState.SignedIn)
            {
                LoginButtonText = "Logout";
                graphClient = provider.GetClient();
                var me = await graphClient.Me.Request().GetAsync();
                SignedInAsEmailText = me.UserPrincipalName;
            }
            else if (provider.State == ProviderState.SignedOut)
            {
                LoginButtonText = "Login";
                SignedInAsEmailText = "You are not signed in";
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
