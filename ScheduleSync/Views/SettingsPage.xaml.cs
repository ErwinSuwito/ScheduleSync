using Microsoft.Toolkit.Uwp.Helpers;
using ScheduleSync.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduleSync.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public string ApplicationVersion => $"Version {SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}";

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IntakeSettings.SaveIntakeSettings();
            base.OnNavigatingFrom(e);
        }

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private async Task<string> GetNotices(string path)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));

            if (file != null)
            {
                return System.IO.File.ReadAllText(file.Path);
            }

            return string.Empty;
        }

        private async void HyperlinkButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;

            switch (btn.Tag)
            {
                case "Review":
                    // This won't work properly until app is associated with the Store.
                    await SystemInformation.LaunchStoreForReviewAsync();
                    break;

                case "PrivacyPolicy":
                    string privacyPolicy = await GetNotices("ms-appx:///Assets/Notices/PrivacyPolicy.md");
                    var privacyPolicyDialog = new NoticeContentDialog("Privacy Policy", privacyPolicy);
                    await privacyPolicyDialog.ShowAsync();
                    break;

                case "License":
                    string license = await GetNotices("ms-appx:///Assets/Notices/License.md");
                    var licenseDialog = new NoticeContentDialog("License", license);
                    await licenseDialog.ShowAsync();
                    break;

                case "ThirdPartyNotice":
                    string thirdPartyLicense = await GetNotices("ms-appx:///Assets/Notices/ThirdPartyLicenses.md");
                    var thirdPartyLicenseDialog = new NoticeContentDialog("Third Party Licenses", thirdPartyLicense);
                    await thirdPartyLicenseDialog.ShowAsync();
                    break;

                default:
                    break;
            }
        }
    }
}
