using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Vayu.CRRTopTenParticipants.Model;

namespace Vayu.CRRTopTenParticipants.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService mDataService;
        private string mUser = Environment.UserName;
        //private string mUser = "mbp";
        #region Properties
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
        /// The m year period list
        /// </summary>
        private List<string> mYearPeriodList;
        /// <summary>
        /// Gets or sets the year period list.
        /// </summary>
        /// <value>
        /// The year period list.
        /// </value>
        public List<string> YearPeriodList
        {
            get
            {
                return mYearPeriodList;
            }
            set
            {
                mYearPeriodList = value;
                RaisePropertyChanged("YearPeriodList");
            }
        }
        /// <summary>
        /// The m report date
        /// </summary>
        private string mReportDate;
        /// <summary>
        /// Gets or sets the report date.
        /// </summary>
        /// <value>
        /// The report date.
        /// </value>
        public string ReportDate
        {
            get
            {
                return mReportDate;
            }
            set
            {
                mReportDate = value;
                RaisePropertyChanged("ReportDate");
            }
        }
        /// <summary>
        /// The m to selected date
        /// </summary>
        private DateTime mToSelectedDate;
        /// <summary>
        /// Gets or sets to selected date.
        /// </summary>
        /// <value>
        /// To selected date.
        /// </value>
        public DateTime ToSelectedDate
        {
            get
            {
                return mToSelectedDate;
            }
            set
            {
                mToSelectedDate = value;
                RaisePropertyChanged("ToSelectedDate");
            }
        }
        /// <summary>
        /// The m from selected date
        /// </summary>
        private DateTime mFromSelectedDate;
        /// <summary>
        /// Gets or sets from selected date.
        /// </summary>
        /// <value>
        /// From selected date.
        /// </value>
        public DateTime FromSelectedDate
        {
            get
            {
                return mFromSelectedDate;
            }
            set
            {
                mFromSelectedDate = value;
                RaisePropertyChanged("FromSelectedDate");
            }
        }
        /// <summary>
        /// The m PNL information list
        /// </summary>
        private List<DataItem> mPnlInfoList;
        /// <summary>
        /// Gets or sets the PNL information list.
        /// </summary>
        /// <value>
        /// The PNL information list.
        /// </value>
        public List<DataItem> PnlInfoList
        {
            get
            {
                return mPnlInfoList;
            }
            set
            {
                mPnlInfoList = value;
                RaisePropertyChanged("PnlInfoList");
            }
        }
        /// <summary>
        /// The m market selected item
        /// </summary>
        private string mMarketSelectedItem;
        /// <summary>
        /// Gets or sets the market selected item.
        /// </summary>
        /// <value>
        /// The market selected item.
        /// </value>
        public string MarketSelectedItem
        {
            get
            {
                return mMarketSelectedItem;
            }
            set
            {
                mMarketSelectedItem = value;
                RaisePropertyChanged("MarketSelectedItem");
            }
        }
        /// <summary>
        /// The m items count
        /// </summary>
        private int mItemsCount;
        /// <summary>
        /// Gets or sets the items count.
        /// </summary>
        /// <value>
        /// The items count.
        /// </value>
        public int ItemsCount
        {
            get
            {
                return mItemsCount;
            }
            set
            {
                mItemsCount = value;
                RaisePropertyChanged("ItemsCount");
            }
        }
        /// <summary>
        /// The m is visible
        /// </summary>
        private bool mIsVisible;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                return mIsVisible;
            }
            set
            {
                mIsVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }
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

        /// <summary>
        /// Gets or sets the helper data item list.
        /// </summary>
        /// <value>
        /// The helper data item list.
        /// </value>
        public List<DataItem> HelperDataItemList { get; set; }

        /// <summary>
        /// Gets the select month command.
        /// </summary>
        /// <value>
        /// The select month command.
        /// </value>
        public DelegateCommand SelectMonthCommand { get; private set; }
        /// <summary>
        /// Gets the select year command.
        /// </summary>
        /// <value>
        /// The select year command.
        /// </value>
        public DelegateCommand SelectYearCommand { get; private set; }
        /// <summary>
        /// Gets the select all command.
        /// </summary>
        /// <value>
        /// The select all command.
        /// </value>
        public DelegateCommand SelectAllCommand { get; private set; }
        /// <summary>
        /// Gets the generate report command.
        /// </summary>
        /// <value>
        /// The generate report command.
        /// </value>
        public DelegateCommand GenerateReportCommand { get; private set; }
        /// <summary>
        /// Gets the remove month command.
        /// </summary>
        /// <value>
        /// The remove month command.
        /// </value>
        public DelegateCommand RemoveMonthCommand { get; private set; }
        /// <summary>
        /// Gets the remove all months command.
        /// </summary>
        /// <value>
        /// The remove all months command.
        /// </value>
        public DelegateCommand RemoveAllMonthsCommand { get; private set; }
        #endregion
        /// <summary>
        /// The m sort order
        /// </summary>
        public bool mSortOrder = false;

        public MainWindowViewModel(IDataService dataService = null)
        {
            if (dataService == null)
            {
                dataService = new Model.DataService();
            }
            mDataService = dataService;
            if (ItemsCount == 0)
            {
                ItemsCount = 10;
            }
            SelectMonthCommand = new DelegateCommand(() => FillPeriodList("Month"));
            SelectYearCommand = new DelegateCommand(() => FillPeriodList("year"));
            //SelectAllCommand = new DelegateCommand(() => FillPeriodList("all"));
            GenerateReportCommand = new DelegateCommand(() => PrepareReport());
            if (MarketList == null)
            {
                // MarketList = new List<string>() { "PJM", "MISO", "CAISO", "SPP", "ERCOT", "NYISO" };
                //MarketList = new List<string>() { "PJM", "MISO", "SPP" };
                MarketList = new List<string>() { "ERCOT" };// "PJM",

            }
            //if (YearPeriodList == null)
            //{
            //    YearPeriodList = PrepareYearList().OrderBy(a => a).ToList();
            //}
            MarketSelectedItem = MarketList.Find(a => a.ToLower().Contains("ercot"));
            FromSelectedDate = DateTime.Today;
            ToSelectedDate = DateTime.Today;
        }
        /// <summary>
        /// Prepares the top participants PNL report.
        /// </summary>
        private void PrepareReport()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            List<DataItem> tempPnlInfoList = new List<DataItem>();
            try
            {
                PnlInfoList = new List<DataItem>();

                ReportDate = "";
                if (FromSelectedDate > ToSelectedDate)
                {
                    MessageBox.Show("to date should be less than from date");
                    Mouse.OverrideCursor = null;
                    return;
                }
                mDataService.GetTop10ParticipantData((item, error) =>
                {
                    if (item.Count > 0)
                    {
                        //PnlInfoList = null; 
                        HelperDataItemList = item.OrderByDescending(a => a.PNL).ToList();
                        HelperDataItemList.ForEach(a => a.Rank = HelperDataItemList.FindIndex(k => k == a) + 1);
                        tempPnlInfoList = HelperDataItemList.Take(ItemsCount).ToList();

                    }
                    if (tempPnlInfoList.Count > 0)
                    {
                        ReportDate = tempPnlInfoList.Max(m => m.ReportDate).ToShortDateString();
                    }
                }, GetMarketKey(), FromSelectedDate, ToSelectedDate);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //tempPnlInfoList.Add(UpdateTotal(tempPnlInfoList)); 
            List<DataItem> itemListTotal = new List<DataItem>();
            itemListTotal.Add(UpdateTotal(tempPnlInfoList));
            foreach (DataItem item in tempPnlInfoList)
            {
                itemListTotal.Add(item);
            }
            if (!((mUser == "Programmer1") || (mUser == "gojira") || (mUser == "sangramp") || (mUser == "neelams") || (mUser == "dilsha") || (mUser == "darshand")))
            {
                itemListTotal.RemoveAll(x => x.Participant.ToUpper() == "VAYU_CRR_STRAT");
                itemListTotal.RemoveAll(x => x.Participant.ToUpper() == "VAYU");
                itemListTotal.RemoveAll(x => x.Participant.ToUpper() == "RISK_VAYU_CRR");
            }
            if ((mUser == "Programmer1") || (mUser == "sangramp"))
            {
                PnlInfoList = itemListTotal;
            }
            else
            {
                itemListTotal.RemoveAll(x => x.Participant.ToUpper() == "ISO1");
                itemListTotal.RemoveAll(x => x.Participant.ToUpper() == "XISO2");
                PnlInfoList = itemListTotal;
            }
            //foreach (DataItem data in tempPnlInfoList) 
            //{ 
            //    PnlInfoList.Add(data); 
            //} 
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Updates the total for all periods.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <returns></returns>
        private DataItem UpdateTotal(List<DataItem> itemList)
        {
            DataItem total = new DataItem();
            total.Rank = 0;
            total.Participant = "TOTAL";
            total.Company = "TOTAL";
            // total.ReportDate=DateTime.Now.ToString("y")
            total.Cost = itemList.Sum(x => x.Cost);
            total.DAPrice = itemList.Sum(x => x.DAPrice);
            total.PNL = itemList.Sum(x => x.PNL);
            total.Monthly = itemList.Sum(x => x.Monthly);
            total.Annual = itemList.Sum(x => x.Annual);
            total.Q1 = itemList.Sum(x => x.Q1);
            total.Q2 = itemList.Sum(x => x.Q2);
            total.Q3 = itemList.Sum(x => x.Q3);
            total.Q4 = itemList.Sum(x => x.Q4);
            total.YR1 = itemList.Sum(x => x.YR1);
            total.YR2 = itemList.Sum(x => x.YR2);
            total.YR3 = itemList.Sum(x => x.YR3);
            total.YRALL = itemList.Sum(x => x.YRALL);
            total.MW = itemList.Sum(x => x.MW);
            //total.CostMonthly = itemList.Sum(x => x.CostMonthly);
            //total.CostAnnual = itemList.Sum(x => x.CostAnnual);
            //total.CostQ1 = itemList.Sum(x => x.CostQ1);
            //total.CostQ2 = itemList.Sum(x => x.CostQ2);
            //total.CostQ3 = itemList.Sum(x => x.CostQ3);
            //total.CostQ4 = itemList.Sum(x => x.CostQ4);
            //total.CostYR1 = itemList.Sum(x => x.CostYR1);
            //total.CostYR2 = itemList.Sum(x => x.CostYR2);
            //total.CostYR3 = itemList.Sum(x => x.CostYR3);
            //total.CostYRALL = itemList.Sum(x => x.CostYRALL);
            return total;
            //itemList.Add(total);
        }
        /// <summary>
        /// Sets the market key for markets .
        /// </summary>
        /// <returns></returns>
        private int GetMarketKey()
        {
            switch (MarketSelectedItem.ToLower())
            {
                case "pjm": return 1;
                case "miso": return 2;
                case "caiso": return 7;
                case "ercot": return 9;
                case "spp": return 12;
                default: return 0;
            }
        }
        /// <summary>
        /// Prepares the Auunal Participants list.
        /// </summary>
        /// <returns></returns>
        private List<string> PrepareYearList()
        {
            List<DateTime> monthlist = new List<DateTime>();
            List<string> yearList = new List<string>();
            try
            {
                if (mDataService != null)
                {
                    mDataService.GetPeriodData((ditem, error) =>
                    {
                        if (error != null)
                        {
                            return;
                        }
                        else
                        {
                            monthlist = ditem;
                        }
                    });
                }
                if (monthlist.Count > 0)
                {
                    DateTime maxyear = monthlist.Max();
                    DateTime minyear = monthlist.Min();

                    for (; minyear.Year <= maxyear.Year; minyear = minyear.AddYears(1))
                    {
                        yearList.Add("FTR " + (minyear.Year).ToString() + " " + (minyear.Year + 1).ToString());
                        yearList.Add("CRR " + (minyear.Year).ToString() + " " + (minyear.Year + 1).ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return yearList;
        }
        /// <summary>
        /// Fills the period list for selected year.
        /// </summary>
        /// <param name="type">The type.</param>
        private void FillPeriodList(string type)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "month":
                        FromSelectedDate = DateTime.Parse(DateTime.Today.Month + "/1/" + DateTime.Today.Year);
                        ToSelectedDate = DateTime.Today;
                        break;
                    case "year":
                        FromSelectedDate = DateTime.Parse("1/1/" + DateTime.Today.Year);
                        ToSelectedDate = DateTime.Today;
                        break;
                    default:
                        break;
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// Handles the sorting of participants in grid.
        /// </summary>
        /// <param name="tempData">The temporary data.</param>
        /// <param name="orderDirection">The order direction.</param>
        internal void HandleSort(List<DataItem> tempData, System.ComponentModel.ListSortDirection orderDirection)
        {
            DataItem totalItem = tempData.Where(m => m.Participant.ToLower().Contains("total")).FirstOrDefault();
            if (totalItem != null)
            {
                tempData.Remove(totalItem);
            }
            var secondHalf = tempData.Where(m => m.GetType().GetProperty(SortCondition).GetValue(m) == null).ToList();
            var firstHalf = tempData.Where(m => m.GetType().GetProperty(SortCondition).GetValue(m) != null).ToList();
            List<DataItem> tempList = new List<DataItem>();
            if (orderDirection == System.ComponentModel.ListSortDirection.Ascending)
            {
                firstHalf = firstHalf.OrderBy(m => m.GetType().GetProperty(SortCondition).GetValue(m)).ToList();
            }
            else
            {
                firstHalf = firstHalf.OrderByDescending(m => m.GetType().GetProperty(SortCondition).GetValue(m)).ToList();
            }
            firstHalf.Insert(0, totalItem);
            tempList = firstHalf.Concat(secondHalf).ToList();
            if (!(mUser == "Programmer1") || (mUser == "sangramp") || (mUser == "gojira") || (mUser == "neelams") || (mUser == "darshand") || (mUser == "dilsha")) //((mUser == "Programmer1") || (mUser == "sangramp"))
            {
                tempList.RemoveAll(x => x.Participant.ToUpper() == "VAYU_CRR_STRAT");
                tempList.RemoveAll(x => x.Participant.ToUpper() == "VAYU");
                tempList.RemoveAll(x => x.Participant.ToUpper() == "RISK_VAYU_CRR");
            }
            PnlInfoList = tempList.ToList();
        }
    }
}
