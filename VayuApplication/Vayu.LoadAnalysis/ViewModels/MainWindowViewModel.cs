using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vayu.LoadAnalysis.Models;
using Vayu.NodePriceLibrary;

namespace Vayu.LoadAnalysis.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// The   data service
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// The   load list
        /// </summary>
        private List<Load> _loadList = new List<Load>();
        /// <summary>
        /// The  load forecast list
        /// </summary>
        private List<Load> _loadForecastList = new List<Load>();
        /// <summary>
        /// The   market
        /// </summary>
        private LoadMarket _market = new LoadMarket();

        #region Properties

        /// <summary>
        /// The   constraint contingency list
        /// </summary>
        private List<ConstraintContingency> _constraintContingencyList = new List<ConstraintContingency>();
        /// <summary>
        /// Gets or sets the constraint contingency list.
        /// </summary>
        /// <value>
        /// The constraint contingency list.
        /// </value>
        public List<ConstraintContingency> ConstraintContingencyList
        {
            get
            {
                return _constraintContingencyList;
            }
            set
            {
                _constraintContingencyList = value;
                RaisePropertyChanged("ConstraintContingencyList");
            }
        }
        /// <summary>
        /// The m source sink node list
        /// </summary>
        private List<SourceSinkData> _sourceSinkNodeList = new List<SourceSinkData>();
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
                return _sourceSinkNodeList;
            }
            set
            {
                _sourceSinkNodeList = value;
                RaisePropertyChanged("SourceSinkNodeList");
            }
        }
        /// <summary>
        /// The m zone loads
        /// </summary>
        private List<ZoneLoads> _zoneLoads = new List<ZoneLoads>();
        /// <summary>
        /// Gets or sets the zone loads.
        /// </summary>
        /// <value>
        /// The zone loads.
        /// </value>
        public List<ZoneLoads> ZoneLoads
        {
            get { return _zoneLoads; }
            set
            {
                _zoneLoads = value;
                RaisePropertyChanged("ZoneLoads");
            }
        }
        /// <summary>
        /// The m plot model
        /// </summary>
        private PlotModel _plotModel;
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
                return _plotModel;
            }
            set
            {
                _plotModel = value;
                RaisePropertyChanged("PlotModel");
            }
        }
        /// <summary>
        /// The m start date
        /// </summary>
        private DateTime _startDate;
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
                return _startDate;
            }
            set
            {
                _startDate = value.Date;
                RaisePropertyChanged("StartDate");
                //FetchDataAndUpdateChartCommand();
            }
        }
        /// <summary>
        /// The m end date
        /// </summary>
        private DateTime _endDate;
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
                return _endDate;
            }
            set
            {
                _endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }
        /// <summary>
        /// The m source sink
        /// </summary>
        private SourceSinkData _sourceSink;
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
                return _sourceSink;
            }
            set
            {
                _sourceSink = value;
                RaisePropertyChanged("SourceSink");
            }
        }
        /// <summary>
        /// The m source cong
        /// </summary>
        private double? _sourceCong;
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
                return _sourceCong;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    _sourceCong = null;
                }
                else
                {
                    _sourceCong = value;
                }
                RaisePropertyChanged("SourceCong");
            }
        }
        /// <summary>
        /// The m sink cong
        /// </summary>
        private double? _sinkCong;
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
                return _sinkCong;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    _sinkCong = null;
                }
                else
                {
                    _sinkCong = value;
                }
                RaisePropertyChanged("SinkCong");
            }
        }
        /// <summary>
        /// The m source price
        /// </summary>
        private double? _sourcePrice;
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
                return _sourcePrice;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    _sourcePrice = null;
                }
                else
                {
                    _sourcePrice = value;
                }
                RaisePropertyChanged("SourcePrice");
            }
        }
        /// <summary>
        /// The m sink price
        /// </summary>
        private double? _sinkPrice;
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
                return _sinkPrice;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    _sinkPrice = null;
                }
                else
                {
                    _sinkPrice = value;
                }
                RaisePropertyChanged("SinkPrice");
            }
        }
        /// <summary>
        /// The m hour
        /// </summary>
        private int _hour;
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
                return _hour;
            }
            set
            {
                _hour = value;
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
        private string _constraint;
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
                return _constraint;
            }
            set
            {
                _constraint = value;
                RaisePropertyChanged("Constraint");
            }
        }
        /// <summary>
        /// The mcontingency
        /// </summary>
        private string _contingency;
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
                return _contingency;
            }
            set
            {
                _contingency = value;
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
        private double? _maxDart;
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
                return _maxDart;
            }
            set
            {
                _maxDart = value;
                RaisePropertyChanged("MaxDart");
            }
        }
        /// <summary>
        /// The   minimum dart
        /// </summary>
        private double? _minDart;
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
                return _minDart;
            }
            set
            {
                _minDart = value;
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
        /// <param name="dataService">The data service.</param

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.loadDBCommands();
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
        public void SetData(DateTime startDate, DateTime endDate, int hour, int marketKey, SourceSinkData sourceSink, double? maxDart, double? minDart)
        {
            switch (marketKey)
            {

                case 9:
                    _market.MarketKey = marketKey;
                    _market.MarketName = "ERCOT";
                    _market.LoadKey = 2213;
                    _market.LoadForecast = 34;
                    _market.LoadName = "ERCOT Total";
                    break;

            }

            StartDate = startDate;
            EndDate = endDate;
            Hour = hour;
            SourceSink = sourceSink;
            SourceSinkNodeList.Add(sourceSink);
            MaxDart = maxDart;
            MinDart = minDart;
            Market = marketKey;

            FetchDataAndUpdateChartCommand();
        }

        /// <summary>
        /// Fetches the data and updates the chart.
        /// </summary>
        public void FetchDataAndUpdateChartCommand()
        {
            PlotModel = null;
            GetLoadData();
            GetConstraintContingencyData();
            GetSourceSinkPrices();
            if (_loadList.Count > 0)
            {
                RefreshChart();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the constraint contingency data.
        /// </summary>
        private void GetConstraintContingencyData()
        {
            if (StartDate == null)
            {
                return;
            }
            _dataService.GetConstraintContingencyData((constraintContingencyList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                ConstraintContingencyList = constraintContingencyList;
            }, StartDate.Date.AddHours(Hour), StartDate.Date.AddHours(Hour + 1), _market.MarketKey);
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
            _dataService.GetLoadGraphData((loadList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                _loadList = loadList;
            }, StartDate, EndDate, Market, _market.LoadKey);

            _dataService.GetLoadForecastGraphData((loadList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                _loadForecastList = loadList;
            }, StartDate.AddDays(1), EndDate.AddDays(1), _market.MarketKey, _market.LoadForecast);

            _dataService.GetHourlyZoneLoads((loadList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                ZoneLoads = loadList;
            }, StartDate.AddHours(Hour), _market.MarketKey);
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
            plotModel.Title = _market.LoadName + "(" + SourceSink.Source + " -> " + SourceSink.Sink + ") (" + StartDate.Date.ToString("MM/dd/yyyy") + ")";
            plotModel.Axes.Add(new LinearAxis
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
            plotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Hour"
            });

            var dataItemValues = new Collection<Item>();
            var dataItemForecastValues = new Collection<Item>();

            for (int i = 0, j = 0, k = 0; i < 24; i++)
            {
                if (_loadForecastList.Count > k)
                {
                    if (_loadForecastList[k].Hour == i)
                    {
                        Item forecast = new Item();
                        forecast.X = _loadForecastList[k].Hour;
                        forecast.Y = _loadForecastList[k].MegaWatts;
                        dataItemForecastValues.Add(forecast);
                        k++;
                    }
                }
                if (_loadList.Count > j)
                {
                    if (_loadList[j].Hour == i)
                    {
                        Item load = new Item();
                        load.X = _loadList[j].Hour;
                        load.Y = _loadList[j].MegaWatts;
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
            var l = new Legend
            {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendTextColor = OxyColors.Black,

            };

            plotModel.Series.Add(lineSeries1);
            plotModel.Series.Add(lineSeries2);
            plotModel.Legends.Add(l);
            return plotModel;
        }

        /// <summary>
        /// Gets the source sink prices.
        /// </summary>
        private void GetSourceSinkPrices()
        {
            List<Node> dalist = new List<Node>();
            List<Node> rtlist = new List<Node>();

            _dataService.GetSourceSinkHourlyPrices((rtloadList, daloadList, error) =>
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
    /// Item class
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
