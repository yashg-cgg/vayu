
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Vayu.ConstraintContingencyHistory.Views
{
    /// <summary>
    /// Interaction logic for ErcotSensitivities.xaml
    /// </summary>
    public partial class ErcotSensitivities : Window


    {

        public ErcotSensitivities()
        {
            InitializeComponent();
        }
    }
    public class FontColorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number;
            double.TryParse(value.ToString(), out number);
            SolidColorBrush myBrush = new SolidColorBrush();
            if (number < 0)
            {
                myBrush = new SolidColorBrush(Colors.Red);
            }
            return number;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
