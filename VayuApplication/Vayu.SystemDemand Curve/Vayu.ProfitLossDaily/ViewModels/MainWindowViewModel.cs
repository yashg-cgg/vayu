using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.NodePriceLibrary;
using Vayu.ProfitLossDaily.Model;

namespace Vayu.ProfitLossDaily.ViewModels
{

    public class MainWindowViewModel : BindableBase
    {
        #region GlobalDataMembers

        private string mUser = Environment.UserName;
        private readonly IDataService _dataService;

        public DelegateCommand SelectAllCommand { private set; get; }

        public DelegateCommand RunCalcCommand { private set; get; }

        public DelegateCommand RunExportCSVCommand { private set; get; }

        public DelegateCommand ResetCommand { private set; get; }

        public DelegateCommand TodaysDateCommand { private set; get; }

        public DelegateCommand CalculateExposureCommand { private set; get; }

        private bool mSortMWChecked = true;

        public bool SortMWChecked
        {
            get
            {
                return mSortMWChecked;
            }
            set
            {
                mSortMWChecked = value;
                RaisePropertyChanged("SortMWChecked");
            }
        }

        private bool mSortDollarChecked = false;

        public bool SortDollarChecked
        {
            get
            {
                return mSortDollarChecked;
            }
            set
            {
                mSortDollarChecked = value;
                RaisePropertyChanged("SortDollarChecked");
            }
        }
        #endregion

        #region Public Properties
        private string mSelectedProduct;

        public string SelectedProduct
        {
            get
            {
                return mSelectedProduct;
            }
            set
            {
                mSelectedProduct = value;
                ResetValues(false);
                SetMarket();
                RaisePropertyChanged("SelectedProduct");
                if (SelectedProduct == "Virtual")
                {
                    IsSinkVisible = false;
                    IsIncVisible = true;
                }
                else
                {
                    IsSinkVisible = true;
                    IsIncVisible = false;
                }

            }
        }

        private bool _isDAChecked;

        public bool isDAChecked
        {
            get { return _isDAChecked; }
            set
            {
                _isDAChecked = value;
                RaisePropertyChanged("isDAChecked");
                DrawFirstGraph("DA");
            }
        }

        private bool _isRTChecked;

        public bool isRTChecked
        {
            get { return _isRTChecked; }
            set
            {
                _isRTChecked = value;
                RaisePropertyChanged("isRTChecked");
                DrawFirstGraph("RT");
            }
        }

        private bool _isRTCongChecked;

        public bool isRTCongChecked
        {
            get { return _isRTCongChecked; }
            set
            {
                _isRTCongChecked = value;
                RaisePropertyChanged("isRTCongChecked");
                DrawFirstGraph("RTCong");
            }
        }

        private bool _isDACongChecked;

        public bool isDACongChecked
        {
            get { return _isDACongChecked; }
            set
            {
                _isDACongChecked = value;
                RaisePropertyChanged("_isDACongChecked");
                DrawFirstGraph("DACong");
            }
        }
        private bool _isRTLossChecked;

        public bool isRTLossChecked
        {
            get { return _isRTLossChecked; }
            set
            {
                _isRTLossChecked = value;
                RaisePropertyChanged("isRTLossChecked");
                DrawFirstGraph("RTLoss");
            }
        }

        private bool _isDALossChecked;

        public bool isDALossChecked
        {
            get { return _isDALossChecked; }
            set
            {
                _isDALossChecked = value;
                RaisePropertyChanged("isDALossChecked");
                DrawFirstGraph("DALoss");
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

        public List<Portfolio> SelectedPortfolioList { get; set; }

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
                SetAccountList();

                if (SelectedMarket != "ERCOT External")
                {
                    ResetValues(true);
                    if (StartDate != DateTime.Today)
                    {
                        //SummaryChecked = true;
                        if (SelectedProduct.ToLower() == "virtual")
                            IsIncVisible = true;
                    }
                }

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
                SetAccountList();
                RaisePropertyChanged("EndDate");
            }
        }

        private string mSelectedMarket;

        public string SelectedMarket
        {
            get
            {
                return mSelectedMarket;
            }
            set
            {
                mSelectedMarket = value;
                SetAccountList();

                if (SelectedMarket == "ERCOT External")
                {
                    StartDate = DateTime.Today.AddDays(-61);
                    EndDate = DateTime.Today.AddDays(-61);
                }

                RaisePropertyChanged("SelectedMarket");

            }
        }

        private List<Account> mAccountList;

        public List<Account> AccountList
        {
            get
            {
                return mAccountList;
            }
            set
            {
                mAccountList = value;
                RaisePropertyChanged("AccountList");
            }
        }

        private ObservableCollection<string> mUserList;

        public ObservableCollection<string> UserList
        {
            get
            {
                return mUserList;
            }
            set
            {
                mUserList = value;
                RaisePropertyChanged("UserList");
            }
        }

        private ObservableCollection<string> mPJMUserList;

        public ObservableCollection<string> PJMUserList
        {
            get
            {
                return mPJMUserList;
            }
            set
            {
                mPJMUserList = value;
                RaisePropertyChanged("PJMUserList");
            }
        }

        private ObservableCollection<string> mERCOTUserList;
        public ObservableCollection<string> ERCOTUserList
        {
            get
            {
                return mERCOTUserList;
            }
            set
            {
                mERCOTUserList = value;
                RaisePropertyChanged("ERCOTUserList");
            }
        }

        private ObservableCollection<string> mCAISOUserList;

        public ObservableCollection<string> CAISOUserList
        {
            get
            {
                return mCAISOUserList;
            }
            set
            {
                mCAISOUserList = value;
                RaisePropertyChanged("CAISOUserList");
            }
        }

        private List<Portfolio> mPortfolioList;

        public List<Portfolio> PortfolioList
        {
            get
            {
                return mPortfolioList;
            }
            set
            {
                mPortfolioList = value;
                RaisePropertyChanged("PortfolioList");
            }
        }

        private bool mSummaryChecked = false;

        public bool SummaryChecked
        {
            get
            {
                return mSummaryChecked;
            }
            set
            {
                mSummaryChecked = value;
                RaisePropertyChanged("SummaryChecked");
                Refresh();
                IncomplteHrsChecked = false;
                if (value)
                {
                    IscomplteHrsEnabled = false;
                    IsSinkVisible = false;
                }
                else
                {
                    IscomplteHrsEnabled = true;
                }
                if (SelectedProduct == "Virtual" && !value)
                {
                    IsIncVisible = true; IsSinkVisible = false;
                }
                else
                {
                    IsIncVisible = false;
                }
                if (SelectedProduct == "UPTO" && !value)
                {
                    IsSinkVisible = true;
                }
            }
        }

        private bool mLatestHourChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [latest hourly checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [latest hourly checked]; otherwise, <c>false</c>.
        /// </value>
        public bool LatestHourlyChecked
        {
            get
            {
                return mLatestHourChecked;
            }
            set
            {
                mLatestHourChecked = value;
                RaisePropertyChanged("LatestHourlyChecked");
                Refresh();
            }
        }
        /// <summary>
        /// The m show incomplete checked
        /// </summary>
        private bool mShowIncompleteChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show incomplete checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show incomplete checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowIncompleteChecked
        {
            get
            {
                return mShowIncompleteChecked;
            }
            set
            {
                mShowIncompleteChecked = value;
                Refresh();
                RaisePropertyChanged("ShowIncompleteChecked");
            }
        }
        /// <summary>
        /// The m total PNL
        /// </summary>
        private string mTotalPnl;
        /// <summary>
        /// Gets or sets the total PNL.
        /// </summary>
        /// <value>
        /// The total PNL.
        /// </value>
        public string TotalPnl
        {
            get
            {
                return mTotalPnl;
            }
            set
            {
                mTotalPnl = value;
                RaisePropertyChanged("TotalPnl");
            }
        }
        /// <summary>
        /// The m total pay collect
        /// </summary>
        private string mTotalPayCollect;
        /// <summary>
        /// Gets or sets the total pay collect.
        /// </summary>
        /// <value>
        /// The total pay collect.
        /// </value>
        public string TotalPayCollect
        {
            get
            {
                return mTotalPayCollect;
            }
            set
            {
                mTotalPayCollect = value;
                RaisePropertyChanged("TotalPayCollect");
            }
        }
        /// <summary>
        /// The m mw
        /// </summary>
        private string mMW;
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public string MW
        {
            get
            {
                return mMW;
            }
            set
            {
                mMW = value;
                RaisePropertyChanged("MW");
            }
        }
        /// <summary>
        /// The m mw win percent
        /// </summary>
        private string mMWWinPercent;
        /// <summary>
        /// Gets or sets the mw win percent.
        /// </summary>
        /// <value>
        /// The mw win percent.
        /// </value>
        public string MWWinPercent
        {
            get
            {
                return mMWWinPercent;
            }
            set
            {
                mMWWinPercent = value;
                RaisePropertyChanged("MWWinPercent");
            }
        }
        /// <summary>
        /// The m PNL list
        /// </summary>
        private List<Pnl> mPNLList;
        /// <summary>
        /// Gets or sets the PNL list.
        /// </summary>
        /// <value>
        /// The PNL list.
        /// </value>
        public List<Pnl> PNLList
        {
            get
            {
                return mPNLList;
            }
            set
            {
                mPNLList = value;
                RaisePropertyChanged("PNLList");
                pnlChecked = true;
                DrawFirstGraph("PNL");
                DrawSecondGraph("PNL");
                DrawThirdGraph();
                DrawFourthGraph();
            }
        }
        /// <summary>
        /// The m bid detail list
        /// </summary>
        private List<BidDetails> mBidDetailList;
        /// <summary>
        /// Gets or sets the bid details.
        /// </summary>
        /// <value>
        /// The bid details.
        /// </value>
        public List<BidDetails> BidDetails
        {
            get
            {
                return mBidDetailList;
            }
            set
            {
                mBidDetailList = value;
                RaisePropertyChanged("BidDetails");
            }
        }
        /// <summary>
        /// The m total PNL
        /// </summary>
        private double? mTotalPNL;
        /// <summary>
        /// Gets or sets the total PNL.
        /// </summary>
        /// <value>
        /// The total PNL.
        /// </value>
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
        private double? mTotalDACong;
        public double? TotalDACong
        {
            get
            {
                return mTotalDACong;
            }
            set
            {
                mTotalDACong = value;
                RaisePropertyChanged("TotalDACong");
            }
        }
        private double? mTotalRTCong;
        public double? TotalRTCong
        {
            get
            {
                return mTotalRTCong;
            }
            set
            {
                mTotalRTCong = value;
                RaisePropertyChanged("TotalRTCong");
            }
        }
        private double? mTotalDALoss;
        public double? TotalDALoss
        {
            get
            {
                return mTotalDALoss;
            }
            set
            {
                mTotalDALoss = value;
                RaisePropertyChanged("TotalDALoss");
            }
        }
        private double? mTotalRTLoss;
        public double? TotalRTLoss
        {
            get
            {
                return mTotalRTLoss;
            }
            set
            {
                mTotalRTLoss = value;
                RaisePropertyChanged("TotalRTLoss");
            }
        }

        /// <summary>
        /// The m total mw
        /// </summary>
        private double? mTotalMW;
        /// <summary>
        /// Gets or sets the total mw.
        /// </summary>
        /// <value>
        /// The total mw.
        /// </value>
        public double? TotalMW
        {
            get
            {
                return mTotalMW;
            }
            set
            {
                mTotalMW = value;
                RaisePropertyChanged("TotalMW");
            }
        }
        /// <summary>
        /// The m percent mw
        /// </summary>
        private double? mPercentMW;
        /// <summary>
        /// Gets or sets the percent mw.
        /// </summary>
        /// <value>
        /// The percent mw.
        /// </value>
        public double? PercentMW
        {
            get
            {
                return mPercentMW;
            }
            set
            {
                mPercentMW = value;
                RaisePropertyChanged("PercentMW");
            }
        }
        /// <summary>
        /// The m total collect
        /// </summary>
        private double? mTotalCollect;
        /// <summary>
        /// Gets or sets the total collect.
        /// </summary>
        /// <value>
        /// The total collect.
        /// </value>
        public double? TotalCollect
        {
            get
            {
                return mTotalCollect;
            }
            set
            {
                mTotalCollect = value;
                RaisePropertyChanged("TotalCollect");
            }
        }
        /// <summary>
        /// The m win text
        /// </summary>
        private double? mWinText;
        /// <summary>
        /// Gets or sets the win text.
        /// </summary>
        /// <value>
        /// The win text.
        /// </value>
        public double? WinText
        {
            get
            {
                return mWinText;
            }
            set
            {
                mWinText = value;
                RaisePropertyChanged("WinText");
            }
        }
        /// <summary>
        /// The m total net PNL
        /// </summary>
        private double? mTotalNetPNL;
        /// <summary>
        /// Gets or sets the total net PNL.
        /// </summary>
        /// <value>
        /// The total net PNL.
        /// </value>
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
        /// <summary>
        /// The m total fee
        /// </summary>
        private double? mTotalFee;
        /// <summary>
        /// Gets or sets the total fee.
        /// </summary>
        /// <value>
        /// The total fee.
        /// </value>
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
        /// <summary>
        /// The m incomplte HRS checked
        /// </summary>
        private bool mIncomplteHrsChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [incomplte HRS checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [incomplte HRS checked]; otherwise, <c>false</c>.
        /// </value>
        public bool IncomplteHrsChecked
        {
            get
            {
                return mIncomplteHrsChecked;
            }
            set
            {
                mIncomplteHrsChecked = value;
                CalculateCommand();
                RaisePropertyChanged("IncomplteHrsChecked");
            }
        }
        /// <summary>
        /// The m iscomplte HRS enabled
        /// </summary>
        private bool mIscomplteHrsEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether [iscomplte HRS enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [iscomplte HRS enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool IscomplteHrsEnabled
        {
            get
            {
                return mIscomplteHrsEnabled;
            }
            set
            {
                mIscomplteHrsEnabled = value;
                RaisePropertyChanged("IscomplteHrsEnabled");
            }
        }
        /// <summary>
        /// The mplot data first
        /// </summary>
        private PlotModel mplotDataFirst;
        /// <summary>
        /// Gets or sets the plot data first.
        /// </summary>
        /// <value>
        /// The plot data first.
        /// </value>
        public PlotModel PlotDataFirst
        {
            get
            {
                return mplotDataFirst;
            }
            set
            {
                mplotDataFirst = value;
                RaisePropertyChanged("PlotDataFirst");
            }
        }
        /// <summary>
        /// The mplot data second
        /// </summary>
        private PlotModel mplotDataSecond;

        public PlotModel PlotDataSecond
        {
            get
            {
                return mplotDataSecond;
            }
            set
            {
                mplotDataSecond = value;
                RaisePropertyChanged("PlotDataSecond");
            }
        }


        private PlotModel mplotDataThird;

        public PlotModel PlotDataThird
        {
            get
            {
                return mplotDataThird;
            }
            set
            {
                mplotDataThird = value;
                RaisePropertyChanged("PlotDataThird");
            }
        }

        private PlotModel mplotDataFour;
        public PlotModel PlotDataFour
        {
            get
            {
                return mplotDataFour;
            }
            set
            {
                mplotDataFour = value;
                RaisePropertyChanged("PlotDataFour");
            }
        }
        private bool mIsCalculateEnable;
        public bool IsCalculateEnable
        {
            get
            {
                return mIsCalculateEnable;
            }
            set
            {
                mIsCalculateEnable = value;
                RaisePropertyChanged("IsCalculateEnable");
            }
        }
        private bool virtualSummary = false;

        public bool VirtualSummary
        {
            get
            {
                return virtualSummary;
            }
            set
            {
                virtualSummary = value;
                RaisePropertyChanged("VirtualSummary");
                if (value && SelectedProduct == "UPTO")
                {
                    IsIncVisible = false;
                }
                else IsIncVisible = true;
            }
        }

        private bool isIncVisible = true;

        public bool IsIncVisible
        {
            get
            {
                return isIncVisible;
            }
            set
            {
                isIncVisible = value;
                RaisePropertyChanged("IsIncVisible");
            }
        }

        private bool mIsSinkVisible;

        public bool IsSinkVisible
        {
            get
            {
                return mIsSinkVisible;
            }
            set
            {
                mIsSinkVisible = value;
                RaisePropertyChanged("IsSinkVisible");
            }
        }


        private string mPortfolioVisible;

        public string PortfolioVisible
        {
            get
            {
                return mPortfolioVisible;
            }
            set
            {
                mPortfolioVisible = value;
                RaisePropertyChanged("PortfolioVisible");
            }
        }



        private List<PNLConstraints> mConstraintsList;


        public List<PNLConstraints> ConstraintsList
        {
            get
            {
                return mConstraintsList;
            }
            set
            {
                mConstraintsList = value;
                RaisePropertyChanged("ConstraintsList");
            }
        }

        private bool pnlChecked;
        public bool PNLChecked
        {
            get { return pnlChecked; }
            set
            {
                pnlChecked = value;
                RaisePropertyChanged("PNLChecked");
                if (pnlChecked)
                {
                    DAChecked = false;
                    RTChecked = false;
                    DrawFirstGraph("PNL");
                    DrawSecondGraph("PNL");
                }
            }
        }

        private bool rtChecked;
        public bool RTChecked
        {
            get { return rtChecked; }
            set
            {
                rtChecked = value;
                RaisePropertyChanged("RTChecked");
                if (rtChecked)
                {
                    DAChecked = false;
                    PNLChecked = false;
                    // DrawFirstGraph("RT");
                    //  DrawSecondGraph("RT");
                }
            }
        }

        private bool daChecked;
        public bool DAChecked
        {
            get { return daChecked; }
            set
            {
                daChecked = value;
                RaisePropertyChanged("DAChecked");
                if (DAChecked)
                {
                    RTChecked = false;
                    PNLChecked = false;
                }
            }
        }
        #endregion
        public MainWindowViewModel(IDataService dataService)
        {
            try
            {
                _dataService = dataService;
                _dataService.loadDBCommands();
                UserList = DBAccess.GetUserList();
                ERCOTUserList = DBAccess.GetERCOTUserList();
                SetMarket();
                SetDesign();
                StartDate = DateTime.Now.Date;
                EndDate = DateTime.Now.Date;
                RunCalcCommand = new DelegateCommand(() => CalculateCommand());
                ResetCommand = new DelegateCommand(() => Reset());
                TodaysDateCommand = new DelegateCommand(() => TodaySDateClick());
                RunExportCSVCommand = new DelegateCommand(() => ExportToCSVCommand());
                CalculateExposureCommand = new DelegateCommand(() => CalculateExposure());
                ProductList = new List<string> { "UPTO" };
                SelectedProduct = "UPTO";
                IsCalculateEnable = true;
                isRTChecked = true;
                isDAChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Private Methods

        private void SetDesign()
        {
            List<string> marketList = new List<string>();
            if (UserList.Contains(mUser))
            {

                PortfolioVisible = "Visible";

            }
            else if (ERCOTUserList.Contains(mUser))
            {

                PortfolioVisible = "Collapsed";

            }
        }


        private void CalculateExposure() // Calculates exposure of portfolio and shown in exposure tab
        {
            try
            {
                ConstraintsList = new List<PNLConstraints>();
                if (SelectedMarket.ToUpper() == "ERCOT")
                    ConstraintsList = _dataService.GetPNLConstraints(PNLList, SortDollarChecked, StartDate, EndDate.AddDays(1), 9);

                List<PNLConstraints> ConstraintsListfinal = new List<PNLConstraints>();
                foreach (PNLConstraints itemPNLConstraints in ConstraintsList)
                {
                    PNLConstraints mPNLConstraints = new PNLConstraints();
                    mPNLConstraints.constraintNum = itemPNLConstraints.constraintNum;
                    mPNLConstraints.monitoredName = itemPNLConstraints.monitoredName;
                    mPNLConstraints.contingName = itemPNLConstraints.contingName;
                    if (SortDollarChecked)
                    {
                        mPNLConstraints.he1Value = Math.Round((!itemPNLConstraints.he1Value.HasValue ? 0 : itemPNLConstraints.he1Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he2Value = Math.Round((!itemPNLConstraints.he2Value.HasValue ? 0 : itemPNLConstraints.he2Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he3Value = Math.Round((!itemPNLConstraints.he3Value.HasValue ? 0 : itemPNLConstraints.he3Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he4Value = Math.Round((!itemPNLConstraints.he4Value.HasValue ? 0 : itemPNLConstraints.he4Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he5Value = Math.Round((!itemPNLConstraints.he5Value.HasValue ? 0 : itemPNLConstraints.he5Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he6Value = Math.Round((!itemPNLConstraints.he6Value.HasValue ? 0 : itemPNLConstraints.he6Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he7Value = Math.Round((!itemPNLConstraints.he7Value.HasValue ? 0 : itemPNLConstraints.he7Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he8Value = Math.Round((!itemPNLConstraints.he8Value.HasValue ? 0 : itemPNLConstraints.he8Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he9Value = Math.Round((!itemPNLConstraints.he9Value.HasValue ? 0 : itemPNLConstraints.he9Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he10Value = Math.Round((!itemPNLConstraints.he10Value.HasValue ? 0 : itemPNLConstraints.he10Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he11Value = Math.Round((!itemPNLConstraints.he11Value.HasValue ? 0 : itemPNLConstraints.he11Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he12Value = Math.Round((!itemPNLConstraints.he12Value.HasValue ? 0 : itemPNLConstraints.he12Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he13Value = Math.Round((!itemPNLConstraints.he13Value.HasValue ? 0 : itemPNLConstraints.he13Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he14Value = Math.Round((!itemPNLConstraints.he14Value.HasValue ? 0 : itemPNLConstraints.he14Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he15Value = Math.Round((!itemPNLConstraints.he15Value.HasValue ? 0 : itemPNLConstraints.he15Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he16Value = Math.Round((!itemPNLConstraints.he16Value.HasValue ? 0 : itemPNLConstraints.he16Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he17Value = Math.Round((!itemPNLConstraints.he17Value.HasValue ? 0 : itemPNLConstraints.he17Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he18Value = Math.Round((!itemPNLConstraints.he18Value.HasValue ? 0 : itemPNLConstraints.he18Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he19Value = Math.Round((!itemPNLConstraints.he19Value.HasValue ? 0 : itemPNLConstraints.he19Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he20Value = Math.Round((!itemPNLConstraints.he20Value.HasValue ? 0 : itemPNLConstraints.he20Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he21Value = Math.Round((!itemPNLConstraints.he21Value.HasValue ? 0 : itemPNLConstraints.he21Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he22Value = Math.Round((!itemPNLConstraints.he22Value.HasValue ? 0 : itemPNLConstraints.he22Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he23Value = Math.Round((!itemPNLConstraints.he23Value.HasValue ? 0 : itemPNLConstraints.he23Value.Value), 0, MidpointRounding.AwayFromZero);
                        mPNLConstraints.he24Value = Math.Round((!itemPNLConstraints.he24Value.HasValue ? 0 : itemPNLConstraints.he24Value.Value), 0, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        mPNLConstraints.he1Value = Math.Round((!itemPNLConstraints.he1Value.HasValue ? 0 : itemPNLConstraints.he1Value.Value), 1);
                        mPNLConstraints.he2Value = Math.Round((!itemPNLConstraints.he2Value.HasValue ? 0 : itemPNLConstraints.he2Value.Value), 1);
                        mPNLConstraints.he3Value = Math.Round((!itemPNLConstraints.he3Value.HasValue ? 0 : itemPNLConstraints.he3Value.Value), 1);
                        mPNLConstraints.he4Value = Math.Round((!itemPNLConstraints.he4Value.HasValue ? 0 : itemPNLConstraints.he4Value.Value), 1);
                        mPNLConstraints.he5Value = Math.Round((!itemPNLConstraints.he5Value.HasValue ? 0 : itemPNLConstraints.he5Value.Value), 1);
                        mPNLConstraints.he6Value = Math.Round((!itemPNLConstraints.he6Value.HasValue ? 0 : itemPNLConstraints.he6Value.Value), 1);
                        mPNLConstraints.he7Value = Math.Round((!itemPNLConstraints.he7Value.HasValue ? 0 : itemPNLConstraints.he7Value.Value), 1);
                        mPNLConstraints.he8Value = Math.Round((!itemPNLConstraints.he8Value.HasValue ? 0 : itemPNLConstraints.he8Value.Value), 1);
                        mPNLConstraints.he9Value = Math.Round((!itemPNLConstraints.he9Value.HasValue ? 0 : itemPNLConstraints.he9Value.Value), 1);
                        mPNLConstraints.he10Value = Math.Round((!itemPNLConstraints.he10Value.HasValue ? 0 : itemPNLConstraints.he10Value.Value), 1);
                        mPNLConstraints.he11Value = Math.Round((!itemPNLConstraints.he11Value.HasValue ? 0 : itemPNLConstraints.he11Value.Value), 1);
                        mPNLConstraints.he12Value = Math.Round((!itemPNLConstraints.he12Value.HasValue ? 0 : itemPNLConstraints.he12Value.Value), 1);
                        mPNLConstraints.he13Value = Math.Round((!itemPNLConstraints.he13Value.HasValue ? 0 : itemPNLConstraints.he13Value.Value), 1);
                        mPNLConstraints.he14Value = Math.Round((!itemPNLConstraints.he14Value.HasValue ? 0 : itemPNLConstraints.he14Value.Value), 1);
                        mPNLConstraints.he15Value = Math.Round((!itemPNLConstraints.he15Value.HasValue ? 0 : itemPNLConstraints.he15Value.Value), 1);
                        mPNLConstraints.he16Value = Math.Round((!itemPNLConstraints.he16Value.HasValue ? 0 : itemPNLConstraints.he16Value.Value), 1);
                        mPNLConstraints.he17Value = Math.Round((!itemPNLConstraints.he17Value.HasValue ? 0 : itemPNLConstraints.he17Value.Value), 1);
                        mPNLConstraints.he18Value = Math.Round((!itemPNLConstraints.he18Value.HasValue ? 0 : itemPNLConstraints.he18Value.Value), 1);
                        mPNLConstraints.he19Value = Math.Round((!itemPNLConstraints.he19Value.HasValue ? 0 : itemPNLConstraints.he19Value.Value), 1);
                        mPNLConstraints.he20Value = Math.Round((!itemPNLConstraints.he20Value.HasValue ? 0 : itemPNLConstraints.he20Value.Value), 1);
                        mPNLConstraints.he21Value = Math.Round((!itemPNLConstraints.he21Value.HasValue ? 0 : itemPNLConstraints.he21Value.Value), 1);
                        mPNLConstraints.he22Value = Math.Round((!itemPNLConstraints.he22Value.HasValue ? 0 : itemPNLConstraints.he22Value.Value), 1);
                        mPNLConstraints.he23Value = Math.Round((!itemPNLConstraints.he23Value.HasValue ? 0 : itemPNLConstraints.he23Value.Value), 1);
                        mPNLConstraints.he24Value = Math.Round((!itemPNLConstraints.he24Value.HasValue ? 0 : itemPNLConstraints.he24Value.Value), 1);
                    }
                    ConstraintsListfinal.Add(mPNLConstraints);
                }
                List<PNLConstraints> ResultList = new List<PNLConstraints>();

                foreach (PNLConstraints constraints in ConstraintsListfinal)
                {

                    constraints.Total = (constraints.he1Value == null ? 0 : constraints.he1Value) + (constraints.he2Value == null ? 0 : constraints.he2Value) + (constraints.he3Value == null ? 0 : constraints.he3Value) + (constraints.he4Value == null ? 0 : constraints.he4Value) +
                                          (constraints.he5Value == null ? 0 : constraints.he5Value) + (constraints.he6Value == null ? 0 : constraints.he6Value) + (constraints.he7Value == null ? 0 : constraints.he7Value) +
                                         (constraints.he8Value == null ? 0 : constraints.he8Value) + (constraints.he9Value == null ? 0 : constraints.he9Value) + (constraints.he10Value == null ? 0 : constraints.he10Value) +
                                         (constraints.he11Value == null ? 0 : constraints.he11Value) + (constraints.he12Value == null ? 0 : constraints.he12Value) + (constraints.he13Value == null ? 0 : constraints.he13Value) +
                                        (constraints.he14Value == null ? 0 : constraints.he14Value) + (constraints.he15Value == null ? 0 : constraints.he15Value) + (constraints.he16Value == null ? 0 : constraints.he16Value) + (constraints.he17Value == null ? 0 : constraints.he17Value) +
                                        (constraints.he18Value == null ? 0 : constraints.he18Value) + (constraints.he19Value == null ? 0 : constraints.he19Value) +
                                        (constraints.he20Value == null ? 0 : constraints.he20Value) + (constraints.he21Value == null ? 0 : constraints.he21Value) + (constraints.he22Value == null ? 0 : constraints.he22Value) +
                                        (constraints.he23Value == null ? 0 : constraints.he23Value) + (constraints.he24Value == null ? 0 : constraints.he24Value);
                    ResultList.Add(constraints);
                }
                ConstraintsList = null;
                ConstraintsList = ResultList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TodaySDateClick()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
        }

        private void ResetValues(bool isDateChange)
        {
            if (!isDateChange)
            {
                MarketList = null;
            }
            PNLList = null;
            TotalPNL = null;
            TotalMW = null;
            PercentMW = null;
            WinText = null;
            TotalCollect = null;
            TotalFee = null;
            TotalNetPNL = null;
            SelectedMarket = null;
            IsCalculateEnable = true;
        }


        private int GetMarketKey(string market)
        {
            int key = 9;

            if (market == "ERCOT")
            {
                key = 9;
                return key;
            }
            if (market == "ERCOT External")
            {
                key = 10;
                return key;
            }
            return key;
        }


        private void SetAccountList()
        {
            AccountList = null;
            if (SelectedProduct == null)
            {
                return;
            }
            if (SelectedMarket == null)
            {
                return;
            }
            string product = SelectedProduct == "Virtual" ? "Virtual" : "EES/PTP";
            List<Account> accountList = DBAccess.GetAccount(mUser, product, SelectedMarket);
            AccountList = accountList;
        }

        private void CalculateThreaded() // calculates daily pnl and fees for both summery and details
        {
            try
            {
                double count = 0.0;
                TotalPNL = null;
                TotalMW = null;
                PercentMW = null;
                WinText = null;
                TotalCollect = null;
                TotalNetPNL = null;
                TotalFee = null; TotalDACong = null; TotalRTCong = null; TotalDALoss = null; TotalRTLoss = null;
                double winPercentage = 0.0;
                if (UserList.Contains(mUser))
                {
                    if (SelectedPortfolioList == null)
                    {
                        Mouse.OverrideCursor = null;
                        return;
                    }
                }

                List<int> portfolioKeyList = new List<int>();
                List<Pnl> pnlList = new List<Pnl>();
                List<Bid> bidList = new List<Bid>();
                foreach (Portfolio portfolio in SelectedPortfolioList)
                {
                    portfolioKeyList.Add(portfolio.ID);
                    List<Bid> tempList = DBAccess.GetCleareds(portfolio.IsUptos, GetMarketKey(portfolio.Market), portfolio.ID, StartDate, EndDate);
                    bidList.AddRange(tempList);
                }
                //  double feePerMW = DBAccess.GetFeePerMW(StartDate, EndDate, GetMarketKey(SelectedMarket), SelectedProduct, portfolioKeyList);
                double feePerMW = 0.03;
                bool External = false;
                if (SelectedMarket == "ERCOT External")
                {
                    External = true;
                }
                Tuple<List<Node>, List<Node>> nodeTuple = GetNodeLists(bidList, SelectedProduct == "UPTO", External);
                List<Vayu.NodePriceLibrary.Node> rtList = nodeTuple.Item2;
                List<Vayu.NodePriceLibrary.Node> daList = nodeTuple.Item1;
                if (bidList.Count == 0)
                {
                    return;
                }
                double total = 0;
                double mwsummary = 0;
                List<Pnl> tempList1 = new List<Pnl>();
                List<Pnl> finalPnlList = new List<Pnl>();
                #region Summery
                List<Pnl> IncompleteHrsPnlList = new List<Pnl>();
                if (SummaryChecked)
                {
                    Dictionary<DateTime, Dictionary<int, PnlFee>> date1Hash = _dataService.GetPnlFee(StartDate, EndDate, SelectedMarket, portfolioKeyList);
                    if (date1Hash.Count == 0)
                    {
                        MessageBox.Show("Pnl Not Yet loaded for Summery Please check after some time");
                        return;
                    }
                    List<DateTime> dateList = date1Hash.Keys.ToList<DateTime>();
                    dateList.Sort();
                    foreach (DateTime date in dateList)
                    {
                        if (date != DateTime.Today)
                        {
                            List<int> portfolioList = date1Hash[date].Keys.ToList<int>();
                            foreach (int portfolioKey in portfolioList)
                            {
                                Pnl pnl = new Pnl();
                                pnl.MarketDate = date;

                                if (SelectedMarket == "ERCOT External")
                                {
                                    pnl.PortfolioName = _dataService.GetExternalPortfolioName(portfolioKey);
                                }
                                else
                                {
                                    pnl.PortfolioName = DBAccess.GetPortfolioName(portfolioKey);
                                }
                                //pnl.PortfolioName = DBAccess.GetPortfolioName(portfolioKey);
                                pnl.PnlValue = date1Hash[date][portfolioKey].Pnl;
                                pnl.PayCollect = date1Hash[date][portfolioKey].DollarsCleared;
                                pnl.Fee = date1Hash[date][portfolioKey].Fee;
                                pnl.NetPnl = (double)pnl.PnlValue + (double)pnl.Fee;
                                pnl.MW = date1Hash[date][portfolioKey].MW;
                                total += ((double)pnl.PnlValue + (double)pnl.Fee);
                                pnl.CummPnl = total;
                                tempList1.Add(pnl);
                                if (total > 0)
                                {
                                    winPercentage++;
                                }
                                count++;
                            }
                        }
                    }
                    if (EndDate == DateTime.Today)
                    {
                        Dictionary<int, Pnl> pnlHash = new Dictionary<int, Pnl>();
                        Dictionary<int, PnlFee> TodayFeeHash = null;
                        if (date1Hash != null && date1Hash.Count > 0)
                        {
                            TodayFeeHash = date1Hash[EndDate];
                        }
                        if (SelectedMarket == "ERCOT External")
                        {
                            DARTNode.GetDART(rtList, daList, EndDate.AddMonths(-2), 1, true, false);

                        }
                        else
                        {
                            DARTNode.GetDART(rtList, daList, EndDate, 1, true, false);

                        }

                        double summw = 0;
                        foreach (Bid bid in bidList)
                        {

                            DateTime date = bid.MarketDateTime.Hour == 0 ? bid.MarketDateTime.AddDays(-1).Date : bid.MarketDateTime.Date;
                            if (date != DateTime.Today)
                            {
                                continue;
                            }


                            string sourceNodeKey = bid.MarketDateTime.ToString() + bid.Source.ToString();
                            string sinkNodeKey = bid.MarketDateTime.ToString() + bid.Sink.ToString();
                            if (DARTNode.sDAHash.ContainsKey(sourceNodeKey) && DARTNode.sRTHash.ContainsKey(sourceNodeKey) &&
                                DARTNode.sDAHash.ContainsKey(sinkNodeKey) && DARTNode.sRTHash.ContainsKey(sinkNodeKey) &&
                                !double.IsNaN(DARTNode.sDAHash[sinkNodeKey]) && !double.IsNaN(DARTNode.sDAHash[sourceNodeKey]) &&
                                !double.IsNaN(DARTNode.sRTHash[sinkNodeKey]) && !double.IsNaN(DARTNode.sRTHash[sourceNodeKey]))
                            {
                                Pnl pnl = new Pnl();

                                pnl.PnlValue = ((DARTNode.sRTHash[sinkNodeKey] - DARTNode.sRTHash[sourceNodeKey]) - (DARTNode.sDAHash[sinkNodeKey] - DARTNode.sDAHash[sourceNodeKey])) * bid.MW;

                                if (pnlHash.ContainsKey(bid.PortfolioKey))
                                {
                                    pnl.PnlValue += pnlHash[bid.PortfolioKey].PnlValue;
                                    pnlHash.Remove(bid.PortfolioKey);
                                }
                                pnlHash.Add(bid.PortfolioKey, pnl);
                            }

                        }
                        foreach (var item in pnlHash)
                        {
                            int portKey = item.Key;
                            Pnl todayPnl = pnlHash[portKey];
                            PnlFee todayFee = TodayFeeHash[portKey];
                            todayPnl.MW = todayFee.MW;
                            todayPnl.PayCollect = todayFee.DollarsCleared;
                            double gross = (double)todayPnl.PnlValue;
                            todayPnl.NetPnl = gross + todayFee.Fee;
                            if (todayPnl.NetPnl > 0)
                            {
                                winPercentage++;
                            }
                            count++;
                        }

                        List<int> portfolioList = pnlHash.Keys.ToList<int>();
                        foreach (int portfolioKey in portfolioList)
                        {
                            Pnl pnl = new Pnl();
                            pnl.MarketDate = DateTime.Today;
                            if (SelectedMarket == "ERCOT External")
                            {
                                pnl.PortfolioName = _dataService.GetExternalPortfolioName(portfolioKey);
                            }
                            else
                            {
                                pnl.PortfolioName = DBAccess.GetPortfolioName(portfolioKey);
                            }

                            pnl.PnlValue = pnlHash[portfolioKey].PnlValue;
                            pnl.PayCollect = pnlHash[portfolioKey].PayCollect;
                            pnl.Fee = pnlHash[portfolioKey].Fee;
                            pnl.MW = pnlHash[portfolioKey].MW;
                            pnl.NetPnl = (double)pnl.PnlValue + (double)pnl.Fee;
                            total += ((double)pnl.PnlValue + (double)pnl.Fee);
                            pnl.CummPnl = total;
                            tempList1.Add(pnl);

                        }
                    }
                }
                #endregion

                else
                {
                    if (SelectedProduct == "UPTO")
                    {
                        if (SelectedMarket == "ERCOT External")
                        {
                            //DARTNode.GetDART(rtList, daList, StartDate.AddMonths(-2), (EndDate - StartDate).Days + 1, false, true);
                            DARTNode.GetDART(rtList, daList, StartDate, (EndDate - StartDate).Days + 1, false, true);

                        }
                        else
                        {
                            DARTNode.GetDART(rtList, daList, StartDate, (EndDate - StartDate).Days + 1, false, true);

                        }

                    }
                    else
                    {
                        DARTNode.GetDART(rtList, daList, StartDate, (EndDate - StartDate).Days + 1, false, true);
                    }
                    DateTime tempdt = DateTime.Now;
                    if (tempdt.Minute >= 3)
                    {
                        tempdt = DateTime.Now.AddHours(2);
                    }
                    else
                    {
                        tempdt = DateTime.Now.AddHours(1);
                    }
                    TotalDACong = 0; TotalRTCong = 0; TotalRTLoss = 0; TotalDALoss = 0;
                    foreach (Bid bid in bidList)
                    {
                        Pnl pnl = new Pnl();
                        if (SelectedMarket == "ERCOT External")
                        {
                            pnl.PortfolioName = _dataService.GetExternalPortfolioName(bid.PortfolioKey);
                        }
                        else
                        {
                            pnl.PortfolioName = DBAccess.GetPortfolioName(bid.PortfolioKey);
                        }
                        pnl.MarketDate = bid.MarketDateTime.Hour == 0 ? bid.MarketDateTime.AddDays(-1).Date : bid.MarketDateTime.Date;
                        pnl.HE = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                        PricingNode tempSourceNode = DBAccess.GetNode(bid.Source, bid.Market);

                        pnl.Source = tempSourceNode.NodeName;
                        pnl.SourceZone = tempSourceNode.Zone;
                        string sourceKey = bid.MarketDateTime.ToString() + bid.Source.ToString();
                        double da = double.NaN;
                        double rt = double.NaN;
                        double daCong = double.NaN;
                        double daLoss = double.NaN;
                        double rtCong = double.NaN;
                        double rtLoss = double.NaN;
                        #region Uptos
                        if (SelectedProduct == "UPTO")
                        {
                            string sinkKey = bid.MarketDateTime.ToString() + bid.Sink.ToString();
                            PricingNode tempSinkNode = DBAccess.GetNode(bid.Sink, bid.Market);
                            pnl.Sink = tempSinkNode.NodeName;
                            pnl.SinkZone = tempSinkNode.Zone;
                            pnl.MW = bid.MW;
                            if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                            {
                                LMP sourceLmp = DARTNode.sDALmpHash[sourceKey];
                                LMP sinkLmp = DARTNode.sDALmpHash[sinkKey];
                                if (sourceLmp.Price != double.NaN && sinkLmp.Price != double.NaN)
                                {
                                    da = sinkLmp.Price - sourceLmp.Price;
                                    daCong = sinkLmp.Congestion - sourceLmp.Congestion;
                                    daLoss = sinkLmp.Loss - sourceLmp.Loss;
                                }

                            }
                            if (LatestHourlyChecked)
                            {
                                string sourceKey1 = tempdt.ToString("M/d/yyyy h:00:00 tt") + bid.Source.ToString();
                                string sinkKey1 = tempdt.ToString("M/d/yyyy h:00:00 tt") + bid.Sink.ToString();
                                if (sourceKey1 == sourceKey && sinkKey1 == sinkKey)
                                {
                                    if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                                    {
                                        LMP sourceLmp = DARTNode.sRTLmpHash[sourceKey];
                                        LMP sinkLmp = DARTNode.sRTLmpHash[sinkKey];
                                        if (sourceLmp.Price != double.NaN && sinkLmp.Price != double.NaN)
                                        {
                                            rt = sinkLmp.Price - sourceLmp.Price;
                                            rtCong = sinkLmp.Congestion - sourceLmp.Congestion;
                                            rtLoss = sinkLmp.Loss - sourceLmp.Loss;
                                        }
                                    }

                                }
                                else
                                {
                                    rt = double.NaN;
                                }


                            }
                            else
                            {
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                                {
                                    LMP sourceLmp = DARTNode.sRTLmpHash[sourceKey];
                                    LMP sinkLmp = DARTNode.sRTLmpHash[sinkKey];
                                    if (sourceLmp.Price != double.NaN && sinkLmp.Price != double.NaN)
                                    {
                                        rt = sinkLmp.Price - sourceLmp.Price;
                                        rtCong = sinkLmp.Congestion - sourceLmp.Congestion;
                                        rtLoss = sinkLmp.Loss - sourceLmp.Loss;
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Virtuals
                        else
                        {
                            pnl.IncDEC = bid.MW < 0 ? "INC" : "DEC";
                            pnl.MW = Math.Abs(bid.MW);
                            if (DARTNode.sDALmpHash.ContainsKey(sourceKey))
                            {
                                LMP dalmp = DARTNode.sDALmpHash[sourceKey];
                                if (dalmp.Price != double.NaN)
                                {
                                    da = dalmp.Price;
                                    daCong = dalmp.Congestion;
                                    daLoss = dalmp.Loss;
                                }
                            }
                            if (DARTNode.sRTLmpHash.ContainsKey(sourceKey))
                            {
                                LMP rtlmp = DARTNode.sRTLmpHash[sourceKey];
                                if (rtlmp.Price != double.NaN)
                                {
                                    rt = rtlmp.Price;
                                    rtCong = rtlmp.Congestion;
                                    rtLoss = rtlmp.Loss;
                                }
                            }
                        }
                        #endregion
                        pnl.Fee = -Math.Abs(bid.MW) * 0.03;
                        //if (SelectedProduct == "UPTO" && !SummaryChecked)
                        //{
                        //    pnl.Fee = pnl.Fee * 0.08;  // fees rate is 8 cents per MW only for uptos
                        //}
                        if (!double.IsNaN(da))
                        {
                            pnl.DA = da;
                            pnl.DACong = daCong;
                            pnl.DALoss = daLoss;
                            pnl.PayCollect = (da * bid.MW);
                            pnl.DACongTot = daCong * bid.MW;
                            pnl.DALossTot = daLoss * bid.MW;
                        }
                        if (!double.IsNaN(rt))
                        {
                            pnl.RT = rt;
                            pnl.RTCong = rtCong;
                            pnl.RTLoss = rtLoss;
                            pnl.RTCongTot = rtCong * bid.MW;
                            pnl.RTLossTot = rtLoss * bid.MW;
                        }
                        if (!double.IsNaN(da) && !double.IsNaN(rt))
                        {
                            pnl.DART = rt - da;
                            pnl.PnlValue = pnl.DART * bid.MW;
                            if (pnl.PnlValue != null && pnl.Fee != null)
                            {
                                pnl.NetPnl = (double)pnl.PnlValue + (double)pnl.Fee;
                            }
                            if (pnl.DART * bid.MW > 0)
                            {
                                ++winPercentage;
                            }
                        }
                        pnlList.Add(pnl);


                    }
                    if (IncomplteHrsChecked)
                    {
                        finalPnlList = pnlList.ToList();
                        tempList1 = pnlList.Where(a => a.RT == null).ToList();
                        IncompleteHrsPnlList = pnlList.Where(a => a.RT != null).ToList();
                    }

                    else
                    {
                        finalPnlList = pnlList.ToList();
                        tempList1 = pnlList.Where(a => a.RT != null).ToList();
                        //  pnlList = tempList1.ToList();
                    }
                }

                TotalMW = tempList1.Sum(a => a.MW);


                if (IncomplteHrsChecked)
                {
                    TotalPNL = IncompleteHrsPnlList.Sum(a => a.PnlValue);
                    TotalNetPNL = IncompleteHrsPnlList.Sum(a => a.PnlValue) + pnlList.Sum(a => a.Fee);
                    TotalMW = pnlList.Sum(a => a.MW);
                    TotalCollect = pnlList.Sum(a => a.PayCollect);
                    TotalFee = pnlList.Sum(a => a.Fee);
                    TotalRTCong = pnlList.Sum(a => a.RTCongTot);
                    TotalRTLoss = pnlList.Sum(a => a.RTLossTot);
                    TotalDACong = pnlList.Sum(a => a.DACongTot);
                    TotalDALoss = pnlList.Sum(a => a.DALossTot);

                }
                else
                {
                    TotalPNL = tempList1.Sum(a => a.PnlValue);
                    TotalNetPNL = tempList1.Sum(a => a.NetPnl);
                    TotalCollect = tempList1.Sum(a => a.PayCollect);
                    TotalFee = tempList1.Sum(a => a.Fee);
                    TotalDACong = tempList1.Sum(a => a.DACongTot);
                    TotalDALoss = tempList1.Sum(a => a.DALossTot);

                    TotalRTCong = tempList1.Sum(a => a.RTCongTot);
                    TotalRTLoss = tempList1.Sum(a => a.RTLossTot);
                }

                if (SummaryChecked)
                {
                    pnlList = tempList1.ToList();
                    TotalCollect = pnlList.Sum(a => a.PayCollect);
                    TotalDACong = pnlList.Sum(a => a.DACongTot);
                    TotalDALoss = pnlList.Sum(a => a.DALossTot);

                    TotalRTCong = pnlList.Sum(a => a.RTCongTot);
                    TotalRTLoss = pnlList.Sum(a => a.RTLossTot);

                }

                count = tempList1.Count();
                PercentMW = TotalPNL / TotalMW;
                WinText = winPercentage / count;
                PNLList = pnlList.ToList();
                if (LatestHourlyChecked)
                {
                    PNLList = tempList1.ToList();
                }
                var sortedList = PNLList.OrderBy(a => a.HE).ThenBy(b => b.Source).ThenBy(c => c.Sink);
                PNLList = null;
                PNLList = sortedList.ToList<Pnl>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsCalculateEnable = true;
            }
            IsCalculateEnable = true;
        }

        private Tuple<List<Node>, List<Node>> GetNodeLists(List<Bid> bidList, bool isUpTo, bool isExternal)
        {
            Hashtable daList = new Hashtable();
            Hashtable rtList = new Hashtable();

            Action<Bid, int> buildList = (bid, val) =>
            {

                Node pricingNode = new Node();
                pricingNode.Market = bid.Market;
                pricingNode.NodeId = val;

                PricingNode priceNode = DBAccess.GetNode(val, bid.Market);
                pricingNode.NodeName = priceNode.NodeName;
                pricingNode.PNodeId = priceNode.ExternalNodeId;
                rtList[val] = pricingNode;

                Node node1 = new Node();

                node1.Market = bid.Market;
                node1.NodeId = val;
                node1.NodeName = priceNode.NodeName;
                node1.PNodeId = priceNode.ExternalNodeId;
                daList[val] = node1;
            };
            foreach (Bid bid in bidList)
            {
                buildList(bid, bid.Source);
                if (isUpTo)
                {
                    buildList(bid, bid.Sink);
                }
            }
            return new Tuple<List<Node>, List<Node>>(daList.Values.Cast<Node>().ToList(), rtList.Values.Cast<Node>().ToList());
        }

        private void ExportToCSVThreaded()
        {
            try
            {
                if (SelectedPortfolioList == null)
                {
                    Mouse.OverrideCursor = null;
                    return;
                }
                if (PNLList == null || PNLList.Count == 0)
                {
                    System.Windows.MessageBox.Show("No data to export to");
                    return;
                }
                SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
                dialog.FileName = "Portfolio_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if ((bool)dialog.ShowDialog())
                {
                    if (dialog.FileName != "")
                    {
                        if (PNLList != null && PNLList.Count > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (PropertyInfo item in PNLList[0].GetType().GetProperties())
                            {
                                if (item.Name.ToUpper() == "PORTFOLIONAME" || item.Name.ToUpper() == "MARKETDATE" || item.Name.ToUpper() == "HE" || item.Name.ToUpper() == "MW" || item.Name.ToUpper() == "PNLVALUE" || item.Name.ToUpper() == "SOURCE" || item.Name.ToUpper() == "SINK" ||
                                item.Name.ToUpper() == "SOURCEZONE" || item.Name.ToUpper() == "SINKZONE" || item.Name.ToUpper() == "DA" || item.Name.ToUpper() == "RT" || item.Name.ToUpper() == "DART" || item.Name.ToUpper() == "FEE" ||
                                item.Name.ToUpper() == "DACONG" || item.Name.ToUpper() == "RTCONG" || item.Name.ToUpper() == "DALOSS" || item.Name.ToUpper() == "RTLOSS" || item.Name.ToUpper() == "NETPNL" || item.Name.ToUpper() == "PAYCOLLECT")
                                {
                                    builder.Append(item.Name.ToUpper() + ",");
                                }

                            }
                            builder.AppendLine();
                            foreach (Pnl item in PNLList)
                            {
                                foreach (PropertyInfo propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name.ToUpper() == "PORTFOLIONAME" || propItem.Name.ToUpper() == "MARKETDATE" || propItem.Name.ToUpper() == "HE" || propItem.Name.ToUpper() == "MW" || propItem.Name.ToUpper() == "PNLVALUE" || propItem.Name.ToUpper() == "SOURCE" || propItem.Name.ToUpper() == "SINK" ||
                                         propItem.Name.ToUpper() == "SOURCEZONE" || propItem.Name.ToUpper() == "SINKZONE" || propItem.Name.ToUpper() == "DA" || propItem.Name.ToUpper() == "RT" || propItem.Name.ToUpper() == "DART" || propItem.Name.ToUpper() == "FEE" ||
                                propItem.Name.ToUpper() == "DACONG" || propItem.Name.ToUpper() == "RTCONG" || propItem.Name.ToUpper() == "DALOSS" || propItem.Name.ToUpper() == "RTLOSS" || propItem.Name.ToUpper() == "NETPNL" || propItem.Name.ToUpper() == "PAYCOLLECT")
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // This methods exports the data inside data grid into csv file

        private bool MajorStepRequired()
        {
            if ((EndDate - StartDate).Days < 120)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private List<Coordinates> GetCoordinates(bool isSecond, string type)
        {
            List<Coordinates> summaryValues = new List<Coordinates>();
            double? pnlValue = 0.0;
            try
            {
                if (!SummaryChecked)
                {
                    if (PNLList != null && PNLList.Count > 0)
                    {
                        Dictionary<int, double?> dateHash = new Dictionary<int, double?>();
                        foreach (Pnl pnl in PNLList)
                        {
                            pnlValue = new double();
                            if (type == "PNL")
                                pnlValue = pnl.NetPnl;
                            if (type == "RT")
                                pnlValue = pnl.RT * pnl.MW;
                            if (type == "DA")
                                pnlValue = pnl.DA * pnl.MW;
                            if (type == "DACong")
                                pnlValue = pnl.DACong * pnl.MW;
                            if (type == "DALoss")
                                pnlValue = pnl.DALoss * pnl.MW;
                            if (type == "RTCong")
                                pnlValue = pnl.RTCong * pnl.MW;
                            if (type == "RTLoss")
                                pnlValue = pnl.RTLoss * pnl.MW;

                            if (dateHash.ContainsKey(pnl.HE))
                            {
                                pnlValue += dateHash[pnl.HE];
                                dateHash.Remove(pnl.HE);
                            }
                            if (pnlValue != null)
                            {
                                dateHash.Add(pnl.HE, pnlValue);
                            }
                        }
                        List<int> dateList = dateHash.Keys.ToList<int>();
                        dateList.Sort();
                        double sumPnl = 0;
                        foreach (int hour in dateList)
                        {
                            Coordinates coordinate = new Coordinates();
                            coordinate.Hour = hour;
                            if (isSecond)
                            {
                                sumPnl += (double)dateHash[hour];
                            }
                            else
                            {
                                sumPnl = (double)dateHash[hour];
                            }
                            coordinate.PNL = sumPnl;
                            summaryValues.Add(coordinate);
                        }
                    }
                }
                else
                {
                    if (PNLList != null && PNLList.Count > 0)
                    {
                        Dictionary<DateTime, double?> dateHash = new Dictionary<DateTime, double?>();
                        foreach (Pnl pnl in PNLList)
                        {
                            pnlValue = new double();
                            pnlValue = pnl.NetPnl;
                            if (dateHash.ContainsKey(pnl.MarketDate))
                            {
                                pnlValue += dateHash[pnl.MarketDate];
                                dateHash.Remove(pnl.MarketDate);
                            }
                            if (pnlValue != null)
                            {
                                dateHash.Add(pnl.MarketDate, pnlValue);
                            }
                        }
                        List<DateTime> dateList = dateHash.Keys.ToList<DateTime>();
                        dateList.Sort();
                        double sumPnl = 0;
                        foreach (DateTime date in dateList)
                        {
                            Coordinates coordinate = new Coordinates();
                            coordinate.Hour = date;
                            if (isSecond)
                            {
                                sumPnl += (double)dateHash[date];
                            }
                            else
                            {
                                sumPnl = (double)dateHash[date];
                            }
                            coordinate.PNL = sumPnl;
                            summaryValues.Add(coordinate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return summaryValues;
        }

        private List<Coordinates> GetGraphCoordinates()
        {
            List<Coordinates> summaryValues = new List<Coordinates>();
            try
            {
                if (!SummaryChecked)
                {
                    if (PNLList != null && PNLList.Count > 0)
                    {
                        Dictionary<string, Dictionary<int, double>> pkvaluesHash = new Dictionary<string, Dictionary<int, double>>();
                        List<string> pkHash = PNLList.Select(x => x.PortfolioName).Distinct().ToList();
                        foreach (string portfolio in pkHash)
                        {
                            Dictionary<int, double> hrvaluesHash = new Dictionary<int, double>();
                            foreach (Pnl pnl in PNLList)
                            {
                                if (portfolio == pnl.PortfolioName)
                                {
                                    double sumPnl = pnl.NetPnl;
                                    if (hrvaluesHash.ContainsKey(pnl.HE))
                                    {
                                        sumPnl += hrvaluesHash[pnl.HE];
                                        hrvaluesHash[pnl.HE] = sumPnl;
                                    }
                                    else
                                    {
                                        hrvaluesHash.Add(pnl.HE, sumPnl);
                                    }
                                }
                            }
                            pkvaluesHash.Add(portfolio, hrvaluesHash);
                        }
                        List<string> pkList = pkvaluesHash.Keys.ToList();
                        foreach (string pk in pkList)
                        {
                            foreach (var item in pkvaluesHash[pk])
                            {
                                Coordinates coordinate = new Coordinates();
                                coordinate.Hour = item.Key;
                                coordinate.PNL = item.Value;
                                coordinate.Portfolio = pk;
                                summaryValues.Add(coordinate);
                            }
                        }
                    }
                }
                else
                {
                    if (PNLList != null && PNLList.Count > 0)
                    {
                        foreach (Pnl pnl in PNLList)
                        {
                            Coordinates coordinate = new Coordinates();
                            coordinate.Hour = pnl.MarketDate;
                            coordinate.PNL = pnl.NetPnl;
                            coordinate.Portfolio = pnl.PortfolioName;
                            summaryValues.Add(coordinate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return summaryValues;
        }

        #endregion

        #region Public Methods

        public void ExportToCSVCommand()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        } // calls exporttoCSV thread for exportind Data Grid Data to .csv file

        public void Reset()
        {
            ResetValues(false);
        }

        public void SetPortfolioList(List<Portfolio> portfolioList)
        {
            PortfolioList = null;
            if (portfolioList.Count > 0)
            {
                PortfolioList = portfolioList;
            }
        }

        public void SetMarket()
        {
            List<string> marketList = new List<string>();
            if (UserList.Contains(mUser))  // used for putting restrictions based on machine username 
            {
                if (!marketList.Contains("ERCOT"))
                    marketList.Add("ERCOT");
                if (!marketList.Contains("ERCOT External"))
                    marketList.Add("ERCOT External");

                MarketList = marketList;
            }
            if (ERCOTUserList.Contains(mUser))  // used for putting restrictions based on machine username 
            {
                if (!marketList.Contains("ERCOT"))
                    marketList.Add("ERCOT");
                if (!marketList.Contains("ERCOT External"))
                    marketList.Add("ERCOT External");
                //selectedMarketValue = "CAISO";
                // marketList.Add("ERCOT");
                MarketList = marketList;
            }
        }

        public void Refresh()
        {
            CalculateCommand();
        }

        public void CalculateCommand()
        {
            IsCalculateEnable = false;
            //CalculateThreaded();
            //Task.Factory.StartNew(() => { CalculateThreaded(); });
            CalculateThreaded();
        }

        public void DrawFirstGraph(string type)
        {
            List<Coordinates> summaryValues = GetCoordinates(false, "PNL");
            List<Coordinates> summaryValuesRT = GetCoordinates(false, "RT");
            List<Coordinates> summaryValuesDA = GetCoordinates(false, "DA");
            List<Coordinates> summaryValuesDACong = GetCoordinates(false, "DACong");
            List<Coordinates> summaryValuesRTCong = GetCoordinates(false, "RTCong");
            List<Coordinates> summaryValuesDALoss = GetCoordinates(false, "DALoss");
            List<Coordinates> summaryValuesRTLoss = GetCoordinates(false, "RTLoss");
            PlotModel plotModel = new PlotModel();

            try
            {
                LinearAxis LinearAxis = new LinearAxis();
                plotModel.Axes.Add(new LinearAxis()
                {
                    Position = AxisPosition.Left,
                    TickStyle = TickStyle.Outside,
                    Key = "PNLYAxis",
                    Title = type,
                    StringFormat = "$0,00",
                    // MinimumPadding = 0.06,
                    //MaximumRange = 0.06,
                    // IsAxisVisible = true
                });

                plotModel.PlotAreaBackground = OxyColors.White;
                plotModel.Background = OxyColors.White;
                if (!SummaryChecked)
                {
                    List<string> values = new List<string>();
                    foreach (Coordinates item in summaryValues)
                    {
                        if (item.Hour.GetType() == typeof(Int32)) values.Add(((Int32)item.Hour).ToString());
                    }

                    CategoryAxis categoryAxis = new CategoryAxis();
                    categoryAxis = new CategoryAxis()
                    {
                        Position = AxisPosition.Bottom,
                        AxislineStyle = LineStyle.Solid,
                        TickStyle = TickStyle.Outside,
                        MajorGridlineStyle = LineStyle.Solid,
                        MajorGridlineColor = OxyColor.FromAColor(20, OxyColors.DarkBlue),
                        LabelField = "HOUR",
                        Title = "HOUR",
                        Key = "HourXAxis",
                        StringFormat = "0",
                        MajorStep = MajorStepRequired() ? 5 : 1,
                        IsPanEnabled = true,
                        IsZoomEnabled = true,
                        FontSize = 10,
                        IsTickCentered = true,
                        GapWidth = 0.05,
                        AxisTitleDistance = 30,
                        Angle = 90
                    };
                    if (summaryValues.Count > 0 && summaryValues.Count < 22)
                    {
                        for (int i = 0; i <= summaryValues.Max(a => Int32.Parse(a.Hour.ToString())); i++)
                        {
                            categoryAxis.Labels.Add(i.ToString("00"));
                        }
                    }
                    else
                    {
                        foreach (Coordinates item in summaryValues)
                        {
                            categoryAxis.Labels.Add(item.Hour.ToString());
                        }
                    }
                    plotModel.Axes.Add(categoryAxis);
                    plotModel.Series.Add(new BarSeries()
                    {
                        FillColor = OxyColors.IndianRed,
                        ItemsSource = summaryValues.ToList(),
                        ValueField = "PNL",
                        XAxisKey = "PNLYAxis",
                        YAxisKey = "HourXAxis",
                        TrackerFormatString = "HE: " + "{Hour:0}," + "PNL: " + "{PNL:0}",
                        NegativeFillColor = OxyColors.MediumBlue

                    });

                    //RT
                    if (isRTChecked)
                    {
                        #region RT
                        var lineSeries1 = new LineSeries();
                        var dataItemValues = new Collection<Axis>(); // use with non DateTime x axis

                        dataItemValues = new Collection<Axis>();
                        for (int i = 0; i < summaryValuesRT.Count; i++)
                        {
                            dataItemValues.Add(new Axis() { X = i, Y = Math.Round(summaryValuesRT[i].PNL, 2), n = i + 1 });
                        }
                        lineSeries1 = new LineSeries()
                        {
                            CanTrackerInterpolatePoints = false,
                            DataFieldX = "X",
                            DataFieldY = "Y",
                            ItemsSource = dataItemValues,
                            TrackerFormatString = "HE: " + "{n:0}, " + "RT: " + "{Y:0}",
                            XAxisKey = "HourXAxis",
                            MarkerType = MarkerType.Diamond,
                            MarkerSize = 2,
                            MarkerStrokeThickness = 0
                        };
                        lineSeries1.Color = OxyColors.DarkGoldenrod;
                        lineSeries1.MarkerFill = OxyColors.Goldenrod;
                        plotModel.Series.Add(lineSeries1);
                        #endregion 
                    }

                    //DA
                    if (isDAChecked)
                    {
                        #region DA
                        var lineSeries2 = new LineSeries();
                        var dataItemValues2 = new Collection<Axis>(); // use with non DateTime x axis

                        dataItemValues2 = new Collection<Axis>();
                        for (int i = 0; i < summaryValuesDA.Count; i++)
                        {
                            dataItemValues2.Add(new Axis() { X = i, Y = Math.Round(summaryValuesDA[i].PNL, 2), n = i + 1 });
                        }
                        lineSeries2 = new LineSeries()
                        {
                            CanTrackerInterpolatePoints = false,
                            DataFieldX = "X",
                            DataFieldY = "Y",
                            ItemsSource = dataItemValues2,
                            TrackerFormatString = "HE: " + "{n:0}, " + "DA: " + "{Y:0}",
                            //TrackerFormatString = "{Hour:0}, {PNL:0.###}",
                            XAxisKey = "HourXAxis",
                            MarkerType = MarkerType.Diamond,
                            MarkerSize = 2,
                            MarkerStrokeThickness = 0
                        };
                        lineSeries2.Color = OxyColors.DarkGreen;
                        lineSeries2.MarkerFill = OxyColors.DarkGreen;
                        plotModel.Series.Add(lineSeries2);
                        #endregion 
                    }

                    //RTCOng
                    if (isRTCongChecked)
                    {
                        #region RTCOng
                        var lineSeries3 = new LineSeries();
                        var dataItemValues3 = new Collection<Axis>(); // use with non DateTime x axis

                        dataItemValues3 = new Collection<Axis>();
                        for (int i = 0; i < summaryValuesRT.Count; i++)
                        {
                            //   summaryValuesRTCong.RemoveAll(a => a.RT == 0 && a.PNL == 0);
                            dataItemValues3.Add(new Axis() { X = i, Y = Math.Round(summaryValuesRTCong[i].PNL, 2), n = i + 1 });
                        }
                        lineSeries3 = new LineSeries()
                        {
                            CanTrackerInterpolatePoints = false,
                            DataFieldX = "X",
                            DataFieldY = "Y",
                            ItemsSource = dataItemValues3,
                            TrackerFormatString = "HE: " + "{n:0}, " + "RTCong: " + "{Y:0}",
                            //TrackerFormatString = "{Hour:0}, {PNL:0.###}",
                            XAxisKey = "HourXAxis",
                            MarkerType = MarkerType.Diamond,
                            MarkerSize = 2,
                            MarkerStrokeThickness = 0
                        };
                        lineSeries3.Color = OxyColors.GreenYellow;
                        lineSeries3.MarkerFill = OxyColors.GreenYellow;
                        plotModel.Series.Add(lineSeries3);
                        #endregion 
                    }

                    //RTLoss
                    if (isRTLossChecked)
                    {
                        #region RTLoss
                        var lineSeries4 = new LineSeries();
                        var dataItemValues4 = new Collection<Axis>(); // use with non DateTime x axis

                        dataItemValues4 = new Collection<Axis>();
                        for (int i = 0; i < summaryValuesRT.Count; i++)
                        {
                            // summaryValuesRTLoss.RemoveAll(a => a.RT == 0 && a.PNL == 0);
                            dataItemValues4.Add(new Axis() { X = i, Y = Math.Round(summaryValuesRTLoss[i].PNL, 2), n = i + 1 });
                        }
                        lineSeries4 = new LineSeries()
                        {
                            CanTrackerInterpolatePoints = false,
                            DataFieldX = "X",
                            DataFieldY = "Y",
                            ItemsSource = dataItemValues4,
                            TrackerFormatString = "HE: " + "{n:0}, " + "RTLoss: " + "{Y:0}",
                            //TrackerFormatString = "{Hour:0}, {PNL:0.###}",
                            XAxisKey = "HourXAxis",
                            MarkerType = MarkerType.Diamond,
                            MarkerSize = 2,
                            MarkerStrokeThickness = 0
                        };
                        lineSeries4.Color = OxyColors.Orange;
                        lineSeries4.MarkerFill = OxyColors.Orange;
                        plotModel.Series.Add(lineSeries4);
                        #endregion 
                    }

                    //DACong
                    if (isDACongChecked)
                    {
                        #region DACong
                        var lineSeries5 = new LineSeries();
                        var dataItemValues5 = new Collection<Axis>(); // use with non DateTime x axis

                        dataItemValues5 = new Collection<Axis>();
                        for (int i = 0; i < summaryValuesDA.Count; i++)
                        {
                            dataItemValues5.Add(new Axis() { X = i, Y = Math.Round(summaryValuesDACong[i].PNL, 2), n = i + 1 });
                        }
                        lineSeries5 = new LineSeries()
                        {
                            CanTrackerInterpolatePoints = false,
                            DataFieldX = "X",
                            DataFieldY = "Y",
                            ItemsSource = dataItemValues5,
                            TrackerFormatString = "HE: " + "{n:0}, " + "DACOng: " + "{Y:0}",
                            //TrackerFormatString = "{Hour:0}, {PNL:0.###}",
                            XAxisKey = "HourXAxis",
                            MarkerType = MarkerType.Diamond,
                            MarkerSize = 2,
                            MarkerStrokeThickness = 0
                        };
                        lineSeries5.Color = OxyColors.Red;
                        lineSeries5.MarkerFill = OxyColors.Red;
                        plotModel.Series.Add(lineSeries5);
                        #endregion 
                    }

                    //DALoss
                    if (isDALossChecked)
                    {
                    }
                }
                else
                {

                    List<string> values = new List<string>();
                    foreach (Coordinates item in summaryValues)
                    {
                        if (item.Hour.GetType() == typeof(DateTime))
                        {
                            values.Add(((DateTime)item.Hour).ToString("M/d/yy"));
                        }
                    }
                    values = values.OrderBy(a => a).ToList();
                    plotModel.Axes.Add(
                           new CategoryAxis()
                           {
                               Position = AxisPosition.Bottom,
                               AxislineStyle = LineStyle.Solid,
                               TickStyle = TickStyle.Outside,
                               MajorGridlineStyle = LineStyle.Solid,
                               MajorGridlineColor = OxyColor.FromAColor(20, OxyColors.DarkBlue),
                               MajorStep = MajorStepRequired() ? 5 : 1,
                               IsPanEnabled = true,
                               IsZoomEnabled = true,
                               FontSize = 10,
                               IsTickCentered = true,
                               GapWidth = 0.05,
                               AxisTitleDistance = 30,
                               LabelField = "Hour",
                               Title = "Date",
                               Key = "PNLYAxis",
                               //Labels = values,
                               Angle = 90
                           });
                    plotModel.PlotMargins = new OxyThickness(20, 4, 20, 70);
                    plotModel.Series.Add(new BarSeries()
                    {
                        FillColor = OxyColors.IndianRed,
                        ItemsSource = summaryValues.OrderBy(a => a.Hour).ToList(),
                        ValueField = type, //"PNL",
                        //XAxisKey = "HourXAxis",
                        //YAxisKey = "PNLYAxis",
                        TrackerFormatString = "{0}\n{Hour:MM/dd/yyyy}\n{4:$0,00.00}",
                        NegativeFillColor = OxyColors.MediumBlue,
                    });
                    var lineSeries1 = new LineSeries();
                    var dataItemValues = new Collection<Axis>(); // use with non DateTime x axis

                    dataItemValues = new Collection<Axis>();
                    for (int i = 0; i < summaryValuesRT.Count; i++)
                    {
                        dataItemValues.Add(new Axis() { X = i, Y = Math.Round(summaryValuesRT[i].PNL, 2), n = i + 1 });
                    }
                    lineSeries1 = new LineSeries()
                    {
                        CanTrackerInterpolatePoints = false,
                        DataFieldX = "X",
                        DataFieldY = "Y",
                        ItemsSource = dataItemValues,
                        TrackerFormatString = "HE: " + "{n:0}, " + "RT: " + "{Y:0}",
                        XAxisKey = "HourXAxis",
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 2,
                        MarkerStrokeThickness = 0
                    };
                    lineSeries1.Color = OxyColors.DarkGoldenrod;
                    lineSeries1.MarkerFill = OxyColors.Goldenrod;
                    plotModel.Series.Add(lineSeries1);

                    //DA
                    var lineSeries2 = new LineSeries();
                    var dataItemValues2 = new Collection<Axis>(); // use with non DateTime x axis

                    dataItemValues2 = new Collection<Axis>();
                    for (int i = 0; i < summaryValuesDA.Count; i++)
                    {
                        dataItemValues2.Add(new Axis() { X = i, Y = Math.Round(summaryValuesDA[i].PNL, 2), n = i + 1 });
                    }
                    lineSeries2 = new LineSeries()
                    {
                        CanTrackerInterpolatePoints = false,
                        DataFieldX = "X",
                        DataFieldY = "Y",
                        ItemsSource = dataItemValues2,
                        TrackerFormatString = "HE: " + "{n:0}, " + "DA: " + "{Y:0}",
                        XAxisKey = "HourXAxis",
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 2,
                        MarkerStrokeThickness = 0
                    };
                    lineSeries2.Color = OxyColors.DarkGreen;
                    lineSeries2.MarkerFill = OxyColors.DarkGreen;
                    plotModel.Series.Add(lineSeries2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            PlotDataFirst = plotModel as PlotModel;
        }
        public void DrawSecondGraph(string type)
        {
            List<Coordinates> summaryValues = GetCoordinates(true, type);
            PlotModel plotModel = new PlotModel();
            OxyColor c = OxyColors.DarkBlue;
            plotModel.Axes.Add(new LinearAxis()//AxisPosition.Right
            {
                Key = "Y2AxisB",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.05,
                StringFormat = "$0,00",
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 0.995,
                IntervalLength = 40,
                Title = "PNL"
            });
            if (!SummaryChecked)
            {
                CategoryAxis categoryAxis = new CategoryAxis();
                categoryAxis = new CategoryAxis()//AxisPosition.Bottom
                {
                    Position = AxisPosition.Bottom,
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromAColor(20, c),
                    IsPanEnabled = true,
                    IsZoomEnabled = true,
                    TickStyle = TickStyle.None,
                    Key = "XAxisBCategory",
                    TextColor = OxyColors.Transparent,
                    MajorStep = MajorStepRequired() ? 5 : 1
                };
                if (summaryValues.Count > 0 && summaryValues.Count < 22)
                {
                    for (int i = 0; i <= summaryValues.Max(a => Int32.Parse(a.Hour.ToString())); i++)
                    {
                        categoryAxis.Labels.Add(i.ToString("00"));
                    }
                }
                else
                {
                    foreach (Coordinates item in summaryValues)
                    {
                        categoryAxis.Labels.Add(item.Hour.ToString());
                    }
                }
                plotModel.Axes.Add(categoryAxis);
              //  plotModel.PlotMargins = new OxyThickness(20, 4, 20, 70);
                AreaSeries areaSeries1 = new AreaSeries()
                {

                    Fill = OxyColors.LightBlue,
                    Color = OxyColors.Black,
                    StrokeThickness = 1,
                    DataFieldX = "Hour",
                    DataFieldY = "PNL",
                    LineStyle = LineStyle.Solid,
                    TrackerFormatString = "{0}\n{2:0}\n{4:$0,00}",
                    XAxisKey = "XAxisBCategory",
                    YAxisKey = "Y2AxisB"
                };
                foreach (Coordinates item in summaryValues)
                {
                    areaSeries1.Points.Add(new DataPoint(Convert.ToDouble(item.Hour), item.PNL));
                    areaSeries1.Points2.Add(new DataPoint(Convert.ToDouble(item.Hour), 0));
                }
                plotModel.Series.Add(areaSeries1);
            }
            else
            {
                plotModel.Axes.Add(
                       new DateTimeAxis()
                       {
                           Position = AxisPosition.Bottom,
                           AxislineStyle = LineStyle.Solid,
                           TickStyle = TickStyle.Outside,
                           MajorGridlineStyle = LineStyle.Solid,
                           MajorGridlineColor = OxyColor.FromAColor(20, OxyColors.DarkBlue),
                           MajorStep = MajorStepRequired() ? 5 : 1,
                           IsPanEnabled = true,
                           IsZoomEnabled = true,
                           FontSize = 10,
                           AxisTitleDistance = 30,
                           Title = "Date",
                           Key = "HourXAxis",
                           Angle = 90,

                       });
               // plotModel.PlotMargins = new OxyThickness(20, 4, 20, 70);
                AreaSeries areaSeries1 = new AreaSeries()
                {
                    Fill = OxyColors.LightBlue,
                    DataFieldX2 = "Hour",
                    DataFieldY2 = "PNL",
                    Color = OxyColors.Black,
                    StrokeThickness = 1,
                    MarkerFill = OxyColors.Transparent,
                    DataFieldX = "Hour",
                    DataFieldY = "PNL",
                    LineStyle = LineStyle.Solid,
                    CanTrackerInterpolatePoints = false,
                    TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
                    XAxisKey = "HourXAxis",
                    YAxisKey = "Y2AxisB"
                };
                foreach (Coordinates item in summaryValues)
                {
                    areaSeries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime(item.Hour)), item.PNL));
                    areaSeries1.Points2.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime(item.Hour)), 0));
                }
                plotModel.Series.Add(areaSeries1);
            }
            PlotDataSecond = plotModel as PlotModel;
        }

        public void DrawThirdGraph()
        {
            List<Coordinates> summaryValues = GetGraphCoordinates();
            PlotModel plotModel = new PlotModel();
            plotModel.PlotAreaBackground = OxyColors.White;
            plotModel.Background = OxyColors.White;
            plotModel.TextColor = OxyColors.White;
            //plotModel.PlotAreaBorderThickness = 0;
            //plotModel = new PlotModel() {  LegendPlacement = LegendPlacement.Outside, LegendPosition = LegendPosition.RightTop, LegendOrientation = LegendOrientation.Vertical };

            plotModel.Axes.Add(new LinearAxis()//AxisPosition.Left
            {
                Position = AxisPosition.Left,
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                Title = "PNL",
                StringFormat = "$0,00",
            }); ;

            if (!SummaryChecked)
            {
                List<string> items = summaryValues.Select(x => x.Hour.ToString()).Distinct().ToList();
                List<string> portfolios = summaryValues.Select(x => x.Portfolio.ToString()).Distinct().ToList();
                List<XValues> pkValues = new List<XValues>();
                List<XValues> xaxisValues = new List<XValues>();
                for (int i = 1; i <= 24; i++)
                {
                    xaxisValues.Add(new XValues { Hour = i });
                }
                Parallel.ForEach(portfolios, p =>
                {
                    pkValues.Add(new XValues { Portfolio = (p) });
                });
                plotModel.Axes.Add(new CategoryAxis
                {
                    ItemsSource = xaxisValues.ToList(),
                    LabelField = "Hour",
                    Title = "Hour",
                    MajorStep = MajorStepRequired() ? 5 : 1
                });
                foreach (var item in pkValues)
                {
                    List<Coordinates> collItems = new List<Coordinates>();
                    for (int i = 1; i <= 24; i++)
                    {
                        Coordinates eachItem = summaryValues.Where(a => Convert.ToInt32(a.Hour) == i && a.Portfolio == item.Portfolio).FirstOrDefault();
                        if (eachItem == null)
                        {
                            collItems.Add(new Coordinates { Hour = i, Portfolio = item.Portfolio, PNL = 0 });
                        }
                        else
                        {
                            collItems.Add(new Coordinates { Hour = i, Portfolio = item.Portfolio, PNL = eachItem.PNL });
                        }
                    }
                    plotModel.Series.Add(new BarSeries() //ColumnSeries
                    {
                        Title = item.Portfolio.ToString(),
                        ItemsSource = collItems.ToList(),
                        ValueField = "PNL",

                        TrackerFormatString = "{Portfolio:0} --> {Hour:0}, {PNL:0,00}"
                    });
                }
            }
            else
            {
                List<string> portfolios = summaryValues.Select(x => x.Portfolio.ToString()).Distinct().ToList();
                List<XValues> pkValues = new List<XValues>();
                List<XValues> xaxisValues = new List<XValues>();
                List<DateTime> dateList = new List<DateTime>();
                Dictionary<string, Dictionary<DateTime, double>> portfolioHash = new Dictionary<string, Dictionary<DateTime, double>>();
                foreach (Coordinates coordinate in summaryValues)
                {
                    Dictionary<DateTime, double> dateHash = new Dictionary<DateTime, double>();
                    if (portfolioHash.ContainsKey(coordinate.Portfolio))
                    {
                        dateHash = portfolioHash[coordinate.Portfolio];
                        portfolioHash.Remove(coordinate.Portfolio);
                    }
                    dateHash.Add((DateTime)coordinate.Hour, coordinate.PNL);
                    portfolioHash.Add(coordinate.Portfolio, dateHash);
                    if (!dateList.Contains((DateTime)coordinate.Hour))
                    {
                        dateList.Add((DateTime)coordinate.Hour);
                    }
                }
                foreach (DateTime date in dateList)
                {
                    xaxisValues.Add(new XValues { Hour = date.ToString("d") });
                }
                Parallel.ForEach(portfolios, p =>
                {
                    pkValues.Add(new XValues { Portfolio = (p) });
                });
                plotModel.Axes.Add(new CategoryAxis
                {
                    ItemsSource = xaxisValues.ToList(),
                    LabelField = "Hour",
                    Title = "Date",
                    Angle = 90,
                    FontSize = 10,
                    IsTickCentered = true,
                    GapWidth = 0.05,
                    AxisTitleDistance = 30,
                    MajorStep = MajorStepRequired() ? 5 : 1

                });
                foreach (XValues item in pkValues)
                {
                    List<Coordinates> collItems = new List<Coordinates>();
                    if (portfolioHash.ContainsKey(item.Portfolio))
                    {
                        Dictionary<DateTime, double> dateHash = portfolioHash[item.Portfolio];
                        foreach (DateTime date in dateList)
                        {
                            if (dateHash.ContainsKey(date))
                            {
                                collItems.Add(new Coordinates { Hour = date.ToString("d"), Portfolio = item.Portfolio, PNL = dateHash[date] });
                            }
                            else
                            {
                                collItems.Add(new Coordinates { Hour = date.ToString("d"), Portfolio = item.Portfolio, PNL = 0 });
                            }
                        }
                        plotModel.Series.Add(new BarSeries() //ColumnSeries
                        {
                            Title = item.Portfolio.ToString(),
                            ItemsSource = collItems.ToList(),
                            ValueField = "PNL",
                            TrackerFormatString = "{Portfolio:0}\n{Hour:0}, {PNL:0,00}"
                        }); //ColumnSeries
                    }
                }
            }
            PlotDataThird = plotModel as PlotModel;
        }

        public void DrawFourthGraph()
        {
            List<Coordinates> summaryValues = GetGraphCoordinates();
            List<DateTime> dateList = new List<DateTime>();
            List<string> portfolios = summaryValues.Select(x => x.Portfolio.ToString()).Distinct().ToList();
            List<string> pkValues = new List<string>();
            Parallel.ForEach(portfolios, p =>
            {
                pkValues.Add(p);
            });
            Dictionary<string, double> totalPnlHash = new Dictionary<string, double>();
            Dictionary<string, Dictionary<object, double>> portfolioHash = new Dictionary<string, Dictionary<object, double>>();
            foreach (Coordinates coordinate in summaryValues)
            {
                try
                {
                    double totalPnl = 0;
                    Dictionary<object, double> pnlHash = new Dictionary<object, double>();
                    if (portfolioHash.ContainsKey(coordinate.Portfolio))
                    {
                        pnlHash = portfolioHash[coordinate.Portfolio];
                        portfolioHash.Remove(coordinate.Portfolio);
                        if (totalPnlHash.ContainsKey(coordinate.Portfolio))
                        {
                            totalPnl = totalPnlHash[coordinate.Portfolio];
                            totalPnlHash.Remove(coordinate.Portfolio);
                        }
                    }
                    double pnl = coordinate.PNL;
                    if (!SummaryChecked)
                    {
                        if (pnlHash.ContainsKey((int)coordinate.Hour))
                        {
                            pnl += pnlHash[(int)coordinate.Hour];
                            pnlHash.Remove((int)coordinate.Hour);
                        }
                        pnlHash.Add((int)coordinate.Hour, pnl);
                    }
                    else
                    {
                        totalPnl += pnl;
                        totalPnlHash.Add(coordinate.Portfolio, totalPnl);
                        if (pnlHash.ContainsKey(((DateTime)coordinate.Hour).Date))
                        {
                            pnlHash.Remove(((DateTime)coordinate.Hour).Date);
                        }
                        pnlHash.Add(((DateTime)coordinate.Hour).Date, totalPnl);
                        if (!dateList.Contains(((DateTime)coordinate.Hour).Date))
                        {
                            dateList.Add(((DateTime)coordinate.Hour).Date);
                        }
                    }
                    List<object> keys = pnlHash.Keys.ToList<object>();
                    portfolioHash.Add(coordinate.Portfolio, pnlHash);
                }
                catch (Exception ex)
                {

                }
            }
            PlotModel plotModel = new PlotModel();
            OxyColor c = OxyColors.DarkBlue;
            plotModel.Axes.Add(new LinearAxis()//AxisPosition.Right
            {
                Position = AxisPosition.Right,
                Key = "Y2AxisB",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.05,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 0.995,
                StringFormat = "$0,00",
                IntervalLength = 40,
                Title = "PNL"
            });
            if (!SummaryChecked)
            {
                try
                {
                    CategoryAxis categoryAxis = new CategoryAxis();
                    categoryAxis = new CategoryAxis()//AxisPosition.Bottom
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MajorGridlineColor = OxyColor.FromAColor(20, c),
                        IsPanEnabled = true,
                        IsZoomEnabled = true,
                        TickStyle = TickStyle.None,
                        Key = "XAxisBCategory",
                        TextColor = OxyColors.Transparent,
                        MajorStep = MajorStepRequired() ? 5 : 1

                    };
                    for (int hour = 1; hour < 25; hour++)
                    {
                        categoryAxis.Labels.Add(hour.ToString("00"));
                    }
                    plotModel.Axes.Add(categoryAxis);
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                plotModel.Axes.Add(
                                new DateTimeAxis()
                                {
                                    Position = AxisPosition.Bottom,
                                    AxislineStyle = LineStyle.Solid,
                                    MajorGridlineColor = OxyColor.FromAColor(20, OxyColors.DarkBlue),
                                    IsPanEnabled = false,
                                    IsZoomEnabled = true,
                                    FontSize = 10,
                                    Title = "Date",
                                    Angle = 90,
                                    MajorStep = MajorStepRequired() ? 5 : 1
                                });
            }
            List<string> portfolioList = new List<string>();
            foreach (string portfolio in pkValues)
            {
                Dictionary<object, double> pnlHash = portfolioHash[portfolio];
                if (!SummaryChecked)
                {
                    try
                    {
                        AreaSeries areaSeries1 = new AreaSeries()
                        {
                            StrokeThickness = 1,
                            LineStyle = LineStyle.Solid,
                            TrackerFormatString = portfolio + "\n" + "{2:0}\n{4:$0,00}",
                            //TrackerFormatString = "{2:0}\n{4:$0,00}",
                            XAxisKey = "XAxisBCategory",
                            YAxisKey = "Y2AxisB"
                        };
                        for (int hour = 1; hour < 25; hour++)
                        {
                            double pnl = 0;
                            if (pnlHash.ContainsKey(hour))
                            {
                                for (int hour1 = 1; hour1 <= hour; hour1++)
                                {
                                    if (pnlHash.ContainsKey(hour1))
                                    {
                                        pnl += pnlHash[hour1];
                                    }
                                }
                            }
                            if (pnl != 0)
                            {
                                areaSeries1.Points.Add(new DataPoint(hour, pnl));

                            }
                        }
                       // plotModel.PlotMargins = new OxyThickness(20, 4, 20, 70);
                        plotModel.Series.Add(areaSeries1);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                    try
                    {
                        AreaSeries areaSeries1 = new AreaSeries()
                        {
                            DataFieldX2 = "X",
                            DataFieldY2 = "Y",
                            StrokeThickness = 1,
                            Fill = OxyColors.Transparent,
                            DataFieldX = "X",
                            DataFieldY = "Y",
                            LineStyle = LineStyle.Solid,
                            CanTrackerInterpolatePoints = false,
                            TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
                            XAxisKey = "HourXAxis",
                            YAxisKey = "Y2AxisB"
                        };
                        foreach (DateTime date in dateList.OrderBy(a => a))
                        {
                            double pnl = 0;
                            if (pnlHash.ContainsKey(date))
                            {
                                pnl = pnlHash[date];
                            }
                            if (pnl != 0)
                            {
                                areaSeries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime(date)), pnl));
                            }
                        }
                        //plotModel.PlotMargins = new OxyThickness(20, 4, 20, 70);
                        plotModel.Series.Add(areaSeries1);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            PlotDataFour = plotModel as PlotModel;
        }

        #endregion
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

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {

        private BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter.Convert(value, targetType, parameter, culture) as Visibility?;
            return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter.ConvertBack(value, targetType, parameter, culture) as bool?;
            return result == true ? false : true;
        }

    }

    public class Axis
    {

        public int X { get; set; }
        public double Y { get; set; }
        public int n { get; set; }
    }
}
