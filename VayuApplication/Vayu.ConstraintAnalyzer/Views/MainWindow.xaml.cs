using System.Windows;

namespace Vayu.ConstraintAnalyzer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DatePicker2.Visibility = System.Windows.Visibility.Hidden;
            Label1.Visibility = System.Windows.Visibility.Hidden;
        }

        #region Events

        /// <summary>
        /// Handles the Checked event of the DaRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DaRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ImpactRadioButton.IsEnabled = false;
            ShiftedRadioButton.IsEnabled = false;
            ShadowRadioButton.IsChecked = true;
            FamilyCheckBox.IsChecked = false;
            FamilyComboBox.IsEnabled = false;
        }

        /// <summary>
        /// Handles the Checked event of the DateRangeCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DateRangeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DatePicker2.Visibility = System.Windows.Visibility.Visible;
            Label1.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Handles the Unchecked event of the DateRangeCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DateRangeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DatePicker2.Visibility = System.Windows.Visibility.Hidden;
            Label1.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Handles the Checked event of the RtRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RtRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ImpactRadioButton != null)
                ImpactRadioButton.IsEnabled = true;
            if (ShiftedRadioButton != null)
                ShiftedRadioButton.IsEnabled = true;
            if (FamilyComboBox != null)
                FamilyComboBox.IsEnabled = true;
        }

        #endregion
    }
}
