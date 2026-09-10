
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.CRRTopTenParticipants.Model;
using Vayu.CRRTopTenParticipants.ViewModels;

namespace Vayu.CRRTopTenParticipants.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ListSortDirection orderDirection;
        public MainWindow()
        {
            InitializeComponent();
        }
        public void SetPositions(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the mktComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void mktComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTopPNL != null)
            {
                if (mktComboBox.SelectedValue.ToString() != "PJM")
                {
                    dgTopPNL.Columns[13].Visibility = Visibility.Collapsed;
                    dgTopPNL.Columns[14].Visibility = Visibility.Collapsed;
                    dgTopPNL.Columns[15].Visibility = Visibility.Collapsed;
                    dgTopPNL.Columns[16].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgTopPNL.Columns[13].Visibility = Visibility.Visible;
                    dgTopPNL.Columns[14].Visibility = Visibility.Visible;
                    dgTopPNL.Columns[15].Visibility = Visibility.Visible;
                    dgTopPNL.Columns[16].Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Handles the Sorting event of the dgTopPNL control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridSortingEventArgs"/> instance containing the event data.</param>
        private void dgTopPNL_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            var DataContext = this.DataContext as MainWindowViewModel;
            if (DataContext != null)
            {
                DataContext.mSortOrder = !DataContext.mSortOrder;
                if (DataContext.mSortOrder)
                {
                    orderDirection = ListSortDirection.Ascending;
                }
                else
                {
                    orderDirection = ListSortDirection.Descending;
                }
                DataContext.SortCondition = e.Column.SortMemberPath;
                DataContext.HandleSort(DataContext.PnlInfoList, orderDirection);
            }
        }
    }
    public class CellBackgroundColorConverter : IValueConverter
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
                var objItem = value as DataItem;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "pnl":
                                if (objItem.PNL < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "mw":
                                if (objItem.MW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "monthly":
                                if (objItem.Monthly < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "annual":
                                if (objItem.Annual < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "q1":
                                if (objItem.Q1 < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "q2":
                                if (objItem.Q2 < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "q3":
                                if (objItem.Q3 < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "q4":
                                if (objItem.Q4 < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "yr1":
                                if (objItem.YR1 < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "yr2":
                                if (objItem.YR2 < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "yr3":
                                if (objItem.YR3 < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "yrall":
                                if (objItem.YRALL < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "daprc":
                                if (objItem.DAPrice < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "cost":
                                if (objItem.Cost < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            //case "costmonthly" :
                            //    if(objItem.CostMonthly<0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costannual":
                            //    if (objItem.CostAnnual < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costq1":
                            //    if (objItem.CostQ1 < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costq2":
                            //    if (objItem.CostQ2 < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costq3":
                            //    if (objItem.CostQ3 < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costq4":
                            //    if (objItem.CostQ4 < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costyr1":
                            //    if (objItem.CostYR1 < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costyr2":
                            //    if (objItem.CostYR2 < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costyr3":
                            //    if (objItem.CostYR3 < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            //case "costyrall":
                            //    if (objItem.CostYRALL < 0)
                            //    {
                            //        return new SolidColorBrush(Colors.Red);
                            //    }
                            //    return new SolidColorBrush(Colors.Black);
                            default:
                                return new SolidColorBrush();
                        }
                    }
                    else return new SolidColorBrush(Colors.Black);
                }
                else return new SolidColorBrush(Colors.Black);
            }
            else return new SolidColorBrush(Colors.Black);
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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class BooleanVisibilityConverter : IValueConverter
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
            if (value != null)
            {

            }
            return null;
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
            throw new NotImplementedException();
        }
    }
}
