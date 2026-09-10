using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.DBLibrary;

namespace Vayu.LTC_Graphs.ViewModels
{

    /// <summary>
    /// 
    /// </summary>
    public static class NullableFunctions
    {
        /// <summary>
        /// Nulls the sum.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static double? NullSum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            if (source.All(x => !selector(x).HasValue))
                return null;
            else
                return source.Select(x => selector(x)).Sum();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GraphItem
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public DateTime X { get; set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double? Y { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class SourceSinkConverter : IValueConverter
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
            string name = string.Empty;
            try
            {
                var sourceSink = (value as System.Windows.Controls.ListBoxItem).Content as SourceSinkDetail;
                if (sourceSink == null)
                {
                    return name;
                }
                else
                {
                    try
                    {
                        if (sourceSink.Source == null && sourceSink.Sink == null)
                        {
                            name = "";
                        }
                        else if (sourceSink.Source != null && sourceSink.Sink != null)
                        {
                            name = sourceSink.Source + " --> " + sourceSink.Sink;
                        }
                        else if (sourceSink.Source == null)
                        {
                            name = sourceSink.Sink.ToString();
                        }
                        else if (sourceSink.Sink == null)
                        {
                            name = sourceSink.Source.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return name;
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
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    internal class ValueToForegroundConverter : System.Windows.Data.IValueConverter
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
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                double doubVal;
                if (double.TryParse(value.ToString(), out doubVal))
                {
                    if (doubVal < 0)
                        return new SolidColorBrush(Colors.Red);
                    else return new SolidColorBrush(Colors.Black);
                }
                else return new SolidColorBrush();
            }
            else return new SolidColorBrush();
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
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
