
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Vayu.ERCOT_Market_Overview.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _capturing;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            endDatePicker.Visibility = System.Windows.Visibility.Hidden;
            // Label2.Visibility = System.Windows.Visibility.Hidden;
            SpecificConstraint.Visibility = System.Windows.Visibility.Hidden;
            _capturing = false;
        }
        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }

        /// <summary>
        /// Handles the Checked event of the DateRangeCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void DateRangeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            endDatePicker.Visibility = System.Windows.Visibility.Visible;
            //Label2.Visibility = System.Windows.Visibility.Visible;
            SpecificConstraint.Visibility = System.Windows.Visibility.Visible;

        }

        /// <summary>
        /// Handles the Unchecked event of the DateRangeCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DateRangeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            endDatePicker.Visibility = System.Windows.Visibility.Hidden;
            // Label2.Visibility = System.Windows.Visibility.Hidden;
            SpecificConstraint.Visibility = System.Windows.Visibility.Hidden;



        }

        private void FamilyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
    public class ValueToForegroundColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            double doubleValue = 0.0;
            if (value != null)
            {
                double.TryParse(value.ToString(), out doubleValue);
                if (doubleValue < 0)
                {
                    brush = new SolidColorBrush(Colors.Red);
                }
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
