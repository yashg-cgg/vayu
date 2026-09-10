using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vayu.LMP15Mins.Model;
using Vayu.NodePriceLibrary;

namespace Vayu.LMP15Mins.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;

        /// <summary>
        /// The m load list
        /// </summary>
        private List<Load> mLoadList = new List<Load>();
        /// <summary>
        /// The m load forecast list
        /// </summary>
        private List<Load> mLoadForecastList = new List<Load>();
        /// <summary>
        /// The m market
        /// </summary>
        private LoadMarket mMarket = new LoadMarket();

        #region Properties

        /// <summary>
        /// The m constraint contingency list
        /// </summary>
        private List<LMP15MinSourceSink> mLMP15MinList = new List<LMP15MinSourceSink>();
        /// <summary>
        /// Gets or sets the constraint contingency list.
        /// </summary>
        /// <value>
        /// The constraint contingency list.
        /// </value>
        public List<LMP15MinSourceSink> LMP15MinList
        {
            get
            {
                return mLMP15MinList;
            }
            set
            {
                mLMP15MinList = value;
                RaisePropertyChanged("LMP15MinList");
            }
        }
        /// <summary>
        /// The m source sink node list
        /// </summary>
        private List<SourceSinkData> mSourceSinkNodeList = new List<SourceSinkData>();
        /// <summary>
        /// Gets or sets the source sink node list.
        /// </summary>
        /// <value>
        /// The source sink node list.
        /// </value>
        public List<SourceSinkData> SourceSinkNodeList
        {
            get
            {
                return mSourceSinkNodeList;
            }
            set
            {
                mSourceSinkNodeList = value;
                RaisePropertyChanged("SourceSinkNodeList");
            }
        }
        /// <summary>
        /// The m zone loads
        /// </summary>
        private List<ZoneLoads> mZoneLoads = new List<ZoneLoads>();
        /// <summary>
        /// Gets or sets the zone loads.
        /// </summary>
        /// <value>
        /// The zone loads.
        /// </value>
        public List<ZoneLoads> ZoneLoads
        {
            get { return mZoneLoads; }
            set
            {
                mZoneLoads = value;
                RaisePropertyChanged("ZoneLoads");
            }
        }
        /// <summary>
        /// The m plot model
        /// </summary>
        private PlotModel mPlotModel;
        /// <summary>
        /// Gets or sets the plot model.
        /// </summary>
        /// <value>
        /// The plot model.
        /// </value>
        public PlotModel PlotModel
        {
            get
            {
                return mPlotModel;
            }
            set
            {
                mPlotModel = value;
                RaisePropertyChanged("PlotModel");
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
                mStartDate = value.Date;
                RaisePropertyChanged("StartDate");
                //FetchDataAndUpdateChartCommand();
            }
        }
        /// <summary>
        /// The m end date
        /// </summary>
        private DateTime mEndDate;
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate
        {
            get
            {
                return mEndDate;
            }
            set
            {
                mEndDate = value;
                RaisePropertyChanged("EndDate");
            }
        }
        /// <summary>
        /// The m source sink
        /// </summary>
        private SourceSinkData mSourceSink;
        /// <summary>
        /// Gets or sets the source sink.
        /// </summary>
        /// <value>
        /// The source sink.
        /// </value>
        public SourceSinkData SourceSink
        {
            get
            {
                return mSourceSink;
            }
            set
            {
                mSourceSink = value;
                RaisePropertyChanged("SourceSink");
            }
        }
        /// <summary>
        /// The m source cong
        /// </summary>
        private double? mSourceCong;
        /// <summary>
        /// Gets or sets the source cong.
        /// </summary>
        /// <value>
        /// The source cong.
        /// </value>
        public double? SourceCong
        {
            get
            {
                return mSourceCong;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSourceCong = null;
                }
                else
                {
                    mSourceCong = value;
                }
                RaisePropertyChanged("SourceCong");
            }
        }
        /// <summary>
        /// The m sink cong
        /// </summary>
        private double? mSinkCong;
        /// <summary>
        /// Gets or sets the sink cong.
        /// </summary>
        /// <value>
        /// The sink cong.
        /// </value>
        public double? SinkCong
        {
            get
            {
                return mSinkCong;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSinkCong = null;
                }
                else
                {
                    mSinkCong = value;
                }
                RaisePropertyChanged("SinkCong");
            }
        }
        /// <summary>
        /// The m source price
        /// </summary>
        private double? mSourcePrice;
        /// <summary>
        /// Gets or sets the source price.
        /// </summary>
        /// <value>
        /// The source price.
        /// </value>
        public double? SourcePrice
        {
            get
            {
                return mSourcePrice;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSourcePrice = null;
                }
                else
                {
                    mSourcePrice = value;
                }
                RaisePropertyChanged("SourcePrice");
            }
        }
        /// <summary>
        /// The m sink price
        /// </summary>
        private double? mSinkPrice;
        /// <summary>
        /// Gets or sets the sink price.
        /// </summary>
        /// <value>
        /// The sink price.
        /// </value>
        public double? SinkPrice
        {
            get
            {
                return mSinkPrice;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSinkPrice = null;
                }
                else
                {
                    mSinkPrice = value;
                }
                RaisePropertyChanged("SinkPrice");
            }
        }
        /// <summary>
        /// The m hour
        /// </summary>
        private int mHour;
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public int Hour
        {
            get
            {
                return mHour;
            }
            set
            {
                mHour = value;
                RaisePropertyChanged("Hour");
            }
        }
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public int Market { get; set; }
        /// <summary>
        /// The mconstraint
        /// </summary>
        private string mconstraint;
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Constraint
        {
            get
            {
                return mconstraint;
            }
            set
            {
                mconstraint = value;
                RaisePropertyChanged("Constraint");
            }
        }
        /// <summary>
        /// The mSource
        /// </summary>
        private string mSource;
        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        /// <value>
        /// The Source.
        /// </value>
        public string Source
        {
            get
            {
                return mSource;
            }
            set
            {
                mSource = value;
                RaisePropertyChanged("Source");
            }
        }
        /// <summary>
        /// The mSink 
        /// </summary>
        /// <summary>
        /// The mSink 
        /// </summary>
        private string mSink;
        /// <summary>
        /// Gets or sets the Sink .
        /// </summary>
        /// <value>
        /// The Sink .
        /// </value>
        public string Sink
        {
            get
            {
                return mSink;
            }
            set
            {
                mSink = value;
                RaisePropertyChanged("Sink");
            }
        }

        private string mSource15minsLMP;
        public string Source15minsLMP
        {
            get
            {
                return mSource15minsLMP;
            }
            set
            {
                mSource15minsLMP = value;
                RaisePropertyChanged("Source15minsLMP");
            }
        }

        private string mSink15minsLMP;
        public string Sink15minsLMP
        {
            get
            {
                return mSink15minsLMP;
            }
            set
            {
                mSink15minsLMP = value;
                RaisePropertyChanged("Sink15minsLMP");
            }
        }

        private string mSink_Source;
        public string Sink_Source
        {
            get
            {
                return mSink_Source;
            }
            set
            {
                mSink_Source = value;
                RaisePropertyChanged("Sink_Source");
            }
        }
        /// <summary>
        /// The mcontingency
        /// </summary>
        private string mcontingency;
        /// <summary>
        /// Gets or sets the contingency.
        /// </summary>
        /// <value>
        /// The contingency.
        /// </value>
        public string Contingency
        {
            get
            {
                return mcontingency;
            }
            set
            {
                mcontingency = value;
                RaisePropertyChanged("Contingency");
            }
        }
        /// <summary>
        /// Gets or sets the shadow price.
        /// </summary>
        /// <value>
        /// The shadow price.
        /// </value>
        public double ShadowPrice { get; set; }
        /// <summary>
        /// The m maximum dart
        /// </summary>
        private double? mMaxDart;
        /// <summary>
        /// Gets or sets the maximum dart.
        /// </summary>
        /// <value>
        /// The maximum dart.
        /// </value>
        public double? MaxDart
        {
            get
            {
                return mMaxDart;
            }
            set
            {
                mMaxDart = value;
                RaisePropertyChanged("MaxDart");
            }
        }
        /// <summary>
        /// The m minimum dart
        /// </summary>
        private double? mMinDart;
        /// <summary>
        /// Gets or sets the minimum dart.
        /// </summary>
        /// <value>
        /// The minimum dart.
        /// </value>
        public double? MinDart
        {
            get
            {
                return mMinDart;
            }
            set
            {
                mMinDart = value;
                RaisePropertyChanged("MinDart");
            }
        }

        /// <summary>
        /// Gets or sets the run retrieve fetch data and update chart command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data and update chart command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataAndUpdateChartCommand { private set; get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>

        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            mDataService.loadDBCommands();
            RunRetrieveFetchDataAndUpdateChartCommand = new DelegateCommand(FetchDataAndUpdateChartCommand);
        }
        #region Public Methods

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="maxDart">The maximum dart.</param>
        /// <param name="minDart">The minimum dart.</param>
        public void SetData(DateTime startDate, DateTime endDate, int marketKey, SourceSinkData sourceSink)
        {


            StartDate = startDate;
            EndDate = endDate;
            SourceSink = sourceSink;
            SourceSinkNodeList.Add(sourceSink);


            PlotModel = null;

            GetLMP15MinData(startDate, EndDate, sourceSink);

            if (mLoadList.Count > 0)
            {
                RefreshChart();
            }
        }

        /// <summary>
        /// Fetches the data and updates the chart.
        /// </summary>
        public void FetchDataAndUpdateChartCommand()
        {

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the constraint contingency data.
        /// </summary>
        private void GetLMP15MinData(DateTime startDate, DateTime EndDate, SourceSinkData sourceSink)
        {
            if (StartDate == null)
            {
                return;
            }
            mDataService.GetLMP15MinDataa((LMP15Minlist, error) =>
            {
                if (error != null)
                {
                    return;
                }
                LMP15MinList = LMP15Minlist;
            }, startDate, EndDate, 9, sourceSink);
        }

        /// <summary>
        /// Gets the load data.
        /// </summary>
        private void GetLoadData()
        {
            if (StartDate == null)
            {
                return;
            }
            if (EndDate < StartDate)
            {
                return;
            }
            mDataService.GetLoadGraphData((loadList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                mLoadList = loadList;
            }, StartDate, EndDate, Market, mMarket.LoadKey);

            //mDataService.GetLoadForecastGraphData((loadList, error) =>
            //{
            //    if (error != null)
            //    {
            //        return;
            //    }
            //    mLoadForecastList = loadList;
            //}, StartDate.AddDays(1), EndDate.AddDays(1), mMarket.MarketKey, mMarket.LoadForecast);

            //mDataService.GetHourlyZoneLoads((loadList, error) =>
            //{
            //    if (error != null)
            //    {
            //        return;
            //    }
            //    ZoneLoads = loadList;
            //}, StartDate.AddHours(Hour), mMarket.MarketKey);
        }

        /// <summary>
        /// Refreshes the chart.
        /// </summary>
        private void RefreshChart()
        {
            PlotModel = CreatePlotModel();
        }

        /// <summary>
        /// Creates the plot model.
        /// </summary>
        /// <returns></returns>
        private PlotModel CreatePlotModel()
        {
            var plotModel = new PlotModel();
            var c = OxyColors.DarkBlue;
            plotModel.Title = mMarket.LoadName + "(" + SourceSink.Source + " -> " + SourceSink.Sink + ") (" + StartDate.Date.ToString("MM/dd/yyyy") + ")";
            plotModel.Axes.Add(new LinearAxis//(AxisPosition.Left)
            {
                Key = "YAxis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                StartPosition = 0,
                EndPosition = 1,
                Title = "MW"
            });
            plotModel.Axes.Add(new LinearAxis() //AxisPosition.Bottom
            {
                Title = "Hour",

            });

            var dataItemValues = new Collection<Item>();
            var dataItemForecastValues = new Collection<Item>();

            for (int i = 0, j = 0, k = 0; i < 24; i++)
            {
                if (mLoadForecastList.Count > k)
                {
                    if (mLoadForecastList[k].Hour == i)
                    {
                        Item forecast = new Item();
                        forecast.X = mLoadForecastList[k].Hour;
                        forecast.Y = mLoadForecastList[k].MegaWatts;
                        dataItemForecastValues.Add(forecast);
                        k++;
                    }
                }
                if (mLoadList.Count > j)
                {
                    if (mLoadList[j].Hour == i)
                    {
                        Item load = new Item();
                        load.X = mLoadList[j].Hour;
                        load.Y = mLoadList[j].MegaWatts;
                        dataItemValues.Add(load);
                        j++;
                    }
                }
            }
            var lineSeries1 = new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = dataItemForecastValues,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerStrokeThickness = 1,
                MarkerStroke = OxyColors.Black,
                Title = "Forecast"
            };
            var lineSeries2 = new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = dataItemValues,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerStrokeThickness = 1,
                Title = "Load",
                Color = OxyColors.Yellow,
                MarkerFill = OxyColors.Yellow,
                MarkerStroke = OxyColors.Black
            };

            plotModel.Series.Add(lineSeries1);
            plotModel.Series.Add(lineSeries2);
            return plotModel;
        }

        /// <summary>
        /// Gets the source sink prices.
        /// </summary>
        private void GetSourceSinkPrices()
        {
            List<Node> dalist = new List<Node>();
            List<Node> rtlist = new List<Node>();

            mDataService.GetSourceSinkHourlyPrices((rtloadList, daloadList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                rtlist = rtloadList;
                dalist = daloadList;
            }, StartDate, Hour, SourceSink);
            if (SourceSink.Sink != null)
            {
                if (rtlist[0].LmpTimePriceList[0].Lmp != null && dalist[0].LmpTimePriceList[0].Lmp != null &&
                    rtlist[1].LmpTimePriceList[0].Lmp != null && dalist[1].LmpTimePriceList[0].Lmp != null)
                {
                    SourceCong = rtlist[0].LmpTimePriceList[0].Lmp.Congestion - dalist[0].LmpTimePriceList[0].Lmp.Congestion;
                    SinkCong = rtlist[1].LmpTimePriceList[0].Lmp.Congestion - dalist[1].LmpTimePriceList[0].Lmp.Congestion;
                    SourcePrice = rtlist[0].LmpTimePriceList[0].Lmp.Price - dalist[0].LmpTimePriceList[0].Lmp.Price;
                    SinkPrice = rtlist[1].LmpTimePriceList[0].Lmp.Price - dalist[1].LmpTimePriceList[0].Lmp.Price;
                }
            }
            else
            {
                if (rtlist[0].LmpTimePriceList[0].Lmp != null && dalist[0].LmpTimePriceList[0].Lmp != null)
                {
                    SourceCong = rtlist[0].LmpTimePriceList[0].Lmp.Congestion - dalist[0].LmpTimePriceList[0].Lmp.Congestion;
                    SourcePrice = rtlist[0].LmpTimePriceList[0].Lmp.Price - dalist[0].LmpTimePriceList[0].Lmp.Price;
                }
            }
        }

        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int? X { get; set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double? Y { get; set; }
    }
}
