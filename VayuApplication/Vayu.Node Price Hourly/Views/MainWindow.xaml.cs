
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;
using Vayu.CommonControls;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.LMPriceWindow;
using Vayu.NodeLMPLibrary;


namespace Vayu.Node_Price_Hourly.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declaration

        /// <summary>
        /// The Vayu. database connection
        /// </summary>
        private SqlConnection VayuConnetion;

        /// <summary>
        /// The m select all nodes command
        /// </summary>
        private SqlCommand mSelectAllNodesCommand;
        /// <summary>
        /// The m selectl nodes values command
        /// </summary>
        private SqlCommand mSelectlNodesValuesCommand;

        private SqlCommand mSelectErcotNodesCommand;
        /// <summary>
        /// The m select source sink node command
        /// </summary>
        private SqlCommand mSelectSourceSinkNodeCommand;
        /// <summary>
        /// 
        /// </summary>
        private SqlCommand mSelectercotSourceSinkNodeCommand;
        /// <summary>
        /// The m select FTR source sink node command
        /// </summary>
        private SqlCommand mSelectFtrSourceSinkNodeCommand;

        private SqlCommand mSelectSeasionData;
        /// <summary>
        /// The m node hash
        /// </summary>
        private Dictionary<string, Vayu.DBLibrary.NodeDetail> mNodeHash = new Dictionary<string, Vayu.DBLibrary.NodeDetail>();
        /// <summary>
        /// The m selected node hash
        /// </summary>
        private Dictionary<int, int> mSelectedNodeHash = new Dictionary<int, int>();
        /// <summary>
        /// The m node type hash
        /// </summary>
        private Dictionary<int, List<string>> mNodeTypeHash = new Dictionary<int, List<string>>();
        /// <summary>
        /// The m zone hash
        /// </summary>
        private Dictionary<int, List<string>> mZoneHash = new Dictionary<int, List<string>>();
        /// <summary>
        /// The m PJM source sink list
        /// </summary>
        private List<string> mPJMSourceSinkList = new List<string>();
        private List<string> mErcotSourceSinkList = new List<string>();
        /// <summary>
        /// The m PJM FTR source sink list
        /// </summary>
        private List<int> mPJMFtrSourceSinkList = new List<int>();
        /// <summary>
        /// The m is first time
        /// </summary>
        private bool mIsFirstTime = true;
        /// <summary>
        /// The s market selected
        /// </summary>
        public static int sMarketSelected = 1;
        /// <summary>
        /// The s is DST
        /// </summary>
        public static bool sIsDST = false;
        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName;
        /// <summary>
        /// The m uptos path list
        /// </summary>
        private List<string> mUptosPathList = new List<string>();
        private List<string> mErcotUptosPathList = new List<string>();
        /// <summary>
        /// The m hour LMP list
        /// </summary>
        public List<NodeHourLMP> mHourLmpList = new List<NodeHourLMP>();
        /// <summary>
        /// The m LMP list
        /// </summary>
        public List<NodeDisplayData> mLmpList = new List<NodeDisplayData>();

        List<NodeData> tempnodeList = new List<NodeData>();
        /// <summary>
        /// The LMP dates helper
        /// </summary>
        private LMPDatesHelper lmpDatesHelper;
        /// <summary>
        /// The list zone
        /// </summary>
        public List<ZoneInfo> ListZone = new List<ZoneInfo>();

        /// <summary>
        /// Gets or sets the selected items.
        /// </summary>
        /// <value>
        /// The selected items.
        /// </value>
        public List<NodeData> SelectedItems { get; set; }
        public List<SeasonData> listseasionData = new List<SeasonData>();
        #endregion

        public MainWindow()
        {
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            sIsDST = curTimeZone.IsDaylightSavingTime(DateTime.Today.AddDays(1).AddHours(7));
            InitializeComponent();
            loadDBCommands();
            InitGui();
            lmpDatesHelper = new LMPDatesHelper(VayuConnetion);
            LoadColorZones();
        }


        #region Private Methods

        /// <summary>
        /// Initializes the GUI.
        /// </summary>
        private void InitGui()
        {
            ToNodeDatePicker.SelectedDate = SetToCST(DateTime.Today);
            FromNodeDatePicker.SelectedDate = SetToCST(DateTime.Today);
            PathComboBox.Text = "UPTOs";
            FillSourceSinkHash();
            GetErcotSourceSinkHash();
            //GetSeasionData();
        }
        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnetion = new VayuDBConnection().GetInstance().GetSqlConnection();
            mSelectAllNodesCommand = new SqlCommand();
            mSelectAllNodesCommand.CommandText = "select NodeName, NodeKey, zone, NodeType.Label, externalnodeid, Node.nodeTypeKey from Node, NodeType where Node.MarketKey = @MarketKey AND Node.NodeTypeKey=nodetype.NodeTypeKey order by Node.NodeName";
            mSelectAllNodesCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectAllNodesCommand.Connection = VayuConnetion;
            //
            mSelectlNodesValuesCommand = new SqlCommand();
            mSelectlNodesValuesCommand.CommandText = "SELECT nodekey, NodeName, Longitude, Latitude, kv, PSSENAME FROM dbo.NodeGeoImport (nolock) WHERE NodeName=@NodeName";
            mSelectlNodesValuesCommand.Parameters.AddWithValue("@NodeName", "NodeName");
            mSelectlNodesValuesCommand.Connection = VayuConnetion;
            //
            mSelectErcotNodesCommand = new SqlCommand();
            mSelectErcotNodesCommand.CommandText = " select NodeKey,NodeName,Longitude,Latitude,NodeTypeKey,Zone from ercot.[dbo].Node where NodeName=@NodeName";
            mSelectErcotNodesCommand.Parameters.AddWithValue("@NodeName", "NodeName");
            mSelectErcotNodesCommand.Connection = VayuConnetion;
            //
            mSelectSourceSinkNodeCommand = new SqlCommand();
            mSelectSourceSinkNodeCommand.CommandText = "select src.SourceNodeName, sink.SinkNodeName from EESPathList (nolock) src inner join Node (nolock) n on n.NodeKey = src.SourceNodeKey inner join EESPathList sink on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey inner join Node n2 on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = @MarketKey and n2.MarketKey = @MarketKey and src.MarketKey = @MarketKey and sink.MarketKey = @MarketKey order by n.NodeName";
            mSelectSourceSinkNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectSourceSinkNodeCommand.Connection = VayuConnetion;
            //
            mSelectFtrSourceSinkNodeCommand = new SqlCommand();
            mSelectFtrSourceSinkNodeCommand.CommandText = "select distinct SourceNodeKey, SinkNodeKey from pjm.FTRAuctionResults (nolock)";
            mSelectFtrSourceSinkNodeCommand.Connection = VayuConnetion;

            mSelectSeasionData = new SqlCommand();
            mSelectSeasionData.CommandText = "Select Fall.SourceName,Fall.SinkName,UTC.SourceNodeKey,UTC.SinkNodeKey,Fall.MinLMP as RTFallMin,Fall.MaxLMP as RTFallMax,Spr.MinLMP as RTSpringMin,Spr.MaxLMP as RTSpringMax ,  " +
                                            " Summ.MinLMP as RTSummerMin,Summ.MaxLMP as RTSummerMax,Win.MinLMP as RTWinterMin,Win.MaxLMP as RTWinterMax,UTC.PathMinRT from Vayu..RTRangeFall AS Fall " +
                                            " join Vayu..RTRangeSpring As Spr on Spr.SourceName=Fall.SourceName and Spr.SinkName=Fall.SinkName  " +
                                            " join Vayu..RTRangeSummer AS Summ on Summ.SourceName=Fall.SourceName and Summ.SinkName=Fall.SinkName " +
                                            " join Vayu..RTRangeWinter AS Win on Win.SourceName=Fall.SourceName and Win.SinkName=Fall.SinkName " +
                                            " join Vayu..Node As N1 on N1.NodeName=Fall.SourceName " +
                                            " join Vayu..Node As N2 on N2.NodeName=Fall.SinkName  " +
                                            " join Vayu..UTCPathHistoricData UTC on N1.NodeKey=UTC.SourceNodeKey and N2.NodeKey=UTC.SinkNodeKey " +
                                            " where N1.MarketKey=9 and N2.MarketKey=9";
            mSelectSeasionData.Connection = VayuConnetion;
        }
        /// <summary>
        /// Calculates the dart stats.
        /// </summary>
        /// <param name="indexDateTime">The index date time.</param>
        /// <param name="number">The number.</param>
        /// <param name="sum">The sum.</param>
        /// <param name="outSum">The out sum.</param>
        /// <param name="total">The total.</param>
        /// <param name="outTotal">The out total.</param>
        /// <param name="offPeakSum">The off peak sum.</param>
        /// <param name="outOffPeakSum">The out off peak sum.</param>
        /// <param name="peakSum">The peak sum.</param>
        /// <param name="outPeakSum">The out peak sum.</param>
        /// <param name="offPeakTotal">The off peak total.</param>
        /// <param name="outOffPeakTotal">The out off peak total.</param>
        /// <param name="peakTotal">The peak total.</param>
        /// <param name="outPeakTotal">The out peak total.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="outMax">The out maximum.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="outMin">The out minimum.</param>

        private void CalcDartStats(DateTime indexDateTime, double number, double sum, out double outSum, double total, out double outTotal, double offPeakSum, out double outOffPeakSum,
                                    double peakSum, out double outPeakSum, double offPeakTotal, out double outOffPeakTotal, double peakTotal, out double outPeakTotal, double max,
                                    out double outMax, double min, out double outMin)
        {
            outSum = sum;
            outTotal = total;

            outOffPeakSum = offPeakSum;
            outOffPeakTotal = offPeakTotal;

            outPeakSum = peakSum;
            outPeakTotal = peakTotal;

            outMax = max;
            outMin = min;

            if (double.IsNaN(number))
                return;

            outSum += number;
            outTotal++;

            if (lmpDatesHelper.TimingSession.IsOffPeak(indexDateTime))
            {
                outOffPeakSum += number;
                outOffPeakTotal++;
            }
            else
            {
                outPeakSum += number;
                outPeakTotal++;
            }

            if (max < number)
                outMax = number;

            if (min > number)
                outMin = number;

        }

        private void CalcDartStatsNew(DateTime indexDateTime, double number, double sum, out double outSum, double total, out double outTotal, double offPeakSum, out double outOffPeakSum,
                                    double peakSum, out double outPeakSum, double offPeakTotal, out double outOffPeakTotal, double peakTotal, out double outPeakTotal, double max,
                                    out double outMax, double min, out double outMin, double congestionSum, out double outCongestionSum, double lossSum, out double outLossSum,
                                    double congestionTotal, out double outCongestionTotal, double lossTotal, out double outLossTotal,
                                    double congestionPeakSum, out double outCongestionPeakSum, double lossPeakSum, out double outLossPeakSum,
                                    double congestionOffPeakSum, out double outCongestionOffPeakSum, double lossOffPeakSum, out double outLossOffPeakSum,
                                    double congestionPeakTotal, out double outCongestionPeakTotal, double lossPeakTotal, out double outLossPeakTotal,
                                    double congestionOffPeakTotal, out double outCongestionOffPeakTotal, double lossOffPeakTotal, out double outLossOffPeakTotal,
                                    double congestionMax, out double outCongestionMax, double lossMax, out double outLossMax,
                                    double congestionMin, out double outCongestionMin, double lossMin, out double outLossMin)
        {
            outSum = sum;
            outTotal = total;

            outOffPeakSum = offPeakSum;
            outOffPeakTotal = offPeakTotal;

            outPeakSum = peakSum;
            outPeakTotal = peakTotal;

            outMax = max;
            outMin = min;

            outCongestionSum = congestionSum;
            outLossSum = lossSum;

            outCongestionTotal = congestionTotal;
            outLossTotal = lossTotal;

            outCongestionPeakSum = congestionPeakSum;
            outLossPeakSum = lossPeakSum;

            outCongestionOffPeakSum = congestionOffPeakSum;
            outLossOffPeakSum = lossOffPeakSum;

            outCongestionPeakTotal = congestionPeakTotal;
            outLossPeakTotal = lossPeakTotal;

            outCongestionOffPeakTotal = congestionOffPeakTotal;
            outLossOffPeakTotal = lossOffPeakTotal;

            outCongestionMax = congestionMax;
            outLossMax = lossMax;

            outCongestionMin = congestionMin;
            outLossMin = lossMin;

            if (double.IsNaN(number))
                return;

            outSum += number;

            outCongestionSum += number;
            outLossSum += number;

            outTotal++;

            outCongestionTotal++;
            outLossTotal++;

            if (lmpDatesHelper.TimingSession.IsOffPeak(indexDateTime))
            {
                outOffPeakSum += number;

                outCongestionOffPeakSum += number;
                outLossOffPeakSum += number;


                outOffPeakTotal++;

                outCongestionOffPeakTotal++;
                outLossOffPeakTotal++;
            }
            else
            {
                outPeakSum += number;

                outCongestionPeakSum += number;
                outLossPeakSum += number;


                outPeakTotal++;

                outCongestionPeakTotal++;
                outLossPeakTotal++;
            }

            if (max < number)
                outMax = number;

            if (min > number)
                outMin = number;


            if (congestionMax < number)
                outCongestionMax = number;

            if (congestionMin > number)
                outCongestionMin = number;


            if (lossMax < number)
                outLossMax = number;

            if (lossMin > number)
                outLossMin = number;

        }

        /// <summary>
        /// Gets the selected zones.
        /// </summary>
        /// <returns></returns>
        private List<string> GetSelectedZones()
        {
            return null;
        }

        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <returns></returns>
        private int GetMarket()
        {
            int marketKey = 0;

            marketKey = 9;

            return marketKey;
        }

        int myCount = 0;

        /// <summary>
        /// Sets the node table.
        /// </summary>
        private void SetNodeTable()
        {
            if (mIsFirstTime)
            {
                return;
            }
            if (ToNodeDatePicker.Text == "")
            {
                return;
            }
            lmpDatesHelper = new LMPDatesHelper(VayuConnetion);
            Mouse.OverrideCursor = Cursors.Wait;
            DateTime start = DateTime.Parse(FromNodeDatePicker.Text);
            DateTime end = DateTime.Parse(ToNodeDatePicker.Text);
            mLmpList.Clear();
            nodeDataGrid.Items.Clear();
            List<string> nodeNameList = mNodeHash.Keys.ToList<string>();
            string nodeType = (string)NodeTypeComboBox.SelectedValue;
            nodeType = nodeType == null ? "ALL" : nodeType;

            bool isErcotChecked = (bool)ercotRadioButton.IsChecked;
            bool isNullChecked = (bool)NullCheckBox.IsChecked;
            string path = PathComboBox.Text;
            bool isMaxChecked = (bool)MaxRadioButton.IsChecked;
            bool isMinChecked = (bool)MinRadioButton.IsChecked;
            bool isAvgChecked = (bool)AvgRadioButton.IsChecked;
            bool isPeakAvgChecked = (bool)PeakAvgRadioButton.IsChecked;
            bool isOffPeakAvgChecked = (bool)OffPeakRadioButton.IsChecked;
            bool isDaChecked = (bool)daRadioButton.IsChecked;
            bool isRtChecked = (bool)rtRadioButton.IsChecked;
            bool isDartChecked = (bool)dartRadioButton.IsChecked;
            bool isRatioChecked = (bool)ratioRadioButton.IsChecked;
            bool isAggAvg = (bool)AggAvgRadioButton.IsChecked;
            bool isAggMax = (bool)AggMaxRadioButton.IsChecked;
            bool isAggMin = (bool)AggMinRadioButton.IsChecked;
            bool isHourChecked = (bool)HourRadioButton.IsChecked;
            double compareMin = double.MinValue;
            double compareMax = double.MaxValue;
            string type = dartComboBox.Text;
            int hour = Int32.Parse(hourComboBox.Text);

            bool isPriceChecked = (bool)rdbPrice.IsChecked;
            bool isCongestionChecked = (bool)rdbCongestion.IsChecked;
            bool isLossChecked = (bool)rdbLoss.IsChecked;

            try
            {
                compareMin = double.Parse(MinTextBox.Text);
            }
            catch (Exception ex)
            {
            }
            try
            {
                compareMax = double.Parse(MaxTextBox.Text);
            }
            catch (Exception ex)
            {
            }
            List<DateTime> dateList = new List<DateTime>();
            if (CollectionRadioButton.IsChecked == true)
            {
                foreach (string dateStr in DateCollectionListBox.Items)
                {
                    dateList.Add(DateTime.Parse(dateStr));
                }
            }
            else
            {
                while (start <= end)
                {
                    dateList.Add(start);
                    start = start.AddDays(1);
                }
            }
            if (!dateList.Any())
            {
                MessageBox.Show("No dates available for initialization.");
                return;
            }
            tempnodeList = new List<NodeData>();
            int marketKey = GetMarket();
            lmpDatesHelper.TimingSession.Initialize(marketKey, dateList.First(), dateList.First());
            List<string> nameList = new List<string>();
            var zones = ZoneListBox.SelectedItems;
            string[] zoneList = new string[zones.Count];
            zones.CopyTo(zoneList, 0);
            var sinkZones = SinkZoneListBox.SelectedItems;
            string[] sinkZoneList = new string[sinkZones.Count];
            sinkZones.CopyTo(sinkZoneList, 0);

            nameList = UptosCheckBox.IsChecked == true ? mErcotUptosPathList : nodeNameList;

            // List<string> nameList = UptosCheckBox.IsChecked == true ? mUptosPathList : nodeNameList;
            List<NodeData> nodeList = new List<NodeData>();
            foreach (string nodeName in nameList)
            {
                string name = nodeName;
                string sink = null;
                decimal RTMaxAll = 00; decimal RTMinAll = 00; DateTime PathMinRT = new DateTime();
                if (UptosCheckBox.IsChecked == true)
                {
                    string[] tokens = nodeName.Split('?');
                    name = tokens[0];
                    sink = tokens[1];
                    nodeDataGrid.Columns[3].Visibility = System.Windows.Visibility.Visible;
                    if (nodeTypeCheckBox.IsChecked == true)
                    {
                        nodeDataGrid.Columns[4].Visibility = System.Windows.Visibility.Visible;
                    }
                    nodeDataGrid.Columns[5].Visibility = System.Windows.Visibility.Visible;
                    if (isErcotChecked)
                    {
                        RTMinAll = Convert.ToDecimal(tokens[2]);
                        RTMaxAll = Convert.ToDecimal(tokens[3]);
                        PathMinRT = Convert.ToDateTime(tokens[4]);

                        nodeDataGrid.Columns[13].Visibility = System.Windows.Visibility.Visible;
                        nodeDataGrid.Columns[14].Visibility = System.Windows.Visibility.Visible;
                        nodeDataGrid.Columns[15].Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        nodeDataGrid.Columns[13].Visibility = System.Windows.Visibility.Hidden;
                        nodeDataGrid.Columns[14].Visibility = System.Windows.Visibility.Hidden;
                        nodeDataGrid.Columns[15].Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                else
                {
                    nodeDataGrid.Columns[3].Visibility = System.Windows.Visibility.Hidden;
                    nodeDataGrid.Columns[4].Visibility = System.Windows.Visibility.Hidden;
                    nodeDataGrid.Columns[5].Visibility = System.Windows.Visibility.Hidden;

                    nodeDataGrid.Columns[13].Visibility = System.Windows.Visibility.Hidden;
                    nodeDataGrid.Columns[14].Visibility = System.Windows.Visibility.Hidden;
                    nodeDataGrid.Columns[15].Visibility = System.Windows.Visibility.Hidden;
                }
                if (!mNodeHash.ContainsKey(name))
                {
                    continue;
                }
                NodeDetail nodeDetail = mNodeHash[name];
                NodeDetail sinkDetail = null;
                if (sink != null && mNodeHash.ContainsKey(sink))
                {
                    sinkDetail = mNodeHash[sink];
                }
                NodeData nodeData = new NodeData();
                NodeDisplayData nodeDData = new NodeDisplayData();
                nodeData.NodeName = name;
                nodeData.ZoneName = nodeDetail.Zone;
                nodeData.NodeType = nodeDetail.Type;
                nodeDData.NodeName = name;
                nodeDData.ZoneName = nodeDetail.Zone;
                nodeDData.NodeType = nodeDetail.Type;

                #region RTMinMax
                if (isErcotChecked)
                {
                    if (UptosCheckBox.IsChecked == true)
                    {
                        //nodeData.RTMaxFall = RTMaxFall;
                        //nodeData.RTMinFall = RTMinFall;

                        //nodeData.RTMaxSpring = RTMaxSpring;
                        //nodeData.RTMinSpring = RTMinSpring;

                        //nodeData.RTMinSummer = RTMinSummer;
                        //nodeData.RTMaxSummer = RTMaxSummer;

                        //nodeData.RTMinWinter = RTMinWinter;
                        //nodeData.RTMaxWinter = RTMaxWinter;

                        //decimal min1 = Math.Min(nodeData.RTMinWinter, nodeData.RTMinSpring);
                        //decimal min2 = Math.Min(nodeData.RTMinSummer, nodeData.RTMinFall);
                        //decimal max1 = Math.Max(nodeData.RTMaxWinter, nodeData.RTMaxSpring);
                        //decimal max2 = Math.Max(nodeData.RTMaxSummer, nodeData.RTMaxFall);
                        //nodeData.SRTMin = Math.Min(min1, min2);
                        //nodeData.SRTMax = Math.Max(max1, max2);
                        //nodeData.PathMinRT = PathMinRT;

                        nodeData.SRTMin = RTMinAll;
                        nodeData.SRTMax = RTMaxAll;
                        nodeData.PathMinRT = PathMinRT;
                    }
                }
                #endregion RTMinMAx
                if (sinkDetail != null)
                {
                    nodeData.SinkName = sink;
                    nodeData.SinkZone = sinkDetail.Zone;
                    nodeData.SinkType = sinkDetail.Type;
                    nodeDData.SinkName = sink;
                    nodeDData.SinkZone = sinkDetail.Zone;
                    nodeDData.SinkType = sinkDetail.Type;
                }

                double daPriceMax = double.MinValue;
                double daCongestionMax = double.MinValue;
                double daLossMax = double.MinValue;

                double rtPriceMax = double.MinValue;
                double rtCongestionMax = double.MinValue;
                double rtLossMax = double.MinValue;

                double dartPriceMax = double.MinValue;
                double dartCongestionMax = double.MinValue;
                double dartLossMax = double.MinValue;

                double ratioPriceMax = double.MinValue;
                double ratioCongestionMax = double.MinValue;
                double ratioLossMax = double.MinValue;


                double daPriceMin = double.MaxValue;
                double daCongestionMin = double.MaxValue;
                double daLossMin = double.MaxValue;

                double rtPriceMin = double.MaxValue;
                double rtCongestionMin = double.MaxValue;
                double rtLossMin = double.MaxValue;

                double dartPriceMin = double.MaxValue;
                double dartCongestionMin = double.MaxValue;
                double dartLossMin = double.MaxValue;

                double ratioPriceMin = double.MaxValue;
                double ratioCongestionMin = double.MaxValue;
                double ratioLossMin = double.MaxValue;


                double daPriceSum = 0;
                double daCongestionSum = 0;
                double daLossSum = 0;

                double rtPriceSum = 0;
                double rtCongestionSum = 0;
                double rtLossSum = 0;

                double dartPriceSum = 0;
                double dartCongestionSum = 0;
                double dartLossSum = 0;

                double ratioPriceSum = 0;
                double ratioCongestionSum = 0;
                double ratioLossSum = 0;


                double daPricePeakSum = 0;
                double daCongestionPeakSum = 0;
                double daLossPeakSum = 0;

                double rtPricePeakSum = 0;
                double rtCongestionPeakSum = 0;
                double rtLossPeakSum = 0;

                double dartPricePeakSum = 0;
                double dartCongestionPeakSum = 0;
                double dartLossPeakSum = 0;

                double ratioPricePeakSum = 0;
                double ratioCongestionPeakSum = 0;
                double ratioLossPeakSum = 0;


                double daPriceOffPeakSum = 0;
                double daCongestionOffPeakSum = 0;
                double daLossOffPeakSum = 0;

                double rtPriceOffPeakSum = 0;
                double rtCongestionOffPeakSum = 0;
                double rtLossOffPeakSum = 0;

                double dartPriceOffPeakSum = 0;
                double dartCongestionOffPeakSum = 0;
                double dartLossOffPeakSum = 0;

                double ratioPriceOffPeakSum = 0;
                double ratioCongestionOffPeakSum = 0;
                double ratioLossOffPeakSum = 0;


                double daPriceTotal = 0;
                double daCongestionTotal = 0;
                double daLossTotal = 0;

                double rtPriceTotal = 0;
                double rtCongestionTotal = 0;
                double rtLossTotal = 0;

                double dartPriceTotal = 0;
                double dartCongestionTotal = 0;
                double dartLossTotal = 0;

                double ratioPriceTotal = 0;
                double ratioCongestionTotal = 0;
                double ratioLossTotal = 0;


                double daPricePeakTotal = 0;
                double daCongestionPeakTotal = 0;
                double daLossPeakTotal = 0;

                double rtPricePeakTotal = 0;
                double rtCongestionPeakTotal = 0;
                double rtLossPeakTotal = 0;

                double dartPricePeakTotal = 0;
                double dartCongestionPeakTotal = 0;
                double dartLossPeakTotal = 0;

                double ratioPricePeakTotal = 0;
                double ratioCongestionPeakTotal = 0;
                double ratioLossPeakTotal = 0;


                double daPriceOffPeakTotal = 0;
                double daCongestionOffPeakTotal = 0;
                double daLossOffPeakTotal = 0;

                double rtPriceOffPeakTotal = 0;
                double rtCongestionOffPeakTotal = 0;
                double rtLossOffPeakTotal = 0;

                double dartPriceOffPeakTotal = 0;
                double dartCongestionOffPeakTotal = 0;
                double dartLossOffPeakTotal = 0;

                double ratioPriceOffPeakTotal = 0;
                double ratioCongestionOffPeakTotal = 0;
                double ratioLossOffPeakTotal = 0;


                for (int i = 0; i < 24; i++)
                {
                    #region declaration

                    #region da declaration

                    double daPrice = double.NaN;
                    double daCongestion = double.NaN;
                    double daLoss = double.NaN;

                    #endregion

                    #region rt declaration

                    double rtPrice = double.NaN;
                    double rtCongestion = double.NaN;
                    double rtLoss = double.NaN;

                    #endregion

                    #region dart declaration

                    double dartPrice = double.NaN;
                    double dartCongestion = double.NaN;
                    double dartLoss = double.NaN;

                    #endregion

                    #region ratio declaration

                    double ratioPrice = double.NaN;
                    double ratioCongestion = double.NaN;
                    double ratioLoss = double.NaN;

                    #endregion

                    #endregion

                    #region  old code

                    //#region DA

                    //double count = 0;
                    //foreach (DateTime date in dateList)
                    //{
                    //    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    //    {
                    //        continue;
                    //    }
                    //    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    //    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    //    if (DARTNode.sDAHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                    //    {
                    //        if (double.IsNaN(da))
                    //        {
                    //            da = DARTNode.sDAHash[sourceKey];
                    //            if (sinkKey != null)
                    //            {
                    //                double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                    //                da = sinkDa - DARTNode.sDAHash[sourceKey];
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (isAggAvg)
                    //            {
                    //                if (sinkKey != null)
                    //                {
                    //                    double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                    //                    da += (sinkDa - DARTNode.sDAHash[sourceKey]);
                    //                }
                    //                else
                    //                {
                    //                    da += DARTNode.sDAHash[sourceKey];
                    //                }
                    //            }
                    //            if (isAggMax)
                    //            {
                    //                if (sinkKey != null)
                    //                {
                    //                    double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                    //                    if (da < sinkDa - DARTNode.sDAHash[sourceKey])
                    //                    {
                    //                        da = sinkDa - DARTNode.sDAHash[sourceKey];
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (da < DARTNode.sDAHash[sourceKey])
                    //                    {
                    //                        da = DARTNode.sDAHash[sourceKey];
                    //                    }
                    //                }
                    //            }
                    //            if (isAggMin)
                    //            {
                    //                if (sinkKey != null)
                    //                {
                    //                    double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                    //                    if (da > sinkDa - DARTNode.sDAHash[sourceKey])
                    //                    {
                    //                        da = sinkDa - DARTNode.sDAHash[sourceKey];
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (da > DARTNode.sDAHash[sourceKey])
                    //                    {
                    //                        da = DARTNode.sDAHash[sourceKey];
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        count++;
                    //    }
                    //}
                    //if (!double.IsNaN(da))
                    //{
                    //    if (isAggAvg)
                    //        da = da / count;

                    //    DateTime indexDateTime = dateList.First().AddHours(i + 1);
                    //    CalcDartStats(indexDateTime, da, daSum, out daSum, daTotal, out daTotal, daOffPeakSum, out daOffPeakSum, daPeakSum, out daPeakSum, daOffPeakTotal,
                    //                    out daOffPeakTotal, daPeakTotal, out daPeakTotal, daMax, out daMax, daMin, out daMin);
                    //}

                    //#endregion

                    //#region RT

                    //count = 0;
                    //foreach (DateTime date in dateList)
                    //{
                    //    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    //    {
                    //        continue;
                    //    }
                    //    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    //    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    //    if (DARTNode.sRTHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                    //    {
                    //        if (double.IsNaN(rt))
                    //        {
                    //            rt = DARTNode.sRTHash[sourceKey];
                    //            if (sinkKey != null)
                    //            {
                    //                if (sinkKey != null)
                    //                {
                    //                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                    //                    rt = sinkRt - DARTNode.sRTHash[sourceKey];
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (isAggAvg)
                    //            {
                    //                if (sinkKey != null)
                    //                {
                    //                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                    //                    rt += (sinkRt - DARTNode.sRTHash[sourceKey]);
                    //                }
                    //                else
                    //                {
                    //                    rt += DARTNode.sRTHash[sourceKey];
                    //                }
                    //            }
                    //            if (isAggMax)
                    //            {
                    //                if (sinkKey != null)
                    //                {
                    //                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                    //                    if (rt < sinkRt - DARTNode.sRTHash[sourceKey])
                    //                    {
                    //                        rt = sinkRt - DARTNode.sRTHash[sourceKey];
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (rt < DARTNode.sRTHash[sourceKey])
                    //                    {
                    //                        rt = DARTNode.sRTHash[sourceKey];
                    //                    }
                    //                }
                    //            }
                    //            if (isAggMin)
                    //            {
                    //                if (sinkKey != null)
                    //                {
                    //                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                    //                    if (rt > sinkRt - DARTNode.sRTHash[sourceKey])
                    //                    {
                    //                        rt = sinkRt - DARTNode.sRTHash[sourceKey];
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (rt > DARTNode.sRTHash[sourceKey])
                    //                    {
                    //                        rt = DARTNode.sRTHash[sourceKey];
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        count++;
                    //    }
                    //}
                    //if (!double.IsNaN(rt))
                    //{
                    //    if (isAggAvg)
                    //        rt = rt / count;

                    //    DateTime indexDateTime = dateList.First().AddHours(i + 1);
                    //    CalcDartStats(indexDateTime, rt, rtSum, out rtSum, rtTotal, out rtTotal, rtOffPeakSum, out rtOffPeakSum, rtPeakSum, out rtPeakSum, rtOffPeakTotal,
                    //                    out rtOffPeakTotal, rtPeakTotal, out rtPeakTotal, rtMax, out rtMax, rtMin, out rtMin);
                    //}

                    //#endregion

                    //#region DART

                    //count = 0;
                    //foreach (DateTime date in dateList)
                    //{
                    //    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    //    {
                    //        continue;
                    //    }
                    //    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    //    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    //    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sourceKey)
                    //        && !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                    //    {
                    //        if (double.IsNaN(dart))
                    //        {
                    //            dart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                    //            if (UptosCheckBox.IsChecked == true)
                    //            {
                    //                double sinkDart = 0;
                    //                if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                {
                    //                    sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    dart = sinkDart - dart;
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            double tempDart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                    //            if (isAggAvg)
                    //            {
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    }
                    //                    dart += (sinkDart - tempDart);
                    //                }
                    //                else
                    //                {
                    //                    dart += tempDart;
                    //                }
                    //            }
                    //            if (isAggMax)
                    //            {
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    }
                    //                    if (dart < sinkDart - tempDart)
                    //                    {
                    //                        dart = sinkDart - tempDart;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (dart < tempDart)
                    //                    {
                    //                        dart = tempDart;
                    //                    }
                    //                }
                    //            }
                    //            if (isAggMin)
                    //            {
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    }
                    //                    if (dart > sinkDart - tempDart)
                    //                    {
                    //                        dart = sinkDart - tempDart;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (dart > tempDart)
                    //                    {
                    //                        dart = tempDart;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        count++;
                    //    }
                    //}
                    //if (!double.IsNaN(dart))
                    //{
                    //    if (isAggAvg)
                    //        dart = dart / count;

                    //    DateTime indexDateTime = dateList.First().AddHours(i + 1);
                    //    CalcDartStats(indexDateTime, dart, dartSum, out dartSum, dartTotal, out dartTotal, dartOffPeakSum, out dartOffPeakSum, dartPeakSum, out dartPeakSum, dartOffPeakTotal,
                    //                    out dartOffPeakTotal, dartPeakTotal, out dartPeakTotal, dartMax, out dartMax, dartMin, out dartMin);
                    //}

                    //#endregion

                    //#region RATIO

                    //count = 0;
                    //foreach (DateTime date in dateList)
                    //{
                    //    double temprdart = double.NaN;
                    //    double temprda = double.NaN;
                    //    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    //    {
                    //        continue;
                    //    }
                    //    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    //    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    //    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sourceKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                    //    {
                    //        if (double.IsNaN(ratio))
                    //        {
                    //            temprdart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                    //            temprda = DARTNode.sDAHash[sourceKey];
                    //            if (UptosCheckBox.IsChecked == true)
                    //            {
                    //                double sinkDart = 0;
                    //                if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                {
                    //                    sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    temprdart = sinkDart - temprdart;
                    //                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                    //                }
                    //            }
                    //            if (temprda != 0)
                    //            {
                    //                ratio = temprdart / temprda;
                    //            }
                    //            else
                    //            {
                    //                ratio = temprdart;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            temprdart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                    //            temprda = DARTNode.sDAHash[sourceKey];
                    //            double tempratio = 0;
                    //            if (temprda != 0)
                    //            {
                    //                tempratio = temprdart / temprda;
                    //            }
                    //            else
                    //            {
                    //                tempratio = temprdart;
                    //            }
                    //            if (isAggAvg)
                    //            {
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    }
                    //                    temprdart = sinkDart - temprdart;
                    //                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                    //                    if (temprda != 0)
                    //                    {
                    //                        tempratio = temprdart / temprda;
                    //                    }
                    //                    else
                    //                    {
                    //                        tempratio = temprdart;
                    //                    }
                    //                    ratio += tempratio;
                    //                }
                    //                else
                    //                {
                    //                    ratio += tempratio;
                    //                }
                    //            }
                    //            if (isAggMax)
                    //            {
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    }
                    //                    temprdart = sinkDart - temprdart;
                    //                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                    //                    if (temprda != 0)
                    //                    {
                    //                        tempratio = temprdart / temprda;
                    //                    }
                    //                    else
                    //                    {
                    //                        tempratio = temprdart;
                    //                    }
                    //                    ratio += tempratio;
                    //                    if (ratio < tempratio)
                    //                    {
                    //                        ratio = tempratio;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (ratio < tempratio)
                    //                    {
                    //                        ratio = tempratio;
                    //                    }
                    //                }
                    //            }
                    //            if (isAggMin)
                    //            {
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                    //                    && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                    //                    }
                    //                    temprdart = sinkDart - temprdart;
                    //                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                    //                    if (temprda != 0)
                    //                    {
                    //                        tempratio = temprdart / temprda;
                    //                    }
                    //                    else
                    //                    {
                    //                        tempratio = temprdart;
                    //                    }
                    //                    ratio += tempratio;
                    //                    if (ratio > tempratio)
                    //                    {
                    //                        ratio = tempratio;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (ratio > tempratio)
                    //                    {
                    //                        ratio = tempratio;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        count++;
                    //    }
                    //}
                    //if (!double.IsNaN(ratio))
                    //{
                    //    if (isAggAvg)
                    //        ratio = ratio / count;

                    //    DateTime indexDateTime = dateList.First().AddHours(i + 1);
                    //    CalcDartStats(indexDateTime, ratio, ratioSum, out ratioSum, ratioTotal, out ratioTotal, ratioOffPeakSum, out ratioOffPeakSum, ratioPeakSum, out ratioPeakSum, ratioOffPeakTotal,
                    //                    out ratioOffPeakTotal, ratioPeakTotal, out ratioPeakTotal, ratioMax, out ratioMax, ratioMin, out ratioMin);
                    //}

                    //#endregion

                    #endregion

                    #region LMP New Code


                    #region DA

                    if (isDaChecked)
                    {
                        double count = 0;

                        #region DA Price

                        if (isPriceChecked)
                        {
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region DA Price Calculation

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Price))
                                {
                                    if (double.IsNaN(daPrice))
                                    {
                                        daPrice = DARTNode.sDALmpHash[sourceKey].Price;
                                        if (sinkKey != null)
                                        {
                                            double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                                            daPrice = sinkDa - DARTNode.sDALmpHash[sourceKey].Price;
                                        }
                                    }
                                    else
                                    {
                                        if (isAggAvg)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                                                daPrice += (sinkDa - DARTNode.sDALmpHash[sourceKey].Price);
                                            }
                                            else
                                            {
                                                daPrice += DARTNode.sDALmpHash[sourceKey].Price;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                                                if (daPrice < sinkDa - DARTNode.sDALmpHash[sourceKey].Price)
                                                {
                                                    daPrice = sinkDa - DARTNode.sDALmpHash[sourceKey].Price;
                                                }
                                            }
                                            else
                                            {
                                                if (daPrice < DARTNode.sDALmpHash[sourceKey].Price)
                                                {
                                                    daPrice = DARTNode.sDALmpHash[sourceKey].Price;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                                                if (daPrice > sinkDa - DARTNode.sDALmpHash[sourceKey].Price)
                                                {
                                                    daPrice = sinkDa - DARTNode.sDALmpHash[sourceKey].Price;
                                                }
                                            }
                                            else
                                            {
                                                if (daPrice > DARTNode.sDALmpHash[sourceKey].Price)
                                                {
                                                    daPrice = DARTNode.sDALmpHash[sourceKey].Price;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(daPrice))
                            {
                                if (isAggAvg)
                                    daPrice = daPrice / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, daPrice, daPriceSum, out daPriceSum, daPriceTotal, out daPriceTotal, daPriceOffPeakSum, out daPriceOffPeakSum, daPricePeakSum, out daPricePeakSum
                                                , daPriceOffPeakTotal, out daPriceOffPeakTotal, daPricePeakTotal, out daPricePeakTotal, daPriceMax, out daPriceMax, daPriceMin, out daPriceMin);
                            }
                        }

                        #endregion

                        #region DA Congestion

                        if (isCongestionChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region DA Congestion Calculation

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Congestion))
                                {
                                    if (double.IsNaN(daCongestion))
                                    {
                                        daCongestion = DARTNode.sDALmpHash[sourceKey].Congestion;
                                        if (sinkKey != null)
                                        {
                                            double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                                            daCongestion = sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion;
                                        }
                                    }
                                    else
                                    {
                                        if (isAggAvg)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                                                daCongestion += (sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion);
                                            }
                                            else
                                            {
                                                daCongestion += DARTNode.sDALmpHash[sourceKey].Congestion;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                                                if (daCongestion < sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion)
                                                {
                                                    daCongestion = sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion;
                                                }
                                            }
                                            else
                                            {
                                                if (daCongestion < DARTNode.sDALmpHash[sourceKey].Congestion)
                                                {
                                                    daCongestion = DARTNode.sDALmpHash[sourceKey].Congestion;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                                                if (daCongestion > sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion)
                                                {
                                                    daCongestion = sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion;
                                                }
                                            }
                                            else
                                            {
                                                if (daCongestion > DARTNode.sDALmpHash[sourceKey].Congestion)
                                                {
                                                    daCongestion = DARTNode.sDALmpHash[sourceKey].Congestion;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(daCongestion))
                            {
                                if (isAggAvg)
                                    daCongestion = daCongestion / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, daCongestion, daCongestionSum, out daCongestionSum, daCongestionTotal, out daCongestionTotal, daCongestionOffPeakSum, out daCongestionOffPeakSum
                                                , daCongestionPeakSum, out daCongestionPeakSum, daCongestionOffPeakTotal, out daCongestionOffPeakTotal, daCongestionPeakTotal, out daCongestionPeakTotal
                                                , daCongestionMax, out daCongestionMax, daCongestionMin, out daCongestionMin);
                            }

                        }

                        #endregion

                        #region DA Loss

                        if (isLossChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region DA Loss Calculation

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Loss))
                                {
                                    if (double.IsNaN(daLoss))
                                    {
                                        daLoss = DARTNode.sDALmpHash[sourceKey].Loss;
                                        if (sinkKey != null)
                                        {
                                            double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                                            daLoss = sinkDa - DARTNode.sDALmpHash[sourceKey].Loss;
                                        }
                                    }
                                    else
                                    {
                                        if (isAggAvg)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                                                daLoss += (sinkDa - DARTNode.sDALmpHash[sourceKey].Loss);
                                            }
                                            else
                                            {
                                                daLoss += DARTNode.sDALmpHash[sourceKey].Loss;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                                                if (daLoss < sinkDa - DARTNode.sDALmpHash[sourceKey].Loss)
                                                {
                                                    daLoss = sinkDa - DARTNode.sDALmpHash[sourceKey].Loss;
                                                }
                                            }
                                            else
                                            {
                                                if (daLoss < DARTNode.sDALmpHash[sourceKey].Loss)
                                                {
                                                    daLoss = DARTNode.sDALmpHash[sourceKey].Loss;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                                                if (daLoss > sinkDa - DARTNode.sDALmpHash[sourceKey].Loss)
                                                {
                                                    daLoss = sinkDa - DARTNode.sDALmpHash[sourceKey].Loss;
                                                }
                                            }
                                            else
                                            {
                                                if (daLoss > DARTNode.sDALmpHash[sourceKey].Loss)
                                                {
                                                    daLoss = DARTNode.sDALmpHash[sourceKey].Loss;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(daLoss))
                            {
                                if (isAggAvg)
                                    daLoss = daLoss / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, daLoss, daLossSum, out daLossSum, daLossTotal, out daLossTotal, daLossOffPeakSum, out daLossOffPeakSum, daLossPeakSum, out daLossPeakSum, daLossOffPeakTotal,
                                                out daLossOffPeakTotal, daLossPeakTotal, out daLossPeakTotal, daLossMax, out daLossMax, daLossMin, out daLossMin);
                            }

                        }

                        #endregion

                    }

                    #endregion


                    #region RT

                    if (isRtChecked)
                    {
                        double count = 0;

                        #region RT Price

                        if (isPriceChecked)
                        {
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region RT Price Calculation
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Price))
                                {
                                    if (double.IsNaN(rtPrice))
                                    {
                                        rtPrice = DARTNode.sRTLmpHash[sourceKey].Price;
                                        if (sinkKey != null)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                                                rtPrice = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (isAggAvg)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                                                rtPrice += (sinkRt - DARTNode.sRTLmpHash[sourceKey].Price);
                                            }
                                            else
                                            {
                                                rtPrice += DARTNode.sRTLmpHash[sourceKey].Price;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                                                if (rtPrice < sinkRt - DARTNode.sRTLmpHash[sourceKey].Price)
                                                {
                                                    rtPrice = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                                                }
                                            }
                                            else
                                            {
                                                if (rtPrice < DARTNode.sRTLmpHash[sourceKey].Price)
                                                {
                                                    rtPrice = DARTNode.sRTLmpHash[sourceKey].Price;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                                                if (rtPrice > sinkRt - DARTNode.sRTLmpHash[sourceKey].Price)
                                                {
                                                    rtPrice = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                                                }
                                            }
                                            else
                                            {
                                                if (rtPrice > DARTNode.sRTLmpHash[sourceKey].Price)
                                                {
                                                    rtPrice = DARTNode.sRTLmpHash[sourceKey].Price;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }
                                #endregion
                            }
                            if (!double.IsNaN(rtPrice))
                            {
                                if (isAggAvg)
                                    rtPrice = rtPrice / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, rtPrice, rtPriceSum, out rtPriceSum, rtPriceTotal, out rtPriceTotal, rtPriceOffPeakSum, out rtPriceOffPeakSum, rtPricePeakSum, out rtPricePeakSum
                                                , rtPriceOffPeakTotal, out rtPriceOffPeakTotal, rtPricePeakTotal, out rtPricePeakTotal, rtPriceMax, out rtPriceMax, rtPriceMin, out rtPriceMin);
                            }
                        }

                        #endregion

                        #region RT Congestion

                        if (isCongestionChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region RT Congestion Calculation
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Congestion))
                                {
                                    if (double.IsNaN(rtCongestion))
                                    {
                                        rtCongestion = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                        if (sinkKey != null)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                                                rtCongestion = sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (isAggAvg)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                                                rtCongestion += (sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion);
                                            }
                                            else
                                            {
                                                rtCongestion += DARTNode.sRTLmpHash[sourceKey].Congestion;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                                                if (rtCongestion < sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion)
                                                {
                                                    rtCongestion = sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion;
                                                }
                                            }
                                            else
                                            {
                                                if (rtCongestion < DARTNode.sRTLmpHash[sourceKey].Congestion)
                                                {
                                                    rtCongestion = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                                                if (rtCongestion > sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion)
                                                {
                                                    rtCongestion = sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion;
                                                }
                                            }
                                            else
                                            {
                                                if (rtCongestion > DARTNode.sRTLmpHash[sourceKey].Congestion)
                                                {
                                                    rtCongestion = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }
                                #endregion
                            }
                            if (!double.IsNaN(rtCongestion))
                            {
                                if (isAggAvg)
                                    rtCongestion = rtCongestion / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, rtCongestion, rtCongestionSum, out rtCongestionSum, rtCongestionTotal, out rtCongestionTotal, rtCongestionOffPeakSum, out rtCongestionOffPeakSum
                                                , rtCongestionPeakSum, out rtCongestionPeakSum, rtCongestionOffPeakTotal, out rtCongestionOffPeakTotal, rtCongestionPeakTotal, out rtCongestionPeakTotal
                                                , rtCongestionMax, out rtCongestionMax, rtCongestionMin, out rtCongestionMin);
                            }
                        }

                        #endregion

                        #region RT Loss

                        if (isLossChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region RT Loss Calculation
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Loss))
                                {
                                    if (double.IsNaN(rtLoss))
                                    {
                                        rtLoss = DARTNode.sRTLmpHash[sourceKey].Loss;
                                        if (sinkKey != null)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                                                rtLoss = sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (isAggAvg)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                                                rtLoss += (sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss);
                                            }
                                            else
                                            {
                                                rtLoss += DARTNode.sRTLmpHash[sourceKey].Loss;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                                                if (rtLoss < sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss)
                                                {
                                                    rtLoss = sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss;
                                                }
                                            }
                                            else
                                            {
                                                if (rtLoss < DARTNode.sRTLmpHash[sourceKey].Loss)
                                                {
                                                    rtLoss = DARTNode.sRTLmpHash[sourceKey].Loss;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                                                if (rtLoss > sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss)
                                                {
                                                    rtLoss = sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss;
                                                }
                                            }
                                            else
                                            {
                                                if (rtLoss > DARTNode.sRTLmpHash[sourceKey].Loss)
                                                {
                                                    rtLoss = DARTNode.sRTLmpHash[sourceKey].Loss;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }
                                #endregion

                            }
                            if (!double.IsNaN(rtLoss))
                            {
                                if (isAggAvg)
                                    rtLoss = rtLoss / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, rtLoss, rtLossSum, out rtLossSum, rtLossTotal, out rtLossTotal, rtLossOffPeakSum, out rtLossOffPeakSum, rtLossPeakSum, out rtLossPeakSum, rtLossOffPeakTotal,
                                                out rtLossOffPeakTotal, rtLossPeakTotal, out rtLossPeakTotal, rtLossMax, out rtLossMax, rtLossMin, out rtLossMin);
                            }
                        }

                        #endregion
                    }

                    #endregion


                    #region DART

                    if (isDartChecked)
                    {
                        double count = 0;

                        #region DART Price

                        if (isPriceChecked)
                        {
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region DART Price Calculation

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                                    && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Price))
                                {
                                    if (double.IsNaN(dartPrice))
                                    {
                                        dartPrice = (DARTNode.sRTLmpHash[sourceKey].Price - DARTNode.sDALmpHash[sourceKey].Price);
                                        if (UptosCheckBox.IsChecked == true)
                                        {
                                            double sinkDart = 0;
                                            if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                            {
                                                sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                dartPrice = sinkDart - dartPrice;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        double tempDart = (DARTNode.sRTLmpHash[sourceKey].Price - DARTNode.sDALmpHash[sourceKey].Price);
                                        if (isAggAvg)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                }
                                                dartPrice += (sinkDart - tempDart);
                                            }
                                            else
                                            {
                                                dartPrice += tempDart;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                }
                                                if (dartPrice < sinkDart - tempDart)
                                                {
                                                    dartPrice = sinkDart - tempDart;
                                                }
                                            }
                                            else
                                            {
                                                if (dartPrice < tempDart)
                                                {
                                                    dartPrice = tempDart;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                }
                                                if (dartPrice > sinkDart - tempDart)
                                                {
                                                    dartPrice = sinkDart - tempDart;
                                                }
                                            }
                                            else
                                            {
                                                if (dartPrice > tempDart)
                                                {
                                                    dartPrice = tempDart;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(dartPrice))
                            {
                                if (isAggAvg)
                                    dartPrice = dartPrice / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, dartPrice, dartPriceSum, out dartPriceSum, dartPriceTotal, out dartPriceTotal, dartPriceOffPeakSum, out dartPriceOffPeakSum, dartPricePeakSum, out dartPricePeakSum, dartPriceOffPeakTotal,
                                                out dartPriceOffPeakTotal, dartPricePeakTotal, out dartPricePeakTotal, dartPriceMax, out dartPriceMax, dartPriceMin, out dartPriceMin);
                            }
                        }

                        #endregion

                        #region DART Congestion

                        if (isCongestionChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region DART Congestion Calculation

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                                    && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Congestion))
                                {
                                    if (double.IsNaN(dartCongestion))
                                    {
                                        dartCongestion = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                                        if (UptosCheckBox.IsChecked == true)
                                        {
                                            double sinkDart = 0;
                                            if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                            {
                                                sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                dartCongestion = sinkDart - dartCongestion;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        double tempDart = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                                        if (isAggAvg)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                }
                                                dartCongestion += (sinkDart - tempDart);
                                            }
                                            else
                                            {
                                                dartCongestion += tempDart;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                }
                                                if (dartCongestion < sinkDart - tempDart)
                                                {
                                                    dartCongestion = sinkDart - tempDart;
                                                }
                                            }
                                            else
                                            {
                                                if (dartCongestion < tempDart)
                                                {
                                                    dartCongestion = tempDart;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                }
                                                if (dartCongestion > sinkDart - tempDart)
                                                {
                                                    dartCongestion = sinkDart - tempDart;
                                                }
                                            }
                                            else
                                            {
                                                if (dartCongestion > tempDart)
                                                {
                                                    dartCongestion = tempDart;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(dartCongestion))
                            {
                                if (isAggAvg)
                                    dartCongestion = dartCongestion / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, dartCongestion, dartCongestionSum, out dartCongestionSum, dartCongestionTotal, out dartCongestionTotal
                                                , dartCongestionOffPeakSum, out dartCongestionOffPeakSum, dartCongestionPeakSum, out dartCongestionPeakSum, dartCongestionOffPeakTotal,
                                                out dartCongestionOffPeakTotal, dartCongestionPeakTotal, out dartCongestionPeakTotal, dartCongestionMax, out dartCongestionMax
                                                , dartCongestionMin, out dartCongestionMin);
                            }
                        }

                        #endregion

                        #region DART Loss

                        if (isLossChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region DART Loss Calculation

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                                    && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Loss))
                                {
                                    if (double.IsNaN(dartLoss))
                                    {
                                        dartLoss = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                                        if (UptosCheckBox.IsChecked == true)
                                        {
                                            double sinkDart = 0;
                                            if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                            {
                                                sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                dartLoss = sinkDart - dartLoss;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        double tempDart = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                                        if (isAggAvg)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                }
                                                dartLoss += (sinkDart - tempDart);
                                            }
                                            else
                                            {
                                                dartLoss += tempDart;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                }
                                                if (dartLoss < sinkDart - tempDart)
                                                {
                                                    dartLoss = sinkDart - tempDart;
                                                }
                                            }
                                            else
                                            {
                                                if (dartLoss < tempDart)
                                                {
                                                    dartLoss = tempDart;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                }
                                                if (dartLoss > sinkDart - tempDart)
                                                {
                                                    dartLoss = sinkDart - tempDart;
                                                }
                                            }
                                            else
                                            {
                                                if (dartLoss > tempDart)
                                                {
                                                    dartLoss = tempDart;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(dartLoss))
                            {
                                if (isAggAvg)
                                    dartLoss = dartLoss / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, dartLoss, dartLossSum, out dartLossSum, dartLossTotal, out dartLossTotal, dartLossOffPeakSum, out dartLossOffPeakSum, dartLossPeakSum, out dartLossPeakSum, dartLossOffPeakTotal,
                                                out dartLossOffPeakTotal, dartLossPeakTotal, out dartLossPeakTotal, dartLossMax, out dartLossMax, dartLossMin, out dartLossMin);
                            }
                        }

                        #endregion

                    }

                    #endregion


                    #region RATIO

                    if (isRatioChecked)
                    {
                        double count = 0;

                        #region RATIO Price

                        if (isPriceChecked)
                        {
                            foreach (DateTime date in dateList)
                            {
                                double temprdart = double.NaN;
                                double temprda = double.NaN;
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region RATIO Price Calculation

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Price))
                                {
                                    if (double.IsNaN(ratioPrice))
                                    {
                                        temprdart = (DARTNode.sRTLmpHash[sourceKey].Price - DARTNode.sDALmpHash[sourceKey].Price);
                                        temprda = DARTNode.sDALmpHash[sourceKey].Price;
                                        if (UptosCheckBox.IsChecked == true)
                                        {
                                            double sinkDart = 0;
                                            if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                            {
                                                sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Price - temprda;
                                            }
                                        }
                                        if (temprda != 0)
                                        {
                                            ratioPrice = temprdart / temprda;
                                        }
                                        else
                                        {
                                            ratioPrice = temprdart;
                                        }
                                    }
                                    else
                                    {
                                        temprdart = (DARTNode.sRTLmpHash[sourceKey].Price - DARTNode.sDALmpHash[sourceKey].Price);
                                        temprda = DARTNode.sDALmpHash[sourceKey].Price;
                                        double tempratio = 0;
                                        if (temprda != 0)
                                        {
                                            tempratio = temprdart / temprda;
                                        }
                                        else
                                        {
                                            tempratio = temprdart;
                                        }
                                        if (isAggAvg)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Price - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioPrice += tempratio;
                                            }
                                            else
                                            {
                                                ratioPrice += tempratio;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Price - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioPrice += tempratio;
                                                if (ratioPrice < tempratio)
                                                {
                                                    ratioPrice = tempratio;
                                                }
                                            }
                                            else
                                            {
                                                if (ratioPrice < tempratio)
                                                {
                                                    ratioPrice = tempratio;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sDALmpHash[sinkKey].Price);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Price - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioPrice += tempratio;
                                                if (ratioPrice > tempratio)
                                                {
                                                    ratioPrice = tempratio;
                                                }
                                            }
                                            else
                                            {
                                                if (ratioPrice > tempratio)
                                                {
                                                    ratioPrice = tempratio;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(ratioPrice))
                            {
                                if (isAggAvg)
                                    ratioPrice = ratioPrice / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, ratioPrice, ratioPriceSum, out ratioPriceSum, ratioPriceTotal, out ratioPriceTotal, ratioPriceOffPeakSum, out ratioPriceOffPeakSum
                                                , ratioPricePeakSum, out ratioPricePeakSum, ratioPriceOffPeakTotal, out ratioPriceOffPeakTotal, ratioPricePeakTotal, out ratioPricePeakTotal
                                                , ratioPriceMax, out ratioPriceMax, ratioPriceMin, out ratioPriceMin);
                            }
                        }

                        #endregion

                        #region RATIO Congestion

                        if (isCongestionChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                double temprdart = double.NaN;
                                double temprda = double.NaN;
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region RATIO Congestion

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Congestion))
                                {
                                    if (double.IsNaN(ratioCongestion))
                                    {
                                        temprdart = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                                        temprda = DARTNode.sDALmpHash[sourceKey].Congestion;
                                        if (UptosCheckBox.IsChecked == true)
                                        {
                                            double sinkDart = 0;
                                            if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                            {
                                                sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Congestion - temprda;
                                            }
                                        }
                                        if (temprda != 0)
                                        {
                                            ratioCongestion = temprdart / temprda;
                                        }
                                        else
                                        {
                                            ratioCongestion = temprdart;
                                        }
                                    }
                                    else
                                    {
                                        temprdart = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                                        temprda = DARTNode.sDALmpHash[sourceKey].Congestion;
                                        double tempratio = 0;
                                        if (temprda != 0)
                                        {
                                            tempratio = temprdart / temprda;
                                        }
                                        else
                                        {
                                            tempratio = temprdart;
                                        }
                                        if (isAggAvg)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Congestion - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioCongestion += tempratio;
                                            }
                                            else
                                            {
                                                ratioCongestion += tempratio;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Congestion - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioCongestion += tempratio;
                                                if (ratioCongestion < tempratio)
                                                {
                                                    ratioCongestion = tempratio;
                                                }
                                            }
                                            else
                                            {
                                                if (ratioCongestion < tempratio)
                                                {
                                                    ratioCongestion = tempratio;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Congestion - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioCongestion += tempratio;
                                                if (ratioCongestion > tempratio)
                                                {
                                                    ratioCongestion = tempratio;
                                                }
                                            }
                                            else
                                            {
                                                if (ratioCongestion > tempratio)
                                                {
                                                    ratioCongestion = tempratio;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(ratioCongestion))
                            {
                                if (isAggAvg)
                                    ratioCongestion = ratioCongestion / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, ratioCongestion, ratioCongestionSum, out ratioCongestionSum, ratioCongestionTotal, out ratioCongestionTotal
                                                , ratioCongestionOffPeakSum, out ratioCongestionOffPeakSum, ratioCongestionPeakSum, out ratioCongestionPeakSum
                                                , ratioCongestionOffPeakTotal, out ratioCongestionOffPeakTotal, ratioCongestionPeakTotal, out ratioCongestionPeakTotal
                                                , ratioCongestionMax, out ratioCongestionMax, ratioCongestionMin, out ratioCongestionMin);
                            }
                        }

                        #endregion

                        #region RATIO Loss

                        if (isLossChecked)
                        {
                            count = 0;
                            foreach (DateTime date in dateList)
                            {
                                double temprdart = double.NaN;
                                double temprda = double.NaN;
                                if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                                {
                                    continue;
                                }
                                string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                                string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                                #region RATIO Loss

                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Loss))
                                {
                                    if (double.IsNaN(ratioLoss))
                                    {
                                        temprdart = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                                        temprda = DARTNode.sDALmpHash[sourceKey].Loss;
                                        if (UptosCheckBox.IsChecked == true)
                                        {
                                            double sinkDart = 0;
                                            if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                            {
                                                sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Loss - temprda;
                                            }
                                        }
                                        if (temprda != 0)
                                        {
                                            ratioLoss = temprdart / temprda;
                                        }
                                        else
                                        {
                                            ratioLoss = temprdart;
                                        }
                                    }
                                    else
                                    {
                                        temprdart = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                                        temprda = DARTNode.sDALmpHash[sourceKey].Loss;
                                        double tempratio = 0;
                                        if (temprda != 0)
                                        {
                                            tempratio = temprdart / temprda;
                                        }
                                        else
                                        {
                                            tempratio = temprdart;
                                        }
                                        if (isAggAvg)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Loss - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioLoss += tempratio;
                                            }
                                            else
                                            {
                                                ratioLoss += tempratio;
                                            }
                                        }
                                        if (isAggMax)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Loss - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioLoss += tempratio;
                                                if (ratioLoss < tempratio)
                                                {
                                                    ratioLoss = tempratio;
                                                }
                                            }
                                            else
                                            {
                                                if (ratioLoss < tempratio)
                                                {
                                                    ratioLoss = tempratio;
                                                }
                                            }
                                        }
                                        if (isAggMin)
                                        {
                                            if (UptosCheckBox.IsChecked == true)
                                            {
                                                double sinkDart = 0;
                                                if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                                                && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                                                {
                                                    sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                                                }
                                                temprdart = sinkDart - temprdart;
                                                temprda = DARTNode.sDALmpHash[sinkKey].Loss - temprda;
                                                if (temprda != 0)
                                                {
                                                    tempratio = temprdart / temprda;
                                                }
                                                else
                                                {
                                                    tempratio = temprdart;
                                                }
                                                ratioLoss += tempratio;
                                                if (ratioLoss > tempratio)
                                                {
                                                    ratioLoss = tempratio;
                                                }
                                            }
                                            else
                                            {
                                                if (ratioLoss > tempratio)
                                                {
                                                    ratioLoss = tempratio;
                                                }
                                            }
                                        }
                                    }
                                    count++;
                                }

                                #endregion

                            }
                            if (!double.IsNaN(ratioLoss))
                            {
                                if (isAggAvg)
                                    ratioLoss = ratioLoss / count;

                                DateTime indexDateTime = dateList.First().AddHours(i + 1);

                                CalcDartStats(indexDateTime, ratioLoss, ratioLossSum, out ratioLossSum, ratioLossTotal, out ratioLossTotal, ratioLossOffPeakSum, out ratioLossOffPeakSum
                                                , ratioLossPeakSum, out ratioLossPeakSum, ratioLossOffPeakTotal, out ratioLossOffPeakTotal, ratioLossPeakTotal, out ratioLossPeakTotal
                                                , ratioLossMax, out ratioLossMax, ratioLossMin, out ratioLossMin);
                            }
                        }

                        #endregion

                    }

                    #endregion



                    #region commented code

                    //#region Congestion

                    //count = 0;
                    //foreach (DateTime date in dateList)
                    //{
                    //    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    //    {
                    //        continue;
                    //    }
                    //    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    //    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                    //    if (isDaChecked)
                    //    {
                    //        if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Congestion))
                    //        {
                    //            if (double.IsNaN(congestion))
                    //            {
                    //                congestion = DARTNode.sDALmpHash[sourceKey].Congestion;
                    //            }
                    //            else
                    //            {
                    //                if (isAggAvg)
                    //                {
                    //                    congestion += DARTNode.sDALmpHash[sourceKey].Congestion;
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (congestion < DARTNode.sDALmpHash[sourceKey].Congestion)
                    //                    {
                    //                        congestion = DARTNode.sDALmpHash[sourceKey].Congestion;
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (congestion > DARTNode.sDALmpHash[sourceKey].Congestion)
                    //                    {
                    //                        congestion = DARTNode.sDALmpHash[sourceKey].Congestion;
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //    else if (isRtChecked)
                    //    {
                    //        if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Congestion))
                    //        {
                    //            if (double.IsNaN(congestion))
                    //            {
                    //                congestion = DARTNode.sRTLmpHash[sourceKey].Congestion;
                    //            }
                    //            else
                    //            {
                    //                if (isAggAvg)
                    //                {
                    //                    congestion += DARTNode.sRTLmpHash[sourceKey].Congestion;
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (congestion < DARTNode.sRTLmpHash[sourceKey].Congestion)
                    //                    {
                    //                        congestion = DARTNode.sRTLmpHash[sourceKey].Congestion;
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (congestion > DARTNode.sRTLmpHash[sourceKey].Congestion)
                    //                    {
                    //                        congestion = DARTNode.sRTLmpHash[sourceKey].Congestion;
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //    else if (isDartChecked)
                    //    {
                    //        if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                    //        && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Congestion))
                    //        {
                    //            if (double.IsNaN(dart))
                    //            {
                    //                dart = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        dart = sinkDart - dart;
                    //                    }
                    //                }
                    //            }
                    //            else
                    //            {
                    //                double tempDart = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                    //                if (isAggAvg)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        }
                    //                        dart += (sinkDart - tempDart);
                    //                    }
                    //                    else
                    //                    {
                    //                        dart += tempDart;
                    //                    }
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        }
                    //                        if (dart < sinkDart - tempDart)
                    //                        {
                    //                            dart = sinkDart - tempDart;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (dart < tempDart)
                    //                        {
                    //                            dart = tempDart;
                    //                        }
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        }
                    //                        if (dart > sinkDart - tempDart)
                    //                        {
                    //                            dart = sinkDart - tempDart;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (dart > tempDart)
                    //                        {
                    //                            dart = tempDart;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //    else if (isRatioChecked)
                    //    {
                    //        double temprdart = double.NaN;
                    //        double temprda = double.NaN;

                    //        if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                    //                    && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Congestion))
                    //        {
                    //            if (double.IsNaN(ratio))
                    //            {
                    //                temprdart = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                    //                temprda = DARTNode.sDALmpHash[sourceKey].Congestion;
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Congestion - temprda;
                    //                    }
                    //                }
                    //                if (temprda != 0)
                    //                {
                    //                    ratio = temprdart / temprda;
                    //                }
                    //                else
                    //                {
                    //                    ratio = temprdart;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                temprdart = (DARTNode.sRTLmpHash[sourceKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                    //                temprda = DARTNode.sDALmpHash[sourceKey].Congestion;
                    //                double tempratio = 0;
                    //                if (temprda != 0)
                    //                {
                    //                    tempratio = temprdart / temprda;
                    //                }
                    //                else
                    //                {
                    //                    tempratio = temprdart;
                    //                }
                    //                if (isAggAvg)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        }
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Congestion - temprda;
                    //                        if (temprda != 0)
                    //                        {
                    //                            tempratio = temprdart / temprda;
                    //                        }
                    //                        else
                    //                        {
                    //                            tempratio = temprdart;
                    //                        }
                    //                        ratio += tempratio;
                    //                    }
                    //                    else
                    //                    {
                    //                        ratio += tempratio;
                    //                    }
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        }
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Price - temprda;
                    //                        if (temprda != 0)
                    //                        {
                    //                            tempratio = temprdart / temprda;
                    //                        }
                    //                        else
                    //                        {
                    //                            tempratio = temprdart;
                    //                        }
                    //                        ratio += tempratio;
                    //                        if (ratio < tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (ratio < tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sinkKey].Congestion);
                    //                        }
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Congestion - temprda;
                    //                        if (temprda != 0)
                    //                        {
                    //                            tempratio = temprdart / temprda;
                    //                        }
                    //                        else
                    //                        {
                    //                            tempratio = temprdart;
                    //                        }
                    //                        ratio += tempratio;
                    //                        if (ratio > tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (ratio > tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //}
                    //if (!double.IsNaN(congestion))
                    //{
                    //    if (isAggAvg)
                    //        congestion = congestion / count;

                    //    DateTime indexDateTime = dateList.First().AddHours(i + 1);

                    //    //CalcDartStats(indexDateTime, da, daSum, out daSum, daTotal, out daTotal, daOffPeakSum, out daOffPeakSum, daPeakSum, out daPeakSum, daOffPeakTotal,
                    //    //                out daOffPeakTotal, daPeakTotal, out daPeakTotal, daMax, out daMax, daMin, out daMin);

                    //    CalcDartStats(indexDateTime, da, daSum, out daSum, daTotal, out daTotal, daOffPeakSum, out daOffPeakSum, daPeakSum, out daPeakSum, daOffPeakTotal,
                    //                    out daOffPeakTotal, daPeakTotal, out daPeakTotal, daMax, out daMax, daMin, out daMin,
                    //                    congestionSum, out congestionSum, lossSum, out lossSum, congestionTotal, out congestionTotal, lossTotal, out lossTotal,
                    //                    congestionPeakSum, out congestionPeakSum, lossPeakSum, out lossPeakSum, congestionOffPeakSum, out congestionOffPeakSum, lossOffPeakSum, out lossOffPeakSum,
                    //                    congestionPeakTotal, out congestionPeakTotal, lossPeakTotal, out lossPeakTotal, congestionOffPeakTotal, out congestionOffPeakTotal, lossOffPeakTotal, out lossOffPeakTotal,
                    //                    congestionMax, out congestionMax, lossMax, out lossMax, congestionMin, out congestionMin, lossMin, out lossMin);
                    //}

                    //#endregion

                    //#region Loss

                    //count = 0;
                    //foreach (DateTime date in dateList)
                    //{
                    //    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    //    {
                    //        continue;
                    //    }
                    //    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    //    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();

                    //    if (isDaChecked)
                    //    {
                    //        if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Loss))
                    //        {
                    //            if (double.IsNaN(loss))
                    //            {
                    //                loss = DARTNode.sDALmpHash[sourceKey].Loss;
                    //            }
                    //            else
                    //            {
                    //                if (isAggAvg)
                    //                {
                    //                    loss += DARTNode.sDALmpHash[sourceKey].Loss;
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (loss < DARTNode.sDALmpHash[sourceKey].Loss)
                    //                    {
                    //                        loss = DARTNode.sDALmpHash[sourceKey].Loss;
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (loss > DARTNode.sDALmpHash[sourceKey].Loss)
                    //                    {
                    //                        loss = DARTNode.sDALmpHash[sourceKey].Loss;
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //    else if (isRtChecked)
                    //    {
                    //        if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Loss))
                    //        {
                    //            if (double.IsNaN(loss))
                    //            {
                    //                loss = DARTNode.sRTLmpHash[sourceKey].Loss;
                    //            }
                    //            else
                    //            {
                    //                if (isAggAvg)
                    //                {
                    //                    loss += DARTNode.sRTLmpHash[sourceKey].Loss;
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (loss < DARTNode.sRTLmpHash[sourceKey].Loss)
                    //                    {
                    //                        loss = DARTNode.sRTLmpHash[sourceKey].Loss;
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (loss > DARTNode.sRTLmpHash[sourceKey].Loss)
                    //                    {
                    //                        loss = DARTNode.sRTLmpHash[sourceKey].Loss;
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //    else if (isDartChecked)
                    //    {
                    //        if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                    //        && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Loss))
                    //        {
                    //            if (double.IsNaN(dart))
                    //            {
                    //                dart = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        dart = sinkDart - dart;
                    //                    }
                    //                }
                    //            }
                    //            else
                    //            {
                    //                double tempDart = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                    //                if (isAggAvg)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        }
                    //                        dart += (sinkDart - tempDart);
                    //                    }
                    //                    else
                    //                    {
                    //                        dart += tempDart;
                    //                    }
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        }
                    //                        if (dart < sinkDart - tempDart)
                    //                        {
                    //                            dart = sinkDart - tempDart;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (dart < tempDart)
                    //                        {
                    //                            dart = tempDart;
                    //                        }
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        }
                    //                        if (dart > sinkDart - tempDart)
                    //                        {
                    //                            dart = sinkDart - tempDart;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (dart > tempDart)
                    //                        {
                    //                            dart = tempDart;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //    else if (isRatioChecked)
                    //    {
                    //        double temprdart = double.NaN;
                    //        double temprda = double.NaN;

                    //        if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sourceKey)
                    //                    && !double.IsNaN(DARTNode.sDALmpHash[sourceKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Loss))
                    //        {
                    //            if (double.IsNaN(ratio))
                    //            {
                    //                temprdart = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                    //                temprda = DARTNode.sDALmpHash[sourceKey].Loss;
                    //                if (UptosCheckBox.IsChecked == true)
                    //                {
                    //                    double sinkDart = 0;
                    //                    if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                    {
                    //                        sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Loss - temprda;
                    //                    }
                    //                }
                    //                if (temprda != 0)
                    //                {
                    //                    ratio = temprdart / temprda;
                    //                }
                    //                else
                    //                {
                    //                    ratio = temprdart;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                temprdart = (DARTNode.sRTLmpHash[sourceKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                    //                temprda = DARTNode.sDALmpHash[sourceKey].Loss;
                    //                double tempratio = 0;
                    //                if (temprda != 0)
                    //                {
                    //                    tempratio = temprdart / temprda;
                    //                }
                    //                else
                    //                {
                    //                    tempratio = temprdart;
                    //                }
                    //                if (isAggAvg)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        }
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Loss - temprda;
                    //                        if (temprda != 0)
                    //                        {
                    //                            tempratio = temprdart / temprda;
                    //                        }
                    //                        else
                    //                        {
                    //                            tempratio = temprdart;
                    //                        }
                    //                        ratio += tempratio;
                    //                    }
                    //                    else
                    //                    {
                    //                        ratio += tempratio;
                    //                    }
                    //                }
                    //                if (isAggMax)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        }
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Price - temprda;
                    //                        if (temprda != 0)
                    //                        {
                    //                            tempratio = temprdart / temprda;
                    //                        }
                    //                        else
                    //                        {
                    //                            tempratio = temprdart;
                    //                        }
                    //                        ratio += tempratio;
                    //                        if (ratio < tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (ratio < tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                }
                    //                if (isAggMin)
                    //                {
                    //                    if (UptosCheckBox.IsChecked == true)
                    //                    {
                    //                        double sinkDart = 0;
                    //                        if (DARTNode.sDALmpHash.ContainsKey(sinkKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey)
                    //                        && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss))
                    //                        {
                    //                            sinkDart = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sinkKey].Loss);
                    //                        }
                    //                        temprdart = sinkDart - temprdart;
                    //                        temprda = DARTNode.sDALmpHash[sinkKey].Loss - temprda;
                    //                        if (temprda != 0)
                    //                        {
                    //                            tempratio = temprdart / temprda;
                    //                        }
                    //                        else
                    //                        {
                    //                            tempratio = temprdart;
                    //                        }
                    //                        ratio += tempratio;
                    //                        if (ratio > tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (ratio > tempratio)
                    //                        {
                    //                            ratio = tempratio;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //}
                    //if (!double.IsNaN(loss))
                    //{
                    //    if (isAggAvg)
                    //        loss = loss / count;

                    //    DateTime indexDateTime = dateList.First().AddHours(i + 1);
                    //    //CalcDartStats(indexDateTime, da, daSum, out daSum, daTotal, out daTotal, daOffPeakSum, out daOffPeakSum, daPeakSum, out daPeakSum, daOffPeakTotal,
                    //    //                out daOffPeakTotal, daPeakTotal, out daPeakTotal, daMax, out daMax, daMin, out daMin);

                    //    CalcDartStats(indexDateTime, da, daSum, out daSum, daTotal, out daTotal, daOffPeakSum, out daOffPeakSum, daPeakSum, out daPeakSum, daOffPeakTotal,
                    //                    out daOffPeakTotal, daPeakTotal, out daPeakTotal, daMax, out daMax, daMin, out daMin,
                    //                    congestionSum, out congestionSum, lossSum, out lossSum, congestionTotal, out congestionTotal, lossTotal, out lossTotal,
                    //                    congestionPeakSum, out congestionPeakSum, lossPeakSum, out lossPeakSum, congestionOffPeakSum, out congestionOffPeakSum, lossOffPeakSum, out lossOffPeakSum,
                    //                    congestionPeakTotal, out congestionPeakTotal, lossPeakTotal, out lossPeakTotal, congestionOffPeakTotal, out congestionOffPeakTotal, lossOffPeakTotal, out lossOffPeakTotal,
                    //                    congestionMax, out congestionMax, lossMax, out lossMax, congestionMin, out congestionMin, lossMin, out lossMin);
                    //}

                    //#endregion

                    #endregion

                    #endregion


                    double number = double.NaN;
                    if (isDaChecked)
                    {
                        if (isPriceChecked)
                        {
                            number = daPrice;
                        }
                        else if (isCongestionChecked)
                        {
                            number = daCongestion;
                        }
                        else if (isLossChecked)
                        {
                            number = daLoss;
                        }
                    }
                    else if (isRtChecked)
                    {
                        if (isPriceChecked)
                        {
                            number = rtPrice;
                        }
                        else if (isCongestionChecked)
                        {
                            number = rtCongestion;
                        }
                        else if (isLossChecked)
                        {
                            number = rtLoss;
                        }
                    }
                    else if (isRatioChecked)
                    {
                        if (isPriceChecked)
                        {
                            number = ratioPrice;
                        }
                        else if (isCongestionChecked)
                        {
                            number = ratioCongestion;
                        }
                        else if (isLossChecked)
                        {
                            number = ratioLoss;
                        }
                    }
                    else
                    {
                        if (isPriceChecked)
                        {
                            number = dartPrice;
                        }
                        else if (isCongestionChecked)
                        {
                            number = dartCongestion;
                        }
                        else if (isLossChecked)
                        {
                            number = dartLoss;
                        }
                    }
                    if (double.IsNaN(number))
                    {
                        nodeData = SetNodeData(i, nodeData, null, daPrice, rtPrice, ratioPrice);
                        nodeDData.GetType().GetProperty("HE" + (i + 1)).SetValue(nodeDData, null, null);
                    }
                    else
                    {
                        if (isPriceChecked)
                        {
                            nodeData = SetNodeData(i, nodeData, Math.Round(number, 2), daPrice, rtPrice, ratioPrice);
                        }
                        else if (isCongestionChecked)
                        {
                            nodeData = SetNodeData(i, nodeData, Math.Round(number, 2), daCongestion, rtCongestion, ratioCongestion);
                        }
                        else if (isLossChecked)
                        {
                            nodeData = SetNodeData(i, nodeData, Math.Round(number, 2), daLoss, rtLoss, ratioLoss);
                        }

                        nodeDData.GetType().GetProperty("HE" + (i + 1)).SetValue(nodeDData, Math.Round(number, 2), null);
                    }
                }

                #region Assigning min max avg values

                #region Price

                if (isPriceChecked)
                {
                    if (daPriceMax != double.MinValue)
                    {
                        nodeData.DAMax = Math.Round(daPriceMax, 2);
                    }
                    else
                    {
                        nodeData.DAMax = null;
                    }

                    if (rtPriceMax != double.MinValue)
                    {
                        nodeData.RTMax = Math.Round(rtPriceMax, 2);
                    }
                    else
                    {
                        nodeData.RTMax = null;
                    }
                    if (dartPriceMax != double.MinValue)
                    {
                        nodeData.DARTMax = Math.Round(dartPriceMax, 2);
                    }
                    else
                    {
                        nodeData.DARTMax = null;
                    }
                    if (ratioPriceMax != double.MinValue)
                    {
                        nodeData.RATIOMax = Math.Round(ratioPriceMax, 2);
                    }
                    else
                    {
                        nodeData.RATIOMax = null;
                    }
                    if (daPriceMin != double.MinValue)
                    {
                        nodeData.DAMin = Math.Round(daPriceMin, 2);
                    }
                    if (rtPriceMin != double.MinValue)
                    {
                        nodeData.RTMin = Math.Round(rtPriceMin, 2);
                    }
                    if (dartPriceMin != double.MinValue)
                    {
                        nodeData.DARTMin = Math.Round(dartPriceMin, 2);
                    }
                    if (ratioPriceMin != double.MinValue)
                    {
                        nodeData.RATIOMin = Math.Round(ratioPriceMin, 2);
                    }
                    if (daPriceTotal > 0)
                    {
                        nodeData.DAAvg = Math.Round((daPriceSum / daPriceTotal), 2);
                    }
                    if (rtPriceTotal > 0)
                    {
                        nodeData.RTAvg = Math.Round((rtPriceSum / rtPriceTotal), 2);
                    }
                    if (dartPriceTotal > 0)
                    {
                        nodeData.DARTAvg = Math.Round((dartPriceSum / dartPriceTotal), 2);
                    }
                    if (ratioPriceTotal > 0)
                    {
                        nodeData.RATIOAvg = Math.Round((ratioPriceSum / ratioPriceTotal), 2);
                    }

                    if (daPricePeakTotal > 0)
                    {
                        nodeData.DAPeakAvg = Math.Round((daPricePeakSum / daPricePeakTotal), 2);
                    }
                    if (rtPricePeakTotal > 0)
                    {
                        nodeData.RTPeakAvg = Math.Round((rtPricePeakSum / rtPricePeakTotal), 2);
                    }
                    if (dartPricePeakTotal > 0)
                    {
                        nodeData.DARTPeakAvg = Math.Round((dartPricePeakSum / dartPricePeakTotal), 2);
                    }
                    if (ratioPricePeakTotal > 0)
                    {
                        nodeData.RATIOPeakAvg = Math.Round((ratioPricePeakSum / ratioPricePeakTotal), 2);
                    }
                    if (daPriceOffPeakTotal > 0)
                    {
                        nodeData.DAOffPeakAvg = Math.Round((daPriceOffPeakSum / daPriceOffPeakTotal), 2);
                    }
                    if (rtPriceOffPeakTotal > 0)
                    {
                        nodeData.RTOffPeakAvg = Math.Round((rtPriceOffPeakSum / rtPriceOffPeakTotal), 2);
                    }
                    if (dartPriceOffPeakTotal > 0)
                    {
                        nodeData.DARTOffPeakAvg = Math.Round((dartPriceOffPeakSum / dartPriceOffPeakTotal), 2);
                    }
                    if (ratioPriceOffPeakTotal > 0)
                    {
                        nodeData.RATIOOffPeakAvg = Math.Round((ratioPriceOffPeakSum / ratioPriceOffPeakTotal), 2);
                    }

                }

                #endregion


                #region Congestion

                if (isCongestionChecked)
                {
                    if (daCongestionMax != double.MinValue)
                    {
                        nodeData.DAMax = Math.Round(daCongestionMax, 2);
                    }
                    else
                    {
                        nodeData.DAMax = null;
                    }

                    if (rtCongestionMax != double.MinValue)
                    {
                        nodeData.RTMax = Math.Round(rtCongestionMax, 2);
                    }
                    else
                    {
                        nodeData.RTMax = null;
                    }
                    if (dartCongestionMax != double.MinValue)
                    {
                        nodeData.DARTMax = Math.Round(dartCongestionMax, 2);
                    }
                    else
                    {
                        nodeData.DARTMax = null;
                    }
                    if (ratioCongestionMax != double.MinValue)
                    {
                        nodeData.RATIOMax = Math.Round(ratioCongestionMax, 2);
                    }
                    else
                    {
                        nodeData.RATIOMax = null;
                    }
                    if (daCongestionMin != double.MinValue)
                    {
                        nodeData.DAMin = Math.Round(daCongestionMin, 2);
                    }
                    if (rtCongestionMin != double.MinValue)
                    {
                        nodeData.RTMin = Math.Round(rtCongestionMin, 2);
                    }
                    if (dartCongestionMin != double.MinValue)
                    {
                        nodeData.DARTMin = Math.Round(dartCongestionMin, 2);
                    }
                    if (ratioCongestionMin != double.MinValue)
                    {
                        nodeData.RATIOMin = Math.Round(ratioCongestionMin, 2);
                    }
                    if (daCongestionTotal > 0)
                    {
                        nodeData.DAAvg = Math.Round((daCongestionSum / daCongestionTotal), 2);
                    }
                    if (rtCongestionTotal > 0)
                    {
                        nodeData.RTAvg = Math.Round((rtCongestionSum / rtCongestionTotal), 2);
                    }
                    if (dartCongestionTotal > 0)
                    {
                        nodeData.DARTAvg = Math.Round((dartCongestionSum / dartCongestionTotal), 2);
                    }
                    if (ratioCongestionTotal > 0)
                    {
                        nodeData.RATIOAvg = Math.Round((ratioCongestionSum / ratioCongestionTotal), 2);
                    }

                    if (daCongestionPeakTotal > 0)
                    {
                        nodeData.DAPeakAvg = Math.Round((daCongestionPeakSum / daCongestionPeakTotal), 2);
                    }
                    if (rtCongestionPeakTotal > 0)
                    {
                        nodeData.RTPeakAvg = Math.Round((rtCongestionPeakSum / rtCongestionPeakTotal), 2);
                    }
                    if (dartCongestionPeakTotal > 0)
                    {
                        nodeData.DARTPeakAvg = Math.Round((dartCongestionPeakSum / dartCongestionPeakTotal), 2);
                    }
                    if (ratioCongestionPeakTotal > 0)
                    {
                        nodeData.RATIOPeakAvg = Math.Round((ratioCongestionPeakSum / ratioCongestionPeakTotal), 2);
                    }
                    if (daCongestionOffPeakTotal > 0)
                    {
                        nodeData.DAOffPeakAvg = Math.Round((daCongestionOffPeakSum / daCongestionOffPeakTotal), 2);
                    }
                    if (rtCongestionOffPeakTotal > 0)
                    {
                        nodeData.RTOffPeakAvg = Math.Round((rtCongestionOffPeakSum / rtCongestionOffPeakTotal), 2);
                    }
                    if (dartCongestionOffPeakTotal > 0)
                    {
                        nodeData.DARTOffPeakAvg = Math.Round((dartCongestionOffPeakSum / dartCongestionOffPeakTotal), 2);
                    }
                    if (ratioCongestionOffPeakTotal > 0)
                    {
                        nodeData.RATIOOffPeakAvg = Math.Round((ratioCongestionOffPeakSum / ratioCongestionOffPeakTotal), 2);
                    }

                }

                #endregion


                #region Loss

                if (isLossChecked)
                {
                    if (daLossMax != double.MinValue)
                    {
                        nodeData.DAMax = Math.Round(daLossMax, 2);
                    }
                    else
                    {
                        nodeData.DAMax = null;
                    }

                    if (rtLossMax != double.MinValue)
                    {
                        nodeData.RTMax = Math.Round(rtLossMax, 2);
                    }
                    else
                    {
                        nodeData.RTMax = null;
                    }
                    if (dartLossMax != double.MinValue)
                    {
                        nodeData.DARTMax = Math.Round(dartLossMax, 2);
                    }
                    else
                    {
                        nodeData.DARTMax = null;
                    }
                    if (ratioLossMax != double.MinValue)
                    {
                        nodeData.RATIOMax = Math.Round(ratioLossMax, 2);
                    }
                    else
                    {
                        nodeData.RATIOMax = null;
                    }
                    if (daLossMin != double.MinValue)
                    {
                        nodeData.DAMin = Math.Round(daLossMin, 2);
                    }
                    if (rtLossMin != double.MinValue)
                    {
                        nodeData.RTMin = Math.Round(rtLossMin, 2);
                    }
                    if (dartLossMin != double.MinValue)
                    {
                        nodeData.DARTMin = Math.Round(dartLossMin, 2);
                    }
                    if (ratioLossMin != double.MinValue)
                    {
                        nodeData.RATIOMin = Math.Round(ratioLossMin, 2);
                    }
                    if (daLossTotal > 0)
                    {
                        nodeData.DAAvg = Math.Round((daLossSum / daLossTotal), 2);
                    }
                    if (rtLossTotal > 0)
                    {
                        nodeData.RTAvg = Math.Round((rtLossSum / rtLossTotal), 2);
                    }
                    if (dartLossTotal > 0)
                    {
                        nodeData.DARTAvg = Math.Round((dartLossSum / dartLossTotal), 2);
                    }
                    if (ratioLossTotal > 0)
                    {
                        nodeData.RATIOAvg = Math.Round((ratioLossSum / ratioLossTotal), 2);
                    }

                    if (daLossPeakTotal > 0)
                    {
                        nodeData.DAPeakAvg = Math.Round((daLossPeakSum / daLossPeakTotal), 2);
                    }
                    if (rtLossPeakTotal > 0)
                    {
                        nodeData.RTPeakAvg = Math.Round((rtLossPeakSum / rtLossPeakTotal), 2);
                    }
                    if (dartLossPeakTotal > 0)
                    {
                        nodeData.DARTPeakAvg = Math.Round((dartLossPeakSum / dartLossPeakTotal), 2);
                    }
                    if (ratioLossPeakTotal > 0)
                    {
                        nodeData.RATIOPeakAvg = Math.Round((ratioLossPeakSum / ratioLossPeakTotal), 2);
                    }
                    if (daLossOffPeakTotal > 0)
                    {
                        nodeData.DAOffPeakAvg = Math.Round((daLossOffPeakSum / daLossOffPeakTotal), 2);
                    }
                    if (rtLossOffPeakTotal > 0)
                    {
                        nodeData.RTOffPeakAvg = Math.Round((rtLossOffPeakSum / rtLossOffPeakTotal), 2);
                    }
                    if (dartLossOffPeakTotal > 0)
                    {
                        nodeData.DARTOffPeakAvg = Math.Round((dartLossOffPeakSum / dartLossOffPeakTotal), 2);
                    }
                    if (ratioLossOffPeakTotal > 0)
                    {
                        nodeData.RATIOOffPeakAvg = Math.Round((ratioLossOffPeakSum / ratioLossOffPeakTotal), 2);
                    }

                }

                #endregion

                #endregion


                if (isDaChecked)
                {
                    nodeData.Max = nodeData.DAMax;
                    nodeData.Min = nodeData.DAMin;
                    nodeData.Avg = nodeData.DAAvg;
                    nodeData.PeakAvg = nodeData.DAPeakAvg;
                    nodeData.OffPeakAvg = nodeData.DAOffPeakAvg;
                    nodeData.Avg1_11 = nodeData.Avg1_11;
                    nodeData.Avg12_24 = nodeData.Avg12_24;
                }
                else if (isRtChecked)
                {
                    nodeData.Max = nodeData.RTMax;
                    nodeData.Min = nodeData.RTMin;
                    nodeData.Avg = nodeData.RTAvg;
                    nodeData.PeakAvg = nodeData.RTPeakAvg;
                    nodeData.OffPeakAvg = nodeData.RTOffPeakAvg;
                    nodeData.Avg1_11 = nodeData.Avg1_11;
                    nodeData.Avg12_24 = nodeData.Avg12_24;
                }
                else if (isDartChecked)
                {
                    nodeData.Max = nodeData.DARTMax;
                    nodeData.Min = nodeData.DARTMin;
                    nodeData.Avg = nodeData.DARTAvg;
                    nodeData.PeakAvg = nodeData.DARTPeakAvg;
                    nodeData.OffPeakAvg = nodeData.DARTOffPeakAvg;
                    nodeData.Avg1_11 = nodeData.Avg1_11;
                    nodeData.Avg12_24 = nodeData.Avg12_24;
                }
                else
                {
                    nodeData.Max = nodeData.RATIOMax;
                    nodeData.Min = nodeData.RATIOMin;
                    nodeData.Avg = nodeData.RATIOAvg;
                    nodeData.PeakAvg = nodeData.RATIOPeakAvg;
                    nodeData.OffPeakAvg = nodeData.RATIOOffPeakAvg;
                    nodeData.Avg1_11 = nodeData.Avg1_11;
                    nodeData.Avg12_24 = nodeData.Avg12_24;
                }


                nodeDData.Max = nodeData.Max;
                nodeDData.Min = nodeData.Min;
                nodeDData.Avg = nodeData.Avg;
                nodeDData.PeakAvg = nodeData.PeakAvg;
                nodeDData.OffPeakAvg = nodeData.OffPeakAvg;
                nodeData.Avg1_11 = nodeData.Avg1_11;
                nodeData.Avg12_24 = nodeData.Avg12_24;

                List<string> nodeTypeList = mNodeTypeHash[marketKey];

                if (IsValidNode(isNullChecked, nodeData, nodeType, path, zoneList, sinkZoneList, nodeName, isDaChecked, isRtChecked, isDartChecked, isRatioChecked,
                    compareMin, compareMax, isMinChecked, isMaxChecked, isAvgChecked, isPeakAvgChecked, isOffPeakAvgChecked, isHourChecked, hour, type))
                {
                    mLmpList.Add(nodeDData);
                    nodeList.Add(nodeData);
                }
                else
                {

                }
            }
            List<int> avg111List = Get111ist();
            List<int> avg1224List = Ge1224List();
            List<int> offPeakList = GetOffPeakList();
            List<int> onPeakList = GetTotalHoursList().Except(offPeakList).ToList();
            #region ParallelForEach Optional
            //Parallel.ForEach(nodeList, new ParallelOptions { MaxDegreeOfParallelism = 2 }, item =>
            //{
            //    if (isDaChecked)
            //    {
            //        item.OffPeakAvg = CalculateOffPeakAvg(item, "DA", offPeakList);
            //        item.PeakAvg = CalculateOffPeakAvg(item, "DA", onPeakList);
            //        item.Avg1_11 = CalculateOffPeakAvg(item, "DA", avg111List);
            //        item.Avg12_24 = CalculateOffPeakAvg(item, "DA", avg1224List);
            //    }
            //    else if (isRtChecked)
            //    {
            //        item.OffPeakAvg = CalculateOffPeakAvg(item, "RT", offPeakList);
            //        item.PeakAvg = CalculateOffPeakAvg(item, "RT", onPeakList);
            //        item.Avg1_11 = CalculateOffPeakAvg(item, "RT", avg111List);
            //        item.Avg12_24 = CalculateOffPeakAvg(item, "RT", avg1224List);
            //    }
            //    else if (isDartChecked)
            //    {
            //        item.OffPeakAvg = CalculateOffPeakAvg(item, "HE", offPeakList);
            //        item.PeakAvg = CalculateOffPeakAvg(item, "HE", onPeakList);
            //        item.Avg1_11 = CalculateOffPeakAvg(item, "HE", avg111List);
            //        item.Avg12_24 = CalculateOffPeakAvg(item, "HE", avg1224List);
            //    }
            //    else if (isRatioChecked)
            //    {
            //        item.OffPeakAvg = CalculateOffPeakAvg(item, "RATIO", offPeakList);
            //        item.PeakAvg = CalculateOffPeakAvg(item, "RATIO", onPeakList);
            //        item.Avg1_11 = CalculateOffPeakAvg(item, "RATIO", avg111List);
            //        item.Avg12_24 = CalculateOffPeakAvg(item, "RATIO", avg1224List);
            //    }
            //});
            #endregion  ParallelForEach Optional
            foreach (NodeData itNew in nodeList.OrderBy(k => k.NodeName))
            {
                if (isDaChecked)
                {
                    itNew.OffPeakAvg = CalculateOffPeakAvg(itNew, "DA", offPeakList);
                    itNew.PeakAvg = CalculateOffPeakAvg(itNew, "DA", onPeakList);
                    itNew.Avg1_11 = CalculateOffPeakAvg(itNew, "DA", avg111List);
                    itNew.Avg12_24 = CalculateOffPeakAvg(itNew, "DA", avg1224List);
                }
                else if (isRtChecked)
                {
                    itNew.OffPeakAvg = CalculateOffPeakAvg(itNew, "RT", offPeakList);
                    itNew.PeakAvg = CalculateOffPeakAvg(itNew, "RT", onPeakList);
                    itNew.Avg1_11 = CalculateOffPeakAvg(itNew, "RT", avg111List);
                    itNew.Avg12_24 = CalculateOffPeakAvg(itNew, "RT", avg1224List);
                }
                else if (isDartChecked)
                {
                    itNew.OffPeakAvg = CalculateOffPeakAvg(itNew, "HE", offPeakList);
                    itNew.PeakAvg = CalculateOffPeakAvg(itNew, "HE", onPeakList);
                    itNew.Avg1_11 = CalculateOffPeakAvg(itNew, "HE", avg111List);
                    itNew.Avg12_24 = CalculateOffPeakAvg(itNew, "HE", avg1224List);
                }
                else if (isRatioChecked)
                {
                    itNew.OffPeakAvg = CalculateOffPeakAvg(itNew, "RATIO", offPeakList);
                    itNew.PeakAvg = CalculateOffPeakAvg(itNew, "RATIO", onPeakList);
                    itNew.Avg1_11 = CalculateOffPeakAvg(itNew, "RATIO", avg111List);
                    itNew.Avg12_24 = CalculateOffPeakAvg(itNew, "RATIO", avg1224List);
                }

                nodeDataGrid.Items.Add(itNew);
                tempnodeList.Add(itNew);
            }
            totalRecordsLabel.Content = nodeList.Count.ToString();
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Ge1224s the list.
        /// </summary>
        /// <returns></returns>
        private List<int> Ge1224List()
        {
            return new List<int> { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
        }

        /// <summary>
        /// Get111ists this instance.
        /// </summary>
        /// <returns></returns>
        private List<int> Get111ist()
        {
            return new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        }
        public void GetSeasionData()
        {
            SeasonData data = new SeasonData();
            try
            {
                if (VayuConnetion.State == ConnectionState.Closed)
                    VayuConnetion.Open();
                SqlDataReader reader = mSelectSeasionData.ExecuteReader();
                while (reader.Read())
                {
                    data = new SeasonData();
                    data.SourceName = Convert.ToString(reader.GetValue(0));
                    data.SinkName = Convert.ToString(reader.GetValue(1));
                    data.SourceNodekey = Convert.ToInt64(reader.GetValue(2));
                    data.SinkNodekey = Convert.ToInt64(reader.GetValue(3));
                    data.RTMinFall = Convert.ToInt64(reader.GetValue(4));
                    data.RTMaxFall = Convert.ToInt64(reader.GetValue(5));
                    data.RTMinSpring = Convert.ToInt64(reader.GetValue(6));
                    data.RTMaxSpring = Convert.ToInt64(reader.GetValue(7));
                    data.RTMinSummer = Convert.ToInt64(reader.GetValue(8));
                    data.RTMaxSummer = Convert.ToInt64(reader.GetValue(9));

                    data.RTMinWinter = Convert.ToInt64(reader.GetValue(10));
                    data.RTMaxWinter = Convert.ToInt64(reader.GetValue(11));
                    data.PathMinRT = Convert.ToDateTime(reader.GetValue(12));
                    listseasionData.Add(data);

                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// Gets the total hours list.
        /// </summary>
        /// <returns></returns>
        private List<int> GetTotalHoursList()
        {
            List<int> hrList = new List<int>();
            for (int i = 1; i <= 24; i++)
            {
                hrList.Add(i);
            }
            return hrList;
        }

        /// <summary>
        /// Calculates the off peak average.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="expr">The expr.</param>
        /// <param name="offPeakList">The off peak list.</param>
        /// <returns></returns>
        private double? CalculateOffPeakAvg(NodeData a, string expr, List<int> offPeakList)
        {
            try
            {
                IEnumerable<string> properties = from it in a.GetType().GetProperties() select it.Name;
                double daoffVal = 0, countVal = 0;
                {
                    foreach (string propItem in properties.Where(q => q.StartsWith(expr.ToUpper())))
                    {
                        try
                        {
                            int hrVal;
                            if (int.TryParse(System.Text.RegularExpressions.Regex.Match(propItem, @"\d+").Value, out hrVal))
                            {
                                if (offPeakList.Contains(hrVal))
                                {
                                    var val = a.GetType().GetProperty(propItem).GetValue(a);
                                    double tempVal;
                                    if (val != null)
                                    {
                                        if (double.TryParse(val.ToString(), out tempVal) && !double.IsNaN(tempVal))
                                        {
                                            daoffVal += tempVal;
                                            countVal++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    if (countVal == 0)
                    {
                        return double.NaN;
                    }
                    else
                    {
                        return Math.Round((daoffVal / countVal), 2);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the off peak list.
        /// </summary>
        /// <returns></returns>
        private List<int> GetOffPeakList()
        {
            if (sMarketSelected == 1)
            {
                return new List<int> { 1, 2, 3, 4, 5, 6, 7, 24 };
            }
            else if (sMarketSelected == 2)
            {
                if (FromNodeDatePicker.SelectedDate.Value.IsDaylightSavingTime())
                {
                    return new List<int> { 1, 2, 3, 4, 5, 6, 23, 24 };
                }
                else
                {
                    return new List<int> { 1, 2, 3, 4, 5, 6, 7, 24 };
                }
            }
            else
            {
                return new List<int> { 1, 2, 3, 4, 5, 6, 23, 24 };
            }
        }
        /// <summary>
        /// Sets the node table.
        /// </summary>
        /// <param name="selectedHour">The selected hour.</param>
        private void SetNodeTable(int selectedHour)
        {
            if (mIsFirstTime)
            {
                return;
            }
            if (ToNodeDatePicker.Text == "")
            {
                return;
            }
            if (!(bool)lmpHourCheckBox.IsChecked)
            {
                return;
            }
            Mouse.OverrideCursor = Cursors.Wait;
            DateTime start = DateTime.Parse(FromNodeDatePicker.Text);
            DateTime end = DateTime.Parse(ToNodeDatePicker.Text);
            mHourLmpList.Clear();
            nodeHourLmpDataGrid.Items.Clear();
            List<string> nameList = new List<string>();
            List<string> nodeNameList = mNodeHash.Keys.ToList<string>();
            string nodeType = (string)NodeTypeComboBox.SelectedValue;
            nodeType = nodeType == null ? "ALL" : nodeType;

            bool isErcotChecked = (bool)ercotRadioButton.IsChecked;
            bool isNullChecked = (bool)NullCheckBox.IsChecked;
            string path = PathComboBox.Text;
            bool isMaxChecked = (bool)MaxRadioButton.IsChecked;
            bool isMinChecked = (bool)MinRadioButton.IsChecked;
            bool isAvgChecked = (bool)AvgRadioButton.IsChecked;
            bool isPeakAvgChecked = (bool)PeakAvgRadioButton.IsChecked;
            bool isOffPeakAvgChecked = (bool)OffPeakRadioButton.IsChecked;
            bool isDaChecked = (bool)daRadioButton.IsChecked;
            bool isRtChecked = (bool)rtRadioButton.IsChecked;
            bool isDartChecked = (bool)dartRadioButton.IsChecked;
            bool isRatioChecked = (bool)ratioRadioButton.IsChecked;
            bool isAggAvg = (bool)AggAvgRadioButton.IsChecked;
            bool isAggMax = (bool)AggMaxRadioButton.IsChecked;
            bool isAggMin = (bool)AggMinRadioButton.IsChecked;
            bool isHourChecked = (bool)HourRadioButton.IsChecked;
            string type = dartComboBox.Text;
            int hour = Int32.Parse(hourComboBox.Text);
            List<DateTime> dateList = new List<DateTime>();
            if (CollectionRadioButton.IsChecked == true)
            {
                foreach (string dateStr in DateCollectionListBox.Items)
                {
                    dateList.Add(DateTime.Parse(dateStr));
                }
            }
            else
            {
                while (start <= end)
                {
                    dateList.Add(start);
                    start = start.AddDays(1);
                }
            }

            nameList = UptosCheckBox.IsChecked == true ? mErcotUptosPathList : nodeNameList;
            foreach (string nodeName in nameList)
            {
                string name = nodeName;
                string sink = null;
                if (UptosCheckBox.IsChecked == true)
                {
                    string[] tokens = nodeName.Split('?');
                    name = tokens[0];
                    sink = tokens[1];
                    nodeHourLmpDataGrid.Columns[3].Visibility = System.Windows.Visibility.Visible;
                    nodeHourLmpDataGrid.Columns[4].Visibility = System.Windows.Visibility.Visible;
                    nodeHourLmpDataGrid.Columns[5].Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    nodeHourLmpDataGrid.Columns[3].Visibility = System.Windows.Visibility.Hidden;
                    nodeHourLmpDataGrid.Columns[4].Visibility = System.Windows.Visibility.Hidden;
                    nodeHourLmpDataGrid.Columns[5].Visibility = System.Windows.Visibility.Hidden;
                }
                if (!mNodeHash.ContainsKey(name))
                {
                    continue;
                }
                NodeDetail nodeDetail = mNodeHash[name];
                NodeDetail sinkDetail = null;
                if (sink != null && mNodeHash.ContainsKey(sink))
                {
                    sinkDetail = mNodeHash[sink];
                }
                NodeHourLMP nodeData = new NodeHourLMP();
                nodeData.NodeName = name;
                nodeData.ZoneName = nodeDetail.Zone;
                nodeData.NodeType = nodeDetail.Type;
                if (sinkDetail != null)
                {
                    nodeData.SinkName = sink;
                    nodeData.SinkZone = sinkDetail.Zone;
                    nodeData.SinkType = sinkDetail.Type;
                }
                var zones = ZoneListBox.SelectedItems;
                string[] zoneList = new string[zones.Count];
                zones.CopyTo(zoneList, 0);
                var sinkZones = SinkZoneListBox.SelectedItems;
                string[] sinkZoneList = new string[sinkZones.Count];
                sinkZones.CopyTo(sinkZoneList, 0);
                int i = selectedHour - 1;
                double da = double.NaN;
                double rt = double.NaN;
                double dart = double.NaN;
                double ratio = double.NaN;

                double congestion = double.NaN;
                double loss = double.NaN;

                double count = 0;
                foreach (DateTime date in dateList)
                {
                    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    {
                        continue;
                    }
                    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    if (DARTNode.sDAHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                    {
                        if (double.IsNaN(da))
                        {
                            da = DARTNode.sDAHash[sourceKey];
                            if (sinkKey != null)
                            {
                                double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                                da = sinkDa - DARTNode.sDAHash[sourceKey];
                            }
                        }
                        else
                        {
                            if (isAggAvg)
                            {
                                if (sinkKey != null)
                                {
                                    double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                                    da += (sinkDa - DARTNode.sDAHash[sourceKey]);
                                }
                                else
                                {
                                    da += DARTNode.sDAHash[sourceKey];
                                }
                            }
                            if (isAggMax)
                            {
                                if (sinkKey != null)
                                {
                                    double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                                    if (da < sinkDa - DARTNode.sDAHash[sourceKey])
                                    {
                                        da = sinkDa - DARTNode.sDAHash[sourceKey];
                                    }
                                }
                                else
                                {
                                    if (da < DARTNode.sDAHash[sourceKey])
                                    {
                                        da = DARTNode.sDAHash[sourceKey];
                                    }
                                }
                            }
                            if (isAggMin)
                            {
                                if (sinkKey != null)
                                {
                                    double sinkDa = (DARTNode.sDAHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDAHash[sinkKey])) ? DARTNode.sDAHash[sinkKey] : 0;
                                    if (da > sinkDa - DARTNode.sDAHash[sourceKey])
                                    {
                                        da = sinkDa - DARTNode.sDAHash[sourceKey];
                                    }
                                }
                                else
                                {
                                    if (da > DARTNode.sDAHash[sourceKey])
                                    {
                                        da = DARTNode.sDAHash[sourceKey];
                                    }
                                }
                            }
                        }
                        count++;
                    }
                }
                if (!double.IsNaN(da))
                {
                    if (isAggAvg)
                    {
                        da = da / count;
                    }
                }
                count = 0;
                foreach (DateTime date in dateList)
                {
                    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    {
                        continue;
                    }
                    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    if (DARTNode.sRTHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                    {
                        if (double.IsNaN(rt))
                        {
                            rt = DARTNode.sRTHash[sourceKey];
                            if (sinkKey != null)
                            {
                                if (sinkKey != null)
                                {
                                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                                    rt = sinkRt - DARTNode.sRTHash[sourceKey];
                                }
                            }
                        }
                        else
                        {
                            if (isAggAvg)
                            {
                                if (sinkKey != null)
                                {
                                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                                    rt += (sinkRt - DARTNode.sRTHash[sourceKey]);
                                }
                                else
                                {
                                    rt += DARTNode.sRTHash[sourceKey];
                                }
                            }
                            if (isAggMax)
                            {
                                if (sinkKey != null)
                                {
                                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                                    if (rt < sinkRt - DARTNode.sRTHash[sourceKey])
                                    {
                                        rt = sinkRt - DARTNode.sRTHash[sourceKey];
                                    }
                                }
                                else
                                {
                                    if (rt < DARTNode.sRTHash[sourceKey])
                                    {
                                        rt = DARTNode.sRTHash[sourceKey];
                                    }
                                }
                            }
                            if (isAggMin)
                            {
                                if (sinkKey != null)
                                {
                                    double sinkRt = (DARTNode.sRTHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTHash[sinkKey])) ? DARTNode.sRTHash[sinkKey] : 0;
                                    if (rt > sinkRt - DARTNode.sRTHash[sourceKey])
                                    {
                                        rt = sinkRt - DARTNode.sRTHash[sourceKey];
                                    }
                                }
                                else
                                {
                                    if (rt > DARTNode.sRTHash[sourceKey])
                                    {
                                        rt = DARTNode.sRTHash[sourceKey];
                                    }
                                }
                            }
                        }
                        count++;
                    }
                }
                if (!double.IsNaN(rt))
                {
                    if (isAggAvg)
                    {
                        rt = rt / count;
                    }
                }
                count = 0;
                foreach (DateTime date in dateList)
                {
                    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    {
                        continue;
                    }
                    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sourceKey) &&
                        !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                    {
                        if (double.IsNaN(dart))
                        {
                            dart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                            if (UptosCheckBox.IsChecked == true)
                            {
                                double sinkDart = 0;
                                if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                    !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                {
                                    sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    dart = sinkDart - dart;
                                }
                            }
                        }
                        else
                        {
                            double tempDart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                            if (isAggAvg)
                            {
                                if (UptosCheckBox.IsChecked == true)
                                {
                                    double sinkDart = 0;
                                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey)
                                        && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                    {
                                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    }
                                    dart += (sinkDart - tempDart);
                                }
                                else
                                {
                                    dart += tempDart;
                                }
                            }
                            if (isAggMax)
                            {
                                if (UptosCheckBox.IsChecked == true)
                                {
                                    double sinkDart = 0;
                                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                        !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                    {
                                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    }
                                    if (dart < sinkDart - tempDart)
                                    {
                                        dart = sinkDart - tempDart;
                                    }
                                }
                                else
                                {
                                    if (dart < tempDart)
                                    {
                                        dart = tempDart;
                                    }
                                }
                            }
                            if (isAggMin)
                            {
                                if (UptosCheckBox.IsChecked == true)
                                {
                                    double sinkDart = 0;
                                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                        !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                    {
                                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    }
                                    if (dart > sinkDart - tempDart)
                                    {
                                        dart = sinkDart - tempDart;
                                    }
                                }
                                else
                                {
                                    if (dart > tempDart)
                                    {
                                        dart = tempDart;
                                    }
                                }
                            }
                        }
                        count++;
                    }
                }
                if (!double.IsNaN(dart))
                {
                    if (isAggAvg)
                    {
                        dart = dart / count;
                    }
                }
                count = 0;
                foreach (DateTime date in dateList)
                {
                    double temprdart = double.NaN;
                    double temprda = double.NaN;
                    if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                    {
                        continue;
                    }
                    string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                    string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sourceKey) &&
                        !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                    {
                        if (double.IsNaN(ratio))
                        {
                            temprdart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                            temprda = DARTNode.sDAHash[sourceKey];
                            if (UptosCheckBox.IsChecked == true)
                            {
                                double sinkDart = 0;
                                if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                    !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                {
                                    sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    temprdart = sinkDart - temprdart;
                                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                                }
                            }
                            if (temprda != 0)
                            {
                                ratio = temprdart / temprda;
                            }
                            else
                            {
                                ratio = temprdart;
                            }
                        }
                        else
                        {
                            temprdart = (DARTNode.sRTHash[sourceKey] - DARTNode.sDAHash[sourceKey]);
                            temprda = DARTNode.sDAHash[sourceKey];
                            double tempratio = 0;
                            if (temprda != 0)
                            {
                                tempratio = temprdart / temprda;
                            }
                            else
                            {
                                tempratio = temprdart;
                            }
                            if (isAggAvg)
                            {
                                if (UptosCheckBox.IsChecked == true)
                                {
                                    double sinkDart = 0;
                                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                        !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                    {
                                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    }
                                    temprdart = sinkDart - temprdart;
                                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                                    if (temprda != 0)
                                        tempratio = temprdart / temprda;
                                    else
                                        tempratio = temprdart;
                                    ratio += tempratio;
                                }
                                else
                                {
                                    ratio += tempratio;
                                }
                            }
                            if (isAggMax)
                            {
                                if (UptosCheckBox.IsChecked == true)
                                {
                                    double sinkDart = 0;
                                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                        !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                    {
                                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    }
                                    temprdart = sinkDart - temprdart;
                                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                                    if (temprda != 0)
                                    {
                                        tempratio = temprdart / temprda;
                                    }
                                    else
                                    {
                                        tempratio = temprdart;
                                    }
                                    ratio += tempratio;
                                    if (ratio < tempratio)
                                    {
                                        ratio = tempratio;
                                    }
                                }
                                else
                                {
                                    if (ratio < tempratio)
                                    {
                                        ratio = tempratio;
                                    }
                                }
                            }
                            if (isAggMin)
                            {
                                if (UptosCheckBox.IsChecked == true)
                                {
                                    double sinkDart = 0;
                                    if (DARTNode.sDAHash.ContainsKey(sinkKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                        !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                    {
                                        sinkDart = (DARTNode.sRTHash[sinkKey] - DARTNode.sDAHash[sinkKey]);
                                    }
                                    temprdart = sinkDart - temprdart;
                                    temprda = DARTNode.sDAHash[sinkKey] - temprda;
                                    if (temprda != 0)
                                    {
                                        tempratio = temprdart / temprda;
                                    }
                                    else
                                    {
                                        tempratio = temprdart;
                                    }
                                    ratio += tempratio;
                                    if (ratio > tempratio)
                                    {
                                        ratio = tempratio;
                                    }
                                }
                                else
                                {
                                    if (ratio > tempratio)
                                    {
                                        ratio = tempratio;
                                    }
                                }
                            }
                        }
                        count++;
                    }
                }
                if (!double.IsNaN(ratio))
                {
                    if (isAggAvg)
                    {
                        ratio = ratio / count;
                    }
                }
                if ((double.IsNaN(da) && double.IsNaN(rt)))
                {
                    continue;
                }
                double? nullValue = null;
                nodeData.Hour = selectedHour;
                nodeData.DA = double.IsNaN(da) ? nullValue : Math.Round(da, 2);
                nodeData.RT = double.IsNaN(rt) ? nullValue : Math.Round(rt, 2);
                nodeData.DART = double.IsNaN(dart) ? nullValue : Math.Round(dart, 2);
                nodeData.RATIO = double.IsNaN(ratio) ? nullValue : Math.Round(ratio, 2);

                nodeData.Congestion = double.IsNaN(congestion) ? nullValue : Math.Round(congestion, 2);
                nodeData.Loss = double.IsNaN(loss) ? nullValue : Math.Round(loss, 2);

                mHourLmpList.Add(nodeData);
                nodeHourLmpDataGrid.Items.Add(nodeData);
            }
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Determines whether [is valid node] [the specified is null checked].
        /// </summary>
        /// <param name="isNullChecked">The is null checked.</param>
        /// <param name="nodeData">The node data.</param>
        /// <param name="nodeType">Type of the node.</param>
        /// <param name="path">The path.</param>
        /// <param name="zoneList">The zone list.</param>
        /// <param name="sinkZoneList">The sink zone list.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="isDaChecked">if set to <c>true</c> [is da checked].</param>
        /// <param name="isRtChecked">if set to <c>true</c> [is rt checked].</param>
        /// <param name="isDartChecked">if set to <c>true</c> [is dart checked].</param>
        /// <param name="isRatioChecked">if set to <c>true</c> [is ratio checked].</param>
        /// <param name="compareMin">The compare minimum.</param>
        /// <param name="compareMax">The compare maximum.</param>
        /// <param name="isMinChecked">The is minimum checked.</param>
        /// <param name="isMaxChecked">The is maximum checked.</param>
        /// <param name="isAvgChecked">The is average checked.</param>
        /// <param name="isPeakAvgChecked">The is peak average checked.</param>
        /// <param name="isOffPeakAvgChecked">The is off peak average checked.</param>
        /// <param name="isHourChecked">The is hour checked.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is valid node] [the specified is null checked]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidNode(bool? isNullChecked, NodeData nodeData, string nodeType, string path, string[] zoneList, string[] sinkZoneList,
                                string nodeName, bool isDaChecked, bool isRtChecked, bool isDartChecked, bool isRatioChecked, double compareMin, double
                                compareMax, bool? isMinChecked, bool? isMaxChecked, bool? isAvgChecked, bool? isPeakAvgChecked,
                                bool? isOffPeakAvgChecked, bool? isHourChecked, int hour, string type)
        {
            double? max = double.MinValue;
            double? min = double.MaxValue;
            if (isDaChecked)
            {
                max = nodeData.DAMax;
                min = nodeData.DAMin;
            }
            if (isRtChecked)
            {
                max = nodeData.RTMax;
                min = nodeData.RTMin;
            }
            if (isDartChecked)
            {
                max = nodeData.DARTMax;
                min = nodeData.DARTMin;
            }
            if (isRatioChecked)
            {
                max = nodeData.RATIOMax;
                min = nodeData.RATIOMin;
            }
            if ((max == double.MinValue || min == double.MaxValue) && isNullChecked == false)
            {
                return false;
            }

            if (ercotRadioButton.IsChecked == true)
            {
                if (sMarketSelected == 9 && path == "UPTOs" && !mErcotSourceSinkList.Contains(nodeData.NodeName))
                {
                    return false;
                }
            }
            if (sMarketSelected == 1 && path == "FTR" && !mPJMFtrSourceSinkList.Contains(mNodeHash[nodeData.NodeName].ID))
            {
                return false;
            }
            if (nodeType != "ALL" && nodeData.NodeType != nodeType)
            {
                return false;
            }
            string zone = nodeData.ZoneName == null || nodeData.ZoneName == "" ? "NONE" : nodeData.ZoneName;
            if (!zoneList.Contains(zone))
            {
                return false;
            }
            if (ercotRadioButton.IsChecked == true)
            {
                string sinkZone = string.Empty;
                if (UptosCheckBox.IsChecked == true)
                {
                    sinkZone = nodeData.SinkZone == string.Empty ? "NONE" : nodeData.SinkZone;//change
                }
                else
                {
                    sinkZone = nodeData.SinkZone == null ? "NONE" : nodeData.SinkZone;//change
                }
                if (!sinkZoneList.Contains(sinkZone))
                {
                    return false;
                }
            }

            if (isHourChecked == true)
            {
                if (hour == 1 && (((nodeData.DA1 < compareMin || nodeData.DA1 > compareMax) && type == "DA") ||
                                    ((nodeData.RT1 < compareMin || nodeData.RT1 > compareMax) && type == "RT") ||
                                    (((nodeData.RT1 - nodeData.DA1) < compareMin || (nodeData.RT1 - nodeData.DA1) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO1 < compareMin || nodeData.RATIO1 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 2 && (((nodeData.DA2 < compareMin || nodeData.DA2 > compareMax) && type == "DA") ||
                                    ((nodeData.RT2 < compareMin || nodeData.RT2 > compareMax) && type == "RT") ||
                                    (((nodeData.RT2 - nodeData.DA2) < compareMin || (nodeData.RT2 - nodeData.DA2) > compareMax) && type == "DART")) ||
                                    ((nodeData.RATIO2 < compareMin || nodeData.RATIO2 > compareMax) && type == "RATIO"))
                {
                    return false;
                }
                if (hour == 3 && (((nodeData.DA3 < compareMin || nodeData.DA3 > compareMax) && type == "DA") ||
                                    ((nodeData.RT3 < compareMin || nodeData.RT3 > compareMax) && type == "RT") ||
                                    (((nodeData.RT3 - nodeData.DA3) < compareMin || (nodeData.RT3 - nodeData.DA3) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO3 < compareMin || nodeData.RATIO3 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 4 && (((nodeData.DA4 < compareMin || nodeData.DA4 > compareMax) && type == "DA") ||
                                    ((nodeData.RT4 < compareMin || nodeData.RT4 > compareMax) && type == "RT") ||
                                    (((nodeData.RT4 - nodeData.DA4) < compareMin || (nodeData.RT4 - nodeData.DA4) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO4 < compareMin || nodeData.RATIO4 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 5 && (((nodeData.DA5 < compareMin || nodeData.DA5 > compareMax) && type == "DA") ||
                                    ((nodeData.RT5 < compareMin || nodeData.RT5 > compareMax) && type == "RT") ||
                                    (((nodeData.RT5 - nodeData.DA5) < compareMin || (nodeData.RT5 - nodeData.DA5) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO5 < compareMin || nodeData.RATIO5 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 6 && (((nodeData.DA6 < compareMin || nodeData.DA6 > compareMax) && type == "DA") ||
                                    ((nodeData.RT6 < compareMin || nodeData.RT6 > compareMax) && type == "RT") ||
                                    (((nodeData.RT6 - nodeData.DA6) < compareMin || (nodeData.RT6 - nodeData.DA6) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO6 < compareMin || nodeData.RATIO6 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 7 && (((nodeData.DA7 < compareMin || nodeData.DA7 > compareMax) && type == "DA") ||
                                    ((nodeData.RT7 < compareMin || nodeData.RT7 > compareMax) && type == "RT") ||
                                    (((nodeData.RT7 - nodeData.DA7) < compareMin || (nodeData.RT7 - nodeData.DA7) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO7 < compareMin || nodeData.RATIO7 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 8 && (((nodeData.DA8 < compareMin || nodeData.DA8 > compareMax) && type == "DA") ||
                                    ((nodeData.RT8 < compareMin || nodeData.RT8 > compareMax) && type == "RT") ||
                                    (((nodeData.RT8 - nodeData.DA8) < compareMin || (nodeData.RT8 - nodeData.DA8) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO8 < compareMin || nodeData.RATIO8 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 9 && (((nodeData.DA9 < compareMin || nodeData.DA9 > compareMax) && type == "DA") ||
                                    ((nodeData.RT9 < compareMin || nodeData.RT9 > compareMax) && type == "RT") ||
                                    (((nodeData.RT9 - nodeData.DA9) < compareMin || (nodeData.RT9 - nodeData.DA9) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO9 < compareMin || nodeData.RATIO9 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 10 && (((nodeData.DA10 < compareMin || nodeData.DA10 > compareMax) && type == "DA") ||
                                    ((nodeData.RT10 < compareMin || nodeData.RT10 > compareMax) && type == "RT") ||
                                    (((nodeData.RT10 - nodeData.DA10) < compareMin || (nodeData.RT10 - nodeData.DA10) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO10 < compareMin || nodeData.RATIO10 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 11 && (((nodeData.DA11 < compareMin || nodeData.DA11 > compareMax) && type == "DA") ||
                                    ((nodeData.RT11 < compareMin || nodeData.RT11 > compareMax) && type == "RT") ||
                                    (((nodeData.RT11 - nodeData.DA11) < compareMin || (nodeData.RT11 - nodeData.DA11) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO11 < compareMin || nodeData.RATIO11 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 12 && (((nodeData.DA12 < compareMin || nodeData.DA12 > compareMax) && type == "DA") ||
                                    ((nodeData.RT12 < compareMin || nodeData.RT12 > compareMax) && type == "RT") ||
                                    (((nodeData.RT12 - nodeData.DA12) < compareMin || (nodeData.RT12 - nodeData.DA12) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO12 < compareMin || nodeData.RATIO12 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 13 && (((nodeData.DA13 < compareMin || nodeData.DA13 > compareMax) && type == "DA") ||
                                    ((nodeData.RT13 < compareMin || nodeData.RT13 > compareMax) && type == "RT") ||
                                    (((nodeData.RT13 - nodeData.DA13) < compareMin || (nodeData.RT13 - nodeData.DA13) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO13 < compareMin || nodeData.RATIO13 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 14 && (((nodeData.DA14 < compareMin || nodeData.DA14 > compareMax) && type == "DA") ||
                                    ((nodeData.RT14 < compareMin || nodeData.RT14 > compareMax) && type == "RT") ||
                                    (((nodeData.RT14 - nodeData.DA14) < compareMin || (nodeData.RT14 - nodeData.DA14) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO14 < compareMin || nodeData.RATIO14 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 15 && (((nodeData.DA15 < compareMin || nodeData.DA15 > compareMax) && type == "DA") ||
                                    ((nodeData.RT15 < compareMin || nodeData.RT15 > compareMax) && type == "RT") ||
                                    (((nodeData.RT15 - nodeData.DA15) < compareMin || (nodeData.RT15 - nodeData.DA15) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO15 < compareMin || nodeData.RATIO15 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 16 && (((nodeData.DA16 < compareMin || nodeData.DA16 > compareMax) && type == "DA") ||
                                    ((nodeData.RT16 < compareMin || nodeData.RT16 > compareMax) && type == "RT") ||
                                    (((nodeData.RT16 - nodeData.DA16) < compareMin || (nodeData.RT16 - nodeData.DA16) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO16 < compareMin || nodeData.RATIO16 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 17 && (((nodeData.DA17 < compareMin || nodeData.DA17 > compareMax) && type == "DA") ||
                                    ((nodeData.RT17 < compareMin || nodeData.RT17 > compareMax) && type == "RT") ||
                                    (((nodeData.RT17 - nodeData.DA17) < compareMin || (nodeData.RT17 - nodeData.DA17) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO17 < compareMin || nodeData.RATIO17 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 18 && (((nodeData.DA18 < compareMin || nodeData.DA18 > compareMax) && type == "DA") ||
                                    ((nodeData.RT18 < compareMin || nodeData.RT18 > compareMax) && type == "RT") ||
                                    (((nodeData.RT18 - nodeData.DA18) < compareMin || (nodeData.RT18 - nodeData.DA18) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO18 < compareMin || nodeData.RATIO18 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 19 && (((nodeData.DA19 < compareMin || nodeData.DA19 > compareMax) && type == "DA") ||
                                    ((nodeData.RT19 < compareMin || nodeData.RT19 > compareMax) && type == "RT") ||
                                    (((nodeData.RT19 - nodeData.DA19) < compareMin || (nodeData.RT19 - nodeData.DA19) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO19 < compareMin || nodeData.RATIO19 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 20 && (((nodeData.DA20 < compareMin || nodeData.DA20 > compareMax) && type == "DA") ||
                                    ((nodeData.RT20 < compareMin || nodeData.RT20 > compareMax) && type == "RT") ||
                                    (((nodeData.RT20 - nodeData.DA20) < compareMin || (nodeData.RT20 - nodeData.DA20) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO20 < compareMin || nodeData.RATIO20 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 21 && (((nodeData.DA21 < compareMin || nodeData.DA21 > compareMax) && type == "DA") ||
                                    ((nodeData.RT21 < compareMin || nodeData.RT21 > compareMax) && type == "RT") ||
                                    (((nodeData.RT21 - nodeData.DA21) < compareMin || (nodeData.RT21 - nodeData.DA21) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO21 < compareMin || nodeData.RATIO21 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 22 && (((nodeData.DA22 < compareMin || nodeData.DA22 > compareMax) && type == "DA") ||
                                    ((nodeData.RT22 < compareMin || nodeData.RT22 > compareMax) && type == "RT") ||
                                    (((nodeData.RT22 - nodeData.DA22) < compareMin || (nodeData.RT22 - nodeData.DA22) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO22 < compareMin || nodeData.RATIO22 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 23 && (((nodeData.DA23 < compareMin || nodeData.DA23 > compareMax) && type == "DA") ||
                                    ((nodeData.RT23 < compareMin || nodeData.RT23 > compareMax) && type == "RT") ||
                                    (((nodeData.RT23 - nodeData.DA23) < compareMin || (nodeData.RT23 - nodeData.DA23) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO23 < compareMin || nodeData.RATIO23 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
                if (hour == 24 && (((nodeData.DA24 < compareMin || nodeData.DA24 > compareMax) && type == "DA") ||
                                    ((nodeData.RT24 < compareMin || nodeData.RT24 > compareMax) && type == "RT") ||
                                    (((nodeData.RT24 - nodeData.DA24) < compareMin || (nodeData.RT24 - nodeData.DA24) > compareMax) && type == "DART") ||
                                    ((nodeData.RATIO24 < compareMin || nodeData.RATIO24 > compareMax) && type == "RATIO")))
                {
                    return false;
                }
            }
            if (isMinChecked == true && (((nodeData.DAMin < compareMin || nodeData.DAMin > compareMax) && type == "DA") ||
                                    ((nodeData.RTMin < compareMin || nodeData.RTMin > compareMax) && type == "RT") ||
                                    ((nodeData.DARTMin < compareMin || nodeData.DARTMin > compareMax) && type == "DART") ||
                                    ((nodeData.RATIOMin < compareMin || nodeData.RATIOMin > compareMax) && type == "RATIO")))
            {
                return false;
            }
            if (isMaxChecked == true && (((nodeData.DAMax < compareMin || nodeData.DAMax > compareMax) && type == "DA") ||
                                        ((nodeData.RTMax < compareMin || nodeData.RTMax > compareMax) && type == "RT") ||
                                        ((nodeData.DARTMax < compareMin || nodeData.DARTMax > compareMax) && type == "DART") ||
                                    ((nodeData.RATIOMax < compareMin || nodeData.RATIOMax > compareMax) && type == "RATIO")))
            {
                return false;
            }
            if (isAvgChecked == true && (((nodeData.DAAvg < compareMin || nodeData.DAAvg > compareMax) && type == "DA") ||
                                        ((nodeData.RTAvg < compareMin || nodeData.RTAvg > compareMax) && type == "RT") ||
                                        ((nodeData.DARTAvg < compareMin || nodeData.DARTAvg > compareMax) && type == "DART") ||
                                    ((nodeData.RATIOAvg < compareMin || nodeData.RATIOAvg > compareMax) && type == "RATIO")))
            {
                return false;
            }
            if (isPeakAvgChecked == true && (((nodeData.DAPeakAvg < compareMin || nodeData.DAPeakAvg > compareMax) && type == "DA") ||
                                            ((nodeData.RTPeakAvg < compareMin || nodeData.RTPeakAvg > compareMax) && type == "RT") ||
                                            ((nodeData.DARTPeakAvg < compareMin || nodeData.DARTPeakAvg > compareMax) && type == "DART") ||
                                    ((nodeData.RATIOPeakAvg < compareMin || nodeData.RATIOPeakAvg > compareMax) && type == "RATIO")))
            {
                return false;
            }
            if (isOffPeakAvgChecked == true && (nodeData.DAOffPeakAvg < compareMin || nodeData.DAOffPeakAvg > compareMax))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Sets the node data.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="nodeData">The node data.</param>
        /// <param name="number">The number.</param>
        /// <param name="da">The da.</param>
        /// <param name="rt">The rt.</param>
        /// <param name="ratio">The ratio.</param>
        /// <returns></returns>
        private NodeData SetNodeData(int i, NodeData nodeData, double? number, double? da, double? rt, double? ratio)
        {
            if (i == 0)
            {
                nodeData.HE1 = number;
                nodeData.DA1 = da;
                nodeData.RT1 = rt;
                nodeData.RATIO1 = ratio;
            }
            if (i == 1)
            {
                nodeData.HE2 = number;
                nodeData.DA2 = da;
                nodeData.RT2 = rt;
                nodeData.RATIO2 = ratio;
            }
            if (i == 2)
            {
                nodeData.HE3 = number;
                nodeData.DA3 = da;
                nodeData.RT3 = rt;
                nodeData.RATIO3 = ratio;
            }
            if (i == 3)
            {
                nodeData.HE4 = number;
                nodeData.DA4 = da;
                nodeData.RT4 = rt;
                nodeData.RATIO4 = ratio;
            }
            if (i == 4)
            {
                nodeData.HE5 = number;
                nodeData.DA5 = da;
                nodeData.RT5 = rt;
                nodeData.RATIO5 = ratio;
            }
            if (i == 5)
            {
                nodeData.HE6 = number;
                nodeData.DA6 = da;
                nodeData.RT6 = rt;
                nodeData.RATIO6 = ratio;
            }
            if (i == 6)
            {
                nodeData.HE7 = number;
                nodeData.DA7 = da;
                nodeData.RT7 = rt;
                nodeData.RATIO7 = ratio;
            }
            if (i == 7)
            {
                nodeData.HE8 = number;
                nodeData.DA8 = da;
                nodeData.RT8 = rt;
                nodeData.RATIO8 = ratio;
            }
            if (i == 8)
            {
                nodeData.HE9 = number;
                nodeData.DA9 = da;
                nodeData.RT9 = rt;
                nodeData.RATIO9 = ratio;
            }
            if (i == 9)
            {
                nodeData.HE10 = number;
                nodeData.DA10 = da;
                nodeData.RT10 = rt;
                nodeData.RATIO10 = ratio;
            }
            if (i == 10)
            {
                nodeData.HE11 = number;
                nodeData.DA11 = da;
                nodeData.RT11 = rt;
                nodeData.RATIO11 = ratio;
            }
            if (i == 11)
            {
                nodeData.HE12 = number;
                nodeData.DA12 = da;
                nodeData.RT12 = rt;
                nodeData.RATIO12 = ratio;
            }
            if (i == 12)
            {
                nodeData.HE13 = number;
                nodeData.DA13 = da;
                nodeData.RT13 = rt;
                nodeData.RATIO13 = ratio;
            }
            if (i == 13)
            {
                nodeData.HE14 = number;
                nodeData.DA14 = da;
                nodeData.RT14 = rt;
                nodeData.RATIO14 = ratio;
            }
            if (i == 14)
            {
                nodeData.HE15 = number;
                nodeData.DA15 = da;
                nodeData.RT15 = rt;
                nodeData.RATIO15 = ratio;
            }
            if (i == 15)
            {
                nodeData.HE16 = number;
                nodeData.DA16 = da;
                nodeData.RT16 = rt;
                nodeData.RATIO16 = ratio;
            }
            if (i == 16)
            {
                nodeData.HE17 = number;
                nodeData.DA17 = da;
                nodeData.RT17 = rt;
                nodeData.RATIO17 = ratio;
            }
            if (i == 17)
            {
                nodeData.HE18 = number;
                nodeData.DA18 = da;
                nodeData.RT18 = rt;
                nodeData.RATIO18 = ratio;
            }
            if (i == 18)
            {
                nodeData.HE19 = number;
                nodeData.DA19 = da;
                nodeData.RT19 = rt;
                nodeData.RATIO19 = ratio;
            }
            if (i == 19)
            {
                nodeData.HE20 = number;
                nodeData.DA20 = da;
                nodeData.RT20 = rt;
                nodeData.RATIO20 = ratio;
            }
            if (i == 20)
            {
                nodeData.HE21 = number;
                nodeData.DA21 = da;
                nodeData.RT21 = rt;
                nodeData.RATIO21 = ratio;
            }
            if (i == 21)
            {
                nodeData.HE22 = number;
                nodeData.DA22 = da;
                nodeData.RT22 = rt;
                nodeData.RATIO22 = ratio;
            }
            if (i == 22)
            {
                nodeData.HE23 = number;
                nodeData.DA23 = da;
                nodeData.RT23 = rt;
                nodeData.RATIO23 = ratio;
            }
            if (i == 23)
            {
                nodeData.HE24 = number;
                nodeData.DA24 = da;
                nodeData.RT24 = rt;
                nodeData.RATIO24 = ratio;
            }
            return nodeData;
        }
        /// <summary>
        /// Gets the prices.
        /// </summary>
        private void GetPrices()
        {
            DateTime start = DateTime.Parse(FromNodeDatePicker.Text);
            DateTime end = DateTime.Parse(ToNodeDatePicker.Text);
            List<DateTime> dateList = new List<DateTime>();
            if (CollectionRadioButton.IsChecked == true)
            {
                foreach (string dateStr in DateCollectionListBox.Items)
                {
                    dateList.Add(DateTime.Parse(dateStr));
                }
            }
            else
            {
                while (start <= end)
                {
                    dateList.Add(start);
                    start = start.AddDays(1);
                }
            }
            int market = 1;

            if ((bool)ercotRadioButton.IsChecked)
            {
                market = 9;
            }

            foreach (DateTime date in dateList)
            {
                //DARTNode.GetAllDarts(market, date, date.AddDays(1));
                DARTNode.GetAllDartsAndLMP(market, date, date.AddDays(1));
            }
        }
        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void Refresh()
        {
            Dispatcher.BeginInvoke(new refreshDelegate(this.RefreshThreaded));
        }

        /// <summary>
        /// Shows the node map.
        /// </summary>
        private void ShowNodeMap()
        {
            List<Tuple<String, String>> ListOfNodesDetails = new List<Tuple<String, String>>();
            myMap.Children.Clear();
            LoadColorZones();
            if (nodeDataGrid.SelectedItems.Count > 0)
            {
                for (int i = 0; i < nodeDataGrid.SelectedItems.Count; i++)
                {
                    NodeData selectedFile = (NodeData)nodeDataGrid.SelectedItems[i];
                    ListOfNodesDetails.Add(new Tuple<String, String>(selectedFile.NodeName, selectedFile.ZoneName));
                }
            }
            if (mIsFirstTime)
            {
                return;
            }
            VayuConnetion.Open();
            foreach (var nodeName in ListOfNodesDetails)
            {
                mSelectlNodesValuesCommand.Parameters["@NodeName"].Value = nodeName.Item1;
                SqlDataReader drNode = mSelectlNodesValuesCommand.ExecuteReader();
                while (drNode.Read())
                {
                    double Longitude = Convert.ToDouble(drNode.GetValue(2));
                    double Latitude = Convert.ToDouble(drNode.GetValue(3));
                    Pushpin pin = new Pushpin();
                    Location location = new Location(Latitude, Longitude);
                    pin.Location = location;
                    string toolTip = drNode.GetValue(1).ToString() + "\n" + nodeName.Item2;
                    ToolTip tt = new ToolTip();
                    tt.Content = toolTip;
                    tt.FontWeight = FontWeights.Bold;
                    pin.ToolTip = tt;
                    myMap.Center = location;
                    myMap.ZoomLevel = 7;
                    myMap.Children.Add(pin);
                }
                drNode.Close();
            }
            VayuConnetion.Close();
        }
        /// <summary>
        /// Loads the color zones.
        /// </summary>
        private void LoadColorZones()
        {
            List<ZoneInfo> zlist = ZoneModel.GetZoneList(sMarketSelected);
            ListZone = zlist;
            foreach (var item in ListZone)
            {
                foreach (var zoneitem in item.RegionInfoList)
                {
                    MapPolygon zonePolygon = new MapPolygon();
                    #region ZoneColor
                    switch (zoneitem.Name)
                    {
                        case "AEP":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
                            break;
                        case "APS":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 219, 133, 108));
                            break;
                        case "AEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 77, 77));
                            break;
                        case "ATSI":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 179, 191, 128));
                            break;
                        case "BGE":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 204, 204));
                            break;
                        case "COMED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 153, 0));
                            break;
                        case "DAYTON":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 108, 247, 49));
                            break;
                        case "DEOK":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 51, 153, 255));
                            break;
                        case "DOM":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 76, 153));
                            break;
                        case "DPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 54, 219));
                            break;
                        case "DUQ":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(50, 204, 204, 0));
                            break;
                        case "JCPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255));
                            break;
                        case "METED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 49, 252, 35));
                            break;
                        case "PECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 153, 76));
                            break;
                        case "PENELEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 103, 57, 238));
                            break;
                        case "PEPCO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 0));
                            break;
                        case "PPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 194, 126, 231));
                            break;
                        case "PSEG":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 255));
                            break;
                        case "RECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 76, 0));
                            break;
                        case "EKPC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 102));
                            break;
                        default:
                            break;
                    }
                    #endregion ZoneColor
                    zonePolygon.Locations = zoneitem.Locations;
                    zonePolygon.ToolTip = zoneitem.Name;
                    myMap.Children.Add(zonePolygon);
                }
            }
        }
        /// <summary>
        /// Refreshes the threaded.
        /// </summary>
        private void RefreshThreaded()
        {
            if (mIsFirstTime)
            {
                return;
            }
            if (VayuConnetion == null)
            {
                return;
            }


            Mouse.OverrideCursor = Cursors.Wait;
            mNodeHash = DBAccess.GetAllNodes(GetMarket(), mNodeTypeHash, mZoneHash);
            mIsFirstTime = true;
            totalRecordsLabel.Content = string.Empty;
            SetZoneComboBox();
            SetNodeTypesComboBox();
            mIsFirstTime = false;
            GetPrices();
            SetNodeTable(int.Parse(filterLmpHourComboBox.Text));
            SetNodeTable();
            Mouse.OverrideCursor = null;
        }
        /// <summary>
        /// Sets the node types ComboBox.
        /// </summary>
        private void SetNodeTypesComboBox()
        {
            NodeTypeComboBox.Items.Clear();
            int market = 1;

            if ((bool)ercotRadioButton.IsChecked)
            {
                market = 9;
            }

            List<string> nodeTypeList = mNodeTypeHash[market];
            nodeTypeList.Sort();
            NodeTypeComboBox.Items.Add("ALL");
            foreach (string nodeType in nodeTypeList)
            {
                NodeTypeComboBox.Items.Add(nodeType);
            }
            NodeTypeComboBox.Text = "ALL";
        }
        /// <summary>
        /// Fills the source sink hash.
        /// </summary>
        private void FillSourceSinkHash()
        {
            if (VayuConnetion.State == ConnectionState.Closed)
            {
                VayuConnetion.Open();
            }

            mSelectSourceSinkNodeCommand.Parameters["@MarketKey"].Value = 1;
            SqlDataReader reader = mSelectSourceSinkNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                string source = reader.GetString(0);
                if (!mPJMSourceSinkList.Contains(source))
                {
                    mPJMSourceSinkList.Add(source);
                }
                string sink = reader.GetString(1);
                if (!mPJMSourceSinkList.Contains(sink))
                {
                    mPJMSourceSinkList.Add(sink);
                }
                mUptosPathList.Add(source + "?" + sink);
            }
            reader.Close();
            //reader = mSelectFtrSourceSinkNodeCommand.ExecuteReader();
            //while (reader.Read())
            //{
            //    if (!mPJMFtrSourceSinkList.Contains((int)reader.GetDecimal(0)))
            //    {
            //        mPJMFtrSourceSinkList.Add((int)reader.GetDecimal(0));
            //    }
            //    if (!mPJMFtrSourceSinkList.Contains((int)reader.GetDecimal(0)))
            //    {
            //        mPJMFtrSourceSinkList.Add((int)reader.GetDecimal(0));
            //    }
            //}
            VayuConnetion.Close();
        }

        private void GetErcotSourceSinkHash()
        {
            VayuConnetion.Open();
            mSelectercotSourceSinkNodeCommand = new SqlCommand();
            //mSelectercotSourceSinkNodeCommand.CommandText = "select src.SourceNodeName, sink.SinkNodeName from Vayu.. EESPathList (nolock) src inner join Vayu.. Node (nolock) n on n.NodeKey = src.SourceNodeKey " +
            //                                                " inner join Vayu..EESPathList sink on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey " +
            //                                                " inner join Vayu..Node n2 on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = 9 and n2.MarketKey = 9 and src.MarketKey = 9 and sink.MarketKey = 9 " +
            //                                                " order by n.NodeName ";
            mSelectercotSourceSinkNodeCommand.CommandText = " select src.SourceNodeName, sink.SinkNodeName,Fall.MinLMP RTFallMin,Fall.MaxLMP RTFallMax,Spr.MinLMP as RTSpringMin,Spr.MaxLMP as RTSpringMax , " +
                                                           " Summ.MinLMP as RTSummerMin,Summ.MaxLMP as RTSummerMax,Win.MinLMP as RTWinterMin,Win.MaxLMP as RTWinterMax,UTC.PathMinRT " +
                                                           " from Vayu.. EESPathList (nolock) src inner join Vayu.. Node (nolock) n on n.NodeKey = src.SourceNodeKey " +
                                                           " inner join Vayu..EESPathList sink on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey  " +
                                                           " inner join Vayu..Node n2 on n2.NodeKey = sink.SinkNodeKey " +
                                                           " left join Vayu..RTRangeFall AS Fall on Fall.SourceName=src.SourceNodeName and Fall.SinkName=src.SinkNodeName " +
                                                           " left join Vayu..RTRangeSummer AS Summ on Summ.SourceName=src.SourceNodeName and Summ.SinkName=src.SinkNodeName " +
                                                           " left join Vayu..RTRangeWinter AS Win on Win.SourceName=src.SourceNodeName and Win.SinkName=src.SinkNodeName " +
                                                           " left join Vayu..RTRangeSpring AS Spr on Spr.SourceName=src.SourceNodeName and Spr.SinkName=src.SinkNodeName " +
                                                           " join Vayu..UTCPathHistoricData UTC on N.NodeKey=UTC.SourceNodeKey and N2.NodeKey=UTC.SinkNodeKey " +
                                                           " where n.MarketKey = 9 and n2.MarketKey = 9 and src.MarketKey = 9 and sink.MarketKey = 9 order by n.NodeName ";
            mSelectercotSourceSinkNodeCommand.Connection = VayuConnetion;
            SqlDataReader reader = mSelectercotSourceSinkNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                string source = reader.GetString(0);
                if (!mErcotSourceSinkList.Contains(source))
                {
                    mErcotSourceSinkList.Add(source);
                }
                string sink = reader.GetString(1);
                if (!mErcotSourceSinkList.Contains(sink))
                {
                    mErcotSourceSinkList.Add(sink);
                }
                decimal RTMinFall = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                decimal RTMaxFall = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);

                decimal RTMinSpring = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                decimal RTMaxSpring = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);

                decimal RTMinSummer = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                decimal RTMaxSummer = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);

                decimal RTMinWinter = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                decimal RTMaxWinter = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                DateTime PathMinRT = reader.IsDBNull(10) ? DateTime.Today : reader.GetDateTime(10);

                decimal min1 = Math.Min(RTMinWinter, RTMinSpring);
                decimal min2 = Math.Min(RTMinSummer, RTMinFall);
                decimal max1 = Math.Max(RTMaxWinter, RTMaxSpring);
                decimal max2 = Math.Max(RTMaxSummer, RTMaxFall);
                decimal SRTMin = Math.Min(min1, min2);
                decimal SRTMax = Math.Max(max1, max2);

                mErcotUptosPathList.Add(source + "?" + sink + "?" + SRTMin + "?" + SRTMax + "?" + PathMinRT);
            }
            reader.Close();
            VayuConnetion.Close();
        }
        /// <summary>
        /// Sets to CST.
        /// </summary>
        /// <param name="datetime">The datetime.</param>
        /// <returns></returns>
        private DateTime SetToCST(DateTime datetime)
        {
            TimeZone zone = TimeZone.CurrentTimeZone;
            string zoneName = zone.StandardName;
            DateTime CSTLocal = datetime;

            if (zone.StandardName.StartsWith("Mountain"))
            {
                CSTLocal = datetime.AddHours(-1);
            }
            else if (zone.StandardName.StartsWith("East"))
            {
                CSTLocal = datetime.AddHours(1);
            }
            else if (zone.StandardName.StartsWith("Pacific"))
            {
                CSTLocal = datetime.AddHours(-2);
            }
            return CSTLocal;
        }

        #endregion

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
        /// 
        /// </summary>
        public delegate void refreshDelegate();

        #region Events

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
        /// Handles the Click event of the exportMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void exportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel<NodeHourLMP, List<NodeHourLMP>> s = new ExportToExcel<NodeHourLMP, List<NodeHourLMP>>();

            ICollectionView view = CollectionViewSource.GetDefaultView(mHourLmpList);
            if (mHourLmpList.Count > 0)
            {
                view = CollectionViewSource.GetDefaultView(mHourLmpList);
            }
            if (!(bool)UptosCheckBox.IsChecked)
            {
                List<string> propertyName = new List<string>();
                propertyName.Add("SinkName");
                propertyName.Add("SinkType");
                propertyName.Add("SinkZone");
                propertyName.Add("Hour");
                s.dataToPrint = mHourLmpList;
                s.GenerateReport(propertyName);
            }
            else
            {
                s.dataToPrint = mHourLmpList;
                s.GenerateReport();
            }
        }
        /// <summary>
        /// Handles the Click event of the NodeAnalyzer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NodeAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            int marketKey = GetMarket();
            var selectedItems = SelectedItems;
            List<SourceSinkData> sourceSinkList = new List<SourceSinkData>();
            foreach (NodeData nodeData in selectedItems)
            {
                SourceSinkData sourceSinkData = new SourceSinkData();
                Vayu.DBLibrary.NodeDetail nodeDetail = mNodeHash[nodeData.NodeName];
                PricingNode priceNode = new PricingNode();
                priceNode.ExternalNodeId = nodeDetail.ExternalID;
                priceNode.MarketKey = marketKey;
                priceNode.NodeKey = nodeDetail.ID;
                priceNode.NodeName = nodeData.NodeName;
                priceNode.NodeTypeKey = nodeDetail.TypeKey;
                priceNode.Zone = nodeDetail.Zone;
                sourceSinkData.Source = priceNode;
                if (nodeData.SinkName != null)
                {
                    nodeDetail = mNodeHash[nodeData.SinkName];
                    priceNode = new PricingNode();
                    priceNode.ExternalNodeId = nodeDetail.ExternalID;
                    priceNode.MarketKey = 1;
                    priceNode.NodeKey = nodeDetail.ID;
                    priceNode.NodeName = nodeData.SinkName;
                    priceNode.NodeTypeKey = nodeDetail.TypeKey;
                    priceNode.Zone = nodeDetail.Zone;
                    sourceSinkData.Sink = priceNode;
                }
                sourceSinkList.Add(sourceSinkData);
            }
            LmpForm.OpenLMPStatisticAnalyzer(marketKey, sourceSinkList);
        }
        /// <summary>
        /// Handles the Click event of the LMPGraph control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LMPGraph_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SelectedItems;
            int marketKey = GetMarket();
            List<SourceSinkData> sourceSinkList = new List<SourceSinkData>();
            foreach (NodeData nodeData in selectedItems)
            {
                Vayu.DBLibrary.NodeDetail nodeDetail = mNodeHash[nodeData.NodeName];
                SourceSinkData sourceSink = new SourceSinkData();
                sourceSink.Source = new PricingNode();
                sourceSink.Source.ExternalNodeId = nodeDetail.ExternalID;
                sourceSink.Source.MarketKey = marketKey;
                sourceSink.Source.NodeKey = nodeDetail.ID;
                sourceSink.Source.NodeName = nodeData.NodeName;
                sourceSink.Source.NodeTypeKey = nodeDetail.TypeKey;
                sourceSink.Source.Zone = nodeDetail.Zone;
                if (UptosCheckBox.IsChecked == true)
                {
                    nodeDetail = mNodeHash[nodeData.SinkName];
                    sourceSink.Sink = new PricingNode();
                    sourceSink.Sink.ExternalNodeId = nodeDetail.ExternalID;
                    sourceSink.Sink.MarketKey = marketKey;
                    sourceSink.Sink.NodeKey = nodeDetail.ID;
                    sourceSink.Sink.NodeName = nodeData.SinkName;
                    sourceSink.Sink.NodeTypeKey = nodeDetail.TypeKey;
                    sourceSink.Sink.Zone = nodeDetail.Zone;
                }
                sourceSinkList.Add(sourceSink);
            }
            DateTime start = DateTime.Parse(FromNodeDatePicker.Text);
            DateTime end = DateTime.Parse(ToNodeDatePicker.Text);
            LmpForm.OpenLmpGraphs(marketKey, sourceSinkList, start, end);
        }
        /// <summary>
        /// Handles the Click event of the refreshButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            mIsFirstTime = false;
            Thread th = new Thread(() =>
            {
                Refresh();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }
        /// <summary>
        /// Sets the zone ComboBox.
        /// </summary>
        private void SetZoneComboBox()
        {
            ZoneListBox.Items.Clear();
            SinkZoneListBox.Items.Clear();
            int market = 1;

            if ((bool)ercotRadioButton.IsChecked)
            {
                market = 9;
            }

            List<string> zoneList = mZoneHash[market];
            zoneList.Sort();
            ZoneListBox.Items.Add("NONE");
            SinkZoneListBox.Items.Add("NONE");
            foreach (string zone in zoneList)
            {
                ZoneListBox.Items.Add(zone);
                SinkZoneListBox.Items.Add(zone);
            }
            ZoneListBox.SelectAll();
            SinkZoneListBox.SelectAll();
        }
        /// <summary>
        /// Handles the Unchecked event of the Daily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Daily_Unchecked(object sender, RoutedEventArgs e)
        {
            nodeDataGrid.Columns[35].Visibility = System.Windows.Visibility.Hidden;
            nodeDataGrid.Columns[36].Visibility = System.Windows.Visibility.Hidden;
            nodeDataGrid.Columns[37].Visibility = System.Windows.Visibility.Hidden;
            nodeDataGrid.Columns[38].Visibility = System.Windows.Visibility.Hidden;
            nodeDataGrid.Columns[39].Visibility = System.Windows.Visibility.Hidden;
            nodeDataGrid.Columns[40].Visibility = System.Windows.Visibility.Hidden;
            nodeDataGrid.Columns[41].Visibility = System.Windows.Visibility.Hidden;
        }
        /// <summary>
        /// Handles the Checked event of the Daily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Daily_Checked(object sender, RoutedEventArgs e)
        {
            nodeDataGrid.Columns[35].Visibility = System.Windows.Visibility.Visible;
            nodeDataGrid.Columns[36].Visibility = System.Windows.Visibility.Visible;
            nodeDataGrid.Columns[37].Visibility = System.Windows.Visibility.Visible;
            nodeDataGrid.Columns[38].Visibility = System.Windows.Visibility.Visible;
            nodeDataGrid.Columns[39].Visibility = System.Windows.Visibility.Visible;
            nodeDataGrid.Columns[40].Visibility = System.Windows.Visibility.Visible;
            nodeDataGrid.Columns[41].Visibility = System.Windows.Visibility.Visible;
        }


        /// <summary>
        /// Handles the Checked event of the ercotRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ercotRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)ercotRadioButton.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                sMarketSelected = 9;
                Refresh();
            }
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the ToNodeDatePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ToNodeDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        /// <summary>
        /// Handles the Checked event of the daRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void daRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)daRadioButton.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the rtRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void rtRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)rtRadioButton.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the dartRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void dartRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)dartRadioButton.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the NullCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NullCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (tabControl1 != null)
            {
                tabControl1.SelectedIndex = 0;
            }
            SetNodeTable();
        }
        /// <summary>
        /// Handles the Unchecked event of the NullCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NullCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetNodeTable();
        }
        /// <summary>
        /// Handles the SelectionChanged event of the ZoneComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ZoneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!mIsFirstTime && DARTNode.sDAHash != null)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the 1 event of the nodeDataGrid_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void nodeDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ShowNodeMap();
        }
        /// <summary>
        /// Handles the SelectionChanged event of the NodeTypeComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void NodeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!mIsFirstTime && DARTNode.sDAHash != null)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }

        /// <summary>
        /// Handles the Click event of the addButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            FromNodeDatePicker.Text = DateTime.Parse(FromNodeDatePicker.Text).AddDays(1).ToShortDateString();
            ToNodeDatePicker.Text = DateTime.Parse(ToNodeDatePicker.Text).AddDays(1).ToShortDateString();
        }
        /// <summary>
        /// Handles the Click event of the subtractButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void subtractButton_Click(object sender, RoutedEventArgs e)
        {
            FromNodeDatePicker.Text = DateTime.Parse(FromNodeDatePicker.Text).AddDays(-1).ToShortDateString();
            ToNodeDatePicker.Text = DateTime.Parse(ToNodeDatePicker.Text).AddDays(-1).ToShortDateString();
        }
        /// <summary>
        /// Handles the Click event of the todayButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void todayButton_Click(object sender, RoutedEventArgs e)
        {
            FromNodeDatePicker.Text = DateTime.Today.ToShortDateString();
            ToNodeDatePicker.Text = DateTime.Today.ToShortDateString();
        }
        /// <summary>
        /// Handles the Checked event of the MaxRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MaxRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MaxRadioButton.IsChecked == true)
            {
                SetNodeTable();
            }
        }

        /// <summary>
        /// Handles the Checked event of the MinRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MinRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MinRadioButton.IsChecked == true)
            {
                SetNodeTable();
            }
        }

        /// <summary>
        /// Handles the Checked event of the AvgRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AvgRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (AvgRadioButton.IsChecked == true)
            {
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the PeakAvgRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PeakAvgRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (PeakAvgRadioButton.IsChecked == true)
            {
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the HourRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void HourRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (HourRadioButton.IsChecked == true)
            {
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the OffPeakRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OffPeakRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (OffPeakRadioButton.IsChecked == true)
            {
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the DropDownClosed event of the PathComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PathComboBox_DropDownClosed(object sender, EventArgs e)
        {
            SetNodeTable();
        }
        /// <summary>
        /// Handles the Click event of the SinkSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SinkSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SinkZoneListBox.SelectAll();
        }
        /// <summary>
        /// Handles the Click event of the SinkUnSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SinkUnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SinkZoneListBox.UnselectAll();
        }
        /// <summary>
        /// Handles the Click event of the SelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ZoneListBox.SelectAll();
        }
        /// <summary>
        /// Handles the Click event of the UnSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ZoneListBox.UnselectAll();
        }
        /// <summary>
        /// Handles the Click event of the filterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void filterButton_Click(object sender, RoutedEventArgs e)
        {
            SetNodeTable();
        }
        /// <summary>
        /// Handles the Click event of the ZoneFilterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ZoneFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SetNodeTable();
        }
        /// <summary>
        /// Handles the Checked event of the AggAvgRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AggAvgRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (AggAvgRadioButton.IsChecked == true)
            {
                //SetNodeTable(int.Parse(filterLmpHourComboBox.SelectedItem.ToString().Substring(filterLmpHourComboBox.SelectedItem.ToString().LastIndexOf(':') + 1)));
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the AggMaxRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AggMaxRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (AggMaxRadioButton.IsChecked == true)
            {
                //SetNodeTable(int.Parse(filterLmpHourComboBox.SelectedItem.ToString().Substring(filterLmpHourComboBox.SelectedItem.ToString().LastIndexOf(':') + 1)));
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the Checked event of the AggMinRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AggMinRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (AggMinRadioButton.IsChecked == true)
            {
                //SetNodeTable(int.Parse(filterLmpHourComboBox.SelectedItem.ToString().Substring(filterLmpHourComboBox.SelectedItem.ToString().LastIndexOf(':') + 1)));
                SetNodeTable();
            }
        }
        /// <summary>
        /// Handles the SelectedDateChanged event of the FromNodeDatePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void FromNodeDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FromNodeDatePicker.SelectedDate.HasValue)
                sIsDST = FromNodeDatePicker.SelectedDate.Value.IsDaylightSavingTime();
        }
        /// <summary>
        /// Handles the Checked event of the RangeRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RangeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            FromNodeDatePicker.IsEnabled = true;
            ToNodeDatePicker.IsEnabled = true;
            if (CollectionComboBox != null)
            {
                CollectionComboBox.IsEnabled = false;
                CollectionComboBox.Items.Clear();
                CollectionComboBox.Text = "";
                DateCollectionListBox.Items.Clear();
            }
        }
        /// <summary>
        /// Handles the Checked event of the CollectionRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CollectionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            FromNodeDatePicker.IsEnabled = false;
            ToNodeDatePicker.IsEnabled = false;
            if (CollectionComboBox != null)
            {
                CollectionComboBox.IsEnabled = true;
            }
            CollectionComboBox.Items.Clear();
            CollectionComboBox.Text = "";
            List<string> dateNameList = DBAccess.GetDateNames();
            foreach (string dateName in dateNameList)
            {
                CollectionComboBox.Items.Add(dateName);
            }
        }

        /// <summary>
        /// Handles the DropDownClosed event of the CollectionComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CollectionComboBox_DropDownClosed(object sender, EventArgs e)
        {
            DateCollectionListBox.Items.Clear();
            List<DateTime> dateList = DBAccess.GetDateRange(CollectionComboBox.Text);
            foreach (DateTime date in dateList)
            {
                DateCollectionListBox.Items.Add(date.ToShortDateString());
            }
        }

        /// <summary>
        /// Handles the Checked event of the UptosCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UptosCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// Handles the Unchecked event of the UptosCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UptosCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// Handles the Click event of the MenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel<NodeDisplayData, List<NodeDisplayData>> s =
                new ExportToExcel<NodeDisplayData, List<NodeDisplayData>>();

            List<NodeDisplayData> tempList = new List<NodeDisplayData>();
            //ExportToExcel<NodeData, List<NodeData>> s =
            //    new ExportToExcel<NodeData, List<NodeData>>();

            ICollectionView view = CollectionViewSource.GetDefaultView(tempnodeList);// mLmpList
            if (mLmpList.Count > 0)
            {
                view = CollectionViewSource.GetDefaultView(tempnodeList);//mLmpList
            }
            List<string> propertyName = new List<string>();

            //  if (!(bool)UptosCheckBox.IsChecked)
            {
                propertyName.Add("SinkName");
                propertyName.Add("SinkType");
                propertyName.Add("SinkZone");
                foreach (var item in tempnodeList)
                {
                    foreach (var propItem in item.GetType().GetProperties())
                    {
                        if (propItem.Name == "NodeName" || propItem.Name == "NodeType" || propItem.Name == "ZoneName" || propItem.Name == "SinkName" || propItem.Name == "SinkType" || propItem.Name == "SinkZone" || propItem.Name == "Max" || propItem.Name == "Min" || propItem.Name == "Avg" || propItem.Name == "PeakAvg" || propItem.Name == "OffPeakAvg" ||
                            propItem.Name == "PeakAvg" || propItem.Name == "OffPeakAvg" || propItem.Name == "HE1" || propItem.Name == "HE2" || propItem.Name == "HE3" || propItem.Name == "HE4" || propItem.Name == "HE5" || propItem.Name == "HE6" || propItem.Name == "HE7" || propItem.Name == "HE8" ||
                            propItem.Name == "HE9" || propItem.Name == "HE10" || propItem.Name == "HE11" || propItem.Name == "HE12" || propItem.Name == "HE13" || propItem.Name == "HE14" || propItem.Name == "HE15" || propItem.Name == "HE16" || propItem.Name == "HE17" || propItem.Name == "HE18" ||
                            propItem.Name == "HE19" || propItem.Name == "HE20" || propItem.Name == "HE21" || propItem.Name == "HE22" || propItem.Name == "HE23" || propItem.Name == "HE24" || propItem.Name == "SRTMin" || propItem.Name == "SRTMax" || propItem.Name == "PathMinRT")
                        {
                            NodeDisplayData nodedisplay = new NodeDisplayData();
                            nodedisplay.NodeName = item.NodeName;
                            nodedisplay.NodeType = item.NodeType;
                            nodedisplay.ZoneName = item.ZoneName;
                            nodedisplay.SinkName = item.SinkName;
                            nodedisplay.SinkType = item.SinkType;
                            nodedisplay.SinkZone = item.SinkZone;
                            nodedisplay.Max = item.Max;
                            nodedisplay.Min = item.Min;
                            nodedisplay.Avg = item.Avg;
                            nodedisplay.PeakAvg = item.PeakAvg;
                            nodedisplay.OffPeakAvg = item.OffPeakAvg;
                            nodedisplay.SRTMin = item.SRTMin;
                            nodedisplay.SRTMax = item.SRTMax;
                            nodedisplay.PathMinRT = item.PathMinRT;
                            nodedisplay.HE1 = item.HE1;
                            nodedisplay.HE2 = item.HE2;
                            nodedisplay.HE3 = item.HE3;
                            nodedisplay.HE4 = item.HE4;
                            nodedisplay.HE5 = item.HE5;
                            nodedisplay.HE6 = item.HE6;
                            nodedisplay.HE7 = item.HE7;
                            nodedisplay.HE8 = item.HE8;
                            nodedisplay.HE9 = item.HE9;
                            nodedisplay.HE10 = item.HE10;
                            nodedisplay.HE11 = item.HE11;
                            nodedisplay.HE12 = item.HE12;
                            nodedisplay.HE13 = item.HE13;
                            nodedisplay.HE14 = item.HE14;
                            nodedisplay.HE15 = item.HE15;
                            nodedisplay.HE16 = item.HE16;
                            nodedisplay.HE17 = item.HE17;
                            nodedisplay.HE18 = item.HE18;
                            nodedisplay.HE19 = item.HE19;
                            nodedisplay.HE20 = item.HE20;
                            nodedisplay.HE21 = item.HE21;
                            nodedisplay.HE22 = item.HE22;
                            nodedisplay.HE23 = item.HE23;
                            nodedisplay.HE24 = item.HE24;

                            tempList.Add(nodedisplay);
                        }
                        break;
                    }
                }
                s.dataToPrint = tempList; //mLmpList;
                if (!(bool)UptosCheckBox.IsChecked)
                    s.GenerateReport(propertyName);
                else
                    s.GenerateReport();
            }
            //else
            //{
            //    var nodedata = from nodes in tempnodeList
            //                   select nodes;
            //    tempList = (List<NodeDisplayData>)nodedata;
            //    s.dataToPrint = tempList; //mLmpList;
            //    s.GenerateReport();
            //}
        }

        /// <summary>
        /// Handles the Click event of the nodeTypeCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void nodeTypeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (UptosCheckBox.IsChecked == true && nodeTypeCheckBox.IsChecked == true)
            {
                nodeDataGrid.Columns[4].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                nodeDataGrid.Columns[4].Visibility = System.Windows.Visibility.Hidden;
            }
            if (nodeTypeCheckBox.IsChecked == true)
            {
                nodeDataGrid.Columns[1].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                nodeDataGrid.Columns[1].Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// Handles the Click event of the ratioRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ratioRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ratioRadioButton.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the filterLmpHourComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void filterLmpHourComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!filterLmpHourComboBox.SelectedItem.ToString().Equals("") && (bool)lmpHourCheckBox.IsChecked == true)
            {
                SetNodeTable(int.Parse(filterLmpHourComboBox.SelectedItem.ToString().Substring(filterLmpHourComboBox.SelectedItem.ToString().LastIndexOf(':') + 1)));
            }
        }

        /// <summary>
        /// Handles the Checked event of the lmpHourCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void lmpHourCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (mIsFirstTime)
            {
                return;
            }
            if (lmpHourCheckBox.IsChecked == false)
            {
                nodeHourLmpDataGrid.Items.Clear();
            }
            else
            {
                SetNodeTable(int.Parse(filterLmpHourComboBox.SelectedItem.ToString().Substring(filterLmpHourComboBox.SelectedItem.ToString().LastIndexOf(':') + 1)));
            }
        }

        /// <summary>
        /// Handles the SelectedCellsChanged event of the nodeDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedCellsChangedEventArgs"/> instance containing the event data.</param>
        private void nodeDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            List<NodeData> selectedItems = new List<NodeData>();
            List<int> rowIndex = new List<int>();

            foreach (var cell in nodeDataGrid.SelectedCells)
            {
                DataGridCell cellItem = DataGridInfo.GetCell(cell);
                int index = DataGridInfo.GetRowIndex(cellItem);
                if (!rowIndex.Contains(index))
                {
                    selectedItems.Add(cell.Item as NodeData);
                    rowIndex.Add(index);
                }
            }
            SelectedItems = selectedItems;
        }

        /// <summary>
        /// Handles the Click event of the NodeMaps control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NodeMaps_Click(object sender, RoutedEventArgs e)
        {
            List<Tuple<String, String>> ListOfNodesDetails = new List<Tuple<String, String>>();
            var selectedItems = SelectedItems;
            myMap.Children.Clear();
            LoadColorZones();
            VayuConnetion.Open();
            foreach (var nodeName in selectedItems)
            {
                SqlDataReader drNode;
                mSelectErcotNodesCommand.Parameters["@NodeName"].Value = nodeName.NodeName;
                drNode = mSelectErcotNodesCommand.ExecuteReader();
                while (drNode.Read())
                {
                    double Longitude = Convert.ToDouble(drNode.GetValue(2));
                    double Latitude = Convert.ToDouble(drNode.GetValue(3));
                    Pushpin pin = new Pushpin();
                    Location location = new Location(Latitude, Longitude);
                    pin.Location = location;
                    string toolTip = drNode.GetValue(1).ToString() + "\n" + nodeName.ZoneName;
                    ToolTip tt = new ToolTip();
                    tt.Content = toolTip;
                    tt.FontWeight = FontWeights.Bold;
                    pin.ToolTip = tt;
                    myMap.Center = location;
                    myMap.ZoomLevel = 7;
                    myMap.Children.Add(pin);
                }
                drNode.Close();
            }
            VayuConnetion.Close();
            tabControl1.SelectedIndex = 2;
        }


        private void rdbPrice_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)rdbPrice.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }

        private void rdbCongestion_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)rdbCongestion.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }

        private void rdbLoss_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)rdbLoss.IsChecked)
            {
                if (tabControl1 != null)
                {
                    tabControl1.SelectedIndex = 0;
                }
                SetNodeTable();
            }
        }


        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    public class NodeData
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { set; get; }
        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        public string NodeType { set; get; }
        /// <summary>
        /// Gets or sets the name of the zone.
        /// </summary>
        /// <value>
        /// The name of the zone.
        /// </value>
        public string ZoneName { set; get; }
        /// <summary>
        /// Gets or sets the name of the sink.
        /// </summary>
        /// <value>
        /// The name of the sink.
        /// </value>
        public string SinkName { set; get; }
        /// <summary>
        /// Gets or sets the type of the sink.
        /// </summary>
        /// <value>
        /// The type of the sink.
        /// </value>
        public string SinkType { set; get; }
        /// <summary>
        /// Gets or sets the sink zone.
        /// </summary>
        /// <value>
        /// The sink zone.
        /// </value>
        public string SinkZone { set; get; }
        /// <summary>
        /// Gets or sets the da maximum.
        /// </summary>
        /// <value>
        /// The da maximum.
        /// </value>
        public double? DAMax { set; get; }
        /// <summary>
        /// Gets or sets the rt maximum.
        /// </summary>
        /// <value>
        /// The rt maximum.
        /// </value>
        public double? RTMax { set; get; }
        /// <summary>
        /// Gets or sets the dart maximum.
        /// </summary>
        /// <value>
        /// The dart maximum.
        /// </value>
        public double? DARTMax { set; get; }
        /// <summary>
        /// Gets or sets the ratio maximum.
        /// </summary>
        /// <value>
        /// The ratio maximum.
        /// </value>
        public double? RATIOMax { set; get; }
        /// <summary>
        /// Gets or sets the da minimum.
        /// </summary>
        /// <value>
        /// The da minimum.
        /// </value>
        public double? DAMin { set; get; }
        /// <summary>
        /// Gets or sets the rt minimum.
        /// </summary>
        /// <value>
        /// The rt minimum.
        /// </value>
        public double? RTMin { set; get; }
        /// <summary>
        /// Gets or sets the dart minimum.
        /// </summary>
        /// <value>
        /// The dart minimum.
        /// </value>
        public double? DARTMin { set; get; }
        /// <summary>
        /// Gets or sets the ratio minimum.
        /// </summary>
        /// <value>
        /// The ratio minimum.
        /// </value>
        public double? RATIOMin { set; get; }
        /// <summary>
        /// Gets or sets the da average.
        /// </summary>
        /// <value>
        /// The da average.
        /// </value>
        public double? DAAvg { set; get; }
        /// <summary>
        /// Gets or sets the rt average.
        /// </summary>
        /// <value>
        /// The rt average.
        /// </value>
        public double? RTAvg { set; get; }
        /// <summary>
        /// Gets or sets the dart average.
        /// </summary>
        /// <value>
        /// The dart average.
        /// </value>
        public double? DARTAvg { set; get; }
        /// <summary>
        /// Gets or sets the ratio average.
        /// </summary>
        /// <value>
        /// The ratio average.
        /// </value>
        public double? RATIOAvg { set; get; }
        /// <summary>
        /// Gets or sets the da peak average.
        /// </summary>
        /// <value>
        /// The da peak average.
        /// </value>
        public double? DAPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the rt peak average.
        /// </summary>
        /// <value>
        /// The rt peak average.
        /// </value>
        public double? RTPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the dart peak average.
        /// </summary>
        /// <value>
        /// The dart peak average.
        /// </value>
        public double? DARTPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the ratio peak average.
        /// </summary>
        /// <value>
        /// The ratio peak average.
        /// </value>
        public double? RATIOPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the da off peak average.
        /// </summary>
        /// <value>
        /// The da off peak average.
        /// </value>
        public double? DAOffPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the rt off peak average.
        /// </summary>
        /// <value>
        /// The rt off peak average.
        /// </value>
        public double? RTOffPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the dart off peak average.
        /// </summary>
        /// <value>
        /// The dart off peak average.
        /// </value>
        public double? DARTOffPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the ratio off peak average.
        /// </summary>
        /// <value>
        /// The ratio off peak average.
        /// </value>
        public double? RATIOOffPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public double? Max { set; get; }
        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double? Min { set; get; }
        /// <summary>
        /// Gets or sets the average.
        /// </summary>
        /// <value>
        /// The average.
        /// </value>
        public double? Avg { set; get; }
        /// <summary>
        /// Gets or sets the peak average.
        /// </summary>
        /// <value>
        /// The peak average.
        /// </value>
        public double? PeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the off peak average.
        /// </summary>
        /// <value>
        /// The off peak average.
        /// </value>
        public double? OffPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the avg1 11.
        /// </summary>
        /// <value>
        /// The avg1 11.
        /// </value>
        public double? Avg1_11 { set; get; }
        /// <summary>
        /// Gets or sets the avg12 24.
        /// </summary>
        /// <value>
        /// The avg12 24.
        /// </value>
        public double? Avg12_24 { set; get; }

        public double? CongestionAvg { set; get; }
        public double? CongestionPeakAvg { set; get; }
        public double? CongestionOffPeakAvg { set; get; }
        public double? CongestionAvg1_11 { set; get; }
        public double? CongestionAvg12_24 { set; get; }

        public double? LossAvg { set; get; }
        public double? LossPeakAvg { set; get; }
        public double? LossOffPeakAvg { set; get; }
        public double? LossAvg1_11 { set; get; }
        public double? LossAvg12_24 { set; get; }

        /// <summary>
        /// Gets or sets the h e1.
        /// </summary>
        /// <value>
        /// The h e1.
        /// </value>
        public double? HE1 { set; get; }

        /// <summary>
        /// Gets or sets the h e2.
        /// </summary>
        /// <value>
        /// The h e2.
        /// </value>
        public double? HE2 { set; get; }
        /// <summary>
        /// Gets or sets the h e3.
        /// </summary>
        /// <value>
        /// The h e3.
        /// </value>
        public double? HE3 { set; get; }
        /// <summary>
        /// Gets or sets the h e4.
        /// </summary>
        /// <value>
        /// The h e4.
        /// </value>
        public double? HE4 { set; get; }
        /// <summary>
        /// Gets or sets the h e5.
        /// </summary>
        /// <value>
        /// The h e5.
        /// </value>
        public double? HE5 { set; get; }
        /// <summary>
        /// Gets or sets the h e6.
        /// </summary>
        /// <value>
        /// The h e6.
        /// </value>
        public double? HE6 { set; get; }
        /// <summary>
        /// Gets or sets the h e7.
        /// </summary>
        /// <value>
        /// The h e7.
        /// </value>
        public double? HE7 { set; get; }
        /// <summary>
        /// Gets or sets the h e8.
        /// </summary>
        /// <value>
        /// The h e8.
        /// </value>
        public double? HE8 { set; get; }
        /// <summary>
        /// Gets or sets the h e9.
        /// </summary>
        /// <value>
        /// The h e9.
        /// </value>
        public double? HE9 { set; get; }
        /// <summary>
        /// Gets or sets the h e10.
        /// </summary>
        /// <value>
        /// The h e10.
        /// </value>
        public double? HE10 { set; get; }
        /// <summary>
        /// Gets or sets the h e11.
        /// </summary>
        /// <value>
        /// The h e11.
        /// </value>
        public double? HE11 { set; get; }
        /// <summary>
        /// Gets or sets the h e12.
        /// </summary>
        /// <value>
        /// The h e12.
        /// </value>
        public double? HE12 { set; get; }
        /// <summary>
        /// Gets or sets the h e13.
        /// </summary>
        /// <value>
        /// The h e13.
        /// </value>
        public double? HE13 { set; get; }
        /// <summary>
        /// Gets or sets the h e14.
        /// </summary>
        /// <value>
        /// The h e14.
        /// </value>
        public double? HE14 { set; get; }
        /// <summary>
        /// Gets or sets the h e15.
        /// </summary>
        /// <value>
        /// The h e15.
        /// </value>
        public double? HE15 { set; get; }
        /// <summary>
        /// Gets or sets the h e16.
        /// </summary>
        /// <value>
        /// The h e16.
        /// </value>
        public double? HE16 { set; get; }
        /// <summary>
        /// Gets or sets the h e17.
        /// </summary>
        /// <value>
        /// The h e17.
        /// </value>
        public double? HE17 { set; get; }
        /// <summary>
        /// Gets or sets the h e18.
        /// </summary>
        /// <value>
        /// The h e18.
        /// </value>
        public double? HE18 { set; get; }
        /// <summary>
        /// Gets or sets the h e19.
        /// </summary>
        /// <value>
        /// The h e19.
        /// </value>
        public double? HE19 { set; get; }
        /// <summary>
        /// Gets or sets the h e20.
        /// </summary>
        /// <value>
        /// The h e20.
        /// </value>
        public double? HE20 { set; get; }
        /// <summary>
        /// Gets or sets the h e21.
        /// </summary>
        /// <value>
        /// The h e21.
        /// </value>
        public double? HE21 { set; get; }
        /// <summary>
        /// Gets or sets the h e22.
        /// </summary>
        /// <value>
        /// The h e22.
        /// </value>
        public double? HE22 { set; get; }
        /// <summary>
        /// Gets or sets the h e23.
        /// </summary>
        /// <value>
        /// The h e23.
        /// </value>
        public double? HE23 { set; get; }
        /// <summary>
        /// Gets or sets the h e24.
        /// </summary>
        /// <value>
        /// The h e24.
        /// </value>
        public double? HE24 { set; get; }
        /// <summary>
        /// Gets or sets the d a1.
        /// </summary>
        /// <value>
        /// The d a1.
        /// </value>
        public double? DA1 { set; get; }
        /// <summary>
        /// Gets or sets the d a2.
        /// </summary>
        /// <value>
        /// The d a2.
        /// </value>
        public double? DA2 { set; get; }
        /// <summary>
        /// Gets or sets the d a3.
        /// </summary>
        /// <value>
        /// The d a3.
        /// </value>
        public double? DA3 { set; get; }
        /// <summary>
        /// Gets or sets the d a4.
        /// </summary>
        /// <value>
        /// The d a4.
        /// </value>
        public double? DA4 { set; get; }
        /// <summary>
        /// Gets or sets the d a5.
        /// </summary>
        /// <value>
        /// The d a5.
        /// </value>
        public double? DA5 { set; get; }
        /// <summary>
        /// Gets or sets the d a6.
        /// </summary>
        /// <value>
        /// The d a6.
        /// </value>
        public double? DA6 { set; get; }
        /// <summary>
        /// Gets or sets the d a7.
        /// </summary>
        /// <value>
        /// The d a7.
        /// </value>
        public double? DA7 { set; get; }
        /// <summary>
        /// Gets or sets the d a8.
        /// </summary>
        /// <value>
        /// The d a8.
        /// </value>
        public double? DA8 { set; get; }
        /// <summary>
        /// Gets or sets the d a9.
        /// </summary>
        /// <value>
        /// The d a9.
        /// </value>
        public double? DA9 { set; get; }
        /// <summary>
        /// Gets or sets the d a10.
        /// </summary>
        /// <value>
        /// The d a10.
        /// </value>
        public double? DA10 { set; get; }
        /// <summary>
        /// Gets or sets the d a11.
        /// </summary>
        /// <value>
        /// The d a11.
        /// </value>
        public double? DA11 { set; get; }
        /// <summary>
        /// Gets or sets the d a12.
        /// </summary>
        /// <value>
        /// The d a12.
        /// </value>
        public double? DA12 { set; get; }
        /// <summary>
        /// Gets or sets the d a13.
        /// </summary>
        /// <value>
        /// The d a13.
        /// </value>
        public double? DA13 { set; get; }
        /// <summary>
        /// Gets or sets the d a14.
        /// </summary>
        /// <value>
        /// The d a14.
        /// </value>
        public double? DA14 { set; get; }
        /// <summary>
        /// Gets or sets the d a15.
        /// </summary>
        /// <value>
        /// The d a15.
        /// </value>
        public double? DA15 { set; get; }
        /// <summary>
        /// Gets or sets the d a16.
        /// </summary>
        /// <value>
        /// The d a16.
        /// </value>
        public double? DA16 { set; get; }
        /// <summary>
        /// Gets or sets the d a17.
        /// </summary>
        /// <value>
        /// The d a17.
        /// </value>
        public double? DA17 { set; get; }
        /// <summary>
        /// Gets or sets the d a18.
        /// </summary>
        /// <value>
        /// The d a18.
        /// </value>
        public double? DA18 { set; get; }
        /// <summary>
        /// Gets or sets the d a19.
        /// </summary>
        /// <value>
        /// The d a19.
        /// </value>
        public double? DA19 { set; get; }
        /// <summary>
        /// Gets or sets the d a20.
        /// </summary>
        /// <value>
        /// The d a20.
        /// </value>
        public double? DA20 { set; get; }
        /// <summary>
        /// Gets or sets the d a21.
        /// </summary>
        /// <value>
        /// The d a21.
        /// </value>
        public double? DA21 { set; get; }
        /// <summary>
        /// Gets or sets the d a22.
        /// </summary>
        /// <value>
        /// The d a22.
        /// </value>
        public double? DA22 { set; get; }
        /// <summary>
        /// Gets or sets the d a23.
        /// </summary>
        /// <value>
        /// The d a23.
        /// </value>
        public double? DA23 { set; get; }
        /// <summary>
        /// Gets or sets the d a24.
        /// </summary>
        /// <value>
        /// The d a24.
        /// </value>
        public double? DA24 { set; get; }
        /// <summary>
        /// Gets or sets the r t1.
        /// </summary>
        /// <value>
        /// The r t1.
        /// </value>
        public double? RT1 { set; get; }
        /// <summary>
        /// Gets or sets the r t2.
        /// </summary>
        /// <value>
        /// The r t2.
        /// </value>
        public double? RT2 { set; get; }
        /// <summary>
        /// Gets or sets the r t3.
        /// </summary>
        /// <value>
        /// The r t3.
        /// </value>
        public double? RT3 { set; get; }
        /// <summary>
        /// Gets or sets the r t4.
        /// </summary>
        /// <value>
        /// The r t4.
        /// </value>
        public double? RT4 { set; get; }
        /// <summary>
        /// Gets or sets the r t5.
        /// </summary>
        /// <value>
        /// The r t5.
        /// </value>
        public double? RT5 { set; get; }
        /// <summary>
        /// Gets or sets the r t6.
        /// </summary>
        /// <value>
        /// The r t6.
        /// </value>
        public double? RT6 { set; get; }
        /// <summary>
        /// Gets or sets the r t7.
        /// </summary>
        /// <value>
        /// The r t7.
        /// </value>
        public double? RT7 { set; get; }
        /// <summary>
        /// Gets or sets the r t8.
        /// </summary>
        /// <value>
        /// The r t8.
        /// </value>
        public double? RT8 { set; get; }
        /// <summary>
        /// Gets or sets the r t9.
        /// </summary>
        /// <value>
        /// The r t9.
        /// </value>
        public double? RT9 { set; get; }
        /// <summary>
        /// Gets or sets the r T10.
        /// </summary>
        /// <value>
        /// The r T10.
        /// </value>
        public double? RT10 { set; get; }
        /// <summary>
        /// Gets or sets the r T11.
        /// </summary>
        /// <value>
        /// The r T11.
        /// </value>
        public double? RT11 { set; get; }
        /// <summary>
        /// Gets or sets the r T12.
        /// </summary>
        /// <value>
        /// The r T12.
        /// </value>
        public double? RT12 { set; get; }
        /// <summary>
        /// Gets or sets the r T13.
        /// </summary>
        /// <value>
        /// The r T13.
        /// </value>
        public double? RT13 { set; get; }
        /// <summary>
        /// Gets or sets the r T14.
        /// </summary>
        /// <value>
        /// The r T14.
        /// </value>
        public double? RT14 { set; get; }
        /// <summary>
        /// Gets or sets the r T15.
        /// </summary>
        /// <value>
        /// The r T15.
        /// </value>
        public double? RT15 { set; get; }
        /// <summary>
        /// Gets or sets the r T16.
        /// </summary>
        /// <value>
        /// The r T16.
        /// </value>
        public double? RT16 { set; get; }
        /// <summary>
        /// Gets or sets the r T17.
        /// </summary>
        /// <value>
        /// The r T17.
        /// </value>
        public double? RT17 { set; get; }
        /// <summary>
        /// Gets or sets the r T18.
        /// </summary>
        /// <value>
        /// The r T18.
        /// </value>
        public double? RT18 { set; get; }
        /// <summary>
        /// Gets or sets the r T19.
        /// </summary>
        /// <value>
        /// The r T19.
        /// </value>
        public double? RT19 { set; get; }
        /// <summary>
        /// Gets or sets the r T20.
        /// </summary>
        /// <value>
        /// The r T20.
        /// </value>
        public double? RT20 { set; get; }
        /// <summary>
        /// Gets or sets the r T21.
        /// </summary>
        /// <value>
        /// The r T21.
        /// </value>
        public double? RT21 { set; get; }
        /// <summary>
        /// Gets or sets the r T22.
        /// </summary>
        /// <value>
        /// The r T22.
        /// </value>
        public double? RT22 { set; get; }
        /// <summary>
        /// Gets or sets the r T23.
        /// </summary>
        /// <value>
        /// The r T23.
        /// </value>
        public double? RT23 { set; get; }
        /// <summary>
        /// Gets or sets the r T24.
        /// </summary>
        /// <value>
        /// The r T24.
        /// </value>
        public double? RT24 { set; get; }
        /// <summary>
        /// Gets or sets the rati o1.
        /// </summary>
        /// <value>
        /// The rati o1.
        /// </value>
        public double? RATIO1 { set; get; }
        /// <summary>
        /// Gets or sets the rati o2.
        /// </summary>
        /// <value>
        /// The rati o2.
        /// </value>
        public double? RATIO2 { set; get; }
        /// <summary>
        /// Gets or sets the rati o3.
        /// </summary>
        /// <value>
        /// The rati o3.
        /// </value>
        public double? RATIO3 { set; get; }
        /// <summary>
        /// Gets or sets the rati o4.
        /// </summary>
        /// <value>
        /// The rati o4.
        /// </value>
        public double? RATIO4 { set; get; }
        /// <summary>
        /// Gets or sets the rati o5.
        /// </summary>
        /// <value>
        /// The rati o5.
        /// </value>
        public double? RATIO5 { set; get; }
        /// <summary>
        /// Gets or sets the rati o6.
        /// </summary>
        /// <value>
        /// The rati o6.
        /// </value>
        public double? RATIO6 { set; get; }
        /// <summary>
        /// Gets or sets the rati o7.
        /// </summary>
        /// <value>
        /// The rati o7.
        /// </value>
        public double? RATIO7 { set; get; }
        /// <summary>
        /// Gets or sets the rati o8.
        /// </summary>
        /// <value>
        /// The rati o8.
        /// </value>
        public double? RATIO8 { set; get; }
        /// <summary>
        /// Gets or sets the rati o9.
        /// </summary>
        /// <value>
        /// The rati o9.
        /// </value>
        public double? RATIO9 { set; get; }
        /// <summary>
        /// Gets or sets the rati o10.
        /// </summary>
        /// <value>
        /// The rati o10.
        /// </value>
        public double? RATIO10 { set; get; }
        /// <summary>
        /// Gets or sets the rati o11.
        /// </summary>
        /// <value>
        /// The rati o11.
        /// </value>
        public double? RATIO11 { set; get; }
        /// <summary>
        /// Gets or sets the rati o12.
        /// </summary>
        /// <value>
        /// The rati o12.
        /// </value>
        public double? RATIO12 { set; get; }
        /// <summary>
        /// Gets or sets the rati o13.
        /// </summary>
        /// <value>
        /// The rati o13.
        /// </value>
        public double? RATIO13 { set; get; }
        /// <summary>
        /// Gets or sets the rati o14.
        /// </summary>
        /// <value>
        /// The rati o14.
        /// </value>
        public double? RATIO14 { set; get; }
        /// <summary>
        /// Gets or sets the rati o15.
        /// </summary>
        /// <value>
        /// The rati o15.
        /// </value>
        public double? RATIO15 { set; get; }
        /// <summary>
        /// Gets or sets the rati o16.
        /// </summary>
        /// <value>
        /// The rati o16.
        /// </value>
        public double? RATIO16 { set; get; }
        /// <summary>
        /// Gets or sets the rati o17.
        /// </summary>
        /// <value>
        /// The rati o17.
        /// </value>
        public double? RATIO17 { set; get; }
        /// <summary>
        /// Gets or sets the rati o18.
        /// </summary>
        /// <value>
        /// The rati o18.
        /// </value>
        public double? RATIO18 { set; get; }
        /// <summary>
        /// Gets or sets the rati o19.
        /// </summary>
        /// <value>
        /// The rati o19.
        /// </value>
        public double? RATIO19 { set; get; }
        /// <summary>
        /// Gets or sets the rati o20.
        /// </summary>
        /// <value>
        /// The rati o20.
        /// </value>
        public double? RATIO20 { set; get; }
        /// <summary>
        /// Gets or sets the rati o21.
        /// </summary>
        /// <value>
        /// The rati o21.
        /// </value>
        public double? RATIO21 { set; get; }
        /// <summary>
        /// Gets or sets the rati o22.
        /// </summary>
        /// <value>
        /// The rati o22.
        /// </value>
        public double? RATIO22 { set; get; }
        /// <summary>
        /// Gets or sets the rati o23.
        /// </summary>
        /// <value>
        /// The rati o23.
        /// </value>
        public double? RATIO23 { set; get; }
        /// <summary>
        /// Gets or sets the rati o24.
        /// </summary>
        /// <value>
        /// The rati o24.
        /// </value>
        public double? RATIO24 { set; get; }

        public decimal SRTMin { get; set; }
        public decimal SRTMax { get; set; }
        public DateTime PathMinRT { get; set; }

        public decimal RTMaxWinter { get; set; }
        public decimal RTMinWinter { get; set; }
        public decimal RTMaxSpring { get; set; }
        public decimal RTMinSpring { get; set; }
        public decimal RTMaxSummer { get; set; }
        public decimal RTMinSummer { get; set; }
        public decimal RTMaxFall { get; set; }
        public decimal RTMinFall { get; set; }
        public decimal RTMaxAll { get; set; }
        public decimal RTMinAll { get; set; }
    }

    public class SeasonData
    {
        public string SourceName { get; set; }
        public string SinkName { get; set; }
        public long SourceNodekey { get; set; }
        public long SinkNodekey { get; set; }
        public decimal RTMaxWinter { get; set; }
        public decimal RTMinWinter { get; set; }
        public decimal RTMaxSpring { get; set; }
        public decimal RTMinSpring { get; set; }
        public decimal RTMaxSummer { get; set; }
        public decimal RTMinSummer { get; set; }
        public decimal RTMaxFall { get; set; }
        public decimal RTMinFall { get; set; }
        public decimal RTMaxAll { get; set; }
        public decimal RTMinAll { get; set; }
        public DateTime PathMinRT { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class NodeDisplayData
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { set; get; }
        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        public string NodeType { set; get; }
        /// <summary>
        /// Gets or sets the name of the zone.
        /// </summary>
        /// <value>
        /// The name of the zone.
        /// </value>
        public string ZoneName { set; get; }
        /// <summary>
        /// Gets or sets the name of the sink.
        /// </summary>
        /// <value>
        /// The name of the sink.
        /// </value>
        public string SinkName { set; get; }
        /// <summary>
        /// Gets or sets the type of the sink.
        /// </summary>
        /// <value>
        /// The type of the sink.
        /// </value>
        public string SinkType { set; get; }
        /// <summary>
        /// Gets or sets the sink zone.
        /// </summary>
        /// <value>
        /// The sink zone.
        /// </value>
        public string SinkZone { set; get; }
        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public double? Max { set; get; }
        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double? Min { set; get; }
        /// <summary>
        /// Gets or sets the average.
        /// </summary>
        /// <value>
        /// The average.
        /// </value>
        public double? Avg { set; get; }
        /// <summary>
        /// Gets or sets the peak average.
        /// </summary>
        /// <value>
        /// The peak average.
        /// </value>
        public double? PeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the off peak average.
        /// </summary>
        /// <value>
        /// The off peak average.
        /// </value>
        public double? OffPeakAvg { set; get; }
        /// <summary>
        /// Gets or sets the h e1.
        /// </summary>
        /// <value>
        /// The h e1.
        /// </value>
        public double? HE1 { set; get; }
        /// <summary>
        /// Gets or sets the h e2.
        /// </summary>
        /// <value>
        /// The h e2.
        /// </value>
        public double? HE2 { set; get; }
        /// <summary>
        /// Gets or sets the h e3.
        /// </summary>
        /// <value>
        /// The h e3.
        /// </value>
        public double? HE3 { set; get; }
        /// <summary>
        /// Gets or sets the h e4.
        /// </summary>
        /// <value>
        /// The h e4.
        /// </value>
        public double? HE4 { set; get; }
        /// <summary>
        /// Gets or sets the h e5.
        /// </summary>
        /// <value>
        /// The h e5.
        /// </value>
        public double? HE5 { set; get; }
        /// <summary>
        /// Gets or sets the h e6.
        /// </summary>
        /// <value>
        /// The h e6.
        /// </value>
        public double? HE6 { set; get; }
        /// <summary>
        /// Gets or sets the h e7.
        /// </summary>
        /// <value>
        /// The h e7.
        /// </value>
        public double? HE7 { set; get; }
        /// <summary>
        /// Gets or sets the h e8.
        /// </summary>
        /// <value>
        /// The h e8.
        /// </value>
        public double? HE8 { set; get; }
        /// <summary>
        /// Gets or sets the h e9.
        /// </summary>
        /// <value>
        /// The h e9.
        /// </value>
        public double? HE9 { set; get; }
        /// <summary>
        /// Gets or sets the h e10.
        /// </summary>
        /// <value>
        /// The h e10.
        /// </value>
        public double? HE10 { set; get; }
        /// <summary>
        /// Gets or sets the h e11.
        /// </summary>
        /// <value>
        /// The h e11.
        /// </value>
        public double? HE11 { set; get; }
        /// <summary>
        /// Gets or sets the h e12.
        /// </summary>
        /// <value>
        /// The h e12.
        /// </value>
        public double? HE12 { set; get; }
        /// <summary>
        /// Gets or sets the h e13.
        /// </summary>
        /// <value>
        /// The h e13.
        /// </value>
        public double? HE13 { set; get; }
        /// <summary>
        /// Gets or sets the h e14.
        /// </summary>
        /// <value>
        /// The h e14.
        /// </value>
        public double? HE14 { set; get; }
        /// <summary>
        /// Gets or sets the h e15.
        /// </summary>
        /// <value>
        /// The h e15.
        /// </value>
        public double? HE15 { set; get; }
        /// <summary>
        /// Gets or sets the h e16.
        /// </summary>
        /// <value>
        /// The h e16.
        /// </value>
        public double? HE16 { set; get; }
        /// <summary>
        /// Gets or sets the h e17.
        /// </summary>
        /// <value>
        /// The h e17.
        /// </value>
        public double? HE17 { set; get; }
        /// <summary>
        /// Gets or sets the h e18.
        /// </summary>
        /// <value>
        /// The h e18.
        /// </value>
        public double? HE18 { set; get; }
        /// <summary>
        /// Gets or sets the h e19.
        /// </summary>
        /// <value>
        /// The h e19.
        /// </value>
        public double? HE19 { set; get; }
        /// <summary>
        /// Gets or sets the h e20.
        /// </summary>
        /// <value>
        /// The h e20.
        /// </value>
        public double? HE20 { set; get; }
        /// <summary>
        /// Gets or sets the h e21.
        /// </summary>
        /// <value>
        /// The h e21.
        /// </value>
        public double? HE21 { set; get; }
        /// <summary>
        /// Gets or sets the h e22.
        /// </summary>
        /// <value>
        /// The h e22.
        /// </value>
        public double? HE22 { set; get; }
        /// <summary>
        /// Gets or sets the h e23.
        /// </summary>
        /// <value>
        /// The h e23.
        /// </value>
        public double? HE23 { set; get; }
        /// <summary>
        /// Gets or sets the h e24.
        /// </summary>
        /// <value>
        /// The h e24.
        /// </value>
        public double? HE24 { set; get; }

        public decimal? SRTMin { set; get; }
        public decimal? SRTMax { set; get; }
        public DateTime? PathMinRT { set; get; }

    }
    /// <summary>
    /// 
    /// </summary>
    public class NodeHourLMP
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { set; get; }
        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        public string NodeType { set; get; }
        /// <summary>
        /// Gets or sets the name of the zone.
        /// </summary>
        /// <value>
        /// The name of the zone.
        /// </value>
        public string ZoneName { set; get; }
        /// <summary>
        /// Gets or sets the name of the sink.
        /// </summary>
        /// <value>
        /// The name of the sink.
        /// </value>
        public string SinkName { set; get; }
        /// <summary>
        /// Gets or sets the type of the sink.
        /// </summary>
        /// <value>
        /// The type of the sink.
        /// </value>
        public string SinkType { set; get; }
        /// <summary>
        /// Gets or sets the sink zone.
        /// </summary>
        /// <value>
        /// The sink zone.
        /// </value>
        public string SinkZone { set; get; }
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public int Hour { get; set; }
        /// <summary>
        /// Gets or sets the da.
        /// </summary>
        /// <value>
        /// The da.
        /// </value>
        public double? DA { set; get; }
        /// <summary>
        /// Gets or sets the rt.
        /// </summary>
        /// <value>
        /// The rt.
        /// </value>
        public double? RT { set; get; }
        /// <summary>
        /// Gets or sets the dart.
        /// </summary>
        /// <value>
        /// The dart.
        /// </value>
        public double? DART { set; get; }
        /// <summary>
        /// Gets or sets the ratio.
        /// </summary>
        /// <value>
        /// The ratio.
        /// </value>
        public double? RATIO { set; get; }

        public double? Congestion { set; get; }

        public double? Loss { set; get; }

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
            NodeData node = (NodeData)values[1];
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
            if ((string)cell.Column.Header == "Peak Avg")
            {
                if (node.PeakAvg != null)
                {
                    number = (double)node.PeakAvg;
                }
            }
            if ((string)cell.Column.Header == "Off Peak Avg")
            {
                if (node.OffPeakAvg != null)
                {
                    number = (double)node.OffPeakAvg;
                }
            }
            if ((string)cell.Column.Header == "1")
            {
                if (node.HE1 != null)
                {
                    number = (double)node.HE1;
                }
            }
            if ((string)cell.Column.Header == "2")
            {
                if (node.HE2 != null)
                {
                    number = (double)node.HE2;
                }
            }
            if ((string)cell.Column.Header == "3")
            {
                if (node.HE3 != null)
                {
                    number = (double)node.HE3;
                }
            }
            if ((string)cell.Column.Header == "4")
            {
                if (node.HE4 != null)
                {
                    number = (double)node.HE4;
                }
            }
            if ((string)cell.Column.Header == "5")
            {
                if (node.HE5 != null)
                {
                    number = (double)node.HE5;
                }
            }
            if ((string)cell.Column.Header == "6")
            {
                if (node.HE6 != null)
                {
                    number = (double)node.HE6;
                }
            }
            if ((string)cell.Column.Header == "7")
            {
                if (node.HE7 != null)
                {
                    number = (double)node.HE7;
                }
            }
            if ((string)cell.Column.Header == "8")
            {
                if (node.HE8 != null)
                {
                    number = (double)node.HE8;
                }
            }
            if ((string)cell.Column.Header == "9")
            {
                if (node.HE9 != null)
                {
                    number = (double)node.HE9;
                }
            }
            if ((string)cell.Column.Header == "10")
            {
                if (node.HE10 != null)
                {
                    number = (double)node.HE10;
                }
            }
            if ((string)cell.Column.Header == "11")
            {
                if (node.HE11 != null)
                {
                    number = (double)node.HE11;
                }
            }
            if ((string)cell.Column.Header == "12")
            {
                if (node.HE12 != null)
                {
                    number = (double)node.HE12;
                }
            }
            if ((string)cell.Column.Header == "13")
            {
                if (node.HE13 != null)
                {
                    number = (double)node.HE13;
                }
            }
            if ((string)cell.Column.Header == "14")
            {
                if (node.HE14 != null)
                {
                    number = (double)node.HE14;
                }
            }
            if ((string)cell.Column.Header == "15")
            {
                if (node.HE15 != null)
                {
                    number = (double)node.HE15;
                }
            }
            if ((string)cell.Column.Header == "16")
            {
                if (node.HE16 != null)
                {
                    number = (double)node.HE16;
                }
            }
            if ((string)cell.Column.Header == "17")
            {
                if (node.HE17 != null)
                {
                    number = (double)node.HE17;
                }
            }
            if ((string)cell.Column.Header == "18")
            {
                if (node.HE18 != null)
                {
                    number = (double)node.HE18;
                }
            }
            if ((string)cell.Column.Header == "19")
            {
                if (node.HE19 != null)
                {
                    number = (double)node.HE19;
                }
            }
            if ((string)cell.Column.Header == "20")
            {
                if (node.HE20 != null)
                {
                    number = (double)node.HE20;
                }
            }
            if ((string)cell.Column.Header == "21")
            {
                if (node.HE21 != null)
                {
                    number = (double)node.HE21;
                }
            }
            if ((string)cell.Column.Header == "22")
            {
                if (node.HE22 != null)
                {
                    number = (double)node.HE22;
                }
            }
            if ((string)cell.Column.Header == "23")
            {
                if (node.HE23 != null)
                {
                    number = (double)node.HE23;
                }
            }
            if ((string)cell.Column.Header == "24")
            {
                if (node.HE24 != null)
                {
                    number = (double)node.HE24;
                }
            }
            if ((string)cell.Column.Header == "Price")
            {

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
            var cell = (DataGridCell)values[0];
            Int32 headerTest = -1;
            Int32.TryParse((string)cell.Column.Header, out headerTest);
            if (headerTest > 0)
            {
                if (MainWindow.sMarketSelected == 2 && MainWindow.sIsDST)
                {
                    if ((string)cell.Column.Header == "1" || (string)cell.Column.Header == "2" || (string)cell.Column.Header == "3" ||
                       (string)cell.Column.Header == "4" || (string)cell.Column.Header == "5" || (string)cell.Column.Header == "6" ||
                       (string)cell.Column.Header == "23" || (string)cell.Column.Header == "24")
                    {
                        mybrush = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        mybrush = new SolidColorBrush(Colors.LightGoldenrodYellow);
                    }
                }

                else if (MainWindow.sMarketSelected == 1 || (MainWindow.sMarketSelected == 2 && !MainWindow.sIsDST))
                {
                    if ((string)cell.Column.Header == "1" || (string)cell.Column.Header == "2" || (string)cell.Column.Header == "3" ||
                        (string)cell.Column.Header == "4" || (string)cell.Column.Header == "5" || (string)cell.Column.Header == "6" ||
                        (string)cell.Column.Header == "7" || (string)cell.Column.Header == "24")
                    {
                        mybrush = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        mybrush = new SolidColorBrush(Colors.LightGoldenrodYellow);
                    }
                }
                else
                {
                    if ((string)cell.Column.Header == "1" || (string)cell.Column.Header == "2" || (string)cell.Column.Header == "3" ||
                        (string)cell.Column.Header == "4" || (string)cell.Column.Header == "5" || (string)cell.Column.Header == "6" ||
                        (string)cell.Column.Header == "23" || (string)cell.Column.Header == "24")
                    {
                        mybrush = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        mybrush = new SolidColorBrush(Colors.LightGoldenrodYellow);
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
}
