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
using System.Windows.Data;
using System.Windows.Media;
using Vayu.TemperatureGraph.Model;

namespace Vayu.TemperatureGraph.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        //public DelegateCommand RefreshTempData { get; private set; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
        public DelegateCommand RunRefreshCommand { private set; get; }
        private Dictionary<string, List<PlotData>> mTemperatureAllList = new Dictionary<string, List<PlotData>>();
        private List<PlotData> mMinTemperatureallList = new List<PlotData>();
        private List<PlotData> mMaxTemperatureallList = new List<PlotData>();
        private List<PlotData> mAVGTemperatureallList = new List<PlotData>();
        private Dictionary<string, OxyColor> mOxyColorList;
        private PlotModel mTempModel;
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
                SetMinMaxAvg();
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
                SetMinMaxAvg();
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
                SetMinMaxAvg();
                RaisePropertyChanged("Enddate");
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
                SetTemperature();
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
                SetMinMaxAvg();
                RefreshPlot();
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
                SetMinMaxAvg();
                RefreshPlot();
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
                SetMinMaxAvg();
                RefreshPlot();
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
            RunRefreshCommand = new DelegateCommand(Refresh);
            RunExportCSVCommand = new DelegateCommand(ExportToCSVCommand);

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
        private void Refresh()
        {
            bool current = true;
            if (RadioCurrentChecked)
                current = true;
            else
                current = false;
            _dataService.GetAllTemperatureData((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                AllTemperatureList = item1;
            }, StartDate, Enddate, current, MarketComboSelectedValue.ToString());
            SetTemperature();
            FillColorList();
            SetMinMaxAvg();
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
                RefreshPlot();
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
                StringFormat = "MMM:dd",
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
                LegendPadding = 3,
            };

            mTempModel.Legends.Add(l);

        }
        private void RefreshPlot()
        {
            mTempModel = null;
            if (mTemperatureAllList != null || mTemperatureAllList.Count != 0)
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
                        //mTempModel = new PlotModel();
                        if (mAllTemperatureList == null)
                            return;
                        try
                        {
                            if (mOxyColorList == null)
                                FillColorList();
                            foreach (var item in mTemperatureAllList.Keys)
                            {
                                if (mTemperatureAllList.Count > 0)
                                {
                                    if (KeyChecked(item))
                                    {
                                        if (mTemperatureAllList[item].Count > 0)
                                        {
                                            if (mTempModel.Series.Where(x => x.Title.ToLower().Contains(item.ToLower())).Count() > 0)
                                            {
                                                mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains(item.ToLower())).FirstOrDefault());
                                            }
                                            mTempModel.Series.Add(CreateSeries(mTemperatureAllList[item].OrderBy(j => j.Date).ToList(), item));
                                        }
                                        else
                                        {
                                            if (mTempModel.Series.Where(a => a.Title.ToLower().Contains(item.ToLower())).Count() > 0)
                                            {
                                                mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains(item.ToLower())).FirstOrDefault());
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    if (mTempModel.Series.Where(a => a.Title.ToLower().Contains(item.ToLower())).Count() > 0)
                                    {
                                        mTempModel.Series.Remove(mTempModel.Series.Where(a => a.Title.ToLower().Contains(item.ToLower())).FirstOrDefault());
                                    }
                                }
                            }
                            PlotDataModel = null;
                            PlotDataModel = mTempModel as PlotModel;
                            //PlotDataModel = mTempModel;
                            //PlotDataModel.Update();
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
            if (TemperatureList == null || TemperatureList.Count == 0)
            {
                System.Windows.MessageBox.Show("No data to export to");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
            dialog.FileName = "TemperatureRecords" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    if (TemperatureList != null && TemperatureList.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (PropertyInfo item in TemperatureList[0].GetType().GetProperties())
                        {
                            if (item.Name == "Zone" || item.Name == "City" || item.Name == "MaxMinAvg" || item.Name == "Day1" || item.Name == "Day2" || item.Name == "Day3" || item.Name == "Day4" || item.Name == "Day5" || item.Name ==
                                "Day6" || item.Name == "Day7" || item.Name == "Day8" || item.Name == "Day9" || item.Name == "Day10" || item.Name == "Day11" ||
                                 item.Name == "Day12" || item.Name == "Day13" || item.Name == "Day14" || item.Name == "Day15" || item.Name == "Day16" ||
                                 item.Name == "Day17" || item.Name == "Day18" || item.Name == "Day19" || item.Name == "Day20" || item.Name == "Day21" ||
                                 item.Name == "Day22" || item.Name == "Day23" || item.Name == "Day24" || item.Name == "Day25" || item.Name == "Day26" || item.Name == "Day27"
                                || item.Name == "Day28" || item.Name == "Day29" || item.Name == "Day30" || item.Name == "Day31")
                            {
                                builder.Append(item.Name + ",");
                            }

                        }
                        builder.AppendLine();
                        foreach (Temperature item in TemperatureList)
                        {
                            foreach (PropertyInfo propItem in item.GetType().GetProperties())
                            {
                                if (propItem.Name == "Zone" || propItem.Name == "City" || propItem.Name == "MaxMinAvg" || propItem.Name == "Day1" || propItem.Name == "Day2" || propItem.Name == "Day3" || propItem.Name == "Day4" || propItem.Name == "Day5" || propItem.Name ==
                           "Day6" || propItem.Name == "Day7" || propItem.Name == "Day8" || propItem.Name == "Day9" || propItem.Name == "Day10" || propItem.Name == "Day11" ||
                            propItem.Name == "Day12" || propItem.Name == "Day13" || propItem.Name == "Day14" || propItem.Name == "Day15" || propItem.Name == "Day16" ||
                            propItem.Name == "Day17" || propItem.Name == "Day18" || propItem.Name == "Day19" || propItem.Name == "Day20" || propItem.Name == "Day21" ||
                            propItem.Name == "Day22" || propItem.Name == "Day23" || propItem.Name == "Day24" || propItem.Name == "Day25" || propItem.Name == "Day26" || propItem.Name == "Day27"
                           || propItem.Name == "Day28" || propItem.Name == "Day29" || propItem.Name == "Day30" || propItem.Name == "Day31")
                                {
                                    builder.Append(propItem.GetValue(item) + ",");
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
            _dataService.GetAllTemperatureData((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                AllTemperatureList = item1;
            }, StartDate, Enddate, current, MarketComboSelectedValue.ToString());
            //SetTemperature();
            if (AllTemperatureList != null)
            {
                var all = AllTemperatureList.OrderBy(x => x.City).ThenBy(y => y.Date);
                //var tempmin = (from b in AllTemperatureList orderby b.Date select new { b.City, b.Zone, b.Min, b.Date }).ToList();
                SetGridsForMin(all);
                SetGridsForMax(all);
                SetGridsForAvg(all);
                TemperatureList = new List<Temperature>();
                if (RadioAllChecked)
                {
                    TemperatureList.Clear();
                    TemperatureList.AddRange(MinTemperatureList);
                    TemperatureList.AddRange(MaxTemperatureList);
                    TemperatureList.AddRange(AvgTemperatureList);
                }
                else if (RadioMaxChecked)
                {
                    TemperatureList.Clear();
                    TemperatureList.AddRange(MaxTemperatureList);
                }

                else if (RadioMinChecked)
                {
                    TemperatureList.Clear();
                    TemperatureList.AddRange(MinTemperatureList);
                }
                else if (RadioAvgChecked)
                {
                    TemperatureList.Clear();
                    TemperatureList.AddRange(AvgTemperatureList);
                }
                TemperatureList = TemperatureList.Where(x => x.Zone != null & x.City != null).ToList();
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
        //public void SetZonesBox()
        //{
        //    SelectzoneList = ListCityZones.Where(x => x.City == SelectedCityValue).ToList().Select(x => x.Zone).ToList();
        //}
        //public void SetCityBox()
        //{
        //    SelectCityList = ListCityZones.Where(x => x.Zone == mSelectedZoneValue.SingleOrDefault()).ToList().Select(x => x.City).ToList();

        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("Max", OxyColors.DarkBlue);
            mOxyColorList.Add("Min", OxyColors.DarkOrange);
            mOxyColorList.Add("Avg", OxyColors.DarkRed);
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
        //private LineSeries CreateSeries(List<Temperature> list, string key)
        //{
        //    //list.RemoveAll(a => a.LoadForecast == 0);
        //    //List<GraphItem> grpList = new List<GraphItem>();
        //    //try
        //    //{
        //    //    list.ForEach(dItem =>
        //    //    {
        //    //        grpList.Add(new GraphItem
        //    //        {
        //    //            X = dItem.MarketDateTime,
        //    //            Y = dItem.LoadForecast
        //    //        });

        //    //    });
        //    //}
        //    //catch
        //    //{
        //    //}
        //    return new LineSeries()
        //    {
        //        CanTrackerInterpolatePoints = false,
        //        DataFieldX = "X",
        //        DataFieldY = "Y",
        //        ItemsSource = grpList,
        //        MarkerType = MarkerType.Diamond,
        //        TrackerFormatString = "{0}\n{X:MM:dd}\n{Y:###}" + " 'F",
        //        MarkerSize = 1,
        //        TextColor = OxyColors.Black,
        //        MarkerStrokeThickness = 0.5,
        //        Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
        //        LineStyle = GetLineStyle("Min"),
        //        MarkerStroke = OxyColors.White,
        //        Title = key.ToUpper()
        //    };
        //}
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
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = key + "--->" + " " + SelectedCityValue.ToString()
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
            public int? Y { get; set; }
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
