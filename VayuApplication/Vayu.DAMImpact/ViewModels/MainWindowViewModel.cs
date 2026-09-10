using Microsoft.Office.Interop.Excel;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.DAMImpact.Model;

namespace Vayu.DAMImpact.ViewModels
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

        public DelegateCommand RefreshAllCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand RefreshCommandops { get; private set; }
        public DelegateCommand ExportToExcelCommand { get; private set; }
        public DelegateCommand ExportToExcelCommandGrid2 { get; private set; }
        public DelegateCommand RefreshCommand1 { get; private set; }
        public DelegateCommand ExportToExcelCommandSinkGrid1 { get; private set; }
        public DelegateCommand ExportToExcelCommandSinkGrid2 { get; private set; }

        private List<string> mNotificationType;
        public List<string> NotificationType
        {
            get { return mNotificationType; }
            set
            {
                mNotificationType = value;
                RaisePropertyChanged("NotificationType");
            }
        }
        private string mNotificationTypeSelectedItem;
        public string NotificationTypeSelectedItem
        {
            get { return mNotificationTypeSelectedItem; }
            set
            {
                mNotificationTypeSelectedItem = value;
                RaisePropertyChanged("NotificationTypeSelectedItem");
                if (NotificationTypeSelectedItem == "General Notification")
                {
                    Chkgenraltype = "Visible";
                    ChkSubmmissiontype = "Visible";
                    grpSearch = false;
                }
                else
                {
                    Chkgenraltype = "Visible";
                    ChkSubmmissiontype = "Visible";
                    grpSearch = true;
                }
            }
        }
        private DateTime mStartSelectedDate = DateTime.Today;
        public DateTime StartSelectedDate
        {
            get { return mStartSelectedDate; }
            set
            {
                mStartSelectedDate = value;
                RaisePropertyChanged("StartSelectedDate");
            }
        }
        private DateTime mEndSelectedDate = DateTime.Today.AddDays(1);
        public DateTime EndSelectedDate
        {
            get { return mEndSelectedDate; }
            set
            {
                mEndSelectedDate = value;
                RaisePropertyChanged("EndSelectedDate");
            }
        }


        private List<RTImpactModel> mRTImpactsourceProp;
        public List<RTImpactModel> RTImpactsourceProp
        {
            get { return mRTImpactsourceProp; }
            set
            {
                mRTImpactsourceProp = value;
                RaisePropertyChanged("RTImpactsourceProp");
            }
        }
        private int mSourceHoursClearedProp;
        public int SourceHoursClearedProp
        {
            get { return mSourceHoursClearedProp; }
            set
            {
                mSourceHoursClearedProp = value;
                RaisePropertyChanged("SourceHoursClearedProp");
            }
        }

        private int mTotalHoursClearedProp;
        public int TotalHoursClearedProp
        {
            get { return mTotalHoursClearedProp; }
            set
            {
                mTotalHoursClearedProp = value;
                RaisePropertyChanged("TotalHoursClearedProp");
            }
        }

        private List<RTImpactModel> mRTImpactsinkProp;
        public List<RTImpactModel> RTImpactsinkProp
        {
            get { return mRTImpactsinkProp; }
            set
            {
                mRTImpactsinkProp = value;
                RaisePropertyChanged("RTImpactsinkProp");
            }
        }
        private int mSinkHoursClearedProp;
        public int SinkHoursClearedProp
        {
            get { return mSinkHoursClearedProp; }
            set
            {
                mSinkHoursClearedProp = value;
                RaisePropertyChanged("SinkHoursClearedProp");
            }
        }

        private List<RTImpactModel> mRTImpactsourceFMAProp;
        public List<RTImpactModel> RTImpactsourceFMAProp
        {
            get { return mRTImpactsourceFMAProp; }
            set
            {
                mRTImpactsourceFMAProp = value;
                RaisePropertyChanged("RTImpactsourceFMAProp");
            }
        }
        private int mSourceHoursPriceSetProp;
        public int SourceHoursPriceSetProp
        {
            get { return mSourceHoursPriceSetProp; }
            set
            {
                mSourceHoursPriceSetProp = value;
                RaisePropertyChanged("SourceHoursPriceSetProp");
            }
        }
        private double mSourcePercentageProp;
        public double SourcePercentageProp
        {
            get { return mSourcePercentageProp; }
            set
            {
                mSourcePercentageProp = value;
                RaisePropertyChanged("SourcePercentageProp");
            }
        }
        private double mSinkPercentageProp;
        public double SinkPercentageProp
        {
            get { return mSinkPercentageProp; }
            set
            {
                mSinkPercentageProp = value;
                RaisePropertyChanged("SinkPercentageProp");
            }
        }

        private string mTotalPercentageProp;
        public string TotalPercentageProp
        {
            get { return mTotalPercentageProp; }
            set
            {
                mTotalPercentageProp = value;
                RaisePropertyChanged("TotalPercentageProp");
            }
        }

        private List<RTImpactModel> mRTImpactsinkFMAProp;
        public List<RTImpactModel> RTImpactsinkFMAProp
        {
            get { return mRTImpactsinkFMAProp; }
            set
            {
                mRTImpactsinkFMAProp = value;
                RaisePropertyChanged("RTImpactsinkFMAProp");
            }
        }
        private int mSinkHoursPriceSetProp;
        public int SinkHoursPriceSetProp
        {
            get { return mSinkHoursPriceSetProp; }
            set
            {
                mSinkHoursPriceSetProp = value;
                RaisePropertyChanged("SinkHoursPriceSetProp");
            }
        }

        private int mTotalHoursPriceSetProp;
        public int TotalHoursPriceSetProp
        {
            get { return mTotalHoursPriceSetProp; }
            set
            {
                mTotalHoursPriceSetProp = value;
                RaisePropertyChanged("TotalHoursPriceSetProp");
            }
        }

        private string mChkgenraltype;
        public string Chkgenraltype
        {
            get
            {
                return mChkgenraltype;
            }
            set
            {
                mChkgenraltype = value;
                RaisePropertyChanged("Chkgenraltype");
            }
        }
        private string mChkSubmmissiontype;
        public string ChkSubmmissiontype
        {
            get
            {
                return mChkSubmmissiontype;
            }
            set
            {
                mChkSubmmissiontype = value;
                RaisePropertyChanged("ChkSubmmissiontype");
            }
        }
        private bool mgrpSearch;
        public bool grpSearch
        {
            get
            {
                return mgrpSearch;
            }
            set
            {
                mgrpSearch = value;
                RaisePropertyChanged("grpSearch");
            }
        }
        private string mStausName;
        public string StausName
        {
            get
            {
                return mStausName;
            }
            set
            {
                mStausName = value;
                RaisePropertyChanged("StausName");
            }
        }
        private List<string> mStaus;
        public List<string> Staus
        {
            get { return mStaus; }
            set
            {
                mStaus = value;
                RaisePropertyChanged("Staus");
            }
        }
        private string mStausNameSelectedItem;
        public string StausNameSelectedItem
        {
            get { return mStausNameSelectedItem; }
            set
            {
                mStausNameSelectedItem = value;
                RaisePropertyChanged("StausNameSelectedItem");
            }
        }
        private List<string> mStatus;
        public List<string> Status
        {
            get { return mStatus; }
            set
            {
                mStatus = value;
                RaisePropertyChanged("Status");
            }
        }
        private string mStatusNameSelectedItem = "All";
        public string StatusNameSelectedItem
        {
            get { return mStatusNameSelectedItem; }
            set
            {
                mStatusNameSelectedItem = value;
                RaisePropertyChanged("StatusNameSelectedItem");
            }
        }



        private DateTime mStartSelectedDate1 = DateTime.Today;
        public DateTime StartSelectedDate1
        {
            get { return mStartSelectedDate1; }
            set
            {
                mStartSelectedDate1 = value;
                RaisePropertyChanged("StartSelectedDate1");
            }
        }
        private DateTime mEndSelectedDate1 = DateTime.Today.AddDays(1);
        public DateTime EndSelectedDate1
        {
            get { return mEndSelectedDate1; }
            set
            {
                mEndSelectedDate1 = value;
                RaisePropertyChanged("EndSelectedDate1");
            }
        }

        public Worksheet WS { get; private set; }

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            StatusList();
            //ExportToExcelCommand1 = new GalaSoft.MvvmLight.Command.RelayCommand(() => ExportToExcelMessage());
            List<string> tempTypeList = new List<string>();
            tempTypeList.Add("General Notification");
            tempTypeList.Add("Submission Notification");
            NotificationType = tempTypeList.ToList();
            NotificationTypeSelectedItem = NotificationType.FirstOrDefault();
            List<string> tempstatusList = new List<string>();
            tempstatusList.Add("ACCEPTED");
            tempstatusList.Add("ERRORS");
            Staus = tempstatusList.ToList();
            StausNameSelectedItem = Staus.FirstOrDefault();
            RefreshAllCommand = new DelegateCommand(() => Refreshall());
            RefreshCommand = new DelegateCommand(() => Refresh());
            RefreshCommandops = new DelegateCommand(() => Refreshops());
            ExportToExcelCommand = new DelegateCommand(() => ExportToExcel());
            ExportToExcelCommandGrid2 = new DelegateCommand(() => ExportToExcelGrid2());
            ExportToExcelCommandSinkGrid1 = new DelegateCommand(() => ExportToExcelSink());
            ExportToExcelCommandSinkGrid2 = new DelegateCommand(() => ExportToExcelSinkGrid2());
            StartSelectedDate = DateTime.Today;
            DateTime now = DateTime.Now;
            StartSelectedDate1 = new DateTime(now.Year, now.Month, 1);
            EndSelectedDate1 = StartSelectedDate1.AddMonths(1).AddDays(-1);
            Timmer();
            Refreshall();

        }

        private void Refreshops()
        {
            try
            {
                //string status = string.Empty;
                //status = "All";
                //List<Message> templist = _dataService.GetMessages(StartSelectedDate1.Date, EndSelectedDate1.Date, status);
                //mStatusNameSelectedItem = "All";
                //RaisePropertyChanged("StatusNameSelectedItem");
                //MessageList = templist;
                List<RTImpactModel> templist2 = _dataService.GetRTImpactSink(StartSelectedDate.Date).Item1;
                int SinkHoursCleared = _dataService.GetRTImpactSink(StartSelectedDate.Date).Item2;
                List<RTImpactModel> templist4 = _dataService.GetRTImpactSinkFMA(StartSelectedDate.Date).Item1;
                int SinkHoursPriceSet = _dataService.GetRTImpactSinkFMA(StartSelectedDate.Date).Item2;
                double SinkPercentage = ((double)SinkHoursPriceSet / (double)SinkHoursCleared) * 100;

                SinkPercentageProp = SinkPercentage;


                RTImpactsinkProp = templist2.ToList();
                SinkHoursClearedProp = SinkHoursCleared;
                RTImpactsinkFMAProp = templist4.ToList();
                SinkHoursPriceSetProp = SinkHoursPriceSet;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        public void Timmer()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(5);
            var timer = new System.Threading.Timer((e) =>
            {
                Refresh();
                Refreshops();
            }, null, startTimeSpan, periodTimeSpan);
        }

        public void StatusList()
        {
            List<string> tempstatusList = new List<string>();
            tempstatusList.Add("All");
            tempstatusList.Add("Active");
            tempstatusList.Add("Cancelled");
            Status = tempstatusList;
        }
        public void Refreshall()
        {
            Refresh();
            Refreshops();
            int TotalHoursCleared = Math.Max(SourceHoursClearedProp, SinkHoursClearedProp);
            int TotalHoursPriceSet = SourceHoursPriceSetProp + SinkHoursPriceSetProp;
            double TotalPercentage = (double)SourcePercentageProp + (double)SinkPercentageProp;
            string formattedTotalPercentageF2 = TotalPercentage.ToString("F2");
            string formattedTotalPercentage = formattedTotalPercentageF2 + "%";
            TotalHoursClearedProp = TotalHoursCleared;
            TotalHoursPriceSetProp = TotalHoursPriceSet;
            TotalPercentageProp = formattedTotalPercentage;
        }

        public void Refresh()
        {
            try
            {

                List<RTImpactModel> templist = _dataService.GetRTImpactsource(StartSelectedDate.Date).Item1;
                int sourceHoursCleared = _dataService.GetRTImpactsource(StartSelectedDate.Date).Item2;
                // List<RTImpactModel> templist2= _dataService.GetRTImpactSink(StartSelectedDate1.Date).Item1;
                List<RTImpactModel> templist3 = _dataService.GetRTImpactSourceFMA(StartSelectedDate.Date).Item1;
                int SourceHoursPriceSet = _dataService.GetRTImpactSourceFMA(StartSelectedDate.Date).Item2;
                RTImpactsourceProp = templist.ToList();
                SourceHoursClearedProp = sourceHoursCleared;
                // RTImpactsinkProp = templist2.ToList();
                RTImpactsourceFMAProp = templist3.ToList();
                SourceHoursPriceSetProp = SourceHoursPriceSet;

                double SourcePercentage = ((double)SourceHoursPriceSet / (double)sourceHoursCleared) * 100;

                SourcePercentageProp = SourcePercentage;



            }
            catch (Exception)
            {

                throw;
            }
        }


        private void ExportToExcel()
        {
            try
            {
                // Combine data from all four DataGrids into a single list
                List<RTImpactModel> allData = new List<RTImpactModel>();

                // Source Prop DataGrid
                AddDataFromCollection(allData, RTImpactsourceProp);

                // Source FMA Prop DataGrid
                AddDataFromCollection(allData, RTImpactsourceFMAProp);
                // Check if there is any data to export
                if (allData.Count > 0)
                {
                    // Generate Excel report
                    ExportToExcelNotification<RTImpactModel, List<RTImpactModel>> objSource = new ExportToExcelNotification<RTImpactModel, List<RTImpactModel>>();
                    objSource.dataToPrint = allData;
                    objSource.GenerateReport("Combined Data from DataGrids");
                }
                else
                {
                    MessageBox.Show("No data to export.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddDataFromCollection(List<RTImpactModel> dataList, IEnumerable collection)
        {
            foreach (var item in collection)
            {
                dataList.Add((RTImpactModel)item);
            }
        }

        private void ExportToExcelGrid2()
        {
            if (RTImpactsourceFMAProp.Count > 0)
            {
                try
                {
                    ExportToExcelNotification<RTImpactModel, List<RTImpactModel>> objSource = new ExportToExcelNotification<RTImpactModel, List<RTImpactModel>>();
                    List<RTImpactModel> lstPathwise = new List<RTImpactModel>();
                    ICollectionView viewSource = CollectionViewSource.GetDefaultView(RTImpactsourceFMAProp);
                    foreach (var item in viewSource.SourceCollection)
                    {
                        lstPathwise.Add((RTImpactModel)item);
                    }
                    objSource.dataToPrint = lstPathwise;
                    objSource.GenerateReport("DAM Impact source Price set");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ExportToExcelSink()
        {
            try
            {
                // Combine data from all four DataGrids into a single list
                List<RTImpactModel> allData = new List<RTImpactModel>();

                // Source Prop DataGrid
                AddDataFromCollection(allData, RTImpactsinkProp);

                // Source FMA Prop DataGrid
                AddDataFromCollection(allData, RTImpactsinkFMAProp);
                // Check if there is any data to export
                if (allData.Count > 0)
                {
                    // Generate Excel report
                    ExportToExcelNotification<RTImpactModel, List<RTImpactModel>> objSource = new ExportToExcelNotification<RTImpactModel, List<RTImpactModel>>();
                    objSource.dataToPrint = allData;
                    objSource.GenerateReport("Combined Data from DataGrids");
                }
                else
                {
                    MessageBox.Show("No data to export.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ExportToExcelSinkGrid2()
        {
            if (RTImpactsinkFMAProp.Count > 0)
            {
                try
                {
                    ExportToExcelNotification<RTImpactModel, List<RTImpactModel>> objSource = new ExportToExcelNotification<RTImpactModel, List<RTImpactModel>>();
                    List<RTImpactModel> lstPathwise = new List<RTImpactModel>();
                    ICollectionView viewSource = CollectionViewSource.GetDefaultView(RTImpactsinkFMAProp);
                    foreach (var item in viewSource.SourceCollection)
                    {
                        lstPathwise.Add((RTImpactModel)item);
                    }
                    objSource.dataToPrint = lstPathwise;
                    objSource.GenerateReport("DAM Impact Sink Price set");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }

    public class DataGridBackColorConverter : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush();
            double number;

            if (value != null)
            {
                double.TryParse(value.ToString(), out number);
                if (value.ToString() == "ERRORS")
                    mybrush = new SolidColorBrush(Colors.Red);

            }
            return mybrush;
        }
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class DataGridColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if value is 1
            if (value != null && value.ToString() == "1")
            {
                return Brushes.Green; // Change color to desired color if value is 1
            }
            else
            {
                return Brushes.Transparent; // No color if value is not 1
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueToForegroundColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1].ToString() == "1")
            {

            }
            // Ensure we have both values
            //if (values.Length == 2 && values[0] != null && values[1] != null)
            {
                // Extract values from the array
                var selectedItem = values[0];
                var cellContent = values[1].ToString();


                if (selectedItem != null && cellContent == "1")
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }
            // Default color
            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
