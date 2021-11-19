using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ScheduleSync.Controls
{
    public sealed partial class IntakeSettingsControl : UserControl
    {
        #region Dependency Properties
        public string IntakeCode
        {
            get { return (string)GetValue(IntakeCodeProperty); }
            set { Intake = GetIntakeCode();  SetValue(IntakeCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IntakeCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntakeCodeProperty =
            DependencyProperty.Register("IntakeCode", typeof(string), typeof(IntakeSettingsControl), new PropertyMetadata(""));

        public string TutorialGroup
        {
            get { return (string)GetValue(TutorialGroupProperty); }
            set { Intake = GetIntakeCode(); SetValue(TutorialGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TutorialGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TutorialGroupProperty =
            DependencyProperty.Register("TutorialGroup", typeof(string), typeof(IntakeSettingsControl), new PropertyMetadata(""));

        public bool IsFsStudent
        {
            get { return (bool)GetValue(IsFsStudentProperty); }
            set { Intake = GetIntakeCode(); SetValue(IsFsStudentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFsStudent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFsStudentProperty =
            DependencyProperty.Register("IsFsStudent", typeof(bool), typeof(IntakeSettingsControl), new PropertyMetadata(false));

        public string Intake
        {
            get { return (string)GetValue(IntakeProperty); }
            set { SetValue(IntakeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Intake.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntakeProperty =
            DependencyProperty.Register("Intake", typeof(string), typeof(IntakeSettingsControl), new PropertyMetadata(""));

        public bool IsLoadSettingNeeded
        {
            get { return (bool)GetValue(IsLoadSettingNeededProperty); }
            set { SetValue(IsLoadSettingNeededProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoadSettingNeeded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadSettingNeededProperty =
            DependencyProperty.Register("IsLoadSettingNeeded", typeof(bool), typeof(IntakeSettingsControl), new PropertyMetadata(true));

        #endregion
        
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public List<string> EnteredIntakeCodes = new List<string>();

        public IntakeSettingsControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsLoadSettingNeeded)
            {
                if (localSettings.Values["IntakeCode"] != null)
                {
                    IntakeCode = localSettings.Values["IntakeCode"].ToString();
                    TutorialGroup = localSettings.Values["TutorialGroup"].ToString();
                    bool.TryParse(localSettings.Values["IsFsStudent"].ToString(), out bool _isForeignStudent);
                    IsFsStudent = _isForeignStudent;
                }
            } 

            if (localSettings.Containers.ContainsKey("EnteredIntakeCodes") == false)
            {
                localSettings.CreateContainer("EnteredIntakeCodes", ApplicationDataCreateDisposition.Always);
            }

            for (int i = 0; i < localSettings.Containers["EnteredIntakeCodes"].Values.Count; i++)
            {
                EnteredIntakeCodes.Add(localSettings.Containers["EnteredIntakeCodes"].Values.ElementAt(i).ToString());
            }
        }

        public void SaveIntakeSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["IntakeCode"] = IntakeCode.Trim();
            localSettings.Values["TutorialGroup"] = TutorialGroup.Trim();
            localSettings.Values["IsFsStudent"] = IsFsStudent;
            localSettings.Values["RequireFirstRun"] = false;
        }

        /// <summary>
        /// Returns the complete intake code for the user, with their student type and tutorial group
        /// </summary>
        /// <returns></returns>
        public string GetIntakeCode()
        {
            string studentType = (IsFsStudent == true) ? "(FS)" : "(LS)";
            return IntakeCode + studentType + " " + TutorialGroup;
        }

        private void IntakeCodeAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var suitableItems = new List<string>();
            var splitText = sender.Text.ToLower().Split(' ');
            
            foreach (var intake in EnteredIntakeCodes)
            {
                var found = splitText.All((key) =>
                {
                    return intake.ToLower().Contains(key);
                });

                if (found)
                {
                    suitableItems.Add(intake);
                }
            }

            sender.ItemsSource = suitableItems;
        }

        private void IntakeCodeAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            IntakeCodeAutoSuggestBox.Text = args.SelectedItem.ToString();
        }
    }
}
