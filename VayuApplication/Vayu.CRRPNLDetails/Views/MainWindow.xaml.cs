
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Vayu.CRRPNLDetails.ViewModels;

namespace Vayu.CRRPNLDetails.Views
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
        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ParticipantList.SelectAll();
            FocusManager.SetFocusedElement(this, ParticipantList);
            var dataContext = this.DataContext as MainWindowViewModel;
            if (dataContext != null)
            {
                dataContext.SelectAllParticipants();
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ParticipantList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ParticipantList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as MainWindowViewModel;
            List<string> selectedItems = new List<string>();
            foreach (var item in ParticipantList.SelectedItems)
            {
                selectedItems.Add((string)item);
            }
            if (dataContext != null)
            {
                dataContext.SelectedParticipant(selectedItems);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as MainWindowViewModel;
            List<string> selectedItems = new List<string>();
            foreach (var item in ParticipantsSelectedList.SelectedItems)
            {
                selectedItems.Add((string)item);
            }
            if (dataContext != null)
            {
                dataContext.DeleteParticipant(selectedItems);
            }
        }
        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SelectedMonthList.SelectAll();
            FocusManager.SetFocusedElement(this, SelectedMonthList);
            var dataContext = this.DataContext as MainWindowViewModel;
            if (dataContext != null)
            {
                dataContext.SelectAllDates();
            }
        }

        /// <summary>
        /// Handles the 1 event of the ListBox_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ListBox_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as MainWindowViewModel;
            List<string> selectedItems = new List<string>();
            foreach (var item in DateSelectedList.SelectedItems)
            {
                selectedItems.Add((string)item);
            }
            if (dataContext != null)
            {
                dataContext.DeleteDates(selectedItems);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the SelectedMonthList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void SelectedMonthList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as MainWindowViewModel;
            List<string> selectedItems = new List<string>();
            foreach (var item in SelectedMonthList.SelectedItems)
            {
                selectedItems.Add((string)item);
            }
            if (dataContext != null)
            {
                dataContext.SelectedDates(selectedItems);
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

        /// <summary>
        /// Handles the Sorting event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.DataGridSortingEventArgs"/> instance containing the event data.</param>
        private void DataGrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            e.Handled = true;
            var dataContext = this.DataContext as MainWindowViewModel;
            if (dataContext != null)
            {
                dataContext.mSortOrder = !dataContext.mSortOrder;
                if (dataContext.mSortOrder)
                {
                    orderDirection = ListSortDirection.Ascending;
                }
                else
                {
                    orderDirection = ListSortDirection.Descending;
                }
                dataContext.SortCondition = e.Column.SortMemberPath;
                dataContext.HandleSort(dataContext.FTRDetailsDataList, orderDirection);
            }
        }

        //private void MWHGrid_Sorting(object sender, DataGridSortingEventArgs e)
        //{
        //    e.Handled = true;
        //    var dataContext = this.DataContext as MainViewModel;
        //    if (dataContext != null)
        //    {
        //        dataContext.mSortOrder = !dataContext.mSortOrder;
        //        if (dataContext.mSortOrder)
        //        {
        //            orderDirection = ListSortDirection.Ascending;
        //        }
        //        else
        //        {
        //            orderDirection = ListSortDirection.Descending;
        //        }
        //        dataContext.SortCondition = e.Column.SortMemberPath;
        //        dataContext.HandleSort(dataContext.FTRDetailsDataList, orderDirection);
        //    }
        //}



        //private void DApric_Click(object sender, RoutedEventArgs e)
        //{
        //    if(DApric.IsChecked==true)
        //    {
        //        DaPNLCostGrid.Visibility = Visibility.Visible;
        //        MWHGrid.Visibility = Visibility.Hidden;

        //    }
        //}

        private void MWhpric_Click(object sender, RoutedEventArgs e)
        {
            if (MWhpric.IsChecked == true)
            {
                //MWHGrid.Visibility = Visibility.Visible;
                //DaPNLCostGrid.Visibility = Visibility.Hidden;
            }


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
                    return (val.HasValue ? string.Format("{0,0:C2}", val) : "");
                }
                else
                    return null;
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
        }
    }
}
