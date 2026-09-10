using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.MarketNodePriceLib;
using Vayu.NodePriceGraph.Model;
using Vayu.NodePriceLibrary;

namespace Vayu.NodePriceGraph.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        #region Declaration

        /// <summary>
        /// The nodal profile view model
        /// </summary>
        private static Vayu.LMPStatistics.ViewModels.MainWindowViewModel nodalProfileViewModel = new Vayu.LMPStatistics.ViewModels.MainWindowViewModel(new Vayu.LMPStatistics.Model.DataService());
        /// <summary>
        /// The nodal profile window
        /// </summary>
        private static Vayu.LMPStatistics.Views.MainWindow nodalProfileWindow = new Vayu.LMPStatistics.Views.MainWindow();

        /// <summary>
        /// The start date
        /// </summary>
        private DateTime? startDate;
        /// <summary>
        /// The end date
        /// </summary>
        private DateTime? endDate;
        /// <summary>
        /// The energy
        /// </summary>
        const string Energy = "Energy";
        /// <summary>
        /// The m is swap
        /// </summary>
        private bool mIsSwap = false;
        /// <summary>
        /// The m rt hash
        /// </summary>
        private Dictionary<int, Node> mRTHash = new Dictionary<int, Node>();
        /// <summary>
        /// The m da hash
        /// </summary>
        private Dictionary<int, Node> mDAHash = new Dictionary<int, Node>();
        /// <summary>
        /// The m fill source sink hash
        /// </summary>
        private Dictionary<string, SourceSinkData> mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
        /// <summary>
        /// The m filled LMP list
        /// </summary>
        private List<LMP> mFilledLMPList = new List<LMP>();
        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m price node list
        /// </summary>
        private List<PricingNode> mPriceNodeList = new List<PricingNode>();
        /// <summary>
        /// The m source sink data list
        /// </summary>
        private List<SourceSinkData> mSourceSinkDataList = new List<SourceSinkData>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the run fetch data and update chart command.
        /// </summary>
        /// <value>
        /// The run fetch data and update chart command.
        /// </value>
        public DelegateCommand RunFetchDataAndUpdateChartCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add source sink command.
        /// </summary>
        /// <value>
        /// The add source sink command.
        /// </value>
        public DelegateCommand AddSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run today date command.
        /// </summary>
        /// <value>
        /// The run today date command.
        /// </value>
        public DelegateCommand RunTodayDateCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove source sink command.
        /// </summary>
        /// <value>
        /// The remove source sink command.
        /// </value>
        public DelegateCommand RemoveSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove all source sink command.
        /// </summary>
        /// <value>
        /// The remove all source sink command.
        /// </value>
        public DelegateCommand RemoveAllSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the swap command.
        /// </summary>
        /// <value>
        /// The swap command.
        /// </value>
        public DelegateCommand SwapCommand { private set; get; }
        /// <summary>
        /// Gets or sets the LMP command.
        /// </summary>
        /// <value>
        /// The LMP command.
        /// </value>
        public DelegateCommand LmpCommand { private set; get; }

        /// <summary>
        /// The m source sink data selected
        /// </summary>
        private SourceSinkData mSourceSinkDataSelected;
        /// <summary>
        /// Gets or sets the source sink data selected.
        /// </summary>
        /// <value>
        /// The source sink data selected.
        /// </value>
        public SourceSinkData SourceSinkDataSelected
        {
            get
            {
                return mSourceSinkDataSelected;
            }
            set
            {
                if (mSourceSinkDataSelected == null || !mSourceSinkDataSelected.Equals(value))
                {
                    mSourceSinkDataSelected = value;
                    FetchDataAndUpdateChartCommand();
                    RaisePropertyChanged("SourceSinkDataSelected");
                }
            }
        }
        /// <summary>
        /// The m source sink list
        /// </summary>
        private List<SourceSinkData> mSourceSinkList;
        /// <summary>
        /// Gets or sets the source sink list.
        /// </summary>
        /// <value>
        /// The source sink list.
        /// </value>
        public List<SourceSinkData> SourceSinkList
        {
            get
            {
                return mSourceSinkList;
            }
            set
            {
                mSourceSinkList = value;
                RaisePropertyChanged("SourceSinkList");
            }
        }
        /// <summary>
        /// The m uptos checked
        /// </summary>
        private bool mUptosChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [uptos checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [uptos checked]; otherwise, <c>false</c>.
        /// </value>
        public bool UptosChecked
        {
            get
            {
                return mUptosChecked;
            }
            set
            {
                mUptosChecked = value;
                if (mUptosChecked)
                {
                    IsSinkEnabled = true;
                }
                else
                {
                    IsSinkEnabled = false;
                }
                StackTrace stackTrace = new StackTrace();
                MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                if (methodBase.Name != "SetValuesFromPortfolio")
                {
                    SetSourceSink();
                }
                RaisePropertyChanged("UptosChecked");
            }
        }
        /// <summary>
        /// The m is uptos enabled
        /// </summary>
        private bool mIsUptosEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is uptos enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is uptos enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsUptosEnabled
        {
            get
            {
                return mIsUptosEnabled;
            }
            set
            {
                mIsUptosEnabled = value;
                RaisePropertyChanged("IsUptosEnabled");
            }
        }

        /// <summary>
        /// The m LMP plot model
        /// </summary>
        private PlotModel mLmpPlotModel;
        /// <summary>
        /// Gets or sets the LMP plot model.
        /// </summary>
        /// <value>
        /// The LMP plot model.
        /// </value>
        public PlotModel LmpPlotModel
        {
            get
            {
                return mLmpPlotModel;
            }
            set
            {
                mLmpPlotModel = value;
                RaisePropertyChanged("LmpPlotModel");
            }
        }
        /// <summary>
        /// The m source node list
        /// </summary>
        private List<PricingNode> mSourceNodeList;
        /// <summary>
        /// Gets or sets the source node list.
        /// </summary>
        /// <value>
        /// The source node list.
        /// </value>
        public List<PricingNode> SourceNodeList
        {
            get
            {
                return mSourceNodeList;
            }
            set
            {
                mSourceNodeList = value;
                RaisePropertyChanged("SourceNodeList");
            }
        }
        /// <summary>
        /// The m sink node list
        /// </summary>
        private List<PricingNode> mSinkNodeList;
        /// <summary>
        /// Gets or sets the sink node list.
        /// </summary>
        /// <value>
        /// The sink node list.
        /// </value>
        public List<PricingNode> SinkNodeList
        {
            get
            {
                return mSinkNodeList;
            }
            set
            {
                mSinkNodeList = value;
                RaisePropertyChanged("SinkNodeList");
            }
        }
        /// <summary>
        /// The m LMP list
        /// </summary>
        private List<LMP> mLMPList;
        /// <summary>
        /// Gets or sets the LMP list.
        /// </summary>
        /// <value>
        /// The LMP list.
        /// </value>
        public List<LMP> LMPList
        {
            get
            {
                return mLMPList;
            }
            set
            {
                mLMPList = value;
                RaisePropertyChanged("LMPList");
            }
        }
        private string _DaysSelectedTxtValue;

        public string DaysSelectedTxtValue
        {
            get { return _DaysSelectedTxtValue; }
            set
            {
                _DaysSelectedTxtValue = value;
                RaisePropertyChanged("DaysSelectedTxtValue");
            }
        }
        /// <summary>
        /// The m LMP type selected value
        /// </summary>
        private string mLMPTypeSelectedValue;
        /// <summary>
        /// Gets or sets the LMP type selected value.
        /// </summary>
        /// <value>
        /// The LMP type selected value.
        /// </value>
        public string LMPTypeSelectedValue
        {
            get
            {
                return mLMPTypeSelectedValue;
            }
            set
            {
                mLMPTypeSelectedValue = value;
                RaisePropertyChanged("LMPTypeSelectedValue");
                RefreshChart();
            }
        }
        /// <summary>
        /// The m LMP component selected value
        /// </summary>
        private string mLMPComponentSelectedValue;
        /// <summary>
        /// Gets or sets the LMP component selected value.
        /// </summary>
        /// <value>
        /// The LMP component selected value.
        /// </value>
        public string LMPComponentSelectedValue
        {
            get
            {
                return mLMPComponentSelectedValue;
            }
            set
            {
                mLMPComponentSelectedValue = value;
                RaisePropertyChanged("LMPComponentSelectedValue");
                RefreshChart();
            }
        }
        /// <summary>
        /// The m source node data selected
        /// </summary>
        private PricingNode mSourceNodeDataSelected;
        /// <summary>
        /// Gets or sets the source node data selected.
        /// </summary>
        /// <value>
        /// The source node data selected.
        /// </value>
        public PricingNode SourceNodeDataSelected
        {
            get
            {
                return mSourceNodeDataSelected;
            }
            set
            {
                mSourceNodeDataSelected = value;
                RaisePropertyChanged("NodeDataSelected");
            }
        }
        /// <summary>
        /// The m LMP component list
        /// </summary>
        private List<String> mLMPComponentList;
        /// <summary>
        /// Gets or sets the LMP component list.
        /// </summary>
        /// <value>
        /// The LMP component list.
        /// </value>
        public List<String> LMPComponentList
        {
            get
            {
                return mLMPComponentList;
            }
            set
            {
                mLMPComponentList = value;
                RaisePropertyChanged("LMPComponentList");
            }
        }
        /// <summary>
        /// The m LMP type list
        /// </summary>
        private List<String> mLMPTypeList;
        /// <summary>
        /// Gets or sets the LMP type list.
        /// </summary>
        /// <value>
        /// The LMP type list.
        /// </value>
        public List<String> LMPTypeList
        {
            get
            {
                return mLMPTypeList;
            }
            set
            {
                mLMPTypeList = value;
                RaisePropertyChanged("LMPTypeList");
            }
        }
        /// <summary>
        /// The m iso market list
        /// </summary>
        private List<String> mISOMarketList;
        /// <summary>
        /// Gets or sets the iso market list.
        /// </summary>
        /// <value>
        /// The iso market list.
        /// </value>
        public List<String> ISOMarketList
        {
            get
            {
                return mISOMarketList;
            }
            set
            {
                mISOMarketList = value;
                //SetSourceSink();
                RaisePropertyChanged("ISOMarketList");
            }
        }
        /// <summary>
        /// The m from date
        /// </summary>
        private DateTime mFromDate;
        /// <summary>
        /// Gets or sets from date.
        /// </summary>
        /// <value>
        /// From date.
        /// </value>
        public DateTime FromDate
        {
            get
            {
                return mFromDate;
            }
            set
            {
                mFromDate = value;
                RaisePropertyChanged("FromDate");
                FetchDataAndUpdateChartCommand();
            }
        }
        /// <summary>
        /// The market combo selected value
        /// </summary>
        private string marketComboSelectedValue;
        /// <summary>
        /// Gets or sets the market combo selected value.
        /// </summary>
        /// <value>
        /// The market combo selected value.
        /// </value>
        public string MarketComboSelectedValue
        {
            get
            {
                return marketComboSelectedValue;
            }
            set
            {
                marketComboSelectedValue = value;
                //StackTrace stackTrace = new StackTrace();
                //MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                //if (methodBase.Name != "SetValuesFromPortfolio")
                //{
                //if ( marketComboSelectedValue.Equals("ERCOT"))
                //{
                //    UptosChecked = false;
                //    IsUptosEnabled = false;
                //}
                //else
                {
                    IsUptosEnabled = true;
                }
                RemoveAllSourceSink();
                SetSourceSink();
                // }
                RaisePropertyChanged("MarketComboSelectedValue");
            }
        }
        /// <summary>
        /// The m is sink enabled
        /// </summary>
        private bool mIsSinkEnabled;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is sink enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is sink enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsSinkEnabled
        {
            get
            {
                return mIsSinkEnabled;
            }
            set
            {
                mIsSinkEnabled = value;
                //SetSourceSink();
                RaisePropertyChanged("IsSinkEnabled");
            }
        }
        /// <summary>
        /// The m source combo selected item
        /// </summary>
        private PricingNode mSourceComboSelectedItem;
        /// <summary>
        /// Gets or sets the source combo selected item.
        /// </summary>
        /// <value>
        /// The source combo selected item.
        /// </value>
        public PricingNode SourceComboSelectedItem
        {
            get
            {
                return mSourceComboSelectedItem;
            }
            set
            {
                mSourceComboSelectedItem = value;
                RaisePropertyChanged("SourceComboSelectedItem");
            }
        }
        /// <summary>
        /// The m sink combo selected item
        /// </summary>
        private PricingNode mSinkComboSelectedItem;
        /// <summary>
        /// Gets or sets the sink combo selected item.
        /// </summary>
        /// <value>
        /// The sink combo selected item.
        /// </value>
        public PricingNode SinkComboSelectedItem
        {
            get
            {
                return mSinkComboSelectedItem;
            }
            set
            {
                mSinkComboSelectedItem = value;
                RaisePropertyChanged("SinkComboSelectedItem");
            }
        }

        /// <summary>
        /// The node detail list
        /// </summary>
        private NodeDetailList nodeDetailList;
        /// <summary>
        /// Gets or sets the detail list.
        /// </summary>
        /// <value>
        /// The detail list.
        /// </value>
        public NodeDetailList DetailList
        {
            get { return nodeDetailList; }
            set { nodeDetailList = value; RaisePropertyChanged("DetailList"); }
        }

        #endregion

        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            mDataService.loadDBCommands();

            ISOMarketList = new List<string> { "ERCOT" };
            MarketComboSelectedValue = "ERCOT";
            LMPTypeList = new List<string> { "RT & DA", "RT", "DA", "RT-DA", "DA-RT" };
            LMPComponentList = new List<string> { "LMP", "CONGESTION", "LOSS", Energy };
            FromDate = DateTime.UtcNow.Date;

            LMPTypeSelectedValue = "RT & DA";
            LMPComponentSelectedValue = "LMP";
            DaysSelectedTxtValue = "1";
            RunFetchDataAndUpdateChartCommand = new DelegateCommand(FetchDataAndUpdateChartCommand);
            RemoveSourceSinkCommand = new DelegateCommand(RemoveSourceSink);
            RemoveAllSourceSinkCommand = new DelegateCommand(RemoveAllSourceSink);
            AddSourceSinkCommand = new DelegateCommand(AddSourceSink);
            RunTodayDateCommand = new DelegateCommand(TodayDate);
            SwapCommand = new DelegateCommand(Swap);
            LmpCommand = new DelegateCommand(Lmp);
        }
        #region Public Methods

        /// <summary>
        /// Adds the dates.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public void AddDates(DateTime start, DateTime end)
        {
            startDate = start;
            endDate = end;
            TimeSpan span = end - start;
            FromDate = start;
            DaysSelectedTxtValue = (span.Days + 1).ToString();
        }
        /// <summary>
        /// Adds the nodes.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        public void AddNodes(List<SourceSinkData> sourceSinkList)
        {
            SourceSinkList = null;
            foreach (SourceSinkData sourceSinkData in sourceSinkList)
            {
                string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                                sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                }
            }
            SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
            FetchDataAndUpdateChartCommand();
        }
        /// <summary>
        /// Sets the source sink.
        /// </summary>
        public void SetSourceSink()
        {
            string product = UptosChecked ? "UPTO" : "CRR";
            DBAccess.GetSourceSinkNodeList(
                    (item1, error) =>
                    {
                        SourceNodeList = item1.Item1;
                        SinkNodeList = item1.Item2;
                    }, MarketComboSelectedValue, product);
        }

        /// <summary>
        /// Todays the date.
        /// </summary>
        public void TodayDate()
        {
            //FromDate = DateTime.UtcNow.Date;
            FromDate = DateTime.Today;
        }
        /// <summary>
        /// Fetches the data and update chart command.
        /// </summary>
        public void FetchDataAndUpdateChartCommand()
        {
            LmpPlotModel = null;
            FetchData();
            RefreshChart();
        }

        /// <summary>
        /// Refreshes the chart.
        /// </summary>
        public void RefreshChart()
        {
            LmpPlotModel = CreatePlotModel();
        }
        /// <summary>
        /// Sets the market.
        /// </summary>
        /// <param name="market">The market.</param>
        public void SetMarket(string market)
        {
            MarketComboSelectedValue = market;
        }
        /// <summary>
        /// Adds the source sink.
        /// </summary>
        public void AddSourceSink()
        {
            if (SourceComboSelectedItem != null && (!UptosChecked || SinkComboSelectedItem != null))
            {
                int key = SourceComboSelectedItem.MarketKey;

                SourceSinkData sourceSinkData = new SourceSinkData();
                sourceSinkData.Source = SourceComboSelectedItem;
                sourceSinkData.Sink = !UptosChecked ? null : SinkComboSelectedItem;
                string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                            sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                    SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                }
                SourceComboSelectedItem = null;
                SinkComboSelectedItem = null;
            }
            FetchDataAndUpdateChartCommand();
        }

        /// <summary>
        /// Removes the source sink.
        /// </summary>
        public void RemoveSourceSink()
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            string sourceSinkKey = "";
            if (sourceSinkData == null)
            {
                return;
            }
            if (sourceSinkData.Sink == null)
            {
                sourceSinkKey = sourceSinkData.Source.NodeKey.ToString();
            }
            else
            {
                sourceSinkKey = sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
            }
            if (mFillSourceSinkHash.ContainsKey(sourceSinkKey))
            {
                mFillSourceSinkHash.Remove(sourceSinkKey);
            }
            SourceSinkList = null;
            SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
        }

        /// <summary>
        /// Removes all source sink.
        /// </summary>
        public void RemoveAllSourceSink()
        {
            mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
            SourceSinkList = null;
            ResetGrids();
        }

        /// <summary>
        /// Sets the source sink list from portfolio.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        public void SetSourceSinkListFromPortfolio(List<SourceSinkData> sourceSinkList)
        {
            try
            {
                foreach (SourceSinkData sourceSink in sourceSinkList)
                {
                    if (sourceSink.Sink == null)
                    {
                        if (!mSourceSinkDataList.Exists(compareSourcesSink => (compareSourcesSink.Source.NodeKey == sourceSink.Source.NodeKey)))
                        {
                            mSourceSinkDataList.Add(sourceSink);
                        }
                    }
                    else
                    {
                        if (!mSourceSinkDataList.Exists(compareSourceSink => (compareSourceSink.Source.NodeKey == sourceSink.Source.NodeKey && compareSourceSink.Sink.NodeKey == sourceSink.Sink.NodeKey)))
                        {
                            mSourceSinkDataList.Add(sourceSink);
                        }
                    }
                }
                SourceSinkList = null;
                SourceSinkList = mSourceSinkDataList;
                SourceSinkDataSelected = SourceSinkList[mSourceSinkDataList.Count - 1];
            }
            catch
            {
            }

        }

        /// <summary>
        /// Sets the values from portfolio.
        /// </summary>
        /// <param name="values">The values.</param>
        public void SetValuesFromPortfolio(List<string> values)
        {
            try
            {
                UptosChecked = bool.Parse(values[1]);
                MarketComboSelectedValue = ISOMarketList.Where(x => x.ToUpper().Contains(values[0].ToString().ToUpper())).FirstOrDefault();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Swaps this instance.
        /// </summary>
        public void Swap()
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            if (sourceSinkData == null)
            {
                return;
            }
            if (SourceSinkDataSelected.Sink != null)
            {
                mIsSwap = true;
                PricingNode source = sourceSinkData.Source;
                PricingNode sink = sourceSinkData.Sink;
                RemoveSourceSink();
                SourceComboSelectedItem = sink;
                SinkComboSelectedItem = source;
                AddSourceSink();
                FetchDataAndUpdateChartCommand();
                mIsSwap = false;
            }
            else
            {
                return;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// LMPs this instance.
        /// </summary>
        private void Lmp()
        {

            nodalProfileWindow.DataContext = nodalProfileViewModel;
            if (SourceSinkList != null)
            {
                nodalProfileViewModel.SetSourceSinkList(SourceSinkList);
                nodalProfileWindow.Show();
            }
            else
                MessageBox.Show("Please add Source/Sink.");
        }
        /// <summary>
        /// Fetches the data.
        /// </summary>
        private void FetchData()
        {
            if (DaysSelectedTxtValue == null)
            {
                return;
            }
            int days = Int32.Parse(DaysSelectedTxtValue);
            if (SourceSinkList == null)
            {
                return;
            }
            List<Node> rtList = new List<Node>();
            List<Node> daList = new List<Node>();
            List<PricingNode> sourceSinkNodeList = new List<PricingNode>();
            foreach (SourceSinkData sourceSinkData in SourceSinkList)
            {
                if (!sourceSinkNodeList.Contains(sourceSinkData.Source))
                {
                    sourceSinkNodeList.Add(sourceSinkData.Source);
                }
                if (sourceSinkData.Sink != null && !sourceSinkNodeList.Contains(sourceSinkData.Sink))
                {
                    sourceSinkNodeList.Add(sourceSinkData.Sink);
                }
            }
            foreach (PricingNode priceNode in sourceSinkNodeList)
            {
                Node node = new Node();
                node.Market = priceNode.MarketKey;
                node.NodeId = priceNode.NodeKey;
                node.NodeName = priceNode.NodeName;
                node.PNodeId = priceNode.ExternalNodeId;
                rtList.Add(node);
                Node node1 = new Node();
                node1.Market = node.Market;
                node1.NodeId = node.NodeId;
                node1.NodeName = node.NodeName;
                node1.PNodeId = node.PNodeId;
                daList.Add(node1);
            }
            DARTNode.GetDART(rtList, daList, FromDate, days, false, true);
            mRTHash = new Dictionary<int, Node>();
            mDAHash = new Dictionary<int, Node>();
            foreach (Node node in rtList)
            {
                if (!mRTHash.ContainsKey(node.NodeId))
                {
                    mRTHash.Add(node.NodeId, node);
                }
            }
            foreach (Node node in daList)
            {
                if (!mDAHash.ContainsKey(node.NodeId))
                {
                    mDAHash.Add(node.NodeId, node);
                }
            }
        }
        /// <summary>
        /// Creates the plot model.
        /// </summary>
        /// <returns></returns>
        private PlotModel CreatePlotModel()
        {

            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            bool isDst = curTimeZone.IsDaylightSavingTime(DateTime.Today.AddDays(1).AddHours(7));
            mFilledLMPList = new List<LMP>();
            if (SourceSinkList == null)
            {
                return null;
            }

            if (!endDate.HasValue)
                endDate = FromDate;

            int days = 1;
            int.TryParse(DaysSelectedTxtValue, out days);
            if (!startDate.HasValue)
                startDate = endDate.Value.AddDays(-days);

            DetailList = new NodeDetailList();

            Vayu.NodePriceGraph.Model.DataService dataService = mDataService as Vayu.NodePriceGraph.Model.DataService;
            LMPDatesHelper lmpHelper = new LMPDatesHelper(dataService.VayuConnection);
            lmpHelper.TimingSession.Initialize(marketComboSelectedValue, startDate.Value, endDate.Value);
            var plotModel1 = new PlotModel { Title = "LMP" };
            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis()
            {
                Key = "Y1Axis",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = .05,
                MinimumPadding = .05,
                StartPosition = 0,
                EndPosition = 0.995
            });
            // X axis
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                Key = "Xx1Axis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 0.995,
                IsTickCentered = true
            };

            plotModel1.Axes.Add(categoryAxis);

            for (int i = 0; i < 24 * days; i++)
            {
                categoryAxis.Labels.Add((i + 1).ToString());
            }
            plotModel1.Series.Clear();
            var dataItemValues = new Collection<ChartItem>();
            var lineSeries1 = new LineSeries();
            int j = 0;
            foreach (SourceSinkData sourceSinkData in SourceSinkList)
            {
                Node daSourceNode = mDAHash[sourceSinkData.Source.NodeKey];
                Node rtSourceNode = mRTHash[sourceSinkData.Source.NodeKey];
                Node daSinkNode = sourceSinkData.Sink == null ? null : mDAHash[sourceSinkData.Sink.NodeKey];
                Node rtSinkNode = sourceSinkData.Sink == null ? null : mRTHash[sourceSinkData.Sink.NodeKey];
                for (int k = 0; k < 2; k++)
                {
                    double minPrice = double.MaxValue;
                    double maxPrice = double.MinValue;
                    double count = 0;
                    double sum = 0;
                    double peakCount = 0;
                    double peakSum = 0;
                    double offPeakCount = 0;
                    double offPeakSum = 0;
                    dataItemValues = new Collection<ChartItem>();

                    for (int i = 0; i < 24 * days; i++)
                    {
                        double price = double.NaN;
                        if (k == 0)
                        {
                            if (rtSourceNode.LmpTimePriceList[i].Lmp != null)
                            {
                                if (LMPComponentSelectedValue == Energy)
                                {
                                    double sEng = rtSourceNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Congestion - rtSourceNode.LmpTimePriceList[i].Lmp.Loss;
                                    if (rtSinkNode == null)
                                        price = sEng;
                                    else
                                    {
                                        double siEng = rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSinkNode.LmpTimePriceList[i].Lmp.Congestion - rtSinkNode.LmpTimePriceList[i].Lmp.Loss;
                                        price = (siEng - sEng);
                                    }
                                }
                                else if (LMPComponentSelectedValue == "LMP")
                                {
                                    price = rtSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Price :
                                                            rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price;
                                }
                                else if (LMPComponentSelectedValue == "LOSS")
                                {
                                    price = rtSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Loss :
                                                             rtSinkNode.LmpTimePriceList[i].Lmp.Loss - rtSourceNode.LmpTimePriceList[i].Lmp.Loss;
                                }
                                else
                                {
                                    price = rtSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Congestion :
                                                            rtSinkNode.LmpTimePriceList[i].Lmp.Congestion - rtSourceNode.LmpTimePriceList[i].Lmp.Congestion;
                                }
                            }
                        }
                        else
                        {
                            if (daSourceNode.LmpTimePriceList[i].Lmp != null)
                            {
                                if (LMPComponentSelectedValue == Energy)
                                {
                                    double sEng = daSourceNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Congestion -
                                        daSourceNode.LmpTimePriceList[i].Lmp.Loss;
                                    if (daSinkNode == null)
                                        price = sEng;
                                    else
                                    {
                                        double siEng = daSinkNode.LmpTimePriceList[i].Lmp.Price - daSinkNode.LmpTimePriceList[i].Lmp.Congestion -
                                            daSinkNode.LmpTimePriceList[i].Lmp.Loss;
                                        price = (siEng - sEng);
                                    }
                                }
                                else if (LMPComponentSelectedValue == "LMP")
                                {
                                    price = daSinkNode == null ? daSourceNode.LmpTimePriceList[i].Lmp.Price :
                                                        daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price;
                                }
                                else if (LMPComponentSelectedValue == "LOSS")
                                {
                                    price = daSinkNode == null ? daSourceNode.LmpTimePriceList[i].Lmp.Loss :
                                                        daSinkNode.LmpTimePriceList[i].Lmp.Loss - daSourceNode.LmpTimePriceList[i].Lmp.Loss;
                                }
                                else
                                {
                                    price = daSinkNode == null ? daSourceNode.LmpTimePriceList[i].Lmp.Congestion :
                                                        daSinkNode.LmpTimePriceList[i].Lmp.Congestion - daSourceNode.LmpTimePriceList[i].Lmp.Congestion;
                                }
                            }
                        }
                        if (LMPTypeSelectedValue == "RT-DA")
                        {
                            if (daSourceNode.LmpTimePriceList[i].Lmp != null && rtSourceNode.LmpTimePriceList[i].Lmp != null)
                            {
                                if (LMPComponentSelectedValue == Energy)
                                {
                                    double sEng = rtSourceNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Congestion -
                                        rtSourceNode.LmpTimePriceList[i].Lmp.Loss;
                                    double sdaEng = daSourceNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Congestion -
                                        daSourceNode.LmpTimePriceList[i].Lmp.Loss;

                                    sEng = sEng - sdaEng;

                                    if (daSinkNode == null)
                                        price = sEng;
                                    else
                                    {
                                        double sidaEng = daSinkNode.LmpTimePriceList[i].Lmp.Price - daSinkNode.LmpTimePriceList[i].Lmp.Congestion -
                                            daSinkNode.LmpTimePriceList[i].Lmp.Loss;
                                        double siEng = rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSinkNode.LmpTimePriceList[i].Lmp.Congestion -
                                            rtSinkNode.LmpTimePriceList[i].Lmp.Loss;
                                        siEng = siEng - sidaEng;
                                        price = (siEng - sEng);
                                    }
                                }
                                else if (LMPComponentSelectedValue == "LMP")
                                {
                                    price = daSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price :
                                    (rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price) -
                                    (daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price);
                                }
                                else if (LMPComponentSelectedValue == "LOSS")
                                {
                                    price = daSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Loss - daSourceNode.LmpTimePriceList[i].Lmp.Loss :
                                    (rtSinkNode.LmpTimePriceList[i].Lmp.Loss - rtSourceNode.LmpTimePriceList[i].Lmp.Loss) -
                                    (daSinkNode.LmpTimePriceList[i].Lmp.Loss - daSourceNode.LmpTimePriceList[i].Lmp.Loss);
                                }
                                else
                                {
                                    price = daSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Congestion - daSourceNode.LmpTimePriceList[i].Lmp.Congestion :
                                    (rtSinkNode.LmpTimePriceList[i].Lmp.Congestion - rtSourceNode.LmpTimePriceList[i].Lmp.Congestion) -
                                    (daSinkNode.LmpTimePriceList[i].Lmp.Congestion - daSourceNode.LmpTimePriceList[i].Lmp.Congestion);
                                }
                            }
                        }
                        if (LMPTypeSelectedValue == "DA-RT")
                        {
                            if (daSourceNode.LmpTimePriceList[i].Lmp != null && rtSourceNode.LmpTimePriceList[i].Lmp != null)
                            {
                                if (LMPComponentSelectedValue == Energy)
                                {
                                    double sEng = rtSourceNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Congestion -
                                        rtSourceNode.LmpTimePriceList[i].Lmp.Loss;
                                    double sdaEng = daSourceNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Congestion -
                                        daSourceNode.LmpTimePriceList[i].Lmp.Loss;

                                    sEng = sdaEng - sEng;

                                    if (daSinkNode == null)
                                        price = sEng;
                                    else
                                    {
                                        double sidaEng = daSinkNode.LmpTimePriceList[i].Lmp.Price - daSinkNode.LmpTimePriceList[i].Lmp.Congestion -
                                            daSinkNode.LmpTimePriceList[i].Lmp.Loss;
                                        double siEng = rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSinkNode.LmpTimePriceList[i].Lmp.Congestion -
                                            rtSinkNode.LmpTimePriceList[i].Lmp.Loss;
                                        siEng = sidaEng - siEng;
                                        price = (siEng - sEng);
                                    }
                                }
                                else if (LMPComponentSelectedValue == "LMP")
                                {
                                    price = daSinkNode == null ? daSourceNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price :
                                    (daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price) -
                                    (rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price);
                                }
                                else if (LMPComponentSelectedValue == "LOSS")
                                {
                                    price = daSinkNode == null ? daSourceNode.LmpTimePriceList[i].Lmp.Loss - rtSourceNode.LmpTimePriceList[i].Lmp.Loss :
                                    (daSinkNode.LmpTimePriceList[i].Lmp.Loss - daSourceNode.LmpTimePriceList[i].Lmp.Loss) -
                                    (rtSinkNode.LmpTimePriceList[i].Lmp.Loss - rtSourceNode.LmpTimePriceList[i].Lmp.Loss);
                                }
                                else
                                {
                                    price = daSinkNode == null ? daSourceNode.LmpTimePriceList[i].Lmp.Congestion - rtSourceNode.LmpTimePriceList[i].Lmp.Congestion :
                                    (daSinkNode.LmpTimePriceList[i].Lmp.Congestion - daSourceNode.LmpTimePriceList[i].Lmp.Congestion) -
                                    (rtSinkNode.LmpTimePriceList[i].Lmp.Congestion - rtSourceNode.LmpTimePriceList[i].Lmp.Congestion);
                                }
                            }
                        }
                        if (!double.IsNaN(price))
                        {
                            if (maxPrice < price)
                            {
                                maxPrice = price;
                            }
                            if (minPrice > price)
                            {
                                minPrice = price;
                            }
                            sum += price;
                            count++;

                            if (lmpHelper.TimingSession.IsOffPeak(daSourceNode.LmpTimePriceList[i].MarketTime))
                            {
                                offPeakSum += price;
                                offPeakCount++;
                            }
                            else
                            {
                                peakSum += price;
                                peakCount++;
                            }

                            dataItemValues.Add(new ChartItem() { X = i, Y = price });
                        }
                    }

                    lineSeries1 = new LineSeries()
                    {
                        CanTrackerInterpolatePoints = false,
                        DataFieldX = "X",
                        DataFieldY = "Y",
                        ItemsSource = dataItemValues,
                        TrackerFormatString = "{0}\n{2:0}\n{4:$0.00}",
                        XAxisKey = "Xx1Axis",
                        //XAxisKey = "Y1Axis",
                        // XAxisKey = "XAxisCategory",
                        YAxisKey = "Y1Axis",
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 2,
                        MarkerStrokeThickness = 0
                    };
                    if (j == 0)
                    {
                        lineSeries1.Color = OxyColors.Goldenrod;
                        lineSeries1.MarkerFill = OxyColors.Goldenrod;
                    }
                    else if (j == 1)
                    {
                        lineSeries1.Color = OxyColors.Green;
                        lineSeries1.MarkerFill = OxyColors.Green;
                    }
                    else if (j == 2)
                    {
                        lineSeries1.Color = OxyColors.Blue;
                        lineSeries1.MarkerFill = OxyColors.Blue;
                    }
                    else if (j == 3)
                    {
                        lineSeries1.Color = OxyColors.Red;
                        lineSeries1.MarkerFill = OxyColors.Red;
                    }
                    else if (j == 4)
                    {
                        lineSeries1.Color = OxyColors.Black;
                        lineSeries1.MarkerFill = OxyColors.Black;
                    }
                    else if (j == 5)
                    {
                        lineSeries1.Color = OxyColors.Orange;
                        lineSeries1.MarkerFill = OxyColors.Orange;
                    }
                    else if (j == 6)
                    {
                        lineSeries1.Color = OxyColors.HotPink;
                        lineSeries1.MarkerFill = OxyColors.HotPink;
                    }
                    else if (j == 7)
                    {
                        lineSeries1.Color = OxyColors.Purple;
                        lineSeries1.MarkerFill = OxyColors.Purple;
                    }
                    else if (j == 8)
                    {
                        lineSeries1.Color = OxyColors.SaddleBrown;
                        lineSeries1.MarkerFill = OxyColors.SaddleBrown;
                    }
                    else if (j == 9)
                    {
                        lineSeries1.Color = OxyColors.Salmon;
                        lineSeries1.MarkerFill = OxyColors.Salmon;
                    }
                    else if (j == 10)
                    {
                        lineSeries1.Color = OxyColors.SlateGray;
                        lineSeries1.MarkerFill = OxyColors.SlateGray;
                    }
                    else
                    {
                        lineSeries1.Color = OxyColors.MistyRose;
                        lineSeries1.MarkerFill = OxyColors.MistyRose;
                    }

                    j++;
                    string sinkName = rtSinkNode == null ? "" : "->" + rtSinkNode.NodeName;
                    string rowHead = string.Empty;
                    string pathName = string.Empty;

                    if (k == 0)
                    {
                        if (LMPTypeSelectedValue == "RT & DA" || LMPTypeSelectedValue == "RT")
                        {
                            lineSeries1.Title = rtSourceNode.NodeName + sinkName + " (RT)";
                            pathName = rtSourceNode.NodeName + sinkName;
                            rowHead = "RT";
                        }
                        else if (LMPTypeSelectedValue != "DA")
                        {
                            lineSeries1.Title = rtSourceNode.NodeName + sinkName + " (" + LMPTypeSelectedValue + ")";
                            pathName = rtSourceNode.NodeName + sinkName;
                            rowHead = LMPTypeSelectedValue;
                        }
                    }
                    else
                    {
                        lineSeries1.Title = daSourceNode.NodeName + sinkName + " (DA)";
                        pathName = daSourceNode.NodeName + sinkName;
                        lineSeries1.LineStyle = LineStyle.Dash;
                        rowHead = "DA";
                    }
                    if (k == 0 && LMPTypeSelectedValue != "DA")
                    {
                        plotModel1.Series.Add(lineSeries1);
                        DetailList.FillByItemList(FromDate, lineSeries1.ItemsSource as IEnumerable<ChartItem>, rowHead, pathName);
                    }
                    if (k == 1 && (LMPTypeSelectedValue == "RT & DA" || LMPTypeSelectedValue == "DA"))
                    {
                        plotModel1.Series.Add(lineSeries1);
                        DetailList.FillByItemList(FromDate, lineSeries1.ItemsSource as IEnumerable<ChartItem>, rowHead, pathName);
                    }
                    if (lineSeries1.Title != null)
                    {
                        LMP lmp = new LMP();
                        lmp.Name = lineSeries1.Title;
                        lmp.Max = Math.Round(maxPrice, 2);
                        lmp.Min = Math.Round(minPrice, 2);
                        lmp.Avg = Math.Round(sum / count, 2);
                        lmp.OnPkAvg = Math.Round(peakSum / peakCount, 2);
                        lmp.OffPkAvg = Math.Round(offPeakSum / offPeakCount, 2);
                        if (LMPTypeSelectedValue == "RT & DA" || lmp.Name.IndexOf("(" + LMPTypeSelectedValue + ")") != -1)
                        {
                            mFilledLMPList.Add(lmp);
                        }
                    }
                }
            }
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
            };

            plotModel1.Legends.Add(l);

            LMPList = null;
            LMPList = mFilledLMPList;
            return plotModel1;
        }
        /// <summary>
        /// Resets the grids.
        /// </summary>
        private void ResetGrids()
        {
            LMPList = new List<LMP>();
            DetailList = new NodeDetailList();
            LmpPlotModel = null;
        }

        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    public class LMP
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double Min { get; set; }
        /// <summary>
        /// Gets or sets the average.
        /// </summary>
        /// <value>
        /// The average.
        /// </value>
        public double Avg { get; set; }
        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public double Max { get; set; }
        /// <summary>
        /// Gets or sets the on pk average.
        /// </summary>
        /// <value>
        /// The on pk average.
        /// </value>
        public double OnPkAvg { get; set; }
        /// <summary>
        /// Gets or sets the off pk average.
        /// </summary>
        /// <value>
        /// The off pk average.
        /// </value>
        public double OffPkAvg { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChartItem
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int X { get; set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double Y { get; set; }
    }
}
