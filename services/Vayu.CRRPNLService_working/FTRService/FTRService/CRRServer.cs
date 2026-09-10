using Vayu.NodeLMPLibrary;
using Vayu.LMP;
using Vayu.CRRCalculationLibrary;
using Vayu.NodePriceLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Globalization;
using Vayu.CommonAccessLibrary;

namespace Vayu.FTRService
{
    /// <summary>
    /// FTRService
    /// </summary>
    [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
   public class CRRServer : ISourceSink
    {
        #region Private Members
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuDBConnection;
        
        /// <summary>
        /// The select node command
        /// </summary>
        private SqlCommand mSelectNodeCommand;
        /// <summary>
        /// The select all ercot source and sink FTR command
        /// </summary>
        private SqlCommand mSelectAllErcotSourceAndSinkFtrCommand;
        /// <summary>
        /// The select all miso source and sink FTR command
        /// </summary>
        private SqlCommand mSelectAllMisoSourceAndSinkFtrCommand;
        /// <summary>
        /// The select all caiso source and sink FTR command
        /// </summary>
        private SqlCommand mSelectAllCaisoSourceAndSinkFtrCommand;
        /// <summary>
        /// The select all SPP source and sink FTR command
        /// </summary>
        private SqlCommand mSelectAllSPPSourceAndSinkFtrCommand;
        /// <summary>
        /// The select all PJM source and sink FTR command
        /// </summary>
        private SqlCommand mSelectAllPJMSourceAndSinkFtrCommand;
        /// <summary>
        /// The select calendar command
        /// </summary>
        private SqlCommand mSelectCalendarCommand;
        /// <summary>
        /// The select period command
        /// </summary>
        private SqlCommand mSelectPeriodCommand;
        /// <summary>
        /// The select period date command
        /// </summary>
        private SqlCommand mSelectPeriodDateCommand;
        private SqlCommand mSelectPriceCommand;
        private SqlCommand mSelectQuarterlyPriceCommand;

        /// The select Ercot COmmand
        /// </summary>

        private SqlCommand mSelectPriceCommandErcot;
        private SqlCommand mSelectPriceCommandErcotOption;
        private SqlCommand mSelectPeakYN_daterang;

        private SqlCommand mSelectPriceCommandErcotOptionMonthly;
        private SqlCommand mSelectPriceCommandPJMOptionMonthly;

        private SqlCommand mSelectQuarterlyPriceCommandErcot;
        private SqlCommand mSelectQuarterlyPriceCommandErcotOption;
        /// <summary>
        /// The period hash
        /// </summary>
        private static Dictionary<int, List<DateTime>> sPeriodHash = new Dictionary<int, List<DateTime>>();
        /// <summary>
        /// The period date hash
        /// </summary>
        private static Dictionary<string, CRRHours> sPeriodDateHash = new Dictionary<string, CRRHours>();
        /// <summary>
        /// The period hour hash
        /// </summary>
        private static Dictionary<int, CRRHours> sPeriodHourHash = new Dictionary<int, CRRHours>();
        /// <summary>
        /// The miso market hours
        /// </summary>
        private static MarketHours sMISOMarketHours = new MarketHours();
        /// <summary>
        /// The PJM market hours
        /// </summary>
        private static MarketHours sPJMMarketHours = new MarketHours();
        /// <summary>
        /// The ercot market hours
        /// </summary>
        private static MarketHours sErcotMarketHours = new MarketHours();
        /// <summary>
        /// The SPP market hours
        /// </summary>
        private static MarketHours sSPPMarketHours = new MarketHours();
        /// <summary>
        /// The ercots market hours
        /// </summary>
        private static MarketHours sErcotsMarketHours = new MarketHours();
        /// <summary>
        /// The caiso market hours
        /// </summary>
        private static MarketHours sCAISOMarketHours = new MarketHours();
        /// <summary>
        /// The node hash
        /// </summary>
        private static ConcurrentDictionary<long, int> sNodeHash = new ConcurrentDictionary<long, int>();
        private string tradingDBString = new VayuDBConnection().GetInstance().GetSqlConnection().ToString();
        private string tradingDBStringERCOT = new VayuDBConnection().GetInstance().GetSqlConnection().ToString();
        Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> yearHash = new Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the FTR.
        /// </summary>
        /// <param name="marketkey">The marketkey.</param>
        /// <param name="account">The account.</param>
        /// <param name="period">The period.</param>
        /// <param name="gethourlypnl">if set to <c>true</c> [get hourly pnl].</param>
        /// <returns>Source Sink</returns>
        public List<SourceSink> GetFTR(int marketkey, string account, DateTime period, bool gethourlypnl = false)
        {
            return GetFTRs(marketkey, new List<string> { account }, period, gethourlypnl);
        }
        /// <summary>
        /// Gets the ftr.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="accounts">The accounts.</param>
        /// <param name="period">The period.</param>
        /// <param name="gethourlypnl">if set to <c>true</c> [gethourlypnl].</param>
        /// <returns></returns>
        public List<SourceSink> GetFTRs(int marketKey, List<string> accounts, DateTime period, bool gethourlypnl = false)
        {
            List<SourceSink> sourceSinkList = new List<SourceSink>();
            if (marketKey == 9)
            {
                string account = "";
                for (int i = 0; i < accounts.Count(); i++)
                {
                    account += (i == accounts.Count() - 1) ? "'" + accounts[i] + "'" : "'" + accounts[i] + "',";
                }
                InitDB();
                try
                {
                    SetMarketKey(marketKey, period);
                    SetPeriodDateHash(marketKey, period);
                    List<long> nodeList = new List<long>();
                    DateTime startDate = DateTime.Now;
                    Console.WriteLine(DateTime.Now + " Get Source Sink " + marketKey + " " + period.ToShortDateString());
                    sourceSinkList = GetSourceSinkList(marketKey, account, period, nodeList);
                    DateTime endDate = DateTime.Now;
                    Console.WriteLine("SourceSink " + (endDate - startDate).Seconds);
                    startDate = DateTime.Now;
                    DownloadDAPrice(period, marketKey, nodeList);
                    endDate = DateTime.Now;
                    Console.WriteLine("Download " + (endDate - startDate).Seconds);
                    startDate = DateTime.Now;
                    Calculation(marketKey, period, sourceSinkList);
                    endDate = DateTime.Now;
                    Console.WriteLine("Calculate " + (endDate - startDate).Seconds);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            else
            {
                string account = "";
                for (int i = 0; i < accounts.Count(); i++)
                {
                    account += (i == accounts.Count() - 1) ? "'" + accounts[i] + "'" : "'" + accounts[i] + "',";
                }
                InitDB();
                try
                {
                    SetMarketKey(marketKey, period);
                    SetPeriodDateHash(marketKey, period);
                    List<long> nodeList = new List<long>();
                    DateTime startDate = DateTime.Now;
                    Console.WriteLine(DateTime.Now + " Get Source Sink " + marketKey + " " + period.ToShortDateString());
                    sourceSinkList = GetSourceSinkList(marketKey, account, period, nodeList);
                    DateTime endDate = DateTime.Now;
                    Console.WriteLine("SourceSink " + (endDate - startDate).Seconds);
                    startDate = DateTime.Now;
                    DownloadDAPrice(period, marketKey, nodeList);
                    endDate = DateTime.Now;
                    Console.WriteLine("Download " + (endDate - startDate).Seconds);
                    startDate = DateTime.Now;
                    Calculation(marketKey, period, sourceSinkList);
                    endDate = DateTime.Now;
                    Console.WriteLine("Calculate " + (endDate - startDate).Seconds);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return sourceSinkList;
        }
        /// <summary>
        /// Gets the ft rs from source sinks.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="sourceSinkList">The source sink list.</param>
        /// <param name="period">The period.</param>
        /// <param name="gethourlypnl">if set to <c>true</c> [gethourlypnl].</param>
        /// <returns></returns>
        public List<SourceSink> GetFTRsFromSourceSinks(int marketKey, List<SourceSink> sourceSinkList, DateTime period, bool gethourlypnl = false)
        {
            InitDB();
            SetMarketKey(marketKey, period);
            List<long> nodeList = new List<long>();
            foreach (SourceSink sourceSink in sourceSinkList)
            {
                if (!nodeList.Contains(sourceSink.SourceNodeId))
                {
                    nodeList.Add(sourceSink.SourceNodeId);
                }
                if (!nodeList.Contains(sourceSink.SinkNodeId))
                {
                    nodeList.Add(sourceSink.SinkNodeId);
                }
            }
            SetPeriodDateHash(marketKey, period);
            DownloadDAPrice(period, marketKey, nodeList);
            Calculation(marketKey, period, sourceSinkList);
            return sourceSinkList; ;
        }
        /// <summary>
        /// Connects this instance.
        /// </summary>
        public void Connect()
        {
            //using (ServiceHost host = new ServiceHost(typeof(FTRServer), new Uri("net.tcp://localhost:7014")))
            using (ServiceHost host = new ServiceHost(typeof(CRRServer), new Uri("net.tcp://localhost:8007")))
            {
                TcpTransportBindingElement transport = new TcpTransportBindingElement();
                transport.TransferMode = TransferMode.Streamed;
                BinaryMessageEncodingBindingElement encoder = new BinaryMessageEncodingBindingElement();
                CustomBinding binding = new CustomBinding(encoder, transport);
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
                myBinding.OpenTimeout = new TimeSpan(0, 12, 0);
                myBinding.SendTimeout = new TimeSpan(0, 12, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                myBinding.TransactionFlow = false;
                myBinding.MaxReceivedMessageSize = int.MaxValue;
                myBinding.MaxBufferPoolSize = int.MaxValue;
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.TransferMode = TransferMode.Buffered;
                myBinding.ReaderQuotas.MaxArrayLength = 5000000;
                host.AddServiceEndpoint(typeof(ISourceSink), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    //Console.WriteLine("Successfully opened port 7014.");
                    Console.WriteLine("Successfully opened port 8007 for CRRPNLService.");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Gets the ercot market calendar with NERC holidays.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns>Calender Hash</returns>
        private Dictionary<DateTime, string> GetCalendar(DateTime period)
        {
            Dictionary<DateTime, string> calendarHash = new Dictionary<DateTime, string>();
            //mSelectCalendarCommand.Parameters["@marketyear"].Value = period.Year;
            VayuDBConnection.Open();
            SqlDataReader Creader = mSelectCalendarCommand.ExecuteReader();
            while (Creader.Read())
            {
                calendarHash.Add(Creader.GetDateTime(0), Creader.GetString(1));
            }
            Creader.Close();
            VayuDBConnection.Close();
            return calendarHash;
        }
        /// <summary>
        /// Sets the market key.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="period">The period.</param>
        private void SetMarketKey(int marketKey, DateTime period)
        {
            MarketHours marketHours = GetPeakOffPeakHours(period, marketKey);
            if (marketKey == 1)
            {
                sPJMMarketHours = marketHours;
            }
            else if (marketKey == 2)
            {
                sMISOMarketHours = marketHours;
            }
            else if (marketKey == 9)
            {
                sErcotMarketHours = marketHours;
            }
            else if (marketKey == 12)
            {
                sSPPMarketHours = marketHours;
            }
            else if (marketKey == 7)
            {
                sCAISOMarketHours = marketHours;
            }
        }
        /// <summary>
        /// Initializes the database.
        /// </summary>
        private void InitDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //Retrieve ERCOT weekend and holidays
            mSelectCalendarCommand = new SqlCommand();
            // mSelectCalendarCommand.CommandText = "SELECT MarketDateTime, PeakYN as flag FROM Vayu_Vayu..MarketTime where MarketYear = @marketyear";

            mSelectCalendarCommand.CommandText = "SELECT MarketDateTime, PeakYN as flag  FROM Vayu..MarketTime where PeakYN = 'w' and DATENAME(DW, MarketDateTime) not in ('Saturday', 'Sunday') order by MarketDateTime";
            //mSelectCalendarCommand.Parameters.AddWithValue("@marketyear", "marketyear");
            mSelectCalendarCommand.Connection = VayuDBConnection;
            //
            mSelectPeriodCommand = new SqlCommand();
            mSelectPeriodCommand.CommandText = "select startdate, enddate, peakhrs, offpeakhrs, 24hrs from period where periodkey = @periodkey";
            mSelectPeriodCommand.Parameters.AddWithValue("@periodkey", "periodkey");
            mSelectPeriodCommand.Connection = VayuDBConnection;
            //
            mSelectPeriodDateCommand = new SqlCommand();
            mSelectPeriodDateCommand.CommandText = "select peakhrs, offpeakhrs, 24hrs from period where startdate = @startdate and enddate = @enddate and marketkey = @marketkey";
            mSelectPeriodDateCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectPeriodDateCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPeriodDateCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPeriodDateCommand.Connection = VayuDBConnection;
            //
            //mSelectAllErcotSourceAndSinkFtrCommand = new SqlCommand();
            //mSelectAllErcotSourceAndSinkFtrCommand.CommandText = "select 0, A.AccountHolder, A.Category, A.HedgeType, A.Class, A.Source, B.NodeKey " +
            //                                "as SourceNodeKey, A.Sink, C.NodeKey as SinkNodeKey, A.StartDate, A.EndDate, A.TimeOfUse, SUM(A.MW), SUM(A.ShadowPrice*A.MW)/SUM(A.MW), B.Zone " +
            //                                "as SourceZone, C.Zone as SinkZone, 'ANNUAL' as Auction " +
            //                                "from ERCOT.CRRAnnualAuctionResults A " +
            //                                "left join Node B on A.Source = B.nodename " +
            //                                "left join Node C on A.Sink = C.nodename " +
            //                                "where StartDate <= @StartDate and EndDate >= @EndDate and AccountHolder in (@participant) " +
            //                                "group by A.AccountHolder, A.Category, A.HedgeType, A.Class, A.Source, B.NodeKey, A.Sink, C.NodeKey, A.StartDate, A.EndDate, A.TimeOfUse, B.Zone, C.Zone " +
            //                                "union " +
            //                                "select 0, M.AccountHolder, M.Category, M.HedgeType, M.Class, M.Source, D.NodeKey " +
            //                                "as SourceNodeKey, M.Sink, E.NodeKey as SinkNodeKey, M.StartDate, M.EndDate, M.TimeOfUse, " +
            //                                "SUM(M.MW), SUM(M.ShadowPrice*M.MW)/SUM(M.MW), D.Zone as SourceZone, E.Zone as SinkZone, 'MONTHLY' as Auction " +
            //                                "from ERCOT.CRRMonthlyAuctionResults M " +
            //                                "left join Node D on M.Source = D.nodename " +
            //                                "left join Node E on M.Sink = E.nodename " +
            //                                "where  StartDate <= @StartDate and EndDate >= @EndDate and AccountHolder in (@participant) " +
            //                                "group by M.AccountHolder, M.Category, M.HedgeType, M.Class, M.Source, D.NodeKey, M.Sink, E.NodeKey, M.StartDate, M.EndDate, M.TimeOfUse, D.Zone, E.Zone";
            //mSelectAllErcotSourceAndSinkFtrCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            //mSelectAllErcotSourceAndSinkFtrCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            //mSelectAllErcotSourceAndSinkFtrCommand.CommandTimeout = 300;
            //mSelectAllErcotSourceAndSinkFtrCommand.Connection = VayuDBConnection;
            //


            mSelectAllErcotSourceAndSinkFtrCommand = new SqlCommand();
            mSelectAllErcotSourceAndSinkFtrCommand.CommandText = "select SourceName, SinkName, TimeUse, 'Monthly', Bid, Hedge, sum(MW) as clearedmw," +
                                                                "sum(MW*ShadowPrice)/sum(MW) as obligationmcp, sum(MW*ShadowPrice)/sum(MW) as optionmcp, 'Varient', min(CRR_ID)," +
                                                                "AccountHolder, Sourcekey, [Sinkkey], C.Zone as SourceZone, D.Zone as SinkZone, B.CRRAuctionName, periodKey, B.CRRAuctionKey,(ShadowPrice) ShadowPrice " +//sum(ShadowPrice)
                                                                "from Vayu..CRRAuctionResults A left join Node C on A.Sourcekey = C.NodeKey " +
                                                                "left join Node D on A.[Sinkkey] = D.NodeKey, Vayu..CRRAuction B " +
                                                                "where A.CRRAuctionKey = B.CRRAuctionKey  " +
                                                                //"and SourceName='BARROW_ALL' and SinkName='NWF_NWF1' and TimeUse='Off-peak' " +
                                                                //"and SourceName='AMO_AMOCO_S1' and SinkName='LZ_HOUSTON' and TimeUse='Off-peak' " +
                                                                "and A.AccountHolder in (@participant) " +//and Hedge='OBL' 
                                                                "and A.CRRAuctionKey in (select CRRAuctionKey from Vayu..CRRAuction where CRRAuctionStartDate between @startdate and @enddate) " +
                                                                "group by SourceName, SinkName, TimeUse, periodkey, Bid, Hedge, AccountHolder, Sourcekey, [Sinkkey], C.Zone, D.Zone, " +
                                                                "B.CRRAuctionName, periodkey, B.CRRAuctionKey,ShadowPrice order by SourceName, SinkName";
            mSelectAllErcotSourceAndSinkFtrCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectAllErcotSourceAndSinkFtrCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectAllErcotSourceAndSinkFtrCommand.CommandTimeout = 300;
            mSelectAllErcotSourceAndSinkFtrCommand.Connection = VayuDBConnection;


            mSelectAllCaisoSourceAndSinkFtrCommand = new SqlCommand();
            mSelectAllCaisoSourceAndSinkFtrCommand.CommandText = "select sourcenode, sinknode, classtype, periodtype, tradetype, hedgetype, sum(clearedmw) as clearedmw, " +
                                                                "sum(clearedmw*clearingprice)/sum(clearedmw) as obligationmcp, sum(clearedmw*clearingprice)/sum(clearedmw) as optionmcp, 'Varient', min(ftrid), " +
                                                                "participant, sourcenodekey, sinknodekey, C.Zone as SourceZone, D.Zone as SinkZone, B.FTRAuctionName, periodKey, B.ftrauctionkey " +
                                                                "from caiso.FTRAuctionResults A left join Node C on A.sourcenodekey = C.NodeKey " +
                                                                "left join Node D on A.sinknodekey = D.NodeKey, caiso.ftrauction B " +
                                                                "where A.ftrauctionkey = B.ftrauctionkey and A.participant in (@participant) " +
                                                                "and A.FTRAuctionKey in (select FTRAuctionKey from caiso.FTRAuction where FTRAuctionStartDate between @startdate and @enddate) " +
                                                                "group by sourcenode, sinknode, classtype, periodtype, tradetype, hedgetype, participant, sourcenodekey, sinknodekey, C.Zone, D.Zone, " +
                                                                "B.FTRAuctionName, periodkey, B.FTRAuctionKey order by sourcenode, sinknode";
            mSelectAllCaisoSourceAndSinkFtrCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectAllCaisoSourceAndSinkFtrCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectAllCaisoSourceAndSinkFtrCommand.CommandTimeout = 300;
            mSelectAllCaisoSourceAndSinkFtrCommand.Connection = VayuDBConnection;
            //
            mSelectAllMisoSourceAndSinkFtrCommand = new SqlCommand();
            mSelectAllMisoSourceAndSinkFtrCommand.CommandText = "select sourcenode, sinknode, classtype, periodtype, tradetype, hedgetype, sum(clearedmw) as clearedmw, " +
                                                                "sum(clearedmw*clearingprice)/sum(clearedmw) as obligationmcp, sum(clearedmw*clearingprice)/sum(clearedmw) as optionmcp, 'Varient', min(ftrid), " +
                                                                "participant, sourcenodekey, sinknodekey, C.Zone as SourceZone, D.Zone as SinkZone, B.FTRAuctionName, periodKey, B.ftrauctionkey " +
                                                                "from MISO.FTRAuctionResults A left join Node C on A.sourcenodekey = C.NodeKey " +
                                                                "left join Node D on A.sinknodekey = D.NodeKey, miso.ftrauction B " +
                                                                "where A.ftrauctionkey = B.ftrauctionkey and A.participant in (@participant) " +
                                                                "and A.FTRAuctionKey in (select FTRAuctionKey from MISO.FTRAuction where FTRAuctionStartDate between @startdate and @enddate) " +
                                                                "group by sourcenode, sinknode, classtype, periodtype, tradetype, hedgetype, participant, sourcenodekey, sinknodekey, C.Zone, D.Zone," +
                                                                "B.FTRAuctionName, periodkey, B.FTRAuctionKey order by sourcenode, sinknode";
            mSelectAllMisoSourceAndSinkFtrCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectAllMisoSourceAndSinkFtrCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectAllMisoSourceAndSinkFtrCommand.CommandTimeout = 300;
            mSelectAllMisoSourceAndSinkFtrCommand.Connection = VayuDBConnection;
            //
            mSelectAllSPPSourceAndSinkFtrCommand = new SqlCommand();
            mSelectAllSPPSourceAndSinkFtrCommand.CommandText = "select sourcenode, sinknode, classtype, periodtype, tradetype, hedgetype, sum(clearedmw) as clearedmw, " +
                                                                "sum(clearedmw*clearingprice)/sum(clearedmw) as obligationmcp, sum(clearedmw*clearingprice)/sum(clearedmw) as optionmcp, 'Varient', min(ftrid), " +
                                                                "participant, sourcenodekey, sinknodekey, C.Zone as SourceZone, D.Zone as SinkZone, B.FTRAuctionName, periodKey, B.ftrauctionkey " +
                                                                "from SPP.FTRAuctionResults A left join Node C on A.sourcenodekey = C.NodeKey " +
                                                                "left join Node D on A.sinknodekey = D.NodeKey, spp.ftrauction B " +
                                                                "where A.ftrauctionkey = B.ftrauctionkey and A.participant in (@participant) " +
                                                                "and A.FTRAuctionKey in (select FTRAuctionKey from SPP.FTRAuction where FTRAuctionStartDate between @startdate and @enddate) " +
                                                                "group by sourcenode, sinknode, classtype, periodtype, tradetype, hedgetype, participant, sourcenodekey, sinknodekey, C.Zone, D.Zone, " +
                                                                "B.FTRAuctionName, periodkey, B.FTRAuctionKey order by sourcenode, sinknode";
            mSelectAllSPPSourceAndSinkFtrCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectAllSPPSourceAndSinkFtrCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectAllSPPSourceAndSinkFtrCommand.CommandTimeout = 300;
            mSelectAllSPPSourceAndSinkFtrCommand.Connection = VayuDBConnection;
            //
            mSelectAllPJMSourceAndSinkFtrCommand = new SqlCommand();
            mSelectAllPJMSourceAndSinkFtrCommand.CommandText = "select sourcenodename, sinknodename, classtype, periodtype, tradetype, hedgetype, sum(clearedmw) as clearedmw, sum(clearedmw*obligationPrice)/sum(clearedmw) " +
                                                                "as obligationmcp, sum(clearedmw*optionprice)/sum(clearedmw) as optionmcp, 'Varient', min(ftrid), participant, sourcenodekey, sinknodekey, N.Zone as SourceZone, M.Zone " +
                                                                "as SinkZone, min(b.FTRAuctionName), periodkey, b.ftrauctionkey from pjm.ftrauctionresults a left join (select distinct(ExternalNodeID), MIN(NodeKey) as NodeKey, MIN(Zone) as Zone " +
                                                                "from Node group by ExternalNodeID) N on a.sourcenodekey = N.ExternalNodeID left join (select distinct(ExternalNodeID), MIN(NodeKey) as NodeKey, " +
                                                                "MIN(Zone) as Zone from Node group by  ExternalNodeID) M on a.sinknodekey = M.ExternalNodeID,  pjm.ftrauction b " +
                                                                "where a.ftrauctionkey = b.ftrauctionkey and A.participant in (@participant) " +
                                                                " and a.FTRAuctionKey in (select FTRAuctionKey from PJM.FTRAuction where FTRAuctionStartDate <= @enddate and FTRAuctionEndDate >= @startdate) " +
                                                                "group by sourcenodename, sinknodename, classtype, periodtype, tradetype, hedgetype, participant, sourcenodekey, sinknodekey, N.Zone, M.Zone, " +
                                                                "b.ftrauctionkey, periodkey order by sourcenodename, sinknodename";
            mSelectAllPJMSourceAndSinkFtrCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectAllPJMSourceAndSinkFtrCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectAllPJMSourceAndSinkFtrCommand.CommandTimeout = 300;
            mSelectAllPJMSourceAndSinkFtrCommand.Connection = VayuDBConnection;
            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select min(nodekey) from node where externalnodeid = @externalnodeid";
            mSelectNodeCommand.Parameters.AddWithValue("@externalnodeid", "externalnodeid");
            mSelectNodeCommand.Connection = VayuDBConnection;

            //
            mSelectPriceCommand = new SqlCommand();
            mSelectPriceCommand.CommandText = "select c.StartDate, b.LMPOnPeak - a.LMPOnPeak, b.LMPOffPeak - a.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey   from pjm.FTRAuctionNodePrice a, pjm.FTRAuctionNodePrice b, period c  " +
                                    "where a.nodekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'monthly' and startdate >= @startdate and enddate <= @enddate) " +
                                    "and b.nodekey = @sink and b.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'monthly' and startdate >= @startdate and enddate <= @enddate) " +
                                    "and a.PeriodKey = b.PeriodKey and a.FTRAuctionKey = b.FTRAuctionKey and a.PeriodKey = c.PeriodKey order by a.periodkey, a.ftrauctionkey";
            mSelectPriceCommand.Parameters.Add("@source", "nodekey");
            mSelectPriceCommand.Parameters.Add("@sink", "sink");
            mSelectPriceCommand.Parameters.Add("@startdate", "startdate");
            mSelectPriceCommand.Parameters.Add("@enddate", "enddate");
            mSelectPriceCommand.Connection = VayuDBConnection;

            //for ercot
            mSelectPriceCommandErcot = new SqlCommand();
            mSelectPriceCommandErcot.CommandText = "select c.StartDate,   a.LMPOnPeak - b.LMPOnPeak, a.LMPOffPeak - b.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey, a.PeakWE - b.PeakWE, c.PEAKWE  from CRRAuctionNodePrice a,CRRAuctionNodePrice b, period c  " +
                              "where a.nodekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 9 and periodtype = 'monthly' and startdate >= @startdate and enddate <= @enddate) " +
                              "and b.nodekey = @sink and b.periodkey in (select PeriodKey from period where marketkey = 9 and periodtype = 'monthly' and startdate >= @startdate and enddate <= @enddate) " +
                              "and a.PeriodKey = b.PeriodKey and a.CRRAuctionkey = b.CRRAuctionkey and a.PeriodKey = c.PeriodKey order by a.periodkey, a.CRRAuctionkey";
            mSelectPriceCommandErcot.Parameters.Add("@source", "Sourcekey");
            mSelectPriceCommandErcot.Parameters.Add("@sink", "Sinkkey");
            mSelectPriceCommandErcot.Parameters.Add("@startdate", "startdate");
            mSelectPriceCommandErcot.Parameters.Add("@enddate", "enddate");
            mSelectPriceCommandErcot.Connection = VayuDBConnection;

            mSelectPriceCommandErcotOption = new SqlCommand();
            mSelectPriceCommandErcotOption.CommandText = "select distinct c.StartDate,  a.PeakWD, a.OffPeak, c.PeakHrs, c.OffPeakHrs, c.periodkey, a.PeakWE, c.PEAKWE   from CRRAuctionOptionPrices a, period c  " +
                              "where a.Sourcekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 9 and periodtype = 'monthly' and startdate >= @startdate and enddate <= @enddate) " +
                              "and a.Sinkkey = @sink " +
                              "and a.PeriodKey = c.PeriodKey";
            mSelectPriceCommandErcotOption.Parameters.Add("@source", "Sourcekey");
            mSelectPriceCommandErcotOption.Parameters.Add("@sink", "Sinkkey");
            mSelectPriceCommandErcotOption.Parameters.Add("@startdate", "startdate");
            mSelectPriceCommandErcotOption.Parameters.Add("@enddate", "enddate");
            mSelectPriceCommandErcotOption.Connection = VayuDBConnection;

            mSelectPriceCommandErcotOptionMonthly = new SqlCommand();
            mSelectPriceCommandErcotOptionMonthly.CommandText = "select c.StartDate,   a.PeakWD, a.OffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey, a.PeakWE, C.PeakWE from CRRAuctionOptionPrice a join  period c "
                                                        + "on a.StartDate = c.StartDate and a.EndDate = c.EndDate "
                                                        + "where a.Sourcekey = @source and a.Sinkkey = @sink  and c.StartDate >= @startdate "
                                                        + "and c.enddate <= @enddate and BidType = 'buy' "
                                                        + " order by c.periodkey, a.CRRAuctionkey";
            mSelectPriceCommandErcotOptionMonthly.Parameters.Add("@source", "Sourcekey");
            mSelectPriceCommandErcotOptionMonthly.Parameters.Add("@sink", "Sinkkey");
            mSelectPriceCommandErcotOptionMonthly.Parameters.Add("@startdate", "startdate");
            mSelectPriceCommandErcotOptionMonthly.Parameters.Add("@enddate", "enddate");
            mSelectPriceCommandErcotOptionMonthly.Connection = VayuDBConnection;

            mSelectPriceCommandPJMOptionMonthly = new SqlCommand();
            mSelectPriceCommandPJMOptionMonthly.CommandText = "select c.StartDate,   a.LMPOnPeak, a.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey "
                                + "from pjm.FTRAuctionOptionPrice a join period c "
                                + "on a.PeriodKey = c.PeriodKey "
                                + " where a.SourceNodeID = @source and a.SinkNodeID = @sink and a.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = "
                                + " 'monthly' and startdate >= @startdate and enddate <= @enddate) "
                                + " order by c.periodkey, a.FTRAuctionKey";
            mSelectPriceCommandPJMOptionMonthly.Parameters.Add("@source", "Sourcekey");
            mSelectPriceCommandPJMOptionMonthly.Parameters.Add("@sink", "Sinkkey");
            mSelectPriceCommandPJMOptionMonthly.Parameters.Add("@startdate", "startdate");
            mSelectPriceCommandPJMOptionMonthly.Parameters.Add("@enddate", "enddate");
            mSelectPriceCommandPJMOptionMonthly.Connection = VayuDBConnection;



        }
        /// <summary>
        /// Sets the period date hash.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="period">The period.</param>
        private void SetPeriodDateHash(int marketKey, DateTime period)
        {
            string key = marketKey + period.ToShortDateString();
            if (!sPeriodDateHash.ContainsKey(key))
            {
                if (VayuDBConnection.State == ConnectionState.Open)
                {
                    VayuDBConnection.Close();
                }
                VayuDBConnection.Open();
                DateTime endDate = period.AddMonths(1).AddDays(-1);
                if (marketKey == 9)
                    mSelectPeriodDateCommand.CommandText = "select peakhrs, offpeakhrs, 24hrs,PeakWE from Vayu..period where startdate = @startdate and enddate = @enddate and marketkey = @marketkey";
                mSelectPeriodDateCommand.Parameters["@marketkey"].Value = marketKey;
                mSelectPeriodDateCommand.Parameters["@startdate"].Value = period;
                mSelectPeriodDateCommand.Parameters["@enddate"].Value = endDate;
                SqlDataReader reader = mSelectPeriodDateCommand.ExecuteReader();
                while (reader.Read())
                {
                    CRRHours ftrHours = new CRRHours();
                    ftrHours.Peak = reader.GetInt32(0);
                    ftrHours.OffPeak = reader.GetInt32(1);
                    ftrHours.Total = reader.GetInt32(2);
                    if (marketKey == 9)
                        ftrHours.PeakWE = reader.GetInt32(3);
                    sPeriodDateHash.Add(key, ftrHours);
                    break;
                }
                reader.Close();
                VayuDBConnection.Close();
            }
        }
        /// <summary>
        /// Gets the node key.
        /// </summary>
        /// <param name="externalNodeId">The external node identifier.</param>
        /// <returns></returns>
        private int GetNodeKey(long externalNodeId)
        {
            if (externalNodeId == 2155502042)
            {

            }
            if (!sNodeHash.ContainsKey(externalNodeId))
            {
                VayuDBConnection.Open();
                mSelectNodeCommand.Parameters["@externalnodeid"].Value = externalNodeId;
                SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
                while (reader.Read())
                {
                    sNodeHash.TryAdd(externalNodeId, (int)reader.GetValue(0));
                }
                reader.Close();
                VayuDBConnection.Close();
            }
            return sNodeHash[externalNodeId];
        }


        /// <summary>
        /// Download Day-ahead price for each node.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="nodeList">The node list.</param>
        private void DownloadDAPrice(DateTime period, int marketKey, List<long> nodeList)
        {
            DateTime firstDay = new DateTime(period.Year, period.Month, 1);
            DateTime lastDay = firstDay.AddMonths(1);
            if (lastDay > DateTime.Today.AddDays(2))
            {
                lastDay = DateTime.Today.AddDays(2);
            }
            int days = (lastDay - firstDay).Days;
            List<Node> rtList = new List<Node>();
            List<Node> daList = new List<Node>();
            foreach (long nodeKey in nodeList)
            {
                Node node = new Node();
                if (marketKey == 1)
                {
                    node.NodeId = GetNodeKey(nodeKey);
                    node.PNodeId = (int)nodeKey;
                }
                else
                {
                    node.NodeId = (int)nodeKey;
                }
                node.Market = marketKey;
                daList.Add(node);
                rtList.Add(node);
            }
            DARTNode.GetDADART(rtList, daList, firstDay, days, false, false, false);
        }
        /// <summary>
        /// Gets the market hours.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <returns>Market Hours</returns>
        private MarketHours GetMarketHours(int marketKey)
        {
            MarketHours marketHours = sPJMMarketHours;
            if (marketKey == 2)
            {
                marketHours = sMISOMarketHours;
            }
            else if (marketKey == 9)
            {
                marketHours = sErcotMarketHours;
            }
            else if (marketKey == 12)
            {
                marketHours = sSPPMarketHours;
            }
            else if (marketKey == 7)
            {
                marketHours = sCAISOMarketHours;
            }
            return marketHours;
        }
        /// <summary>
        /// Gets the ercot peak off peak.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="peakList">The peak list.</param>
        /// <param name="offPeakList">The off peak list.</param>
        private void GetErcotPeakOffPeak(DateTime startDate, DateTime endDate, List<DateTime> peakList, List<DateTime> offPeakList)
        {
            while (startDate < endDate)
            {
                for (int i = 1; i < 25; i++)
                {
                    startDate = startDate.AddHours(1);
                    if (startDate.Hour > 6 && startDate.Hour < 23)
                    {
                        peakList.Add(startDate);
                    }
                    else
                    {
                        offPeakList.Add(startDate);
                    }
                }
            }
        }
        /// <summary>
        /// Calculations the specified market key.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="period">The period.</param>
        /// <param name="sourceSinkList">The source sink list.</param>
        private void Calculation(int marketKey, DateTime period, List<SourceSink> sourceSinkList)
        {
            MarketHours marketHours = GetMarketHours(marketKey);
            Dictionary<DateTime, string> calendarHash = new Dictionary<DateTime, string>();
            if (marketKey == 9)
            {
                calendarHash = GetCalendar(period);
            }
            //Parallel.ForEach(sourceSinkList, sourceSink =>
            foreach (SourceSink sourceSink in sourceSinkList)
            {
                if(sourceSink.Source== "AMO_AMOCO_G1")
                {
                    if(sourceSink.Sink== "TXCTY_CTB")
                    {

                    }

                }
                double cost = 0;
                double sourcePrice = 0;
                double sourceRTPrice = 0;
                double sinkPrice = 0;
                double sinkRTPrice = 0;
                double pnl = 0;
                double da = 0;
                double rt = 0;
                double costMw = 0;
                double totalCost = 0;
                double totalDaPrice = 0;
                double totalRTPrice = 0;
                double totalPnl = 0;
                int hours = 0;               
                sourceSink.MW = Math.Round(sourceSink.MW * 10.0) * 0.1;


                Dictionary<DateTime, double> pnlHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> daHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> rtHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> costHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> dailyPnlHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> dailyDaHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> dailyRtHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> dailyCostHash = new Dictionary<DateTime, double>();
                DateTime firstDay = new DateTime(period.Year, period.Month, 1);
                DateTime lastDay = firstDay.AddMonths(1);
                LMPDatesHelper lmpDatesHelper = new LMPDatesHelper();
                List<DateTime> peakDateTimeList = new List<DateTime>();
                List<DateTime> offPeakDateTimeList = new List<DateTime>();
                CRRHours ftrPeriodKeyHours = null;
                string isPeak = string.Empty;
                if (marketKey == 2 || marketKey == 7 || marketKey == 12 || marketKey == 1 || marketKey == 9)
                {
                    string key = marketKey + period.ToShortDateString();
                    if (!sPeriodDateHash.ContainsKey(key))
                    {
                        Console.WriteLine("Key not found " + key);
                    }
                    else
                        ftrPeriodKeyHours = sPeriodDateHash[key];
                }
                if (marketKey == 9)
                {
                    GetErcotPeakOffPeak(firstDay, lastDay, peakDateTimeList, offPeakDateTimeList);
                }
                else
                {
                    peakDateTimeList = lmpDatesHelper.GetPeakDates(marketKey, firstDay, lastDay, false);
                    offPeakDateTimeList = lmpDatesHelper.GetOffPeakDates(marketKey, firstDay, lastDay, false);
                }
                //peaklist
                if (sourceSink.ClassType.ToUpper() == "ONPEAK" || sourceSink.ClassType.ToUpper() == "PEAKWD" || sourceSink.ClassType.ToUpper() == "PEAKWE")
                {
                    if (marketKey == 9)
                    {
                        //cost = sourceSink.ShadowPrice;
                        costMw = CalculateCost(marketKey, sourceSink, period);
                        cost = costMw;
                        if (sourceSink.ClassType.ToUpper() == "ONPEAK" || sourceSink.ClassType.ToUpper() == "PEAKWD")
                            totalCost = (costMw * sourceSink.MW) * ftrPeriodKeyHours.Peak;
                        else
                            totalCost = (costMw * sourceSink.MW) * ftrPeriodKeyHours.PeakWE;
                    }
                    else
                    {
                        costMw = CalculateCost(marketKey, sourceSink, period);
                        if (ftrPeriodKeyHours != null)
                        {
                            cost = costMw / (double)ftrPeriodKeyHours.Peak;
                        }
                        else
                        {
                            cost = costMw / marketHours.Onpeakhash[period.Month];
                        }
                        totalCost = (costMw * sourceSink.MW);
                    }
                    foreach (DateTime date in peakDateTimeList)
                    {

                        bool isHoliday = calendarHash.ContainsKey(date) ? true : false;
                        if (marketKey == 9)
                        {
                            try
                            {
                                DateTime holidayDate = date.Hour == 0 ? date.Date.AddDays(-1) : date.Date;

                                DayOfWeek satsunday11 = date.DayOfWeek;
                                string Weekendays = satsunday11.ToString();
                                if ((satsunday11 == DayOfWeek.Saturday) || (satsunday11 == DayOfWeek.Sunday))
                                {

                                    if (sourceSink.ClassType.ToUpper() == "PEAKWD")
                                    {


                                        continue;

                                    }
                                }
                                else
                                {
                                    if (sourceSink.ClassType.ToUpper() == "PEAKWE")
                                    {
                                        if (isHoliday)
                                        {

                                        }
                                        else
                                        {
                                            continue;
                                        }

                                    }
                                    else if (sourceSink.ClassType.ToUpper() == "PEAKWD")
                                    {
                                        if (!isHoliday)
                                        {

                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }
                                //
                            }
                            catch (Exception ex)
                            {

                                throw;
                            }

                        }
                        string sourceKey = string.Empty;
                        if (marketKey == 9)
                        {
                            if (!sNodeHash.ContainsKey(sourceSink.SourceNodeId))
                            {
                                // GetNodeKey(sourceSink.SourceNodeId);
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            }
                            else
                            {
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            }
                            string sinkKey = string.Empty;
                            if (!sNodeHash.ContainsKey(sourceSink.SinkNodeId))
                            {
                                // GetNodeKey(sourceSink.SinkNodeId);
                                sinkKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            }
                            else
                            {
                                sinkKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            }
                            if (!DARTNode.dictDALmpHash.ContainsKey(sourceKey) || !DARTNode.dictDALmpHash.ContainsKey(sinkKey))
                            {
                                continue;
                            }
                            sourcePrice = marketKey == 9 ? DARTNode.dictDALmpHash[sourceKey].Price : DARTNode.dictDALmpHash[sourceKey].Congestion;
                            sourceRTPrice = marketKey == 9 ? DARTNode.dictRTLmpHash[sourceKey].Price : DARTNode.dictRTLmpHash[sourceKey].Congestion;
                            sinkPrice = marketKey == 9 ? DARTNode.dictDALmpHash[sinkKey].Price : DARTNode.dictDALmpHash[sinkKey].Congestion;
                            sinkRTPrice = marketKey == 9 ? DARTNode.dictRTLmpHash[sinkKey].Price : DARTNode.dictRTLmpHash[sinkKey].Congestion;
                        }
                        else
                        {
                            if (!sNodeHash.ContainsKey(sourceSink.SourceNodeId))
                            {
                                GetNodeKey(sourceSink.SourceNodeId);
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            }
                            else
                            {
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            }
                            string sinkKey = string.Empty;
                            if (!sNodeHash.ContainsKey(sourceSink.SinkNodeId))
                            {
                                GetNodeKey(sourceSink.SinkNodeId);
                                sinkKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            }
                            else
                            {
                                sinkKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            }

                            if (!DARTNode.dictDALmpHash.ContainsKey(sourceKey) || !DARTNode.dictDALmpHash.ContainsKey(sinkKey))
                            {
                                continue;
                            }
                            sourcePrice = marketKey == 9 ? DARTNode.dictDALmpHash[sourceKey].Price : DARTNode.dictDALmpHash[sourceKey].Congestion;
                            sinkPrice = marketKey == 9 ? DARTNode.dictDALmpHash[sinkKey].Price : DARTNode.dictDALmpHash[sinkKey].Congestion;
                        }
                        if (double.IsNaN(sourcePrice) || double.IsNaN(sinkPrice))
                        {
                            continue;
                        }
                        //if (!isHoliday)
                        {
                            da = (sinkPrice - sourcePrice) * sourceSink.MW;
                            rt = (sinkRTPrice - sourceRTPrice) * sourceSink.MW;
                            pnl = ((sinkPrice - sourcePrice) - cost) * sourceSink.MW;
                            if(double.IsNaN(rt))
                            {
                                rt = 0;
                            }
                        }
                        if (marketKey == 9)
                        {
                            //isPeak = calendarHash[date];
                            //int hour = isPeak == "Y" ? 16 : 8;

                            if (sourceSink.HedgeType == "OPT")
                            {
                                da = Math.Max(0, da);
                                rt = Math.Max(0, rt);
                                double dain = 0;
                                dain = sinkPrice - sourcePrice;
                                //if (!isHoliday)
                                pnl = (Math.Max(0, dain) - sourceSink.ShadowPrice) * sourceSink.MW;
                            }
                        }
                        else
                        {
                            if (sourceSink.HedgeType.ToUpper() == "OPTION" && sinkPrice - sourcePrice < 0)
                                pnl = (0 - cost) * sourceSink.MW;
                            if (sourceSink.HedgeType == "OPT" && (sinkPrice - sourcePrice) < 0)
                                pnl = (0 - sourceSink.ShadowPrice) * sourceSink.MW;
                        }
                        totalDaPrice += da;
                        totalRTPrice += rt;
                        totalPnl += pnl;
                        hours++;
                        if (pnl == 0)
                            pnlHash.Add(date, pnl);
                        daHash.Add(date, da);
                        rtHash.Add(date, rt);
                        costHash.Add(date, cost);
                        DateTime currentDate = DateTime.Parse(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + "/ " + date.Day + "/" + date.Year);
                        if (date.Hour == 0)
                        {
                            currentDate = currentDate.AddDays(-1);
                        }
                        if (!dailyPnlHash.ContainsKey(currentDate))
                        {
                            dailyPnlHash.Add(currentDate, pnl);
                            dailyDaHash.Add(currentDate, da);
                            dailyRtHash.Add(currentDate, rt);
                            dailyCostHash.Add(currentDate, costMw);
                        }
                        else
                        {
                            dailyPnlHash[currentDate] = dailyPnlHash[currentDate] + pnl;
                            dailyDaHash[currentDate] = dailyDaHash[currentDate] + da;
                            dailyRtHash[currentDate] = dailyRtHash[currentDate] + rt;
                            dailyCostHash[currentDate] = dailyCostHash[currentDate] + costMw;
                        }
                    }
                }
                else //ofpeaklist
                {
                    if (marketKey == 9)
                    {
                        //cost = sourceSink.ShadowPrice;
                        costMw = CalculateCost(marketKey, sourceSink, period);
                        cost = costMw;
                        totalCost = (costMw * sourceSink.MW) * ftrPeriodKeyHours.OffPeak;
                    }
                    else
                    {
                        costMw = CalculateCost(marketKey, sourceSink, period);
                        if (ftrPeriodKeyHours != null)
                        {
                            cost = costMw / (double)ftrPeriodKeyHours.OffPeak;
                        }
                        else
                        {
                            cost = costMw / marketHours.Offpeakhash[period.Month];
                        }
                        totalCost = costMw * sourceSink.MW;
                    }
                    foreach (DateTime date in offPeakDateTimeList)
                    {
                        string sourceKey = string.Empty;
                        string sinkKey = string.Empty;
                        if (marketKey == 9)
                        {
                            if (!sNodeHash.ContainsKey(sourceSink.SourceNodeId))
                            {
                                // GetNodeKey(sourceSink.SourceNodeId);
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            }
                            else
                            {
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            }
                            if (!sNodeHash.ContainsKey(sourceSink.SinkNodeId))
                            {
                                // GetNodeKey(sourceSink.SinkNodeId);
                                sinkKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            }
                            else
                            {
                                sinkKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            }
                            if (!DARTNode.dictDALmpHash.ContainsKey(sourceKey) || !DARTNode.dictDALmpHash.ContainsKey(sinkKey))
                            {
                                continue;
                            }
                            sourcePrice = marketKey == 9 ? DARTNode.dictDALmpHash[sourceKey].Price : DARTNode.dictDALmpHash[sourceKey].Congestion;
                            sinkPrice = marketKey == 9 ? DARTNode.dictDALmpHash[sinkKey].Price : DARTNode.dictDALmpHash[sinkKey].Congestion;
                            sourceRTPrice = marketKey == 9 ? DARTNode.dictRTLmpHash[sourceKey].Price : DARTNode.dictRTLmpHash[sourceKey].Congestion;
                            sinkRTPrice = marketKey == 9 ? DARTNode.dictRTLmpHash[sinkKey].Price : DARTNode.dictRTLmpHash[sinkKey].Congestion;
                        }
                        else
                        {
                            if (sNodeHash.ContainsKey(sourceSink.SourceNodeId))
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            else
                            {
                                GetNodeKey(sourceSink.SourceNodeId);
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SourceNodeId].ToString() : date.ToString() + sourceSink.SourceNodeId.ToString();
                            }
                            if (sNodeHash.ContainsKey(sourceSink.SinkNodeId))
                                sinkKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            else
                            {
                                GetNodeKey(sourceSink.SinkNodeId);
                                sourceKey = marketKey == 1 ? date.ToString() + sNodeHash[sourceSink.SinkNodeId].ToString() : date.ToString() + sourceSink.SinkNodeId.ToString();
                            }
                            if (!DARTNode.dictDALmpHash.ContainsKey(sourceKey) || !DARTNode.dictDALmpHash.ContainsKey(sinkKey))
                            {
                                continue;
                            }
                            sourcePrice = marketKey == 9 ? DARTNode.dictDALmpHash[sourceKey].Price : DARTNode.dictDALmpHash[sourceKey].Congestion;
                            sinkPrice = marketKey == 9 ? DARTNode.dictDALmpHash[sinkKey].Price : DARTNode.dictDALmpHash[sinkKey].Congestion;

                        }
                        if (double.IsNaN(sourcePrice) || double.IsNaN(sinkPrice))
                        {
                            continue;
                        }
                        da = (sinkPrice - sourcePrice) * sourceSink.MW;
                        rt = (sinkRTPrice - sourceRTPrice) * sourceSink.MW;
                        if(double.IsNaN(rt))
                        {
                            rt = 0;
                        }
                        pnl = ((sinkPrice - sourcePrice) - cost) * sourceSink.MW;
                        if (marketKey == 9)
                        {
                            if (sourceSink.HedgeType == "OPT")
                            {

                                da = Math.Max(0, da);
                                rt = Math.Max(0, rt);
                                double dain = 0;
                                dain = sinkPrice - sourcePrice;
                                pnl = (Math.Max(0, dain) - sourceSink.ShadowPrice) * sourceSink.MW;
                            }


                        }
                        else
                        {
                            if (sourceSink.HedgeType.ToUpper() == "OPTION" && sinkPrice - sourcePrice < 0)
                            {
                                pnl = (0 - cost) * sourceSink.MW;
                            }
                            if (sourceSink.HedgeType == "OPT" && (sinkPrice - sourcePrice) < 0)
                            {
                                pnl = (0 - sourceSink.ShadowPrice) * sourceSink.MW;
                            }
                        }
                        totalDaPrice += da;
                        totalRTPrice += rt;
                        totalPnl += pnl;
                        hours++;
                        pnlHash.Add(date, pnl);
                        daHash.Add(date, da);
                        rtHash.Add(date, rt);
                        costHash.Add(date, cost);
                        DateTime currentDate = DateTime.Parse(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + "/ " + date.Day + "/" + date.Year);

                        if (currentDate.ToString()== "12/14/2021 12:00:00 AM")
                        {
                           
                        }
                        if (date.Hour == 0)
                        {
                            currentDate = currentDate.AddDays(-1);
                        }
                        if (!dailyPnlHash.ContainsKey(currentDate))
                        {
                            dailyPnlHash.Add(currentDate, pnl);
                            dailyDaHash.Add(currentDate, da);
                            dailyRtHash.Add(currentDate, rt);
                            dailyCostHash.Add(currentDate, costMw);
                        }
                        else
                        {
                            dailyPnlHash[currentDate] = dailyPnlHash[currentDate] + pnl;
                            dailyDaHash[currentDate] = dailyDaHash[currentDate] + da;
                            dailyRtHash[currentDate] = dailyRtHash[currentDate] + rt;
                            dailyCostHash[currentDate] = dailyCostHash[currentDate] + costMw;
                        }
                    }
                }
                sourceSink.PNL = pnlHash;
                sourceSink.DAPrice = daHash;
                sourceSink.RTPrice = rtHash;
                sourceSink.Cost = costHash;
                sourceSink.Hours = hours;
                sourceSink.Costmonthlytotal = totalCost;
                sourceSink.DAmonthlytotal = totalDaPrice;
                sourceSink.RTmonthlytotal = totalRTPrice;
                sourceSink.PNLmonthlytotal = totalPnl;
                //daily value
                sourceSink.dailyPNL = dailyPnlHash;
                sourceSink.dailyDAPrice = dailyDaHash;
                sourceSink.dailyRTPrice = dailyRtHash;
                sourceSink.dailyCost = dailyCostHash;
                //});
            }
        }
        /// <summary>
        /// Calculates the cost.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="period">The period.</param>
        /// <returns>Cost</returns>
        private double CalculateCost(int marketKey, SourceSink sourceSink, DateTime period)
        {
            MarketHours marketHours = GetMarketHours(marketKey);
            double newCost = 0;
            double cost = sourceSink.Obligation;
            if (sourceSink.HedgeType.ToUpper() == "OPTION" || sourceSink.HedgeType.ToUpper() == "OPT")
            {
                cost = sourceSink.Option;
            }
            if (marketKey == 2 || marketKey == 12 || marketKey == 7 || marketKey == 1 || marketKey == 9)
            {
                string key = marketKey + period.ToShortDateString();
                CRRHours ftrPeriodHours = sPeriodDateHash[key];
                CRRHours ftrPeriodKeyHours = sPeriodHourHash[sourceSink.PeriodKey];
                if (sourceSink.ClassType.ToUpper() == "ONPEAK" || sourceSink.ClassType.ToUpper() == "PEAKWD")
                {
                    newCost = cost * ((double)ftrPeriodHours.Peak / (double)ftrPeriodKeyHours.Peak);
                }
                else if (sourceSink.ClassType.ToUpper() == "24H")
                {
                    newCost = cost * (((double)ftrPeriodHours.Peak + (double)ftrPeriodHours.OffPeak) / ((double)ftrPeriodKeyHours.Peak + (double)ftrPeriodKeyHours.OffPeak));
                }
                //else if( sourceSink.ClassType.ToUpper() == "PEAKWE")
                //{

                //}
                else
                {
                    newCost = cost * ((double)ftrPeriodHours.OffPeak / (double)ftrPeriodKeyHours.OffPeak);
                }
                return newCost;
            }
            return cost;
        }
        /// <summary>
        /// Gets the source sink list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="account">The account.</param>
        /// <param name="period">The period.</param>
        /// <param name="nodeList">The node list.</param>
        /// <returns>Source Sink List</returns>
        private List<SourceSink> GetSourceSinkList(int marketKey, string account, DateTime period, List<long> nodeList)
        {
            List<SourceSink> sourceSinkList = new List<SourceSink>();
            if (marketKey == 9)
            {

                int ftrMonth = period.Month;
                string ftrMonthString = period.ToString("M").ToUpper().Substring(0, 3);
                SqlDataReader reader = null;
                VayuDBConnection.Open();
                int count = marketKey == 1 ? 1 : 1;

                for (int i = 0; i < count; i++)
                {
                    if (marketKey == 9)
                    {
                        DateTime firstDay = new DateTime(period.Year, period.Month, 1);
                        DateTime lastDay = firstDay.AddMonths(1);
                        mSelectAllErcotSourceAndSinkFtrCommand.Parameters["@StartDate"].Value = firstDay;
                        mSelectAllErcotSourceAndSinkFtrCommand.Parameters["@EndDate"].Value = firstDay;
                        mSelectAllErcotSourceAndSinkFtrCommand.CommandText = mSelectAllErcotSourceAndSinkFtrCommand.CommandText.Replace("@participant", account);
                        reader = mSelectAllErcotSourceAndSinkFtrCommand.ExecuteReader();

                    }

                    while (reader.Read())
                    {
                        SourceSink sourcesink = new SourceSink();
                        // string value = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        sourcesink.Month = period.ToString("y");
                        if (marketKey == 9)
                        {

                            sourcesink.Source = reader.GetString(0).Trim();
                            sourcesink.Sink = reader.GetString(1).Trim();
                            sourcesink.ClassType = reader.GetString(2).Trim();
                            sourcesink.PeriodType = reader.GetString(3).Trim();
                            sourcesink.TradeType = reader.GetString(4).Trim();
                            sourcesink.HedgeType = reader.GetString(5).Trim();
                            sourcesink.MW = (sourcesink.TradeType.ToUpper() == "SELL") ? -((double)reader.GetDecimal(6)) : (double)reader.GetDecimal(6);    //invert the mw sign if it is a SELL                          
                            sourcesink.Obligation = reader.IsDBNull(7) ? 0 : (double)reader.GetDecimal(7);
                            //   sourcesink.Option = (!reader.IsDBNull(8)) ? (double)reader.GetDecimal(8) : 0;
                            if (sourcesink.HedgeType.ToUpper() == "OPT")
                            {
                                sourcesink.Option = (!reader.IsDBNull(8)) ? (double)reader.GetDecimal(8) : 0;
                                if (sourcesink.Option >= 0 && sourcesink.Option < 0.01)
                                {
                                    sourcesink.Option = 0.01;

                                }
                                else
                                {
                                    sourcesink.Option = (!reader.IsDBNull(8)) ? (double)reader.GetDecimal(8) : 0;
                                }

                            }

                            sourcesink.Period = reader.GetString(9).Trim();
                            sourcesink.Ftrid = Convert.ToInt64(reader.GetValue(10));
                            //sourcesink.Ftrid = (long)(reader.GetValue(10));
                            sourcesink.Participant = reader.GetString(11).Trim();
                            sourcesink.SourceNodeId = Convert.ToInt64(reader.GetValue(12));
                            sourcesink.SinkNodeId = Convert.ToInt64(reader.GetValue(13));
                            sourcesink.ShadowPrice = reader.IsDBNull(19) ? 0 : (double)reader.GetDecimal(19);
                            sourcesink.PeriodKey = Convert.ToInt32(reader.GetValue(17));
                            //

                            if (!sPeriodHash.ContainsKey(sourcesink.PeriodKey))
                            {
                                //VayuDBConnection.Open();
                                if (marketKey == 9)
                                    mSelectPeriodCommand.CommandText = "select startdate, enddate, peakhrs, offpeakhrs, 24hrs from Vayu..period where periodkey = @periodkey";
                                mSelectPeriodCommand.Parameters["@periodkey"].Value = sourcesink.PeriodKey;
                                SqlDataReader reader1 = mSelectPeriodCommand.ExecuteReader();
                                while (reader1.Read())
                                {
                                    List<DateTime> monthList = new List<DateTime>();
                                    DateTime startDate = reader1.GetDateTime(0);
                                    DateTime endDate = reader1.GetDateTime(1);
                                    CRRHours ftrHours = new CRRHours();
                                    ftrHours.Peak = reader1.GetInt32(2);
                                    ftrHours.OffPeak = reader1.GetInt32(3);
                                    ftrHours.Total = reader1.GetInt32(4);
                                    sPeriodHourHash.Add(sourcesink.PeriodKey, ftrHours);
                                    while (startDate < endDate)
                                    {
                                        if (!monthList.Contains(startDate))
                                        {
                                            monthList.Add(startDate);
                                        }
                                        startDate = startDate.AddMonths(1);
                                    }
                                    sPeriodHash.Add(sourcesink.PeriodKey, monthList);
                                    break;
                                }
                                reader1.Close();
                               // VayuDBConnection.Close();
                            }
                            if (!sPeriodHash.ContainsKey(sourcesink.PeriodKey) || !sPeriodHash[sourcesink.PeriodKey].Contains(period))
                            {
                                continue;
                            }
                            //
                        }

                        sourcesink.SourceZone = (reader.IsDBNull(14)) ? "" : reader.GetString(14);
                        sourcesink.SinkZone = (reader.IsDBNull(15)) ? "" : reader.GetString(15);
                        sourcesink.AuctionName = reader.GetString(16).Trim();
                        if (!nodeList.Contains(sourcesink.SourceNodeId))
                        {
                            nodeList.Add(sourcesink.SourceNodeId);
                        }
                        if (!nodeList.Contains(sourcesink.SinkNodeId))
                        {
                            nodeList.Add(sourcesink.SinkNodeId);
                        }
                        sourceSinkList.Add(sourcesink);
                    }
                    reader.Close();
                }

                VayuDBConnection.Close();
                // return sourceSinkList;
            }
            else
            {
                int ftrYear = (period < DateTime.Parse("6/1/" + period.Year)) ? period.Year - 1 : period.Year;
                if (marketKey == 7)
                {
                    ftrYear = period.Year;
                }
                int ftrMonth = period.Month;
                string ftrMonthString = period.ToString("M").ToUpper().Substring(0, 3);
                SqlDataReader reader = null;
                VayuDBConnection.Open();
                int count = marketKey == 1 ? 1 : 1;
                // List<SourceSink> sourceSinkList = new List<SourceSink>();
                for (int i = 0; i < count; i++)
                {
                    if (marketKey == 1)
                    {
                        //DateTime firstDay = new DateTime(period.Year, period.Month, 1);
                        mSelectAllPJMSourceAndSinkFtrCommand.Parameters["@startdate"].Value = DateTime.Parse("6/1/" + ftrYear);
                        mSelectAllPJMSourceAndSinkFtrCommand.Parameters["@enddate"].Value = DateTime.Parse("May/31/" + (ftrYear + 1));
                        mSelectAllPJMSourceAndSinkFtrCommand.CommandText = mSelectAllPJMSourceAndSinkFtrCommand.CommandText.Replace("@participant", account);
                        reader = mSelectAllPJMSourceAndSinkFtrCommand.ExecuteReader();
                    }
                    else if (marketKey == 2)
                    {
                        mSelectAllMisoSourceAndSinkFtrCommand.Parameters["@startdate"].Value = DateTime.Parse("6/1/" + ftrYear);
                        mSelectAllMisoSourceAndSinkFtrCommand.Parameters["@enddate"].Value = DateTime.Parse("5/31/" + (ftrYear + 1));
                        mSelectAllMisoSourceAndSinkFtrCommand.CommandText = mSelectAllMisoSourceAndSinkFtrCommand.CommandText.Replace("@participant", account);
                        reader = mSelectAllMisoSourceAndSinkFtrCommand.ExecuteReader();
                    }
                    else if (marketKey == 9)
                    {
                        //DateTime firstDay = new DateTime(period.Year, period.Month, 1);
                        //DateTime lastDay = firstDay.AddMonths(1);
                        //mSelectAllErcotSourceAndSinkFtrCommand.Parameters["@StartDate"].Value = firstDay;
                        //mSelectAllErcotSourceAndSinkFtrCommand.Parameters["@EndDate"].Value = firstDay;
                        //mSelectAllErcotSourceAndSinkFtrCommand.CommandText = mSelectAllErcotSourceAndSinkFtrCommand.CommandText.Replace("@participant", account);
                        //reader = mSelectAllErcotSourceAndSinkFtrCommand.ExecuteReader();

                    }
                    else if (marketKey == 12)
                    {
                        mSelectAllSPPSourceAndSinkFtrCommand.Parameters["@startdate"].Value = DateTime.Parse("6/1/" + ftrYear);
                        mSelectAllSPPSourceAndSinkFtrCommand.Parameters["@enddate"].Value = DateTime.Parse("5/31/" + (ftrYear + 1));
                        mSelectAllSPPSourceAndSinkFtrCommand.CommandText = mSelectAllSPPSourceAndSinkFtrCommand.CommandText.Replace("@participant", account);
                        reader = mSelectAllSPPSourceAndSinkFtrCommand.ExecuteReader();
                    }
                    else if (marketKey == 7)
                    {
                        mSelectAllCaisoSourceAndSinkFtrCommand.Parameters["@startdate"].Value = DateTime.Parse("1/1/" + ftrYear);
                        mSelectAllCaisoSourceAndSinkFtrCommand.Parameters["@enddate"].Value = DateTime.Parse("12/31/" + ftrYear);
                        mSelectAllCaisoSourceAndSinkFtrCommand.CommandText = mSelectAllCaisoSourceAndSinkFtrCommand.CommandText.Replace("@participant", account);
                        reader = mSelectAllCaisoSourceAndSinkFtrCommand.ExecuteReader();
                    }
                    while (reader.Read())
                    {
                        SourceSink sourcesink = new SourceSink();
                        string value = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        if (marketKey == 2 || marketKey == 7 || marketKey == 12 || marketKey == 1)
                        {
                            sourcesink.PeriodKey = reader.IsDBNull(17) ? 0 : reader.GetInt32(17);
                            object obj = reader.GetFieldType(18);
                            sourcesink.AuctionKey = reader.IsDBNull(18) ? 0 : (int)reader.GetDecimal(18);
                            if (sourcesink.PeriodKey == 0)
                            {
                                continue;
                            }
                            if (!sPeriodHash.ContainsKey(sourcesink.PeriodKey))
                            {
                                VayuDBConnection.Open();
                                mSelectPeriodCommand.Parameters["@periodkey"].Value = sourcesink.PeriodKey;
                                SqlDataReader reader1 = mSelectPeriodCommand.ExecuteReader();
                                while (reader1.Read())
                                {
                                    List<DateTime> monthList = new List<DateTime>();
                                    DateTime startDate = reader1.GetDateTime(0);
                                    DateTime endDate = reader1.GetDateTime(1);
                                    CRRHours ftrHours = new CRRHours();
                                    ftrHours.Peak = reader1.GetInt32(2);
                                    ftrHours.OffPeak = reader1.GetInt32(3);
                                    ftrHours.Total = reader1.GetInt32(4);
                                    sPeriodHourHash.Add(sourcesink.PeriodKey, ftrHours);
                                    while (startDate < endDate)
                                    {
                                        if (!monthList.Contains(startDate))
                                        {
                                            monthList.Add(startDate);
                                        }
                                        startDate = startDate.AddMonths(1);
                                    }
                                    sPeriodHash.Add(sourcesink.PeriodKey, monthList);
                                    break;
                                }
                                reader1.Close();
                                VayuDBConnection.Close();
                            }
                            if (!sPeriodHash.ContainsKey(sourcesink.PeriodKey) || !sPeriodHash[sourcesink.PeriodKey].Contains(period))
                            {
                                continue;
                            }
                        }
                        sourcesink.Month = period.ToString("y");
                        if (marketKey == 9)
                        {
                            //sourcesink.CRR_ID = Convert.ToInt64(reader.GetValue(0));
                            //sourcesink.Participant = reader.GetString(1).Trim();
                            //sourcesink.Category = reader.GetString(2).Trim();
                            //sourcesink.PeriodType = "";
                            //sourcesink.HedgeType = value.Trim();
                            //sourcesink.TradeType = reader.GetString(4).Trim();
                            //sourcesink.Source = reader.GetString(5).Trim();
                            //sourcesink.SourceNodeId = (int)reader.GetDecimal(6);
                            //sourcesink.Sink = reader.GetString(7).Trim();
                            //sourcesink.SinkNodeId = (int)reader.GetDecimal(8);
                            //sourcesink.StartDate = reader.GetDateTime(9);
                            //sourcesink.EndDate = reader.GetDateTime(10);
                            //sourcesink.TimeofUse = reader.GetString(11).Trim();
                            //sourcesink.ClassType = reader.GetString(11).Trim();
                            //sourcesink.MW = reader.GetDouble(12);
                            //if (sourcesink.TradeType.ToUpper() == "SELL")
                            //{
                            //    sourcesink.MW = -reader.GetDouble(12);
                            //}
                            //sourcesink.ShadowPrice = reader.GetDouble(13);


                        }
                        else
                        {
                            sourcesink.Source = reader.GetString(0).Trim();
                            sourcesink.Sink = reader.GetString(1).Trim();
                            sourcesink.ClassType = reader.GetString(2).Trim();
                            if (sourcesink.ClassType.ToUpper() == "PEAK")
                            {
                                sourcesink.ClassType = "OnPeak";
                            }
                            sourcesink.PeriodType = reader.GetString(3).Trim();
                            sourcesink.TradeType = reader.GetString(4).Trim();
                            sourcesink.HedgeType = reader.GetString(5).Trim();
                            sourcesink.MW = (sourcesink.TradeType.ToUpper() == "SELL") ? -((double)reader.GetDecimal(6)) : (double)reader.GetDecimal(6);    //invert the mw sign if it is a SELL                          
                            sourcesink.Obligation = reader.IsDBNull(7) ? 0 : (double)reader.GetDecimal(7);
                            sourcesink.Option = (!reader.IsDBNull(8)) ? (double)reader.GetDecimal(8) : 0;
                            sourcesink.Period = reader.GetString(9).Trim();
                            sourcesink.Ftrid = Convert.ToInt64(reader.GetValue(10));
                            //sourcesink.Ftrid = (long)(reader.GetValue(10));
                            sourcesink.Participant = reader.GetString(11).Trim();
                            sourcesink.SourceNodeId = reader.IsDBNull(12) ? 0 : (long)reader.GetDecimal(12);
                            sourcesink.SinkNodeId = reader.IsDBNull(13) ? 0 : (long)reader.GetDecimal(13);
                        }
                        sourcesink.SourceZone = (reader.IsDBNull(14)) ? "" : reader.GetString(14);
                        sourcesink.SinkZone = (reader.IsDBNull(15)) ? "" : reader.GetString(15);
                        sourcesink.AuctionName = reader.GetString(16).Trim();
                        if (!nodeList.Contains(sourcesink.SourceNodeId))
                        {
                            nodeList.Add(sourcesink.SourceNodeId);
                        }
                        if (!nodeList.Contains(sourcesink.SinkNodeId))
                        {
                            nodeList.Add(sourcesink.SinkNodeId);
                        }
                        sourceSinkList.Add(sourcesink);
                    }
                    reader.Close();
                }

                VayuDBConnection.Close();
                // return sourceSinkList;
            }
            return sourceSinkList;
        }
        /// <summary>
        /// Gets the peak off peak hours.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns>Saved Market Hours</returns>
        private MarketHours GetPeakOffPeakHours(DateTime period, int marketKey)
        {
            MarketHours savedMarketHours = new MarketHours();
            LMPDatesHelper lmpDatesHelper = new LMPDatesHelper();
            int ftrYear = (period < DateTime.Parse("6/1/" + period.Year)) ? period.Year - 1 : period.Year;
            if (marketKey == 9)
            {
                ftrYear = period.Year;
            }
            if (savedMarketHours == null || savedMarketHours.Year != ftrYear)
            {
                savedMarketHours = new MarketHours();
                int monthyTotal = 0;
                int yearlyTotalOffeak = 0;
                int YearlyTotalPeak = 0;
                int quarterlyTotalOffPeak = 0;
                int quarterlyTotalPeak = 0;
                int j = 1;
                MarketHours marketHours = new MarketHours();
                Dictionary<int, int> monthlyHoursOffPeakHash = new Dictionary<int, int>();
                Dictionary<int, int> monthlyHoursPeakHash = new Dictionary<int, int>();
                Dictionary<int, int> quarterlyOffPeakHash = new Dictionary<int, int>();
                Dictionary<int, int> quarterlyPeakHash = new Dictionary<int, int>();

                if(VayuDBConnection.State==ConnectionState.Closed)
                   VayuDBConnection.Open();

                for (int i = 1; i <= 12; i++)
                {
                    DateTime startTime = marketKey == 9 || marketKey == 7 ? DateTime.Parse("1/1/" + ftrYear).AddHours(1).AddMonths(i - 1) : DateTime.Parse("6/1/" + ftrYear).AddHours(1).AddMonths(i - 1);
                    DateTime endTime = marketKey == 9 || marketKey == 7 ? DateTime.Parse("1/1/" + ftrYear).AddMonths(i) : DateTime.Parse("6/1/" + ftrYear).AddMonths(i);
                    //counting offpeak hours
                    monthyTotal = lmpDatesHelper.GetOffPeakDates(marketKey, startTime, endTime, false).Count;
                    monthlyHoursOffPeakHash.Add(startTime.Month, monthyTotal);
                    yearlyTotalOffeak += monthyTotal;
                    quarterlyTotalOffPeak += monthyTotal;
                    //counting onpeak hours
                    monthyTotal = lmpDatesHelper.GetPeakDates(marketKey, startTime, endTime, false).Count;
                    monthlyHoursPeakHash.Add(startTime.Month, monthyTotal);
                    YearlyTotalPeak += monthyTotal;
                    quarterlyTotalPeak += monthyTotal;
                    //calculate the total hours for each quarter
                    if (i != 0 && (i % 3) == 0)
                    {
                        quarterlyOffPeakHash.Add(j, quarterlyTotalOffPeak);
                        quarterlyPeakHash.Add(j, quarterlyTotalPeak);
                        quarterlyTotalOffPeak = 0;
                        quarterlyTotalPeak = 0;
                        j++;
                    }
                }
                marketHours.TotalHoursOffpeak = yearlyTotalOffeak;
                marketHours.Offpeakhash = monthlyHoursOffPeakHash;
                marketHours.TotalHoursOnpeak = YearlyTotalPeak;
                marketHours.Onpeakhash = monthlyHoursPeakHash;
                marketHours.OffpeakQuarter = quarterlyOffPeakHash;
                marketHours.OnpeakQuarter = quarterlyPeakHash;
                marketHours.Year = ftrYear;
                //market hash
                savedMarketHours = marketHours;
                VayuDBConnection.Close();
            }
            return savedMarketHours;
        }
        private Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> getFtrPjmData(int marketkey, long SourceId, long SinkId, DateTime startDate, DateTime endDate, string HedgeType, int ftrQuarterMonth, string auctionType)
        {
            List<int> yearList = new List<int>();
            InitDB();
            int sourcenodekey = GetNodeKey(SourceId);
            int sinknodekey = GetNodeKey(SinkId);
            //if (state == null)
            //{

            //}
            // return;
            VayuDBConnection.Open();
            Dictionary<string, DailyValues> priceHash = new Dictionary<string, DailyValues>();
            SqlDataReader reader;
            if (HedgeType == "OBL")
            {
                if (ftrQuarterMonth != 0)
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }
                    mSelectQuarterlyPriceCommand = new SqlCommand();
                    foreach (int year in yearList)
                    {
                        mSelectQuarterlyPriceCommand.CommandText = " select c.startdate ,  si.LMPOnPeak - b.LMPOnPeak , si.LMPOffPeak - b.LMPOffPeak  , c.PeakHrs , c.OffPeakHrs , c.PeriodKey  from pjm.FTRAuction a, pjm.FTRAuctionNodePrice b " +
                            " join period c on b.PeriodKey = c.PeriodKey join pjm.FTRAuctionNodePrice si on  b.FTRAuctionKey = si.FTRAuctionKey and b.PeriodKey = si.PeriodKey " +
                            " where  b.periodkey = (select periodkey from period where startdate = '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal'  " +
                            " and marketkey = 1) and a.FTRAuctionKey = b.FTRAuctionKey and b.NodeKey  =  " + sourcenodekey.ToString() + " and  si.NodeKey = " + sinknodekey.ToString() + " and " +
                            " a.FTRAuctionKey = (select max(a.FtrAuctionkey) from pjm.FTRAuction a, pjm.FTRAuctionNodePrice b where  b.periodkey = " +
                            " (select periodkey from period where startdate =  '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal' and marketkey = 1) and a.FTRAuctionKey = b.FTRAuctionKey) ";
                        mSelectQuarterlyPriceCommand.Connection = VayuDBConnection;
                        SqlDataReader rdr = mSelectQuarterlyPriceCommand.ExecuteReader();
                        while (rdr.Read())
                        {
                            DateTime marketDateTime = rdr.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)rdr.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)rdr.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(rdr.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(rdr.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(rdr.GetValue(5));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            string date1 = string.Empty; string date2 = string.Empty;
                            if (marketDateTime.Month != 12)
                            {
                                date1 = marketDateTime.Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            else
                            {
                                date1 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                            if (!priceHash.ContainsKey(date1))
                                priceHash.Add(date1, dailyValues);
                            if (!priceHash.ContainsKey(date2))
                                priceHash.Add(date2, dailyValues);
                        }
                        rdr.Close();
                    }

                }
                else if (auctionType == "Annual")
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }

                    foreach (int year in yearList)
                    {
                        mSelectPriceCommand.CommandText = " select c.StartDate, b.LMPOnPeak - a.LMPOnPeak, b.LMPOffPeak - a.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey from pjm.FTRAuctionNodePrice a, pjm.FTRAuctionNodePrice b, period c " +
                       " where a.nodekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'Annual' and startdate >= @startdate and enddate <= @enddate)  and b.nodekey = @sink and b.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'Annual' " +
                       " and startdate >= @startdate and enddate <= @enddate)  and a.PeriodKey = b.PeriodKey and a.FTRAuctionKey = b.FTRAuctionKey and a.PeriodKey = c.PeriodKey and a.FTRAuctionKey in(select MAX(FTRAuctionKey) from pjm.FTRAuction  where FTRAuctionStartDate = @startdate and FTRAuctionEndDate = @enddate  " +
                       " ) order by a.periodkey, a.FTRAuctionKey ";
                        mSelectPriceCommand.Parameters["@source"].Value = sourcenodekey;
                        mSelectPriceCommand.Parameters["@sink"].Value = sinknodekey;
                        mSelectPriceCommand.Parameters["@startdate"].Value = DateTime.Parse(year + "/" + "06" + "/" + "01");
                        mSelectPriceCommand.Parameters["@enddate"].Value = DateTime.Parse((year + 1) + "/" + "05" + "/" + "31");
                        reader = mSelectPriceCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime marketDateTime = reader.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)reader.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)reader.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                        }
                        reader.Close();
                    }
                }

                else
                {
                    DateTime newEndDate = endDate;
                    if ((endDate.Month == DateTime.Today.Month) && (endDate.Day <= 29))
                        newEndDate = endDate.AddMonths(1);

                    mSelectPriceCommand.Parameters["@source"].Value = sourcenodekey;
                    mSelectPriceCommand.Parameters["@sink"].Value = sinknodekey;
                    mSelectPriceCommand.Parameters["@startdate"].Value = startDate;
                    mSelectPriceCommand.Parameters["@enddate"].Value = newEndDate;
                   // mSelectPriceCommand.Parameters["@enddate"].Value = endDate.AddMonths(1);

                    reader = mSelectPriceCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime marketDateTime = reader.GetDateTime(0);
                        DailyValues dailyValues = new DailyValues();
                        dailyValues.PricePeak = (double?)reader.GetDecimal(1);
                        dailyValues.PriceOffPeak = (double?)reader.GetDecimal(2);
                        dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3)); ;
                        dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                        dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                        string date = marketDateTime.Year + "-" + marketDateTime.Month;
                        if (priceHash.ContainsKey(date))
                        {
                            priceHash.Remove(date);
                        }
                        priceHash.Add(date, dailyValues);
                    }
                    reader.Close();
                }

                VayuDBConnection.Close();

                SqlConnection cmd = new SqlConnection(tradingDBString);
                SqlCommand mSelectDailyValuesCommand = cmd.CreateCommand();
                if (cmd.State != System.Data.ConnectionState.Open)
                {
                    cmd.Open();
                }
                //mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakCong - p.AvgPeakCong,p1.AvgOffpeakCong-p.AvgOffpeakCong,p1.Avg24Cong-p.Avg24Cong,p.PeakHours ,p.OffpeakHours, p.markettypecode " +
                //    "from PJM.NodeLMPDaily p join PJM.NodeLMPDaily p1 on p.MarketDate=p1.MarketDate and p.NodeKey= " + sourcenodekey + " and p1.NodeKey= " + sinknodekey +
                //    " and p.MarketDate>='" + startDate + "' and p.MarketDate<='" + endDate.AddMonths(1) + "' and p1.MarketDate>= '" + startDate + "' and p1.MarketDate<=' " + endDate.AddMonths(1) +
                //    "'and p.markettypecode = p1.markettypecode order by p.markettypecode desc, p.MarketDate desc";

                mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakCong - p.AvgPeakCong,p1.AvgOffpeakCong-p.AvgOffpeakCong,p1.Avg24Cong-p.Avg24Cong,p.PeakHours ,p.OffpeakHours, p.markettypecode " +
               "from PJM.NodeLMPDaily p join PJM.NodeLMPDaily p1 on p.MarketDate=p1.MarketDate and p.NodeKey= " + sourcenodekey + " and p1.NodeKey= " + sinknodekey +
               " and p.MarketDate>='" + startDate + "' and p.MarketDate<='" + endDate + "' and p1.MarketDate>= '" + startDate + "' and p1.MarketDate<=' " + endDate +
               "'and p.markettypecode = p1.markettypecode order by p.markettypecode desc, p.MarketDate desc";
                reader = mSelectDailyValuesCommand.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> monthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        Dictionary<int, DailyValues> daysHash = new Dictionary<int, DailyValues>();
                        DailyValues congValues = new DailyValues();
                        DateTime marketdate = reader.GetDateTime(0);
                        if (yearHash.ContainsKey(marketdate.Year))
                        {
                            monthHash = yearHash[marketdate.Year];
                        }
                        if (monthHash.ContainsKey(marketdate.Month))
                        {
                            daysHash = monthHash[marketdate.Month].Item2;
                        }
                        if (daysHash.ContainsKey(marketdate.Day))
                        {
                            congValues = daysHash[marketdate.Day];
                        }
                        string marketType = reader.GetString(6);
                        if (marketType == "DA")
                        {
                            congValues.DAPeakCong = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetFloat(1);
                            congValues.DAOffPeakCong = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetFloat(2);
                            congValues.DA24Cong = reader.IsDBNull(3) ? (double?)null : (double?)reader.GetFloat((3));
                        }
                        else
                        {
                            congValues.RTPeakCong = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetFloat(1);
                            congValues.RTOffPeakCong = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetFloat(2);
                            congValues.RT24Cong = reader.IsDBNull(3) ? (double?)null : (double?)reader.GetFloat((3));
                        }
                        congValues.PeakHours = Convert.ToInt32(reader.GetValue(4));
                        congValues.OffPeakHours = Convert.ToInt32(reader.GetValue(5));
                        string date = marketdate.Year + "-" + marketdate.Month;
                        DailyValues priceDailyValues = new DailyValues();
                        if (auctionType == "Annual")
                        {
                            if (priceHash.ContainsKey(date))
                            {
                                priceDailyValues = priceHash[date];
                            }
                            else
                            {
                                DateTime priceDate = DateTime.Parse(marketdate.Year.ToString() + "/" + "06" + "/" + "01");
                                if (priceDate.Year == DateTime.Now.Year && !(priceHash.ContainsKey(priceDate.ToString())))
                                {
                                    priceHash.Add(priceDate.ToString("yyyy-MM"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    for (int i = 1; i < 6; i++)
                                    {
                                        if (!priceHash.ContainsKey(priceDate.Year + '-' + i.ToString()))
                                            priceHash.Add(priceDate.Year.ToString() + '-' + i.ToString(), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    }
                                }
                                else
                                {
                                    if (!priceHash.ContainsKey(marketdate.ToString("yyyy-M")))
                                    {
                                        if (marketdate.Month >= 1 && marketdate.Month <= 5)
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                        else
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.ToString("yyyy-M")]);
                                    }
                                }

                                if (priceHash.ContainsKey(priceDate.ToString("yyyy-M")))
                                {

                                }

                            }
                        }
                        if (priceHash.ContainsKey(date))
                        {
                            priceDailyValues = priceHash[date];
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> monthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(priceDailyValues, daysHash);
                        daysHash.Remove(marketdate.Day);
                        daysHash.Add(marketdate.Day, congValues);
                        monthHash.Remove(marketdate.Month);
                        monthHash.Add(marketdate.Month, monthTuple);
                        yearHash.Remove(marketdate.Year);
                        yearHash.Add(marketdate.Year, monthHash);

                    }
                    catch (Exception ex)
                    {

                    }
                }

                foreach (var item in priceHash)
                {
                    Dictionary<int, DailyValues> tempdaysHash = new Dictionary<int, DailyValues>();

                    DateTime mdate = DateTime.Parse(item.Key);

                    if (!yearHash.ContainsKey(mdate.Year))
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        for (int i = 0; i < 31; i++)
                        {
                            tempdaysHash.Add(i, priceHash[item.Key]);
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        yearHash.Add(mdate.Year, tempmonthHash);
                    }
                    else
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = yearHash[mdate.Year];
                        if (!tempmonthHash.ContainsKey(mdate.Month))
                        {
                            for (int i = 0; i < 31; i++)
                            {
                                tempdaysHash.Add(i, priceHash[item.Key]);
                            }
                            Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                            tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        }
                    }

                }
                reader.Close();
            }
            else
            {
                if (ftrQuarterMonth != 0)
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }
                    mSelectQuarterlyPriceCommand = new SqlCommand();
                    foreach (int year in yearList)
                    {
                        mSelectQuarterlyPriceCommand.CommandText = " select c.startdate ,  si.LMPOnPeak - b.LMPOnPeak , si.LMPOffPeak - b.LMPOffPeak  , c.PeakHrs , c.OffPeakHrs , c.PeriodKey  from pjm.FTRAuction a, pjm.FTRAuctionNodePrice b " +
                            " join period c on b.PeriodKey = c.PeriodKey join pjm.FTRAuctionNodePrice si on  b.FTRAuctionKey = si.FTRAuctionKey and b.PeriodKey = si.PeriodKey " +
                            " where  b.periodkey = (select periodkey from period where startdate = '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal'  " +
                            " and marketkey = 1) and a.FTRAuctionKey = b.FTRAuctionKey and b.NodeKey  =  " + sourcenodekey.ToString() + " and  si.NodeKey = " + sinknodekey.ToString() + " and " +
                            " a.FTRAuctionKey = (select max(a.FtrAuctionkey) from pjm.FTRAuction a, pjm.FTRAuctionNodePrice b where  b.periodkey = " +
                            " (select periodkey from period where startdate =  '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal' and marketkey = 1) and a.FTRAuctionKey = b.FTRAuctionKey) ";
                        mSelectQuarterlyPriceCommand.Connection = VayuDBConnection;
                        SqlDataReader rdr = mSelectQuarterlyPriceCommand.ExecuteReader();
                        while (rdr.Read())
                        {
                            DateTime marketDateTime = rdr.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)rdr.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)rdr.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(rdr.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(rdr.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(rdr.GetValue(5));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            string date1 = string.Empty; string date2 = string.Empty;
                            if (marketDateTime.Month != 12)
                            {
                                date1 = marketDateTime.Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            else
                            {
                                date1 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                            if (!priceHash.ContainsKey(date1))
                                priceHash.Add(date1, dailyValues);
                            if (!priceHash.ContainsKey(date2))
                                priceHash.Add(date2, dailyValues);
                        }
                        rdr.Close();
                    }

                }
                else if (auctionType == "Annual")
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }

                    foreach (int year in yearList)
                    {
                        mSelectPriceCommand.CommandText = " select c.StartDate, b.LMPOnPeak - a.LMPOnPeak, b.LMPOffPeak - a.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey from pjm.FTRAuctionNodePrice a, pjm.FTRAuctionNodePrice b, period c " +
                       " where a.nodekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'Annual' and startdate >= @startdate and enddate <= @enddate)  and b.nodekey = @sink and b.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'Annual' " +
                       " and startdate >= @startdate and enddate <= @enddate)  and a.PeriodKey = b.PeriodKey and a.FTRAuctionKey = b.FTRAuctionKey and a.PeriodKey = c.PeriodKey and a.FTRAuctionKey in(select MAX(FTRAuctionKey) from pjm.FTRAuction  where FTRAuctionStartDate = @startdate and FTRAuctionEndDate = @enddate  " +
                       " ) order by a.periodkey, a.FTRAuctionKey ";
                        mSelectPriceCommand.Parameters["@source"].Value = sourcenodekey;
                        mSelectPriceCommand.Parameters["@sink"].Value = sinknodekey;
                        mSelectPriceCommand.Parameters["@startdate"].Value = DateTime.Parse(year + "/" + "06" + "/" + "01");
                        mSelectPriceCommand.Parameters["@enddate"].Value = DateTime.Parse((year + 1) + "/" + "05" + "/" + "31");
                        reader = mSelectPriceCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime marketDateTime = reader.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)reader.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)reader.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                        }
                        reader.Close();
                    }
                }
                else
                {

                    DateTime newEndDate = endDate;
                    if ((endDate.Month == DateTime.Today.Month) && (endDate.Day <= 29))
                        newEndDate = endDate.AddMonths(1);

                    mSelectPriceCommandPJMOptionMonthly.Parameters["@source"].Value = SourceId;
                    mSelectPriceCommandPJMOptionMonthly.Parameters["@sink"].Value = SinkId;
                    mSelectPriceCommandPJMOptionMonthly.Parameters["@startdate"].Value = startDate;
                    // mSelectPriceCommandPJMOptionMonthly.Parameters["@enddate"].Value = endDate.AddMonths(1);
                    mSelectPriceCommandPJMOptionMonthly.Parameters["@enddate"].Value = newEndDate;

                    reader = mSelectPriceCommandPJMOptionMonthly.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime marketDateTime = reader.GetDateTime(0);
                        DailyValues dailyValues = new DailyValues();
                        dailyValues.PricePeak = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetDecimal(1);
                        dailyValues.PriceOffPeak = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetDecimal(2);
                        dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3));
                        dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                        dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                        string date = marketDateTime.Year + "-" + marketDateTime.Month;
                        if (priceHash.ContainsKey(date))
                        {
                            priceHash.Remove(date);
                        }
                        priceHash.Add(date, dailyValues);
                    }
                    reader.Close();
                }

                VayuDBConnection.Close();

                SqlConnection cmd = new SqlConnection(tradingDBString);
                SqlCommand mSelectDailyValuesCommand = cmd.CreateCommand();
                if (cmd.State != System.Data.ConnectionState.Open)
                {
                    cmd.Open();
                }
                //mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakLMP - p.AvgPeakLMP,p1.AvgOffpeakLMP-p.AvgOffpeakLMP,p1.Avg24LMP-p.Avg24LMP,p.PeakHours ,p.OffpeakHours, p.markettypecode, p1.AvgPeakWELMP - p.AvgPeakWELMP ,p.PeakWEHours " +
                //    "from NodeLMPDailys p join NodeLMPDailys p1 on p.MarketDate=p1.MarketDate and p.NodeKey= " + sourcenodekey + " and p1.NodeKey= " + sinknodekey + 
                //    " and p.MarketDate>='" + startDate + "' and p.MarketDate<='" + endDate.AddMonths(1) + "' and p1.MarketDate>= '" + startDate + "' and p1.MarketDate<=' " + endDate.AddMonths(1) +
                //    "'and p.markettypecode = p1.markettypecode order by p.markettypecode desc, p.MarketDate desc";

               /* mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakCong - p.AvgPeakCong,p1.AvgOffpeakCong - p.AvgOffpeakCong,p1.Avg24Cong - p.Avg24Cong,p.PeakHours , "
                                                        +"p.OffpeakHours, p.markettypecode from pjm.NodeLMPDaily p join pjm.NodeLMPDaily p1 "
                                                        + "on p.MarketDate = p1.MarketDate and p.NodeKey = " + sourcenodekey + "    and p1.NodeKey = " + sinknodekey +" and p1.MarketDate >= '" + startDate + "'  and p1.MarketDate <= '" + endDate.AddMonths(1) + " ' and p.markettypecode = p1.markettypecode "
                                                        + "union "

                                                        + "select MarketDate, AvgPeakCong, AvgOffpeakCong, Avg24Cong, PeakHours, OffpeakHours, MarketTypeCode "
                                                        + "from[PJM].[NodeLMPDailysOPT] (nolock)where SourceNodeKey = " + sourcenodekey + " and SinkNodeKey = " + sinknodekey + " and MarketDate>= '" + startDate + "'  and MarketDate<= '" + endDate.AddMonths(1) + "' order by p.markettypecode desc, "
                                                        +"p.MarketDate desc ";*/


                mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakCong - p.AvgPeakCong,p1.AvgOffpeakCong - p.AvgOffpeakCong,p1.Avg24Cong - p.Avg24Cong,p.PeakHours , "
                                                        + "p.OffpeakHours, p.markettypecode from pjm.NodeLMPDaily p join pjm.NodeLMPDaily p1 "
                                                        + "on p.MarketDate = p1.MarketDate and p.NodeKey = " + sourcenodekey + "    and p1.NodeKey = " + sinknodekey + " and p1.MarketDate >= '" + startDate + "'  and p1.MarketDate <= '" + endDate + " ' and p.markettypecode = p1.markettypecode "
                                                        + "union "

                                                        + "select MarketDate, AvgPeakCong, AvgOffpeakCong, Avg24Cong, PeakHours, OffpeakHours, MarketTypeCode "
                                                        + "from[PJM].[NodeLMPDailysOPT] (nolock)where SourceNodeKey = " + sourcenodekey + " and SinkNodeKey = " + sinknodekey + " and MarketDate>= '" + startDate + "'  and MarketDate<= '" + endDate + "' order by p.markettypecode desc, "
                                                        + "p.MarketDate desc ";
                mSelectDailyValuesCommand.CommandTimeout = 300000;
                reader = mSelectDailyValuesCommand.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> monthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        Dictionary<int, DailyValues> daysHash = new Dictionary<int, DailyValues>();
                        DailyValues congValues = new DailyValues();
                        DateTime marketdate = reader.GetDateTime(0);
                        if (yearHash.ContainsKey(marketdate.Year))
                        {
                            monthHash = yearHash[marketdate.Year];
                        }
                        if (monthHash.ContainsKey(marketdate.Month))
                        {
                            daysHash = monthHash[marketdate.Month].Item2;
                        }
                        if (daysHash.ContainsKey(marketdate.Day))
                        {
                            congValues = daysHash[marketdate.Day];
                        }

                        string marketType = reader.GetString(6);
                        if (marketType == "DA")
                        {
                            congValues.DAPeakCong = reader.IsDBNull(1) ? (double?)0 : (double?)reader.GetFloat(1);
                            congValues.DAOffPeakCong = reader.IsDBNull(2) ? (double?)0 : (double?)reader.GetFloat(2);
                            congValues.DA24Cong = reader.IsDBNull(3) ? (double?)0 : (double?)reader.GetFloat((3));

                        }
                        else
                        {
                            congValues.RTPeakCong = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetFloat(1);
                            congValues.RTOffPeakCong = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetFloat(2);
                            congValues.RT24Cong = reader.IsDBNull(3) ? (double?)null : (double?)reader.GetFloat((3));
                        }
                        congValues.PeakHours = Convert.ToInt32(reader.GetValue(4));
                        congValues.OffPeakHours = Convert.ToInt32(reader.GetValue(5));

                      // congValues.PeakWEHours = Convert.ToInt32(reader.GetValue(8));
                        string date = marketdate.Year + "-" + marketdate.Month;
                        DailyValues priceDailyValues = new DailyValues();
                        if (auctionType == "Annual")
                        {
                            if (priceHash.ContainsKey(date))
                            {
                                priceDailyValues = priceHash[date];
                            }
                            else
                            {
                                DateTime priceDate = DateTime.Parse(marketdate.Year.ToString() + "/" + "06" + "/" + "01");
                                if (priceDate.Year == DateTime.Now.Year && !(priceHash.ContainsKey(priceDate.ToString())))
                                {
                                    priceHash.Add(priceDate.ToString("yyyy-MM"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    for (int i = 1; i < 6; i++)
                                    {
                                        if (!priceHash.ContainsKey(priceDate.Year + '-' + i.ToString()))
                                            priceHash.Add(priceDate.Year.ToString() + '-' + i.ToString(), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    }
                                }
                                else
                                {
                                    if (!priceHash.ContainsKey(marketdate.ToString("yyyy-M")))
                                    {
                                        if (marketdate.Month >= 1 && marketdate.Month <= 5)
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                        else
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.ToString("yyyy-M")]);
                                    }
                                }

                                if (priceHash.ContainsKey(priceDate.ToString("yyyy-M")))
                                {

                                }

                            }
                        }
                        if (priceHash.ContainsKey(date))
                        {
                            priceDailyValues = priceHash[date];
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> monthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(priceDailyValues, daysHash);
                        daysHash.Remove(marketdate.Day);
                        daysHash.Add(marketdate.Day, congValues);
                        monthHash.Remove(marketdate.Month);
                        monthHash.Add(marketdate.Month, monthTuple);
                        yearHash.Remove(marketdate.Year);
                        yearHash.Add(marketdate.Year, monthHash);

                    }
                    catch (Exception ex)
                    {

                    }
                }

                foreach (var item in priceHash)
                {
                    Dictionary<int, DailyValues> tempdaysHash = new Dictionary<int, DailyValues>();

                    DateTime mdate = DateTime.Parse(item.Key);

                    if (!yearHash.ContainsKey(mdate.Year))
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        for (int i = 0; i < 31; i++)
                        {
                            tempdaysHash.Add(i, priceHash[item.Key]);
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        yearHash.Add(mdate.Year, tempmonthHash);
                    }
                    else
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = yearHash[mdate.Year];
                        if (!tempmonthHash.ContainsKey(mdate.Month))
                        {
                            for (int i = 0; i < 31; i++)
                            {
                                tempdaysHash.Add(i, priceHash[item.Key]);
                            }
                            Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                            tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        }
                    }

                }
                reader.Close();


            }
            return yearHash;

        }

        public Dictionary<DateTime, string> GetPeakYN_daterange(DateTime startDate, DateTime endDate)
        {


            Dictionary<DateTime, string> peaklist = new Dictionary<DateTime, string>();

            if (VayuDBConnection.State == ConnectionState.Closed)
                VayuDBConnection.Open();

            mSelectPeakYN_daterang = new SqlCommand();
            //  mSelectPeakYN_daterang.CommandText = "select MarketDateTime, PeakYN from MarketTime where MarketKey=9 and MarketDateTime >=@startdate and MarketDateTime<=@enddate and (PeakYN='W') order by MarketDateTime";

            mSelectPeakYN_daterang.CommandText = "select MarketDateTime, PeakYN from MarketTime where Datename(dw,MarketDateTime) not in('Saturday','Sunday') and (PeakYN='W') and (Marketkey=9) and  MarketDateTime >=@startdate and MarketDateTime<=@enddate order by MarketDateTime ";
            mSelectPeakYN_daterang.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPeakYN_daterang.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPeakYN_daterang.Connection = VayuDBConnection;

            mSelectPeakYN_daterang.Parameters["@startdate"].Value = startDate;  
            mSelectPeakYN_daterang.Parameters["@enddate"].Value = endDate;


            SqlDataReader reader = mSelectPeakYN_daterang.ExecuteReader();
            while (reader.Read())
            {
                string peakyn = reader.GetString(1);
                DateTime MarketDateTime = reader.GetDateTime(0);
                peaklist.Add(MarketDateTime, peakyn);
            }
            reader.Close();
            VayuDBConnection.Close();

            return peaklist;
        }
        private Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> getFtrErcotData(int marketkey, long SourceId, long SinkId, DateTime startDate, DateTime endDate, string Hedgetype, int ftrQuarterMonth, string auctionType)
        {
            String culture = "lt-LT";
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            List<int> yearList = new List<int>();
            InitDB();
            long sourcenodekey = SourceId;
            long sinknodekey = SinkId;
            VayuDBConnection.Open();
            Dictionary<string, DailyValues> priceHash = new Dictionary<string, DailyValues>();
            SqlDataReader reader;
            if (Hedgetype == "OBL")
            {
                if (ftrQuarterMonth != 0)
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }
                    mSelectQuarterlyPriceCommandErcot = new SqlCommand();
                    foreach (int year in yearList)
                    {
                        mSelectQuarterlyPriceCommandErcot.CommandText = " select c.startdate ,  si.LMPOnPeak - b.LMPOnPeak , si.LMPOffPeak - b.LMPOffPeak  , c.PeakHrs , c.OffPeakHrs , c.PeriodKey, si.PeakWE - b.PeakWE, c.PEAKWE from crrauction a,CRRAuctionNodePrice b " +
                            " join period c on b.PeriodKey = c.PeriodKey join CRRAuctionNodePrice si on  b.CRRAuctionKey = si.CRRAuctionKey and b.PeriodKey = si.PeriodKey " +
                            " where  b.periodkey = (select periodkey from period where startdate = '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal'  " +
                            " and marketkey = 9) and a.CRRAuctionKey = b.CRRAuctionKey and b.NodeKey  =  " + sourcenodekey.ToString() + " and  si.NodeKey = " + sinknodekey.ToString() + " and " +
                            " a.CRRAuctionKey = (select max(a.CRRAuctionkey) from crrauction a, CRRAuctionNodePrice b where  b.periodkey = " +
                            " (select periodkey from period where startdate =  '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal' and marketkey = 9) and a.CRRAuctionKey = b.CRRAuctionKey) ";
                        mSelectQuarterlyPriceCommandErcot.Connection = VayuDBConnection;
                        SqlDataReader rdr = mSelectQuarterlyPriceCommandErcot.ExecuteReader();
                        while (rdr.Read())
                        {
                            DateTime marketDateTime = rdr.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)rdr.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)rdr.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(rdr.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(rdr.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(rdr.GetValue(5));
                            dailyValues.PriceWE = (double?)rdr.GetDecimal(6);
                            dailyValues.PeakWE = Convert.ToInt32(rdr.GetValue(7));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            string date1 = string.Empty; string date2 = string.Empty;
                            if (marketDateTime.Month != 12)
                            {
                                date1 = marketDateTime.Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            else
                            {
                                date1 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                            if (!priceHash.ContainsKey(date1))
                                priceHash.Add(date1, dailyValues);
                            if (!priceHash.ContainsKey(date2))
                                priceHash.Add(date2, dailyValues);
                        }
                        rdr.Close();
                    }

                }
                else if (auctionType == "Annual")
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }

                    foreach (int year in yearList)
                    {
                        mSelectPriceCommandErcot.CommandText = " select c.StartDate, b.LMPOnPeak - a.LMPOnPeak, b.LMPOffPeak - a.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey, a.PeakWE - b.PeakWE, c.PEAKWE from CRRAuctionNodePrice a, CRRAuctionNodePrice b, period c " +
                       " where a.nodekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 9 and periodtype = 'Annual' and startdate >= @startdate and enddate <= @enddate)  and b.nodekey = @sink and b.periodkey in (select PeriodKey from period where marketkey = 9 and periodtype = 'Annual' " +
                       " and startdate >= @startdate and enddate <= @enddate)  and a.PeriodKey = b.PeriodKey and a.CRRAuctionkey = b.CRRAuctionkey and a.PeriodKey = c.PeriodKey and a.CRRAuctionkey in(select MAX(CRRAuctionKey) from crrauction  where CRRAuctionStartDate = @startdate and CRRAuctionEndDate = @enddate  " +
                       " ) order by a.periodkey, a.CRRAuctionkey ";
                        mSelectPriceCommandErcot.Parameters["@source"].Value = sourcenodekey;
                        mSelectPriceCommandErcot.Parameters["@sink"].Value = sinknodekey;
                        mSelectPriceCommandErcot.Parameters["@startdate"].Value = DateTime.Parse(year + "/" + "06" + "/" + "01");
                        mSelectPriceCommandErcot.Parameters["@enddate"].Value = DateTime.Parse((year + 1) + "/" + "05" + "/" + "31");
                        reader = mSelectPriceCommandErcot.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime marketDateTime = reader.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)reader.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)reader.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                            dailyValues.PriceWE = (double?)reader.GetDecimal(6);
                            dailyValues.PeakWE = Convert.ToInt32(reader.GetValue(7));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                        }
                        reader.Close();
                    }
                }
                else
                {

                    DateTime newEndDate = endDate;
                    if ((endDate.Month == DateTime.Today.Month) && (endDate.Day <= 29))
                        newEndDate = endDate.AddMonths(1);

                    mSelectPriceCommandErcot.Parameters["@source"].Value = sourcenodekey;
                    mSelectPriceCommandErcot.Parameters["@sink"].Value = sinknodekey;
                    mSelectPriceCommandErcot.Parameters["@startdate"].Value = startDate;
                  //  mSelectPriceCommandErcot.Parameters["@enddate"].Value = endDate.AddMonths(1);
                     mSelectPriceCommandErcot.Parameters["@enddate"].Value = newEndDate;

                    reader = mSelectPriceCommandErcot.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime marketDateTime = reader.GetDateTime(0);
                        DailyValues dailyValues = new DailyValues();
                        dailyValues.PricePeak = (double?)reader.GetDecimal(1);
                        dailyValues.PriceOffPeak = (double?)reader.GetDecimal(2);
                        dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3));
                        dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                        dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                        dailyValues.PriceWE = (double?)reader.GetDecimal(6); //reader.IsDBNull(6) ? (double?)null : (double?)reader.GetFloat(6);
                        dailyValues.PeakWE = Convert.ToInt32(reader.GetValue(7));
                        string date = marketDateTime.Year + "-" + marketDateTime.Month;
                        if (priceHash.ContainsKey(date))
                        {
                            priceHash.Remove(date);
                        }
                        priceHash.Add(date, dailyValues);
                    }
                    reader.Close();
                }

                VayuDBConnection.Close();

                SqlConnection cmd = new SqlConnection(tradingDBStringERCOT);
                SqlCommand mSelectDailyValuesCommand = cmd.CreateCommand();
                if (cmd.State != System.Data.ConnectionState.Open)
                {
                    cmd.Open();
                }


                // 
                //mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakLMP - p.AvgPeakLMP,p1.AvgOffpeakLMP-p.AvgOffpeakLMP,p1.Avg24LMP-p.Avg24LMP,p.PeakHours ,p.OffpeakHours, p.markettypecode, p1.AvgPeakWELMP - p.AvgPeakWELMP ,p.PeakWEHours " +
                //    "from NodeLMPDailys p join NodeLMPDailys p1 on p.MarketDate=p1.MarketDate and p.NodeKey= " + sourcenodekey + " and p1.NodeKey= " + sinknodekey +
                //    " and p.MarketDate>='" + startDate + "' and p.MarketDate<='" + endDate.AddMonths(1) + "' and p1.MarketDate>= '" + startDate + "' and p1.MarketDate<=' " + endDate.AddMonths(1) +
                //    "'and p.markettypecode = p1.markettypecode order by p.markettypecode desc, p.MarketDate desc";

                mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakLMP - p.AvgPeakLMP,p1.AvgOffpeakLMP-p.AvgOffpeakLMP,p1.Avg24LMP-p.Avg24LMP,p.PeakHours ,p.OffpeakHours, p.markettypecode, p1.AvgPeakWELMP - p.AvgPeakWELMP ,p.PeakWEHours " +
                  "from NodeLMPDailys p join NodeLMPDailys p1 on p.MarketDate=p1.MarketDate and p.NodeKey= " + sourcenodekey + " and p1.NodeKey= " + sinknodekey +
                  " and p.MarketDate>='" + startDate + "' and p.MarketDate<='" + endDate + "' and p1.MarketDate>= '" + startDate + "' and p1.MarketDate<=' " + endDate +
                  "'and p.markettypecode = p1.markettypecode order by p.markettypecode desc, p.MarketDate desc";
                reader = mSelectDailyValuesCommand.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> monthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        Dictionary<int, DailyValues> daysHash = new Dictionary<int, DailyValues>();
                        DailyValues congValues = new DailyValues();
                        DateTime marketdate = reader.GetDateTime(0);
                        if (yearHash.ContainsKey(marketdate.Year))
                        {
                            monthHash = yearHash[marketdate.Year];
                        }
                        if (monthHash.ContainsKey(marketdate.Month))
                        {
                            daysHash = monthHash[marketdate.Month].Item2;
                        }
                        if (daysHash.ContainsKey(marketdate.Day))
                        {
                            congValues = daysHash[marketdate.Day];
                        }
                        string marketType = reader.GetString(6);
                        if (marketType == "DA")
                        {
                            congValues.DAPeakCong = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetFloat(1);
                            congValues.DAOffPeakCong = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetFloat(2);
                            congValues.DA24Cong = reader.IsDBNull(3) ? (double?)null : (double?)reader.GetFloat((3));
                            congValues.DAPeakWECong = reader.IsDBNull(7) ? (double?)null : (double?)reader.GetFloat((7));
                        }
                        else
                        {
                            congValues.RTPeakCong = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetFloat(1);
                            congValues.RTOffPeakCong = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetFloat(2);
                            congValues.RT24Cong = reader.IsDBNull(3) ? (double?)null : (double?)reader.GetFloat((3));
                            congValues.RTPeakWECong = reader.IsDBNull(7) ? (double?)null : (double?)reader.GetFloat((7));
                        }
                        congValues.PeakHours = Convert.ToInt32(reader.GetValue(4));
                        congValues.OffPeakHours = Convert.ToInt32(reader.GetValue(5));
                        congValues.PeakWEHours = Convert.ToInt32(reader.GetValue(8));
                        string date = marketdate.Year + "-" + marketdate.Month;
                        DailyValues priceDailyValues = new DailyValues();
                        if (auctionType == "Annual")
                        {
                            if (priceHash.ContainsKey(date))
                            {
                                priceDailyValues = priceHash[date];
                            }
                            else
                            {
                                DateTime priceDate = DateTime.Parse(marketdate.Year.ToString() + "/" + "06" + "/" + "01");
                                if (priceDate.Year == DateTime.Now.Year && !(priceHash.ContainsKey(priceDate.ToString())))
                                {
                                    priceHash.Add(priceDate.ToString("yyyy-MM"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    for (int i = 1; i < 6; i++)
                                    {
                                        if (!priceHash.ContainsKey(priceDate.Year + '-' + i.ToString()))
                                            priceHash.Add(priceDate.Year.ToString() + '-' + i.ToString(), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    }
                                }
                                else
                                {
                                    if (!priceHash.ContainsKey(marketdate.ToString("yyyy-M")))
                                    {
                                        if (marketdate.Month >= 1 && marketdate.Month <= 5)
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                        else
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.ToString("yyyy-M")]);
                                    }
                                }

                                if (priceHash.ContainsKey(priceDate.ToString("yyyy-M")))
                                {

                                }

                            }
                        }
                        if (priceHash.ContainsKey(date))
                        {
                            priceDailyValues = priceHash[date];
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> monthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(priceDailyValues, daysHash);
                        daysHash.Remove(marketdate.Day);
                        daysHash.Add(marketdate.Day, congValues);
                        monthHash.Remove(marketdate.Month);
                        monthHash.Add(marketdate.Month, monthTuple);
                        yearHash.Remove(marketdate.Year);
                        yearHash.Add(marketdate.Year, monthHash);

                    }
                    catch (Exception ex)
                    {

                    }
                }

                foreach (var item in priceHash)
                {
                    Dictionary<int, DailyValues> tempdaysHash = new Dictionary<int, DailyValues>();

                    DateTime mdate = DateTime.Parse(item.Key);

                    if (!yearHash.ContainsKey(mdate.Year))
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        for (int i = 1; i <=31; i++)// for (int i = 0; i <=31; i++)
                        {
                            tempdaysHash.Add(i, priceHash[item.Key]);
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        yearHash.Add(mdate.Year, tempmonthHash);
                    }
                    else
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = yearHash[mdate.Year];
                        if (!tempmonthHash.ContainsKey(mdate.Month))
                        {
                            for (int i = 1; i <=31; i++)// for (int i = 0; i <=31; i++)
                            {
                                tempdaysHash.Add(i, priceHash[item.Key]);
                            }
                            Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                            tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        }
                    }

                }
                reader.Close();

            }
            else
            {
                if (ftrQuarterMonth != 0)
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }
                    mSelectQuarterlyPriceCommandErcotOption = new SqlCommand();
                    foreach (int year in yearList)
                    {
                        mSelectQuarterlyPriceCommandErcotOption.CommandText = " select c.startdate ,  si.LMPOnPeak - b.LMPOnPeak , si.LMPOffPeak - b.LMPOffPeak  , c.PeakHrs , c.OffPeakHrs , c.PeriodKey, si.PeakWE - b.PeakWE, c.PEAKWE from crrauction a,CRRAuctionNodePrice b " +
                            " join period c on b.PeriodKey = c.PeriodKey join CRRAuctionNodePrice si on  b.CRRAuctionKey = si.CRRAuctionKey and b.PeriodKey = si.PeriodKey " +
                            " where  b.periodkey = (select periodkey from period where startdate = '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal'  " +
                            " and marketkey = 9) and a.CRRAuctionKey = b.CRRAuctionKey and b.NodeKey  =  " + sourcenodekey.ToString() + " and  si.NodeKey = " + sinknodekey.ToString() + " and " +
                            " a.CRRAuctionKey = (select max(a.CRRAuctionkey) from crrauction a, CRRAuctionNodePrice b where  b.periodkey = " +
                            " (select periodkey from period where startdate =  '" + ftrQuarterMonth.ToString() + "/1/" + year.ToString() + "' and periodtype = 'seasonal' and marketkey = 9) and a.CRRAuctionKey = b.CRRAuctionKey) ";
                        mSelectQuarterlyPriceCommandErcotOption.Connection = VayuDBConnection;
                        SqlDataReader rdr = mSelectQuarterlyPriceCommandErcotOption.ExecuteReader();
                        while (rdr.Read())
                        {
                            DateTime marketDateTime = rdr.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)rdr.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)rdr.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(rdr.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(rdr.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(rdr.GetValue(5));
                            dailyValues.PriceWE = (double?)rdr.GetDecimal(6);
                            dailyValues.PeakWE = Convert.ToInt32(rdr.GetValue(7));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            string date1 = string.Empty; string date2 = string.Empty;
                            if (marketDateTime.Month != 12)
                            {
                                date1 = marketDateTime.Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            else
                            {
                                date1 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(1).Month;
                                date2 = marketDateTime.AddYears(1).Year + "-" + marketDateTime.AddMonths(2).Month;
                            }
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                            if (!priceHash.ContainsKey(date1))
                                priceHash.Add(date1, dailyValues);
                            if (!priceHash.ContainsKey(date2))
                                priceHash.Add(date2, dailyValues);
                        }
                        rdr.Close();
                    }

                }
                else if (auctionType == "Annual")
                {
                    int years = endDate.Year - startDate.Year;
                    for (int i = 0; i <= years; i++)
                    {
                        yearList.Add(startDate.Year + i);
                    }

                    foreach (int year in yearList)
                    {
                        mSelectPriceCommandErcotOption.CommandText = " select c.StartDate, b.LMPOnPeak - a.LMPOnPeak, b.LMPOffPeak - a.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey, a.PeakWE - b.PeakWE, c.PEAKWE from CRRAuctionNodePrice a, CRRAuctionNodePrice b, period c " +
                       " where a.nodekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 9 and periodtype = 'Annual' and startdate >= @startdate and enddate <= @enddate)  and b.nodekey = @sink and b.periodkey in (select PeriodKey from period where marketkey = 9 and periodtype = 'Annual' " +
                       " and startdate >= @startdate and enddate <= @enddate)  and a.PeriodKey = b.PeriodKey and a.CRRAuctionkey = b.CRRAuctionkey and a.PeriodKey = c.PeriodKey and a.CRRAuctionkey in(select MAX(CRRAuctionKey) from crrauction  where CRRAuctionStartDate = @startdate and CRRAuctionEndDate = @enddate  " +
                       " ) order by a.periodkey, a.CRRAuctionkey ";
                        mSelectPriceCommandErcotOption.Parameters["@source"].Value = sourcenodekey;
                        mSelectPriceCommandErcotOption.Parameters["@sink"].Value = sinknodekey;
                        mSelectPriceCommandErcotOption.Parameters["@startdate"].Value = DateTime.Parse(year + "/" + "06" + "/" + "01");
                        mSelectPriceCommandErcotOption.Parameters["@enddate"].Value = DateTime.Parse((year + 1) + "/" + "05" + "/" + "31");
                        reader = mSelectPriceCommandErcotOption.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime marketDateTime = reader.GetDateTime(0);
                            DailyValues dailyValues = new DailyValues();
                            dailyValues.PricePeak = (double?)reader.GetDecimal(1);
                            dailyValues.PriceOffPeak = (double?)reader.GetDecimal(2);
                            dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3)); ;
                            dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                            dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                            dailyValues.PriceWE = (double?)reader.GetDecimal(6);
                            dailyValues.PeakWE = Convert.ToInt32(reader.GetValue(7));
                            string date = marketDateTime.Year + "-" + marketDateTime.Month;
                            if (priceHash.ContainsKey(date))
                            {
                                priceHash.Remove(date);
                            }
                            priceHash.Add(date, dailyValues);
                        }
                        reader.Close();
                    }
                }
                else
                {
                    DateTime newEndDate = endDate;
                    if ((endDate.Month == DateTime.Today.Month) && (endDate.Day <= 29))
                    {
                        newEndDate = endDate.AddMonths(1);
                    }

                    mSelectPriceCommandErcotOptionMonthly.Parameters["@source"].Value = sourcenodekey;
                    mSelectPriceCommandErcotOptionMonthly.Parameters["@sink"].Value = sinknodekey;
                    mSelectPriceCommandErcotOptionMonthly.Parameters["@startdate"].Value = startDate;
                    // mSelectPriceCommandErcotOptionMonthly.Parameters["@enddate"].Value = endDate.AddMonths(1);
                    mSelectPriceCommandErcotOptionMonthly.Parameters["@enddate"].Value = newEndDate;
                    reader = mSelectPriceCommandErcotOptionMonthly.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime marketDateTime = reader.GetDateTime(0);
                        DailyValues dailyValues = new DailyValues();
                        dailyValues.PricePeak = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetDecimal(1);
                        dailyValues.PriceOffPeak = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetDecimal(2);
                        dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3));
                        dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                        dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                        dailyValues.PriceWE = reader.IsDBNull(6) ? (double?)null : (double?)reader.GetDecimal(6);//reader.IsDBNull(6) ? (double?)null : (double?)reader.GetFloat(6);
                        dailyValues.PeakWE = Convert.ToInt32(reader.GetValue(7));
                        string date = marketDateTime.Year + "-" + marketDateTime.Month;
                        if (priceHash.ContainsKey(date))
                        {
                            priceHash.Remove(date);
                        }
                        priceHash.Add(date, dailyValues);
                    }
                    reader.Close();
                }

                VayuDBConnection.Close();

                SqlConnection cmd = new SqlConnection(tradingDBStringERCOT);
                SqlCommand mSelectDailyValuesCommand = cmd.CreateCommand();
                if (cmd.State != System.Data.ConnectionState.Open)
                {
                    cmd.Open();
                }
                //mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakLMP - p.AvgPeakLMP,p1.AvgOffpeakLMP-p.AvgOffpeakLMP,p1.Avg24LMP-p.Avg24LMP,p.PeakHours ,p.OffpeakHours, p.markettypecode, p1.AvgPeakWELMP - p.AvgPeakWELMP ,p.PeakWEHours " +
                //    "from NodeLMPDailys p join NodeLMPDailys p1 on p.MarketDate=p1.MarketDate and p.NodeKey= " + sourcenodekey + " and p1.NodeKey= " + sinknodekey + 
                //    " and p.MarketDate>='" + startDate + "' and p.MarketDate<='" + endDate.AddMonths(1) + "' and p1.MarketDate>= '" + startDate + "' and p1.MarketDate<=' " + endDate.AddMonths(1) +
                //    "'and p.markettypecode = p1.markettypecode order by p.markettypecode desc, p.MarketDate desc";

               /* mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakLMP - p.AvgPeakLMP,p1.AvgOffpeakLMP - p.AvgOffpeakLMP,p1.Avg24LMP - p.Avg24LMP,p.PeakHours , "
                                + " p.OffpeakHours, p.markettypecode, p1.AvgPeakWELMP - p.AvgPeakWELMP ,p.PeakWEHours from NodeLMPDailys p join NodeLMPDailys p1 "
                                + " on p.MarketDate = p1.MarketDate and p.NodeKey = " + sourcenodekey + " and p1.NodeKey = " + sinknodekey + " and p.MarketDate >= '" + startDate + "' and p.MarketDate <= '" + endDate.AddMonths(1) + "'  "
                                + " and p1.MarketDate >= '" + startDate + "' and p1.MarketDate <= '" + endDate.AddMonths(1) + "' and p.markettypecode = p1.markettypecode and p.markettypecode = 'rt' "
                                + " union "
                                + " select MarketDate, AvgPeakLMP, AvgOffpeakLMP, Avg24LMP, PeakHours, OffpeakHours, MarketTypeCode, AvgPeakWELMP,PeakWEHours "
                                + " from Vayu..NodeLMPDailysOPT (nolock) where SourceNodeKey = " + sourcenodekey + " and SinkNodeKey = " + sinknodekey + " and MarketDate>= '" + startDate + "' and MarketDate<= '" + endDate.AddMonths(1) + "'  "
                                + " union "
                                + " select MarketDate, AvgPeakLMP, AvgOffpeakLMP, Avg24LMP, PeakHours, OffpeakHours, MarketTypeCode, AvgPeakWELMP,PeakWEHours "
                                + " from Vayu..NodeLMPDailysOption (nolock) where SourceNodeKey = " + sourcenodekey + " and SinkNodeKey = " + sinknodekey + " and MarketDate>= '" + startDate + "' and MarketDate<= '" + endDate.AddMonths(1) + "'   order by p.markettypecode desc, "
                                + " p.MarketDate desc ";
                */

                mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakLMP - p.AvgPeakLMP,p1.AvgOffpeakLMP - p.AvgOffpeakLMP,p1.Avg24LMP - p.Avg24LMP,p.PeakHours , "
                               + " p.OffpeakHours, p.markettypecode, p1.AvgPeakWELMP - p.AvgPeakWELMP ,p.PeakWEHours from NodeLMPDailys p join NodeLMPDailys p1 "
                               + " on p.MarketDate = p1.MarketDate and p.NodeKey = " + sourcenodekey + " and p1.NodeKey = " + sinknodekey + " and p.MarketDate >= '" + startDate + "' and p.MarketDate <= '" + endDate + "'  "
                               + " and p1.MarketDate >= '" + startDate + "' and p1.MarketDate <= '" + endDate + "' and p.markettypecode = p1.markettypecode and p.markettypecode = 'rt' "
                               + " union "
                               + " select MarketDate, AvgPeakLMP, AvgOffpeakLMP, Avg24LMP, PeakHours, OffpeakHours, MarketTypeCode, AvgPeakWELMP,PeakWEHours "
                               + " from Vayu..NodeLMPDailysOPT (nolock) where SourceNodeKey = " + sourcenodekey + " and SinkNodeKey = " + sinknodekey + " and MarketDate>= '" + startDate + "' and MarketDate<= '" + endDate + "'  "
                               + " union "
                               + " select MarketDate, AvgPeakLMP, AvgOffpeakLMP, Avg24LMP, PeakHours, OffpeakHours, MarketTypeCode, AvgPeakWELMP,PeakWEHours "
                               + " from Vayu..NodeLMPDailysOption (nolock) where SourceNodeKey = " + sourcenodekey + " and SinkNodeKey = " + sinknodekey + " and MarketDate>= '" + startDate + "' and MarketDate<= '" + endDate + "'   order by p.markettypecode desc, "
                               + " p.MarketDate desc ";

                mSelectDailyValuesCommand.CommandTimeout = 300000;
                reader = mSelectDailyValuesCommand.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> monthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        Dictionary<int, DailyValues> daysHash = new Dictionary<int, DailyValues>();
                        DailyValues congValues = new DailyValues();
                        DateTime marketdate = reader.GetDateTime(0);
                        if (yearHash.ContainsKey(marketdate.Year))
                        {
                            monthHash = yearHash[marketdate.Year];
                        }
                        if (monthHash.ContainsKey(marketdate.Month))
                        {
                            daysHash = monthHash[marketdate.Month].Item2;
                        }
                        if (daysHash.ContainsKey(marketdate.Day))
                        {
                            congValues = daysHash[marketdate.Day];
                        }

                        string marketType = reader.GetString(6);
                        if (marketType == "DA")
                        {
                            congValues.DAPeakCong = reader.IsDBNull(1) ? (double?)0 : (double?)reader.GetFloat(1);
                            congValues.DAOffPeakCong = reader.IsDBNull(2) ? (double?)0 : (double?)reader.GetFloat(2);
                            congValues.DA24Cong = reader.IsDBNull(3) ? (double?)0 : (double?)reader.GetFloat((3));
                            congValues.DAPeakWECong = reader.IsDBNull(7) ? (double?)0 : (double?)reader.GetFloat((7));

                        }
                        else
                        {
                            congValues.RTPeakCong = reader.IsDBNull(1) ? (double?)null : (double?)reader.GetFloat(1);
                            congValues.RTOffPeakCong = reader.IsDBNull(2) ? (double?)null : (double?)reader.GetFloat(2);
                            congValues.RT24Cong = reader.IsDBNull(3) ? (double?)null : (double?)reader.GetFloat((3));
                            congValues.RTPeakWECong = reader.IsDBNull(7) ? (double?)null : (double?)reader.GetFloat((7));
                        }
                        congValues.PeakHours = Convert.ToInt32(reader.GetValue(4));
                        congValues.OffPeakHours = Convert.ToInt32(reader.GetValue(5));
                        congValues.PeakWEHours = Convert.ToInt32(reader.GetValue(8));
                        string date = marketdate.Year + "-" + marketdate.Month;
                        DailyValues priceDailyValues = new DailyValues();
                        if (auctionType == "Annual")
                        {
                            if (priceHash.ContainsKey(date))
                            {
                                priceDailyValues = priceHash[date];
                            }
                            else
                            {
                                DateTime priceDate = DateTime.Parse(marketdate.Year.ToString() + "/" + "06" + "/" + "01");
                                if (priceDate.Year == DateTime.Now.Year && !(priceHash.ContainsKey(priceDate.ToString())))
                                {
                                    priceHash.Add(priceDate.ToString("yyyy-MM"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    for (int i = 1; i < 6; i++)
                                    {
                                        if (!priceHash.ContainsKey(priceDate.Year + '-' + i.ToString()))
                                            priceHash.Add(priceDate.Year.ToString() + '-' + i.ToString(), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                    }
                                }
                                else
                                {
                                    if (!priceHash.ContainsKey(marketdate.ToString("yyyy-M")))
                                    {
                                        if (marketdate.Month >= 1 && marketdate.Month <= 5)
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.AddYears(-1).ToString("yyyy-M")]);
                                        else
                                            priceHash.Add(marketdate.ToString("yyyy-M"), priceHash[priceDate.ToString("yyyy-M")]);
                                    }
                                }

                                if (priceHash.ContainsKey(priceDate.ToString("yyyy-M")))
                                {

                                }

                            }
                        }
                        if (priceHash.ContainsKey(date))
                        {
                            priceDailyValues = priceHash[date];
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> monthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(priceDailyValues, daysHash);
                        daysHash.Remove(marketdate.Day);
                        daysHash.Add(marketdate.Day, congValues);
                        monthHash.Remove(marketdate.Month);
                        monthHash.Add(marketdate.Month, monthTuple);
                        yearHash.Remove(marketdate.Year);
                        yearHash.Add(marketdate.Year, monthHash);

                    }
                    catch (Exception ex)
                    {

                    }
                }

                foreach (var item in priceHash)
                {
                    Dictionary<int, DailyValues> tempdaysHash = new Dictionary<int, DailyValues>();

                    DateTime mdate = DateTime.Parse(item.Key);

                    if (!yearHash.ContainsKey(mdate.Year))
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        for (int i = 1; i <= 31; i++)// for (int i = 0; i < 31; i++)
                        {
                            tempdaysHash.Add(i, priceHash[item.Key]);
                        }
                        Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                        tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        yearHash.Add(mdate.Year, tempmonthHash);
                    }
                    else
                    {
                        DailyValues tempPriceValues = priceHash[item.Key];
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> tempmonthHash = yearHash[mdate.Year];
                        if (!tempmonthHash.ContainsKey(mdate.Month))
                        {
                            for (int i = 1; i <= 31; i++)
                            {
                                tempdaysHash.Add(i, priceHash[item.Key]);
                            }
                            Tuple<DailyValues, Dictionary<int, DailyValues>> tempmonthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(tempPriceValues, tempdaysHash);
                            tempmonthHash.Add(mdate.Month, tempmonthTuple);
                        }
                    }

                }
                reader.Close();


            }

            return yearHash;


        }
        #endregion

        public Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> FillFTRPathData(int Marketkey, long SourceId, long SinkId, DateTime startDate, DateTime endDate, string periodType, string Hedgetype, int ftrQuarterMonth = 0, string auctionType = "Monthly")
        {
            if (Marketkey == 9)
            {
                yearHash = getFtrErcotData(Marketkey, SourceId, SinkId, startDate, endDate, Hedgetype, ftrQuarterMonth = 0, auctionType = "Monthly");
            }

            return yearHash;
        }



    }
}


