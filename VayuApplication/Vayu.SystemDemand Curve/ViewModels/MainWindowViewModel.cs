using OxyPlot;
using OxyPlot.Annotations;
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
using Vayu.LoadGraphLibrary;
using Vayu.SystemDemand_Curve.Model;

namespace Vayu.SystemDemand_Curve.ViewModels
{
    public class MainWindowViewModel : BindableBase, ILoadGraphCallback
    {
        #region Declaration

        private readonly IDataService _dataService;
        private static readonly object lockObj = new object();
        private static string mUser = Environment.UserName;
        private Dictionary<string, List<LoadDataItem>> mLoadList;
        private Dictionary<string, OxyColor> mOxyColorList;
        private ILoadGraph mLoadProxy;
        private DuplexChannelFactory<ILoadGraph> pipeFactory;
        private WCFSubscriberLoadGraphServer mWCFSubscriberLoadGraphServer;
        private System.Timers.Timer heartBeatTimer = null;
        private string mLoadServerUrl = ServiceConnections.GetLoadGraphService();

        #endregion

        #region Properties

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }

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
                if (ZoneSelectedItem != null)
                {
                    ConnectLoadGraphServer();
                    ReconfigureProxySettings();
                    GetLoadData();
                    GetLiveData();
                    var firstTask = new Task(() => mWCFSubscriberLoadGraphServer.UnsubscribeAll());
                    var secondTask = firstTask.ContinueWith((t) => mWCFSubscriberLoadGraphServer.startConnectAndSubscribeAll());
                    firstTask.Start();
                }
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
        private string mFrozenUpdateTime;
        public string FrozenUpdateTime
        {
            get
            {
                return mFrozenUpdateTime;
            }
            set
            {
                mFrozenUpdateTime = value;
                RaisePropertyChanged("FrozenUpdateTime");
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
        private bool mPRTChecked;
        public bool PRTChecked
        {
            get
            {
                return mPRTChecked;
            }
            set
            {
                mPRTChecked = value;
                RaisePropertyChanged("PRTChecked");
                RefreshPlot();
            }
        }
        private bool mWSIChecked;
        public bool WSIChecked
        {
            get
            {
                return mWSIChecked;
            }
            set
            {
                mWSIChecked = value; RaisePropertyChanged("WSIChecked");
                RefreshPlot();
            }
        }
        private bool mDTNChecked;
        public bool DTNChecked
        {
            get
            {
                return mDTNChecked;
            }
            set
            {
                mDTNChecked = value;
                RaisePropertyChanged("DTNChecked");
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
        private bool _NetLoadHighChecked;

        public bool NetLoadHighChecked
        {
            get { return _NetLoadHighChecked; }
            set
            {
                _NetLoadHighChecked = value;
                RaisePropertyChanged("NetLoadHighChecked");
                RefreshPlot();
            }
        }

        private bool _FrozenNetLoadChecked;

        public bool FrozenNetLoadChecked
        {
            get { return _FrozenNetLoadChecked; }
            set
            {
                _FrozenNetLoadChecked = value;
                RaisePropertyChanged("FrozenNetLoadChecked");
                RefreshPlot();
            }
        }

        private bool mDAChecked;
        public bool DAChecked
        {
            get
            {
                return mDAChecked;
            }
            set
            {
                mDAChecked = value;
                RaisePropertyChanged("DAChecked");
                RefreshPlot();
            }
        }
        private bool mGenscapeChecked;
        public bool GenscapeChecked
        {
            get
            {
                return mGenscapeChecked;
            }
            set
            {
                mGenscapeChecked = value;
                RaisePropertyChanged("GenscapeChecked");
                RefreshPlot();
            }
        }
        private bool mHourlyChecked;
        public bool HourlyChecked
        {
            get
            {
                return mHourlyChecked;
            }
            set
            {
                mHourlyChecked = value;
                RaisePropertyChanged("HourlyChecked");
                RefreshPlot();
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
        private bool mISODayHighChecked;
        public bool ISODayHighChecked
        {
            get
            {
                return mISODayHighChecked;
            }
            set
            {
                mISODayHighChecked = value; RaisePropertyChanged("ISODayHighChecked");
                RefreshPlot();
            }
        }
        private bool mFrozenPRTChecked;
        public bool FrozenPRTChecked
        {
            get
            {
                return mFrozenPRTChecked;
            }
            set
            {
                mFrozenPRTChecked = value;
                RaisePropertyChanged("FrozenPRTChecked");
                RefreshPlot();
            }
        }
        private bool mFrozenWSIChecked;
        public bool FrozenWSIChecked
        {
            get
            {
                return mFrozenWSIChecked;
            }
            set
            {
                mFrozenWSIChecked = value;
                RaisePropertyChanged("FrozenWSIChecked");
                RefreshPlot();
            }
        }
        private bool mFrozenDTNChecked;
        public bool FrozenDTNChecked
        {
            get
            {
                return mFrozenDTNChecked;
            }
            set
            {
                mFrozenDTNChecked = value;
                RaisePropertyChanged("FrozenDTNChecked");
                RefreshPlot();
            }
        }
        private bool mFrozenISOChecked;
        public bool FrozenISOChecked
        {
            get
            {
                return mFrozenISOChecked;
            }
            set
            {
                mFrozenISOChecked = value;
                RaisePropertyChanged("FrozenISOChecked");
                RefreshPlot();
            }
        }
        private bool mFiveFrozenChecked;
        public bool FiveFrozenChecked
        {
            get
            {
                return mFiveFrozenChecked;
            }
            set
            {
                mFiveFrozenChecked = value;
                RaisePropertyChanged("FiveFrozenChecked");
                GetFrozenLoadData(16);
                Refresh();
            }
        }
        private bool mElevenFrozenChecked = true;
        public bool ElevenFrozenChecked
        {

            get
            {
                return mElevenFrozenChecked;
            }

            set
            {
                mElevenFrozenChecked = value;
                RaisePropertyChanged("ElevenFrozenChecked");
                GetFrozenLoadData(10);
                Refresh();
            }
        }

        #endregion

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
            FromSelectedDate = DateTime.Today.AddDays(-1);
            EndSelectedDate = DateTime.Today.AddDays(8);
            RefreshCommand = new DelegateCommand(() => Refresh());
            CloseCommand = new DelegateCommand(() => CloseAndUnsubscribe());
            Task.Factory.StartNew(() => FillColorList());
            LoadLoadData();
            InitProxyConfigLMPServer();
            ConnectLoadGraphServer();
            if (heartBeatTimer == null)
            {
                heartBeatTimer = new System.Timers.Timer(180000);
            }
            heartBeatTimer.Enabled = true;
            heartBeatTimer.Elapsed += heartBeatTimer_Elapsed;

        }

        #region Private Methods

        private void RefreshPlot()
        {
            if (mLoadList != null && mLoadList.Count > 0 && PlotDataModel != null)
            {
                lock (lockObj)
                {
                    try
                    {
                        FixLoadData();
                        /*mTempModel = new PlotModel();
                        PlotDataModel = null;
                        /*foreach (var item in mTempModel.Series.ToList())
                        {
                            mTempModel.Series.Remove(item);
                        }
                        foreach (var kItem in mLoadList)
                        {
                            if (KeyChecked(kItem.Key))
                            {
                                mTempModel.Series.Add(CreateSeries(mLoadList[kItem.Key].OrderBy(o => o.MarketDateTime).ToList(), kItem.Key));
                            }
                        }
                        PlotDataModel = mTempModel as PlotModel;
                        PlotDataModel.LegendOrientation = LegendOrientation.Horizontal;
                        PlotDataModel.LegendPlacement = LegendPlacement.Inside;
                        PlotDataModel.LegendPosition = LegendPosition.BottomLeft;
                        PlotDataModel.Update();*/
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
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

        private void CheckAllCheckboxes()
        {
            foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(a => a.Name.Contains("Checked") &&
                !a.Name.Contains("Five") && !a.Name.Contains("Eleven") && a.PropertyType == typeof(bool)))
            {
                item.SetValue(this, true);
            }
        }

        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("CURRENT", OxyColors.Yellow);
            mOxyColorList.Add("ISO", OxyColors.Lime);
            mOxyColorList.Add("DA", OxyColors.Red);
            mOxyColorList.Add("FROZENISO", OxyColors.Lime);
            mOxyColorList.Add("HOURLY", OxyColors.Orange);
            mOxyColorList.Add("Net Load", OxyColors.DeepSkyBlue);
            mOxyColorList.Add("Forecast Net Load", OxyColors.DeepSkyBlue);
            mOxyColorList.Add("Frozen Net Load", OxyColors.DeepSkyBlue);
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
            if ((EndSelectedDate - FromSelectedDate).Days > 90)
            {
                FromSelectedDate = DateTime.Today.AddDays(-90);
                System.Windows.MessageBox.Show("Difference between selected dates is much higher.\n Please select date range upto 90 ");
                return;
            }
            GetLoadData();
            GetLiveData();
            RefreshPlot();
        }

        private void LoadLoadData()
        {
            FillZoneList();
        }
        private void GetLoadData()
        {
            try
            {
                _dataService.GetLoadData((item, ex) =>
                {
                    mLoadList = item;
                }, FromSelectedDate, EndSelectedDate, ZoneSelectedItem);
                _dataService.GetFrozenData((item, ex) =>
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

                FixLoadData();
            }
            catch (Exception ex)
            {
            }
        }

        private void GetFrozenLoadData(int FrozenHour)
        {
            try
            {
                _dataService.GetFrozenData((item, ex) =>
                {
                    foreach (var key in item)
                    {
                        if (mLoadList.ContainsKey(key.Key))
                        {
                            mLoadList.Remove(key.Key);
                        }
                        mLoadList.Add(key.Key, key.Value.OrderBy(a => a.MarketDateTime).ToList());
                    }
                }, FromSelectedDate, EndSelectedDate.AddDays(-1), ZoneSelectedItem, FrozenHour);
            }
            catch (Exception ex)
            {
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
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.WhiteSmoke,
                EndPosition = 1,
                Position = AxisPosition.Left,
                Title = "MW ----->",
                AxisTitleDistance = 0
            });
            tempModel.Axes.Add(new DateTimeAxis
            {
                Title = "Market Date ---->",
                Position = AxisPosition.Bottom,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.WhiteSmoke,
                AxisTitleDistance = 0,
                StringFormat = "HH:mm\n MMM-dd",
                MajorGridlineStyle = LineStyle.Solid,
                AxislineThickness = 3,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
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
                        var seriesAndAnnotations = CreateSeries(mLoadList[item].OrderBy(j => j.MarketDateTime).ToList(), item);
                        var series = seriesAndAnnotations.Item1;
                        if (item == "Net Load")
                        {

                            var series1 = CreateSeries1(mLoadList[item].OrderBy(j => j.MarketDateTime).ToList(), item);
                            var netseries = series1.Item1;
                            tempModel.Series.Add(netseries);

                        }
                        var textAnnotations = seriesAndAnnotations.Item2;
                        tempModel.Series.Add(series);

                        if (item == "PEAK LOAD OF DAY")
                        {
                            foreach (var textAnnotation in textAnnotations)
                            {
                                tempModel.Annotations.Add(textAnnotation);
                            }
                        }
                        // tempModel.Series.Add(CreateSeries(mLoadList[item].OrderBy(j => j.MarketDateTime).ToList(), item));
                    }
                }

                var l = new Legend
                {
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.RightTop,
                    LegendTextColor = OxyColors.White,
                    LegendPadding = 3
                };

                tempModel.Legends.Add(l);
                PlotDataModel = tempModel;


            }
            catch (Exception ex)
            {
            }
        }


        private void FixPeakLoadData()
        {
            PlotDataModel = new PlotModel();
            DateTime from = FromSelectedDate;
            DateTime to = EndSelectedDate;
            var Model = new PlotModel();
        }
        private bool KeyChecked(string item)
        {
            switch (item.ToLower())
            {
                case "iso": return ISOChecked;
                case "peak load of day": return ISODayHighChecked;
                case "da": return DAChecked;
                case "current": return CURRENTChecked;
                case "net load": return NetLoadHighChecked;
                case "frozen net load": return FrozenNetLoadChecked;
                case "frozeniso": return FrozenISOChecked;
                case "hourly": return HourlyChecked;
                default: return true;
            }

        }

        private Tuple<LineSeries, List<TextAnnotation>> CreateSeries(List<LoadDataItem> list, string key)
        {
            list.RemoveAll(a => a.LoadForecast == 0);
            List<GraphItem> grpList = new List<GraphItem>();
            List<GraphItem> grpList1 = new List<GraphItem>();
            try
            {
                list.ForEach(dItem =>
                {
                    grpList1.Add(new GraphItem
                    {
                        X = dItem.MarketDateTime,
                        Y = dItem.LoadForecast
                    });
                });
            }

            catch (Exception ex)
            {
            }
            if (key != "PEAK LOAD OF DAY")
            {
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
            }
            string Title = key.ToUpper();
            if (Title == "NET LOAD")
            {
                Title = "Forecast NET LOAD";
            }
            var series = new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " MW",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 1.5,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.Gray,
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = Title

            };


            // Create a list to hold text annotations
            var textAnnotations = new List<TextAnnotation>();

            // Add text annotations to the list
            foreach (var dataPoint in grpList1)
            {
                double yoffset = 800;
                var textAnnotation = new TextAnnotation
                {
                    Text = $"{dataPoint.Y:0.###} MW\n{dataPoint.X:MM-dd HH:mm}",
                    // Text = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " MW",
                    TextPosition = new DataPoint(DateTimeAxis.ToDouble(dataPoint.X), (double)dataPoint.Y + yoffset),
                    StrokeThickness = 0,
                    TextColor = OxyColors.OrangeRed,
                    FontSize = 14,
                };

                textAnnotations.Add(textAnnotation);
            }

            // Return the series and text annotations
            return new Tuple<LineSeries, List<TextAnnotation>>(series, textAnnotations);
        }

        private Tuple<LineSeries, List<TextAnnotation>> CreateSeries1(List<LoadDataItem> list, string key)
        {
            list.RemoveAll(a => a.LoadForecast == 0);
            List<GraphItem> darkLineData = new List<GraphItem>();
            List<GraphItem> thinLineData = new List<GraphItem>();
            var currentTime = DateTime.Now;

            foreach (var item in list)
            {
                if (item.MarketDateTime < currentTime.AddHours(-1))
                {
                    darkLineData.Add(new GraphItem
                    {
                        X = item.MarketDateTime,
                        Y = item.LoadForecast
                    });
                }
                else
                {
                    thinLineData.Add(new GraphItem
                    {
                        X = item.MarketDateTime,
                        Y = item.LoadForecast
                    });
                }
            }

            var darkLineSeries = new LineSeries
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = darkLineData,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " MW",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 1.5,
                StrokeThickness = 6,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.Gray,
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = "Actual NET LOAD"//key.ToUpper()
            };

            var thinLineSeries = new LineSeries
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = thinLineData,
                MarkerType = MarkerType.None,
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " MW",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.5,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.Gray,
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                StrokeThickness = 0.5 // Set the thickness for thin line
            };

            // Create a list to hold text annotations
            var textAnnotations = new List<TextAnnotation>();

            // Add text annotations to the list
            foreach (var dataPoint in darkLineData)
            {
                double yOffset = 800;

                var textAnnotation = new TextAnnotation
                {
                    Text = $"{dataPoint.Y:0.###} MW\n{dataPoint.X:MM-dd HH:mm}",
                    TextPosition = new DataPoint(DateTimeAxis.ToDouble(dataPoint.X), (double)dataPoint.Y + yOffset),
                    StrokeThickness = 0,
                    TextColor = OxyColors.OrangeRed,
                    FontSize = 14,
                };

                textAnnotations.Add(textAnnotation);
            }

            // Return the series and text annotations
            return new Tuple<LineSeries, List<TextAnnotation>>(darkLineSeries, textAnnotations);
        }

        private LineStyle GetLineStyle(string key)
        {
            if (key.ToUpper().StartsWith("FROZEN"))
            {
                return LineStyle.Dot;
            }
            else
            {
                return LineStyle.Solid;
            }
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
        private void ConnectLoadGraphServer()
        {
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.OpenTimeout = new TimeSpan(0, 5, 0);
            myBinding.SendTimeout = new TimeSpan(0, 5, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 5, 0);
            myBinding.Security.Mode = SecurityMode.None;
            myBinding.MaxConnections = 1000;
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

        private void GetLiveData()
        {
            if (EndSelectedDate.Date < DateTime.Now.Date)
            {
                var firstTask = new Task(() => mWCFSubscriberLoadGraphServer.UnsubscribeAll());
                firstTask.Start();
                return;
            }

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
        public void SetGraph(string Zone, DateTime StartDate, DateTime EndDate, Dictionary<int, Dictionary<int, double>> hourHash, Dictionary<int, double> hour1Hash, Dictionary<int, double> hour2Hash, Dictionary<int, double> hour3Hash, Dictionary<int, double> hour4Hash, Dictionary<int, Dictionary<int, double>> hour5Hash, Dictionary<int, double> hour6Hash, Dictionary<int, double> hour7Hash, Dictionary<int, double> hour8Hash, Dictionary<int, double> hour9Hash, Dictionary<int, double> hour10Hash, Dictionary<int, double> hour11Hash, Dictionary<int, double> hour12Hash, Dictionary<int, double> hour13Hash, Dictionary<int, double> hour14Hash, Dictionary<int, double> hour15Hash)
        {
            if (Zone.ToLower() == ZoneSelectedItem.ToLower())
            {

                DateTime dateTimeValue = _dataService.GetFrozenUpdateTimeLoads(StartDate, EndDate, Zone);
                FrozenUpdateTime = dateTimeValue.ToString("yyyy-MM-dd HH:mm");
                DateTime RTdateTimeValue = _dataService.GetRTUpdateTimeLoads();
                UpdateTime = RTdateTimeValue.ToString("yyyy-MM-dd HH:mm");

                try
                {
                    if (hour5Hash != null && hour5Hash.Count > 0)
                    {
                        PrepareLoadSeries(hour5Hash, "History");
                    }
                    if (hourHash != null && hourHash.Count > 0)
                    {
                        PrepareLoadSeries(hourHash, "Current");
                    }
                    if (hour1Hash != null && hour1Hash.Count > 0)
                    {
                        PrepareLoadSeries(hour1Hash, "ISO");
                    }
                    if (hour2Hash != null && hour2Hash.Count > 0)
                    {
                        // PrepareLoadSeries(hour2Hash, "PRT");
                    }
                    if (hour3Hash != null && hour3Hash.Count > 0)
                    {
                        // PrepareLoadSeries(hour3Hash, "TESLA");
                    }
                    if (hour10Hash != null && hour10Hash.Count > 0)
                    {
                        // PrepareLoadSeries(hour10Hash, "WSI");
                    }
                    if (hour13Hash != null && hour13Hash.Count > 0)
                    {
                        // PrepareLoadSeries(hour13Hash, "DTN");
                    }
                    if (hour4Hash != null && hour4Hash.Count > 0)
                    {
                        PrepareLoadSeries(hour4Hash, "DA");
                    }
                    RefreshPlot();
                }
                catch (Exception ex)
                {
                }
            }
        }
        private void PrepareLoadSeries(Dictionary<int, double> hour3Hash, string key)
        {
            List<LoadDataItem> grpItemsCurrent = GetHelperLoadList(hour3Hash);
            if (mLoadList.ContainsKey(key))
            {
                List<LoadDataItem> tempList = mLoadList[key];
                if (tempList.Count == 0)
                {
                    tempList = grpItemsCurrent.OrderBy(a => a.MarketDateTime).ToList();
                }
                else
                {
                    tempList.ForEach(a =>
                    {
                        LoadDataItem tempItem = grpItemsCurrent.Where(p => p.MarketDateTime == a.MarketDateTime).FirstOrDefault();
                        if (tempItem != null)
                        {
                            a.LoadForecast = tempItem.LoadForecast;
                        }
                    });
                }
                mLoadList.Remove(key);
                mLoadList.Add(key, tempList);
            }
        }

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
            if (mWCFSubscriberLoadGraphServer == null && ZoneSelectedItem != null)
            {
                mWCFSubscriberLoadGraphServer = new WCFSubscriberLoadGraphServer(this, null);
                bool proxyEndPointEnabled0 = true;
                WCFSubscriberLoadGraphServer.ProxySetting ps0 = new WCFSubscriberLoadGraphServer.ProxySetting(mLoadServerUrl, proxyEndPointEnabled0, false, ZoneSelectedItem,
                    FromSelectedDate, EndSelectedDate);
                mWCFSubscriberLoadGraphServer.proxySettingHash.Add(0, ps0);
            }
        }

        #endregion

        internal void SetConnectedStatusLoadGraphServer(int proxyIndex, bool bRemoteConnected)
        {
        }
    }
    public class GraphItem
    {
        public DateTime X { get; set; }
        public double? Y { get; set; }
    }
}
