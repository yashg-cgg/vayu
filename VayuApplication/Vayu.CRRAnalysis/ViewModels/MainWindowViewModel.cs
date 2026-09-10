using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Vayu.CRRAnalysis.Model;
using Vayu.DBLibrary;

namespace Vayu.CRRAnalysis.ViewModels
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

        private readonly IDataService _dataService;
        public DelegateCommand ImportCommand { private set; get; }
        public DelegateCommand RetrieveOldCommand { private set; get; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
        private string mUser = Environment.UserName;

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
        public List<FTRBid> FinalList;
        private List<FTRBid> mPathList;
        public List<FTRBid> PathList
        {
            get
            {
                return mPathList;
            }
            set
            {
                mPathList = value;
                RaisePropertyChanged("PathList");
            }
        }

        private Portfolio mSelectedPortfolio;
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

        private List<CRRAuction> mTraderPortfolioComboList;

        public List<CRRAuction> TraderPortfolioComboList
        {
            get
            {
                return mTraderPortfolioComboList;
            }
            set
            {
                mTraderPortfolioComboList = value;
                RaisePropertyChanged("TraderPortfolioComboList");

            }
        }
        private CRRAuction mTraderPortfolioComboSelectedItem;

        public CRRAuction TraderPortfolioComboSelectedItem
        {
            get
            {
                return mTraderPortfolioComboSelectedItem;
            }
            set
            {
                mTraderPortfolioComboSelectedItem = value;
                RaisePropertyChanged("TraderPortfolioComboSelectedItem");
            }
        }
        private List<string> mAuctionPortfolioComboList;
        public List<string> AuctionPortfolioComboList
        {
            get
            {
                return mAuctionPortfolioComboList;
            }
            set
            {
                mAuctionPortfolioComboList = value;
                RaisePropertyChanged("AuctionPortfolioComboList");
            }
        }
        private string mAuctionSelectedItem;
        public string AuctionSelectedItem
        {
            get
            {
                return mAuctionSelectedItem;
            }
            set
            {
                mAuctionSelectedItem = value;
                if (!ShowNewPortfolioChecked)
                    SetPortfolioList();
                if (ShowNewPortfolioChecked)
                    SetPortfolioList();
                RaisePropertyChanged("AuctionSelectedItem");
            }
        }
        private List<Portfolio> mPortfolioComboList;
        public List<Portfolio> PortfolioComboList
        {
            get
            {
                return mPortfolioComboList;
            }
            set
            {
                mPortfolioComboList = value;
                RaisePropertyChanged("PortfolioComboList");
            }
        }
        private bool mShowNewPortfolioChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show new portfolio checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show new portfolio checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowNewPortfolioChecked
        {
            get
            {
                return mShowNewPortfolioChecked;
            }
            set
            {
                mShowNewPortfolioChecked = value;
                if (!ShowNewPortfolioChecked)
                {
                    AuctionPortfolioComboList = null;
                    SetAuctionList();
                }
                if (ShowNewPortfolioChecked)
                {
                    SetNewAuctionPortfolioList();
                }
                RaisePropertyChanged("ShowNewPortfolioChecked");
            }
        }
        private Portfolio mPortfolioComboSelectedItem;
        public Portfolio PortfolioComboSelectedItem
        {
            get
            {
                return mPortfolioComboSelectedItem;
            }
            set
            {
                mPortfolioComboSelectedItem = value;
                RaisePropertyChanged("PortfolioComboSelectedItem");
            }
        }

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            ImportCommand = new DelegateCommand(Import);
            RetrieveOldCommand = new DelegateCommand(RetrieveOld);
            RunExportCSVCommand = new DelegateCommand(ExportToCSVCommand);
            SetUserPortfolioList();

        }

        public void ExportToCSVCommand()
        {
            if (TraderPortfolioComboSelectedItem != null)
                Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
            if (PortfolioComboSelectedItem != null)
                Task.Factory.StartNew(() => { ExportToCSVThreadedOld(); });
        }

        private void ExportToCSVThreadedOld()
        {
            try
            {
                if (PortfolioComboSelectedItem == null)
                {
                    Mouse.OverrideCursor = null;
                    return;
                }
                if (PathList == null || PathList.Count == 0)
                {
                    System.Windows.MessageBox.Show("No data to export to");
                    return;
                }
                SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
                dialog.FileName = PortfolioComboSelectedItem.Name + "_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if ((bool)dialog.ShowDialog())
                {
                    if (dialog.FileName != "")
                    {
                        if (PathList != null && PathList.Count > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (PropertyInfo item in PathList[0].GetType().GetProperties())
                            {

                                if (item.Name == "Source" || item.Name == "Sink" || item.Name == "ClassType" || item.Name == "MW1"
                                    || item.Name == "Price1" || item.Name == "HedgeType" ||
                                item.Name == "Premium" ||
item.Name == "PriceLastOBL" || item.Name == "PriceLastOPT" || item.Name == "PriceLastDiff" || item.Name == "PricePrevOBL" ||
item.Name == "PricePrevOPT" || item.Name == "PricePrevDiff" || item.Name == "MWOwnedOBL" || item.Name == "MWOwnedOPT" || item.Name == "MWSubmittedOBL" ||
item.Name == "MWSubmittedOPT" || item.Name == "MWClearedOBL" || item.Name == "MWClearedOPT" || item.Name == "MinDA" || item.Name == "MaxDA" || item.Name == "MedianDA45")
                                {
                                    builder.Append(item.Name + ",");
                                }
                            }
                            builder.AppendLine();
                            foreach (FTRBid item in PathList)
                            {
                                foreach (PropertyInfo propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name == "Source" || propItem.Name == "Sink" || propItem.Name == "ClassType" || propItem.Name == "MW1"
                                   || propItem.Name == "Price1" || propItem.Name == "HedgeType" ||
                               propItem.Name == "Premium" ||
propItem.Name == "PriceLastOBL" || propItem.Name == "PriceLastOPT" || propItem.Name == "PriceLastDiff" || propItem.Name == "PricePrevOBL" ||
propItem.Name == "PricePrevOPT" || propItem.Name == "PricePrevDiff" || propItem.Name == "MWOwnedOBL" || propItem.Name == "MWOwnedOPT" || propItem.Name == "MWSubmittedOBL" ||
propItem.Name == "MWSubmittedOPT" || propItem.Name == "MWClearedOBL" || propItem.Name == "MWClearedOPT" || propItem.Name == "MinDA" || propItem.Name == "MaxDA" || propItem.Name == "MedianDA45")
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
        }

        private void ExportToCSVThreaded()
        {
            try
            {
                if (TraderPortfolioComboSelectedItem == null)
                {
                    Mouse.OverrideCursor = null;
                    return;
                }
                if (PathList == null || PathList.Count == 0)
                {
                    System.Windows.MessageBox.Show("No data to export to");
                    return;
                }
                SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
                dialog.FileName = TraderPortfolioComboSelectedItem.Name + "_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if ((bool)dialog.ShowDialog())
                {
                    if (dialog.FileName != "")
                    {
                        if (PathList != null && PathList.Count > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (PropertyInfo item in PathList[0].GetType().GetProperties())
                            {

                                if (item.Name == "Source" || item.Name == "Sink" || item.Name == "ClassType" || item.Name == "MW1"
                                    || item.Name == "Price1" || item.Name == "HedgeType" ||
                                item.Name == "Premium" ||
item.Name == "PriceLastOBL" || item.Name == "PriceLastOPT" || item.Name == "PriceLastDiff" || item.Name == "PricePrevOBL" ||
item.Name == "PricePrevOPT" || item.Name == "PricePrevDiff" || item.Name == "MWOwnedOBL" || item.Name == "MWOwnedOPT" || item.Name == "MWSubmittedOBL" ||
item.Name == "MWSubmittedOPT" || item.Name == "MWClearedOBL" || item.Name == "MWClearedOPT" || item.Name == "MinDA" || item.Name == "MaxDA" || item.Name == "MedianDA45")
                                {
                                    builder.Append(item.Name + ",");
                                }
                            }
                            builder.AppendLine();
                            foreach (FTRBid item in PathList)
                            {
                                foreach (PropertyInfo propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name == "Source" || propItem.Name == "Sink" || propItem.Name == "ClassType" || propItem.Name == "MW1"
                                   || propItem.Name == "Price1" || propItem.Name == "HedgeType" ||
                               propItem.Name == "Premium" ||
propItem.Name == "PriceLastOBL" || propItem.Name == "PriceLastOPT" || propItem.Name == "PriceLastDiff" || propItem.Name == "PricePrevOBL" ||
propItem.Name == "PricePrevOPT" || propItem.Name == "PricePrevDiff" || propItem.Name == "MWOwnedOBL" || propItem.Name == "MWOwnedOPT" || propItem.Name == "MWSubmittedOBL" ||
propItem.Name == "MWSubmittedOPT" || propItem.Name == "MWClearedOBL" || propItem.Name == "MWClearedOPT" || propItem.Name == "MinDA" || propItem.Name == "MaxDA" || propItem.Name == "MedianDA45")
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
        }
        public void Import()
        {
            PathList = new List<FTRBid>();
            Func<string, double?> func = (str) =>
            {
                if (str == null || str.Trim().Length == 0)
                {
                    return null;
                }
                return double.Parse(str);
            };
            if (TraderPortfolioComboSelectedItem == null)
            {
                MessageBox.Show("Please select portfolio");
                return;
            }
            int marketKey = 9;
            CRRAuction ftrAuction = new CRRAuction();
            ftrAuction.Name = TraderPortfolioComboSelectedItem.Name;
            ftrAuction.Key = TraderPortfolioComboSelectedItem.Key;
            Tuple<int, string> detail = GetPortfolioAuctionDetail();


            int portfolioKey = detail.Item1;
            ftrAuction.Name = detail.Item2;
            Dictionary<int, string> ListKeys = _dataService.GetPrevKeys(ftrAuction.Name);
            string periodError = string.Empty;
            DateTime ftrMonth = _dataService.GetAuctionStartDate(9, ftrAuction.Name);
            ImportHelper import = ImportHelper.ConstructByMarket("Ercot", ftrMonth);
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = fileDialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            List<Bid> bidList = new List<Bid>();
            List<FTRBid> ftrBidList = new List<FTRBid>();
            using (FileStream stream = File.Open(fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader fs = new StreamReader(stream);
                string line = fs.ReadLine();
                line = fs.ReadLine();

                while (line != null)
                {
                    try
                    {
                        string[] tokens = line.Split(',');
                        FTRBid bid = new FTRBid();
                        bid.mParticipant = import.Participant;
                        bid.mMarketKey = import.MarketKey;
                        bid.HedgeType = tokens[17].ToString().ToLower() == "obl" ? "OBL" : tokens[17].ToString().ToLower() == "opt" ? "OPT" : tokens[17].ToString();//"OBL";
                        string tempSource = tokens[0].Replace("?", " ");
                        bid.Source = tempSource.Replace("�", " ");
                        string tempSink = tokens[1].Replace("?", " ");
                        bid.Sink = tempSink.Replace("�", " ");

                        if (bid.Source.Trim().Length == 0 || bid.Sink.Trim().Length == 0)
                        {
                            break;
                        }
                        bid.ClassType = tokens[3].ToUpper();
                        bid.PeriodName = tokens[4];
                        Period p = new Period();
                        if (marketKey == 1)
                        {
                            p = import.GetPeriodByName(bid.PeriodName);
                        }
                        else
                        {
                            p = import.GetPeriodErcotByName(bid.PeriodName);
                        }

                        if (p == null)
                        {
                            if (string.IsNullOrEmpty(periodError))
                            {
                                periodError = "Errors in reading Period Name, All records are not imported.";
                            }
                            continue;
                        }
                        bid.PeriodKey = p.PeriodKey;
                        bid.month = ftrMonth;
                        bid.mPeriodStartDate = p.Date;
                        if (bid.ClassType.ToUpper() == "PEAKWE")
                        {
                            bid.PeriodHours = p.PeakWEHours;
                        }
                        else

                            if (bid.ClassType.ToUpper() == "PEAK" || bid.ClassType.ToUpper() == "ONPEAK" || bid.ClassType.ToUpper() == "PEAKWD")
                        {
                            bid.PeriodHours = p.PeakHours;
                        }

                        else
                        {
                            bid.PeriodHours = p.OffPeakHours;
                            if (marketKey == 1)
                            {
                                bid.ClassType = "OFFPEAK";
                            }
                            else
                            {
                                bid.ClassType = "OFF-PEAK";
                            }
                        }

                        bid.MW1 = func(tokens[5]);
                        bid.Price1 = func(tokens[6]);

                        ftrBidList.Add(bid);
                    }
                    catch (Exception ex)
                    {

                    }
                    line = fs.ReadLine();
                }
                fs.Close();

            }
            //SetPortfolioList();
            //AddPortfolio();


            FinalList = ftrBidList;
            Retrieve(ListKeys, ftrMonth);

            SetAuctionList();
            if (string.IsNullOrEmpty(periodError))
            {
                MessageBox.Show("Import Done.");
            }
            else
            {
                MessageBox.Show(periodError);
            }
        }

        private void Retrieve(Dictionary<int, string> listkey, DateTime ftrMonth)
        {
            Dictionary<string, double> dictPrevMonth = new Dictionary<string, double>();
            Dictionary<string, double> dictPrev2Month = new Dictionary<string, double>();

            Dictionary<string, double> dictPrevMonthNode = new Dictionary<string, double>();
            Dictionary<string, double> dictPrev2MonthNode = new Dictionary<string, double>();

            Dictionary<string, double> dictPrevMonthOptions = new Dictionary<string, double>();
            Dictionary<string, double> dictPrev2MonthOptions = new Dictionary<string, double>();
            Dictionary<string, double> dictOptionsSubmittedMW = new Dictionary<string, double>();

            Dictionary<string, double> dictOptionsSubmitted = new Dictionary<string, double>();

            Dictionary<string, double> dictOptionsClearead = new Dictionary<string, double>();
            Dictionary<string, double> dictMin = new Dictionary<string, double>();
            Dictionary<string, double> dictMax = new Dictionary<string, double>();
            Dictionary<string, double> dicmedian45 = new Dictionary<string, double>();
            List<string> listsourcesink = new List<string>();
            string nodesnames = "";
            if (FinalList.Count > 0)
            {
                foreach (FTRBid item in FinalList)
                {
                    if (!listsourcesink.Contains(item.Source))
                    {
                        listsourcesink.Add(item.Source);
                        nodesnames = nodesnames + "'" + item.Source + "',";
                    }
                    if (!listsourcesink.Contains(item.Sink))
                    {
                        listsourcesink.Add(item.Sink);
                        nodesnames = nodesnames + "'" + item.Source + "',";
                    }

                }
                nodesnames = nodesnames.Remove(nodesnames.Length - 1);
            }
            dictOptionsSubmittedMW = _dataService.GetPrevOptionsData(ftrMonth);
            dictMin = _dataService.GetMin();
            dictMax = _dataService.GetMax();
            dicmedian45 = _dataService.GetMedian45();
            bool check = true;
            foreach (var item in listkey)
            {
                if (!check)
                {
                    dictPrev2Month = _dataService.GetPrevData(item.Key);
                    dictPrev2MonthNode = _dataService.GetPrevNodeData(item.Key, nodesnames);
                    dictPrev2MonthOptions = _dataService.GetPrevOptionData(item.Key);
                    //dictOptionsClearead = _dataService.GetClearedOptionData(item.Key);
                }
                if (check)
                {
                    dictPrevMonth = _dataService.GetPrevData(item.Key);
                    dictPrevMonthNode = _dataService.GetPrevNodeData(item.Key, nodesnames);
                    dictPrevMonthOptions = _dataService.GetPrevOptionData(item.Key);
                    dictOptionsSubmitted = _dataService.GetSubmittedOptionData(item.Key);
                    dictOptionsClearead = _dataService.GetClearedOptionData(item.Key);
                    check = false;
                }
            }
            if (FinalList != null)
            {
                foreach (FTRBid item in FinalList)
                {
                    item.Premium = item.Price1 * item.MW1 * item.PeriodHours;
                    string keymedian = (item.Source.Trim() + item.Sink.Trim() + item.ClassType.Trim()).ToLower();
                    string key = (item.Source.Trim() + item.Sink.Trim() + item.HedgeType.Trim() + item.ClassType.Trim()).ToLower();
                    string keyOBL = (item.Source.Trim() + item.Sink.Trim() + "OBL" + item.ClassType.Trim()).ToLower();
                    string keyOPT = (item.Source.Trim() + item.Sink.Trim() + "OPT" + item.ClassType.Trim()).ToLower();
                    string keyOBLminmax = (item.Source.Trim() + item.Sink.Trim() + item.ClassType.Trim()).ToLower();
                    string keyOPTminmax = (item.Source.Trim() + item.Sink.Trim() + item.ClassType.Trim()).ToLower();

                    //OBL
                    if (dictPrevMonth.ContainsKey(keyOBL))
                    {
                        item.PriceLastOBL = Math.Round(dictPrevMonth[keyOBL], 2);
                    }
                    else
                    {
                        if (dictPrevMonthNode.ContainsKey(keyOBL))
                            item.PriceLastOBL = Math.Round(dictPrevMonthNode[keyOBL], 2);
                    }

                    if (dictPrev2Month.ContainsKey(keyOBL))
                    {
                        item.PricePrevOBL = Math.Round(dictPrev2Month[keyOBL], 2);
                    }
                    else
                    {
                        if (dictPrev2MonthNode.ContainsKey(keyOBL))
                            item.PricePrevOBL = Math.Round(dictPrev2MonthNode[keyOBL], 2);
                    }

                    //OPT
                    if (dictPrevMonth.ContainsKey(keyOPT))
                    {
                        item.PriceLastOPT = Math.Round(dictPrevMonth[keyOPT], 2);
                    }
                    else
                    {
                        if (dictPrevMonthOptions.ContainsKey(keyOPT))
                        {
                            //if(dictPrevMonthOptions[keyOPT]!= item.PricePrevOBL)
                            if (dictPrevMonthOptions[keyOPT] > 0)
                                item.PriceLastOPT = Math.Round(dictPrevMonthOptions[keyOPT], 2);

                        }
                    }
                    if (dictPrev2Month.ContainsKey(keyOPT))
                    {
                        item.PricePrevOPT = Math.Round(dictPrev2Month[keyOPT], 2);
                    }
                    else
                    {
                        if (dictPrev2MonthOptions.ContainsKey(keyOPT))
                        {
                            if (dictPrev2MonthOptions[keyOPT] > 0)
                                item.PricePrevOPT = Math.Round(dictPrev2MonthOptions[keyOPT], 2);
                        }
                    }

                    //
                    if (item.HedgeType == "OPT")
                    {
                        if (item.PriceLastOPT.HasValue)
                            item.PriceLastDiff = Math.Round((item.Price1.Value - item.PriceLastOPT.Value), 2);
                        if (item.PricePrevOPT.HasValue)
                            item.PricePrevDiff = Math.Round((item.Price1.Value - item.PricePrevOPT.Value), 2);

                    }
                    if (item.HedgeType == "OBL")
                    {
                        if (item.PriceLastOBL.HasValue)
                            item.PriceLastDiff = Math.Round((item.Price1.Value - item.PriceLastOBL.Value), 2);
                        if (item.PricePrevOBL.HasValue)
                            item.PricePrevDiff = Math.Round((item.Price1.Value - item.PricePrevOBL.Value), 2);
                    }
                    //OwnSubmitted
                    if (dictOptionsSubmittedMW.ContainsKey(keyOBL))
                        item.MWOwnedOBL = Math.Round(dictOptionsSubmittedMW[keyOBL], 2);
                    else
                        item.MWOwnedOBL = 0;
                    if (dictOptionsSubmittedMW.ContainsKey(keyOPT))
                        item.MWOwnedOPT = Math.Round(dictOptionsSubmittedMW[keyOPT], 2);
                    else
                        item.MWOwnedOPT = 0;
                    //submitted
                    if (dictOptionsSubmitted.ContainsKey(keyOPT))
                    {
                        item.MWSubmittedOPT = Math.Round(dictOptionsSubmitted[keyOPT], 2);
                    }

                    if (dictOptionsSubmitted.ContainsKey(keyOBL))
                    {
                        item.MWSubmittedOBL = dictOptionsSubmitted[keyOBL];
                    }

                    //Cleared
                    if (dictOptionsClearead.ContainsKey(keyOPT))
                    {
                        item.MWClearedOPT = Math.Round(dictOptionsClearead[keyOPT], 2);
                    }

                    if (dictOptionsClearead.ContainsKey(keyOBL))
                    {
                        item.MWClearedOBL = Math.Round(dictOptionsClearead[keyOBL], 2);
                    }

                    //Min

                    if (dictMin.ContainsKey(keyOPTminmax))
                    {
                        item.MinDA = Math.Round(dictMin[keyOPTminmax], 2);
                    }
                    else if (dictMin.ContainsKey(keyOBLminmax))
                    {
                        item.MinDA = Math.Round(dictMin[keyOBLminmax], 2);
                    }

                    //Max

                    if (dictMax.ContainsKey(keyOPTminmax))
                    {
                        item.MaxDA = Math.Round(dictMax[keyOPTminmax], 2);
                    }
                    else if (dictMax.ContainsKey(keyOBLminmax))
                    {
                        item.MaxDA = Math.Round(dictMax[keyOBLminmax], 2);
                    }
                    //GetMedian45

                    if (item.Source == "CPSES_UNIT1" && item.Sink == "WCPP_ST1")
                    {

                    }
                    if (dicmedian45.ContainsKey(keymedian))
                    {
                        item.MedianDA45 = Math.Round(dicmedian45[keymedian], 2);
                    }

                }
            }
            PathList = FinalList;
        }

        private void SetAuctionList()
        {
            AuctionPortfolioComboList = _dataService.GetAuctionList(false);
        }
        private void SetNewAuctionPortfolioList()
        {
            AuctionPortfolioComboList = _dataService.GetNewAuctionList(true);
        }
        private void SetUserPortfolioList()
        {
            TraderPortfolioComboList = null;
            List<CRRAuction> auctionList = _dataService.GetFtrAuctions("Ercot");
            List<CRRAuction> addPortfolioList = new List<CRRAuction>();

            List<Tuple<int, string>> portfolioList = _dataService.GetPortfolioList("Ercot");
            if (!((mUser == "user1") || (mUser == "user6" || mUser == "piyush")))
            {
                portfolioList.RemoveAll(x => x.Item2 == "SIGMA_FTR_STRAT");
                portfolioList.RemoveAll(x => x.Item2 == "RISK_SIGMA_FTR");
            }
            foreach (CRRAuction auction in auctionList)
            {
                foreach (Tuple<int, string> sppPortfolio in portfolioList)
                {
                    CRRAuction addPortfolio = new CRRAuction();
                    addPortfolio.Key = auction.Key;
                    addPortfolio.Name = auction.Name + "-" + sppPortfolio.Item2;
                    addPortfolioList.Add(addPortfolio);
                }
            }
            TraderPortfolioComboList = null;
            TraderPortfolioComboList = addPortfolioList;
        }
        private void SetPortfolioList()
        {
            try
            {
                DateTime sendDate = DateTime.Parse(DateTime.Now.ToShortDateString());
                string product = "FTR";
                List<Portfolio> portfolioList = null;
                portfolioList = DBAccess.GetFTRPortfolio(sendDate, mUser, product, "Ercot", AuctionSelectedItem);
                PortfolioComboList = null;
                if (portfolioList.Count > 0)
                {
                    if (!((mUser == "user1") || (mUser == "user6" || mUser == "piyush")))
                    {
                        portfolioList.RemoveAll(x => x.Name == "SIGMA_FTR_STRAT");
                        portfolioList.RemoveAll(x => x.Name == "RISK_SIGMA_FTR");
                    }
                    PortfolioComboList = portfolioList;
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
        }
        public void RetrieveOld()
        {
            if (PortfolioComboSelectedItem == null)
            {
                // MessageBox.Show("Please add portfolio");
                return;
            }

            List<CRRTransaction> TransactionList = null;
            string auctionName = string.Empty;
            if (AuctionSelectedItem.ToLower().Contains("round"))
            {
                auctionName = AuctionSelectedItem.Substring(0, 20);
            }
            else
            {
                auctionName = AuctionSelectedItem;
            }
            TransactionList = DBAccess.GetFtrTransactions(auctionName, 9);

            List<string> ids = new List<string>();
            if (TransactionList != null && TransactionList.Count != 0)
            {
                foreach (var item in TransactionList)
                {
                    if (item.IsSelected)
                        ids.Add(item.ID);
                }
            }
            double? totalMW = 0;
            PathList = null;
            List<FTRBid> pathsList = new List<FTRBid>();
            // foreach (Portfolio portfolio in PortfolioList)
            // {
            //List<FTRBid> pathList = new List<FTRBid>();
            //if(SelectedTransaction)
            List<FTRBid> pathList = DBAccess.GetFtrBidList(PortfolioComboSelectedItem.ID, AuctionSelectedItem, false, 9, ids);
            pathsList.AddRange(pathList);
            // }
            int marketKey = 9;
            Dictionary<int, string> ListKeys = _dataService.GetPrevKeys(auctionName);
            string periodError = string.Empty;
            DateTime ftrMonth = _dataService.GetAuctionStartDate(9, auctionName);

            FinalList = pathsList;
            Retrieve(ListKeys, ftrMonth);

            SetAuctionList();
        }

        private Tuple<int, string> GetPortfolioAuctionDetail()
        {
            string name = string.Empty;
            int portKey = 727;
            int marketKey = 9;
            List<Tuple<int, string>> sppPortfolioList = _dataService.GetPortfolioList("Ercot");
            foreach (Tuple<int, string> misoSppPortfolio in sppPortfolioList)
            {
                if (TraderPortfolioComboSelectedItem.Name.IndexOf("-") != -1)
                {
                    string temp = TraderPortfolioComboSelectedItem.Name.Substring(TraderPortfolioComboSelectedItem.Name.IndexOf("-") + 1);
                    if (temp == misoSppPortfolio.Item2)
                    {
                        name = TraderPortfolioComboSelectedItem.Name.Replace("-" + misoSppPortfolio.Item2, "");
                        portKey = misoSppPortfolio.Item1;
                        break;
                    }
                }
            }
            return new Tuple<int, string>(portKey, name);
        }
    }
}
