using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.NodePriceLibrary;

namespace Vayu.ProfitLossHour.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.ProfitLossHour.Model.IHourlyDataService" />
    public partial class HourlyDataService : IHourlyDataService
    {
        #region Global Data Members
        /// <summary>
        /// The m select date portfolio command
        /// </summary>
        private SqlCommand mSelectDatePortfolioCommand;
        /// <summary>
        /// The m select portfolio by admin command
        /// </summary>
        private SqlCommand mSelectPortfolioByAdminCommand;
        /// <summary>
        /// The m select portfolio by user command
        /// </summary>
        private SqlCommand mSelectPortfolioByUserCommand;
        /// <summary>
        /// The m select portfolio command
        /// </summary>
        private SqlCommand mSelectPortfolioCommand;
        /// <summary>
        /// The m select market command
        /// </summary>
        private SqlCommand mSelectMarketCommand;
        #endregion

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        public void loadDBCommands()
        {

            mSelectPortfolioByUserCommand = new SqlCommand();
            mSelectPortfolioByUserCommand.CommandText = "select STRIP,Portfolio_ID from PORTFOLIO where ACCOUNT in (select ACCOUNT_ID from END_USER_ACCOUNT where ENDUSER_KEY = " +
                                                        "(select ENDUSER_KEY from END_USER where Replace(AD_LOGIN,' ','') like '%' + Replace(@AD_LOGIN,' ','') + ',%' " +
                            " OR Replace(AD_LOGIN,' ','') like '%,' + Replace(@AD_LOGIN,' ','') + ',%' OR Replace(AD_LOGIN,' ','') like '%,' + Replace(@AD_LOGIN,' ','') + '%' OR AD_LOGIN =@AD_LOGIN)) " +
                            " and Product = @product and hub = @hub order by STRIP";
            mSelectPortfolioByUserCommand.Parameters.AddWithValue("@AD_LOGIN", "AD_LOGIN");
            mSelectPortfolioByUserCommand.Parameters.AddWithValue("@product", "");
            mSelectPortfolioByUserCommand.Parameters.AddWithValue("@hub", "");

            mSelectDatePortfolioCommand = new SqlCommand();
            mSelectDatePortfolioCommand.CommandText = "select Distinct PortfolioKey from ClearedEES where PortfolioKey=@PortfolioKey and MarketDateTime >= @StartDate AND MarketDateTime <= @EndDate";
            mSelectDatePortfolioCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mSelectDatePortfolioCommand.Parameters.AddWithValue("@StartDate", "@StartDate");
            mSelectDatePortfolioCommand.Parameters.AddWithValue("@EndDate", "@EndDate");

            mSelectPortfolioByAdminCommand = new SqlCommand();
            mSelectPortfolioByAdminCommand.CommandText = "select STRIP,Portfolio_ID from PORTFOLIO where Product = @product and hub = @hub";

            mSelectPortfolioByAdminCommand.Parameters.AddWithValue("@product", "");
            mSelectPortfolioByAdminCommand.Parameters.AddWithValue("@hub", "");
            //
            mSelectPortfolioCommand = new SqlCommand();
            mSelectPortfolioCommand.CommandText = "SELECT PORTFOLIO_ID, PRODUCT, HUB, STRIP FROM PORTFOLIO WHERE ACCOUNT = @ACCOUNT ORDER BY PRODUCT, HUB, STRIP ";
            mSelectPortfolioCommand.Parameters.AddWithValue("@ACCOUNT", "ACCOUNT");

            mSelectMarketCommand = new SqlCommand();
            mSelectMarketCommand.CommandText = "select market from portfolio where portfoliokey = @portfoliokey";
            mSelectMarketCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");

        }

        /// <summary>
        /// 
        /// </summary>
        struct PNL
        {
            /// <summary>
            /// The source
            /// </summary>
            public int source;
            /// <summary>
            /// The sink
            /// </summary>
            public int sink;
            /// <summary>
            /// The source name
            /// </summary>
            public string sourceName;
            /// <summary>
            /// The sink name
            /// </summary>
            public string sinkName;
            /// <summary>
            /// Gets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get { return (sourceName ?? "") + (string.IsNullOrEmpty(sinkName) ? "" : " -> " + (sinkName ?? "")); } }
            /// <summary>
            /// The hour
            /// </summary>
            public int Hour;
            /// <summary>
            /// The date
            /// </summary>
            public DateTime Date;
            /// <summary>
            /// The PNL value
            /// </summary>
            public double PNLValue;
            /// <summary>
            /// The mw value
            /// </summary>
            public double MWValue;
            /// <summary>
            /// Gets the PNL double.
            /// </summary>
            /// <value>
            /// The PNL double.
            /// </value>
            public double PNLDouble
            {
                get { return (double.IsNaN(PNLValue) ? 0 : PNLValue); }
            }
        }

        /// <summary>
        /// Logs the specified rt list.
        /// </summary>
        /// <param name="rtList">The rt list.</param>
        /// <param name="daList">The da list.</param>
        private void Log(List<Vayu.NodePriceLibrary.Node> rtList, List<Vayu.NodePriceLibrary.Node> daList)
        {
            StreamWriter file = new StreamWriter(File.Create(@"C:\Temp\HourlyLog.txt"));

            Action<List<Vayu.NodePriceLibrary.Node>, string> action = (list, name) =>
            {
                file.WriteLine("\r\n-----" + name + "------");
                foreach (var item in list)
                {
                    file.WriteLine("--------------Node:" + item.NodeName);
                    foreach (var item2 in item.TimePriceList)
                        file.WriteLine(item2.MarketTime.ToString() + "......" + item2.Price.ToString());
                }
            };

            action(rtList, "RT");
            action(daList, "DA");
            file.Close();
        }

        /// <summary>
        /// Gets the PNL list.
        /// </summary>
        /// <param name="bids">The bids.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="isUpto">if set to <c>true</c> [is upto].</param>
        /// <returns></returns>
        public HourlyPNLList GetPNLList(List<Bid> bids, DateTime StartDate, bool isUpto)
        {
            List<PNL> pnlList = new List<PNL>();

            Hashtable nodeHash = new Hashtable();
            HourlyPNLList hourlyPNLList = new HourlyPNLList();

            Tuple<List<Node>, List<Node>> nodeTuple = GetNodeLists(bids, isUpto);
            List<Vayu.NodePriceLibrary.Node> rtList = nodeTuple.Item2;
            List<Vayu.NodePriceLibrary.Node> daList = nodeTuple.Item1;

            //ClearCache();
            DARTNode.GetDART(rtList, daList, StartDate, 1, true, true);

            //Log(rtList, daList);
            foreach (var item in nodeTuple.Item1)
                nodeHash[item.NodeId] = item.NodeName;

            foreach (var item in nodeTuple.Item2)
                nodeHash[item.NodeId] = item.NodeName;

            foreach (var bid in bids)
            {
                PNL pnl = new PNL();
                pnl.Date = bid.MarketDateTime.Hour == 0 ? bid.MarketDateTime.AddDays(-1).Date : bid.MarketDateTime.Date;
                pnl.Hour = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                pnl.sourceName = nodeHash[bid.Source] as string;
                string sourceKey = bid.MarketDateTime.ToString() + bid.Source.ToString();
                double da = double.NaN;
                double rt = double.NaN;
                pnl.MWValue = bid.MW;

                if (isUpto)
                {
                    string sinkKey = bid.MarketDateTime.ToString() + bid.Sink.ToString();
                    pnl.sinkName = nodeHash[bid.Sink] as string;
                    //pnl.MWValue = bid.MW;

                    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey) &&
                        !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sDAHash[sinkKey]))
                        da = DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey];

                    if (DARTNode.sRTHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                        !double.IsNaN(DARTNode.sRTHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                        rt = DARTNode.sRTHash[sinkKey] - DARTNode.sRTHash[sourceKey];

                    //pnl.PNLValue = (rt - da) * pnl.MWValue;
                }
                else
                {
                    //pnl.MWValue = Math.Abs(bid.MW);

                    if (DARTNode.sDAHash.ContainsKey(sourceKey) &&
                        !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                        da = DARTNode.sDAHash[sourceKey];

                    if (DARTNode.sRTHash.ContainsKey(sourceKey) &&
                        !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                        rt = DARTNode.sRTHash[sourceKey];

                    //pnl.PNLValue = (rt - da) * pnl.MWValue;
                }

                pnl.PNLValue = (rt - da) * pnl.MWValue;
                pnlList.Add(pnl);
            }

            foreach (var groupItem in pnlList.GroupBy(x => x.Key))
            {
                HourlyPNL pnlRec = new HourlyPNL();
                pnlRec.NodeName = groupItem.Key;

                foreach (var item in groupItem.GroupBy(x => x.Hour))
                {
                    if (item.Key - 1 < 0)
                        continue;

                    HourData data = pnlRec.HourlyList[item.Key - 1];
                    data.ClearedMW = item.Sum(x => x.MWValue);
                    data.PNL = item.Sum(x => x.PNLDouble);
                }

                hourlyPNLList.Add(pnlRec);
            }

            return hourlyPNLList;
        }

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private Hashtable GetHash(List<Vayu.NodePriceLibrary.Node> list)
        {
            Hashtable hash = new Hashtable();
            foreach (var item in list)
            {
                List<TimePrice> TimePriceList = item.TimePriceList;

                foreach (var timePrice in item.TimePriceList)
                {
                    if (!double.IsNaN(timePrice.Price))
                        hash[timePrice.MarketTime.ToString() + item.NodeId.ToString()] = timePrice.Price;
                }
            }

            return hash;
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
                    buildList(bid, bid.Sink);
            }

            return new Tuple<List<Node>, List<Node>>(daList.Values.Cast<Node>().ToList(),
                rtList.Values.Cast<Node>().ToList());
        }

        /// <summary>
        /// Gets the cleareds.
        /// </summary>
        /// <param name="StartDate">The start date.</param>
        /// <param name="portKey">The port key.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="isUpTos">if set to <c>true</c> [is up tos].</param>
        /// <returns></returns>
        public List<Bid> GetCleareds(DateTime StartDate, int portKey, int marketKey, bool isUpTos)
        {
            List<Bid> tempList = DBAccess.GetCleareds(isUpTos, marketKey, portKey, StartDate, StartDate.AddDays(1));
            return tempList;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{Vayu.ProfitLossHour.Model.HourlyPNL}" />
    public class HourlyPNLList : ObservableCollection<HourlyPNL>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HourlyPNLList"/> class.
        /// </summary>
        public HourlyPNLList() : base()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HourlyPNLList"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public HourlyPNLList(IEnumerable<HourlyPNL> list) : base(list)
        {
        }
        /// <summary>
        /// Updates the total.
        /// </summary>
        public void UpdateTotal()
        {
            foreach (var item in this)
            {
                item.UpdateTotal();
            }
            HourlyPNL totalPnl = new HourlyPNL();
            totalPnl.NodeName = "TOTAL";
            for (int i = 0; i <= 24; i++)
            {
                totalPnl.HourlyList[i].ClearedMW = this.Sum(x => x.HourlyList[i].ClearedMW);
                totalPnl.HourlyList[i].PNL = this.Sum(x => x.HourlyList[i].PNL);
            }
            this.Insert(0, totalPnl);
        }
    }
    //public class HourData
    //{
    //    private double? mPnl;
    //    public double? PNL
    //    {
    //        get
    //        {
    //            return mPnl;
    //        }
    //        set
    //        {
    //            if (double.IsNaN(value.GetValueOrDefault()))
    //            {
    //                mPnl = 0;
    //            }
    //            else
    //            {
    //                mPnl = value;
    //            }
    //        }
    //    }
    //    public double? ClearedMW { get; set; }
    //}
}