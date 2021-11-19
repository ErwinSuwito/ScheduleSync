using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduleSync.Controls
{
    public sealed partial class SyncOtherIntakeContentDialog : ContentDialog
    {
        public string intake = string.Empty;
        public string tutorialGroup = string.Empty;
        public bool isForeignStudent = false;

        public SyncOtherIntakeContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            intake = IntakeSettingsControl.IntakeCode;
            tutorialGroup = IntakeSettingsControl.TutorialGroup;
            isForeignStudent = IntakeSettingsControl.IsFsStudent;

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Containers.ContainsKey("EnteredIntakeCodes"))
            {
                if (!IntakeSettingsControl.EnteredIntakeCodes.Contains(intake))
                    localSettings.Containers["EnteredIntakeCodes"].Values[(IntakeSettingsControl.EnteredIntakeCodes.Count - 1).ToString()] = intake;
            }

            this.Hide();
        }
    }
}
