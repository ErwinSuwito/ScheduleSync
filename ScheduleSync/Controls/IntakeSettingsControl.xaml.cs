using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ScheduleSync.Controls
{
    public sealed partial class IntakeSettingsControl : UserControl
    {
        public string IntakeCode
        {
            get { return (string)GetValue(IntakeCodeProperty); }
            set { SetValue(IntakeCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IntakeCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntakeCodeProperty =
            DependencyProperty.Register("IntakeCode", typeof(string), typeof(IntakeSettingsControl), new PropertyMetadata(""));

        public string TutorialGroup
        {
            get { return (string)GetValue(TutorialGroupProperty); }
            set { SetValue(TutorialGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TutorialGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TutorialGroupProperty =
            DependencyProperty.Register("TutorialGroup", typeof(string), typeof(IntakeSettingsControl), new PropertyMetadata(""));

        public bool IsFsStudent
        {
            get { return (bool)GetValue(IsFsStudentProperty); }
            set { SetValue(IsFsStudentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFsStudent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFsStudentProperty =
            DependencyProperty.Register("IsFsStudent", typeof(bool), typeof(IntakeSettingsControl), new PropertyMetadata(false));

        public IntakeSettingsControl()
        {
            this.InitializeComponent();
        }
    }
}
