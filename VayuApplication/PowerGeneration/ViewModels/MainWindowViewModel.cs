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
using Vayu.PowerGeneration.Model;
using Vayu.WindServiceLibrary;

namespace Vayu.PowerGeneration.ViewModels
{

    public class MainWindowViewModel : BindableBase, Vayu.WindServiceLibrary.IWindDataCallback
    {
        private string _title = "Vayu-Power Generation";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        #region Declaration

        /// <summary>
        /// The object data service
        /// </summary>
        private IDataService objDataService;
        /// <summary>
        /// The m factory wind
        /// </summary>
        private DuplexChannelFactory<IWindDataProvider> mFactoryWind;
        /// <summary>
        /// The m wind server
        /// </summary>
        private Vayu.WindServiceLibrary.IWindDataProvider mWindServer;

        /// <summary>
        /// The wind reconnect timer
        /// </summary>
        private System.Timers.Timer windReconnectTimer;

        List<LatestWindData> mSolarList;
        /// <summary>
        /// The wind hb timer
        /// </summary>
        private System.Timers.Timer windHBTimer;

        /// <summary>
        /// The LST wind data list
        /// </summary>
        private Dictionary<string, List<WindData>> lstWindDataList;

        /// <summary>
        /// The m temporary model
        /// </summary>
        private PlotModel mTempModel;

        /// <summary>
        /// The dictionary oxy color
        /// </summary>
        private Dictionary<string, OxyColor> dictOxyColor;

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
        /// The m zone list
        /// </summary>
        private List<string> mZoneList;
        /// <summary>
        /// Gets or sets the zone list.
        /// </summary>
        /// <value>
        /// The zone list.
        /// </value>
        public List<string> ZoneList
        {
            get { return mZoneList; }
            set
            {
                mZoneList = value;
                RaisePropertyChanged("ZoneList");
            }
        }
        private List<string> mPowerTypeList;
        public List<string> PowerTypeList
        {
            get { return mPowerTypeList; }
            set
            {
                mPowerTypeList = value;
                RaisePropertyChanged("PowerTypeList");
            }
        }

        /// <summary>
        /// The m plot data model
        /// </summary>
        private PlotModel mPlotDataModel;
        /// <summary>
        /// Gets or sets the plot data model.
        /// </summary>
        /// <value>
        /// The plot data model.
        /// </value>
        public PlotModel PlotDataModel
        {
            get { return mPlotDataModel; }
            set
            {
                mPlotDataModel = value;
                RaisePropertyChanged("PlotDataModel");
            }
        }

        /// <summary>
        /// The m zone selected item
        /// </summary>
        private string mZoneSelectedItem;
        /// <summary>
        /// Gets or sets the zone selected item.
        /// </summary>
        /// <value>
        /// The zone selected item.
        /// </value>
        public string ZoneSelectedItem
        {
            get { return mZoneSelectedItem; }
            set
            {
                mZoneSelectedItem = value;
                RaisePropertyChanged("ZoneSelectedItem");
                if (ZoneSelectedItem == null)
                    GetLatestWindData();
            }
        }
        private string mPowerTypeSelectedItem;
        public string PowerTypeSelectedItem
        {
            get { return mPowerTypeSelectedItem; }
            set
            {
                mPowerTypeSelectedItem = value;
                RaisePropertyChanged("PowerTypeSelectedItem");
                if (mPowerTypeSelectedItem != null)
                    GetMarketlist(mPowerTypeSelectedItem.ToString());
                GetLatestWindData();

            }
        }

        /// <summary>
        /// The m from selected date
        /// </summary>
        private DateTime mFromSelectedDate;
        /// <summary>
        /// Gets or sets from selected date.
        /// </summary>
        /// <value>
        /// From selected date.
        /// </value>
        public DateTime FromSelectedDate
        {
            get { return mFromSelectedDate; }
            set
            {
                mFromSelectedDate = value;
                RaisePropertyChanged("FromSelectedDate");
            }
        }

        /// <summary>
        /// The m end selected date
        /// </summary>
        private DateTime mEndSelectedDate;
        /// <summary>
        /// Gets or sets the end selected date.
        /// </summary>
        /// <value>
        /// The end selected date.
        /// </value>
        public DateTime EndSelectedDate
        {
            get { return mEndSelectedDate; }
            set
            {
                mEndSelectedDate = value;
                RaisePropertyChanged("EndSelectedDate");
            }
        }

        /// <summary>
        /// The m actual checked
        /// </summary>
        private bool mActualChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [actual checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [actual checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ActualChecked
        {
            get { return mActualChecked; }
            set
            {
                mActualChecked = value;
                RaisePropertyChanged("ActualChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The m potential checked
        /// </summary>
        private bool mPotentialChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [potential checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [potential checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PotentialChecked
        {
            get { return mPotentialChecked; }
            set
            {
                mPotentialChecked = value;
                RaisePropertyChanged("PotentialChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The m forecast checked
        /// </summary>
        private bool mForecastChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [forecast checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [forecast checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ForecastChecked
        {
            get { return mForecastChecked; }
            set
            {
                mForecastChecked = value;
                RaisePropertyChanged("ForecastChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The m s actual checked
        /// </summary>
        private bool mSActualChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [s actual checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [s actual checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SActualChecked
        {
            get { return mSActualChecked; }
            set
            {
                mSActualChecked = value;
                RaisePropertyChanged("SActualChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The m s potential checked
        /// </summary>
        private bool mSPotentialChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [s potential checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [s potential checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SPotentialChecked
        {
            get { return mSPotentialChecked; }
            set
            {
                mSPotentialChecked = value;
                RaisePropertyChanged("SPotentialChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The m s forecast checked
        /// </summary>
        private bool mSForecastChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [s forecast checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [s forecast checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SForecastChecked
        {
            get { return mSForecastChecked; }
            set
            {
                mSForecastChecked = value;
                RaisePropertyChanged("SForecastChecked");
                RefreshPlot();
            }
        }

        /// <summary>
        /// The m update time
        /// </summary>
        private string mUpdateTime;
        /// <summary>
        /// Gets or sets the update time.
        /// </summary>
        /// <value>
        /// The update time.
        /// </value>
        public string UpdateTime
        {
            get { return mUpdateTime; }
            set
            {
                mUpdateTime = value;
                RaisePropertyChanged("UpdateTime");
            }
        }

        /// <summary>
        /// The m data hash
        /// </summary>
        private List<LatestWindData> mDataHash;
        /// <summary>
        /// Gets or sets the data hash.
        /// </summary>
        /// <value>
        /// The data hash.
        /// </value>
        public List<LatestWindData> DataHash
        {
            get { return mDataHash; }
            set
            {
                mDataHash = value;
                RaisePropertyChanged("DataHash");
            }
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataService"></param>

        public MainWindowViewModel(IDataService dataService)
        {
            FillColorList();
            FromSelectedDate = DateTime.Today;
            EndSelectedDate = DateTime.Today.AddDays(1);

            if (dataService == null)
            {
                objDataService = new Model.DataService();
            }
            else
            {
                objDataService = dataService;
            }

            RefreshCommand = new DelegateCommand(Refresh);
            CloseCommand = new DelegateCommand(CloseAndUnsubscribe);

            List<string> tempTypeList = new List<string>();
            tempTypeList.Add("Wind Power");
            tempTypeList.Add("Solar Power");
            PowerTypeList = tempTypeList.ToList();
            PowerTypeSelectedItem = PowerTypeList.FirstOrDefault();
            List<string> tempZoneList = new List<string>();
            tempZoneList.Add("ERCOT Total");
            tempZoneList.Add("ERCOT Houston");
            tempZoneList.Add("ERCOT West");
            tempZoneList.Add("ERCOT North");

            tempZoneList.Add("ERCOT Panhandle");
            tempZoneList.Add("ERCOT Coastal");
            tempZoneList.Add("ERCOT NorthRegion");
            tempZoneList.Add("ERCOT South");
            tempZoneList.Add("ERCOT WestRegion");
            ZoneList = tempZoneList.ToList();
            ZoneSelectedItem = ZoneList.FirstOrDefault();
            ActualChecked = true;
            ForecastChecked = true;

            GetLatestWindData();
        }

        #region Private Methods

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void Refresh()
        {
            GetLatestWindData();
        }

        /// <summary>
        /// Refreshes the plot.
        /// </summary>
        private void RefreshPlot()
        {
            mTempModel = null;
            if (DataHash != null && DataHash.Count > 0)
            {
                if (mTempModel == null)
                {
                    mTempModel = new PlotModel();
                    CreateAxes();
                }

                if (ActualChecked)
                {
                    FixActual(DataHash.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("actual")).ToList());
                }
                else
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).FirstOrDefault());
                    }
                }

                if (ForecastChecked)
                {
                    FixForecast(DataHash.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("forecast")).ToList());
                }
                else
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).FirstOrDefault());
                    }
                }

                #region Commented Code for Future Use for PotentialChecked, SActualChecked, SPotentialChecked, SForecastChecked
                //if (PotentialChecked)
                //{
                //    FixPotential(DataHash.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("potential")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).FirstOrDefault());
                //    }
                //}

                //if (SActualChecked)
                //{
                //    FixSActual(DataHash.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("sactual")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).FirstOrDefault());
                //    }
                //}

                //if (SPotentialChecked)
                //{
                //    FixSPotential(DataHash.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("spotential")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).FirstOrDefault());
                //    }
                //}

                //if (SForecastChecked)
                //{
                //    FixSForecast(DataHash.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("sforecast")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).FirstOrDefault());
                //    }
                //}
                #endregion
                PlotDataModel = null;
                PlotDataModel = mTempModel as PlotModel;
            }
        }

        /// <summary>
        /// Creates the series.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private Series CreateSeries(List<LatestWindData> list, string key)
        {
            List<GraphItem> grpList = new List<GraphItem>();
            string type = ZoneSelectedItem.ToString();
            try
            {
                if (type == "ERCOT Total" && (key == "Actual" || key == "Forecast"))
                {
                    list.ForEach(dItem =>
                    {
                        grpList.Add(new GraphItem
                        {
                            X = dItem.MarketDate,
                            Y = dItem.Value
                        });
                    });
                }
                if (type == "ERCOT Houston")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTSouth_Houston }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForeCastSouth_Houston }); });
                }
                if (type == "ERCOT West")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTWest }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForeCastWest }); });
                }
                if (type == "ERCOT North")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTNorth }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForeCastNorth }); });
                }
                if (type == "ERCOT Panhandle")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTPANHANDLE }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForecastPANHANDLE }); });
                }
                if (type == "ERCOT Coastal")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTCOASTAL }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForecastCOASTAL }); });
                }
                if (type == "ERCOT NorthRegion")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTNorthRegion }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForecastNorthRegion }); });
                }
                if (type == "ERCOT South")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTSouth }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForecastSouth }); });
                }
                if (type == "ERCOT WestRegion")
                {
                    if (key == "Actual")
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.RTWestRegion }); });
                    else
                        list.ForEach(dItem => { grpList.Add(new GraphItem { X = dItem.MarketDate, Y = dItem.ForecastWestRegion }); });
                }
            }
            catch { }
            return new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:MMM-dd HH:mm}\n{Y:0.###}" + "Value",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.5,
                Color = dictOxyColor.ContainsKey(key) ? dictOxyColor[key] : OxyColors.MediumPurple,
                LineStyle = LineStyle.Solid,
                MarkerStroke = OxyColors.White,
                Title = key.ToUpper()
            };
        }

        /// <summary>
        /// Fixes the actual.
        /// </summary>
        /// <param name="list">The list.</param>
        private void FixActual(List<LatestWindData> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).FirstOrDefault());
                    }
                    mTempModel.Series.Add(CreateSeries(list, "Actual"));
                }
                catch { }
            }
        }

        /// <summary>
        /// Fixes the forecast.
        /// </summary>
        /// <param name="list">The list.</param>
        private void FixForecast(List<LatestWindData> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).FirstOrDefault());
                    }
                    mTempModel.Series.Add(CreateSeries(list, "Forecast"));
                }
                catch { }
            }
        }

        /// <summary>
        /// Fixes the s forecast.
        /// </summary>
        /// <param name="list">The list.</param>
        private void FixSForecast(List<LatestWindData> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).FirstOrDefault());
                    }
                    mTempModel.Series.Add(CreateSeries(list, "SForecast"));
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// Fixes the s potential.
        /// </summary>
        /// <param name="list">The list.</param>
        private void FixSPotential(List<LatestWindData> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).FirstOrDefault());
                    }
                    mTempModel.Series.Add(CreateSeries(list, "SPotential"));
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// Fixes the s actual.
        /// </summary>
        /// <param name="list">The list.</param>
        private void FixSActual(List<LatestWindData> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).FirstOrDefault());
                    }
                    mTempModel.Series.Add(CreateSeries(list, "SActual"));
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// Fixes the potential.
        /// </summary>
        /// <param name="list">The list.</param>
        private void FixPotential(List<LatestWindData> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).FirstOrDefault());
                    }
                    mTempModel.Series.Add(CreateSeries(list, "Potential"));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Creates the axes.
        /// </summary>
        private void CreateAxes()
        {
            mTempModel.Axes.Add(new LinearAxis
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
                Title = "Value ------->",
                AxisTitleDistance = 0
            });

            mTempModel.Axes.Add(new DateTimeAxis
            {
                Title = "Date ------>",
                Position = AxisPosition.Bottom,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.WhiteSmoke,
                AxisTitleDistance = 0,
                StringFormat = "MMM-dd",
                MajorGridlineStyle = LineStyle.Solid,
                AxislineThickness = 3,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255)
            });
            var l = new Legend
            {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendTextColor = OxyColors.White,
                LegendMargin = 0
            };

            mTempModel.Legends.Add(l);
            mTempModel.TitlePadding = 3;
        }

        /// <summary>
        /// Fills the color list.
        /// </summary>
        private void FillColorList()
        {
            dictOxyColor = new Dictionary<string, OxyColor>();
            dictOxyColor.Add("Forecast", OxyColors.Red);
            dictOxyColor.Add("Actual", OxyColors.Yellow);
            dictOxyColor.Add("Potential", OxyColors.Green);
            dictOxyColor.Add("SActual", OxyColors.Blue);
            dictOxyColor.Add("SPotential", OxyColors.Maroon);
            dictOxyColor.Add("SForecast", OxyColors.DarkGoldenrod);
        }

        /// <summary>
        /// Closes the and unsubscribe.
        /// </summary>
        private void CloseAndUnsubscribe()
        {
            if (windHBTimer != null)
            {
                windHBTimer.Enabled = false;
                windHBTimer.Close();
                windHBTimer = null;
            }

            if (windReconnectTimer != null)
            {
                windReconnectTimer.Enabled = false;
                windReconnectTimer.Close();
                windReconnectTimer = null;
            }

            if (mWindServer != null)
            {
                try
                {
                    mWindServer.Unsubscribe();
                    mWindServer = null;
                }
                catch { }
            }
        }

        /// <summary>
        /// Gets the latest wind data.
        /// </summary>
        private void GetLatestWindData()
        {
            try
            {
                if (PowerTypeSelectedItem == "Wind Power")
                {
                    if (mFactoryWind == null || mWindServer == null)
                    {
                        GetFactoryObject();
                    }
                    List<LatestWindData> WindData = mWindServer.GetLatestWindData(FromSelectedDate, EndSelectedDate, GetMarketKey(ZoneSelectedItem.ToString()));

                    SetLatestWindData(WindData);
                    mWindServer.Subscribe(FromSelectedDate, EndSelectedDate);

                    if (windHBTimer == null)
                    {
                        windHBTimer = new System.Timers.Timer(180000);
                        windHBTimer.Elapsed += new System.Timers.ElapsedEventHandler(windHBTimer_Elapsed);
                    }

                    windHBTimer.Enabled = true;

                    if (windReconnectTimer != null)
                    {
                        windReconnectTimer.Enabled = false;
                        windReconnectTimer.Close();
                        windReconnectTimer = null;
                    }
                }
                else
                {
                    mSolarList = new List<LatestWindData>();
                    objDataService.GetSolarData((item, ex) =>
                    {
                        mSolarList = item;
                    }, FromSelectedDate, EndSelectedDate, " ");

                    SetLatestWindData(mSolarList.ToList());
                }
            }
            catch (EndpointNotFoundException)
            {
                try
                {
                    if (windHBTimer != null)
                    {
                        windHBTimer.Enabled = false;
                        windHBTimer.Close();
                        windHBTimer = null;
                    }

                    if (windReconnectTimer == null)
                    {
                        windReconnectTimer = new System.Timers.Timer(10000) { Enabled = true };
                        windReconnectTimer.Elapsed += windReconnectTimer_Elapsed;
                    }
                    else
                    {
                        windReconnectTimer.Enabled = true;
                    }
                }
                catch { }
            }
            catch (CommunicationObjectFaultedException)
            {
                mWindServer = null;
                mFactoryWind = null;
            }
            catch { }
        }

        private int GetMarketKey(string market)
        {
            string[] data = market.Split(' ');

            if (data[0] == "ERCOT")
            {
                return 9;
            }

            return 1;
        }

        public void GetMarketlist(string type)
        {
            if (type == "Wind Power")
            {
                List<string> tempZoneList = new List<string>();
                tempZoneList.Add("ERCOT Total");
                tempZoneList.Add("ERCOT Houston");
                tempZoneList.Add("ERCOT West");
                tempZoneList.Add("ERCOT North");

                tempZoneList.Add("ERCOT Panhandle");
                tempZoneList.Add("ERCOT Coastal");
                tempZoneList.Add("ERCOT NorthRegion");
                tempZoneList.Add("ERCOT South");
                tempZoneList.Add("ERCOT WestRegion");
                ZoneList = tempZoneList.ToList();
                ZoneSelectedItem = ZoneList.FirstOrDefault();
            }
            if (type == "Solar Power")
            {
                List<string> tempZoneList = new List<string>();
                tempZoneList.Add("ERCOT Total");
                ZoneList = tempZoneList.ToList();
                ZoneSelectedItem = ZoneList.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the factory object.
        /// </summary>
        private void GetFactoryObject()
        {
            string mEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetWindService();

            NetTcpBinding binding = new NetTcpBinding();
            binding.CloseTimeout = new TimeSpan(0, 12, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 12, 0);
            binding.SendTimeout = new TimeSpan(0, 12, 0);
            binding.OpenTimeout = new TimeSpan(0, 12, 0);
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.Security.Mode = SecurityMode.None;

            mFactoryWind = new DuplexChannelFactory<IWindDataProvider>(new InstanceContext(this), binding, mEndPoint);
            mWindServer = mFactoryWind.CreateChannel();
            mFactoryWind.Faulted += mFactoryWind_Faulted;
        }

        /// <summary>
        /// Draws the graph.
        /// </summary>
        private void DrawGraph()
        {
            PlotDataModel = new PlotModel();
            DateTime from = FromSelectedDate;
            DateTime to = EndSelectedDate;
            if (lstWindDataList == null || lstWindDataList.Count == 0)
            {
                return;
            }

            try
            {
                if (dictOxyColor == null)
                {
                    FillColorList();
                }

                foreach (var item in lstWindDataList.Keys)
                {
                }

                PlotDataModel = mTempModel;
                //PlotDataModel.Update();
            }
            catch { }
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
                case "actual": return ActualChecked;
                case "forecast": return ForecastChecked;
                case "potential": return PotentialChecked;
                case "sactual": return SActualChecked;
                case "spotential": return SPotentialChecked;
                case "sforecast": return SForecastChecked;
                default: return true;
            }
        }

        #endregion

        /// <summary>
        /// Sets the latest wind data.
        /// </summary>
        /// <param name="PJMWindDataList">The PJM wind data list.</param>
        public void SetLatestWindData(List<LatestWindData> PJMWindDataList)
        {
            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (mTempModel == null)
            {
                mTempModel = new PlotModel();
                CreateAxes();
            }

            if (PJMWindDataList.Count > 0)
            {
                if (ActualChecked)
                {
                    FixActual(PJMWindDataList.OrderBy(a => a.MarketDate).Where(a => a.WindType == "Actual").ToList());
                }
                else
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).FirstOrDefault());
                    }
                }

                if (ForecastChecked)
                {
                    FixForecast(PJMWindDataList.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("forecast")).ToList());
                }
                else
                {
                    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).Count() > 0)
                    {
                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).FirstOrDefault());
                    }
                }

                #region Commented Code for Future Use for PotentialChecked, SActualChecked, SPotentialChecked, SForecastChecked
                //if (PotentialChecked)
                //{
                //    FixPotential(PJMWindDataList.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("potential")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).FirstOrDefault());
                //    }
                //}

                //if (SActualChecked)
                //{
                //    FixSActual(PJMWindDataList.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("sactual")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).FirstOrDefault());
                //    }
                //}

                //if (SPotentialChecked)
                //{
                //    FixSPotential(PJMWindDataList.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("spotential")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).FirstOrDefault());
                //    }
                //}

                //if (SForecastChecked)
                //{
                //    FixSForecast(PJMWindDataList.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("sforecast")).ToList());
                //}
                //else
                //{
                //    if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).Count() > 0)
                //    {
                //        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).FirstOrDefault());
                //    }
                //}
                #endregion

                var list = PJMWindDataList.OrderBy(a => a.MarketDate).Where(a => a.WindType.ToLower().Equals("sforecast")).ToList();

                DataHash = PJMWindDataList.ToList();
                PlotDataModel = null;
                PlotDataModel = mTempModel as PlotModel;
            }
            else
            {
                if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).Count() > 0)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("actual")).FirstOrDefault());
                }
                if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).Count() > 0)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("forecast")).FirstOrDefault());
                }
                if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).Count() > 0)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("potential")).FirstOrDefault());
                }
                if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).Count() > 0)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sactual")).FirstOrDefault());
                }
                if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).Count() > 0)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("spotential")).FirstOrDefault());
                }
                if (mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).Count() > 0)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Equals("sforecast")).FirstOrDefault());
                }
            }
        }

        #region Events

        /// <summary>
        /// Handles the Faulted event of the mFactoryWind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void mFactoryWind_Faulted(object sender, EventArgs e)
        {
            windReconnectTimer_Elapsed(null, null);
        }

        /// <summary>
        /// Handles the Elapsed event of the windReconnectTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        void windReconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                windReconnectTimer.Enabled = false;

                if (windHBTimer != null)
                {
                    windHBTimer.Enabled = false;
                    windHBTimer.Close();
                    windReconnectTimer = null;
                }

                if (mFactoryWind == null || mWindServer == null)
                {
                    GetFactoryObject();
                }

                mWindServer.HeartBeat();
            }
            catch
            {
                mWindServer = null;
                GetLatestWindData();
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the windHBTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void windHBTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                windHBTimer.Enabled = false;

                if (mFactoryWind == null || mWindServer == null)
                {
                    GetFactoryObject();
                }

                mWindServer.HeartBeat();

                windHBTimer.Enabled = true;

                if (windReconnectTimer != null)
                {
                    windReconnectTimer.Enabled = false;
                    windReconnectTimer.Close();
                    windReconnectTimer = null;
                }
            }
            catch (Exception)
            {
                try
                {
                    if (windReconnectTimer == null)
                    {
                        windReconnectTimer = new System.Timers.Timer(20000) { Enabled = true };
                        windReconnectTimer.Elapsed += windReconnectTimer_Elapsed;
                    }

                    if (!windReconnectTimer.Enabled)
                    {
                        windReconnectTimer.Enabled = true;
                    }
                }
                catch { }
            }
        }

        #endregion


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
