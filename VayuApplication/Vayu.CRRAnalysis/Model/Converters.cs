using System;
using System.Globalization;
using System.Windows.Data;

namespace Vayu.CRRAnalysis.Model
{
    public class IsLesserConverter : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string dStr = (value ?? "").ToString();
            double dVal = 0;
            double.TryParse(dStr, out dVal);

            if (dVal < 0 || dStr.Contains("("))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Activator.CreateInstance(targetType);
        }
    }
}
