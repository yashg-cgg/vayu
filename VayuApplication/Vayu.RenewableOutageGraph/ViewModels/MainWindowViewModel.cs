using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.DBLibrary;
using Vayu.RenewableOutageGraph.Model;

namespace Vayu.RenewableOutageGraph.ViewModels
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
        public DelegateCommand RunRefreshCommand { private set; get; }
        public List<MWPlotData> RenewablePlotList = new List<MWPlotData>();
        private Dictionary<string, List<PlotData>> mTemperatureAllList = new Dictionary<string, List<PlotData>>();
        private Dictionary<string, OxyColor> mOxyColorList;
        public List<String> SourceNodeListField = new List<String>();
        private PlotModel mTempModel;
        private PlotModel mRenewableTempModel;
        private List<String> mISOMarketList;
        public List<String> ISOMarketList
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
        private DateTime mStartDate;
        public DateTime StartDate
        {
            get
            {
                return mStartDate;
            }
            set
            {
                mStartDate = value.Date;

                RaisePropertyChanged("StartDate");
            }
        }
        private DateTime mEndDate;
        public DateTime Enddate
        {
            get
            {
                return mEndDate;
            }
            set
            {
                mEndDate = value.Date;

                RaisePropertyChanged("Enddate");
            }
        }
        private OxyPlot.PlotModel plotDataModel;
        public OxyPlot.PlotModel PlotDataModel
        {
            get { return plotDataModel; }
            set { plotDataModel = value; RaisePropertyChanged("PlotDataModel"); }
        }
        private bool mRadioCurrentChecked = true;
        public bool RadioCurrentChecked
        {
            get
            {
                return mRadioCurrentChecked;
            }
            set
            {
                mRadioCurrentChecked = value;
                RaisePropertyChanged("RadioCurrentChecked");
            }
        }
        private List<RenewableOutage> mRenewableOutageList;
        public List<RenewableOutage> RenewableOutageList
        {
            get
            {
                return mRenewableOutageList;
            }
            set
            {
                //SetComboBox();
                mRenewableOutageList = value;
                RaisePropertyChanged("RenewableOutageList");
            }
        }
        private string mSelectedCityValue;
        public string SelectedCityValue
        {
            get
            {
                return mSelectedCityValue;
            }
            set
            {

                mSelectedCityValue = value;
                mTemperatureAllList.Clear();
                if (mTempModel != null)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains("Avg".ToLower())).FirstOrDefault());
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains("Min".ToLower())).FirstOrDefault());
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains("Max".ToLower())).FirstOrDefault());
                }

                RaisePropertyChanged("SelectedCityValue");
                //SetZonesBox();
            }
        }
        private string marketComboSelectedValue;
        public string MarketComboSelectedValue
        {
            get
            {
                return marketComboSelectedValue;
            }
            set
            {
                marketComboSelectedValue = value;
                SetNodeList("UPTO");//9 for ERCOT

                RaisePropertyChanged("MarketComboSelectedValue");

            }
        }
        private List<String> mSourceNodeList;

        public List<String> SourceNodeList
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

        public MainWindowViewModel(IDataService dataService)
        {
            ISOMarketList = new List<string> { "ERCOT" };

            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            Enddate = DateTime.Today;
            SelectedCityValue = "Allentown";
            _dataService = dataService;
            MarketComboSelectedValue = "ERCOT";
            RunRefreshCommand = new DelegateCommand(Refresh);

        }
        private void Refresh()
        {
            bool current = true;
            if (RadioCurrentChecked)
                current = true;
            else
                current = false;


            RenewableOutageList = _dataService.GetRenewableOutage(StartDate, Enddate);
            SetRenewableResoursePlotList();
        }
        private void RefreshHourlyPlot()
        {
            mRenewableTempModel = null;
            if (RenewablePlotList != null || RenewablePlotList.Count != 0)
            {
                //lock (lockObj)
                {
                    try
                    {
                        if (mRenewableTempModel == null)
                        {
                            mRenewableTempModel = new PlotModel();
                            CreateAxes();
                        }

                        try
                        {
                            if (mOxyColorList == null)
                                FillColorList();
                            //foreach(var unit in HourlyPlotList)
                            //{
                            if (mRenewableTempModel.Series.Count > 0)
                            {
                                mRenewableTempModel.Series.Remove(mRenewableTempModel.Series.FirstOrDefault());
                            }

                            mRenewableTempModel.Series.Add(CreateRenewableSeries(RenewablePlotList.OrderBy(j => j.Date).ToList()));

                            PlotDataModel = null;
                            PlotDataModel = mRenewableTempModel as PlotModel;
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
        private void SetRenewableResoursePlotList()
        {
            if (mRenewableOutageList != null)
            {
                RenewablePlotList.Clear();
                var list = mRenewableOutageList;
                foreach (var item in list)
                {
                    MWPlotData pd = new MWPlotData();
                    pd.Date = item.Date;
                    pd.MW = item.MW;
                    RenewablePlotList.Add(pd);
                }
                RenewablePlotList = RenewablePlotList.OrderBy(x => x.Date).ToList();


                RefreshHourlyPlot();
            }
        }
        private void CreateAxes()
        {
            mRenewableTempModel.Axes.Add(new LinearAxis
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
                Title = "'MW ----->",
                AxisTitleDistance = 0
            });
            mRenewableTempModel.Axes.Add(new DateTimeAxis
            {
                Title = "Market Date ---->",
                Position = AxisPosition.Bottom,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.WhiteSmoke,
                AxisTitleDistance = 0,
                StringFormat = "MMM-dd\nHH:mm",
                MajorGridlineStyle = LineStyle.Solid,
                AxislineThickness = 3,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
            });

            var l = new Legend
            {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.BottomLeft,
                LegendTextColor = OxyColors.White,
                LegendMargin = 0
            };

            mRenewableTempModel.Legends.Add(l);
            mRenewableTempModel.TitlePadding = 3;

        }
        private Series CreateRenewableSeries(List<MWPlotData> list)
        {
            //list.RemoveAll(a => a.Temperature == 0);
            List<GraphItem> grpList = new List<GraphItem>();
            try
            {
                list.ForEach(dItem =>
                {
                    grpList.Add(new GraphItem
                    {
                        X = dItem.Date,
                        Y = dItem.MW
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
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:###MW}" /*"{0}\n{X:MMM:dd}\n{Y:###}"*/,
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.5,
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid, //GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = "RT --->" + " "
            };
        }
        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("Max", OxyColors.DarkBlue);
            mOxyColorList.Add("Min", OxyColors.DarkOrange);
            mOxyColorList.Add("Avg", OxyColors.DarkRed);
            mOxyColorList.Add("General", OxyColors.Green);
        }

        public void SetNodeList(string product)
        {
            SourceNodeListField.Clear();
            DBAccess.GetSourceSinkNodeList(
                    (item1, error) =>
                    {
                        foreach (var i in item1.Item1)
                        {
                            SourceNodeListField.Add(i.NodeName);
                        }
                    }, MarketComboSelectedValue, product);
            SourceNodeList = SourceNodeListField;
        }

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
    public class PlotData
    {
        public DateTime Date { get; set; }
        public string City { get; set; }
        public int? Temperature { get; set; }
    }
    public class MWPlotData
    {
        public DateTime Date { get; set; }
        public double? MW { get; set; }

    }

    public class DataGridBackColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush();
            double number;

            if (value != null)
            {
                if (value.ToString().Contains("($"))
                {
                    mybrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    double.TryParse(value.ToString(), out number);
                    if (number == 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number > 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number < 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                }

                if (value.ToString().Contains("SELL"))
                {
                    mybrush = new SolidColorBrush(Colors.Blue);
                }
                if (value.ToString().Contains("BUY"))
                {
                    mybrush = new SolidColorBrush(Colors.Black);
                }

            }
            return mybrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ValueToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            double doubleValue = 0.0;
            double.TryParse(value.ToString(), out doubleValue);
            if (doubleValue < 0)
            {
                brush = new SolidColorBrush(Colors.Red);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
