using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.DBLibrary;
using Vayu.HourlyTemp_Grpah.Model;

namespace Vayu.HourlyTemp_Grpah.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        //public GalaSoft.MvvmLight.Command.RelayCommand RefreshTempData { get; private set; }
        public List<Tuple<string, string>> CityIcaoCodeList = new List<Tuple<string, string>>();
        public List<string> CityIcaoCodeList1 = new List<string>();
        public Dictionary<int, string> dictNodeCountyHash = new Dictionary<int, string>();
        public DelegateCommand RunExportCSVCommand { private set; get; }
        public DelegateCommand RunRefreshCommand { private set; get; }
        private Dictionary<string, List<PlotData>> mTemperatureAllList = new Dictionary<string, List<PlotData>>();
        private List<PlotData> mMinTemperatureallList = new List<PlotData>();
        private List<PlotData> mMaxTemperatureallList = new List<PlotData>();
        private List<PlotData> mAVGTemperatureallList = new List<PlotData>();
        public List<NodeDetail> mSelectNodeList = new List<NodeDetail>();
        private Dictionary<string, List<PlotData>> mTemperatureHourlyList = new Dictionary<string, List<PlotData>>();
        public List<PlotData> HourlyPlotList = new List<PlotData>();
        public List<String> SourceNodeListField = new List<String>();
        private Dictionary<string, OxyColor> mOxyColorList;
        private PlotModel mTempModel;
        private List<String> mISOMarketList;
        /// <summary>
        /// The m source node list
        /// </summary>
        private List<String> mSourceNodeList;
        /// <summary>
        /// Gets or sets the source node list.
        /// </summary>
        /// <value>
        /// The source node list.
        /// </value>
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
        /// <summary>
        /// The m source node list
        /// </summary>
        private List<PricingNode> mSinkNodeList;
        /// <summary>
        /// Gets or sets the source node list.
        /// </summary>
        /// <value>
        /// The source node list.
        /// </value>
        public List<PricingNode> SinkNodeList
        {
            get
            {
                return mSinkNodeList;
            }
            set
            {
                mSinkNodeList = value;
                RaisePropertyChanged("SinkNodeList");
            }
        }
        /// <summary>
        /// Gets or sets the source node list.
        /// </summary>
        /// <value>
        /// The source node list.
        /// </value>
        public List<NodeDetail> SelectNodeList
        {
            get
            {
                return mSelectNodeList;
            }
            set
            {
                mSelectNodeList = value;
                RaisePropertyChanged("SelectNodeList");
            }
        }
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
        private List<CityZones> mListCityZones;
        public List<CityZones> ListCityZones
        {
            get
            {
                return mListCityZones;
            }
            set
            {
                mListCityZones = value;
                RaisePropertyChanged("ListCityZones");
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
                GetCity();
                RaisePropertyChanged("MarketComboSelectedValue");

            }
        }
        private List<string> mSelectedZoneValue;
        public List<string> SelectedZoneValue
        {
            get
            {
                return mSelectedZoneValue;
            }
            set
            {

                mSelectedZoneValue = value;
                RaisePropertyChanged("SelectedZoneValue");
                //SetCityBox();
            }
        }


        private string mSelectedNodeValue;
        public string SelectedNodeValue
        {
            get
            {
                return mSelectedNodeValue;
            }
            set
            {
                mSelectedNodeValue = value;
                PricingNode node = DBAccess.GetNodeFromName(mSelectedNodeValue, 9);

                mTemperatureAllList.Clear();
                if (mTempModel != null)
                {
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains("Avg".ToLower())).FirstOrDefault());
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains("Min".ToLower())).FirstOrDefault());
                    mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains("Max".ToLower())).FirstOrDefault());
                }
                if (dictNodeCountyHash.ContainsKey(node.NodeKey))
                {
                    SelectedCityValue = dictNodeCountyHash[node.NodeKey];
                }
                else
                {
                    SelectedCityValue = "";
                }

                //SetMinMaxAvg();
                RaisePropertyChanged("SelectedNodeValue");
                //SetZonesBox();
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

        private List<HourlyTemperatureData> mHourlyTemperatureList;
        public List<HourlyTemperatureData> HourlyTemperatureList
        {
            get
            {
                return mHourlyTemperatureList;
            }
            set
            {
                //SetComboBox();
                mHourlyTemperatureList = value;
                RaisePropertyChanged("HourlyTemperatureList");
            }
        }
        private List<String> mSelectCityList;
        public List<String> SelectCityList
        {
            get
            {
                return mSelectCityList;
            }
            set
            {
                //SetComboBox();
                mSelectCityList = value;
                RaisePropertyChanged("SelectCityList");
            }
        }

        private List<String> mSelectzoneList;
        public List<String> SelectzoneList
        {
            get
            {
                return mSelectzoneList;
            }
            set
            {
                //SetComboBox();
                mSelectzoneList = value;
                RaisePropertyChanged("SelectzoneList");
            }
        }
        private bool mRadioMaxChecked;
        public bool RadioMaxChecked
        {
            get
            {
                return mRadioMaxChecked;
            }
            set
            {
                mRadioMaxChecked = value;
                SetTemperature();
                RaisePropertyChanged("RadioMaxChecked");
            }
        }
        private bool mRadioMinChecked;
        public bool RadioMinChecked
        {
            get
            {
                return mRadioMinChecked;
            }
            set
            {
                mRadioMinChecked = value;
                SetTemperature();
                RaisePropertyChanged("RadioMinChecked");
            }
        }

        private bool mRadioAvgChecked;
        public bool RadioAvgChecked
        {
            get
            {
                return mRadioAvgChecked;
            }
            set
            {
                mRadioAvgChecked = value;
                SetTemperature();
                RaisePropertyChanged("RadioAvgChecked");
            }
        }
        private bool mRadioAllChecked = true;
        public bool RadioAllChecked
        {
            get
            {
                return mRadioAllChecked;
            }
            set
            {
                mRadioAllChecked = value;
                RaisePropertyChanged("RadioAllChecked");
            }
        }
        private bool mMaxChecked = true;
        public bool MaxChecked
        {
            get { return mMaxChecked; }
            set
            {

                mMaxChecked = value;
                RaisePropertyChanged("MaxChecked");
            }
        }
        private bool mMinChecked = true;
        public bool MinChecked
        {
            get { return mMinChecked; }
            set
            {
                mMinChecked = value;
                RaisePropertyChanged("MinChecked");
            }
        }
        private bool mAvgChecked = true;
        public bool AvgChecked
        {
            get { return mAvgChecked; }
            set
            {
                mAvgChecked = value;
                RaisePropertyChanged("AvgChecked");
            }
        }
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
        private IEnumerable<TemperatureData> mAllTemperatureList;
        /// <summary>
        /// Gets or sets the summary data list.
        /// </summary>
        /// <value>
        /// The summary data list.
        /// </value>

        public IEnumerable<TemperatureData> AllTemperatureList
        {
            get
            {
                return mAllTemperatureList;
            }
            set
            {
                mAllTemperatureList = value;
                RaisePropertyChanged("AllTemperatureList");
            }
        }
        private List<Temperature> mTemperatureList;
        public List<Temperature> TemperatureList
        {
            get
            {
                return mTemperatureList;
            }
            set
            {
                mTemperatureList = value;
                RaisePropertyChanged("TemperatureList");
            }
        }
        private List<Temperature> mMinTemperatureList;
        public List<Temperature> MinTemperatureList
        {
            get
            {
                return mMinTemperatureList;
            }
            set
            {
                mMinTemperatureList = value;
                RaisePropertyChanged("MinTemperatureList");
            }
        }
        private List<Temperature> mMaxTemperatureList;
        public List<Temperature> MaxTemperatureList
        {
            get
            {
                return mMaxTemperatureList;
            }
            set
            {
                mMaxTemperatureList = value;
                RaisePropertyChanged("MaxTemperatureList");
            }
        }
        private List<Temperature> mAvgTemperatureList;
        public List<Temperature> AvgTemperatureList
        {
            get
            {
                return mAvgTemperatureList;
            }
            set
            {
                mAvgTemperatureList = value;
                RaisePropertyChanged("AvgTemperatureList");
            }
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
                SetTemperature();
                RaisePropertyChanged("RadioCurrentChecked");
            }
        }
        private bool mRadioForecastChecked;
        public bool RadioForecastChecked
        {
            get
            {
                return mRadioForecastChecked;
            }
            set
            {
                mRadioForecastChecked = value;
                SetTemperature();
                RaisePropertyChanged("RadioForecastChecked");
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
            CityIcaoCodeList = _dataService.GetCityIcaoCodeList(MarketComboSelectedValue);
            CityIcaoCodeList1 = _dataService.GetCityIcaoCodeList1("ERCOT");
            dictNodeCountyHash = _dataService.GetCountyHashByNodeKey();
            RunRefreshCommand = new DelegateCommand(Refresh);
            RunExportCSVCommand = new DelegateCommand(ExportToCSVCommand);
        }

        public void SetPreInputs(PricingNode node)
        {
            try
            {
                SelectedNodeValue = node.NodeName;
                SelectedCityValue = dictNodeCountyHash[node.NodeKey];
                Refresh();
            }
            catch (Exception)
            {


            }
        }




        //private void PrepareNodes(int marketKey)
        //{
        //    SelectNodeList = null;
        //    Dictionary<int, List<string>> nodeTypeHash = new Dictionary<int, List<string>>();
        //    Dictionary<int, List<string>> zoneHash = new Dictionary<int, List<string>>();
        //    Dictionary<string, NodeDetail> nodeHash = DBAccess.GetAllNodes(marketKey, nodeTypeHash, zoneHash);
        //    SelectNodeList = nodeHash.Values.ToList();
        //}

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
        public void GetCity()
        {
            _dataService.GetCityZoneNames((item, ex) =>
            {
                if (ex == null)
                {
                    ListCityZones = item;
                }
            }, MarketComboSelectedValue.ToString());
            SelectCityList = ListCityZones.Select(x => x.City).ToList();
            if (MarketComboSelectedValue == "PJM")
                SelectedCityValue = "Allentown";
            else
                SelectedCityValue = "Amarillo";
        }

        /// <summary>
        /// Gets all node releated data on the basis of market key.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="nodeTypeHash">The node type hash.</param>
        /// <param name="zoneHash">The zone hash.</param>
        /// <returns></returns>

        private void Refresh()
        {
            bool current = true;
            if (RadioCurrentChecked)
                current = true;
            else
                current = false;

            if (CityIcaoCodeList1.Contains(SelectedCityValue))
            {
                string icaocode = CityIcaoCodeList.Where(x => x.Item1 == SelectedCityValue).FirstOrDefault().Item2;
                _dataService.GetAllTemperatureDataList((item1, error) =>
                {
                    if (error != null)
                    {
                        return;
                    }
                    HourlyTemperatureList = item1;
                }, StartDate, Enddate, current, icaocode, MarketComboSelectedValue.ToString());


                SetTemperature();
                FillColorList();
                SetMinMaxAvg();
                SetHourlyPlot();
            }
            else
            {
                MessageBox.Show("The node data is not Available");
            }
        }

        public void TemperatureNewOpen()
        {
            Vayu.HourlyTemp_Grpah.Views.MainWindow window = new Vayu.HourlyTemp_Grpah.Views.MainWindow();
            window.DataContext = new Vayu.HourlyTemp_Grpah.ViewModels.MainWindowViewModel(new HourlyTemp_Grpah.Model.DataService());
            window.Show();
        }

        private void SetSelectedCityValue(PricingNode selectedNode)
        {

        }

        private void SetMinMaxAvg()
        {
            mTemperatureAllList.Clear();
            mAVGTemperatureallList.Clear();
            mMinTemperatureallList.Clear();
            mMaxTemperatureallList.Clear();
            if (AllTemperatureList != null)
            {
                var all = AllTemperatureList.OrderBy(x => x.City).ThenBy(y => y.Date).Where(x => x.City == SelectedCityValue);
                foreach (var item in all)
                {
                    PlotData mPlot = new PlotData();
                    mPlot.Date = item.Date;
                    mPlot.City = item.City;
                    mPlot.Temperature = item.Avg;
                    mAVGTemperatureallList.Add(mPlot);
                    if (mTemperatureAllList.ContainsKey("Avg"))
                    {
                        mTemperatureAllList.Remove("Avg");
                    }
                    mTemperatureAllList.Add("Avg", mAVGTemperatureallList);
                }
                foreach (var item in all)
                {
                    PlotData mPlotAvg = new PlotData();
                    mPlotAvg.Date = item.Date;
                    mPlotAvg.City = item.City;

                    mPlotAvg.Temperature = item.Min;
                    mMinTemperatureallList.Add(mPlotAvg);
                    if (mTemperatureAllList.ContainsKey("Min"))
                    {
                        mTemperatureAllList.Remove("Min");
                    }
                    mTemperatureAllList.Add("Min", mMinTemperatureallList);
                }
                foreach (var item in all)
                {
                    PlotData mPlotMax = new PlotData();
                    mPlotMax.Date = item.Date;
                    mPlotMax.City = item.City;
                    mPlotMax.Temperature = item.Max;
                    mMaxTemperatureallList.Add(mPlotMax);
                    if (mTemperatureAllList.ContainsKey("Max"))
                    {
                        mTemperatureAllList.Remove("Max");
                    }
                    mTemperatureAllList.Add("Max", mMaxTemperatureallList);
                }
            }
        }

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
                Title = "'F ----->",
                AxisTitleDistance = 0
            });
            mTempModel.Axes.Add(new DateTimeAxis
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
                LegendTextColor = OxyColors.White
            };

            mTempModel.Legends.Add(l);

            mTempModel.TitlePadding = 3;
        }


        private void SetHourlyPlot()
        {
            if (mHourlyTemperatureList != null)
            {
                HourlyPlotList.Clear();
                var list = mHourlyTemperatureList.OrderByDescending(x => x.Date).Take(6);
                foreach (var item in list)
                {
                    if (item.Hour1 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour1;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 1, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour2 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour2;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 2, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour3 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour3;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 3, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour4 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour4;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 4, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour5 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour5;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 5, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour6 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour6;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 6, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour7 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour7;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 7, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour8 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour8;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 8, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour9 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour9;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 9, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour10 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour10;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 10, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour11 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour11;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 11, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour12 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour12;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 12, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour13 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour13;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 13, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour14 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour14;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 14, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour15 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour15;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 15, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour16 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour16;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 16, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour17 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour17;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 17, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour18 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour18;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 18, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour19 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour19;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 19, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour20 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour20;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 20, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour21 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour21;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 21, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour22 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour22;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 22, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour23 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour23;
                        pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 23, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                    if (item.Hour24 != null)
                    {
                        PlotData pdata = new PlotData();
                        pdata.City = item.City;
                        pdata.Temperature = item.Hour24;
                        pdata.Date = Convert.ToDateTime(item.Date.AddDays(1));
                        //pdata.Date = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 24, 0, 0);
                        HourlyPlotList.Add(pdata);
                    }
                }
                HourlyPlotList = HourlyPlotList.OrderBy(x => x.Date).ToList();


                RefreshHourlyPlot();
            }
        }
        private void RefreshHourlyPlot()
        {
            mTempModel = null;
            if (HourlyPlotList != null || HourlyPlotList.Count != 0)
            {
                //lock (lockObj)
                {
                    try
                    {
                        if (mTempModel == null)
                        {
                            mTempModel = new PlotModel();
                            CreateAxes();
                        }

                        try
                        {
                            if (mOxyColorList == null)
                                FillColorList();
                            //foreach(var unit in HourlyPlotList)
                            //{
                            if (mTempModel.Series.Count > 0)
                            {
                                mTempModel.Series.Remove(mTempModel.Series.FirstOrDefault());
                            }

                            mTempModel.Series.Add(CreateHourlySeries(HourlyPlotList.OrderBy(j => j.Date).ToList()));

                            PlotDataModel = null;
                            PlotDataModel = mTempModel as PlotModel;
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

        public void ExportToCSVCommand()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        }


        private void ExportToCSVThreaded()
        {
            if (HourlyTemperatureList == null || HourlyTemperatureList.Count == 0)
            {
                System.Windows.MessageBox.Show("No data to export to");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
            dialog.FileName = "HourlyTemperatureRecords" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    if (HourlyTemperatureList != null && HourlyTemperatureList.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (PropertyInfo item in HourlyTemperatureList[0].GetType().GetProperties())
                        {
                            if (item.Name == "Date" || item.Name == "City" || item.Name == "Zone" || item.Name == "Max" || item.Name == "Avg" || item.Name == "Min" || item.Name == "Hour1" || item.Name == "Hour2" || item.Name == "Hour3" || item.Name == "Hour4" || item.Name == "Hour5" || item.Name ==
                                "Hour6" || item.Name == "Hour7" || item.Name == "Hour8" || item.Name == "Hour9" || item.Name == "Hour10" || item.Name == "Hour11" ||
                                 item.Name == "Hour12" || item.Name == "Hour13" || item.Name == "Hour14" || item.Name == "Hour15" || item.Name == "Hour16" ||
                                 item.Name == "Hour17" || item.Name == "Hour18" || item.Name == "Hour19" || item.Name == "Hour20" || item.Name == "Hour21" ||
                                 item.Name == "Hour22" || item.Name == "Hour23" || item.Name == "Hour24")
                            {
                                builder.Append(item.Name + ",");
                            }

                        }
                        builder.AppendLine();
                        foreach (HourlyTemperatureData item in HourlyTemperatureList)
                        {
                            foreach (PropertyInfo propItem in item.GetType().GetProperties())
                            {
                                if (propItem.Name == "Date" || propItem.Name == "City" || propItem.Name == "Zone" || propItem.Name == "Max" || propItem.Name == "Min" || propItem.Name == "Hour1" || propItem.Name == "Hour2" || propItem.Name == "Hour3" || propItem.Name == "Hour4" || propItem.Name == "Hour5" || propItem.Name ==
                                "Hour6" || propItem.Name == "Hour7" || propItem.Name == "Hour8" || propItem.Name == "Hour9" || propItem.Name == "Hour10" || propItem.Name == "Hour11" ||
                                 propItem.Name == "Hour12" || propItem.Name == "Hour13" || propItem.Name == "Hour14" || propItem.Name == "Hour15" || propItem.Name == "Hour16" ||
                                 propItem.Name == "Hour17" || propItem.Name == "Hour18" || propItem.Name == "Hour19" || propItem.Name == "Hour20" || propItem.Name == "Hour21" ||
                                 propItem.Name == "Hour22" || propItem.Name == "Hour23" || propItem.Name == "Hour24")
                                {
                                    builder.Append(propItem.GetValue(item) + ",");
                                }
                                if (propItem.Name == "Avg")
                                {
                                    double avg = (double)propItem.GetValue(item);
                                    builder.Append(avg.ToString("0.00") + ",");
                                }
                            }
                            builder.AppendLine();
                        }
                        if (builder.Length > 0)
                        {
                            using (TextWriter str = new StreamWriter(dialog.FileName, false))
                            {
                                str.Write(builder.ToString());
                                str.Flush();
                                str.Close();
                                str.Dispose();
                            }
                            if (File.Exists(dialog.FileName))
                            {
                                System.Windows.MessageBox.Show("Successfully saved the file " + dialog.FileName);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Couldnot save the file");
                            }
                        }
                    }
                }
            }
        }

        private void SetTemperature()
        {
            bool current = true;
            if (RadioCurrentChecked)
                current = true;
            else
                current = false;
            //_dataService.GetAllTemperatureData((item1, error) =>
            //{
            //    if (error != null)
            //    {
            //        return;
            //    }
            //    AllTemperatureList = item1;
            //}, StartDate, Enddate, current, MarketComboSelectedValue.ToString());
            string icaocode = CityIcaoCodeList.Where(x => x.Item1 == SelectedCityValue).FirstOrDefault().Item2;
            _dataService.GetAllTemperatureDataList((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                HourlyTemperatureList = item1;
            }, StartDate, Enddate, current, icaocode, MarketComboSelectedValue.ToString());
            //SetTemperature();
            if (HourlyTemperatureList != null)
            {
                var all = HourlyTemperatureList.OrderBy(x => x.City).ThenBy(y => y.Date);
                //var tempmin = (from b in AllTemperatureList orderby b.Date select new { b.City, b.Zone, b.Min, b.Date }).ToList();
                //SetGridsForMin(all);
                //SetGridsForMax(all);
                //SetGridsForAvg(all);
                //TemperatureList = new List<Temperature>();
                //if (RadioAllChecked)
                //{
                //    TemperatureList.Clear();
                //    TemperatureList.AddRange(MinTemperatureList);
                //    TemperatureList.AddRange(MaxTemperatureList);
                //    TemperatureList.AddRange(AvgTemperatureList);
                //}
                //else if (RadioMaxChecked)
                //{
                //    TemperatureList.Clear();
                //    TemperatureList.AddRange(MaxTemperatureList);
                //}

                //else if (RadioMinChecked)
                //{
                //    TemperatureList.Clear();
                //    TemperatureList.AddRange(MinTemperatureList);
                //}
                //else if (RadioAvgChecked)
                //{
                //    TemperatureList.Clear();
                //    TemperatureList.AddRange(AvgTemperatureList);
                //}
                //TemperatureList = TemperatureList.Where(x => x.Zone != null & x.City != null).ToList();
            }
        }

        private void SetGridsForAvg(IOrderedEnumerable<TemperatureData> all)
        {
            AvgTemperatureList = new List<Temperature>();
            Temperature mtemp = new Temperature();
            foreach (var item in all)
            {
                mtemp.Zone = item.Zone;
                mtemp.City = item.City;
                mtemp.MaxMinAvg = "Avg";
                int eday = Enddate.Day;
                int day = item.Date.Day;
                switch (day)
                {
                    case 1:
                        {
                            if (eday == day)
                            {
                                mtemp.Day1 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day1 = item.Avg.Value;
                            }
                            break;
                        }
                    case 2:
                        {
                            if (eday == day)
                            {
                                mtemp.Day2 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day2 = item.Avg.Value;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (eday == day)
                            {
                                mtemp.Day3 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day3 = item.Avg.Value;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (eday == day)
                            {
                                mtemp.Day4 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day4 = item.Avg.Value;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (eday == day)
                            {
                                mtemp.Day5 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day5 = item.Avg.Value;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (eday == day)
                            {
                                mtemp.Day6 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day6 = item.Avg.Value;
                            }
                            break;
                        }
                    case 7:
                        {
                            if (eday == day)
                            {
                                mtemp.Day7 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day7 = item.Avg.Value;
                            }
                            break;
                        }
                    case 8:
                        {
                            if (eday == day)
                            {
                                mtemp.Day8 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day8 = item.Avg.Value;
                            }
                            break;
                        }
                        break;
                    case 9:
                        {
                            if (eday == day)
                            {
                                mtemp.Day9 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day9 = item.Avg.Value;
                            }
                            break;
                        }
                        break;
                    case 10:
                        {
                            if (eday == day)
                            {
                                mtemp.Day10 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day10 = item.Avg.Value;
                            }
                            break;
                        }
                    case 11:
                        {
                            if (eday == day)
                            {
                                mtemp.Day11 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day11 = item.Avg.Value;
                            }
                            break;
                        }
                    case 12:
                        {
                            if (eday == day)
                            {
                                mtemp.Day12 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day12 = item.Avg.Value;
                            }
                            break;
                        }
                    case 13:
                        {
                            if (eday == day)
                            {
                                mtemp.Day13 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day13 = item.Avg.Value;
                            }
                            break;
                        }
                    case 14:
                        {
                            if (eday == day)
                            {
                                mtemp.Day14 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day14 = item.Avg.Value;
                            }
                            break;
                        }
                    case 15:
                        {
                            if (eday == day)
                            {
                                mtemp.Day15 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day15 = item.Avg.Value;
                            }
                            break;
                        }
                    case 16:
                        {
                            if (eday == day)
                            {
                                mtemp.Day16 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day16 = item.Avg.Value;
                            }
                            break;
                        }
                    case 17:
                        {
                            if (eday == day)
                            {
                                mtemp.Day17 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day17 = item.Avg.Value;
                            }
                            break;
                        }
                    case 18:
                        {
                            if (eday == day)
                            {
                                mtemp.Day18 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day18 = item.Avg.Value;
                            }
                            break;
                        }
                    case 19:
                        {
                            if (eday == day)
                            {
                                mtemp.Day19 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day19 = item.Avg.Value;
                            }
                            break;
                        }
                    case 20:
                        {
                            if (eday == day)
                            {
                                mtemp.Day20 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day20 = item.Avg.Value;
                            }
                            break;
                        }
                    case 21:
                        {
                            if (eday == day)
                            {
                                mtemp.Day21 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day21 = item.Avg.Value;
                            }
                            break;
                        }
                    case 22:
                        {
                            if (eday == day)
                            {
                                mtemp.Day22 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day22 = item.Avg.Value;
                            }
                            break;
                        }
                    case 23:
                        {
                            if (eday == day)
                            {
                                mtemp.Day23 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day23 = item.Avg.Value;
                            }
                            break;
                        }
                    case 24:
                        {
                            if (eday == day)
                            {
                                mtemp.Day24 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day24 = item.Avg.Value;
                            }
                            break;
                        }
                    case 25:
                        {
                            if (eday == day)
                            {
                                mtemp.Day25 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day25 = item.Avg.Value;
                            }
                            break;
                        }
                    case 26:
                        {
                            if (eday == day)
                            {
                                mtemp.Day26 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day26 = item.Avg.Value;
                            }
                            break;
                        }
                    case 27:
                        {
                            if (eday == day)
                            {
                                mtemp.Day27 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day27 = item.Avg.Value;
                            }
                            break;
                        }
                    case 28:
                        {
                            if (eday == day)
                            {
                                mtemp.Day28 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day28 = item.Avg.Value;
                            }
                            break;
                        }
                    case 29:
                        {
                            if (eday == day)
                            {
                                mtemp.Day29 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day29 = item.Avg.Value;
                            }
                            break;
                        }
                    case 30:
                        {
                            if (eday == day)
                            {
                                mtemp.Day30 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day30 = item.Avg.Value;
                            }
                            break;
                        }
                    case 31:
                        {
                            if (eday == day)
                            {
                                mtemp.Day31 = item.Avg.Value;
                                AvgTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day31 = item.Avg.Value;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            AvgTemperatureList.Add(mtemp);
            //AvgTemperatureList.RemoveAll(item => item == null);
        }

        private void SetGridsForMax(IOrderedEnumerable<TemperatureData> all)
        {
            MaxTemperatureList = new List<Temperature>();
            Temperature mtemp = new Temperature();
            foreach (var item in all)
            {
                mtemp.Zone = item.Zone;
                mtemp.City = item.City;
                mtemp.MaxMinAvg = "Max";
                int eday = Enddate.Day;
                int day = item.Date.Day;
                switch (day)
                {
                    case 1:
                        {
                            if (eday == day)
                            {
                                mtemp.Day1 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day1 = item.Max.Value;
                            }
                            break;
                        }
                    case 2:
                        {
                            if (eday == day && mtemp.Day1 != null)
                            {
                                mtemp.Day2 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day2 = item.Max.Value;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (eday == day)
                            {
                                mtemp.Day3 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day3 = item.Max.Value;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (eday == day)
                            {
                                mtemp.Day4 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day4 = item.Max.Value;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (eday == day)
                            {
                                mtemp.Day5 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day5 = item.Max.Value;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (eday == day)
                            {
                                mtemp.Day6 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day6 = item.Max.Value;
                            }
                            break;
                        }
                    case 7:
                        {
                            if (eday == day)
                            {
                                mtemp.Day7 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day7 = item.Max.Value;
                            }
                            break;
                        }
                    case 8:
                        {
                            if (eday == day)
                            {
                                mtemp.Day8 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day8 = item.Max.Value;
                            }
                            break;
                        }
                        break;
                    case 9:
                        {
                            if (eday == day)
                            {
                                mtemp.Day9 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day9 = item.Max.Value;
                            }
                            break;
                        }
                        break;
                    case 10:
                        {
                            if (eday == day)
                            {
                                mtemp.Day10 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day10 = item.Max.Value;
                            }
                            break;
                        }
                    case 11:
                        {
                            if (eday == day)
                            {
                                mtemp.Day11 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day11 = item.Max.Value;
                            }
                            break;
                        }
                    case 12:
                        {
                            if (eday == day)
                            {
                                mtemp.Day12 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day12 = item.Max.Value;
                            }
                            break;
                        }
                    case 13:
                        {
                            if (eday == day)
                            {
                                mtemp.Day13 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day13 = item.Max.Value;
                            }
                            break;
                        }
                    case 14:
                        {
                            if (eday == day)
                            {
                                mtemp.Day14 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day14 = item.Max.Value;
                            }
                            break;
                        }
                    case 15:
                        {
                            if (eday == day)
                            {
                                mtemp.Day15 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day15 = item.Max.Value;
                            }
                            break;
                        }
                    case 16:
                        {
                            if (eday == day)
                            {
                                mtemp.Day16 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day16 = item.Max.Value;
                            }
                            break;
                        }
                    case 17:
                        {
                            if (eday == day)
                            {
                                mtemp.Day17 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day17 = item.Max.Value;
                            }
                            break;
                        }
                    case 18:
                        {
                            if (eday == day)
                            {
                                mtemp.Day18 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day18 = item.Max.Value;
                            }
                            break;
                        }
                    case 19:
                        {
                            if (eday == day)
                            {
                                mtemp.Day19 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day19 = item.Max.Value;
                            }
                            break;
                        }
                    case 20:
                        {
                            if (eday == day)
                            {
                                mtemp.Day20 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day20 = item.Max.Value;
                            }
                            break;
                        }
                    case 21:
                        {
                            if (eday == day)
                            {
                                mtemp.Day21 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day21 = item.Max.Value;
                            }
                            break;
                        }
                    case 22:
                        {
                            if (eday == day)
                            {
                                mtemp.Day22 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day22 = item.Max.Value;
                            }
                            break;
                        }
                    case 23:
                        {
                            if (eday == day)
                            {
                                mtemp.Day23 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day23 = item.Max.Value;
                            }
                            break;
                        }
                    case 24:
                        {
                            if (eday == day)
                            {
                                mtemp.Day24 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day24 = item.Max.Value;
                            }
                            break;
                        }
                    case 25:
                        {
                            if (eday == day)
                            {
                                mtemp.Day25 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day25 = item.Max.Value;
                            }
                            break;
                        }
                    case 26:
                        {
                            if (eday == day)
                            {
                                mtemp.Day26 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day26 = item.Max.Value;
                            }
                            break;
                        }
                    case 27:
                        {
                            if (eday == day)
                            {
                                mtemp.Day27 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day27 = item.Max.Value;
                            }
                            break;
                        }
                    case 28:
                        {
                            if (eday == day)
                            {
                                mtemp.Day28 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day28 = item.Max.Value;
                            }
                            break;
                        }
                    case 29:
                        {
                            if (eday == day)
                            {
                                mtemp.Day29 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day29 = item.Max.Value;
                            }
                            break;
                        }
                    case 30:
                        {
                            if (eday == day)
                            {
                                mtemp.Day30 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day30 = item.Max.Value;
                            }
                            break;
                        }
                    case 31:
                        {
                            if (eday == day)
                            {
                                mtemp.Day31 = item.Max.Value;
                                MaxTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day31 = item.Max.Value;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            MaxTemperatureList.Add(mtemp);
            //MaxTemperatureList.RemoveAll(item => item == null);
        }

        private void SetGridsForMin(IOrderedEnumerable<TemperatureData> all)
        {
            MinTemperatureList = new List<Temperature>();
            Temperature mtemp = new Temperature();
            foreach (var item in all)
            {
                mtemp.Zone = item.Zone;
                mtemp.City = item.City;
                mtemp.MaxMinAvg = "Min";
                int eday = Enddate.Day;
                int day = item.Date.Day;
                switch (day)
                {
                    case 1:
                        {
                            if (eday == day)
                            {
                                mtemp.Day1 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day1 = item.Min.Value;
                            }
                            break;
                        }
                    case 2:
                        {
                            if (eday == day)
                            {
                                mtemp.Day2 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day2 = item.Min.Value;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (eday == day)
                            {
                                mtemp.Day3 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day3 = item.Min.Value;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (eday == day)
                            {
                                mtemp.Day4 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day4 = item.Min.Value;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (eday == day)
                            {
                                mtemp.Day5 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day5 = item.Min.Value;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (eday == day)
                            {
                                mtemp.Day6 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day6 = item.Min.Value;
                            }
                            break;
                        }
                    case 7:
                        {
                            if (eday == day)
                            {
                                mtemp.Day7 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day7 = item.Min.Value;
                            }
                            break;
                        }
                    case 8:
                        {
                            if (eday == day)
                            {
                                mtemp.Day8 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day8 = item.Min.Value;
                            }
                            break;
                        }
                        break;
                    case 9:
                        {
                            if (eday == day)
                            {
                                mtemp.Day9 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day9 = item.Min.Value;
                            }
                            break;
                        }
                        break;
                    case 10:
                        {
                            if (eday == day)
                            {
                                mtemp.Day10 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day10 = item.Min.Value;
                            }
                            break;
                        }
                    case 11:
                        {
                            if (eday == day)
                            {
                                mtemp.Day11 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day11 = item.Min.Value;
                            }
                            break;
                        }
                    case 12:
                        {
                            if (eday == day)
                            {
                                mtemp.Day12 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day12 = item.Min.Value;
                            }
                            break;
                        }
                    case 13:
                        {
                            if (eday == day)
                            {
                                mtemp.Day13 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day13 = item.Min.Value;
                            }
                            break;
                        }
                    case 14:
                        {
                            if (eday == day)
                            {
                                mtemp.Day14 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day14 = item.Min.Value;
                            }
                            break;
                        }
                    case 15:
                        {
                            if (eday == day)
                            {
                                mtemp.Day15 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day15 = item.Min.Value;
                            }
                            break;
                        }
                    case 16:
                        {
                            if (eday == day)
                            {
                                mtemp.Day16 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day16 = item.Min.Value;
                            }
                            break;
                        }
                    case 17:
                        {
                            if (eday == day)
                            {
                                mtemp.Day17 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day17 = item.Min.Value;
                            }
                            break;
                        }
                    case 18:
                        {
                            if (eday == day)
                            {
                                mtemp.Day18 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day18 = item.Min.Value;
                            }
                            break;
                        }
                    case 19:
                        {
                            if (eday == day)
                            {
                                mtemp.Day19 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day19 = item.Min.Value;
                            }
                            break;
                        }
                    case 20:
                        {
                            if (eday == day)
                            {
                                mtemp.Day20 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day20 = item.Min.Value;
                            }
                            break;
                        }
                    case 21:
                        {
                            if (eday == day)
                            {
                                mtemp.Day21 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day21 = item.Min.Value;
                            }
                            break;
                        }
                    case 22:
                        {
                            if (eday == day)
                            {
                                mtemp.Day22 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day22 = item.Min.Value;
                            }
                            break;
                        }
                    case 23:
                        {
                            if (eday == day)
                            {
                                mtemp.Day23 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day23 = item.Min.Value;
                            }
                            break;
                        }
                    case 24:
                        {
                            if (eday == day)
                            {
                                mtemp.Day24 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day24 = item.Min.Value;
                            }
                            break;
                        }
                    case 25:
                        {
                            if (eday == day)
                            {
                                mtemp.Day25 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day25 = item.Min.Value;
                            }
                            break;
                        }
                    case 26:
                        {
                            if (eday == day)
                            {
                                mtemp.Day26 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day26 = item.Min.Value;
                            }
                            break;
                        }
                    case 27:
                        {
                            if (eday == day)
                            {
                                mtemp.Day27 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day27 = item.Min.Value;
                            }
                            break;
                        }
                    case 28:
                        {
                            if (eday == day)
                            {
                                mtemp.Day28 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day28 = item.Min.Value;
                            }
                            break;
                        }
                    case 29:
                        {
                            if (eday == day)
                            {
                                mtemp.Day29 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day29 = item.Min.Value;
                            }
                            break;
                        }
                    case 30:
                        {
                            if (eday == day)
                            {
                                mtemp.Day30 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day30 = item.Min.Value;
                            }
                            break;
                        }
                    case 31:
                        {
                            if (eday == day)
                            {
                                mtemp.Day31 = item.Min.Value;
                                MinTemperatureList.Add(mtemp);
                                mtemp = new Temperature();
                            }
                            else
                            {
                                mtemp.Day31 = item.Min.Value;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            MinTemperatureList.Add(mtemp);
            //MinTemperatureList.RemoveAll(item => item == null);

        }

        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("Max", OxyColors.DarkBlue);
            mOxyColorList.Add("Min", OxyColors.DarkOrange);
            mOxyColorList.Add("Avg", OxyColors.DarkRed);
            mOxyColorList.Add("General", OxyColors.Green);
        }
        private bool KeyChecked(string item)
        {
            switch (item.ToLower())
            {
                case "min": return MinChecked;
                case "max": return MaxChecked;
                case "avg": return AvgChecked;
                default: return true;
            }
        }

        private LineStyle GetLineStyle(string key)
        {
            if (key.StartsWith("Min"))
            {
                return LineStyle.DashDashDot;
            }
            else if (key.StartsWith("Max"))
            {
                return LineStyle.LongDash;
            }
            else
            {
                return LineStyle.Dot;
            }
        }
        private Series CreateSeries(List<PlotData> list, string key)
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
                        Y = dItem.Temperature
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
                TrackerFormatString = "{0}\n{X:MMM:dd}\n{Y:###}",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.5,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
                LineStyle = LineStyle.DashDashDot, //GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = key + "--->" + " " + SelectedCityValue.ToString()
            };
        }

        private Series CreateHourlySeries(List<PlotData> list)
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
                        Y = dItem.Temperature
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
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:###F}" /*"{0}\n{X:MMM:dd}\n{Y:###}"*/,
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.5,
                Color = OxyColors.MediumPurple,
                LineStyle = LineStyle.DashDashDot, //GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = "Hourly --->" + " " + SelectedCityValue.ToString()
            };
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
    public class CityZones
    {
        public string City { get; set; }
        public string Zone { get; set; }
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
