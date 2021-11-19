# ScheduleSync
Sync your APU timetable with your Outlook Calendar.
Get it on the [Microsoft Store](https://www.microsoft.com/store/apps/9P8F2XFQVX57)

![ScheduleSync](/Assets/ScheduleSync%20Home.png)

## Building
You'll need to add a new ClientSecret.cs file with the following content before building the project:

```
namespace ScheduleSync
{
    public class ClientSecret
    {
        public static string ClientId = "YOUR-CLIENT-ID-HERE";
    }
}
```

You can [register here](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app) to get a **ClientID**.
