using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using Vayu.DBLibrary;
using Vayu.WorkbookStatistics.Model;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class UptosPTPBidEntryViewModel : BindableBase
    {
        #region Declaration

        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName;
        /// <summary>
        /// The m path window
        /// </summary>
        private WorkbookStatistics.Views.MainWindow mPathWindow = null;
        /// <summary>
        /// The m delete
        /// </summary>
        private bool mDelete = false;

        #endregion

        #region Properties

        #region Relay Command Properties

        /// <summary>
        /// Gets or sets the run ramp command.
        /// </summary>
        /// <value>
        /// The run ramp command.
        /// </value>
        public DelegateCommand RunRampCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run midnight command.
        /// </summary>
        /// <value>
        /// The run midnight command.
        /// </value>
        public DelegateCommand RunMidnightCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run clear command.
        /// </summary>
        /// <value>
        /// The run clear command.
        /// </value>
        public DelegateCommand RunClearCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run off peak command.
        /// </summary>
        /// <value>
        /// The run off peak command.
        /// </value>
        public DelegateCommand RunOffPeakCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run uncheck all command.
        /// </summary>
        /// <value>
        /// The run uncheck all command.
        /// </value>
        public DelegateCommand RunUncheckAllCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run peak command.
        /// </summary>
        /// <value>
        /// The run peak command.
        /// </value>
        public DelegateCommand RunPeakCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run check all command.
        /// </summary>
        /// <value>
        /// The run check all command.
        /// </value>
        public DelegateCommand RunCheckAllCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run save command.
        /// </summary>
        /// <value>
        /// The run save command.
        /// </value>
        public DelegateCommand RunSaveCommand { private set; get; }

        #endregion

        /// <summary>
        /// The m parent model
        /// </summary>
        private WorkbookStatistics.ViewModels.MainWindowViewModel mParentModel;
        /// <summary>
        /// Gets or sets the parent model.
        /// </summary>
        /// <value>
        /// The parent model.
        /// </value>
        public WorkbookStatistics.ViewModels.MainWindowViewModel ParentModel
        {
            get
            {
                return mParentModel;
            }
            set
            {
                mParentModel = value;
                RaisePropertyChanged("ParentModel");
            }
        }
        /// <summary>
        /// The market identifier
        /// </summary>
        Dictionary<string, int> marketID;
        /// <summary>
        /// The m trade selected date
        /// </summary>
        private DateTime mTradeSelectedDate;
        /// <summary>
        /// Gets or sets the trade selected date.
        /// </summary>
        /// <value>
        /// The trade selected date.
        /// </value>
        public DateTime TradeSelectedDate
        {
            get
            {
                return mTradeSelectedDate;
            }
            set
            {
                mTradeSelectedDate = value;
                RaisePropertyChanged("TradeSelectedDate");
            }
        }
        /// <summary>
        /// The m market name
        /// </summary>
        private string mMarketName;
        /// <summary>
        /// Gets or sets the name of the market.
        /// </summary>
        /// <value>
        /// The name of the market.
        /// </value>
        public string MarketName
        {
            get
            {
                return mMarketName;
            }
            set
            {
                mMarketName = value;
                RaisePropertyChanged("mMarketName");
            }
        }
        /// <summary>
        /// The m portfolio name
        /// </summary>
        private string mPortfolioName;
        /// <summary>
        /// Gets or sets the name of the portfolio.
        /// </summary>
        /// <value>
        /// The name of the portfolio.
        /// </value>
        public string PortfolioName
        {
            get
            {
                return mPortfolioName;
            }
            set
            {
                mPortfolioName = value;
                RaisePropertyChanged("PortfolioName");
            }
        }
        /// <summary>
        /// The m portfolio key
        /// </summary>
        private int mPortfolioKey;
        /// <summary>
        /// Gets or sets the portfolio key.
        /// </summary>
        /// <value>
        /// The portfolio key.
        /// </value>
        public int PortfolioKey
        {
            get
            {
                return mPortfolioKey;
            }
            set
            {
                mPortfolioKey = value;
                RaisePropertyChanged("PortfolioKey");
            }
        }
        /// <summary>
        /// The m trade identifier
        /// </summary>
        private string mTradeID;
        /// <summary>
        /// Gets or sets the trade identifier.
        /// </summary>
        /// <value>
        /// The trade identifier.
        /// </value>
        public string TradeID
        {
            get
            {
                return mTradeID;
            }
            set
            {
                mTradeID = value;
                RaisePropertyChanged("TradeID");
            }
        }
        /// <summary>
        /// The m source combo selected item
        /// </summary>
        private PricingNode mSourceComboSelectedItem;
        /// <summary>
        /// Gets or sets the source combo selected item.
        /// </summary>
        /// <value>
        /// The source combo selected item.
        /// </value>
        public PricingNode SourceComboSelectedItem
        {
            get
            {
                return mSourceComboSelectedItem;
            }
            set
            {
                mSourceComboSelectedItem = value;
                RaisePropertyChanged("SourceComboSelectedItem");

            }
        }
        /// <summary>
        /// The m sink combo selected item
        /// </summary>
        private PricingNode mSinkComboSelectedItem;
        /// <summary>
        /// Gets or sets the sink combo selected item.
        /// </summary>
        /// <value>
        /// The sink combo selected item.
        /// </value>
        public PricingNode SinkComboSelectedItem
        {
            get
            {
                return mSinkComboSelectedItem;
            }
            set
            {
                mSinkComboSelectedItem = value;
                RaisePropertyChanged("SinkComboSelectedItem");
            }
        }
        /// <summary>
        /// The m market list
        /// </summary>
        private List<string> mMarketList;
        /// <summary>
        /// Gets or sets the market list.
        /// </summary>
        /// <value>
        /// The market list.
        /// </value>
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
        /// <summary>
        /// The m market combo selected value
        /// </summary>
        private string mMarketComboSelectedValue;
        /// <summary>
        /// Gets or sets the market combo selected value.
        /// </summary>
        /// <value>
        /// The market combo selected value.
        /// </value>
        public string MarketComboSelectedValue
        {
            get
            {
                return mMarketComboSelectedValue;
            }
            set
            {
                mMarketComboSelectedValue = value;
                SetSourceSink();
                RaisePropertyChanged("MarketComboSelectedValue");

            }
        }
        /// <summary>
        /// The m source node list
        /// </summary>
        private List<PricingNode> mSourceNodeList;
        /// <summary>
        /// Gets or sets the source node list.
        /// </summary>
        /// <value>
        /// The source node list.
        /// </value>
        public List<PricingNode> SourceNodeList
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
        /// The m sink node list
        /// </summary>
        private List<PricingNode> mSinkNodeList;
        /// <summary>
        /// Gets or sets the sink node list.
        /// </summary>
        /// <value>
        /// The sink node list.
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
        /// The m h e1 checked
        /// </summary>
        private bool mHE1Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e1 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e1 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE1Checked
        {
            get
            {
                return mHE1Checked;
            }
            set
            {
                mHE1Checked = value;
                FixAverages();
                RaisePropertyChanged("HE1Checked");
            }
        }
        /// <summary>
        /// The m h e2 checked
        /// </summary>
        private bool mHE2Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e2 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e2 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE2Checked
        {
            get
            {
                return mHE2Checked;
            }
            set
            {
                mHE2Checked = value;
                FixAverages();
                RaisePropertyChanged("HE2Checked");
            }
        }
        /// <summary>
        /// The m h e3 checked
        /// </summary>
        private bool mHE3Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e3 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e3 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE3Checked
        {
            get
            {
                return mHE3Checked;
            }
            set
            {
                mHE3Checked = value;
                FixAverages();
                RaisePropertyChanged("HE3Checked");
            }
        }
        /// <summary>
        /// The m h e4 checked
        /// </summary>
        private bool mHE4Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e4 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e4 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE4Checked
        {
            get
            {
                return mHE4Checked;
            }
            set
            {
                mHE4Checked = value;
                FixAverages();
                RaisePropertyChanged("HE4Checked");
            }
        }
        /// <summary>
        /// The m h e5 checked
        /// </summary>
        private bool mHE5Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e5 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e5 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE5Checked
        {
            get
            {
                return mHE5Checked;
            }
            set
            {
                mHE5Checked = value;
                FixAverages();
                RaisePropertyChanged("HE5Checked");
            }
        }
        /// <summary>
        /// The m h e6 checked
        /// </summary>
        private bool mHE6Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e6 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e6 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE6Checked
        {
            get
            {
                return mHE6Checked;
            }
            set
            {
                mHE6Checked = value;
                FixAverages();
                RaisePropertyChanged("HE6Checked");
            }
        }
        /// <summary>
        /// The m h e7 checked
        /// </summary>
        private bool mHE7Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e7 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e7 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE7Checked
        {
            get
            {
                return mHE7Checked;
            }
            set
            {
                mHE7Checked = value;
                FixAverages();
                RaisePropertyChanged("HE7Checked");
            }
        }
        /// <summary>
        /// The m h e8 checked
        /// </summary>
        private bool mHE8Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e8 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e8 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE8Checked
        {
            get
            {
                return mHE8Checked;
            }
            set
            {
                mHE8Checked = value;
                FixAverages();
                RaisePropertyChanged("HE8Checked");
            }
        }
        /// <summary>
        /// The m h e9 checked
        /// </summary>
        private bool mHE9Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e9 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e9 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE9Checked
        {
            get
            {
                return mHE9Checked;
            }
            set
            {
                mHE9Checked = value;
                FixAverages();
                RaisePropertyChanged("HE9Checked");
            }
        }
        /// <summary>
        /// The m h e10 checked
        /// </summary>
        private bool mHE10Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e10 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e10 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE10Checked
        {
            get
            {
                return mHE10Checked;
            }
            set
            {
                mHE10Checked = value;
                FixAverages();
                RaisePropertyChanged("HE10Checked");
            }
        }
        /// <summary>
        /// The m h e11 checked
        /// </summary>
        private bool mHE11Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e11 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e11 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE11Checked
        {
            get
            {
                return mHE11Checked;
            }
            set
            {
                mHE11Checked = value;
                FixAverages();
                RaisePropertyChanged("HE11Checked");
            }
        }
        /// <summary>
        /// The m h e12 checked
        /// </summary>
        private bool mHE12Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e12 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e12 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE12Checked
        {
            get
            {
                return mHE12Checked;
            }
            set
            {
                mHE12Checked = value;
                FixAverages();
                RaisePropertyChanged("HE12Checked");
            }
        }
        /// <summary>
        /// The m h e13 checked
        /// </summary>
        private bool mHE13Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e13 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e13 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE13Checked
        {
            get
            {
                return mHE13Checked;
            }
            set
            {
                mHE13Checked = value;
                FixAverages();
                RaisePropertyChanged("HE13Checked");
            }
        }
        /// <summary>
        /// The m h e14 checked
        /// </summary>
        private bool mHE14Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e14 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e14 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE14Checked
        {
            get
            {
                return mHE14Checked;
            }
            set
            {
                mHE14Checked = value;
                FixAverages();
                RaisePropertyChanged("HE14Checked");
            }
        }
        /// <summary>
        /// The m h e15 checked
        /// </summary>
        private bool mHE15Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e15 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e15 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE15Checked
        {
            get
            {
                return mHE15Checked;
            }
            set
            {
                mHE15Checked = value;
                FixAverages();
                RaisePropertyChanged("HE15Checked");
            }
        }
        /// <summary>
        /// The m h e16 checked
        /// </summary>
        private bool mHE16Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e16 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e16 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE16Checked
        {
            get
            {
                return mHE16Checked;
            }
            set
            {
                mHE16Checked = value;
                FixAverages();
                RaisePropertyChanged("HE16Checked");
            }
        }
        /// <summary>
        /// The m h e17 checked
        /// </summary>
        private bool mHE17Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e17 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e17 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE17Checked
        {
            get
            {
                return mHE17Checked;
            }
            set
            {
                mHE17Checked = value;
                FixAverages();
                RaisePropertyChanged("HE17Checked");
            }
        }
        /// <summary>
        /// The m h e18 checked
        /// </summary>
        private bool mHE18Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e18 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e18 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE18Checked
        {
            get
            {
                return mHE18Checked;
            }
            set
            {
                mHE18Checked = value;
                FixAverages();
                RaisePropertyChanged("HE18Checked");
            }
        }
        /// <summary>
        /// The m h e19 checked
        /// </summary>
        private bool mHE19Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e19 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e19 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE19Checked
        {
            get
            {
                return mHE19Checked;
            }
            set
            {
                mHE19Checked = value;
                FixAverages();
                RaisePropertyChanged("HE19Checked");
            }
        }
        /// <summary>
        /// The m h e20 checked
        /// </summary>
        private bool mHE20Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e20 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e20 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE20Checked
        {
            get { return mHE20Checked; }
            set
            {
                mHE20Checked = value;
                FixAverages();
                RaisePropertyChanged("HE20Checked");
            }
        }
        /// <summary>
        /// The m h e21 checked
        /// </summary>
        private bool mHE21Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e21 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e21 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE21Checked
        {
            get { return mHE21Checked; }
            set
            {
                mHE21Checked = value;
                FixAverages();
                RaisePropertyChanged("HE21Checked");
            }
        }
        /// <summary>
        /// The m h e22 checked
        /// </summary>
        private bool mHE22Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e22 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e22 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE22Checked
        {
            get
            {
                return mHE22Checked;
            }
            set
            {
                mHE22Checked = value;
                FixAverages();
                RaisePropertyChanged("HE22Checked");
            }
        }
        /// <summary>
        /// The m h e23 checked
        /// </summary>
        private bool mHE23Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e23 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e23 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE23Checked
        {
            get
            {
                return mHE23Checked;
            }
            set
            {
                mHE23Checked = value;
                FixAverages();
                RaisePropertyChanged("HE23Checked");
            }
        }
        /// <summary>
        /// The m h e24 checked
        /// </summary>
        private bool mHE24Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [h e24 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e24 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE24Checked
        {
            get
            {
                return mHE24Checked;
            }
            set
            {
                mHE24Checked = value;
                FixAverages();
                RaisePropertyChanged("HE24Checked");
            }
        }
        /// <summary>
        /// The total mw
        /// </summary>
        private double totalMW;
        /// <summary>
        /// Gets or sets the total mw.
        /// </summary>
        /// <value>
        /// The total mw.
        /// </value>
        public double TotalMW
        {
            get { return totalMW; }
            set
            {
                totalMW = value;
                RaisePropertyChanged("TotalMW");
            }
        }
        /// <summary>
        /// The total price
        /// </summary>
        private double totalPrice;
        /// <summary>
        /// Gets or sets the total price.
        /// </summary>
        /// <value>
        /// The total price.
        /// </value>
        public double TotalPrice
        {
            get { return totalPrice; }
            set
            {
                totalPrice = value;
                RaisePropertyChanged("TotalPrice");
            }
        }
        /// <summary>
        /// The m w
        /// </summary>
        private double mW;
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW
        {
            get { return mW; }
            set
            {
                mW = value;
                RaisePropertyChanged("MW");
                FixAverages();
            }
        }
        /// <summary>
        /// The price
        /// </summary>
        private double price;
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price
        {
            get { return price; }
            set
            {
                price = value;
                RaisePropertyChanged("Price");
                FixAverages();
            }
        }
        /// <summary>
        /// The msubmit date
        /// </summary>
        private DateTime msubmitDate;
        /// <summary>
        /// Gets or sets the submit date.
        /// </summary>
        /// <value>
        /// The submit date.
        /// </value>
        public DateTime SubmitDate
        {
            get { return msubmitDate; }
            set
            {
                msubmitDate = value;
                RaisePropertyChanged("SubmitDate");
            }
        }

        #endregion
        public UptosPTPBidEntryViewModel(IDataService dataService)
        {
            mDataService = dataService;
            mDataService.loadDBCommands();
            List<string> marketList = new List<string> { "ERCOT" };
            MarketList = null;
            MarketList = marketList;
            mDelete = false;
            RunRampCommand = new DelegateCommand(RampCommand);
            RunMidnightCommand = new DelegateCommand(MidnightCommand);
            RunClearCommand = new DelegateCommand(uncheckAll);
            RunOffPeakCommand = new DelegateCommand(OffPeakCommand);
            RunCheckAllCommand = new DelegateCommand(CheckAllCommand);
            RunUncheckAllCommand = new DelegateCommand(uncheckAll);
            RunPeakCommand = new DelegateCommand(PeakCommand);
            RunSaveCommand = new DelegateCommand(SaveCommand);
            marketID = new Dictionary<string, int>();
            marketID.Add("ERCOT", 9);
        }
        #region Public Methods


        public void SetValues(string market, Portfolio portfolio, DateTime tradeSelectedDate, DateTime submitDate)
        {
            if (market == "ERCOT")
            {
                MarketComboSelectedValue = market;
                TradeSelectedDate = tradeSelectedDate;
                PortfolioName = portfolio.Name;
                PortfolioKey = portfolio.ID;
                SubmitDate = submitDate;
                TradeID = mDataService.SetTradeId(portfolio.ID.ToString(), market);
            }
        }

        public void CheckAllCommand()
        {
            int start = 1;
            int end = 25;
            for (int i = start; i < end; i++)
            {
                setHour(i);
            }
            FixAverages();
        }

        public void SaveCommand()
        {
            string heStr = HeList();
            if (SourceComboSelectedItem == null || SinkComboSelectedItem == null || Price == 0 || MW == 0 || heStr == null || heStr.Trim().Length == 0)
            {
                MessageBox.Show("Please Enter All the Values");
                return;
            }
            string market1 = MarketComboSelectedValue;
            string tradeId = TradeID;
            if (mDelete)
            {
                mDataService.DeleteUptosPtpBids(tradeId, SubmitDate, market1);
            }
            try
            {
                string source = mSourceComboSelectedItem.NodeName;
                string sink = mSinkComboSelectedItem.NodeName;
                string tradeID = TradeID;
                double price = Price;
                double mw = MW;
                string[] hourTokens = heStr.Split('.');
                List<Bid> bidList = new List<Bid>();
                string key = MarketComboSelectedValue;
                PricingNode sourceNode = DBAccess.GetNodeFromName(source, marketID[key]);
                PricingNode sinkNode = DBAccess.GetNodeFromName(sink, marketID[key]);
                List<ValidateBids> validateBidsList = new List<ValidateBids>();
                ValidateBids validateBids = new ValidateBids();
                validateBids.Source = sourceNode.NodeKey;
                validateBids.Sink = sinkNode.NodeKey;
                validateBids.Price = price;
                validateBids.MW = mw;
                validateBids.AnalysisType = heStr;
                validateBidsList.Add(validateBids);
                foreach (string hour in hourTokens)
                {
                    if (hour != "")
                    {
                        Bid bid = new Bid();
                        bid.Source = sourceNode.NodeKey;
                        bid.Sink = sinkNode.NodeKey;
                        bid.MW = mw;
                        bid.Price = price;
                        bid.Market = marketID[key];
                        bid.MarketDateTime = TradeSelectedDate.AddHours(int.Parse(hour));
                        bid.PortfolioKey = PortfolioKey;
                        bid.BidId = tradeID;
                        bid.IsUptos = true;
                        bidList.Add(bid);
                    }
                }
                if (ParentModel.ValidateBids(validateBidsList))
                {
                    DBAccess.SaveBids(bidList, mUser);
                }
                else
                {
                    return;
                }
                if (!string.IsNullOrEmpty(PortfolioName))
                {
                    if (ParentModel != null)
                    {
                        Portfolio port = new Portfolio();
                        port.Name = PortfolioName;
                        port.ID = PortfolioKey;
                        port.Market = MarketComboSelectedValue;
                        port.IsUptos = true;
                        TradeID = mDataService.SetTradeId(port.ID.ToString(), MarketComboSelectedValue);
                        Path path = new Path();
                        path.AnalysisType = heStr;
                        path.BidId = tradeId;
                        path.IsUptos = true;
                        path.Market = 9;
                        path.MarketDateTime = TradeSelectedDate;
                        path.MW = mw;
                        path.Portfolio = PortfolioName;
                        path.PortfolioDate = TradeSelectedDate;
                        path.PortfolioKey = PortfolioKey;
                        path.Price = price;
                        path.Submit = true;
                        path.Sink = sink;
                        path.Status = "IMPORTED";
                        path.SinkZone = sinkNode.Zone;
                        path.Source = source;
                        path.SourceZone = sourceNode.Zone;
                        path.SourcePNodeId = sourceNode.ExternalNodeId;
                        path.SinkPNodeId = sinkNode.ExternalNodeId;
                        ParentModel.SetValues(MarketComboSelectedValue, port, ParentModel.PortfolioDate, path);
                    }
                }
            }
            catch
            {
            }
        }

        public string HeList()
        {
            string heStr = "";
            if (mHE1Checked == true)
            {
                heStr += 1 + ".";
            }
            if (mHE2Checked == true)
            {
                heStr += 2 + ".";
            }
            if (mHE3Checked == true)
            {
                heStr += 3 + ".";
            }
            if (mHE4Checked == true)
            {
                heStr += 4 + ".";
            }
            if (mHE5Checked == true)
            {
                heStr += 5 + ".";
            }
            if (mHE6Checked == true)
            {
                heStr += 6 + ".";
            }
            if (mHE7Checked == true)
            {
                heStr += 7 + ".";
            }
            if (mHE8Checked == true)
            {
                heStr += 8 + ".";
            }
            if (mHE9Checked == true)
            {
                heStr += 9 + ".";
            }
            if (mHE10Checked == true)
            {
                heStr += 10 + ".";
            }
            if (mHE11Checked == true)
            {
                heStr += 11 + ".";
            }
            if (mHE12Checked == true)
            {
                heStr += 12 + ".";
            }
            if (mHE13Checked == true)
            {
                heStr += 13 + ".";
            }
            if (mHE14Checked == true)
            {
                heStr += 14 + ".";
            }
            if (mHE15Checked == true)
            {
                heStr += 15 + ".";
            }
            if (mHE16Checked == true)
            {
                heStr += 16 + ".";
            }
            if (mHE17Checked == true)
            {
                heStr += 17 + ".";
            }
            if (mHE18Checked == true)
            {
                heStr += 18 + ".";
            }
            if (mHE19Checked == true)
            {
                heStr += 19 + ".";
            }
            if (mHE20Checked == true)
            {
                heStr += 20 + ".";
            }
            if (mHE21Checked == true)
            {
                heStr += 21 + ".";
            }
            if (mHE22Checked == true)
            {
                heStr += 22 + ".";
            }
            if (mHE23Checked == true)
            {
                heStr += 23 + ".";
            }
            if (mHE24Checked == true)
            {
                heStr += 24 + ".";
            }
            heStr.TrimEnd('.');
            if (heStr == null || heStr.Length == 0)
            {
                return null;
            }
            heStr = heStr.Substring(0, heStr.Length - 1);
            return heStr;
        }

        public void OffPeakCommand()
        {
            uncheckAll();
            if (mMarketComboSelectedValue.ToString() == "ERCOT")
            {
                int start = 1;
                int end = 25;
                for (int i = start; i < end; i++)
                {
                    setHour(i);
                    if (i == 6)
                    {
                        i = 22;
                    }
                }
            }
            else
            {
                int start = 1;
                int end = 25;
                for (int i = start; i < end; i++)
                {
                    setHour(i);
                    if (i == 7)
                    {
                        i = 23;
                    }
                }
            }

            FixAverages();
        }

        public void PeakCommand()
        {
            uncheckAll();
            if (mMarketComboSelectedValue.ToString() == "ERCOT")
            {
                int start = 7;
                int end = 23;
                for (int i = start; i < end; i++)
                {
                    setHour(i);
                }
            }
            else
            {
                int start = 8;
                int end = 24;
                for (int i = start; i < end; i++)
                {
                    setHour(i);
                }
            }
            FixAverages();
        }

        public void RampCommand()
        {
            uncheckAll();
            int start = 7;
            int end = 21;
            for (int i = start; i < end; i++)
            {
                setHour(i);
                if (i == 9)
                {
                    i = 17;
                }
            }
            FixAverages();
        }

        public void MidnightCommand()
        {
            uncheckAll();
            int start = 1;
            int end = 25;
            for (int i = start; i < end; i++)
            {
                setHour(i);
                if (i == 4)
                {
                    i = 22;
                }
            }
            FixAverages();
        }

        public void SetSourceSink()
        {
            DBAccess.GetSourceSinkNodeList(
                    (item1, error) =>
                    {
                        SourceNodeList = item1.Item1;
                        SinkNodeList = item1.Item2;
                    }, MarketComboSelectedValue, "UPTO");
        }

        #endregion

        #region Private Methods


        private void setHour(int hour)
        {
            if (hour == 1)
            {
                HE1Checked = true;
            }
            if (hour == 2)
            {
                HE2Checked = true;
            }
            if (hour == 3)
            {
                HE3Checked = true;
            }
            if (hour == 4)
            {
                HE4Checked = true;
            }
            if (hour == 5)
            {
                HE5Checked = true;
            }
            if (hour == 6)
            {
                HE6Checked = true;
            }
            if (hour == 7)
            {
                HE7Checked = true;
            }
            if (hour == 8)
            {
                HE8Checked = true;
            }
            if (hour == 9)
            {
                HE9Checked = true;
            }
            if (hour == 10)
            {
                HE10Checked = true;
            }
            if (hour == 11)
            {
                HE11Checked = true;
            }
            if (hour == 12)
            {
                HE12Checked = true;
            }
            if (hour == 13)
            {
                HE13Checked = true;
            }
            if (hour == 14)
            {
                HE14Checked = true;
            }
            if (hour == 15)
            {
                HE15Checked = true;
            }
            if (hour == 16)
            {
                HE16Checked = true;
            }
            if (hour == 17)
            {
                HE17Checked = true;
            }
            if (hour == 18)
            {
                HE18Checked = true;
            }
            if (hour == 19)
            {
                HE19Checked = true;
            }
            if (hour == 20)
            {
                HE20Checked = true;
            }
            if (hour == 21)
            {
                HE21Checked = true;
            }
            if (hour == 22)
            {
                HE22Checked = true;
            }
            if (hour == 23)
            {
                HE23Checked = true;
            }
            if (hour == 24)
            {
                HE24Checked = true;
            }
        }

        private void uncheckAll()
        {
            HE1Checked = false;
            HE2Checked = false;
            HE3Checked = false;
            HE4Checked = false;
            HE5Checked = false;
            HE6Checked = false;
            HE7Checked = false;
            HE8Checked = false;
            HE9Checked = false;
            HE10Checked = false;
            HE11Checked = false;
            HE12Checked = false;
            HE13Checked = false;
            HE14Checked = false;
            HE15Checked = false;
            HE16Checked = false;
            HE17Checked = false;
            HE18Checked = false;
            HE19Checked = false;
            HE20Checked = false;
            HE21Checked = false;
            HE22Checked = false;
            HE23Checked = false;
            HE24Checked = false;
            FixAverages();
        }

        private void FixAverages()
        {
            double[] averageItems = GetAverages();

            try
            {
                this.TotalPrice = averageItems[0];
            }
            catch
            {
            }

            try
            {
                this.TotalMW = averageItems[1];
            }
            catch
            {
            }
        }

        private double[] GetAverages()
        {
            List<double> averageList = new List<double>();
            int heCount = 0;
            for (int heNum = 1; heNum <= 24; heNum++)
            {
                if ((bool)this.GetType().GetProperty("HE" + heNum + "Checked").GetValue(this))
                {
                    heCount = heCount + 1;
                }
            }
            averageList.Add(heCount * this.Price * this.MW);
            averageList.Add(heCount * this.MW);
            return averageList.ToArray();
        }

        #endregion
    }
}
