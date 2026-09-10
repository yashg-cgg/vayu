using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vayu.FuelMix.Model;

namespace Vayu.FuelMix.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        private readonly IDataService _dataService;
        private static readonly object lockObj = new object();
        private static string mUser = Environment.UserName;
        private Dictionary<string, List<LoadDataItem>> mLoadList;
        private Dictionary<string, OxyColor> mOxyColorList;



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
        private string mTrackerFormatString { get; set; }
        public string TrackerFormatString
        {
            get
            {
                return mTrackerFormatString;
            }
            set
            {
                mTrackerFormatString = value; RaisePropertyChanged("TrackerFormatString");
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
        private string sProductSelectedItem;
        public string ProductSelectedItem
        {
            get
            {
                return sProductSelectedItem;
            }
            set
            {
                sProductSelectedItem = value;
                RaisePropertyChanged("ProductSelectedItem");

                if (ProductSelectedItem != null)
                {

                    GetLoadData();
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
        public string[] ProductList { get; set; }

        private bool mPowerStorageChecked;
        public bool PowerStorageChecked
        {
            get
            {
                return mPowerStorageChecked;
            }
            set
            {
                mPowerStorageChecked = value; RaisePropertyChanged("PowerStorageChecked");
                RefreshPlot();
            }
        }
        private bool mHydroChecked;
        public bool HydroChecked
        {
            get
            {
                return mHydroChecked;
            }
            set
            {
                mHydroChecked = value;
                RaisePropertyChanged("HydroChecked");
                RefreshPlot();
            }
        }
        private bool mNaturalGasChecked;
        public bool NaturalGasChecked
        {
            get
            {
                return mNaturalGasChecked;
            }
            set
            {
                mNaturalGasChecked = value;
                RaisePropertyChanged("NaturalGasChecked");
                RefreshPlot();
            }
        }
        private bool mCoalandLigniteChecked;
        public bool CoalandLigniteChecked
        {
            get
            {
                return mCoalandLigniteChecked;
            }
            set
            {
                mCoalandLigniteChecked = value;
                RaisePropertyChanged("CoalandLigniteChecked");
                RefreshPlot();
            }
        }
        private bool mNuclearChecked;
        public bool NuclearChecked
        {
            get
            {
                return mNuclearChecked;
            }
            set
            {
                mNuclearChecked = value;
                RaisePropertyChanged("NuclearChecked");
                RefreshPlot();
            }
        }
        private bool mOtherChecked;
        public bool OtherChecked
        {
            get
            {
                return mOtherChecked;
            }
            set
            {
                mOtherChecked = value;
                RaisePropertyChanged("OtherChecked");
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
        }//TotalChecked
        private bool mWindChecked;
        public bool WindChecked
        {
            get
            {
                return mWindChecked;
            }
            set
            {
                mWindChecked = value;
                RaisePropertyChanged("WindChecked");
                RefreshPlot();
            }
        }
        private bool sTotalChecked;
        public bool TotalChecked
        {
            get
            {
                return sTotalChecked;
            }
            set
            {
                sTotalChecked = value;
                RaisePropertyChanged("TotalChecked");
                RefreshPlot();
            }
        }
        private bool mSolarChecked;
        public bool SolarChecked
        {
            get { return mSolarChecked; }
            set
            {
                mSolarChecked = value;
                RaisePropertyChanged("SolarChecked");
                RefreshPlot();
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
            FromSelectedDate = DateTime.Today.AddDays(-2);
            EndSelectedDate = DateTime.Today.AddDays(0);
            ProductList = new string[] { "Outages", "Renewables" };
            ProductSelectedItem = ProductList.FirstOrDefault();
            RefreshCommand = new DelegateCommand(() => Refresh());
            Task.Factory.StartNew(() => FillColorList());
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

                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
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
            mOxyColorList.Add("Solar", OxyColors.Yellow);
            mOxyColorList.Add("Wind", OxyColors.Blue);
            mOxyColorList.Add("Hydro", OxyColors.DeepSkyBlue);
            mOxyColorList.Add("Power Storage", OxyColors.DarkOrange);
            mOxyColorList.Add("Natural Gas", OxyColors.Green);
            mOxyColorList.Add("Coal and Lignite", OxyColors.Red);
            mOxyColorList.Add("Nuclear", OxyColors.DarkViolet);
            mOxyColorList.Add("Other", OxyColors.Maroon);
            mOxyColorList.Add("Total", OxyColors.SlateGray);

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
            RefreshPlot();
        }
        private void GetLoadData()
        {
            try
            {
                _dataService.GetFuelMixData((item, ex) =>
                {
                    mLoadList = item;
                }, FromSelectedDate, EndSelectedDate, ProductSelectedItem);

                FixLoadData();
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
                        tempModel.Series.Add(CreateSeries(mLoadList[item].OrderBy(j => j.MarketDateTime).ToList(), item));
                    }
                }

                var leg = new Legend
                {
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.RightTop,
                    LegendTextColor = OxyColors.White,
                };
                tempModel.Legends.Add(leg);
                tempModel.TitlePadding = 3;
                PlotDataModel = tempModel;
            }
            catch (Exception ex)
            {
            }
        }

        private bool KeyChecked(string item)
        {
            switch (item.ToLower())
            {

                case "solar": return SolarChecked;
                case "wind": return WindChecked;
                case "hydro": return HydroChecked;
                case "power storage": return PowerStorageChecked;
                case "natural gas": return NaturalGasChecked;
                case "coal and lignite": return CoalandLigniteChecked;
                case "nuclear": return NuclearChecked;
                case "other": return OtherChecked;
                case "total": return TotalChecked;
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
                MarkerStrokeThickness = 0.2,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = key.ToUpper()
            };
        }
        private LineStyle GetLineStyle(string key)
        {
            return LineStyle.Solid;
        }


        public void SetGraph(string Zone, DateTime StartDate, DateTime EndDate, Dictionary<int, Dictionary<int, double>> hourHash, Dictionary<int, double> hour1Hash, Dictionary<int, double> hour2Hash, Dictionary<int, double> hour3Hash, Dictionary<int, double> hour4Hash, Dictionary<int, Dictionary<int, double>> hour5Hash, Dictionary<int, double> hour6Hash, Dictionary<int, double> hour7Hash, Dictionary<int, double> hour8Hash, Dictionary<int, double> hour9Hash, Dictionary<int, double> hour10Hash, Dictionary<int, double> hour11Hash, Dictionary<int, double> hour12Hash, Dictionary<int, double> hour13Hash, Dictionary<int, double> hour14Hash, Dictionary<int, double> hour15Hash)
        {
            if (Zone.ToLower() == ProductSelectedItem.ToLower())
            {
                UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
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

        #endregion

        internal void SetConnectedStatusLoadGraphServer(int proxyIndex, bool bRemoteConnected)
        {
        }

        public class GraphItem
        {
            public DateTime X { get; set; }
            public double? Y { get; set; }
        }
    }
}

