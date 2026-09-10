using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;
using Vayu.LoadCurve.Model;
using Vayu.LoadGraphLibrary;

namespace Vayu.LoadCurve.ViewModels
{
    public class MainWindowViewModel : BindableBase, ILoadGraphCallback
    {
        #region Declaration

        /// <summary>
        /// The data service
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// The m load list
        /// </summary>
        private Dictionary<string, List<LoadDataItem>> mLoadList = new Dictionary<string, List<LoadDataItem>>();
        /// <summary>
        /// The m load proxy
        /// </summary>
        private ILoadGraph mLoadProxy;
        /// <summary>
        /// The pipe factory
        /// </summary>
        private DuplexChannelFactory<ILoadGraph> pipeFactory;
        /// <summary>
        /// The m load server URL
        /// </summary>
        private string mLoadServerUrl = ServiceConnections.GetLoadGraphService();
        /// <summary>
        /// The m WCF subscriber load graph server
        /// </summary>
        private WCFSubscriberLoadGraphServer mWCFSubscriberLoadGraphServer;
        /// <summary>
        /// The heart beat timer
        /// </summary>
        private System.Timers.Timer heartBeatTimer = null;
        /// <summary>
        /// The m temporary model
        /// </summary>
        private PlotModel _tempModel;

        private Legend _legendModel = new Legend();
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object lockObj = new object();
        /// <summary>
        /// The m oxy color list
        /// </summary>
        private Dictionary<string, OxyColor> mOxyColorList;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public DelegateCommand RefreshCommand { get; private set; }
        /// <summary>
        /// Gets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public DelegateCommand CloseCommand { get; private set; }

        /// <summary>
        /// The plot data model
        /// </summary>
        private OxyPlot.PlotModel plotDataModel;
        /// <summary>
        /// Gets or sets the plot data model.
        /// </summary>
        /// <value>
        /// The plot data model.
        /// </value>
        public OxyPlot.PlotModel PlotDataModel
        {
            get { return plotDataModel; }
            set { plotDataModel = value; RaisePropertyChanged("PlotDataModel"); }
        }

        /// <summary>
        /// The update time
        /// </summary>
        private string updateTime;
        /// <summary>
        /// Gets or sets the update time.
        /// </summary>
        /// <value>
        /// The update time.
        /// </value>
        public string UpdateTime
        {
            get { return updateTime; }
            set
            {
                updateTime = value;
                RaisePropertyChanged("UpdateTime");
            }
        }

        /// <summary>
        /// The zone list
        /// </summary>
        private List<string> zoneList;
        /// <summary>
        /// Gets or sets the zone list.
        /// </summary>
        /// <value>
        /// The zone list.
        /// </value>
        public List<string> ZoneList
        {
            get { return zoneList; }
            set
            {
                zoneList = value;
                RaisePropertyChanged("ZoneList");
            }
        }

        /// <summary>
        /// The zone selected item
        /// </summary>
        private string zoneSelectedItem;
        /// <summary>
        /// Gets or sets the zone selected item.
        /// </summary>
        /// <value>
        /// The zone selected item.
        /// </value>
        public string ZoneSelectedItem
        {
            get { return zoneSelectedItem; }
            set
            {
                zoneSelectedItem = value;
                RaisePropertyChanged("ZoneSelectedItem");
                if (mWCFSubscriberLoadGraphServer == null)
                    InitProxyConfigLMPServer();
                if (ZoneSelectedItem != null || ZoneSelectedItem != string.Empty)
                {
                    ConnectLoadGraphServer();
                    ReconfigureProxySettings();
                    GetLiveData();
                    GetFrozenLoadsData();
                    var firstTask = new Task(() => mWCFSubscriberLoadGraphServer.UnsubscribeAll());
                    var secondTask = firstTask.ContinueWith((t) => mWCFSubscriberLoadGraphServer.startConnectAndSubscribeAll());
                    firstTask.Start();
                }
            }
        }

        /// <summary>
        /// From selected date
        /// </summary>
        private DateTime fromSelectedDate;
        /// <summary>
        /// Gets or sets from selected date.
        /// </summary>
        /// <value>
        /// From selected date.
        /// </value>
        public DateTime FromSelectedDate
        {
            get { return fromSelectedDate; }
            set
            {
                fromSelectedDate = value;
                RaisePropertyChanged("FromSelectedDate");
            }
        }
        /// <summary>
        /// The end selected date
        /// </summary>
        private DateTime endSelectedDate;
        /// <summary>
        /// Gets or sets the end selected date.
        /// </summary>
        /// <value>
        /// The end selected date.
        /// </value>
        public DateTime EndSelectedDate
        {
            get { return endSelectedDate; }
            set
            {
                endSelectedDate = value;
                RaisePropertyChanged("EndSelectedDate");
                //if (value != null)
                //    endSelectedDate = endSelectedDate.AddHours(23).AddMinutes(55);
            }
        }

        /// <summary>
        /// The current checked
        /// </summary>
        private bool currentChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [current checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current checked]; otherwise, <c>false</c>.
        /// </value>
        public bool CURRENTChecked
        {
            get { return currentChecked; }
            set
            {
                currentChecked = value;
                RaisePropertyChanged("CURRENTChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The iso checked
        /// </summary>
        private bool isoChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [iso checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [iso checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ISOChecked
        {
            get { return isoChecked; }
            set
            {
                isoChecked = value;
                RaisePropertyChanged("ISOChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The PRT checked
        /// </summary>
        private bool prtChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [PRT checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [PRT checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PRTChecked
        {
            get { return prtChecked; }
            set
            {
                prtChecked = value;
                RaisePropertyChanged("PRTChecked");
                RefreshPlot();
            }
        }
        /// <summary>
        /// The wsi checked
        /// </summary>
        private bool wsiChecked;

        /// <summary>
        /// Gets or sets a value indicating whether [wsi checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wsi checked]; otherwise, <c>false</c>.
        /// </value>
        public bool WSIChecked
        {
            get { return wsiChecked; }
            set
            {
                wsiChecked = value;
                RaisePropertyChanged("WSIChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The DTN checked
        /// </summary>
        private bool dtnChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [DTN checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [DTN checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DTNChecked
        {
            get { return dtnChecked; }
            set
            {
                dtnChecked = value;
                RaisePropertyChanged("DTNChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The da checked
        /// </summary>
        private bool daChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [da checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [da checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DAChecked
        {
            get { return daChecked; }
            set
            {
                daChecked = value;
                RaisePropertyChanged("DAChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The fda checked
        /// </summary>
        private bool fdaChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [fda checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fda checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FDAChecked
        {
            get { return fdaChecked; }
            set
            {
                fdaChecked = value;
                RaisePropertyChanged("FDAChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The hiso checked
        /// </summary>
        private bool hisoChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [hiso checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hiso checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HISOChecked
        {
            get { return hisoChecked; }
            set
            {
                hisoChecked = value;
                RaisePropertyChanged("HISOChecked");
                RefreshPlot();
            }
        }
        /// <summary>
        /// The fiso checked
        /// </summary>
        private bool fisoChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [fiso checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fiso checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FISOChecked
        {
            get { return fisoChecked; }
            set
            {
                fisoChecked = value;
                RaisePropertyChanged("FISOChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The FPRT checked
        /// </summary>
        private bool fprtChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [FPRT checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [FPRT checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FPRTChecked
        {
            get { return fprtChecked; }
            set
            {
                fprtChecked = value;
                RaisePropertyChanged("FPRTChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The fwsi checked
        /// </summary>
        private bool fwsiChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [fwsi checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fwsi checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FWSIChecked
        {
            get { return fwsiChecked; }
            set
            {
                fwsiChecked = value;
                RaisePropertyChanged("FWSIChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The FDTN checked
        /// </summary>
        private bool fdtnChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [FDTN checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [FDTN checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FDTNChecked
        {
            get { return fdtnChecked; }
            set
            {
                fdtnChecked = value;
                RaisePropertyChanged("FDTNChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The hda checked
        /// </summary>
        private bool hdaChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [hda checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hda checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HDAChecked
        {
            get { return hdaChecked; }
            set
            {
                hdaChecked = value;
                RaisePropertyChanged("HDAChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The ciso checked
        /// </summary>
        private bool cisoChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [ciso checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [ciso checked]; otherwise, <c>false</c>.
        /// </value>
        public bool CISOChecked
        {
            get { return cisoChecked; }
            set
            {
                cisoChecked = value;
                RaisePropertyChanged("CISOChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The five frozen checked
        /// </summary>
        private bool fiveFrozenChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [five frozen checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [five frozen checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FiveFrozenChecked
        {
            get { return fiveFrozenChecked; }
            set
            {
                fiveFrozenChecked = value;
                RaisePropertyChanged("FiveFrozenChecked");
                GetFrozenLoadsData();
                Refresh();
            }
        }
        /// <summary>
        /// The eleven frozen checked
        /// </summary>
        private bool elevenFrozenChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [eleven frozen checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [eleven frozen checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ElevenFrozenChecked
        {
            get { return elevenFrozenChecked; }
            set
            {
                elevenFrozenChecked = value;
                RaisePropertyChanged("ElevenFrozenChecked");
                GetFrozenLoadsData();
                Refresh();
            }
        }

        /// <summary>
        /// The p rt frozen checked
        /// </summary>
        private bool pRTFrozenChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [PRT frozen checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [PRT frozen checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PRTFrozenChecked
        {
            get { return pRTFrozenChecked; }
            set
            {
                pRTFrozenChecked = value;
                RaisePropertyChanged("PRTFrozenChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The w si frozen checked
        /// </summary>
        private bool wSIFrozenChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [wsi frozen checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wsi frozen checked]; otherwise, <c>false</c>.
        /// </value>
        public bool WSIFrozenChecked
        {
            get { return wSIFrozenChecked; }
            set
            {
                wSIFrozenChecked = value;
                RaisePropertyChanged("WSIFrozenChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The d tn frozen checked
        /// </summary>
        private bool dTNFrozenChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [DTN frozen checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [DTN frozen checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DTNFrozenChecked
        {
            get { return dTNFrozenChecked; }
            set
            {
                dTNFrozenChecked = value;
                RaisePropertyChanged("DTNFrozenChecked");
                RefreshPlot();
            }
        }
        /// <summary>
        /// The i so forecast frozen checked
        /// </summary>
        private bool iSOForecastFrozenChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [iso forecast frozen checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [iso forecast frozen checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ISOForecastFrozenChecked
        {
            get { return iSOForecastFrozenChecked; }
            set
            {
                iSOForecastFrozenChecked = value;
                RaisePropertyChanged("ISOForecastFrozenChecked");
                RefreshPlot();
            }
        }

        #endregion
        public MainWindowViewModel(IDataService dataService)
        {
            FromSelectedDate = DateTime.Today.AddDays(-1);
            EndSelectedDate = DateTime.Today.AddDays(1);
            FillColorList();
            if (_dataService == null)
                _dataService = new Model.DataService();
            else
                _dataService = dataService;
            FillZoneList();
            CheckAllCheckBoxes();
            RefreshCommand = new DelegateCommand(() => Refresh());
            CloseCommand = new DelegateCommand(() => CloseAndUnsubscribe());
            Task.Factory.StartNew(() => FillColorList());

            InitProxyConfigLMPServer();
            ConnectLoadGraphServer();
            if (heartBeatTimer == null)
                heartBeatTimer = new System.Timers.Timer(180000);
            heartBeatTimer.Enabled = true;
            heartBeatTimer.Elapsed += heartBeatTimer_Elapsed;
        }
        #region Private Methods

        /// <summary>
        /// Loads the load data.
        /// </summary>
        private void LoadLoadData()
        {
            FillZoneList();
        }
        /// <summary>
        /// Initializes the proxy configuration LMP server.
        /// </summary>
        private void InitProxyConfigLMPServer()
        {
            if (mWCFSubscriberLoadGraphServer == null)
            {
                mWCFSubscriberLoadGraphServer = new WCFSubscriberLoadGraphServer(this, null);
                bool proxyEndPointEnabled0 = true;
                WCFSubscriberLoadGraphServer.ProxySetting ps0 = new WCFSubscriberLoadGraphServer.ProxySetting(mLoadServerUrl, proxyEndPointEnabled0, false, ZoneSelectedItem,
                    FromSelectedDate, EndSelectedDate);
                mWCFSubscriberLoadGraphServer.proxySettingHash.Add(0, ps0);
            }
        }
        /// <summary>
        /// Handles the Elapsed event of the heartBeatTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        void heartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            heartBeatTimer.Enabled = false;
            try
            {
                if (mLoadProxy == null)
                    ConnectLoadGraphServer();
                mLoadProxy.HeartBeat();
            }
            catch
            {
                if (mWCFSubscriberLoadGraphServer == null)
                    InitProxyConfigLMPServer();
                mWCFSubscriberLoadGraphServer.ReconnectAndSubscribeAll(null, null);
            }
            heartBeatTimer.Enabled = true;
        }
        /// <summary>
        /// Fills the color list.
        /// </summary>
        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("CURRENT", OxyColors.Yellow);
            mOxyColorList.Add("HISO", OxyColors.Yellow);
            mOxyColorList.Add("PRT", OxyColors.HotPink);
            mOxyColorList.Add("DTN", OxyColors.Orange);
            mOxyColorList.Add("WSI", OxyColors.White);
            //mOxyColorList.Add("TESLA", OxyColors.Blue);
            mOxyColorList.Add("ISO", OxyColors.Lime);
            mOxyColorList.Add("DA", OxyColors.Red);
            mOxyColorList.Add("FISO", OxyColors.Lime);
            mOxyColorList.Add("FPRT", OxyColors.HotPink);
            //mOxyColorList.Add("FTESLA", OxyColors.Blue);
            mOxyColorList.Add("FWSI", OxyColors.White);
            mOxyColorList.Add("FDTN", OxyColors.Orange);
            mOxyColorList.Add("HDA", OxyColors.Red);
            mOxyColorList.Add("FDA", OxyColor.FromRgb(255, 192, 192));
            mOxyColorList.Add("CISO", OxyColors.Lime);
            mOxyColorList.Add("FROZENISO", OxyColors.Lime);
            mOxyColorList.Add("FROZENPRT", OxyColors.HotPink);
            mOxyColorList.Add("FROZENDTN", OxyColors.Orange);
            mOxyColorList.Add("FROZENWSI", OxyColors.White);
            // mOxyColorList.Add("FROZENTESLA", OxyColors.Blue);
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void Refresh()
        {
            if (mLoadList != null)
                mLoadList.Clear();
            if (FromSelectedDate >= EndSelectedDate)
            {
                System.Windows.MessageBox.Show("Please select thro'date to be greater than start date");
                return;
            }
            if ((EndSelectedDate - FromSelectedDate).Days > 30)
            {
                FromSelectedDate = DateTime.Today.AddDays(-30);
                System.Windows.MessageBox.Show("Difference between selected dates is much higher.\n Please select date range upto 30");
                return;
            }
            GetLiveData();
            GetFrozenLoadsData();
            RefreshPlot();
        }

        /// <summary>
        /// Gets the live data.
        /// </summary>
        private void GetLiveData()
        {
            try
            {
                if (mLoadProxy == null)
                {
                    ConnectLoadGraphServer();
                }
                if (mWCFSubscriberLoadGraphServer == null)
                {
                    InitProxyConfigLMPServer();
                }
                if (DateTime.Today <= EndSelectedDate && DateTime.Today >= FromSelectedDate)
                {
                    HashValues hashValues = mLoadProxy.GetLoadValues(ZoneSelectedItem, FromSelectedDate, EndSelectedDate);
                    SetGraph(hashValues.ZoneName.ToLower(), FromSelectedDate, EndSelectedDate,
                                            hashValues.HourZeroHash, hashValues.Hour1Hash, hashValues.Hour2Hash, hashValues.Hour3Hash,
                                            hashValues.Hour4Hash, hashValues.HourFiveHash, hashValues.Hour6Hash, hashValues.Hour7Hash, hashValues.Hour8Hash, hashValues.Hour9Hash,
                                            hashValues.Hour10Hash, hashValues.Hour11Hash, hashValues.Hour12Hash, hashValues.Hour13Hash, hashValues.Hour14Hash, hashValues.Hour15Hash);
                    ReconfigureProxySettings();
                    var firstTask = new Task(() => mWCFSubscriberLoadGraphServer.UnsubscribeAll());
                    var secondTask = firstTask.ContinueWith((t) => mWCFSubscriberLoadGraphServer.startConnectAndSubscribeAll());
                    firstTask.Start();
                }
                else
                {
                    HashValues hashValues = mLoadProxy.GetLoadValues(ZoneSelectedItem, FromSelectedDate, EndSelectedDate);
                    SetGraph(hashValues.ZoneName.ToLower(), FromSelectedDate, EndSelectedDate,
                                            hashValues.HourZeroHash, hashValues.Hour1Hash, hashValues.Hour2Hash, hashValues.Hour3Hash,
                                            hashValues.Hour4Hash, hashValues.HourFiveHash, hashValues.Hour6Hash, hashValues.Hour7Hash, hashValues.Hour8Hash, hashValues.Hour9Hash,
                                            hashValues.Hour10Hash, hashValues.Hour11Hash, hashValues.Hour12Hash, hashValues.Hour13Hash, hashValues.Hour14Hash, hashValues.Hour15Hash);
                }
            }
            catch (CommunicationObjectFaultedException)
            {
                mLoadProxy = null;
                mWCFSubscriberLoadGraphServer.ReconnectAndSubscribeAll(null, null);
            }

        }

        /// <summary>
        /// Gets the frozen loads data.
        /// </summary>
        private void GetFrozenLoadsData()
        {
            try
            {
                _dataService.GetFrozenLoadsData((item, ex) =>
                {
                    foreach (var item1 in item)
                    {
                        if (mLoadList.ContainsKey(item1.Key))
                        {
                            mLoadList.Remove(item1.Key);
                        }
                        mLoadList.Add(item1.Key, item1.Value.OrderBy(a => a.MarketDateTime).ToList());
                    }

                }, FromSelectedDate, EndSelectedDate, ZoneSelectedItem, FiveFrozenChecked ? 16 : 10);
            }
            catch
            {
            }

            RefreshPlot();
        }

        /// <summary>
        /// Reconfigures the proxy settings.
        /// </summary>
        private void ReconfigureProxySettings()
        {
            ((WCFSubscriberLoadGraphServer.ProxySetting)mWCFSubscriberLoadGraphServer.proxySettingHash[0]).mZone = ZoneSelectedItem;
            ((WCFSubscriberLoadGraphServer.ProxySetting)mWCFSubscriberLoadGraphServer.proxySettingHash[0]).mStartDate = FromSelectedDate;
            ((WCFSubscriberLoadGraphServer.ProxySetting)mWCFSubscriberLoadGraphServer.proxySettingHash[0]).mEndDate = EndSelectedDate;


        }

        /// <summary>
        /// Connects the load graph server.
        /// </summary>
        private void ConnectLoadGraphServer()
        {
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.OpenTimeout = new TimeSpan(0, 5, 0);
            myBinding.SendTimeout = new TimeSpan(0, 5, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 5, 0);
            myBinding.Security.Mode = SecurityMode.None;
            myBinding.MaxConnections = 1000;
            //myBinding.MaxReceivedMessageSize = int.MaxValue;
            // myBinding.MaxBufferPoolSize = int.MaxValue;
            try
            {
                pipeFactory = new DuplexChannelFactory<ILoadGraph>(new InstanceContext(this), myBinding, mLoadServerUrl);
                mLoadProxy = pipeFactory.CreateChannel();
            }
            catch (EndpointNotFoundException)
            {
            }
            catch
            {

            }
        }

        /// <summary>
        /// Fills the zone list.
        /// </summary>
        private void FillZoneList()
        {
            _dataService.GetLoadNames((item, ex) =>
            {
                if (ex == null)
                {
                    ZoneList = item.OrderBy(a => a).ToList();
                    // zoneList = item.ToList();
                    ZoneSelectedItem = item.Find(a => a.ToLower().Contains("ercot total"));
                }
            });
        }
        /// <summary>
        /// Checks all check boxes.
        /// </summary>
        private void CheckAllCheckBoxes()
        {
            foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(a => a.Name.Contains("Checked") && a.PropertyType == typeof(bool)))
                item.SetValue(this, true);

        }

        /// <summary>
        /// Prepares the load series.
        /// </summary>
        /// <param name="hourHash">The hour hash.</param>
        /// <param name="keySwitch">The key switch.</param>
        private void PrepareLoadSeries(Dictionary<int, Dictionary<int, double>> hourHash, string keySwitch)
        {
            List<LoadDataItem> grpItemsCurrent = GetHelperLoadList(hourHash, keySwitch);
            if (mLoadList.ContainsKey(keySwitch))
            {
                mLoadList.Remove(keySwitch);
            }
            mLoadList.Add(keySwitch.ToUpper(), grpItemsCurrent.OrderBy(k => k.MarketDateTime).ToList());
        }
        /// <summary>
        /// Prepares the load series.
        /// </summary>
        /// <param name="hour3Hash">The hour3 hash.</param>
        /// <param name="key">The key.</param>
        private void PrepareLoadSeries(Dictionary<int, double> hour3Hash, string key)
        {
            List<LoadDataItem> grpItemsCurrent = GetHelperLoadList(hour3Hash);
            if (mLoadList.ContainsKey(key))
            {
                mLoadList.Remove(key);
            }
            mLoadList.Add(key, grpItemsCurrent.OrderBy(a => a.MarketDateTime).ToList());
        }
        /// <summary>
        /// Gets the helper load list.
        /// </summary>
        /// <param name="hourHash">The hour hash.</param>
        /// <param name="keySwitch">The key switch.</param>
        /// <returns></returns>
        private List<LoadDataItem> GetHelperLoadList(Dictionary<int, Dictionary<int, double>> hourHash, string keySwitch)
        {
            List<LoadDataItem> grpItems = new List<LoadDataItem>();
            foreach (var hourItem in hourHash.OrderBy(a => a.Key))
            {
                foreach (var minuteItem in hourHash[hourItem.Key].OrderBy(a => a.Key))
                {
                    try
                    {
                        grpItems.Add(new LoadDataItem
                        {
                            MarketDateTime = DateTime.Today.AddHours(hourItem.Key).AddMinutes(minuteItem.Key),
                            LoadForecast = minuteItem.Value
                        });
                    }
                    catch
                    {
                    }
                }

            }
            return grpItems.OrderBy(a => a.MarketDateTime).ToList();
        }
        /// <summary>
        /// Gets the helper load list.
        /// </summary>
        /// <param name="hour3Hash">The hour3 hash.</param>
        /// <returns></returns>
        private List<LoadDataItem> GetHelperLoadList(Dictionary<int, double> hour3Hash)
        {
            List<LoadDataItem> tempLst = new List<LoadDataItem>();
            hour3Hash.Keys.ToList().ForEach(a =>
            {
                if (a >= 0)
                {
                    tempLst.Add(new LoadDataItem
                    {
                        LoadForecast = hour3Hash[a],
                        MarketDateTime = DateTime.Today.AddHours(a)
                    });
                }
            });
            return tempLst.OrderBy(a => a.MarketDateTime).ToList();
        }
        /// <summary>
        /// Closes the and unsubscribe.
        /// </summary>
        private void CloseAndUnsubscribe()
        {
            try
            {
                if (mWCFSubscriberLoadGraphServer != null)
                    mWCFSubscriberLoadGraphServer.UnsubscribeAndDisconnectAll();
                if (heartBeatTimer != null)
                {
                    heartBeatTimer.Enabled = false;
                    heartBeatTimer.Close();
                    heartBeatTimer = null;
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// Refreshes the plot.
        /// </summary>
        private void RefreshPlot()
        {
            if (mLoadList != null && mLoadList.Count > 0)
            {
                lock (lockObj)
                {
                    try
                    {
                        PlotDataModel = new PlotModel();
                        DateTime from = FromSelectedDate;
                        DateTime to = EndSelectedDate;
                        _tempModel = new PlotModel();
                        if (mLoadList == null || mLoadList.Count == 0)
                            return;
                        _tempModel.Axes.Add(new LinearAxis
                        {
                            Key = "YAxis",
                            MajorGridlineStyle = LineStyle.Solid,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                            MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                            IsPanEnabled = false,
                            IsZoomEnabled = true,
                            MaximumPadding = 0.5,
                            MinimumPadding = 0.1,
                            TextColor = OxyColors.White,
                            TitleColor = OxyColors.WhiteSmoke,
                            EndPosition = 1,
                            Position = AxisPosition.Left,
                            Title = "MW ----->",
                            AxisTitleDistance = 0
                        });
                        _tempModel.Axes.Add(new DateTimeAxis
                        {
                            Title = "Market Date ---->",
                            Position = AxisPosition.Bottom,
                            TextColor = OxyColors.White,
                            TitleColor = OxyColors.WhiteSmoke,
                            AxisTitleDistance = 0,
                            StringFormat = "HH:mm",
                            MajorGridlineStyle = LineStyle.Solid,
                            AxislineThickness = 3,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                            MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                        });
                        try
                        {
                            if (mOxyColorList == null)
                                FillColorList();
                            foreach (var item in mLoadList.Keys)
                            {
                                if (KeyChecked(item))
                                    _tempModel.Series.Add(CreateSeries(mLoadList[item].OrderBy(j => j.MarketDateTime).ToList(), item));
                            }
                            var l = new Legend
                            {
                                LegendOrientation = LegendOrientation.Horizontal,
                                LegendPlacement = LegendPlacement.Inside,
                                LegendPosition = LegendPosition.BottomLeft,
                                LegendTextColor = OxyColors.White,
                                LegendPadding = 3,
                            };
                            _tempModel.Legends.Add(l);

                            PlotDataModel = _tempModel;
                            //PlotDataModel.Updated();

                        }
                        catch
                        {
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        /// <summary>
        /// Keys the checked.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private bool KeyChecked(string item)
        {
            switch (item.ToLower())
            {
                case "prt": return PRTChecked;
                case "wsi": return WSIChecked;
                case "dtn": return DTNChecked;
                case "iso": return ISOChecked;
                // case "tesla": return TeslaChecked;
                case "da": return DAChecked;
                case "current": return CURRENTChecked;
                case "hiso": return HISOChecked;
                case "fiso": return FISOChecked;
                case "fprt": return FPRTChecked;
                //case "ftesla": return FTeslaChecked;
                case "fwsi": return FWSIChecked;
                case "fdtn": return FDTNChecked;
                case "hda": return HDAChecked;
                case "fda": return FDAChecked;
                case "ciso": return CISOChecked;
                case "frozeniso": return ISOForecastFrozenChecked;
                // case "frozentesla": return TESLAFrozenChecked;
                case "frozenprt": return PRTFrozenChecked;
                case "frozenwsi": return WSIFrozenChecked;
                case "frozendtn": return DTNFrozenChecked;
                default: return true;

            }

        }
        /// <summary>
        /// Creates the series.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private LineSeries CreateSeries(List<LoadDataItem> list, string key)
        {
            list.RemoveAll(a => a.LoadForecast == 0);
            List<GraphItem> grpList = new List<GraphItem>();
            try
            {
                list.ForEach(dItem =>
                {
                    grpList.Add(new GraphItem
                    {
                        X = dItem.MarketDateTime,
                        Y = dItem.LoadForecast
                    });

                });
            }
            catch
            {
            }
            return new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:HH:mm}\n{Y:0,###}" + " MW",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.5,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = key.ToUpper()
            };
        }
        /// <summary>
        /// Gets the line style.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private LineStyle GetLineStyle(string key)
        {
            if (key.StartsWith("HISO") || key.Contains("FTESLA") || key.Contains("FDTN") || key.Contains("FISO") || key.Contains("FPRT") || key.Contains("FWSI") || key.Contains("HDA") || key.Contains("FDA") || key.Contains("CISO") || key.ToUpper().StartsWith("FROZEN"))
            {
                return LineStyle.Dot;
            }
            else
            {
                return LineStyle.Solid;
            }
        }

        #endregion
        /// <summary>
        /// Sets the connected status load graph server.
        /// </summary>
        /// <param name="proxyIndex">Index of the proxy.</param>
        /// <param name="bRemoteConnected">if set to <c>true</c> [b remote connected].</param>
        internal void SetConnectedStatusLoadGraphServer(int proxyIndex, bool bRemoteConnected)
        {

        }

        /// <summary>
        /// Sets the graph.
        /// </summary>
        /// <param name="Zone">The zone.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <param name="hourHash">The hour hash.</param>
        /// <param name="hour1Hash">The hour1 hash.</param>
        /// <param name="hour2Hash">The hour2 hash.</param>
        /// <param name="hour3Hash">The hour3 hash.</param>
        /// <param name="hour4Hash">The hour4 hash.</param>
        /// <param name="hour5Hash">The hour5 hash.</param>
        /// <param name="hour6Hash">The hour6 hash.</param>
        /// <param name="hour7Hash">The hour7 hash.</param>
        /// <param name="hour8Hash">The hour8 hash.</param>
        /// <param name="hour9Hash">The hour9 hash.</param>
        /// <param name="hour10Hash">The hour10 hash.</param>
        /// <param name="hour11Hash">The hour11 hash.</param>
        /// <param name="hour12Hash">The hour12 hash.</param>
        /// <param name="hour13Hash">The hour13 hash.</param>
        /// <param name="hour14Hash">The hour14 hash.</param>
        /// <param name="hour15Hash">The hour15 hash.</param>
        public void SetGraph(string Zone, DateTime StartDate, DateTime EndDate, Dictionary<int, Dictionary<int, double>> hourHash, Dictionary<int, double> hour1Hash, Dictionary<int, double> hour2Hash, Dictionary<int, double> hour3Hash, Dictionary<int, double> hour4Hash, Dictionary<int, Dictionary<int, double>> hour5Hash, Dictionary<int, double> hour6Hash, Dictionary<int, double> hour7Hash, Dictionary<int, double> hour8Hash, Dictionary<int, double> hour9Hash, Dictionary<int, double> hour10Hash, Dictionary<int, double> hour11Hash, Dictionary<int, double> hour12Hash, Dictionary<int, double> hour13Hash, Dictionary<int, double> hour14Hash, Dictionary<int, double> hour15Hash)
        {
            if (Zone.ToLower() == ZoneSelectedItem.ToLower())
            {
                mLoadList.Clear();
                UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                try
                {
                    if (hour5Hash != null && hour5Hash.Count > 0)
                        PrepareLoadSeries(hour5Hash, "HISO".ToUpper());
                    if (hourHash != null && hourHash.Count > 0)
                        PrepareLoadSeries(hourHash, "Current".ToUpper());
                    if (hour1Hash != null && hour1Hash.Count > 0)
                        PrepareLoadSeries(hour1Hash, "ISO".ToUpper());
                    if (hour2Hash != null && hour2Hash.Count > 0)
                        PrepareLoadSeries(hour2Hash, "PRT".ToUpper());
                    /* if (hour3Hash != null && hour3Hash.Count > 0)
                         PrepareLoadSeries(hour3Hash, "TESLA".ToUpper());*/
                    if (hour10Hash != null && hour10Hash.Count > 0)
                        PrepareLoadSeries(hour10Hash, "WSI".ToUpper());
                    if (hour13Hash != null && hour13Hash.Count > 0)
                        PrepareLoadSeries(hour13Hash, "DTN".ToUpper());
                    if (hour4Hash != null && hour4Hash.Count > 0)
                        PrepareLoadSeries(hour4Hash, "DA".ToUpper());
                    if (hour9Hash != null && hour9Hash.Count > 0)
                        PrepareLoadSeries(hour9Hash, "FISO".ToUpper());
                    if (hour7Hash != null && hour7Hash.Count > 0)
                        PrepareLoadSeries(hour7Hash, "FPRT".ToUpper());
                    /*  if (hour8Hash != null && hour8Hash.Count > 0)
                          PrepareLoadSeries(hour8Hash, "FTESLA".ToUpper());*/
                    if (hour11Hash != null && hour11Hash.Count > 0)
                        PrepareLoadSeries(hour11Hash, "FWSI".ToUpper());
                    if (hour14Hash != null && hour14Hash.Count > 0)
                        PrepareLoadSeries(hour14Hash, "FDTN".ToUpper());
                    if (hour6Hash != null && hour6Hash.Count > 0)
                        PrepareLoadSeries(hour6Hash, "HDA".ToUpper());
                    if (hour15Hash != null && hour15Hash.Count > 0)
                        PrepareLoadSeries(hour15Hash, "FDA".ToUpper());
                    if (ZoneSelectedItem.ToLower() == "pjm southern region")
                    {
                        if (hour1Hash != null && hour1Hash.Count > 0)
                            PrepareLoadSeries(hour1Hash, "CISO".ToUpper());
                    }
                    GetFrozenLoadsData();
                    RefreshPlot();
                }
                catch
                {
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public class GraphItem
        {
            /// <summary>
            /// Gets or sets the x.
            /// </summary>
            /// <value>
            /// The x.
            /// </value>
            public DateTime X { get; set; }
            /// <summary>
            /// Gets or sets the y.
            /// </summary>
            /// <value>
            /// The y.
            /// </value>
            public double? Y { get; set; }
        }
    }
}
