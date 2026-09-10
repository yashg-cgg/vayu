
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.ConstraintSensitivityAlgorithm.Model;
using Vayu.ConstraintSensitivityAlgorithm.ViewModels;

namespace Vayu.ConstraintSensitivityAlgorithm.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            List<ConstraintContingency> pList = System.Linq.Enumerable.Cast<ConstraintContingency>(ConstraintTable.Items).ToList();
            List<string> names = new List<string>();
            List<string> contNames = new List<string>();
            foreach (ConstraintContingency cctlist in pList)
            {
                string contingency = cctlist.Contingency;
                contNames.Add(contingency);
                //resultsDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            }
            //Contingency_AutoCompleteBox.ItemsSource = contNames;


            foreach (ConstraintContingency cclist in pList)
            {
                string Constraint = cclist.Constraint;

                names.Add(Constraint);
            }
            //Constraint_AutoCompleteBox.ItemsSource = names;

        }
        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        public void SetPosition(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the ConstraintTable control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        public void ConstraintTable_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as MainWindowViewModel;
            List<ConstraintContingency> selectedItems = new List<ConstraintContingency>();
            foreach (var item in ConstraintTable.SelectedItems)
            {
                selectedItems.Add((ConstraintContingency)item);
            }
            dataContext.SelectConstraintCongingencyList = selectedItems;
        }
        /// <summary>
        /// Handles the SelectionChanged event of the AddedConstraintTable control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        public void AddedConstraintTable_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as MainWindowViewModel;
            List<ConstraintContingency> selectedItems = new List<ConstraintContingency>();
            foreach (var item in AddedConstraintTable.SelectedItems)
            {
                selectedItems.Add((ConstraintContingency)item);
            }
            dataContext.SelectAddedConstraintCongingencyList = selectedItems;
        }

        /// <summary>
        /// Handles the Click event of the LMPGraph control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void LMPGraph_Click(object sender, RoutedEventArgs e)
        {
            if (resultsDataGrid.SelectedCells != null && resultsDataGrid.SelectedCells.Count > 0)
            {
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowLMPGraphs("");
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
            if (resultsDataGrid.SelectedCells != null && resultsDataGrid.SelectedCells.Count >= 1)
            {
                string column = resultsDataGrid.SelectedCells[0].Column.Header.ToString();
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowNodeAnalyzer();
                }
            }
        }
        /// <summary>
        /// Handles the SelectedCellsChanged event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedCellsChangedEventArgs"/> instance containing the event data.</param>
        public void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            List<int> rowIndex = new List<int>();
            MainWindowViewModel dataContext = DataContext as MainWindowViewModel;
            List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> selectedItems = new List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector>();
            List<Tuple<string, string>> sourceSinkList = new List<Tuple<string, string>>();
            foreach (var cell in resultsDataGrid.SelectedCells)
            {
                DataGridCell cellItem = Vayu.CommonControls.DataGridInfo.GetCell(cell);
                int index = Vayu.CommonControls.DataGridInfo.GetRowIndex(cellItem);
                if (!rowIndex.Contains(index))
                {
                    Vayu.ConstraintSensitivityAlgorithm.Model.Vector node = (Vayu.ConstraintSensitivityAlgorithm.Model.Vector)cell.Item;
                    sourceSinkList.Add(new Tuple<string, string>(node.Source, node.Sink));
                    rowIndex.Add(index);
                }
            }
            dataContext.SetSourceSinks(sourceSinkList);
        }
        /// <summary>
        /// Handles the SelectionChanged event of the Constraint_AutoCompleteBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Constraint_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoCompleteBox auto = (AutoCompleteBox)sender;
            try
            {
                if (auto.SelectedItem != null)
                {
                    string selectedItem = auto.SelectedItem.ToString();
                    ConstraintTable.SelectedItem = (DataContext as MainWindowViewModel).ConstraintList.Where(a => a.Constraint == selectedItem).FirstOrDefault();
                    ConstraintTable.ScrollIntoView(ConstraintTable.SelectedItem);
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// Handles the SelectionChanged event of the Contingency_AutoCompleteBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Contingency_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoCompleteBox auto = (AutoCompleteBox)sender;
            try
            {
                if (auto.SelectedItem != null)
                {
                    string selectedItem = auto.SelectedItem.ToString();
                    ConstraintTable.SelectedItem = (DataContext as MainWindowViewModel).ConstraintList.Where(a => a.Contingency == selectedItem).FirstOrDefault();
                    ConstraintTable.ScrollIntoView(ConstraintTable.SelectedItem);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handles the 1 event of the SearchUserControl_TextUpdatedEvent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SearchUserControl_TextUpdatedEvent_1(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
            if (datacontext != null && datacontext.ConstraintList != null)
            {
                ConstraintContingency tempItem = sender as ConstraintContingency;
                if (tempItem != null)
                {
                    List<ConstraintContingency> templIst = datacontext.ConstraintList.ToList();
                    Parallel.ForEach(templIst, a =>
                    {
                        if ((a.Constraint == tempItem.Constraint) && (a.Contingency == tempItem.Contingency))
                        {
                            a.IsSelected = true;
                        }
                        else
                        {
                            a.IsSelected = false;
                        }
                    });
                    datacontext.ConstraintList = templIst.ToList();
                }
            }
        }

        #endregion

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
            Vayu.ConstraintSensitivityAlgorithm.Model.Vector vector = (Vayu.ConstraintSensitivityAlgorithm.Model.Vector)values[1];
            string headerText = string.Empty;

            if (cell.Column.Header is TextBlock)
                headerText = (cell.Column.Header as TextBlock).Text;
            else
                headerText = cell.Column.Header as string;

            double number = 0;
            if (headerText == "Source Sensitivity" || headerText == "Node Sensitivity")
            {
                if (vector.SourceSensitivity != null)
                {
                    number = vector.SourceSensitivity;
                }
            }
            if (headerText == "Sink Sensitivity")
            {
                if (vector.SinkSensitivity != null)
                {
                    number = vector.SinkSensitivity;
                }
            }
            if (headerText == "Sensitivity")
            {
                if (vector.Sensitivity != null)
                {
                    number = vector.Sensitivity;
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