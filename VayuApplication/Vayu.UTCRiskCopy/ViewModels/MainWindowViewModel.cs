using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using Vayu.DBLibrary;
using Vayu.UTCRiskCopy.Model;


namespace Vayu.UTCRiskCopy.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        public DelegateCommand TodaysDateCommand { private set; get; }
        public DelegateCommand CopyCommand { private set; get; }

        public DelegateCommand CopyToTestCommand { private set; get; }

        private string mUser = Environment.UserName;

        private List<string> mProductList;
        /// <summary>
        /// Gets or sets the product list.
        /// </summary>
        /// <value>
        /// The product list.
        /// </value>
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
        private string mSelectedProduct;
        /// <summary>
        /// Gets or sets the selected product.
        /// </summary>
        /// <value>
        /// The selected product.
        /// </value>
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
                //SetMarket();
                RaisePropertyChanged("SelectedProduct");

            }
        }
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
                SetAccountList();
                ResetValues(true);
                RaisePropertyChanged("StartDate");
            }
        }
        private List<Account> mAccountList;
        /// <summary>
        /// Gets or sets the account list.
        /// </summary>
        /// <value>
        /// The account list.
        /// </value>
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
        private string mSelectedMarket;
        /// <summary>
        /// Gets or sets the selected market.
        /// </summary>
        /// <value>
        /// The selected market.
        /// </value>
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
                RaisePropertyChanged("SelectedMarket");
                if (SelectedMarket == "PJM")
                    RiskEnabled = false;
                else
                    RiskEnabled = true;
            }
        }
        private List<Portfolio> mPortfolioList;
        /// <summary>
        /// Gets or sets the portfolio list.
        /// </summary>
        /// <value>
        /// The portfolio list.
        /// </value>
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
        private string mSelectAllVisible;
        /// <summary>
        /// Gets or sets the select all visible.
        /// </summary>
        /// <value>
        /// The select all visible.
        /// </value>
        public string SelectAllVisible
        {
            get
            {
                return mSelectAllVisible;
            }
            set
            {
                mSelectAllVisible = value;
                RaisePropertyChanged("SelectAllVisible");
            }
        }
        private string mUnSelectAllVisible;
        /// <summary>
        /// Gets or sets the un select all visible.
        /// </summary>
        /// <value>
        /// The un select all visible.
        /// </value>
        public string UnSelectAllVisible
        {
            get
            {
                return mUnSelectAllVisible;
            }
            set
            {
                mUnSelectAllVisible = value;
                RaisePropertyChanged("UnSelectAllVisible");
            }
        }
        private List<string> mMarketList;
        public List<Portfolio> SelectedPortfolioList;
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

        private List<string> mRiskAcoountList;
        public List<string> RiskAcoountList
        {
            get { return mRiskAcoountList; }
            set
            {
                mRiskAcoountList = value;
                RaisePropertyChanged("RiskAcoountList");
            }
        }
        private string mSelectedRiskAcoount;
        public string SelectedRiskAcoount
        {
            get
            {
                return mSelectedRiskAcoount;
            }
            set
            {
                mSelectedRiskAcoount = value;
                RaisePropertyChanged("SelectedRiskAcoount");
            }
        }
        private bool mRiskEnabled;
        public bool RiskEnabled
        {
            get
            {
                return mRiskEnabled;
            }
            set
            {
                mRiskEnabled = value;
                RaisePropertyChanged("RiskEnabled");
            }
        }
        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            //ProductList = new List<string> { "UPTO", "Virtual", "FTR" };
            ProductList = new List<string> { "UPTO" };
            SelectedProduct = "UPTO";
            MarketList = new List<string> { "ERCOT" };
            StartDate = DateTime.Now.Date.AddDays(1);
            CopyCommand = new DelegateCommand(() => Copy());
            CopyToTestCommand = new DelegateCommand(() => CopyToTest());
            RiskAcoountList = new List<string> { "RISK", "RISK1" };
            SelectedRiskAcoount = "RISK";
        }



        private void Copy()
        {
            int pKey = 0;
            if (SelectedMarket == "PJM")
            {
                pKey = 999;
            }
            else
            {
                if (SelectedRiskAcoount == "RISK")
                {
                    pKey = 3012;
                }
                else
                {
                    pKey = 3022;
                }
            }
            _dataService.InsertBids(SelectedPortfolioList, StartDate, mSelectedMarket, pKey);
            string portfolios = "";
            foreach (var item in SelectedPortfolioList)
            {
                portfolios = item.Name + "," + portfolios;
            }

            MessageBox.Show("Copied Bids for the portfolio : " + portfolios.Remove(portfolios.Length - 1), "Copied", MessageBoxButton.OK);
        }

        private void CopyToTest()
        {
            _dataService.InsertBidsToTest(SelectedPortfolioList, StartDate, mSelectedMarket);
            string portfolios = "";
            foreach (var item in SelectedPortfolioList)
            {
                portfolios = item.Name + "," + portfolios;
            }
            MessageBox.Show("Copyed Bids for the portfolio : " + portfolios.Remove(portfolios.Length - 1), "Copied", MessageBoxButton.OK);
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
            accountList.Remove(accountList.Find(x => x.Trader == "VayuCHECK"));
            accountList.Remove(accountList.Find(x => x.Trader == "VayuTEST"));
            AccountList = accountList;
        }
        private void ResetValues(bool isDateChange)
        {
            //if (!isDateChange)
            //{
            //    MarketList = null;
            //}
            AccountList = null;
            PortfolioList = null;
        }
        public void SetPortfolioList(List<Portfolio> portfolioList)
        {
            PortfolioList = null;
            if (portfolioList.Count > 0)
            {
                PortfolioList = portfolioList;
            }
        }
    }
}
