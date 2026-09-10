
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.ConstraintContingencyHistory.Model;
using Vayu.ConstraintContingencyHistory.ViewModels;

namespace Vayu.ConstraintContingencyHistory.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        //private void SeasonButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button button && button.Tag != null)
        //    {
        //        string season = button.Tag.ToString();
        //        // Update the corresponding property in MainViewModel
        //        switch (season)
        //        {
        //            case "Winter":
        //                ViewModels.IsWinterClicked = true;
        //                break;
        //            case "Spring":
        //                ViewModels.IsSpringClicked = true;
        //                break;
        //            case "Summer":
        //                ViewModels.IsSummerClicked = true;
        //                break;
        //            case "Fall":
        //                ViewModels.IsFallClicked = true;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}
        #region Events

        private void constraintDataGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            MainWindowViewModel dx = DataContext as MainWindowViewModel;
            if (dx != null)
            {
                if (constraintDataGrid.SelectedCells != null && constraintDataGrid.SelectedCells.Count > 0)
                {
                    var item = constraintDataGrid.SelectedCells[0].Item as Vayu.ConstraintContingencyHistory.Model.Constraint;
                    dx.SelectedConstraint = item;
                    var column = (constraintDataGrid.SelectedCells[0].Column.Header as System.Windows.Controls.TextBlock);
                    if (column != null)
                    {
                        try
                        {
                            dx.ColumnSelected = item.GetType().GetProperty("HE" + column.Text);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContingencySearchBox.SelectedValue != null && ContingencySearchBox.SelectedValue != null)
            {
                constraintDataGrid.Columns[1].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[2].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[3].Visibility = Visibility.Collapsed;
                constraintDataGrid.Columns[4].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[5].Visibility = System.Windows.Visibility.Visible;
                constraintDataGrid.Columns[6].Visibility = System.Windows.Visibility.Visible;
                constraintDataGrid.Columns[6].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                constraintDataGrid.Columns[1].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[2].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[3].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[4].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[5].Visibility = System.Windows.Visibility.Visible;
                constraintDataGrid.Columns[6].Visibility = System.Windows.Visibility.Visible;
                constraintDataGrid.Columns[7].Visibility = System.Windows.Visibility.Visible;
            }

            if (marketname.SelectedItem == "SystemLambda")
            {

                constraintDataGrid.Columns[1].Visibility = Visibility.Collapsed;
                constraintDataGrid.Columns[2].Visibility = Visibility.Collapsed;
                constraintDataGrid.Columns[3].Visibility = Visibility.Collapsed;
                constraintDataGrid.Columns[4].Visibility = Visibility.Collapsed;
                constraintDataGrid.Columns[5].Visibility = System.Windows.Visibility.Visible;
                constraintDataGrid.Columns[6].Visibility = System.Windows.Visibility.Collapsed;
                constraintDataGrid.Columns[7].Visibility = System.Windows.Visibility.Collapsed;
                constraintDataGrid.Columns[8].Visibility = System.Windows.Visibility.Collapsed;

                ContingencySearchBox.IsEnabled = false;
                ConstraintSearchBox.IsEnabled = false;
                PasteButton.IsEnabled = false;

            }
            else
            {
                ContingencySearchBox.IsEnabled = true;
                ConstraintSearchBox.IsEnabled = true;
                PasteButton.IsEnabled = true;
            }
        }
        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            constraintDataGrid.Columns[1].Visibility = Visibility.Collapsed;
            constraintDataGrid.Columns[2].Visibility = Visibility.Collapsed;
            constraintDataGrid.Columns[3].Visibility = Visibility.Collapsed;
            constraintDataGrid.Columns[4].Visibility = Visibility.Collapsed;
        }

        #endregion

        private void SelectionChanged_Con(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DataService ds = new DataService();
            MainWindowViewModel dx = DataContext as MainWindowViewModel;

            if (dx.ConstraintChecked)
            {
                if (((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem != null)
                {
                    dx.SelectedConstraintItem = ((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString();
                    if (dx.RTChecked)
                        dx.ContingencySearchList = ds.GetAllContingencies(((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString(), true, dx.GetMarketKey());
                    else if (dx.DAChecked)
                        dx.ContingencySearchList = ds.GetAllContingencies(((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString(), false, dx.GetMarketKey());
                }
            }
            else if (dx.ContingencyChecked)
            {
                if (((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem != null)
                {
                    dx.SelectedConstraintItem = ((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString();
                    if (dx.RTChecked)
                        dx.ContingencySearchList = ds.GetAllConstraints(((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString(), true, dx.GetMarketKey());
                    else if (dx.DAChecked)
                        dx.ContingencySearchList = ds.GetAllConstraints(((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString(), false, dx.GetMarketKey());
                }
            }
        }

        private void ConstHistory_Click(object sender, RoutedEventArgs e)
        {
            //constraintDataGrid.Columns[3].Visibility = System.Windows.Visibility.Hidden;
            //constraintDataGrid.Columns[4].Visibility = System.Windows.Visibility.Hidden;
            //constraintDataGrid.Columns[5].Visibility = System.Windows.Visibility.Hidden;
            //constraintDataGrid.Columns[6].Visibility = System.Windows.Visibility.Hidden;
            //constraintDataGrid.Columns[7].Visibility = System.Windows.Visibility.Hidden;
        }


        public void WinterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {

                viewModel.IsWinterClicked = true;
                viewModel.IsSummerClicked = false;
                viewModel.IsSpringClicked = false;
                viewModel.IsFallClicked = false;
            }
            //  MainViewModel mv = DataContext as MainViewModel;
            //(DataContext as MainViewModel)?.IsWinterClicked = true;

            // bool rtChecked = mv.RTChecked;
            //bool daChecked = mv.DAChecked;
            // mv.RefreshList();


        }
        private void SpringButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Set IsWinterClicked to true
                viewModel.IsSpringClicked = true;
                viewModel.IsWinterClicked = false;
                viewModel.IsSummerClicked = false;
                viewModel.IsFallClicked = false;
            }

        }
        private void SummerButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Set IsWinterClicked to true
                viewModel.IsSummerClicked = true;
                viewModel.IsWinterClicked = false;
                viewModel.IsSpringClicked = false;
                viewModel.IsFallClicked = false;
            }


        }
        private void FallButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Set IsWinterClicked to true
                viewModel.IsFallClicked = true;
                viewModel.IsSummerClicked = false;
                viewModel.IsWinterClicked = false;
                viewModel.IsSpringClicked = false;
            }

        }



        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        //private void DateRangeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    groupBox14.Visibility = System.Windows.Visibility.Collapsed;
        //    groupBox15.Visibility = System.Windows.Visibility.Collapsed;
        //}

        private void constraintDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked element
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // Find the parent DataGridCell
            while (dep != null && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            // If a DataGridCell is found, execute the command
            if (dep is DataGridCell)
            {
                // Get the DataContext (your data item) from the DataGridCell
                Constraint selectedItem = ((DataGridCell)dep).DataContext as Constraint;

                // Execute the command from the ViewModel
                if (selectedItem != null)
                {
                    Constraint capturedSelectedItem = selectedItem;

                    ((MainWindowViewModel)DataContext).CellClicked(capturedSelectedItem);
                    // ((MainWindowViewModel)DataContext).CellClickCommand.Execute(capturedSelectedItem);
                }
            }
        }

        private void AllFrequencyButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            ((MainWindowViewModel)DataContext).AllButtonClickCommand.Execute(dep);
        }
    }
    class CurrencyColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                double val = 0;
                if (double.TryParse(value.ToString(), out val))
                {
                    if (val < 0)
                    {
                        return System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        return System.Windows.Media.Brushes.Black;
                    }
                }
                else
                {
                    return System.Windows.Media.Brushes.Black;
                }
            }
            else
            {
                return System.Windows.Media.Brushes.Black;
            }
        }





        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    class SPPMarketColumnVisibilityConverter : IValueConverter
    {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
