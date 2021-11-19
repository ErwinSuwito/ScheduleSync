using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

            this.Hide();
        }
    }
}
