
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.ConstraintExposure.ViewModels;

namespace Vayu.ConstraintExposure.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ListSortDirection listSortDirection;
        private bool IsSortingInitialized = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void FetchData()
        {
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
        #region Events

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (tbDays.Text.Length > 2)
            {
                MessageBox.Show("Please do not enter Day Lookback greater than 2 digits.  Program will now exit.");
            }
            FetchData();
        }
        private void cbFamily_Checked(object sender, RoutedEventArgs e)
        {
            capture.SelectedValue = "MWs";
            RTSettleDatePicker.IsEnabled = false;
            cbLookBack.IsChecked = false;
            cbLookBack.IsEnabled = false;
            tbDays.Text = "";
            tbDays.IsEnabled = false;
        }
        private void cbFamily_Unchecked(object sender, RoutedEventArgs e)
        {
            capture.SelectedValue = "Dollars";
            RTSettleDatePicker.IsEnabled = true;
            cbLookBack.IsEnabled = true;
            tbDays.IsEnabled = true;
            cbBest.IsChecked = false;
        }

        private void Window_Activated(object sender, EventArgs e)

        {
            Vayu.ConstraintExposure.ViewModels.MainWindowViewModel currentModel = DataContext as Vayu.ConstraintExposure.ViewModels.MainWindowViewModel;
            if (currentModel != null && currentModel.SourceComboSelectedItem != null)
            {
                if (!string.IsNullOrEmpty(currentModel.SourceComboSelectedItem.NodeName) && sourceComboBox.IsEnabled == true)
                {
                    //nodalComboBox.Text = currentModel.SourceComboSelectedItem.NodeName;
                    //sourceComboBox.Text=currentModel.SourceNodeComboSelectedItem.NodeName;
                }
            }
        }

        private void sourceComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }
        private void sinkComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }

        //private void nodalComboBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    ((ComboBox)sender).IsDropDownOpen = true;
        //}

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (capture.SelectedValue as string == "Dollars")
            {
                cbLookBack.IsEnabled = false;
                cbLookBack.IsChecked = false;
                tbDays.Text = string.Empty;
                tbDays.IsEnabled = false;
                //    DASettleDatePicker.IsEnabled = false;
                //  DASettleDatePicker.IsEnabled = true;
                //rbINCDEC.IsEnabled = false;
                cbFamily.IsEnabled = false;
                FamilyComboBox.IsEnabled = false;
                FamilyComboBox.SelectedItem = null;
                cbBest.IsEnabled = false;
            }
            else
            {
                cbLookBack.IsEnabled = true;
                tbDays.IsEnabled = true;
                tbDays.Text = string.Empty;
                DASettleDatePicker.IsEnabled = false;
                cbFamily.IsEnabled = true;
                FamilyComboBox.IsEnabled = true;
                cbBest.IsEnabled = true;
                if (virtualType.Text == "Node")
                    rbINCDEC.IsEnabled = true;

                /// DASettleDatePicker.IsEnabled = true;
                // RTSettleDatePicker.IsEnabled = true;
            }

            /*IsEnabled*/

            if (rbRTDA.SelectedItem == "RT")
            {
                DASettleDatePicker.IsEnabled = false;
                RTSettleDatePicker.IsEnabled = true;

            }

            if (rbRTDA.SelectedItem == "DA")
            {
                DASettleDatePicker.IsEnabled = true;
                RTSettleDatePicker.IsEnabled = false;

            }
        }

        private void mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mode.SelectedValue as string == "Virtuals")
            {
                virtualType.IsEnabled = true;
                // nodalComboBox.IsEnabled = true;
                rbINCDEC.IsEnabled = true;

                if (capture.SelectedValue as string == "Dollars")
                {
                    FamilyComboBox.IsEnabled = true;
                    FamilyComboBox.SelectedItem = null;
                    cbFamily.IsEnabled = true;
                    cbFamily.IsChecked = false;
                    cbBest.IsEnabled = true;
                    cbBest.IsChecked = false;
                }
                else
                    capture.SelectedValue = string.Empty;

                BtnPaste.IsEnabled = true;
                //addSourceSinkButton.IsEnabled = true;
                //removeSourceSinkButton.IsEnabled = true;
                //swapSourceSinkButton.IsEnabled = true;
            }
            else
            {
                virtualType.SelectedItem = null;
                virtualType.IsEnabled = false;
                rbINCDEC.SelectedItem = null;
                rbINCDEC.IsEnabled = false;
                //nodalComboBox.IsEnabled = false;
                FamilyComboBox.IsEnabled = true;
                FamilyComboBox.SelectedItem = null;
                cbFamily.IsEnabled = true;
                cbBest.IsEnabled = true;
                sourceComboBox.IsEnabled = true;
                sinkComboBox.IsEnabled = true;
                //nodalComboBox.SelectedItem = null;
                if (cbFamily.IsChecked == true)
                {
                    cbFamily.IsChecked = false;
                }
                if (cbBest.IsChecked == true)
                {
                    cbBest.IsChecked = false;
                }
                BtnPaste.IsEnabled = true;
                //addSourceSinkButton.IsEnabled = true;
                //removeSourceSinkButton.IsEnabled = true;
                //swapSourceSinkButton.IsEnabled = true;
                DASettleDatePicker.IsEnabled = false;
            }
        }

        private void virtualType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((virtualType.SelectedValue as string) == "Nodal" && virtualType.IsEnabled)
            {
                //nodalComboBox.IsEnabled = true;
                sourceComboBox.IsEnabled = true;
                sinkComboBox.IsEnabled = true;
                BtnPaste.IsEnabled = false;
                sinkComboBox.IsEnabled = false;
                rbINCDEC.IsEnabled = true;
                //addSourceSinkButton.IsEnabled = false;
                //removeSourceSinkButton.IsEnabled = false;
                //swapSourceSinkButton.IsEnabled = false;
            }
            else
            {
                rbINCDEC.IsEnabled = false;
                sourceComboBox.IsEnabled = true; ;
                sinkComboBox.IsEnabled = true;
                BtnPaste.IsEnabled = true;
                //addSourceSinkButton.IsEnabled = true;
                //removeSourceSinkButton.IsEnabled = true;
                //swapSourceSinkButton.IsEnabled = true;
                //nodalComboBox.IsEnabled = false;
                //nodalComboBox.SelectedItem = null;
            }
        }

        private void DataGrid_LayoutUpdated(object sender, EventArgs e)
        {
            return;
            //MainViewModel viewModel = this.DataContext as MainViewModel;
            //
            //if (viewModel == null || viewModel.ConstraintsList == null || !IsSortingInitialized )// || !e.Column.SortDirection.HasValue)
            //    return;
            //
            //List<Constraints> dataList = dataGrid.DataContext as List<Constraints>;
            //List<Constraints> tempList = dataList.Where(x => x.summaryConstraintNum == 0).ToList();
            //List<Constraints> summaryList = dataList.Where(x => x.summaryConstraintNum != 0).ToList();
            //
            //tempList.AddRange(summaryList.OrderBy(x => x.summaryConstraintNum));
            //viewModel.ConstraintsList = new System.Collections.ObjectModel.ObservableCollection<Constraints>(tempList);
            //IsSortingInitialized = false;
            /*var obj = MyModel.MyList.FirstOrDefault(x => x.NodeName == "TOTAL");
            if (obj != null)
                MyModel.MyList.Remove(obj);

            
            if (sortOrder)
                MyModel.MyList = new HourlyPNLList(MyModel.MyList.OrderBy(x => x.SortValue(e.Column.Header as string )));
            else
                MyModel.MyList = new HourlyPNLList(MyModel.MyList.OrderByDescending(x => x.SortValue(e.Column.Header as string)));

            sortOrder = !sortOrder;
            e.Column.SortDirection = (sortOrder ? ListSortDirection.Ascending : ListSortDirection.Descending);

            if (obj != null)
                MyModel.MyList.Insert(0, obj);*/

        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            try
            {
                e.Handled = true;
                var datacontext = this.DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.mSortOrder = !datacontext.mSortOrder;
                    if (datacontext.mSortOrder)
                    {
                        listSortDirection = ListSortDirection.Ascending;
                    }
                    else
                    {
                        listSortDirection = ListSortDirection.Descending;
                    }
                    datacontext.SortCondition = e.Column.SortMemberPath;
                    datacontext.HandleSort(datacontext.ConstraintsList, listSortDirection);
                }

            }
            catch (Exception)
            {
                //throw;
            }
        }

        #endregion

        private void Markets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Markets.SelectedItem == "ERCOT")
            {
                rbRTDA.ItemsSource = new List<string>() { "DA", "RT" };
            }
            if (Markets.SelectedItem == "PJM")
            {
                rbRTDA.ItemsSource = new List<string>() { "RT" };
            }

        }

        private void rbRTDA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rbRTDA.SelectedItem == "RT")
            {
                DASettleDatePicker.IsEnabled = false;
                RTSettleDatePicker.IsEnabled = true;
                cbDateDARange.IsEnabled = false;
                label1.IsEnabled = false;
                DASettleDatePicker.IsEnabled = false;
            }

            if (rbRTDA.SelectedItem == "DA")
            {
                DASettleDatePicker.IsEnabled = true;
                RTSettleDatePicker.IsEnabled = false;
                cbDateDARange.IsEnabled = true;
                label1.IsEnabled = true;
                DASettleDatePicker.IsEnabled = true;

            }
        }
    }
    public class ForeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
                double number;
                if (value != null)
                {
                    value = value.ToString().Replace("$", "");
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
            catch (Exception)
            {
                return value;
                //throw;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
            //throw new System.NotImplementedException();
        }
    }

    public class TextColumnConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double? val = 0.0;
                if (value != null)
                {
                    val = double.Parse(value.ToString()) as double?;
                    return (val.HasValue ? string.Format("{0:#,##0.00;-#,##0.00;''}", val) : "");
                }
                else
                    return null;
            }
            catch (Exception)
            {
                return value;
                //throw;
            }
            //double.TryParse(value as string, out val);
            //if(val==0)
            //{
            //    return null;
            //}

            //DataGridRow row = value as DataGridRow;
            //Vayu.ConstraintExposure.ViewModel.Constraints constraint = row.Item as Vayu.ConstraintExposure.ViewModel.Constraints;
            //double? doub = constraint.GetType().GetProperty(parameter as string).GetValue(constraint) as double?;
            //return (doub.HasValue ? string.Format("{0:#,##0.00;-#,##0.00;''}", doub) : "");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
