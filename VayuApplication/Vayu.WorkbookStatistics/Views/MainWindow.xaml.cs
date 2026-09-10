
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;
using Vayu.CommonControls;
using Vayu.WorkbookStatistics.Model;
using Vayu.WorkbookStatistics.ViewModels;

namespace Vayu.WorkbookStatistics.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declaration

        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName;
        private ListSortDirection listSortDirection;
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;
        /// <summary>
        /// The m select name date range command
        /// </summary>
        private SqlCommand mSelectNameDateRangeCommand;
        /// <summary>
        /// The m select date date range command
        /// </summary>
        private SqlCommand mSelectDateDateRangeCommand;
        /// <summary>
        /// The m model
        /// </summary>
        private MainWindowViewModel mModel;
        /// <summary>
        /// The m different columns
        /// </summary>
        private bool mDifferentColumns = false;
        /// <summary>
        /// The m start copy column
        /// </summary>
        private int mStartCopyColumn = -1;
        /// <summary>
        /// The m colume name
        /// </summary>
        private string mColumeName = string.Empty;
        /// <summary>
        /// The is validated
        /// </summary>
        private bool isValidated = true;
        /// <summary>
        /// The m current sort column
        /// </summary>
        private DataGridColumn mCurrentSortColumn;
        /// <summary>
        /// The m current sort direction
        /// </summary>
        private ListSortDirection mCurrentSortDirection;

        #endregion
        public MainWindow(MainWindowViewModel workBookViewModel)
        {
            mModel = workBookViewModel;
            InitializeComponent();

            loadDBCommands();
        }

        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }
        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectNameDateRangeCommand = new SqlCommand();
            mSelectNameDateRangeCommand.CommandText = "select distinct datename from daterange where trader = @trader order by datename";
            mSelectNameDateRangeCommand.Parameters.AddWithValue("@trader", "trader");
            mSelectNameDateRangeCommand.Connection = VayuConnection;
            //
            mSelectDateDateRangeCommand = new SqlCommand();
            mSelectDateDateRangeCommand.CommandText = "select marketdate from daterange where trader = @trader and datename = @datename order by marketdate";
            mSelectDateDateRangeCommand.Parameters.AddWithValue("@trader", "trader");
            mSelectDateDateRangeCommand.Parameters.AddWithValue("@datename", "datename");
            mSelectDateDateRangeCommand.Connection = VayuConnection;
        }
        #region Events

        /// <summary>
        /// Handles the Click event of the LMPStatistics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void LMPStatistics_Click(object sender, RoutedEventArgs e)
        {
            if (PathDataGrid.SelectedCells != null && PathDataGrid.SelectedCells.Count >= 1)
            {
                string column = PathDataGrid.SelectedCells[0].Column.Header.ToString();
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowLMPStatistics();
                }
            }
        }
        /// <summary>
        /// Handles the Click event of the LMPGraph control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void LMPGraph_Click(object sender, RoutedEventArgs e)
        {
            if (PathDataGrid.SelectedCells != null && PathDataGrid.SelectedCells.Count > 0)
            {
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowLMPGraphs("");
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
            MainWindowViewModel dataContext = DataContext as MainWindowViewModel;
            if (dataContext.SelectChange)
            {
                List<int> rowIndex = new List<int>();
                List<WorkbookStatistics.ViewModels.Path> selectedItems = new List<WorkbookStatistics.ViewModels.Path>();
                List<Tuple<string, string>> sourceSinkList = new List<Tuple<string, string>>();
                List<Path> selectedPath = new List<Path>();
                foreach (var cell in PathDataGrid.SelectedCells)
                {
                    DataGridCell cellItem = Vayu.CommonControls.DataGridInfo.GetCell(cell);
                    int index = Vayu.CommonControls.DataGridInfo.GetRowIndex(cellItem);
                    if (!rowIndex.Contains(index))
                    {
                        WorkbookStatistics.ViewModels.Path node = (WorkbookStatistics.ViewModels.Path)cell.Item;
                        sourceSinkList.Add(new Tuple<string, string>(node.Source, node.Sink));
                        selectedPath.Add(cell.Item as Path);
                        rowIndex.Add(index);
                    }
                }
                dataContext.SelectedPathByCell = selectedPath;
                dataContext.SetSourceSinks(sourceSinkList);
            }
        }
        /// <summary>
        /// Handles the LoadingRow event of the PathGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridRowEventArgs"/> instance containing the event data.</param>
        private void PathGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        /// <summary>
        /// Handles the Click event of the PeakButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PeakButton_Click(object sender, RoutedEventArgs e)
        {
            SetHours("PEAK");
        }
        /// <summary>
        /// Handles the Click event of the OffPeakButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OffPeakButton_Click(object sender, RoutedEventArgs e)
        {
            SetHours("OFFPEAK");
        }
        /// <summary>
        /// Handles the Click event of the AllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            SetHours("ALL");
        }
        /// <summary>
        /// Sets the hours.
        /// </summary>
        /// <param name="hourType">Type of the hour.</param>
        private void SetHours(string hourType)
        {
            mModel.UpdateRefresh(false);
            if (hourType == "ALL" || hourType == "OFFPEAK")
            {
                HE1.IsChecked = true;
                HE2.IsChecked = true;
                HE3.IsChecked = true;
                HE4.IsChecked = true;
                HE5.IsChecked = true;
                HE6.IsChecked = true;
                if (marketComboBox.Text == "PJM" || hourType == "ALL")
                {
                    HE7.IsChecked = true;
                }
                else
                {
                    HE7.IsChecked = false;
                }
                HE24.IsChecked = true;
            }
            else
            {
                HE1.IsChecked = false;
                HE2.IsChecked = false;
                HE3.IsChecked = false;
                HE4.IsChecked = false;
                HE5.IsChecked = false;
                HE6.IsChecked = false;
                if (marketComboBox.Text == "PJM")
                {
                    HE7.IsChecked = false;
                }
                else
                {
                    HE7.IsChecked = true;
                }
                HE24.IsChecked = false;
            }
            if (hourType == "ALL" || hourType == "PEAK")
            {
                HE8.IsChecked = true;
                HE9.IsChecked = true;
                HE10.IsChecked = true;
                HE11.IsChecked = true;
                HE12.IsChecked = true;
                HE13.IsChecked = true;
                HE14.IsChecked = true;
                HE15.IsChecked = true;
                HE16.IsChecked = true;
                HE17.IsChecked = true;
                HE18.IsChecked = true;
                HE19.IsChecked = true;
                HE20.IsChecked = true;
                HE21.IsChecked = true;
                HE22.IsChecked = true;
                if (marketComboBox.Text == "PJM" || hourType == "ALL")
                {
                    HE23.IsChecked = true;
                }
                else
                {
                    HE23.IsChecked = false;
                }
            }
            else
            {
                HE8.IsChecked = false;
                HE9.IsChecked = false;
                HE10.IsChecked = false;
                HE11.IsChecked = false;
                HE12.IsChecked = false;
                HE13.IsChecked = false;
                HE14.IsChecked = false;
                HE15.IsChecked = false;
                HE16.IsChecked = false;
                HE17.IsChecked = false;
                HE18.IsChecked = false;
                HE19.IsChecked = false;
                HE20.IsChecked = false;
                HE21.IsChecked = false;
                HE22.IsChecked = false;
                if (marketComboBox.Text == "PJM")
                {
                    HE23.IsChecked = false;
                }
                else
                {
                    HE23.IsChecked = true;
                }
            }
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the NoneDaysButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NoneDaysButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            MondayCheckBox.IsChecked = false;
            TuesdayCheckBox.IsChecked = false;
            WednesdayCheckBox.IsChecked = false;
            ThursdayCheckBox.IsChecked = false;
            FridayCheckBox.IsChecked = false;
            SaturdayCheckBox.IsChecked = false;
            SundayCheckBox.IsChecked = false;
            mModel.UpdateChartCommand(true);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }

        /// <summary>
        /// Handles the GotFocus event of the sourceComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void sourceComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }
        /// <summary>
        /// Handles the GotFocus event of the sinkComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void sinkComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }
        /// <summary>
        /// Handles the MouseDoubleClick event of the ListView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PathGraphsTab.IsSelected = true;
        }
        /// <summary>
        /// Handles the Click event of the WeekDaysButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WeekDaysButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            MondayCheckBox.IsChecked = true;
            TuesdayCheckBox.IsChecked = true;
            WednesdayCheckBox.IsChecked = true;
            ThursdayCheckBox.IsChecked = true;
            FridayCheckBox.IsChecked = true;
            SaturdayCheckBox.IsChecked = false;
            SundayCheckBox.IsChecked = false;
            mModel.UpdateChartCommand(true);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the WeekEndsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WeekEndsButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            MondayCheckBox.IsChecked = false;
            TuesdayCheckBox.IsChecked = false;
            WednesdayCheckBox.IsChecked = false;
            ThursdayCheckBox.IsChecked = false;
            FridayCheckBox.IsChecked = false;
            SaturdayCheckBox.IsChecked = true;
            SundayCheckBox.IsChecked = true;
            mModel.UpdateChartCommand(true);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the AllDaysButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AllDaysButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            MondayCheckBox.IsChecked = true;
            TuesdayCheckBox.IsChecked = true;
            WednesdayCheckBox.IsChecked = true;
            ThursdayCheckBox.IsChecked = true;
            FridayCheckBox.IsChecked = true;
            SaturdayCheckBox.IsChecked = true;
            SundayCheckBox.IsChecked = true;
            mModel.UpdateChartCommand(true);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the addPath control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void addPath_Click(object sender, RoutedEventArgs e)
        {
            List<Path> pathList = new List<Path>();
            foreach (var item in PathDataGrid.Items)
            {
                pathList.Add((Path)item);
            }
            mModel.AddPath(pathList);
        }
        /// <summary>
        /// Handles the Click event of the deletePath control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void deletePath_Click(object sender, RoutedEventArgs e)
        {
            //List<Path> pathList = new List<Path>();
            //foreach (var item in PathDataGrid.Items)
            //{
            //    pathList.Add((Path)item);
            //}
            List<Path> selectedPathList = new List<Path>();
            foreach (var item in PathDataGrid.SelectedCells)
            {
                if (mModel.PathList != null)
                {
                    Path rowitem = (Path)item.Item;
                    Path findPath = mModel.PathList.FirstOrDefault(t => t.BidId.Equals(rowitem.BidId));
                    if (findPath != null)
                    {
                        selectedPathList.Add(rowitem);
                    }
                }
            }
            mModel.DeletePath(PathDataGrid.ItemsSource as List<Path>, selectedPathList);
        }
        /// <summary>
        /// Handles the Click event of the btnHoursNone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnHoursNone_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            HE1.IsChecked = false;
            HE2.IsChecked = false;
            HE3.IsChecked = false;
            HE4.IsChecked = false;
            HE5.IsChecked = false;
            HE6.IsChecked = false;
            HE7.IsChecked = false;
            HE8.IsChecked = false;
            HE9.IsChecked = false;
            HE10.IsChecked = false;
            HE11.IsChecked = false;
            HE12.IsChecked = false;
            HE13.IsChecked = false;
            HE14.IsChecked = false;
            HE15.IsChecked = false;
            HE16.IsChecked = false;
            HE17.IsChecked = false;
            HE18.IsChecked = false;
            HE19.IsChecked = false;
            HE20.IsChecked = false;
            HE21.IsChecked = false;
            HE22.IsChecked = false;
            HE23.IsChecked = false;
            HE24.IsChecked = false;
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the KeyDown event of the PathDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void PathDataGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            WorkbookStatistics.ViewModels.MainWindowViewModel context = DataContext as WorkbookStatistics.ViewModels.MainWindowViewModel;
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                string clipvalue = string.Empty;
                try
                {
                    clipvalue = Clipboard.GetText();
                }
                catch (Exception)
                {
                    MessageBox.Show("No Data Copied or Error Reading Clipboard", "Portfolio Analyzer", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                StringBuilder invalidUpdates = new StringBuilder();
                mColumeName = PathDataGrid.SelectedCells[0].Column.Header.ToString();
                var dgCellInfo = PathDataGrid.SelectedCells[0];
                double textValue;

                int updatecounter = 1;
                foreach (var cell in PathDataGrid.SelectedCells)
                {
                    updatecounter++;
                    WorkbookStatistics.ViewModels.Path editedRow = cell.Item as WorkbookStatistics.ViewModels.Path;
                    if (editedRow.Status.Equals("Valid") || editedRow.Status.Equals("SUBMITTED"))
                    {
                        continue;
                    }
                    switch (mColumeName)
                    {
                        case "Analysis Type":
                            if (context.RowsUpdated(editedRow, "Analysis Type", clipvalue) == false)
                            {
                                invalidUpdates.Append(editedRow.BidId + ",");
                            }
                            editedRow.AnalysisType = clipvalue;
                            break;
                        case "MW":
                            if (double.TryParse((string)clipvalue, out textValue))
                            {
                                if (context.RowsUpdated(editedRow, "MW", clipvalue) == false)
                                {
                                    invalidUpdates.Append(editedRow.BidId + ",");
                                }
                                editedRow.MW = textValue;
                            }
                            else
                            {
                                MessageBox.Show("Invalid Value", "Portfolio Analyzer", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            break;
                        case "Price":
                            if (double.TryParse((string)clipvalue, out textValue))
                            {
                                if (context.RowsUpdated(editedRow, "Price", clipvalue) == false)
                                {
                                    invalidUpdates.Append(editedRow.BidId + ",");
                                }
                                editedRow.Price = textValue;
                            }
                            else
                            {
                                MessageBox.Show("Invalid Value", "Portfolio Analyzer", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            break;
                    }
                }
                if (invalidUpdates.Length > 0)
                {
                    MessageBox.Show("Bid not updated : " + invalidUpdates.ToString(), "Portfolio Analysis", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    PathDataGrid.Items.Refresh();
                    Path[] paths = new Path[PathDataGrid.Items.Count];
                    PathDataGrid.Items.CopyTo(paths, 0);
                    context.UpdateRows(paths.ToList<Path>());
                }
            }
        }
        /// <summary>
        /// Handles the Click event of the WinterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WinterButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            DecCheckBox.IsChecked = true;
            JanCheckBox.IsChecked = true;
            FebCheckBox.IsChecked = true;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the SpringButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SpringButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            MarCheckBox.IsChecked = true;
            AprCheckBox.IsChecked = true;
            MayCheckBox.IsChecked = true;
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the SummerButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SummerButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            JunCheckBox.IsChecked = true;
            JulCheckBox.IsChecked = true;
            AugCheckBox.IsChecked = true;
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the FallButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FallButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            SepCheckBox.IsChecked = true;
            OctCheckBox.IsChecked = true;
            NovCheckBox.IsChecked = true;
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the AllMonthButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AllMonthButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            DecCheckBox.IsChecked = true;
            JanCheckBox.IsChecked = true;
            FebCheckBox.IsChecked = true;
            MarCheckBox.IsChecked = true;
            AprCheckBox.IsChecked = true;
            MayCheckBox.IsChecked = true;
            JunCheckBox.IsChecked = true;
            JulCheckBox.IsChecked = true;
            AugCheckBox.IsChecked = true;
            SepCheckBox.IsChecked = true;
            OctCheckBox.IsChecked = true;
            NovCheckBox.IsChecked = true;
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Click event of the NoneMonthButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NoneMonthButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.UpdateRefresh(false);
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
            mModel.UpdateChartCommand(false);
            mModel.UpdateRefresh(true);
            mModel.SetPathChart();
            mModel.UpdateRefreshPath(true);
        }
        /// <summary>
        /// Handles the Checked event of the CollectionRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CollectionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (CollectionComboBox != null)
            //{
            //    CollectionComboBox.IsEnabled = true;
            //}
            //CollectionComboBox.Items.Clear();
            //CollectionComboBox.Text = "";
            //VayuDbConn.Open();
            //mSelectNameDateRangeCommand.Parameters["@trader"].Value = mUser;
            //SqlDataReader reader = mSelectNameDateRangeCommand.ExecuteReader();
            //while (reader.Read())
            //{
            //    CollectionComboBox.Items.Add(reader.GetString(0));
            //}
            VayuConnection.Close();
        }
        /// <summary>
        /// Handles the Click event of the submitButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            mModel.Submit();
        }
        /// <summary>
        /// Handles the Click event of the retrieveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void retrieveButton_Click(object sender, RoutedEventArgs e)
        {
            detailsTab.SelectedIndex = 0;
        }
        /// <summary>
        /// Handles the CellEditEnding event of the PathDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellEditEndingEventArgs"/> instance containing the event data.</param>
        private void PathDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            WorkbookStatistics.ViewModels.Path editedRow = e.Row.DataContext as WorkbookStatistics.ViewModels.Path;
            if (editedRow.Status.Equals("Valid") || editedRow.Status.Equals("SUBMITTED"))
            {
                return;
            }
            WorkbookStatistics.ViewModels.MainWindowViewModel context = DataContext as WorkbookStatistics.ViewModels.MainWindowViewModel;
            if (e.Column.Header.Equals("Analysis Type"))
            {
                FrameworkElement element = PathDataGrid.Columns[2].GetCellContent(e.Row);
                string ahour = ((TextBox)element).Text;
                context.RowsUpdated(editedRow, "Analysis Type", ahour);
            }
            if (e.Column.Header.Equals("Price"))
            {
                FrameworkElement element = PathDataGrid.Columns[3].GetCellContent(e.Row);
                string price = ((TextBox)element).Text;
                double textValue;
                if (double.TryParse(price.ToString(), out textValue))
                {
                    context.RowsUpdated(editedRow, "Price", price);
                }
            }
            if (e.Column.Header.Equals("MW"))
            {
                FrameworkElement element = PathDataGrid.Columns[4].GetCellContent(e.Row);
                string mw = ((TextBox)element).Text;
                double textValue;
                if (double.TryParse(mw.ToString(), out textValue))
                {
                    context.RowsUpdated(editedRow, "MW", mw);
                }
            }

            if (e.Column.Header.Equals("Comments"))
            {
                try
                {
                    FrameworkElement element = PathDataGrid.Columns[PathDataGrid.Columns.Count - 1].GetCellContent(e.Row);
                    string comment = ((TextBox)element).Text;
                    context.RowsUpdated(editedRow, "Comments", comment);
                }
                catch { }
            }
        }
        /// <summary>
        /// Handles the MouseDoubleClick event of the PathDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PathDataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                PathNodeTab.IsSelected = true;
                WorkbookStatistics.ViewModels.Path info = PathDataGrid.SelectedCells[0].Item as WorkbookStatistics.ViewModels.Path;
                string path = info.Sink == null || info.Sink.Length == 0 ? info.Source : info.Source + "->" + info.Sink;
                pathComboList.SelectedValue = path;
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Handles the SelectedCellsChanged event of the PathDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedCellsChangedEventArgs"/> instance containing the event data.</param>
        private void PathDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var deletePath = DataContext as WorkbookStatistics.ViewModels.MainWindowViewModel;
            List<Path> selectedPath = new List<Path>();
            List<int> rowIndex = new List<int>();
            foreach (var cell in PathDataGrid.SelectedCells)
            {
                DataGridCell cellitem = Vayu.CommonControls.DataGridInfo.GetCell(cell);
                int index = Vayu.CommonControls.DataGridInfo.GetRowIndex(cellitem);
                if (!rowIndex.Contains(index))
                {
                    selectedPath.Add(cell.Item as Path);
                    rowIndex.Add(index);
                }
            }
            deletePath.SelectedPathByCell = selectedPath;
        }
        /// <summary>
        /// Handles the BeginningEdit event of the PathDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridBeginningEditEventArgs"/> instance containing the event data.</param>
        private void PathDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Path editedPath = e.Row.Item as Path;
            if (editedPath.Status.Equals("Valid") || editedPath.Status.Equals("SUBMITTED"))
            {
                e.Cancel = true;
            }
        }
        /// <summary>
        /// Handles the Sorting event of the PathDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridSortingEventArgs"/> instance containing the event data.</param>
        private void PathDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column == null)
            {
                return;
            }
            if (mModel.SortCondition == e.Column.SortMemberPath)
            {
                if (mModel.SortOrder == false)
                    mModel.SortOrder = true;
                else
                    mModel.SortOrder = false;
            }
            else
            {
                mModel.SortCondition = e.Column.SortMemberPath;
                mModel.SortOrder = true;
                mCurrentSortColumn = e.Column;
                mCurrentSortDirection = ListSortDirection.Ascending;
            }
            if (e.Column.SortMemberPath.Equals("AsBidRisk") || e.Column.SortMemberPath.Equals("AsBidRiskReward"))
            {
                e.Handled = true;
                mModel.SortingNulls();

                ListSortDirection direction = (mModel.SortOrder == true) ?
                ListSortDirection.Ascending : ListSortDirection.Descending;

                e.Column.SortDirection = direction;
                mCurrentSortDirection = direction;
            }
        }
        /// <summary>
        /// Handles the TargetUpdated event of the PathDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataTransferEventArgs"/> instance containing the event data.</param>
        private void PathDataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (mCurrentSortColumn != null)
            {
                mCurrentSortColumn.SortDirection = mCurrentSortDirection;
            }
        }
        /// <summary>
        /// Handles the 1 event of the chkSubmit_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void chkSubmit_Click_1(object sender, RoutedEventArgs e)
        {
            if ((bool)chkSubmit.IsChecked)
            {
                MainWindowViewModel dx = DataContext as MainWindowViewModel;
                if (dx != null)
                {
                    dx.CheckAll(true);
                }
            }
            else
            {
                MainWindowViewModel dx = DataContext as MainWindowViewModel;
                if (dx != null)
                {
                    dx.CheckAll(false);
                }
            }
        }
        /// <summary>
        /// Handles the 1 event of the CheckBox_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk != null && mModel != null)
            {
                mModel.UpdateRows(chk.CommandParameter == null ? "" : chk.CommandParameter.ToString(), (bool)chk.IsChecked);
                e.Handled = true;
            }
        }

        #endregion

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>> p = new ExportToExcelNodeSpread<Model.HourlyPivotData, List<Model.HourlyPivotData>>();
            ICollectionView view = CollectionViewSource.GetDefaultView(DayComparisonListView.ItemsSource);
            if (DayComparisonListView.Items.Count > 0)
            {
                view = CollectionViewSource.GetDefaultView(DayComparisonListView.SelectedItems);
            }
            List<HourlyPivotData> itemList = new List<Model.HourlyPivotData>();
            foreach (var item in view.SourceCollection)
            {
                itemList.Add((HourlyPivotData)item);
            }
            p.dataToPrint = itemList;
            p.GenerateReport();
        }
        private void PathMW_Sorting(object sender, DataGridSortingEventArgs e)
        {
            try
            {
                e.Handled = true;
                var dataContext = this.DataContext as MainWindowViewModel;
                if (dataContext != null)
                {
                    dataContext.mSortOrder = !dataContext.mSortOrder;
                    if (dataContext.mSortOrder)
                    {
                        listSortDirection = ListSortDirection.Ascending;
                    }
                    else
                    {
                        listSortDirection = ListSortDirection.Descending;
                    }
                    dataContext.SortCondition = e.Column.SortMemberPath;
                    dataContext.HandleSort(dataContext.PathMWList, listSortDirection);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void PortfolioChkBox_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[0].Visibility = Visibility.Visible;
        }

        private void PortfolioChkBox_UnChecked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[0].Visibility = Visibility.Collapsed;
        }

        private void SourceRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[1].Visibility = Visibility.Visible;
            PathMWdatagrid.Columns[2].Visibility = Visibility.Collapsed;
        }

        private void SinkRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[1].Visibility = Visibility.Collapsed;
            PathMWdatagrid.Columns[2].Visibility = Visibility.Visible;
        }

        private void NoneRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            PathMWdatagrid.Columns[1].Visibility = Visibility.Visible;
            PathMWdatagrid.Columns[2].Visibility = Visibility.Visible;
        }

        private void SetWindowSize(object sender, RoutedEventArgs e)
        {
            this.Width = 400;
            this.Height = 600;
        }

        //private void Srch_ConstraintTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        e.Handled = true;
        //        AutoCompleteBox auto = (AutoCompleteBox)sender;
        //        if(auto!=null)
        //        {
        //            string selectedItem = auto.SelectedItem.ToString();
        //            List<Exposure> ConstraintExposureList = System.Linq.Enumerable.Cast<Exposure>(ExposureDataGrid.Items).ToList();
        //            ExposureDataGrid.SelectedItem = ConstraintExposureList.Find(a => a.Constraint == selectedItem);
        //            ExposureDataGrid.ScrollIntoView(ExposureDataGrid.SelectedItem);
        //        }
        //    }
        //    catch (Exception)
        //    {                
        //        //throw;
        //    }

        //}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
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
                DataGridCell cell = null;
                string cellHeader = "";
                int cellHeaderTest = -1;
                if (values[2] != DependencyProperty.UnsetValue) // cell object value to get header names
                {
                    cell = (DataGridCell)values[2];
                    cellHeader = (string)cell.Column.Header;
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
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (testFactorsAbove.Length >= 10 && node != null && node.Date != null)
                    {
                        if (cellHeader.Contains("Tot") && textValue < testFactorsBelow[7])
                        {
                            mybrush = new SolidColorBrush(Colors.White);
                        }
                        else if (cellHeader.Contains("Tot") && textValue > testFactorsAbove[7] && textValue < testFactorsAbove[9])
                        {
                            mybrush = new SolidColorBrush(Colors.Black);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue < testFactorsBelow[2]) // m*-1
                        {
                            mybrush = new SolidColorBrush(Colors.White);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue > testFactorsBelow[2] && textValue < testFactorsBelow[4]) // between m*-1 and m*-6
                        {
                            mybrush = new SolidColorBrush(Colors.Black);
                        }
                        else if (textValue < 0)
                        {
                            mybrush = new SolidColorBrush(Colors.Red);
                        }
                    }
                    else if (textValue < 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Red);
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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class DataGridCellBackColorConverter : IMultiValueConverter
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
                DataGridCell cell = null;
                string cellHeader = "";
                Int32 cellHeaderTest = -1;
                if (values[2] != DependencyProperty.UnsetValue) // cell object value
                {
                    cell = (DataGridCell)values[2];
                    cellHeader = (string)cell.Column.Header;
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
                    mybrush = new SolidColorBrush(Colors.LightGray);
                }
                //if (node.RowName != "Spread" || node.Date == null) // cj todo: take out node.date == null check. allow spread avg in summary to show heatmap colors, but not spread total in summary.
                //{
                //    return mybrush;
                //}
                //// 
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
                            mybrush = new SolidColorBrush(Color.FromRgb(13, 138, 0));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 1]) // *10
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(50, 158, 38));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 2]) // *6
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(20, 209, 0));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 3]) // *2
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(77, 233, 60));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 4]) // *1
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(120, 233, 108));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 0]) // *-15
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(165, 0, 8));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 1]) // *-10
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(190, 46, 53));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 2]) // *-6
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(251, 0, 13));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 3]) // *-2
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(253, 65, 75));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 4]) // *-1
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(253, 118, 125));
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
                //  DataGridCell cell = null;
                string cellHeader = "";
                Int32 cellHeaderTest = -1;
                if (!values[2].Equals("")) // cell object value
                {
                    //  cell = (DataGridCell)values[2];
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
                // DataGridCell cell = null;
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class LocationsViewConverter : IValueConverter
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
            var locs = value as System.Collections.Generic.IEnumerable<Location>;
            if (locs != null && locs.Any())
            {
                return new LocationRect(locs.Max(l => l.Latitude), locs.Min(l => l.Longitude), // NWSE
                                        locs.Min(l => l.Latitude), locs.Max(l => l.Longitude));
            }
            else
            {
                return null;
            }
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
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ConvertToCurrencyFormat : IValueConverter
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
            if (value != null && value.ToString().Contains("DART"))
            {
                StringBuilder buildHelper = new StringBuilder();
                string[] strArray = value.ToString().Split(' ');
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i] != "")
                    {
                        if (i == strArray.Length - 1)
                        {
                            try
                            {
                                buildHelper.Append(string.Format("{0:C}", int.Parse(strArray[i])).Replace(".00", ""));
                            }
                            catch
                            {
                                buildHelper.Append(strArray[i]);
                            }
                        }
                        else
                        {
                            buildHelper.Append(strArray[i] + "\n");
                        }
                    }
                }
                return buildHelper.ToString();
            }
            else
            {
                return value;
            }
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    public class PathMWsCellConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(0.0))
                return string.Empty;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
