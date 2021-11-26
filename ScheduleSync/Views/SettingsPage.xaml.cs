﻿using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using ScheduleSync.Controls;
using ScheduleSync.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        public string ApplicationVersion => $"Version {SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}";
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public List<string> _ignoredModules = new List<string>();

        public List<string> IgnoredModules
        {
            get { return _ignoredModules; }
            set
            {
                _ignoredModules = value;
                this.OnPropertyChanged();
            }
        }

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GetIgnoredModules();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IntakeSettings.SaveIntakeSettings();
            base.OnNavigatingFrom(e);
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

        private void SubmitIgnoredModuleButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AddIgnoredModules(ModuleNameTextBox.Text);

            AddIgnoredModuleFlyout.Hide();
        }

        private void GetIgnoredModules()
        {
            List<string> ignoredModules = new List<string>();

            if (localSettings.Containers.ContainsKey("IgnoredModulesName") == false)
            {
                localSettings.CreateContainer("IgnoredModulesName", ApplicationDataCreateDisposition.Always);
            }

            for (int i = 0; i < localSettings.Containers["IgnoredModulesName"].Values.Count; i++)
            {
                ignoredModules.Add(localSettings.Containers["IgnoredModulesName"].Values.ElementAt(i).Value.ToString());
            }

            IgnoredModules = ignoredModules;
        }

        private void AddIgnoredModules(string ignoredModuleName)
        {
            if (string.IsNullOrEmpty(ignoredModuleName))
                return;

            if (localSettings.Containers.ContainsKey("IgnoredModulesName"))
            {
                localSettings.Containers["IgnoredModulesName"].Values[(_ignoredModules.Count - 1).ToString()] = ignoredModuleName;
            }

            GetIgnoredModules();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
