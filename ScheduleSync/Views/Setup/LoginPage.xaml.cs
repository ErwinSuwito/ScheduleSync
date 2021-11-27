using CommunityToolkit.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduleSync.Views.Setup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private bool isNextBtnEnabled = false;
        public bool IsNextBtnEnabled 
        {
            get { return this.isNextBtnEnabled; }
            set
            {
                this.isNextBtnEnabled = value;
                this.OnPropertyChanged();
            }
        }

        IProvider provider = ProviderManager.Instance.GlobalProvider;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            provider.StateChanged += Provider_StateChanged;
            CheckLoginState();
        }

        private void Provider_StateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            CheckLoginState();
        }

        private void CheckLoginState()
        {
            if (provider.State == ProviderState.SignedIn)
            {
                IsNextBtnEnabled = true;
            }
            else
            {
                IsNextBtnEnabled = false;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (provider.State == ProviderState.SignedIn)
            {
                var navigationTransitionInfo = new SlideNavigationTransitionInfo();
                navigationTransitionInfo.Effect = SlideNavigationTransitionEffect.FromRight;
                this.Frame.Navigate(typeof(IntakeSetupPage), null, navigationTransitionInfo);
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
