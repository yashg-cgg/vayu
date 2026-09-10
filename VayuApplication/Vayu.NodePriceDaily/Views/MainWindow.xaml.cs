
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.NodePriceDaily.Model;
using Vayu.NodePriceDaily.ViewModels;

namespace Vayu.NodePriceDaily.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            nodeDataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
        }

        public void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            List<int> rowIndex = new List<int>();
            MainWindowViewModel dataContext = DataContext as MainWindowViewModel;
            List<DailyLMPDisplay> selectedItems = new List<DailyLMPDisplay>();
            List<Tuple<string, string>> sourceSinkList = new List<Tuple<string, string>>();
            foreach (var cell in nodeDataGrid.SelectedCells)
            {
                DataGridCell cellItem = Vayu.CommonControls.DataGridInfo.GetCell(cell);
                int index = Vayu.CommonControls.DataGridInfo.GetRowIndex(cellItem);
                if (!rowIndex.Contains(index))
                {
                    DailyLMPDisplay node = (DailyLMPDisplay)cell.Item;
                    sourceSinkList.Add(new Tuple<string, string>(node.NodeName, null));
                    rowIndex.Add(index);
                }
            }
            dataContext.SetSourceSinks(sourceSinkList);
        }
        /// <summary>
        /// Handles the Click event of the LMPGraph control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void LMPGraph_Click(object sender, RoutedEventArgs e)
        {
            if (nodeDataGrid.SelectedCells != null && nodeDataGrid.SelectedCells.Count > 0)
            {
                string column = nodeDataGrid.SelectedCells[0].Column.Header.ToString();
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowLMPGraphs(column);
                }
            }
        }
        /// <summary>
        /// Handles the Click event of the NodeAnalyzer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void NodeAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            if (nodeDataGrid.SelectedCells != null && nodeDataGrid.SelectedCells.Count >= 1)
            {
                string column = nodeDataGrid.SelectedCells[0].Column.Header.ToString();
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowNodeAnalyzer();
                }
            }
        }
        /// <summary>
        /// Sets the positions.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        public void SetPositions(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }
    }

    public class DataGridCellForeColorConverterSimple : IValueConverter
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
            SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
            double number;
            if (value != null)
            {
                Double.TryParse(value.ToString(), out number);
                if (number >= 0)
                {
                    mybrush = new SolidColorBrush(Colors.Black);
                }
                if (number < 0)
                {
                    mybrush = new SolidColorBrush(Colors.Red);
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class DataGridCellColorConverter : IMultiValueConverter
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
            DataGridCell cell = (DataGridCell)values[0];
            DailyLMPDisplay dailyLmpDisplay = (DailyLMPDisplay)values[1];
            Int32 headerTest = -1;
            Int32.TryParse((string)cell.Column.Header, out headerTest);
            if (headerTest > 0)
            {
                if ((string)cell.Column.Header == "1" || (string)cell.Column.Header == "2" || (string)cell.Column.Header == "3" ||
                   (string)cell.Column.Header == "4" || (string)cell.Column.Header == "5" || (string)cell.Column.Header == "6" ||
                   (string)cell.Column.Header == "7" || (string)cell.Column.Header == "8" || (string)cell.Column.Header == "9" ||
                   (string)cell.Column.Header == "10" || (string)cell.Column.Header == "11" || (string)cell.Column.Header == "12" ||
                   (string)cell.Column.Header == "13" || (string)cell.Column.Header == "14" || (string)cell.Column.Header == "15" ||
                   (string)cell.Column.Header == "16" || (string)cell.Column.Header == "17" || (string)cell.Column.Header == "18" ||
                   (string)cell.Column.Header == "19" || (string)cell.Column.Header == "20" || (string)cell.Column.Header == "21" ||
                   (string)cell.Column.Header == "22" || (string)cell.Column.Header == "23" || (string)cell.Column.Header == "24" ||
                   (string)cell.Column.Header == "25" || (string)cell.Column.Header == "26" || (string)cell.Column.Header == "27" ||
                   (string)cell.Column.Header == "28" || (string)cell.Column.Header == "29" || (string)cell.Column.Header == "30" || (string)cell.Column.Header == "31")
                {
                    if (dailyLmpDisplay.PeakHash.ContainsKey(headerTest))
                    {
                        if (dailyLmpDisplay.PeakHash[headerTest])
                        {
                            mybrush = new SolidColorBrush(Colors.LightGoldenrodYellow);
                        }
                        else
                        {
                            mybrush = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }
            }
            return mybrush;
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
    public class DataGridCellForeColorConverter : IMultiValueConverter
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
            var cell = (DataGridCell)values[0];
            DailyLMPDisplay node = (DailyLMPDisplay)values[1];
            double number = 0;
            if ((string)cell.Column.Header == "Max")
            {
                if (node.Max != null)
                {
                    number = (double)node.Max;
                }
            }
            if ((string)cell.Column.Header == "Min")
            {
                if (node.Min != null)
                {
                    number = (double)node.Min;
                }
            }
            if ((string)cell.Column.Header == "Avg")
            {
                if (node.Avg != null)
                {
                    number = (double)node.Avg;
                }
            }
            if ((string)cell.Column.Header == "1")
            {
                if (node.D1 != null)
                {
                    number = (double)node.D1;
                }
            }
            if ((string)cell.Column.Header == "2")
            {
                if (node.D2 != null)
                {
                    number = (double)node.D2;
                }
            }
            if ((string)cell.Column.Header == "3")
            {
                if (node.D3 != null)
                {
                    number = (double)node.D3;
                }
            }
            if ((string)cell.Column.Header == "4")
            {
                if (node.D4 != null)
                {
                    number = (double)node.D4;
                }
            }
            if ((string)cell.Column.Header == "5")
            {
                if (node.D5 != null)
                {
                    number = (double)node.D5;
                }
            }
            if ((string)cell.Column.Header == "6")
            {
                if (node.D6 != null)
                {
                    number = (double)node.D6;
                }
            }
            if ((string)cell.Column.Header == "7")
            {
                if (node.D7 != null)
                {
                    number = (double)node.D7;
                }
            }
            if ((string)cell.Column.Header == "8")
            {
                if (node.D8 != null)
                {
                    number = (double)node.D8;
                }
            }
            if ((string)cell.Column.Header == "9")
            {
                if (node.D9 != null)
                {
                    number = (double)node.D9;
                }
            }
            if ((string)cell.Column.Header == "10")
            {
                if (node.D10 != null)
                {
                    number = (double)node.D10;
                }
            }
            if ((string)cell.Column.Header == "11")
            {
                if (node.D11 != null)
                {
                    number = (double)node.D11;
                }
            }
            if ((string)cell.Column.Header == "12")
            {
                if (node.D12 != null)
                {
                    number = (double)node.D12;
                }
            }
            if ((string)cell.Column.Header == "13")
            {
                if (node.D13 != null)
                {
                    number = (double)node.D13;
                }
            }
            if ((string)cell.Column.Header == "14")
            {
                if (node.D14 != null)
                {
                    number = (double)node.D14;
                }
            }
            if ((string)cell.Column.Header == "15")
            {
                if (node.D15 != null)
                {
                    number = (double)node.D15;
                }
            }
            if ((string)cell.Column.Header == "16")
            {
                if (node.D16 != null)
                {
                    number = (double)node.D16;
                }
            }
            if ((string)cell.Column.Header == "17")
            {
                if (node.D17 != null)
                {
                    number = (double)node.D17;
                }
            }
            if ((string)cell.Column.Header == "18")
            {
                if (node.D18 != null)
                {
                    number = (double)node.D18;
                }
            }
            if ((string)cell.Column.Header == "19")
            {
                if (node.D19 != null)
                {
                    number = (double)node.D19;
                }
            }
            if ((string)cell.Column.Header == "20")
            {
                if (node.D20 != null)
                {
                    number = (double)node.D20;
                }
            }
            if ((string)cell.Column.Header == "21")
            {
                if (node.D21 != null)
                {
                    number = (double)node.D21;
                }
            }
            if ((string)cell.Column.Header == "22")
            {
                if (node.D22 != null)
                {
                    number = (double)node.D22;
                }
            }
            if ((string)cell.Column.Header == "23")
            {
                if (node.D23 != null)
                {
                    number = (double)node.D23;
                }
            }
            if ((string)cell.Column.Header == "24")
            {
                if (node.D24 != null)
                {
                    number = (double)node.D24;
                }
            }
            if ((string)cell.Column.Header == "25")
            {
                if (node.D25 != null)
                {
                    number = (double)node.D25;
                }
            }
            if ((string)cell.Column.Header == "26")
            {
                if (node.D26 != null)
                {
                    number = (double)node.D26;
                }
            }
            if ((string)cell.Column.Header == "27")
            {
                if (node.D27 != null)
                {
                    number = (double)node.D27;
                }
            }
            if ((string)cell.Column.Header == "28")
            {
                if (node.D28 != null)
                {
                    number = (double)node.D28;
                }
            }
            if ((string)cell.Column.Header == "29")
            {
                if (node.D29 != null)
                {
                    number = (double)node.D29;
                }
            }
            if ((string)cell.Column.Header == "30")
            {
                if (node.D30 != null)
                {
                    number = (double)node.D30;
                }
            }
            if ((string)cell.Column.Header == "31")
            {
                if (node.D31 != null)
                {
                    number = (double)node.D31;
                }
            }
            if (number < 0)
            {
                mybrush = new SolidColorBrush(Colors.Red);
            }
            return mybrush;
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
