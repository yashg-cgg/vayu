using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using Vayu.CRRAuction.Model;

namespace Vayu.CRRAuction.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// The data service
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// The m data service
        /// </summary>
        private IDataService mDataService;

        //public const string WelcomeTitlePropertyName = "WelcomeTitle";

        //private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = _dataService ?? new DataService();

            MarketList = new List<string> { "ERCOT" };
            MarketSelectedItem = "ERCOT";
            GetAuctionData();
        }

        #region Properties

        /// <summary>
        /// The market list
        /// </summary>
        private List<string> marketList;
        /// <summary>
        /// Gets or sets the market list.
        /// </summary>
        /// <value>
        /// The market list.
        /// </value>
        public List<string> MarketList
        {
            get { return marketList; }
            set
            {
                marketList = value;
                RaisePropertyChanged("MarketList");
            }
        }
        /// <summary>
        /// The market selected item
        /// </summary>
        private string marketSelectedItem;
        //private string marketSelectedItem = CommonAcc;

        /// <summary>
        /// Gets or sets the market selected item.
        /// </summary>
        /// <value>
        /// The market selected item.
        /// </value>
        public string MarketSelectedItem
        {
            get { return marketSelectedItem; }
            set
            {
                if (marketSelectedItem != value)
                {
                    marketSelectedItem = value;
                    AuctionList = new List<FtrAuctionType>();
                    System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    GetAuctionData();
                    System.Windows.Input.Mouse.OverrideCursor = null;
                    RaisePropertyChanged("MarketSelectedItem");
                }
            }
        }
        /// <summary>
        /// The auction list
        /// </summary>
        private List<FtrAuctionType> auctionList;
        /// <summary>
        /// Gets or sets the auction list.
        /// </summary>
        /// <value>
        /// The auction list.
        /// </value>
        public List<FtrAuctionType> AuctionList
        {
            get { return auctionList; }
            set
            {
                auctionList = value;
                RaisePropertyChanged("AuctionList");
            }
        }

        #endregion

        /// <summary>
        /// Gets the auction data.
        /// </summary>
        private void GetAuctionData()
        {
            mDataService.GetAuctionData((a, e) =>
            {
                if (e == null)
                {
                    if (a != null && a.Count > 0)
                    {
                        AuctionList = a.Where(p => p.Market.ToUpper() == MarketSelectedItem.ToUpper()).ToList();
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(e.Message);
                }
            }, MarketSelectedItem ?? "ERCOT");
        }

    }
}
