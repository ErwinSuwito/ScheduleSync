# ScheduleSync
Sync your APU timetable with your Outlook Calendar.

## Building
You'll need to add a new ClientSecret.cs file with the following content before building the project:

```
namespace ScheduleSync
{
    public class ClientSecret
    {
        public static string GraphApiClientId = "YOUR-CLIENT-ID-HERE";
    }
}
```

You can [register here](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app) to get a **ClientID**.
