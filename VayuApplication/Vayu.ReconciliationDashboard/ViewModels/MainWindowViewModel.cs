using Microsoft.Win32;
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
using Vayu.ReconciliationDashboard.Model;

namespace Vayu.ReconciliationDashboard.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        private DateTime today = DateTime.Today;
        public DelegateCommand RunRefreshommand { private set; get; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
        public DelegateCommand yesterdayDateCommand { private set; get; }
        public DelegateCommand MtdDateCommand { private set; get; }
        public DelegateCommand YtdDateCommand { private set; get; }
        public DelegateCommand incdDateCommand { private set; get; }
        public DelegateCommand TodaysDateCommand { private set; get; }
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
                //Refresh();
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
                //  Refresh();
            }
        }
        private bool mUTCChecked = true;
        public bool UTCChecked
        {
            get
            {
                return mUTCChecked;
            }
            set
            {
                mUTCChecked = value;
                RaisePropertyChanged("UTCChecked");
            }
        }

        private bool mVIRTUALChecked;
        public bool VIRTUALChecked
        {
            get
            {
                return mVIRTUALChecked;
            }
            set
            {
                mVIRTUALChecked = value;
                RaisePropertyChanged("VIRTUALChecked");
            }
        }

        private bool mFTRChecked;
        public bool FTRChecked
        {
            get
            {
                return mFTRChecked;
            }
            set
            {
                mFTRChecked = value;
                RaisePropertyChanged("FTRChecked");
            }
        }


        private bool mAllProductChecked;
        public bool AllProductChecked
        {
            get
            {
                return mAllProductChecked;
            }
            set
            {
                mAllProductChecked = value;
                RaisePropertyChanged("AllProductChecked");
            }
        }


        private bool mPJMChecked = true;
        public bool PJMChecked
        {
            get
            {
                return mPJMChecked;
            }
            set
            {
                mPJMChecked = value;
                RaisePropertyChanged("PJMChecked");
            }
        }


        private bool mMISOChecked;
        public bool MISOChecked
        {
            get
            {
                return mMISOChecked;
            }
            set
            {
                mMISOChecked = value;
                RaisePropertyChanged("MISOChecked");
            }
        }

        private bool mERCOTChecked;
        public bool ERCOTChecked
        {
            get
            {
                return mERCOTChecked;
            }
            set
            {
                mERCOTChecked = value;
                RaisePropertyChanged("ERCOTChecked");
                Refresh();
            }
        }
        private bool mALLChecked;
        public bool ALLChecked
        {
            get
            {
                return mALLChecked;
            }
            set
            {
                mALLChecked = value;
                RaisePropertyChanged("ALLChecked");
            }
        }
        private double? mTotalPNL;
        public double? TotalPNL
        {
            get
            {
                return mTotalPNL;
            }
            set
            {
                mTotalPNL = value;
                RaisePropertyChanged("TotalPNL");
            }
        }

        private double? mTotalFee;
        public double? TotalFee
        {
            get
            {
                return mTotalFee;
            }
            set
            {
                mTotalFee = value;
                RaisePropertyChanged("TotalFee");
            }
        }
        private double? mTotalNetPNL;
        public double? TotalNetPNL
        {
            get
            {
                return mTotalNetPNL;
            }
            set
            {
                mTotalNetPNL = value;
                RaisePropertyChanged("TotalNetPNL");
            }
        }
        private double? mTotalMWs;
        public double? TotalMWs
        {
            get
            {
                return mTotalMWs;
            }
            set
            {
                mTotalMWs = value;
                RaisePropertyChanged("TotalMWs");
            }
        }

        private double? mTimeRangeISO;
        public double? TimeRangeISO
        {
            get
            {
                return mTimeRangeISO;
            }
            set
            {
                mTimeRangeISO = value;
                RaisePropertyChanged("TimeRangeISO");
            }
        }


        private double? mCurrentTotalMWs;
        public double? CurrentTotalMWs
        {
            get
            {
                return mCurrentTotalMWs;
            }
            set
            {
                mCurrentTotalMWs = value;
                RaisePropertyChanged("CurrentTotalMWs");
            }
        }

        private double? mCurrentTotalNetPNL;
        public double? CurrentTotalNetPNL
        {
            get
            {
                return mCurrentTotalNetPNL;
            }
            set
            {
                mCurrentTotalNetPNL = value;
                RaisePropertyChanged("CurrentTotalNetPNL");
            }
        }

        private double? mCurrentISO;
        public double? CurrentISO
        {
            get
            {
                return mCurrentISO;
            }
            set
            {
                mCurrentISO = value;
                RaisePropertyChanged("CurrentISO");
            }
        }

        private double? mCurrentTotalPNL;
        public double? CurrentTotalPNL
        {
            get
            {
                return mCurrentTotalPNL;
            }
            set
            {
                mCurrentTotalPNL = value;
                RaisePropertyChanged("CurrentTotalPNL");
            }
        }

        private double? mCurrentTotalFee;
        public double? CurrentTotalFee
        {
            get
            {
                return mCurrentTotalFee;
            }
            set
            {
                mCurrentTotalFee = value;
                RaisePropertyChanged("CurrentTotalFee");
            }
        }
        private List<ReconcilationMTLY> mPNLList;
        public List<ReconcilationMTLY> PNLList
        {
            get
            {
                return mPNLList;
            }
            set
            {
                mPNLList = value;
                RaisePropertyChanged("PNLList");
            }
        }

        private List<ReconcilationDLY> mPNLDailyList;
        public List<ReconcilationDLY> PNLDailyList
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

        private List<ReconcilationMTLY> mPNLListAll;
        public List<ReconcilationMTLY> PNLListAll
        {
            get
            {
                return mPNLListAll;
            }
            set
            {
                mPNLListAll = value;
                RaisePropertyChanged("PNLListAll");
            }
        }

        private List<ReconcilationDLY> mPNLDailyListAll;
        public List<ReconcilationDLY> PNLDailyListAll
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


        private List<string> mProductList;
        public List<string> ProductList
        {
            get
            {
                return mProductList;
            }
            set
            {
                mProductList = value;
                //if (SelectedProduct == "CRR" && SelectedMarket == "ERCOT")
                //{
                //    PNLList = null;
                //    PNLDailyList = null;
                //    CurrentTotalPNL = 0;
                //    CurrentTotalFee = 0;
                //    CurrentTotalNetPNL = 0;
                //    CurrentISO = 0;
                //    TotalPNL = 0;
                //    TotalFee = 0;
                //    TotalNetPNL = 0;
                //    TimeRangeISO = 0;
                //    iTotalPNLYTD = 0;
                //    iTotalFeeYTD = 0;
                //    iTotalNetPNLYTD = 0;
                //    YTDISO = 0;
                //    iTotalPNL = 0;
                //    iTotalFee = 0;
                //    iTotalNetPNL = 0;
                //    MessageBox.Show("Please select proper market for FTR", "InvalidProductForErcot", MessageBoxButton.OK);
                //}
                //else
                {
                    GetAllData();
                    DisplayRefresh(true);
                }
                RaisePropertyChanged("ProductList");
            }
        }
        private List<string> mMarketList;
        public List<string> MarketList
        {
            get
            {
                return mMarketList;
            }
            set
            {
                mMarketList = value;
                RaisePropertyChanged("MarketList");
            }
        }
        private string mSelectedProduct = "PTP";
        public string SelectedProduct
        {
            get
            {
                return mSelectedProduct;
            }
            set
            {
                mSelectedProduct = value;

                // ResetValues(false);
                //if (SelectedProduct == "CRR" && SelectedMarket == "ERCOT")
                //{
                //    PNLList = null;
                //    PNLDailyList = null;
                //    CurrentTotalPNL = null;
                //    CurrentTotalFee = null;
                //    CurrentISO = null;
                //    CurrentTotalNetPNL = null;
                //    TotalPNL = null;
                //    TotalFee = null;
                //    TotalNetPNL = null;
                //    TimeRangeISO = null;
                //    iTotalPNLYTD = null;
                //    iTotalFeeYTD = null;
                //    YTDISO = null;
                //    iTotalNetPNLYTD = null;
                //    iTotalPNL = null;
                //    iTotalFee = null;
                //    iTotalNetPNL = null;
                //    MessageBox.Show("Please select proper market for FTR", "InvalidProductForErcot", MessageBoxButton.OK);
                //}
                //else
                {

                    GetAllData();
                    DisplayRefresh(true);
                }
                RaisePropertyChanged("SelectedProduct");
            }
        }
        private string mSelectedMarket = "ERCOT";
        public string SelectedMarket
        {
            get
            {
                return mSelectedMarket;
            }
            set
            {
                mSelectedMarket = value;
                //if (SelectedProduct == "CRR" && SelectedMarket == "ERCOT")
                //{
                //    PNLList = null;
                //    PNLDailyList = null;
                //    CurrentTotalPNL = null;
                //    CurrentISO = null;
                //    CurrentTotalFee = null;
                //    CurrentTotalNetPNL = null;
                //    TotalPNL = null;
                //    TotalFee = null;
                //    TotalNetPNL = null;
                //    TimeRangeISO = null;
                //    iTotalPNLYTD = null;
                //    iTotalFeeYTD = null;
                //    iTotalNetPNLYTD = null;
                //    YTDISO = null;
                //    iTotalPNL = null;
                //    iTotalFee = null;
                //    iTotalNetPNL = null;
                //    MessageBox.Show("Please select proper market for FTR", "InvalidProductForErcot", MessageBoxButton.OK);
                //}
                //else
                {

                    GetAllData();
                    DisplayRefresh(true);
                }
                RaisePropertyChanged("SelectedMarket");
            }
        }
        private double? miTotalPNL;
        public double? iTotalPNL
        {
            get
            {
                return miTotalPNL;
            }
            set
            {
                miTotalPNL = value;
                RaisePropertyChanged("iTotalPNL");
            }
        }

        private double? miTotalFee;
        public double? iTotalFee
        {
            get
            {
                return miTotalFee;
            }
            set
            {
                miTotalFee = value;
                RaisePropertyChanged("iTotalFee");
            }
        }
        private double? miTotalNetPNL;
        public double? iTotalNetPNL
        {
            get
            {
                return miTotalNetPNL;
            }
            set
            {
                miTotalNetPNL = value;
                RaisePropertyChanged("iTotalNetPNL");
            }
        }
        private double? miTotalPNLYTD;
        public double? iTotalPNLYTD
        {
            get
            {
                return miTotalPNLYTD;
            }
            set
            {
                miTotalPNLYTD = value;
                RaisePropertyChanged("iTotalPNLYTD");
            }
        }

        private double? miTotalFeeYTD;
        public double? iTotalFeeYTD
        {
            get
            {
                return miTotalFeeYTD;
            }
            set
            {
                miTotalFeeYTD = value;
                RaisePropertyChanged("iTotalFeeYTD");
            }
        }
        private double? miTotalNetPNLYTD;
        public double? iTotalNetPNLYTD
        {
            get
            {
                return miTotalNetPNLYTD;
            }
            set
            {
                miTotalNetPNLYTD = value;
                RaisePropertyChanged("iTotalNetPNLYTD");
            }
        }

        private double? miTotalMWsYTD;
        public double? iTotalMWsYTD
        {
            get
            {
                return miTotalMWsYTD;
            }
            set
            {
                miTotalMWsYTD = value;
                RaisePropertyChanged("iTotalMWsYTD");
            }
        }

        private double? mYTDISO;
        public double? YTDISO
        {
            get
            {
                return mYTDISO;
            }
            set
            {
                mYTDISO = value;
                RaisePropertyChanged("YTDISO");
            }
        }
        public void SetMarket()
        {
            List<string> marketList = new List<string>();
            //marketList.Add("PJM");
            marketList.Add("ERCOT");
            MarketList = marketList;
        }
        public MainWindowViewModel(IDataService dataService = null)
        {
            _dataService = dataService;
            StartDate = new DateTime(today.Year, today.Month, 01);
            EndDate = DateTime.Today;
            //if (SelectedMarket == "PJO")
            //    StartDate = new DateTime(2018, 05, 18);
            //else
            //    StartDate = new DateTime(2019, 04, 05);
            //EndDate = DateTime.Today.AddDays(-1);\
            var dt = DateTime.Now;
            StartDate = new DateTime(dt.Year, dt.Month, 1);
            EndDate = DateTime.Today;
            SetMarket();
            ProductList = new List<string> { "PTP" };
            //ProductList = new List<string> { "PTP" };
            RunRefreshommand = new DelegateCommand(RefreshMtdDateClick);
            RunExportCSVCommand = new DelegateCommand(ExportCSVData);
            TodaysDateCommand = new DelegateCommand(TodaysDateClick);
            yesterdayDateCommand = new DelegateCommand(YesterdayDateClick);
            MtdDateCommand = new DelegateCommand(MtdDateClick);
            YtdDateCommand = new DelegateCommand(YtdDateClick);
            incdDateCommand = new DelegateCommand(incdDateClick);
            //Refresh();
            GetAllData();
        }

        private void incdDateClick()
        {
            try
            {
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }

                else
                {
                    if (SelectedMarket == "PJM")
                        StartDate = new DateTime(2018, 05, 18);
                    else
                        StartDate = new DateTime(2018, 04, 05);
                    EndDate = DateTime.Today.AddDays(0);
                    DisplayYesterday(false);
                    DisplayToday(false);
                    DisplayMTD(false);
                    DisplayYTD(false);
                    DisplayInception(true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void YesterdayDateClick()
        {
            try
            {
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                    StartDate = DateTime.Today.AddDays(-1);
                    EndDate = DateTime.Today.AddDays(-1);
                    DisplayYesterday(true);
                    DisplayToday(false);
                    DisplayMTD(false);
                    DisplayYTD(false);
                    DisplayInception(false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void MtdDateClick()
        {
            try
            {
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                    var dt = DateTime.Now;
                    StartDate = new DateTime(dt.Year, dt.Month, 1);
                    EndDate = DateTime.Today;
                    DisplayYesterday(false);
                    DisplayToday(false);
                    DisplayMTD(true);
                    DisplayYTD(false);
                    DisplayInception(false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void RefreshMtdDateClick()
        {
            try
            {
                GetAllData();
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                    var dt = DateTime.Now;
                    DisplayYesterday(false);
                    DisplayToday(false);
                    DisplayMTDRefresh(true);
                    DisplayYTD(false);
                    DisplayInception(false);
                    DisplayRefresh(false);
                }

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
                List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
                List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();
                List<string> stMonthList = new List<string>();
                DateTime sDate = StartDate;
                DateTime eDate = EndDate;
                string st = sDate.ToString("MMM");
                currentdatePNLList = PNLListAll.Where(x => x.DateFormat >= sDate && x.DateFormat <= eDate).ToList<ReconcilationMTLY>();

                // currentdatePNLList = PNLListAll.Where(x => (stMonthList.Contains(x.Month || (x.Month == "CurrentMonth")).ToList<ReconcilationMTLY>();
                if (currentdatePNLList.Count == 0)
                {
                    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                }
                currentmonthPNLList = PNLDailyListAll.Where(x => x.DateFormat >= sDate && x.DateFormat <= eDate).ToList<ReconcilationDLY>();
                if (ShowData)
                {
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;
                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                        item.DailyRunningTotal = runningtotal;
                        currentdailylist.Add(item);
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        //runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                        runningtotalm = runningtotalm + (item.PNL == null ? item.Gross : item.PNL);
                        //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                        double? a = (item.PNL == null ? item.Gross : item.PNL);
                        item.MonthlyRunningTotal = runningtotalm;
                        item.MWs = item.MWs == null ? 0 : item.MWs;
                    }
                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;

                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }
                //if (currentmonthPNLList.Count > 0)
                //{
                //    iTotalPNLYTD = currentmonthPNLList.Sum(x => x.DailyGross);
                //    iTotalFeeYTD = currentmonthPNLList.Sum(x => x.DailyFee);
                //    iTotalNetPNLYTD = currentmonthPNLList.Sum(x => x.DailyPNL);
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void TodaysDateClick()
        {
            try
            {
                StartDate = DateTime.Today;
                EndDate = DateTime.Today;
                DisplayYesterday(false);
                DisplayToday(true);
                DisplayMTD(false);
                DisplayYTD(false);
                DisplayInception(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void YtdDateClick()
        {
            try
            {
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                    var dt = DateTime.Now;
                    StartDate = new DateTime(dt.Year, 1, 1);
                    EndDate = DateTime.Today;
                    DisplayYesterday(false);
                    DisplayToday(false);
                    DisplayMTD(false);
                    DisplayYTD(true);
                    DisplayInception(false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void GetAllData()
        {
            PNLList = null;
            PNLDailyList = null;
            CurrentTotalPNL = null;
            CurrentISO = null;
            CurrentTotalFee = null;
            CurrentTotalNetPNL = null;
            CurrentTotalMWs = null;
            TotalPNL = null;
            TotalFee = null;
            TotalNetPNL = null;
            TotalMWs = null;
            TimeRangeISO = null;
            iTotalPNLYTD = null;
            iTotalFeeYTD = null;
            iTotalNetPNLYTD = null;
            YTDISO = null;
            iTotalPNL = null;
            iTotalFee = null;
            iTotalNetPNL = null;
            iTotalMWsYTD = null;
            //if (SelectedProduct == "CRR" && SelectedMarket == "ERCOT")
            //{
            //    MessageBox.Show("Please select proper market for FTR", "InvalidProductForErcot", MessageBoxButton.OK);
            //}
            //else
            {
                int marketkey = 1;
                DateTime sdate = new DateTime();
                DateTime edate = new DateTime();
                if (SelectedProduct == "PTP")
                {
                    if (SelectedMarket == "PJM")
                    {
                        marketkey = 1;
                        sdate = new DateTime(2018, 05, 01);
                    }
                    else if (SelectedMarket == "ERCOT")
                    {
                        marketkey = 9;
                        sdate = new DateTime(2018, 08, 01);
                    }
                    edate = EndDate;
                    DataService ds = new DataService();
                    PNLListAll = ds.GetReconcilationList(sdate, DateTime.Today, marketkey);
                    PNLDailyListAll = ds.GetReconcilationListDaily(sdate, DateTime.Today, marketkey).OrderBy(x => x.DateFormat).ToList<ReconcilationDLY>();
                    DisplayYesterday(false);
                    DisplayToday(false);
                    DisplayMTD(true);
                    DisplayYTD(false);
                    DisplayInception(false);
                }

                if (SelectedProduct == "CRR")
                {
                    if (SelectedMarket == "PJM")
                    {
                        marketkey = 1;
                        sdate = new DateTime(2020, 09, 01);
                    }
                    else
                    {
                        marketkey = 9;
                        sdate = new DateTime(2022, 01, 01);
                    }
                    if (marketkey == 1)
                    {
                        edate = EndDate;
                        DataService ds = new DataService();
                        PNLListAll = ds.GetMonthlyFTR(sdate, DateTime.Today, marketkey);
                        PNLDailyListAll = ds.GetDailyFTR(sdate, DateTime.Today, marketkey).OrderBy(x => x.DateFormat).ToList<ReconcilationDLY>();

                    }
                    if (marketkey == 9)
                    {
                        edate = EndDate;
                        DataService ds = new DataService();
                        PNLListAll = ds.GetMonthlyCRR(sdate, DateTime.Today, marketkey);
                        PNLDailyListAll = ds.GetDailyCRR(sdate, DateTime.Today, marketkey).OrderBy(x => x.DateFormat).ToList<ReconcilationDLY>();

                    }
                    DisplayYesterday(false);
                    DisplayToday(false);
                    DisplayMTD(true);
                    DisplayYTD(false);
                    DisplayInception(false);


                }
                Mouse.OverrideCursor = null;
            }
        }

        private void DisplayYesterday(bool ShowData)
        {
            try
            {
                List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
                List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();
                DateTime date = DateTime.Today;
                string st = date.ToString("MMM");
                currentdatePNLList = PNLListAll.Where(x => x.Month == st + " " + date.Year).ToList<ReconcilationMTLY>();
                if (currentdatePNLList.Count == 0)
                {
                    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                }
                currentmonthPNLList = PNLDailyListAll.Where(x => x.DateFormat == DateTime.Today.AddDays(-1)).ToList<ReconcilationDLY>();
                if (ShowData)
                {
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;
                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                            //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                            item.MonthlyRunningTotal = runningtotalm;
                            item.MWs = item.MWs == null ? 0 : item.MWs;
                        }
                    }
                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;
                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DisplayToday(bool ShowData)
        {
            try
            {
                List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
                List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();
                DateTime date = DateTime.Today;
                string st = date.ToString("MMM");
                currentdatePNLList = PNLListAll.Where(x => x.Month == st + " " + date.Year).ToList<ReconcilationMTLY>();
                if (currentdatePNLList.Count == 0)
                {
                    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                }
                currentmonthPNLList = PNLDailyListAll.Where(x => x.DateFormat == DateTime.Today.AddDays(0)).ToList<ReconcilationDLY>();
                if (currentdatePNLList.Count == 0)
                {
                    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentDate").ToList<ReconcilationMTLY>();
                }
                if (ShowData)
                {
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;
                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        if (SelectedMarket == "PJM")
                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                        else if (SelectedMarket == "ERCOT")

                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee;// + item.ISOMiscellaneousCharges;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        // if (item.DateFormat <= maxdate)
                        {
                            runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                            //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                            item.MonthlyRunningTotal = runningtotalm;
                            item.MWs = item.MWs == null ? 0 : item.MWs;
                        }
                    }
                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;
                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }
                if (currentmonthPNLList.Count > 0)
                {
                    foreach (ReconcilationDLY itemcurrentdatePNLList in currentmonthPNLList)
                    {
                        CurrentTotalPNL = itemcurrentdatePNLList.DailyGross;
                        CurrentTotalFee = itemcurrentdatePNLList.DailyFee;
                        CurrentISO = itemcurrentdatePNLList.ISOMiscellaneousCharges;
                        CurrentTotalNetPNL = itemcurrentdatePNLList.DailyPNL;
                        CurrentTotalMWs = itemcurrentdatePNLList.MWs;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DisplayMTD(bool ShowData)
        {
            try
            {
                List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
                List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();
                //DateTime date = DateTime.Today;
                //DateTime sDate = new DateTime(date.Year, date.Month, 1);
                //DateTime eDate = DateTime.Today;
                //string st = date.ToString("MMM");
                //currentdatePNLList = PNLListAll.Where(x => x.Month == st + " " + date.Year).ToList<ReconcilationMTLY>();
                //if (currentdatePNLList.Count == 0)
                //{
                //    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                //}
                //currentmonthPNLList = PNLDailyListAll.Where(x => x.DateFormat >= sDate && x.DateFormat <= eDate).ToList<ReconcilationDLY>();

                DateTime date = StartDate;
                DateTime sDate = StartDate;
                DateTime eDate = EndDate;
                string st = date.ToString("MMM");
                currentdatePNLList = PNLListAll.Where(x => x.Month == st + " " + date.Year || x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                if (currentdatePNLList.Count == 0)
                {
                    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                }
                currentmonthPNLList = PNLDailyListAll.Where(x => x.DateFormat >= sDate && x.DateFormat <= eDate).ToList<ReconcilationDLY>();

                if (ShowData)
                {
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;
                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                            //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                            item.MonthlyRunningTotal = runningtotalm;
                            //item.MWs =MWsm;
                        }
                    }
                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;
                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }
                if (currentmonthPNLList.Count > 0)
                {
                    TotalPNL = currentmonthPNLList.Sum(x => x.DailyGross);
                    TotalFee = currentmonthPNLList.Sum(x => x.DailyFee);
                    TimeRangeISO = currentmonthPNLList.Sum(x => x.ISOMiscellaneousCharges);
                    TotalNetPNL = currentmonthPNLList.Sum(x => x.DailyPNL);
                    TotalMWs = Math.Round(Convert.ToDouble(currentmonthPNLList.Sum(x => x.MWs)), 2);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DisplayMTDRefresh(bool ShowData)
        {
            try
            {
                List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
                List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();


                DateTime date = StartDate;
                DateTime sDate = StartDate;
                DateTime eDate = EndDate;
                string st = date.ToString("MMM");
                // currentdatePNLList = PNLListAll.Where(x => x.Month == st + " " + date.Year || x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                currentdatePNLList = PNLListAll.Where(x => x.DateFormat >= sDate && x.DateFormat <= eDate || (x.Month == "CurrentMonth")).ToList<ReconcilationMTLY>();
                if (currentdatePNLList.Count == 0)
                {
                    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                }
                currentmonthPNLList = PNLDailyListAll.Where(x => x.DateFormat >= sDate && x.DateFormat <= eDate).ToList<ReconcilationDLY>();
                if (ShowData)
                {
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;

                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        // if (item.DateFormat <= maxdate)
                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                            //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                            item.MonthlyRunningTotal = runningtotalm;
                            item.MWs = item.MWs == null ? 0 : item.MWs;
                        }
                    }
                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;
                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }
                if (currentmonthPNLList.Count > 0)
                {
                    TotalPNL = currentmonthPNLList.Sum(x => x.DailyGross);
                    TotalFee = currentmonthPNLList.Sum(x => x.DailyFee);
                    TimeRangeISO = currentmonthPNLList.Sum(x => x.ISOMiscellaneousCharges);
                    TotalNetPNL = currentmonthPNLList.Sum(x => x.DailyPNL);
                    TotalMWs = Math.Round(Convert.ToDouble(currentmonthPNLList.Sum(x => x.MWs)), 2);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DisplayYTD(bool ShowData)
        {
            try
            {
                List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
                List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();
                DateTime date = StartDate;
                DateTime sDate = new DateTime(DateTime.Today.Year, 01, 1);
                // DateTime eDate = EndDate;
                DateTime eDate = DateTime.Today;
                string st = date.ToString("MMM");
                currentdatePNLList = PNLListAll.Where(x => x.Month.Contains(date.Year.ToString()) || (x.Month == "CurrentMonth")).ToList<ReconcilationMTLY>();
                //currentdatePNLList = PNLListAll.Where(x => (x.Month.Contains(date.Year.ToString())) || (x.Month == "CurrentMonth")).ToList<ReconcilationMTLY>();
                if (currentdatePNLList.Count == 0)
                {
                    currentdatePNLList = PNLListAll.Where(x => x.Month == "CurrentMonth").ToList<ReconcilationMTLY>();
                }
                currentmonthPNLList = PNLDailyListAll.Where(x => x.DateFormat >= sDate && x.DateFormat <= eDate).ToList<ReconcilationDLY>();

                if (ShowData)
                {
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;
                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        // if (item.DateFormat <= maxdate)
                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                            //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                            item.MonthlyRunningTotal = runningtotalm;
                            item.MWs = item.MWs == null ? 0 : item.MWs;
                        }
                    }
                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;
                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }
                if (ShowData)
                {
                    //List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    //double? runningtotal = 0;
                    //foreach (ReconcilationDLY item in currentmonthPNLList)
                    //{
                    //    // if (item.DateFormat <= maxdate)
                    //    {
                    //        runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                    //        item.DailyRunningTotal = runningtotal;
                    //        currentdailylist.Add(item);
                    //    }

                    //}
                    //double? runningtotalm = 0;
                    //foreach (ReconcilationMTLY item in currentdatePNLList)
                    //{
                    //    //runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                    //    //if (item.DateFormat <= maxdate)
                    //    {

                    //        runningtotalm = runningtotalm + (item.PNL == null ? item.Gross : item.PNL);
                    //        double? a = item.PNL == null ? item.Gross : item.PNL;
                    //        item.MonthlyRunningTotal = runningtotalm;
                    //    }
                    //}
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;
                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        // if (item.DateFormat <= maxdate)
                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                            //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                            item.MonthlyRunningTotal = runningtotalm;
                            item.MWs = item.MWs == null ? 0 : item.MWs;
                        }
                    }

                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;


                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }

                if (currentmonthPNLList.Count > 0)
                {
                    iTotalPNLYTD = currentmonthPNLList.Sum(x => x.DailyGross);
                    iTotalFeeYTD = currentmonthPNLList.Sum(x => x.DailyFee);
                    YTDISO = currentmonthPNLList.Sum(x => x.ISOMiscellaneousCharges);
                    iTotalNetPNLYTD = currentmonthPNLList.Sum(x => x.DailyPNL);
                    iTotalMWsYTD = Math.Round(Convert.ToDouble(currentmonthPNLList.Sum(x => x.MWs)), 2);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DisplayInception(bool ShowData)
        {
            try
            {
                List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
                List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();
                DateTime date = DateTime.Today;
                string st = date.ToString("MMM");
                currentdatePNLList = PNLListAll;
                currentmonthPNLList = PNLDailyListAll;
                if (ShowData)
                {
                    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                    double? runningtotal = 0;
                    foreach (ReconcilationDLY item in currentmonthPNLList)
                    {
                        // if (item.DateFormat <= maxdate)
                        {
                            runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                            item.DailyRunningTotal = runningtotal;
                            currentdailylist.Add(item);
                        }
                    }
                    double? runningtotalm = 0;
                    //double? MWsm = 0;
                    foreach (ReconcilationMTLY item in currentdatePNLList)
                    {
                        //runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                        //if (item.DateFormat <= maxdate)
                        {
                            runningtotalm = runningtotalm + (item.PNL == null ? item.Gross : item.PNL);
                            double? a = (item.PNL == null ? item.Gross : item.PNL);
                            //MWsm = MWsm + (item.MWs == null ? 0 : item.MWs);
                            item.MonthlyRunningTotal = runningtotalm;
                            item.MWs = item.MWs == null ? 0 : item.MWs;
                        }
                    }
                    PNLDailyList = currentmonthPNLList;
                    PNLList = currentdatePNLList;


                    if (PNLList.Count != 0)
                    {
                        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                    }
                    if (PNLDailyList.Count != 0)
                    {
                        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                    }
                }
                if (currentmonthPNLList.Count > 0)
                {
                    iTotalPNL = currentmonthPNLList.Sum(x => x.DailyGross);
                    iTotalFee = currentmonthPNLList.Sum(x => x.DailyFee);
                    iTotalNetPNL = currentmonthPNLList.Sum(x => x.DailyPNL);
                }
                //if (ShowData)
                //{
                //    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                //    double? runningtotal = 0;
                //    foreach (ReconcilationDLY item in currentmonthPNLList)
                //    {
                //        //if (item.DateFormat <= maxdate)
                //        {
                //            //runningtotal = runningtotal + item.DailyGross + item.DailyFee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                //            // item.DailyRunningTotal = runningtotal;
                //            currentdailylist.Add(item);
                //        }
                //    }
                //    //double? runningtotalm = 0;
                //    //foreach (ReconcilationMTLY item in currentdatePNLList)
                //    //{
                //    //    //runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                //    //    runningtotalm = runningtotalm + (item.PNL == null ? item.Gross : item.PNL);
                //    //    double? a = (item.PNL == null ? item.Gross : item.PNL);
                //    //    item.MonthlyRunningTotal = runningtotalm;
                //    //}
                //    //double? runningtotal = 0;
                //    //foreach (ReconcilationDLY item in currentmonthPNLList)
                //    //{
                //    //    runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                //    //    item.DailyRunningTotal = runningtotal;
                //    //    currentdailylist.Add(item);
                //    //}
                //    double? runningtotalm = 0;
                //    foreach (ReconcilationMTLY item in currentdatePNLList)
                //    {
                //        //runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                //        //if (item.DateFormat <= maxdate)
                //        {
                //            runningtotalm = runningtotalm + (item.PNL == null ? item.Gross : item.PNL);
                //            double? a = (item.PNL == null ? item.Gross : item.PNL);
                //            item.MonthlyRunningTotal = runningtotalm;
                //        }
                //    }
                //    PNLDailyList = currentmonthPNLList;
                //    PNLList = currentdatePNLList;

                //    if (PNLList.Count != 0)
                //    {
                //        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                //            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                //    }
                //    if (PNLDailyList.Count != 0)
                //    {
                //        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                //            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                //    }
                //}
                //if (ShowData)
                //{
                //    List<ReconcilationDLY> currentdailylist = new List<ReconcilationDLY>();
                //    double? runningtotal = 0;
                //    foreach (ReconcilationDLY item in currentmonthPNLList)
                //    {
                //        // if (item.DateFormat <= maxdate)
                //        {
                //            //runningtotal = runningtotal + item.DailyGross + item.DailyFee + item.ISOMiscellaneousCharges;
                //            //item.DailyRunningTotal=
                //            // item.DailyRunningTotal = runningtotal;
                //            currentdailylist.Add(item);
                //        }
                //    }
                //    double? runningtotalm = 0;
                //    foreach (ReconcilationMTLY item in currentdatePNLList)
                //    {
                //        //if (item.DateFormat <= maxdate)
                //        {

                //            runningtotalm = runningtotalm + item.Gross + item.Fee + (item.ISOMiscellaneousCharges == null ? 0 : item.ISOMiscellaneousCharges);
                //            item.MonthlyRunningTotal = runningtotalm;
                //        }
                //    }
                //    PNLDailyList = currentmonthPNLList;
                //    PNLList = currentdatePNLList;
                //    if (PNLList.Count != 0)
                //    {
                //        if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                //            PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                //    }
                //    if (PNLDailyList.Count != 0)
                //    {
                //        if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                //            PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                //    }
                //}
                //if (currentmonthPNLList.Count > 0)
                //{
                //    iTotalPNL = currentmonthPNLList.Sum(x => x.DailyGross);
                //    iTotalFee = currentmonthPNLList.Sum(x => x.DailyFee);
                //    iTotalNetPNL = currentmonthPNLList.Sum(x => x.DailyPNL);
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void Refresh()
        {
            List<ReconcilationMTLY> currentdatePNLList = new List<ReconcilationMTLY>();
            List<ReconcilationDLY> currentmonthPNLList = new List<ReconcilationDLY>();
            int marketkey = 1;
            if (SelectedMarket == "PJM")
                marketkey = 1;
            else if (SelectedMarket == "ERCOT")
                marketkey = 9;
            DataService ds = new DataService();
            var dt = DateTime.Now;
            currentdatePNLList = ds.GetReconcilationList(DateTime.Today, DateTime.Today, marketkey);
            currentmonthPNLList = ds.GetReconcilationListDaily(new DateTime(dt.Year, dt.Month, 1), DateTime.Today, marketkey);
            if (StartDate <= EndDate)
            {

                PNLList = ds.GetReconcilationList(StartDate, DateTime.Today, marketkey);
                PNLDailyList = ds.GetReconcilationListDaily(StartDate, DateTime.Today, marketkey);

                double gross = 0;
                double fee = 0;
                double net = 0;
                if (PNLList.Count > 0)
                {
                    foreach (ReconcilationDLY itemReconcilation in currentmonthPNLList)
                    {
                        try
                        {
                            if (itemReconcilation.DailyGross.HasValue)
                                gross = gross + itemReconcilation.DailyGross.Value;
                            if (itemReconcilation.DailyFee.HasValue)
                                fee = fee + itemReconcilation.DailyFee.Value;
                            if (itemReconcilation.DailyPNL.HasValue)
                                net = net + itemReconcilation.DailyPNL.Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    TotalPNL = gross;
                    TotalFee = fee;
                    TotalNetPNL = net;
                }
                double igross = 0;
                double ifee = 0;
                double inet = 0;
                if (PNLList.Count > 0)
                {
                    foreach (ReconcilationMTLY itemReconcilation in PNLList)
                    {
                        try
                        {
                            if (itemReconcilation.Gross.HasValue)
                                igross = igross + itemReconcilation.Gross.Value;
                            if (itemReconcilation.Fee.HasValue)
                                ifee = ifee + itemReconcilation.Fee.Value;
                            if (itemReconcilation.PNL.HasValue)
                                inet = inet + itemReconcilation.PNL.Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    iTotalPNL = igross;
                    iTotalFee = ifee;
                    iTotalNetPNL = igross + ifee;
                }
                if (currentdatePNLList.Count > 0)
                {
                    foreach (ReconcilationMTLY itemcurrentdatePNLList in currentdatePNLList)
                    {
                        CurrentTotalPNL = itemcurrentdatePNLList.Gross;
                        CurrentTotalFee = itemcurrentdatePNLList.Fee;
                        CurrentISO = itemcurrentdatePNLList.ISOMiscellaneousCharges;
                        CurrentTotalNetPNL = itemcurrentdatePNLList.PNL;
                    }

                }
                if (PNLList.Count != 0)
                {
                    if (PNLList[PNLList.Count - 1].Month == DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year)
                        PNLList[PNLList.Count - 1].Month = "CurrentMonth";
                }
                if (PNLDailyList.Count != 0)
                {
                    if (PNLDailyList[PNLDailyList.Count - 1].Date.ToString() == DateTime.Today.ToString())
                        PNLDailyList[PNLDailyList.Count - 1].Date = "CurrentDate";
                }
            }
            else
                MessageBox.Show("StartDate should be less than EndDate", "Date");
        }
        private void ExportCSVData()
        {
            if (PNLList != null)
            {
                SaveFileDialog savefiledialog = new SaveFileDialog();
                savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                savefiledialog.FilterIndex = 1;
                savefiledialog.RestoreDirectory = true;
                savefiledialog.FileName = "PNLSheet" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                if ((bool)savefiledialog.ShowDialog())
                {

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("Monthly");
                    foreach (PropertyInfo item in PNLList[0].GetType().GetProperties())
                    {
                        builder.Append(item.Name + ",");
                    }
                    builder.ToString().Remove(builder.Length - 1, 1);
                    builder.AppendLine();

                    foreach (ReconcilationMTLY item in PNLList)
                    {
                        foreach (PropertyInfo propName in item.GetType().GetProperties())
                        {
                            builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                        }
                        builder.AppendLine();
                    }
                    builder.AppendLine();
                    builder.AppendLine();
                    builder.AppendLine("Daily");
                    //daily
                    foreach (PropertyInfo item in PNLDailyList[0].GetType().GetProperties())
                    {
                        builder.Append(item.Name + ",");
                    }
                    builder.ToString().Remove(builder.Length - 1, 1);
                    builder.AppendLine();
                    foreach (ReconcilationDLY item in PNLDailyList)
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
                var objItem = value as ReconcilationDLY;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "dailyfee":
                                if (objItem.DailyFee < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailygross":
                                if (objItem.DailyGross < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailypnl":
                                if (objItem.DailyPNL < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailyrunningtotal":
                                if (objItem.DailyRunningTotal < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "rtm":
                                if (objItem.RTM < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dam":
                                if (objItem.DAM < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "isomiscellaneouscharges":
                                if (objItem.ISOMiscellaneousCharges < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailygrossiso":
                                if (objItem.ISOMiscellaneousCharges < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dailyfeeiso":
                                if (objItem.ISOMiscellaneousCharges < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dollarscleared":
                                if (objItem.DollarsCleared < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }

                                return new SolidColorBrush(Colors.Black);
                            case "dailyfeepermw":
                                if (objItem.DailyFeePerMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "mws":
                                if (objItem.MWs < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "net":
                                if (objItem.Net < 0)
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
    public class CellBackgroundColorConverter1 : IValueConverter
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
                var objItem = value as ReconcilationMTLY;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "fee":
                                if (objItem.Fee < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "gross":
                                if (objItem.Gross < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "monthlypnlfromstatement":
                                if (objItem.MonthlyPNLFromStatement < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "pnl":
                                if (objItem.PNL < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "monthlyrunningtotal":
                                if (objItem.MonthlyRunningTotal < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "isomiscellaneouscharges":
                                if (objItem.ISOMiscellaneousCharges < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "monthgrossiso":
                                if (objItem.ISOMiscellaneousCharges < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "monthfeeiso":
                                if (objItem.ISOMiscellaneousCharges < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "feepermw":
                                if (objItem.FeePerMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "rtm":
                                if (objItem.RTM < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "dam":
                                if (objItem.DAM < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);

                            case "mws":
                                if (objItem.MWs < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "net":
                                if (objItem.Net < 0)
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
                else

                    return new SolidColorBrush(Colors.Black);
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
                var objItem = value as ReconcilationDLY;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "dailyfee":
                                if (objItem.DailyFee == 0)
                                {
                                    return new SolidColorBrush(Colors.LightBlue);
                                }
                                return new SolidColorBrush(Colors.White);
                            case "rtm":
                                if (objItem.RTM == 0)
                                {
                                    return new SolidColorBrush(Colors.LightBlue);
                                }
                                return new SolidColorBrush(Colors.White);
                            case "dam":
                                if (objItem.DAM == 0)
                                {
                                    return new SolidColorBrush(Colors.LightBlue);
                                }
                                return new SolidColorBrush(Colors.White);
                            case "net":
                                if (objItem.Net == 0)
                                {
                                    return new SolidColorBrush(Colors.LightBlue);
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
