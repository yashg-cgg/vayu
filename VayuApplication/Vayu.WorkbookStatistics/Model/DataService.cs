using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Numerics;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.NodePriceLibrary;
using Vayu.WorkbookStatistics.ViewModels;

namespace Vayu.WorkbookStatistics.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;

        #region SQL Commands

        /// <summary>
        /// The m select risk limits command
        /// </summary>
        private SqlCommand mSelectRiskLimitsCommand;
        /// <summary>
        /// The m insert ees bids command
        /// </summary>
        private SqlCommand mInsertEESBidsCommand;
        /// <summary>
        /// The m insert ercot ees bids command
        /// </summary>
        private SqlCommand mInsertErcotEESBidsCommand;
        /// <summary>
        /// The m select PTP bids command
        /// </summary>
        private SqlCommand mSelectPTPBidsCommand;
        /// <summary>
        /// The m select constraint contingecy command
        /// </summary>
        private SqlCommand mSelectConstraintContingecyCommand;
        /// <summary>
        /// The m select constraint by identifier command
        /// </summary>
        private SqlCommand mSelectConstraintByIDCommand;

        private SqlCommand cmdSelectConstraintByIDForNewConstraints;

        /// <summary>
        /// The m select constraint by identifier no exist command
        /// </summary>
        private SqlCommand mSelectConstraintByIDNoExistCommand;
        /// <summary>
        /// The m select ercot portfolio command
        /// </summary>
        private SqlCommand mSelectErcotPortfolioCommand;
        /// <summary>
        /// The m select senstivity command
        /// </summary>
        private SqlCommand mSelectSenstivityCommand;
        /// <summary>
        /// The m select ercot deenergized nodes command
        /// </summary>
        private SqlCommand mSelectErcotDeenergizedNodesCmd;
        /// <summary>
        /// The m select senstivity by identifier command
        /// </summary>
        private SqlCommand mSelectSenstivityByIDCommand;
        /// <summary>
        /// The m select PTP bid ids command
        /// </summary>
        private SqlCommand mSelectPTPBidIdsCommand;
        /// <summary>
        /// The m insert preference command
        /// </summary>
        private SqlCommand mInsertPreferenceCommand;
        /// <summary>
        /// The m delete ees bids by identifier command
        /// </summary>
        private SqlCommand mDeleteEESBidsByIdCommand;
        /// <summary>
        /// The m delete ercot ees bids by identifier command
        /// </summary>
        private SqlCommand mDeleteErcotEESBidsByIdCommand;
        /// <summary>
        /// The m select preference command
        /// </summary>
        private SqlCommand mSelectPreferenceCommand;
        /// <summary>
        /// The m select maximum load command
        /// </summary>
        private SqlCommand mSelectMaxLoadCommand;
        private SqlCommand sSelectSubmittedCount;

        /// <summary>
        /// The m delete preference command
        /// </summary>
        private SqlCommand mDeletePreferenceCommand;
        /// <summary>
        /// The m select database bids command
        /// </summary>
        private SqlCommand mSelectDBBidsCommand;
        /// <summary>
        /// The m select start checked constraint command
        /// </summary>
        private SqlCommand mSelectStartCheckedConstraintCommand;
        /// <summary>
        /// The m select outage checked constraint command
        /// </summary>
        private SqlCommand mSelectOutageCheckedConstraintCommand;
        /// <summary>
        /// The m select constraint contigency start checked command
        /// </summary>
        private SqlCommand mSelectConstraintContigencyStartCheckedCommand;
        /// <summary>
        /// The m select sensitivity start checked command
        /// </summary>
        private SqlCommand mSelectSensitivityStartCheckedCommand;
        /// <summary>
        /// The m select start checked node command
        /// </summary>
        private SqlCommand mSelectStartCheckedNodeCommand;
        private SqlCommand mSelectloadDailyCommand;
        private SqlCommand mSelectloadHourlyCommand;
        private SqlCommand mSelectFuelSourceCommand;
        private SqlCommand stUpdateEESBids;
        private SqlCommand stUpdateErcotEESBids;
        private SqlCommand stUpdateEESBidsSourceSinkHrs;
        private SqlCommand stUpdateErcotEESBidsSourceSinkHrs;
        #endregion

        /// <summary>
        /// The exposure hour list
        /// </summary>
        private List<ExposureHour> ExposureHourList = null;
        /// <summary>
        /// The m path list
        /// </summary>
        private List<Path> mPathList = null;
        /// <summary>
        /// The m price nodes
        /// </summary>
        private Node[] mPriceNodes = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectRiskLimitsCommand = new SqlCommand();
            mSelectRiskLimitsCommand.CommandText = "select credit, mw, risk, mw_per_node, season_risk from risklimits where portfolioid = @portfolioid";
            mSelectRiskLimitsCommand.Parameters.AddWithValue("@portfolioid", "portfolioid");
            mSelectRiskLimitsCommand.Connection = VayuConnection;
            //
            mInsertEESBidsCommand = new SqlCommand();
            mInsertEESBidsCommand.CommandText = "Insert into EESBIDS values (@scheduleid,@BidStatus,0,@EndMarketDateTime,@RequestedMW,@ClearedMW,@EndUserKey,@SourceNodeKey, " +
                                                                                            "@SinkNodeKey,null, null, @Source,@Sink,@Price,@PortfolioKey,@SubmittedDateTime,@comments)";
            mInsertEESBidsCommand.Parameters.AddWithValue("@BidStatus", "BidStatus");
            mInsertEESBidsCommand.Parameters.AddWithValue("@scheduleid", "scheduleid");
            mInsertEESBidsCommand.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
            mInsertEESBidsCommand.Parameters.AddWithValue("@RequestedMW", "RequestedMW");
            mInsertEESBidsCommand.Parameters.AddWithValue("@ClearedMW", "ClearedMW");
            mInsertEESBidsCommand.Parameters.AddWithValue("@EndUserKey", "EndUserKey");
            mInsertEESBidsCommand.Parameters.AddWithValue("@SourceNodeKey", "SourceNodeKey");
            mInsertEESBidsCommand.Parameters.AddWithValue("@SinkNodeKey", "SinkNodeKey");
            mInsertEESBidsCommand.Parameters.AddWithValue("@Source", "Source");
            mInsertEESBidsCommand.Parameters.AddWithValue("@Sink", "Sink");
            mInsertEESBidsCommand.Parameters.AddWithValue("@Price", "Price");
            mInsertEESBidsCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mInsertEESBidsCommand.Parameters.AddWithValue("@SubmittedDateTime", "SubmittedDateTime");
            mInsertEESBidsCommand.Parameters.AddWithValue("@comments", "comments");
            mInsertEESBidsCommand.Connection = VayuConnection;
            //           
            mInsertErcotEESBidsCommand = new SqlCommand();
            mInsertErcotEESBidsCommand.CommandText = "Insert into ercotptpbids values (@bidid,@Source,@Sink,@Price,@EndMarketDateTime,@RequestedMW,@BidStatus,@EndUserKey,@PortfolioKey,@SubmittedDateTime, @comments)";
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@bidid", "bidid");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@Source", "Source");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@Sink", "Sink");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@Price", "Price");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@RequestedMW", "RequestedMW");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@BidStatus", "BidStatus");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@EndUserKey", "EndUserKey");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@SubmittedDateTime", "SubmittedDateTime");
            mInsertErcotEESBidsCommand.Parameters.AddWithValue("@comments", "comments");
            mInsertErcotEESBidsCommand.Connection = VayuConnection;
            //
            mSelectPTPBidsCommand = new SqlCommand();
            mSelectPTPBidsCommand.CommandText = "select daily.SourceNodeKey, daily.SinkNodeKey, HourEndingNum, MW1, Price1, MW2, Price2, MW3,Price3,MW4,Price4 from ptpbidhourly as hourly inner join ptpbiddaily as daily on hourly.BidId=daily.BidId and daily.tradeDate=@tradeDate and BidStatusCode='VALID' and daily.TraderId=@traderId";
            mSelectPTPBidsCommand.Parameters.AddWithValue("@tradeDate", "tradeDate");
            mSelectPTPBidsCommand.Parameters.AddWithValue("@traderId", "traderId");
            mSelectPTPBidsCommand.Connection = VayuConnection;
            //
            mSelectConstraintContingecyCommand = new SqlCommand();
            mSelectConstraintContingecyCommand.CommandText = " select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor, marketdatetime, dollarimpact " +
                                                             " from  RTMasterConstraint as A (nolock) inner join RTImpact as B on A.ConstraintRTNum= B.ConstraintRTNum " +
                                                             " where B.Date >= @start and B.Date < @end";
            mSelectConstraintContingecyCommand.Parameters.AddWithValue("@start", "MarkedDateTime");
            mSelectConstraintContingecyCommand.Parameters.AddWithValue("@end", "MarkedDateTime");
            // mSelectConstraintContingecyCommand.Connection = VayuDbConn;
            //
            mSelectSenstivityCommand = new SqlCommand();
            mSelectSenstivityCommand.CommandText = "select NodeKey,Sensitivity, constraintrtnum from RTMasterVector_new (nolock) where ConstraintRTNum in " +
                                                   " (select ConstraintRTNum from  RTImpact(nolock)  where RTImpact.Date >= @start and RTImpact.Date < @end)";
            mSelectSenstivityCommand.Parameters.AddWithValue("@start", "MarkedDateTime");
            mSelectSenstivityCommand.Parameters.AddWithValue("@end", "MarkedDateTime");
            mSelectSenstivityCommand.Connection = VayuConnection;
            //
            mSelectPTPBidIdsCommand = new SqlCommand();
            mSelectPTPBidIdsCommand.CommandText = "select distinct bidid from ercotptpbids where endmarketdatetime > @start and endmarketdatetime <= @end " +
                                                    "and portfoliokey = @portfoliokey";
            mSelectPTPBidIdsCommand.Parameters.AddWithValue("@start", "endmarketdatetime");
            mSelectPTPBidIdsCommand.Parameters.AddWithValue("@end", "endmarketdatetime");
            mSelectPTPBidIdsCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mSelectPTPBidIdsCommand.Connection = VayuConnection;
            //
            mInsertPreferenceCommand = new SqlCommand();
            mInsertPreferenceCommand.CommandText = "insert analysis_preference values (@trader, @preference, 'TraderApp')";
            mInsertPreferenceCommand.Parameters.AddWithValue("@trader", "trader");
            mInsertPreferenceCommand.Parameters.AddWithValue("@preference", "preference");
            mInsertPreferenceCommand.Connection = VayuConnection;
            //
            mSelectConstraintByIDCommand = new SqlCommand();
            //  mSelectConstraintByIDCommand.CommandText = "select * from [dbo].[PJM_rt_outages_Job1]";
            mSelectConstraintByIDCommand.CommandText = "SELECT  b.ConstraintName, d.ContingencyText, c.ConstraintRTNum, c.ShiftFactor,  b.Source , c.DollarImpact, MAX(d.ShadowPrice) ShadowPrice "
                + " FROM PJM_rt_outages a (nolock)  JOIN PJM.ConstraintOutages b ON b.Driver = a.Equipment  JOIN PJM.ConstraintRT d (nolock) ON d.ConstraintText = B.ConstraintName  JOIN RTMasterConstraint c ON c.MonitoredText = b.ConstraintName AND c.ContingencyText = d.ContingencyText "
                + " WHERE (StartDate >= @constraintDate and StartDate <=  GETDATE()) AND EndDate >= GETDATE()  AND OutageStatus not in ('Cancelle', 'Cancelled', 'Complete', 'Completed', 'Denied') GROUP BY b.ConstraintName, d.ContingencyText, c.ConstraintRTNum, c.ShiftFactor, c.DollarImpact ,  b.Source  "
                + " HAVING MAX(d.ShadowPrice) > 500 ORDER BY MAX(d.ShadowPrice) DESC ";
            mSelectConstraintByIDCommand.Parameters.AddWithValue("@constraintDate", "constraintDate");
            mSelectConstraintByIDCommand.Connection = VayuConnection;
            //
            cmdSelectConstraintByIDForNewConstraints = VayuConnection.CreateCommand();
            //cmdSelectConstraintByIDForNewConstraints.CommandText = "SELECT DISTINCT b.ConstraintName, b.Contingency, c.ConstraintRTNum, c.ShiftFactor, c.DollarImpact "
            //                                                    + "FROM PJM_rt_outages a "
            //                                                    + "JOIN PJM.ConstraintOutages b ON b.Driver = a.Equipment "
            //                                                    + "JOIN RTMasterConstraint c ON c.MonitoredText = b.ConstraintName " //  AND c.ContingencyText = b.Contingency
            //                                                    + "WHERE (StartDate >= @StartDate AND StartDate <= @EndDate) AND EndDate >= @EndDate "
            //                                                    + "ORDER BY b.ConstraintName ";
            cmdSelectConstraintByIDForNewConstraints.CommandText = "SELECT * FROM PJM.NewConstraints_Job";
            //cmdSelectConstraintByIDForNewConstraints.Parameters.AddWithValue("@StartDate", "StartDate");
            //cmdSelectConstraintByIDForNewConstraints.Parameters.AddWithValue("@EndDate", "EndDate");
            //
            mSelectSenstivityByIDCommand = new SqlCommand();
            mSelectSenstivityByIDCommand.CommandText = "select * from [dbo].[PJM_rt_outages_Job2]";
            mSelectSenstivityByIDCommand.Parameters.AddWithValue("@constraintDate", "constraintDate");
            mSelectSenstivityByIDCommand.Connection = VayuConnection;
            //
            mSelectConstraintByIDNoExistCommand = new SqlCommand();
            mSelectConstraintByIDNoExistCommand.CommandText = "select A.MonitoredText, A.ContingencyText, A.constraintrtnum, A.shiftfactor from RTMasterConstraint A inner join RiskConstraints as B On A.ConstraintRTNum = B.ConstraintRTNum WHERE b.Date=@constraintDate AND ShiftFactor IS NULL";
            mSelectConstraintByIDNoExistCommand.Parameters.AddWithValue("@constraintDate", "constraintDate");
            mSelectConstraintByIDNoExistCommand.Connection = VayuConnection;
            //
            mDeleteEESBidsByIdCommand = new SqlCommand();
            mDeleteEESBidsByIdCommand.CommandText = "delete EESBIDS where scheduleid = @scheduleid and bidstatus = 'IMPORTED' and endmarketdatetime > @start and endmarketdatetime <= @end";
            mDeleteEESBidsByIdCommand.Parameters.AddWithValue("@scheduleid", "scheduleid");
            mDeleteEESBidsByIdCommand.Parameters.AddWithValue("@start", "EndMarketDateTime");
            mDeleteEESBidsByIdCommand.Parameters.AddWithValue("@end", "EndMarketDateTime");
            mDeleteEESBidsByIdCommand.Connection = VayuConnection;
            //
            mDeleteErcotEESBidsByIdCommand = new SqlCommand();
            mDeleteErcotEESBidsByIdCommand.CommandText = "delete ercotptpbids where bidid = @bidid and bidstatus = 'IMPORTED' and endmarketdatetime > @start and endmarketdatetime <= @end";
            mDeleteErcotEESBidsByIdCommand.Parameters.AddWithValue("@bidid", "bidid");
            mDeleteErcotEESBidsByIdCommand.Parameters.AddWithValue("@start", "EndMarketDateTime");
            mDeleteErcotEESBidsByIdCommand.Parameters.AddWithValue("@end", "EndMarketDateTime");
            mDeleteErcotEESBidsByIdCommand.Connection = VayuConnection; //VayuDbConnr;
            //
            mSelectPreferenceCommand = new SqlCommand();
            mSelectPreferenceCommand.CommandText = "select preference from analysis_preference where trader = @trader and AppType = 'TraderApp'";
            mSelectPreferenceCommand.Parameters.AddWithValue("@trader", "trader");
            mSelectPreferenceCommand.Connection = VayuConnection;
            //
            mDeletePreferenceCommand = new SqlCommand();
            mDeletePreferenceCommand.CommandText = "delete analysis_preference where trader = @trader and AppType = 'TraderApp'";
            mDeletePreferenceCommand.Parameters.AddWithValue("@trader", "trader");
            mDeletePreferenceCommand.Connection = VayuConnection;
            //
            mSelectMaxLoadCommand = new SqlCommand();
            mSelectMaxLoadCommand.CommandText = "select MAX(MW) from LoadForecasts where LoadForecastTypeKey = 10 and CONVERT(date, marketdatetime) = @loadDate";
            mSelectMaxLoadCommand.Parameters.AddWithValue("@loadDate", "loadDate");
            mSelectMaxLoadCommand.Connection = VayuConnection;
            // sSelectSubmittedCount

            sSelectSubmittedCount = new SqlCommand();
            sSelectSubmittedCount.CommandText = "select count(*) from ErcotPTPBids where BidStatus='Valid' and PortfolioKey= @portfoliokey and EndMarketDateTime> @SubmitDate and EndMarketDateTime<= @SubmitDate";
            sSelectSubmittedCount.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            sSelectSubmittedCount.Parameters.AddWithValue("@SubmitDate", "SubmitDate");
            sSelectSubmittedCount.Parameters.AddWithValue("@EndDate", "EndDate");
            sSelectSubmittedCount.Connection = VayuConnection;

            //
            mSelectErcotPortfolioCommand = new SqlCommand();
            mSelectErcotPortfolioCommand.CommandText = "select label from portfolio where portfoliokey = @portfoliokey";
            mSelectErcotPortfolioCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mSelectErcotPortfolioCommand.Connection = VayuConnection;
            //
            mSelectDBBidsCommand = new SqlCommand();
            mSelectDBBidsCommand.CommandText = "select SourceNodeKey,SinkNodeKey,Price,RequestedMW  from TradingData.dbo.EESBids where EndMarketDateTime > @startdate  and EndMarketDateTime<=@enddate  and PortfolioKey=@PortfolioKey";
            mSelectDBBidsCommand.Parameters.AddWithValue("@startdate", "EndMarketDateTime");
            mSelectDBBidsCommand.Parameters.AddWithValue("@enddate", "EndMarketDateTime");
            mSelectDBBidsCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mSelectDBBidsCommand.Connection = VayuConnection;

            //
            mSelectStartCheckedConstraintCommand = new SqlCommand();
            mSelectStartCheckedConstraintCommand.CommandText = "select distinct c.constraintRTNum " +
                                "from pjm.ConstraintOutages a, PJM_rt_outages b, RTMasterConstraint c " +
                                "where b.StartDate >= @StartDate and b.startdate < @EndDate and (b.EndDate > @StartDate or " +
                                "b.EndDate is null) and a.driver = b.Equipment and c.MonitoredText = a.ConstraintName " +
                                "and c.MarketDateTime > dateadd(year, -1, convert(date, getdate())) and abs(c.shadowprice) > @ShadowPrice " +
                                "order by c.constraintRTNum";
            mSelectStartCheckedConstraintCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectStartCheckedConstraintCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectStartCheckedConstraintCommand.Parameters.AddWithValue("@ShadowPrice", "ShadowPrice");
            mSelectStartCheckedConstraintCommand.Connection = VayuConnection;

            //
            mSelectOutageCheckedConstraintCommand = new SqlCommand();
            mSelectOutageCheckedConstraintCommand.CommandText = "select distinct c.constraintRTNum " +
                                "from pjm.ConstraintOutages a, PJM_rt_outages b, RTMasterConstraint c " +
                                "where b.StartDate <=@EndDate and (b.EndDate > @StartDate or " +
                                "b.EndDate is null) and a.driver = b.Equipment and c.MonitoredText = a.ConstraintName " +
                                "and c.MarketDateTime > dateadd(year, -1, convert(date, getdate())) and abs(c.shadowprice) > @ShadowPrice " +
                                "order by c.constraintRTNum";
            mSelectOutageCheckedConstraintCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectOutageCheckedConstraintCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectOutageCheckedConstraintCommand.Parameters.AddWithValue("@ShadowPrice", "ShadowPrice");
            mSelectOutageCheckedConstraintCommand.Connection = VayuConnection;

            //
            mSelectConstraintContigencyStartCheckedCommand = new SqlCommand();
            mSelectConstraintContigencyStartCheckedCommand.CommandText = " select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor, marketdatetime, dollarimpact " +
                                                             " from  RTMasterConstraint as A (nolock) inner join RTImpact as B on A.ConstraintRTNum= B.ConstraintRTNum " +
                                                             " where A.ConstraintRTNum=@ConstraintRTNum";
            //mSelectConstraintContigencyStartCheckedCommand.Parameters.AddWithValue("@start", "start");
            //mSelectConstraintContigencyStartCheckedCommand.Parameters.AddWithValue("@end", "end");
            mSelectConstraintContigencyStartCheckedCommand.Parameters.AddWithValue("@ConstraintRTNum", "ConstraintRTNum");
            mSelectConstraintContigencyStartCheckedCommand.Connection = VayuConnection;

            //
            mSelectSensitivityStartCheckedCommand = new SqlCommand();
            mSelectSensitivityStartCheckedCommand.CommandText = "select NodeKey,Sensitivity, constraintrtnum from RTMasterVector_new (nolock) where ConstraintRTNum in " +
                                                          " (select ConstraintRTNum from  RTImpact(nolock) where ConstraintRTNum =@ConstraintRTNum) and NodeKey=@NodeKey";
            mSelectSensitivityStartCheckedCommand.Parameters.AddWithValue("@ConstraintRTNum", "ConstraintRTNum");
            mSelectSensitivityStartCheckedCommand.Parameters.AddWithValue("@NodeKey", "NodeKey");
            mSelectSensitivityStartCheckedCommand.Connection = VayuConnection;
            //
            mSelectStartCheckedNodeCommand = new SqlCommand();
            mSelectStartCheckedNodeCommand.CommandText = "select  nodename,nodekey from node where marketkey = @MarketKey";
            mSelectStartCheckedNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectStartCheckedNodeCommand.Connection = VayuConnection;

            mSelectFuelSourceCommand = new SqlCommand();
            mSelectFuelSourceCommand.CommandText = "select N.NodeName,FuelSource from Vayu..ErcotNodeFuelSource F join Vayu..node N on N.NodeKey=F.NodeKey where FuelSource is not null order by FuelSource ";
            mSelectFuelSourceCommand.Connection = VayuConnection;

            stUpdateEESBids = new SqlCommand();
            stUpdateEESBids.CommandText = "Update EESBids Set RequestedMW=(case when RequestedMW*@num<0.1 then 0.1 else RequestedMW*@num end) where PortfolioKey=@PortfolioKey and EndMarketDateTime> @start and  EndMarketDateTime<=@end";
            stUpdateEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            stUpdateEESBids.Parameters.AddWithValue("@start", "EndMarketDateTime");
            stUpdateEESBids.Parameters.AddWithValue("@end", "EndMarketDateTime");
            stUpdateEESBids.Parameters.AddWithValue("@num", "num");
            stUpdateEESBids.Connection = VayuConnection;


            stUpdateErcotEESBids = new SqlCommand();
            stUpdateErcotEESBids.CommandText = "Update ErcotPTPBids Set RequestedMW=(case when RequestedMW*@num<0.1 then 0.1 else RequestedMW*@num end) where PortfolioKey=@PortfolioKey and EndMarketDateTime>= @HEStartDateTime and  EndMarketDateTime<=@HEEndDateTime";
            stUpdateErcotEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            stUpdateErcotEESBids.Parameters.AddWithValue("@HEStartDateTime", "HEStartDateTime");
            stUpdateErcotEESBids.Parameters.AddWithValue("@HEEndDateTime", "HEEndDateTime");
            stUpdateErcotEESBids.Parameters.AddWithValue("@num", "num");
            stUpdateErcotEESBids.Connection = VayuConnection;


            stUpdateEESBidsSourceSinkHrs = new SqlCommand();
            stUpdateEESBidsSourceSinkHrs.CommandText = "Update EESBids Set RequestedMW=(case when RequestedMW*@num<0.1 then 0.1 else RequestedMW*@num end) where SourceNodeKey=@SourceNodeKey and SinkNodeKey=@SinkNodeKey and  PortfolioKey=@PortfolioKey and EndMarketDateTime= @start";
            stUpdateEESBidsSourceSinkHrs.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            stUpdateEESBidsSourceSinkHrs.Parameters.AddWithValue("@start", "EndMarketDateTime");
            stUpdateEESBidsSourceSinkHrs.Parameters.AddWithValue("@num", "num");
            stUpdateEESBidsSourceSinkHrs.Parameters.AddWithValue("@SourceNodeKey", "SourceNodeKey");
            stUpdateEESBidsSourceSinkHrs.Parameters.AddWithValue("@SinkNodeKey", "SinkNodeKey");
            stUpdateEESBidsSourceSinkHrs.Connection = VayuConnection;


            stUpdateErcotEESBidsSourceSinkHrs = new SqlCommand();
            stUpdateErcotEESBidsSourceSinkHrs.CommandText = "Update ErcotPTPBids Set RequestedMW=(case when RequestedMW*@num<1 then RequestedMW else RequestedMW*@num end) where  SourceNodeKey=@SourceNodeKey and SinkNodeKey=@SinkNodeKey and  PortfolioKey=@PortfolioKey and EndMarketDateTime= @EndMarketDateTime";
            stUpdateErcotEESBidsSourceSinkHrs.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            stUpdateErcotEESBidsSourceSinkHrs.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
            stUpdateErcotEESBidsSourceSinkHrs.Parameters.AddWithValue("@num", "num");
            stUpdateErcotEESBidsSourceSinkHrs.Parameters.AddWithValue("@SourceNodeKey", "SourceNodeKey");
            stUpdateErcotEESBidsSourceSinkHrs.Parameters.AddWithValue("@SinkNodeKey", "SinkNodeKey");
            stUpdateErcotEESBidsSourceSinkHrs.Connection = VayuConnection;

        }
        /// <summary>
        /// Gets the risk limit.
        /// </summary>
        /// <param name="portfolioId">The portfolio identifier.</param>
        /// <returns></returns>
        public RiskLimit GetRiskLimit(int portfolioId)
        {
            RiskLimit riskLimit = new RiskLimit();
            if (VayuConnection.State == ConnectionState.Closed) VayuConnection.Open();
            mSelectRiskLimitsCommand.Parameters["@portfolioid"].Value = portfolioId;
            SqlDataReader reader = mSelectRiskLimitsCommand.ExecuteReader();
            while (reader.Read())
            {
                riskLimit.Credit = reader.GetDouble(0);
                riskLimit.TotalMW = reader.GetDouble(1);
                riskLimit.Risk = reader.GetDouble(2);
                riskLimit.MWPerNode = reader.GetDouble(3);
                riskLimit.SeasonRisk = reader.GetValue(4).ToString().Equals(DBNull.Value) ? 0.0 : Convert.ToDouble(reader.GetValue(4));
            }
            reader.Close();
            VayuConnection.Close();
            return riskLimit;
        }
        /// <summary>
        /// Gets the sensitivity.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="IsDollar">if set to <c>true</c> [is dollar].</param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivity(DateTime startDate, DateTime endDate, bool IsDollar, string market, bool isDA)
        {
            Dictionary<int, Dictionary<int, Sensitivity>> nodeHash = new Dictionary<int, Dictionary<int, Sensitivity>>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<int, Tuple<string, string, double, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, double>>();
            if (market.ToUpper() == "ERCOT")
            {
                if (isDA)
                {
                    mSelectConstraintContingecyCommand.CommandText = "select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor, marketdatetime, dollarimpact " +
                                                    " from  Vayu..DAMasterConstraint as A (nolock) inner join Vayu..DAImpact as B on A.ConstraintRTNum= B.ConstraintRTNum  " +
                                                    " where B.Date >= @start and B.Date <= @end";
                }
                else
                {
                    mSelectConstraintContingecyCommand.CommandText = " select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor, marketdatetime, dollarimpact " +
                                                     " from  Vayu..RTMasterConstraint as A (nolock) inner join Vayu..RTImpact as B on A.ConstraintRTNum= B.ConstraintRTNum " +
                                                     " where B.Date >= @start and B.Date < @end";
                }
                mSelectConstraintContingecyCommand.Parameters["@start"].Value = startDate;
                mSelectConstraintContingecyCommand.Parameters["@end"].Value = endDate;
                mSelectConstraintContingecyCommand.Connection = VayuConnection;
            }
            else
            {
                mSelectConstraintContingecyCommand.Parameters["@start"].Value = startDate;
                mSelectConstraintContingecyCommand.Parameters["@end"].Value = endDate;
                mSelectConstraintContingecyCommand.Connection = VayuConnection;
            }
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
            if (market.ToUpper() == "ERCOT")
            {
                if (isDA)
                {
                    mSelectSenstivityCommand.CommandText = "select NodeKey,Sensitivity, constraintrtnum from Vayu..DAMasterVector (nolock) where ConstraintRTNum in " +
                                             " (select ConstraintRTNum from  Vayu..DAImpact as DAI(nolock) where DAI.Date >=@start and DAI.Date < @end)";
                }
                else
                {
                    mSelectSenstivityCommand.CommandText = "select NodeKey,Sensitivity, constraintrtnum from Vayu..RTMasterVector_new (nolock) where ConstraintRTNum in " +
                                           " (select ConstraintRTNum from  Vayu..RTImpact(nolock)  where RTImpact.Date >= @start and RTImpact.Date < @end)";
                }
                mSelectSenstivityCommand.Parameters["@start"].Value = startDate;
                mSelectSenstivityCommand.Parameters["@end"].Value = endDate;
                mSelectSenstivityCommand.Connection = VayuConnection;
            }
            else
            {
                mSelectSenstivityCommand.Parameters["@start"].Value = startDate;
                mSelectSenstivityCommand.Parameters["@end"].Value = endDate;
                mSelectSenstivityCommand.Connection = VayuConnection;
            }

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

                if (constraintNum == 822063)
                {

                }

                Sensitivity sensitivity = new Sensitivity();
                sensitivity.SensitivityValue = sensivityValue;
                sensitivity.ID = constraintNum;
                sensitivity.Constraint = tuple.Item1;
                sensitivity.Contingency = tuple.Item2;
                sensitivity.ShiftFactor = tuple.Item3;
                sensitivity.DollarImpact = tuple.Item4;
                if (!sensitivityHash.ContainsKey(constraintNum))
                    sensitivityHash.Add(constraintNum, sensitivity);
                nodeHash.Add(node, sensitivityHash);
            }
            reader.Close();
            VayuConnection.Close();
            return nodeHash;
        }
        /// <summary>
        /// Gets the sensitivity on start checked.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="IsDollar">if set to <c>true</c> [is dollar].</param>
        /// <param name="StartDateChecked">if set to <c>true</c> [start date checked].</param>
        /// <param name="OutageChecked">if set to <c>true</c> [outage checked].</param>
        /// <param name="ShadowPrice">The shadow price.</param>
        /// <param name="PathList">The path list.</param>
        /// <param name="MarketKey">The market key.</param>
        /// <param name="ConstraintRTNumList">The constraint rt number list.</param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<int, Dictionary<int, Sensitivity>>> GetSensitivityOnStartChecked(DateTime startDate, DateTime endDate, bool IsDollar, bool StartDateChecked, bool OutageChecked, string ShadowPrice, List<Path> PathList, int MarketKey, out List<string> ConstraintRTNumList)
        {
            List<string> tempConstraintList = GetOutageConstraintList(StartDateChecked, ShadowPrice, startDate);
            Dictionary<int, Dictionary<int, Dictionary<int, Sensitivity>>> constraintHash = new Dictionary<int, Dictionary<int, Dictionary<int, Sensitivity>>>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<int, Tuple<string, string, double, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, double>>();
            foreach (string constraintRTNum in tempConstraintList)
            {
                //mSelectConstraintContigencyStartCheckedCommand.Parameters["@start"].Value = startDate;
                // mSelectConstraintContigencyStartCheckedCommand.Parameters["@end"].Value = endDate;
                mSelectConstraintContigencyStartCheckedCommand.Parameters["@ConstraintRTNum"].Value = constraintRTNum;
                SqlDataReader reader = mSelectConstraintContigencyStartCheckedCommand.ExecuteReader();
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
            }
            // mSelectSenstivityCommand.Parameters["@start"].Value = startDate;
            //mSelectSenstivityCommand.Parameters["@end"].Value = endDate;
            Dictionary<string, int> TempNodeHash = GetNodeHash(MarketKey);
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            foreach (string ConstraintRTNum in tempConstraintList)
            {
                Dictionary<int, Dictionary<int, Sensitivity>> nodeHash = new Dictionary<int, Dictionary<int, Sensitivity>>();
                foreach (Path path in PathList)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        //mSelectSensitivityStartCheckedCommand.Parameters[].Value=path.Source
                        if (i == 0)
                        {
                            mSelectSensitivityStartCheckedCommand.Parameters["@NodeKey"].Value = TempNodeHash[path.Source];
                        }
                        else
                        {
                            mSelectSensitivityStartCheckedCommand.Parameters["@NodeKey"].Value = TempNodeHash[path.Sink];
                        }
                        mSelectSensitivityStartCheckedCommand.Parameters["@ConstraintRTNum"].Value = ConstraintRTNum;
                        SqlDataReader reader1 = mSelectSensitivityStartCheckedCommand.ExecuteReader();
                        while (reader1.Read())
                        {
                            int node = (int)reader1.GetDecimal(0);
                            double sensivityValue = (double)reader1.GetDecimal(1);
                            int constraintNum = (int)reader1.GetDecimal(2);
                            if (!constraintContingencyHash.ContainsKey(constraintNum))
                            {
                                continue;
                            }
                            Tuple<string, string, double, double> tuple = constraintContingencyHash[constraintNum];
                            Dictionary<int, Sensitivity> sensitivityHash = new Dictionary<int, Sensitivity>();
                            if (nodeHash.ContainsKey(node))
                            {
                                //sensitivityHash = nodeHash[node];
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
                        reader1.Close();
                    }
                }
                constraintHash.Add(Convert.ToInt32(ConstraintRTNum), nodeHash);
            }
            //while (reader1.Read())
            //{
            //    int node = (int)reader1.GetDecimal(0);
            //    double sensivityValue = (double)reader1.GetDecimal(1);
            //    int constraintNum = (int)reader1.GetDecimal(2);
            //    if (!constraintContingencyHash.ContainsKey(constraintNum))
            //    {
            //        continue;
            //    }
            //    Tuple<string, string, double, double> tuple = constraintContingencyHash[constraintNum];
            //    Dictionary<int, Sensitivity> sensitivityHash = new Dictionary<int, Sensitivity>();
            //    if (nodeHash.ContainsKey(node))
            //    {
            //        sensitivityHash = nodeHash[node];
            //        nodeHash.Remove(node);
            //    }
            //    Sensitivity sensitivity = new Sensitivity();
            //    sensitivity.SensitivityValue = sensivityValue;
            //    sensitivity.ID = constraintNum;
            //    sensitivity.Constraint = tuple.Item1;
            //    sensitivity.Contingency = tuple.Item2;
            //    sensitivity.ShiftFactor = tuple.Item3;
            //    sensitivity.DollarImpact = tuple.Item4;
            //    sensitivityHash.Add(constraintNum, sensitivity);
            //    nodeHash.Add(node, sensitivityHash);
            //}
            ConstraintRTNumList = tempConstraintList;
            VayuConnection.Close();
            return constraintHash;
        }
        /// <summary>
        /// Gets the sensitivity by constraint i ds.
        /// </summary>
        /// <param name="ConstraintDate">The constraint date.</param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivityByConstraintIDs(DateTime ConstraintDate)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<int, Dictionary<int, Sensitivity>> nodeHash = new Dictionary<int, Dictionary<int, Sensitivity>>();
            Dictionary<int, Tuple<string, string, double, string, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, string, double>>();

            mSelectConstraintByIDCommand.Parameters["@constraintDate"].Value = ConstraintDate;
            mSelectConstraintByIDCommand.CommandTimeout = 60 * 1000;
            SqlDataReader reader = mSelectConstraintByIDCommand.ExecuteReader();

            while (reader.Read())
            {
                string constraint = reader.GetString(0);
                constraint = constraint.Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").Replace("Reacinf-ctg", "").TrimStart();
                string contingency = reader.GetString(1);
                contingency = contingency.Replace("Contingency", "").TrimStart();
                int constraintRtNum = (int)reader.GetDecimal(2);
                string RiskType = reader.GetString(4);
                double dollarImact = reader.IsDBNull(5) ? 0 : (double)reader.GetDecimal(5);

                double shiftFactor = double.NaN;
                try
                {
                    shiftFactor = (double)reader.GetDecimal(3);
                }
                catch (Exception ex)
                {

                }
                Tuple<string, string, double, string, double> tuple = new Tuple<string, string, double, string, double>(constraint, contingency, shiftFactor, RiskType, dollarImact);
                if (!constraintContingencyHash.ContainsKey(constraintRtNum))
                {
                    constraintContingencyHash.Add(constraintRtNum, tuple);
                }
            }
            reader.Close();
            mSelectSenstivityByIDCommand.Parameters["@constraintDate"].Value = ConstraintDate;
            reader = mSelectSenstivityByIDCommand.ExecuteReader();
            while (reader.Read())
            {
                int node = (int)reader.GetDecimal(0);
                double sensivityValue = (double)reader.GetDecimal(1);
                int constraintNum = (int)reader.GetDecimal(2);
                if (!constraintContingencyHash.ContainsKey(constraintNum))
                {
                    continue;
                }
                Tuple<string, string, double, string, double> tuple = constraintContingencyHash[constraintNum];
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
                sensitivity.RiskType = tuple.Item4;
                sensitivity.DollarImpact = tuple.Item5;
                sensitivityHash.Add(constraintNum, sensitivity);
                nodeHash.Add(node, sensitivityHash);
            }
            reader.Close();
            VayuConnection.Close();
            return nodeHash;
        }

        public Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivityByConstraintIDsForNewConstraints(DateTime ConstraintDate)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<int, Dictionary<int, Sensitivity>> nodeHash = new Dictionary<int, Dictionary<int, Sensitivity>>();
            Dictionary<int, Tuple<string, string, double, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, double>>();

            //cmdSelectConstraintByIDForNewConstraints.Parameters["@StartDate"].Value = ConstraintDate.AddDays(-2);
            //cmdSelectConstraintByIDForNewConstraints.Parameters["@EndDate"].Value = ConstraintDate;
            SqlDataReader reader = cmdSelectConstraintByIDForNewConstraints.ExecuteReader();

            while (reader.Read())
            {
                string constraint = reader.GetString(0);
                constraint = constraint.Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").Replace("Reacinf-ctg", "").TrimStart();
                string contingency = reader.IsDBNull(1) ? "" : reader.GetString(1);
                contingency = contingency.Replace("Contingency", "").TrimStart();
                int constraintRtNum = (int)reader.GetDecimal(2);
                //string RiskType = reader.GetString(4);
                double dollarImact = reader.IsDBNull(4) ? 0 : (double)reader.GetDecimal(4);

                double shiftFactor = double.NaN;
                try
                {
                    shiftFactor = (double)reader.GetDecimal(3);
                }
                catch (Exception ex)
                {

                }
                Tuple<string, string, double, double> tuple = new Tuple<string, string, double, double>(constraint, contingency, shiftFactor, dollarImact);
                if (!constraintContingencyHash.ContainsKey(constraintRtNum))
                {
                    constraintContingencyHash.Add(constraintRtNum, tuple);
                }
            }
            reader.Close();
            mSelectSenstivityByIDCommand.Parameters["@constraintDate"].Value = ConstraintDate;
            reader = mSelectSenstivityByIDCommand.ExecuteReader();
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

        /// <summary>
        /// Gets the constraint not exist.
        /// </summary>
        /// <param name="ConstraintDate">The constraint date.</param>
        /// <returns></returns>
        public List<Sensitivity> GetConstraintNotExist(DateTime ConstraintDate)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<Sensitivity> constraintNotExistList = new List<Sensitivity>();
            mSelectConstraintByIDNoExistCommand.Parameters["@constraintDate"].Value = ConstraintDate;
            SqlDataReader reader = mSelectConstraintByIDNoExistCommand.ExecuteReader();
            while (reader.Read())
            {
                string constraint = reader.GetString(0);
                constraint = constraint.Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").Replace("Reacinf-ctg", "").TrimStart();
                string contingency = reader.GetString(1);
                contingency = contingency.Replace("Contingency", "").TrimStart();
                int constraintRtNum = (int)reader.GetDecimal(2);
                Sensitivity sensitivity = new Sensitivity();
                sensitivity.Constraint = constraint;
                sensitivity.Contingency = contingency;
                sensitivity.ID = constraintRtNum;
                constraintNotExistList.Add(sensitivity);
            }
            reader.Close();
            VayuConnection.Close();
            return constraintNotExistList;
        }
        /// <summary>
        /// Gets the bid identifier.
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        /// <returns></returns>
        public BigInteger GetBidId(int portfolio)
        {
            DateTime now = DateTime.Now;
            BigInteger tempBidId = BigInteger.Parse(portfolio.ToString() + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() +
            now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + now.Millisecond.ToString());
            return tempBidId;
        }
        /// <summary>
        /// Gets the deenergized nodes.
        /// </summary>
        /// <returns></returns>
        public List<DeenergizedNode> GetDeenergizedNodes()
        {
            List<DeenergizedNode> ListDeenergizedNodes = new List<DeenergizedNode>();
            DeenergizedNode NodeDeenergized = new DeenergizedNode();
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                mSelectErcotDeenergizedNodesCmd = new SqlCommand();
                mSelectErcotDeenergizedNodesCmd.CommandText = "Select Name,MarketDate,Hours from Vayu..DeenergizedNodes where MarketDate='" + DateTime.Now.Date + "'";
                mSelectErcotDeenergizedNodesCmd.Connection = VayuConnection;
                SqlDataReader reader = mSelectErcotDeenergizedNodesCmd.ExecuteReader();
                while (reader.Read())
                {
                    NodeDeenergized = new DeenergizedNode();
                    NodeDeenergized.Name = reader.GetString(0);
                    NodeDeenergized.MarketDate = reader.GetDateTime(1);
                    NodeDeenergized.Hours = reader.GetInt32(2);
                    ListDeenergizedNodes.Add(NodeDeenergized);
                }
            }
            catch
            {

            }
            VayuConnection.Close();
            return ListDeenergizedNodes.ToList();
        }

        public List<string> GetStrDeenergizedNodes()
        {

            List<string> listNodes = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            mSelectErcotDeenergizedNodesCmd = new SqlCommand();
            mSelectErcotDeenergizedNodesCmd.CommandText = "Select Name,MarketDate,Hours from Vayu..DeenergizedNodes where MarketDate='" + DateTime.Now.Date + "'";
            mSelectErcotDeenergizedNodesCmd.Connection = VayuConnection;
            SqlDataReader reader = mSelectErcotDeenergizedNodesCmd.ExecuteReader();
            while (reader.Read())
            {
                // NodeDeenergized = new DeenergizedNode();
                string NodeName = reader.GetString(0);
                //NodeDeenergized.MarketDate = reader.GetDateTime(1);
                //  NodeDeenergized.Hours = reader.GetInt32(2);
                listNodes.Add(NodeName);
            }
            reader.Close();
            VayuConnection.Close();

            return listNodes;
        }
        /// <summary>
        /// Moves the trade.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="traderList">The trader list.</param>
        /// <param name="submitDate">The submit date.</param>
        /// <param name="forfeitureList">The forfeiture list.</param>
        public void MoveTrade(Action<List<Path>, List<string>> callback, List<Portfolio> traderList, DateTime submitDate, List<string> forfeitureList)
        {
            mPathList = new List<Path>();
            List<string> ignoreList = new List<string>();
            Dictionary<string, List<int>> sourceSinkHash = new Dictionary<string, List<int>>();
            foreach (Portfolio portfolio in traderList)
            {
                if (portfolio != null)
                {
                    var tempBidId = GetBidId(portfolio.ID);
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    mSelectPTPBidsCommand.Parameters["@tradeDate"].Value = submitDate;
                    mSelectPTPBidsCommand.Parameters["@traderId"].Value = portfolio.Name;
                    SqlDataReader reader = mSelectPTPBidsCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        int sourceNodeKey = Convert.ToInt32(reader.GetValue(0));
                        int sinkNodeKey = Convert.ToInt32(reader.GetValue(1));
                        int hour = reader.GetInt32(2);
                        PricingNode sourcePricingNode = DBAccess.GetNode(sourceNodeKey);
                        string sourceNode = sourcePricingNode.NodeName;
                        string sourceZone = sourcePricingNode.Zone;
                        PricingNode sinkPricingNode = DBAccess.GetNode(sinkNodeKey);
                        string sinkNode = sinkPricingNode.NodeName;
                        string sinkZone = sinkPricingNode.Zone;
                        if (forfeitureList != null)
                        {
                            string forfeitureKey = sourceNode + "?" + sinkNode;
                            if (forfeitureList.Contains(forfeitureKey))
                            {
                                if (!ignoreList.Contains(forfeitureKey))
                                {
                                    ignoreList.Add(forfeitureKey);
                                }
                                continue;
                            }
                        }
                        if (!(reader.IsDBNull(3) && reader.IsDBNull(4)))
                        {
                            double mw = Math.Round(Convert.ToDouble(reader.GetValue(3)), 2);
                            double price = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            List<int> hourList = new List<int>();
                            string key = sourceNodeKey + "?" + sinkNodeKey + "?" + price + "?" + mw;
                            if (sourceSinkHash.ContainsKey(key))
                            {
                                hourList = sourceSinkHash[key];
                                sourceSinkHash.Remove(key);
                            }
                            if (!hourList.Contains(hour))
                            {
                                hourList.Add(hour);
                            }
                            else
                            {
                                string bidId = tempBidId++ + ".1";
                                SaveUptosPtpBids(reader.GetInt32(2).ToString(), sourceNode, sinkNode, sourceNodeKey, sinkNodeKey, bidId, Math.Round(Convert.ToDouble(reader.GetValue(4)), 2),
                                                    Math.Round(Convert.ToDouble(reader.GetValue(3)), 2), portfolio.ID, portfolio.Market, submitDate, portfolio.Name, sourceZone, sinkZone);

                            }
                            sourceSinkHash.Add(key, hourList);
                        }
                        if (!(reader.IsDBNull(5) && reader.IsDBNull(6)))
                        {
                            double mw = Math.Round(Convert.ToDouble(reader.GetValue(5)), 2);
                            double price = Math.Round(Convert.ToDouble(reader.GetValue(6)), 2);
                            List<int> hourList = new List<int>();
                            string key = sourceNodeKey + "?" + sinkNodeKey + "?" + price + "?" + mw;
                            if (sourceSinkHash.ContainsKey(key))
                            {
                                hourList = sourceSinkHash[key];
                                sourceSinkHash.Remove(key);
                            }
                            if (!hourList.Contains(hour))
                            {
                                hourList.Add(hour);
                            }
                            else
                            {
                                string bidId = tempBidId++ + ".1";
                                SaveUptosPtpBids(reader.GetInt32(2).ToString(), sourceNode, sinkNode, sourceNodeKey, sinkNodeKey, bidId, Math.Round(Convert.ToDouble(reader.GetValue(6)), 2),
                                                    Math.Round(Convert.ToDouble(reader.GetValue(5)), 2), portfolio.ID, portfolio.Market, submitDate, portfolio.Name, sourceZone, sinkZone);

                            }
                            sourceSinkHash.Add(key, hourList);
                        }
                        if (!(reader.IsDBNull(7) && reader.IsDBNull(8)))
                        {
                            double mw = Math.Round(Convert.ToDouble(reader.GetValue(7)), 2);
                            double price = Math.Round(Convert.ToDouble(reader.GetValue(8)), 2);
                            List<int> hourList = new List<int>();
                            string key = sourceNodeKey + "?" + sinkNodeKey + "?" + price + "?" + mw;
                            if (sourceSinkHash.ContainsKey(key))
                            {
                                hourList = sourceSinkHash[key];
                                sourceSinkHash.Remove(key);
                            }
                            if (!hourList.Contains(hour))
                            {
                                hourList.Add(hour);
                            }
                            else
                            {
                                string bidId = tempBidId++ + ".1";
                                SaveUptosPtpBids(reader.GetInt32(2).ToString(), sourceNode, sinkNode, sourceNodeKey, sinkNodeKey, bidId, Math.Round(Convert.ToDouble(reader.GetValue(8)), 2),
                                                    Math.Round(Convert.ToDouble(reader.GetValue(7)), 2), portfolio.ID, portfolio.Market, submitDate, portfolio.Name, sourceZone, sinkZone);

                            }
                            sourceSinkHash.Add(key, hourList);
                        }
                        if (!(reader.IsDBNull(9) && reader.IsDBNull(10)))
                        {
                            double mw = Math.Round(Convert.ToDouble(reader.GetValue(9)), 2);
                            double price = Math.Round(Convert.ToDouble(reader.GetValue(10)), 2);
                            List<int> hourList = new List<int>();
                            string key = sourceNodeKey + "?" + sinkNodeKey + "?" + price + "?" + mw;
                            if (sourceSinkHash.ContainsKey(key))
                            {
                                hourList = sourceSinkHash[key];
                                sourceSinkHash.Remove(key);
                            }
                            if (!hourList.Contains(hour))
                            {
                                hourList.Add(hour);
                            }
                            else
                            {
                                string bidId = tempBidId++ + ".1";
                                SaveUptosPtpBids(reader.GetInt32(2).ToString(), sourceNode, sinkNode, sourceNodeKey, sinkNodeKey, bidId, Math.Round(Convert.ToDouble(reader.GetValue(10)), 2),
                                                    Math.Round(Convert.ToDouble(reader.GetValue(9)), 2), portfolio.ID, portfolio.Market, submitDate, portfolio.Name, sourceZone, sinkZone);

                            }
                            sourceSinkHash.Add(key, hourList);
                        }
                    }
                    VayuConnection.Close();
                    List<string> sourceSinkKeyList = sourceSinkHash.Keys.ToList<string>();
                    foreach (string sourceSinkKey in sourceSinkKeyList)
                    {
                        string[] sourceSinkKeys = sourceSinkKey.Split('?');
                        int sourceNodeKey = Int32.Parse(sourceSinkKeys[0]);
                        int sinkNodeKey = Int32.Parse(sourceSinkKeys[1]);
                        double price = double.Parse(sourceSinkKeys[2]);
                        double mw = double.Parse(sourceSinkKeys[3]);


                        PricingNode sourcePricingNode = DBAccess.GetNode(sourceNodeKey, portfolio.MarketKey);
                        string sourceNode = sourcePricingNode.NodeName;
                        string sourceZone = sourcePricingNode.Zone;
                        PricingNode sinkPricingNode = DBAccess.GetNode(sinkNodeKey, portfolio.MarketKey);
                        string sinkNode = sinkPricingNode.NodeName;
                        string sinkZone = sinkPricingNode.Zone;
                        List<int> hourList = sourceSinkHash[sourceSinkKey];
                        string hourStr = "";
                        foreach (int hour in hourList)
                        {
                            if (hourStr.Length == 0)
                            {
                                hourStr = hour.ToString();
                            }
                            else
                            {
                                hourStr = hourStr + "." + hour;
                            }
                        }
                        string bidId = tempBidId++ + ".1";
                        SaveUptosPtpBids(hourStr, sourceNode, sinkNode, sourceNodeKey, sinkNodeKey, bidId,
                            price, mw, portfolio.ID, portfolio.Market, submitDate, portfolio.Name, sourceZone, sinkZone);
                    }
                    VayuConnection.Close();
                }
            }
            callback(mPathList, ignoreList);
        }
        /// <summary>
        /// Saves the uptos PTP bids.
        /// </summary>
        /// <param name="hourStrs">The hour STRS.</param>
        /// <param name="source">The source.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="srcKey">The source key.</param>
        /// <param name="snkKey">The SNK key.</param>
        /// <param name="bidId">The bid identifier.</param>
        /// <param name="price">The price.</param>
        /// <param name="mw">The mw.</param>
        /// <param name="portfoliokey">The portfoliokey.</param>
        /// <param name="market">The market.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="sourceZone">The source zone.</param>
        /// <param name="sinkZone">The sink zone.</param>
        public void SaveUptosPtpBids(string hourStrs, string source, string sink, int srcKey, int snkKey, string bidId, double price, double mw, int portfoliokey, string market, DateTime startDate, string portfolio, string sourceZone, string sinkZone)
        {
            Path path = new Path();
            DateTime toDate = startDate;
            string[] hourTokens = hourStrs.Split('.');
            foreach (string hourStr in hourTokens)
            {
                double hour = double.Parse(hourStr);
                if (market == "PJM")
                {
                    mInsertEESBidsCommand.Parameters["@BidStatus"].Value = "IMPORTED";
                    mInsertEESBidsCommand.Parameters["@scheduleid"].Value = bidId;
                    mInsertEESBidsCommand.Parameters["@EndMarketDateTime"].Value = toDate.AddHours(hour);
                    mInsertEESBidsCommand.Parameters["@RequestedMW"].Value = mw;
                    mInsertEESBidsCommand.Parameters["@ClearedMW"].Value = 0;
                    mInsertEESBidsCommand.Parameters["@EndUserKey"].Value = "28";
                    mInsertEESBidsCommand.Parameters["@SourceNodeKey"].Value = srcKey;
                    mInsertEESBidsCommand.Parameters["@SinkNodeKey"].Value = snkKey;
                    mInsertEESBidsCommand.Parameters["@Source"].Value = source;
                    mInsertEESBidsCommand.Parameters["@Sink"].Value = sink;
                    mInsertEESBidsCommand.Parameters["@Price"].Value = price;
                    mInsertEESBidsCommand.Parameters["@PortfolioKey"].Value = portfoliokey;
                    mInsertEESBidsCommand.Parameters["@SubmittedDateTime"].Value = DateTime.Now;
                    mInsertEESBidsCommand.Parameters["@comments"].Value = string.Empty;
                    mInsertEESBidsCommand.ExecuteNonQuery();
                }
                else
                {
                    mInsertErcotEESBidsCommand.Parameters["@bidid"].Value = bidId;
                    mInsertErcotEESBidsCommand.Parameters["@Source"].Value = source;
                    mInsertErcotEESBidsCommand.Parameters["@Sink"].Value = sink;
                    mInsertErcotEESBidsCommand.Parameters["@Price"].Value = price;
                    mInsertErcotEESBidsCommand.Parameters["@EndMarketDateTime"].Value = toDate.AddHours(hour);
                    mInsertErcotEESBidsCommand.Parameters["@RequestedMW"].Value = mw;
                    mInsertErcotEESBidsCommand.Parameters["@BidStatus"].Value = "IMPORTED";
                    mInsertErcotEESBidsCommand.Parameters["@EndUserKey"].Value = 5;
                    mInsertErcotEESBidsCommand.Parameters["@PortfolioKey"].Value = portfoliokey;
                    mInsertErcotEESBidsCommand.Parameters["@SubmittedDateTime"].Value = DateTime.Now;
                    mInsertErcotEESBidsCommand.Parameters["@comments"].Value = string.Empty;
                    mInsertErcotEESBidsCommand.ExecuteNonQuery();
                }
            }
            path.Source = source;
            path.Sink = sink;
            path.AnalysisType = hourStrs;
            path.Price = price;
            path.MW = mw;
            path.Portfolio = portfolio;
            path.BidId = bidId;
            path.Status = "IMPORTED";
            path.SourceZone = sourceZone;
            path.SinkZone = sinkZone;
            mPathList.Add(path);
        }
        /// <summary>
        /// Deletes the uptos PTP bids.
        /// </summary>
        /// <param name="bidId">The bid identifier.</param>
        /// <param name="submitDate">The submit date.</param>
        /// <param name="market">The market.</param>
        public void DeleteUptosPtpBids(string bidId, DateTime submitDate, string market)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            DateTime marketDate = submitDate;
            DateTime toDate = marketDate;
            if (market == "PJM")
            {
                mDeleteEESBidsByIdCommand.Parameters["@scheduleid"].Value = bidId;
                mDeleteEESBidsByIdCommand.Parameters["@start"].Value = toDate;
                mDeleteEESBidsByIdCommand.Parameters["@end"].Value = toDate.AddDays(1);
                mDeleteEESBidsByIdCommand.ExecuteNonQuery();
            }
            else
            {
                mDeleteErcotEESBidsByIdCommand.Parameters["@bidid"].Value = bidId;
                mDeleteErcotEESBidsByIdCommand.Parameters["@start"].Value = toDate;
                mDeleteErcotEESBidsByIdCommand.Parameters["@end"].Value = toDate.AddDays(1);
                mDeleteErcotEESBidsByIdCommand.ExecuteNonQuery();
            }
            VayuConnection.Close();
        }
        /// <summary>
        /// Sets the trade identifier.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public string SetTradeId(string portfolioKey, string market)
        {
            DateTime now = DateTime.Now;
            string tradeID = GetBidId(Int32.Parse(portfolioKey)) + ".1";
            if (market == "ERCOT")
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }

                string init = portfolioKey;
                mSelectErcotPortfolioCommand.Parameters["@portfoliokey"].Value = portfolioKey;
                SqlDataReader reader = mSelectErcotPortfolioCommand.ExecuteReader();
                while (reader.Read())
                {
                    init = reader.GetString(0);
                }
                reader.Close();
                init = init.Replace("ERCOT ", "");
                DateTime start = DateTime.Today.AddDays(1);
                mSelectPTPBidIdsCommand.Parameters["@portfoliokey"].Value = portfolioKey;
                mSelectPTPBidIdsCommand.Parameters["@start"].Value = start;
                mSelectPTPBidIdsCommand.Parameters["@end"].Value = start.AddDays(1);
                reader = mSelectPTPBidIdsCommand.ExecuteReader();
                List<int> bidIdList = new List<int>();
                while (reader.Read())
                {
                    string bidIdStr = reader.GetString(0);
                    init = bidIdStr.Substring(0, bidIdStr.IndexOf("_"));
                    try
                    {
                        bidIdList.Add(Int32.Parse(bidIdStr.Substring(bidIdStr.IndexOf("_") + 1)));
                    }
                    catch (FormatException)
                    {

                    }
                }
                bidIdList.Sort();
                int bidId = bidIdList.Count == 0 ? 0 : bidIdList[bidIdList.Count - 1];
                reader.Close();
                VayuConnection.Close();
                tradeID = init + "_" + ++bidId;
            }
            return tradeID;
        }
        /// <summary>
        /// Saves the preference.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="preference">The preference.</param>
        public void SavePreference(string user, string preference)
        {
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mDeletePreferenceCommand.Parameters[0].Value = user;
                mDeletePreferenceCommand.ExecuteNonQuery();
                mInsertPreferenceCommand.Parameters["@trader"].Value = user;
                mInsertPreferenceCommand.Parameters["@preference"].Value = preference;
                mInsertPreferenceCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                VayuConnection.Close();
            }
        }
        /// <summary>
        /// Gets the preference.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public string GetPreference(string user)
        {
            string preference = string.Empty;
            if (mSelectPreferenceCommand != null)
            {
                if (mSelectPreferenceCommand.Connection.State.Equals(ConnectionState.Closed))
                    mSelectPreferenceCommand.Connection.Open();
                mSelectPreferenceCommand.Parameters[0].Value = user;
                try
                {
                    IDataReader reader = mSelectPreferenceCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        preference = reader.GetValue(0).ToString();
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                finally
                {
                    VayuConnection.Close();
                }
            }
            return preference;
        }
        /// <summary>
        /// Gets the maximum load.
        /// </summary>
        /// <param name="similarDate">The similar date.</param>
        /// <returns></returns>
        public double GetMaxLoad(DateTime similarDate)
        {
            double maxLoad = double.NaN;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectMaxLoadCommand.Parameters["@loadDate"].Value = similarDate;
            SqlDataReader reader = mSelectMaxLoadCommand.ExecuteReader();
            while (reader.Read())
            {

                maxLoad = reader.IsDBNull(0) ? -9999999 : Convert.ToDouble(reader.GetValue(0));
            }
            VayuConnection.Close();
            return maxLoad;
        }

        public int CheckSubmittedPortfolio(DateTime SubmitDate, int portfolio)
        {
            //int count=0;
            //if (VayuConnection.State == ConnectionState.Closed)
            //{
            //    VayuConnection.Open();
            //}
            //sSelectSubmittedCount.Parameters["@portfoliokey"].Value = portfolio;
            //sSelectSubmittedCount.Parameters["@SubmitDate"].Value = SubmitDate;
            //sSelectSubmittedCount.Parameters["@EndDate"].Value = SubmitDate.AddDays(1);
            //SqlDataReader reader = sSelectSubmittedCount.ExecuteReader();
            //while (reader.Read())
            //{

            //    //count = reader.GetInt32(0); 
            //   int count1 = Convert.ToInt32(reader.GetValue(0));
            //    count = count1;
            //}
            //VayuConnection.Close();
            //return count;

            int count = 0;
            using (SqlConnection VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand sSelectSubmittedCount = new SqlCommand())
                {
                    sSelectSubmittedCount.CommandText = @"
            SELECT COUNT(*) 
            FROM ErcotPTPBids 
            WHERE BidStatus = 'Valid' 
              AND PortfolioKey = @portfoliokey 
              AND EndMarketDateTime > @SubmitDate 
              AND EndMarketDateTime <= @EndDate";
                    sSelectSubmittedCount.Parameters.AddWithValue("@portfoliokey", portfolio);
                    sSelectSubmittedCount.Parameters.AddWithValue("@SubmitDate", SubmitDate);
                    sSelectSubmittedCount.Parameters.AddWithValue("@EndDate", SubmitDate.AddDays(1));
                    sSelectSubmittedCount.Connection = VayuConnection;

                    count = (int)sSelectSubmittedCount.ExecuteScalar();
                }
            }
            return count;
        }

        public List<Portfolio> GetExternalPortfolio(DateTime sDate, DateTime eDate)
        {


            #region new by sangramp
            SqlDataReader portfilioreader = null;
            List<Portfolio> portfolioList_all = new List<Portfolio>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand cmdgetallPortfolios = VayuConnection.CreateCommand();
            cmdgetallPortfolios.CommandText = "select distinct  b.ParticipantID,b.Participant from Vayu..DAM60DAYPTPBIDS a join Vayu..Company b " +
                                            "  ON a.ParticipantID = b.ParticipantID where a.DeliveryDate > @START and a.DeliveryDate <= @END " +
                                            "  and b.Marketkey = 9 and b.Participant not in ('QENJRE') order by b.Participant ";
            cmdgetallPortfolios.Parameters.AddWithValue("@START", "ENDMARKETDATETIME");
            cmdgetallPortfolios.Parameters.AddWithValue("@END", "ENDMARKETDATETIME");
            cmdgetallPortfolios.Connection = VayuConnection;

            cmdgetallPortfolios.Parameters["@START"].Value = sDate;
            cmdgetallPortfolios.Parameters["@END"].Value = eDate;
            portfilioreader = cmdgetallPortfolios.ExecuteReader();
            while (portfilioreader.Read())
            {
                string name = portfilioreader.GetString(1);
                int id = portfilioreader.GetInt32(0);
                Portfolio portfolio = new Portfolio();
                portfolio.ID = id;
                portfolio.Name = name;
                if (!portfolioList_all.Contains(portfolio))
                {
                    portfolioList_all.Add(portfolio);
                }
            }
            cmdgetallPortfolios.Connection.Close();
            #endregion

            return portfolioList_all;
        }
        /// <summary>
        /// Gets the database bids.
        /// </summary>
        /// <param name="marketDateTime">The market date time.</param>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <returns></returns>
        ///
        public List<Bid> GetDBBids(DateTime marketDateTime, int portfolioKey)
        {
            List<Bid> dbBids = new List<Bid>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectDBBidsCommand.Parameters["@startdate"].Value = marketDateTime;
            mSelectDBBidsCommand.Parameters["@enddate"].Value = marketDateTime.AddDays(1);
            mSelectDBBidsCommand.Parameters["@PortfolioKey"].Value = portfolioKey;
            SqlDataReader reader = mSelectDBBidsCommand.ExecuteReader();
            while (reader.Read())
            {
                Bid biD = new Bid();
                biD.Source = Convert.ToInt32(reader.GetValue(0));
                biD.Sink = Convert.ToInt32(reader.GetValue(1));
                biD.Price = Convert.ToDouble(reader.GetValue(2));
                biD.MW = Convert.ToDouble(reader.GetValue(3));
                dbBids.Add(biD);
            }
            VayuConnection.Close();
            return dbBids;

        }
        public Dictionary<string, string> GetFuelSource()
        {
            Dictionary<string, string> DictFuel = new Dictionary<string, string>();
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                SqlDataReader reader = mSelectFuelSourceCommand.ExecuteReader();
                while (reader.Read())
                {
                    string NodeName = reader.GetString(0);
                    string FuelName = reader.GetString(1);
                    if (!DictFuel.ContainsKey(NodeName))
                        DictFuel.Add(NodeName, FuelName);
                }
                DictFuel.Add("Blank", "Blank");
            }
            catch (Exception ex)
            {

            }
            return DictFuel;
        }
        #endregion
        internal double? GetMaxDailyLoad(DateTime date)
        {
            try
            {
                // loadDBCommands();
                double? maxLoad = 0.0;
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "select  max(MW) from LoadRT where LoadsKey = 25 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";
                cmd.Connection = VayuConnection;
                cmd.Parameters.AddWithValue("@StartDate", date.AddDays(-1));
                cmd.Parameters.AddWithValue("@EndDate", date);
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                maxLoad = Vayu.CommonAccessLibrary.CommonDataConversions.GetDouble(cmd.ExecuteScalar());
                VayuConnection.Close();
                return maxLoad;
            }
            catch
            {
                return 0.0;
            }
        }
        internal double? GetMaxHourlyLoad(DateTime date)
        {
            try
            {
                //loadDBCommands();
                double? maxLoad = 0.0;
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "select  max(MW) from LoadRTH where LoadsKey = 25 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";
                cmd.Connection = VayuConnection;
                cmd.Parameters.AddWithValue("@StartDate", date.AddDays(-1));
                cmd.Parameters.AddWithValue("@EndDate", date);
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                maxLoad = Vayu.CommonAccessLibrary.CommonDataConversions.GetDouble(cmd.ExecuteScalar());
                VayuConnection.Close();
                return maxLoad;
            }
            catch
            {
                return 0.0;
            }
        }
        internal List<ClearedPathsHelper> GetClearedPaths(string market, List<Path> Pathlist, int portfolioKey, DateTime sDate, DateTime eDate, DateTime portfolioDate, int marketKey)
        {
            List<DateTime> dateTimeList = new List<DateTime>();
            List<Bid> bidList = DBAccess.GetBids(market, portfolioKey, null, portfolioDate, portfolioDate.AddDays(1),
                                    true, "MOVED");
            Dictionary<string, PricingNode> NodeDetailsHash = DBAccess.GetPricingNodesForMarket(marketKey);
            Dictionary<int, List<DateTime>> nodeDateHash = new Dictionary<int, List<DateTime>>();
            //  foreach (Path path in Pathlist)
            //   List<Node> nodeList = new List<Node>();
            List<int> nodeList = new List<int>();
            {

                List<string> sourceList = Pathlist.Select(a => a.Source).ToList();
                List<string> sinkList = Pathlist.Select(a => a.Sink).ToList();
                foreach (string node1 in sourceList)
                {
                    Node node = new Node();
                    if (!NodeDetailsHash.ContainsKey(node1))
                    {
                        continue;
                    }
                    PricingNode noedeDet = NodeDetailsHash[node1];
                    node.NodeId = noedeDet.NodeKey;
                    node.NodeName = node1;
                    if (!nodeList.Contains(noedeDet.NodeKey))
                    {
                        nodeList.Add(noedeDet.NodeKey);
                    }

                }

                foreach (string node1 in sinkList)
                {
                    Node node = new Node();
                    PricingNode noedeDet = NodeDetailsHash[node1];
                    node.NodeId = noedeDet.NodeKey;
                    node.NodeName = node1;
                    if (!nodeList.Contains(noedeDet.NodeKey))
                    {
                        nodeList.Add(noedeDet.NodeKey);
                    }
                }
            }
            DateTime tempStartate = sDate;
            while (tempStartate <= eDate)
            {
                dateTimeList.Add(tempStartate);
                tempStartate = tempStartate.AddHours(1);
            }
            foreach (int nodeKey in nodeList)
            {
                nodeDateHash.Add(nodeKey, dateTimeList);
            }


            DARTNode.GetDartMarket(nodeDateHash, "da", 9);

            List<ClearedPathsHelper> clearedList = new List<ClearedPathsHelper>();
            foreach (Path path in Pathlist)
            {
                if (path.Source == "DPL NORTH" || path.Sink == "DPL_ODEC")
                {

                }
                PricingNode sourceNode = NodeDetailsHash[path.Source];
                PricingNode sinkNode = NodeDetailsHash[path.Sink];
                double da = double.MaxValue;
                List<string> hoursList = new List<string>();
                DateTime eDate1 = eDate.AddDays(1);
                while (eDate1 >= sDate)
                {
                    DateTime clearedDate = eDate1.Hour == 0 ? eDate1.AddDays(-1) : eDate1;
                    int hour = clearedDate.Hour == 0 ? 24 : clearedDate.Hour;
                    hoursList = path.AnalysisType.Split('.').ToList();
                    if (!hoursList.Contains(hour.ToString()))
                    {
                        eDate1 = eDate1.AddHours(-1);
                        continue;
                    }
                    string sourceKey = clearedDate.ToString() + sourceNode.NodeKey;
                    string sinkKey = clearedDate.ToString() + sinkNode.NodeKey;
                    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey)
                                               && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                    {
                        da = (DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey]);
                    }
                    if (path.Price > da)
                    {
                        ClearedPathsHelper cph = new ClearedPathsHelper();
                        cph.Source = path.Source;
                        cph.Sink = path.Sink;
                        cph.Hours = path.AnalysisType;
                        cph.Price = path.Price;
                        cph.ClearedDate = clearedDate.ToString("yyyy-MM-dd");
                        if (clearedDate < DateTime.Today)
                        {
                            cph.Days = (clearedDate.Date - DateTime.Today.Date).Days;
                        }
                        else
                        {
                            cph.Days = 0;
                        }
                        clearedList.Add(cph);
                        break;
                    }
                    eDate1 = eDate1.AddHours(-1);
                }
            }

            foreach (Path path in Pathlist)
            {
                bool isExists = clearedList.Exists(a => a.Source == path.Source && a.Sink == path.Sink && a.Hours == path.AnalysisType && a.Price == path.Price);
                if (!isExists)
                {
                    ClearedPathsHelper cph = new ClearedPathsHelper();

                    cph.Source = path.Source;
                    cph.Sink = path.Sink;
                    cph.Hours = path.AnalysisType;
                    cph.Price = path.Price;
                    cph.ClearedDate = "Never Cleared In Selected Date Range";
                    clearedList.Add(cph);
                }
            }



            return clearedList;
        }

        internal Dictionary<string, double> GetLoadsData(List<DateTime> datetimeList, string Market)
        {
            try
            {
                Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    //List<DateTime> temiList = datetimeList.GroupBy(x => x.ToString("dd-MM-yyyy"));
                    foreach (DateTime date in datetimeList)
                    {
                        double load = 0;
                        if (!loadDictHash.ContainsKey(date.AddDays(-1).Date.ToString("dd-MM-yyyy")))
                        {
                            if (Market == "PJM")
                            {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuConnection.State == ConnectionState.Closed)
                                {
                                    VayuConnection.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from LoadRT where loadskey=25 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate"
                                                                      + " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date.AddDays(-1));
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date);
                                mSelectloadDailyCommand.Connection = VayuConnection;
                                SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                }
                                reader.Close();
                                if (load == 0)
                                {
                                    mSelectloadHourlyCommand = new SqlCommand();
                                    mSelectloadHourlyCommand.CommandText = "select  max(MW) from LoadRTH where LoadsKey = 25 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@StartDate", date.Date.AddDays(-1));
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@EndDate", date.Date);
                                    mSelectloadHourlyCommand.Connection = VayuConnection;
                                    SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                    while (reader1.Read())
                                    {
                                        load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                    }
                                    reader1.Close();
                                }
                            }
                            else //For Ercot
                            {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuConnection.State == ConnectionState.Closed)
                                {
                                    VayuConnection.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from Vayu..LoadRT where loadskey=2213 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate"
                                                                      + " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date.AddDays(-1));
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date);
                                mSelectloadDailyCommand.Connection = VayuConnection;
                                SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                }
                                reader.Close();
                                if (load == 0)
                                {
                                    mSelectloadHourlyCommand = new SqlCommand();
                                    mSelectloadHourlyCommand.CommandText = "select  max(MW) from Vayu..LoadRT where LoadsKey = 2213 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";//LoadRTH 
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@StartDate", date.Date.AddDays(-1));
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@EndDate", date.Date);
                                    mSelectloadHourlyCommand.Connection = VayuConnection;
                                    SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                    while (reader1.Read())
                                    {
                                        load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                    }
                                    reader1.Close();
                                }
                            }
                            loadDictHash.Add(date.AddDays(-1).Date.ToString("dd-MM-yyyy"), load);
                        }

                    }
                }
                VayuConnection.Close();
                return loadDictHash;

            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        private List<string> GetOutageConstraintList(bool StartChecked, string ShadowPrice, DateTime StartDate)
        {
            List<string> OutageConstraintList = new List<string>();
            if (StartChecked)
            {
                mSelectStartCheckedConstraintCommand.Parameters["@StartDate"].Value = StartDate.Date;
                mSelectStartCheckedConstraintCommand.Parameters["@EndDate"].Value = StartDate.Date.AddDays(1);
                mSelectStartCheckedConstraintCommand.Parameters["@ShadowPrice"].Value = ShadowPrice;
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                SqlDataReader reader = mSelectStartCheckedConstraintCommand.ExecuteReader();
                while (reader.Read())
                {
                    OutageConstraintList.Add(reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)));
                }
                reader.Close();

            }
            else
            {
                mSelectOutageCheckedConstraintCommand.Parameters["@StartDate"].Value = StartDate;
                mSelectOutageCheckedConstraintCommand.Parameters["@EndDate"].Value = StartDate.AddDays(1).Date;
                mSelectOutageCheckedConstraintCommand.Parameters["@ShadowPrice"].Value = ShadowPrice;
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                SqlDataReader reader = mSelectOutageCheckedConstraintCommand.ExecuteReader();
                while (reader.Read())
                {
                    OutageConstraintList.Add(reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)));
                }
                reader.Close();
            }
            return OutageConstraintList;

        }

        private Dictionary<string, int> GetNodeHash(int marketKey)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<string, int> Nodehash = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            mSelectStartCheckedNodeCommand.Parameters["@MarketKey"].Value = marketKey;
            SqlDataReader reader = mSelectStartCheckedNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                Nodehash.Add(reader[0].ToString(), (int)reader.GetValue(1));
            }
            reader.Close();
            //VayuDbConn.Close();
            return Nodehash;
        }

        public Dictionary<DateTime, Dictionary<int, double>> GetHourlyImpact(DateTime startDate, DateTime endDate, string market, bool isDA)
        {
            Dictionary<DateTime, Dictionary<int, double>> hourlyImpactHash = new Dictionary<DateTime, Dictionary<int, double>>();
            SqlCommand getHourlyIpmactCmd = new SqlCommand();// VayuDbConn.CreateCommand();
            if (market.ToUpper() == "PJM")
            {
                getHourlyIpmactCmd.CommandText = " select ConstraintRTNum , Date , Hour , Impact from RTImpact where date >= @startDate and date < @endDate ";
                getHourlyIpmactCmd.Connection = VayuConnection;
            }
            else if (market.ToUpper() == "ERCOT")
            {
                if (isDA)
                    getHourlyIpmactCmd.CommandText = "select ConstraintRTNum , Date , Hour , Impact from Vayu..DAImpact where date >= @startDate and date < @endDate ";
                else
                    getHourlyIpmactCmd.CommandText = " select ConstraintRTNum , Date , Hour , Impact from Vayu..RTImpact where date >= @startDate and date < @endDate ";

                getHourlyIpmactCmd.Connection = VayuConnection;
            }

            getHourlyIpmactCmd.Parameters.AddWithValue("@startDate", startDate);
            getHourlyIpmactCmd.Parameters.AddWithValue("@endDate", endDate);
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }

                SqlDataReader rdr = getHourlyIpmactCmd.ExecuteReader();
                while (rdr.Read())
                {
                    int constraintnum = Convert.ToInt32(rdr.GetValue(0));
                    DateTime constDate = Convert.ToDateTime(rdr.GetValue(1));
                    int hour = Convert.ToInt32(rdr.GetValue(2));
                    DateTime constdatetime = hour == 24 ? constDate.AddDays(1) : constDate.AddHours(hour);
                    double impact = Convert.ToDouble(rdr.GetValue(3));
                    if (hourlyImpactHash.ContainsKey(constdatetime))
                    {
                        Dictionary<int, double> tempImpacthash = hourlyImpactHash[constdatetime];
                        if (!tempImpacthash.ContainsKey(constraintnum))
                        {
                            tempImpacthash.Add(constraintnum, impact);
                        }
                    }
                    else
                    {
                        Dictionary<int, double> tempImpacthash = new Dictionary<int, double>();
                        tempImpacthash.Add(constraintnum, impact);
                        hourlyImpactHash.Add(constdatetime, tempImpacthash);
                    }
                }
                rdr.Close();
                VayuConnection.Close();
            }
            catch (Exception ex)
            {
            }
            return hourlyImpactHash;
        }
        public void UpdateScaleNumber(double num, Portfolio TraderPortfolioSelected, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                int portfoliokey = TraderPortfolioSelected.ID;

                int marketkey = TraderPortfolioSelected.MarketKey;
                if (marketkey == 0)
                {

                    if (TraderPortfolioSelected.Market == "ERCOT")
                        marketkey = 9;
                }

                loadDBCommands();
                if (marketkey == 9)
                {
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    stUpdateErcotEESBids.Parameters["@PortfolioKey"].Value = portfoliokey;
                    stUpdateErcotEESBids.Parameters["@HEStartDateTime"].Value = StartDate;
                    stUpdateErcotEESBids.Parameters["@HEEndDateTime"].Value = EndDate;
                    stUpdateErcotEESBids.Parameters["@num"].Value = num;
                    stUpdateErcotEESBids.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
        }

        internal void UpdateScaleNumberSourceSinkHRS(double num, Portfolio TraderPortfolioSelected, DateTime dateTime, string Source, string Sink)
        {
            try
            {
                loadDBCommands();
                int portfoliokey = TraderPortfolioSelected.ID;
                int marketkey = TraderPortfolioSelected.MarketKey;
                int sourcekey = DBAccess.GetNodeFromName(Source, marketkey).NodeKey;
                int sinkkey = DBAccess.GetNodeFromName(Sink, marketkey).NodeKey;
                if (marketkey == 0)
                {
                    if (TraderPortfolioSelected.Market == "ERCOT")
                        marketkey = 9;
                }
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                SqlConnection conn = new SqlConnection();

                conn = VayuConnection;
                SqlCommand cmd = new SqlCommand("UpdateScalingErcot", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@PortfolioKey", SqlDbType.Int);
                cmd.Parameters["@PortfolioKey"].Value = portfoliokey;

                cmd.Parameters.Add("@EndMarketDateTime", SqlDbType.DateTime);
                cmd.Parameters["@EndMarketDateTime"].Value = dateTime;

                cmd.Parameters.Add("@num", SqlDbType.Decimal);
                cmd.Parameters["@num"].Value = num;

                cmd.Parameters.Add("@SourceNodeKey", SqlDbType.BigInt);
                cmd.Parameters["@SourceNodeKey"].Value = sourcekey;

                cmd.Parameters.Add("@SinkNodeKey", SqlDbType.BigInt);
                cmd.Parameters["@SinkNodeKey"].Value = sinkkey;

                try
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    conn.Close();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    conn.Close();
                }
            }

            catch (Exception ex)
            {

            }
        }
    }
    public class RiskLimmit
    {
        public int PortfolioId { get; set; }

        public double Credit { get; set; }

        public double TotalMW { get; set; }

        public double MWPerNode { get; set; }

        public double Risk { get; set; }

        public double SeasonRisk { get; set; }
    }

    public class PathListMW
    {
        public int SourceNodekey { get; set; }

        public int SinkNodekey { get; set; }

        public double mw { get; set; }
        /// <summary>
        /// Gets or sets the type of the analysis.
        /// </summary>
        /// <value>
        /// The type of the analysis.
        /// </value>
        public string AnalysisType { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price { get; set; }
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ExposureHour
    {
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
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the sensitivity.
        /// </summary>
        /// <value>
        /// The sensitivity.
        /// </value>
        public double? Sensitivity { get; set; }
        /// <summary>
        /// Gets or sets the shift.
        /// </summary>
        /// <value>
        /// The shift.
        /// </value>
        public double? Shift { get; set; }
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime { get; set; }
    }
    public class RiskLimit
    {
        public int PortfolioId { get; set; }

        public double Credit { get; set; }

        public double TotalMW { get; set; }

        public double MWPerNode { get; set; }

        public double Risk { get; set; }

        public double SeasonRisk { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ExposureNodeDetail
    {
        /// <summary>
        /// Gets or sets the source node.
        /// </summary>
        /// <value>
        /// The source node.
        /// </value>
        public string SourceNode { get; set; }
        /// <summary>
        /// Gets or sets the sink node.
        /// </summary>
        /// <value>
        /// The sink node.
        /// </value>
        public string SinkNode { get; set; }
        /// <summary>
        /// Gets or sets the constraint text.
        /// </summary>
        /// <value>
        /// The constraint text.
        /// </value>
        public string ConstraintText { get; set; }
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public double? Constraint { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Exposure
    {
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
        /// Gets or sets the h e1.
        /// </summary>
        /// <value>
        /// The h e1.
        /// </value>
        public double? HE1 { get; set; }
        /// <summary>
        /// Gets or sets the h e2.
        /// </summary>
        /// <value>
        /// The h e2.
        /// </value>
        public double? HE2 { get; set; }
        /// <summary>
        /// Gets or sets the h e3.
        /// </summary>
        /// <value>
        /// The h e3.
        /// </value>
        public double? HE3 { get; set; }
        /// <summary>
        /// Gets or sets the h e4.
        /// </summary>
        /// <value>
        /// The h e4.
        /// </value>
        public double? HE4 { get; set; }
        /// <summary>
        /// Gets or sets the h e5.
        /// </summary>
        /// <value>
        /// The h e5.
        /// </value>
        public double? HE5 { get; set; }
        /// <summary>
        /// Gets or sets the h e6.
        /// </summary>
        /// <value>
        /// The h e6.
        /// </value>
        public double? HE6 { get; set; }
        /// <summary>
        /// Gets or sets the h e7.
        /// </summary>
        /// <value>
        /// The h e7.
        /// </value>
        public double? HE7 { get; set; }
        /// <summary>
        /// Gets or sets the h e8.
        /// </summary>
        /// <value>
        /// The h e8.
        /// </value>
        public double? HE8 { get; set; }
        /// <summary>
        /// Gets or sets the h e9.
        /// </summary>
        /// <value>
        /// The h e9.
        /// </value>
        public double? HE9 { get; set; }
        /// <summary>
        /// Gets or sets the h e10.
        /// </summary>
        /// <value>
        /// The h e10.
        /// </value>
        public double? HE10 { get; set; }
        /// <summary>
        /// Gets or sets the h e11.
        /// </summary>
        /// <value>
        /// The h e11.
        /// </value>
        public double? HE11 { get; set; }
        /// <summary>
        /// Gets or sets the h e12.
        /// </summary>
        /// <value>
        /// The h e12.
        /// </value>
        public double? HE12 { get; set; }
        /// <summary>
        /// Gets or sets the h e13.
        /// </summary>
        /// <value>
        /// The h e13.
        /// </value>
        public double? HE13 { get; set; }
        /// <summary>
        /// Gets or sets the h e14.
        /// </summary>
        /// <value>
        /// The h e14.
        /// </value>
        public double? HE14 { get; set; }
        /// <summary>
        /// Gets or sets the h e15.
        /// </summary>
        /// <value>
        /// The h e15.
        /// </value>
        public double? HE15 { get; set; }
        /// <summary>
        /// Gets or sets the h e16.
        /// </summary>
        /// <value>
        /// The h e16.
        /// </value>
        public double? HE16 { get; set; }
        /// <summary>
        /// Gets or sets the h e17.
        /// </summary>
        /// <value>
        /// The h e17.
        /// </value>
        public double? HE17 { get; set; }
        /// <summary>
        /// Gets or sets the h e18.
        /// </summary>
        /// <value>
        /// The h e18.
        /// </value>
        public double? HE18 { get; set; }
        /// <summary>
        /// Gets or sets the h e19.
        /// </summary>
        /// <value>
        /// The h e19.
        /// </value>
        public double? HE19 { get; set; }
        /// <summary>
        /// Gets or sets the h e20.
        /// </summary>
        /// <value>
        /// The h e20.
        /// </value>
        public double? HE20 { get; set; }
        /// <summary>
        /// Gets or sets the h e21.
        /// </summary>
        /// <value>
        /// The h e21.
        /// </value>
        public double? HE21 { get; set; }
        /// <summary>
        /// Gets or sets the h e22.
        /// </summary>
        /// <value>
        /// The h e22.
        /// </value>
        public double? HE22 { get; set; }
        /// <summary>
        /// Gets or sets the h e23.
        /// </summary>
        /// <value>
        /// The h e23.
        /// </value>
        public double? HE23 { get; set; }
        /// <summary>
        /// Gets or sets the h e24.
        /// </summary>
        /// <value>
        /// The h e24.
        /// </value>
        public double? HE24 { get; set; }
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
    }
    /// <summary>
    /// 
    /// </summary>
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
        public Microsoft.Maps.MapControl.WPF.Location MapLocation { get; set; }
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
    }
}
