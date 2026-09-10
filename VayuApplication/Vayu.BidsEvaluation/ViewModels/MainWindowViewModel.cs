using Microsoft.Win32;
using OxyPlot;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.BidsEvaluation.Model;

namespace Vayu.BidsEvaluation.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        private DateTime today = DateTime.Today;
        public DelegateCommand RunRefreshommand { private set; get; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
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

        private double? mTotalSubmitted;
        public double? TotalSubmitted
        {
            get
            {
                return mTotalSubmitted;
            }
            set
            {
                mTotalSubmitted = value;
                RaisePropertyChanged("TotalSubmitted");
            }
        }

        private double? mTotalCleared;
        public double? TotalCleared
        {
            get
            {
                return mTotalCleared;
            }
            set
            {
                mTotalCleared = value;
                RaisePropertyChanged("TotalCleared");
            }
        }
        private double? mTotalSubmittedMW;
        public double? TotalSubmittedMW
        {
            get
            {
                return mTotalSubmittedMW;
            }
            set
            {
                mTotalSubmittedMW = value;
                RaisePropertyChanged("TotalSubmittedMW");
            }
        }

        private double? mTotalClearedMW;
        public double? TotalClearedMW
        {
            get
            {
                return mTotalClearedMW;
            }
            set
            {
                mTotalClearedMW = value;
                RaisePropertyChanged("TotalClearedMW");
            }
        }
        private double? mPercentageMWs;
        public double? PercentageMWs
        {
            get
            {
                return mPercentageMWs;
            }
            set
            {
                mPercentageMWs = value;
                RaisePropertyChanged("PercentageMWs");
            }
        }

        private List<BidsEvaluationDLY> mPNLDailyList;
        public List<BidsEvaluationDLY> PNLDailyList
        {
            get
            {
                return mPNLDailyList;
            }
            set
            {
                mPNLDailyList = value;
                RaisePropertyChanged("PNLDailyList");
            }
        }
        private List<BidsEvaluationDLY> mPNLDailyListMonthly;
        public List<BidsEvaluationDLY> PNLDailyListMonthly
        {
            get
            {
                return mPNLDailyListMonthly;
            }
            set
            {
                mPNLDailyListMonthly = value;
                RaisePropertyChanged("PNLDailyListMonthly");
            }
        }

        private List<BidsEvaluationDLY> mPNLDailyListAll;
        public List<BidsEvaluationDLY> PNLDailyListAll
        {
            get
            {
                return mPNLDailyListAll;
            }
            set
            {
                mPNLDailyListAll = value;
                RaisePropertyChanged("PNLDailyListAll");
            }
        }
        private bool mDailyChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [Daily checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Daily checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DailyChecked
        {
            get
            {
                return mDailyChecked;
            }
            set
            {
                mDailyChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("DailyChecked");
            }
        }
        private bool mMonthlyChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [Monthly checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Monthly checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MonthlyChecked
        {
            get
            {
                return mMonthlyChecked;
            }
            set
            {
                mMonthlyChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("MonthlyChecked");
            }
        }
        private PlotModel mPlotModelLower;

        public PlotModel PlotModelLower
        {
            get
            {
                return mPlotModelLower;
            }
            set
            {
                mPlotModelLower = value;
                RaisePropertyChanged("PlotModelLower");
            }
        }
        public MainWindowViewModel(IDataService dataService = null)
        {
            // _dataService = dataService;
            if (_dataService == null)
            {
                this._dataService = new Model.DataService();
            }
            else
            {
                this._dataService = _dataService;
            }
            //StartDate = new DateTime(today.Year, today.Month, 01);
            EndDate = DateTime.Today.AddDays(-1);
            StartDate = DateTime.Today.AddDays(-1);
            // StartDate = new DateTime(dt.Year, dt.Month, 1);
            //EndDate = DateTime.Today.AddDays(-1);
            RunExportCSVCommand = new DelegateCommand(ExportCSVData);
            RunRefreshommand = new DelegateCommand(MtdDateClick);
            //Refresh();
            GetAllData();
        }

        private void MtdDateClick()
        {
            try
            {
                GetAllData();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DisplayRefresh(bool ShowData)
        {
            try
            {
                List<BidsEvaluationDLY> currentmonthPNLList = new List<BidsEvaluationDLY>();
                List<string> stMonthList = new List<string>();
                DateTime sDate = StartDate;
                DateTime eDate = EndDate;
                string st = sDate.ToString("MMM");
                if (ShowData)
                {
                    List<BidsEvaluationDLY> currentdailylist = new List<BidsEvaluationDLY>();
                    double? runningtotal = 0;
                    foreach (BidsEvaluationDLY item in currentmonthPNLList)
                    {
                        runningtotal = runningtotal + item.SubmittedCount + item.TotalUnclearedBids + item.TotalUnclearedMW;
                        item.PercentageMWCleared = runningtotal;
                        currentdailylist.Add(item);
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    PNLDailyList = currentmonthPNLList;

                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].PortfolioName.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].PortfolioName = "CurrentDate";
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void GetAllData()
        {
            PNLDailyList = null;
            {
                int marketkey = 9;
                DataService ds = new DataService();
                PNLDailyList = ds.GetBidsListDaily(StartDate, EndDate, marketkey).OrderBy(x => x.MarketDate).ToList<BidsEvaluationDLY>();
                DisplayMTDRefresh();
                Mouse.OverrideCursor = null;
            }
            mPNLDailyListMonthly = null;
            {
                int marketkey = 9;
                DataService ds = new DataService();
                PNLDailyListMonthly = ds.GetBidsListDailyMonthly(StartDate, EndDate, marketkey).OrderBy(x => x.MarketDate).ToList<BidsEvaluationDLY>();
                DisplayMTDRefresh();
                Mouse.OverrideCursor = null;
            }

        }
        private void DisplayMTDRefresh()
        {
            try
            {
                if (PNLDailyList.Count > 0)
                {
                    TotalSubmitted = PNLDailyList.Sum(x => x.SubmittedCount);
                    TotalCleared = PNLDailyList.Sum(x => x.ClearedCount);

                    TotalSubmittedMW = PNLDailyList.Sum(x => x.RequestedMW);
                    TotalClearedMW = PNLDailyList.Sum(x => x.ClearedMW);
                }
                PercentageMWs = PNLDailyList.Sum(x => x.PercentageMWCleared) / PNLDailyList.Count;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void Refresh()
        {
            List<BidsEvaluationDLY> currentmonthPNLList = new List<BidsEvaluationDLY>();
            int marketkey = 9;
            DataService ds = new DataService();
            var dt = DateTime.Now;
            currentmonthPNLList = ds.GetBidsListDaily(new DateTime(dt.Year, dt.Month, 1), DateTime.Today, marketkey);
            if (StartDate <= EndDate)
            {
                PNLDailyList = ds.GetBidsListDaily(StartDate, DateTime.Today, marketkey);

                if (PNLDailyList.Count != 0)
                {
                    if (PNLDailyList[PNLDailyList.Count - 1].PortfolioName.ToString() == DateTime.Today.ToString())
                        PNLDailyList[PNLDailyList.Count - 1].PortfolioName = "CurrentDate";
                }
            }
            else
                MessageBox.Show("StartDate should be less than EndDate", "Date");
        }
        private void ExportCSVData()
        {
            if (PNLDailyList != null)
            {
                SaveFileDialog savefiledialog = new SaveFileDialog();
                savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                savefiledialog.FilterIndex = 1;
                savefiledialog.RestoreDirectory = true;
                savefiledialog.FileName = "ErcotEvaluationBids" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                if ((bool)savefiledialog.ShowDialog())
                {

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("ErcotEvaluationBidsReport");
                    foreach (PropertyInfo item in PNLDailyList[0].GetType().GetProperties())
                    {
                        builder.Append(item.Name + ",");
                    }
                    builder.ToString().Remove(builder.Length - 1, 1);
                    builder.AppendLine();

                    foreach (BidsEvaluationDLY item in PNLDailyList)
                    {
                        foreach (PropertyInfo propName in item.GetType().GetProperties())
                        {
                            builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                        }
                        builder.AppendLine();
                    }
                    try
                    {
                        TextWriter writer = new StreamWriter(savefiledialog.FileName);
                        writer.Write(builder.ToString());
                        writer.Flush();
                        writer.Close();
                        MessageBox.Show("Successfully created the file");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Unable To Create File");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Insert the Data");
            }
        }
        public void UpdateChartCommand(bool isCalculatable)
        {
            //PathDayPlotModelUpper = null;
            //List<Node> asBidDaSendFilteredSortedList = new List<Node>();
            //List<Node> asBidRtSendFilteredSortedList = new List<Node>();
            //List<Node> asBidDartSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeDaSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeRtSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeDartSendFilteredSortedList = new List<Node>();
            //List<Node> nodeList = GetNodeList(out asBidDaSendFilteredSortedList, out asBidRtSendFilteredSortedList, out asBidDartSendFilteredSortedList, out mustTakeDaSendFilteredSortedList,
            //                                out mustTakeRtSendFilteredSortedList, out mustTakeDartSendFilteredSortedList, null);
            //if (nodeList == null)
            //{
            //    return;
            //}
            //PlotModelUpper = CreatePlotModelUpper(nodeList);
            //PlotModelLower = CreatePlotModelLower(nodeList);
            //List<Node> daSendFilteredSortedList;
            //List<Node> rtSendFilteredSortedList;
            //List<Node> dartSendFilteredSortedList;
            //if (AsBidChecked)
            //{
            //    daSendFilteredSortedList = asBidDaSendFilteredSortedList;
            //    rtSendFilteredSortedList = asBidRtSendFilteredSortedList;
            //    dartSendFilteredSortedList = asBidDartSendFilteredSortedList;
            //}
            //else
            //{
            //    daSendFilteredSortedList = mustTakeDaSendFilteredSortedList;
            //    rtSendFilteredSortedList = mustTakeRtSendFilteredSortedList;
            //    dartSendFilteredSortedList = mustTakeDartSendFilteredSortedList;
            //}
            //if (isCalculatable)
            //{
            //    RefreshDayComparisonGridCommand(daSendFilteredSortedList, rtSendFilteredSortedList, dartSendFilteredSortedList);
            //}
            //SetPathRisk();
        }
        public void SetPathChart()
        {
            //if (PathComboSelectedValue == null)
            //{
            //    return;
            //}
            //List<Node> asBidDaSendFilteredSortedList = new List<Node>();
            //List<Node> asBidRtSendFilteredSortedList = new List<Node>();
            //List<Node> asBidDartSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeDaSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeRtSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeDartSendFilteredSortedList = new List<Node>();
            //List<Node> nodeList = GetNodeList(out asBidDaSendFilteredSortedList, out asBidRtSendFilteredSortedList, out asBidDartSendFilteredSortedList, out mustTakeDaSendFilteredSortedList,
            //                                out mustTakeRtSendFilteredSortedList, out mustTakeDartSendFilteredSortedList, PathComboSelectedValue);
            //if (nodeList == null)
            //{
            //    return;
            //}
            //PathPlotModelUpper = null;
            //PathPlotModelLower = null;
            //if (nodeList.Count > 0)
            //{
            //    PathPlotModelUpper = CreatePlotModelUpper(nodeList);
            //    PathPlotModelLower = CreatePlotModelLower(nodeList);
            //}
        }
    }
    public class ValueToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            double doubleValue = 0.0;
            if (value != null)
            {
                double.TryParse(value.ToString(), out doubleValue);
                if (doubleValue < 0)
                {
                    brush = new SolidColorBrush(Colors.Red);
                }
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CellBackgroundColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var objItem = value as BidsEvaluationDLY;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "dailyfee":
                                if (objItem.TotalUnclearedBids < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailygross":
                                if (objItem.SubmittedCount < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailypnl":
                                if (objItem.ClearedMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailyrunningtotal":
                                if (objItem.PercentageMWCleared < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "isomiscellaneouscharges":
                                if (objItem.TotalUnclearedMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailygrossiso":
                                if (objItem.TotalUnclearedMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailyfeeiso":
                                if (objItem.TotalUnclearedMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);

                            default:
                                return new SolidColorBrush();
                        }
                    }
                    else return new SolidColorBrush(Colors.Black);
                }
                else return new SolidColorBrush(Colors.Black);
            }
            else return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CellBackgroundColorConverter2 : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var objItem = value as BidsEvaluationDLY;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "percentagemwcleared":
                                if (objItem.PercentageMWCleared > 70)
                                {
                                    return new SolidColorBrush(Colors.LightGreen);
                                }
                                return new SolidColorBrush(Colors.White);

                            default:
                                return new SolidColorBrush();
                        }
                    }
                    else return new SolidColorBrush(Colors.Black);
                }
                else return new SolidColorBrush(Colors.Black);
            }
            else return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
