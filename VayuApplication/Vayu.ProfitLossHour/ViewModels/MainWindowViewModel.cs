using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.NodePriceLibrary;
using Vayu.ProfitLossHour.Model;

namespace Vayu.ProfitLossHour.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Global Data Members
        /// <summary>
        /// The data service
        /// </summary>
        private readonly IHourlyDataService _dataService;
        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName;
        //private string mUser = "quant6";
        public bool mSortOrder = false;
        /// <summary>
        /// Gets or sets the calculate command.
        /// </summary>
        /// <value>
        /// The calculate command.
        /// </value>
        public DelegateCommand CalculateCommand { private set; get; }
        #endregion

        #region Prpperties
        /// <summary>
        /// The m hour PNL list
        /// </summary>
        private List<HourlyPNL> mHourPNLList;
        /// <summary>
        /// Gets or sets the hour PNL list.
        /// </summary>
        /// <value>
        /// The hour PNL list.
        /// </value>
        public List<HourlyPNL> HourPNLList
        {
            get
            {
                return mHourPNLList;
            }
            set
            {
                mHourPNLList = value;
                RaisePropertyChanged("HourPNLList");
            }
        }
        /// <summary>
        /// The m type list
        /// </summary>
        private List<string> mTypeList;
        /// <summary>
        /// Gets or sets the type list.
        /// </summary>
        /// <value>
        /// The type list.
        /// </value>
        public List<string> TypeList
        {
            get
            {
                return mTypeList;
            }
            set
            {
                mTypeList = value;
                RaisePropertyChanged("TypeList");
            }
        }
        /// <summary>
        /// The m PNL value
        /// </summary>
        private double mPNLValue;
        /// <summary>
        /// Gets or sets the PNL value.
        /// </summary>
        /// <value>
        /// The PNL value.
        /// </value>
        public double PNLValue
        {
            get
            {
                return mPNLValue;
            }
            set
            {
                mPNLValue = value;
                RaisePropertyChanged("PNLValue");
            }
        }
        /// <summary>
        /// The m mw value
        /// </summary>
        private double mMWValue;
        /// <summary>
        /// Gets or sets the mw value.
        /// </summary>
        /// <value>
        /// The mw value.
        /// </value>
        public double MWValue
        {
            get
            {
                return mMWValue;
            }
            set
            {
                mMWValue = value;
                RaisePropertyChanged("MWValue");
            }
        }
        /// <summary>
        /// The m selected market
        /// </summary>
        private string mSelectedMarket = null;
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
                HourPNLList = null;
                if (mSelectedMarket != null)
                {
                    SetAccountList();
                }
                RaisePropertyChanged("SelectedMarket");
            }
        }
        /// <summary>
        /// The m selected type
        /// </summary>
        private string mSelectedType;
        /// <summary>
        /// Gets or sets the type of the selected.
        /// </summary>
        /// <value>
        /// The type of the selected.
        /// </value>
        public string SelectedType
        {
            get
            {
                return mSelectedType;
            }
            set
            {
                mSelectedType = value;
                HourPNLList = null;
                if (mSelectedType != null)
                {
                    FillMarketList();
                }
                RaisePropertyChanged("SelectedType");
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
        /// The m selected portfolio
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
            get
            {
                return mSelectedPortfolio;
            }
            set
            {
                mSelectedPortfolio = value;
                HourPNLList = null;
                RaisePropertyChanged("SelectedPortfolio");
            }
        }
        /// <summary>
        /// The m selected account
        /// </summary>
        private Account mSelectedAccount;
        /// <summary>
        /// Gets or sets the selected account.
        /// </summary>
        /// <value>
        /// The selected account.
        /// </value>
        public Account SelectedAccount
        {
            get
            {
                return mSelectedAccount;
            }
            set
            {
                mSelectedAccount = value;
                HourPNLList = null;
                if (mSelectedAccount != null)
                {
                    SetPortfolioList();
                }
                RaisePropertyChanged("SelectedAccount");
            }
        }
        /// <summary>
        /// The m account list
        /// </summary>
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
        /// <summary>
        /// The m portfolio list
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
        /// The m start date
        /// </summary>
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
                Clear();
                RaisePropertyChanged("StartDate");
            }
        }
        /// <summary>
        /// The m user list
        /// </summary>
        private ObservableCollection<string> mUserList;
        /// <summary>
        /// Gets or sets the user list.
        /// </summary>
        /// <value>
        /// The user list.
        /// </value>
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
        /// The m realised checked
        /// </summary>
        private bool mRealizedChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [Realized checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Realized checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RealizedChecked
        {
            get { return mRealizedChecked; }
            set
            {
                mRealizedChecked = value;
                RaisePropertyChanged("RealizedChecked");

            }
        }


        /// <summary>
        /// The m realised checked
        /// </summary>
        private bool mUnRealizedChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [UnRealized checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [UnRealized checked]; otherwise, <c>false</c>.
        /// </value>
        public bool UnRealizedChecked
        {
            get { return mUnRealizedChecked; }
            set
            {
                mUnRealizedChecked = value;
                RaisePropertyChanged("UnRealizedChecked");

            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualHourlyPnlViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IHourlyDataService dataService)
        {
            RealizedChecked = true;
            UserList = DBAccess.GetUserList();
            ERCOTUserList = DBAccess.GetERCOTUserList();
            FillType();
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            StartDate = startDate;
            CalculateCommand = new DelegateCommand(Calculate);
        }
        /// <summary>
        /// Sets the account list.
        /// </summary>
        private void SetAccountList()
        {
            AccountList = null;
            if (SelectedType == null)
            {
                return;
            }
            if (SelectedMarket == null)
            {
                return;
            }
            string product = SelectedType == "Virtual" ? "Virtual" : "EES/PTP";
            List<Account> accountList = DBAccess.GetAccount(mUser, product, SelectedMarket);
            AccountList = accountList;
        }
        /// <summary>
        /// Sets the portfolio list.
        /// </summary>
        public void SetPortfolioList()
        {
            PortfolioList = null;
            if (SelectedAccount != null && SelectedAccount.PortfolioList != null && SelectedAccount.PortfolioList.Count > 0)
            {
                PortfolioList = SelectedAccount.PortfolioList;
            }
        }
        /// <summary>
        /// Clears this instance.
        /// </summary>
        private void Clear()
        {
            MWValue = 0;
            PNLValue = 0;
            HourPNLList = null;
            SelectedType = null;
            SelectedAccount = null;
            SelectedMarket = null;
            SelectedPortfolio = null;
            AccountList = null;
            MarketList = null;
            PortfolioList = null;
        }
        /// <summary>
        /// Calculates this instance.
        /// </summary>
        private void Calculate()
        {
            if (SelectedPortfolio == null)
            {
                return;
            }
            Dictionary<string, List<Bid>> sourceSinkHash = new Dictionary<string, List<Bid>>();

            if (RealizedChecked)
            {
                List<Bid> bidList = DBAccess.GetCleareds(SelectedPortfolio.IsUptos, GetMarketKey(SelectedPortfolio.Market), SelectedPortfolio.ID, StartDate, StartDate);

                foreach (Bid bid in bidList)
                {
                    string name = null;
                    if (bid.IsUptos)
                    {
                        PricingNode sourcePriceNode = DBAccess.GetNode(bid.Source, bid.Market);
                        PricingNode sinkPriceNode = DBAccess.GetNode(bid.Sink, bid.Market);
                        name = sourcePriceNode.NodeName + "->" + sinkPriceNode.NodeName;
                    }
                    else
                    {
                        PricingNode priceNode = DBAccess.GetNode(bid.Source, bid.Market);
                        name = priceNode.NodeName;
                    }
                    List<Bid> tempBidList = new List<Bid>();
                    if (sourceSinkHash.ContainsKey(name))
                    {
                        tempBidList = sourceSinkHash[name];
                        sourceSinkHash.Remove(name);
                    }
                    tempBidList.Add(bid);
                    sourceSinkHash.Add(name, tempBidList);
                }
                Tuple<List<Node>, List<Node>> nodeTuple = GetNodeLists(bidList, SelectedType == "UPTO");
                List<Vayu.NodePriceLibrary.Node> rtList = nodeTuple.Item2;
                List<Vayu.NodePriceLibrary.Node> daList = nodeTuple.Item1;
                DARTNode.GetDART(rtList, daList, StartDate, 1, true, false);
                List<string> sourceSinkKeyList = sourceSinkHash.Keys.ToList<string>();
                sourceSinkKeyList.Sort();
                List<HourlyPNL> hourlyPnlList = new List<HourlyPNL>();
                double totalMW = 0;
                double totalPnl = 0;
                HourlyPNL totalHourlyPnl = new HourlyPNL();
                totalHourlyPnl.NodeName = "TOTAL";
                for (int i = 0; i < 25; i++)
                {
                    totalHourlyPnl.HourlyList[i] = new HourData();
                }
                hourlyPnlList.Add(totalHourlyPnl);
                foreach (string sourceSinkName in sourceSinkKeyList)
                {
                    List<Bid> tempBidList = sourceSinkHash[sourceSinkName];
                    HourlyPNL hourlyPnl = new HourlyPNL();
                    hourlyPnl.NodeName = sourceSinkName;

                    foreach (Bid bid in tempBidList)
                    {
                        int hour = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                        HourData hourData = hourlyPnl.HourlyList[hour - 1];
                        double? pnlDouble = null;

                        if (hourData == null)
                            hourData = new HourData();

                        if (bid.IsUptos)
                        {
                            string sourceNodeKey = bid.MarketDateTime.ToString() + bid.Source.ToString();
                            string sinkNodeKey = bid.MarketDateTime.ToString() + bid.Sink.ToString();
                            if (DARTNode.sDAHash.ContainsKey(sourceNodeKey) && DARTNode.sRTHash.ContainsKey(sourceNodeKey) &&
                                DARTNode.sDAHash.ContainsKey(sinkNodeKey) && DARTNode.sRTHash.ContainsKey(sinkNodeKey) &&
                                !double.IsNaN(DARTNode.sDAHash[sourceNodeKey]) && !double.IsNaN(DARTNode.sDAHash[sinkNodeKey]) &&
                                !double.IsNaN(DARTNode.sRTHash[sourceNodeKey]) && !double.IsNaN(DARTNode.sRTHash[sinkNodeKey]))
                            {
                                pnlDouble = ((DARTNode.sRTHash[sinkNodeKey] - DARTNode.sRTHash[sourceNodeKey])
                                    - (DARTNode.sDAHash[sinkNodeKey] - DARTNode.sDAHash[sourceNodeKey])) * bid.MW;
                            }
                        }
                        else
                        {
                            string nodeKey = bid.MarketDateTime.ToString() + bid.Source.ToString();
                            if (DARTNode.sDAHash.ContainsKey(nodeKey) && DARTNode.sRTHash.ContainsKey(nodeKey)
                                && !double.IsNaN(DARTNode.sDAHash[nodeKey]) && !double.IsNaN(DARTNode.sRTHash[nodeKey]))
                            {
                                pnlDouble = (DARTNode.sRTHash[nodeKey] - DARTNode.sDAHash[nodeKey]) * bid.MW;
                            }
                        }

                        if (pnlDouble.HasValue)
                        {
                            totalPnl += pnlDouble.Value;
                            totalHourlyPnl.HourlyList[hour - 1].PNL = (totalHourlyPnl.HourlyList[hour - 1].PNL.GetValueOrDefault() + pnlDouble.Value);
                            hourData.PNL = (hourData.PNL.GetValueOrDefault() + pnlDouble.Value);
                        }

                        if (!double.IsNaN(bid.MW))
                        {
                            totalMW += bid.MW;
                            totalHourlyPnl.HourlyList[hour - 1].ClearedMW = (totalHourlyPnl.HourlyList[hour - 1].ClearedMW.GetValueOrDefault() + bid.MW);
                            hourData.ClearedMW = hourData.ClearedMW.GetValueOrDefault() + bid.MW;
                        }

                        hourlyPnl.HourlyList[hour - 1] = hourData;
                    }
                    hourlyPnl.UpdateTotal();
                    hourlyPnlList.Add(hourlyPnl);
                }
                totalHourlyPnl.UpdateTotal();
                MWValue = totalMW;
                PNLValue = totalPnl;
                HourPNLList = hourlyPnlList;
            }
            else if (UnRealizedChecked)
            {
                List<Bid> bidList = DBAccess.GetUnCleareds(SelectedPortfolio.IsUptos, GetMarketKey(SelectedPortfolio.Market), SelectedPortfolio.ID, StartDate, StartDate);
                foreach (Bid bid in bidList)
                {
                    string name = null;
                    if (bid.IsUptos)
                    {
                        PricingNode sourcePriceNode = DBAccess.GetNode(bid.Source, bid.Market);
                        PricingNode sinkPriceNode = DBAccess.GetNode(bid.Sink, bid.Market);
                        name = sourcePriceNode.NodeName + "->" + sinkPriceNode.NodeName;
                    }
                    else
                    {
                        PricingNode priceNode = DBAccess.GetNode(bid.Source, bid.Market);
                        name = priceNode.NodeName;
                    }
                    List<Bid> tempBidList = new List<Bid>();
                    if (sourceSinkHash.ContainsKey(name))
                    {
                        tempBidList = sourceSinkHash[name];
                        sourceSinkHash.Remove(name);
                    }
                    tempBidList.Add(bid);
                    sourceSinkHash.Add(name, tempBidList);
                }
                Tuple<List<Node>, List<Node>> nodeTuple = GetNodeLists(bidList, SelectedType == "UPTO");
                List<Vayu.NodePriceLibrary.Node> rtList = nodeTuple.Item2;
                List<Vayu.NodePriceLibrary.Node> daList = nodeTuple.Item1;
                DARTNode.GetDART(rtList, daList, StartDate, 1, true, false);
                List<string> sourceSinkKeyList = sourceSinkHash.Keys.ToList<string>();
                sourceSinkKeyList.Sort();
                List<HourlyPNL> hourlyPnlList = new List<HourlyPNL>();
                double totalMW = 0;
                double totalPnl = 0;
                HourlyPNL totalHourlyPnl = new HourlyPNL();
                totalHourlyPnl.NodeName = "TOTAL";
                for (int i = 0; i < 25; i++)
                {
                    totalHourlyPnl.HourlyList[i] = new HourData();
                }
                hourlyPnlList.Add(totalHourlyPnl);
                foreach (string sourceSinkName in sourceSinkKeyList)
                {
                    List<Bid> tempBidList = sourceSinkHash[sourceSinkName];
                    HourlyPNL hourlyPnl = new HourlyPNL();
                    hourlyPnl.NodeName = sourceSinkName;

                    foreach (Bid bid in tempBidList)
                    {
                        int hour = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                        HourData hourData = hourlyPnl.HourlyList[hour - 1];
                        double? pnlDouble = null;

                        if (hourData == null)
                            hourData = new HourData();

                        if (bid.IsUptos)
                        {
                            string sourceNodeKey = bid.MarketDateTime.ToString() + bid.Source.ToString();
                            string sinkNodeKey = bid.MarketDateTime.ToString() + bid.Sink.ToString();
                            if (DARTNode.sDAHash.ContainsKey(sourceNodeKey) && DARTNode.sRTHash.ContainsKey(sourceNodeKey) &&
                                DARTNode.sDAHash.ContainsKey(sinkNodeKey) && DARTNode.sRTHash.ContainsKey(sinkNodeKey) &&
                                !double.IsNaN(DARTNode.sDAHash[sourceNodeKey]) && !double.IsNaN(DARTNode.sDAHash[sinkNodeKey]) &&
                                !double.IsNaN(DARTNode.sRTHash[sourceNodeKey]) && !double.IsNaN(DARTNode.sRTHash[sinkNodeKey]))
                            {
                                pnlDouble = ((DARTNode.sRTHash[sinkNodeKey] - DARTNode.sRTHash[sourceNodeKey])
                                    - (DARTNode.sDAHash[sinkNodeKey] - DARTNode.sDAHash[sourceNodeKey])) * bid.MW;
                            }
                        }
                        else
                        {
                            string nodeKey = bid.MarketDateTime.ToString() + bid.Source.ToString();
                            if (DARTNode.sDAHash.ContainsKey(nodeKey) && DARTNode.sRTHash.ContainsKey(nodeKey)
                                && !double.IsNaN(DARTNode.sDAHash[nodeKey]) && !double.IsNaN(DARTNode.sRTHash[nodeKey]))
                            {
                                pnlDouble = (DARTNode.sRTHash[nodeKey] - DARTNode.sDAHash[nodeKey]) * bid.MW;
                            }
                        }

                        if (pnlDouble.HasValue)
                        {
                            totalPnl += pnlDouble.Value;
                            totalHourlyPnl.HourlyList[hour - 1].PNL = (totalHourlyPnl.HourlyList[hour - 1].PNL.GetValueOrDefault() + pnlDouble.Value);
                            hourData.PNL = (hourData.PNL.GetValueOrDefault() + pnlDouble.Value);
                        }

                        if (!double.IsNaN(bid.MW))
                        {
                            totalMW += bid.MW;
                            totalHourlyPnl.HourlyList[hour - 1].ClearedMW = (totalHourlyPnl.HourlyList[hour - 1].ClearedMW.GetValueOrDefault() + bid.MW);
                            hourData.ClearedMW = hourData.ClearedMW.GetValueOrDefault() + bid.MW;
                        }

                        hourlyPnl.HourlyList[hour - 1] = hourData;
                    }
                    hourlyPnl.UpdateTotal();
                    hourlyPnlList.Add(hourlyPnl);
                }
                totalHourlyPnl.UpdateTotal();
                MWValue = totalMW;
                PNLValue = totalPnl;
                HourPNLList = hourlyPnlList;

            }

        }
        internal void HandleSort(List<HourlyPNL> tempData, System.ComponentModel.ListSortDirection orderDirection)
        {
            try
            {
                HourlyPNL totalItem = tempData.Where(m => m.NodeName.ToLower().Contains("total")).FirstOrDefault();
                if (totalItem != null)
                {
                    tempData.Remove(totalItem);
                }
                var secondHalf = new List<HourlyPNL>();
                var firstHalf = new List<HourlyPNL>();
                foreach (HourlyPNL item in tempData)
                {
                    if (SortCondition == "Total")
                    {
                        if (item.Total.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour1")
                    {
                        if (item.Hour1.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour2")
                    {
                        if (item.Hour2.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour3")
                    {
                        if (item.Hour3.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour4")
                    {
                        if (item.Hour4.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour5")
                    {
                        if (item.Hour5.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour6")
                    {
                        if (item.Hour6.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour7")
                    {
                        if (item.Hour7.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour8")
                    {
                        if (item.Hour8.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour9")
                    {
                        if (item.Hour9.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour10")
                    {
                        if (item.Hour10.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour11")
                    {
                        if (item.Hour11.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour12")
                    {
                        if (item.Hour12.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour13")
                    {
                        if (item.Hour13.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour14")
                    {
                        if (item.Hour14.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour15")
                    {
                        if (item.Hour15.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour16")
                    {
                        if (item.Hour16.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour17")
                    {
                        if (item.Hour17.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour18")
                    {
                        if (item.Hour18.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour19")
                    {
                        if (item.Hour19.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour20")
                    {
                        if (item.Hour20.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour21")
                    {
                        if (item.Hour21.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour22")
                    {
                        if (item.Hour22.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour23")
                    {
                        if (item.Hour23.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }

                    if (SortCondition == "Hour24")
                    {
                        if (item.Hour24.PNL != null)
                            firstHalf.Add(item);
                        else
                            secondHalf.Add(item);
                    }
                }
                //var secondHalf = tempData.Where(m => m.GetType().GetProperty(SortCondition).GetValue(m) == null).ToList();
                //var firstHalf = tempData.Where(m => m.GetType().GetProperty(SortCondition).GetValue(m) != null).ToList();
                List<HourlyPNL> tempList = new List<HourlyPNL>();
                IEnumerable<HourData> mHourDataList = new List<HourData>();
                Type type = typeof(HourlyPNL);
                if (orderDirection == System.ComponentModel.ListSortDirection.Ascending)
                {

                    // firstHalf = firstHalf.OrderBy(m => m.GetType().GetProperty(SortCondition).GetValue(m.Hour1.PNL.Value)).ToList();
                    //mHourDataList = firstHalf.Select(m => m.GetType().GetProperty(SortCondition).GetValue(m)).ToList().Cast<HourData>();
                    //mHourDataList.OrderBy(m => m.PNL.Value);
                    if (SortCondition == "Total")
                        firstHalf = firstHalf.OrderBy(m => m.Total.PNL.Value).ToList();

                    if (SortCondition == "Hour1")
                        firstHalf = firstHalf.OrderBy(m => m.Hour1.PNL.Value).ToList();

                    if (SortCondition == "Hour2")
                        firstHalf = firstHalf.OrderBy(m => m.Hour2.PNL.Value).ToList();

                    if (SortCondition == "Hour3")
                        firstHalf = firstHalf.OrderBy(m => m.Hour3.PNL.Value).ToList();

                    if (SortCondition == "Hour4")
                        firstHalf = firstHalf.OrderBy(m => m.Hour4.PNL.Value).ToList();

                    if (SortCondition == "Hour5")
                        firstHalf = firstHalf.OrderBy(m => m.Hour5.PNL.Value).ToList();

                    if (SortCondition == "Hour6")
                        firstHalf = firstHalf.OrderBy(m => m.Hour6.PNL.Value).ToList();

                    if (SortCondition == "Hour7")
                        firstHalf = firstHalf.OrderBy(m => m.Hour7.PNL.Value).ToList();

                    if (SortCondition == "Hour8")
                        firstHalf = firstHalf.OrderBy(m => m.Hour8.PNL.Value).ToList();

                    if (SortCondition == "Hour9")
                        firstHalf = firstHalf.OrderBy(m => m.Hour9.PNL.Value).ToList();

                    if (SortCondition == "Hour10")
                        firstHalf = firstHalf.OrderBy(m => m.Hour10.PNL.Value).ToList();

                    if (SortCondition == "Hour11")
                        firstHalf = firstHalf.OrderBy(m => m.Hour11.PNL.Value).ToList();

                    if (SortCondition == "Hour12")
                        firstHalf = firstHalf.OrderBy(m => m.Hour12.PNL.Value).ToList();

                    if (SortCondition == "Hour13")
                        firstHalf = firstHalf.OrderBy(m => m.Hour13.PNL.Value).ToList();

                    if (SortCondition == "Hour14")
                        firstHalf = firstHalf.OrderBy(m => m.Hour14.PNL.Value).ToList();

                    if (SortCondition == "Hour15")
                        firstHalf = firstHalf.OrderBy(m => m.Hour15.PNL.Value).ToList();

                    if (SortCondition == "Hour16")
                        firstHalf = firstHalf.OrderBy(m => m.Hour16.PNL.Value).ToList();

                    if (SortCondition == "Hour17")
                        firstHalf = firstHalf.OrderBy(m => m.Hour17.PNL.Value).ToList();

                    if (SortCondition == "Hour18")
                        firstHalf = firstHalf.OrderBy(m => m.Hour18.PNL.Value).ToList();

                    if (SortCondition == "Hour19")
                        firstHalf = firstHalf.OrderBy(m => m.Hour19.PNL.Value).ToList();

                    if (SortCondition == "Hour20")
                        firstHalf = firstHalf.OrderBy(m => m.Hour20.PNL.Value).ToList();

                    if (SortCondition == "Hour21")
                        firstHalf = firstHalf.OrderBy(m => m.Hour21.PNL.Value).ToList();

                    if (SortCondition == "Hour22")
                        firstHalf = firstHalf.OrderBy(m => m.Hour22.PNL.Value).ToList();

                    if (SortCondition == "Hour23")
                        firstHalf = firstHalf.OrderBy(m => m.Hour23.PNL.Value).ToList();

                    if (SortCondition == "Hour24")
                        firstHalf = firstHalf.OrderBy(m => m.Hour24.PNL.Value).ToList();
                }
                else
                {
                    if (SortCondition == "Total")
                        firstHalf = firstHalf.OrderByDescending(m => m.Total.PNL.Value).ToList();

                    if (SortCondition == "Hour1")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour1.PNL.Value).ToList();

                    if (SortCondition == "Hour2")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour2.PNL.Value).ToList();

                    if (SortCondition == "Hour3")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour3.PNL.Value).ToList();

                    if (SortCondition == "Hour4")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour4.PNL.Value).ToList();

                    if (SortCondition == "Hour5")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour5.PNL.Value).ToList();

                    if (SortCondition == "Hour6")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour6.PNL.Value).ToList();

                    if (SortCondition == "Hour7")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour7.PNL.Value).ToList();

                    if (SortCondition == "Hour8")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour8.PNL.Value).ToList();

                    if (SortCondition == "Hour9")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour9.PNL.Value).ToList();

                    if (SortCondition == "Hour10")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour10.PNL.Value).ToList();

                    if (SortCondition == "Hour11")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour11.PNL.Value).ToList();

                    if (SortCondition == "Hour12")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour12.PNL.Value).ToList();

                    if (SortCondition == "Hour13")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour13.PNL.Value).ToList();

                    if (SortCondition == "Hour14")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour14.PNL.Value).ToList();

                    if (SortCondition == "Hour15")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour15.PNL.Value).ToList();

                    if (SortCondition == "Hour16")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour16.PNL.Value).ToList();

                    if (SortCondition == "Hour17")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour17.PNL.Value).ToList();

                    if (SortCondition == "Hour18")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour18.PNL.Value).ToList();

                    if (SortCondition == "Hour19")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour19.PNL.Value).ToList();

                    if (SortCondition == "Hour20")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour20.PNL.Value).ToList();

                    if (SortCondition == "Hour21")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour21.PNL.Value).ToList();

                    if (SortCondition == "Hour22")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour22.PNL.Value).ToList();

                    if (SortCondition == "Hour23")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour23.PNL.Value).ToList();

                    if (SortCondition == "Hour24")
                        firstHalf = firstHalf.OrderByDescending(m => m.Hour24.PNL.Value).ToList();
                }
                firstHalf.Insert(0, totalItem);
                tempList = firstHalf.Concat(secondHalf).ToList();
                HourPNLList = tempList.ToList();
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        private int GetMarketKey(string market)
        {
            if (market == "ERCOT")
            {
                return 9;
            }

            return 9;
        }
        /// <summary>
        /// Fills the type.
        /// </summary>
        private void FillType()
        {
            List<string> typeList = new List<string>();
            typeList.Add("UPTO");
            TypeList = typeList;
        }
        /// <summary>
        /// Gets the node lists.
        /// </summary>
        /// <param name="bidList">The bid list.</param>
        /// <param name="isUpTo">if set to <c>true</c> [is up to].</param>
        /// <returns></returns>
        private Tuple<List<Node>, List<Node>> GetNodeLists(List<Bid> bidList, bool isUpTo)
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
        /// <summary>
        /// Fills the market list.
        /// </summary>
        private void FillMarketList()
        {
            MarketList = null;
            List<string> marketList = new List<string>();
            if (SelectedType == "UPTO")
            {
                if (UserList.Contains(mUser))
                {
                    marketList.Add("ERCOT");
                    MarketList = marketList;
                }
                if (ERCOTUserList.Contains(mUser))
                {
                    marketList.Add("ERCOT");
                }
            }
            //marketList.Sort();
            MarketList = marketList;
        }
        #endregion

    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class IsGreaterConverter : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HourData)
            {
                HourData data = value as HourData;
                return data.PNL > 0;
            }
            else if (value is double)
                return ((value as double?).GetValueOrDefault() > 0);
            else
                return false;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class IsLesserConverter : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HourData)
            {
                HourData data = value as HourData;
                return data.PNL < 0;
            }
            else if (value is double)
                return ((value as double?).GetValueOrDefault() < 0);
            else
                return false;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Activator.CreateInstance(targetType);
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public class MoveFirstSortBehaviour
    {
        /// <summary>
        /// Gets the move first on sort.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string GetMoveFirstOnSort(DependencyObject obj)
        {
            return (string)obj.GetValue(MoveFirstOnSortProperty);
        }

        /// <summary>
        /// Sets the move first on sort.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetMoveFirstOnSort(DependencyObject obj, string value)
        {
            obj.SetValue(MoveFirstOnSortProperty, value);
        }

        /// <summary>
        /// The move first on sort property
        /// </summary>
        public static readonly DependencyProperty MoveFirstOnSortProperty =
            DependencyProperty.RegisterAttached("MoveFirstOnSort", typeof(string), typeof(MoveFirstSortBehaviour),
            new UIPropertyMetadata(string.Empty, OnMoveFirstOnSortChanged));

        /// <summary>
        /// Called when [move first on sort changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnMoveFirstOnSortChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            if (dataGrid == null)
                return;

            dataGrid.Sorting += (s, args) =>
            {
                dataGrid.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    if (!(dataGrid.ItemsSource is ListCollectionView))
                        return;

                    ListCollectionView list = dataGrid.ItemsSource as ListCollectionView;
                    if (!(list.SourceCollection is HourlyPNLList))
                        return;

                    HourlyPNLList pnlList = list.SourceCollection as HourlyPNLList;
                    pnlList.UpdateTotal();

                    //var view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);

                    //if (view == null) 
                    //    return;

                    //((ListCollectionView) dataGrid.ItemsSource).SourceCollection.GetType()

                    //view.MoveCurrentToLast();
                    ////view.MoveCurrentToFirst();
                }, null);
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.DataGrid" />
    public class DataGridExt : DataGrid
    {
        /// <summary>
        /// Occurs when [sorted].
        /// </summary>
        public event EventHandler<ValueEventArgs<DataGridColumn>> Sorted;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Controls.DataGrid.Sorting" /> event.
        /// </summary>
        /// <param name="eventArgs">The data for the event.</param>
        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            base.OnSorting(eventArgs);

            if (Sorted == null) return;
            var column = eventArgs.Column;
            Sorted(this, new ValueEventArgs<DataGridColumn>(column));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.EventArgs" />
    public class ValueEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueEventArgs{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ValueEventArgs(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; set; }

    }
}
