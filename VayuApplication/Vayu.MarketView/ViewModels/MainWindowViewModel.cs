//using Microsoft.Maps.MapControl.WPF;   // Bing Maps replaced by Mapsui/OSM. Kept commented for traceability.
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Vayu.CommonControls;
using Vayu.DBLibrary;
using Vayu.LatestConstraintsInformationLibrary;
using Vayu.MarketView.Model;
using Vayu.MarketView.Views;
using Vayu.NodePriceFiveMinLibrary;
using Vayu.NodePriceLibrary;
using Location = Vayu.MarketView.Model.MapLocation;           // Mapsui migration: retarget Location alias.
using LocationCollection = Vayu.MarketView.Model.MapLocationCollection; // Mapsui migration: retarget LocationCollection alias.

namespace Vayu.MarketView.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Declaration
        List<string> ListstrDeenergizedNodes;

        /// <summary>
        /// The lock constraint
        /// </summary>
        private readonly object lockConstraint = new object();
        /// <summary>
        /// The lock LMP object
        /// </summary>
        private readonly object lockLmpObj = new object();
        /// <summary>
        /// The lock settlement up to object
        /// </summary>
        private readonly object lockSettlementUpToObj = new object();
        /// <summary>
        /// The lock constraint map
        /// </summary>
        private readonly object lockConstraintMap = new object();
        /// <summary>
        /// The location task
        /// </summary>
        private Task locationTask;
        /// <summary>
        /// The m selected market
        /// </summary>
        private MarketsEnum mSelectedMarket;
        /// <summary>
        /// The m settlement only
        /// </summary>
        private bool mSettlementOnly = true;
        /// <summary>
        /// The m is all day
        /// </summary>
        private bool mIsAllDay;
        /// <summary>
        /// The m count
        /// </summary>
        private int mCount;
        /// <summary>
        /// The m update time
        /// </summary>
        private string mUpdateTime;
        /// <summary>
        /// The m upto nodes
        /// </summary>
        private Hashtable mUptoNodes;
        /// <summary>
        /// The m FTR nodes
        /// </summary>
        private Hashtable mFTRNodes;
        /// <summary>
        /// The m Virtual nodes
        /// </summary>
        private Hashtable mVirtualNodes;
        /// <summary>
        /// The m upto only
        /// </summary>
        private bool mUPTOOnly = true;
        /// <summary>
        /// The m selected constrain
        /// </summary>
        private object mSelectedConstrain;
        /// <summary>
        /// The m map center location
        /// </summary>
        private Location mMapCenterLocation;
        /// <summary>
        /// The m market date time
        /// </summary>
        private DateTime mMarketDateTime;

        /// <summary>
        /// The trader port folio list
        /// </summary>
        private ObservableCollection<Portfolio> traderPortFolioList;
        /// <summary>
        /// The trader portfolio selected item
        /// </summary>
        private Portfolio traderPortfolioSelectedItem;
        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName.ToLower();
        /// <summary>
        /// The m bid list
        /// </summary>
        private List<int> mBidList = new List<int>();

        /// <summary>
        /// The m data service
        /// </summary>
        private IDataService mDataService;
        /// <summary>
        /// The m constraint channel
        /// </summary>
        private IConstraintInfoProvider mConstraintChannel;
        /// <summary>
        /// The m LMP channel
        /// </summary>
        private INodePriceFiveMin mLMPChannel;

        private ILMPMarketView mPriceChannel;
        /// <summary>
        /// The m latest LMP list
        /// </summary>
        private LMPDataList mLatestLMPList;
        /// <summary>
        /// The m llocation list
        /// </summary>
        private PointMapPathLocationList mLlocationList;
        /// <summary>
        /// The m latest constraints list
        /// </summary>
        private ConstraintList mLatestConstraintsList;
        /// <summary>
        /// The m constraint timer
        /// </summary>
        private DispatcherTimerEx mConstraintTimer;
        /// <summary>
        /// The m LMP timer
        /// </summary>
        private DispatcherTimerEx mLmpTimer;
        /// <summary>
        /// The m settlement enable
        /// </summary>
        private bool mSettlementEnable;
        /// <summary>
        /// The m upto enable
        /// </summary>
        private bool mUPTOEnable;
        /// <summary>
        /// The m constraint location list
        /// </summary>
        private MultiLocationList mConstraintLocationList;
        /// <summary>
        /// The m node hash cache
        /// </summary>
        private NodeLocationHash mNodeHashCache;
        //private ConstraintRTGeoHash mConstraintRTGeoHash;
        /// <summary>
        /// The m eclips constraint location list
        /// </summary>
        private PointMapPathLocationList mEclipsConstraintLocationList;
        /// <summary>
        /// The m navigate constraints
        /// </summary>
        private PointMapPathLocationList mNavigateConstraints;
        /// <summary>
        /// The m navigate visibility
        /// </summary>
        private Visibility mNavigateVisibility;
        /// <summary>
        /// The list zone
        /// </summary>
        public List<ZoneInfo> ListZone = new List<ZoneInfo>();
        // public ObservableCollection<ZoneInfo> ListZone = new ObservableCollection<ZoneInfo>();       
        Dictionary<string, string> dictZones = new Dictionary<string, string>();
        /// <summary>
        /// Occurs when [updatemap].
        /// </summary>
        public event EventHandler Updatemap;

        /// <summary>
        /// All portfolio
        /// </summary>
        private Portfolio allPortfolio;

        Node[] historcalNodes = null;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the value of Zone Selected.
        /// </summary>
        /// <value>
        /// The value of Zone selected.
        /// </value>
        private string mZoneSelectedItem;

        public string ZoneSelectedItem
        {
            get
            {
                return mZoneSelectedItem;
            }
            set
            {
                mZoneSelectedItem = value;
                RaisePropertyChanged("ZoneSelectedItem");
            }
        }

        /// <summary>
        /// Gets or sets the map center location.
        /// </summary>
        /// <value>
        /// The map center location.
        /// </value>
        /// 
        public Location MapCenterLocation
        {
            get
            {
                return mMapCenterLocation;
            }
            set
            {
                mMapCenterLocation = value;
                RaisePropertyChanged("MapCenterLocation");
            }
        }

        // ------------------------------------------------------------------
        // Mapsui/OSM migration:
        // The following five Bing-typed properties (Map / MapLayer) were
        // exposed to the View only for the legacy Bing map. The Mapsui view
        // manages its layers internally, so these are no longer used.
        // Kept commented for traceability (do not delete).
        // ------------------------------------------------------------------
        ///// <summary>
        ///// Gets or sets the UI map.
        ///// </summary>
        ///// <value>
        ///// The UI map.
        ///// </value>
        //public Map UIMap { get; set; }
        ///// <summary>
        ///// Gets or sets the navigate layer.
        ///// </summary>
        ///// <value>
        ///// The navigate layer.
        ///// </value>
        //public MapLayer NavigateLayer { get; set; }
        ///// <summary>
        ///// Gets or sets the poly constrains layer.
        ///// </summary>
        ///// <value>
        ///// The poly constrains layer.
        ///// </value>
        //public MapLayer PolyConstrainsLayer { get; set; }
        ///// <summary>
        ///// Gets or sets the constrains layer.
        ///// </summary>
        ///// <value>
        ///// The constrains layer.
        ///// </value>
        //public MapLayer ConstrainsLayer { get; set; }
        ///// <summary>
        ///// Gets or sets the LMP map layer.
        ///// </summary>
        ///// <value>
        ///// The LMP map layer.
        ///// </value>
        //public MapLayer LMPMapLayer { get; set; }

        /// <summary>
        /// The is expanded
        /// </summary>
        private bool _IsExpanded;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {

                _IsExpanded = true; ;

            }
        }

        /// <summary>
        /// Gets or sets the selected constrain.
        /// </summary>
        /// <value>
        /// The selected constrain.
        /// </value>
        public object SelectedConstrain
        {
            get
            {
                return mSelectedConstrain;
            }
            set
            {
                mSelectedConstrain = value;
                RaisePropertyChanged("SelectedConstrain");
                OnConstraintSelectionChanged();
            }
        }
        public string Market;
        /// <summary>
        /// Gets the upto nodes.
        /// </summary>
        /// <value>
        /// The upto nodes.
        /// </value>
        public Hashtable UPTONodes
        {
            get
            {
                Market = "ERCOT";
                if (mUptoNodes == null)
                {
                    List<int> nodeKeys = NodeHelper.Instance.GetErcotUpToNodeKeys(Market);

                    mUptoNodes = new Hashtable(nodeKeys.ToDictionary(x => x));
                }

                return mUptoNodes;
            }
        }

        /// <summary>
        /// Gets the FTR nodes.
        /// </summary>
        /// <value>
        /// The upto nodes.
        /// </value>
        public Hashtable FTRNodes
        {
            get
            {
                if (mFTRNodes == null)
                {
                    List<int> FTRnodeKeys = NodeHelper.Instance.GetFTRNodeKeys();
                    mFTRNodes = new Hashtable(FTRnodeKeys.ToDictionary(x => x));
                }

                return mFTRNodes;
            }
        }
        /// <summary>
        /// Gets the Vitrual nodes.
        /// </summary>
        /// <value>
        /// The upto nodes.
        /// </value>
        public Hashtable VirtualNodes
        {
            get
            {
                if (mVirtualNodes == null)
                {
                    List<int> FTRnodeKeys = NodeHelper.Instance.GetVitrualNodeKeys();
                    mVirtualNodes = new Hashtable(FTRnodeKeys.ToDictionary(x => x));
                }

                return mVirtualNodes;
            }
        }
        private bool _isErcotEnabled;

        public bool isErcotEnabled
        {
            get { return _isErcotEnabled; }
            set
            {
                _isErcotEnabled = value;
                RaisePropertyChanged("isErcotEnabled");
            }
        }

        /// <summary>
        /// Gets or sets the selected market.
        /// </summary>
        /// <value>
        /// The selected market.
        /// </value>
        public MarketsEnum SelectedMarket
        {
            get
            {
                return mSelectedMarket;
            }
            set
            {
                //LogWriter.Log(DateTime.Now.ToString());
                mSelectedMarket = value;

                RaisePropertyChanged("SelectedMarket");
                dictZones = GetZone();
                DistinctZoneList = new List<string>() { "All" };
                DistinctZoneList.AddRange(dictZones.Values.Where(values => !string.IsNullOrEmpty(values)).Distinct().OrderBy(values => values).ToList());
                OnMarketSelectionChanged();
                RaisePropertyChanged("LMPHeader");
                RaisePropertyChanged("ConstraintsHeader");

                if (mSelectedMarket == MarketsEnum.ERCOT)
                {
                    isErcotEnabled = true;
                    if (mSelectedMarket == MarketsEnum.ERCOT && string.IsNullOrEmpty(ZoneSelectedItem))
                    {
                        ZoneSelectedItem = DistinctZoneList[0];
                    }
                }
                else
                {
                    isErcotEnabled = false;
                }
                UTCChecked = true;
                SetPortfolio();

            }
        }

        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime
        {
            get
            {
                return mMarketDateTime;
            }
            set
            {
                mMarketDateTime = value;
                RaisePropertyChanged("MarketDateTime");
                //if (MarketDateTime == DateTime.Today)
                //{
                //    OnMarketSelectionChanged();
                //}
                //else
                //{
                //    RefreshConstrains();
                //}
                //if (MarketDateTime != DateTime.Today)
                //{
                //    RefreshLMP();
                //    CloseAndUnsubscribe();
                //}
                UpdateTime = MarketDateTime.ToString("MM-dd ") + DateTime.Now.ToString("HH:mm:ss");

            }
        }

        private int _sliderValue;
        private DateTime TempDate => DateTime.Now;
        //private DateTime StartDate => DateTime.Now.Date.AddHours(-24);
        //private DateTime StartDate => new DateTime(TempDate.Year, TempDate.Month, TempDate.Day, TempDate.Hour, 0, 0).AddHours(-48);
        private DateTime StartDate
        {
            get
            {
                // Round down minutes to nearest 5
                int roundedMinutes = (TempDate.Minute / 5) * 5;
                return new DateTime(
                    TempDate.Year,
                    TempDate.Month,
                    TempDate.Day,
                    TempDate.Hour,
                    roundedMinutes,
                    0
                ).AddHours(-48);
            }
        }

        private string _sliderDisplayTime;
        private bool _isHistoricalChecked;

        public string SliderDisplayTime
        {
            get => _sliderDisplayTime;
            set
            {
                _sliderDisplayTime = value;
                RaisePropertyChanged("SliderDisplayTime");
            }
        }

        public int SliderValue
        {
            get => _sliderValue;
            set
            {

                // Snap to the nearest multiple of 5 minutes
                int roundedMinutes = (int)(Math.Round((value * 5) / 5.0) * 5); // Ensures only multiples of 5

                // Convert minutes back to slider step
                _sliderValue = roundedMinutes / 5;

                mMarketDateTime = StartDate.AddMinutes(_sliderValue * 5);
                SliderDisplayTime = mMarketDateTime.ToString("MMM dd, HH:mm");

                if (_isHistoricalChecked)
                {
                    // Async execution for historical data
                    ProcessHistoricalDataAsync(mMarketDateTime);
                }

                RaisePropertyChanged("SliderValue");
                RaisePropertyChanged("SliderDisplayTime");
            }
        }

        private CancellationTokenSource _cts;

        private async void ProcessHistoricalDataAsync(DateTime targetTime)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            try
            {
                // Capture current state for thread safety
                var currentNodes = historcalNodes.ToList();
                var currentDeenergized = ListstrDeenergizedNodes.ToList();
                var currentZones = new Dictionary<string, string>(dictZones);

                var tempList = await Task.Run(() =>
                {
                    var result = new List<LmpData>();
                    var tempItems = from a in currentNodes
                                    where a.LmpTimePriceList != null
                                    select new
                                    {
                                        lmp = a.LmpTimePriceList
                                              .OrderByDescending(k => k.MarketTime == targetTime)
                                              .FirstOrDefault(),
                                        nodename = a.NodeName,
                                        nodekey = a.NodeId,
                                        isdeenergized = currentDeenergized.Contains(a.NodeName)
                                    };

                    foreach (var item in tempItems)
                    {
                        if (item.lmp == null) continue;
                        if (currentZones.TryGetValue(item.nodename, out var zone))
                        {
                            result.Add(new LmpData(item.lmp, item.nodekey,
                                                item.nodename, zone, item.isdeenergized));
                        }
                    }
                    return result;
                });

                // UI thread operations
                lmpHistoricalTempList = tempList.OrderBy(a => a.NodeName).ToList();
                ProcessSettlementUpTos(lmpHistoricalTempList, 9);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
        }

        public bool IsHistoricalChecked
        {
            get => _isHistoricalChecked;
            set
            {
                _isHistoricalChecked = value;
                if (!IsHistoricalChecked)
                {
                    mMarketDateTime = DateTime.Today.Date;

                    LMPChannel.Subscribe(9);
                    if (LMPTimer == null)
                    {
                        LMPTimer = DispatcherTimerEx.Create(LMPTimer_Tick);
                        LMPTimer.Start();
                    }
                    RefreshLMP();
                    SliderValue = 576;
                    mMarketDateTime = StartDate.AddMinutes(_sliderValue * 5);
                    SliderDisplayTime = mMarketDateTime.ToString("MMM dd, HH:mm");
                    RefreshLMPAsync();

                }
                else
                {
                    mMarketDateTime = DateTime.Now;
                    LMPTimer.Stop();
                    LMPChannel.Unsubscribe();

                }
                RaisePropertyChanged("IsHistoricalChecked");

            }
        }
        /// <summary>
        /// Gets or sets the trader portfolio list.
        /// </summary>
        /// <value>
        /// The trader portfolio list.
        /// </value>
        public ObservableCollection<Portfolio> TraderPortfolioList
        {
            get
            {
                return traderPortFolioList;
            }
            set
            {
                traderPortFolioList = value;
                RaisePropertyChanged("TraderPortfolioList");
            }
        }
        /// <summary>
        /// Gets or sets the trader portfolio selected item.
        /// </summary>
        /// <value>
        /// The trader portfolio selected item.
        /// </value>
        public Portfolio TraderPortfolioSelectedItem
        {
            get
            {
                return traderPortfolioSelectedItem;
            }
            set
            {
                traderPortfolioSelectedItem = value;
                RaisePropertyChanged("TraderPortfolioSelectedItem");
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [settlement only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [settlement only]; otherwise, <c>false</c>.
        /// </value>
        public bool SettlementOnly
        {
            get
            {
                if (SelectedMarket == MarketsEnum.ERCOT)
                {
                    return mSettlementOnly;
                }
                return true;
            }
            set
            {
                mSettlementOnly = value;
                RaisePropertyChanged("SettlementOnly");
                Mouse.OverrideCursor = Cursors.Wait;
                OnSettlementUpToChecked();
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [upto only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [upto only]; otherwise, <c>false</c>.
        /// </value>
        public bool UPTOOnly
        {
            get
            {

                return false;
            }
            set
            {
                mUPTOOnly = value;
                RaisePropertyChanged("UPTOOnly");
                Mouse.OverrideCursor = Cursors.Wait;
                OnSettlementUpToChecked();
                SetPortfolio();
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private bool mUTCChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [UTC checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [UTC checked]; otherwise, <c>false</c>.
        /// </value>
        public bool UTCChecked
        {
            get { return mUTCChecked; }
            set
            {
                mUTCChecked = value;
                RaisePropertyChanged("UTCChecked");
                mCRRChecked = false;
                mVirtualChecked = false;
                fillSearchList(true);
            }
        }

        private bool mCRRChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [FTR checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [UTC checked]; otherwise, <c>false</c>.
        /// </value>
        public bool CRRChecked
        {
            get { return mCRRChecked; }
            set
            {
                mCRRChecked = value;
                RaisePropertyChanged("CRRChecked");
                mUTCChecked = false;
                mVirtualChecked = false;
                fillSearchList(true);
            }
        }
        private bool mVirtualChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [Virtual checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Virtual checked]; otherwise, <c>false</c>.
        /// </value>
        public bool VirtualChecked
        {
            get { return mVirtualChecked; }
            set
            {
                mVirtualChecked = value;
                RaisePropertyChanged("VirtualChecked");
                mUTCChecked = false;
                mCRRChecked = false;
                fillSearchList(true);
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [settlement enable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [settlement enable]; otherwise, <c>false</c>.
        /// </value>
        public bool SettlementEnable
        {
            get
            {
                return mSettlementEnable;
            }
            set
            {
                mSettlementEnable = value;
                RaisePropertyChanged("SettlementEnable");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [upto enable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [upto enable]; otherwise, <c>false</c>.
        /// </value>
        public bool UPTOEnable
        {
            get
            {
                return mUPTOEnable;
            }
            set
            {
                mUPTOEnable = value;
                RaisePropertyChanged("UPTOEnable");
            }
        }
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get
            {
                return mCount;
            }
            set
            {
                mCount = value;
                RaisePropertyChanged("Count");
            }
        }



        /// <summary>
        /// Gets or sets a value indicating whether this instance is all day.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is all day; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllDay
        {
            get
            {
                return mIsAllDay;
            }
            set
            {
                mIsAllDay = value;
                RaisePropertyChanged("IsAllDay");
                if (MarketDateTime == DateTime.Today)
                {
                    Task.Factory.StartNew(() => RefreshConstrains());
                }
            }
        }

        /// <summary>
        /// Gets or sets the update time.
        /// </summary>
        /// <value>
        /// The update time.
        /// </value>
        public string UpdateTime
        {
            get
            {
                return mUpdateTime;
            }
            set
            {
                mUpdateTime = value;
                RaisePropertyChanged("UpdateTime");
            }
        }

        /// <summary>
        /// Gets the LMP header.
        /// </summary>
        /// <value>
        /// The LMP header.
        /// </value>
        public string LMPHeader
        {
            get
            {
                string header = ((int)SelectedMarket == 0 ? "" : SelectedMarket.ToString()) + " Locational Marginal Prices";
                return header;
            }
        }

        /// <summary>
        /// Gets the constraints header.
        /// </summary>
        /// <value>
        /// The constraints header.
        /// </value>
        public string ConstraintsHeader
        {
            get
            {
                string header = ((int)SelectedMarket == 0 ? "" : SelectedMarket.ToString()) + " Constraints";
                return header;
            }
        }
        /// <summary>
        /// Gets the markets.
        /// </summary>
        /// <value>
        /// The markets.
        /// </value>
        public IEnumerable<MarketsEnum> Markets
        {
            get
            {
                return Enum.GetValues(typeof(MarketsEnum)).Cast<MarketsEnum>();
            }
        }
        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        public string CurrentUser { get { return Environment.UserName; } }
        /// <summary>
        /// The text change text box
        /// </summary>
        private string _TextChangeTxtBox;
        /// <summary>
        /// Gets or sets the text change text box.
        /// </summary>
        /// <value>
        /// The text change text box.
        /// </value>
        public string TextChangeTxtBox
        {
            get { return _TextChangeTxtBox; }
            set
            {
                _TextChangeTxtBox = value;
                RaisePropertyChanged("TextChangeTxtBox");
            }
        }
        private string mShadowPriceHeader;
        public string ShadowPriceHeader
        {
            get
            {
                return mShadowPriceHeader;
            }
            set
            {
                mShadowPriceHeader = value;
                RaisePropertyChanged("ShadowPriceHeader");
            }
        }

        private List<string> mDistinctZoneList;
        /// <summary>
        /// Gets or sets the zones in zone: dropdown.
        /// </summary>
        /// <value>
        /// The distinct zone list.
        /// </value>
        public List<string> DistinctZoneList
        {
            get
            {
                return mDistinctZoneList;
            }
            set
            {
                mDistinctZoneList = value;
                RaisePropertyChanged("DistinctZoneList");
            }
        }

        #region Relay Command Properties

        /// <summary>
        /// Gets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public DelegateCommand CloseCommand { get; private set; }
        /// <summary>
        /// Gets the loaded command.
        /// </summary>
        /// <value>
        /// The loaded command.
        /// </value>
        public DelegateCommand LoadedCommand { get; private set; }
        /// <summary>
        /// Gets the on current.
        /// </summary>
        /// <value>
        /// The on current.
        /// </value>
        public DelegateCommand OnCurrent { get; private set; }
        /// <summary>
        /// Gets the on refresh.
        /// </summary>
        /// <value>
        /// The on refresh.
        /// </value>
        public DelegateCommand OnRefresh { get; private set; }
        /// <summary>
        /// Gets or sets the navigate constraints.
        /// </summary>
        /// <value>
        /// The navigate constraints.
        /// </value>
        public DelegateCommand NavigateConstraints { get; set; }
        /// <summary>
        /// Gets or sets the analyze constraints.
        /// </summary>
        /// <value>
        /// The analyze constraints.
        /// </value>
        public DelegateCommand AnalyzeConstraints { get; set; }
        public DelegateCommand ConstraintHistory { get; set; }
        public DelegateCommand AllErcotShiftFactors { get; set; }
        /// <summary>
        /// Gets or sets the un pin.
        /// </summary>
        /// <value>
        /// The un pin.
        /// </value>
        public DelegateCommand UnPin { get; set; }
        public DelegateCommand PlayCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        /// <summary>
        /// Gets or sets the add all portfolio.
        /// </summary>
        /// <value>
        /// The add all portfolio.
        /// </value>
        public DelegateCommand AddAllPortfolio { get; set; }
        /// <summary>
        /// Gets or sets the add portfolio.
        /// </summary>
        /// <value>
        /// The add portfolio.
        /// </value>
        public DelegateCommand AddPortfolio { get; set; }
        /// <summary>
        /// Gets or sets the list portfolio selection changed.
        /// </summary>
        /// <value>
        /// The list portfolio selection changed.
        /// </value>
        public DelegateCommand ListPortfolioSelectionChanged { get; set; }
        /// <summary>
        /// Gets or sets the remove portfolio.
        /// </summary>
        /// <value>
        /// The remove portfolio.
        /// </value>
        public DelegateCommand RemovePortfolio { get; set; }
        /// <summary>
        /// Gets or sets the navigate LMP.
        /// </summary>
        /// <value>
        /// The navigate LMP.
        /// </value>
        public DelegateCommand NavigateLMP { get; set; }
        /// <summary>
        /// Gets or sets the load zones.
        /// </summary>
        /// <value>
        /// The load zones.
        /// </value>
        public DelegateCommand LoadZones { get; set; }
        /// <summary>
        /// Gets or sets the load all zones.
        /// </summary>
        /// <value>
        /// The load all zones.
        /// </value>
        public DelegateCommand LoadAllZones { get; set; }
        /// <summary>
        /// Gets or sets the clear zones.
        /// </summary>
        /// <value>
        /// The clear zones.
        /// </value>
        public DelegateCommand ClearZones { get; set; }

        #endregion


        /// <summary>
        /// Gets or sets the navigate constraint locations.
        /// </summary>
        /// <value>
        /// The navigate constraint locations.
        /// </value>
        public PointMapPathLocationList NavigateConstraintLocations
        {
            get
            {
                return mNavigateConstraints;
            }
            set
            {
                mNavigateConstraints = value;
                RaisePropertyChanged("NavigateConstraintLocations");
            }
        }

        /// <summary>
        /// The selected LMP
        /// </summary>
        private LmpData selectedLMP;
        /// <summary>
        /// Gets or sets the selected LMP.
        /// </summary>
        /// <value>
        /// The selected LMP.
        /// </value>
        public LmpData SelectedLMP
        {
            get { return selectedLMP; }
            set { selectedLMP = value; RaisePropertyChanged("SelectedLMP"); OnLMPSelectionChanged(); }
        }

        /// <summary>
        /// The zone list
        /// </summary>
        private ObservableCollection<ZoneInfo> zoneList;
        /// <summary>
        /// Gets or sets the zone list.
        /// </summary>
        /// <value>
        /// The zone list.
        /// </value>
        public ObservableCollection<ZoneInfo> ZoneList
        {
            get { return zoneList; }
            set { zoneList = value; RaisePropertyChanged("ZoneList"); }
        }

        /// <summary>
        /// The zone location list
        /// </summary>
        private ZonePolyLineLocationList zoneLocationList;
        /// <summary>
        /// Gets or sets the zone location list.
        /// </summary>
        /// <value>
        /// The zone location list.
        /// </value>
        public ZonePolyLineLocationList ZoneLocationList
        {
            get { return zoneLocationList; }
            set { zoneLocationList = value; RaisePropertyChanged("ZoneLocationList"); }
        }

        /// <summary>
        /// The navigate LMP locations
        /// </summary>
        private PointMapPathLocationList navigateLMPLocations;
        /// <summary>
        /// Gets or sets the navigate LMP locations.
        /// </summary>
        /// <value>
        /// The navigate LMP locations.
        /// </value>
        public PointMapPathLocationList NavigateLMPLocations
        {
            get { return navigateLMPLocations; }
            set { navigateLMPLocations = value; RaisePropertyChanged("NavigateLMPLocations"); }
        }

        /// <summary>
        /// Gets or sets the node location hash cache.
        /// </summary>
        /// <value>
        /// The node location hash cache.
        /// </value>
        public NodeLocationHash NodeLocationHashCache
        {
            get
            {
                return mNodeHashCache;
            }
            set
            {
                mNodeHashCache = value;
            }
        }

        /// <summary>
        /// Gets or sets the constraint timer.
        /// </summary>
        /// <value>
        /// The constraint timer.
        /// </value>
        public DispatcherTimerEx ConstraintTimer
        {
            get
            {
                return mConstraintTimer;
            }
            set
            {
                mConstraintTimer = value;
            }
        }
        /// <summary>
        /// Gets or sets the LMP timer.
        /// </summary>
        /// <value>
        /// The LMP timer.
        /// </value>
        public DispatcherTimerEx LMPTimer
        {
            get
            {
                return mLmpTimer;
            }
            set
            {
                mLmpTimer = value;
            }
        }

        /// <summary>
        /// Gets or sets the constraint channel.
        /// </summary>
        /// <value>
        /// The constraint channel.
        /// </value>
        public IConstraintInfoProvider ConstraintChannel
        {
            get
            {
                if (mConstraintChannel == null)
                {
                    mConstraintChannel = PrepareFactoryProxy<IConstraintInfoProvider>();
                }
                return mConstraintChannel;
            }
            set
            {
                mConstraintChannel = value;
            }
        }
        /// <summary>
        /// Gets or sets the LMP channel.
        /// </summary>
        /// <value>
        /// The LMP channel.
        /// </value>
        public INodePriceFiveMin LMPChannel
        {
            get
            {
                if (mLMPChannel == null)
                {
                    mLMPChannel = PrepareFactoryProxy<INodePriceFiveMin>();
                }
                return mLMPChannel;
            }
            set { mLMPChannel = value; }
        }

        public ILMPMarketView PriceChannel
        {
            get
            {
                if (mPriceChannel == null)
                {
                    mPriceChannel = PrepareFactoryProxy<ILMPMarketView>();
                }
                return mPriceChannel;
            }
            set { mPriceChannel = value; }
        }

        /// <summary>
        /// Gets or sets the latest constraints list.
        /// </summary>
        /// <value>
        /// The latest constraints list.
        /// </value>
        public ConstraintList LatestConstraintsList
        {
            get
            {
                return mLatestConstraintsList;
            }
            set
            {
                mLatestConstraintsList = value;
                RaisePropertyChanged("LatestConstraintsList");
            }
        }
        /// <summary>
        /// Gets or sets the latest constraints list data.
        /// </summary>
        /// <value>
        /// The latest constraints list data.
        /// </value>
        public ConstraintList LatestConstraintsListData
        {
            get
            {
                return mLatestConstraintsList;
            }
            set
            {
                mLatestConstraintsList = value;
            }
        }

        List<LmpData> lmpHistoricalTempList = new List<LmpData>();
        /// <summary>
        /// Gets or sets the latest LMP list.
        /// </summary>
        /// <value>
        /// The latest LMP list.
        /// </value>
        public LMPDataList LatestLMPList
        {
            get
            {
                return mLatestLMPList;
            }
            set
            {
                mLatestLMPList = value;
                RaisePropertyChanged("LatestLMPList");
            }
        }
        /// <summary>
        /// Gets or sets the location list.
        /// </summary>
        /// <value>
        /// The location list.
        /// </value>
        public PointMapPathLocationList LocationList
        {
            get
            {
                return mLlocationList;
            }
            set
            {
                mLlocationList = value;
                RaisePropertyChanged("LocationList");
            }
        }
        /// <summary>
        /// Gets or sets the poly line constraints.
        /// </summary>
        /// <value>
        /// The poly line constraints.
        /// </value>
        public MultiLocationList PolyLineConstraints
        {
            get
            {
                return mConstraintLocationList;
            }
            set
            {
                mConstraintLocationList = value;
                RaisePropertyChanged("PolyLineConstraints");
            }
        }
        /// <summary>
        /// Gets or sets the eclips line constraints.
        /// </summary>
        /// <value>
        /// The eclips line constraints.
        /// </value>
        public PointMapPathLocationList EclipsLineConstraints
        {
            get
            {
                return mEclipsConstraintLocationList;
            }
            set
            {
                mEclipsConstraintLocationList = value;
                RaisePropertyChanged("EclipsLineConstraints");
            }
        }
        //public LocationList LocationDataList { get; private set; }
        /// <summary>
        /// Gets the m settlent locations list.
        /// </summary>
        /// <value>
        /// The m settlent locations list.
        /// </value>
        public Hashtable mSettlentLocationsList { get; private set; }
        /// <summary>
        /// Gets the m node zone list.
        /// </summary>
        /// <value>
        /// The m node zone list.
        /// </value>
        public Dictionary<string, string> mNodeZoneList { get; private set; }
        /// <summary>
        /// Gets or sets the navigate visibility.
        /// </summary>
        /// <value>
        /// The navigate visibility.
        /// </value>
        public Visibility NavigateVisibility
        {
            get
            {
                return mNavigateVisibility;
            }
            set
            {
                //mNavigateVisibility = value;
                RaisePropertyChanged("NavigateVisibility");
            }
        }

        /// <summary>
        /// The l mp navigate visibility
        /// </summary>
        private Visibility lMPNavigateVisibility;
        /// <summary>
        /// Gets or sets the LMP navigate visibility.
        /// </summary>
        /// <value>
        /// The LMP navigate visibility.
        /// </value>
        public Visibility LMPNavigateVisibility
        {
            get { return lMPNavigateVisibility; }
            set { lMPNavigateVisibility = value; RaisePropertyChanged("LMPNavigateVisibility"); }
        }

        /// <summary>
        /// The portfolio list
        /// </summary>
        private ObservableCollection<Portfolio> portfolioList;
        /// <summary>
        /// Gets or sets the portfolio list.
        /// </summary>
        /// <value>
        /// The portfolio list.
        /// </value>
        public ObservableCollection<Portfolio> PortfolioList
        {
            get { return portfolioList; }
            set { portfolioList = value; RaisePropertyChanged("PortfolioList"); }
        }

        /// <summary>
        /// The selected portfolio list item
        /// </summary>
        private Portfolio selectedPortfolioListItem;
        /// <summary>
        /// Gets or sets the selected portfolio list item.
        /// </summary>
        /// <value>
        /// The selected portfolio list item.
        /// </value>
        public Portfolio SelectedPortfolioListItem
        {
            get { return selectedPortfolioListItem; }
            set { selectedPortfolioListItem = value; RaisePropertyChanged("SelectedPortfolioListItem"); }
        }

        /// <summary>
        /// The m map locations
        /// </summary>
        private LocationCollection mMapLocations;
        /// <summary>
        /// Gets or sets the map location.
        /// </summary>
        /// <value>
        /// The map location.
        /// </value>
        public LocationCollection MapLocation
        {
            get { return mMapLocations; }
            set
            {
                mMapLocations = value;
                RaisePropertyChanged("MapLocation");
            }
        }

        #endregion

        public MainWindowViewModel(IDataService _dataService = null)
        {
            if (_dataService == null)
            {
                mDataService = new Model.DataService();
            }
            else
            {
                mDataService = _dataService;
            }
            _playTimer = new DispatcherTimer();
            _playTimer.Interval = TimeSpan.FromMilliseconds(2); // Adjust interval as needed
            _playTimer.Tick += PlayTimer_Tick;
            ListstrDeenergizedNodes = mDataService.GetDeenergizedNodes();
            //Vayu.WorkbookStatistics.Model.DataService workBookObj = new WorkbookStatistics.Model.DataService();
            //mMapCenterLocation = new Location(33.3683, -95.2734); 
            CloseCommand = new DelegateCommand(() => CloseAndUnsubscribe());
            LoadedCommand = new DelegateCommand(() => WindowLoaded());
            OnRefresh = new DelegateCommand(() => RefereshData());
            NavigateConstraints = new DelegateCommand(() => OnNavigateConstraints());
            AnalyzeConstraints = new DelegateCommand(() => OnAnalyzeConstraints());
            ConstraintHistory = new DelegateCommand(() => OnConstraintHistory());
            AllErcotShiftFactors = new DelegateCommand(GetAllErcotShiftFactors);
            NavigateLMP = new DelegateCommand(() => OnNavigateLMP());
            AddAllPortfolio = new DelegateCommand(() => OnAddAllPortfolio());
            AddPortfolio = new DelegateCommand(() => OnAddPortfolio());
            RemovePortfolio = new DelegateCommand(() => OnRemovePortfolio());
            UnPin = new DelegateCommand(() => OnUnPin());
            PlayCommand = new DelegateCommand(() => StartPlay());
            StopCommand = new DelegateCommand(() => StopPlay());
            LoadAllZones = new DelegateCommand(() => OnLoadAllZones());

            mSettlentLocationsList = new Hashtable();
            LatestConstraintsList = new ConstraintList();
            LatestLMPList = new LMPDataList();
            PortfolioList = new ObservableCollection<Portfolio>();
            PolyLineConstraints = new MultiLocationList();
            EclipsLineConstraints = new PointMapPathLocationList();
            NavigateConstraintLocations = new PointMapPathLocationList();
            mMarketDateTime = DateTime.Now;
            SliderValue = 576;
            SliderDisplayTime = MarketDateTime.AddHours(-48).ToString("MMM dd, HH:mm");
            IsHistoricalChecked = false;
            SelectedMarket = Markets.First();

        }
        #region Private Methods

        /// <summary>
        /// Called when [clear zones].
        /// </summary>
        private void OnClearZones()
        {
            if (ZoneList != null)
                return;

            foreach (var item in ZoneList)
                item.IsSelected = false;

            OnLoadZones();
        }

        /// <summary>
        /// Called when [load all zones].
        /// </summary>
        private void OnLoadAllZones()
        {
            if (ZoneList != null)
                return;

            foreach (var item in ZoneList)
                item.IsSelected = true;

            OnLoadZones();
        }

        /// <summary>
        /// Called when [un pin].
        /// </summary>
        private void OnUnPin()
        {
            LMPNavigateVisibility = Visibility.Hidden;
            NavigateLMPLocations = new PointMapPathLocationList();
        }
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (SliderValue > 0 && SliderValue <= 576) // Ensure it doesn't exceed maximum value
            {
                SliderValue--;
                // RefreshLMP();
            }
            else
            {
                StopPlay(); // Stop playback when reaching the end
                IsHistoricalChecked = false;
                RaisePropertyChanged("IsHistoricalChecked");

            }
        }
        private DispatcherTimer _playTimer;
        private bool _isPlaying = false;
        public void StartPlay()
        {
            if (!_isPlaying)
            {
                _isPlaying = true;
                _playTimer.Start();
            }
        }

        public void StopPlay()
        {
            if (_isPlaying)
            {
                _isPlaying = false;
                _playTimer.Stop();
            }
        }
        public void Dispose()
        {
            if (_playTimer != null)
            {
                _playTimer.Stop();
                _playTimer.Tick -= PlayTimer_Tick;
                _playTimer = null;
            }
        }

        /// <summary>
        /// Windows the loaded.
        /// </summary>
        private void WindowLoaded()
        {
            RaisePropertyChanged("Markets");
        }

        /// <summary>
        /// Called when [analyze constraints].
        /// </summary>
        private void OnAnalyzeConstraints()
        {
        }
        private List<Constraint> mHistoryConstraintList;
        public List<Constraint> HistoryConstraintList
        {
            get { return mHistoryConstraintList; }
            set
            {
                mHistoryConstraintList = value;
                RaisePropertyChanged("HistoryConstraintList");
            }
        }

        private void OnConstraintHistory()
        {
            DataService ds = new DataService();
            LatestConstraint latest = SelectedConstrain as LatestConstraint;
            string ConstraintName = latest.ConstraintText;
            string FromToStation = latest.FromStation + "-" + latest.ToStation;
            string ToFromStation = latest.ToStation + "-" + latest.FromStation;
            Dictionary<string, string> CTList = ds.getConstraintByRT(latest.ConstraintText, latest.ContigencyText);
            if (CTList.ContainsKey(FromToStation))
            {
                ConstraintName = CTList[FromToStation];
            }
            else if (CTList.ContainsKey(ToFromStation))
            {
                ConstraintName = CTList[ToFromStation];
            }
            HistoryConstraintList = null;
            HistoryConstraintList = ds.GetAllHistoricalConstraintsData(ConstraintName, latest.ContigencyText, true, 9);
            ConstraintHistory view = new ConstraintHistory();
            view.DataContext = new ConstraintHistoryViewModel(this, HistoryConstraintList, latest.ConstraintText, latest.ContigencyText);
            view.Title = "Constraint History";
            view.Show();

        }

        private void GetAllErcotShiftFactors()
        {
            LatestConstraint latest = SelectedConstrain as LatestConstraint;
            try
            {
                if (latest.ConstraintText == null)
                {
                    MessageBox.Show("Please select a constraint");
                    return;
                }

                bool isDA = false;

                string constraintName = latest.ConstraintText;
                string contingencyName = latest.ContigencyText;

                DataService ds = new DataService();
                string FromToStation = latest.FromStation + "-" + latest.ToStation;
                string ToFromStation = latest.ToStation + "-" + latest.FromStation;
                Dictionary<string, string> CTList = ds.getConstraintByRT(latest.ConstraintText, latest.ContigencyText);
                if (CTList.ContainsKey(FromToStation))
                {
                    constraintName = CTList[FromToStation];
                }
                else if (CTList.ContainsKey(ToFromStation))
                {
                    constraintName = CTList[ToFromStation];
                }
                List<SensitivityHelper> tempsensitivityList = ds.GetErcotSensitivities(constraintName, contingencyName, isDA);
                if (tempsensitivityList != null && tempsensitivityList.Count > 0)
                {
                    SensitivityHelper firstHelper = tempsensitivityList.First();
                    ErcotSensitivities view = new ErcotSensitivities();
                    view.DataContext = new ErcotSensitivitiesViewModel(tempsensitivityList, firstHelper.ConstraintId, constraintName, contingencyName, isDA);
                    view.Title = "Sensitivity";
                    view.Show();
                }
                else
                {
                    MessageBox.Show("No Sensitivities found for the selected constraint");
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Please Retry it.");
            }

        }

        /// <summary>
        /// Called when [add all portfolio].
        /// </summary>
        private void OnAddAllPortfolio()
        {
            PortfolioList = new ObservableCollection<Portfolio>(TraderPortfolioList);
            PortfolioList.Remove(allPortfolio);


            //Task lmpTask = Task.Run(() => RefreshLMP());
            PortfolioSelection();
        }

        /// <summary>
        /// Called when [add portfolio].
        /// </summary>
        private void OnAddPortfolio()
        {
            //if (PortfolioList.FirstOrDefault(x => x.CompareTo(TraderPortfolioSelectedItem) >= 0) == null)
            //    PortfolioList.Add(TraderPortfolioSelectedItem);


            if (!(portfolioList.Contains(TraderPortfolioSelectedItem)))
            {
                PortfolioList.Add(TraderPortfolioSelectedItem);
            }


            //Task lmpTask = Task.Run(() => RefreshLMP());
            PortfolioSelection();
        }

        /// <summary>
        /// Fills the search list.
        /// </summary>
        /// <param name="isRt">if set to <c>true</c> [is rt].</param>
        private void fillSearchList(bool isRt)
        {
            DataService ds = new DataService();

            if (UTCChecked)
            {
                OnSettlementUpToChecked();
                SetPortfolio();
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            if (VirtualChecked)
            {
                OnSettlementUpToChecked();
                SetPortfolio();
                Mouse.OverrideCursor = Cursors.Arrow;
            }

        }

        /// <summary>
        /// Called when [remove portfolio].
        /// </summary>
        private void OnRemovePortfolio()
        {
            if (PortfolioList != null && PortfolioList.Count != 0 && SelectedPortfolioListItem != null)
                PortfolioList.Remove(SelectedPortfolioListItem);

            if (PortfolioList != null && PortfolioList.Count == 0)
                TraderPortfolioSelectedItem = allPortfolio;

            //Task lmpTask = Task.Run(() => RefreshLMP());
            PortfolioSelection();
        }

        /// <summary>
        /// Called when [navigate constraints].
        /// </summary>
        private void OnNavigateConstraints()
        {
            LatestConstraint constrain = SelectedConstrain as LatestConstraint;
            if (constrain == null)
                return;

            //ConstraintRTGeoHashCache.UpdateConstraintHash(new LatestConstraint[] { constrain }, (int)SelectedMarket);
            //ConstraintRTGeo geo = ConstraintRTGeoHashCache[constrain.ConstraintKey] as ConstraintRTGeo;

            //if (geo == null)
            //    return;

            NodeGeoDetail source = NodeLocationHashCache[constrain.SourceNodeKey] as NodeGeoDetail;
            NodeGeoDetail sink = NodeLocationHashCache[constrain.SinkNodeKey] as NodeGeoDetail;
            List<PointMapPath> eclipsLine = new List<PointMapPath>();

            ConstructPointMapPath(source, constrain, eclipsLine);
            PointMapPath point = ConstructPointMapPath(sink, constrain, eclipsLine);

            if (point != null)
                point.MyColor = Brushes.OrangeRed;

            if (eclipsLine.Count != 0)
                MapCenterLocation = eclipsLine[0].MapLocation;

            NavigateVisibility = Visibility.Visible;
            NavigateConstraintLocations = new PointMapPathLocationList(eclipsLine);
        }

        /// <summary>
        /// Called when [navigate LMP].
        /// </summary>
        private void OnNavigateLMP()
        {
            LmpData sel = SelectedLMP;
            if (sel == null) return;
            NodeGeoDetail source = NodeLocationHashCache[sel.NodeKey] as NodeGeoDetail;
            if (source == null) return;

            // Keep existing eclipsLine bookkeeping so overlays continue to work.
            var eclipsLine = NavigateLMPLocations != null
                ? new List<PointMapPath>(NavigateLMPLocations)
                : new List<PointMapPath>();
            PointMapPath point = ConstructPointMapPath(source, null, eclipsLine);
            if (point != null) point.MyColor = Brushes.OrangeRed;

            LMPNavigateVisibility = Visibility.Visible;
            NavigateLMPLocations = new PointMapPathLocationList(eclipsLine);

            // Reuse the node-centering + pin-placement path
            LmpNodeSelected?.Invoke(this, source);
        }
        /// <summary>
        /// Sets the portfolio.
        /// </summary>
        private void SetPortfolio()
        {
            bool isUpt = true;// (UPTOOnly & UPTOEnable);
            string product = isUpt ? "EES/PTP" : "Virtual";
            string MARKET = SelectedMarket.ToString();
            List<Portfolio> portfolioList = DBAccess.GetPortfolio(MarketDateTime, mUser, product, SelectedMarket.ToString());
            foreach (Portfolio portfolio in portfolioList)
                portfolio.IsUptos = isUpt;

            //portfolioBindList.Add()
            //allPortfolio = new Portfolio() { Name = "ALL" };
            //portfolioList.Add(allPortfolio);

            TraderPortfolioList = new System.Collections.ObjectModel.ObservableCollection<Portfolio>(portfolioList);
            TraderPortfolioSelectedItem = allPortfolio;

            if (PortfolioList != null)
                PortfolioList.Clear();
        }
        /// <summary>
        /// Called when [settlement up to checked].
        /// </summary>
        private async void OnSettlementUpToChecked()
        {
            if (MarketDateTime.Date == DateTime.Today.Date)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Task updateTask = null;
                if (LatestLMPList == null || LatestLMPList.OriginalList == null || LatestLMPList.Count == 0 ||
                    LatestLMPList.OriginalList.Count() == 0)
                {
                    updateTask = Task.Run(() => RefreshLMP());
                }
                else
                {
                    updateTask = Task.Run(() => ProcessSettlementUpTos(LatestLMPList.OriginalList, LatestLMPList.MarketKey));
                }
                await updateTask;
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// Called when [constraint selection changed].
        /// </summary>
        private void OnConstraintSelectionChanged()
        {
            NavigateVisibility = Visibility.Hidden;
            NavigateConstraintLocations = new PointMapPathLocationList();
        }
        /// <summary>
        /// Raised whenever the LMP DataGrid selection changes.
        /// The View subscribes to this to pan/zoom the map to the selected
        /// node and paint a temporary green highlight for ~2 seconds.
        /// (Added as part of Mapsui/OSM migration.)
        /// </summary>
        public event EventHandler<NodeGeoDetail> LmpNodeSelected;

        /// <summary>
        /// Called when [LMP selection changed].
        /// </summary>
        private void OnLMPSelectionChanged()
        {
            if (selectedLMP == null) return;
            var node = NodeLocationHashCache != null
                ? NodeLocationHashCache[selectedLMP.NodeKey] as NodeGeoDetail
                : null;
            if (node == null) return;

            // View handles pan/zoom + green highlight via this event.
            // Do NOT also set MapCenterLocation here — it triggers a second
            // navigation at DefaultResolution which overwrites the medium zoom.
            LmpNodeSelected?.Invoke(this, node);
        }
        /// <summary>
        /// Refereshes the data.
        /// </summary>
        private async void RefereshData()
        {
            RefreshLMPAsync();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(30); // 30 minutes
            timer.Tick += (sender, args) =>
            {
                // Call your method here
                RefreshLMPAsync();
            };
            timer.Start();
            if (!IsHistoricalChecked)
            {

                Mouse.OverrideCursor = Cursors.Wait;
                Task.WaitAll(locationTask);
                Task constrainsTask = Task.Run(() => RefreshConstrains());
                Task lmpTask = Task.Run(() => RefreshLMP());
                await Task.WhenAll(constrainsTask, lmpTask);
                StartTimers();
                LMPTimer.Start();
                LMPChannel.Subscribe(9);
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            else
            {
                LMPTimer.Stop();
                LMPChannel.Unsubscribe();

            }
        }


        private async void RefreshLMPAsync()
        {
            Task lmpTask = Task.Run(() => RefreshAsync());
            await Task.WhenAll(lmpTask);

        }

        private void RefreshAsync()
        {
            Node[] latestNodes = null;

            latestNodes = PriceChannel.GetAllFiveMinPrice(9, MarketDateTime.AddHours(-48), MarketDateTime.AddDays(1), false);
            historcalNodes = latestNodes;

        }
        /// <summary>
        /// Refreshes the LMP.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        private void RefreshLMP(Node[] nodes = null)
        {
            try
            {
                if (nodes != null && nodes.Count() != 0)
                {
                    Node node = nodes.FirstOrDefault();
                    if (LatestLMPList != null && LatestLMPList.MarketKey != node.Market)
                    {
                        return;
                    }
                }

                mSettlentLocationsList = mDataService.GetSettlementLocations((int)SelectedMarket);
                mNodeZoneList = mDataService.GetNodeZone((int)SelectedMarket);

                SetLatestLmp((int)SelectedMarket, nodes);

                UpdateTime = MarketDateTime.ToString("MM-dd ") + DateTime.Now.ToString("HH:mm:ss");
                MarketDateTime = mMarketDateTime;
                SliderDisplayTime = mMarketDateTime.ToString("MMM dd, HH:mm");
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Refreshes the constrains.
        /// </summary>
        /// <param name="list">The list.</param>
        private void RefreshConstrains(List<LatestConstraint> list = null)
        {

            if ((int)SelectedMarket == 9)
                ShadowPriceHeader = "Contingency";
            if (!Enum.IsDefined(SelectedMarket.GetType(), SelectedMarket) || ConstraintChannel == null)
            {
                return;
            }
            try
            {
                List<LatestConstraint> constraintsList = list;
                if (list == null)
                {
                    if (MarketDateTime != DateTime.Today)
                    {
                        //IsAllDay = true;
                        IsAllDay = false;
                    }
                    constraintsList = ConstraintChannel.GetNSAActiveConstraint((int)SelectedMarket,
                        MarketDateTime, IsAllDay);
                }
                try
                {
                    lock (lockConstraint)
                    {
                        if (constraintsList == null || constraintsList.Count == 0)
                        {
                            LatestConstraintsList = new ConstraintList();
                        }

                        else
                        {
                            LoadSinkSourceZoneAndZoneFilter(constraintsList);
                            //LoadSinkSourceZoneAndFilterByZone(constraintsList);
                            //if (IsAllDay)
                            //{
                            //    if (ZoneSelectedItem != null)
                            //    {
                            //        //LatestConstraintsListData = new ConstraintList(constraintsList.Where(z => z.SourceZone == ZoneSelectedItem || z.SinkZone == ZoneSelectedItem).OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //        //LoadSinkSourceZone();
                            //        //LatestConstraintsList = new ConstraintList(constraintsList.Where(z => z.SourceZone == ZoneSelectedItem || z.SinkZone == ZoneSelectedItem).OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //        LoadSinkSourceZoneAndZoneFilter(constraintsList);
                            //    }
                            //    else
                            //    {
                            //        //LatestConstraintsListData = new ConstraintList(constraintsList.OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //        //LoadSinkSourceZone();
                            //        //LatestConstraintsList = new ConstraintList(constraintsList.OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //    }

                            //}
                            //else
                            //{
                            //    //if (ZoneSelectedItem != null)
                            //    //{
                            //    //    DateTime maxDateTime = constraintsList.Max(x => x.MarketDate);
                            //    //    LatestConstraintsListData = new ConstraintList(constraintsList.Where(a => a.MarketDate == maxDateTime && (a.SourceZone == ZoneSelectedItem || a.SinkZone == ZoneSelectedItem)).
                            //    //        OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //    //    LoadSinkSourceZone();
                            //    //    DateTime MaxDateTime = constraintsList.Max(x => x.MarketDate);
                            //    //    LatestConstraintsList = new ConstraintList(constraintsList.Where(a => a.MarketDate == MaxDateTime && (a.SourceZone == ZoneSelectedItem || a.SinkZone == ZoneSelectedItem)).
                            //    //        OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //    //}
                            //    //else
                            //    //{
                            //    //    DateTime maxDateTime = constraintsList.Max(x => x.MarketDate);
                            //    //    LatestConstraintsListData = new ConstraintList(constraintsList.Where(a => a.MarketDate == maxDateTime).
                            //    //        OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //    //    LoadSinkSourceZone();
                            //    //    DateTime MaxDateTime = constraintsList.Max(x => x.MarketDate);
                            //    //    LatestConstraintsList = new ConstraintList(constraintsList.Where(a => a.MarketDate == MaxDateTime).
                            //    //        OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                            //    //}
                            //}
                        }
                        RefreshConstraintLayer();
                        RefreshLMP();
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex1)
            {
                mConstraintChannel = null;
            }
        }

        /// <summary>
        /// Constructs the point map path.
        /// </summary>
        /// <param name="bus">The bus.</param>
        /// <param name="item">The item.</param>
        /// <param name="pointLine">The point line.</param>
        /// <returns></returns>
        private PointMapPath ConstructPointMapPath(NodeGeoDetail bus, LatestConstraint item, List<PointMapPath> pointLine)
        {
            PointMapPath mapPath = null;
            if (bus == null)
            {
                return mapPath;
            }
            mapPath = new PointMapPath();
            mapPath.MapLocation = bus.MapLocation;
            mapPath.ToolTipText = bus.BusName + "\r\nVolt: " + (item != null ? item.ConstraintKV.ToString() : "");
            mapPath.Name = bus.BusName;
            if (pointLine != null)
                pointLine.Add(mapPath);

            return mapPath;
        }
        /// <summary>
        /// Portfolioes the selection.
        /// </summary>
        private void PortfolioSelection()
        {
            ObservableCollection<PortfolioBid> PortfolioBidList = new ObservableCollection<PortfolioBid>();
            SetPortfolio();
            if (PortfolioList == null)
                return;


            //mBidList = DBAccess.GetCurrentBids(SelectedMarket.ToString(), PortfolioList.Select(x => x.ID), MarketDateTime, (UPTOOnly & UPTOEnable));
            //mBidList = DBAccess.GetCurrentBids(SelectedMarket.ToString(), PortfolioList.Select(x => x.ID), MarketDateTime, (true));
            mBidList = DBAccess.GetCurrenERCOTBids(SelectedMarket.ToString(), PortfolioList.Select(x => x.ID), MarketDateTime, (true));
            ProcessSettlementUpTos(LatestLMPList.OriginalList, (int)SelectedMarket);
        }
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        private int GetMarketKey(string market)
        {

            if (market == "ERCOT")
            {
                return 9;
            }
            return 9;
        }
        /// <summary>
        /// Refreshes the constraint layer.
        /// </summary>
        private void RefreshConstraintLayer()
        {
            lock (lockConstraintMap)
            {
                List<MultiMapPath> polyLine = new List<MultiMapPath>();
                List<PointMapPath> eclipsLine = new List<PointMapPath>();
                foreach (var item in LatestConstraintsList)
                {
                    NodeGeoDetail source = NodeLocationHashCache[item.SourceNodeKey] as NodeGeoDetail;
                    NodeGeoDetail sink = NodeLocationHashCache[item.SinkNodeKey] as NodeGeoDetail;
                    if (source != null && sink != null && item.SourceNodeKey != item.SinkNodeKey)
                    {
                        MultiMapPath mapPath = new MultiMapPath();
                        mapPath.Locations.Add(source.MapLocation);
                        mapPath.Name += source.BusName;
                        mapPath.Locations.Add(sink.MapLocation);
                        mapPath.Name += " " + sink.BusName;
                        mapPath.AddExtraPoints();
                        polyLine.Add(mapPath);
                    }
                    else
                    {
                        PointMapPath point1 = ConstructPointMapPath(source, item, eclipsLine);
                        PointMapPath point2 = ConstructPointMapPath(sink, item, eclipsLine);
                    }
                }

                EclipsLineConstraints = new PointMapPathLocationList(eclipsLine);
                PolyLineConstraints = new MultiLocationList(polyLine);
            }

            if (Updatemap != null)
                Updatemap(this, new EventArgs());
        }

        /// <summary>
        /// Refreshes the constraint raise property changed method.
        /// </summary>
        public void RefreshConstraint_RaisePropertyChanged()
        {
            RefreshConstraintLayer();
            RaisePropertyChanged("MapCenterLocation");
            if (Updatemap != null)
                Updatemap(this, new EventArgs());
        }
        /// <summary>
        /// Starts the timers.
        /// </summary>
        private void StartTimers()
        {
            if (ConstraintTimer == null)
            {
                ConstraintTimer = DispatcherTimerEx.Create(ConstraintTimer_Tick);
            }
            ConstraintTimer_Tick(ConstraintTimer, new EventArgs());
            if (LMPTimer == null)
            {
                LMPTimer = DispatcherTimerEx.Create(LMPTimer_Tick);
            }
            else
            {
                LMPTimer_Tick(LMPTimer, new EventArgs());
            }
        }

        /// <summary>
        /// Closes the and unsubscribe.
        /// </summary>
        private void CloseAndUnsubscribe()
        {
            Action<DispatcherTimerEx> ShutdownTimers = (timer) =>
            {
                if (timer == null)
                    return;

                timer.ShutdownInProgress = true;
                timer.Shutdown();
            };
            ShutdownTimers(ConstraintTimer);
            ShutdownTimers(LMPTimer);
            ConstraintTimer = null;
            LMPTimer = null;
            if (ConstraintChannel != null)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ConstraintChannel.Unsubscribe();
                        mConstraintChannel = null;
                    }
                    catch { }
                });

            }
            if (LMPChannel != null)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        LMPChannel.Unsubscribe();
                        mLMPChannel = null;
                    }
                    catch { }
                });
            }
        }
        /// <summary>
        /// Sets the latest LMP.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="nodesArg">The nodes argument.</param>
        private void SetLatestLmp(int marketKey, Node[] nodesArg = null)
        {
            if (IsHistoricalChecked)
            {
                //Action nullReturn = () => { PriceChannel = null; };
                //if (!Enum.IsDefined(SelectedMarket.GetType(), SelectedMarket) || PriceChannel == null)
                //{
                //    return;
                //}

            }
            else
            {
                Action nullReturn = () => { LMPChannel = null; };
                if (!Enum.IsDefined(SelectedMarket.GetType(), SelectedMarket) || LMPChannel == null)
                {
                    return;
                }

            }

            try
            {
                lock (lockLmpObj)
                {
                    Node[] latestNodes = nodesArg;
                    //if (latestNodes == null && IsHistoricalChecked)
                    //{
                    //    latestNodes = PriceChannel.GetAllFiveMinPrice(marketKey, MarketDateTime.AddHours(-48), MarketDateTime.AddDays(1), false);
                    //    historcalNodes = latestNodes;
                    //}
                    //else
                    if (latestNodes == null)
                    {
                        latestNodes = LMPChannel.GetNodesForMarket(marketKey);
                    }
                    if (latestNodes == null && latestNodes.Count() == 0)
                        goto ReturnBlank;

                    var missingNames = latestNodes.Where(x => x.NodeName == null);
                    foreach (var item in missingNames)
                    {
                        PricingNode node = DBAccess.GetNode(item.NodeId, marketKey);
                        item.NodeName = node.NodeName;
                    }

                    List<LmpData> lmpTempList = new List<LmpData>();
                    var tempItems = from a in latestNodes
                                    where a.LmpTimePriceList != null
                                    select new
                                    {
                                        lmp = a.LmpTimePriceList.OrderByDescending(k => k.MarketTime).FirstOrDefault(),
                                        nodename = a.NodeName,
                                        nodekey = a.NodeId,
                                        //EnergyPrice = a.
                                        isdeenergized = ListstrDeenergizedNodes.Contains(a.NodeName)
                                    };

                    foreach (var item in tempItems)
                    {
                        if (item.lmp == null)
                            continue;
                        if (dictZones.ContainsKey(item.nodename))
                            lmpTempList.Add(new LmpData(item.lmp, item.nodekey, item.nodename, dictZones[item.nodename], item.isdeenergized));
                    }

                    if (lmpTempList.Count == 0)
                        goto ReturnBlank;

                    lmpTempList = lmpTempList.OrderBy(a => a.NodeName).ToList();
                    ProcessSettlementUpTos(lmpTempList, marketKey);

                ReturnBlank:;
                }
            }
            catch (Exception ex)
            {
                mLMPChannel = null;
            }
        }

        public Dictionary<string, string> GetZone()
        {
            Dictionary<string, string> dictZone = new Dictionary<string, string>();
            try
            {
                dictZone = mDataService.GetNodeZone(GetMarketKey(SelectedMarket.ToString()));
            }
            catch
            {

            }
            return dictZone;
        }



        private List<LmpData> originalLMPList;
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    RaisePropertyChanged(nameof(SearchText));
                    FilterDataGrid();
                }
            }
        }


        private void FilterDataGrid()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                LatestLMPList = new LMPDataList(originalLMPList);
            }
            else
            {
                var searchTerms = SearchText.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(term => term.Trim())
                                             .ToList();

                var filteredList = originalLMPList
                    .Where(x => searchTerms.Any(term => x.NodeName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();

                var sortedFilteredList = filteredList
                    .OrderByDescending(x => searchTerms.Any(term => x.NodeName.Equals(term, StringComparison.OrdinalIgnoreCase)))
                    .ThenBy(x => x.NodeName)
                    .ToList();

                var remainingList = originalLMPList
                    .Where(x => !filteredList.Contains(x))
                    .ToList();

                var combinedList = sortedFilteredList.Concat(remainingList).ToList();

                LatestLMPList = new LMPDataList(combinedList);
            }
        }





        /// <summary>
        /// Processes the settlement up tos.
        /// </summary>
        /// <param name="lmpList">The LMP list.</param>
        /// <param name="marketKey">The market key.</param>
        private void ProcessSettlementUpTos(IEnumerable<LmpData> lmpList, int marketKey)
        {
            try
            {
                if (lmpList == null)
                    return;

                lock (lockSettlementUpToObj)
                {
                    List<LmpData> tempList = null;
                    List<LmpData> filteredNodes = new List<LmpData>();

                    if (SettlementOnly)
                        tempList = lmpList.Where(x => mSettlentLocationsList.ContainsKey(x.NodeKey)).ToList();
                    else
                        tempList = lmpList.ToList();
                    if (marketKey == (int)MarketsEnum.ERCOT && UPTONodes != null)
                    {
                        List<int> nodeKeys = new List<int>();
                        if (UTCChecked)
                            nodeKeys = NodeHelper.Instance.GetErcotUpToNodeKeys("ERCOT");
                        else
                            nodeKeys = NodeHelper.Instance.GetErcotUpToNodeKeys("ERCOT", false);

                        mUptoNodes = new Hashtable(nodeKeys.ToDictionary(x => x));
                        foreach (var item in tempList)
                        {
                            if (UPTONodes.ContainsKey(item.NodeKey))
                                filteredNodes.Add(item);
                        }
                    }

                    if (mBidList != null && PortfolioList != null && PortfolioList.Count != 0)
                        filteredNodes = lmpList.Where(s => mBidList.Contains(s.NodeKey)).ToList();
                    Tuple<string, Dictionary<int, double>> DACongHash = mDataService.GetDACongestionByNodeAndHour(DateTime.Now);
                    if (!IsHistoricalChecked)
                    {
                        DACongHash = mDataService.GetDACongestionByNodeAndHour(DateTime.Now);
                        while (DACongHash.Item1.Length != 0)
                        {
                            DACongHash = mDataService.GetDACongestionByNodeAndHour(DateTime.Now);
                        }
                    }
                    else
                    {
                        DateTime dt = DateTime.Parse(filteredNodes[0].DateTime);
                        DACongHash = mDataService.GetDACongestionByNodeAndHour(dt);
                        while (DACongHash.Item1.Length != 0)
                        {
                            DACongHash = mDataService.GetDACongestionByNodeAndHour(dt);
                        }
                    }
                    foreach (var node in filteredNodes)
                    {
                        node.DACongestion = DACongHash.Item2.ContainsKey(node.NodeKey) ? DACongHash.Item2[node.NodeKey] : 0;
                        node.DART = node.Congestion - node.DACongestion;
                    }

                    var filteredDistinctNodes = filteredNodes.GroupBy(d => new { d.NodeName }).Select(y => y.First());
                    LatestLMPList = new LMPDataList(filteredDistinctNodes, lmpList);
                    if (LatestLMPList.Count() < 700)
                    {
                        RefreshLMP();
                    }
                    if (ZoneSelectedItem != null && ZoneSelectedItem != "All")
                    {
                        var orderlist = new LMPDataList(LatestLMPList.Where(i => i.Zone == ZoneSelectedItem));
                        LatestLMPList = orderlist;
                    }
                    LatestLMPList.MarketKey = marketKey;
                    TextChangeTxtBox = marketKey.ToString();
                    //originalLMPList = lmpList.ToList();
                    originalLMPList = LatestLMPList.ToList();
                    LatestLMPList = new LMPDataList(originalLMPList);
                }
            }
            catch (Exception ex)
            {

            }

        }

        /// <summary>
        /// Prepares the factory proxy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T PrepareFactoryProxy<T>()
            where T : class
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.OpenTimeout = new TimeSpan(6, 0, 0);
            binding.CloseTimeout = new TimeSpan(6, 0, 0);
            binding.ReceiveTimeout = new TimeSpan(6, 0, 0);
            binding.SendTimeout = new TimeSpan(6, 0, 0);
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.MaxBufferSize = int.MaxValue;
            binding.TransferMode = TransferMode.Buffered;
            binding.Security.Mode = SecurityMode.None;
            binding.TransactionFlow = false;
            binding.ReaderQuotas.MaxArrayLength = int.MaxValue;

            try
            {
                if (typeof(T) == typeof(IConstraintInfoProvider))
                {
                    DuplexChannelFactory<IConstraintInfoProvider> mConstraintFactory = new DuplexChannelFactory<IConstraintInfoProvider>
                        (new InstanceContext(new ConstraintCallback(this)), binding, Vayu.CommonAccessLibrary.ServiceConnections.GetLatestConstraintService());
                    T channel = mConstraintFactory.CreateChannel() as T;
                    mConstraintFactory.Faulted += OnChannelFactory_Faulted;

                    return channel;
                }
                else if (typeof(T) == typeof(INodePriceFiveMin))
                {
                    binding.TransactionFlow = false;
                    DuplexChannelFactory<INodePriceFiveMin> mLMPFactory = new DuplexChannelFactory<INodePriceFiveMin>
                        (new InstanceContext(new LmpCallback(this)), binding, Vayu.CommonAccessLibrary.ServiceConnections.GetLMPFiveMinService());
                    T channel = mLMPFactory.CreateChannel() as T;
                    mLMPFactory.Faulted += OnChannelFactory_Faulted;

                    return channel;
                }
                else
                {
                    ChannelFactory<ILMPMarketView> mLMPFactory = new ChannelFactory<ILMPMarketView>
                         (binding, Vayu.CommonAccessLibrary.ServiceConnections.GetMarketViewService());
                    T channel = mLMPFactory.CreateChannel() as T;
                    mLMPFactory.Faulted += OnChannelFactory_Faulted;

                    return channel;
                }
            }
            catch (Exception ex)
            {

            }

            return default(T);
        }

        #endregion

        #region Public Methods

        public void SetErcotZoneList()
        {

        }

        /// <summary>
        /// Called when [load zones].
        /// </summary>
        public void OnLoadZones()
        {
            ZonePolyLineLocationList locList = new ZonePolyLineLocationList();
            foreach (var item in ZoneList)
            {
                if (!item.IsSelected)
                    continue;

                foreach (var ritem in item.RegionInfoList)
                {
                    ZonePolyLineLocation loc = new ZonePolyLineLocation();
                    loc.Name = item.Name;
                    // Mapsui migration: RegionInfo.Locations still returns a Bing LocationCollection,
                    // and ZonePolyLineLocation.ZoneLocations is now MapLocationCollection.
                    // Adapter performs an explicit coordinate-preserving copy (no reprojection).
                    // loc.ZoneLocations = ritem.Locations;
                    loc.ZoneLocations = MapLocationCollectionAdapter.FromBing(ritem.Locations);
                    loc.FillColor = ritem.FillColor;
                    locList.Add(loc);
                }
            }
            ZoneLocationList = locList;
        }

        /// <summary>
        /// Called when [market selection changed].
        /// </summary>
        public void OnMarketSelectionChanged()
        {
            RaisePropertyChanged("UPTOOnly");
            locationTask = Task.Run(() => { mNodeHashCache = NodeLocationHash.GetLocationHash((int)SelectedMarket); });
            RefereshData();

        }

        public void TemperatureNewOpen()
        {
            //Vayu.TemperatureGraphNew.MainWindow window = new Vayu.TemperatureGraphNew.MainWindow();
            //window.DataContext = new Vayu.TemperatureGraphNew.ViewModel.MainViewModel(new TemperatureGraphNew.Model.DataService());
            //window.Show();
        }

        /// <summary>
        /// Shows the LMP graphs.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        public void ShowLMPGraphs(List<Tuple<string, string>> sourceSinkList)
        {
            if (!Enum.IsDefined(SelectedMarket.GetType(), SelectedMarket))
            {
                return;
            }
            Vayu.LMPriceWindow.LmpForm.SetSourceSinks(sourceSinkList, (int)SelectedMarket);
            Vayu.LMPriceWindow.LmpForm.OpenLmpGraphs((int)SelectedMarket, Vayu.LMPriceWindow.LmpForm.GetSourceSinks(),
                DateTime.Today, DateTime.Today);
        }
        /// <summary>
        /// Shows the node analyzer.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        public void ShowNodeAnalyzer(List<Tuple<string, string>> sourceSinkList)
        {
            if (!Enum.IsDefined(SelectedMarket.GetType(), SelectedMarket))
            {
                return;
            }
            Vayu.LMPriceWindow.LmpForm.SetSourceSinks(sourceSinkList, (int)SelectedMarket);
            Vayu.LMPriceWindow.LmpForm.OpenLMPStatisticAnalyzer((int)SelectedMarket, Vayu.LMPriceWindow.LmpForm.GetSourceSinks());
        }

        /// <summary>
        /// Loads the sink source zone.
        /// </summary>
        public void LoadSinkSourceZone(/*List<LatestConstraint> ConstraintList, out List<LatestConstraint> latestZoneWiseConstraintsList*/)
        {
            List<string> foundSourceZoneList = new List<string>();
            List<string> foundSinkZoneList = new List<string>();
            LatestConstraint pNode = new LatestConstraint();
            foreach (var item in LatestConstraintsList)
            {
                pNode = mDataService.GetNode(item.SourceNodeKey);
                foundSourceZoneList.Add(pNode.SourceZone);
            }
            for (int i = 0; i < LatestConstraintsList.Count; i++)
            {

                LatestConstraintsList[i].SourceZone = foundSourceZoneList[i];
            }
            foreach (var item in LatestConstraintsList)
            {
                pNode = mDataService.GetNode(item.SinkNodeKey);
                foundSinkZoneList.Add(pNode.SourceZone);
            }
            for (int i = 0; i < LatestConstraintsList.Count; i++)
            {
                LatestConstraintsList[i].SinkZone = foundSinkZoneList[i];
            }
            //var ZonalLate
            //foreach (var item in ConstraintList)
            //{
            //    pNode = mDataService.GetNode(item.SourceNodeKey);
            //    foundSourceZoneList.Add(pNode.SourceZone);
            //}
            //for (int i = 0; i < ConstraintList.Count; i++)
            //{
            //    ConstraintList[i].SourceZone = foundSourceZoneList[i];
            //}
            //foreach (var item in ConstraintList)
            //{
            //    pNode = mDataService.GetNode(item.SinkNodeKey);
            //    foundSinkZoneList.Add(pNode.SourceZone);
            //}
            //for (int i = 0; i < ConstraintList.Count; i++)
            //{
            //    ConstraintList[i].SinkZone = foundSinkZoneList[i];
            //}
            //latestZoneWiseConstraintsList = ConstraintList;           
        }
        public void LoadSinkSourceZoneAndZoneFilter(List<LatestConstraint> constraintsList)
        {
            try
            {
                List<string> foundSourceZoneList = new List<string>();
                List<string> foundSinkZoneList = new List<string>();
                LatestConstraint pNode = new LatestConstraint();
                foreach (var item in constraintsList)
                {
                    pNode = mDataService.GetNode(item.SourceNodeKey);
                    foundSourceZoneList.Add(pNode.SourceZone);
                }
                for (int i = 0; i < constraintsList.Count; i++)
                {
                    constraintsList[i].SourceZone = foundSourceZoneList[i];
                }
                foreach (var item in constraintsList)
                {
                    pNode = mDataService.GetNode(item.SinkNodeKey);
                    foundSinkZoneList.Add(pNode.SourceZone);
                }
                for (int i = 0; i < constraintsList.Count; i++)
                {
                    constraintsList[i].SinkZone = foundSinkZoneList[i];
                }
                if (IsAllDay)
                {

                    LatestConstraintsListData = new ConstraintList(constraintsList.OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                    LatestConstraintsList = new ConstraintList(constraintsList.OrderByDescending(a => Math.Abs(a.ShadowPrice)));

                }
                else
                {
                    DateTime maxDateTime = constraintsList.Max(x => x.MarketDate);
                    LatestConstraintsListData = new ConstraintList(constraintsList.Where(a => a.MarketDate == maxDateTime).
                        OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                    DateTime MaxDateTime = constraintsList.Max(x => x.MarketDate);
                    LatestConstraintsList = new ConstraintList(constraintsList.Where(a => a.MarketDate == MaxDateTime).
                        OrderByDescending(a => Math.Abs(a.ShadowPrice)));
                }
            }

            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when [channel factory faulted].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnChannelFactory_Faulted(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the Tick event of the ConstraintTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ConstraintTimer_Tick(object sender, EventArgs e)
        {
            if (ConstraintTimer.ShutdownInProgress)
            {
                ConstraintTimer.Shutdown();
                return;
            }

            if (ConstraintChannel == null)
                return;

            ConstraintTimer.Stop();

            try
            {
                ConstraintChannel.HeartBeat();

                if (!ConstraintTimer.Subscribed || ConstraintTimer.MarketKey != (int)SelectedMarket)
                {
                    ConstraintTimer.Subscribed = true;
                    ConstraintTimer.MarketKey = (int)SelectedMarket;
                    ConstraintChannel.Subscribe((int)SelectedMarket, DateTime.Now);
                }
            }
            catch
            {
                if (!ConstraintTimer.ReRun)
                {
                    ConstraintTimer.ReRun = true;
                    ConstraintTimer.Subscribed = false;
                    ConstraintTimer.MarketKey = 0;
                    ConstraintChannel = null;
                    ConstraintTimer_Tick(sender, e);
                    return;
                }
                else
                    ConstraintTimer.ReRun = false;
            }
            finally
            {
                ConstraintTimer.Start();
            }
        }

        /// <summary>
        /// Handles the Tick event of the LMPTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void LMPTimer_Tick(object sender, EventArgs e)
        {
            if (LMPTimer.ShutdownInProgress)
            {
                LMPTimer.Shutdown();
                return;
            }

            if (LMPChannel == null)
                return;

            LMPTimer.Stop();

            try
            {
                LMPChannel.HeartBeat();

                if (!LMPTimer.Subscribed || LMPTimer.MarketKey != (int)SelectedMarket)
                {
                    LMPTimer.Subscribed = true;
                    LMPTimer.MarketKey = (int)SelectedMarket;
                    if (!IsHistoricalChecked)
                        LMPChannel.Subscribe((int)SelectedMarket);
                    else
                    {
                        LMPChannel.Subscribe(1);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!LMPTimer.ReRun)
                {
                    LMPTimer.ReRun = true;
                    LMPTimer.Subscribed = false;
                    LMPTimer.MarketKey = 0;
                    LMPChannel = null;
                    LMPTimer_Tick(LMPTimer, new EventArgs());
                    return;
                }
                else
                    LMPTimer.ReRun = false;
            }
            finally
            {
                LMPTimer.Start();
            }
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Vayu.INodePriceFiveMinCallback" />
        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
        class LmpCallback : INodePriceFiveMinCallback
        {
            /// <summary>
            /// Gets or sets the parent model.
            /// </summary>
            /// <value>
            /// The parent model.
            /// </value>
            public MainWindowViewModel ParentModel { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LmpCallback"/> class.
            /// </summary>
            /// <param name="parnet">The parnet.</param>
            public LmpCallback(MainWindowViewModel parnet)
            {
                ParentModel = parnet;
            }

            /// <summary>
            /// Sends the price.
            /// </summary>
            /// <param name="nodes">The nodes.</param>
            public async void SendPrice(Node[] nodes)
            {
                Task task = Task.Run(() => ParentModel.RefreshLMP(nodes));
                await task;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Vayu.iconstraintInfoCallback" />
        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
        class ConstraintCallback : IConstraintInfoCallback
        {
            /// <summary>
            /// Gets or sets the parent model.
            /// </summary>
            /// <value>
            /// The parent model.
            /// </value>
            public MainWindowViewModel ParentModel { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConstraintCallback"/> class.
            /// </summary>
            /// <param name="parnet">The parnet.</param>
            public ConstraintCallback(MainWindowViewModel parnet)
            {
                ParentModel = parnet;
            }

            /// <summary>
            /// Sets the constraints.
            /// </summary>
            /// <param name="constraintList">The constraint list.</param>
            public async void SetConstraints(List<LatestConstraint> constraintList)
            {
                Task task = Task.Run(() => ParentModel.RefreshConstrains(constraintList));
                await task;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum MarketsEnum
    {
        ERCOT = 9
    }

    /// <summary>
    /// 
    /// </summary>
    public class PortfolioBid
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price { get; set; }
    }

    public class SensitivityHelper
    {
        public int ConstraintId { get; set; }
        public string Constraint { get; set; }
        public string Contingency { get; set; }
        public string NodeName { get; set; }

        public int NodeKey { get; set; }
        public string Source { get; set; }
        public double Sensitivity { get; set; }
        public string Zone { get; set; }
    }
}