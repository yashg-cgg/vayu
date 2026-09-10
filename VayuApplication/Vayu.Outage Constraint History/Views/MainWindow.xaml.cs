
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Vayu.Outage_Constraint_History.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
    }
    public class DataGridBackColorConverter : IValueConverter
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
            SolidColorBrush mybrush = new SolidColorBrush();
            double number;

            if (value != null)
            {
                if (value.ToString().Contains("($"))
                {
                    mybrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    double.TryParse(value.ToString(), out number);
                    if (number == 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number > 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number < 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                }
            }
            return mybrush;
        }
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
