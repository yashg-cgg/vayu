using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Vayu.MarketViewNameSpace.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class DataGridBackColorConverterLMP : IValueConverter
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
                double.TryParse(value.ToString(), out number);
                BrushConverter brush = new BrushConverter();
                if (number < -10)
                {
                    return new SolidColorBrush(Color.FromRgb(115, 0, 148));
                }
                if (number >= -10 && number < 0)
                {
                    return new SolidColorBrush(Color.FromRgb(0, 77, 173));
                }
                if (number >= 0 && number < 6)
                {
                    return new SolidColorBrush(Color.FromRgb(49, 89, 255));
                }
                if (number >= 6 && number < 14)
                {
                    return new SolidColorBrush(Color.FromRgb(57, 113, 255));
                }
                if (number >= 14 && number < 16)
                {
                    return new SolidColorBrush(Color.FromRgb(57, 138, 255));
                }
                if (number >= 16 && number < 20)
                {
                    return new SolidColorBrush(Color.FromRgb(57, 162, 255));
                }
                if (number >= 20 && number < 30)
                {
                    return new SolidColorBrush(Color.FromRgb(49, 190, 255));
                }
                if (number >= 30 && number < 34)
                {
                    return new SolidColorBrush(Color.FromRgb(41, 215, 255));
                }
                if (number >= 34 && number < 38)
                {
                    return new SolidColorBrush(Color.FromRgb(24, 243, 255));
                }
                if (number >= 38 && number < 42)
                {
                    return new SolidColorBrush(Color.FromRgb(41, 255, 247));
                }
                if (number >= 42 && number < 46)
                {
                    return new SolidColorBrush(Color.FromRgb(90, 255, 222));
                }
                if (number >= 46 && number < 50)
                {
                    return new SolidColorBrush(Color.FromRgb(123, 255, 206));
                }
                if (number >= 50 && number < 56)
                {
                    return new SolidColorBrush(Color.FromRgb(148, 255, 173));
                }
                if (number >= 56 && number < 62)
                {
                    return new SolidColorBrush(Color.FromRgb(173, 255, 156));
                }
                if (number >= 62 && number < 68)
                {
                    return new SolidColorBrush(Color.FromRgb(198, 255, 132));
                }
                if (number >= 68 && number < 76)
                {
                    return new SolidColorBrush(Color.FromRgb(206, 255, 107));
                }
                else if (number >= 76 && number < 82)
                {
                    return new SolidColorBrush(Color.FromRgb(231, 255, 82));
                }
                if (number >= 82 && number < 90)
                {
                    return new SolidColorBrush(Color.FromRgb(239, 255, 57));
                }
                if (number >= 90 && number < 100)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 255, 24));
                }
                if (number >= 100 && number < 115)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 239, 0));
                }
                if (number >= 115 && number < 125)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 219, 0));
                }
                if (number >= 125 && number < 150)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 195, 0));
                }
                if (number >= 150 && number < 200)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 170, 0));
                }
                if (number >= 200 && number < 250)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 150, 0));
                }
                if (number >= 250 && number < 300)
                {
                    return new SolidColorBrush(Color.FromRgb(156, 162, 165));
                }
                if (number >= 300 && number < 400)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 101, 0));
                }
                if (number >= 400 && number < 500)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 81, 0));
                }
                if (number >= 500 && number < 600)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 48, 0));
                }
                if (number >= 600)
                {
                    return new SolidColorBrush(Color.FromRgb(255, 48, 0));
                }
                return Brushes.Black;
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ValueToForegroundConveter : IValueConverter
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
            double.TryParse(value.ToString(), out doubleValue);
            if (doubleValue < 0)
            {
                brush = new SolidColorBrush(Colors.Red);
            }
            return brush;
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
            throw new NotImplementedException();
        }
    }

}
