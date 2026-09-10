
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.DBLibrary;
using Vayu.LTC_PortfolioAnnual.Model;
using Vayu.LTC_PortfolioAnnual.ViewModels;

namespace Vayu.LTC_PortfolioAnnual.Views
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDataService dataService;

        public MainWindow()
        {
            InitializeComponent();
            dataService = new DataService(); // Ensure this is initialized
            this.DataContext = new MainWindowViewModel(dataService);
        }

        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }
        private void selectionISOChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var datacontext = this.DataContext as MainWindowViewModel;
            if (datacontext != null)
            {
                if (datacontext.MarketComboSelectedValue == "ERCOT")
                {
                    PeakWEname.Visibility = Visibility.Visible;
                }
            }
        }
        /// <summary>
        /// Handles the selected event of the DataGrid control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                CRRTransaction transaction = item as CRRTransaction;
                transaction.IsSelected = true;
            }
            foreach (var item in e.RemovedItems)
            {
                CRRTransaction transaction = item as CRRTransaction;
                transaction.IsSelected = false;
            }
        }

        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PathGraphsTab.IsSelected = true;
        }

        private void ExportAll_Click(object sender, RoutedEventArgs e)
        {
            Vayu.CommonControls.ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>> s =
                           new Vayu.CommonControls.ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>>();
            ICollectionView view = CollectionViewSource.GetDefaultView(DayComparisonListView.ItemsSource);
            List<HourlyPivotData> itemlist = new List<HourlyPivotData>();
            foreach (var item in view.SourceCollection)
            {
                itemlist.Add((HourlyPivotData)item);
            }
            s.dataToPrint = itemlist;
            s.GenerateReport();
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[6].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[7].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[8].Visibility = System.Windows.Visibility.Hidden;
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[6].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[7].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[8].Visibility = System.Windows.Visibility.Hidden;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Visible;
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Visible;
            ExposureList.Columns[6].Visibility = System.Windows.Visibility.Visible;
            ExposureList.Columns[7].Visibility = System.Windows.Visibility.Visible;
            ExposureList.Columns[8].Visibility = System.Windows.Visibility.Visible;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Hidden;
            ExposureList.Columns[1].Visibility = System.Windows.Visibility.Visible;
            ExposureList.Columns[6].Visibility = System.Windows.Visibility.Visible;
            ExposureList.Columns[7].Visibility = System.Windows.Visibility.Visible;
            ExposureList.Columns[8].Visibility = System.Windows.Visibility.Visible;
        }
        private void SinkRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[2].Visibility = System.Windows.Visibility.Visible;
            PathMWdatagrid.Columns[1].Visibility = System.Windows.Visibility.Hidden;
            PathMWdatagrid.Columns[3].Visibility = System.Windows.Visibility.Hidden;
            PathMWdatagrid.Columns[4].Visibility = System.Windows.Visibility.Visible;
        }

        private void SourceRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[1].Visibility = System.Windows.Visibility.Visible;
            PathMWdatagrid.Columns[2].Visibility = System.Windows.Visibility.Hidden;
            PathMWdatagrid.Columns[4].Visibility = System.Windows.Visibility.Hidden;
            PathMWdatagrid.Columns[3].Visibility = System.Windows.Visibility.Visible;
        }

        private void NoneRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[1].Visibility = System.Windows.Visibility.Visible;
            PathMWdatagrid.Columns[2].Visibility = System.Windows.Visibility.Visible;
            PathMWdatagrid.Columns[3].Visibility = System.Windows.Visibility.Visible;
            PathMWdatagrid.Columns[4].Visibility = System.Windows.Visibility.Visible;
        }

        private void OffPeakChk_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[6].Visibility = System.Windows.Visibility.Visible;
        }

        private void PeakChk_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[5].Visibility = System.Windows.Visibility.Visible;
        }

        private void PeakChk_Unchecked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[5].Visibility = System.Windows.Visibility.Hidden;
        }

        private void OffPeakChk_Unchecked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[6].Visibility = System.Windows.Visibility.Hidden;
        }

        private void PeakWEChk_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[7].Visibility = System.Windows.Visibility.Visible;
        }

        private void PeakWEChk_Unchecked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[7].Visibility = System.Windows.Visibility.Hidden;
        }
        public bool isdark { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class TextBlockBackColorConverter : IMultiValueConverter
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
            Color mybrush = Color.FromRgb(255, 255, 255);
            try
            {
                int sumTotFactorShift = 0;
                // capture input values to coloring algorithm
                // values[0] is factors above numbers
                double[] testFactorsAbove = new double[] { };
                if (values[0] != DependencyProperty.UnsetValue && values[0] != null)
                {
                    if (((double[])values[0]).Length >= 10)
                    {
                        testFactorsAbove = (double[])values[0];
                    }
                }
                // values[1] is factors below numbers
                double[] testFactorsBelow = new double[] { };
                if (values[1] != DependencyProperty.UnsetValue && values[1] != null)
                {
                    if (((double[])values[1]).Length >= 10)
                    {
                        testFactorsBelow = (double[])values[1];
                    }
                }
                //DataGridCell cell = null;
                string cellHeader = "";
                Int32 cellHeaderTest = -1;
                if (!values[2].Equals("")) // cell object value
                {
                    //cell = (DataGridCell)values[2];
                    cellHeader = values[2].ToString(); //(string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                HourlyPivotData node = null;
                if (values[3] != DependencyProperty.UnsetValue) // itemsource value
                {
                    node = (HourlyPivotData)values[3];
                }
                double textValue = 0;
                if (values[4] != DependencyProperty.UnsetValue) // cell text value if any
                {
                    double.TryParse((string)values[4], out textValue);
                    //Trace.WriteLine(values[3]);
                }
                bool isRowWeekend = false;
                if (node.Date != null)
                {
                    isRowWeekend = (((DateTime)node.Date).DayOfWeek == DayOfWeek.Saturday || ((DateTime)node.Date).DayOfWeek == DayOfWeek.Sunday);
                }

                if (isRowWeekend)
                {
                    mybrush = Color.FromRgb(211, 211, 211);
                }
                //if (node.RowName != "Spread" || node.Date == null) // cj todo: take out node.date == null check. allow spread avg in summary to show heatmap colors, but not spread total in summary.
                //{
                //    return mybrush;
                //}
                // 
                if (cellHeader == "")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader == "Day")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader == "Date")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader.Contains("Avg") || cellHeader.Contains("Tot") || cellHeaderTest != -1)
                {
                    if (testFactorsAbove.Length >= 10)
                    {
                        if (cellHeader.Contains("Tot"))
                        {
                            sumTotFactorShift = 5;
                        }
                        if (textValue > testFactorsAbove[sumTotFactorShift + 0]) // *15
                        {
                            mybrush = Color.FromRgb(13, 138, 0);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 1]) // *10
                        {
                            mybrush = Color.FromRgb(50, 158, 38);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 2]) // *6
                        {
                            mybrush = Color.FromRgb(20, 209, 0);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 3]) // *2
                        {
                            mybrush = Color.FromRgb(77, 233, 60);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 4]) // *1
                        {
                            mybrush = Color.FromRgb(120, 233, 108);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 0]) // *-15
                        {
                            mybrush = Color.FromRgb(165, 0, 8);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 1]) // *-10
                        {
                            mybrush = Color.FromRgb(190, 46, 53);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 2]) // *-6
                        {
                            mybrush = Color.FromRgb(251, 0, 13);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 3]) // *-2
                        {
                            mybrush = Color.FromRgb(253, 65, 75);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 4]) // *-1
                        {
                            mybrush = Color.FromRgb(253, 118, 125);
                        }
                        else
                        {
                            // todo: add weekend gray/white logic here
                        }
                    }
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class TextBlockForeColorConverter : IMultiValueConverter
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
            Color mybrush = Color.FromRgb(0, 0, 0);
            try
            {
                // capture input values to coloring algorithm
                double[] testFactorsAbove = new double[] { };
                if (values[0] != DependencyProperty.UnsetValue && values[0] != null)
                {
                    if (((double[])values[0]).Length >= 10)
                    {
                        testFactorsAbove = (double[])values[0];
                    }
                }
                double[] testFactorsBelow = new double[] { };
                if (values[1] != DependencyProperty.UnsetValue && values[1] != null)
                {
                    if (((double[])values[1]).Length >= 10)
                    {
                        testFactorsBelow = (double[])values[1];
                    }
                }
                //DataGridCell cell = null;
                string cellHeader = "";
                int cellHeaderTest = -1;
                if (values[2] != DependencyProperty.UnsetValue) // cell object value to get header names
                {
                    //cell = (DataGridCell)values[2];
                    cellHeader = values[2].ToString();//(string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                HourlyPivotData node = null;
                if (values[3] != DependencyProperty.UnsetValue) // itemsource value to get date for row
                {
                    node = (HourlyPivotData)values[3];
                }
                double textValue = 0;
                if (values[4] != DependencyProperty.UnsetValue) // cell text value to get numeric value to test
                {
                    double.TryParse((string)values[4], out textValue);
                    //Trace.WriteLine(values[3]);
                }
                if (cellHeader.Contains("Avg") || cellHeader.Contains("Tot") || cellHeaderTest != -1)
                {
                    if (textValue >= 0)
                    {
                        mybrush = Color.FromRgb(0, 0, 0);
                    }
                    else if (testFactorsAbove.Length >= 10 && node != null && node.Date != null)
                    {
                        if (cellHeader.Contains("Tot") && textValue < testFactorsBelow[7])
                        {
                            mybrush = Color.FromRgb(255, 255, 255);
                        }
                        else if (cellHeader.Contains("Tot") && textValue > testFactorsAbove[7] && textValue < testFactorsAbove[9])
                        {
                            mybrush = Color.FromRgb(0, 0, 0);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue < testFactorsBelow[2]) // m*-1
                        {
                            mybrush = Color.FromRgb(255, 255, 255);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue > testFactorsBelow[2] && textValue < testFactorsBelow[4]) // between m*-1 and m*-6
                        {
                            mybrush = Color.FromRgb(0, 0, 0);
                        }
                        else if (textValue < 0)
                        {
                            mybrush = Color.FromRgb(255, 0, 0);
                        }
                    }
                    else if (textValue < 0)
                    {
                        mybrush = Color.FromRgb(255, 0, 0);
                    }
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

    public class DataGridCellForeColorConverterSimple : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
            double number;
            if (value != null)
            {
                Double.TryParse(value.ToString(), out number);
                //Double.TryParse((string)value, out number);
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

    }

}
