using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Vayu.NodePriceMonitor.Model;
using Vayu.NodePriceMonitorLibrary;

namespace Vayu.NodePriceMonitor.ViewModels
{
    public class MainWindowViewModel : BindableBase, INodePriceCallback
    {
        #region Declaration

        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m lock object
        /// </summary>
        private static object mLockObject = new object();
        /// <summary>
        /// The s user
        /// </summary>
        private static string sUser = Environment.UserName;
        /// <summary>
        /// The m is first
        /// </summary>
        private bool mIsFirst = true;
        /// <summary>
        /// The m subscriber LMP monitor
        /// </summary>
        private SubscriberLMPMonitor mSubscriberLMPMonitor;
        /// <summary>
        /// The da price
        /// </summary>
        private Dictionary<int, double> DAPrice = new Dictionary<int, double>();
        /// <summary>
        /// The rt compare price
        /// </summary>
        private Dictionary<int, double> RTComparePrice = new Dictionary<int, double>();
        /// <summary>
        /// The da compare price
        /// </summary>
        private Dictionary<int, double> DAComparePrice = new Dictionary<int, double>();
        /// <summary>
        /// The m cache node price price list
        /// </summary>
        public List<HourlyLMP> mCacheNodePricePriceList = new List<HourlyLMP>();
        /// <summary>
        /// The m exante list
        /// </summary>
        private List<HourlyNodePriceDetails> mExanteList = new List<HourlyNodePriceDetails>();
        /// <summary>
        /// The m cache da price
        /// </summary>
        private Dictionary<string, Dictionary<int, double>> mCacheDAPrice = new Dictionary<string, Dictionary<int, double>>();
        /// <summary>
        /// The m cache rt price
        /// </summary>
        private Dictionary<string, Dictionary<int, double>> mCacheRTPrice = new Dictionary<string, Dictionary<int, double>>();

        private System.Timers.Timer heartBeatTimer = null;
        private INodePrice proxy;

        private string mlmpMonitorServerUrl = Vayu.CommonAccessLibrary.ServiceConnections.GetLMPMonitorService();
        #endregion

        #region Properties


        /// <summary>
        /// The m iso market list
        /// </summary>
        private List<MarketNode> mISOMarketList;
        /// <summary>
        /// Gets or sets the iso market list.
        /// </summary>
        /// <value>
        /// The iso market list.
        /// </value>
        public List<MarketNode> ISOMarketList
        {
            get
            {
                return mISOMarketList;
            }
            set
            {
                mISOMarketList = value;
                RaisePropertyChanged("ISOMarketList");
            }
        }

        /// <summary>
        /// The m market combo selected value
        /// </summary>
        private MarketNode mMarketComboSelectedValue;
        /// <summary>
        /// Gets or sets the market combo selected value.
        /// </summary>
        /// <value>
        /// The market combo selected value.
        /// </value>
        public MarketNode MarketComboSelectedValue
        {
            get
            {
                return mMarketComboSelectedValue;
            }
            set
            {
                if (mMarketComboSelectedValue != value)
                {
                    mFirstChanged = true;
                    mMarketComboSelectedValue = value;
                    if (mCacheNodePricePriceList != null)
                    {
                        mCacheNodePricePriceList.Clear();
                    }
                    if (HourlyPrices != null)
                    {
                        HourlyPrices.Clear();
                    }
                    if (mExanteList != null)
                    {
                        mExanteList.Clear();
                    }
                    SetNode();
                }
                RaisePropertyChanged("MarketComboSelectedValue");
            }
        }
        /// <summary>
        /// The m market node list
        /// </summary>
        private List<MarketNode> mMarketNodeList;
        /// <summary>
        /// Gets or sets the market node list.
        /// </summary>
        /// <value>
        /// The market node list.
        /// </value>
        public List<MarketNode> MarketNodeList
        {
            get
            {
                return mMarketNodeList;
            }
            set
            {
                mMarketNodeList = value;
                RaisePropertyChanged("MarketNodeList");
            }
        }
        /// <summary>
        /// The m market node combo selected value
        /// </summary>
        private MarketNode mMarketNodeComboSelectedValue;
        /// <summary>
        /// Gets or sets the market node combo selected value.
        /// </summary>
        /// <value>
        /// The market node combo selected value.
        /// </value>
        public MarketNode MarketNodeComboSelectedValue
        {
            get
            {
                return mMarketNodeComboSelectedValue;
            }
            set
            {
                if (mMarketNodeComboSelectedValue != value & value != null)
                {
                    mMarketNodeComboSelectedValue = value;
                    mFirstChanged = true;
                    if (!mIsFirst)
                    {
                        DateNodeChange();
                    }
                }
                RaisePropertyChanged("MarketNodeComboSelectedValue");
            }
        }
        /// <summary>
        /// The m hourly prices
        /// </summary>
        private List<HourlyNodePriceDetails> mHourlyPrices;
        /// <summary>
        /// Gets or sets the hourly prices.
        /// </summary>
        /// <value>
        /// The hourly prices.
        /// </value>
        private List<HourlyNodePriceDetails> HourlyPrices
        {
            get
            {
                return mHourlyPrices;
            }
            set
            {
                mHourlyPrices = value;
                lock (mLockObject)
                {
                    SetNodePriceHourlyPrices();
                }
            }
        }
        /// <summary>
        /// The m start date
        /// </summary>
        private DateTime mStartDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate
        {
            get
            {
                return mStartDate;
            }
            set
            {
                if (mStartDate != value)
                {
                    mStartDate = value;
                    if (!mIsFirst)
                    {
                        DateNodeChange();
                    }
                }
                RaisePropertyChanged("StartDate");
            }
        }
        /// <summary>
        /// The m compare date
        /// </summary>
        private DateTime mCompareDate;
        /// <summary>
        /// Gets or sets the compare date.
        /// </summary>
        /// <value>
        /// The compare date.
        /// </value>
        public DateTime CompareDate
        {
            get
            {
                return mCompareDate;
            }
            set
            {
                if (mCompareDate != value)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    mCompareDate = value;
                    DAComparePrice = GetDAPrices(mCompareDate);
                    RTComparePrice = GetRTPrices(mCompareDate);
                    SetNodePriceHourlyPrices();
                    Mouse.OverrideCursor = null;
                }
                RaisePropertyChanged("CompareDate");
            }
        }
        /// <summary>
        /// The m node price price list
        /// </summary>
        private List<HourlyLMP> mNodePricePriceList;
        /// <summary>
        /// Gets or sets the node price price list.
        /// </summary>
        /// <value>
        /// The node price price list.
        /// </value>
        public List<HourlyLMP> NodePricePriceList
        {
            get
            {
                return mNodePricePriceList;
            }
            set
            {
                mNodePricePriceList = value;
                RaisePropertyChanged("NodePricePriceList");
            }
        }
        /// <summary>
        /// The m selected hour type
        /// </summary>
        private HourType mSelectedHourType;
        /// <summary>
        /// Gets or sets the type of the selected hour.
        /// </summary>
        /// <value>
        /// The type of the selected hour.
        /// </value>
        public HourType SelectedHourType
        {
            get
            {
                return mSelectedHourType;
            }
            set
            {
                if (mSelectedHourType != value)
                {
                    mSelectedHourType = value;
                    FilterData(0);
                }
                RaisePropertyChanged("SelectedHourType");
            }
        }
        /// <summary>
        /// The m user strategy list
        /// </summary>
        private List<Strategy> mUserStrategyList;
        /// <summary>
        /// Gets or sets the user strategy list.
        /// </summary>
        /// <value>
        /// The user strategy list.
        /// </value>
        public List<Strategy> UserStrategyList
        {
            get
            {
                return mUserStrategyList;
            }
            set
            {
                mUserStrategyList = value;
                RaisePropertyChanged("UserStrategyList");
            }
        }
        /// <summary>
        /// The m selected strategy
        /// </summary>
        private Strategy mSelectedStrategy;
        /// <summary>
        /// Gets or sets the selected strategy.
        /// </summary>
        /// <value>
        /// The selected strategy.
        /// </value>
        public Strategy SelectedStrategy
        {
            get { return mSelectedStrategy; }
            set
            {
                if (mSelectedStrategy != value)
                {
                    mSelectedStrategy = value;
                    SetStrategy();
                    RaisePropertyChanged("SelectedStrategy");
                }
            }
        }
        /// <summary>
        /// The m strategy text
        /// </summary>
        private string mStrategyText;
        /// <summary>
        /// Gets or sets the strategy text.
        /// </summary>
        /// <value>
        /// The strategy text.
        /// </value>
        public string StrategyText
        {
            get
            {
                return mStrategyText;
            }
            set
            {
                mStrategyText = value;
                RaisePropertyChanged("StrategyText");
            }
        }
        /// <summary>
        /// The m da peak
        /// </summary>
        private string mDAPeak;
        /// <summary>
        /// Gets or sets the da peak.
        /// </summary>
        /// <value>
        /// The da peak.
        /// </value>
        public string DAPeak
        {
            get
            {
                return mDAPeak;
            }
            set
            {
                mDAPeak = value;
                RaisePropertyChanged("DAPeak");
            }
        }
        /// <summary>
        /// The m da off peak
        /// </summary>
        private string mDAOffPeak;
        /// <summary>
        /// Gets or sets the da off peak.
        /// </summary>
        /// <value>
        /// The da off peak.
        /// </value>
        public string DAOffPeak
        {
            get
            {
                return mDAOffPeak;
            }
            set
            {
                mDAOffPeak = value;
                RaisePropertyChanged("DAOffPeak");
            }
        }
        /// <summary>
        /// The m da total
        /// </summary>
        private string mDATotal;
        /// <summary>
        /// Gets or sets the da total.
        /// </summary>
        /// <value>
        /// The da total.
        /// </value>
        public string DATotal
        {
            get
            {
                return mDATotal;
            }
            set
            {
                mDATotal = value;
                RaisePropertyChanged("DATotal");
            }
        }
        /// <summary>
        /// The m rt peak
        /// </summary>
        private string mRTPeak;
        /// <summary>
        /// Gets or sets the rt peak.
        /// </summary>
        /// <value>
        /// The rt peak.
        /// </value>
        public string RTPeak
        {
            get
            {
                return mRTPeak;
            }
            set
            {
                mRTPeak = value;
                RaisePropertyChanged("RTPeak");
            }
        }
        /// <summary>
        /// The m rt off peak
        /// </summary>
        private string mRTOffPeak;
        /// <summary>
        /// Gets or sets the rt off peak.
        /// </summary>
        /// <value>
        /// The rt off peak.
        /// </value>
        public string RTOffPeak
        {
            get
            {
                return mRTOffPeak;
            }
            set
            {
                mRTOffPeak = value;
                RaisePropertyChanged("RTOffPeak");
            }
        }
        /// <summary>
        /// The m rt total
        /// </summary>
        private string mRTTotal;
        /// <summary>
        /// Gets or sets the rt total.
        /// </summary>
        /// <value>
        /// The rt total.
        /// </value>
        public string RTTotal
        {
            get
            {
                return mRTTotal;
            }
            set
            {
                mRTTotal = value;
                RaisePropertyChanged("RTTotal");
            }
        }
        /// <summary>
        /// The m peak difference
        /// </summary>
        private string mPeakDiff;
        /// <summary>
        /// Gets or sets the peak difference.
        /// </summary>
        /// <value>
        /// The peak difference.
        /// </value>
        public string PeakDiff
        {
            get
            {
                return mPeakDiff;
            }
            set
            {
                mPeakDiff = value;
                RaisePropertyChanged("PeakDiff");
            }
        }
        /// <summary>
        /// The m off peak difference
        /// </summary>
        private string mOffPeakDiff;
        /// <summary>
        /// Gets or sets the off peak difference.
        /// </summary>
        /// <value>
        /// The off peak difference.
        /// </value>
        public string OffPeakDiff
        {
            get
            {
                return mOffPeakDiff;
            }
            set
            {
                mOffPeakDiff = value;
                RaisePropertyChanged("OffPeakDiff");
            }
        }
        /// <summary>
        /// The m total difference
        /// </summary>
        private string mTotalDiff;
        /// <summary>
        /// Gets or sets the total difference.
        /// </summary>
        /// <value>
        /// The total difference.
        /// </value>
        public string TotalDiff
        {
            get
            {
                return mTotalDiff;
            }
            set
            {
                mTotalDiff = value;
                RaisePropertyChanged("TotalDiff");
            }
        }
        /// <summary>
        /// The m header0
        /// </summary>
        private string mHeader0 = "5";
        /// <summary>
        /// Gets or sets the header0.
        /// </summary>
        /// <value>
        /// The header0.
        /// </value>
        public string Header0
        {
            get
            {
                return mHeader0;
            }
            set
            {
                mHeader0 = value;
                RaisePropertyChanged("Header0");
            }
        }
        /// <summary>
        /// The m header5
        /// </summary>
        private string mHeader5 = "10";
        /// <summary>
        /// Gets or sets the header5.
        /// </summary>
        /// <value>
        /// The header5.
        /// </value>
        public string Header5
        {
            get
            {
                return mHeader5;
            }
            set
            {
                mHeader5 = value;
                RaisePropertyChanged("Header5");
            }
        }
        /// <summary>
        /// The m header10
        /// </summary>
        private string mHeader10 = "15";
        /// <summary>
        /// Gets or sets the header10.
        /// </summary>
        /// <value>
        /// The header10.
        /// </value>
        public string Header10
        {
            get
            {
                return mHeader10;
            }
            set
            {
                mHeader10 = value;
                RaisePropertyChanged("Header10");
            }
        }
        /// <summary>
        /// The m header15
        /// </summary>
        private string mHeader15 = "20";
        /// <summary>
        /// Gets or sets the header15.
        /// </summary>
        /// <value>
        /// The header15.
        /// </value>
        public string Header15
        {
            get
            {
                return mHeader15;
            }
            set
            {
                mHeader15 = value;
                RaisePropertyChanged("Header15");
            }
        }
        /// <summary>
        /// The m header20
        /// </summary>
        private string mHeader20 = "25";
        /// <summary>
        /// Gets or sets the header20.
        /// </summary>
        /// <value>
        /// The header20.
        /// </value>
        public string Header20
        {
            get
            {
                return mHeader20;
            }
            set
            {
                mHeader20 = value;
                RaisePropertyChanged("Header20");
            }
        }
        /// <summary>
        /// The m header25
        /// </summary>
        private string mHeader25 = "30";
        /// <summary>
        /// Gets or sets the header25.
        /// </summary>
        /// <value>
        /// The header25.
        /// </value>
        public string Header25
        {
            get { return mHeader25; }
            set
            {
                mHeader25 = value;
                RaisePropertyChanged("Header25");
            }
        }
        /// <summary>
        /// The m header30
        /// </summary>
        private string mHeader30 = "35";
        /// <summary>
        /// Gets or sets the header30.
        /// </summary>
        /// <value>
        /// The header30.
        /// </value>
        public string Header30
        {
            get
            {
                return mHeader30;
            }
            set
            {
                mHeader30 = value;
                RaisePropertyChanged("Header30");
            }
        }
        /// <summary>
        /// The m header35
        /// </summary>
        private string mHeader35 = "40";
        /// <summary>
        /// Gets or sets the header35.
        /// </summary>
        /// <value>
        /// The header35.
        /// </value>
        public string Header35
        {
            get
            {
                return mHeader35;
            }
            set
            {
                mHeader35 = value;
                RaisePropertyChanged("Header35");
            }
        }
        /// <summary>
        /// The m header40
        /// </summary>
        private string mHeader40 = "45";
        /// <summary>
        /// Gets or sets the header40.
        /// </summary>
        /// <value>
        /// The header40.
        /// </value>
        public string Header40
        {
            get
            {
                return mHeader40;
            }
            set
            {
                mHeader40 = value;
                RaisePropertyChanged("Header40");
            }
        }
        /// <summary>
        /// The m header45
        /// </summary>
        private string mHeader45 = "50";
        /// <summary>
        /// Gets or sets the header45.
        /// </summary>
        /// <value>
        /// The header45.
        /// </value>
        public string Header45
        {
            get
            {
                return mHeader45;
            }
            set
            {
                mHeader45 = value;
                RaisePropertyChanged("Header45");
            }
        }
        /// <summary>
        /// The m header50
        /// </summary>
        private string mHeader50 = "55";
        /// <summary>
        /// Gets or sets the header50.
        /// </summary>
        /// <value>
        /// The header50.
        /// </value>
        public string Header50
        {
            get
            {
                return mHeader50;
            }
            set
            {
                mHeader50 = value;
                RaisePropertyChanged("Header50");
            }
        }
        /// <summary>
        /// The m header55
        /// </summary>
        private string mHeader55 = "60";
        /// <summary>
        /// Gets or sets the header55.
        /// </summary>
        /// <value>
        /// The header55.
        /// </value>
        public string Header55
        {
            get
            {
                return mHeader55;
            }
            set
            {
                mHeader55 = value;
                RaisePropertyChanged("Header55");
            }
        }
        /// <summary>
        /// The m is DST
        /// </summary>
        private bool mIsDST = false;
        /// <summary>
        /// The m first changed
        /// </summary>
        private bool mFirstChanged;
        /// <summary>
        /// Gets or sets the save strategy command.
        /// </summary>
        /// <value>
        /// The save strategy command.
        /// </value>
        public DelegateCommand SaveStrategyCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete strategy command.
        /// </summary>
        /// <value>
        /// The delete strategy command.
        /// </value>
        public DelegateCommand DeleteStrategyCommand { private set; get; }
        /// <summary>
        /// Gets or sets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public DelegateCommand CloseCommand { private set; get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NodePriceMonitorViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            mDataService.loadDBCommands();
            FillMarket();
            MarketComboSelectedValue = ISOMarketList.Where(t => t.MarketKey == 9).FirstOrDefault();
            StartDate = DateTime.Today;
            CompareDate = DateTime.Today.AddDays(-1);
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            mIsDST = curTimeZone.IsDaylightSavingTime(DateTime.Today.AddDays(1).AddHours(7));
            SelectedHourType = HourType.All;
            //SetHeader();
            DAPrice = GetDAPrices(StartDate);
            DAComparePrice = GetDAPrices(CompareDate);
            RTComparePrice = GetRTPrices(CompareDate);
            ConnectLMPServer();
            InitProxyConfigServer();
            var firstTask = new Task(() => mSubscriberLMPMonitor.startConnectAndSubscribeAll());
            firstTask.Start();
            mIsFirst = false;
            SaveStrategyCommand = new DelegateCommand(SaveStrategy);
            DeleteStrategyCommand = new DelegateCommand(DeleteStrategy);
            CloseCommand = new DelegateCommand(() => UnSubscribeToLive());

            DateNodeChange();
            if (heartBeatTimer == null)
            {
                heartBeatTimer = new System.Timers.Timer(180000);
            }
            heartBeatTimer.Enabled = true;
            heartBeatTimer.Elapsed += heartBeatTimer_Elapsed;
            heartBeatTimer.Start();
        }

        #region Private Methods

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

        void heartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            heartBeatTimer.Enabled = false;
            try
            {

                ConnectLMPServer();
                mSubscriberLMPMonitor.startConnectAndSubscribeAll();

                bool status = proxy.HeartBeat();
                if (status == false)
                {
                    ConnectLMPServer();
                    mSubscriberLMPMonitor.startConnectAndSubscribeAll();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (mSubscriberLMPMonitor == null)
                    InitProxyConfigServer();
                mSubscriberLMPMonitor.ReconnectAndSubscribeAll(null, null);
            }
            heartBeatTimer.Enabled = true;
        }

        /// <summary>
        /// Connects the LMP server.
        /// </summary>
        public void ConnectLMPServer()
        {
            proxy = null;
            NetTcpBinding binding = new NetTcpBinding();
            binding.CloseTimeout = new TimeSpan(0, 5, 0);
            binding.OpenTimeout = new TimeSpan(0, 5, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
            binding.SendTimeout = new TimeSpan(0, 5, 0);
            binding.MaxConnections = 1000;
            binding.Security.Mode = SecurityMode.None;
            DuplexChannelFactory<INodePrice> factory = new DuplexChannelFactory<INodePrice>(this, binding, Vayu.CommonAccessLibrary.ServiceConnections.GetLMPMonitorService());
            try
            {
                proxy = factory.CreateChannel();
                DateTime csttime = SetToCST(StartDate);
                if (mStartDate == DateTime.Today)
                {
                    HourlyPrices = proxy.GetLmpPrint(MarketNodeComboSelectedValue.MarketKey, MarketNodeComboSelectedValue.NodeKey, csttime);
                }
                else
                {
                    HourlyPrices = proxy.GetLmpPrint(MarketNodeComboSelectedValue.MarketKey, MarketNodeComboSelectedValue.NodeKey, csttime);

                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                // proxy = null;
                factory = null;
            }
        }

        /// <summary>
        /// Reconfigures the proxy settings.
        /// </summary>
        private void reconfigureProxySettings()
        {
            DateTime csttime = SetToCST(StartDate);
            ((SubscriberLMPMonitor.ProxySetting)mSubscriberLMPMonitor.proxySettingHash[0]).mMarket = MarketNodeComboSelectedValue.MarketKey;
            ((SubscriberLMPMonitor.ProxySetting)mSubscriberLMPMonitor.proxySettingHash[0]).mHub = MarketNodeComboSelectedValue.NodeKey;
            ((SubscriberLMPMonitor.ProxySetting)mSubscriberLMPMonitor.proxySettingHash[0]).mNodePriceDate = csttime;
            ((SubscriberLMPMonitor.ProxySetting)mSubscriberLMPMonitor.proxySettingHash[0]).mTempNode = MarketNodeComboSelectedValue.TempPrice;
        }

        /// <summary>
        /// Initializes the proxy configuration server.
        /// </summary>
        private void InitProxyConfigServer()
        {
            mSubscriberLMPMonitor = new SubscriberLMPMonitor(this, null);
            string proxyEndPointUrl0 = Vayu.CommonAccessLibrary.ServiceConnections.GetLMPMonitorService();
            bool proxyEndPointEnabled0 = true;
            DateTime csttime = SetToCST(StartDate);
            SubscriberLMPMonitor.ProxySetting ps0 = new SubscriberLMPMonitor.ProxySetting(proxyEndPointUrl0, proxyEndPointEnabled0, false,
                MarketNodeComboSelectedValue.MarketKey, MarketNodeComboSelectedValue.NodeKey, csttime, MarketNodeComboSelectedValue.TempPrice);
            mSubscriberLMPMonitor.proxySettingHash.Add(0, ps0);
        }

        /// <summary>
        /// Dates the node change.
        /// </summary>
        private void DateNodeChange()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (!mIsFirst)
            {
                DAPrice = GetDAPrices(StartDate);
                DAComparePrice = GetDAPrices(CompareDate);
                RTComparePrice = GetRTPrices(CompareDate);
                mCacheNodePricePriceList = new List<HourlyLMP>();
                mExanteList = new List<HourlyNodePriceDetails>();
                DateTime csttime = SetToCST(StartDate);
                SetHeader();
                if (mStartDate != DateTime.Today)
                {
                    ConnectLMPServer();
                    var firstTask = new Task(() => mSubscriberLMPMonitor.UnsubscribeAll());
                    firstTask.Start();
                }
                else
                {
                    ConnectLMPServer();
                    reconfigureProxySettings();
                    var firstTask = new Task(() => mSubscriberLMPMonitor.UnsubscribeAll());
                    var secondTask = firstTask.ContinueWith((t) => mSubscriberLMPMonitor.startConnectAndSubscribeAll());
                    firstTask.Start();
                }
            }
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Sets the strategy.
        /// </summary>
        private void SetStrategy()
        {
            if (SelectedStrategy != null)
            {
                Strategy strategy = UserStrategyList.FirstOrDefault(t => t.StrategyName.Equals(SelectedStrategy.StrategyName));
                if (strategy == null)
                {
                    return;
                }
                mCacheNodePricePriceList = new List<HourlyLMP>();
                MarketComboSelectedValue = ISOMarketList.First(t => t.MarketKey == strategy.MarketKey);
                MarketNodeComboSelectedValue = MarketNodeList.First(t => t.NodeKey == strategy.NodeKey);
                StartDate = strategy.CurrentDate;
                CompareDate = strategy.CompareDate;
                switch (strategy.HourType)
                {
                    case 'A':
                        SelectedHourType = HourType.All;
                        break;
                    case 'O':
                        SelectedHourType = HourType.OffPeak;
                        break;
                    case 'P':
                        SelectedHourType = HourType.OnPeak;
                        break;
                }

            }
            else
            {
                mCacheNodePricePriceList = new List<HourlyLMP>();
                MarketComboSelectedValue = ISOMarketList.First(t => t.MarketKey == 9);
                MarketNodeComboSelectedValue = MarketNodeList.First();
                StartDate = DateTime.Today;
                CompareDate = DateTime.Today.AddDays(-1);
                SelectedHourType = HourType.All;
            }
        }

        /// <summary>
        /// Calculates the average.
        /// </summary>
        /// <param name="templist">The templist.</param>
        private void CalculateAverage(List<HourlyLMP> templist)
        {
            templist.ForEach(a =>
            {
                double total = 0;
                double avgCount = 0;
                foreach (PropertyInfo propItem in a.GetType().GetProperties().Where(h => h.Name.StartsWith("Min")))
                {
                    int z = -1;
                    if (int.TryParse(propItem.Name.Substring(3), out z))
                    {
                        double? val = null;
                        if (MarketComboSelectedValue.MarketKey == 9)
                        {
                            if (propItem.Name.Contains("10") || propItem.Name.Contains("25") || propItem.Name.Contains("40") || propItem.Name.Contains("55"))
                            {
                                val = propItem.GetValue(a) == null ? (double?)null : Convert.ToDouble(propItem.GetValue(a));
                            }
                        }
                        else
                        {
                            val = propItem.GetValue(a) == null ? (double?)null : Convert.ToDouble(propItem.GetValue(a));
                        }
                        if (val != null)
                        {
                            avgCount += 1;
                            total += (double)val;
                        }
                    }
                }
                if (avgCount != 0)
                {
                    if (total != 0)
                    {
                        a.RT = total / avgCount;
                    }
                }
            });
        }

        /// <summary>
        /// Fills the market.
        /// </summary>
        private void FillMarket()
        {
            List<MarketNode> marketList = new List<MarketNode>();
            mDataService.GetMarket((Markets, error) =>
            {
                if (error != null)
                {
                    return;
                }
                marketList = Markets;
            });
            ISOMarketList = marketList;
        }

        /// <summary>
        /// Sets the node.
        /// </summary>
        private void SetNode()
        {
            List<MarketNode> marketList = new List<MarketNode>();
            mDataService.GetMarketNode((Markets, error) =>
            {
                if (error != null)
                {
                    return;
                }
                marketList = Markets;
            }, MarketComboSelectedValue.MarketKey);
            MarketNodeList = marketList;

            MarketNodeComboSelectedValue = MarketNodeList[0];

        }

        /// <summary>
        /// Gets the da prices.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private Dictionary<int, double> GetDAPrices(DateTime date)
        {
            Dictionary<int, double> tempDaPrice = new Dictionary<int, double>();
            if (mCacheDAPrice.ContainsKey(date + "@" + MarketNodeComboSelectedValue.MarketKey + "@" + MarketNodeComboSelectedValue.NodeKey))
            {
                tempDaPrice = mCacheDAPrice[date + "@" + MarketNodeComboSelectedValue.MarketKey + "@" + MarketNodeComboSelectedValue.NodeKey];
            }
            else
            {
                mDataService.GetDAPrice((DAPriceHash, error) =>
                {
                    if (error != null)
                    {
                        return;
                    }
                    tempDaPrice = DAPriceHash;
                    mCacheDAPrice.Add(date.ToString() + "@" + MarketNodeComboSelectedValue.MarketKey + "@" + MarketNodeComboSelectedValue.NodeKey, DAPriceHash);
                }, date, MarketNodeComboSelectedValue.MarketKey, MarketNodeComboSelectedValue.NodeKey);
            }
            return tempDaPrice;
        }

        /// <summary>
        /// Gets the rt prices.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private Dictionary<int, double> GetRTPrices(DateTime date)
        {
            Dictionary<int, double> tempRTPrice = new Dictionary<int, double>();
            if (mCacheRTPrice.ContainsKey(date + "@" + MarketNodeComboSelectedValue.MarketKey + "@" + MarketNodeComboSelectedValue.NodeKey))
            {
                tempRTPrice = mCacheRTPrice[date + "@" + MarketNodeComboSelectedValue.MarketKey + "@" + MarketNodeComboSelectedValue.NodeKey];
            }
            else
            {
                mDataService.GetRTPrice((RTPriceHash, error) =>
                {
                    if (error != null)
                    {
                        return;
                    }
                    tempRTPrice = RTPriceHash;
                    mCacheRTPrice.Add(date + "@" + MarketNodeComboSelectedValue.MarketKey + "@" + MarketNodeComboSelectedValue.NodeKey, RTPriceHash);
                }, date, MarketNodeComboSelectedValue.MarketKey, MarketNodeComboSelectedValue.NodeKey);
            }
            return tempRTPrice;
        }

        /// <summary>
        /// Sets the node price hourly prices.
        /// </summary>
        private void SetNodePriceHourlyPrices()
        {
            if (HourlyPrices == null && mExanteList.Count == 0)
            {
                return;
            }
            List<HourlyLMP> pricesList = new List<HourlyLMP>();
            int lower = 8, upper = 24;
            if (MarketComboSelectedValue.MarketKey == 9)
            {
                lower = 7;
                upper = 23;
            }
            for (int i = 0; i < 24; i++)
            {
                HourlyLMP prices = new HourlyLMP();
                prices.Hour = i + 1;
                prices.HourShow = prices.Hour.ToString();
                prices.IsPeakHour = prices.Hour >= lower && prices.Hour < upper ? true : false;
                if (prices.IsPeakHour)
                {
                    prices.HourTypeIndex = prices.Hour - lower;
                }
                else
                {
                    if (!prices.IsPeakHour && prices.Hour <= lower)
                    {
                        prices.HourTypeIndex = i;
                    }
                    else if (upper == 23)
                    {
                        prices.HourTypeIndex = prices.Hour == 23 ? --lower : lower;
                    }
                    else
                    {
                        prices.HourTypeIndex = --lower;
                    }
                }
                int hourindex = -1;
                if (mExanteList != null)
                {
                    hourindex = mExanteList.FindIndex(t => t.Hour == prices.Hour);
                }
                int avgCount = 0;
                double? total = null;
                var imaginaryValues = from p in mCacheNodePricePriceList where p.Hour == prices.Hour select p.ImaginaryValue;
                List<int> imaginaryList = new List<int>();
                foreach (var imagineVals in imaginaryValues)
                {
                    if (imagineVals != null)
                    {
                        foreach (var imagine in imagineVals)
                        {
                            imaginaryList.Add(imagine);
                        }
                    }
                }
                for (int j = 0; j < 12; j++)
                {
                    string min = "Min" + (j * 5);
                    double? value = null;
                    double? exantedispatch = null;
                    double? exantedispatchvalue = null;

                    if (HourlyPrices != null && i < HourlyPrices.Count)
                    {
                        value = (double?)HourlyPrices[i].GetType().GetProperty(min).GetValue(HourlyPrices[i], null);
                        if (total == null)
                        {
                            avgCount = HourlyPrices[i].MinuteCount;
                            total = HourlyPrices[i].RTSum;
                        }
                    }
                    if (hourindex != -1)
                    {
                        exantedispatchvalue = (double?)mExanteList[hourindex].GetType().GetProperty(min).GetValue(mExanteList[hourindex], null);
                        if (value != null && exantedispatchvalue != null)
                        {
                            if (imaginaryList.Contains(j * 5))
                            {
                                value = null;
                                imaginaryList.Remove(j * 5);
                                HourlyPrices[i].GetType().GetProperty(min).SetValue(HourlyPrices[i], null, null);
                            }
                        }
                    }
                    if (prices.ExanteDispatch != null && prices.ExanteDispatch.Contains(j * 5))
                    {
                        exantedispatch = (double?)prices.GetType().GetProperty(min).GetValue(prices, null);
                    }
                    if (value != null)
                    {
                        prices.GetType().GetProperty(min).SetValue(prices, value, null);
                        if (exantedispatch != null)
                        {
                            prices.ExanteDispatch.Remove(j * 5);
                        }
                    }
                    else if (exantedispatchvalue != null)
                    {
                        prices.GetType().GetProperty(min).SetValue(prices, exantedispatchvalue, null);
                        if (prices.ExanteDispatch != null && !prices.ExanteDispatch.Contains(j * 5))
                        {
                            prices.ExanteDispatch.Add(j * 5);
                        }
                        else
                        {
                            prices.ExanteDispatch = new List<int>();
                            prices.ExanteDispatch.Add(j * 5);
                        }
                        total = total != null ? (total + exantedispatchvalue) : exantedispatchvalue;
                        avgCount++;
                    }
                    if (prices.RT != null)
                    {
                        if (total == null)
                        {
                            total = 0;
                            total = prices.RT;
                            avgCount = 1;
                        }
                    }
                }
                double? originalVal = prices.RT;
                double? rtval = total != null ? (total / (double)avgCount) : null;
                prices.RT = rtval == null ? prices.RT : rtval;
                prices.Combo = prices.RT != null ? prices.RT : prices.DA;
                if (DAPrice.ContainsKey(i + 1))
                {
                    prices.DA = DAPrice[i + 1];
                }
                if (DAComparePrice.ContainsKey(i + 1))
                {
                    prices.CompareDA = DAComparePrice[i + 1];
                }
                if (RTComparePrice.ContainsKey(i + 1))
                {
                    prices.CompareRT = RTComparePrice[i + 1];
                }
                double? nullvar = null;
                prices.ImaginaryValue = imaginaryList;
                prices.Diff = prices.RT != null ? prices.RT - prices.DA : nullvar;
                pricesList.Add(prices);
            }
            mCacheNodePricePriceList = GetOriginalValues(pricesList);
            SetHeaderValues();
            FilterData(0);
        }
        /// <summary>
        /// Gets the original values.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        /// <returns></returns>
        private List<HourlyNodePriceDetails> GetOriginalValues(List<HourlyNodePriceDetails> hourlyPrices)
        {
            if (HourlyPrices != null)
            {
                for (int i = 0; i < HourlyPrices.Count; i++)
                {
                    var imaginaryValues = from p in mCacheNodePricePriceList where p.Hour == HourlyPrices[i].Hour select p.ImaginaryValue;
                    List<int> imaginaryList = new List<int>();
                    foreach (var imagineVals in imaginaryValues)
                    {
                        if (imagineVals != null)
                        {
                            foreach (var imagine in imagineVals)
                            {
                                imaginaryList.Add(imagine);
                            }
                        }
                    }
                    if (hourlyPrices[i].Min0 == null && HourlyPrices[i].Min0 != null)
                    {
                        hourlyPrices[i].Min0 = HourlyPrices[i].Min0;
                    }
                    else if (hourlyPrices[i].Min0 != null)
                    {
                        imaginaryList.Remove(0);
                    }
                    if (hourlyPrices[i].Min5 == null && HourlyPrices[i].Min5 != null)
                    {
                        hourlyPrices[i].Min5 = HourlyPrices[i].Min5;
                    }
                    else if (hourlyPrices[i].Min5 != null)
                    {
                        imaginaryList.Remove(5);
                    }
                    if (hourlyPrices[i].Min10 == null && HourlyPrices[i].Min10 != null)
                    {
                        hourlyPrices[i].Min10 = HourlyPrices[i].Min10;
                    }
                    else if (hourlyPrices[i].Min10 != null)
                    {
                        imaginaryList.Remove(10);
                    }
                    if (hourlyPrices[i].Min15 == null && HourlyPrices[i].Min15 != null)
                    {
                        hourlyPrices[i].Min15 = HourlyPrices[i].Min15;
                    }
                    else if (hourlyPrices[i].Min15 != null)
                    {
                        imaginaryList.Remove(15);
                    }
                    if (hourlyPrices[i].Min20 == null && HourlyPrices[i].Min20 != null)
                    {
                        hourlyPrices[i].Min20 = HourlyPrices[i].Min20;
                    }
                    else if (hourlyPrices[i].Min20 != null)
                    {
                        imaginaryList.Remove(20);
                    }
                    if (hourlyPrices[i].Min25 == null && HourlyPrices[i].Min25 != null)
                    {
                        hourlyPrices[i].Min25 = HourlyPrices[i].Min25;
                    }
                    else if (hourlyPrices[i].Min25 != null)
                    {
                        imaginaryList.Remove(25);
                    }
                    if (hourlyPrices[i].Min30 == null && HourlyPrices[i].Min30 != null)
                    {
                        hourlyPrices[i].Min30 = HourlyPrices[i].Min30;
                    }
                    else if (hourlyPrices[i].Min30 != null)
                    {
                        imaginaryList.Remove(30);
                    }
                    if (hourlyPrices[i].Min35 == null && HourlyPrices[i].Min35 != null)
                    {
                        hourlyPrices[i].Min35 = HourlyPrices[i].Min35;
                    }
                    else if (hourlyPrices[i].Min35 != null)
                    {
                        imaginaryList.Remove(35);
                    }
                    if (hourlyPrices[i].Min40 == null && HourlyPrices[i].Min40 != null)
                    {
                        hourlyPrices[i].Min40 = HourlyPrices[i].Min40;
                    }
                    else if (hourlyPrices[i].Min40 != null)
                    {
                        imaginaryList.Remove(40);
                    }
                    if (hourlyPrices[i].Min45 == null && HourlyPrices[i].Min45 != null)
                    {
                        hourlyPrices[i].Min45 = HourlyPrices[i].Min45;
                    }
                    else if (hourlyPrices[i].Min45 != null)
                    {
                        imaginaryList.Remove(45);
                    }
                    if (hourlyPrices[i].Min50 == null && HourlyPrices[i].Min50 != null)
                    {
                        hourlyPrices[i].Min50 = HourlyPrices[i].Min50;
                    }
                    else if (hourlyPrices[i].Min50 != null)
                    {
                        imaginaryList.Remove(50);
                    }
                    if (hourlyPrices[i].Min55 == null && HourlyPrices[i].Min55 != null)
                    {
                        hourlyPrices[i].Min55 = HourlyPrices[i].Min55;
                    }
                    else if (hourlyPrices[i].Min55 != null)
                    {
                        imaginaryList.Remove(55);
                    }
                    if (i < mCacheNodePricePriceList.Count)
                    {
                        mCacheNodePricePriceList[i].ImaginaryValue = imaginaryList;
                    }
                }
            }
            return hourlyPrices;
        }
        /// <summary>
        /// Gets the original values.
        /// </summary>
        /// <param name="pricesList">The prices list.</param>
        /// <returns></returns>
        private List<HourlyLMP> GetOriginalValues(List<HourlyLMP> pricesList)
        {
            List<HourlyLMP> tempHourlyPrices = new List<HourlyLMP>();
            Dictionary<int, int> hourlyHash = new Dictionary<int, int>();
            Dictionary<int, int> cacheHourlyHash = new Dictionary<int, int>();

            for (int i = 0; i < pricesList.Count; i++)
            {
                if (!hourlyHash.ContainsKey(pricesList[i].Hour))
                {
                    hourlyHash.Add(pricesList[i].Hour, i);
                }
            }
            if (mCacheNodePricePriceList != null)
            {
                for (int i = 0; i < mCacheNodePricePriceList.Count; i++)
                {
                    if (!cacheHourlyHash.ContainsKey(mCacheNodePricePriceList[i].Hour))
                    {
                        cacheHourlyHash.Add(mCacheNodePricePriceList[i].Hour, i);
                    }
                }
            }
            for (int hour = 1; hour < 25; hour++)
            {
                HourlyLMP tempPriceDetail = new HourlyLMP();
                if (hourlyHash.ContainsKey(hour) && pricesList[hourlyHash[hour]].Min0 != null)
                {
                    tempPriceDetail = pricesList[hourlyHash[hour]];
                }
                else if (cacheHourlyHash.ContainsKey(hour))
                {
                    tempPriceDetail = mCacheNodePricePriceList[cacheHourlyHash[hour]];
                }
                else if (hourlyHash.ContainsKey(hour))
                {
                    tempPriceDetail = pricesList[hourlyHash[hour]];
                }
                tempHourlyPrices.Add(tempPriceDetail);
            }
            return tempHourlyPrices;
        }

        /// <summary>
        /// Sets the header.
        /// </summary>
        private void SetHeader()
        {

            Header0 = "5";
            Header5 = "10";
            Header10 = "15";
            Header15 = "20";
            Header20 = "25";
            Header25 = "30";
            Header30 = "35";
            Header35 = "40";
            Header40 = "45";
            Header45 = "50";
            Header50 = "55";
            Header55 = "60";

        }

        /// <summary>
        /// Saves the strategy.
        /// </summary>
        private void SaveStrategy()
        {
            Strategy StrategyExist = UserStrategyList.Where(t => t.StrategyName.Equals(StrategyText)).FirstOrDefault();
            Strategy newstrategy = new Strategy();
            newstrategy.StrategyName = StrategyText;
            newstrategy.UserName = sUser;
            newstrategy.MarketKey = MarketNodeComboSelectedValue.MarketKey;
            newstrategy.NodeKey = MarketNodeComboSelectedValue.NodeKey;
            newstrategy.CurrentDate = StartDate;
            newstrategy.CompareDate = CompareDate;
            switch (SelectedHourType)
            {
                case HourType.All:
                    newstrategy.HourType = 'A';
                    break;
                case HourType.OffPeak:
                    newstrategy.HourType = 'O';
                    break;
                case HourType.OnPeak:
                    newstrategy.HourType = 'P';
                    break;
            }

            if (StrategyExist == null)
            {
                mDataService.InsertStrategy(newstrategy);
            }
            else
            {
                DeleteStrategy();
                mDataService.InsertStrategy(newstrategy);
            }

            if (StrategyText != "" && StrategyText != null)
            {
                SelectedStrategy = UserStrategyList.Where(t => t.StrategyName.Equals(StrategyText)).First();
            }
        }

        /// <summary>
        /// Deletes the strategy.
        /// </summary>
        private void DeleteStrategy()
        {

            //Strategy StrategyExist = UserStrategyList.Where(t => t.StrategyName.Equals(SelectedStrategy.StrategyName)).FirstOrDefault();
            //mDataService.DeleteStrategy(sUser, StrategyExist.NodePriceStrategyId);
            //UserStrategyList.Remove(StrategyExist);
            //SelectedStrategy = null;
            //StrategyText = string.Empty;
            //SetStrategy();
        }



        #endregion

        #region Public Methods

        /// <summary>
        /// Uns the subscribe to live.
        /// </summary>
        public void UnSubscribeToLive()
        {
            try
            {
                mSubscriberLMPMonitor.UnsubscribeAndDisconnectAll();
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Filters the data.
        /// </summary>
        /// <param name="row">The row.</param>
        public void FilterData(int row)
        {
            HourlyLMP Avg = new HourlyLMP();
            Avg.Hour = 0;
            Avg.HourShow = "Avg";

            if (mCacheNodePricePriceList.Count <= 0)
            {
                return;
            }
            List<HourlyLMP> templist = new List<HourlyLMP>();
            if (mCacheNodePricePriceList.Count > 24)
            {
                mCacheNodePricePriceList.RemoveAt(24);
            }
            int lower = 7, upper = 23;
            if (MarketNodeComboSelectedValue.MarketKey == 2)
            {
                if (!mIsDST)
                {
                    lower = 8;
                    upper = 24;
                }
            }
            if (MarketNodeComboSelectedValue.MarketKey == 1)
            {
                lower = 8;
                upper = 24;
            }
            if (MarketComboSelectedValue.MarketKey == 9)
            {
                lower = 7;
                upper = 23;
            }
            if (SelectedHourType == HourType.All)
            {
                templist = mCacheNodePricePriceList;
                Avg.Diff = templist.Average(t => t.Diff);
                Avg.Combo = templist.Average(t => t.Combo);
                Avg.CompareRT = templist.Average(t => t.CompareRT);
                Avg.CompareDA = templist.Average(t => t.CompareDA);
                Avg.RT = templist.Average(t => t.RT);
                Avg.DA = templist.Average(t => t.DA);
            }
            if (SelectedHourType == HourType.OnPeak)
            {
                templist = mCacheNodePricePriceList.Where(t => t.Hour >= lower && t.Hour < upper).ToList();
                Avg.Diff = templist.Average(t => t.Diff);
                Avg.Combo = templist.Average(t => t.Combo);
                Avg.CompareRT = templist.Average(t => t.CompareRT);
                Avg.CompareDA = templist.Average(t => t.CompareDA);
                Avg.RT = templist.Average(t => t.RT);
                Avg.DA = templist.Average(t => t.DA);
            }
            if (SelectedHourType == HourType.OffPeak)
            {
                templist = mCacheNodePricePriceList.Where(t => t.Hour < lower || t.Hour >= upper).ToList();
                Avg.Diff = templist.Average(t => t.Diff);
                Avg.Combo = templist.Average(t => t.Combo);
                Avg.CompareRT = templist.Average(t => t.CompareRT);
                Avg.CompareDA = templist.Average(t => t.CompareDA);
                Avg.RT = templist.Average(t => t.RT);
                Avg.DA = templist.Average(t => t.DA);
            }
            templist.Add(Avg);
            CalculateAverage(templist);
            NodePricePriceList = null;
            NodePricePriceList = templist.ToList();
        }
        /// <summary>
        /// Sends the LMP print.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        public void SendLmpPrint(List<HourlyNodePriceDetails> hourlyPrices)
        {
            for (int i = hourlyPrices.Count; i < 24; i++)
            {
                HourlyNodePriceDetails temp = new HourlyNodePriceDetails();
                temp.Hour = i + 1;
                hourlyPrices.Add(temp);
            }
            HourlyPrices = GetOriginalValues(hourlyPrices);
            //mExanteList = hourlyPrices.ToList();
            RefreshNodePrice();
        }
        /// <summary>
        /// Sends the LMP dispatch.
        /// </summary>
        /// <param name="hourlyExanteDispatch">The hourly exante dispatch.</param>
        public void SendLmpDispatch(List<HourlyNodePriceDetails> hourlyExanteDispatch)
        {
            mExanteList = hourlyExanteDispatch;
            lock (mLockObject)
            {
                SetNodePriceHourlyPrices();
            }
        }
        /// <summary>
        /// Adds the imaginary value.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="value">The value.</param>
        /// <param name="isImaginary">if set to <c>true</c> [is imaginary].</param>
        /// <returns></returns>
        public bool AddImaginaryValue(int hour, int minute, string value, bool isImaginary)
        {
            if (value.Equals(""))
            {
                return false;
            }
            if (minute == 100)
            {
                mCacheNodePricePriceList[hour - 1].RT = double.Parse(value);
            }
            else
            {
                string min = "Min" + minute;
                double? minvalue = null;
                if (isImaginary)
                {
                    minvalue = (double?)mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).GetValue(mCacheNodePricePriceList[hour - 1], null);
                }
                if (minvalue == null)
                {
                    if (mCacheNodePricePriceList[hour - 1].ImaginaryValue != null || mCacheNodePricePriceList[hour - 1].ImaginaryValue.Count > 0)
                    {
                        if (!mCacheNodePricePriceList[hour - 1].ImaginaryValue.Contains(minute))
                        {
                            mCacheNodePricePriceList[hour - 1].ImaginaryValue.Add(minute);
                        }
                    }
                    else
                    {
                        mCacheNodePricePriceList[hour - 1].ImaginaryValue = new List<int>();
                        mCacheNodePricePriceList[hour - 1].ImaginaryValue.Add(minute);
                    }
                    mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).SetValue(mCacheNodePricePriceList[hour - 1], double.Parse(value), null);
                    if (HourlyPrices != null)
                    {
                        if (hour - 1 < HourlyPrices.Count)
                        {
                            HourlyPrices[hour - 1].GetType().GetProperty(min).SetValue(HourlyPrices[hour - 1], double.Parse(value), null);
                            mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).SetValue(mCacheNodePricePriceList[hour - 1], double.Parse(value), null);
                        }
                    }
                }
                else if (minvalue != null && mCacheNodePricePriceList[hour - 1].ImaginaryValue != null && mCacheNodePricePriceList[hour - 1].ImaginaryValue.Contains(minute))
                {
                    mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).SetValue(mCacheNodePricePriceList[hour - 1], double.Parse(value), null);
                }
                else
                {
                    return false;
                }
                double? total = null;
                int avgcount = 0;
                for (int i = 0; i < 12; i++)
                {
                    min = "Min" + (i * 5);
                    minvalue = (double?)mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).GetValue(mCacheNodePricePriceList[hour - 1], null);
                    if (minvalue != null)
                    {
                        if (total == null)
                        {
                            total = minvalue;
                        }
                        else
                        {
                            total += minvalue;
                        }
                        avgcount++;
                    }
                }
                if (total != null)
                {
                    mCacheNodePricePriceList[hour - 1].RT = total / (double)avgcount;
                }

            }
            SetHeaderValues();
            FilterData(1);
            return true;
        }
        /// <summary>
        /// Deletes the imaginary value.
        /// </summary>
        /// <param name="itemDelete">The item delete.</param>
        /// <returns></returns>
        public bool DeleteImaginaryValue(string itemDelete)
        {
            bool isDelete = false;
            if (itemDelete == null)
            {
                return false;
            }
            string[] hourminute = itemDelete.Split(':');
            double? minvalue = null;
            int hour = int.Parse(hourminute[0]);
            string min = string.Empty;
            if (hourminute[1].Equals("RT"))
            {
                min = "RT";
                minvalue = mCacheNodePricePriceList[hour - 1].RT;
            }
            else
            {
                min = "Min" + hourminute[1];
                minvalue = (double?)mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).GetValue(mCacheNodePricePriceList[hour - 1], null);
            }
            if (minvalue != null)
            {
                if (min.Equals("RT") && mCacheNodePricePriceList[hour - 1].RT == null)
                {

                    double? total = null;
                    int avgcount = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        min = "Min" + (i * 5);

                        minvalue = (double?)mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).GetValue(mCacheNodePricePriceList[hour - 1], null);
                        if (minvalue != null)
                        {
                            if (total == null)
                            {
                                total = minvalue;
                            }
                            else
                            {
                                total += minvalue;
                            }
                            avgcount++;
                        }
                    }
                    if (total == null)
                    {
                        mCacheNodePricePriceList[hour - 1].RT = null;
                    }
                    else
                    {
                        mCacheNodePricePriceList[hour - 1].RT = total;
                    }
                    isDelete = true;
                }
                else if (mCacheNodePricePriceList[hour - 1].ImaginaryValue != null)
                {
                    if (mCacheNodePricePriceList[hour - 1].ImaginaryValue.Contains(int.Parse(hourminute[1])))
                    {
                        mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).SetValue(mCacheNodePricePriceList[hour - 1], null, null);
                        mCacheNodePricePriceList[hour - 1].ImaginaryValue.Remove(int.Parse(hourminute[1]));
                        isDelete = true;
                        double? total = null;
                        int avgcount = 0;
                        for (int i = 0; i < 12; i++)
                        {
                            min = "Min" + (i * 5);
                            minvalue = (double?)mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).GetValue(mCacheNodePricePriceList[hour - 1], null);
                            if (minvalue != null)
                            {
                                if (total == null)
                                {
                                    total = minvalue;
                                }
                                else
                                {
                                    total += minvalue;
                                }
                                avgcount++;
                            }
                        }
                        if (total == null)
                        {
                            mCacheNodePricePriceList[hour - 1].RT = null;
                        }
                        else
                        {
                            mCacheNodePricePriceList[hour - 1].RT = total / (double)avgcount;
                        }
                    }
                }
            }
            return isDelete;
        }
        /// <summary>
        /// Refreshes the node price.
        /// </summary>
        public void RefreshNodePrice()
        {
            CalculateAverage(mCacheNodePricePriceList);
            SetHeaderValues();
            FilterData(0);
        }
        /// <summary>
        /// Deletes the imaginary value.
        /// </summary>
        /// <param name="itemdelete">The itemdelete.</param>
        /// <returns></returns>
        public bool DeleteImaginaryValue(string[] itemdelete)
        {
            bool isDelete = false;
            foreach (var item in itemdelete)
            {
                if (item == null)
                {
                    continue;
                }
                string[] hourminute = item.Split(':');
                double? minvalue = null;
                int hour = int.Parse(hourminute[0]);
                string min = string.Empty;
                if (hourminute[1].Equals("RT"))
                {
                    min = "RT";
                    minvalue = mCacheNodePricePriceList[hour - 1].RT;
                }
                else
                {
                    min = "Min" + hourminute[1];
                    minvalue = (double?)mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).GetValue(mCacheNodePricePriceList[hour - 1], null);
                }

                if (minvalue != null)
                {
                    if (min.Equals("RT") && mCacheNodePricePriceList[hour - 1].RT == null)
                    {
                        //mCacheNodePricePriceList[hour - 1].Manual = null;
                        isDelete = true;
                    }
                    else if (mCacheNodePricePriceList[hour - 1].ImaginaryValue != null)
                    {
                        if (mCacheNodePricePriceList[hour - 1].ImaginaryValue.Contains(int.Parse(hourminute[1])))
                        {
                            mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).SetValue(mCacheNodePricePriceList[hour - 1], null, null);
                            mCacheNodePricePriceList[hour - 1].ImaginaryValue.Remove(int.Parse(hourminute[1]));
                            isDelete = true;
                            double? total = null;
                            int avgcount = 0;
                            for (int i = 0; i < 12; i++)
                            {
                                min = "Min" + (i * 5);
                                minvalue = (double?)mCacheNodePricePriceList[hour - 1].GetType().GetProperty(min).GetValue(mCacheNodePricePriceList[hour - 1], null);
                                if (minvalue != null)
                                {
                                    if (total == null)
                                    {
                                        total = minvalue;
                                    }
                                    else
                                    {
                                        total += minvalue;
                                    }
                                    avgcount++;
                                }
                            }
                            if (total == null)
                            {
                                mCacheNodePricePriceList[hour - 1].RT = null;
                                //mCacheNodePricePriceList[hour - 1].Manual = null;
                            }
                            else
                            {
                                mCacheNodePricePriceList[hour - 1].RT = total / (double)avgcount;
                                //mCacheNodePricePriceList[hour - 1].Manual = total / (double)avgcount;
                            }
                        }
                    }
                }
            }
            if (isDelete)
            {
                SetHeaderValues();
                FilterData(0);
            }
            return isDelete;
        }

        /// <summary>
        /// Sets the header values.
        /// </summary>
        public void SetHeaderValues()
        {
            double? rttotal = (from p in mCacheNodePricePriceList
                               where p.RT != null && p.HourShow != "Avg"
                               select p.RT).Average();
            double? rtpeak = (from p in mCacheNodePricePriceList
                              where p.RT != null && p.IsPeakHour == true && p.HourShow != "Avg"
                              select p.RT).Average();
            double? rtoffpeak = (from p in mCacheNodePricePriceList
                                 where p.RT != null && p.IsPeakHour == false && p.HourShow != "Avg"
                                 select p.RT).Average();
            RTPeak = Math.Round((rtpeak == null ? 0 : (double)rtpeak), 2).ToString();
            RTOffPeak = Math.Round((rtoffpeak == null ? 0 : (double)rtoffpeak), 2).ToString();
            RTTotal = Math.Round((rttotal == null ? 0 : (double)rttotal), 2).ToString();
            DAPeak = Math.Round(DAPrice[200], 2).ToString();
            DAOffPeak = Math.Round(DAPrice[300], 2).ToString();
            DATotal = Math.Round(DAPrice[100], 2).ToString();
            OffPeakDiff = Math.Round(((rtoffpeak == null ? 0 : (double)rtoffpeak) - DAPrice[300]), 2).ToString();
            TotalDiff = Math.Round(((rttotal == null ? 0 : (double)rttotal) - DAPrice[100]), 2).ToString();
            PeakDiff = Math.Round(((rtpeak == null ? 0 : (double)rtpeak) - DAPrice[200]), 2).ToString();
        }
        /// <summary>
        /// Sets the connected status LMP server.
        /// </summary>
        /// <param name="proxyIndex">Index of the proxy.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        public void SetConnectedStatusLMPServer(int proxyIndex, bool status)
        {
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ComparisonConverter : IValueConverter
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
            return value.Equals(parameter);
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
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : System.Windows.Data.Binding.DoNothing;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum HourType
    {
        /// <summary>
        /// All
        /// </summary>
        All,
        /// <summary>
        /// The off peak
        /// </summary>
        OffPeak,
        /// <summary>
        /// The on peak
        /// </summary>
        OnPeak
    }
}
