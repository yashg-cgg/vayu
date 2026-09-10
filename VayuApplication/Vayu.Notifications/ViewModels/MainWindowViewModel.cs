using Microsoft.Office.Interop.Excel;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.Notifications.Model;

namespace Vayu.Notifications.ViewModels
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

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand RefreshCommandops { get; private set; }
        public DelegateCommand ExportToExcelCommand { get; private set; }
        public DelegateCommand RefreshCommand1 { get; private set; }
        public DelegateCommand ExportToExcelCommand1 { get; private set; }

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
                    ChkSubmmissiontype = "Hidden";
                    grpSearch = false;
                }
                else
                {
                    Chkgenraltype = "Hidden";
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

        private List<Notification> mNotificationList;
        public List<Notification> NotificationList
        {
            get { return mNotificationList; }
            set
            {
                mNotificationList = value;
                RaisePropertyChanged("NotificationList");
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



        private List<Message> mMessageList;
        public List<Message> MessageList
        {
            get { return mMessageList; }
            set
            {
                mMessageList = value;
                RaisePropertyChanged("MessageList");
            }
        }

        public Worksheet WS { get; private set; }

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            StatusList();
            RefreshCommand1 = new DelegateCommand(() => RefreshCmd());
            ExportToExcelCommand1 = new DelegateCommand(() => ExportToExcelMessage());
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
            RefreshCommand = new DelegateCommand(() => Refresh());
            RefreshCommandops = new DelegateCommand(() => Refreshops());
            ExportToExcelCommand = new DelegateCommand(() => ExportToExcel());
            StartSelectedDate = DateTime.Today;
            DateTime now = DateTime.Now;
            StartSelectedDate1 = new DateTime(now.Year, now.Month, 1);
            EndSelectedDate1 = StartSelectedDate1.AddMonths(1).AddDays(-1);
            Timmer();

        }

        private void Refreshops()
        {
            try
            {
                string status = string.Empty;
                status = "All";
                List<Message> templist = _dataService.GetMessages(StartSelectedDate1.Date, EndSelectedDate1.Date, status);
                mStatusNameSelectedItem = "All";
                RaisePropertyChanged("StatusNameSelectedItem");
                MessageList = templist;

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
                RefreshCmd();
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

        public void Refresh()
        {
            try
            {
                string status = string.Empty;
                if (NotificationTypeSelectedItem == "General Notification")
                    status = "Nostatus";
                if (NotificationTypeSelectedItem == "Submission Notification")
                {
                    if (StausNameSelectedItem == null)
                    {
                        status = "Nostatus";
                    }
                    else
                    {
                        status = StausNameSelectedItem;
                    }
                }
                List<Notification> templist = _dataService.GetNotification(StartSelectedDate.Date, EndSelectedDate.Date, NotificationTypeSelectedItem, status);

                NotificationList = templist.OrderByDescending(o => o.IssuedTime).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void RefreshCmd()
        {
            try
            {
                string status = string.Empty;
                if (StatusNameSelectedItem == "All")
                {
                    status = "All";
                }
                else
                {
                    status = StatusNameSelectedItem;
                }
                List<Message> templist = _dataService.GetMessages(StartSelectedDate1.Date, EndSelectedDate1.Date, status);

                MessageList = templist;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void ExportToExcelMessage()
        {

            if (MessageList.Count > 0)
            {
                try
                {
                    ExportToExcelMessages<Message, List<Message>> objSource = new ExportToExcelMessages<Message, List<Message>>();
                    List<Message> lstPathwise = new List<Message>();
                    ICollectionView viewSource = CollectionViewSource.GetDefaultView(MessageList);
                    foreach (var item in viewSource.SourceCollection)
                    {
                        lstPathwise.Add((Message)item);
                    }
                    objSource.dataToPrint = lstPathwise;
                    objSource.GenerateReport(StatusNameSelectedItem);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


            }
            else
            {
                MessageBox.Show("No data to export");
                return;
            }
        }



        private void ExportToExcel()
        {
            if (NotificationList.Count > 0)
            {
                try
                {
                    ExportToExcelNotification<Notification, List<Notification>> objSource = new ExportToExcelNotification<Notification, List<Notification>>();
                    List<Notification> lstPathwise = new List<Notification>();
                    ICollectionView viewSource = CollectionViewSource.GetDefaultView(NotificationList);
                    foreach (var item in viewSource.SourceCollection)
                    {
                        lstPathwise.Add((Notification)item);
                    }
                    objSource.dataToPrint = lstPathwise;
                    objSource.GenerateReport(NotificationTypeSelectedItem);
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ValueToForegroundColorConverter : IValueConverter
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

            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            double doubleValue = 0.0;
            double.TryParse(value.ToString(), out doubleValue);
            if (value.ToString() == "ERRORS")
                brush = new SolidColorBrush(Colors.Red);

            return brush;
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
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
