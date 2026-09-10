using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Vayu.Actualvs7DayLoad.Model;
using Vayu.LoadGraphLibrary;

namespace Vayu.Actualvs7DayLoad.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly IDataService _dataService;
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        private WCFSubscriberLoadGraphServer mWCFSubscriberLoadGraphServer;
        private System.Timers.Timer heartBeatTimer = null;
        private string mLoadServerUrl = Vayu.CommonAccessLibrary.ServiceConnections.GetLoadGraphService();
        private Dictionary<string, List<LoadDataItem>> mLoadList;
        private ILoadGraph mLoadProxy;
        private DuplexChannelFactory<ILoadGraph> pipeFactory;
        private static readonly object lockObj = new object();
        private Dictionary<string, OxyColor> mOxyColorList;
        private DateTime mFromSelectedDate;
        public DateTime FromSelectedDate
        {
            get
            {
                return mFromSelectedDate;
            }
            set
            {
                mFromSelectedDate = value; RaisePropertyChanged("FromSelectedDate");
            }
        }

        private DateTime mEndSelectedDate;
        public DateTime EndSelectedDate
        {
            get
            {
                return mEndSelectedDate;
            }
            set
            {
                mEndSelectedDate = value;
                RaisePropertyChanged("EndSelectedDate");
                if (value != null)
                {
                    mEndSelectedDate = mEndSelectedDate.AddHours(23).AddMinutes(55);
                }
            }
        }
        private List<string> mZoneList;
        public List<string> ZoneList
        {
            get
            {
                return mZoneList;
            }
            set
            {
                mZoneList = value;
                RaisePropertyChanged("ZoneList");
            }
        }
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
                if (mWCFSubscriberLoadGraphServer == null)
                {
                    InitProxyConfigLMPServer();
                }
                if (ZoneSelectedItem != null || ZoneSelectedItem != string.Empty)
                {
                    ConnectLoadGraphServer();
                    ReconfigureProxySettings();
                    GetLoadData();
                    //  GetLiveData();
                    var firstTask = new Task(() => mWCFSubscriberLoadGraphServer.UnsubscribeAll());
                    var secondTask = firstTask.ContinueWith((t) => mWCFSubscriberLoadGraphServer.startConnectAndSubscribeAll());
                    firstTask.Start();
                }
            }
        }
        private bool mCURRENTChecked;
        public bool CURRENTChecked
        {
            get { return mCURRENTChecked; }
            set
            {
                mCURRENTChecked = value;
                RaisePropertyChanged("CURRENTChecked");
                RefreshPlot();
            }
        }
        private bool mISOChecked;
        public bool ISOChecked
        {
            get
            {
                return mISOChecked;
            }
            set
            {
                mISOChecked = value; RaisePropertyChanged("ISOChecked");
                RefreshPlot();
            }
        }

        private bool _ISO5DAYAvgChecked;

        public bool ISO5DAYAvgChecked
        {
            get { return _ISO5DAYAvgChecked; }
            set
            {
                _ISO5DAYAvgChecked = value;
                RaisePropertyChanged("ISO5DAYAvgChecked");
                RefreshPlot();
            }
        }
        private bool _ISO8DAYAvgChecked;

        public bool ISO8DAYAvgChecked
        {
            get { return _ISO8DAYAvgChecked; }
            set
            {
                _ISO8DAYAvgChecked = value;
                RaisePropertyChanged("ISO8DAYAvgChecked");
                RefreshPlot();
            }
        }

        private bool _ISO13DAYAvgChecked;

        public bool ISO13DAYAvgChecked
        {
            get { return _ISO13DAYAvgChecked; }
            set
            {
                _ISO13DAYAvgChecked = value;
                RaisePropertyChanged("ISO13DAYAvgChecked");
                RefreshPlot();
            }
        }

        private bool _ISO21DAYAvgChecked;

        public bool ISO21DAYAvgChecked
        {
            get { return _ISO21DAYAvgChecked; }
            set
            {
                _ISO21DAYAvgChecked = value;
                RaisePropertyChanged("ISO21DAYAvgChecked");
                RefreshPlot();
            }
        }


        private OxyPlot.PlotModel mPlotDataModel;
        public OxyPlot.PlotModel PlotDataModel
        {
            get
            {
                return mPlotDataModel;
            }
            set
            {
                mPlotDataModel = value; RaisePropertyChanged("PlotDataModel");
            }
        }
        private string mUpdateTime;
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
        private PlotModel mPlotModelUpper;
        /// <summary>
        /// Gets or sets the plot model upper.
        /// </summary>
        /// <value>
        /// The plot model upper.
        /// </value>
        public PlotModel PlotModelUpper
        {
            get
            {
                return mPlotModelUpper;
            }
            set
            {
                mPlotModelUpper = value;
                RaisePropertyChanged("PlotModelUpper");
            }
        }

        public MainWindowViewModel(IDataService dataService)
        {
            if (dataService == null)
            {
                _dataService = new Model.DataService();
            }
            else
            {
                _dataService = dataService;
            }
            CheckAllCheckboxes();
            FromSelectedDate = DateTime.Today.AddDays(-10);
            EndSelectedDate = DateTime.Today.AddDays(7);
            LoadLoadData();
            CloseCommand = new DelegateCommand(() => CloseAndUnsubscribe());
            RefreshCommand = new DelegateCommand(() => Refresh());
            if (heartBeatTimer == null)
            {
                heartBeatTimer = new System.Timers.Timer(180000);
            }
            heartBeatTimer.Enabled = true;
            heartBeatTimer.Elapsed += heartBeatTimer_Elapsed;

        }
        private void CheckAllCheckboxes()
        {
            foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(a => a.Name.Contains("Checked") && a.PropertyType == typeof(bool)))
            {
                item.SetValue(this, true);
            }
        }
        private void LoadLoadData()
        {
            FillZoneList();
        }
        private void FillZoneList()
        {
            _dataService.GetLoadNames((item, ex) =>
            {
                if (ex == null)
                {
                    ZoneList = item.OrderBy(a => a).ToList();
                    ZoneSelectedItem = item.Find(a => a.ToLower().Contains("ercot total"));
                }
            });
        }
        private void RefreshPlot()
        {
            if (mLoadList != null && mLoadList.Count > 0 && PlotDataModel != null)
            {
                lock (lockObj)
                {
                    try
                    {
                        FixLoadData();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        private void FixLoadData()
        {
            PlotDataModel = new PlotModel();
            DateTime from = FromSelectedDate;
            DateTime to = EndSelectedDate;
            PlotModel tempModel = new PlotModel();
            if (mLoadList == null || mLoadList.Count == 0)
            {
                return;
            }
            tempModel.Axes.Add(new LinearAxis
            {
                Key = "YAxis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.WhiteSmoke,
                // EndPosition = 1,
                Position = AxisPosition.Left,
                Title = "MW ----->",
                StartPosition = 0.005,
                EndPosition = 0.995,
                AxisTitleDistance = 0
            });
            tempModel.Axes.Add(new DateTimeAxis
            {
                Title = "Market Date ---->",
                Position = AxisPosition.Bottom,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.WhiteSmoke,
                AxisTitleDistance = 0,
                StringFormat = "MMM-dd",
                MajorGridlineStyle = LineStyle.Solid,
                AxislineThickness = 5,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                StartPosition = 0.005,
                EndPosition = 0.995,
            });
            try
            {
                if (mOxyColorList == null)
                {
                    FillColorList();
                }
                foreach (var item in mLoadList.Keys)
                {
                    if (KeyChecked(item))
                    {
                        try
                        {
                            tempModel.Series.Add(CreateSeries(mLoadList[item].OrderBy(j => j.MarketDateTime).ToList(), item));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                //tempModel.LegendOrientation = LegendOrientation.Horizontal;
                //tempModel.LegendPlacement = LegendPlacement.Outside;
                //tempModel.LegendPosition = LegendPosition.RightTop;
                //tempModel.LegendTextColor = OxyColors.White;



                var l = new Legend
                {
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.RightTop,
                    LegendTextColor = OxyColors.White,
                    //LegendMargin = 0
                };

                tempModel.Legends.Add(l);
                tempModel.TitlePadding = 5;
                PlotDataModel = tempModel;
            }
            catch (Exception ex)
            {
            }
        }
        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("CURRENT", OxyColors.Yellow);
            mOxyColorList.Add("ISO", OxyColors.Lime);
            mOxyColorList.Add("5DAYS-Avg", OxyColors.Red);
            mOxyColorList.Add("8DAYS-Avg", OxyColors.Blue);
            mOxyColorList.Add("13DAYS-Avg", OxyColors.Pink);
            mOxyColorList.Add("21DAYS-Avg", OxyColors.Brown);
        }
        private bool KeyChecked(string item)
        {
            switch (item.ToLower())
            {
                case "iso": return ISOChecked;
                case "current": return CURRENTChecked;
                case "5days-avg": return ISO5DAYAvgChecked;
                case "8days-avg": return ISO8DAYAvgChecked;
                case "13days-avg": return ISO13DAYAvgChecked;
                case "21days-avg": return ISO21DAYAvgChecked;
                default: return true;
            }
        }
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
            catch (Exception ex)
            {
            }
            return new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " MW",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.6,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
                LineStyle = LineStyle.Solid,
                MarkerStroke = OxyColors.White,
                Title = key.ToUpper()
            };
        }
        internal void SetConnectedStatusLoadGraphServer(int proxyIndex, bool bRemoteConnected)
        {
        }
        public void SetGraph(string Zone, DateTime StartDate, DateTime EndDate, Dictionary<int, Dictionary<int, double>> hourHash, Dictionary<int, double> hour1Hash, Dictionary<int, double> hour2Hash, Dictionary<int, double> hour3Hash, Dictionary<int, double> hour4Hash, Dictionary<int, Dictionary<int, double>> hour5Hash, Dictionary<int, double> hour6Hash, Dictionary<int, double> hour7Hash, Dictionary<int, double> hour8Hash, Dictionary<int, double> hour9Hash, Dictionary<int, double> hour10Hash, Dictionary<int, double> hour11Hash, Dictionary<int, double> hour12Hash, Dictionary<int, double> hour13Hash, Dictionary<int, double> hour14Hash, Dictionary<int, double> hour15Hash)
        {
            if (Zone.ToLower() == ZoneSelectedItem.ToLower())
            {
                UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                try
                {
                    if (hourHash != null && hourHash.Count > 0)
                    {
                        PrepareLoadSeries(hourHash, "Current");
                    }
                    if (hour1Hash != null && hour1Hash.Count > 0)
                    {
                        //PrepareLoadSeries(hour1Hash, "ISO");
                    }
                    RefreshPlot();
                }
                catch (Exception ex)
                {
                }
            }
        }
        private void PrepareLoadSeries(Dictionary<int, Dictionary<int, double>> hourHash, string keySwitch)
        {
            List<LoadDataItem> grpItemsCurrent = GetHelperLoadList(hourHash, keySwitch);
            if (mLoadList.ContainsKey("CURRENT"))
            {
                List<LoadDataItem> tempList = mLoadList["CURRENT"];
                grpItemsCurrent.ForEach(a =>
                {
                    tempList.Add(a);
                });
                mLoadList["CURRENT"] = tempList.OrderBy(k => k.MarketDateTime).ToList();
            }
            else
            {
                mLoadList.Add("CURRENT", grpItemsCurrent);
            }
        }
        private List<LoadDataItem> GetHelperLoadList(Dictionary<int, Dictionary<int, double>> hourHash, string keySwitch)
        {
            List<LoadDataItem> grpItems = new List<LoadDataItem>();
            foreach (var hourItem in hourHash.OrderBy(a => a.Key))
            {
                foreach (var minuteItem in hourHash[hourItem.Key].OrderBy(a => a.Key))
                {
                    grpItems.Add(new LoadDataItem
                    {
                        MarketDateTime = keySwitch.ToLower().Contains("history") ? FromSelectedDate.AddHours(hourItem.Key).AddMinutes(minuteItem.Key) :
                                                                                        DateTime.Today.AddHours(hourItem.Key).AddMinutes(minuteItem.Key),
                        LoadForecast = minuteItem.Value
                    });
                }
            }
            return grpItems.OrderBy(a => a.MarketDateTime).ToList();
        }
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
            catch (Exception ex)
            {

            }
        }

        private void ReconfigureProxySettings()
        {
            ((WCFSubscriberLoadGraphServer.ProxySetting)mWCFSubscriberLoadGraphServer.proxySettingHash[0]).mZone = ZoneSelectedItem;
            ((WCFSubscriberLoadGraphServer.ProxySetting)mWCFSubscriberLoadGraphServer.proxySettingHash[0]).mStartDate = FromSelectedDate;
            ((WCFSubscriberLoadGraphServer.ProxySetting)mWCFSubscriberLoadGraphServer.proxySettingHash[0]).mEndDate = EndSelectedDate;
        }
        private int GetMarketKey(string market)
        {
            if (market == "PJM")
            {
                return 1;
            }
            if (market == "MISO")
            {
                return 2;
            }
            if (market == "ERCOT")
            {
                return 9;
            }
            if (market == "CAISO")
            {
                return 7;
            }
            if (market == "SPP")
            {
                return 12;
            }
            return 3;
        }
        private void GetLoadData()
        {
            try
            {
                string[] array = ZoneSelectedItem.Split(' ');
                string Market = array[0].ToString();
                int Marketkey = GetMarketKey(Market);

                _dataService.GetLoadData((item, ex) =>
                {
                    mLoadList = item;
                }, FromSelectedDate, EndSelectedDate, ZoneSelectedItem, Marketkey);
                FixLoadData();

                List<LoadDataItem> LoadDataItemsList = new List<LoadDataItem>();
                var first = mLoadList.FirstOrDefault();
                var last = mLoadList.LastOrDefault();
                foreach (var item in mLoadList)
                {
                    if (item.Key == "ISO")
                        first = item;
                    else if (item.Key == "CURRENT")
                        last = item;
                }
                for (int i = 0; i < (first.Value.Count - 1); i++)
                {
                    LoadDataItem mitem = new LoadDataItem();
                    for (int j = 0; j < last.Value.Count; j++)
                    {
                        if (i == j)
                        {
                            mitem.LoadForecast = last.Value[j].LoadForecast - first.Value[i].LoadForecast;
                            mitem.MarketDateTime = last.Value[j].MarketDateTime;
                            LoadDataItemsList.Add(mitem);
                            break;
                        }
                    }
                }
                for (int i = last.Value.Count; i < first.Value.Count; i++)
                {
                    LoadDataItem mitem = new LoadDataItem();
                    mitem.LoadForecast = 0;
                    mitem.MarketDateTime = first.Value[i].MarketDateTime;
                    LoadDataItemsList.Add(mitem);
                }
                PlotModelUpper = CreatePlotModelUpper(LoadDataItemsList);
            }
            catch (Exception ex)
            {
            }
            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        private PlotModel CreatePlotModelUpper(List<LoadDataItem> mLoadList)
        {
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Actual-------->Forecast";
            var c = OxyColors.IndianRed;
            plotModel1.Axes.Add(new LinearAxis()
            {
                Key = "Y1Axis",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                AxisTitleDistance = 0,

            });
            plotModel1.Axes.Add(new LinearAxis()
            {
                Key = "Y2Axis",
                Position = AxisPosition.Right,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,

            });
            // X axis
            var dataItemValues = new Collection<Item>(); // use with non DateTime x axis
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                //Angle = 90,
                StringFormat = "0",
                MajorStep = 1,
                IsPanEnabled = true,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                TickStyle = TickStyle.Outside,
                // FontSize = 10,
                //AxislineThickness = 3,
                IsTickCentered = true,
                Key = "XAxisCategory",
                GapWidth = 0.01,
                //Position = AxisPosition.Bottom,
                //MinorTickSize=5,
                AxisTitleDistance = 0,

            };
            //categoryAxis.MajorStep = 1;
            plotModel1.Axes.Add(categoryAxis);
            //plotModel1.PlotMargins = new OxyThickness(10, 0, 5, 50);
            var colSeries1 = new BarSeries();
            string colTitle = "Actual-->Forecast";
            colSeries1 = new BarSeries()
            {
                Title = colTitle,
                YAxisKey = "XAxisCategory",
                XAxisKey = "Y2Axis",
                NegativeFillColor = OxyColors.IndianRed,
                FillColor = OxyColors.DarkGreen,

            };
            for (int i = 0; i < (mLoadList.Count - 1); i++)
            {
                //categoryAxis.GapWidth = 1;
                categoryAxis.Labels.Add(mLoadList[i].MarketDateTime.ToString("M/d/yy"));
                var colItem1 = new BarItem(Math.Round(mLoadList[i].LoadForecast.Value), i);

                colSeries1.Items.Add(colItem1);
            }
            //plotModel1.LegendOrientation = LegendOrientation.Horizontal;
            //plotModel1.LegendPlacement = LegendPlacement.Outside;
            //plotModel1.LegendPosition = LegendPosition.RightTop;
            //plotModel1.LegendTextColor = OxyColors.White;
            //plotModel1.TitlePadding = 5;


            var l = new Legend
            {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendTextColor = OxyColors.White,

            };

            plotModel1.Legends.Add(l);
            plotModel1.TitlePadding = 5;
            plotModel1.Series.Add(colSeries1);

            return plotModel1;
        }

        private void CloseAndUnsubscribe()
        {
            try
            {
                if (mWCFSubscriberLoadGraphServer != null)
                {
                    mWCFSubscriberLoadGraphServer.UnsubscribeAndDisconnectAll();
                }
                if (heartBeatTimer != null)
                {
                    heartBeatTimer.Enabled = false;
                    heartBeatTimer.Close();
                    heartBeatTimer = null;
                }
            }
            catch (Exception ex)
            {
            }
        }
        void heartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            heartBeatTimer.Enabled = false;
            try
            {
                if (mLoadProxy == null)
                {
                    ConnectLoadGraphServer();
                }
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
        private void Refresh()
        {
            if (mLoadList != null)
            {
                mLoadList.Clear();
            }
            if (FromSelectedDate >= EndSelectedDate)
            {
                System.Windows.MessageBox.Show("Please select thro' date to be greater than start date");
                return;
            }
            //if ((EndSelectedDate - FromSelectedDate).Days > 30)
            //{
            //    FromSelectedDate = DateTime.Today.AddDays(-30);
            //    System.Windows.MessageBox.Show("Difference between selected dates is much higher.\n Please select date range upto 30");
            //    return;
            //}
            GetLoadData();
            //GetLiveData();
            RefreshPlot();
        }
    }
    public class GraphItem
    {
        public DateTime X { get; set; }
        public double? Y { get; set; }
    }
}
