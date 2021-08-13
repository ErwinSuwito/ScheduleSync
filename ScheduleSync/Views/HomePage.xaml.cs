using System;
using System.Collections.Generic;
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
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        DispatcherTimer dt = new DispatcherTimer();
        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            StartSyncingAnimation();
            dt.Interval = new TimeSpan(0, 0, 5);
            dt.Tick += Dt_Tick;
            dt.Start();
        }

        private void Dt_Tick(object sender, object e)
        {
            dt.Stop();
            StopSyncingAnimation();
        }

        private void StartSyncingAnimation()
        {
            SyncButton.IsEnabled = false;
            ProgressRing.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
        }

        private void StopSyncingAnimation()
        {
            SyncButton.IsEnabled = true;
            ProgressRing.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
        }
    }
}
