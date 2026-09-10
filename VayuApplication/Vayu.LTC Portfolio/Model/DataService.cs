using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;

namespace Vayu.LTC_Portfolio.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The VayuDbConnection database connection
        /// </summary>
        private SqlConnection VayuConnection;

        private SqlCommand mSelectCRRErcotAuctionCommand;
        private SqlCommand mSelectNewPortfolioErcotFtrAuctionCommand;
        private SqlCommand mSelectDistinctErcotCRRCommand;
        private SqlCommand mSelectErcotPriceCommand;
        private SqlCommand mSelectErcotRTPriceCommand;
        #region SQL Commands

        /// <summary>
        /// The m select PJMCRR auction bids command
        /// </summary>
        private SqlCommand mSelectPJMFtrAuctionBidsCommand;
        /// <summary>
        /// The m select miso CRR auction bids command
        /// </summary>
        private SqlCommand mSelectMisoFtrAuctionBidsCommand;

        private SqlCommand mSelectConstraintContingecyCommand;
        /// <summary>
        /// The m select SPPCRR auction bids command
        /// </summary>
        private SqlCommand mSelectSPPFtrAuctionBidsCommand;
        /// <summary>
        /// The m select CRR portfolio command
        /// </summary>
        private SqlCommand mSelectCRRPortfolioCommand;
        /// <summary>
        /// The m select CRR auction command
        /// </summary>
        private SqlCommand mSelectFtrAuctionCommand;
        /// <summary>
        /// The m select distinct PJM CRR command
        /// </summary>
        private SqlCommand mSelectDistinctPjmCRRCommand;
        /// <summary>
        /// The m select distinct miso CRR command
        /// </summary>
        private SqlCommand mSelectDistinctMisoCRRCommand;
        /// <summary>
        /// The m select distinct SPP CRR command
        /// </summary>
        private SqlCommand mSelectDistinctSppCRRCommand;
        /// <summary>
        /// The m select iso command
        /// </summary>
        private SqlCommand mSelectIsoCommand;
        /// <summary>
        /// The m delete CRR command
        /// </summary>
        private SqlCommand mDeletePJMCRRCommand;
        /// <summary>
        /// The m select CRR bid command
        /// </summary>
        private SqlCommand mSelectFTRBidCommand;
        /// <summary>
        /// The m select SPP miso portfolio command
        /// </summary>
        private SqlCommand mSelectSppMisoPortfolioCommand;
        /// <summary>
        /// The m select SPP CRR identifier command
        /// </summary>
        private SqlCommand mSelectSPPCRRIdCommand;
        /// <summary>
        /// The m select caiso CRR auction bids command
        /// </summary>
        private SqlCommand mSelectCaisoFtrAuctionBidsCommand;
        /// <summary>
        /// The m select distinct caiso CRR command
        /// </summary>
        private SqlCommand mSelectDistinctCaisoCRRCommand;
        /// <summary>
        /// The m select miso CRR identifier command
        /// </summary>
        private SqlCommand mSelectMISOCRRIdCommand;
        /// <summary>
        /// The m select new portfolio miso CRR auction command
        /// </summary>
        private SqlCommand mSelectNewPortfolioMisoFtrAuctionCommand;
        /// <summary>
        /// The m select new portfolio caiso CRR auction command
        /// </summary>
        private SqlCommand mSelectNewPortfolioCaisoFtrAuctionCommand;
        /// <summary>
        /// The m select new portfoliopjm CRR auction command
        /// </summary>
        private SqlCommand mSelectNewPortfoliopjmFtrAuctionCommand;
        private SqlCommand mSelectNewPortfolioFtrAuctionCommand;
        /// <summary>
        /// The m select new portfolio SPP CRR auction command
        /// </summary>
        private SqlCommand mSelectNewPortfolioSppFtrAuctionCommand;
        /// <summary>
        /// The m select PJM cost command
        /// </summary>
        private SqlCommand mSelectPjmCostCommand;
        private SqlCommand mSelectErcotCostCommand;
        /// <summary>
        /// The m select PJM price command
        /// </summary>
        private SqlCommand mSelectPjmPriceCommand;
        private SqlCommand mSelectPjmRTPriceCommand;

        private SqlCommand mSelectSenstivityCommand;
        private SqlCommand mSelectSenstivityInfoCommand;
        private SqlCommand mSelectErcotOptionCostCommand;
        private SqlCommand mSelectPJMOptionCostCommand;




        /// <summary>
        /// The m select new Cleared mwh valuesn command
        /// </summary>
        private SqlCommand mSelectClearedMwhCommand;
        #endregion

        /// <summary>
        /// The m PJM load row
        /// </summary>
        private DataRow mPjmLoadRow = null;
        /// <summary>
        /// The m PJMCRRDT
        /// </summary>
        private DataTable mPJMCRRDT = new DataTable();
        private SqlCommand mDeleteErcotCRRCommand;

        private SqlCommand mselectCongestion;
        /// <summary>
        /// The s cost hash
        /// </summary>
        private static Dictionary<string, Dictionary<DateTime, Cost>> sCostHash = new Dictionary<string, Dictionary<DateTime, Cost>>();

        #endregion

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void LoadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();


            //
            mSelectMisoFtrAuctionBidsCommand = new SqlCommand();
            mSelectMisoFtrAuctionBidsCommand.CommandText = "Select b.bidstatuscode, b.AuctionKey, b.ISOCode, b.CategoryCode, b.RegionName, b.ConstraintName, b.MonitoredName, b.TraderName, b.BookName, " +
                        "b.SourceName, b.SinkName,(case when b.classcode= 'PEAK' then a.peakhrs when b.classcode= 'OFF-PEAK' then a.OffpeakHrs else (a.peakhrs + a.OffPeakHrs) end) " +
                        "as classcode,b.HedgeTypeCode, a.periodname, b.TradeTypeCode, b.BidStatusCode, b.RefPrice1, b.RefPrice2, b.RefMW, b.BidStrategyID, b.Credit, b.CRRId, " +
                        "b.RefAuctionPrice,b.Comments, b.MW1, b.PriceMwh1, b.MW2, b.PriceMwh2, b.MW3, b.PriceMwh3, b.MW4, b.PriceMwh4, b.MW5, b.PriceMwh5, b.MW6, b.PriceMwh6, " +
                        "b.ClassCode, a.startdate, a.periodkey from [dbo].FtrAuctionBids b join period a on a.PeriodKey= b.PeriodKey and a.MarketKey=2 " +
                        "where b.BidStatusCode='valid' and b.auctionkey = @auctionkey";
            mSelectMisoFtrAuctionBidsCommand.Parameters.AddWithValue("@auctionkey", "auctionkey");
            mSelectMisoFtrAuctionBidsCommand.Connection = VayuConnection;
            //
            mSelectConstraintContingecyCommand = new SqlCommand();
            mSelectConstraintContingecyCommand.CommandText = " select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor, marketdatetime, dollarimpact " +
                                                             " from  RTMasterConstraint as A (nolock) inner join RTImpact as B on A.ConstraintRTNum= B.ConstraintRTNum " +
                                                             " where B.Date >= @start and B.Date < @end";
            mSelectConstraintContingecyCommand.Parameters.AddWithValue("@start", "MarkedDateTime");
            mSelectConstraintContingecyCommand.Parameters.AddWithValue("@end", "MarkedDateTime");
            mSelectConstraintContingecyCommand.Connection = VayuConnection;
            //
            mSelectSenstivityCommand = new SqlCommand();
            mSelectSenstivityCommand.CommandText = "select NodeKey,Sensitivity, constraintrtnum from RTMasterVector_new (nolock) where ConstraintRTNum in " +
                                                   " (select ConstraintRTNum from  RTImpact(nolock)  where RTImpact.Date >= @start and RTImpact.Date < @end)";
            mSelectSenstivityCommand.Parameters.AddWithValue("@start", "MarkedDateTime");
            mSelectSenstivityCommand.Parameters.AddWithValue("@end", "MarkedDateTime");
            mSelectSenstivityCommand.Connection = VayuConnection;
            //
            mSelectPjmCostCommand = new SqlCommand();

            //mSelectPjmCostCommand.CommandText = "select startdate, lmponpeak, lmpoffpeak, peakhrs, offpeakhrs from pjm.FtrAuctionNodePrice a, Period b where MarketKey = @marketkey and " +
            //                                    "PeriodType = 'monthly' and a.PeriodKey = b.PeriodKey and NodeKey = @nodekey and a.FtrAuctionKey in (select distinct MAX(FtrAuctionkey) from " + 
            //                                    "pjm.FtrAuctionNodePrice where periodkey = b.periodkey group by periodkey) and b.StartDate >= @startdate and b.StartDate <= @enddate order by startdate";
            //
            mSelectPjmCostCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectPjmCostCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPjmCostCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPjmCostCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectPjmCostCommand.Connection = VayuConnection;

            mSelectErcotCostCommand = new SqlCommand();
            //
            mSelectErcotCostCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectErcotCostCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectErcotCostCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectErcotCostCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectErcotCostCommand.Connection = VayuConnection;



            mSelectPjmRTPriceCommand = new SqlCommand();
            mSelectPjmRTPriceCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPjmRTPriceCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPjmRTPriceCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectPjmRTPriceCommand.Connection = VayuConnection;

            mSelectErcotRTPriceCommand = new SqlCommand();
            mSelectErcotRTPriceCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectErcotRTPriceCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectErcotRTPriceCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectErcotRTPriceCommand.Connection = VayuConnection;

            //
            mSelectPjmPriceCommand = new SqlCommand();
            //mSelectPjmPriceCommand.CommandText = "select MarketDate, AvgPeakCong, AvgOffpeakCong, PeakHours, OffpeakHours from pjm.NodeLMPdaily where MarketTypeCode = 'da' and NodeKey = @nodekey and " +
            //                                      "MarketDate >= @startdate and MarketDate <= @enddate order by marketdate";
            mSelectPjmPriceCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPjmPriceCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPjmPriceCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectPjmPriceCommand.Connection = VayuConnection;


            mSelectErcotPriceCommand = new SqlCommand();
            mSelectErcotPriceCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectErcotPriceCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectErcotPriceCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectErcotPriceCommand.Connection = VayuConnection;
            //
            mSelectSPPFtrAuctionBidsCommand = new SqlCommand();
            mSelectSPPFtrAuctionBidsCommand.CommandText = "Select b.bidstatuscode, b.extAuctionKey, b.ISOCode, b.CategoryCode, b.RegionName, b.ConstraintName, b.MonitoredName, b.TraderName, b.BookName, " +
                        "b.SourceName, b.SinkName,(case when b.classcode= 'PEAK' then a.peakhrs when b.classcode= 'OFF-PEAK' then a.OffpeakHrs else (a.peakhrs + a.OffPeakHrs) end) " +
                        "as classcode,b.HedgeTypeCode, a.periodname, b.TradeTypeCode, b.BidStatusCode, b.RefPrice1, b.RefPrice2, b.RefMW, b.BidStrategyID, b.Credit, b.CRRId, " +
                        "b.RefAuctionPrice,b.Comments, b.MW1, b.PriceMwh1, b.MW2, b.PriceMwh2, b.MW3, b.PriceMwh3, b.MW4, b.PriceMwh4, b.MW5, b.PriceMwh5, b.MW6, b.PriceMwh6, " +
                        "b.ClassCode, a.startdate, a.periodkey from [dbo].FtrAuctionBids b join period a on a.PeriodKey= b.extPeriodKey and a.MarketKey=12 " +
                        "where b.BidStatusCode='valid' and b.extAuctionKey = @auctionkey";
            mSelectSPPFtrAuctionBidsCommand.Parameters.AddWithValue("@auctionkey", "auctionkey");
            mSelectSPPFtrAuctionBidsCommand.Connection = VayuConnection;
            //
            mSelectPJMFtrAuctionBidsCommand = new SqlCommand();
            mSelectPJMFtrAuctionBidsCommand.CommandText = "Select b.bidstatuscode, b.extAuctionKey, b.ISOCode, b.CategoryCode, b.RegionName, b.ConstraintName, b.MonitoredName, b.TraderName, b.BookName, " +
                        "b.SourceName, b.SinkName,(case when b.classcode= 'PEAK' then a.peakhrs when b.classcode= 'OFF-PEAK' then a.OffpeakHrs else (a.peakhrs + a.OffPeakHrs) end) " +
                        "as classcode,b.HedgeTypeCode, a.periodname, b.TradeTypeCode, b.BidStatusCode, b.RefPrice1, b.RefPrice2, b.RefMW, b.BidStrategyID, b.Credit, b.CRRId, " +
                        "b.RefAuctionPrice,b.Comments, b.MW1, b.PriceMwh1, b.MW2, b.PriceMwh2, b.MW3, b.PriceMwh3, b.MW4, b.PriceMwh4, b.MW5, b.PriceMwh5, b.MW6, b.PriceMwh6, " +
                        "b.ClassCode, a.startdate, a.periodkey from [dbo].FtrAuctionBids b join period a on a.PeriodKey= b.extPeriodKey and a.MarketKey=1 " +
                        "where b.BidStatusCode='valid' and b.extAuctionKey = @auctionkey and b.BookName  = @bookName";
            mSelectPJMFtrAuctionBidsCommand.Parameters.AddWithValue("@auctionkey", "auctionkey");
            mSelectPJMFtrAuctionBidsCommand.Parameters.AddWithValue("@bookName", "BookName ");
            mSelectPJMFtrAuctionBidsCommand.Connection = VayuConnection;
            //
            mSelectCaisoFtrAuctionBidsCommand = VayuConnection.CreateCommand();
            mSelectCaisoFtrAuctionBidsCommand.CommandText = "Select b.bidstatuscode, b.AuctionKey, b.ISOCode, b.CategoryCode, b.RegionName, b.ConstraintName, b.MonitoredName, b.TraderName, b.BookName, b.SourceName, b.SinkName, " +
                                                        " (case when b.classcode= 'PEAK' then a.PeakHrs when b.classcode= 'OFF-PEAK' then a.OffPeakHrs else a.[24Hrs] end) as classcode,b.HedgeTypeCode, " +
                                                        "  b.PeriodCode, b.TradeTypeCode, b.BidStatusCode, b.RefPrice1, b.RefPrice2, b.RefMW, b.BidStrategyID, b.Credit, b.CRRId, b.RefAuctionPrice,b.Comments, " +
                                                        "  b.MW1, b.PriceMwh1, b.MW2, b.PriceMwh2, b.MW3, b.PriceMwh3, b.MW4, b.PriceMwh4, b.MW5, b.PriceMwh5, b.MW6, b.PriceMwh6,  b.ClassCode, a.StartDate, a.periodKey from FtrAuctionBids b join Period a  " +
                                                        " on a.PeriodKey=b.PeriodKey where b.BidStatusCode='valid' and b.auctionkey = @auctionKey";
            mSelectCaisoFtrAuctionBidsCommand.Parameters.AddWithValue("@auctionKey", "");
            //
            mSelectSppMisoPortfolioCommand = new SqlCommand();
            mSelectSppMisoPortfolioCommand.CommandText = "select Portfolio_ID, Strip from Portfolio where Product = 'CRR' and Active = 'Y' And PORTFOLIO_ID<>3333 and hub=@hub  ";
            //mSelectSppMisoPortfolioCommand.CommandText = "select Portfolio_ID, Strip from Portfolio where Product = 'CRR' and Active = 'Y' And PORTFOLIO_ID=3338 and hub=@hub  ";
            mSelectSppMisoPortfolioCommand.Parameters.AddWithValue("@hub", "hub");
            mSelectSppMisoPortfolioCommand.Connection = VayuConnection;
            //
            mSelectDistinctPjmCRRCommand = new SqlCommand();
            mSelectDistinctPjmCRRCommand.CommandText = "select FtrAuctionKey, FtrAuctionName from PJM.FtrAuction where auctionenddate >= convert(date,getdate())";
            mSelectDistinctPjmCRRCommand.Parameters.AddWithValue("@isocode", "isocode");
            mSelectDistinctPjmCRRCommand.Connection = VayuConnection;
            //
            mSelectDistinctMisoCRRCommand = new SqlCommand();
            mSelectDistinctMisoCRRCommand.CommandText = "select FtrAuctionKey, FtrAuctionName from MISO.FtrAuction where auctionenddate >= convert(date,getdate())";
            mSelectDistinctMisoCRRCommand.Parameters.AddWithValue("@isocode", "isocode");
            mSelectDistinctMisoCRRCommand.Connection = VayuConnection;
            //
            mSelectDistinctSppCRRCommand = new SqlCommand();
            mSelectDistinctSppCRRCommand.CommandText = "select FtrAuctionKey, FtrAuctionName from SPP.FtrAuction where auctionenddate >= convert(date,getdate())";
            mSelectDistinctSppCRRCommand.Parameters.AddWithValue("@isocode", "isocode");
            mSelectDistinctSppCRRCommand.Connection = VayuConnection;
            //
            mSelectDistinctCaisoCRRCommand = new SqlCommand();
            mSelectDistinctCaisoCRRCommand.CommandText = "select FtrAuctionKey, FtrAuctionName from CAISO.FtrAuction where auctionenddate >= convert(date,getdate())";
            mSelectDistinctCaisoCRRCommand.Parameters.AddWithValue("@isocode", "isocode");
            mSelectDistinctCaisoCRRCommand.Connection = VayuConnection;

            mSelectDistinctErcotCRRCommand = new SqlCommand();
            mSelectDistinctErcotCRRCommand.CommandText = "select CRRAuctionKey, CRRAuctionName from CRRAuction where auctionenddate >= convert(date,getdate())";
            // mSelectDistinctErcotCRRCommand.CommandText = "select CRRAuctionKey, CRRAuctionName from CRRAuction  where CRRAuctionName not in (select auction from CRRBids where portfoliokey = 3338)  and CRRAuctionType='Monthly' and auctionenddate <= convert(date,getdate()) order by CRRAuctionName";
            mSelectDistinctErcotCRRCommand.Parameters.AddWithValue("@isocode", "isocode");
            mSelectDistinctErcotCRRCommand.Connection = VayuConnection;

            //
            mSelectFtrAuctionCommand = new SqlCommand();
            mSelectFtrAuctionCommand.CommandText = "select distinct auction,[Month] from FTRBids where marketkey = @marketkey order by [Month] desc";
            mSelectFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectFtrAuctionCommand.Connection = VayuConnection;
            //
            mSelectNewPortfolioMisoFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioMisoFtrAuctionCommand.CommandText = "select distinct(Auction) from FTRBids where MarketKey=@marketkey and Auction in(select FtrAuctionName from MISO.FtrAuction where convert(date, GETDATE()) between AuctionStartDate and AuctionEndDate )";
            mSelectNewPortfolioMisoFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNewPortfolioMisoFtrAuctionCommand.Connection = VayuConnection;
            //
            mSelectNewPortfoliopjmFtrAuctionCommand = new SqlCommand();
            //mSelectNewPortfoliopjmFtrAuctionCommand.CommandText = "select distinct(Auction) from FTRBids where MarketKey = @marketkey and Auction in (select FtrAuctionName from PJM.FtrAuction where convert(date, GETDATE()) between AuctionStartDate and AuctionEndDate )";
            mSelectNewPortfoliopjmFtrAuctionCommand.CommandText = "select distinct(Auction) from FTRBids where MarketKey = @marketkey and Auction in (select FtrAuctionName from PJM.FtrAuction where convert(date, GETDATE()) between DATEADD(DD, -1, AuctionStartDate) and AuctionEndDate )";
            mSelectNewPortfoliopjmFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNewPortfoliopjmFtrAuctionCommand.Connection = VayuConnection;
            //
            mSelectNewPortfolioCaisoFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioCaisoFtrAuctionCommand.CommandText = "select distinct(Auction) from FTRBids where MarketKey=@marketkey and Auction in(select FtrAuctionName from CAISO.FtrAuction where convert(date, GETDATE()) between AuctionStartDate and AuctionEndDate )";
            mSelectNewPortfolioCaisoFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNewPortfolioCaisoFtrAuctionCommand.Connection = VayuConnection;
            //
            mSelectNewPortfolioSppFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioSppFtrAuctionCommand.CommandText = "select distinct(Auction) from FTRBids where MarketKey=@marketkey and Auction in(select FtrAuctionName from SPP.FtrAuction where convert(date, GETDATE()) between AuctionStartDate and AuctionEndDate )";
            mSelectNewPortfolioSppFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNewPortfolioSppFtrAuctionCommand.Connection = VayuConnection;
            //
            mSelectSPPCRRIdCommand = new SqlCommand();
            mSelectSPPCRRIdCommand.CommandText = "select CRRid, clearedmw from SPP.FtrAuctionResults where Participant = 'Sigma' and sourcenode = @sourcenode and sinknode = @sinknode " +
                                                    "and classtype = @classtype and periodkey = @periodkey order by sourcenode, sinknode, classtype, clearedmw";
            mSelectSPPCRRIdCommand.Parameters.AddWithValue("@sourcenode", "sourcenode");
            mSelectSPPCRRIdCommand.Parameters.AddWithValue("@sinknode", "sinknode");
            mSelectSPPCRRIdCommand.Parameters.AddWithValue("@classtype", "classtype");
            mSelectSPPCRRIdCommand.Parameters.AddWithValue("@periodkey", "periodkey");
            mSelectSPPCRRIdCommand.Connection = VayuConnection;
            //
            mSelectMISOCRRIdCommand = new SqlCommand();
            mSelectMISOCRRIdCommand.CommandText = "select CRRid, clearedmw from MISO.FtrAuctionResults where Participant = 'Sigma' and sourcenode = @sourcenode and sinknode = @sinknode " +
                                                    "and classtype = @classtype and periodkey = @periodkey order by sourcenode, sinknode, classtype, clearedmw";
            mSelectMISOCRRIdCommand.Parameters.AddWithValue("@sourcenode", "sourcenode");
            mSelectMISOCRRIdCommand.Parameters.AddWithValue("@sinknode", "sinknode");
            mSelectMISOCRRIdCommand.Parameters.AddWithValue("@classtype", "classtype");
            mSelectMISOCRRIdCommand.Parameters.AddWithValue("@periodkey", "periodkey");
            mSelectMISOCRRIdCommand.Connection = VayuConnection;
            //
            mSelectCRRPortfolioCommand = new SqlCommand();
            mSelectCRRPortfolioCommand.CommandText = "select PORTFOLIO_ID, STRIP from PORTFOLIO where PORTFOLIO_ID in (select PortfolioKey from FTRBids where auction = @auction and marketkey = @marketkey)";
            mSelectCRRPortfolioCommand.Parameters.AddWithValue("@auction", "auction");
            mSelectCRRPortfolioCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectCRRPortfolioCommand.Connection = VayuConnection;
            //
            mPJMCRRDT.Columns.Add("Participant", typeof(string));
            mPJMCRRDT.Columns.Add("Month", typeof(DateTime));
            mPJMCRRDT.Columns.Add("Auction", typeof(string));
            mPJMCRRDT.Columns.Add("PeriodType", typeof(string));
            mPJMCRRDT.Columns.Add("ClassType", typeof(string));
            mPJMCRRDT.Columns.Add("TradeType", typeof(string));
            mPJMCRRDT.Columns.Add("HedgeType", typeof(string));
            mPJMCRRDT.Columns.Add("MW1", typeof(decimal));
            mPJMCRRDT.Columns.Add("Price1", typeof(decimal));
            mPJMCRRDT.Columns.Add("MW2", typeof(decimal));
            mPJMCRRDT.Columns.Add("Price2", typeof(decimal));
            mPJMCRRDT.Columns.Add("MW3", typeof(decimal));
            mPJMCRRDT.Columns.Add("Price3", typeof(decimal));
            mPJMCRRDT.Columns.Add("MW4", typeof(decimal));
            mPJMCRRDT.Columns.Add("Price4", typeof(decimal));
            mPJMCRRDT.Columns.Add("MW5", typeof(decimal));
            mPJMCRRDT.Columns.Add("Price5", typeof(decimal));
            mPJMCRRDT.Columns.Add("MW6", typeof(decimal));
            mPJMCRRDT.Columns.Add("Price6", typeof(decimal));
            mPJMCRRDT.Columns.Add("PortfolioKey", typeof(int));
            mPJMCRRDT.Columns.Add("MarketKey", typeof(int));
            mPJMCRRDT.Columns.Add("PeriodHours", typeof(int));
            mPJMCRRDT.Columns.Add("PeriodName", typeof(string));
            mPJMCRRDT.Columns.Add("Source", typeof(string));
            mPJMCRRDT.Columns.Add("Sink", typeof(string));
            mPJMCRRDT.Columns.Add("Status", typeof(string));
            mPJMCRRDT.Columns.Add("TcrId", typeof(int));
            mPJMCRRDT.Columns.Add("PeriodKey", typeof(int));
            //
            mDeletePJMCRRCommand = new SqlCommand();
            mDeletePJMCRRCommand.CommandText = "delete [dbo].[FTRBids] where status <> 'valid' and portfoliokey = @portfoliokey";
            mDeletePJMCRRCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mDeletePJMCRRCommand.Connection = VayuConnection;
            //

            mDeleteErcotCRRCommand = new SqlCommand();
            mDeleteErcotCRRCommand.CommandText = "delete CRRBids where status <> 'valid' and portfoliokey = @portfoliokey";
            mDeleteErcotCRRCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mDeleteErcotCRRCommand.Connection = VayuConnection;

            mSelectIsoCommand = new SqlCommand();
            mSelectIsoCommand.CommandText = "select isoauctionname from FtrAuctions where auctionname = @auctionname";
            mSelectIsoCommand.Parameters.AddWithValue("@auctionname", "auctionname");
            mSelectIsoCommand.Connection = VayuConnection;
            //
            mSelectNewPortfolioFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioFtrAuctionCommand.CommandText = "select distinct(Auction) from CRRBids where MarketKey = @Marketkey and Auction in (select CRRAuctionName from CRRAuction where convert(date, GETDATE()) between DATEADD(DD, -1, AuctionStartDate) and AuctionEndDate)";
            mSelectNewPortfolioFtrAuctionCommand.Parameters.AddWithValue("@Marketkey", "MarketKey");
            mSelectNewPortfolioFtrAuctionCommand.Connection = VayuConnection;

            //
            mSelectNewPortfolioErcotFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioErcotFtrAuctionCommand.CommandText = "select distinct(Auction) from FTRBids where MarketKey=@marketkey and Auction in(select FtrAuctionName from FtrAuction where convert(date, GETDATE()) between AuctionStartDate and AuctionEndDate )";
            mSelectNewPortfolioErcotFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNewPortfolioErcotFtrAuctionCommand.Connection = VayuConnection;


            mSelectErcotOptionCostCommand = new SqlCommand();
            mSelectErcotOptionCostCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectErcotOptionCostCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectErcotOptionCostCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectErcotOptionCostCommand.Parameters.AddWithValue("@sourcekey", "sourcekey");
            mSelectErcotOptionCostCommand.Parameters.AddWithValue("@sinkkey", "sinkkey");
            mSelectErcotOptionCostCommand.Connection = VayuConnection;

            mSelectPJMOptionCostCommand = new SqlCommand();
            mSelectPJMOptionCostCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectPJMOptionCostCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectPJMOptionCostCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectPJMOptionCostCommand.Parameters.AddWithValue("@sourcekey", "sourcekey");
            mSelectPJMOptionCostCommand.Parameters.AddWithValue("@sinkkey", "sinkkey");
            mSelectPJMOptionCostCommand.Connection = VayuConnection;






        }
        /// <summary>
        /// Gets the SPP miso portfolio list.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        /// 
        public Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivity(DateTime startDate, DateTime endDate, bool IsDollar)
        {
            Dictionary<int, Dictionary<int, Sensitivity>> nodeHash = new Dictionary<int, Dictionary<int, Sensitivity>>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<int, Tuple<string, string, double, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, double>>();
            mSelectConstraintContingecyCommand.Parameters["@start"].Value = startDate;
            mSelectConstraintContingecyCommand.Parameters["@end"].Value = endDate;
            SqlDataReader reader = mSelectConstraintContingecyCommand.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    string constraint = reader.GetString(0);
                    constraint = constraint.Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").Replace("Reacinf-ctg", "").TrimStart();
                    string contingency = reader.GetString(1);
                    contingency = contingency.Replace("Contingency", "").TrimStart();
                    int constraintRtNum = (int)reader.GetDecimal(2);
                    double shiftFactor = (double)reader.GetDecimal(3);
                    DateTime marketDateTime = reader.GetDateTime(4);
                    double dollarImact = reader.IsDBNull(5) ? 0 : (double)reader.GetDecimal(5);
                    Tuple<string, string, double, double> tuple = new Tuple<string, string, double, double>(constraint, contingency, shiftFactor, dollarImact);
                    if (!constraintContingencyHash.ContainsKey(constraintRtNum))
                    {
                        constraintContingencyHash.Add(constraintRtNum, tuple);
                    }
                }
                catch
                {
                }
            }
            reader.Close();
            mSelectSenstivityCommand.Parameters["@start"].Value = startDate;
            mSelectSenstivityCommand.Parameters["@end"].Value = endDate;
            reader = mSelectSenstivityCommand.ExecuteReader();
            while (reader.Read())
            {
                int node = (int)reader.GetDecimal(0);
                double sensivityValue = (double)reader.GetDecimal(1);
                int constraintNum = (int)reader.GetDecimal(2);
                if (!constraintContingencyHash.ContainsKey(constraintNum))
                {
                    continue;
                }
                Tuple<string, string, double, double> tuple = constraintContingencyHash[constraintNum];
                Dictionary<int, Sensitivity> sensitivityHash = new Dictionary<int, Sensitivity>();
                if (nodeHash.ContainsKey(node))
                {
                    sensitivityHash = nodeHash[node];
                    nodeHash.Remove(node);
                }
                Sensitivity sensitivity = new Sensitivity();
                sensitivity.SensitivityValue = sensivityValue;
                sensitivity.ID = constraintNum;
                sensitivity.Constraint = tuple.Item1;
                sensitivity.Contingency = tuple.Item2;
                sensitivity.ShiftFactor = tuple.Item3;
                sensitivity.DollarImpact = tuple.Item4;
                sensitivityHash.Add(constraintNum, sensitivity);
                nodeHash.Add(node, sensitivityHash);
            }
            reader.Close();
            VayuConnection.Close();
            return nodeHash;
        }

        public Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivity(DateTime startDate, DateTime endDate, bool IsDollar, string predectedType, string nodeKeyString)
        {
            Dictionary<int, Dictionary<int, Sensitivity>> nodeHash = new Dictionary<int, Dictionary<int, Sensitivity>>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            string outageCmdText = string.Empty;
            if (predectedType == "NEXTMONTH")
            {
                outageCmdText = "select distinct ConstraintName , Contingency , ConstraintRTNum , ShiftFactor , DollarImpact from PJM_rt_outages outage  join pjm.ConstraintOutages maping on " +
                " outage.Equipment = maping.Driver 	join RTMasterConstraint constraintMaster on maping.ConstraintName = constraintMaster.MonitoredText and maping.Contingency = constraintMaster.ContingencyText " +
                 "  where StartDate between  @start and @MonthEnd and EndDate > @EndDate and RemovedDate is null 	and OutageStatus in ('Active' , 'Approved' , 'Received' , 'Revised')and maping.Contingency is not null and maping.Contingency != '' ";
            }
            else if (predectedType == "ONGOING")
            {
                outageCmdText = "select distinct ConstraintName , Contingency , ConstraintRTNum , ShiftFactor , DollarImpact from PJM_rt_outages outage  join pjm.ConstraintOutages maping on " +
                " outage.Equipment = maping.Driver 	join RTMasterConstraint constraintMaster on maping.ConstraintName = constraintMaster.MonitoredText and maping.Contingency = constraintMaster.ContingencyText " +
                "  where StartDate <  @start  and EndDate > @EndDate and RemovedDate is null 	and OutageStatus in ('Active' , 'Approved' , 'Received' , 'Revised')and maping.Contingency is not null and maping.Contingency != '' ";

            }
            else if (predectedType == "BINDING")
            {
                outageCmdText = "select distinct c.MonitoredText , c.ContingencyText , c.ConstraintRTNum , c.ShiftFactor , c.DollarImpact,a.MarketDateTime , p.PeakHrs , p.OffPeakHrs , p.PeriodKey from pjm.CRRRTBindingConstraints a   join   pjm.ConstraintRT b " +
                    " on a.ConstraintText = b.ConstraintText and a.contingencytext = b.contingencytext join RTMasterConstraint c on a.ConstraintText = c.MonitoredText and a.contingencytext = c.contingencytext ";
            }

            SqlCommand mSelectOutageConstraintContingecyCommand = new SqlCommand();
            Dictionary<int, Tuple<string, string, double, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, double>>();
            mSelectOutageConstraintContingecyCommand.CommandText = outageCmdText;
            mSelectOutageConstraintContingecyCommand.Parameters.AddWithValue("@start", startDate);
            mSelectOutageConstraintContingecyCommand.Parameters.AddWithValue("@EndDate", startDate);
            mSelectOutageConstraintContingecyCommand.Parameters.AddWithValue("@MonthEnd", endDate.AddDays(-1));
            mSelectOutageConstraintContingecyCommand.Connection = VayuConnection;
            SqlDataReader reader = mSelectOutageConstraintContingecyCommand.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    string constraint = reader.GetString(0);
                    constraint = constraint.Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").Replace("Reacinf-ctg", "").TrimStart();
                    string contingency = reader.GetString(1);
                    contingency = contingency.Replace("Contingency", "").TrimStart();
                    int constraintRtNum = (int)reader.GetDecimal(2);
                    double shiftFactor = (double)reader.GetDecimal(3);
                    //  DateTime marketDateTime = reader.GetDateTime(4);
                    double dollarImact = reader.IsDBNull(4) ? 0 : (double)reader.GetDecimal(4);
                    Tuple<string, string, double, double> tuple = new Tuple<string, string, double, double>(constraint, contingency, shiftFactor, dollarImact);
                    if (!constraintContingencyHash.ContainsKey(constraintRtNum))
                    {
                        constraintContingencyHash.Add(constraintRtNum, tuple);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            reader.Close();
            string sensetivityCmdString = string.Empty;
            if (predectedType == "NEXTMONTH")
            {
                sensetivityCmdString = "select NodeKey,Sensitivity, constraintrtnum from RTMasterVector_new (nolock) where ConstraintRTNum in ( " +
               "select distinct ConstraintRTNum  from PJM_rt_outages outage  join pjm.ConstraintOutages maping on " +
                " outage.Equipment = maping.Driver 	join RTMasterConstraint constraintMaster on maping.ConstraintName = constraintMaster.MonitoredText and maping.Contingency = constraintMaster.ContingencyText " +
                 "  where StartDate between  @start and @MonthEnd and EndDate > @EndDate and RemovedDate is null 	and OutageStatus in ('Active' , 'Approved' , 'Received' , 'Revised')and maping.Contingency is not null and maping.Contingency != '' ) " +
                 " and nodekey in ( " + nodeKeyString + ")";
            }
            else if (predectedType == "ONGOING")
            {
                sensetivityCmdString = "select NodeKey,Sensitivity, constraintrtnum from RTMasterVector_new (nolock) where ConstraintRTNum in ( " +
               "select distinct ConstraintRTNum  from PJM_rt_outages outage  join pjm.ConstraintOutages maping on " +
                " outage.Equipment = maping.Driver 	join RTMasterConstraint constraintMaster on maping.ConstraintName = constraintMaster.MonitoredText and maping.Contingency = constraintMaster.ContingencyText " +
                 "  where StartDate <  @start  and EndDate > @EndDate and RemovedDate is null 	and OutageStatus in ('Active' , 'Approved' , 'Received' , 'Revised')and maping.Contingency is not null and maping.Contingency != '' ) " +
                 " and nodekey in ( " + nodeKeyString + ")";

            }
            else if (predectedType == "BINDING")
            {
                sensetivityCmdString = "select nodekey , sensitivity , constraintrtnum from RTMasterVector_new where constraintrtnum in (select distinct ConstraintRTNum " +
                    " from pjm.CRRRTBindingConstraints a  join   pjm.ConstraintRT b on a.ConstraintText = b.ConstraintText and a.contingencytext = b.contingencytext " +
                    " join RTMasterConstraint c on a.ConstraintText = c.MonitoredText and a.contingencytext = c.contingencytext) and nodekey in ( " + nodeKeyString + ") ";
            }
            SqlCommand mSelectOutageSenstivityCommand = new SqlCommand();
            mSelectOutageSenstivityCommand.Connection = VayuConnection;
            mSelectOutageSenstivityCommand.CommandText = sensetivityCmdString;
            mSelectOutageSenstivityCommand.Parameters.AddWithValue("@start", startDate);
            mSelectOutageSenstivityCommand.Parameters.AddWithValue("@EndDate", startDate);
            mSelectOutageSenstivityCommand.Parameters.AddWithValue("@MonthEnd", endDate.AddDays(-1));
            reader = mSelectOutageSenstivityCommand.ExecuteReader();
            while (reader.Read())
            {
                int node = (int)reader.GetDecimal(0);
                double sensivityValue = (double)reader.GetDecimal(1);
                int constraintNum = (int)reader.GetDecimal(2);
                if (!constraintContingencyHash.ContainsKey(constraintNum))
                {
                    continue;
                }
                Tuple<string, string, double, double> tuple = constraintContingencyHash[constraintNum];
                Dictionary<int, Sensitivity> sensitivityHash = new Dictionary<int, Sensitivity>();
                if (nodeHash.ContainsKey(node))
                {
                    sensitivityHash = nodeHash[node];
                    nodeHash.Remove(node);
                }
                Sensitivity sensitivity = new Sensitivity();
                sensitivity.SensitivityValue = sensivityValue;
                sensitivity.ID = constraintNum;
                sensitivity.Constraint = tuple.Item1;
                sensitivity.Contingency = tuple.Item2;
                sensitivity.ShiftFactor = tuple.Item3;
                sensitivity.DollarImpact = tuple.Item4;
                sensitivityHash.Add(constraintNum, sensitivity);
                if (sensitivity.SensitivityValue < 1 && sensitivity.SensitivityValue > -1)
                {
                    nodeHash.Add(node, sensitivityHash);
                }
            }
            reader.Close();
            VayuConnection.Close();
            return nodeHash;
        }


        #region New Methods

        public Dictionary<int, Dictionary<int, double>> GetSensitivityInfo(DateTime startDate, DateTime endDate, string predectedType, string market, string rtorda)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<int, Dictionary<int, double>> constraintHash = new Dictionary<int, Dictionary<int, double>>();
            if (market == "ERCOT")
            {
                if (rtorda == "DA")
                {
                    mSelectSenstivityInfoCommand = new SqlCommand();
                    mSelectSenstivityInfoCommand.CommandText = "select constraintrtnum  , nodekey , sensitivity from DAMasterVector (nolock) where ConstraintRTNum in " +
                                             " (select ConstraintRTNum from  DAImpact as DAI(nolock) where DAI.Date >=@start and DAI.Date < @end)";
                }
                else
                {
                    mSelectSenstivityInfoCommand = new SqlCommand();
                    mSelectSenstivityInfoCommand.CommandText = "select constraintrtnum  , nodekey , sensitivity from RTMasterVector_new (nolock) where ConstraintRTNum in " +
                                           " (select ConstraintRTNum from RTImpact(nolock)  where RTImpact.Date >= @start and RTImpact.Date < @end)";
                }

                mSelectSenstivityInfoCommand.Parameters.AddWithValue("@start", startDate);
                mSelectSenstivityInfoCommand.Parameters.AddWithValue("@end", endDate);
                mSelectSenstivityInfoCommand.Connection = VayuConnection;
            }


            // 
            SqlDataReader reader = mSelectSenstivityInfoCommand.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    int constraintnum = (int)reader.GetDecimal(0);
                    int nodekey = (int)reader.GetDecimal(1);
                    double sensivity = (double)reader.GetDecimal(2);
                    Dictionary<int, double> nodeHash = new Dictionary<int, double>();
                    if (constraintHash.ContainsKey(constraintnum))
                    {
                        nodeHash = constraintHash[constraintnum];
                        constraintHash.Remove(constraintnum);
                    }
                    if (nodeHash.ContainsKey(nodekey))
                    {
                        sensivity = nodeHash[nodekey];
                        nodeHash.Remove(nodekey);
                    }
                    nodeHash.Add(nodekey, sensivity);
                    constraintHash.Add(constraintnum, nodeHash);
                }
                catch
                {
                }
            }
            mSelectSenstivityInfoCommand.Parameters.Clear();
            reader.Close();
            VayuConnection.Close();
            return constraintHash;

        }

        public List<ConstraintContingency> GetConstraintContingencyList(DateTime startDate, DateTime endDate, string predectedType, string market, string rtorda)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            string outageCmdText = string.Empty;
            SqlCommand mSelectOutageConstraintContingecyCommand = new SqlCommand();
            List<ConstraintContingency> lstConstraintContingency = new List<ConstraintContingency>();
            if (market == "PJM")
            {
                if (predectedType == "NEXTMONTH")
                {
                    outageCmdText = "select distinct constraintname, contingency, ConstraintRTNum , ShiftFactor, dollarimpact from pjm.constraintoutages, rtmasterconstraint "
                           + "where driver in (select distinct equipment from pjm_rt_outages where enddate >= @EndDate and startdate < @EndDate and startdate <= @StartDate) " // 
                           + "and monitoredtext = constraintname and contingencytext = contingency";
                    //outageCmdText = "select distinct ConstraintName , Contingency , ConstraintRTNum , ShiftFactor , DollarImpact from PJM_rt_outages outage  join pjm.ConstraintOutages maping on " +
                    //" outage.Equipment = maping.Driver 	join RTMasterConstraint constraintMaster on maping.ConstraintName = constraintMaster.MonitoredText and maping.Contingency = constraintMaster.ContingencyText " +
                    // "  where StartDate between  @start and @MonthEnd and EndDate > @EndDate and RemovedDate is null 	and OutageStatus in ('Active' , 'Approved' , 'Received' , 'Revised')and maping.Contingency is not null and maping.Contingency != '' ";
                }
                else if (predectedType == "ONGOING")
                {
                    outageCmdText = "select distinct constraintname, contingency, ConstraintRTNum , ShiftFactor, dollarimpact from pjm.constraintoutages, rtmasterconstraint "
                            + "where driver in (select distinct equipment from pjm_rt_outages where enddate >= @EndDate) "
                            + "and monitoredtext = constraintname and contingencytext = contingency";
                }
                else if (predectedType == "MONTH")
                {
                    //outageCmdText = "select distinct constraintname, contingency, ConstraintRTNum , ShiftFactor, dollarimpact from pjm.constraintoutages, rtmasterconstraint "
                    //        + "where driver in (select distinct equipment from pjm_rt_outages where enddate >= @EndDate and startdate < @StartDate) "
                    //        + "and monitoredtext = constraintname and contingencytext = contingency";

                    outageCmdText = "select distinct ConstraintText, a.ContingencyText, ConstraintRTNum, ShiftFactor, dollarimpact from PJM.ConstraintRT a "
                                    + "JOIN RTMasterConstraint b ON b.MonitoredText = a.ConstraintText and b.ContingencyText = a.ContingencyText "
                                    + "where a.MarketDateTime >= @StartDate and a.MarketDateTime < @EndDate ";
                }
                else if (predectedType == "BINDING")
                {
                    outageCmdText = "select distinct c.MonitoredText , c.ContingencyText , c.ConstraintRTNum , c.ShiftFactor , c.DollarImpact,a.MarketDateTime , p.PeakHrs , p.OffPeakHrs , p.PeriodKey from pjm.CRRRTBindingConstraints a   join   pjm.ConstraintRT b " +
                                    " on a.ConstraintText = b.ConstraintText and a.contingencytext = b.contingencytext join RTMasterConstraint c on a.ConstraintText = c.MonitoredText and a.contingencytext = c.contingencytext " +
                                    " join Period p on a.MarketDateTime = p.StartDate and a.enddate = p.EndDate " +
                                     "Where a.MarketDateTime>=@StartDate and a.MarketDateTime<=@EndDate "; // and  c.ConstraintRTNum = 4257  ";

                }



                //Dictionary<int, Tuple<string, string, double, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, double>>();


                mSelectOutageConstraintContingecyCommand.CommandText = outageCmdText;
                mSelectOutageConstraintContingecyCommand.Parameters.AddWithValue("@EndDate", endDate);
                if (predectedType == "NEXTMONTH" || predectedType == "MONTH" || predectedType == "BINDING")
                {
                    mSelectOutageConstraintContingecyCommand.Parameters.AddWithValue("@StartDate", startDate);
                }
                mSelectOutageConstraintContingecyCommand.Connection = VayuConnection;
            }
            else
            {
                if (rtorda == "DA")
                {
                    mSelectOutageConstraintContingecyCommand.CommandText = "select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor,dollarimpact, marketdatetime  " +
                                                    " from DAMasterConstraint as A (nolock) inner join DAImpact as B on A.ConstraintRTNum= B.ConstraintRTNum  " +
                                                    " where B.Date >@start and B.Date <= @end";
                }
                else
                {
                    mSelectOutageConstraintContingecyCommand.CommandText = " select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor,dollarimpact, marketdatetime  " +
                                                     " from RTMasterConstraint as A (nolock) inner join RTImpact as B on A.ConstraintRTNum= B.ConstraintRTNum " +
                                                     " where B.Date >= @start and B.Date < @end";
                }
                //mSelectOutageConstraintContingecyCommand.Parameters["@start"].Value = startDate;
                //mSelectOutageConstraintContingecyCommand.Parameters["@end"].Value = endDate;
                mSelectOutageConstraintContingecyCommand.Parameters.AddWithValue("@start", startDate);
                mSelectOutageConstraintContingecyCommand.Parameters.AddWithValue("@end", endDate);

                mSelectOutageConstraintContingecyCommand.Connection = VayuConnection;
            }
            SqlDataReader reader = mSelectOutageConstraintContingecyCommand.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    ConstraintContingency objConstraintContingency = new ConstraintContingency();

                    string constraint = reader.GetString(0);
                    constraint = constraint.Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").Replace("Reacinf-ctg", "").TrimStart();
                    objConstraintContingency.Constraint = constraint;

                    string contingency = reader.GetString(1);
                    contingency = contingency.Replace("Contingency", "").TrimStart();
                    objConstraintContingency.Contingency = contingency;

                    objConstraintContingency.ConstraintRTNum = (int)reader.GetDecimal(2);
                    objConstraintContingency.ShiftFactor = reader.IsDBNull(3) ? 0 : (double)reader.GetDecimal(3);
                    objConstraintContingency.DollarImpact = reader.IsDBNull(4) ? 0 : (double)reader.GetDecimal(4);
                    objConstraintContingency.Date = reader.GetDateTime(5);
                    if (predectedType == "BINDING")
                    {
                        //objConstraintContingency.Month = GetMonth(reader.GetDateTime(5));
                        //objConstraintContingency.PeakHours = Convert.ToInt32(reader.GetValue(6));
                        //objConstraintContingency.OffPeakHours = Convert.ToInt32(reader.GetValue(7));
                        //objConstraintContingency.PeriodKey = Convert.ToInt32(reader.GetValue(8));
                    }
                    lstConstraintContingency.Add(objConstraintContingency);

                    //Tuple<string, string, double, double> tuple = new Tuple<string, string, double, double>(constraint, contingency, shiftFactor, dollarImact);
                    //if (!constraintContingencyHash.ContainsKey(constraintRtNum))
                    //{
                    //    constraintContingencyHash.Add(constraintRtNum, tuple);
                    //}
                }
                catch (Exception ex)
                {
                }
            }
            reader.Close();
            return lstConstraintContingency;
        }

        #endregion

        public string GetMonth(DateTime date)
        {
            string Month = string.Empty;
            int monthid = date.Month;
            switch (monthid)
            {
                case 1:
                    Month = "JANUARY";
                    break;
                case 2:
                    Month = "FEBRUARY";
                    break;
                case 3:
                    Month = "MARCH";
                    break;
                case 4:
                    Month = "APRIL";
                    break;
                case 5:
                    Month = "MAY";
                    break;
                case 6:
                    Month = "JUNE";
                    break;
                case 7:
                    Month = "JULY";
                    break;
                case 8:
                    Month = "AUGUST";
                    break;
                case 9:
                    Month = "SEPTEMBER";
                    break;
                case 10:
                    Month = "OCTOBER";
                    break;
                case 11:
                    Month = "NOVEMBER";
                    break;
                case 12:
                    Month = "DECEMBER";
                    break;
                default:
                    break;
            }
            return Month;
        }

        public List<Tuple<int, string>> GetSppMisoPortfolioList(string market)
        {
            List<Tuple<int, string>> portfolioList = new List<Tuple<int, string>>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectSppMisoPortfolioCommand.Parameters["@hub"].Value = market;
            SqlDataReader reader = mSelectSppMisoPortfolioCommand.ExecuteReader();
            while (reader.Read())
            {
                Tuple<int, string> portfolioTuple = new Tuple<int, string>((int)reader.GetValue(0), reader.GetString(1));
                portfolioList.Add(portfolioTuple);
            }
            reader.Close();
            VayuConnection.Close();
            return portfolioList;
        }
        /// <summary>
        /// Gets the cost.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="sourceKey">The source key.</param>
        /// <param name="sinkKey">The sink key.</param>
        /// <param name="isMustTake">if set to <c>true</c> [is must take].</param>
        /// <returns></returns>
        public Dictionary<DateTime, Cost> GetCost(int marketKey, DateTime startDate, DateTime endDate, int sourceKey, int sinkKey, bool isMustTake)
        {
            if (sinkKey == 6316)
            {

            }
            sCostHash.Clear();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<DateTime, Cost> sourceCostHash = new Dictionary<DateTime, Cost>();
            Dictionary<DateTime, Cost> sinkCostHash = new Dictionary<DateTime, Cost>();
            for (int i = 0; i < 2; i++)
            {
                int nodeKey = i == 0 ? sourceKey : sinkKey;
                string costKey = marketKey + ":" + nodeKey + ":" + startDate + ":" + endDate;
                Cost tempCost = new Cost();
                Dictionary<DateTime, Cost> costHash = new Dictionary<DateTime, Cost>();
                if (i == 0)
                {
                    costHash = sourceCostHash;
                }
                else
                {
                    costHash = sinkCostHash;
                }
                if (sCostHash.ContainsKey(costKey))
                {
                    costHash = sCostHash[costKey];
                }
                else
                {
                    string market = "PJM";
                    if (marketKey == 2)
                        market = "MISO";
                    else if (marketKey == 7)
                        market = "CAISO";
                    else if (marketKey == 12)
                        market = "SPP";
                    if (marketKey == 1)
                    {
                        mSelectPjmCostCommand.CommandText = "select startdate, lmponpeak, lmpoffpeak, peakhrs, offpeakhrs from " + market + ".FtrAuctionNodePrice a, Period b where MarketKey = @marketkey and " +
                                                            "PeriodType = 'monthly' and a.PeriodKey = b.PeriodKey and NodeKey = @nodekey and a.FtrAuctionKey in (select distinct MAX(FtrAuctionkey) from " +
                                                             market + ".FtrAuctionNodePrice where periodkey = b.periodkey group by periodkey) and b.StartDate >= @startdate and b.StartDate <= @enddate order by startdate";
                        mSelectPjmCostCommand.Parameters["@marketkey"].Value = marketKey;
                        mSelectPjmCostCommand.Parameters["@nodekey"].Value = nodeKey;
                        mSelectPjmCostCommand.Parameters["@startdate"].Value = startDate;
                        mSelectPjmCostCommand.Parameters["@enddate"].Value = endDate;
                        SqlDataReader reader = mSelectPjmCostCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            Cost cost = new Cost();
                            DateTime marketDateTime = reader.GetDateTime(0);
                            cost.Peak = (double)reader.GetDecimal(1);
                            cost.OffPeak = (double)reader.GetDecimal(2);
                            cost.PeakHours = reader.GetInt32(3);
                            cost.OffPeakHours = reader.GetInt32(4);
                            costHash.Add(marketDateTime, cost);
                        }
                        reader.Close();

                        mSelectPjmPriceCommand.CommandText = "select MarketDate, AvgPeakCong, AvgOffpeakCong, PeakHours, OffpeakHours from " + market + ".nodelmpdailys where MarketTypeCode = 'da' and NodeKey = @nodekey and " +
                                                     "MarketDate >= @startdate and MarketDate <= @enddate order by marketdate";
                        mSelectPjmPriceCommand.Parameters["@startdate"].Value = startDate;
                        mSelectPjmPriceCommand.Parameters["@enddate"].Value = endDate;
                        mSelectPjmPriceCommand.Parameters["@nodekey"].Value = nodeKey; ;
                        reader = mSelectPjmPriceCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            DACongestion daCongestion = new DACongestion();
                            daCongestion.MarketDateTime = reader.GetDateTime(0);
                            DateTime tempDate = DateTime.Parse(daCongestion.MarketDateTime.Month + "/1/" + daCongestion.MarketDateTime.Year);
                            if (costHash.ContainsKey(tempDate))
                            {
                                Cost cost = costHash[tempDate];
                                if (cost.DACongestionList == null)
                                {
                                    cost.DACongestionList = new List<DACongestion>();
                                }
                                daCongestion.Peak = reader.IsDBNull(1) ? 0 : (double)reader.GetFloat(1);
                                daCongestion.OffPeak = reader.IsDBNull(2) ? 0 : (double)reader.GetFloat(2);
                                daCongestion.PeakHours = (int)reader.GetFloat(3);
                                daCongestion.OffPeakHours = (int)reader.GetFloat(4);
                                cost.DACongestionList.Add(daCongestion);
                            }
                        }
                        reader.Close();
                    }
                    else
                    {
                        mSelectPjmCostCommand.CommandText = "select startdate, class, shadowprice, peakhrs, offpeakhrs from " + market + ".FtrAuctionNodePrice a, Period b where MarketKey = @marketkey and " +
                                                            "PeriodType = 'monthly' and a.PeriodKey = b.PeriodKey and NodeKey = @nodekey and a.FtrAuctionKey in (select distinct MAX(FtrAuctionkey) from " +
                                                             market + ".FtrAuctionNodePrice where periodkey = b.periodkey group by periodkey) and b.StartDate >= @startdate and b.StartDate <= @enddate order by startdate";
                        mSelectPjmCostCommand.Parameters["@marketkey"].Value = marketKey;
                        mSelectPjmCostCommand.Parameters["@nodekey"].Value = nodeKey;
                        mSelectPjmCostCommand.Parameters["@startdate"].Value = startDate;
                        mSelectPjmCostCommand.Parameters["@enddate"].Value = endDate;
                        SqlDataReader reader = mSelectPjmCostCommand.ExecuteReader();

                        while (reader.Read())
                        {
                            Cost cost = new Cost();
                            DateTime marketDateTime = new DateTime();
                            marketDateTime = reader.GetDateTime(0);
                            if (costHash.ContainsKey(marketDateTime))
                            {
                                Cost cost1 = costHash[marketDateTime];
                                costHash.Remove(marketDateTime);
                                string classType = reader.GetString(1);
                                double nodeCost = (double)reader.GetDecimal(2);
                                if (classType.ToLower() == "peak")
                                    cost1.Peak = nodeCost;
                                else if (classType.ToLower() == "off-peak")
                                    cost1.OffPeak = nodeCost;
                                costHash.Add(marketDateTime, cost1);
                            }
                            else
                            {
                                string classType = reader.GetString(1);
                                double nodeCost = (double)reader.GetDecimal(2);
                                if (classType.ToLower() == "peak")
                                    cost.Peak = nodeCost;
                                else if (classType.ToLower() == "off-peak")
                                    cost.OffPeak = nodeCost;
                                cost.PeakHours = reader.GetInt32(3);
                                cost.OffPeakHours = reader.GetInt32(4);
                                costHash.Add(marketDateTime, cost);
                            }


                        }

                        reader.Close();

                        mSelectPjmPriceCommand.CommandText = "select MarketDate, AvgPeakCong, AvgOffpeakCong, PeakHours, OffpeakHours from " + market + ".nodelmpdailys where MarketTypeCode = 'da' and NodeKey = @nodekey and " +
                                                     "MarketDate >= @startdate and MarketDate <= @enddate order by marketdate";
                        mSelectPjmPriceCommand.Parameters["@startdate"].Value = startDate;
                        mSelectPjmPriceCommand.Parameters["@enddate"].Value = endDate;
                        mSelectPjmPriceCommand.Parameters["@nodekey"].Value = nodeKey;
                        reader = mSelectPjmPriceCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            DACongestion daCongestion = new DACongestion();
                            daCongestion.MarketDateTime = reader.GetDateTime(0);
                            DateTime tempDate = DateTime.Parse(daCongestion.MarketDateTime.Month + "/1/" + daCongestion.MarketDateTime.Year);
                            if (costHash.ContainsKey(tempDate))
                            {
                                Cost cost = costHash[tempDate];
                                if (cost.DACongestionList == null)
                                {
                                    cost.DACongestionList = new List<DACongestion>();
                                }
                                daCongestion.Peak = reader.IsDBNull(1) ? 0 : (double)reader.GetFloat(1);
                                daCongestion.OffPeak = reader.IsDBNull(2) ? 0 : (double)reader.GetFloat(2);
                                daCongestion.PeakHours = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(3));
                                daCongestion.OffPeakHours = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(4));
                                cost.DACongestionList.Add(daCongestion);
                            }
                        }
                        reader.Close();
                    }
                    sCostHash.Add(costKey, costHash);
                }
            }
            List<DateTime> marketDateList = sourceCostHash.Keys.ToList<DateTime>();
            marketDateList.Sort();
            foreach (DateTime marketDate in marketDateList)
            {
                Cost sourceCost = sourceCostHash[marketDate];
                if (sinkCostHash.ContainsKey(marketDate))
                {
                    Cost sinkCost = sinkCostHash[marketDate];
                    if (marketKey == 2 && marketKey == 12 && marketKey == 7)
                    {
                        sourceCost.Peak = sourceCost.Peak - sinkCost.Peak;
                        sourceCost.OffPeak = sourceCost.OffPeak - sinkCost.OffPeak;
                    }
                    else
                    {
                        sourceCost.Peak = sinkCost.Peak - sourceCost.Peak;
                        sourceCost.OffPeak = sinkCost.OffPeak - sourceCost.OffPeak;
                    }
                    if (sinkCost.DACongestionList == null)
                    {
                        VayuConnection.Close();
                        return null;
                    }
                    foreach (DACongestion sinkCongestion in sinkCost.DACongestionList)
                    {
                        try
                        {
                            DACongestion sourceCongestion = sourceCost.DACongestionList.Single(s => s.MarketDateTime == sinkCongestion.MarketDateTime);
                            sourceCongestion.Peak = sinkCongestion.Peak - sourceCongestion.Peak;
                            sourceCongestion.OffPeak = sinkCongestion.OffPeak - sourceCongestion.OffPeak;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            VayuConnection.Close();
            return sourceCostHash;
        }
        /// <summary>
        /// Gets the CRR auctions.
        /// </summary>
        /// <param name="isoCode">The iso code.</param>
        /// <returns></returns>
        public List<CRRAuction> GetFtrAuctions(string isoCode)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlDataReader reader = null;
            List<CRRAuction> periodList = new List<CRRAuction>();
            if (isoCode.ToUpper() == "ERCOT")
            {
                mSelectDistinctErcotCRRCommand.Parameters["@isocode"].Value = isoCode;
                reader = mSelectDistinctErcotCRRCommand.ExecuteReader();
            }
            else
            {
                mSelectDistinctPjmCRRCommand.Parameters["@isocode"].Value = isoCode;
                reader = mSelectDistinctPjmCRRCommand.ExecuteReader();
            }
            while (reader.Read())
            {
                CRRAuction auction = new CRRAuction();
                auction.Key = (int)reader.GetDecimal(0);
                auction.Name = reader.GetString(1);
                periodList.Add(auction);
            }
            reader.Close();
            VayuConnection.Close();
            return periodList;
        }
        /// <summary>
        /// Gets the auction list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="chkNewPortFolio">if set to <c>true</c> [CHK new port folio].</param>
        /// <returns></returns>
        public List<string> GetAuctionList(int marketKey, bool chkNewPortFolio)
        {
            List<string> auctionList = new List<string>();

            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand mSelectFtrAuctionCommandFinal = new SqlCommand();
            SqlDataReader reader = null;
            if (!chkNewPortFolio)
            {
                string market = "";
                if (marketKey == 9)
                {
                    mSelectCRRErcotAuctionCommand = new SqlCommand();
                    mSelectCRRErcotAuctionCommand.Connection = VayuConnection;
                    mSelectCRRErcotAuctionCommand.CommandText = "select distinct auction,[Month],a.CRRAuctionStartDate from CRRBids " +
                                " f join CRRAuction a on f.Auction = a.CRRAuctionName where marketkey = @marketkey order by a.CRRAuctionStartDate desc";
                    mSelectCRRErcotAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
                    mSelectCRRErcotAuctionCommand.Parameters["@marketkey"].Value = marketKey;
                    mSelectFtrAuctionCommandFinal = mSelectCRRErcotAuctionCommand;

                }
                reader = mSelectFtrAuctionCommandFinal.ExecuteReader();
            }
            else
            {
                if (marketKey == 9)
                {
                    mSelectNewPortfolioErcotFtrAuctionCommand.Parameters["@marketkey"].Value = marketKey;
                    reader = mSelectNewPortfolioErcotFtrAuctionCommand.ExecuteReader();
                }
            }
            while (reader.Read())
            {
                auctionList.Add(reader.GetString(0));
            }
            reader.Close();
            VayuConnection.Close();
            return auctionList.Distinct().ToList();
        }

        public List<string> GetNewAuctionList(int Marketkey, bool chkNewPortFolio)
        {
            List<string> auctionList = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlDataReader reader = null;
            if (chkNewPortFolio)
            {
                //mSelectNewPortfolioFtrAuctionCommand = new SqlCommand();
                mSelectNewPortfolioFtrAuctionCommand.Parameters["@MarketKey"].Value = Marketkey;
                reader = mSelectNewPortfolioFtrAuctionCommand.ExecuteReader();
                while (reader.Read())
                {
                    auctionList.Add(reader.GetString(0));
                }
                VayuConnection.Close();
            }
            return auctionList.Distinct().ToList();
        }
        /// <summary>
        /// Gets the CRR portfolio list.
        /// </summary>
        /// <param name="auction">The auction.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public List<Portfolio> GetCRRPortfolioList(string auction, int marketKey)
        {
            if (auction == null)
            {
                return null;
            }
            List<Portfolio> portfolioList = new List<Portfolio>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectCRRPortfolioCommand.Parameters["@auction"].Value = auction;
            mSelectCRRPortfolioCommand.Parameters["@marketkey"].Value = marketKey;
            SqlDataReader reader = mSelectCRRPortfolioCommand.ExecuteReader();
            while (reader.Read())
            {
                Portfolio portfolio = new Portfolio();
                portfolio.ID = (int)reader.GetValue(0);
                portfolio.Name = reader.GetString(1);
                portfolioList.Add(portfolio);
            }
            reader.Close();
            VayuConnection.Close();
            return portfolioList;
        }
        /// <summary>
        /// Gets the name of the iso auction.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetIsoAuctionName(string name)
        {
            string isoName = null;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectIsoCommand.Parameters["@auctionname"].Value = name;
            SqlDataReader reader = mSelectIsoCommand.ExecuteReader();
            while (reader.Read())
            {
                isoName = reader.GetString(0);
            }
            reader.Close();
            VayuConnection.Close();
            return isoName;
        }


        public Dictionary<string, int> GetDBPeriodHours(DateTime startDate, DateTime endDate, int Key)
        {
            Dictionary<string, int> periodHours = new Dictionary<string, int>();


            if (Key == 9)
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = " select PeakHrs , OffPeakHrs, PeakWE from Period where PeriodType = 'Monthly' and StartDate = '" + startDate.ToString() + "' and EndDate = '" + endDate.ToString() + "'";
                        cmd.Connection = con;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            periodHours.Add("PeakWD", rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0)));
                            periodHours.Add("Off-peak", rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(1)));
                            periodHours.Add("PeakWE", rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(2)));


                        }
                        rdr.Close();
                        con.Close();
                    }
                }
            }
            return periodHours;
        }

        /// <summary>
        /// Get the clearedMWH foe given Auction.
        /// </summary>
        /// <param name="portfolioKey">The port
        /// folio key.</param>
        /// <summary>
        public Dictionary<string, double> GetDBclrmwh(int marketkey, string selectedAuction, string selectedPortfolio)
        {

            Dictionary<string, double> mwhlist = new Dictionary<string, double>();
            Dictionary<string, string> mwhlist1 = new Dictionary<string, string>();
            Dictionary<string, int> hourdic = new Dictionary<string, int>();
            SqlDataReader reader = null;
            string hours;
            string mwhval;
            int month1 = DateTime.ParseExact(selectedAuction.Substring(0, 3), "MMM", CultureInfo.CurrentCulture).Month;
            int year1 = int.Parse(selectedAuction.Substring(4, 4));
            DateTime firstDay = new DateTime(year1, month1, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

            double total_clearlmwh = 0;

            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            mSelectClearedMwhCommand = new SqlCommand();

            if (marketkey == 9)
            {
                hourdic = GetDBPeriodHours(firstDay, lastDay, marketkey);
                mSelectClearedMwhCommand.CommandText = "select TimeUse, sum(MW) as clearedmw,AccountHolder, B.CRRAuctionName " +
                       " from CRRAuctionResults A left join Node C on A.Sourcekey = C.NodeKey left join Node D on A.[Sinkkey] = D.NodeKey, CRRAuction B " +
                        " where A.CRRAuctionkey = B.CRRAuctionKey and A.AccountHolder in (@portfolio)and A.CRRAuctionkey in " +
                        " (select CRRAuctionkey from CRRAuction where CRRAuctionName = @selectedAuction) " +
                        " group by SourceName, SinkName, TimeUse, periodkey, Bid, Hedge, AccountHolder, Sourcekey, [Sinkkey], C.Zone, D.Zone, " +
                           " B.CRRAuctionName, periodkey, B.CRRAuctionkey,ShadowPrice order by SourceName, SinkName";

                mSelectClearedMwhCommand.Parameters.AddWithValue("@portfolio", selectedPortfolio);
                mSelectClearedMwhCommand.Parameters.AddWithValue("@selectedAuction", selectedAuction);




                mSelectClearedMwhCommand.Connection = VayuConnection;
                reader = mSelectClearedMwhCommand.ExecuteReader();

                while (reader.Read())
                {
                    hours = reader.GetString(0);


                    if (hours.Equals("PeakWD"))
                        total_clearlmwh = total_clearlmwh + (double)reader.GetDecimal(1) * hourdic["PeakWD"];//336

                    if (hours.Equals("Off-peak"))
                        total_clearlmwh = total_clearlmwh + (double)reader.GetDecimal(1) * hourdic["Off-peak"];//248

                    if (hours.Equals("PeakWE"))
                        total_clearlmwh = total_clearlmwh + (double)reader.GetDecimal(1) * hourdic["PeakWE"];//160




                }
            }//marketkey=9

            mwhlist.Add(selectedAuction, total_clearlmwh);


            return mwhlist;
        }

        /// 
        /// Deletes the CRR.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        public void DeleteCRR(int portfolioKey, int mKey)
        {
            if (mKey == 9)
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mDeleteErcotCRRCommand.Parameters["@portfoliokey"].Value = portfolioKey;
                // mDeleteErcotCRRCommand.ExecuteNonQuery();
                VayuConnection.Close();
            }
        }
        public Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> GetCost(int marketKey, DateTime startDate, DateTime endDate, bool isMustTake, List<FTRBid> pathList, string period = "ALL", bool isRtNeeded = false)
        {
            string sourceKeyStr = string.Empty;
            HashSet<int> hashInt = new HashSet<int>();
            List<int> yearList = new List<int>();

            foreach (FTRBid path in pathList)
            {
                if (path.Source.Contains("LAKELYNN11"))
                {

                }
                PricingNode sourceNode = DBAccess.GetNodeFromName(path.Source, marketKey);
                PricingNode sinkNode = DBAccess.GetNodeFromName(path.Sink, marketKey);
                if (sourceNode == null || sinkNode == null)
                {
                    continue;
                }
                path.SourceKey = sourceNode.NodeKey;
                path.SinkKey = sinkNode.NodeKey;
                hashInt.Add(path.SourceKey);
                hashInt.Add(path.SinkKey);
            }
            foreach (var item in hashInt)
            {
                sourceKeyStr += item + ",";
            }
            sourceKeyStr = sourceKeyStr.Trim(',', ' ');
            Dictionary<string, Dictionary<int, Dictionary<DateTime, Cost>>> costHash = new Dictionary<string, Dictionary<int, Dictionary<DateTime, Cost>>>();
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            string market = "PJM";
            if (marketKey == 9)
            {
                market = "Ercot";
            }
            if (marketKey == 9)
            {
                int years = endDate.Year - startDate.Year;
                for (int i = 0; i <= years; i++)
                {
                    yearList.Add(startDate.Year + i);
                }

                mSelectErcotCostCommand.CommandText = "select startdate, lmponpeak, lmpoffpeak, peakhrs, offpeakhrs,a.NodeKey,  a.PeakWE from CRRAuctionNodePrice a, Period b where MarketKey = @marketkey and " +
                                                      " PeriodType = 'monthly' and a.PeriodKey = b.PeriodKey and NodeKey in (" + sourceKeyStr + ") and a.CRRAuctionKey in (select distinct MAX(CRRAuctionKey) from " +
                                                       " CRRAuctionNodePrice where periodkey = b.periodkey group by periodkey) and b.StartDate >= @startdate and b.StartDate <= @enddate  order by startdate";
                mSelectErcotCostCommand.Parameters["@marketkey"].Value = marketKey;
                mSelectErcotCostCommand.Parameters["@startdate"].Value = startDate;
                mSelectErcotCostCommand.Parameters["@enddate"].Value = endDate;
                mSelectErcotCostCommand.CommandTimeout = 300000;
                SqlDataReader reader = mSelectErcotCostCommand.ExecuteReader();

                while (reader.Read())
                {
                    Cost cost = new Cost();
                    cost.MarketDateTime = reader.GetDateTime(0);
                    cost.Peak = (double)reader.GetDecimal(1);
                    cost.OffPeak = (double)reader.GetDecimal(2);
                    cost.PeakHours = reader.GetInt32(3);
                    cost.OffPeakHours = reader.GetInt32(4);
                    cost.NodeKey = (int)reader.GetDecimal(5);
                    cost.PeakWE = reader.IsDBNull(6) ? 0 : (double)reader.GetDecimal(6);
                    if (!costHash.ContainsKey("Monthly"))
                    {
                        Dictionary<int, Dictionary<DateTime, Cost>> tempHash = new Dictionary<int, Dictionary<DateTime, Cost>>();
                        if (!tempHash.ContainsKey(cost.NodeKey))
                            tempHash.Add(cost.NodeKey, new Dictionary<DateTime, Cost>());
                        if (!tempHash[cost.NodeKey].ContainsKey(cost.MarketDateTime))
                        {
                            tempHash[cost.NodeKey].Add(cost.MarketDateTime, cost);
                            costHash.Add("Monthly", tempHash);
                        }
                    }
                    else
                    {
                        Dictionary<int, Dictionary<DateTime, Cost>> tempHash = costHash["Monthly"];
                        if (!tempHash.ContainsKey(cost.NodeKey))
                            tempHash.Add(cost.NodeKey, new Dictionary<DateTime, Cost>());
                        if (!tempHash[cost.NodeKey].ContainsKey(cost.MarketDateTime))
                            tempHash[cost.NodeKey].Add(cost.MarketDateTime, cost);
                    }
                }
                reader.Close();

                mSelectErcotPriceCommand.CommandText = "select MarketDate, AvgPeakLMP, AvgOffpeakLMP, PeakHours, OffpeakHours,NodeKey, AvgPeakWELMP, PeakWEHours from NodeLMPdailys where MarketTypeCode = 'da' and NodeKey in (" + sourceKeyStr + ") and " +
                                             "MarketDate >= @startdate and MarketDate <= @enddate order by marketdate";
                mSelectErcotPriceCommand.Parameters["@startdate"].Value = startDate;
                mSelectErcotPriceCommand.Parameters["@enddate"].Value = endDate;
                reader = mSelectErcotPriceCommand.ExecuteReader();

                while (reader.Read())
                {
                    Dictionary<int, Dictionary<DateTime, Cost>> monthlyCostHash = costHash["Monthly"];
                    if (period == "ALL")
                    {

                    }
                    DACongestion daCongestion = new DACongestion();
                    daCongestion.MarketDateTime = reader.GetDateTime(0);
                    DateTime tempDate = DateTime.Parse(daCongestion.MarketDateTime.Month + "/1/" + daCongestion.MarketDateTime.Year);
                    int nKey = (int)reader.GetDecimal(5);
                    DayOfWeek day = daCongestion.MarketDateTime.DayOfWeek;// tempDate.DayOfWeek;
                    if (!monthlyCostHash.ContainsKey(nKey))
                        continue;
                    //costHash.Add(nKey, new Dictionary<DateTime, Cost>());

                    if (monthlyCostHash[nKey].ContainsKey(tempDate))
                    {

                        Cost cost = monthlyCostHash[nKey][tempDate];
                        if (cost.DACongestionList == null)
                        {
                            cost.DACongestionList = new List<DACongestion>();
                        }
                        daCongestion.Peak = reader.IsDBNull(1) ? 0 : (double)reader.GetFloat(1);
                        daCongestion.OffPeak = reader.IsDBNull(2) ? 0 : (double)reader.GetFloat(2);
                        daCongestion.PeakHours = (int)reader.GetFloat(3);
                        daCongestion.OffPeakHours = (int)reader.GetFloat(4);
                        if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                        {
                            daCongestion.PeakWE = reader.IsDBNull(6) ? 0 : (double)reader.GetFloat(6);
                            daCongestion.PeakHours = (int)reader.GetFloat(7);
                        }


                        if (tempDate.Month == daCongestion.MarketDateTime.Month)
                            cost.DACongestionList.Add(daCongestion);
                    }

                }
                reader.Close();

                #region RTPrices
                if (isRtNeeded)
                {

                    mSelectErcotRTPriceCommand.CommandText = "select MarketDate, AvgPeakLMP, AvgOffpeakLMP, PeakHours, OffpeakHours,NodeKey, AvgPeakWELMP, PeakWEHours  from NodeLMPdailys where MarketTypeCode = 'rt' and NodeKey in (" + sourceKeyStr + ") and " +
                                           "MarketDate >= @startdate and MarketDate <= @enddate order by marketdate";
                    mSelectErcotRTPriceCommand.Parameters["@startdate"].Value = startDate;
                    mSelectErcotRTPriceCommand.Parameters["@enddate"].Value = endDate;
                    reader = mSelectErcotRTPriceCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Dictionary<int, Dictionary<DateTime, Cost>> monthlyCostHash = costHash["Monthly"];
                        if (period == "ALL")
                        {
                        }
                        DACongestion rtCongestion = new DACongestion();
                        rtCongestion.MarketDateTime = reader.GetDateTime(0);
                        DateTime tempDate = DateTime.Parse(rtCongestion.MarketDateTime.Month + "/1/" + rtCongestion.MarketDateTime.Year);
                        int nKey = (int)reader.GetDecimal(5);
                        DayOfWeek dayweek = rtCongestion.MarketDateTime.DayOfWeek;// tempDate.DayOfWeek;
                        if (!monthlyCostHash.ContainsKey(nKey))
                            continue;
                        //costHash.Add(nKey, new Dictionary<DateTime, Cost>());

                        if (monthlyCostHash[nKey].ContainsKey(tempDate))
                        {
                            Cost cost = monthlyCostHash[nKey][tempDate];
                            if (cost.RTCongestionList == null)
                            {
                                cost.RTCongestionList = new List<DACongestion>();
                            }
                            rtCongestion.Peak = reader.IsDBNull(1) ? 0 : (double)reader.GetFloat(1);
                            rtCongestion.OffPeak = reader.IsDBNull(2) ? 0 : (double)reader.GetFloat(2);
                            rtCongestion.PeakHours = (int)reader.GetFloat(3);
                            rtCongestion.OffPeakHours = (int)reader.GetFloat(4);
                            if (dayweek == DayOfWeek.Saturday || dayweek == DayOfWeek.Sunday)
                            {
                                rtCongestion.PeakWE = reader.IsDBNull(6) ? 0 : (double)reader.GetFloat(6);
                                rtCongestion.PeakHours = (int)reader.GetFloat(7);
                            }
                            if (tempDate.Month == rtCongestion.MarketDateTime.Month)
                                cost.RTCongestionList.Add(rtCongestion);
                        }
                    }
                    reader.Close();
                }
                #endregion
            }
            Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> resultCost = new Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>>();
            foreach (var item in pathList)
            {
                foreach (var perioditem in costHash)
                {
                    string periodType = perioditem.Key;
                    Dictionary<int, Dictionary<DateTime, Cost>> tempCostHash = perioditem.Value;
                    if (item.Source.Contains("LAKELYNN11"))
                    {

                    }
                    if (!tempCostHash.ContainsKey(item.SourceKey))
                    {
                        continue;
                    }
                    string strKey = item.SourceKey.ToString() + item.SinkKey.ToString();
                    if (!resultCost.ContainsKey(periodType))
                    {
                        resultCost.Add(periodType, new Dictionary<string, Dictionary<DateTime, Cost>>());
                    }
                    else if (!resultCost[periodType].ContainsKey(strKey))
                        resultCost[periodType].Add(strKey, new Dictionary<DateTime, Cost>());
                    List<DateTime> marketDateList = tempCostHash[item.SourceKey].Keys.ToList<DateTime>();
                    marketDateList.Sort();


                    if (!tempCostHash.ContainsKey(item.SinkKey))
                    {
                        continue;
                    }

                    Dictionary<DateTime, Cost> sinkCostHash = tempCostHash[item.SinkKey];
                    Dictionary<DateTime, Cost> sourceCostHash = tempCostHash[item.SourceKey];
                    foreach (DateTime marketDate in marketDateList)
                    {
                        if (!sinkCostHash.ContainsKey(marketDate) || !sourceCostHash.ContainsKey(marketDate))
                        {
                            continue;
                        }
                        Cost sourceCost = tempCostHash[item.SourceKey][marketDate].Clone() as Cost;
                        //   Cost sourceCost = sourceCostHash[marketDate];
                        if (!resultCost.ContainsKey(periodType))
                        {
                            Dictionary<string, Dictionary<DateTime, Cost>> tempCost = new Dictionary<string, Dictionary<DateTime, Cost>>();

                        }
                        if (!resultCost[periodType].ContainsKey(strKey))
                            resultCost[periodType].Add(strKey, new Dictionary<DateTime, Cost>());
                        if (!resultCost[periodType][strKey].ContainsKey(marketDate))
                        {
                            resultCost[periodType][strKey].Add(marketDate, sourceCost);
                        }
                        else
                        {
                            continue;
                        }

                        Cost sinkCost = sinkCostHash[marketDate];

                        if (marketKey == 9)
                        {
                            sourceCost.Peak = -(sinkCost.Peak - sourceCost.Peak);
                            sourceCost.OffPeak = -(sinkCost.OffPeak - sourceCost.OffPeak);
                            sourceCost.PeakWE = -(sinkCost.PeakWE - sourceCost.PeakWE);
                        }

                        if (sinkCost.DACongestionList == null || sourceCost.DACongestionList == null)
                        {
                            continue;
                        }
                        sourceCost.daHash = new Dictionary<DateTime, DACongestion>(sourceCost.DACongestionList.ToDictionary(x => x.MarketDateTime));
                        foreach (DACongestion sinkCongestion in sinkCost.DACongestionList)
                        {
                            try
                            {
                                DACongestion sourceCongestion = sourceCost.daHash[sinkCongestion.MarketDateTime] as DACongestion;
                                sourceCongestion.Peak = sinkCongestion.Peak - sourceCongestion.Peak;
                                sourceCongestion.OffPeak = sinkCongestion.OffPeak - sourceCongestion.OffPeak;
                                sourceCongestion.PeakWE = sinkCongestion.PeakWE - sourceCongestion.PeakWE;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        if (sinkCost.RTCongestionList == null || sourceCost.RTCongestionList == null)
                        {
                            continue;
                        }
                        sourceCost.rtHash = new Dictionary<DateTime, DACongestion>(sourceCost.RTCongestionList.ToDictionary(x => x.MarketDateTime));
                        foreach (DACongestion sinkCongestion in sinkCost.RTCongestionList)
                        {
                            try
                            {
                                DACongestion sourceCongestion = sourceCost.rtHash[sinkCongestion.MarketDateTime] as DACongestion;
                                sourceCongestion.Peak = sinkCongestion.Peak - sourceCongestion.Peak;
                                sourceCongestion.OffPeak = sinkCongestion.OffPeak - sourceCongestion.OffPeak;
                                sourceCongestion.PeakWE = sinkCongestion.PeakWE - sourceCongestion.PeakWE;
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                    }
                }
            }
            VayuConnection.Close();

            return resultCost;
        }

        public Dictionary<DateTime, Dictionary<string, Cost>> GetOptionPrices(int marketkey, DateTime startDate, DateTime endDate, List<FTRBid> pathList, bool isAsbid, string AuctionSelectedItem, int ID)

        {
            Dictionary<DateTime, Dictionary<string, Cost>> costHash = new Dictionary<DateTime, Dictionary<string, Cost>>();
            if (marketkey == 9)
            {
                try
                {
                    Dictionary<string, Cost> tempHash = new Dictionary<string, Cost>();
                    string sourcekeyList = string.Empty; string sinkkeyList = string.Empty;
                    foreach (FTRBid item in pathList)
                    {
                        //if (item.Source == "EXGNWTL_1" && item.Sink == "DC_L" && item.ClassType == "PEAKWD")//(item.Source == "WOO_WOODWRD1" && item.Sink == "RIGGIN_UNIT1" && item.ClassType == "OFF-PEAK")
                        //{

                        //}
                        //else
                        //    continue;

                        if (item.HedgeType == "OPT")
                        {
                            sourcekeyList += item.SourceKey + ",";
                            sinkkeyList += item.SinkKey + ",";
                        }
                    }
                    sourcekeyList = sourcekeyList.Trim(',', ' ');
                    sinkkeyList = sinkkeyList.Trim(',', ' ');
                    if (VayuConnection.State == ConnectionState.Closed)
                        VayuConnection.Open();

                    mSelectErcotOptionCostCommand.Connection = VayuConnection;
                    mSelectErcotOptionCostCommand.CommandText = " select a.StartDate, a.PeakWD,a.OffPeak,a.PeakWE, peakhrs, offpeakhrs,a.Sourcekey,a.Sinkkey,b.PeakWE from CRRAuctionOptionPrice a " +
                                                                " join Period b on a.StartDate = b.StartDate and a.EndDate = b.EndDate " +
                                                                " where MarketKey = @marketkey and PeriodType = 'monthly' and a.Sourcekey in(" + sourcekeyList + ") and a.Sinkkey in(" + sinkkeyList + ") " +
                                                                " and a.StartDate >= '" + startDate + "' and a.StartDate <= '" + endDate + "'  order by a.startdate";
                    mSelectErcotOptionCostCommand.Parameters["@marketkey"].Value = marketkey;
                    mSelectErcotOptionCostCommand.Parameters["@startdate"].Value = startDate;
                    mSelectErcotOptionCostCommand.Parameters["@enddate"].Value = endDate;
                    mSelectErcotOptionCostCommand.Parameters["@sourcekey"].Value = sourcekeyList;
                    mSelectErcotOptionCostCommand.Parameters["@sinkkey"].Value = sinkkeyList;
                    SqlDataReader reader = mSelectErcotOptionCostCommand.ExecuteReader();
                    while (reader.Read())
                    {

                        Cost objCost = new Cost();
                        objCost.MarketDateTime = reader.GetDateTime(0);
                        if (!reader.IsDBNull(1))
                            objCost.Peak = (double)reader.GetDecimal(1);
                        if (!reader.IsDBNull(2))
                            objCost.OffPeak = (double)reader.GetDecimal(2);
                        if (!reader.IsDBNull(3))
                            objCost.PeakWE = (double)reader.GetDecimal(3);
                        objCost.PeakHours = (int)reader.GetInt32(4);
                        objCost.OffPeakHours = (int)reader.GetInt32(5);
                        objCost.PeakWEHours = (int)reader.GetInt32(8);
                        int sourcekey = (int)reader.GetInt32(6);
                        int sinkkey = (int)reader.GetInt32(7);
                        string key = sourcekey + "?" + sinkkey;
                        if (costHash.ContainsKey(objCost.MarketDateTime))
                        {
                            if (tempHash.ContainsKey(key))
                            {
                                tempHash[key] = objCost;
                            }
                            else
                            {
                                tempHash.Add(key, objCost);
                            }
                            //costHash[objCost.MarketDateTime] = tempHash;
                        }
                        else
                        {
                            tempHash = new Dictionary<string, Cost>();
                            tempHash.Add(key, objCost);
                            costHash.Add(objCost.MarketDateTime, tempHash);

                        }
                    }

                    Dictionary<string, Cost> SourceSinkOpt = GetDailyLMP(startDate, endDate.AddDays(1), sourcekeyList, sinkkeyList, AuctionSelectedItem, ID);
                    Dictionary<string, Cost> tempOptionHash = new Dictionary<string, Cost>();
                    Dictionary<DateTime, string> dictPeakYn = GetPeakYND(startDate, endDate.AddDays(1));
                    Dictionary<DateTime, Cost> periodDic = GetPeriodDetails();
                    List<int> peakList = GetPeakList();
                    List<int> offPeakList = GetOffPeakList();
                    double totalMw = 0; double nodePrice = double.NaN;
                    foreach (var item in pathList)
                    {
                        if (item.Source == "RN_SR_WIND1" && item.Sink == "SIL_SILAS_10")//(item.Source == "WOO_WOODWRD1" && item.Sink == "RIGGIN_UNIT1" && item.ClassType == "OFF-PEAK")
                        {

                        }
                        //else
                        //    continue;
                        Dictionary<string, Cost> optioncost = new Dictionary<string, Cost>(); Cost costHelper = new Cost();
                        string pathkey = item.SourceKey + "?" + item.SinkKey;
                        foreach (var cost in costHash)
                        {
                            Cost objcost = new Cost();
                            double peakpnl = 0; double offpnl = 0; double peakwepnl = 0; double mw = 0; totalMw = 0;
                            decimal peakda = 0, offpeakda = 0, peakweda = 0;

                            DateTime marketDate = cost.Key;
                            if (costHash.ContainsKey(marketDate))
                                optioncost = costHash[marketDate];
                            if (optioncost.ContainsKey(pathkey))
                                costHelper = optioncost[pathkey];
                            tempOptionHash = costHash[marketDate];
                            if (tempOptionHash.ContainsKey(pathkey))
                                objcost = tempOptionHash[pathkey];
                            else
                                continue;
                            if (item.ClassType.ToUpper() == "PEAK" || item.ClassType.ToUpper() == "ONPEAK" || item.ClassType.ToUpper() == "PEAKWD")
                                nodePrice = costHelper.Peak;
                            else if (item.ClassType.ToUpper() == "PEAKWE")
                                nodePrice = costHelper.PeakWE;
                            else
                                nodePrice = costHelper.OffPeak;
                            if (isAsbid)
                            {
                                if (item.TradeType.ToUpper() == "BUY")
                                {
                                    if (item.MW1 != null && item.Price1 > nodePrice)
                                        mw += (double)item.MW1;
                                    if (item.MW2 != null && item.Price2 > nodePrice)
                                        mw += (double)item.MW2;
                                    if (item.MW3 != null && item.Price3 > nodePrice)
                                        mw += (double)item.MW3;
                                    if (item.MW4 != null && item.Price4 > nodePrice)
                                        mw += (double)item.MW4;
                                    if (item.MW5 != null && item.Price5 > nodePrice)
                                        mw += (double)item.MW5;
                                    if (item.MW6 != null && item.Price6 > nodePrice)
                                        mw += (double)item.MW6;
                                }
                                else
                                {
                                    if (item.MW1 != null && item.Price1 < nodePrice)
                                        mw += (double)item.MW1;
                                    if (item.MW2 != null && item.Price2 < nodePrice)
                                        mw += (double)item.MW2;
                                    if (item.MW3 != null && item.Price3 < nodePrice)
                                        mw += (double)item.MW3;
                                    if (item.MW4 != null && item.Price4 < nodePrice)
                                        mw += (double)item.MW4;
                                    if (item.MW5 != null && item.Price5 < nodePrice)
                                        mw += (double)item.MW5;
                                    if (item.MW6 != null && item.Price6 < nodePrice)
                                        mw += (double)item.MW6;
                                    mw = mw * -1;
                                }
                                totalMw += mw;
                            }
                            else
                            {
                                if (item.MW6 != null)
                                {
                                    mw += (double)item.MW6;
                                }
                                if (item.MW5 != null)
                                {
                                    mw += (double)item.MW5;
                                }
                                if (item.MW4 != null)
                                {
                                    mw += (double)item.MW4;
                                }
                                if (item.MW3 != null)
                                {
                                    mw += (double)item.MW3;
                                }
                                if (item.MW2 != null)
                                {
                                    mw += (double)item.MW2;
                                }
                                if (item.MW1 != null)
                                {
                                    mw += (double)item.MW1;
                                }
                                if (item.TradeType.ToUpper() == "SELL")
                                    mw = mw * -1;
                                totalMw += mw;
                            }
                            if (mw == 0.0)
                                continue;
                            List<DACongestion> DACongestionList = new List<DACongestion>();
                            DACongestion congestion = new DACongestion();
                            DateTime fromDate = marketDate;
                            DateTime toDate = marketDate.AddMonths(1).AddDays(-1);
                            Cost periodcost = periodDic[marketDate];
                            while (fromDate <= toDate)
                            {
                                if (fromDate > DateTime.Today)
                                    break;
                                congestion = new DACongestion();
                                congestion.MarketDateTime = fromDate;
                                congestion.PeakHours = periodcost.PeakHours;
                                congestion.OffPeakHours = periodcost.OffPeakHours;
                                congestion.PeakWEHours = periodcost.PeakWEHours;
                                string key = item.SourceKey + ":" + item.SinkKey + ":" + fromDate;
                                if (SourceSinkOpt.ContainsKey(key))
                                {
                                    congestion.Peak = SourceSinkOpt[key].Peak;
                                    congestion.PeakWE = SourceSinkOpt[key].PeakWE;
                                    congestion.OffPeak = SourceSinkOpt[key].OffPeak;

                                    if (item.ClassType.ToUpper() == "PEAK" || item.ClassType.ToUpper() == "ONPEAK" || item.ClassType.ToUpper() == "PEAKWD")
                                    {

                                        double temppnl = ((SourceSinkOpt[key].Peak) * totalMw);
                                        peakpnl += temppnl;
                                    }
                                    else if (item.ClassType.ToUpper() == "PEAKWE")
                                    {

                                        double temppnl = ((SourceSinkOpt[key].PeakWE) * totalMw);
                                        peakwepnl += temppnl;
                                    }
                                    else
                                    {
                                        double temppnl = ((SourceSinkOpt[key].OffPeak) * totalMw);
                                        offpnl += temppnl;
                                    }
                                    congestion.pnl = 0;
                                    congestion.Peakpnl = peakpnl;
                                    congestion.Offpnl = offpnl;
                                    congestion.PeakWEpnl = peakwepnl;
                                    if (!DACongestionList.Contains(congestion))
                                    {
                                        DACongestionList.Add(congestion);
                                        objcost.DACongestionList = DACongestionList.OrderByDescending(a => a.MarketDateTime).ToList();
                                        peakpnl = 0; peakwepnl = 0; offpnl = 0;
                                    }

                                }
                                else
                                {

                                }
                                fromDate = fromDate.AddDays(1);


                            }
                        }
                    }
                }
                catch
                {

                }
            }
            return costHash;
        }

        private Dictionary<string, Cost> GetDailyLMP(DateTime startDate, DateTime endDate, string sourcekeyList, string sinkkeyList, string auctionselected, int Pkey)
        {
            DateTime sdate = startDate;
            DateTime edate = endDate;
            Dictionary<string, Cost> daList = new Dictionary<string, Cost>();
            try
            {
                while (sdate <= edate)
                {
                    if (VayuConnection.State == ConnectionState.Closed)
                        VayuConnection.Open();
                    SqlCommand mselectCongestiondaily = new SqlCommand();
                    mselectCongestiondaily.Connection = VayuConnection;
                    mselectCongestiondaily.CommandTimeout = 300000;
                    mselectCongestiondaily.CommandText = "select distinct AvgPeakLMP,AvgOffpeakLMP,AvgPeakWELMP, OffpeakHours,PeakHours,PeakWEHours, MarketDate ,SourceNodeKey,SinkNodeKey,c.Source,c.sink " +
                                                         " from NodeLMPDailysOption (nolock)a inner join Node b on a.SourceNodeKey = b.NodeKey " +//NodeLMPDailysOption
                                                        " inner join CRRBids c on c.Source = b.NodeName  and c.Auction = '" + auctionselected + "' and c.PortfolioKey = " + Pkey +
                                                        " inner join Node d on a.SinkNodeKey = d.NodeKey and c.Sink = d.NodeName where MarketDate = '" + sdate + "' " +
                                                        " and MarketTypeCode = 'da' order by MarketDate";

                    SqlDataReader reader = mselectCongestiondaily.ExecuteReader();
                    while (reader.Read())
                    {
                        Cost objCost = new Cost();
                        string key = Convert.ToInt32(reader.GetValue(7)) + ":" + Convert.ToInt32(reader.GetValue(8)) + ":" + reader.GetDateTime(6);
                        objCost.Peak = reader.IsDBNull(0) ? (double)0 : (double)reader.GetFloat(0);
                        objCost.OffPeak = reader.IsDBNull(1) ? (double)0 : (double)reader.GetFloat(1);
                        objCost.PeakWE = reader.IsDBNull(2) ? (double)0 : (double)reader.GetFloat(2);
                        int hours = (reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(5))) == 0 ? 0 : Convert.ToInt32(reader.GetValue(5));
                        objCost.PeakHours = hours;
                        objCost.OffPeakHours = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
                        //objCost.PeakHours = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5));
                        if (!daList.ContainsKey(key))
                            daList.Add(key, objCost);
                    }
                    sdate = sdate.AddDays(1);
                    reader.Close();
                    VayuConnection.Close();
                }
                return daList;
            }
            catch (Exception ex)
            {

            }
            return daList;
        }


        public Dictionary<DateTime, string> GetPeakYND(DateTime startDate, DateTime endDate)
        {


            Dictionary<DateTime, string> peaklist = new Dictionary<DateTime, string>();

            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();

            mselectCongestion = new SqlCommand();
            //mselectCongestion.CommandText = "select MarketDateTime, PeakYN from MarketTime where MarketKey=9 and MarketDateTime >=@startdate and MarketDateTime<=@enddate order by MarketDateTime";
            mselectCongestion.CommandText = "select MarketDateTime, PeakYN from MarketTime where MarketKey=9 and MarketDateTime >=@startdate and MarketDateTime<=@enddate order by MarketDateTime";

            mselectCongestion.Parameters.AddWithValue("@startdate", "startdate");
            mselectCongestion.Parameters.AddWithValue("@enddate", "enddate");
            mselectCongestion.Connection = VayuConnection;

            mselectCongestion.Parameters["@startdate"].Value = startDate;
            mselectCongestion.Parameters["@enddate"].Value = endDate;


            SqlDataReader reader = mselectCongestion.ExecuteReader();
            while (reader.Read())
            {
                string peakyn = reader.GetString(1);
                DateTime MarketDateTime = reader.GetDateTime(0);
                peaklist.Add(MarketDateTime, peakyn);
            }
            reader.Close();
            //SigmaDbErcotConnection.Close();




            return peaklist;
        }
        public Dictionary<DateTime, string> GetPeakYN(DateTime startDate, DateTime endDate)
        {


            Dictionary<DateTime, string> peaklist = new Dictionary<DateTime, string>();

            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();

            mselectCongestion = new SqlCommand();
            //mselectCongestion.CommandText = "select MarketDateTime, PeakYN from MarketTime where MarketKey=9 and MarketDateTime >=@startdate and MarketDateTime<=@enddate order by MarketDateTime";
            mselectCongestion.CommandText = "select MarketDateTime, PeakYN from MarketTime where MarketKey=9 and MarketDateTime >=@startdate and MarketDateTime<=@enddate order by MarketDateTime";

            mselectCongestion.Parameters.AddWithValue("@startdate", "startdate");
            mselectCongestion.Parameters.AddWithValue("@enddate", "enddate");
            mselectCongestion.Connection = VayuConnection;

            mselectCongestion.Parameters["@startdate"].Value = startDate;
            mselectCongestion.Parameters["@enddate"].Value = endDate;


            SqlDataReader reader = mselectCongestion.ExecuteReader();
            while (reader.Read())
            {
                string peakyn = reader.GetString(1);
                DateTime MarketDateTime = reader.GetDateTime(0);
                peaklist.Add(MarketDateTime, peakyn);
            }
            reader.Close();
            VayuConnection.Close();




            return peaklist;
        }
        public Dictionary<string, decimal> GetDALMP(DateTime startDate, DateTime endDate, string nodekeyList)
        {
            Dictionary<string, decimal> daList = new Dictionary<string, decimal>();
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            SqlCommand mselectCongestion = new SqlCommand();
            mselectCongestion.Connection = VayuConnection;
            mselectCongestion.CommandText = "select NodeKey,MarketDateTime,LMP from NodeDALMPH where NodeKey in(" + nodekeyList + ") and MarketDateTime>='" + startDate + "' and MarketDateTime<='" + endDate + "' order by MarketDateTime";
            SqlDataReader reader = mselectCongestion.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = reader.GetInt32(0);
                DateTime MarketDateTime = reader.GetDateTime(1);
                //if (MarketDateTime.Hour == 0)
                //    MarketDateTime = MarketDateTime.AddDays(-1);
                decimal price = reader.GetDecimal(2);
                string key = MarketDateTime + "?" + nodekey;
                if (daList.ContainsKey(key))
                    daList[key] = price;
                else
                    daList.Add(key, price);
            }
            reader.Close();
            VayuConnection.Close();
            return daList;
        }
        private List<int> GetOffPeakList()
        {
            return new List<int> { 1, 2, 3, 4, 5, 6, 23, 24 };
        }
        private List<int> GetPeakList()
        {
            return new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 };
        }
        public Dictionary<DateTime, Cost> GetPeriodDetails()
        {
            Dictionary<DateTime, Cost> periodDic = new Dictionary<DateTime, Cost>();
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            SqlCommand mselectPeriod = new SqlCommand();
            mselectPeriod.Connection = VayuConnection;
            mselectPeriod.CommandText = "select startDate,PeakHrs/(PeakHrs/16),OffPeakHrs/(OffPeakHrs/8),PeakWE/(PeakWE/16) from period";
            SqlDataReader reader = mselectPeriod.ExecuteReader();
            while (reader.Read())
            {
                Cost objCost = new Cost();
                objCost.MarketDateTime = reader.GetDateTime(0);
                objCost.PeakHours = reader.GetInt32(1);
                objCost.OffPeakHours = reader.GetInt32(2);
                objCost.PeakWEHours = reader.GetInt32(3);
                objCost.PeakWE = reader.GetInt32(3);
                if (!periodDic.ContainsKey(objCost.MarketDateTime))
                    periodDic.Add(objCost.MarketDateTime, objCost);
            }
            reader.Close();
            VayuConnection.Close();
            return periodDic;
        }
        public Dictionary<int, DateTime> GetMinDaDates(int mKey)
        {
            Dictionary<int, DateTime> minDateDict = new Dictionary<int, DateTime>();
            if (mKey == 9)
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "select nodekey  , MIN(MarketDateTime) from NodeDALMPH group by nodekey order by nodekey ";

                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            int nodekey = Convert.ToInt32(rdr.GetValue(0));
                            DateTime minDate = Convert.ToDateTime(rdr.GetValue(1));
                            if (!minDateDict.ContainsKey(nodekey))
                                minDateDict.Add(nodekey, minDate);
                        }
                        rdr.Close();
                    }
                    con.Close();
                }
            }

            return minDateDict;

        }
        /// <summary>
        /// Gets the auction start date.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="auctionName">Name of the auction.</param>
        /// <returns></returns>
        /// 

        public DateTime GetAuctionStartDate(int marketKey, string auctionName)
        {
            DateTime strStartDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1);
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand cmd = new SqlCommand(); // mSelectIsoCommand.Parameters["@auctionname"].Value = name;
            if (marketKey == 9)
            {
                cmd = VayuConnection.CreateCommand();
                cmd.CommandText = "select CRRAuctionStartDate from CRRAuction where CRRAuctionName = '" + auctionName + "'";
            }

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                strStartDate = reader.GetDateTime(0);
            }
            reader.Close();
            VayuConnection.Close();
            return strStartDate;
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private int GetInt(object obj)
        {
            int pk;
            int.TryParse((obj ?? "").ToString(), out pk);
            return pk;
        }


        public List<NodePriceHelper> GetallCosts(DateTime date, DateTime lastHistTime)
        {

            List<NodePriceHelper> CostList = new List<NodePriceHelper>();
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Connection = con;


                        lastHistTime = date.AddMonths(1).AddDays(-3);
#if TEST
                        cmd.CommandText = " select distinct  NodeKey , LMPOffPeak , LMPOnPeak , PeakHrs , OffPeakHrs , b.PeriodKey from PJM.FtrAuctionNodePrice a,  " +
                                " Period b where MarketKey = 1 and PeriodType = 'monthly'  and a.PeriodKey = b.PeriodKey and a.FtrAuctionKey in (select distinct MAX(FtrAuctionkey) from PJM.FtrAuctionNodePrice where periodkey = b.periodkey group by periodkey)  " +
                                "  and b.StartDate >= @StartDate and b.StartDate <= @EndDate ";
#else
                        cmd.CommandText = " select distinct NodeKey , LMPOffPeak , LMPOnPeak , PeakHrs , OffPeakHrs , b.PeriodKey from CRRAuctionNodePrice a,  " +
                                        "  Period b where MarketKey = 9 and PeriodType = 'monthly'  and a.PeriodKey = b.PeriodKey and a.CRRAuctionKey in (select distinct MAX(CRRAuctionKey) from CRRAuctionNodePrice where periodkey = b.periodkey group by periodkey)  " +
                                        "   and b.StartDate >= @StartDate and b.StartDate <= @EndDate ";
#endif
                        // cmd.Parameters.AddWithValue("@StartDate", lastHistTime);
                        //  cmd.Parameters.AddWithValue("@EndDate", date);

                        cmd.Parameters.AddWithValue("@StartDate", date);
                        cmd.Parameters.AddWithValue("@EndDate", lastHistTime);
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            NodePriceHelper costHelper = new NodePriceHelper();
                            costHelper.NodeKey = Convert.ToInt32(rdr.GetValue(0));
                            costHelper.OffPeakPrice = Convert.ToDouble(rdr.GetValue(1));
                            costHelper.PeakPrice = Convert.ToDouble(rdr.GetValue(2));
                            costHelper.PeakHrs = Convert.ToInt32(rdr.GetValue(3));
                            costHelper.OffPeakHrs = Convert.ToInt32(rdr.GetValue(4));
                            costHelper.PeriodKey = Convert.ToInt32(rdr.GetValue(5));
                            CostList.Add(costHelper);
                        }
                        rdr.Close();
                        cmd.Connection.Close();
                    }
                }
                return CostList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public Dictionary<string, string> getRTDAMinDate(string market, string nodekeylist, string type)
        {

            Dictionary<string, string> rtdadatelist = new Dictionary<string, string>();

            long nodekey;
            DateTime rtdate, dadate;
            string key, value;

            return rtdadatelist;
        }

        public void GetNodeCoordinate(Action<ObservableCollection<NodeCoordinate>, Exception> callback, List<int> sourcesinkNode)
        {
            throw new NotImplementedException();
        }
    }
    public class ConstraintContingency
    {
        public int ConstraintRTNum { get; set; }
        public string Constraint { get; set; }
        public string Contingency { get; set; }
        public double DollarImpact { get; set; }
        public double ShiftFactor { get; set; }
        public double? MW { get; set; }
        public DateTime Date { get; set; }
        public string Month { get; set; }
        public int PeakHours { get; set; }
        public int OffPeakHours { get; set; }
        public int PeriodKey { get; set; }
    }


    public class Exposure
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }

        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Constraint { get; set; }
        /// <summary>
        /// Gets or sets the contingency.
        /// </summary>
        /// <value>
        /// The contingency.
        /// </value>
        public string Contingency { get; set; }


        public string Source { get; set; }

        public string Sink { get; set; }

        /// <summary>
        /// Gets or sets the sum.
        /// </summary>
        /// <value>
        /// The sum.
        /// </value>
        public double? Sum { get; set; }
        /// <summary>
        /// Gets or sets the total he.
        /// </summary>
        /// <value>
        /// The total he.
        /// </value>
        public double? TotalHE { get; set; }
        /// <summary>
        /// Gets or sets the shift.
        /// </summary>
        /// <value>
        /// The shift.
        /// </value>
        public double? Shift { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is shift empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is shift empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsShiftEmpty { get; set; }
        /// <summary>
        /// Gets or sets the type of the risk.
        /// </summary>
        /// <value>
        /// The type of the risk.
        /// </value>
        public string RiskType { get; set; }

        public string ClassType { get; set; }

        public string Month { get; set; }

        public string ConstId { get; set; }

        public double? PeakSum { get; set; }
        public double? PeakWE { get; set; }
        public double? OffPeakSum { get; set; }
        public double? MWhExposure { get; set; }
        public double? PeakWESum { get; set; }
        public double? PeakWEMW { get; set; }
        public double? PeakWEMWhExposure { get; set; }
        public double? PeakMW { get; set; }
        public double? OffPeakMW { get; set; }
        public double? SensSource { get; set; }
        public double? SensSink { get; set; }
        public double? Delta { get; set; }
        public double? PeakMWhExposure { get; set; }
        public double? OffPeakMWhExposure { get; set; }


    }
    public class Sensitivity
    {
        /// <summary>
        /// Gets or sets the sensitivity value.
        /// </summary>
        /// <value>
        /// The sensitivity value.
        /// </value>
        public double SensitivityValue { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Constraint { get; set; }
        /// <summary>
        /// Gets or sets the contingency.
        /// </summary>
        /// <value>
        /// The contingency.
        /// </value>
        public string Contingency { get; set; }
        /// <summary>
        /// Gets or sets the shift factor.
        /// </summary>
        /// <value>
        /// The shift factor.
        /// </value>
        public double ShiftFactor { get; set; }
        /// <summary>
        /// Gets or sets the dollar impact.
        /// </summary>
        /// <value>
        /// The dollar impact.
        /// </value>
        public double DollarImpact { get; set; }
        /// <summary>
        /// Gets or sets the type of the risk.
        /// </summary>
        /// <value>
        /// The type of the risk.
        /// </value>
        public string RiskType { get; set; }
    }

    public class OblOpt
    {
        public int SourceKey { get; set; }
        public int SinnkKey { get; set; }
    }

    public class NodePriceHelper
    {
        public int NodeKey { get; set; }
        public DateTime MktDate { get; set; }
        public double PeakPrice { get; set; }
        public double OffPeakPrice { get; set; }
        public int PeakHrs { get; set; }
        public int OffPeakHrs { get; set; }
        public int PeriodKey { get; set; }
    }

    class PathHelper
    {
        public long SourceKey { get; set; }
        public long SinkKey { get; set; }

        public string HedgeType { get; set; }

    }

    class CRRAlgoHelper
    {
        public string Source { get; set; }
        public string Sink { get; set; }
        public double PeakBidPrice { get; set; }
        public double OffPeakBidPrice { get; set; }


        public double? MaxRTPeak { get; set; }
        public double? MaxRTOffPeak { get; set; }
        public double? MinRTPeak { get; set; }
        public double? MinRTOffPeak { get; set; }

        public double? MaxDAPeak { get; set; }
        public double? MaxDAOffPeak { get; set; }
        public double? MinDAPeak { get; set; }
        public double? MinDAOffPeak { get; set; }

        public double MaxDAOffPeakTotal { get; set; }
        public double MinDAOffPeakTotal { get; set; }
        public double MaxDAPeakTotal { get; set; }
        public double MinDAPeakTotal { get; set; }

        public double MaxRTOffPeakTotal { get; set; }
        public double MinRTOffPeakTotal { get; set; }
        public double MaxRTPeakTotal { get; set; }
        public double MinRTPeakTotal { get; set; }


        public double MaxFRTOffPeakTotal { get; set; }
        public double MinFRTOffPeakTotal { get; set; }
        public double? MaxFRTPeakTotal { get; set; }
        public double? MinFRTPeakTotal { get; set; }

        public double? MaxFRTPeakWETotal { get; set; }
        public double? MinFRTPeakWETotal { get; set; }

        public double? AvgCRROffPeak { get; set; }

        public double? AvgCRRPeak { get; set; }
        public double? AvgCRRPeakWE { get; set; }



        public double MaxRTPeakWE { get; set; }
        public double MinRTPeakWE { get; set; }
        public double MaxDAPeakWE { get; set; }
        public double MinDAPeakWE { get; set; }

        public double MaxRTPeakWETotal { get; set; }
        public double MinRTPeakWETotal { get; set; }
        public double MaxDAPeakWETotal { get; set; }
        public double MinDAPeakWETotal { get; set; }


        public string Hedgetype { get; set; }



    }

    public class NodeCoordinate
    {
        /// <summary>
        /// Gets or sets the nodekey.
        /// </summary>
        /// <value>
        /// The nodekey.
        /// </value>
        public int Nodekey { get; set; }
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the map location.
        /// </summary>
        /// <value>
        /// The map location.
        /// </value>
        //public Microsoft.Maps.MapControl.WPF.Location MapLocation { get; set; }
        /// <summary>
        /// Gets or sets the kv.
        /// </summary>
        /// <value>
        /// The kv.
        /// </value>
        public double? KV { get; set; }
        /// <summary>
        /// Gets or sets the name of the psse.
        /// </summary>
        /// <value>
        /// The name of the psse.
        /// </value>
        public string PsseName { get; set; }

        public string TypeName { get; set; }
    }
}
