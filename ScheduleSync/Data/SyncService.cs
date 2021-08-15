using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Data
{
    public enum SyncResult
    {
        Success,
        Failed
    }

    public class SyncService
    {
        GraphServiceClient graphClient;

        public SyncService()
        {
            ProviderManager.Instance.ProviderStateChanged += Instance_ProviderStateChanged;
            if (ProviderManager.Instance.GlobalProvider.State == ProviderState.SignedIn)
            {
                graphClient = ProviderManager.Instance.GlobalProvider.GetClient();
            }
        }

        private void Instance_ProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider?.State == ProviderState.SignedIn)
            {
                graphClient = provider.GetClient();
            }
        }

        public async Task<SyncResult> SyncEventsAsync(List<Schedule> schedules)
        {
            if (graphClient == null)
                return SyncResult.Failed;

            try
            {
                foreach (var schedule in schedules)
                {
                    Event @event = MakeEvent(schedule);

                    await graphClient.Me.Events.Request().AddAsync(@event);
                }

                return SyncResult.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return SyncResult.Failed;
            }
        }

        public Event MakeEvent(Schedule schedule)
        {
            return new Event()
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
        }
    }
}
