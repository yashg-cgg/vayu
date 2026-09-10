using OxyPlot;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vayu.DBLibrary;
using Vayu.LTC_Graphs.Design;
using Vayu.LTC_Graphs.Model;

namespace Vayu.LTC_Graphs.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Variables and Properties
        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// Gets or sets the current plots model.
        /// </summary>
        /// <value>
        /// The current plots model.
        /// </value>
        private SourceSinkPlotDic currentPlotsModel { get; set; }
        /// <summary>
        /// Gets the add path command.
        /// </summary>
        /// <value>
        /// The add path command.
        /// </value>
        public DelegateCommand AddPathCommand { get; private set; }
        /// <summary>
        /// Gets the swap node command.
        /// </summary>
        /// <value>
        /// The swap node command.
        /// </value>
        public DelegateCommand SwapNodeCommand { get; private set; }
        /// <summary>
        /// Gets the remove path command.
        /// </summary>
        /// <value>
        /// The remove path command.
        /// </value>
        public DelegateCommand RemovePathCommand { get; private set; }
        /// <summary>
        /// Gets the remove all command.
        /// </summary>
        /// <value>
        /// The remove all command.
        /// </value>
        public DelegateCommand RemoveAllCommand { get; private set; }
        /// <summary>
        /// The m CRR price hash
        /// </summary>
        private static Dictionary<DateTime, double> mCRRPriceHash = new Dictionary<DateTime, double>();

        /// <summary>
        /// The m from selected date
        /// </summary>
        private DateTime mFromSelectedDate = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, 1);
        /// <summary>
        /// Gets or sets from selected date.
        /// </summary>
        /// <value>
        /// From selected date.
        /// </value>
        public DateTime FromSelectedDate
        {
            get
            {
                return mFromSelectedDate;
            }
            set
            {
                mFromSelectedDate = value;
                RaisePropertyChanged("FromSelectedDate");
            }
        }
        /// <summary>
        /// The m thro selected date
        /// </summary>
        private DateTime mThroSelectedDate = DateTime.Today.AddMonths(0);
        /// <summary>
        /// Gets or sets the thro selected date.
        /// </summary>
        /// <value>
        /// The thro selected date.
        /// </value>
        public DateTime ThroSelectedDate
        {
            get
            {
                return mThroSelectedDate;
            }
            set
            {
                mThroSelectedDate = value;
                RaisePropertyChanged("ThroSelectedDate");
            }
        }
        /// <summary>
        /// The m node list
        /// </summary>
        private List<NodeDetail> mNodeList;
        /// <summary>
        /// Gets or sets the node list.
        /// </summary>
        /// <value>
        /// The node list.
        /// </value>
        public List<NodeDetail> NodeList
        {
            get
            {
                return mNodeList;
            }
            set
            {
                mNodeList = value;
                RaisePropertyChanged("NodeList");
            }
        }
        /// <summary>
        /// The m source selected item
        /// </summary>
        private NodeDetail mSourceSelectedItem;
        /// <summary>
        /// Gets or sets the source selected item.
        /// </summary>
        /// <value>
        /// The source selected item.
        /// </value>
        public NodeDetail SourceSelectedItem
        {
            get
            {
                return mSourceSelectedItem;
            }
            set
            {
                mSourceSelectedItem = value;
                RaisePropertyChanged("SourceSelectedItem");
            }
        }
        /// <summary>
        /// The m sink selected item
        /// </summary>
        private NodeDetail mSinkSelectedItem;
        /// <summary>
        /// Gets or sets the sink selected item.
        /// </summary>
        /// <value>
        /// The sink selected item.
        /// </value>
        public NodeDetail SinkSelectedItem
        {
            get
            {
                return mSinkSelectedItem;
            }
            set
            {
                mSinkSelectedItem = value;
                RaisePropertyChanged("SinkSelectedItem");
            }
        }
        /// <summary>
        /// The m stats list
        /// </summary>
        private List<StatisticsHelper> mStatsList;
        /// <summary>
        /// Gets or sets the stats list.
        /// </summary>
        /// <value>
        /// The stats list.
        /// </value>
        public List<StatisticsHelper> StatsList
        {
            get
            {
                return mStatsList;
            }
            set
            {
                mStatsList = value;
                RaisePropertyChanged("StatsList");
            }
        }

        /// <summary>
        /// The m market list
        /// </summary>
        private List<string> mMarketList = new List<string> { "ERCOT" };
        /// <summary>
        /// Gets or sets the market list.
        /// </summary>
        /// <value>
        /// The market list.
        /// </value>
        public List<string> MarketList
        {
            get
            {
                return mMarketList;
            }
            set
            {
                mMarketList = value;
                RaisePropertyChanged("MarketList");
            }
        }
        /// <summary>
        /// The m market selected item
        /// </summary>
        private string mMarketSelectedItem;
        /// <summary>
        /// Gets or sets the market selected item.
        /// </summary>
        /// <value>
        /// The market selected item.
        /// </value>
        public string MarketSelectedItem
        {
            get
            {
                return mMarketSelectedItem;
            }
            set
            {
                mMarketSelectedItem = value;
                RaisePropertyChanged("MarketSelectedItem");
                //if ("MISO".Equals(mMarketSelectedItem))
                //{
                //    QuarterlyName = "Seasonal";
                //}
                //else if ("PJM".Equals(mMarketSelectedItem))
                //{
                //    PJMVisible = true;
                //    QuarterlyName = "Quarterly";
                //    IsEnabledPeakWE = false;
                //    PeakName = "Peak";
                //}
                //else
                //{
                //    YearlyChecked = false;
                //    LongTermChecked = false;
                //    PJMVisible = false;
                //    IsEnabledPeakWE = true;
                //    QuartChecked = false;
                //    PeakName = "PeakWD";
                //}
                if (value != null)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() => PrepareNodes(GetMarketKey()));
                    AddedPathList = new SourceSinkNodeList();
                    SelectedAddedPaths = null;
                    RefreshData();
                }
            }
        }

        /// <summary>
        /// The m current plot model
        /// </summary>
        private PlotModel mCurrentPlotModel;
        /// <summary>
        /// Gets or sets the current plot model.
        /// </summary>
        /// <value>
        /// The current plot model.
        /// </value>
        public PlotModel CurrentPlotModel
        {
            get
            {
                return mCurrentPlotModel;
            }
            set
            {
                mCurrentPlotModel = value;
                RaisePropertyChanged("CurrentPlotModel");
            }
        }

        /// <summary>
        /// The m added path list
        /// </summary>
        private SourceSinkNodeList mAddedPathList;
        /// <summary>
        /// Gets or sets the added path list.
        /// </summary>
        /// <value>
        /// The added path list.
        /// </value>
        public SourceSinkNodeList AddedPathList
        {
            get
            {
                return mAddedPathList;
            }
            set
            {
                mAddedPathList = value;
                RaisePropertyChanged("AddedPathList");
            }
        }
        /// <summary>
        /// The m selected added paths
        /// </summary>
        private SourceSinkDetail mSelectedAddedPaths;
        /// <summary>
        /// Gets or sets the selected added paths.
        /// </summary>
        /// <value>
        /// The selected added paths.
        /// </value>
        public SourceSinkDetail SelectedAddedPaths
        {
            get
            {
                return mSelectedAddedPaths;
            }
            set
            {
                if (mSelectedAddedPaths != value)
                {
                    mSelectedAddedPaths = value;
                    RaisePropertyChanged("SelectedAddedPaths");
                    if (value != null)
                    {
                        StatsList = null;
                        CRRLMPList = null;
                        RefreshData();
                    }
                }
            }
        }
        /// <summary>
        /// The mf TRLMP list
        /// </summary>
        private List<LmpHelper> mCRRLMPList;
        /// <summary>
        /// Gets or sets the CRRLMP list.
        /// </summary>
        /// <value>
        /// The CRRLMP list.
        /// </value>
        public List<LmpHelper> CRRLMPList
        {
            get
            {
                return mCRRLMPList;
            }
            set
            {
                mCRRLMPList = value;
                RaisePropertyChanged("CRRLMPList");
            }
        }
        /// <summary>
        /// The m CRR checked
        /// </summary>
        private bool mCRRChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [CRR checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [CRR checked]; otherwise, <c>false</c>.
        /// </value>
        public bool CRRChecked
        {
            get
            {
                return mCRRChecked;
            }
            set
            {
                if (mCRRChecked != value)
                {
                    mCRRChecked = value;
                    RaisePropertyChanged("CRRChecked");
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m da checked
        /// </summary>
        private bool mDAChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [da checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [da checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DAChecked
        {
            get
            {
                return mDAChecked;
            }
            set
            {
                if (mDAChecked != value)
                {
                    mDAChecked = value;
                    RaisePropertyChanged("DAChecked");
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m rt checked
        /// </summary>
        private bool mRTChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [rt checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [rt checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RTChecked
        {
            get
            {
                return mRTChecked;
            }
            set
            {
                if (mRTChecked != value)
                {
                    mRTChecked = value;
                    RaisePropertyChanged("RTChecked");
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m da minus CRR checked
        /// </summary>
        private bool mDAMinusCRRChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [da minus CRR checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [da minus CRR checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DAMinusCRRChecked
        {
            get
            {
                return mDAMinusCRRChecked;
            }
            set
            {
                if (mDAMinusCRRChecked != value)
                {
                    mDAMinusCRRChecked = value;
                    RaisePropertyChanged("DAMinusCRRChecked");
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m peak checked
        /// </summary>
        private bool mPeakChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [peak checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [peak checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PeakChecked
        {
            get
            {
                return mPeakChecked;
            }
            set
            {
                mPeakChecked = value;
                RaisePropertyChanged("PeakChecked");
                if (PeakChecked)
                {
                    mCurrentHourType = HourType.Peak;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m off peak checked
        /// </summary>
        private bool mOffPeakChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [off peak checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [off peak checked]; otherwise, <c>false</c>.
        /// </value>
        public bool OffPeakChecked
        {
            get
            {
                return mOffPeakChecked;
            }
            set
            {
                mOffPeakChecked = value;
                RaisePropertyChanged("OffPeakChecked");
                if (OffPeakChecked)
                {
                    mCurrentHourType = HourType.OffPeak;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m all checked
        /// </summary>
        private bool mAllChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [all checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [all checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AllChecked
        {
            get
            {
                return mAllChecked;
            }
            set
            {
                mAllChecked = value;
                RaisePropertyChanged("AllChecked");
                if (AllChecked)
                {
                    mCurrentHourType = HourType.Day;
                    RefreshData();
                }
            }
        }
        private bool mPeakWEChecked;
        public bool PeakWEChecked
        {
            get
            {
                return mPeakWEChecked;
            }
            set
            {
                mPeakWEChecked = value;
                RaisePropertyChanged("PeakWEChecked");
                if (PeakWEChecked)
                {
                    mCurrentHourType = HourType.PeakWE;
                    RefreshData();
                }
            }
        }
        private bool mIsEnabledPeakWE;
        public bool IsEnabledPeakWE
        {
            get
            {
                return mIsEnabledPeakWE;
            }
            set
            {
                mIsEnabledPeakWE = value;
                RaisePropertyChanged("IsEnabledPeakWE");
            }
        }
        /// <summary>
        /// The m monthly checked
        /// </summary>
        private bool mMonthlyChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [monthly checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [monthly checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MonthlyChecked
        {
            get
            {
                return mMonthlyChecked;
            }
            set
            {
                mMonthlyChecked = value;
                RaisePropertyChanged("MonthlyChecked");
                if (MonthlyChecked)
                {
                    mCurrentInterval = Interval.Monthly;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m quart checked
        /// </summary>
        private bool mQuartChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [quart checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [quart checked]; otherwise, <c>false</c>.
        /// </value>
        public bool QuartChecked
        {
            get
            {
                return mQuartChecked;
            }
            set
            {
                mQuartChecked = value;
                RaisePropertyChanged("QuartChecked");
                if (QuartChecked)
                {
                    mCurrentInterval = Interval.Quarterly;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m yearly checked
        /// </summary>
        private bool mYearlyChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [yearly checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [yearly checked]; otherwise, <c>false</c>.
        /// </value>
        public bool YearlyChecked
        {
            get
            {
                return mYearlyChecked;
            }
            set
            {
                mYearlyChecked = value;
                RaisePropertyChanged("YearlyChecked");
                if (YearlyChecked)
                {
                    mCurrentInterval = Interval.Annually;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m long term checked
        /// </summary>
        private bool mLongTermChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [long term checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [long term checked]; otherwise, <c>false</c>.
        /// </value>
        public bool LongTermChecked
        {
            get
            {
                return mLongTermChecked;
            }
            set
            {
                mLongTermChecked = value;
                RaisePropertyChanged("LongTermChecked");
                if (LongTermChecked)
                {
                    mCurrentInterval = Interval.LongTerm;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m refresh enabled
        /// </summary>
        private bool mRefreshEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether [refresh enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [refresh enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool RefreshEnabled
        {
            get
            {
                return mRefreshEnabled;
            }
            set
            {
                mRefreshEnabled = value;
                RaisePropertyChanged("RefreshEnabled");
            }
        }
        /// <summary>
        /// The m selected index
        /// </summary>
        private int mSelectedIndex;
        /// <summary>
        /// Gets or sets the index of the selected.
        /// </summary>
        /// <value>
        /// The index of the selected.
        /// </value>
        public int SelectedIndex
        {
            get
            {
                return mSelectedIndex;
            }
            set
            {
                mSelectedIndex = value; RaisePropertyChanged("SelectedIndex");
            }
        }
        /// <summary>
        /// The m PJM visible
        /// </summary>
        //private bool mPjmVisible;
        ///// <summary>
        ///// Gets or sets a value indicating whether [PJM visible].
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if [PJM visible]; otherwise, <c>false</c>.
        ///// </value>
        //public bool PJMVisible
        //{
        //    get
        //    {
        //        return mPjmVisible;
        //    }
        //    set
        //    {
        //        mPjmVisible = value; RaisePropertyChanged("PJMVisible");
        //    }
        //}
        /// <summary>
        /// The m current interval
        /// </summary>
        private Interval mCurrentInterval;
        /// <summary>
        /// The m current sub interval
        /// </summary>
        private SubInterval mCurrentSubInterval;
        /// <summary>
        /// The m current price type
        /// </summary>
        private PriceType mCurrentPriceType;
        /// <summary>
        /// The m current hour type
        /// </summary>
        private HourType mCurrentHourType;
        /// <summary>
        /// The m quarterly name
        /// </summary>
        private string mQuarterlyName = "Quarterly";
        /// <summary>
        /// Gets or sets the name of the quarterly.
        /// </summary>
        /// <value>
        /// The name of the quarterly.
        /// </value>
        public string QuarterlyName
        {
            get
            {
                return mQuarterlyName;
            }
            set
            {
                mQuarterlyName = value; RaisePropertyChanged("QuarterlyName");
            }
        }
        private string mPeakName = "Peak";
        public string PeakName { get { return mPeakName; } set { mPeakName = value; RaisePropertyChanged("PeakName"); } }
        /// <summary>
        /// The m yr1 checked
        /// </summary>
        private bool mYr1Checked;
        /// <summary>
        /// Gets or sets a value indicating whether [y r1 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [y r1 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool YR1Checked
        {
            get
            {
                return mYr1Checked;
            }
            set
            {
                mYr1Checked = value;
                RaisePropertyChanged("YR1Checked");
                if (YR1Checked)
                {
                    mCurrentSubInterval = SubInterval.YR1;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m yr2 checked
        /// </summary>
        private bool mYr2Checked;
        /// <summary>
        /// Gets or sets a value indicating whether [y r2 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [y r2 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool YR2Checked
        {
            get
            {
                return mYr2Checked;
            }
            set
            {
                mYr2Checked = value;
                RaisePropertyChanged("YR2Checked");
                if (YR2Checked)
                {
                    mCurrentSubInterval = SubInterval.YR2;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m yr3 checked
        /// </summary>
        private bool mYr3Checked;
        /// <summary>
        /// Gets or sets a value indicating whether [y r3 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [y r3 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool YR3Checked
        {
            get
            {
                return mYr3Checked;
            }
            set
            {
                mYr3Checked = value;
                RaisePropertyChanged("YR3Checked");
                if (YR3Checked)
                {
                    mCurrentSubInterval = SubInterval.YR3;
                    RefreshData();
                }
            }
        }
        /// <summary>
        /// The m yrall checked
        /// </summary>
        private bool mYrallChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [yr all checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [yr all checked]; otherwise, <c>false</c>.
        /// </value>
        public bool YRAllChecked
        {
            get
            {
                return mYrallChecked;
            }
            set
            {
                mYrallChecked = value;
                RaisePropertyChanged("YRAllChecked");
                if (YRAllChecked)
                {
                    mCurrentSubInterval = SubInterval.YRAll;
                    RefreshData();
                }
            }
        }
        #endregion

        public MainWindowViewModel(IDataService _dataService)
        {
            AddPathCommand = new DelegateCommand(() => AddPath());
            SwapNodeCommand = new DelegateCommand(() => SwapPaths());
            RemovePathCommand = new DelegateCommand(() => RemovePath());
            RemoveAllCommand = new DelegateCommand(() => RemoveAll());
            mDataService = _dataService;
            if (mDataService == null)
            {
                mDataService = new DataService();
            }
            MonthlyChecked = true;
            PeakChecked = true;
            IsEnabledPeakWE = false;
            YR1Checked = true;
            MarketSelectedItem = MarketList.Where(a => a == "ERCOT").FirstOrDefault();
            currentPlotsModel = new SourceSinkPlotDic();
        }
        /// <summary>
        /// Prepares the nodes.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        private void PrepareNodes(int marketKey)
        {
            NodeList = null;
            Dictionary<int, List<string>> nodeTypeHash = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> zoneHash = new Dictionary<int, List<string>>();
            Dictionary<string, NodeDetail> nodeHash = DBAccess.GetAllNodes(marketKey, nodeTypeHash, zoneHash);
            NodeList = nodeHash.Values.ToList<NodeDetail>();
        }
        #region Add--Remove
        /// <summary>
        /// Removes all.
        /// </summary>
        private void RemoveAll()
        {
            try
            {
                var tempList = AddedPathList.ToList();
                tempList.Clear();
                AddedPathList = new SourceSinkNodeList(tempList.ToList());
                currentPlotsModel.Clear();
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Removes the path.
        /// </summary>
        private void RemovePath()
        {
            if (SelectedAddedPaths == null)
            {
                return;
            }
            try
            {
                if (AddedPathList.Where(a => a.Source == SelectedAddedPaths.Source && a.Sink == SelectedAddedPaths.Sink).Count() > 0)
                {
                    currentPlotsModel.Remove(SelectedAddedPaths);
                    var tempList = AddedPathList.ToList();
                    tempList.RemoveAll(a => a.Source == SelectedAddedPaths.Source && a.Sink == SelectedAddedPaths.Sink);
                    AddedPathList = new SourceSinkNodeList(tempList.ToList());
                    if (SelectedAddedPaths == null)
                        RefreshData();
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Swaps the paths.
        /// </summary>
        private void SwapPaths()
        {
            if (SelectedAddedPaths == null)
            {
                return;
            }
            try
            {
                int index = SelectedIndex;
                NodeDetail source = SelectedAddedPaths.Sink as NodeDetail;
                NodeDetail sink = SelectedAddedPaths.Source as NodeDetail;

                SelectedAddedPaths.Source = source;
                SelectedAddedPaths.Sink = sink;

                SourceSinkPlot modelPlot = currentPlotsModel.Swap(mDataService, SelectedAddedPaths);
                RefreshData();

                SourceSinkNodeList test = AddedPathList;
                AddedPathList = null;
                AddedPathList = test;
                SelectedIndex = index;
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Adds the path.
        /// </summary>
        private void AddPath()
        {
            List<SourceSinkDetail> tempaddedPathList = AddedPathList == null ? new List<SourceSinkDetail>() : AddedPathList.ToList();
            try
            {
                if (tempaddedPathList.Where(a => a.Source == SourceSelectedItem && a.Sink == SinkSelectedItem).Count() <= 0)
                {
                    tempaddedPathList.Add(new SourceSinkDetail
                    {
                        Source = SourceSelectedItem,
                        Sink = SinkSelectedItem
                    });
                }
            }
            catch (Exception ex)
            {
            }
            AddedPathList = new SourceSinkNodeList(tempaddedPathList.ToList());
            SelectedAddedPaths = AddedPathList.Where(a => a.Source == SourceSelectedItem && a.Sink == SinkSelectedItem).FirstOrDefault();
        }
        #endregion

        #region View-Model Methods
        /// <summary>
        /// Refreshes the data.
        /// </summary>
        private void RefreshData()
        {
            //Utility.StartTimer("RefreshData");
            CurrentPlotModel = null;
            StatsList = null;
            CRRLMPList = null;
            if (SelectedAddedPaths == null)
            {
                return;
            }
            SourceSinkPlot modelPlot = currentPlotsModel.FillOrGet(mDataService, SelectedAddedPaths, FromSelectedDate, ThroSelectedDate, GetMarketKey(), mCurrentHourType);
            if (!modelPlot.SetState(mCurrentInterval, GetStat(), mCurrentHourType, GetMarketKey(), mCurrentSubInterval))
            {
                return;
            }
            CurrentPlotModel = modelPlot.GraphPlotModel;
            StatsList = modelPlot.Summary;
            CRRLMPList = modelPlot.CRRDetails;
        }

        /// <summary>
        /// Gets the stat.
        /// </summary>
        /// <returns></returns>
        private PriceType GetStat()
        {
            PriceType stat = PriceType.None;
            if (RTChecked)
            {
                stat |= PriceType.RT;
            }
            if (DAChecked)
            {
                stat |= PriceType.DA;
            }
            if (CRRChecked)
            {
                stat |= PriceType.CRR;
            }
            if (DAMinusCRRChecked)
            {
                stat |= PriceType.DACRR;
            }
            return stat;
        }
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <returns></returns>
        private int GetMarketKey()
        {
            switch (MarketSelectedItem.ToUpper())
            {

                case "ERCOT": return 9;
                //case "SPP": return 12;
                // case "CAISO": return 7;
                default: return 0;
            }
        }
        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{DBLibrary.SourceSinkDetail}" />
    public class SourceSinkNodeList : ObservableCollection<SourceSinkDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceSinkNodeList"/> class.
        /// </summary>
        public SourceSinkNodeList() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceSinkNodeList"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public SourceSinkNodeList(IEnumerable<SourceSinkDetail> list) : base(list) { }
    }
}
