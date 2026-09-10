using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using Vayu.CRRPeriods.Model;

namespace Vayu.CRRPeriods.ViewModels
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            //_dataService = dataService;
            //_dataService.GetData(
            //    (item, error) =>
            //    {
            //        if (error != null)
            //        {
            //            return;
            //        }

            //        WelcomeTitle = item.Title;
            //    });
            mDataService = _dataService ?? new DataService();
            // MarketList = new List<string> { "PJM", "MISO", "ERCOT", "SPP", "CAISO" };
            MarketList = new List<string> { "ERCOT" };
            MarketSelectedItem = "ERCOT";
            GetPeriodData();
        }

        #region Properties

        /// <summary>
        /// The period list
        /// </summary>
        private List<FtrPeriod> periodList;
        /// <summary>
        /// Gets or sets the period list.
        /// </summary>
        /// <value>
        /// The period list.
        /// </value>
        public List<FtrPeriod> PeriodList
        {
            get { return periodList; }
            set
            {
                periodList = value;
                RaisePropertyChanged("PeriodList");
            }
        }

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
        //private string mVisibilityPeakWE;
        //public string VisibilityPeakWE
        //{
        //    get { return mVisibilityPeakWE; }
        //    set
        //    {
        //        mVisibilityPeakWE = value;
        //        RaisePropertyChanged("VisibilityPeakWE");
        //    }
        //}

        /// <summary>
        /// The market selected item
        /// </summary>
        private string marketSelectedItem;
        /// <summary>
        /// Gets or sets the market selected item.
        /// </summary>
        /// <value>
        /// The market selected item.
        /// </value>
        /// 
        //public string HideColumns
        //{
        //    get { return MarketSelectedItem; }
        //    set
        //    {
        //        if(MarketSelectedItem=="PJM")
        //        {

        //        }
        //    }
        //}

        public string MarketSelectedItem
        {
            get { return marketSelectedItem; }
            set
            {
                if (marketSelectedItem != value)
                {
                    marketSelectedItem = value;
                    GetPeriodData();
                    RaisePropertyChanged("MarketSelectedItem");
                }
            }
        }
        /// <summary>
        /// The m sort order
        /// </summary>
        public bool mSortOrder = false;
        /// <summary>
        /// The sort condition
        /// </summary>
        private string sortCondition;
        /// <summary>
        /// Gets or sets the sort condition.
        /// </summary>
        /// <value>
        /// The sort condition.
        /// </value>
        public string SortCondition
        {
            get
            {
                return sortCondition;
            }
            set
            {
                sortCondition = value;
                RaisePropertyChanged("SortCondition");
            }
        }

        #endregion

        /// <summary>
        /// Sorts FTR data.
        /// </summary>
        /// <param name="FtrtempData">The ftrtemp data.</param>
        /// <param name="orderDirection">The order direction.</param>
        internal void HandleSortedData(List<FtrPeriod> FtrtempData, System.ComponentModel.ListSortDirection orderDirection)
        {
            List<FtrPeriod> FtrSortedData = new List<FtrPeriod>();
            if (orderDirection == System.ComponentModel.ListSortDirection.Ascending)
            {
                FtrSortedData = FtrtempData.OrderBy(m => m.GetType().GetProperty(SortCondition).GetValue(m)).ToList();
            }
            else
            {
                FtrSortedData = FtrtempData.OrderByDescending(m => m.GetType().GetProperty(SortCondition).GetValue(m)).ToList();
            }
            PeriodList = FtrSortedData.ToList();
        }

        /// <summary>
        /// Gets the period data.
        /// </summary>
        private void GetPeriodData()
        {


            mDataService.GetPeriodData((a, e) =>
            {
                if (e == null)
                {
                    if (a != null && a.Count > 0)
                    {
                        PeriodList = a.Where(p => p.Market.ToUpper() == MarketSelectedItem.ToUpper()).ToList();
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
