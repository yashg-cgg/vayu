using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using Vayu.Admin.Model;
using Vayu.DBLibrary;

namespace Vayu.Admin.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Declaration

        /// <summary>
        /// The data service object
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// User
        /// </summary>
        private string mUser = Environment.UserName;

        /// <summary>
        /// Gets or sets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public DelegateCommand SaveCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        public DelegateCommand DeleteCommand { private set; get; }
        /// <summary>
        /// Gets or sets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public DelegateCommand RefreshCommand { private set; get; }

        public DelegateCommand LoadCommand { private set; get; }

        /// <summary>
        /// The portfolio list
        /// </summary>
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

        /// <summary>
        /// The selected portfolio
        /// </summary>
        private Portfolio mSelectedPortfolio;
        /// <summary>
        /// Gets or sets the selected portfolio.
        /// </summary>
        /// <value>
        /// The selected portfolio.
        /// </value>
        public Portfolio SelectedPortfolio
        {
            get { return mSelectedPortfolio; }
            set
            {
                mSelectedPortfolio = value;
                RaisePropertyChanged("SelectedPortfolio");
            }
        }
        public string product;

        //New start
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
        /// The m selected market
        /// </summary>
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



            }
        }


        /// <summary>
        /// Sets the account list.
        /// </summary>
        private void SetAccountList()
        {
            List<string> ProductListddata = new List<string>();
            ProductListddata.Add("UPTO");
            ProductList = ProductListddata;
            //
            string product = (SelectedProduct == "Virtual") ? "Virtual" : (SelectedProduct == "UPTO") ? "EES/PTP" : "FTR";

            if (SelectedProduct == "UPTO")
            {
                GetPortfolios(mSelectedMarket, product);
            }
            GetAdminData();

        }
        /// <summary>
        /// The m product list
        /// </summary>
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

        private DateTime sStartDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate
        {
            get
            { return sStartDate; }
            set
            {
                sStartDate = value.Date;
                RaisePropertyChanged("StartDate");
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
                GetMarket();
                RaisePropertyChanged("SelectedProduct");
                string product = (SelectedProduct == "Virtual") ? "Virtual" : (SelectedProduct == "UPTO") ? "EES/PTP" : "FTR";
                if (SelectedProduct == "Virtual")
                {
                    GetPortfolios(mSelectedMarket, product);
                }
                if (SelectedProduct == "UPTO")
                {
                    GetPortfolios(mSelectedMarket, product);
                }
                if (SelectedProduct == "FTR")
                {
                    GetPortfolios(mSelectedMarket, product);
                }
                GetAdminData();
            }
        }
        //End start

        /// <summary>
        /// The admin data list
        /// </summary>
        private List<AdminData> mAdminDataList;
        /// <summary>
        /// Gets or sets the admin data list.
        /// </summary>
        /// <value>
        /// The admin data list.
        /// </value>
        public List<AdminData> AdminDataList
        {
            get { return mAdminDataList; }
            set
            {
                mAdminDataList = value;
                RaisePropertyChanged("AdminDataList");
            }
        }

        private List<SubmittedData> sSubmittedDataList;
        /// <summary>
        /// Gets or sets the admin data list.
        /// </summary>
        /// <value>
        /// The admin data list.
        /// </value>
        public List<SubmittedData> SubmittedDataList
        {
            get { return sSubmittedDataList; }
            set
            {
                sSubmittedDataList = value;
                RaisePropertyChanged("SubmittedDataList");
            }
        }

        /// <summary>
        /// The factor
        /// </summary>
        private decimal mFactor;
        /// <summary>
        /// Gets or sets the factor.
        /// </summary>
        /// <value>
        /// The factor.
        /// </value>
        public decimal Factor
        {
            get { return mFactor; }
            set
            {
                mFactor = value;
                RaisePropertyChanged("Factor");
            }
        }

        /// <summary>
        /// The portfolio ID
        /// </summary>
        private string mPortfolioID;
        /// <summary>
        /// Gets or sets the portfolio identifier.
        /// </summary>
        /// <value>
        /// The portfolio identifier.
        /// </value>
        public string PortfolioID
        {
            get { return mPortfolioID; }
            set { mPortfolioID = value; }
        }

        /// <summary>
        /// The portfolio name
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
            get { return mPortfolioName; }
            set { mPortfolioName = value; }
        }

        #endregion
        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            GetMarket();
            StartDate = DateTime.Now.Date.AddDays(1);

            SaveCommand = new DelegateCommand(() => Save());
            DeleteCommand = new DelegateCommand(() => Delete());
            RefreshCommand = new DelegateCommand(() => GetAdminData());
            LoadCommand = new DelegateCommand(() => RunLoadCommand());

        }
        #region Private Methods

        //get Markets
        public void GetMarket()
        {
            List<string> marketList = new List<string>();
            marketList.Add("ERCOT");
            MarketList = marketList;
        }
        /// <summary>
        /// Gets the portfolios.
        /// </summary>
        void GetPortfolios(string Market, string Products)
        {
            try
            {

                PortfolioList = _dataService.GetAllPortfolios(Market, Products);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Gets admin data.
        /// </summary>
        void GetAdminData()
        {
            try
            {
                if (mUser == "sangramp" || mUser == "neelams" || mUser == "darshand")
                {
                    AdminDataList = _dataService.GetAdminData(mSelectedMarket, GetUptos(mSelectedProduct));
                }
                else if (mUser == "gojira")
                {
                    AdminDataList = _dataService.GetAdminDataForgojira(mSelectedMarket, GetUptos(mSelectedProduct));
                }

                RaisePropertyChanged("AdminDataList");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// GetMarketkey
        /// </summary>
        public int GetMarketKey(string Market)
        {
            int key = 9;
            if (Market == "ERCOT")
            {
                key = 9;
            }
            return key;
        }

        /// <summary>
        /// GetUptos change
        /// </summary>
        public string GetUptos(string Product)
        {
            string product = (SelectedProduct == "Virtual") ? "Virtual" : (SelectedProduct == "UPTO") ? "EES/PTP" : "FTR";
            return product;
        }

        /// <summary>
        /// Saves admin data in DB.
        /// </summary>
        void Save()
        {
            try
            {
                if (SelectedPortfolio != null)
                {
                    System.Windows.Forms.DialogResult objDialogResult;

                    if (Factor >= 24)
                    {
                        objDialogResult = System.Windows.Forms.MessageBox.Show("Factor value is " + Factor + ". Do you want to continue saving it?", "Warning!", System.Windows.Forms.MessageBoxButtons.YesNo);
                    }
                    else
                    {
                        objDialogResult = System.Windows.Forms.DialogResult.Yes;
                    }

                    if (objDialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        AdminData objAdminData = new AdminData();

                        objAdminData.PortfolioID = SelectedPortfolio.ID.ToString();
                        objAdminData.Factor = Factor;
                        objAdminData.Market = GetMarketKey(mSelectedMarket);
                        objAdminData.Product = GetUptos(mSelectedProduct);
                        string Account = _dataService.GetAccountForPortfolio(objAdminData.PortfolioID);

                        if (Account == "A1")
                        {
                            objAdminData.Account2 = 333;
                        }
                        else
                        {
                            objAdminData.Account2 = 777;
                        }

                        List<AdminData> AdminDataList2 = new List<AdminData>();

                        AdminDataList2 = _dataService.PortfolioExistOrNot(SelectedPortfolio.ID);

                        if (AdminDataList2.Count > 0)
                        {
                            AdminData objAdminDataOld = new AdminData();

                            foreach (var item in AdminDataList2)
                            {
                                objAdminDataOld.PortfolioID = item.PortfolioID;
                                objAdminDataOld.Factor = item.Factor;
                                objAdminDataOld.Account2 = item.Account2;
                                objAdminDataOld.Market = GetMarketKey(mSelectedMarket);
                                objAdminDataOld.Product = GetUptos(mSelectedProduct);
                            }

                            _dataService.DeleteFactor(objAdminDataOld);
                        }

                        if (_dataService.AddFactor(objAdminData))
                        {
                            GetAdminData();
                            MessageBox.Show("Portfolio and Factor saved Successfully.", "Success!");
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Some problem occured while saving data", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select Portfolio", "Warning!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Deletes admin data from DB.
        /// </summary>
        void Delete()
        {
            try
            {
                if (PortfolioID != null)
                {
                    AdminData objAdminData = new AdminData();

                    objAdminData.PortfolioID = PortfolioID;
                    objAdminData.Factor = Factor;
                    objAdminData.Market = GetMarketKey(mSelectedMarket);
                    objAdminData.Product = GetUptos(mSelectedProduct);

                    System.Windows.Forms.DialogResult objDialogResult = System.Windows.Forms.MessageBox.Show("Do you want to delete this Portfolio and Factor?", "Delete Confirmation", System.Windows.Forms.MessageBoxButtons.YesNo);

                    if (objDialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (_dataService.DeleteFactor(objAdminData))
                        {
                            GetAdminData();
                            MessageBox.Show("Portfolio and Factor deleted successfully", "Success!");
                        }
                        ClearFields();
                    }
                }
                else
                {
                    MessageBox.Show("Please select portfolio and factor", "Warning!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Clears the fields.
        /// </summary>
        void ClearFields()
        {
            SelectedPortfolio = null;
            Factor = 0;
        }

        #endregion

        /// <summary>
        /// Assigns the data.
        /// </summary>
        /// <param name="objAdminData">The object admin data.</param>
        public void AssignData(AdminData objAdminData)
        {
            try
            {
                if (objAdminData != null)
                {
                    PortfolioID = objAdminData.PortfolioID;
                    Factor = objAdminData.Factor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RunLoadCommand()
        {
            if (mUser == "gojira" || mUser == "sangramp" || mUser == "darshand" || mUser == "neelams")
            {
                List<SubmittedData> SubmittedPFList = _dataService.GetSubmittedPortfolios(StartDate);
                SubmittedDataList = SubmittedPFList;
                if(SubmittedPFList[0].Status!="Success")
                {
                    MessageBox.Show(SubmittedPFList[0].Status);
                }
                
            }

        }
    }
}
