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
    public sealed partial class HomePage : Page
    {
        private DataAccess da = new DataAccess();
        StorageFile jsonFile;
        StorageFolder tempFolder = ApplicationData.Current.LocalFolder;
        private string scheduleJson;

        public HomePage()
        {
            this.InitializeComponent();
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SetIntakePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            loadPanel.Visibility = Visibility.Visible;

            List<Schedule> timetable = await da.FilterTimetable("UC2F2008SE", "T1", true);

            foreach (Schedule schedule in timetable)
            {
                Event @event = new Event()
                {
                    Subject = schedule.MODID,
                    Start = new DateTimeTimeZone
                    {
                        DateTime = schedule.DATESTAMP_ISO + " " + schedule.TIME_FROM,
                        TimeZone = "Singapore Standard Time"
                    },
                    End = new DateTimeTimeZone
                    {
                        DateTime = schedule.DATESTAMP_ISO + " " + schedule.TIME_TO,
                        TimeZone = "Singapore Standard Time"
                    },
                    Location = new Location
                    {
                        DisplayName = schedule.ROOM
                    },
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = "Your lecturer is <a href=\"mailto:" + schedule.SAMACCOUNTNAME + "@staffemail.apu.edu.my\">" + schedule.NAME + "</a><br><br>Added by ScheduleSync"
                    }
                };

                var provider = ProviderManager.Instance.GlobalProvider;

                if (provider != null && provider.State == ProviderState.SignedIn)
                {
                    await provider.Graph.Me.Events.Request().AddAsync(@event);
                }
            }

            loadPanel.Visibility = Visibility.Collapsed;
        }
    }
}
