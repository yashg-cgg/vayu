
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vayu.UptosPathAnalysisAlgorithm.Model;
using Vayu.UptosPathAnalysisAlgorithm.ViewModels;
namespace Vayu.UptosPathAnalysisAlgorithm.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel datacontext;
        AutoCompleteBox Auto;
        private string[] ignoreNames = new string[] { "CountDays", "MW", "CalcNumber", "Skew", "Kurtosis", "AnalysisTypeInt" };
        private string[] ignore = new string[] { "Zone", "NodeType", "MaxWin", "MaxLoss", "MW", "RiskReward", "CountDays", "CountCleared", "PctWin", "YearlyUpSide", "YearlyRiskReward", "AvgDA", "CalcNumber", "RTMaxComplete", "RTMinComplete", "MustTakeSum", "AMustTakeSum", "DailyMustTakeMin", "ADailyMustTakeMin", "ASum", "AAvg", "AMax", "AStdDev", "AWinPct", "AClearPct", "DailyMin", "DailyMax", "DailyAvg", "ADailyMin", "ADailyMax", "ADailyAvg", "SumToMax", "Sharpe", "Skew", "Kurtosis", "WeeklySumValue", "WeeklyMaxWin", "WeeklyMaxLossRt", "WeeklyPctWin", "WeeklyCountCleared", "AnnualMaxLossRt", "MonthlyMaxLossRt", "IncDec", "YearlyDownSide", "SourceNodeKey", "SinkNodeKey", "HoursCleared", "WeeklyWin", "StdDev", "ConstraintText", "ContingencyText", "family", "Sensitivity", "AMin", "MarketDate" };
        /// <summary>
        /// The ignore c3po names
        /// </summary>
        private string[] ignoreC3poNames = new string[] { "Hour", "Correlation", "RTMedian", "DAMedian", "RTStdDev", "DAStdDev" };
        public MainWindow()
        {
            InitializeComponent();
        }
        #region Events

        /// <summary>
        /// Handles the SelectedCellsChanged event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedCellsChangedEventArgs"/> instance containing the event data.</param>
        public void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            List<int> rowIndex = new List<int>();
            MainWindowViewModel dataContext = DataContext as MainWindowViewModel;
            List<Vayu.UptosPathAnalysisAlgorithm.Model.RobotTypeList> selectedItems = new List<Vayu.UptosPathAnalysisAlgorithm.Model.RobotTypeList>();
            List<Tuple<string, string>> sourceSinkList = new List<Tuple<string, string>>();
            foreach (var cell in RobotDataGrid.SelectedCells)
            {
                DataGridCell cellItem = Vayu.CommonControls.DataGridInfo.GetCell(cell);
                int index = Vayu.CommonControls.DataGridInfo.GetRowIndex(cellItem);
                if (!rowIndex.Contains(index))
                {
                    Vayu.UptosPathAnalysisAlgorithm.Model.RobotTypeList node = (Vayu.UptosPathAnalysisAlgorithm.Model.RobotTypeList)cell.Item;
                    sourceSinkList.Add(new Tuple<string, string>(node.SourceName, node.SinkName));
                    rowIndex.Add(index);
                }
            }
            dataContext.SetSourceSinks(sourceSinkList);
        }

        /// <summary>
        /// Handles the Click event of the NodeAnalyzer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void NodeAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            if (RobotDataGrid.SelectedCells != null && RobotDataGrid.SelectedCells.Count >= 1)
            {
                string column = RobotDataGrid.SelectedCells[0].Column.Header.ToString();
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowNodeAnalyzer();
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
            if (RobotDataGrid.SelectedCells != null && RobotDataGrid.SelectedCells.Count > 0)
            {
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowLMPGraphs("");
                }
            }
        }

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    RobotDataGrid.SelectAllCells();
        //    RobotDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
        //    ApplicationCommands.Copy.Execute(null, RobotDataGrid);
        //    RobotDataGrid.UnselectAllCells();
        //    String result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
        //    File.AppendAllText("D:\\BlockAlgo.csv", result, UnicodeEncoding.UTF8);
        //    MessageBox.Show("Data has been sucessfully exported to CSV file", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        //}
        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            System.Windows.Forms.SaveFileDialog savefiledialog = new System.Windows.Forms.SaveFileDialog();
            savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            savefiledialog.FilterIndex = 1;
            savefiledialog.RestoreDirectory = true;
            MainWindowViewModel datacontext = DataContext as MainWindowViewModel;

            if (datacontext.SelectedRobotType == AlgoType.Ramp)
                savefiledialog.FileName = fileName + "RampAlgo_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.Block_B1)
                savefiledialog.FileName = fileName + "BlockAlgo_B1_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.Block_B2)
                savefiledialog.FileName = fileName + "BlockAlgo_B2_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.Block_B3)
                savefiledialog.FileName = fileName + "BlockAlgo_B3_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.PeakOffPeak)
                savefiledialog.FileName = fileName + "PeakOffPeakAlgo_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.VirtualBlock)
                savefiledialog.FileName = fileName + "VirtualBlockAlgo_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.UptosShort)
                savefiledialog.FileName = fileName + "UptosShortAlgo_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.VirtualShort)
                savefiledialog.FileName = fileName + "VirtualShortAlgo_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.Outage)
                savefiledialog.FileName = fileName + "OutageAlgo_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.PJMPositiveCorrelations)
                savefiledialog.FileName = fileName + "PJMPositiveCorrelations_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.VirtualPath)
                savefiledialog.FileName = fileName + "VirtualPath_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.ErcotPositiveCorrelations)
                savefiledialog.FileName = fileName + "ErcotPositiveCorrelations_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.PJMNegativeCorrelations)
                savefiledialog.FileName = fileName + "PJMNegativeCorrelations_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.ErcotNegativeCorrelations)
                savefiledialog.FileName = fileName + "ErcotNegativeCorrelations_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.ErcotBlock_B1)
                savefiledialog.FileName = fileName + "ErcotBlock_B1_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.ErcotBlock_B2)
                savefiledialog.FileName = fileName + "ErcotBlock_B2_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            else if (datacontext.SelectedRobotType == AlgoType.ErcotSubmittedBidsAlgo)
                savefiledialog.FileName = fileName + "ErcotSubmittedBidsAlgo_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;

            string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), savefiledialog.FileName + ".csv");
            if (savefiledialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = savefiledialog.FileName;
                StreamWriter sw = new StreamWriter(savefiledialog.FileName, false);
                string header = string.Empty;
                try
                {

                    if (datacontext.SelectedRobotType == AlgoType.VirtualBlock)
                    {
                        sw.Write("SourceName");
                        sw.Write(",");
                        sw.Write("IncDec ");
                        sw.Write(",");
                        sw.Write("AnalysisType");
                        sw.Write(",");
                        sw.Write("MW");
                        sw.Write(",");
                        sw.Write("Price");
                        sw.Write(",");
                        sw.Write("SumValue");
                        sw.Write(",");
                        sw.Write("MaxWin");
                        sw.Write(",");
                        sw.Write("MaxLoss");
                        sw.Write(",");
                        sw.Write("RiskReward");
                        sw.Write(",");
                        sw.Write("CountDays");
                        sw.Write(",");
                        sw.Write("CountCleared");
                        sw.Write(",");
                        sw.Write("PctWin");
                        sw.Write(",");
                        sw.Write("YearlyDownSide");
                        sw.Write(",");
                        sw.Write("YearlyRiskReward");
                        sw.Write(",");
                        sw.Write("AvgDA");
                        sw.Write(",");
                        sw.Write("CalcNumber");
                        sw.Write(",");
                        sw.Write("SavedTime");
                        sw.Write(",");
                        sw.Write("MustTakeSum");
                        sw.Write(",");
                        sw.Write("AMustTakeSum");
                        sw.Write(",");
                        sw.Write("DailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ADailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ASum ");
                        sw.Write(",");
                        sw.Write("AAvg ");
                        sw.Write(",");
                        sw.Write("AMin ");
                        sw.Write(",");
                        sw.Write("AMax ");
                        sw.Write(",");
                        sw.Write("AStdDev ");
                        sw.Write(",");
                        sw.Write("AWinPct ");
                        sw.Write(",");
                        sw.Write("AClearPct ");
                        sw.Write(",");
                        sw.Write("DailyMin ");
                        sw.Write(",");
                        sw.Write("DailyMax ");
                        sw.Write(",");
                        sw.Write("DailyAvg ");
                        sw.Write(",");
                        sw.Write("ADailyMin ");
                        sw.Write(",");
                        sw.Write("ADailyMax ");
                        sw.Write(",");
                        sw.Write("ADailyAvg ");
                        sw.Write(",");
                        sw.Write("SumToMax ");
                        sw.Write(",");
                        sw.Write("Sharpe ");
                        sw.Write(",");
                        sw.Write("Skew ");
                        sw.Write(",");
                        sw.Write("Kurtosis ");
                        sw.Write(",");
                        sw.Write("DollarPerMW ");
                        sw.Write(",");
                        sw.Write("WeeklySumValue ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxLossRt  ");
                        sw.Write(",");
                        sw.Write("WeeklyPctWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyCountCleared ");
                        sw.Write(",");
                        sw.Write("AnnualMaxLossRt ");



                    }
                    else if (datacontext.SelectedRobotType == AlgoType.ErcotBlock_B1)
                    {
                        sw.Write("SourceName");
                        sw.Write(",");
                        sw.Write("SinkName ");
                        sw.Write(",");
                        sw.Write("AnalysisType");
                        sw.Write(",");
                        sw.Write("Price");
                        sw.Write(",");
                        sw.Write("SumValue");
                        sw.Write(",");
                        sw.Write("MaxWin");
                        sw.Write(",");
                        sw.Write("MaxLoss");
                        sw.Write(",");
                        sw.Write("MW");
                        sw.Write(",");
                        sw.Write("RiskReward");
                        sw.Write(",");
                        sw.Write("CountDays");
                        sw.Write(",");
                        sw.Write("CountCleared");
                        sw.Write(",");
                        sw.Write("PctWin");
                        sw.Write(",");
                        sw.Write("YearlyDownSide");
                        sw.Write(",");
                        sw.Write("YearlyRiskReward");
                        sw.Write(",");
                        sw.Write("AvgDA");
                        sw.Write(",");
                        sw.Write("CalcNumber");
                        sw.Write(",");
                        sw.Write("SavedTime");
                        sw.Write(",");
                        sw.Write("MustTakeSum");
                        sw.Write(",");
                        sw.Write("AMustTakeSum");
                        sw.Write(",");
                        sw.Write("DailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ADailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ASum ");
                        sw.Write(",");
                        sw.Write("AAvg ");
                        sw.Write(",");
                        sw.Write("AMin ");
                        sw.Write(",");
                        sw.Write("AMax ");
                        sw.Write(",");
                        sw.Write("AStdDev ");
                        sw.Write(",");
                        sw.Write("AWinPct ");
                        sw.Write(",");
                        sw.Write("AClearPct ");
                        sw.Write(",");
                        sw.Write("DailyMin ");
                        sw.Write(",");
                        sw.Write("DailyMax ");
                        sw.Write(",");
                        sw.Write("DailyAvg ");
                        sw.Write(",");
                        sw.Write("ADailyMin ");
                        sw.Write(",");
                        sw.Write("ADailyMax ");
                        sw.Write(",");
                        sw.Write("ADailyAvg ");
                        sw.Write(",");
                        sw.Write("SumToMax ");
                        sw.Write(",");
                        sw.Write("Sharpe ");
                        sw.Write(",");
                        sw.Write("Skew ");
                        sw.Write(",");
                        sw.Write("Kurtosis ");
                        sw.Write(",");
                        sw.Write("DollarPerMW ");
                        sw.Write(",");
                        sw.Write("WeeklySumValue ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxLossRt  ");
                        sw.Write(",");
                        sw.Write("WeeklyPctWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyCountCleared ");
                        sw.Write(",");
                        sw.Write("AnnualMaxLossRt ");
                        sw.Write(",");
                        sw.Write("MonthlyMaxLossRT ");
                        sw.Write(",");
                        sw.Write("SourceZone ");
                        sw.Write(",");
                        sw.Write("SinkZone ");
                        sw.Write(",");
                        sw.Write("RTMax ");
                        sw.Write(",");
                        sw.Write("RTMin ");
                        //
                        sw.Write(",");
                        sw.Write("RTMinFall");
                        sw.Write(",");
                        sw.Write("RTMaxFall");
                        sw.Write(",");
                        sw.Write("RTMinSpring");
                        sw.Write(",");
                        sw.Write("RTMaxSpring");
                        sw.Write(",");
                        sw.Write("RTMinSummer");
                        sw.Write(",");
                        sw.Write("RTMaxSummer");
                        sw.Write(",");
                        sw.Write("RTMinWinter");
                        sw.Write(",");
                        sw.Write("RTMaxWinter");
                        sw.Write(",");
                        sw.Write("PathMinRT");
                        sw.Write(",");
                        sw.Write("SourceFuel");
                        sw.Write(",");
                        sw.Write("SinkFuel");
                    }
                    else if (datacontext.SelectedRobotType == AlgoType.ErcotBlock_B2)
                    {
                        sw.Write("SourceName");
                        sw.Write(",");
                        sw.Write("SinkName ");
                        sw.Write(",");
                        sw.Write("AnalysisType");
                        sw.Write(",");
                        sw.Write("Price");
                        sw.Write(",");
                        sw.Write("SumValue");
                        sw.Write(",");
                        sw.Write("MaxWin");
                        sw.Write(",");
                        sw.Write("MaxLoss");
                        sw.Write(",");
                        sw.Write("MW");
                        sw.Write(",");
                        sw.Write("RiskReward");
                        sw.Write(",");
                        sw.Write("CountDays");
                        sw.Write(",");
                        sw.Write("CountCleared");
                        sw.Write(",");
                        sw.Write("PctWin");
                        sw.Write(",");
                        sw.Write("YearlyDownSide");
                        sw.Write(",");
                        sw.Write("YearlyRiskReward");
                        sw.Write(",");
                        sw.Write("AvgDA");
                        sw.Write(",");
                        sw.Write("CalcNumber");
                        sw.Write(",");
                        sw.Write("SavedTime");
                        sw.Write(",");
                        sw.Write("MustTakeSum");
                        sw.Write(",");
                        sw.Write("AMustTakeSum");
                        sw.Write(",");
                        sw.Write("DailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ADailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ASum ");
                        sw.Write(",");
                        sw.Write("AAvg ");
                        sw.Write(",");
                        sw.Write("AMin ");
                        sw.Write(",");
                        sw.Write("AMax ");
                        sw.Write(",");
                        sw.Write("AStdDev ");
                        sw.Write(",");
                        sw.Write("AWinPct ");
                        sw.Write(",");
                        sw.Write("AClearPct ");
                        sw.Write(",");
                        sw.Write("DailyAvg ");
                        sw.Write(",");
                        sw.Write("ADailyMin ");
                        sw.Write(",");
                        sw.Write("ADailyMax ");
                        sw.Write(",");
                        sw.Write("ADailyAvg ");
                        sw.Write(",");
                        sw.Write("SumToMax ");
                        sw.Write(",");
                        sw.Write("Sharpe ");
                        sw.Write(",");
                        sw.Write("DollarPerMW ");
                        sw.Write(",");
                        sw.Write("WeeklySumValue ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxLossRt  ");
                        sw.Write(",");
                        sw.Write("WeeklyPctWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyCountCleared ");
                        sw.Write(",");
                        sw.Write("AnnualMaxLossRt ");
                        sw.Write(",");
                        sw.Write("MonthlyMaxLossRT ");
                        sw.Write(",");
                        sw.Write("SourceZone ");
                        sw.Write(",");
                        sw.Write("SinkZone ");
                        sw.Write(",");
                        sw.Write("RTMax ");
                        sw.Write(",");
                        sw.Write("RTMin ");
                        //
                        sw.Write(",");
                        sw.Write("RTMinFall");
                        sw.Write(",");
                        sw.Write("RTMaxFall");
                        sw.Write(",");
                        sw.Write("RTMinSpring");
                        sw.Write(",");
                        sw.Write("RTMaxSpring");
                        sw.Write(",");
                        sw.Write("RTMinSummer");
                        sw.Write(",");
                        sw.Write("RTMaxSummer");
                        sw.Write(",");
                        sw.Write("RTMinWinter");
                        sw.Write(",");
                        sw.Write("RTMaxWinter");
                        sw.Write(",");
                        sw.Write("PathMinRT");
                        sw.Write(",");
                        sw.Write("SourceFuel");
                        sw.Write(",");
                        sw.Write("SinkFuel");
                    }
                    else if (datacontext.SelectedRobotType == AlgoType.ErcotPositiveCorrelations)
                    {
                        sw.Write("SourceName");
                        sw.Write(",");
                        sw.Write("SinkName");
                        sw.Write(",");
                        sw.Write("AnalysisType");
                        sw.Write(",");
                        sw.Write("Price");
                        sw.Write(",");
                        sw.Write("SumValue");
                        sw.Write(",");
                        sw.Write("Correlation");
                        sw.Write(",");
                        sw.Write("SourceZone");
                        sw.Write(",");
                        sw.Write("SinkZone");
                        sw.Write(",");
                        //sw.Write("SourceNodeType");
                        //sw.Write(",");
                        //sw.Write("SinkNodeType");
                        //sw.Write(",");
                        sw.Write("SavedTime");
                        sw.Write(",");
                        sw.Write("RTMaxWinter");
                        sw.Write(",");
                        sw.Write("RTMinWinter");
                        sw.Write(",");
                        sw.Write("RTMaxSpring");
                        sw.Write(",");
                        sw.Write("RTMinSpring");
                        sw.Write(",");
                        sw.Write("RTMaxSummer");
                        sw.Write(",");
                        sw.Write("RTMinSummer");
                        sw.Write(",");
                        sw.Write("RTMaxFall");
                        sw.Write(",");
                        sw.Write("RTMinFall");
                        sw.Write(",");
                        sw.Write("RTMaxAll");
                        sw.Write(",");
                        sw.Write("RTMinAll");
                        sw.Write(",");
                        sw.Write("DollarPerMW");
                        sw.Write(",");
                        sw.Write("RTMedian");
                        sw.Write(",");
                        sw.Write("DAMedian");
                        sw.Write(",");
                        sw.Write("RTStdDev");
                        sw.Write(",");
                        sw.Write("DAStdDev");
                        sw.Write(",");
                        sw.Write("AvgRtLoss");
                        sw.Write(",");
                        sw.Write("AvgRtCong");
                        sw.Write(",");
                        sw.Write("CorrelationAsBid");
                        sw.Write(",");
                        sw.Write("RTMedianAsBid");
                        sw.Write(",");
                        sw.Write("DAMedianAsBid");
                        sw.Write(",");
                        sw.Write("RTStdDevAsBid");
                        sw.Write(",");
                        sw.Write("DAStdDevAsBid");
                        sw.Write(",");
                        sw.Write("DARTAsBid");
                        sw.Write(",");
                        sw.Write("DollarPerMWAsBid");
                        sw.Write(",");

                        sw.Write("CountDays");
                        sw.Write(",");
                        sw.Write("CountCleared");
                        sw.Write(",");
                        sw.Write("PctWin");
                        sw.Write(",");
                        sw.Write("PathMinRT");
                        sw.Write(",");

                        sw.Write("SourceFuel");
                        sw.Write(",");
                        sw.Write("SinkFuel");
                        sw.Write(",");

                    }
                    else if (datacontext.SelectedRobotType == AlgoType.PJMNegativeCorrelations)
                    {
                        sw.Write("SourceName");
                        sw.Write(",");
                        sw.Write("SinkName");
                        sw.Write(",");
                        sw.Write("AnalysisType");
                        sw.Write(",");
                        sw.Write("Price");
                        sw.Write(",");
                        sw.Write("SumValue");
                        sw.Write(",");
                        sw.Write("Correlation");
                        sw.Write(",");
                        sw.Write("SourceZone");
                        sw.Write(",");
                        sw.Write("SinkZone");
                        sw.Write(",");
                        //sw.Write("SourceNodeType");
                        //sw.Write(",");
                        //sw.Write("SinkNodeType");
                        //sw.Write(",");
                        sw.Write("SavedTime");
                        sw.Write(",");
                        sw.Write("RTMaxWinter");
                        sw.Write(",");
                        sw.Write("RTMinWinter");
                        sw.Write(",");
                        sw.Write("RTMaxSpring");
                        sw.Write(",");
                        sw.Write("RTMinSpring");
                        sw.Write(",");
                        sw.Write("RTMaxSummer");
                        sw.Write(",");
                        sw.Write("RTMinSummer");
                        sw.Write(",");
                        sw.Write("RTMaxFall");
                        sw.Write(",");
                        sw.Write("RTMinFall");
                        sw.Write(",");
                        sw.Write("RTMaxAll");
                        sw.Write(",");
                        sw.Write("RTMinAll");
                        sw.Write(",");
                        sw.Write("DollarPerMW");
                        sw.Write(",");
                        sw.Write("RTMedian");
                        sw.Write(",");
                        sw.Write("DAMedian");
                        sw.Write(",");
                        sw.Write("RTStdDev");
                        sw.Write(",");
                        sw.Write("DAStdDev");
                        sw.Write(",");
                        sw.Write("AvgRtLoss");
                        sw.Write(",");
                        sw.Write("AvgRtCong");
                        sw.Write(",");
                        sw.Write("CorrelationAsBid");
                        sw.Write(",");
                        sw.Write("RTMedianAsBid");
                        sw.Write(",");
                        sw.Write("DAMedianAsBid");
                        sw.Write(",");
                        sw.Write("RTStdDevAsBid");
                        sw.Write(",");
                        sw.Write("DAStdDevAsBid");
                        sw.Write(",");
                        sw.Write("DARTAsBid");
                        sw.Write(",");
                        sw.Write("DollarPerMWAsBid");
                        sw.Write(",");
                    }
                    else if (datacontext.SelectedRobotType == AlgoType.ErcotNegativeCorrelations)
                    {
                        sw.Write("SourceName");
                        sw.Write(",");
                        sw.Write("SinkName");
                        sw.Write(",");
                        sw.Write("AnalysisType");
                        sw.Write(",");
                        sw.Write("Price");
                        sw.Write(",");
                        sw.Write("SumValue");
                        sw.Write(",");
                        sw.Write("Correlation");
                        sw.Write(",");
                        sw.Write("SourceZone");
                        sw.Write(",");
                        sw.Write("SinkZone");
                        sw.Write(",");
                        //sw.Write("SourceNodeType");
                        //sw.Write(",");
                        //sw.Write("SinkNodeType");
                        //sw.Write(",");
                        sw.Write("SavedTime");
                        sw.Write(",");
                        sw.Write("RTMaxWinter");
                        sw.Write(",");
                        sw.Write("RTMinWinter");
                        sw.Write(",");
                        sw.Write("RTMaxSpring");
                        sw.Write(",");
                        sw.Write("RTMinSpring");
                        sw.Write(",");
                        sw.Write("RTMaxSummer");
                        sw.Write(",");
                        sw.Write("RTMinSummer");
                        sw.Write(",");
                        sw.Write("RTMaxFall");
                        sw.Write(",");
                        sw.Write("RTMinFall");
                        sw.Write(",");
                        sw.Write("RTMaxAll");
                        sw.Write(",");
                        sw.Write("RTMinAll");
                        sw.Write(",");
                        sw.Write("DollarPerMW");
                        sw.Write(",");
                        sw.Write("RTMedian");
                        sw.Write(",");
                        sw.Write("DAMedian");
                        sw.Write(",");
                        sw.Write("RTStdDev");
                        sw.Write(",");
                        sw.Write("DAStdDev");
                        sw.Write(",");
                        sw.Write("AvgRtLoss");
                        sw.Write(",");
                        sw.Write("AvgRtCong");
                        sw.Write(",");
                        sw.Write("CorrelationAsBid");
                        sw.Write(",");
                        sw.Write("RTMedianAsBid");
                        sw.Write(",");
                        sw.Write("DAMedianAsBid");
                        sw.Write(",");
                        sw.Write("RTStdDevAsBid");
                        sw.Write(",");
                        sw.Write("DAStdDevAsBid");
                        sw.Write(",");
                        sw.Write("DARTAsBid");
                        sw.Write(",");
                        sw.Write("DollarPerMWAsBid");
                        sw.Write(",");
                        sw.Write("CountDays");
                        sw.Write(",");
                        sw.Write("CountCleared");
                        sw.Write(",");
                        sw.Write("PctWin");
                        sw.Write(",");
                        sw.Write("PathMinRT");
                        sw.Write(",");
                        sw.Write("SourceFuel");
                        sw.Write(",");
                        sw.Write("SinkFuel");
                        sw.Write(",");
                    }
                    else if (datacontext.SelectedRobotType == AlgoType.ErcotSubmittedBidsAlgo)
                    {
                        sw.Write("SourceName");
                        sw.Write(",");
                        sw.Write("SinkName ");
                        sw.Write(",");
                        sw.Write("AnalysisType");
                        sw.Write(",");
                        sw.Write("Price");
                        sw.Write(",");
                        sw.Write("SumValue");
                        sw.Write(",");
                        sw.Write("MaxWin");
                        sw.Write(",");
                        sw.Write("MaxLoss");
                        sw.Write(",");
                        sw.Write("MW");
                        sw.Write(",");
                        sw.Write("RiskReward");
                        sw.Write(",");
                        sw.Write("CountDays");
                        sw.Write(",");
                        sw.Write("CountCleared");
                        sw.Write(",");
                        sw.Write("PctWin");
                        sw.Write(",");
                        sw.Write("YearlyDownSide");
                        sw.Write(",");
                        sw.Write("YearlyRiskReward");
                        sw.Write(",");
                        sw.Write("AvgDA");
                        sw.Write(",");
                        sw.Write("CalcNumber");
                        sw.Write(",");
                        sw.Write("SavedTime");
                        sw.Write(",");
                        sw.Write("MustTakeSum");
                        sw.Write(",");
                        sw.Write("AMustTakeSum");
                        sw.Write(",");
                        sw.Write("DailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ADailyMustTakeMin");
                        sw.Write(",");
                        sw.Write("ASum ");
                        sw.Write(",");
                        sw.Write("AAvg ");
                        sw.Write(",");
                        sw.Write("AMin ");
                        sw.Write(",");
                        sw.Write("AMax ");
                        sw.Write(",");
                        sw.Write("AStdDev ");
                        sw.Write(",");
                        sw.Write("AWinPct ");
                        sw.Write(",");
                        sw.Write("AClearPct ");
                        sw.Write(",");
                        sw.Write("DailyMin ");
                        sw.Write(",");
                        sw.Write("DailyMax ");
                        sw.Write(",");
                        sw.Write("DailyAvg ");
                        sw.Write(",");
                        sw.Write("ADailyMin ");
                        sw.Write(",");
                        sw.Write("ADailyMax ");
                        sw.Write(",");
                        sw.Write("ADailyAvg ");
                        sw.Write(",");
                        sw.Write("SumToMax ");
                        sw.Write(",");
                        sw.Write("Sharpe ");
                        sw.Write(",");
                        sw.Write("Skew ");
                        sw.Write(",");
                        sw.Write("Kurtosis ");
                        sw.Write(",");
                        sw.Write("DollarPerMW ");
                        sw.Write(",");
                        sw.Write("WeeklySumValue ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyMaxLossRt  ");
                        sw.Write(",");
                        sw.Write("WeeklyPctWin  ");
                        sw.Write(",");
                        sw.Write("WeeklyCountCleared ");
                        sw.Write(",");
                        sw.Write("AnnualMaxLossRt ");
                        sw.Write(",");
                        sw.Write("MonthlyMaxLossRT ");
                        sw.Write(",");
                        sw.Write("SourceZone ");
                        sw.Write(",");
                        sw.Write("SinkZone ");
                        sw.Write(",");
                        sw.Write("RTMax ");
                        sw.Write(",");
                        sw.Write("RTMin ");
                        //
                        sw.Write(",");
                        sw.Write("RTMinFall");
                        sw.Write(",");
                        sw.Write("RTMaxFall");
                        sw.Write(",");
                        sw.Write("RTMinSpring");
                        sw.Write(",");
                        sw.Write("RTMaxSpring");
                        sw.Write(",");
                        sw.Write("RTMinSummer");
                        sw.Write(",");
                        sw.Write("RTMaxSummer");
                        sw.Write(",");
                        sw.Write("RTMinWinter");
                        sw.Write(",");
                        sw.Write("RTMaxWinter");
                        sw.Write(",");
                        sw.Write("PathMinRT");
                    }
                    else
                    {
                        int iColCount = RobotDataGrid.Columns.Count;
                        for (int i = 0; i < iColCount; i++)
                        {
                            if (RobotDataGrid.Columns[i].Visibility == Visibility.Hidden) continue;

                            if (datacontext.SelectedRobotType == AlgoType.VirtualBlock && (datacontext.SelectedRobotType == AlgoType.VirtualShort))
                            {
                                header = RobotDataGrid.Columns[i].Header.ToString();
                                if (header.Contains("Sink"))
                                {
                                    continue;
                                }
                            }
                            if (datacontext.SelectedRobotType == AlgoType.Block_B2 || datacontext.SelectedRobotType == AlgoType.Block_B1 || datacontext.SelectedRobotType == AlgoType.Block_B3 ||
                                datacontext.SelectedRobotType == AlgoType.Outage || datacontext.SelectedRobotType == AlgoType.PeakOffPeak || datacontext.SelectedRobotType == AlgoType.Ramp ||
                                datacontext.SelectedRobotType == AlgoType.UptosShort || datacontext.SelectedRobotType == AlgoType.VirtualPath)
                            {
                                if (RobotDataGrid.Columns[i].Header.ToString() == "Correlation" || RobotDataGrid.Columns[i].Header.ToString() == "PathMinRT" ||
                                    RobotDataGrid.Columns[i].Header.ToString() == "SourceFuel" || RobotDataGrid.Columns[i].Header.ToString() == "SinkFuel")
                                {
                                    continue;
                                }
                            }
                            if (datacontext.SelectedRobotType == AlgoType.PJMPositiveCorrelations || datacontext.SelectedRobotType == AlgoType.PJMNegativeCorrelations)
                            {
                                if (RobotDataGrid.Columns[i].Header.ToString() == "PathMinRT" || RobotDataGrid.Columns[i].Header.ToString() == "SourceFuel" ||
                                    RobotDataGrid.Columns[i].Header.ToString() == "SinkFuel")
                                {
                                    continue;
                                }
                            }
                            sw.Write(RobotDataGrid.Columns[i].Header);
                            if (i < iColCount - 1)
                            {
                                sw.Write(",");
                            }
                        }

                    }

                    sw.Write(sw.NewLine);
                    //RobotDataGrid.SelectAll();
                    var selectedItems = RobotDataGrid.SelectedItems;
                    foreach (RobotTypeList nodeData in RobotDataGrid.Items)
                    {
                        if (datacontext.SelectedRobotType == AlgoType.PJMPositiveCorrelations)
                        {
                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkName);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.Correlation);
                            sw.Write(",");
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkZone);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.SourceNodeType);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkNodeType);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.RTMinWinter);
                                sw.Write(",");
                            }

                            sw.Write(nodeData.RTMaxSpring);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.RTMinSpring);
                                sw.Write(",");
                            }

                            sw.Write(nodeData.RTMaxSummer);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.RTMinSummer);
                                sw.Write(",");
                            }

                            sw.Write(nodeData.RTMaxFall);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.RTMinFall);
                                sw.Write(",");
                            }

                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.RTMinAll);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedian);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedian);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtLoss);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtCong);
                            //
                            sw.Write(",");
                            sw.Write(nodeData.CorrelationAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DARTAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMWAsBid);

                        }

                        else if (datacontext.SelectedRobotType == AlgoType.VirtualPath)
                        {
                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkName);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.UptosShort) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.Price);
                                sw.Write(",");
                                sw.Write(nodeData.SumValue);
                                sw.Write(",");
                                sw.Write(nodeData.Correlation);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");

                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkZone);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.SourceNodeType);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkNodeType);
                                sw.Write(",");
                            }
                            //sw.Write(nodeData.AnalysisType);
                            //sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.UptosShort) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                //sw.Write(nodeData.Price);
                                //sw.Write(",");
                                //sw.Write(nodeData.SumValue);
                                //sw.Write(",");
                                sw.Write(nodeData.MaxWin);
                                sw.Write(",");
                                sw.Write(nodeData.MaxLoss);
                                sw.Write(",");
                                //sw.Write(nodeData.MW);
                                //sw.Write(",");
                                sw.Write(nodeData.RiskReward);
                                sw.Write(",");
                                //sw.Write(nodeData.CountDays);
                                //sw.Write(",");
                                sw.Write(nodeData.CountCleared);
                                sw.Write(",");
                                sw.Write(nodeData.PctWin);
                                sw.Write(",");
                                sw.Write(nodeData.YearlyUpSide);
                                sw.Write(",");
                                sw.Write(nodeData.YearlyRiskReward);
                                sw.Write(",");
                                sw.Write(nodeData.AvgDA);
                                sw.Write(",");

                                //sw.Write(nodeData.CalcNumber);
                                //sw.Write(",");
                                sw.Write(nodeData.SavedTime);
                                {
                                    sw.Write(",");
                                    sw.Write(nodeData.RTMaxWinter);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinWinter);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxSpring);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinSpring);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxSummer);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinSummer);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxFall);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinFall);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxAll);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinAll);
                                        sw.Write(",");
                                    }
                                    sw.Write(nodeData.MustTakeSum);
                                    sw.Write(",");
                                    sw.Write(nodeData.AMustTakeSum);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyMustTakeMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyMustTakeMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.ASum);
                                    sw.Write(",");
                                    sw.Write(nodeData.AAvg);
                                    sw.Write(",");
                                    sw.Write(nodeData.AMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.AMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.AStdDev);
                                    sw.Write(",");
                                    sw.Write(nodeData.AWinPct);
                                    sw.Write(",");
                                    sw.Write(nodeData.AClearPct);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyAvg);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyAvg);
                                    sw.Write(",");
                                    sw.Write(nodeData.SumToMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.Sharpe);
                                    sw.Write(",");
                                    //sw.Write(nodeData.Skew);
                                    //sw.Write(",");
                                    //sw.Write(nodeData.Kurtosis);
                                    //sw.Write(",");
                                    sw.Write(nodeData.DollarPerMW);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklySumValue);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyMaxWin);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyMaxLossRt);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyPctWin);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyCountCleared);
                                    sw.Write(",");
                                    sw.Write(nodeData.AnnualMaxLossRt);
                                    sw.Write(",");
                                    sw.Write(nodeData.MonthlyMaxLossRt);
                                    sw.Write(",");
                                    sw.Write(nodeData.IncDec);
                                    //    sw.Write(",");
                                }
                            }
                        }
                        else if (datacontext.SelectedRobotType == AlgoType.VirtualBlock)
                        {

                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            sw.Write(nodeData.IncDec);
                            sw.Write(",");
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.MW);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.MaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.MaxLoss);
                            sw.Write(",");
                            sw.Write(nodeData.RiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.CountDays);
                            sw.Write(",");
                            sw.Write(nodeData.CountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.PctWin);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyDownSide);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyRiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.AvgDA);
                            sw.Write(",");
                            sw.Write(nodeData.CalcNumber);
                            sw.Write(",");
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.MustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.AMustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ASum);
                            sw.Write(",");
                            sw.Write(nodeData.AAvg);
                            sw.Write(",");
                            sw.Write(nodeData.AMin);
                            sw.Write(",");
                            sw.Write(nodeData.AMax);
                            sw.Write(",");
                            sw.Write(nodeData.AStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AWinPct);
                            sw.Write(",");
                            sw.Write(nodeData.AClearPct);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMin);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMax);
                            sw.Write(",");
                            sw.Write(nodeData.DailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMax);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.SumToMax);
                            sw.Write(",");
                            sw.Write(nodeData.Sharpe);
                            sw.Write(",");
                            sw.Write(nodeData.Skew);
                            sw.Write(",");
                            sw.Write(nodeData.Kurtosis);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklySumValue);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxLossRt);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyPctWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyCountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.AnnualMaxLossRt);


                        }
                        else if (datacontext.SelectedRobotType == AlgoType.ErcotBlock_B1)
                        {

                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            sw.Write(nodeData.SinkName);
                            sw.Write(",");
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.MaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.MaxLoss);
                            sw.Write(",");
                            sw.Write(nodeData.MW);
                            sw.Write(",");
                            sw.Write(nodeData.RiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.CountDays);
                            sw.Write(",");
                            sw.Write(nodeData.CountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.PctWin);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyDownSide);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyRiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.AvgDA);
                            sw.Write(",");
                            sw.Write(nodeData.CalcNumber);
                            sw.Write(",");
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.MustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.AMustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ASum);
                            sw.Write(",");
                            sw.Write(nodeData.AAvg);
                            sw.Write(",");
                            sw.Write(nodeData.AMin);
                            sw.Write(",");
                            sw.Write(nodeData.AMax);
                            sw.Write(",");
                            sw.Write(nodeData.AStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AWinPct);
                            sw.Write(",");
                            sw.Write(nodeData.AClearPct);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMin);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMax);
                            sw.Write(",");
                            sw.Write(nodeData.DailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMax);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.SumToMax);
                            sw.Write(",");
                            sw.Write(nodeData.Sharpe);
                            sw.Write(",");
                            sw.Write(nodeData.Skew);
                            sw.Write(",");
                            sw.Write(nodeData.Kurtosis);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklySumValue);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxLossRt);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyPctWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyCountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.AnnualMaxLossRt);

                            sw.Write(",");
                            sw.Write(nodeData.MonthlyMaxLossRt);
                            sw.Write(",");
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");
                            sw.Write(nodeData.SinkZone);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinAll);
                            //
                            sw.Write(",");
                            sw.Write(nodeData.RTMinFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxWinter);
                            sw.Write(",");
                            sw.Write(nodeData.PathMinRT);
                            sw.Write(",");
                            sw.Write(nodeData.SourceFuel);
                            sw.Write(",");
                            sw.Write(nodeData.SinkFuel);
                        }
                        else if (datacontext.SelectedRobotType == AlgoType.ErcotBlock_B2)
                        {

                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            sw.Write(nodeData.SinkName);
                            sw.Write(",");
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.MaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.MaxLoss);
                            sw.Write(",");
                            sw.Write(nodeData.MW);
                            sw.Write(",");
                            sw.Write(nodeData.RiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.CountDays);
                            sw.Write(",");
                            sw.Write(nodeData.CountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.PctWin);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyDownSide);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyRiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.AvgDA);
                            sw.Write(",");
                            sw.Write(nodeData.CalcNumber);
                            sw.Write(",");
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.MustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.AMustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ASum);
                            sw.Write(",");
                            sw.Write(nodeData.AAvg);
                            sw.Write(",");
                            sw.Write(nodeData.AMin);
                            sw.Write(",");
                            sw.Write(nodeData.AMax);
                            sw.Write(",");
                            sw.Write(nodeData.AStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AWinPct);
                            sw.Write(",");
                            sw.Write(nodeData.AClearPct);
                            sw.Write(",");
                            sw.Write(nodeData.DailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMax);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.SumToMax);
                            sw.Write(",");
                            sw.Write(nodeData.Sharpe);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklySumValue);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxLossRt);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyPctWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyCountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.AnnualMaxLossRt);

                            sw.Write(",");
                            sw.Write(nodeData.MonthlyMaxLossRt);
                            sw.Write(",");
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");
                            sw.Write(nodeData.SinkZone);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinAll);
                            //
                            sw.Write(",");
                            sw.Write(nodeData.RTMinFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxWinter);
                            sw.Write(",");
                            sw.Write(nodeData.PathMinRT);
                            sw.Write(",");
                            sw.Write(nodeData.SourceFuel);
                            sw.Write(",");
                            sw.Write(nodeData.SinkFuel);
                        }
                        else if (datacontext.SelectedRobotType == AlgoType.ErcotPositiveCorrelations)
                        {
                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            sw.Write(nodeData.SinkName);
                            sw.Write(",");
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.Correlation);
                            sw.Write(",");
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");
                            sw.Write(nodeData.SinkZone);
                            sw.Write(",");
                            //sw.Write(nodeData.SourceNodeType);
                            //sw.Write(",");
                            //sw.Write(nodeData.SinkNodeType);
                            //sw.Write(",");
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinAll);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedian);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedian);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtLoss);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtCong);
                            sw.Write(",");
                            sw.Write(nodeData.CorrelationAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DARTAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMWAsBid);
                            sw.Write(",");

                            sw.Write(nodeData.CountDays);
                            sw.Write(",");
                            sw.Write(nodeData.CountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.PctWin);
                            sw.Write(",");
                            sw.Write(nodeData.PathMinRT);
                            sw.Write(",");

                            sw.Write(nodeData.SourceFuel);
                            sw.Write(",");
                            sw.Write(nodeData.SinkFuel);
                            sw.Write(",");
                        }
                        else if (datacontext.SelectedRobotType == AlgoType.PJMNegativeCorrelations)
                        {
                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            sw.Write(nodeData.SinkName);
                            sw.Write(",");
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.Correlation);
                            sw.Write(",");
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");
                            sw.Write(nodeData.SinkZone);
                            sw.Write(",");
                            //sw.Write(nodeData.SourceNodeType);
                            //sw.Write(",");
                            //sw.Write(nodeData.SinkNodeType);
                            //sw.Write(",");
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinAll);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedian);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedian);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtLoss);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtCong);
                            sw.Write(",");
                            sw.Write(nodeData.CorrelationAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DARTAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMWAsBid);
                            sw.Write(",");
                        }
                        else if (datacontext.SelectedRobotType == AlgoType.ErcotNegativeCorrelations)
                        {
                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            sw.Write(nodeData.SinkName);
                            sw.Write(",");
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.Correlation);
                            sw.Write(",");
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");
                            sw.Write(nodeData.SinkZone);
                            sw.Write(",");
                            //sw.Write(nodeData.SourceNodeType);
                            //sw.Write(",");
                            //sw.Write(nodeData.SinkNodeType);
                            //sw.Write(",");
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinAll);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedian);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedian);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtLoss);
                            sw.Write(",");
                            sw.Write(nodeData.AvgRtCong);
                            sw.Write(",");
                            sw.Write(nodeData.CorrelationAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAMedianAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.RTStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DAStdDevAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DARTAsBid);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMWAsBid);
                            sw.Write(",");

                            sw.Write(nodeData.CountDays);
                            sw.Write(",");
                            sw.Write(nodeData.CountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.PctWin);
                            sw.Write(",");
                            sw.Write(nodeData.PathMinRT);
                            sw.Write(",");

                            sw.Write(nodeData.SourceFuel);
                            sw.Write(",");
                            sw.Write(nodeData.SinkFuel);
                            sw.Write(",");
                        }
                        else if (datacontext.SelectedRobotType == AlgoType.ErcotSubmittedBidsAlgo)
                        {
                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            sw.Write(nodeData.SinkName);
                            sw.Write(",");
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            sw.Write(nodeData.Price);
                            sw.Write(",");
                            sw.Write(nodeData.SumValue);
                            sw.Write(",");
                            sw.Write(nodeData.MaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.MaxLoss);
                            sw.Write(",");
                            sw.Write(nodeData.MW);
                            sw.Write(",");
                            sw.Write(nodeData.RiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.CountDays);
                            sw.Write(",");
                            sw.Write(nodeData.CountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.PctWin);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyDownSide);
                            sw.Write(",");
                            sw.Write(nodeData.YearlyRiskReward);
                            sw.Write(",");
                            sw.Write(nodeData.AvgDA);
                            sw.Write(",");
                            sw.Write(nodeData.CalcNumber);
                            sw.Write(",");
                            sw.Write(nodeData.SavedTime);
                            sw.Write(",");
                            sw.Write(nodeData.MustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.AMustTakeSum);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMustTakeMin);
                            sw.Write(",");
                            sw.Write(nodeData.ASum);
                            sw.Write(",");
                            sw.Write(nodeData.AAvg);
                            sw.Write(",");
                            sw.Write(nodeData.AMin);
                            sw.Write(",");
                            sw.Write(nodeData.AMax);
                            sw.Write(",");
                            sw.Write(nodeData.AStdDev);
                            sw.Write(",");
                            sw.Write(nodeData.AWinPct);
                            sw.Write(",");
                            sw.Write(nodeData.AClearPct);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMin);
                            sw.Write(",");
                            sw.Write(nodeData.DailyMax);
                            sw.Write(",");
                            sw.Write(nodeData.DailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMin);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyMax);
                            sw.Write(",");
                            sw.Write(nodeData.ADailyAvg);
                            sw.Write(",");
                            sw.Write(nodeData.SumToMax);
                            sw.Write(",");
                            sw.Write(nodeData.Sharpe);
                            sw.Write(",");
                            sw.Write(nodeData.Skew);
                            sw.Write(",");
                            sw.Write(nodeData.Kurtosis);
                            sw.Write(",");
                            sw.Write(nodeData.DollarPerMW);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklySumValue);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyMaxLossRt);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyPctWin);
                            sw.Write(",");
                            sw.Write(nodeData.WeeklyCountCleared);
                            sw.Write(",");
                            sw.Write(nodeData.AnnualMaxLossRt);

                            sw.Write(",");
                            sw.Write(nodeData.MonthlyMaxLossRt);
                            sw.Write(",");
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");
                            sw.Write(nodeData.SinkZone);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxAll);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinAll);
                            //
                            sw.Write(",");
                            sw.Write(nodeData.RTMinFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxFall);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSpring);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxSummer);
                            sw.Write(",");
                            sw.Write(nodeData.RTMinWinter);
                            sw.Write(",");
                            sw.Write(nodeData.RTMaxWinter);
                            sw.Write(",");
                            sw.Write(nodeData.PathMinRT);
                        }
                        else
                        {
                            sw.Write(nodeData.SourceName);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkName);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.AnalysisType);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.UptosShort) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.Price);
                                sw.Write(",");
                                sw.Write(nodeData.SumValue);
                                sw.Write(",");

                            }
                            sw.Write(nodeData.SourceZone);
                            sw.Write(",");

                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkZone);
                                sw.Write(",");
                            }
                            sw.Write(nodeData.SourceNodeType);
                            sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                sw.Write(nodeData.SinkNodeType);
                                sw.Write(",");
                            }
                            //sw.Write(nodeData.AnalysisType);
                            //sw.Write(",");
                            if (!(datacontext.SelectedRobotType == AlgoType.UptosShort) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                            {
                                //sw.Write(nodeData.Price);
                                //sw.Write(",");
                                //sw.Write(nodeData.SumValue);
                                //sw.Write(",");
                                sw.Write(nodeData.MaxWin);
                                sw.Write(",");
                                sw.Write(nodeData.MaxLoss);
                                sw.Write(",");
                                //sw.Write(nodeData.MW);
                                //sw.Write(",");
                                sw.Write(nodeData.RiskReward);
                                sw.Write(",");
                                //sw.Write(nodeData.CountDays);
                                //sw.Write(",");
                                sw.Write(nodeData.CountCleared);
                                sw.Write(",");
                                sw.Write(nodeData.PctWin);
                                sw.Write(",");
                                sw.Write(nodeData.YearlyUpSide);
                                sw.Write(",");
                                sw.Write(nodeData.YearlyRiskReward);
                                sw.Write(",");
                                sw.Write(nodeData.AvgDA);
                                sw.Write(",");

                                //sw.Write(nodeData.CalcNumber);
                                //sw.Write(",");
                                sw.Write(nodeData.SavedTime);
                                {
                                    sw.Write(",");
                                    sw.Write(nodeData.RTMaxWinter);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinWinter);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxSpring);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinSpring);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxSummer);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinSummer);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxFall);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinFall);
                                        sw.Write(",");
                                    }

                                    sw.Write(nodeData.RTMaxAll);
                                    sw.Write(",");
                                    if (!(datacontext.SelectedRobotType == AlgoType.VirtualBlock) || (datacontext.SelectedRobotType != AlgoType.VirtualShort))
                                    {
                                        sw.Write(nodeData.RTMinAll);
                                        sw.Write(",");
                                    }
                                    sw.Write(nodeData.MustTakeSum);
                                    sw.Write(",");
                                    sw.Write(nodeData.AMustTakeSum);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyMustTakeMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyMustTakeMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.ASum);
                                    sw.Write(",");
                                    sw.Write(nodeData.AAvg);
                                    sw.Write(",");
                                    sw.Write(nodeData.AMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.AMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.AStdDev);
                                    sw.Write(",");
                                    sw.Write(nodeData.AWinPct);
                                    sw.Write(",");
                                    sw.Write(nodeData.AClearPct);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.DailyAvg);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyMin);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.ADailyAvg);
                                    sw.Write(",");
                                    sw.Write(nodeData.SumToMax);
                                    sw.Write(",");
                                    sw.Write(nodeData.Sharpe);
                                    sw.Write(",");
                                    //sw.Write(nodeData.Skew);
                                    //sw.Write(",");
                                    //sw.Write(nodeData.Kurtosis);
                                    //sw.Write(",");
                                    sw.Write(nodeData.DollarPerMW);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklySumValue);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyMaxWin);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyMaxLossRt);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyPctWin);
                                    sw.Write(",");
                                    sw.Write(nodeData.WeeklyCountCleared);
                                    sw.Write(",");
                                    sw.Write(nodeData.AnnualMaxLossRt);
                                    sw.Write(",");
                                    sw.Write(nodeData.MonthlyMaxLossRt);
                                    sw.Write(",");
                                    sw.Write(nodeData.IncDec);
                                    //    sw.Write(",");
                                }
                            }
                            //sw.Write(sw.NewLine);
                        }
                        sw.Write(sw.NewLine);
                    }
                    Mouse.OverrideCursor = null;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    sw.Close();
                    // Process.Start(fileName);
                    MessageBox.Show("Data has been sucessfully exported to CSV file", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        /// <summary>
        /// Handles the 1 event of the ListBox_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ListBox lstBx = sender as ListBox;
            MainWindowViewModel dx = this.DataContext as MainWindowViewModel;
            if (lstBx != null && dx != null)
            {
                try
                {
                    switch (lstBx.Name)
                    {
                        case "lstSourceZones":
                            dx.SourceZoneSelectedItem = lstBx.SelectedItems == null ? lstBx.SelectedItems as List<string> : lstBx.SelectedItems.Cast<string>().ToList();
                            break;
                        case "lstSinkZones":
                            dx.SinkZoneSelectedItem = lstBx.SelectedItems == null ? lstBx.SelectedItems as List<string> : lstBx.SelectedItems.Cast<string>().ToList();
                            break;
                        case "lstSourceNodeType":
                            dx.SourceNodeTypeSelectedItem = lstBx.SelectedItems == null ? lstBx.SelectedItems as List<string> : lstBx.SelectedItems.Cast<string>().ToList();
                            break;
                        case "lstSinkNodeType":
                            dx.SinkNodeTypeSelectedItem = lstBx.SelectedItems == null ? lstBx.SelectedItems as List<string> : lstBx.SelectedItems.Cast<string>().ToList();
                            break;
                        default:
                            break;
                    }
                }
                catch
                {

                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles the 2 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
        {
            Button sendBtn = sender as Button;
            MainWindowViewModel dx = this.DataContext as MainWindowViewModel;
            if (dx != null && sendBtn != null)
            {
                switch (sendBtn.Name)
                {
                    case "SelectSourceZones":
                        lstSourceZones.SelectAll();
                        dx.SelectSourceSink(true, true);
                        break;
                    case "UnselectSourceZones":
                        lstSourceZones.UnselectAll();
                        dx.SelectSourceSink(true, false);
                        break;
                    case "SelectSinkZones":
                        lstSinkZones.SelectAll();
                        dx.SelectSourceSink(false, true);
                        break;
                    case "UnselectSinkZones":
                        lstSinkZones.UnselectAll();
                        dx.SelectSourceSink(false, false);
                        break;
                    case "SelectSourceNodeType":
                        lstSourceNodeType.SelectAll();
                        dx.SelectNodesZones(false, true);
                        break;
                    case "UnselectSourceNodeType":
                        lstSourceNodeType.UnselectAll();
                        dx.SelectNodesZones(false, false);
                        break;
                    case "SelectSinkNodeType":
                        lstSinkNodeType.SelectAll();
                        dx.SelectNodesZones(true, true);
                        break;
                    case "UnselectSinkNodeType":
                        lstSinkNodeType.UnselectAll();
                        dx.SelectNodesZones(true, false);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the 1 event of the RobotDataGrid_AutoGeneratingColumn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridAutoGeneratingColumnEventArgs"/> instance containing the event data.</param>
        private void RobotDataGrid_AutoGeneratingColumn_1(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {


            if (ignoreNames.Contains((e.PropertyName ?? ""), StringComparer.InvariantCultureIgnoreCase))
                e.Cancel = true;
            // UptosPathAnalysisAlgorithm.ViewModel.MainViewModel mviewmodel = new MainViewModel();

            datacontext = DataContext as MainWindowViewModel;
            if (datacontext.ComboSelectedRobotType.AlgorithmList == "Correlations")
            {
                if (ignore.Contains((e.PropertyName ?? ""), StringComparer.InvariantCultureIgnoreCase))
                    e.Cancel = true;
            }
            if (datacontext.ComboSelectedRobotType.AlgorithmList == "Block")
            {
                if (ignoreC3poNames.Contains((e.PropertyName ?? ""), StringComparer.InvariantCultureIgnoreCase))
                    e.Cancel = true;
            }

#if NEW
#else
            //if (BlockRadioButton.IsChecked.GetValueOrDefault() == true)
            //    return;

            if (ignoreC3poNames.Contains((e.PropertyName ?? ""), StringComparer.InvariantCultureIgnoreCase))
                e.Cancel = true;
#endif
        }

        #endregion
    }
}
