
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Vayu.CityTemperatureServiceLibrary;
using Vayu.CommonAccessLibrary;
using Vayu.CommonControls;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.NodePriceFiveMinLibrary;
using Vayu.PowerMap.Controls;
using Vayu.PowerMap.Model;
using Vayu.PowerMap.ViewModel;
using Vayu.PowerMap.ViewModels;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window, INotifyPropertyChanged, INodePriceFiveMinCallback
    {

        public Dictionary<string, int> mMarkets = new Dictionary<string, int>() { { "ERCOT", 9 }, { "MISO", 2 }, { "CAISO", 7 }, { "SPP", 12 }, { "NYISO", 3 }, { "PJM", 1 } };

        private Dictionary<int, Location> mMarketLocation = new Dictionary<int, Location>
                                                                {
                                                                    {1, new Location(38.462, -81.843)},
                                                                    {2, new Location(42.122, -91.775)},
                                                                    {3, new Location(42.71827,  -73.93397 )},
                                                                    {7, new Location(36.778261,  -119.417932 )},
                                                                    {12, new Location(34.75324 ,-92.44964 )},
                                                                    {9, new Location(31.19, -98.05)}
                                                                };
        private string mUser = Environment.UserName.ToLower();
        public static bool sIsDST = false;
        public ObservableCollection<TreeNode> Nodes { get; private set; }
        public ObservableCollection<LItem> Litems { get; private set; }
        private SqlConnection VayuConnection { get; set; }
        private Vayu.LMPStatistics.ViewModels.MainWindowViewModel mLMPStatisticsViewModel;
        private Vayu.LMPStatistics.Views.MainWindow mLMPStatisticsWindow;
        private List<SourceSinkData> mSourceSinkList;
        private updateLatLongDialog latLongUpdate;
        private static DataTable mMISOutagePlannedTable { get; set; }
        private SqlCommand mSelectStationImportGeoCommand { get; set; }
        private SqlCommand mSelectStationGeoCommand { get; set; }
        private static DataTable mMISOOutageActualTable { get; set; }
        private SqlCommand mSelectMISOHistoricalOutageCommand { get; set; }
        private SqlCommand mSelectNYISOHistoricalOutageCommand { get; set; }
        private SqlCommand mSelectERCOTLMPHRTCommand { get; set; }
        private SqlCommand mSelectERCOTLMPHDACommand { get; set; }
        private SqlCommand mSelectPJMLMPHRTCommand { get; set; }
        private SqlCommand mSelectPJMLMPHDACommand { get; set; }
        private SqlCommand mSelectNodeDetailCommand { get; set; }
        private SqlCommand mSelectPJMOutagePlannedCommand { get; set; }
        private SqlCommand mSelectPJMOutageActualCommand { get; set; }
        private SqlCommand mSelectERCOTOutageCommand { get; set; }
        private SqlCommand mSelectSPPActualOutageCommand { get; set; }
        private SqlCommand mSelecSPPHistoricalOutageCommand { get; set; }
        private SqlCommand mSelectNYISOOutagePlannedCommand { get; set; }
        private SqlCommand mSelectCAISOOutagePlannedCommand { get; set; }
        private SqlCommand mSelectERCOTOutagePlannedCommand { get; set; }
        private SqlCommand mSelectMISOOutagePlannedCommand { get; set; }
        private SqlCommand mSelectMISOOutageCommand { get; set; }
        private SqlCommand mSelectPJMHistoricalOutageCommand { get; set; }
        private SqlCommand mSelectERCOTHistoricalOutageCommand { get; set; }
        private SqlCommand mSelectNYISOOutageCommand { get; set; }
        private SqlCommand mSelectCAISOOutageCommand { get; set; }
        //ForIIR Outages
        private SqlCommand mSelectPJMIIROutageCommand { get; set; }
        private SqlCommand mSelectErcotIIROutageCommand { get; set; }
        private SqlCommand mSelectCAISOIIROutageCommand { get; set; }
        private SqlCommand mSelectMISOIIROutageCommand { get; set; }
        private SqlCommand mSelectNYISOIIROutageCommand { get; set; }
        private SqlCommand mSelectSPPIIROutageCommand { get; set; }
        private SqlCommand mSelectPJMHistoricalIIROutageCommand { get; set; }
        private SqlCommand mSelectErcotHistoricalIIROutageCommand { get; set; }
        private SqlCommand mSelectCAISOHistoricalIIROutageCommand { get; set; }
        private SqlCommand mSelectMISOHistoricalIIROutageCommand { get; set; }
        private SqlCommand mSelectNYISOHistoricalIIROutageCommand { get; set; }
        private SqlCommand mSelectSPPHistoricalIIROutageCommand { get; set; }
        //private SqlCommand mSelectConstraintRTCommand { get; set; }
        //private SqlCommand mSelectConstraint5MHRTCommand { get; set; }
        //private SqlCommand mSelectConstraintHRTCommand { get; set; }
        //private SqlCommand mSelectConstraintHDACommand { get; set; }
        private SqlCommand mSelectConstraintHistoryCommand { get; set; }
        private SqlCommand mSelectPJMRevisedOutageCommand;
        private SqlCommand mSelectMisoHighNodesCommand;
        private SqlCommand mSelectMisoLowNodesCommand;
        //private SqlCommand mSelectMisoTimeCommand;
        private SqlCommand mSelectMisoConstraintsCommand;
        //private Dictionary<DateTime, Location> mDateSourcelocation = new Dictionary<DateTime, Location>();
        //private Dictionary<DateTime, Location> mDateSinklocation = new Dictionary<DateTime, Location>();
        private static List<placemark> mPlacemarkList = new List<placemark>();
        private List<placemark> mSubercotplacemarklist = new List<placemark>();
        private List<placemark> mSubplacemarklist = new List<placemark>();
        private List<string> mSelectedItemsERCOT = new List<string>();
        private bool mRefresh = true;
        private List<string> mSelectedItemsPJM = new List<string>();

        private List<Node_Geo> mNodeList { get; set; }
        private List<Node_Geo> mMarketNodeList { get; set; }
        private static DataTable mPJMOutagePlannedTable { get; set; }
        private static DataTable mPJMOutageActualTable { get; set; }
        private static DataTable mERCOTOutagePlannedTable { get; set; }
        private static DataTable mERCOTOutageActualTable { get; set; }
        private static DataTable mNYISOOutagePlannedTable { get; set; }
        private static DataTable mCAISOOutagePlannedTable { get; set; }
        private static DataTable mNYISOOutageActualTable { get; set; }
        private static DataTable mCAISOOutageActualTable { get; set; }
        private static DataTable mSPPOutageActualTable { get; set; }
        private static DataTable mMISOOutageTable { get; set; }
        private static DataTable mConstraintRTMisoDetailsTable { get; set; }
        private MapLayer mLMPMapLayer = new MapLayer();
        private MapLayer mConstraintMapLayer = new MapLayer();
        private MapLayer mOutageMapLayer = new MapLayer();
        private MapLayer mPortfolioMapLayer = new MapLayer();
        private MapLayer mTransmissionMapLayer = new MapLayer();
        private MapLayer mVirtualMapLayer = new MapLayer();
        private MapLayer mNavigationMapLayer = new MapLayer();
        private MapLayer mNavigationLMPLayer = new MapLayer();
        private MapLayer mNavigationIIROutageLayer = new MapLayer();
        private MapLayer mWeatherMapLayer = new MapLayer();
        private MapTileLayer tileLayer = new MapTileLayer();
        private MapLayer mIIROutageMapLayer = new MapLayer();
        private double tileOpacity = 1.0;
        private Dictionary<string, List<Outage>> mOutaget_Collection = new Dictionary<string, List<Outage>>();

        private HashSet<string> sourcesinkUpToList = new HashSet<string>();
        private Task uptoTask;
        public HashSet<string> mPJMSourceSinkUptoList
        {
            get
            {
                if (uptoTask != null)
                    Task.WaitAll(uptoTask);

                return sourcesinkUpToList;
            }
            set { sourcesinkUpToList = value; }
        }

        private List<Outage> mOutagelist = new List<Outage>();
        private List<Node_Geo> mNodeListMapped = new List<Node_Geo>();
        private List<Vayu.NodePriceLibrary.Node> mNodeListNoMap = new List<Vayu.NodePriceLibrary.Node>();
        private Dictionary<string, string> mOutageZoneHash = new Dictionary<string, string>();
        private Dictionary<string, Tuple<DateTime, DateTime>> mRevisionDateHash = new Dictionary<string, Tuple<DateTime, DateTime>>();

        private Task branchTask;
        private List<BranchModel> branchList;

        public List<BranchModel> mBranchList
        {
            get
            {
                if (branchTask != null)
                    Task.WaitAll(branchTask);

                return branchList;
            }
            set { branchList = value; }
        }

        private static List<OutageCorrelationModel> mHistoricalOutagesList { get; set; }
        public int mMarketInContext { get; set; }
        private INodePriceFiveMin mNodeProxy = null;
        private static System.Timers.Timer sReconnectTimer;
        private static System.Timers.Timer sHeartBeatTimer;
        private DuplexChannelFactory<INodePriceFiveMin> mPipeFactory;
        private int mHe;
        private DateTime mExactDate;
        private double mLegendMax = 0;
        private double mLegendMin = 0;
        private List<string> mAddedRevisedList = new List<string>();
        private string mMinVoltText;
        public string MinVoltText
        {
            get
            {
                return mMinVoltText;
            }
            set
            {
                mMinVoltText = value;
                CheckBox_Checked(null, null);
                OutageMain();
                RaisePropertyChanged("MinVoltText");
            }
        }
        private string mMaxVoltText;
        public string MaxVoltText
        {
            get
            {
                return mMaxVoltText;
            }
            set
            {
                mMaxVoltText = value;
                CheckBox_Checked(null, null);
                OutageMain();
                RaisePropertyChanged("MaxText");
            }
        }

        private string mMinText;
        public string MinText
        {
            get
            {
                return mMinText;
            }
            set
            {
                mMinText = value;
                CheckBox_Checked(null, null);
                OutageMain();
                RaisePropertyChanged("MinText");
            }
        }
        private string mMaxText;
        public string MaxText
        {
            get
            {
                return mMaxText;
            }
            set
            {
                mMaxText = value;
                CheckBox_Checked(null, null);
                OutageMain();
                RaisePropertyChanged("MaxText");
            }
        }
        private int mOutageGridCount;
        public int OutageGridCount
        {
            get
            {
                return mOutageGridCount;
            }
            set
            {
                mOutageGridCount = value;
                RaisePropertyChanged("OutageGridCount");
            }
        }
        private int mIIROutageGridCount;
        public int IIROutageGridCount
        {
            get
            {
                return mIIROutageGridCount;
            }
            set
            {
                mIIROutageGridCount = value;
                RaisePropertyChanged("IIROutageGridCount");
            }
        }
        private DateTime mStartTime;
        public DateTime startTime
        {
            get
            {
                return mStartTime;
            }
            set
            {
                mStartTime = value;
                RaisePropertyChanged("start_time");
            }
        }
        private DateTime mEndTime;
        public DateTime endTime
        {
            get
            {
                return mEndTime;
            }
            set
            {
                mEndTime = value;
                RaisePropertyChanged("end_time");
            }
        }
        private DateTime mExactTime;
        public DateTime exactTime
        {
            get
            {
                return mExactTime;
            }
            set
            {
                mExactTime = value;
                RaisePropertyChanged("exact_time");
            }
        }
        public int HE
        {
            get
            {
                return mHe;
            }
            set
            {
                mHe = value;
                exactTime = exactDate.AddHours(mHe);
                RaisePropertyChanged("HE");
            }
        }
        public DateTime exactDate
        {
            get
            {
                return mExactDate;
            }
            set
            {
                mExactDate = value;
                RaisePropertyChanged("exact_date");
            }
        }
        public double legendMax
        {
            get
            {
                return mLegendMax;
            }
            set
            {
                mLegendMax = value;
                RaisePropertyChanged("legend_max");
            }
        }
        public double legendMin
        {
            get
            {
                return mLegendMin;
            }
            set
            {
                mLegendMin = value;
                RaisePropertyChanged("legend_min");
            }
        }
        private DispatcherTimer dispatcherTimer { get; set; }
        public bool? isLMPCheck { get; set; }
        public bool? isConstraintCheck { get; set; }
        public bool? isOutageCheck { get; set; }
        public bool? isPortfolioeCheck { get; set; }
        public bool? isTransmissionCheck { get; set; }
        public bool? isIIROutageCheck { get; set; }
        public bool? isZoneCheck { get; set; }
        IDisposable keyPressSubLMPs = null;
        IDisposable keyPressSubConstraints = null;
        IDisposable keyPressSubOutages = null;
        private LMPPriceType mSelectedLMPPriceType;
        public LMPPriceType SelectedLMPPriceType
        {
            get
            {
                return mSelectedLMPPriceType;
            }
            set
            {
                mSelectedLMPPriceType = value;
                RaisePropertyChanged("SelectedLMPPriceType");
                LMPMain();
            }
        }
        private OutagesSourceType mSelectedOutagesSourceType;
        public OutagesSourceType SelectedOutagesSourceType
        {
            get
            {
                return mSelectedOutagesSourceType;
            }
            set
            {
                mSelectedOutagesSourceType = value;
                RaisePropertyChanged("SelectedOutagesSourceType");
                OutageApply_button_Click(null, null);
            }
        }
        private OutagesScheduleType mSelectedOutagesScheduleType;
        public OutagesScheduleType SelectedOutagesScheduleType
        {
            get
            {
                return mSelectedOutagesScheduleType;
            }
            set
            {
                mSelectedOutagesScheduleType = value;
                RaisePropertyChanged("SelectedOutagesScheduleType");
                OutageApply_button_Click(null, null);
            }
        }
        private bool mLmpNormColorScaleType = false;
        public bool LMPNormColorScaleType
        {
            get
            {
                return mLmpNormColorScaleType;
            }
            set
            {
                mLmpNormColorScaleType = value;
                RaisePropertyChanged("LMPNormColorScaleType");
                LMPMain();
            }
        }
        private bool mOutagesStatusForced;
        public bool OutagesStatusForced
        {
            get
            {
                return mOutagesStatusForced;
            }
            set
            {
                mOutagesStatusForced = value;
                RaisePropertyChanged("OutagesStatusForced");
                OutageApply_button_Click(null, null);
            }
        }
        private bool mOutagesStatusPlanned;
        public bool OutagesStatusPlanned
        {
            get
            {
                return mOutagesStatusPlanned;
            }
            set
            {
                mOutagesStatusPlanned = value;
                RaisePropertyChanged("OutagesStatusPlanned");
                OutageApply_button_Click(null, null);
            }
        }
        private bool mOutagesIncludeComplete;
        public bool OutagesIncludeComplete
        {
            get
            {
                return mOutagesIncludeComplete;
            }
            set
            {
                mOutagesIncludeComplete = value;
                RaisePropertyChanged("OutagesIncludeComplete");
                OutageApply_button_Click(null, null);
            }
        }
        private bool mOutageOpen = true;
        public bool OutageOpen
        {
            get
            {
                return mOutageOpen;
            }
            set
            {
                mOutageOpen = value;
                RaisePropertyChanged("OutageOpen");
                OutageApply_button_Click(null, null);
            }
        }
        private bool mOutageClose = false;
        public bool OutageClose
        {
            get
            {
                return mOutageClose;
            }
            set
            {
                mOutageClose = value;
                RaisePropertyChanged("OutageClose");
                OutageApply_button_Click(null, null);
            }
        }
        private bool mLMPPJMUpTo = true;
        public bool LMPPJMUpTo
        {
            get
            {
                return mLMPPJMUpTo;
            }
            set
            {
                mLMPPJMUpTo = value;
                RaisePropertyChanged("LMPPJMUpTo");
                LMPMain();
            }
        }
        private bool mTransmissionshowstation = false;
        public bool Transmissionshowstation
        {
            get
            {
                return mTransmissionshowstation;
            }
            set
            {
                mTransmissionshowstation = value;
                RaisePropertyChanged("Transmissionshowstation");
                TransmissionMain();
            }
        }
        private bool mCheckAvgTempRange = false;
        public bool CheckAvgTempRange
        {
            get
            {
                return mCheckAvgTempRange;
            }
            set
            {
                mCheckAvgTempRange = value;
                RaisePropertyChanged("CheckAvgTempRange");
                if (CheckAvgTempRange)
                {
                    RefreshRangeTemperatureLayer(temperatureList);
                }
            }
        }
        private bool mCheckMaxTempRange = false;
        public bool CheckMaxTempRange
        {
            get
            {
                return mCheckMaxTempRange;
            }
            set
            {
                mCheckMaxTempRange = value;
                //RaisePropertyChanged("CheckMaxTempRange");
                CheckAvgTempRange = false;
                CheckMinTempRange = false;
                RefreshRangeTemperatureLayer(temperatureList);
            }
        }
        private bool mCheckMinTempRange = false;
        public bool CheckMinTempRange
        {
            get
            {
                return mCheckMinTempRange;
            }
            set
            {
                mCheckMinTempRange = value;
                //RaisePropertyChanged("CheckMinTempRange");
                CheckAvgTempRange = false;
                CheckMaxTempRange = false;
                RefreshRangeTemperatureLayer(temperatureList);
            }
        }
        private bool? _isFahrenheit = true;
        public bool? IsFahrenheit
        {
            get { return _isFahrenheit; }
            set
            {
                _isFahrenheit = value;
                RaisePropertyChanged("IsFahrenheit");
            }
        }
        private bool _isCelsius = false;
        public bool IsCelsius
        {
            get { return _isCelsius; }
            set
            {
                _isCelsius = value;
                RaisePropertyChanged("IsCelsius");
            }
        }
        private bool? _ismph = true;
        public bool? Ismph
        {
            get { return _ismph; }
            set
            {
                _ismph = value;
                RaisePropertyChanged("Ismph");
            }
        }
        private bool _iskmph = false;
        public bool IsKmph
        {
            get { return _iskmph; }
            set
            {
                _iskmph = value;
                RaisePropertyChanged("IsKmph");
            }
        }
        private bool _isknots = false;
        public bool Isknots
        {
            get { return _isknots; }
            set
            {
                _isknots = value;
                RaisePropertyChanged("Isknots");
            }
        }

        #region Select Line and XF Colors

        private Color mOutageLineColor = Colors.Blue;
        private Color mOutageXFMRColor = Colors.Blue;
        private Color mConstraintColor = Colors.Red;
        private Brush mNavigateFromColor = Brushes.DarkOrange;
        private Brush mNavigateToColor = Brushes.SeaGreen;
        private Brush mVirtualColor = Brushes.LightCoral;


        #endregion

        public Dictionary<string, int> Markets
        {
            get
            {
                return mMarkets;
            }
        }


        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (keyPressSubLMPs != null)
                keyPressSubLMPs.Dispose();
            mLMPMapLayer = null;
            mConstraintMapLayer = null;
            mOutageMapLayer = null;
            mPortfolioMapLayer = null;
            mTransmissionMapLayer = null;
            mVirtualMapLayer = null;
            mNavigationMapLayer = null;
            mNavigationLMPLayer = null;
            mIIROutageMapLayer = null;
            try
            {
                GC.Collect();
            }
            catch
            {
            }
        }

        #region Pre_load Data and Initialization

        private bool IsSameDate()
        {
            if ((bool)MainCalendarSilder.IsRange)
            {
                if (startTime == MainCalendarSilder.from_date && endTime == MainCalendarSilder.to_date.AddDays(1)) return true;
            }
            else
            {
                if (exactDate == MainCalendarSilder.exact_date) return true;
            }
            return false;
        }

        private void BuildOutageZoneHash()
        {
            mOutageZoneHash.Add("APSS", "APS");
            mOutageZoneHash.Add("DOM-C", "DOM");
            mOutageZoneHash.Add("DOM-E", "DOM");
            mOutageZoneHash.Add("DOM-N", "DOM");
            mOutageZoneHash.Add("DOM-S", "DOM");
            mOutageZoneHash.Add("DOM-W", "DOM");
            mOutageZoneHash.Add("DYNEGY", "AEP");
            mOutageZoneHash.Add("EKPC", "DEOK");
            mOutageZoneHash.Add("FE", "ATSI");
            mOutageZoneHash.Add("FECL", "ATSI");
            mOutageZoneHash.Add("FEPA", "ATSI");
            mOutageZoneHash.Add("ILL_EQ", "COMED");
            mOutageZoneHash.Add("JC-N", "JC");
            mOutageZoneHash.Add("JC-S", "JC");
            mOutageZoneHash.Add("LINVFT", "PS");
            mOutageZoneHash.Add("NIPS", "COMED");
            mOutageZoneHash.Add("PS-N", "PS");
            mOutageZoneHash.Add("PS-S", "PS");
        }
        private void InitReactiveEvents()
        {
            int msWaitForKeyPresses = 1000;
            var keyPressEventsConstraintMin =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { Constraint_MinPrice_textBox.KeyDown += ev; },
                    (KeyEventHandler ev) => { Constraint_MinPrice_textBox.KeyDown -= ev; });
            var keyPressEventsConstraintMax =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { Constraint_MaxPrice_textBox.KeyDown += ev; },
                    (KeyEventHandler ev) => { Constraint_MaxPrice_textBox.KeyDown -= ev; });
            var joinEventsConstraints = keyPressEventsConstraintMin.Merge(keyPressEventsConstraintMax);
            keyPressSubConstraints = joinEventsConstraints
                .Throttle(TimeSpan.FromMilliseconds(msWaitForKeyPresses))
                .ObserveOnDispatcher()
                .Subscribe(FilterConstraints);
            var keyPressEventsOutagesMin =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { OutageMinKVTextBox.KeyDown += ev; },
                    (KeyEventHandler ev) => { OutageMinKVTextBox.KeyDown -= ev; });
            var keyPressEventsOutagesMax =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { OutageMaxKVTextBox.KeyDown += ev; },
                    (KeyEventHandler ev) => { OutageMaxKVTextBox.KeyDown -= ev; });
            var joinEventsOutages = keyPressEventsOutagesMin.Merge(keyPressEventsOutagesMax);
            keyPressSubOutages = joinEventsOutages
                .Throttle(TimeSpan.FromMilliseconds(msWaitForKeyPresses))
                .ObserveOnDispatcher()
                .Subscribe(FilterOutages);

            var keyPressEventsTransmissionMin =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { Transmisson_MinKV_Textbox.KeyDown += ev; },
                    (KeyEventHandler ev) => { Transmisson_MinKV_Textbox.KeyDown -= ev; });
            var keyPressEventsTransmissionMax =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { Transmisson_MaxKV_Textbox.KeyDown += ev; },
                    (KeyEventHandler ev) => { Transmisson_MaxKV_Textbox.KeyDown -= ev; });
            var joinEventsTransmission = keyPressEventsTransmissionMin.Merge(keyPressEventsTransmissionMax);
            keyPressSubOutages = joinEventsTransmission
                .Throttle(TimeSpan.FromMilliseconds(msWaitForKeyPresses))
                .ObserveOnDispatcher()
                .Subscribe(FilterTransmission);

            var keyPressEventsOutagesMinDuration =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { OutageDurationMinWatermarkTextBox.KeyDown += ev; },
                    (KeyEventHandler ev) => { OutageDurationMinWatermarkTextBox.KeyDown -= ev; });
            var keyPressEventsOutageMaxDuration =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { OutageDurationMaxWatermarkTextBox.KeyDown += ev; },
                    (KeyEventHandler ev) => { OutageDurationMaxWatermarkTextBox.KeyDown -= ev; });
            var joinEventsOutagesDuration = keyPressEventsOutagesMinDuration.Merge(keyPressEventsOutageMaxDuration);
            keyPressSubOutages = joinEventsOutagesDuration
                .Throttle(TimeSpan.FromMilliseconds(msWaitForKeyPresses))
                .ObserveOnDispatcher()
                .Subscribe(FilterOutages);

            var keyPressEventsCorrelationMile =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    (KeyEventHandler ev) => { Outage_Correlation_Mile_textBox.KeyDown += ev; },
                    (KeyEventHandler ev) => { Outage_Correlation_Mile_textBox.KeyDown -= ev; });
            keyPressSubOutages = keyPressEventsCorrelationMile
            .Throttle(TimeSpan.FromMilliseconds(msWaitForKeyPresses))
            .ObserveOnDispatcher()
            .Subscribe(FilterOutageCorrelation);
            var keyPressEventsCorrelationBus =
        Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
        (KeyEventHandler ev) => { Outage_Correlation_Bus_textBox.KeyDown += ev; },
        (KeyEventHandler ev) => { Outage_Correlation_Bus_textBox.KeyDown -= ev; });
            keyPressSubOutages = keyPressEventsCorrelationBus
            .Throttle(TimeSpan.FromMilliseconds(msWaitForKeyPresses))
            .ObserveOnDispatcher()
            .Subscribe(FilterOutageCorrelation);
        }

        private void loadDBCommands()
        {

            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            //
            mSelectStationGeoCommand = new SqlCommand();
            mSelectStationGeoCommand.CommandText =
                "select A.NodeName, " +
                "A.NodeKey, " +
                "A.Longitude, " +
                "A.Latitude, " +
                "C.NodeTypeKey, " +
                "A.KV, " +
                "A.PSSENAME, " +
                "A.MarketKey, " +
                "Zone = case when (C.zone is null or C.zone = '') then 'N/A' else C.zone end " +
                "from nodegeo A left join Node C (NOLOCK) on A.NodeKey = C.NodeKey";
            mSelectStationGeoCommand.Connection = VayuConnection;
            mSelectStationGeoCommand.CommandTimeout = 30;
            //
            mSelectERCOTLMPHRTCommand = new SqlCommand();
            mSelectERCOTLMPHRTCommand.CommandText =
                "select A.NodeName, A.NodeKey, A.Longitude, A.Latitude, B.LMP, B.MarketDateTime, C.NodeTypeKey, A.MarketKey " +
                " from nodegeo A left join NodeLMPH B (NOLOCK) on A.NodeKey = B.NodeKey " +
                " left join Node C (NOLOCK) on A.NodeKey = C.NodeKey " +
                " where B.MarketDateTime = @datetime and A.MarketKey = 9";
            mSelectERCOTLMPHRTCommand.Parameters.AddWithValue("@datetime", "datetime");
            mSelectERCOTLMPHRTCommand.Connection = VayuConnection;
            mSelectERCOTLMPHRTCommand.CommandTimeout = 30;
            //
            mSelectPJMLMPHRTCommand = new SqlCommand();
            mSelectPJMLMPHRTCommand.CommandText =
                "select A.NodeName, A.NodeKey, A.Longitude, A.Latitude, B.LMP, B.MarketDateTime, C.NodeTypeKey, A.KV, A.PSSENAME, A.MarketKey " +
                " from nodegeo A left join NodeLMPH B (NOLOCK) on A.NodeKey = B.NodeKey " +
                " left join Node C (NOLOCK) on A.NodeKey = C.NodeKey " +
                " where B.MarketDateTime = @datetime and A.MarketKey = 1";
            mSelectPJMLMPHRTCommand.Parameters.AddWithValue("@datetime", "datetime");
            mSelectPJMLMPHRTCommand.Connection = VayuConnection;
            mSelectPJMLMPHRTCommand.CommandTimeout = 30;
            //
            mSelectPJMLMPHDACommand = new SqlCommand();
            mSelectPJMLMPHDACommand.CommandText =
                "select A.NodeName, A.NodeKey, A.Longitude, A.Latitude, B.LMP, B.MarketDateTime, C.NodeTypeKey, A.KV, A.PSSENAME, A.MarketKey " +
                " from nodegeo A left join NodeDALMPH B (NOLOCK) on A.NodeKey = B.NodeKey " +
                " left join Node C (NOLOCK) on A.NodeKey = C.NodeKey " +
                " where B.MarketDateTime = @datetime and A.MarketKey = 1";
            mSelectPJMLMPHDACommand.Parameters.AddWithValue("@datetime", "datetime");
            mSelectPJMLMPHDACommand.Connection = VayuConnection;
            mSelectPJMLMPHDACommand.CommandTimeout = 30;
            //
            mSelectPJMOutagePlannedCommand = new SqlCommand();
            mSelectPJMOutagePlannedCommand.CommandText = "select a.TicketID, a.Branch as FromSub, a.ToBranch as ToSub, a.Zone, a.EquipmentType,a.Voltage,a.OutageStatus as status, " +
                                                        "a.Equipment, a.StartDate as PlannedStart, a.EndDate as PlannedEnd, convert(varchar, DateDiff(hour, 0, a.EndDate - a.StartDate)) " +
                                                        "as PlannedDuration, a.OpenClose, a.OutageType as Type, a.Causes, max(a.LastRevised) as date, a.RemovedDate from PJM_rt_outages a inner join " +
                                                        "(select b.TicketID, b.Branch as FromSub, b.ToBranch as ToSub, b.Zone, b.EquipmentType, b.Voltage, b.Equipment, StartDate as PlannedStart, " +
                                                        "b.EndDate as PlannedEnd, convert(varchar, DateDiff(hour, 0, b.EndDate - b.StartDate)) as PlannedDuration, b.OpenClose, b.OutageType as Type, " +
                                                        "b.Causes, max(b.LastRevised) as date, b.RemovedDate from PJM_rt_outages b where (case When (b.RemovedDate is not null And b.RemovedDate < EndDate) " +
                                                        "Then b.RemovedDate Else b.EndDate End) >= @startdate And b.StartDate <= @enddate group by b.TicketID, b.equipmenttype, b.voltage, " +
                                                        "b.equipment, b.branch, b.tobranch, b.zone, startdate, enddate, openclose, outagetype, causes, removeddate)b on b.TicketID=a.TicketID and " +
                                                        "b.FromSub=a.Branch  and a.Zone=b.Zone and a.Equipment=b.Equipment and a.Voltage=b.Voltage and a.EquipmentType=b.EquipmentType and " +
                                                        "a.OutageType=a.OutageType  and a.LastRevised=b.date and (case When (a.RemovedDate is not null And a.RemovedDate < EndDate) Then a.RemovedDate " +
                                                        "Else a.EndDate End) >= @startdate And a.StartDate <=  @enddate group by a.TicketID, a.Branch , a.ToBranch, a.Zone, a.EquipmentType, " +
                                                        "a.Voltage,a.OutageStatus , a.Equipment, a.StartDate , a.EndDate , a.OpenClose, a.OutageType , a.Causes,  a.RemovedDate";
            mSelectPJMOutagePlannedCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPJMOutagePlannedCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPJMOutagePlannedCommand.Connection = VayuConnection;
            mSelectPJMOutagePlannedCommand.CommandTimeout = 30;
            //
            mSelectPJMRevisedOutageCommand = new SqlCommand();
            mSelectPJMRevisedOutageCommand.CommandText = "select startdate, enddate from pjm_rt_outages where ticketid = @ticketid and equipment = @equipment " +
                                                        "and equipmenttype = @equipmenttype and voltage = @voltage and outagestatus = 'revised' and lastrevised " +
                                                        "= (select max(lastrevised) from pjm_rt_outages where ticketid = @ticketid and equipment = @equipment " +
                                                        "and equipmenttype = @equipmenttype and voltage = @voltage and outagestatus = 'revised')";
            mSelectPJMRevisedOutageCommand.Parameters.AddWithValue("@ticketid", "ticketid");
            mSelectPJMRevisedOutageCommand.Parameters.AddWithValue("@equipment", "equipment");
            mSelectPJMRevisedOutageCommand.Parameters.AddWithValue("@equipmenttype", "equipmenttype");
            mSelectPJMRevisedOutageCommand.Parameters.AddWithValue("@voltage", "voltage");
            mSelectPJMRevisedOutageCommand.Connection = VayuConnection;
            mSelectPJMRevisedOutageCommand.CommandTimeout = 30;
            //
            //mSelectPJMOutageActualCommand = new SqlCommand();  //case when a.TicketID<>0 then B.Zone else A.Zone end
            //mSelectPJMOutageActualCommand.CommandText = "select Distinct A.TicketID , A.Branch as FromSub, A.ToBranch as ToSub, case when a.TicketID<>0 then B.Zone else A.Zone end as zone, A.EquipmentType, A.Voltage, A.Status " +
            //    ",A.Equipment, A.Stamp as ActualStart,A.ExitStamp as ActualEnd, max(B.StartDate) as PlannedStart,MAX(B.EndDate) PlannedEnd, " +
            //    "convert(varchar, DateDiff(hour, 0, case when A.TicketID=0 then A.ExitStamp else max(B.ENDDATE)end - case when A.TicketID=0 then A.Stamp else max(B.STARTDATE) END)) as PlannedDuration, " +
            //    "A.Type from PJM_current_rt_outages A " +
            //    " Join pjm_rt_outages B " +
            //    "On A.TicketId = B.TicketId And B.TicketId <> 0 AND " +
            //    "A.Branch=B.Branch and " +
            //    "A.ToBranch=B.ToBranch and " +
            //    "a.EquipmentType=B.EquipmentType and " +
            //    "A.Voltage=b.Voltage " +
            //    "and A.Equipment=B.Equipment " +
            //    "and B.LastRevised = (Select Max(LastRevised) From pjm_rt_outages Where TicketId = A.TicketID) " +
            //    "where (A.exitstamp >= @startdate OR A.exitstamp is null ) And A.Stamp <= @enddate " +
            //    "and cast(@startdate as Date) <= cast(GETDATE() as Date)" +
            //    "group by A.TicketID,A.Branch,A.ToBranch,A.Equipment,A.EquipmentType,A.ExitStamp,A.Stamp,A.Zone,A.Voltage,A.Status,A.Type, B.Zone ";  //, B.Zone
            //mSelectPJMOutageActualCommand.Parameters.AddWithValue("@startdate", "startdate");
            //mSelectPJMOutageActualCommand.Parameters.AddWithValue("@enddate", "enddate");
            //mSelectPJMOutageActualCommand.Connection = VayuConnection;
            //mSelectPJMOutageActualCommand.CommandTimeout = 30;

            mSelectPJMOutageActualCommand = new SqlCommand();  //case when a.TicketID<>0 then B.Zone else A.Zone end
            mSelectPJMOutageActualCommand.CommandText = "select Distinct A.TicketID , A.Branch as FromSub, A.ToBranch as ToSub, case when a.TicketID<>0 then A.Zone else A.Zone end as zone, A.EquipmentType, A.Voltage, A.Status " +
                ",A.Equipment, A.Stamp as ActualStart,A.ExitStamp as ActualEnd, max(A.ExitStamp) as PlannedStart,MAX(A.ExitStamp) PlannedEnd, " +
                "convert(varchar, DateDiff(hour, 0, case when A.TicketID=0 then A.ExitStamp else max(A.ExitStamp)end - case when A.TicketID=0 then A.Stamp else max(A.ExitStamp) END)) as PlannedDuration, " +
                "A.Type from PJM_current_rt_outages A " +
                "where A.Stamp >= @startdate and A.Stamp <= @enddate  " +
                "and cast(@startdate as Date) <= cast(GETDATE() as Date)" +
                "group by A.TicketID,A.Branch,A.ToBranch,A.Equipment,A.EquipmentType,A.ExitStamp,A.Stamp,A.Zone,A.Voltage,A.Status,A.Type, A.Zone ";  //, B.Zone
            mSelectPJMOutageActualCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPJMOutageActualCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPJMOutageActualCommand.Connection = VayuConnection;
            mSelectPJMOutageActualCommand.CommandTimeout = 30;
            //

            mSelectERCOTOutageCommand = new SqlCommand();
            mSelectERCOTOutageCommand.CommandText =
                "select OutageIdentifier as TicketID,EquipmentFromStationName as FromSub,EquipmentToStationName as ToSub, '' as Zone,EquipmentType,VoltageLevel as Voltage,Status = case when ActualStartDate is not null and ActualEndDate is not null then 'Complete' else (case when ActualStartDate is not null and ActualEndDate is null then 'Active' else 'Planned' end) end,EquipmentName as Equipment,ActualStartDate as ActualStart,ActualEndDate as ActualEnd,PlannedStartDate as PlannedStart,PlannedEndtDate as PlannedEnd,convert(varchar, DateDiff(hour, 0, PlannedEndtDate - PlannedStartDate)) as PlannedDuration,OutageStatus,OutageType as Type,SubmitTime as LasRevised from Vayu..Ercot_RT_Outages where ActualStartDate <= @enddate and (ActualEndDate >= @startDate or ActualEndDate is null)";
            mSelectERCOTOutageCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectERCOTOutageCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectERCOTOutageCommand.Connection = VayuConnection;
            mSelectERCOTOutageCommand.CommandTimeout = 30;

            //
            mSelectNYISOOutagePlannedCommand = new SqlCommand();
            mSelectNYISOOutagePlannedCommand.CommandText =
                " select distinct " +
                " PTID as TicketID, " +
                " EquipmentFromStationName as FromSub, " +
                " EquipmentToStationName as ToSub, " +
                " EquipmentType, " +
                " Voltage as Voltage," +
                " EquipmentName as Equipment, " +
                " PlannedStart as PlannedStart," +
                " PlannedEnd as PlannedEnd," +
                " convert(varchar, DateDiff(hour, 0, PlannedEnd - PlannedStart)) as PlannedDuration, " +
                " OutageType as Type " +
                " from [NYISOScheduledOutagesRT]  where " +
                " PlannedStart <= @enddate and (PlannedEnd >= @startDate or PlannedEnd is null)";
            mSelectNYISOOutagePlannedCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectNYISOOutagePlannedCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectNYISOOutagePlannedCommand.Connection = VayuConnection;
            mSelectNYISOOutagePlannedCommand.CommandTimeout = 30;
            //
            mSelectCAISOOutagePlannedCommand = new SqlCommand();
            mSelectCAISOOutagePlannedCommand.CommandText =
                " select distinct " +
                " OutageID as TicketID, " +
                " FromStation as FromSub, " +
                " ToStation as ToSub, " +
                " EquipmentType, " +
                " Voltage as Voltage, " +
                " Equipment as Equipment, " +
                " StartDate as PlannedStart, " +
                " EndDate as PlannedEnd, " +
                " convert(varchar, DateDiff(hour, 0, EndDate - StartDate)) as PlannedDuration, " +
                " OutageType as Type " +
                " from CAISO.Outages where OutageType = 'Planned' and " +
                " StartDate <= @enddate and EndDate >= @startDate ";
            mSelectCAISOOutagePlannedCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectCAISOOutagePlannedCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectCAISOOutagePlannedCommand.Connection = VayuConnection;
            mSelectCAISOOutagePlannedCommand.CommandTimeout = 30;
            //
            mSelectSPPActualOutageCommand = new SqlCommand();
            mSelectSPPActualOutageCommand.CommandText =
                "select distinct " +
                "From_Station as FromSub, " +
                "To_Station as ToSub, " +
                "OutageType," +
                "Rating as Voltage," +
                "Facility as Equipment, " +
                "PlannedOutageStart as PlannedStart," +
                "PlannedOutageEnd as PlannedEnd," +
                "convert(varchar, DateDiff(hour, 0, PlannedOutageEnd - PlannedOutageStart)) as PlannedDuration, " +
                "OutageType as Type " +
                "from SPP.TransmissionOutages  where " +
                "PlannedOutageStart <= @enddate and (PlannedOutageEnd >= @startDate or PlannedOutageEnd is null) and  From_Station is not null";
            mSelectSPPActualOutageCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectSPPActualOutageCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectSPPActualOutageCommand.Connection = VayuConnection;
            mSelectSPPActualOutageCommand.CommandTimeout = 30;

            //
            mSelectNYISOOutageCommand = new SqlCommand();
            mSelectNYISOOutageCommand.CommandText =
                " select PTID as TicketID, " +
                " EquipmentFromStationName as FromSub, " +
                " EquipmentToStationName as ToSub, " +
                " EquipmentToStationName," +
                " EQUIPMENT_TYPE," +
                " Voltage as Voltage," +
                " EquipmentName as Equipment, " +
                " OutageDate as OutageDate," +
                " OutageType as Type " +
                " from NYISO.Actualoutagesrt  where " +
                " OutageDate <= @enddate and (OutageDate >= @startDate or OutageDate is null)";
            mSelectNYISOOutageCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectNYISOOutageCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectNYISOOutageCommand.Connection = VayuConnection;
            mSelectNYISOOutageCommand.CommandTimeout = 30;
            //
            mSelectCAISOOutageCommand = new SqlCommand();
            mSelectCAISOOutageCommand.CommandText =
                " select OutageID as TicketID, " +
                " FromStation as FromSub, " +
                " ToStation as ToSub, " +
                " ToStation, " +
                " EquipmentType, " +
                " Voltage as Voltage, " +
                " Equipment as Equipment, " +
                " StartDate as OutageDate, " +
                " OutageType as Type " +
                " from CAISO.Outages  where OutageType = 'RT' and " +
                " StartDate <=@startDate  and EndDate >= @enddate ";
            mSelectCAISOOutageCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectCAISOOutageCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectCAISOOutageCommand.Connection = VayuConnection;
            mSelectCAISOOutageCommand.CommandTimeout = 30;
            //
            mSelectMISOOutageCommand = new SqlCommand();
            mSelectMISOOutageCommand.CommandText =
            " select FromStation as FromSub, ToStation as ToSub, 'RT' as EQUIPMENT_TYPE,Status = case when ActualStart is not null and case " +
            " when DATEPART(YYYY, convert(date,ActualEnd))='9999' then null else ActualEnd end is not null then 'Complete' else(case " +
            " when ActualStart is not null and (case when DATEPART(YYYY, convert(date,ActualEnd))='9999' then null else case " +
            " when DATEPART(YYYY, convert(date,ActualEnd))='9999' then null else ActualEnd end end) is null then 'Active' " +
            " else null end) end, EquipmentName as Equipment, ActualStart as ActualStart,case when DATEPART(YYYY, convert(date,ActualEnd))='9999' " +
            " then null else ActualEnd end as ActualEnd,ActualStart as PlannedStart,ActualEnd as PlannedEnd, " +
            " convert(varchar, DateDiff(hour, 0, ActualEnd - ActualStart)) as PlannedDuration, KV as Voltage from MISO.OutageRT " +
            " where PublishDate >= @startDate  and PublishDate  <= @enddate ";
            mSelectMISOOutageCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectMISOOutageCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectMISOOutageCommand.Connection = VayuConnection;
            mSelectMISOOutageCommand.CommandTimeout = 30;

            mSelectERCOTOutagePlannedCommand = new SqlCommand();
            mSelectERCOTOutagePlannedCommand.CommandText =
                              "select OutageIdentifier as TicketID,EquipmentFromStationName as FromSub,EquipmentToStationName as ToSub,'' as Zone,EquipmentType,VoltageLevel as Voltage,Status = case when ActualStartDate is not null and ActualEndDate is not null then 'Complete' else   (case when ActualStartDate is not null and ActualEndDate is null then 'Active' else 'Planned' end) end,EquipmentName as Equipment,ActualStartDate as ActualStart,ActualEndDate as ActualEnd,PlannedStartDate as PlannedStart,PlannedEndtDate as PlannedEnd, convert(varchar, DateDiff(hour, 0, PlannedEndtDate - PlannedStartDate)) as PlannedDuration,OutageStatus,OutageType as Type,SubmitTime as LasRevised from Vayu..Ercot_RT_Outages where   PlannedStartDate >= @startdate and PlannedStartDate < @enddate and PlannedEndtDate >= @startDate ";
            mSelectERCOTOutagePlannedCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectERCOTOutagePlannedCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectERCOTOutagePlannedCommand.Connection = VayuConnection;
            mSelectERCOTOutageCommand.CommandTimeout = 30;

            ////OutagePreLoad
            mSelectMISOOutagePlannedCommand = new SqlCommand();
            mSelectMISOOutagePlannedCommand.CommandText =
            " select FromStation as FromSub, ToStation as ToSub, EquipmentType as EQUIPMENT_TYPE , " +
            " Status = case when ActualStart is not null and case when DATEPART(YYYY, convert(date,ActualEnd))='9999' then null else ActualEnd end is not null then 'Complete' else " +
            " (case when ActualStart is not null and (case when DATEPART(YYYY, convert(date,ActualEnd))='9999' then null else " +
            " case when DATEPART(YYYY, convert(date,ActualEnd))='9999' then null else ActualEnd end end) is null then 'Active' else 'Planned' end) end, " +
            " EMSEquipmentName as Equipment ,ActualStart as ActualStart,case when DATEPART(YYYY, convert(date,ActualEnd))='9999' then null else ActualEnd end as ActualEnd,PlannedStart, " +
            " PlannedEnd,convert(varchar, DateDiff(hour, 0, PlannedEnd - PlannedStart)) as PlannedDuration, KV as Voltage  from MISO.OutagePlanned where PlannedStart <= @enddate and PlannedEnd >= @startDate ";
            mSelectMISOOutagePlannedCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectMISOOutagePlannedCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectMISOOutagePlannedCommand.Connection = VayuConnection;
            mSelectMISOOutagePlannedCommand.CommandTimeout = 30;

            mSelectPJMHistoricalOutageCommand = new SqlCommand();
            mSelectPJMHistoricalOutageCommand.CommandText =
                "select A.EquipmentType, " +
                "A.Branch as FromSub, " +
                "A.ToBranch as ToSub, " +
                "A.Voltage, A.Equipment, " +
                "A.Stamp as ActualStart, " +
                "A.ExitStamp as ActualEnd, " +
                "convert(varchar, DateDiff(hour, 0, A.ExitStamp - A.Stamp)) + ' Hours' as AcutalDuration, " +
                "A.Status, A.Type from PJM_Current_RT_outages A " +
                "where A.EquipmentType = @equipmenttype and A.Branch = @branch and A.Voltage = @voltage and A.Equipment = @equipment order by A.stamp desc ";

            mSelectPJMHistoricalOutageCommand.Parameters.AddWithValue("@equipment", "equipment");
            mSelectPJMHistoricalOutageCommand.Parameters.AddWithValue("@equipmenttype", "equipmenttype");
            mSelectPJMHistoricalOutageCommand.Parameters.AddWithValue("@branch", "branch");
            mSelectPJMHistoricalOutageCommand.Parameters.AddWithValue("@voltage", "voltage");
            mSelectPJMHistoricalOutageCommand.Connection = VayuConnection;
            mSelectPJMHistoricalOutageCommand.CommandTimeout = 30;

            mSelectERCOTHistoricalOutageCommand = new SqlCommand();
            mSelectERCOTHistoricalOutageCommand.CommandText = "select EquipmentType, " +
                                                         "EquipmentFromStationName as FromSub, " +
                                                         "EquipmentToStationName as ToSub," +
                                                         "VoltageLevel as Voltage," +
                                                         "EquipmentName as Equipment," +
                                                         "ActualStartDate as ActualStart," +
                                                         "ActualEndDate as ActualEnd," +
                                                         "PlannedStartDate, " +
                                                         "PlannedEndtDate, " +
                                                         "convert(varchar, DateDiff(hour, 0, ActualEndDate - ActualStartDate)) + ' Hours' as ActualDuration, " +
                                                         "convert(varchar, DateDiff(hour, 0, PlannedEndtDate - PlannedStartDate)) + ' Hours' as PlannedDuration, " +
                                                         "Status = case when ActualStartDate is not null and ActualEndDate is not null then 'Complete' else " +
                                                         "(case when ActualStartDate is not null and ActualEndDate is null then 'Active' else 'Planned' end) end, " +
                                                         "OutageType as Type " +
                                                         "from Vayu..Ercot_rt_outages " +
                                                         "where EquipmentName like @equipment and EquipmentType like @equipmenttype and EquipmentFromStationName like @branch " +
                                                         "order by ActualStartDate desc";
            mSelectERCOTHistoricalOutageCommand.Parameters.AddWithValue("@equipment", "equipment");
            mSelectERCOTHistoricalOutageCommand.Parameters.AddWithValue("@equipmenttype", "equipmenttype");
            mSelectERCOTHistoricalOutageCommand.Parameters.AddWithValue("@branch", "branch");
            mSelectERCOTHistoricalOutageCommand.Connection = VayuConnection;
            mSelectERCOTHistoricalOutageCommand.CommandTimeout = 30;

            //

            mSelectNYISOHistoricalOutageCommand = new SqlCommand();
            mSelectNYISOHistoricalOutageCommand.CommandText = "select EquipmentType, " +
                                                         "EquipmentFromStationName as FromSub, " +
                                                         "EquipmentToStationName as ToSub," +
                                                         "Voltage as Voltage," +
                                                         "EquipmentName as Equipment," +
                                                         "Planned_start as PlannedStartDate, " +
                                                         "Planned_end as PlannedEndtDate, " +
                                                         "convert(varchar, DateDiff(hour, 0, Planned_end - Planned_start)) + ' Hours' as PlannedDuration, " +
                                                         "OutageType as Type " +
                                                         "from NYISO_scheduledoutages_rt " +
                                                         "where EquipmentName = @equipment and voltage = @voltage  " +
                                                         "order by Planned_start desc";
            mSelectNYISOHistoricalOutageCommand.Parameters.AddWithValue("@equipment", "equipment");
            mSelectNYISOHistoricalOutageCommand.Parameters.AddWithValue("@voltage", "voltage");
            mSelectNYISOHistoricalOutageCommand.Connection = VayuConnection;
            mSelectNYISOHistoricalOutageCommand.CommandTimeout = 30;

            //
            mSelecSPPHistoricalOutageCommand = new SqlCommand();

            mSelecSPPHistoricalOutageCommand.CommandText =
                                  "select From_Station as FromSub, " +
                                  "To_Station as ToSub," +
                                  "Rating as Voltage," +
                                  "Facility as Equipment," +
                                  "OutageStart as PlannedStart, " +
                                  "ReturntoService as PlannedEnd, " +
                                  "convert(varchar, DateDiff(hour, 0, ReturntoService -  OutageStart)) + ' Hours' as                                                                                                                       PlannedDuration, " +
                                  "OutageType as Type " +
                                  "from SPP.TransmissionOutages " +
                                  "where Facility= @equipment   " +
                                  "order by OutageStart desc";
            mSelecSPPHistoricalOutageCommand.Parameters.AddWithValue("@equipment", "equipment");
            mSelecSPPHistoricalOutageCommand.Parameters.AddWithValue("@voltage", "voltage");
            mSelecSPPHistoricalOutageCommand.Connection = VayuConnection;
            mSelecSPPHistoricalOutageCommand.CommandTimeout = 30;


            ////menuItem_Click else part
            mSelectMISOHistoricalOutageCommand = new SqlCommand();
            mSelectMISOHistoricalOutageCommand.CommandText =
                                                    "select EQUIPMENT_TYPE,FROM_STATION as FromSub,TO_STATION as ToSub,EQUIPMENT as Equipment,Actual_Start as ActualStart," +
                                                    "Actual_End as ActualEnd," +
                                                    "Planned_Start," +
                                                    "Planned_End," +
                                                    "convert(varchar, DateDiff(hour, 0, Actual_End - Actual_Start)) +'Hours' as ActualDuration," +
                                                    "convert(varchar, DateDiff(hour, 0, Planned_End - Planned_Start)) + 'Hours' as PlannedDuration," +
                                                    "Status = case when Actual_Start is not null and Actual_End is not null then 'Complete' else" +
                                                    "(case when Actual_Start is not null and (case when convert(varchar,ACTUAL_END) like '%9999%' then null else ACTUAL_END end) is null then 'Active' else 'Planned' end) end " +
                                                    "from MISO_OUTAGE where Equipment like @equipment+'%' and equipment_type like @equipmenttype+'%'";


            mSelectMISOHistoricalOutageCommand.Parameters.AddWithValue("@equipment", "equipment");
            mSelectMISOHistoricalOutageCommand.Parameters.AddWithValue("@equipmenttype", "equipmenttype");
            mSelectMISOHistoricalOutageCommand.Connection = VayuConnection;
            mSelectMISOHistoricalOutageCommand.CommandTimeout = 30;
            //
            mSelectConstraintHistoryCommand = new SqlCommand();
            mSelectConstraintHistoryCommand.CommandText = "select " +
                    "A.ConstraintText as ConstraintName," +
                    "A.ContingencyText as ContingencyName," +
                    "A.ConstraintBusNameFrom as SubFrom," +
                    "A.ConstraintBusNameTo as SubTo," +
                    "A.MarketKey," +
                    "A.Type," +
                    "count(A.MarketDateTime) as Occurrences," +
                    "SUM(A.ShadowPrice)/12 as ShadowPrice " +
                    "from ConstraintRTGeo A " +
                    "where MarketKey = @marketkey @TimeLinesString " +
                    "group by " +
                    "A.ConstraintText, " +
                    "A.ContingencyText," +
                    "A.ConstraintBusNameFrom," +
                    "A.ConstraintBusNameTo," +
                    "A.MarketKey, " +
                    "A.Type";
            mSelectConstraintHistoryCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectConstraintHistoryCommand.Connection = VayuConnection;
            mSelectConstraintHistoryCommand.CommandTimeout = 30;
            //
            mSelectNodeDetailCommand = new SqlCommand();
            mSelectNodeDetailCommand.CommandText = "Select NodeKey, NodeName, ExternalNodeID, NodeTypeKey From Node Where Marketkey = @marketkey And NodeKey = @NodeKey";
            mSelectNodeDetailCommand.Parameters.AddWithValue("@NodeKey", "NodeKey");
            mSelectNodeDetailCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNodeDetailCommand.Connection = VayuConnection;

            //
            mSelectMisoHighNodesCommand = new SqlCommand();
            mSelectMisoHighNodesCommand.CommandText = "SELECT TOP 1  b.NodeName, a.MarketDateTime  from Nodelmp a inner join node b on b.nodekey = a.nodekey and b.MarketKey = 2 and a.MarketDateTime = @MarketDateTime where a.lmp <> 0 ORDER BY Congestion desc, LMP desc";
            mSelectMisoHighNodesCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mSelectMisoHighNodesCommand.Connection = VayuConnection;

            //
            mSelectMisoLowNodesCommand = new SqlCommand();
            mSelectMisoLowNodesCommand.CommandText = "SELECT TOP 1  b.NodeName,a.MarketDateTime from Nodelmp a inner join node b on b.nodekey = a.nodekey and b.MarketKey = 2 and a.MarketDateTime = @MarketDateTime where a.lmp <> 0 ORDER BY Congestion ASC, LMP ASC";
            mSelectMisoLowNodesCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mSelectMisoLowNodesCommand.Connection = VayuConnection;

            //
            mSelectPJMIIROutageCommand = new SqlCommand();
            mSelectPJMIIROutageCommand.CommandText = "select * from pjm.IIROutage where DeliveryDate >=@StartDate and DeliveryDate<=@EndDate";
            mSelectPJMIIROutageCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectPJMIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectPJMIIROutageCommand.Connection = VayuConnection;

            //
            mSelectErcotIIROutageCommand = new SqlCommand();
            mSelectErcotIIROutageCommand.CommandText = "select * from ERCOT.IIROutage where DeliveryDate >=@StartDate and DeliveryDate<=@EndDate";
            mSelectErcotIIROutageCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectErcotIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectErcotIIROutageCommand.Connection = VayuConnection;

            //
            mSelectMISOIIROutageCommand = new SqlCommand();
            mSelectMISOIIROutageCommand.CommandText = "select * from MISO.IIROutage where DeliveryDate >=@StartDate and DeliveryDate<=@EndDate";
            mSelectMISOIIROutageCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectMISOIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectMISOIIROutageCommand.Connection = VayuConnection;

            //
            mSelectNYISOIIROutageCommand = new SqlCommand();
            mSelectNYISOIIROutageCommand.CommandText = "select * from NYISO.IIROutage where DeliveryDate >=@StartDate and DeliveryDate<=@EndDate";
            mSelectNYISOIIROutageCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectNYISOIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectNYISOIIROutageCommand.Connection = VayuConnection;

            //
            mSelectCAISOIIROutageCommand = new SqlCommand();
            mSelectCAISOIIROutageCommand.CommandText = "select * from pjm.IIROutage where DeliveryDate >=@StartDate and DeliveryDate<=@EndDate";
            mSelectCAISOIIROutageCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectCAISOIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectCAISOIIROutageCommand.Connection = VayuConnection;

            //
            mSelectSPPIIROutageCommand = new SqlCommand();
            mSelectSPPIIROutageCommand.CommandText = "select * from SPP.IIROutage where DeliveryDate >=@StartDate and DeliveryDate<=@EndDate";
            mSelectSPPIIROutageCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectSPPIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectSPPIIROutageCommand.Connection = VayuConnection;

            //
            mSelectPJMHistoricalIIROutageCommand = new SqlCommand();
            mSelectPJMHistoricalIIROutageCommand.CommandText = "select P.OutageID,P.UnitName,P.UnitID,P.OwnerName,P.PlantID,P.PlantName,"
                                                      + "P.NERCRegion,P.PrimaryFuel,P.SecondaryFuel,P.FuelGroup,P.OutputCapacity,P.PowerUsage,"
                                                      + "P.StartDate,P.EndDate,P.Oduration,P.Precision,P.Type,P.Status,P.DeliveryDate,"
                                                      + "P.NERCSubRegion,P.UnitType,P.HeatRate,P.UltimateOwnerID,P.UltimateOwner,"
                                                      + "P.ElectricConnection,P.CapOffline,P.TradeRegion"
                                                    + " from pjm.IIROutage P where unitname=@unitname";
            mSelectPJMHistoricalIIROutageCommand.Parameters.AddWithValue("@unitname", "unitname");
            //mSelectPJMHistoricalIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectPJMHistoricalIIROutageCommand.Connection = VayuConnection;

            //
            mSelectErcotHistoricalIIROutageCommand = new SqlCommand();
            mSelectErcotHistoricalIIROutageCommand.CommandText = "select P.OutageID,P.UnitName,P.UnitID,P.OwnerName,P.PlantID,P.PlantName,"
                                                      + "P.NERCRegion,P.PrimaryFuel,P.SecondaryFuel,P.FuelGroup,P.OutputCapacity,P.PowerUsage,"
                                                      + "P.StartDate,P.EndDate,P.Oduration,P.Precision,P.Type,P.Status,P.DeliveryDate,"
                                                      + "P.NERCSubRegion,P.UnitType,P.HeatRate,P.UltimateOwnerID,P.UltimateOwner,"
                                                      + "P.ElectricConnection,P.CapOffline,P.TradeRegion"
                                                    + " from ERCOT.IIROutage P where unitname=@unitname";
            mSelectErcotHistoricalIIROutageCommand.Parameters.AddWithValue("@unitname", "UnitType");
            //mSelectErcotHistoricalIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectErcotHistoricalIIROutageCommand.Connection = VayuConnection;

            //
            mSelectMISOHistoricalIIROutageCommand = new SqlCommand();
            mSelectMISOHistoricalIIROutageCommand.CommandText = "select P.OutageID,P.UnitName,P.UnitID,P.OwnerName,P.PlantID,P.PlantName,"
                                                      + "P.NERCRegion,P.PrimaryFuel,P.SecondaryFuel,P.FuelGroup,P.OutputCapacity,P.PowerUsage,"
                                                      + "P.StartDate,P.EndDate,P.Oduration,P.Precision,P.Type,P.Status,P.DeliveryDate,"
                                                      + "P.NERCSubRegion,P.UnitType,P.HeatRate,P.UltimateOwnerID,P.UltimateOwner,"
                                                      + "P.ElectricConnection,P.CapOffline,P.TradeRegion"
                                                    + " from MISO.IIROutage P where unitname=@unitname";
            mSelectMISOHistoricalIIROutageCommand.Parameters.AddWithValue("@unitname", "UnitType");
            //mSelectMISOHistoricalIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectMISOHistoricalIIROutageCommand.Connection = VayuConnection;

            //
            mSelectNYISOHistoricalIIROutageCommand = new SqlCommand();
            mSelectNYISOHistoricalIIROutageCommand.CommandText = "select P.OutageID,P.UnitName,P.UnitID,P.OwnerName,P.PlantID,P.PlantName,"
                                                      + "P.NERCRegion,P.PrimaryFuel,P.SecondaryFuel,P.FuelGroup,P.OutputCapacity,P.PowerUsage,"
                                                      + "P.StartDate,P.EndDate,P.Oduration,P.Precision,P.Type,P.Status,P.DeliveryDate,"
                                                      + "P.NERCSubRegion,P.UnitType,P.HeatRate,P.UltimateOwnerID,P.UltimateOwner,"
                                                      + "P.ElectricConnection,P.CapOffline,P.TradeRegion"
                                                    + " from nyiso.IIROutage P where unitname=@unitname";
            mSelectNYISOHistoricalIIROutageCommand.Parameters.AddWithValue("@unitname", "UnitType");
            //mSelectNYISOHistoricalIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectNYISOHistoricalIIROutageCommand.Connection = VayuConnection;

            //
            mSelectSPPHistoricalIIROutageCommand = new SqlCommand();
            mSelectSPPHistoricalIIROutageCommand.CommandText = "select P.OutageID,P.UnitName,P.UnitID,P.OwnerName,P.PlantID,P.PlantName,"
                                                      + "P.NERCRegion,P.PrimaryFuel,P.SecondaryFuel,P.FuelGroup,P.OutputCapacity,P.PowerUsage,"
                                                      + "P.StartDate,P.EndDate,P.Oduration,P.Precision,P.Type,P.Status,P.DeliveryDate,"
                                                      + "P.NERCSubRegion,P.UnitType,P.HeatRate,P.UltimateOwnerID,P.UltimateOwner,"
                                                      + "P.ElectricConnection,P.CapOffline,P.TradeRegion"
                                                    + " from spp.IIROutage P where unitname=@unitname";
            mSelectSPPHistoricalIIROutageCommand.Parameters.AddWithValue("@unitname", "UnitType");
            //mSelectSPPHistoricalIIROutageCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectSPPHistoricalIIROutageCommand.Connection = VayuConnection;

        }


        #endregion

        #region LMP
        //=====================================
        //Type  Market  Label
        //25	9	    AH
        //26	9	    HU
        //27	9	    LCCRN
        //28	9	    LZ
        //29	9	    LZ_DC
        //30	9	    PCCRN
        //31	9	    PUN
        //32	9	    RN
        //33	9	    SH
        //1	    1	    ZONE
        //2	    1	    HUB
        //3	    1	    AGGREGATE
        //4	    1	    INTERFACE
        //5	    1	    EHV
        //6	    1	    LOAD
        //7	    1	    GEN
        //8	    1	    EXT
        //=====================================

        public void SendPrice(Vayu.NodePriceLibrary.Node[] nodes)
        {
            DARTNode.sRTFiveMinHash = new ConcurrentDictionary<string, ConcurrentDictionary<int, double>>();
            foreach (Vayu.NodePriceLibrary.Node priceNode in nodes)
            {
                foreach (Vayu.NodePriceLibrary.LmpTimePrice time in priceNode.LmpTimePriceList)
                {
                    if (!double.IsNaN(time.Lmp.Price))
                    {
                        DateTime hourDate = DateTime.Parse(time.MarketTime.Month + "/" + time.MarketTime.Day + "/" + time.MarketTime.Year + " " + time.MarketTime.Hour + ":00");
                        DateTime tempDate = hourDate.AddHours(1);
                        if (tempDate > hourDate)
                        {
                            exactTime = tempDate;
                        }
                        string priceKey = hourDate.AddHours(1).ToString() + priceNode.NodeId.ToString();
                        ConcurrentDictionary<int, double> minuteHash = new ConcurrentDictionary<int, double>();
                        if (DARTNode.sRTFiveMinHash.ContainsKey(priceKey))
                        {
                            minuteHash = DARTNode.sRTFiveMinHash[priceKey];
                        }
                        else
                        {
                            DARTNode.sRTFiveMinHash.TryAdd(priceKey, minuteHash);
                        }
                        minuteHash.TryAdd(time.MarketTime.Minute, time.Lmp.Price);
                    }
                }
            }
            MapControl.myMap.Children.Remove(mLMPMapLayer);
            mLMPMapLayer.Children.Clear();
            mPlacemarkList.Clear();
            //LMPSideDatagrid.Items.Clear();
            UpdatePrices();
            mRefresh = false;
            MainCalendarSilder.HE = exactTime.Hour;
            mRefresh = true;
            BuildLMPMap(mPlacemarkList);
            MapControl.myMap.Children.Add(mLMPMapLayer);
        }

        private void ConnectToProxy()
        {
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.OpenTimeout = new TimeSpan(0, 30, 0);
            myBinding.SendTimeout = new TimeSpan(0, 12, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
            myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
            myBinding.TransactionFlow = false;
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.TransferMode = TransferMode.Buffered;
            myBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            mPipeFactory = new DuplexChannelFactory<INodePriceFiveMin>(new InstanceContext(this), myBinding,
                new EndpointAddress(Vayu.CommonAccessLibrary.ServiceConnections.GetLMPFiveMinService()));
            foreach (var operationDescription in mPipeFactory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = operationDescription.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                                as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
        }

        private void SubscribeToLiveData()
        {
            try
            {
                mNodeProxy = mPipeFactory.CreateChannel();
                mNodeProxy.Subscribe(mMarketInContext);
                ((ICommunicationObject)mNodeProxy).Faulted += new EventHandler(LoadServerProxyFaulted);
            }
            catch (Exception ex)
            {
                LoadServerProxyFaulted(mNodeProxy, null);
            }
        }

        void LoadServerProxyFaulted(object sender, EventArgs e)
        {
            try
            {
                IChannel channel = sender as IChannel;
                if (channel != null)
                {
                    channel.Abort();
                    channel.Close();
                }
                mNodeProxy = null;
            }
            catch (Exception)
            {
            }
            sReconnectTimer.Start();
        }

        private void HeartBeat(object sender, ElapsedEventArgs e)
        {
            if (mNodeProxy != null)
            {
                mNodeProxy.HeartBeat();
            }
        }

        private void ReconnectToProxy(object sender, ElapsedEventArgs e)
        {
            if (sReconnectTimer != null)
            {
                sReconnectTimer.Stop();
            }
            SubscribeToLiveData();
        }

        //private void LMPsFilters_routedEventHandler(object sender, PropertyChangedEventArgs e)
        //{
        //    LMPMain();
        //}

        private void ConstraintsFilters_routedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            Constraint_Apply_button_Click(null, null);
        }

        private void OutagesFilters_routedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            OutageApply_button_Click(null, null);
        }

        #endregion

        #region Other Methods or Events

        private void Refreshbutton_Click(object sender, RoutedEventArgs e)
        {
            EnableEvents = false;
            LiveCheckBox.IsChecked = false;
            InitParameter();
            BuildDisplay();
            EnableEvents = true;
            DataGridColumn FuelTypeColumn = LMPSideDatagrid.Columns.Where(c => c.Header.ToString() == "Fuel Type").FirstOrDefault();
            DataGridColumn TypeColumn = LMPSideDatagrid.Columns.Where(c => c.Header.ToString() == "Type").FirstOrDefault();
            if (mMarketInContext == 1)
            {

                if (FuelTypeColumn != null)
                {
                    FuelTypeColumn.Visibility = Visibility.Collapsed;
                    TypeColumn.Visibility = Visibility.Visible;
                }
            }
            if (mMarketInContext == 9)
            {
                if (FuelTypeColumn != null)
                {
                    TypeColumn.Visibility = Visibility.Collapsed;
                    FuelTypeColumn.Visibility = Visibility.Visible;
                }
            }

        }

        private void BuildDisplay()
        {
            if (mMarketInContext != 0)
            {
                LMPMain();
                FetchConstraintData();
                OutagePreLoad();
                //IIROutagePreLoad();
                TransmissionMain();
                TemperaturePreLoad();
                FlyTo(mMarketLocation[mMarketInContext], 6);
            }
            else
            {
                MessageBox.Show("Please choose a market to proceed");
            }
        }
        private void Outage_Side_DataGrid2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid mygrid = (DataGrid)sender;
            if (mygrid.SelectedItem != null && MarketlistBox.SelectedItem.ToString() == "PJM")
            {
                object outage = mygrid.SelectedItem;
                MappingDialog mappingdialog = new MappingDialog(outage);
                mappingdialog.Show();
            }
        }
        private void Constraint_Side_DataGrid2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid mygrid = (DataGrid)sender;
            if (mygrid.SelectedItem != null && MarketlistBox.SelectedItem.ToString() == "PJM")
            {
                object myobj = mygrid.SelectedItem;
                MappingDialogConstraint mappingdialog = new MappingDialogConstraint(myobj);
                mappingdialog.Show();
            }
        }
        private void OutageApply_button_Click(object sender, RoutedEventArgs e)
        {
            OutagePreLoad();
        }
        private void Constraint_Apply_button_Click(object sender, RoutedEventArgs e)
        {
            ConstraintMain();
        }
        private void Transmission_Apply_button_Click(object sender, RoutedEventArgs e)
        {
            TransmissionMain();
        }
        private void navigateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            ContextMenu contextMenu = menu.Parent as ContextMenu;
            DataGrid dataGrid = contextMenu.PlacementTarget as DataGrid;
            var myobj = dataGrid.SelectedItem;
            Navigation(myobj);
        }
        private void Navigation(object T)
        {
            MapControl.myMap.Children.Remove(mNavigationMapLayer);
            MapControl.myMap.Children.Remove(mNavigationLMPLayer);
            mNavigationMapLayer.Children.Clear();
            Pushpin fromPushPin = new Pushpin();
            Pushpin toPushPin = new Pushpin();
            fromPushPin.Background = mNavigateFromColor;
            toPushPin.Background = mNavigateToColor;
            if (T is Outage)
            {
                Outage outage = T as Outage;
                fromPushPin.Location = outage.branch_location;
                if (outage.equipmentType == "LINE" || outage.equipmentType == "LN")
                {
                    toPushPin.Location = outage.tobranch_location;
                    mNavigationMapLayer.Children.Add(fromPushPin);
                    mNavigationMapLayer.Children.Add(toPushPin);
                }
                else
                {
                    mNavigationMapLayer.Children.Add(fromPushPin);
                }
                FlyTo(outage.branch_location, 8);
            }
            if (T is Constraintobj)
            {
                Constraintobj constraint = T as Constraintobj;
                fromPushPin.Location = constraint.branch_location;

                if (constraint.type == "XF" || constraint.type == "XFORMER")
                {
                    mNavigationMapLayer.Children.Add(fromPushPin);
                }
                else
                {
                    toPushPin.Location = constraint.tobranch_location;
                    mNavigationMapLayer.Children.Add(fromPushPin);
                    mNavigationMapLayer.Children.Add(toPushPin);
                }
                if (toPushPin.Location != null && fromPushPin.Location != null)
                {
                    FlyTo(constraint.branch_location, 8);
                }
            }
            if (T is point)
            {
                point p = T as point;
                Location location = new Location(p.Parent.Latitude, p.Parent.Longitude);
                fromPushPin.Location = location;
                ToolTip tt = new ToolTip();
                tt.Content = p.Parent.points[0].NodeName;
                tt.FontWeight = FontWeights.Bold;
                fromPushPin.ToolTip = tt;
                mNavigationLMPLayer.Children.Add(fromPushPin);
                FlyTo(location, 8);
            }
            if (T is Temperature)
            {
                Temperature p = T as Temperature;
                Location location = new Location(p.Latitude, p.Longitude);//29.378137 -94.932678
                fromPushPin.Location = location;
                ToolTip tt = new ToolTip();
                tt.Content = p.City;
                tt.FontWeight = FontWeights.Bold;
                fromPushPin.ToolTip = tt;
                mNavigationLMPLayer.Children.Add(fromPushPin);
                FlyTo(location, 8);
            }
            if (T is BranchModel)
            {
                BranchModel branch = T as BranchModel;
                fromPushPin.Location = branch.branch_location;
                toPushPin.Location = branch.tobranch_location;
                mNavigationMapLayer.Children.Add(fromPushPin);
                mNavigationMapLayer.Children.Add(toPushPin);
                FlyTo(branch.branch_location, 8);
            }
            if (T is IIROutages)
            {
                IIROutages iirOutages = T as IIROutages;
                Location location = new Location(iirOutages.Lattitude, iirOutages.Longitude);
                fromPushPin.Location = location;
                //ToolTip TT = new ToolTip();
                //TT.Content=
                mNavigationMapLayer.Children.Add(fromPushPin);
                FlyTo(location, 8);
            }
            MapControl.myMap.Children.Add(mNavigationMapLayer);
            MapControl.myMap.Children.Add(mNavigationLMPLayer);
            Maptab.Focus();
        }
        private void FlyTo(Location location, double zonelevel)
        {

            MapControl.myMap.Center = location;
            MapControl.myMap.ZoomLevel = zonelevel;
        }
        private void congestion_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Congestion congestionWindow = new Congestion();

            CongestionViewModel congestionViewModel = new CongestionViewModel();
            List<OutageZone> equipmentList = new List<OutageZone>();
            foreach (Outage outage in OutageSideDataGrid1.SelectedItems)
            {
                OutageZone outageZone = new OutageZone();
                outageZone.Outage = outage.equipment;
                outageZone.Voltage = outage.voltage;
                string str = outage.equipment;
                if (str.ToLower().StartsWith("s"))
                {
                    try
                    {
                        if (str.IndexOf('-') == 1)
                        {
                            string[] tokens = str.Split('-');
                            double.Parse(tokens[1]);
                            for (int i = 2; i < tokens.Length; i++)
                            {
                                string outvalue = tokens[i];
                                if (i > 2)
                                {
                                    outvalue = outageZone.Outage + "-" + outvalue;
                                }
                                outageZone.Outage = outvalue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                outageZone.Zone = outage.zone;
                equipmentList.Add(outageZone);
            }
            congestionViewModel.SetPath(equipmentList);
            congestionWindow.DataContext = congestionViewModel;
            congestionWindow.Show();
        }
        private void history_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            ContextMenu contextMenu = menu.Parent as ContextMenu;
            DataGrid dataGrid = contextMenu.PlacementTarget as DataGrid;
            DataTable temptable = new DataTable();
            foreach (Outage outage in dataGrid.SelectedItems)
            {
                DataTable temptable1 = GetHistoricalOutages(outage);
                temptable.Merge(temptable1);
            }
            HistorydataGrid.DataContext = temptable;
            Historytab.Focus();
        }
        private void iirHistory_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            ContextMenu contextMenu = menu.Parent as ContextMenu;
            DataGrid dataGrid = contextMenu.PlacementTarget as DataGrid;
            DataTable iirTemptable = new DataTable();
            foreach (IIROutages iiroutages in IIROutageSideDataGrid1.SelectedItems)
            {
                iirTemptable = GetIIRHistoricalOutages(iiroutages);
            }
            HistorydataGrid.DataContext = iirTemptable;
            Historytab.Focus();
        }
        private DataTable GetIIRHistoricalOutages(IIROutages iirOutages)
        {
            DataTable iirTemptable = new DataTable();
            SqlDataAdapter iiradapter = new SqlDataAdapter();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            if (mMarketInContext == mMarkets["PJM"])
            {
                mSelectPJMHistoricalIIROutageCommand.Parameters["@UnitName"].Value = iirOutages.UnitName;
                iiradapter.SelectCommand = mSelectPJMHistoricalIIROutageCommand;
            }
            else if (mMarketInContext == mMarkets["ERCOT"])
            {
                mSelectErcotHistoricalIIROutageCommand.Parameters["@UnitName"].Value = iirOutages.UnitName;
                iiradapter.SelectCommand = mSelectErcotHistoricalIIROutageCommand;
            }
            else if (mMarketInContext == mMarkets["MISO"])
            {
                mSelectMISOHistoricalIIROutageCommand.Parameters["@UnitName"].Value = iirOutages.UnitName;
                iiradapter.SelectCommand = mSelectMISOHistoricalIIROutageCommand;
            }
            else if (mMarketInContext == mMarkets["NYISO"])
            {
                mSelectNYISOHistoricalIIROutageCommand.Parameters["@UnitName"].Value = iirOutages.UnitName;
                iiradapter.SelectCommand = mSelectNYISOHistoricalIIROutageCommand;
            }
            else if (mMarketInContext == mMarkets["SPP"])
            {
                mSelectSPPHistoricalIIROutageCommand.Parameters["@UnitName"].Value = iirOutages.UnitName;
                iiradapter.SelectCommand = mSelectSPPHistoricalIIROutageCommand;
            }
            iiradapter.Fill(iirTemptable);
            VayuConnection.Close();
            return iirTemptable;
        }
        private DataTable GetHistoricalOutages(Outage myoutage)
        {
            DataTable temptable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            if (mMarketInContext == mMarkets["PJM"])
            {
                mSelectPJMHistoricalOutageCommand.Parameters["@equipmenttype"].Value = myoutage.equipmentType;
                mSelectPJMHistoricalOutageCommand.Parameters["@branch"].Value = myoutage.branch;
                mSelectPJMHistoricalOutageCommand.Parameters["@voltage"].Value = myoutage.voltage;
                mSelectPJMHistoricalOutageCommand.Parameters["@equipment"].Value = myoutage.equipment;
                adapter.SelectCommand = mSelectPJMHistoricalOutageCommand;
            }
            else if (mMarketInContext == mMarkets["ERCOT"])
            {
                mSelectERCOTHistoricalOutageCommand.Parameters["@equipmenttype"].Value = myoutage.equipmentType;
                mSelectERCOTHistoricalOutageCommand.Parameters["@branch"].Value = myoutage.branch;
                mSelectERCOTHistoricalOutageCommand.Parameters["@equipment"].Value = myoutage.equipment;
                adapter.SelectCommand = mSelectERCOTHistoricalOutageCommand;
            }
            else if (mMarketInContext == mMarkets["MISO"])
            {
                mSelectMISOHistoricalOutageCommand.Parameters["@equipmenttype"].Value = myoutage.equipmentType;
                mSelectMISOHistoricalOutageCommand.Parameters["@equipment"].Value = myoutage.equipment;
                adapter.SelectCommand = mSelectMISOHistoricalOutageCommand;

            }
            else if (mMarketInContext == mMarkets["SPP"])
            {
                // mSelecSPPHistoricalOutageCommand.Parameters["@voltage"].Value = KeyArray[2];
                mSelecSPPHistoricalOutageCommand.Parameters["@equipment"].Value = myoutage.equipment;
                adapter.SelectCommand = mSelecSPPHistoricalOutageCommand;
            }
            adapter.Fill(temptable);
            VayuConnection.Close();
            return temptable;
        }

        #endregion

        #region INotification
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Example Methods

        private void DrawCurve(Location start, Location finish)
        {
            MapPolyline curve = new MapPolyline();
            curve.Stroke = new SolidColorBrush(Colors.Magenta);
            curve.StrokeThickness = 3.0;
            curve.StrokeDashArray = new DoubleCollection(new double[] { 5, 2 });
            curve.Opacity = 1.0;

            LocationCollection coll = new LocationCollection();

            // Compute control point
            Point p1 = MapControl.myMap.LocationToViewportPoint(start);
            Point p2 = MapControl.myMap.LocationToViewportPoint(finish);

            double boundingBoxHalfWidth = (p2.X - p1.X) / 2.0;
            double boundingBoxHalfHeight = (p1.Y - p2.Y) / 2.0;

            double centerX = (p1.X + p2.X) / 2.0;
            double centerY = (p1.Y + p2.Y) / 2.0;

            double xScale = 1.0;
            double yScale = 1.0;

            double controlX = centerX - (xScale * boundingBoxHalfWidth);
            double controlY = centerY - (yScale * boundingBoxHalfHeight);

            Point controlPt = new Point(controlX, controlY);
            Location controlLoc = MapControl.myMap.ViewportPointToLocation(controlPt);

            // Compute intermediate Locations using the control point
            int numSegments = 31;
            double tDelta = 1.0 / numSegments;
            for (double t = 0.0; t <= 1.0; t += tDelta) // Danger!
            {
                // x coordinate (longitudes)
                double a = (1.0 - t) * (1.0 - t) * start.Longitude;
                double b = 2.0 * (1.0 - t) * t * controlLoc.Longitude;
                double c = t * t * finish.Longitude;
                double Blon = a + b + c;

                // y coordinate (latitudes)
                a = (1.0 - t) * (1.0 - t) * start.Latitude;
                b = 2.0 * (1.0 - t) * t * controlLoc.Latitude;
                c = t * t * finish.Latitude;
                double Blat = a + b + c;

                coll.Add(new Location(Blat, Blon));
            }

            curve.Locations = coll;
            MapControl.myMap.Children.Add(curve);
        }

        private void AddTileOverlay()
        {
            // Create a new map third_maplayer to add the tile overlay to.
            tileLayer = new MapTileLayer();
            // The source of the overlay.
            TileSource tileSource = new TileSource();
            //tileSource.UriFormat = "{UriScheme}://ecn.t0.tiles.virtualearth.net/tiles/r{quadkey}.jpeg?g=129&mkt=en-us&shading=hill&stl=H";
            tileSource.UriFormat =
                "{UriScheme}://ecn.t0.tiles.virtualearth.net/tiles/r{quadkey}.jpeg?g=129&mkt=en-us&shading=&stl=H";
            // Add the tile overlay to the map third_maplayer
            tileLayer.TileSource = tileSource;
            // Add the map third_maplayer to the map
            if (!MapControl.myMap.Children.Contains(tileLayer))
            {
                MapControl.myMap.Children.Add(tileLayer);
            }
            tileLayer.Opacity = tileOpacity;
        }

        #endregion

        private void FilterLMPs(EventPattern<KeyEventArgs> e)
        {
            LMPMain();

            Observable.Timer(TimeSpan.FromMilliseconds(1000))
                .ObserveOnDispatcher()
                .Take(1)
                .Subscribe((d) =>
                {
                });
        }
        private void FilterConstraints(EventPattern<KeyEventArgs> e)
        {
            Constraint_Apply_button_Click(null, null);
            Observable.Timer(TimeSpan.FromMilliseconds(1000))
                .ObserveOnDispatcher()
                .Take(1)
                .Subscribe((d) =>
                {
                });
        }
        private void FilterOutages(EventPattern<KeyEventArgs> e)
        {
            OutageApply_button_Click(null, null);
            Observable.Timer(TimeSpan.FromMilliseconds(1000))
                .ObserveOnDispatcher()
                .Take(1)
                .Subscribe((d) =>
                {
                });
        }
        private void FilterTransmission(EventPattern<KeyEventArgs> e)
        {
            Transmission_Apply_button_Click(null, null);
            Observable.Timer(TimeSpan.FromMilliseconds(1000))
                .ObserveOnDispatcher()
                .Take(1)
                .Subscribe((d) =>
                {

                });
        }
        private void FilterOutageCorrelation(EventPattern<KeyEventArgs> e)
        {
            Outage_Correlation_dataGrid_SelectionChanged(null, null);
            Observable.Timer(TimeSpan.FromMilliseconds(1000))
                .ObserveOnDispatcher()
                .Take(1)
                .Subscribe((d) =>
                {
                });
        }

        private void LMP_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            AutoCompleteBox auto = (AutoCompleteBox)sender;
            if (auto.SelectedItem != null)
            {
                string selectedItem = auto.SelectedItem.ToString();
                List<point> pList = System.Linq.Enumerable.Cast<point>(LMPSideDatagrid.Items).ToList();
                LMPSideDatagrid.SelectedItem = pList.Find(p => p.NodeName == selectedItem);
                LMPSideDatagrid.ScrollIntoView(LMPSideDatagrid.SelectedItem);
            }
        }
        private void Constraint_autoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            AutoCompleteBox auto = (AutoCompleteBox)sender;
            if (auto.SelectedItem != null)
            {
                string selectedItem = auto.SelectedItem.ToString();
                List<Constraintobj> cList = ConstraintGeoGrid.Items.Cast<Constraintobj>().ToList();
                ConstraintGeoGrid.SelectedItem = cList.Find(p => p.constraint == selectedItem);
                ConstraintGeoGrid.ScrollIntoView(ConstraintGeoGrid.SelectedItem);
            }
        }
        private void Outage_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            AutoCompleteBox auto = (AutoCompleteBox)sender;
            if (auto.SelectedItem != null)
            {
                string selectedItem = auto.SelectedItem.ToString();
                List<Outage> cList = OutageSideDataGrid1.Items.Cast<Outage>().ToList();
                OutageSideDataGrid1.SelectedItem = cList.Find(p => p.equipment == selectedItem);
                OutageSideDataGrid1.ScrollIntoView(OutageSideDataGrid1.SelectedItem);
            }
        }
        private void Transmission_autoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            AutoCompleteBox auto = (AutoCompleteBox)sender;
            if (auto.SelectedItem != null)
            {
                string selectedItem = auto.SelectedItem.ToString();
                List<BranchModel> cList = Transmission_Side_datagrid.Items.Cast<BranchModel>().ToList();
                Transmission_Side_datagrid.SelectedItem = cList.Find(p => p.branchname == selectedItem);
                Transmission_Side_datagrid.ScrollIntoView(Transmission_Side_datagrid.SelectedItem);
            }
        }
        private void IIROutage_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            AutoCompleteBox auto = (AutoCompleteBox)sender;
            if (auto.SelectedItem != null)
            {
                string selectedItem = auto.SelectedItem.ToString();
                List<IIROutages> cList = IIROutageSideDataGrid1.Items.Cast<IIROutages>().ToList();
                IIROutageSideDataGrid1.SelectedItem = cList.Find(a => a.PlantUnitName == selectedItem);
                IIROutageSideDataGrid1.ScrollIntoView(IIROutageSideDataGrid1.SelectedItem);
            }
        }
        private void RT_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            /*int interval = Convert.ToInt32(RTtextBox.Text);
            dispatcherTimer.Interval = new TimeSpan(0, 0, interval);
            dispatcherTimer.Start();
            RTCheckBox.Background = Brushes.Green;*/
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            exactDate = curTimeZone.IsDaylightSavingTime(DateTime.Now) ? DateTime.Now : DateTime.Now.AddHours(1);
            // exactDate = DateTime.Now;
            exactTime = exactDate.AddHours(DateTime.Now.Hour + 1);
            MainCalendarSilder.IsHour = true;
            BuildDisplay();
        }
        private void RT_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            /*dispatcherTimer.Stop();
            RTCheckBox.Background = Brushes.Gainsboro;*/
        }
        public void Dispose()
        {
            keyPressSubLMPs.Dispose();
            mLMPMapLayer = null;
            mConstraintMapLayer = null;
            mOutageMapLayer = null;
            mPortfolioMapLayer = null;
            mTransmissionMapLayer = null;
            mVirtualMapLayer = null;
            mNavigationMapLayer = null;
            mNavigationLMPLayer = null;
            mIIROutageMapLayer = null;
        }

        #region Outage Correlation

        private void Correlation_Click(object sender, RoutedEventArgs e)
        {
            Correlation_Main(Outage_dataGrid.Items);
        }
        private void CorrelationSelecteditem_Click(object sender, RoutedEventArgs e)
        {
            Correlation_Main(Outage_dataGrid.SelectedItems);
        }
        private void Correlation_Main(object OutageItem)
        {
            Outage_Correlation_dataGrid.Items.Clear();
            Constraint_Correlation_dataGrid.Items.Clear();
            Task loadCorrelationTask = Task.Factory.StartNew(delegate
            {
                Correlation_Dataloading(OutageItem);
            }).ContinueWith(ret => Outage_CorrelationBuildGrid(), TaskScheduler.FromCurrentSynchronizationContext());
            //Outage_CorrelationBuildGrid();
        }
        private void Correlation_Dataloading(object OutageItem)
        {
            mHistoricalOutagesList = new List<OutageCorrelationModel>();
            foreach (var item in (IEnumerable)OutageItem)
            {
                Dictionary<int, List<string>> stationlevelhash = new Dictionary<int, List<string>>();
                string timestring = "";
                if (item is Outage)
                {
                    OutageCorrelationModel outagecorrelationmodel = new OutageCorrelationModel();
                    DataTable temptable = GetHistoricalOutages(item as Outage);
                    Outage outage = item as Outage;
                    //outagecorrelationmodel.outage = item as Outage;
                    outagecorrelationmodel.equipment = outage.equipment;
                    outagecorrelationmodel.branch = outage.branch;
                    outagecorrelationmodel.tobranch = outage.tobranch;
                    outagecorrelationmodel.branch_location = outage.branch_location;
                    outagecorrelationmodel.tobranch_location = outage.tobranch_location;
                    outagecorrelationmodel.equipmentType = outage.equipmentType;
                    outagecorrelationmodel.voltage = outage.voltage;
                    outagecorrelationmodel.outagetimes = HistoricalOutage_BuildObject(temptable);
                    if (mMarketInContext == mMarkets["PJM"])
                        outagecorrelationmodel.stationlevelhash = BranchBinding(outagecorrelationmodel.branch, outagecorrelationmodel.tobranch, stationlevelhash, 1);
                    foreach (OutageTimes times in outagecorrelationmodel.outagetimes)
                    {
                        if (times.StartTime != null || times.EndTime != null)
                        {
                            timestring += string.Format("{0} - {1}\n", times.StartTime, times.EndTime);
                        }
                    }
                    outagecorrelationmodel.timestring = timestring;
                    SqlCommand command = new SqlCommand();
                    command.Connection = VayuConnection;
                    command.Parameters.AddWithValue("@marketkey", "marketkey");
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    DataTable Constraint_Table = new DataTable();
                    string whereexpression = "";
                    List<string> expressionlist = new List<string>();

                    if (VayuConnection.State == ConnectionState.Open)
                        VayuConnection.Close();

                    VayuConnection.Open();
                    foreach (OutageTimes outagetime in outagecorrelationmodel.outagetimes)
                    {
                        if (outagetime.StartTime != null && outagetime.EndTime != null)
                        {
                            expressionlist.Add(string.Format(" marketdatetime between '{0:MM/dd/yyyy HH:mm:ss}' and '{1:MM/dd/yyyy HH:mm:ss}' ", outagetime.StartTime, outagetime.EndTime));
                        }
                    }
                    if (expressionlist.Count > 0)
                    {
                        whereexpression = " and (" + string.Join("or", expressionlist) + ")";
                        command.CommandText = mSelectConstraintHistoryCommand.CommandText.Replace("@TimeLinesString", whereexpression);
                        command.Parameters["@marketkey"].Value = mMarketInContext;
                        adapter.SelectCommand = command;
                        adapter.Fill(Constraint_Table);
                        outagecorrelationmodel.constraintgroup = HistoricalConstraint_BuildObject(Constraint_Table, outagecorrelationmodel);
                        outagecorrelationmodel.constraintgroup.OrderByDescending(p => p.ConstraintOccurrences);
                    }
                    VayuConnection.Close();
                    mHistoricalOutagesList.Add(outagecorrelationmodel);
                }
            }

        }
        private void Outage_CorrelationBuildGrid()
        {
            foreach (OutageCorrelationModel outagecorrelationmodel in mHistoricalOutagesList)
            {
                Outage_Correlation_dataGrid.Items.Add(outagecorrelationmodel);
            }
            Correlationtab.Focus();
        }

        private List<OutageTimes> HistoricalOutage_BuildObject(DataTable mytable)
        {
            List<OutageTimes> tempoutagetimes = new List<OutageTimes>();
            foreach (DataRow mydatarow in mytable.Rows)
            {
                OutageTimes outagetimes = new OutageTimes();
                if (!(mydatarow["ActualStart"] is DBNull) && mydatarow["ActualStart"] != null)
                {
                    outagetimes.StartTime = Convert.ToDateTime(mydatarow["ActualStart"]);
                }
                if (!(mydatarow["ActualEnd"] is DBNull) && mydatarow["ActualEnd"] != null)
                {
                    outagetimes.EndTime = Convert.ToDateTime(mydatarow["ActualEnd"]);
                }
                if ((!(mydatarow["ActualStart"] is DBNull) && mydatarow["ActualStart"] != null) && (mydatarow["ActualEnd"] is DBNull || mydatarow["ActualEnd"] == null))
                {
                    outagetimes.EndTime = DateTime.Now;
                }
                //tempoutagelist.historicaloutage = outage;
                tempoutagetimes.Add(outagetimes);
            }
            return tempoutagetimes;
        }
        private List<ConstraintsGroup> HistoricalConstraint_BuildObject(DataTable mytable, OutageCorrelationModel outagecorrelationmodel)
        {
            List<ConstraintsGroup> constraintlist = new List<ConstraintsGroup>();
            foreach (DataRow mydatarow in mytable.Rows)
            {
                List<double> distancelist = new List<double>();
                ConstraintsGroup constraint = new ConstraintsGroup();
                constraint.type = mydatarow["Type"].ToString().Trim();
                if (constraint.type == "GC")
                {
                    continue;
                }
                constraint.contingency = mydatarow["ContingencyName"].ToString().Trim();
                constraint.constraint = mydatarow["ConstraintName"].ToString().Trim();
                constraint.constraint_stationFrom = mydatarow["SubFrom"].ToString().Trim();
                constraint.constraint_stationTo = mydatarow["SubTo"].ToString().Trim();
                constraint.MarketKey = Convert.ToInt32(mydatarow["MarketKey"]);
                constraint.ConstraintOccurrences = Convert.ToInt32(mydatarow["Occurrences"]);
                constraint.shadowprice = Convert.ToDouble(mydatarow["ShadowPrice"]);
                constraint.branch_location = FindCoordinate(constraint.constraint_stationFrom);
                constraint.tobranch_location = FindCoordinate(constraint.constraint_stationTo);

                if (outagecorrelationmodel.branch_location != null && constraint.branch_location != null)
                {
                    distancelist.Add(Distance(constraint.branch_location, outagecorrelationmodel.branch_location));
                }
                if (outagecorrelationmodel.branch_location != null && constraint.tobranch_location != null)
                {
                    distancelist.Add(Distance(constraint.tobranch_location, outagecorrelationmodel.branch_location));
                }
                if (outagecorrelationmodel.tobranch_location != null && constraint.branch_location != null)
                {
                    distancelist.Add(Distance(constraint.branch_location, outagecorrelationmodel.tobranch_location));
                }
                if (outagecorrelationmodel.tobranch_location != null && constraint.tobranch_location != null)
                {
                    distancelist.Add(Distance(constraint.tobranch_location, outagecorrelationmodel.tobranch_location));
                }
                if (distancelist.Count > 0)
                {
                    constraint.distancemiles = distancelist.Min();
                }
                if (mMarketInContext == mMarkets["PJM"])
                {
                    foreach (KeyValuePair<int, List<string>> item in outagecorrelationmodel.stationlevelhash)
                    {
                        if (item.Value.Contains(constraint.constraint_stationFrom) || item.Value.Contains(constraint.constraint_stationTo))
                        {
                            constraint.busaway = (double)item.Key - 1;
                            break;
                        }
                    }
                }
                constraintlist.Add(constraint);
            }
            return constraintlist;
        }

        private Dictionary<int, List<string>> BranchBinding(string fromst, string tost, Dictionary<int, List<string>> stationlevelhash, int level)
        {
            List<BranchModel> markettempbranch = mBranchList.Where(p => p.marketkey == mMarketInContext && p.devicetype != "Transformer").ToList();
            if (level == 1)
            {
                stationlevelhash.Add(1, new List<string>());
                stationlevelhash[1].Add(fromst);
                stationlevelhash[1].Add(tost);
                level++;
            }

            int i = 0;
            if ((fromst != null && tost != null) || (level >= stationlevelhash.Count))
                return stationlevelhash;

            while (stationlevelhash[level - 1].Count != 0)
            {
                int counter = 0;
                List<BranchModel> tempbranch = markettempbranch.FindAll(p => p.branch == stationlevelhash[level - 1][i] || p.tobranch == stationlevelhash[level - 1][i]);
                foreach (BranchModel branch in tempbranch)
                {
                    if (!stationlevelhash.ContainsKey(level))
                    {
                        stationlevelhash.Add(level, new List<string>());
                    }
                    if (branch.branch == stationlevelhash[level - 1][i] && !stationlevelhash[level].Contains(branch.tobranch))
                    {
                        stationlevelhash[level].Add(branch.tobranch);
                        counter++;
                    }
                    if (branch.tobranch == stationlevelhash[level - 1][i] && !stationlevelhash[level].Contains(branch.branch))
                    {
                        stationlevelhash[level].Add(branch.branch);
                        counter++;
                    }
                    markettempbranch.Remove(branch);
                }
                i++;
                if (i == stationlevelhash[level - 1].Count)
                {
                    if (markettempbranch.Count < 1000 || level == 20) break;
                    level++;
                    i = 0;
                }
            }
            return stationlevelhash;
        }

        private void Outage_Correlation_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            Constraint_Correlation_dataGrid.Items.Clear();
            Int16 mile;
            Int16 bus;
            if (Outage_Correlation_dataGrid.SelectedItem is OutageCorrelationModel)
            {
                OutageCorrelationModel selectedoutagemodel = Outage_Correlation_dataGrid.SelectedItem as OutageCorrelationModel;
                if (!Int16.TryParse(Outage_Correlation_Mile_textBox.Text, out mile))
                {
                    mile = Int16.MaxValue;
                }
                if (!Int16.TryParse(Outage_Correlation_Bus_textBox.Text, out bus))
                {
                    bus = Int16.MaxValue;
                }

                if (selectedoutagemodel.constraintgroup != null)
                {
                    foreach (ConstraintsGroup constraitsgroup in selectedoutagemodel.constraintgroup)
                    {
                        if (mile >= constraitsgroup.distancemiles || double.IsNaN(constraitsgroup.distancemiles))
                        {
                            if (bus >= constraitsgroup.busaway || double.IsNaN(constraitsgroup.busaway))
                            {
                                Constraint_Correlation_dataGrid.Items.Add(constraitsgroup);
                            }
                        }
                    }
                }
            }
        }
        private double Distance(Location from, Location to)
        {
            double lat1 = from.Latitude;
            double lon1 = from.Longitude;
            double lat2 = to.Latitude;
            double lon2 = to.Longitude;
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            return Math.Abs(dist);
        }
        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        #endregion

        private void PlaceMarkClear_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MapControl.myMap.Children.Remove(mNavigationLMPLayer);
            mNavigationLMPLayer.Children.Clear();

            MapControl.myMap.Children.Remove(mNavigationMapLayer);
            mNavigationMapLayer.Children.Clear();
        }
        private void NodeSpreadAnalysis_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                MenuItem menu = (MenuItem)sender;
                ContextMenu contextMenu = menu.Parent as ContextMenu;
                string[] nodedetails = (menu.Uid).Split('@');

                if (mLMPStatisticsWindow == null || mLMPStatisticsWindow.IsVisible == false)
                {
                    mLMPStatisticsViewModel = new Vayu.LMPStatistics.ViewModels.MainWindowViewModel(new Vayu.LMPStatistics.Model.DataService());
                    mLMPStatisticsWindow = new LMPStatistics.Views.MainWindow();
                    mLMPStatisticsWindow.DataContext = mLMPStatisticsViewModel;
                    mSourceSinkList = new List<SourceSinkData>();
                }
                SourceSinkData sourceSinkData = new SourceSinkData();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mSelectNodeDetailCommand.Parameters["@NodeKey"].Value = nodedetails[0];
                mSelectNodeDetailCommand.Parameters["@marketkey"].Value = nodedetails[1];
                SqlDataReader reader = mSelectNodeDetailCommand.ExecuteReader();
                PricingNode priceNode = new PricingNode();
                if (reader.Read())
                {
                    if (reader[2] != null)
                    {
                        priceNode.ExternalNodeId = int.Parse(reader[2].ToString());
                    }
                    priceNode.NodeTypeKey = int.Parse(reader[3].ToString());
                }
                else
                {
                    priceNode.ExternalNodeId = 0;
                    priceNode.NodeTypeKey = int.Parse(nodedetails[4]);
                }
                priceNode.MarketKey = int.Parse(nodedetails[1]);
                priceNode.NodeKey = int.Parse(nodedetails[0]);
                priceNode.NodeName = nodedetails[2];
                priceNode.Zone = nodedetails[3];
                sourceSinkData.Source = priceNode;
                reader.Close();
                VayuConnection.Close();
                sourceSinkData.Sink = null;
                mSourceSinkList.Add(sourceSinkData);
                mLMPStatisticsViewModel.SetSourceSinkList(mSourceSinkList);
                if (LMP_RT_radioButton.IsChecked == true)
                {
                    mLMPStatisticsViewModel.FilterDayComparisonRTChecked = true;
                    mLMPStatisticsViewModel.SelectedSpreadType = LMPStatistics.ViewModels.SpreadType.RT;
                }
                if (LMP_DA_radioButton.IsChecked == true)
                {
                    mLMPStatisticsViewModel.FilterDayComparisonDAChecked = true;
                    mLMPStatisticsViewModel.SelectedSpreadType = LMPStatistics.ViewModels.SpreadType.DA;
                }
                if (LMP_DART_radioButton.IsChecked == true)
                {
                    mLMPStatisticsViewModel.FilterDayComparisonDARTChecked = true;
                    mLMPStatisticsViewModel.SelectedSpreadType = LMPStatistics.ViewModels.SpreadType.DART;
                }
                mLMPStatisticsWindow.Show();
            }
        }

        private void LMPGraph_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                //MenuItem menu = (MenuItem)sender;
                //ContextMenu contextMenu = menu.Parent as ContextMenu;
                //string[] nodedetails = (menu.Uid).Split('@');
                //NodePriceGraph.ViewModel.MainViewModel mainViewModel = new NodePriceGraph.ViewModel.MainViewModel(new NodePriceGraph.Model.DataService());
                //var window = new NodePriceGraph.MainWindow();
                //window.DataContext = mainViewModel;
                //string market = "PJM ";
                //if (MarketlistBox.SelectedItem.Equals("ERCOT"))
                //{
                //    market = "ERCOT";
                //}
                //if (MarketlistBox.SelectedItem.Equals("MISO"))
                //{
                //    market = "MISO";
                //}
                //if (MarketlistBox.SelectedItem.Equals("CAISO"))
                //{
                //    market = "CAISO";
                //}
                //List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();
                //if (VayuConnection.State == ConnectionState.Closed)
                //{
                //    VayuConnection.Open();
                //}
                //mSelectNodeDetailCommand.Parameters["@NodeKey"].Value = nodedetails[0];
                //mSelectNodeDetailCommand.Parameters["@marketkey"].Value = nodedetails[1];
                //SqlDataReader reader = mSelectNodeDetailCommand.ExecuteReader();
                //SourceSinkData sourceSink = new SourceSinkData();
                //sourceSink.Source = new PricingNode();
                //if (reader.Read())
                //{
                //    if (reader[2] != null)
                //    {
                //        sourceSink.Source.ExternalNodeId = int.Parse(reader[2].ToString());
                //    }
                //    sourceSink.Source.NodeTypeKey = int.Parse(reader[3].ToString());
                //}
                //else
                //{
                //    sourceSink.Source.ExternalNodeId = 0;
                //    sourceSink.Source.NodeTypeKey = int.Parse(nodedetails[4]);
                //}
                //sourceSink.Source.MarketKey = int.Parse(nodedetails[1]);
                //sourceSink.Source.NodeKey = int.Parse(nodedetails[0]);
                //sourceSink.Source.NodeName = nodedetails[2];
                //sourceSink.Source.Zone = nodedetails[3];
                //sourceSinkNodeList.Add(sourceSink);
                //reader.Close();
                //VayuConnection.Close();
                //mainViewModel.AddDates(startTime, startTime);
                //mainViewModel.AddNodes(sourceSinkNodeList);
                //mainViewModel.SetMarket(market);
                //window.Show();
            }
        }
        private void DrillDownExposure_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                MenuItem menu = (MenuItem)sender;
                ContextMenu contextMenu = menu.Parent as ContextMenu;
                string[] nodedetails = (menu.Uid).Split('@');

                ConstraintExposure.ViewModels.MainWindowViewModel mainViewModel = new ConstraintExposure.ViewModels.MainWindowViewModel(new ConstraintExposure.Model.DataService());
                var window = new ConstraintExposure.Views.MainWindow();
                window.DataContext = mainViewModel;
                List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mSelectNodeDetailCommand.Parameters["@NodeKey"].Value = nodedetails[0];
                mSelectNodeDetailCommand.Parameters["@marketkey"].Value = nodedetails[1];
                SqlDataReader reader = mSelectNodeDetailCommand.ExecuteReader();
                SourceSinkData sourceSink = new SourceSinkData();
                sourceSink.Source = new PricingNode();
                if (reader.Read())
                {
                    if (reader[2] != null)
                    {
                        sourceSink.Source.ExternalNodeId = int.Parse(reader[2].ToString());
                    }
                    sourceSink.Source.NodeTypeKey = int.Parse(reader[3].ToString());
                }
                else
                {
                    sourceSink.Source.ExternalNodeId = 0;
                    sourceSink.Source.NodeTypeKey = int.Parse(nodedetails[4]);
                }
                sourceSink.Source.MarketKey = int.Parse(nodedetails[1]);
                sourceSink.Source.NodeKey = int.Parse(nodedetails[0]);
                sourceSink.Source.NodeName = nodedetails[2];
                sourceSink.Source.Zone = nodedetails[3];
                sourceSinkNodeList.Add(sourceSink);
                reader.Close();
                VayuConnection.Close();
                mainViewModel.SetRadio();
                mainViewModel.AddSource(sourceSink.Source, startTime);
                mainViewModel.RetrieveFetchDataCommand();
                window.Show();
            }
        }
        private void UpdateLocation_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                MenuItem menu = (MenuItem)sender;
                ContextMenu contextMenu = menu.Parent as ContextMenu;
                string[] nodedetails = (menu.Uid).Split('@');
                latLongUpdate = new updateLatLongDialog(int.Parse(nodedetails[0].ToString()), nodedetails[2], nodedetails[3], double.Parse(nodedetails[5].ToString()), double.Parse(nodedetails[6].ToString()));
                latLongUpdate.ShowDialog();
                if (latLongUpdate.DialogResult.HasValue && latLongUpdate.DialogResult.Value)
                {
                    BuildNodeHash();
                    Refreshbutton_Click(null, null);
                }
            }
        }

        private void Outage_Planned_radioButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Outage_Planned_radioButton.IsChecked)
            {
                Outage_Start_radioButton.IsEnabled = true;
            }
        }

        private void Outage_Start_radioButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Outage_Start_radioButton.IsChecked)
            {
                OutagePreLoad();
            }
            else
            {
                OutagePreLoad();
            }
        }

        private void Outage_Actual_radioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Outage_Actual_radioButton.IsChecked)
            {
                try
                {
                    // Outage_Start_radioButton.IsChecked = false;
                    // Outage_Start_radioButton.IsEnabled = false;
                }
                catch (Exception)
                {
                }
            }
        }

        private bool IsValidHour(int hour)
        {
            if (HE1.IsChecked == true && hour == 1)
            {
                return true;
            }
            if (HE2.IsChecked == true && hour == 2)
            {
                return true;
            }
            if (HE3.IsChecked == true && hour == 3)
            {
                return true;
            }
            if (HE4.IsChecked == true && hour == 4)
            {
                return true;
            }
            if (HE5.IsChecked == true && hour == 5)
            {
                return true;
            }
            if (HE6.IsChecked == true && hour == 6)
            {
                return true;
            }
            if (HE7.IsChecked == true && hour == 7)
            {
                return true;
            }
            if (HE8.IsChecked == true && hour == 8)
            {
                return true;
            }
            if (HE9.IsChecked == true && hour == 9)
            {
                return true;
            }
            if (HE10.IsChecked == true && hour == 10)
            {
                return true;
            }
            if (HE11.IsChecked == true && hour == 11)
            {
                return true;
            }
            if (HE12.IsChecked == true && hour == 12)
            {
                return true;
            }
            if (HE13.IsChecked == true && hour == 13)
            {
                return true;
            }
            if (HE14.IsChecked == true && hour == 14)
            {
                return true;
            }
            if (HE15.IsChecked == true && hour == 15)
            {
                return true;
            }
            if (HE16.IsChecked == true && hour == 16)
            {
                return true;
            }
            if (HE17.IsChecked == true && hour == 17)
            {
                return true;
            }
            if (HE18.IsChecked == true && hour == 18)
            {
                return true;
            }
            if (HE19.IsChecked == true && hour == 19)
            {
                return true;
            }
            if (HE20.IsChecked == true && hour == 20)
            {
                return true;
            }
            if (HE21.IsChecked == true && hour == 21)
            {
                return true;
            }
            if (HE22.IsChecked == true && hour == 22)
            {
                return true;
            }
            if (HE23.IsChecked == true && hour == 23)
            {
                return true;
            }
            if (HE24.IsChecked == true && hour == 24)
            {
                return true;
            }
            return false;
        }

        private void HE_Click(object sender, RoutedEventArgs e)
        {
            Portfolio_Auto_changes(null, null);
        }

        private void Clearbutton_Click(object sender, RoutedEventArgs e)
        {
            mPortfolioMapLayer.Children.Clear();
            dgPortfolio.Items.Clear();
        }
        private void btnHoursNone_Click(object sender, RoutedEventArgs e)
        {
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
            Portfolio_Auto_changes(null, null);
        }

        private void PeakButton_Click(object sender, RoutedEventArgs e)
        {
            SetHours("PEAK");
        }
        private void OffPeakButton_Click(object sender, RoutedEventArgs e)
        {
            SetHours("OFFPEAK");
        }
        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            SetHours("ALL");
        }

        private void SetHours(string hourType)
        {
            if (hourType == "ALL" || hourType == "OFFPEAK")
            {
                HE1.IsChecked = true;
                HE2.IsChecked = true;
                HE3.IsChecked = true;
                HE4.IsChecked = true;
                HE5.IsChecked = true;
                HE6.IsChecked = true;
                if (MarketlistBox.SelectedItem.ToString() == "PJM" || hourType == "ALL")
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
                if (MarketlistBox.SelectedItem.ToString() == "PJM")
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
                if (MarketlistBox.SelectedItem.ToString() == "PJM" || hourType == "ALL")
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
                if (MarketlistBox.SelectedItem.ToString() == "PJM")
                {
                    HE23.IsChecked = false;
                }
                else
                {
                    HE23.IsChecked = true;
                }
            }
            Portfolio_Auto_changes(null, null);
        }

        private void RefreshButtonPort_Click(object sender, RoutedEventArgs e)
        {
            Portfolio_Auto_changes(null, null);
        }
        public void SetPositions(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }


        private void Temp_radioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (temperatureList != null && MainCalendarSilder.IsRange.GetValueOrDefault())
            {
                RefreshRangeTemperatureLayer(temperatureList);
            }
        }

        private void IIROutage_SideDataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void IIRReportBtn_Click(object sender, RoutedEventArgs e)
        {
            IIRReprtWindow window = new IIRReprtWindow();
            window.GetData(Markets, mMarketInContext, PrevBusinessdate(startTime));
            window.Show();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Outage_dataGrid.SelectAllCells();
            Outage_dataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, Outage_dataGrid);
            String resultat = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            String result = (string)Clipboard.GetData(DataFormats.Text);
            Outage_dataGrid.UnselectAllCells();
            SaveFileDialog dialog = new SaveFileDialog { Filter = "xls|XLS" };
            dialog.FileName = "Outage" + "_" + DateTime.Today.ToString("yyyy-MM-dd") + ".xls";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    System.IO.StreamWriter file1 = new System.IO.StreamWriter(dialog.FileName);
                    file1.WriteLine(result.Replace(',', ' '));
                    file1.Close();
                    MessageBox.Show("Data Exported in Excel File : " + dialog.FileName, "Excel Saved");
                }
            }
        }

        private void Constraints_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            ContextMenu contextMenu = menu.Parent as ContextMenu;
            DataGrid dataGrid = contextMenu.PlacementTarget as DataGrid;
            Vayu.ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            var datacontext = new Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new Vayu.ConstraintContingencyHistory.Model.DataService());
            foreach (Outage outage in dataGrid.SelectedItems)
            {

                string con = outage.branch + "%";
                if (MarketlistBox.SelectedItem.Equals("ERCOT"))
                    datacontext.ShowHistoricalConstraintForMAp(con, 9, false);
            }
            if (datacontext.ConstraintList.Count > 0)
            {
                window.DataContext = datacontext;
                window.Show();
            }
            else
            {
                MessageBox.Show("The selected Constraint Data is not found.");
            }
        }

        private void ExportToCSV(object sender, RoutedEventArgs e)
        {
            ExportToExcelNodeSpread<Temperature, List<Temperature>> p = new ExportToExcelNodeSpread<Temperature, List<Temperature>>();
            ICollectionView view = CollectionViewSource.GetDefaultView(TemperatureListMinMax.ItemsSource);
            if (TemperatureListMinMax.Items.Count > 0)
            {
                view = CollectionViewSource.GetDefaultView(TemperatureListMinMax.ItemsSource);
            }
            List<Temperature> itemList = new List<Temperature>();
            foreach (var item in view.SourceCollection)
            {
                itemList.Add((Temperature)item);
            }
            p.dataToPrint = itemList;
            p.GenerateReport();
        }
    }

    public class StringColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = (value ?? "").ToString();
            if (string.IsNullOrEmpty(strValue))
                return null;

            string[] splits = strValue.Split(',', ' ');
            byte b1, b2, b3;
            byte.TryParse(splits[0], out b1);
            byte.TryParse(splits[1], out b2);
            byte.TryParse(splits[2], out b3);

            //splits[0]
            SolidColorBrush br = new SolidColorBrush(Color.FromRgb(b1, b2, b3));
            return br;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
