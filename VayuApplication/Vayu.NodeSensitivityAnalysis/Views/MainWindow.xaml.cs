using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Vayu.CommonControls;
using Vayu.NodeSensitivityAnalysis.Model;
using Vayu.NodeSensitivityAnalysis.ViewModels;

namespace Vayu.NodeSensitivityAnalysis.Views
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
        #region Property

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (SourceSearchBox.SelectedValue != null && SinkSearchBox.SelectedValue != null)
            {
                constraintDataGrid.Columns[1].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[2].Visibility = Visibility.Visible;
            }
            else
            {
                constraintDataGrid.Columns[1].Visibility = Visibility.Visible;
                constraintDataGrid.Columns[2].Visibility = Visibility.Visible;
            }
        }

        private void sourceBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.IsDropDownOpen = true;
            comboBox.SelectedItem = "AEEC";
            string searchText = comboBox.Text + e.Text;
            //comboBox.Items.Filter = node => ((Item)node).NodeName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        private void constraintDataGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            MainWindowViewModel dx = DataContext as MainWindowViewModel;
            if (dx != null)
            {
                if (constraintDataGrid.SelectedCells != null && constraintDataGrid.SelectedCells.Count > 0)
                {
                    var item = constraintDataGrid.SelectedCells[0].Item as Vayu.NodeSensitivityAnalysis.Model.Constraint;
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
        private void NodeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SinkSearchBox.IsEnabled = true;
        }

        private void Clear_Button(object sender, RoutedEventArgs e)
        {
            SourceSearchBox.SelectedItem = null;
            SinkSearchBox.SelectedItem = null;
            //constraintDataGrid.ItemsSource = null;
            constraintDataGrid.Items.Refresh();
        }

        private void EXpostToExcel(object sender, RoutedEventArgs e)
        {
            ExportToExcelNodeSpread<Constraint, List<Constraint>> p = new ExportToExcelNodeSpread<Constraint, List<Constraint>>();

            ICollectionView view = CollectionViewSource.GetDefaultView(constraintDataGrid.ItemsSource);
            if (constraintDataGrid.Items.Count > 0)
            {
                view = CollectionViewSource.GetDefaultView(constraintDataGrid.ItemsSource);
            }
            List<Constraint> itemList = new List<Constraint>();
            foreach (var item in view.SourceCollection)
            {
                itemList.Add((Constraint)item);
            }

            p.dataToPrint = itemList;
            p.GenerateReport();

        }

        private void PathUnchecked(object sender, RoutedEventArgs e)
        {
            this.SinkSearchBox.IsEnabled = false;
        }
        #endregion
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

    public class Item
    {
        public string NodeName { get; set; }
    }
}
