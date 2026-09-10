using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vayu.DBLibrary;
using Vayu.LMPriceWindow;
using Vayu.NodePriceDaily.Model;

namespace Vayu.NodePriceDaily.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        #region Declaration

        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m node hash
        /// </summary>
        private Dictionary<int, Dictionary<string, Vayu.DBLibrary.NodeDetail>> mNodeHash = new Dictionary<int, Dictionary<string, Vayu.DBLibrary.NodeDetail>>();
        /// <summary>
        /// The m node type hash
        /// </summary>
        private Dictionary<int, List<string>> mNodeTypeHash = new Dictionary<int, List<string>>();
        /// <summary>
        /// The m zone hash
        /// </summary>
        private Dictionary<int, List<string>> mZoneHash = new Dictionary<int, List<string>>();
        /// <summary>
        /// The m daily LMP hash
        /// </summary>
        private Dictionary<DateTime, Dictionary<int, DailyLMP>> mDailyLMPHash = new Dictionary<DateTime, Dictionary<int, DailyLMP>>();
        /// <summary>
        /// The m daily dalmp hash
        /// </summary>
        private Dictionary<DateTime, Dictionary<int, DailyLMP>> mDailyDALMPHash = new Dictionary<DateTime, Dictionary<int, DailyLMP>>();
        /// <summary>
        /// The m is first time
        /// </summary>
        private bool mIsFirstTime = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the run refresh command.
        /// </summary>
        /// <value>
        /// The run refresh command.
        /// </value>
        public DelegateCommand RunRefreshCommand { private set; get; }
        /// <summary>
        /// Gets or sets the show LMP command.
        /// </summary>
        /// <value>
        /// The show LMP command.
        /// </value>
        public DelegateCommand ShowLmpCommand { private set; get; }

        /// <summary>
        /// The m refresh enabled
        /// </summary>
        private bool mRefreshEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether [refresh enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [refresh enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool RefreshEnabled
        {
            get
            {
                return mRefreshEnabled;
            }
            set
            {
                mRefreshEnabled = value;
                RaisePropertyChanged("RefreshEnabled");
            }
        }
        /// <summary>
        /// The m aggregate average checked
        /// </summary>
        private bool mAggAvgChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [aggregate average checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [aggregate average checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AggAvgChecked
        {
            get
            {
                return mAggAvgChecked;
            }
            set
            {
                mAggAvgChecked = value;
                RefreshCommand();
                RaisePropertyChanged("AggAvgChecked");
            }
        }
        /// <summary>
        /// The m aggregate maximum checked
        /// </summary>
        private bool mAggMaxChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [aggregate maximum checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [aggregate maximum checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AggMaxChecked
        {
            get
            {
                return mAggMaxChecked;
            }
            set
            {
                mAggMaxChecked = value;
                RefreshCommand();
                RaisePropertyChanged("AggMaxChecked");
            }
        }
        /// <summary>
        /// The m aggregate minimum checked
        /// </summary>
        private bool mAggMinChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [aggregate minimum checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [aggregate minimum checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AggMinChecked
        {
            get
            {
                return mAggMinChecked;
            }
            set
            {
                mAggMinChecked = value;
                RefreshCommand();
                RaisePropertyChanged("AggMinChecked");
            }
        }
        /// <summary>
        /// The m da checked
        /// </summary>
        private bool mDAChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [da checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [da checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DAChecked
        {
            get
            {
                return mDAChecked;
            }
            set
            {
                mDAChecked = value;
                RefreshCommand();
                RaisePropertyChanged("DAChecked");
            }
        }
        /// <summary>
        /// The m rt checked
        /// </summary>
        private bool mRTChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [rt checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [rt checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RTChecked
        {
            get
            {
                return mRTChecked;
            }
            set
            {
                mRTChecked = value;
                RefreshCommand();
                RaisePropertyChanged("RTChecked");
            }
        }
        /// <summary>
        /// The m dart checked
        /// </summary>
        private bool mDARTChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [dart checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dart checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DARTChecked
        {
            get
            {
                return mDARTChecked;
            }
            set
            {
                mDARTChecked = value;
                RefreshCommand();
                RaisePropertyChanged("DARTChecked");
            }
        }
        /// <summary>
        /// The m average checked
        /// </summary>
        private bool mAverageChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [average checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [average checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AverageChecked
        {
            get
            {
                return mAverageChecked;
            }
            set
            {
                mAverageChecked = value;
                RefreshCommand();
                RaisePropertyChanged("AverageChecked");
            }
        }
        /// <summary>
        /// The m peak checked
        /// </summary>
        private bool mPeakChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [peak checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [peak checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PeakChecked
        {
            get
            {
                return mPeakChecked;
            }
            set
            {
                mPeakChecked = value;
                RefreshCommand();
                RaisePropertyChanged("PeakChecked");
            }
        }
        /// <summary>
        /// The m off peak checked
        /// </summary>
        private bool mOffPeakChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [off peak checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [off peak checked]; otherwise, <c>false</c>.
        /// </value>
        public bool OffPeakChecked
        {
            get
            {
                return mOffPeakChecked;
            }
            set
            {
                mOffPeakChecked = value;
                RefreshCommand();
                RaisePropertyChanged("OffPeakChecked");
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
                mStartDate = value;
                Clear();
                RaisePropertyChanged("StartDate");
            }
        }
        /// <summary>
        /// The m end date
        /// </summary>
        private DateTime mEndDate;
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate
        {
            get
            {
                return mEndDate;
            }
            set
            {
                Clear();
                if (value != new DateTime() && value > DateTime.Today)
                {
                    mEndDate = DateTime.Today;
                }
                else
                {
                    mEndDate = value;
                }
                RaisePropertyChanged("EndDate");
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
                SetTypeList();
                Clear();
                mDailyLMPHash = new Dictionary<DateTime, Dictionary<int, DailyLMP>>();
                mDailyDALMPHash = new Dictionary<DateTime, Dictionary<int, DailyLMP>>();
                RaisePropertyChanged("SelectedMarket");
                RefreshCommand();
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
                if (!mIsFirstTime)
                {
                    RefreshThreaded();
                }
                RaisePropertyChanged("SelectedType");
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
        /// The m daily LMP display list
        /// </summary>
        private List<DailyLMPDisplay> mDailyLMPDisplayList;
        /// <summary>
        /// Gets or sets the daily LMP display list.
        /// </summary>
        /// <value>
        /// The daily LMP display list.
        /// </value>
        public List<DailyLMPDisplay> DailyLMPDisplayList
        {
            get
            {
                return mDailyLMPDisplayList;
            }
            set
            {
                mDailyLMPDisplayList = value;
                RaisePropertyChanged("DailyLMPDisplayList");
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
        /// The m selected product
        /// </summary>
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
                Clear();
                RaisePropertyChanged("SelectedProduct");
            }
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

        #endregion

        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            RunRefreshCommand = new DelegateCommand(RefreshCommand);
            ProductList = new List<string> { "ALL", "UPTOs", "FTR" };
            SelectedProduct = "UPTOs";
            MarketList = new List<string> { "ERCOT" };
            SelectedMarket = "ERCOT";
            StartDate = DateTime.Parse(DateTime.Today.Month + "/1/" + DateTime.Today.Year);
            EndDate = DateTime.Today;
            //int market = 1;
            for (int i = 0; i < 2; i++)
            {
                int market = 9;
                if (i == 9)
                {
                    market = 7;
                }

                if (!mNodeHash.ContainsKey(market))
                {
                    Dictionary<string, NodeDetail> nodeTempDetailHash = DBAccess.GetAllNodes(market, mNodeTypeHash, mZoneHash);
                    mNodeHash.Add(market, nodeTempDetailHash);
                }
            }
            SetTypeList();

        }

        #region Private Methods

        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <returns></returns>
        private int GetMarket()
        {
            int marketKey = 0;
            if (SelectedMarket == "ERCOT")
            {
                marketKey = 9;
            }
            //else if (SelectedMarket == "MISO")
            //{
            //    marketKey = 2;
            //}
            //else if (SelectedMarket == "NYISO")
            //{
            //    marketKey = 3;
            //}
            else if (SelectedMarket == "CAISO")
            {
                marketKey = 7;
            }
            //else if (SelectedMarket == "SPP")
            //{
            //    marketKey = 12;
            //}
            //else
            //{
            //    marketKey = 9;
            //}
            return marketKey;
        }
        /// <summary>
        /// Sets the type list.
        /// </summary>
        private void SetTypeList()
        {
            if (mNodeTypeHash.Count == 0)
            {
                return;
            }
            List<string> typeList = mNodeTypeHash[GetMarket()];
            typeList.Insert(0, "ALL");
            TypeList = null;
            TypeList = typeList;
            SelectedType = "ALL";
        }
        /// <summary>
        /// Clears this instance.
        /// </summary>
        private void Clear()
        {
            DailyLMPDisplayList = null;
        }
        /// <summary>
        /// Gets the aggregrate.
        /// </summary>
        /// <param name="aggTuple">The aggregate tuple.</param>
        /// <param name="lmp">The LMP.</param>
        /// <returns></returns>
        private Tuple<double, double> GetAggregrate(Tuple<double, double> aggTuple, double lmp)
        {
            double count = 0;
            double sum = 0;
            if (AggAvgChecked)
            {
                double tempLmp = double.IsNaN(aggTuple.Item1) ? 0 : aggTuple.Item1;
                count = aggTuple.Item2 + 1;
                sum = tempLmp + lmp;
            }
            if (AggMaxChecked)
            {
                count = 1;
                if (double.IsNaN(aggTuple.Item1) || aggTuple.Item1 < lmp)
                {
                    sum = lmp;
                }
                else
                {
                    sum = aggTuple.Item1;
                }
            }
            if (AggMinChecked)
            {
                count = 1;
                if (double.IsNaN(aggTuple.Item1) || aggTuple.Item1 > lmp)
                {
                    sum = lmp;
                }
                else
                {
                    sum = aggTuple.Item1;
                }
            }
            return new Tuple<double, double>(sum, count);
        }
        /// <summary>
        /// Refreshes the command.
        /// </summary>
        private void RefreshCommand()
        {
            RefreshEnabled = false;
            Task.Factory.StartNew(() => { RefreshThreaded(); });
        }
        /// <summary>
        /// Refreshes the threaded.
        /// </summary>
        private void RefreshThreaded()
        {
            DateTime startDate = StartDate;
            Dictionary<int, bool> peakHash = new Dictionary<int, bool>();
            int mKey = GetMarket();
            while (startDate <= EndDate)
            {
                /*if (mDailyLMPHash.ContainsKey(startDate) && mDailyLMPHash[startDate].Count == 0)
                {
                    mDailyLMPHash.Remove(startDate);
                }
                if (mDailyDALMPHash.ContainsKey(startDate) && mDailyDALMPHash[startDate].Count == 0)
                {
                    mDailyDALMPHash.Remove(startDate);
                }*/
                if (!mDailyLMPHash.ContainsKey(startDate))
                {
                    Dictionary<int, DailyLMP> nodeLMPHash = mDataService.GetDailyLMP(mKey, startDate, false);
                    mDailyLMPHash.Add(startDate, nodeLMPHash);
                }

                if (!mDailyDALMPHash.ContainsKey(startDate))
                {
                    Dictionary<int, DailyLMP> nodeLMPHash = mDataService.GetDailyLMP(mKey, startDate, true);
                    mDailyDALMPHash.Add(startDate, nodeLMPHash);
                }
                bool peakYN = mDataService.IsPeakDay(startDate, mKey);
                if (!peakHash.ContainsKey(startDate.Day))
                {
                    peakHash.Add(startDate.Day, peakYN);
                }
                startDate = startDate.AddDays(1);
            }
            if (!mNodeHash.ContainsKey(mKey))
            {
                return;
            }
            List<DailyLMPDisplay> dailyLmpList = new List<DailyLMPDisplay>();
            Dictionary<string, NodeDetail> nodeDetailHash = mNodeHash[mKey];
            List<string> nodeNameList = new List<string>(nodeDetailHash.Keys);
            Dictionary<string, PricingNode> tempNodeHash = DBAccess.GetPricingNodesForMarket(mKey);
            foreach (string nodeName in nodeNameList)
            {
                if (!nodeDetailHash.ContainsKey(nodeName))
                {
                    continue;
                }
                try
                {
                    PricingNode node = null;
                    if (tempNodeHash.ContainsKey(nodeName))
                    {
                        node = tempNodeHash[nodeName];
                    }
                    else
                    {
                        node = DBAccess.GetNodeFromName(nodeName, mKey);
                    }
                    DailyLMPDisplay dailyLmpDisplay = new DailyLMPDisplay();
                    NodeDetail nodeDetail = nodeDetailHash[nodeName];
                    if (SelectedType != "ALL" && SelectedType != nodeDetail.Type)
                    {
                        continue;
                    }
                    dailyLmpDisplay.NodeName = nodeName;
                    dailyLmpDisplay.ZoneName = nodeDetail.Zone;
                    startDate = StartDate;
                    Dictionary<int, Tuple<double, double>> dayHash = new Dictionary<int, Tuple<double, double>>();
                    Dictionary<DateTime, Dictionary<int, DailyLMP>> dailyLMPHash = new Dictionary<DateTime, Dictionary<int, DailyLMP>>();
                    if (RTChecked || DARTChecked)
                    {
                        dailyLMPHash = mDailyLMPHash;
                    }
                    if (DAChecked)
                    {
                        dailyLMPHash = mDailyDALMPHash;
                    }
                    while (startDate <= EndDate)
                    {
                        if ((!DARTChecked && dailyLMPHash.ContainsKey(startDate)) || (DARTChecked && dailyLMPHash.ContainsKey(startDate) && mDailyDALMPHash.ContainsKey(startDate)))
                        {
                            Dictionary<int, DailyLMP> nodeLMPHash = dailyLMPHash[startDate];
                            Dictionary<int, DailyLMP> nodeDALMPHash = mDailyDALMPHash[startDate];
                            if ((!DARTChecked && nodeLMPHash.ContainsKey(node.NodeKey)) || (DARTChecked && nodeLMPHash.ContainsKey(node.NodeKey) && nodeDALMPHash.ContainsKey(node.NodeKey)))
                            {
                                DailyLMP dailyLmp = nodeLMPHash[node.NodeKey];
                                DailyLMP dailyDaLMP = new DailyLMP();
                                double? lmp = null;
                                if (DARTChecked)
                                {
                                    dailyDaLMP = nodeDALMPHash[node.NodeKey];
                                    lmp = dailyLmp.Average - dailyDaLMP.Average;
                                    if (PeakChecked)
                                    {
                                        lmp = dailyLmp.Peak - dailyDaLMP.Peak;
                                    }
                                    if (OffPeakChecked)
                                    {
                                        lmp = dailyLmp.OffPeak - dailyDaLMP.OffPeak;
                                    }
                                }
                                else
                                {
                                    lmp = dailyLmp.Average;
                                    if (PeakChecked)
                                    {
                                        lmp = dailyLmp.Peak;
                                    }
                                    if (OffPeakChecked)
                                    {
                                        lmp = dailyLmp.OffPeak;
                                    }
                                }
                                Tuple<double, double> lmpTuple = new Tuple<double, double>(double.NaN, 0);
                                if (dayHash.ContainsKey(startDate.Day))
                                {
                                    lmpTuple = dayHash[startDate.Day];
                                    dayHash.Remove(startDate.Day);
                                }
                                if (lmp != null)
                                {
                                    lmpTuple = GetAggregrate(lmpTuple, (double)lmp);
                                }
                                dayHash.Add(startDate.Day, lmpTuple);
                            }
                        }
                        startDate = startDate.AddDays(1);
                    }
                    List<int> dayList = new List<int>(dayHash.Keys);
                    int totaldays = 0;
                    double sum = 0;
                    double max = double.MinValue;
                    double min = double.MaxValue;
                    foreach (int day in dayList)
                    {
                        Tuple<double, double> lmpTuple = dayHash[day];
                        double? lmp = lmpTuple.Item2 == 0 ? null : (double?)Math.Round(lmpTuple.Item1 / lmpTuple.Item2, 2);
                        if (day == 1)
                        {
                            dailyLmpDisplay.D1 = lmp;
                        }
                        if (day == 2)
                        {
                            dailyLmpDisplay.D2 = lmp;
                        }
                        if (day == 3)
                        {
                            dailyLmpDisplay.D3 = lmp;
                        }
                        if (day == 4)
                        {
                            dailyLmpDisplay.D4 = lmp;
                        }
                        if (day == 5)
                        {
                            dailyLmpDisplay.D5 = lmp;
                        }
                        if (day == 6)
                        {
                            dailyLmpDisplay.D6 = lmp;
                        }
                        if (day == 7)
                        {
                            dailyLmpDisplay.D7 = lmp;
                        }
                        if (day == 8)
                        {
                            dailyLmpDisplay.D8 = lmp;
                        }
                        if (day == 9)
                        {
                            dailyLmpDisplay.D9 = lmp;
                        }
                        if (day == 10)
                        {
                            dailyLmpDisplay.D10 = lmp;
                        }
                        if (day == 11)
                        {
                            dailyLmpDisplay.D11 = lmp;
                        }
                        if (day == 12)
                        {
                            dailyLmpDisplay.D12 = lmp;
                        }
                        if (day == 13)
                        {
                            dailyLmpDisplay.D13 = lmp;
                        }
                        if (day == 14)
                        {
                            dailyLmpDisplay.D14 = lmp;
                        }
                        if (day == 15)
                        {
                            dailyLmpDisplay.D15 = lmp;
                        }
                        if (day == 16)
                        {
                            dailyLmpDisplay.D16 = lmp;
                        }
                        if (day == 17)
                        {
                            dailyLmpDisplay.D17 = lmp;
                        }
                        if (day == 18)
                        {
                            dailyLmpDisplay.D18 = lmp;
                        }
                        if (day == 19)
                        {
                            dailyLmpDisplay.D19 = lmp;
                        }
                        if (day == 20)
                        {
                            dailyLmpDisplay.D20 = lmp;
                        }
                        if (day == 21)
                        {
                            dailyLmpDisplay.D21 = lmp;
                        }
                        if (day == 22)
                        {
                            dailyLmpDisplay.D22 = lmp;
                        }
                        if (day == 23)
                        {
                            dailyLmpDisplay.D23 = lmp;
                        }
                        if (day == 24)
                        {
                            dailyLmpDisplay.D24 = lmp;
                        }
                        if (day == 25)
                        {
                            dailyLmpDisplay.D25 = lmp;
                        }
                        if (day == 26)
                        {
                            dailyLmpDisplay.D26 = lmp;
                        }
                        if (day == 27)
                        {
                            dailyLmpDisplay.D27 = lmp;
                        }
                        if (day == 28)
                        {
                            dailyLmpDisplay.D28 = lmp;
                        }
                        if (day == 29)
                        {
                            dailyLmpDisplay.D29 = lmp;
                        }
                        if (day == 30)
                        {
                            dailyLmpDisplay.D30 = lmp;
                        }
                        if (day == 31)
                        {
                            dailyLmpDisplay.D31 = lmp;
                        }
                        if (lmp != null)
                        {
                            totaldays++;
                            sum += (double)lmp;
                            if (max < lmp)
                            {
                                max = (double)lmp;
                            }
                            if (min > lmp)
                            {
                                min = (double)lmp;
                            }
                        }
                    }
                    if (dayList.Count > 0)
                    {
                        dailyLmpDisplay.Avg = (double?)Math.Round((double)(sum / totaldays), 2);
                        dailyLmpDisplay.Max = (double?)Math.Round((double)(double?)max, 2);
                        dailyLmpDisplay.Min = (double?)Math.Round((double)(double?)min, 2);
                        dailyLmpDisplay.PeakHash = peakHash;
                        dailyLmpList.Add(dailyLmpDisplay);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            DailyLMPDisplayList = null;
            DailyLMPDisplayList = dailyLmpList;
            RefreshEnabled = true;
            mIsFirstTime = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the source sinks.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        public void SetSourceSinks(List<Tuple<string, string>> sourceSinkList)
        {
            LmpForm.SetSourceSinks(sourceSinkList, GetMarket());
        }
        /// <summary>
        /// Shows the LMP graphs.
        /// </summary>
        /// <param name="day">The day.</param>
        public void ShowLMPGraphs(string day)
        {
            DateTime endDate = DateTime.Parse(EndDate.Month + "/" + day + "/" + EndDate.Year);
            LmpForm.OpenLmpGraphs(GetMarket(), LmpForm.GetSourceSinks(), endDate, endDate);
        }
        /// <summary>
        /// Shows the node analyzer.
        /// </summary>
        public void ShowNodeAnalyzer()
        {
            LmpForm.OpenLMPStatisticAnalyzer(GetMarket(), LmpForm.GetSourceSinks());
        }

        #endregion
    }
}
