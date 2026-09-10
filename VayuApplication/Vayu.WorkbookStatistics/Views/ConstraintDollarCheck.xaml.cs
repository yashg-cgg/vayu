
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.WorkbookStatistics.Model;

namespace Vayu.WorkbookStatistics.Views
{
    /// <summary>
    /// Interaction logic for ConstraintDollarCheck.xaml
    /// </summary>
    public partial class ConstraintDollarCheck : Window
    {
        public ConstraintDollarCheck()
        {
            InitializeComponent();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class DGDollarCellForeColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
            try
            {
                DataGridCell cell = null;
                string cellHeader = "";
                int cellHeaderTest = -1;
                if (values[0] != DependencyProperty.UnsetValue)
                {
                    cell = (DataGridCell)values[0];
                    cellHeader = (string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                double textValue = 0;
                if (values[1] != DependencyProperty.UnsetValue)
                {
                    double.TryParse((string)values[1], out textValue);
                }
                if (textValue < 0)
                {
                    return mybrush = new SolidColorBrush(Colors.Red);
                }
                return mybrush;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception in DataGridCellForeColorConverter: " + ex.Message);
                return mybrush;
            }
        }
        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class DGDollarCellBackColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.White);
            try
            {
                DataGridCell cell = null;
                string cellHeader = "";
                int cellHeaderTest = -1;
                DataGridRow row = (DataGridRow)values[2];
                int rowindex = row.GetIndex();
                if (values[0] != DependencyProperty.UnsetValue)
                {
                    cell = (DataGridCell)values[0];
                    cellHeader = (string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                double textValue = 0;
                if (values[1] != DependencyProperty.UnsetValue)
                {
                    double.TryParse((string)values[1], out textValue);
                }
                Exposure exposuer = new Exposure();
                if (values[3] != DependencyProperty.UnsetValue)
                {
                    exposuer = (Exposure)values[3];
                }
                if (textValue < 0)
                {
                    if (textValue < -5000)
                    {
                        return mybrush = new SolidColorBrush(Colors.LightPink);
                    }
                }
                if (rowindex % 2 == 0)
                {
                    mybrush = new SolidColorBrush(Colors.White);
                }
                else
                {
                    mybrush = new SolidColorBrush(Colors.LightGray);
                }
                if (!exposuer.IsShiftEmpty)
                {
                    mybrush = new SolidColorBrush(Colors.Teal);
                }
                return mybrush;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception in DataGridCellBackColorConverter: " + ex.Message);
                return mybrush;
            }
        }
        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
