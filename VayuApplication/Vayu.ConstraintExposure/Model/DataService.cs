using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.ConstraintExposure.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.ConstraintExposure.Model.IDataService" />
    public class DataService : IDataService
    {
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection VayuConnection;

        #region SQL Commands

        /// <summary>
        /// The m select path dollars command
        /// </summary>
        private SqlCommand mSelectPathDollarsCommand;
        private SqlCommand mErcotSelectPathDollarsCommandDA;
        private SqlCommand mErcotSelectPathDollarsCommandRT;
        /// <summary>
        /// The m select nodal dollars command
        /// </summary>
        private SqlCommand mSelectNodalDollarsCommand;
        private SqlCommand mErcotSelectNodalDollarsCommand;
        private SqlCommand mErcotSelectNodalDollarsCommandRT;
        private SqlCommand mErcotSelectNodalDollarsCommandDA;
        private SqlCommand mErcotSelectPathMWsOneDateCommandDA;
        private SqlCommand mErcotSelectPathMWsOneDateCommandRT;

        /// <summary>
        /// The m select nodal m ws one date command
        /// </summary>
        private SqlCommand mSelectNodalMWsOneDateCommand;
        private SqlCommand mErcotSelectNodalMWsOneDateCommandDA;
        private SqlCommand mErcotSelectNodalMWsOneDateCommandRT;
        /// <summary>
        /// The m select path m ws on lookback command
        /// </summary>
        private SqlCommand mSelectPathMWsOnLookbackCommand;
        private SqlCommand mErcotSelectPathMWsOnLookbackCommandRT;
        private SqlCommand mErcotSelectPathMWsOnLookbackCommandDA;
        /// <summary>
        /// The m select nodal m ws on lookback command
        /// </summary>
        private SqlCommand mSelectNodalMWsOnLookbackCommand;
        private SqlCommand mErcotSelectNodalMWsOnLookbackCommand;
        /// <summary>
        /// The m select todays hour command
        /// </summary>
        private SqlCommand mSelectTodaysHourCommand;
        /// <summary>
        /// The m select no Sensitivity constraints one date command
        /// </summary>
        private SqlCommand mSelectNoSensitivityConstraintsOneDateCommand;
        private SqlCommand mErcotSelectNoSensitivityConstraintsOneDateCommandRT;
        private SqlCommand mErcotSelectNoSensitivityConstraintsOneDateCommandDA;
        /// <summary>
        /// The m select no Sensitivity constraints on lookback command
        /// </summary>
        private SqlCommand mSelectNoSensitivityConstraintsOnLookbackCommand;
        private SqlCommand mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT;
        private SqlCommand mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA;
        private SqlCommand mErcotSelectNoSensitivityConstraintsOnLookbackCommand;
        /// <summary>
        /// The m select path constraint mw exp command
        /// </summary>
        private SqlCommand mSelectPathConstraintMWExpCommand;
        private SqlCommand mErcotSelectPathConstraintMWExpCommandRT;
        private SqlCommand mErcotSelectPathConstraintMWExpCommandDA;
        /// <summary>
        /// The m select nodal constraint mw exp command
        /// </summary>
        private SqlCommand mSelectNodalConstraintMWExpCommand;
        private SqlCommand mErcotSelectNodalConstraintMWExpCommandRT;
        private SqlCommand mErcotSelectNodalConstraintMWExpCommandDA;
        /// <summary>
        /// The m select nodal constraint mw exp best command
        /// </summary>
        private SqlCommand mSelectNodalConstraintMWExpBestCommand;
        private SqlCommand mErcotSelectNodalConstraintMWExpBestCommandRT;
        private SqlCommand mErcotSelectNodalConstraintMWExpBestCommandDA;
        /// <summary>
        /// The m select path constraint mw exp best command
        /// </summary>
        private SqlCommand mSelectPathConstraintMWExpBestCommand;
        private SqlCommand mErcotSelectPathConstraintMWExpBestCommandDA;
        private SqlCommand mErcotSelectPathConstraintMWExpBestCommandRT;
        /// <summary>
        /// The m select most recent constraint scrape command
        /// </summary>
        private SqlCommand mSelectMostRecentConstraintScrapeCommand;
        /// <summary>
        /// The m select start checked constraints command
        /// </summary>
        private SqlCommand mSelectStartCheckedConstraintsCommandRT;
        private SqlCommand mSelectStartCheckedConstraintsCommandDA;
        /// <summary>
        /// The m select outage checked constraint command
        /// </summary>
        private SqlCommand mSelectOutageCheckedConstraintCommand;
        /// <summary>
        /// The m select path on start checked command
        /// </summary>
        private SqlCommand mSelectPathOnStartCheckedCommand;
        private SqlCommand mErcotSelectPathOnStartCheckedCommand;
        /// <summary>
        /// The m select nodal on start checked command
        /// </summary>
        private SqlCommand mSelectNodalOnStartCheckedCommand;
        private SqlCommand mErcotSelectNodalOnStartCheckedCommand;
        /// <summary>
        /// The m select constraint checked command
        /// </summary>
        private SqlCommand mSelectConstraintCheckedCommand;

        #endregion

        /// <summary>
        /// The s dart end point
        /// </summary>
        private static string sDartEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress();

        #region Public Methods

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectStartCheckedConstraintsCommandRT = new SqlCommand();
            mSelectStartCheckedConstraintsCommandRT.CommandText = "select distinct MonitoredText,c.constraintRTNum,c.ContingencyText, marketdatetime " +
                                "from pjm.ConstraintOutages a, PJM_rt_outages b, RTMasterConstraint c " +
                                "where b.StartDate >= @StartDate and b.startdate < @EndDate and (b.EndDate > @StartDate or " +
                                "b.EndDate is null) and a.driver = b.Equipment and c.MonitoredText = a.ConstraintName " +
                                "and c.MarketDateTime > dateadd(year, -1, convert(date, getdate())) and abs(c.shadowprice) > @ShadowPriceValue " +
                                "order by c.constraintRTNum,MarketDateTime desc, MonitoredText, c.ContingencyText";
            mSelectStartCheckedConstraintsCommandRT.Parameters.AddWithValue("@ShadowPriceValue", "ShadowPriceValue");
            mSelectStartCheckedConstraintsCommandRT.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectStartCheckedConstraintsCommandRT.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectStartCheckedConstraintsCommandRT.Connection = VayuConnection;
            //
            /*
            mSelectStartCheckedConstraintsCommandDA = new SqlCommand();
            mSelectStartCheckedConstraintsCommandDA.CommandText = "select distinct MonitoredText,c.constraintRTNum,c.ContingencyText, marketdatetime " +
                                "from pjm.ConstraintOutages a, PJM_rt_outages b, RTMasterConstraint c " +
                                "where b.StartDate >= @StartDate and b.startdate < @EndDate and (b.EndDate > @StartDate or " +
                                "b.EndDate is null) and a.driver = b.Equipment and c.MonitoredText = a.ConstraintName " +
                                "and c.MarketDateTime > dateadd(year, -1, convert(date, getdate())) and abs(c.shadowprice) > @ShadowPriceValue " +
                                "order by c.constraintRTNum,MarketDateTime desc, MonitoredText, c.ContingencyText";
            mSelectStartCheckedConstraintsCommandDA.Parameters.AddWithValue("@ShadowPriceValue", "ShadowPriceValue");
            mSelectStartCheckedConstraintsCommandDA.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectStartCheckedConstraintsCommandDA.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectStartCheckedConstraintsCommandDA.Connection = SigmaDbConn;*/


            mSelectOutageCheckedConstraintCommand = new SqlCommand();
            mSelectOutageCheckedConstraintCommand.CommandText = "select distinct MonitoredText,c.constraintRTNum,c.ContingencyText, marketdatetime " +
                                "from pjm.ConstraintOutages a, PJM_rt_outages b, RTMasterConstraint c " +
                                "where b.startdate <= @EndDate and (b.EndDate > @StartDate or " +
                                "b.EndDate is null) and a.driver = b.Equipment and c.MonitoredText = a.ConstraintName " +
                                "and c.MarketDateTime > dateadd(year, -1, convert(date, getdate())) and abs(c.shadowprice) > @ShadowPriceValue " +
                                "order by c.constraintRTNum,MarketDateTime desc, MonitoredText, c.ContingencyText";
            mSelectOutageCheckedConstraintCommand.Parameters.AddWithValue("@ShadowPriceValue", "ShadowPriceValue");
            mSelectOutageCheckedConstraintCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectOutageCheckedConstraintCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectOutageCheckedConstraintCommand.Connection = VayuConnection;
            //

            mSelectConstraintCheckedCommand = new SqlCommand();
            mSelectConstraintCheckedCommand.CommandText = "select distinct a.ConstraintRTnum from riskconstraints a join RTMasterConstraint b"
                                                           + " on a.constraintRTnum=b.constraintRTnum where a.date >=@StartDate and"
                                                         + " a.date <@EndDate and risktype='GEN' and abs(b.shadowprice)> @ShadowPriceValue";
            mSelectConstraintCheckedCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectConstraintCheckedCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectConstraintCheckedCommand.Parameters.AddWithValue("@ShadowPriceValue", "ShadowPriceValue");
            mSelectConstraintCheckedCommand.Connection = VayuConnection;
            //
            //Ercot
            mErcotSelectPathDollarsCommandRT = new SqlCommand();


            mErcotSelectPathDollarsCommandRT.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
               "((F.Sensitivity - C.Sensitivity) ), DATEPART(HOUR , G.MarketDateTime) hour,round(G.ShadowPrice,2) ,G.MarketDateTime " +
               "FROM RTMasterConstraint as A " +
               "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum " +
               "INNER JOIN ConstraintRT AS G on A.MonitoredText=G.ConstraintText and  A.ContingencyText=G.ContingencyText " +
               "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
               "WHERE G.MarketDateTime between @realTimeStartDate and @realTimeEndDate " +
               "AND C.NodeKey = @sourceKey and F.NodeKey = @sinkKey order by A.constraintRTNum,hour,G.MarketDateTime";
            mErcotSelectPathDollarsCommandRT.Parameters.AddWithValue("@realTimeStartDate", "realTimeStartDate");
            mErcotSelectPathDollarsCommandRT.Parameters.AddWithValue("@realTimeEndDate", "realTimeEndDate");
            mErcotSelectPathDollarsCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathDollarsCommandRT.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathDollarsCommandRT.Connection = VayuConnection;

            mErcotSelectPathDollarsCommandDA = new SqlCommand();
            mErcotSelectPathDollarsCommandDA.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "((F.Sensitivity - C.Sensitivity) * (round(G.ShadowPrice,2))), DATEPART(HOUR , G.MarketDateTime) hour,round(G.ShadowPrice,2), G.MarketDateTime  " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.constraintRTNum " +
                "INNER JOIN ConstraintDA AS G on A.MonitoredText=G.ConstraintText and  A.ContingencyText=G.ContingencyText " +
                "INNER JOIN DAMasterVector as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE G.MarketDateTime between @realTimeStartDate and @realTimeEndDate " +
                "AND C.NodeKey = @sourceKey and F.NodeKey = @sinkKey order by A.constraintRTNum desc";
            mErcotSelectPathDollarsCommandDA.Parameters.AddWithValue("@realTimeStartDate", "realTimeStartDate");
            mErcotSelectPathDollarsCommandDA.Parameters.AddWithValue("@realTimeEndDate", "realTimeEndDate");
            mErcotSelectPathDollarsCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathDollarsCommandDA.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathDollarsCommandDA.Connection = VayuConnection;


            //
            mSelectPathDollarsCommand = new SqlCommand();
            mSelectPathDollarsCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "(F.Sensitivity - C.Sensitivity) * (D.Impact/12), D.hour " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum " +
                "INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE D.date = CONVERT(date, @realTimeDate) " +
                "AND C.NodeKey = @sourceKey and F.NodeKey = @sinkKey order by A.constraintRTNum desc";
            mSelectPathDollarsCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectPathDollarsCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectPathDollarsCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mSelectPathDollarsCommand.Connection = VayuConnection;
            //
            //Ercot

            mErcotSelectNodalDollarsCommandRT = new SqlCommand();
            mErcotSelectNodalDollarsCommandRT.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "((C.Sensitivity) ), DATEPART(HOUR , G.MarketDateTime) hour,round(G.ShadowPrice,2) ,G.MarketDateTime " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum " +
                "INNER JOIN ConstraintRT AS G on A.MonitoredText = G.ConstraintText and A.ContingencyText = G.ContingencyText " +
                "WHERE G.MarketDateTime between @realTimeStartDate and @realTimeEndDate " +
                "AND C.NodeKey = @sourceKey order by A.constraintRTNum desc";

            mErcotSelectNodalDollarsCommandRT.Parameters.AddWithValue("@realTimeStartDate", "realTimeStartDate");
            mErcotSelectNodalDollarsCommandRT.Parameters.AddWithValue("@realTimeEndDate", "realTimeEndDate");
            mErcotSelectNodalDollarsCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalDollarsCommandRT.Connection = VayuConnection;


            mErcotSelectNodalDollarsCommandDA = new SqlCommand();
            mErcotSelectNodalDollarsCommandDA.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "((C.Sensitivity) ), DATEPART(HOUR , G.MarketDateTime) hour,round(G.ShadowPrice,2) ,G.MarketDateTime " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.constraintRTNum " +
                "INNER JOIN ConstraintDA AS G on A.MonitoredText = G.ConstraintText and A.ContingencyText = G.ContingencyText " +
                "WHERE G.MarketDateTime between @realTimeStartDate and @realTimeEndDate " +
                "AND C.NodeKey = @sourceKey order by A.constraintRTNum desc";

            mErcotSelectNodalDollarsCommandDA.Parameters.AddWithValue("@realTimeStartDate", "realTimeStartDate");
            mErcotSelectNodalDollarsCommandDA.Parameters.AddWithValue("@realTimeEndDate", "realTimeEndDate");
            mErcotSelectNodalDollarsCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalDollarsCommandDA.Connection = VayuConnection;


            //
            mSelectNodalDollarsCommand = new SqlCommand();
            mSelectNodalDollarsCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "(C.Sensitivity) * (D.Impact/12), D.hour " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum " +
                "INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum " +
                "WHERE D.date = CONVERT(date, @realTimeDate) " +
                "AND C.NodeKey = @sourceKey order by A.constraintRTNum desc";
            mSelectNodalDollarsCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectNodalDollarsCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectNodalDollarsCommand.Connection = VayuConnection;
            //
            //Ercot
            mErcotSelectPathMWsOneDateCommandRT = new SqlCommand();
            mErcotSelectPathMWsOneDateCommandRT.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) = CONVERT(date, @realTimeDate) " +
                "and C.Nodekey = @sourceKey and F.NodeKey = @sinkKey ORDER BY PathSensitivity desc";
            mErcotSelectPathMWsOneDateCommandRT.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectPathMWsOneDateCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathMWsOneDateCommandRT.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathMWsOneDateCommandRT.Connection = VayuConnection;



            mErcotSelectPathMWsOneDateCommandDA = new SqlCommand();
            mErcotSelectPathMWsOneDateCommandDA.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN ConstraintDA as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN DAMasterVector as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) = CONVERT(date, @realTimeDate) " +
                "and C.Nodekey = @sourceKey and F.NodeKey = @sinkKey ORDER BY PathSensitivity desc";
            mErcotSelectPathMWsOneDateCommandDA.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectPathMWsOneDateCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathMWsOneDateCommandDA.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathMWsOneDateCommandDA.Connection = VayuConnection;

            //
            //Ercot
            mErcotSelectNodalMWsOneDateCommandRT = new SqlCommand();
            mErcotSelectNodalMWsOneDateCommandRT.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) = CONVERT(date, @realTimeDate) " +
                "and C.Nodekey = @sourceKey ORDER BY Sensitivity desc";
            mErcotSelectNodalMWsOneDateCommandRT.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectNodalMWsOneDateCommandRT.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectNodalMWsOneDateCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalMWsOneDateCommandRT.Connection = VayuConnection;

            mErcotSelectNodalMWsOneDateCommandDA = new SqlCommand();
            mErcotSelectNodalMWsOneDateCommandDA.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN ConstraintDA as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) = CONVERT(date, @realTimeDate) " +
                "and C.Nodekey = @sourceKey ORDER BY Sensitivity desc";
            mErcotSelectNodalMWsOneDateCommandDA.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectNodalMWsOneDateCommandDA.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectNodalMWsOneDateCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalMWsOneDateCommandDA.Connection = VayuConnection;
            //
            mSelectNodalMWsOneDateCommand = new SqlCommand();
            mSelectNodalMWsOneDateCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN PJM.ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) = CONVERT(date, @realTimeDate) " +
                "and C.Nodekey = @sourceKey ORDER BY Sensitivity desc";
            mSelectNodalMWsOneDateCommand.Parameters.AddWithValue("@multiplier", "multiplier");
            mSelectNodalMWsOneDateCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectNodalMWsOneDateCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectNodalMWsOneDateCommand.Connection = VayuConnection;
            //
            mSelectPathMWsOnLookbackCommand = new SqlCommand();
            mSelectPathMWsOnLookbackCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN PJM.ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) between DATEADD(DAY, - @days, CONVERT(date, @realTimeDate)) AND CONVERT(date, @realTimeDate) " +
                "and C.NodeKey = @sourceKey and F.NodeKey = @sinkKey ORDER BY PathSensitivity desc";
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@days", "days");
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mSelectPathMWsOnLookbackCommand.Connection = VayuConnection;
            //

            mSelectPathMWsOnLookbackCommand = new SqlCommand();
            mSelectPathMWsOnLookbackCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN PJM.ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) between DATEADD(DAY, - @days, CONVERT(date, @realTimeDate)) AND CONVERT(date, @realTimeDate) " +
                "and C.NodeKey = @sourceKey and F.NodeKey = @sinkKey ORDER BY PathSensitivity desc";
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@days", "days");
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectPathMWsOnLookbackCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mSelectPathMWsOnLookbackCommand.Connection = VayuConnection;

            //
            //ERCOT


            mErcotSelectPathMWsOnLookbackCommandRT = new SqlCommand();
            mErcotSelectPathMWsOnLookbackCommandRT.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) between DATEADD(DAY, - @days, CONVERT(date, @realTimeDate)) AND CONVERT(date, @realTimeDate) " +
                "and C.NodeKey = @sourceKey ORDER BY Sensitivity desc";
            mErcotSelectPathMWsOnLookbackCommandRT.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectPathMWsOnLookbackCommandRT.Parameters.AddWithValue("@days", "days");
            mErcotSelectPathMWsOnLookbackCommandRT.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectPathMWsOnLookbackCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathMWsOnLookbackCommandRT.Connection = VayuConnection;


            mErcotSelectPathMWsOnLookbackCommandDA = new SqlCommand();
            mErcotSelectPathMWsOnLookbackCommandDA.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN ConstraintDA as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.ConstraintRTNum " +
                "WHERE CONVERT(date, B.MarketDateTime) between DATEADD(DAY, - @days, CONVERT(date, @realTimeDate)) AND CONVERT(date, @realTimeDate) " +
                "and C.NodeKey = @sourceKey ORDER BY Sensitivity desc";
            mErcotSelectPathMWsOnLookbackCommandDA.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectPathMWsOnLookbackCommandDA.Parameters.AddWithValue("@days", "days");
            mErcotSelectPathMWsOnLookbackCommandDA.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectPathMWsOnLookbackCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathMWsOnLookbackCommandDA.Connection = VayuConnection;



            //
            mSelectTodaysHourCommand = new SqlCommand();
            mSelectTodaysHourCommand.CommandText = "select MAX(datepart(HH, marketdatetime))+1 from PJM.NodeLMP where NodeKey = 30 and CONVERT(date, marketdatetime) = @today";
            mSelectTodaysHourCommand.Parameters.AddWithValue("@today", "today");
            mSelectTodaysHourCommand.Connection = VayuConnection;
            //
            //ERCOT
            mErcotSelectNoSensitivityConstraintsOneDateCommandRT = new SqlCommand();
            mErcotSelectNoSensitivityConstraintsOneDateCommandRT.CommandText = "SELECT distinct B.shiftfactor, B.ConstraintRTNum, A.ConstraintText, A.ContingencyText " +
            "FROM ConstraintRT as A " +
            "LEFT JOIN RTMasterConstraint as B " +
            "ON A.ContingencyText = B.ContingencyText and B.MonitoredText = A.ConstraintText " +
            "WHERE CONVERT(date, A.marketdatetime) = @realTimeDate " +
            "and A.ConstraintText != 'None' and B.shiftfactor is null";
            mErcotSelectNoSensitivityConstraintsOneDateCommandRT.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectNoSensitivityConstraintsOneDateCommandRT.Connection = VayuConnection;


            mErcotSelectNoSensitivityConstraintsOneDateCommandDA = new SqlCommand();
            mErcotSelectNoSensitivityConstraintsOneDateCommandDA.CommandText = "SELECT distinct B.shiftfactor, B.ConstraintRTNum, A.ConstraintText, A.ContingencyText " +
            "FROM ConstraintDA as A " +
            "LEFT JOIN DAMasterConstraint as B " +
            "ON A.ContingencyText = B.ContingencyText and B.MonitoredText = A.ConstraintText " +
            "WHERE CONVERT(date, A.marketdatetime) = @realTimeDate " +
            "and A.ConstraintText != 'None' and B.shiftfactor is null";
            mErcotSelectNoSensitivityConstraintsOneDateCommandDA.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectNoSensitivityConstraintsOneDateCommandDA.Connection = VayuConnection;


            //
            mSelectNoSensitivityConstraintsOneDateCommand = new SqlCommand();
            mSelectNoSensitivityConstraintsOneDateCommand.CommandText = "SELECT distinct B.shiftfactor, B.ConstraintRTNum, A.ConstraintText, A.ContingencyText " +
            "FROM PJM.ConstraintRT as A " +
            "LEFT JOIN RTMasterConstraint as B " +
            "ON A.ContingencyText = B.ContingencyText and B.MonitoredText = A.ConstraintText " +
            "WHERE CONVERT(date, A.marketdatetime) = @realTimeDate " +
            "and A.ConstraintText != 'None' and B.shiftfactor is null";
            mSelectNoSensitivityConstraintsOneDateCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectNoSensitivityConstraintsOneDateCommand.Connection = VayuConnection;
            //
            mSelectNoSensitivityConstraintsOnLookbackCommand = new SqlCommand();
            mSelectNoSensitivityConstraintsOnLookbackCommand.CommandText = "SELECT distinct B.shiftfactor, B.ConstraintRTNum, A.ConstraintText, A.ContingencyText " +
            "FROM PJM.ConstraintRT as A " +
            "LEFT JOIN RTMasterConstraint as B " +
            "ON A.ContingencyText = B.ContingencyText and B.MonitoredText = A.ConstraintText " +
            "WHERE CONVERT(date, A.MarketDateTime) between DATEADD(DAY, - @days, CONVERT(date, @realTimeDate)) AND CONVERT(date, @realTimeDate) " +
            "and A.ConstraintText != 'None' and B.shiftfactor is null";
            mSelectNoSensitivityConstraintsOnLookbackCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectNoSensitivityConstraintsOnLookbackCommand.Parameters.AddWithValue("@days", "days");
            mSelectNoSensitivityConstraintsOnLookbackCommand.Connection = VayuConnection;



            mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT = new SqlCommand();
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT.CommandText = "SELECT distinct B.shiftfactor, B.ConstraintRTNum, A.ConstraintText, A.ContingencyText " +
            "FROM ConstraintRT as A " +
            "LEFT JOIN RTMasterConstraint as B " +
            "ON A.ContingencyText = B.ContingencyText and B.MonitoredText = A.ConstraintText " +
            "WHERE CONVERT(date, A.MarketDateTime) between DATEADD(DAY, - @days, CONVERT(date, @realTimeDate)) AND CONVERT(date, @realTimeDate) " +
            "and A.ConstraintText != 'None' and B.shiftfactor is null";
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT.Parameters.AddWithValue("@days", "days");
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT.Connection = VayuConnection;

            mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA = new SqlCommand();
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA.CommandText = "SELECT distinct B.shiftfactor, B.ConstraintRTNum, A.ConstraintText, A.ContingencyText " +
            "FROM ConstraintDA as A " +
            "LEFT JOIN DAMasterConstraint as B " +
            "ON A.ContingencyText = B.ContingencyText and B.MonitoredText = A.ConstraintText " +
            "WHERE CONVERT(date, A.MarketDateTime) between DATEADD(DAY, - @days, CONVERT(date, @realTimeDate)) AND CONVERT(date, @realTimeDate) " +
            "and A.ConstraintText != 'None' and B.shiftfactor is null";
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA.Parameters.AddWithValue("@days", "days");
            mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA.Connection = VayuConnection;


            //
            //Ercot
            mErcotSelectPathConstraintMWExpCommandRT = new SqlCommand();
            mErcotSelectPathConstraintMWExpCommandRT.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE C.NodeKey = @sourceKey and F.NodeKey = @sinkKey and A.MonitoredText = @familyName " +
                "ORDER BY PathSensitivity desc";
            mErcotSelectPathConstraintMWExpCommandRT.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectPathConstraintMWExpCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathConstraintMWExpCommandRT.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathConstraintMWExpCommandRT.Connection = VayuConnection;

            mErcotSelectPathConstraintMWExpCommandDA = new SqlCommand();
            mErcotSelectPathConstraintMWExpCommandDA.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN ConstraintDA as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN DAMasterVector as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE C.NodeKey = @sourceKey and F.NodeKey = @sinkKey and A.MonitoredText = @familyName " +
                "ORDER BY PathSensitivity desc";
            mErcotSelectPathConstraintMWExpCommandDA.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectPathConstraintMWExpCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathConstraintMWExpCommandDA.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathConstraintMWExpCommandDA.Connection = VayuConnection;


            //
            mSelectPathConstraintMWExpCommand = new SqlCommand();
            mSelectPathConstraintMWExpCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN PJM.ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +

                "WHERE C.NodeKey = @sourceKey and F.NodeKey = @sinkKey and A.MonitoredText = @familyName " +
                "ORDER BY PathSensitivity desc";
            mSelectPathConstraintMWExpCommand.Parameters.AddWithValue("@familyName", "familyName");
            mSelectPathConstraintMWExpCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectPathConstraintMWExpCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mSelectPathConstraintMWExpCommand.Connection = VayuConnection;
            //
            //Ercot
            mErcotSelectNodalConstraintMWExpCommandRT = new SqlCommand();
            mErcotSelectNodalConstraintMWExpCommandRT.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +

                "WHERE C.NodeKey = @sourceKey and A.MonitoredText = @familyName " +
                "ORDER BY Sensitivity desc";
            mErcotSelectNodalConstraintMWExpCommandRT.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectNodalConstraintMWExpCommandRT.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectNodalConstraintMWExpCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalConstraintMWExpCommandRT.Connection = VayuConnection;


            mErcotSelectNodalConstraintMWExpCommandDA = new SqlCommand();
            mErcotSelectNodalConstraintMWExpCommandDA.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN ConstraintDA as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.ConstraintRTNum " +
                "WHERE C.NodeKey = @sourceKey and A.MonitoredText = @familyName " +
                "ORDER BY Sensitivity desc";
            mErcotSelectNodalConstraintMWExpCommandDA.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectNodalConstraintMWExpCommandDA.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectNodalConstraintMWExpCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalConstraintMWExpCommandDA.Connection = VayuConnection;
            //
            mSelectNodalConstraintMWExpCommand = new SqlCommand();
            mSelectNodalConstraintMWExpCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN PJM.ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                 "WHERE C.NodeKey = @sourceKey and A.MonitoredText = @familyName " +
                "ORDER BY Sensitivity desc";
            mSelectNodalConstraintMWExpCommand.Parameters.AddWithValue("@multiplier", "multiplier");
            mSelectNodalConstraintMWExpCommand.Parameters.AddWithValue("@familyName", "familyName");
            mSelectNodalConstraintMWExpCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectNodalConstraintMWExpCommand.Connection = VayuConnection;
            //
            //Ercot

            mErcotSelectPathConstraintMWExpBestCommandRT = new SqlCommand();
            mErcotSelectPathConstraintMWExpBestCommandRT.CommandText = "SELECT top 1 A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE C.NodeKey = @sourceKey and F.NodeKey = @sinkKey and A.MonitoredText = @familyName " +
                "ORDER BY A.score, A.MarketDateTime desc";
            mErcotSelectPathConstraintMWExpBestCommandRT.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectPathConstraintMWExpBestCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathConstraintMWExpBestCommandRT.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathConstraintMWExpBestCommandRT.Connection = VayuConnection;




            mErcotSelectPathConstraintMWExpBestCommandDA = new SqlCommand();
            mErcotSelectPathConstraintMWExpBestCommandDA.CommandText = "SELECT top 1 A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN ConstraintDA as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN DAMasterVector as F on A.constraintRTNum = F.ConstraintRTNum " +
                "WHERE C.NodeKey = @sourceKey and F.NodeKey = @sinkKey and A.MonitoredText = @familyName " +
                "ORDER BY A.score, A.MarketDateTime desc";
            mErcotSelectPathConstraintMWExpBestCommandDA.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectPathConstraintMWExpBestCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathConstraintMWExpBestCommandDA.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathConstraintMWExpBestCommandDA.Connection = VayuConnection;
            //
            mSelectPathConstraintMWExpBestCommand = new SqlCommand();
            mSelectPathConstraintMWExpBestCommand.CommandText = "SELECT top 1 A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "F.Sensitivity - C.Sensitivity as PathSensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN PJM.ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTMasterVector_new as F on A.constraintRTNum = F.ConstraintRTNum " +
                "INNER JOIN RTFamily as G on A.ConstraintRTNum = G.ConstraintRTNum " +
                "WHERE C.NodeKey = @sourceKey and F.NodeKey = @sinkKey and G.MonitoredText = @familyName " +
                "ORDER BY A.score, A.MarketDateTime desc";
            mSelectPathConstraintMWExpBestCommand.Parameters.AddWithValue("@familyName", "familyName");
            mSelectPathConstraintMWExpBestCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectPathConstraintMWExpBestCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mSelectPathConstraintMWExpBestCommand.Connection = VayuConnection;
            //
            //Ercot
            mErcotSelectNodalConstraintMWExpBestCommandRT = new SqlCommand();
            mErcotSelectNodalConstraintMWExpBestCommandRT.CommandText = "SELECT top 1 A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +

                "WHERE C.NodeKey = @sourceKey and A.MonitoredText = @familyName " +
                "ORDER BY A.score, A.MarketDateTime desc";
            mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalConstraintMWExpBestCommandRT.Connection = VayuConnection;


            mErcotSelectNodalConstraintMWExpBestCommandDA = new SqlCommand();
            mErcotSelectNodalConstraintMWExpBestCommandDA.CommandText = "SELECT top 1 A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM DAMasterConstraint as A " +
                "INNER JOIN ConstraintDA as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN DAMasterVector as C on A.constraintRTNum = C.ConstraintRTNum " +

                "WHERE C.NodeKey = @sourceKey and A.MonitoredText = @familyName " +
                "ORDER BY A.score, A.MarketDateTime desc";
            mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters.AddWithValue("@multiplier", "multiplier");
            mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters.AddWithValue("@familyName", "familyName");
            mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalConstraintMWExpBestCommandDA.Connection = VayuConnection;


            //
            mSelectNodalConstraintMWExpBestCommand = new SqlCommand();
            mSelectNodalConstraintMWExpBestCommand.CommandText = "SELECT top 1 A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                "C.Sensitivity*@multiplier as Sensitivity " +
                "FROM RTMasterConstraint as A " +
                "INNER JOIN PJM.ConstraintRT as B on A.ContingencyText = B.ContingencyText and A.MonitoredText = B.ConstraintText " +
                "INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.ConstraintRTNum " +
                "INNER JOIN RTFamily as G on A.ConstraintRTNum = G.ConstraintRTNum " +
                "WHERE C.NodeKey = @sourceKey and G.MonitoredText = @familyName " +
                "ORDER BY A.score, A.MarketDateTime desc";
            mSelectNodalConstraintMWExpBestCommand.Parameters.AddWithValue("@multiplier", "multiplier");
            mSelectNodalConstraintMWExpBestCommand.Parameters.AddWithValue("@familyName", "familyName");
            mSelectNodalConstraintMWExpBestCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectNodalConstraintMWExpBestCommand.Connection = VayuConnection;
            //
            mSelectMostRecentConstraintScrapeCommand = new SqlCommand();
            mSelectMostRecentConstraintScrapeCommand.CommandText = "select max(datepart(HH, MarketDateTime)) from PJM.ConstraintRT " +
            "WHERE convert(date, MarketDateTime) = @realTimeDate and constraintText!= 'None'";
            mSelectMostRecentConstraintScrapeCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectMostRecentConstraintScrapeCommand.Connection = VayuConnection;

            //
            //Ercot
            mErcotSelectPathOnStartCheckedCommand = new SqlCommand();
            mErcotSelectPathOnStartCheckedCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, (F.Sensitivity - C.Sensitivity) * (D.Impact/12),D.hour"
                                                           + " FROM RTMasterConstraint as A INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum"
                                                           + " INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum INNER JOIN RTMasterVector_new as F on"
                                                            + " A.constraintRTNum = F.ConstraintRTNum WHERE a.constraintRTNum=@ConstraintRTNum AND C.NodeKey =  @sourceKey"
                                                          + " and F.NodeKey = @sinkKey";
            mErcotSelectPathOnStartCheckedCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectPathOnStartCheckedCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mErcotSelectPathOnStartCheckedCommand.Parameters.AddWithValue("@ConstraintRTNum", "ConstraintRTNum");
            mErcotSelectPathOnStartCheckedCommand.Connection = VayuConnection;
            //
            mSelectPathOnStartCheckedCommand = new SqlCommand();
            mSelectPathOnStartCheckedCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, (F.Sensitivity - C.Sensitivity) * (D.Impact/12),D.hour"
                                                           + " FROM RTMasterConstraint as A INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum"
                                                           + " INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum INNER JOIN RTMasterVector_new as F on"
                                                            + " A.constraintRTNum = F.ConstraintRTNum WHERE a.constraintRTNum=@ConstraintRTNum AND C.NodeKey =  @sourceKey"
                                                          + " and F.NodeKey = @sinkKey";
            mSelectPathOnStartCheckedCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectPathOnStartCheckedCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mSelectPathOnStartCheckedCommand.Parameters.AddWithValue("@ConstraintRTNum", "ConstraintRTNum");
            mSelectPathOnStartCheckedCommand.Connection = VayuConnection;

            //
            mSelectNodalOnStartCheckedCommand = new SqlCommand();
            mSelectNodalOnStartCheckedCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText,"
                                                           + "(C.Sensitivity) * (D.Impact/12), D.hour FROM RTMasterConstraint as A"
                                                            + " INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum"
                                                          + " INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum"
                                                           + " WHERE a.constraintRTNum=@ConstraintRTNum AND C.NodeKey = @sourceKey";
            mSelectNodalOnStartCheckedCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectNodalOnStartCheckedCommand.Parameters.AddWithValue("@ConstraintRTNum", "ConstraintRTNum");
            mSelectNodalOnStartCheckedCommand.Connection = VayuConnection;
            //
            //Ercot
            mErcotSelectNodalOnStartCheckedCommand = new SqlCommand();
            mErcotSelectNodalOnStartCheckedCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText,"
                                                           + "(C.Sensitivity) * (D.Impact/12), D.hour FROM RTMasterConstraint as A"
                                                            + " INNER JOIN RTMasterVector_new as C on A.constraintRTNum = C.constraintRTNum"
                                                          + " INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum"
                                                           + " WHERE a.constraintRTNum=@ConstraintRTNum AND C.NodeKey = @sourceKey";
            mErcotSelectNodalOnStartCheckedCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mErcotSelectNodalOnStartCheckedCommand.Parameters.AddWithValue("@ConstraintRTNum", "ConstraintRTNum");
            mErcotSelectNodalOnStartCheckedCommand.Connection = VayuConnection;
            //

        }

        /// <summary>
        /// Fills the pnode hash.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<string>> FillPnodeHash()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            Dictionary<int, List<string>> tempPnodeHash = new Dictionary<int, List<string>>();
            // List<string> NodeNameList = new List<string>();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "select ExternalNodeID , NodeName from Node where ExternalNodeID in (" +
                                "select ExternalNodeID   from Node where MarketKey = 1 group by externalnodeid   having  COUNT(ExternalNodeID) >1 " +
                                ")";
            cmd.Connection = VayuConnection;
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                int externalNodeId = Convert.ToInt32(rdr.GetValue(0));
                string nodeName = rdr.GetValue(1).ToString();
                if (!tempPnodeHash.ContainsKey(externalNodeId))
                {
                    List<string> NodeNameList = new List<string>();
                    NodeNameList.Add(nodeName);
                    tempPnodeHash.Add(externalNodeId, NodeNameList);
                }
                else
                {
                    List<string> NodeNameList = tempPnodeHash[externalNodeId];
                    NodeNameList.Add(nodeName);
                }
            }
            rdr.Close();
            VayuConnection.Close();
            return tempPnodeHash;
        }

        /// <summary>
        /// Gets constraint list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="uptosChecked">if set to <c>true</c> [uptos checked].</param>
        /// <param name="virtualsChecked">if set to <c>true</c> [virtuals checked].</param>
        /// <param name="nodalChecked">if set to <c>true</c> [nodal checked].</param>
        /// <param name="pathChecked">if set to <c>true</c> [path checked].</param>
        /// <param name="incChecked">if set to <c>true</c> [inc checked].</param>
        /// <param name="decChecked">if set to <c>true</c> [decimal checked].</param>
        /// <param name="lookBackChecked">if set to <c>true</c> [look back checked].</param>
        /// <param name="sinkComboSelectedItem">The sink combo selected item.</param>
        /// <param name="sourceComboSelectedItem">The source combo selected item.</param>
        /// <param name="mwChecked">if set to <c>true</c> [mw checked].</param>
        /// <param name="dollarsChecked">if set to <c>true</c> [dollars checked].</param>
        /// <param name="daysVal">The days value.</param>
        /// <param name="daSettleSelected">The da settle selected.</param>
        /// <param name="rtSettleSelected">The rt settle selected.</param>
        /// <param name="familyChecked">if set to <c>true</c> [family checked].</param>
        /// <param name="selectedFamily">The selected family.</param>
        /// <param name="bestChecked">if set to <c>true</c> [best checked].</param>
        /// <param name="startChecked">if set to <c>true</c> [start checked].</param>
        /// <param name="OutageChecked">if set to <c>true</c> [outage checked].</param>
        /// <param name="ConstraintChecked">if set to <c>true</c> [constraint checked].</param>
        /// <param name="ShadowPriceValue">The shadow price value.</param>
        /// <param name="constraintsList">The constraints list.</param>
        public void GetConstraints(Action<List<Model.Constraints>, Exception> callback, bool uptosChecked,
            bool virtualsChecked, bool nodalChecked, bool pathChecked, bool incChecked, bool decChecked,
            bool lookBackChecked, PricingNode sinkComboSelectedItem, PricingNode sourceComboSelectedItem, bool mwChecked,
            bool dollarsChecked, string daysVal, DateTime daSettleSelected, DateTime rtSettleSelected,
            bool familyChecked, string selectedFamily, bool bestChecked, bool startChecked, bool OutageChecked, bool ConstraintChecked, string ShadowPriceValue, List<Model.Constraints> constraintsList, int marketKey, string RTORDA)
        {
            DateTime DateToPassDollar = DateTime.Today;
            if (sourceComboSelectedItem == null)
            {
                return;
            }
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            SqlDataReader reader = null;
            SqlDataReader reader1 = null;

            //ERCOT 
            if (marketKey == 9)
            {
                if ((lookBackChecked && mwChecked && uptosChecked) || (lookBackChecked && mwChecked && pathChecked))
                {
                    if (RTORDA == "RT")
                    {
                        if (incChecked)
                        {

                            mErcotSelectPathMWsOnLookbackCommandRT.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectPathMWsOnLookbackCommandRT.Parameters["@multiplier"].Value = 1;
                        }
                        else
                        {
                            mErcotSelectPathMWsOnLookbackCommandRT.Parameters["@multiplier"].Value = -1;
                        }



                        mErcotSelectPathMWsOnLookbackCommandRT.Parameters["@days"].Value = (daysVal.ToString() == "") ? 0 : Convert.ToInt32(daysVal.ToString());
                        mErcotSelectPathMWsOnLookbackCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                        mErcotSelectPathMWsOnLookbackCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        //  mErcotSelectPathMWsOnLookbackCommandRT.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;

                        if (mErcotSelectPathMWsOnLookbackCommandRT.Connection.State != ConnectionState.Open)
                        {
                            mErcotSelectPathMWsOnLookbackCommandRT.Connection.Open();
                        }
                        reader = mErcotSelectPathMWsOnLookbackCommandRT.ExecuteReader();

                        mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                        mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT.Parameters["@days"].Value = (daysVal.ToString() == "") ? 0 : Convert.ToInt32(daysVal.ToString());
                        reader1 = mErcotSelectNoSensitivityConstraintsOnLookbackCommandRT.ExecuteReader();
                    }
                    if (RTORDA == "DA")
                    {

                        if (incChecked)
                        {
                            mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@multiplier"].Value = 1;
                        }
                        else
                        {
                            mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@multiplier"].Value = -1;
                        }
                        // mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@multiplier"].Value = 1;
                        mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@days"].Value = (daysVal.ToString() == "") ? 0 : Convert.ToInt32(daysVal.ToString());
                        mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@realTimeDate"].Value = daSettleSelected.Date;
                        mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        // mErcotSelectPathMWsOnLookbackCommandDA.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                        reader = mErcotSelectPathMWsOnLookbackCommandDA.ExecuteReader();

                        mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA.Parameters["@realTimeDate"].Value = daSettleSelected.Date;
                        mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA.Parameters["@days"].Value = (daysVal.ToString() == "") ? 0 : Convert.ToInt32(daysVal.ToString());
                        reader1 = mErcotSelectNoSensitivityConstraintsOnLookbackCommandDA.ExecuteReader();
                    }
                }
                if ((!familyChecked && !lookBackChecked && mwChecked && uptosChecked) || (!familyChecked && !lookBackChecked && mwChecked && pathChecked))
                {//mwcheck
                    if (RTORDA == "RT")
                    {
                        mErcotSelectPathMWsOneDateCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                        mErcotSelectPathMWsOneDateCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        mErcotSelectPathMWsOneDateCommandRT.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                        if (mErcotSelectPathMWsOneDateCommandRT.Connection.State != ConnectionState.Open)
                        {
                            mErcotSelectPathMWsOneDateCommandRT.Connection.Open();
                        }
                        reader = mErcotSelectPathMWsOneDateCommandRT.ExecuteReader();
                        mErcotSelectNoSensitivityConstraintsOneDateCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                        reader1 = mErcotSelectNoSensitivityConstraintsOneDateCommandRT.ExecuteReader();
                    }
                    if (RTORDA == "DA")
                    {
                        mErcotSelectPathMWsOneDateCommandDA.Parameters["@realTimeDate"].Value = daSettleSelected.Date;
                        mErcotSelectPathMWsOneDateCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        mErcotSelectPathMWsOneDateCommandDA.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                        if (mErcotSelectPathMWsOneDateCommandDA.Connection.State != ConnectionState.Open)
                        {
                            mErcotSelectPathMWsOneDateCommandDA.Connection.Open();
                        }
                        reader = mErcotSelectPathMWsOneDateCommandDA.ExecuteReader();
                        mErcotSelectNoSensitivityConstraintsOneDateCommandDA.Parameters["@realTimeDate"].Value = daSettleSelected.Date;
                        reader1 = mErcotSelectNoSensitivityConstraintsOneDateCommandDA.ExecuteReader();


                    }
                }
                if (!familyChecked && !lookBackChecked && mwChecked && nodalChecked)//remaining
                {
                    if (RTORDA == "RT")
                    {
                        if (incChecked)
                        {
                            mErcotSelectNodalMWsOneDateCommandRT.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectNodalMWsOneDateCommandRT.Parameters["@multiplier"].Value = 1;
                        }
                        else
                        {
                            mErcotSelectNodalMWsOneDateCommandRT.Parameters["@multiplier"].Value = -1;
                        }
                        mErcotSelectNodalMWsOneDateCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                        mErcotSelectNodalMWsOneDateCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        reader = mErcotSelectNodalMWsOneDateCommandRT.ExecuteReader();

                        mErcotSelectNoSensitivityConstraintsOneDateCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                        reader1 = mErcotSelectNoSensitivityConstraintsOneDateCommandRT.ExecuteReader();//tochange
                    }

                    if (RTORDA == "DA")
                    {
                        if (incChecked)
                        {
                            mErcotSelectNodalMWsOneDateCommandDA.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectNodalMWsOneDateCommandDA.Parameters["@multiplier"].Value = 1;
                        }
                        else
                        {
                            mErcotSelectNodalMWsOneDateCommandDA.Parameters["@multiplier"].Value = -1;
                        }
                        mErcotSelectNodalMWsOneDateCommandDA.Parameters["@realTimeDate"].Value = daSettleSelected.Date;
                        mErcotSelectNodalMWsOneDateCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        reader = mErcotSelectNodalMWsOneDateCommandDA.ExecuteReader();
                        mErcotSelectNoSensitivityConstraintsOneDateCommandDA.Parameters["@realTimeDate"].Value = daSettleSelected.Date;
                        reader1 = mErcotSelectNoSensitivityConstraintsOneDateCommandDA.ExecuteReader();//tochange}

                    }
                }
                if (!familyChecked && lookBackChecked && mwChecked && nodalChecked)//remaining
                {
                    if (incChecked)
                    {
                        mErcotSelectNodalMWsOnLookbackCommand.Parameters["@multiplier"].Value = -1;
                    }
                    else if (decChecked)
                    {
                        mErcotSelectNodalMWsOnLookbackCommand.Parameters["@multiplier"].Value = 1;
                    }
                    mErcotSelectNodalMWsOnLookbackCommand.Parameters["@days"].Value = (daysVal.ToString() == "") ? 0 : Convert.ToInt32(daysVal.ToString());
                    mErcotSelectNodalMWsOnLookbackCommand.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                    mErcotSelectNodalMWsOnLookbackCommand.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                    if (mErcotSelectNodalMWsOnLookbackCommand.Connection.State != ConnectionState.Open)
                    {
                        mErcotSelectNodalMWsOnLookbackCommand.Connection.Open();
                    }
                    reader = mErcotSelectNodalMWsOnLookbackCommand.ExecuteReader();
                    mErcotSelectNoSensitivityConstraintsOnLookbackCommand.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                    mErcotSelectNoSensitivityConstraintsOnLookbackCommand.Parameters["@days"].Value = (daysVal.ToString() == "") ? 0 : Convert.ToInt32(daysVal.ToString());
                    reader1 = mErcotSelectNoSensitivityConstraintsOnLookbackCommand.ExecuteReader();
                }
                if ((familyChecked && mwChecked && uptosChecked && !bestChecked) || (familyChecked && mwChecked && pathChecked && !bestChecked))
                {
                    //mErcotSelectPathConstraintMWExpCommand.Parameters["@familyName"].Value = selectedFamily.ToString();
                    //mErcotSelectPathConstraintMWExpCommand.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                    //mErcotSelectPathConstraintMWExpCommand.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                    //reader = mErcotSelectPathConstraintMWExpCommand.ExecuteReader();

                    if (RTORDA == "RT")
                    {
                        mErcotSelectPathConstraintMWExpCommandRT.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectPathConstraintMWExpCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        mErcotSelectPathConstraintMWExpCommandRT.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                        reader = mErcotSelectPathConstraintMWExpCommandRT.ExecuteReader();

                    }
                    if (RTORDA == "DA")
                    {
                        mErcotSelectPathConstraintMWExpCommandDA.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectPathConstraintMWExpCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        mErcotSelectPathConstraintMWExpCommandDA.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                        reader = mErcotSelectPathConstraintMWExpCommandDA.ExecuteReader();

                    }
                }
                if (familyChecked && mwChecked && nodalChecked && !bestChecked)
                {
                    //if (incChecked)
                    //{
                    //    mErcotSelectNodalConstraintMWExpCommand.Parameters["@multiplier"].Value = -1;
                    //}
                    //else if (decChecked)
                    //{
                    //    mErcotSelectNodalConstraintMWExpCommand.Parameters["@multiplier"].Value = 1;
                    //}
                    //mErcotSelectNodalConstraintMWExpCommand.Parameters["@familyName"].Value = selectedFamily.ToString();
                    //mErcotSelectNodalConstraintMWExpCommand.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                    //reader = mErcotSelectNodalConstraintMWExpCommand.ExecuteReader();

                    if (RTORDA == "RT")
                    {
                        if (incChecked)
                        {
                            mErcotSelectNodalConstraintMWExpCommandRT.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectNodalConstraintMWExpCommandRT.Parameters["@multiplier"].Value = 1;
                        }
                        mErcotSelectNodalConstraintMWExpCommandRT.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectNodalConstraintMWExpCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        reader = mErcotSelectNodalConstraintMWExpCommandRT.ExecuteReader();
                    }

                    if (RTORDA == "DA")
                    {
                        if (incChecked)
                        {
                            mErcotSelectNodalConstraintMWExpCommandDA.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectNodalConstraintMWExpCommandDA.Parameters["@multiplier"].Value = 1;
                        }
                        mErcotSelectNodalConstraintMWExpCommandDA.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectNodalConstraintMWExpCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        reader = mErcotSelectNodalConstraintMWExpCommandDA.ExecuteReader();
                    }


                }
                if ((familyChecked && mwChecked && uptosChecked && bestChecked) || (familyChecked && mwChecked && pathChecked && bestChecked))
                {
                    //mErcotSelectPathConstraintMWExpBestCommandDA.Parameters["@familyName"].Value = selectedFamily.ToString();
                    //mErcotSelectPathConstraintMWExpBestCommand.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                    //mErcotSelectPathConstraintMWExpBestCommand.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                    //reader = mErcotSelectPathConstraintMWExpBestCommand.ExecuteReader();



                    if (RTORDA == "RT")
                    {
                        mErcotSelectPathConstraintMWExpBestCommandRT.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectPathConstraintMWExpBestCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        mErcotSelectPathConstraintMWExpBestCommandRT.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                        reader = mErcotSelectPathConstraintMWExpBestCommandRT.ExecuteReader();
                    }

                    if (RTORDA == "DA")
                    {
                        mErcotSelectPathConstraintMWExpBestCommandDA.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectPathConstraintMWExpBestCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        mErcotSelectPathConstraintMWExpBestCommandDA.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                        reader = mErcotSelectPathConstraintMWExpBestCommandDA.ExecuteReader();
                    }
                }
                if (familyChecked && mwChecked && nodalChecked && bestChecked)
                {
                    //if (incChecked)
                    //{
                    //    mErcotSelectNodalConstraintMWExpBestCommand.Parameters["@multiplier"].Value = -1;
                    //}
                    //else if (decChecked)
                    //{
                    //    mErcotSelectNodalConstraintMWExpBestCommand.Parameters["@multiplier"].Value = 1;
                    //}
                    //else
                    //{
                    //    mErcotSelectNodalConstraintMWExpBestCommand.Parameters["@multiplier"].Value = -1;
                    //}
                    //mErcotSelectNodalConstraintMWExpBestCommand.Parameters["@familyName"].Value = selectedFamily.ToString();
                    //mErcotSelectNodalConstraintMWExpBestCommand.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                    //reader = mErcotSelectNodalConstraintMWExpBestCommand.ExecuteReader();


                    if (RTORDA == "RT")
                    {
                        if (incChecked)
                        {
                            mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters["@multiplier"].Value = 1;
                        }
                        else
                        {
                            mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters["@multiplier"].Value = -1;
                        }
                        mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectNodalConstraintMWExpBestCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        reader = mErcotSelectNodalConstraintMWExpBestCommandRT.ExecuteReader();

                    }

                    if (RTORDA == "DA")
                    {

                        if (incChecked)
                        {
                            mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters["@multiplier"].Value = -1;
                        }
                        else if (decChecked)
                        {
                            mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters["@multiplier"].Value = 1;
                        }
                        else
                        {
                            mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters["@multiplier"].Value = -1;
                        }
                        mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters["@familyName"].Value = selectedFamily.ToString();
                        mErcotSelectNodalConstraintMWExpBestCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                        reader = mErcotSelectNodalConstraintMWExpBestCommandDA.ExecuteReader();

                    }
                }
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        if (incChecked)
                        {
                            Model.Constraints constraint = new Model.Constraints();
                            constraint.constraintNum = Convert.ToInt32(reader.GetValue(1));
                            constraint.monitoredName = GetConstraintName(reader.GetValue(2).ToString());
                            constraint.contingName = GetContingencyName(reader.GetValue(3).ToString());
                            constraint.he1Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he2Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he3Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he4Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he5Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he6Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he7Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he8Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he9Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he10Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he11Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he12Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he13Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he14Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he15Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he16Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he17Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he18Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he19Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he20Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he21Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he22Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he23Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he24Value = -1 * Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.shiftFactor = -1 * Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);
                            constraintsList.Add(constraint);
                        }
                        else
                        {
                            Model.Constraints constraint = new Model.Constraints();
                            constraint.constraintNum = Convert.ToInt32(reader.GetValue(1));
                            constraint.monitoredName = GetConstraintName(reader.GetValue(2).ToString());
                            constraint.contingName = GetContingencyName(reader.GetValue(3).ToString());
                            constraint.he1Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he2Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he3Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he4Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he5Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he6Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he7Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he8Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he9Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he10Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he11Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he12Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he13Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he14Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he15Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he16Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he17Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he18Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he19Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he20Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he21Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he22Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he23Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.he24Value = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                            constraint.shiftFactor = Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);
                            constraintsList.Add(constraint);
                        }
                    }
                    reader.Close();
                }
                if (reader1 != null)
                {
                    while (reader1.Read())
                    {
                        if (!reader1.IsDBNull(0))
                        {
                            Model.Constraints missing = new Model.Constraints();
                            missing.constraintNum = 0;
                            missing.monitoredName = GetConstraintName(reader1.GetValue(2).ToString());
                            missing.contingName = GetContingencyName(reader1.GetValue(3).ToString());
                            constraintsList.Add(missing);
                        }
                    }
                    reader1.Close();
                }
                //DOLLARS below
                if ((dollarsChecked && uptosChecked) || (dollarsChecked && pathChecked) || (dollarsChecked && nodalChecked))
                {
                    List<OutageConstraint> outageConstraintList = new List<OutageConstraint>();
                    Dictionary<int, Model.Constraints> constraintHash = new Dictionary<int, Model.Constraints>();
                    if (startChecked || OutageChecked || ConstraintChecked)
                    {
                        outageConstraintList = GetOutageConstraintList(startChecked, OutageChecked, ConstraintChecked, ShadowPriceValue, rtSettleSelected, RTORDA, marketKey);
                        var constraintrtnumlist = outageConstraintList.GroupBy(x => x.constraintRTNum).ToList();
                        foreach (OutageConstraint cons in outageConstraintList)
                        {
                            if (dollarsChecked && nodalChecked)
                            {
                                mErcotSelectNodalOnStartCheckedCommand.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                                mErcotSelectNodalOnStartCheckedCommand.Parameters["@constraintRTNum"].Value = cons.constraintRTNum;
                                reader = mErcotSelectNodalOnStartCheckedCommand.ExecuteReader();
                            }
                            else
                            {
                                mErcotSelectPathOnStartCheckedCommand.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                                mErcotSelectPathOnStartCheckedCommand.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                                mErcotSelectPathOnStartCheckedCommand.Parameters["@constraintRTNum"].Value = cons.constraintRTNum;
                                reader = mErcotSelectPathOnStartCheckedCommand.ExecuteReader();
                            }

                            FillConstraintHashERCOT(reader, constraintHash, incChecked, rtSettleSelected.Date, RTORDA);

                        }

                    }
                    //List<ViewModel.Constraints> roughList = new List<ViewModel.Constraints>();
                    else
                    {
                        if (dollarsChecked && nodalChecked)
                        {


                            if (RTORDA == "RT")
                            {
                                DateToPassDollar = rtSettleSelected.Date;
                                mErcotSelectNodalDollarsCommandRT.Parameters["@realTimeStartDate"].Value = rtSettleSelected.AddMinutes(-5);
                                mErcotSelectNodalDollarsCommandRT.Parameters["@realTimeEndDate"].Value = rtSettleSelected.AddHours(24);
                                mErcotSelectNodalDollarsCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                                reader = mErcotSelectNodalDollarsCommandRT.ExecuteReader();

                            }

                            if (RTORDA == "DA")
                            {
                                DateToPassDollar = daSettleSelected.Date;
                                mErcotSelectNodalDollarsCommandDA.Parameters["@realTimeStartDate"].Value = daSettleSelected.AddHours(1);
                                mErcotSelectNodalDollarsCommandDA.Parameters["@realTimeEndDate"].Value = daSettleSelected.AddHours(24);
                                mErcotSelectNodalDollarsCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                                reader = mErcotSelectNodalDollarsCommandDA.ExecuteReader();

                            }



                        }
                        else
                        {
                            if (RTORDA == "RT")
                            {
                                DateToPassDollar = rtSettleSelected.Date;
                                mErcotSelectPathDollarsCommandRT.Parameters["@realTimeStartDate"].Value = rtSettleSelected.AddMinutes(-5);
                                mErcotSelectPathDollarsCommandRT.Parameters["@realTimeEndDate"].Value = rtSettleSelected.AddHours(24);

                                //  mErcotSelectPathDollarsCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                                mErcotSelectPathDollarsCommandRT.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                                mErcotSelectPathDollarsCommandRT.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                                reader = mErcotSelectPathDollarsCommandRT.ExecuteReader();
                            }

                            if (RTORDA == "DA")
                            {

                                DateToPassDollar = daSettleSelected.Date;
                                mErcotSelectPathDollarsCommandDA.Parameters["@realTimeStartDate"].Value = daSettleSelected.AddHours(1);
                                mErcotSelectPathDollarsCommandDA.Parameters["@realTimeEndDate"].Value = daSettleSelected.AddHours(24);
                                mErcotSelectPathDollarsCommandDA.Parameters["@sourceKey"].Value = sourceComboSelectedItem.NodeKey;
                                mErcotSelectPathDollarsCommandDA.Parameters["@sinkKey"].Value = sinkComboSelectedItem.NodeKey;
                                reader = mErcotSelectPathDollarsCommandDA.ExecuteReader();
                            }
                        }
                        FillConstraintHashERCOT(reader, constraintHash, incChecked, DateToPassDollar, RTORDA);
                    }
                    constraintsList = constraintHash.Values.ToList<Model.Constraints>();
                    Model.Constraints sums = new Model.Constraints();
                    sums.summaryConstraintNum = -1;
                    sums.monitoredName = "";
                    sums.contingName = "Calculated";
                    foreach (Model.Constraints item in constraintsList)
                    {
                        sums.he1Value = Convert.ToDouble(sums.he1Value) + Convert.ToDouble(item.he1Value);
                        sums.he2Value = Convert.ToDouble(sums.he2Value) + Convert.ToDouble(item.he2Value);
                        sums.he3Value = Convert.ToDouble(sums.he3Value) + Convert.ToDouble(item.he3Value);
                        sums.he4Value = Convert.ToDouble(sums.he4Value) + Convert.ToDouble(item.he4Value);
                        sums.he5Value = Convert.ToDouble(sums.he5Value) + Convert.ToDouble(item.he5Value);
                        sums.he6Value = Convert.ToDouble(sums.he6Value) + Convert.ToDouble(item.he6Value);
                        sums.he7Value = Convert.ToDouble(sums.he7Value) + Convert.ToDouble(item.he7Value);
                        sums.he8Value = Convert.ToDouble(sums.he8Value) + Convert.ToDouble(item.he8Value);
                        sums.he9Value = Convert.ToDouble(sums.he9Value) + Convert.ToDouble(item.he9Value);
                        sums.he10Value = Convert.ToDouble(sums.he10Value) + Convert.ToDouble(item.he10Value);
                        sums.he11Value = Convert.ToDouble(sums.he11Value) + Convert.ToDouble(item.he11Value);
                        sums.he12Value = Convert.ToDouble(sums.he12Value) + Convert.ToDouble(item.he12Value);
                        sums.he13Value = Convert.ToDouble(sums.he13Value) + Convert.ToDouble(item.he13Value);
                        sums.he14Value = Convert.ToDouble(sums.he14Value) + Convert.ToDouble(item.he14Value);
                        sums.he15Value = Convert.ToDouble(sums.he15Value) + Convert.ToDouble(item.he15Value);
                        sums.he16Value = Convert.ToDouble(sums.he16Value) + Convert.ToDouble(item.he16Value);
                        sums.he17Value = Convert.ToDouble(sums.he17Value) + Convert.ToDouble(item.he17Value);
                        sums.he18Value = Convert.ToDouble(sums.he18Value) + Convert.ToDouble(item.he18Value);
                        sums.he19Value = Convert.ToDouble(sums.he19Value) + Convert.ToDouble(item.he19Value);
                        sums.he20Value = Convert.ToDouble(sums.he20Value) + Convert.ToDouble(item.he20Value);
                        sums.he21Value = Convert.ToDouble(sums.he21Value) + Convert.ToDouble(item.he21Value);
                        sums.he22Value = Convert.ToDouble(sums.he22Value) + Convert.ToDouble(item.he22Value);
                        sums.he23Value = Convert.ToDouble(sums.he23Value) + Convert.ToDouble(item.he23Value);
                        sums.he24Value = Convert.ToDouble(sums.he24Value) + Convert.ToDouble(item.he24Value);

                    }
                    sums.shiftFactor = null;
                    constraintsList.Add(sums);
                    int hourCount = 24;


                    if (RTORDA == "RT")
                    {
                        if (rtSettleSelected.Date == DateTime.Today)
                        {
                            DateTime dt = GetMaxLMPDateTime(marketKey);
                            hourCount = dt.Hour;
                        }
                    }

                    if (!startChecked && !OutageChecked && !ConstraintChecked)
                    {
                        /* ViewModel.Constraints actuals = GetCongestionRow(marketKey,rtSettleSelected.Date, sourceComboSelectedItem.NodeKey, (virtualsChecked ? 0 : sinkComboSelectedItem.NodeKey), hourCount, (dollarsChecked && nodalChecked), CongestionTypes.RTCongestion, incChecked);
                         ViewModel.Constraints daCongestion = GetCongestionRow(marketKey, rtSettleSelected.Date, sourceComboSelectedItem.NodeKey, (virtualsChecked ? 0 : sinkComboSelectedItem.NodeKey), hourCount, (dollarsChecked && nodalChecked), CongestionTypes.DACongestion, incChecked);
                         ViewModel.Constraints dartCongestion = GetCongestionRow(CongestionTypes.DARTCongestion);
                         constraintsList.Add(actuals);
                         constraintsList.Add(daCongestion);
                        */

                        Model.Constraints actuals = new Model.Constraints();
                        Model.Constraints daCongestion = new Model.Constraints();
                        if (RTORDA == "DA")
                        {
                            actuals = GetCongestionRow(marketKey, daSettleSelected.Date, sourceComboSelectedItem.NodeKey, (virtualsChecked ? 0 : sinkComboSelectedItem.NodeKey), hourCount, (dollarsChecked && nodalChecked), CongestionTypes.RTCongestion, incChecked);
                            daCongestion = GetCongestionRow(marketKey, daSettleSelected.Date, sourceComboSelectedItem.NodeKey, (virtualsChecked ? 0 : sinkComboSelectedItem.NodeKey), hourCount, (dollarsChecked && nodalChecked), CongestionTypes.DACongestion, incChecked);

                        }
                        if (RTORDA == "RT")
                        {
                            actuals = GetCongestionRow(marketKey, rtSettleSelected.Date, sourceComboSelectedItem.NodeKey, (virtualsChecked ? 0 : sinkComboSelectedItem.NodeKey), hourCount, (dollarsChecked && nodalChecked), CongestionTypes.RTCongestion, incChecked);
                            daCongestion = GetCongestionRow(marketKey, rtSettleSelected.Date, sourceComboSelectedItem.NodeKey, (virtualsChecked ? 0 : sinkComboSelectedItem.NodeKey), hourCount, (dollarsChecked && nodalChecked), CongestionTypes.DACongestion, incChecked);

                        }
                        Model.Constraints dartCongestion = GetCongestionRow(CongestionTypes.DARTCongestion);
                        constraintsList.Add(actuals);
                        constraintsList.Add(daCongestion);

                        for (int i = 1; i < 24; i++)
                        {
                            try
                            {
                                double? da = daCongestion.GetType().GetProperty("he" + i + "Value").GetValue(daCongestion) as double?;
                                double? rt = actuals.GetType().GetProperty("he" + i + "Value").GetValue(actuals) as double?;
                                //  if (incChecked)
                                {
                                    //   dartCongestion.GetType().GetProperty("he" + i + "Value").SetValue(dartCongestion, (da - rt));
                                }
                                //  else
                                {
                                    dartCongestion.GetType().GetProperty("he" + i + "Value").SetValue(dartCongestion, (rt - da));
                                }
                            }
                            catch { }
                        }

                        constraintsList.Add(dartCongestion);
                    }

                    mErcotSelectNoSensitivityConstraintsOneDateCommandRT.Parameters["@realTimeDate"].Value = rtSettleSelected.Date;
                    reader = mErcotSelectNoSensitivityConstraintsOneDateCommandRT.ExecuteReader();//tochange
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            Model.Constraints missing = new Model.Constraints();
                            missing.constraintNum = 0;
                            missing.monitoredName = GetConstraintName(reader.GetValue(2).ToString());
                            missing.contingName = GetContingencyName(reader.GetValue(3).ToString());
                            constraintsList.Add(missing);
                        }
                    }
                    reader.Close();
                }
                VayuConnection.Close();
                callback(constraintsList, null);
            }
        }



        /// <summary>
        /// Gets Price Nodes.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="isDA">if set to <c>true</c> [is da].</param>
        /// <returns></returns>
        public Node[] RunLmp(int marketKey, DateTime startDate, DateTime endDate, bool isDA = false)
        {
            Node[] priceNodes = null;
            while (true)
            {
                ILMP nodeProxy = GetProxy();
                try
                {
                    priceNodes = nodeProxy.GetAllPrice(marketKey, isDA, startDate, endDate, false);
                    break;
                }
                catch (Exception ex)
                {
                }
            }
            return priceNodes;
        }

        /// <summary>
        /// Gets the maximum LMP date time.
        /// </summary>
        /// <returns></returns>
        public DateTime GetMaxLMPDateTime(int marketKey)
        {
            ILMP nodeProxy = GetProxy();
            Node[] nodes = nodeProxy.GetAllUptoPrice(marketKey, false, DateTime.Now.Date, DateTime.Now.Date.AddDays(1), false);

            if (nodes.Length > 0)
            {
                DateTime dt = nodes[0].LmpTimePriceList.Where(x => x.Lmp != null).Max(x => x.MarketTime);
                return dt;
            }
            else
                return DateTime.Now;
        }

        /// <summary>
        /// Gets Node Proxy.
        /// </summary>
        /// <returns></returns>
        public ILMP GetProxy()
        {
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.Security.Mode = SecurityMode.None;
            myBinding.OpenTimeout = new TimeSpan(0, 30, 0);
            myBinding.SendTimeout = new TimeSpan(0, 12, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
            myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
            myBinding.TransactionFlow = false;
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.TransferMode = TransferMode.Buffered;
            myBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            ChannelFactory<ILMP> pipeFactory = new ChannelFactory<ILMP>(myBinding, new EndpointAddress(sDartEndPoint));
            ILMP nodeProxy = pipeFactory.CreateChannel();
            return nodeProxy;
        }

        /// <summary>
        /// Gets congestion.
        /// </summary>
        /// <param name="typ">The typ.</param>
        /// <returns></returns>
        public Model.Constraints GetCongestionRow(CongestionTypes typ)
        {
            Model.Constraints actuals = new Model.Constraints();
            actuals.monitoredName = "";
            actuals.shiftFactor = null;

            switch (typ)
            {
                case CongestionTypes.RTCongestion:
                    actuals.contingName = "RT Congestion";
                    actuals.summaryConstraintNum = -2;
                    break;
                case CongestionTypes.DACongestion:
                    actuals.contingName = "DA Congestion";
                    actuals.summaryConstraintNum = -3;
                    break;
                case CongestionTypes.DARTCongestion:
                    actuals.contingName = "DART Congestion";
                    actuals.summaryConstraintNum = -4;
                    break;
                default:
                    break;
            }

            return actuals;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the name of the constraint.
        /// </summary>
        /// <param name="constraintName">Name of the constraint.</param>
        /// <returns></returns>
        private string GetConstraintName(string constraintName)
        {
            return constraintName.Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
        }
        /// <summary>
        /// Gets the name of the contingency.
        /// </summary>
        /// <param name="contingencyName">Name of the contingency.</param>
        /// <returns></returns>
        private string GetContingencyName(string contingencyName)
        {
            return contingencyName.Replace("Contingency", "");
        }

        /// <summary>
        /// Fills the constraint hash.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="constraintHash">The constraint hash.</param>
        /// <param name="incChecked">if set to <c>true</c> [inc checked].</param>

        private void FillConstraintHashERCOT(SqlDataReader reader, Dictionary<int, Model.Constraints> constraintHash, bool incChecked, DateTime EndDate, string RTORDA = "")
        {
            Dictionary<int, List<ConstraintHelper>> AllConstraintDic = new Dictionary<int, List<ConstraintHelper>>();
            Dictionary<int, List<ConstraintHelper>> RTConstraintDic = new Dictionary<int, List<ConstraintHelper>>();
            Dictionary<string, Dictionary<DateTime, double>> PreviousDaydic = new Dictionary<string, Dictionary<DateTime, double>>();

            ConstraintHelper constraintobj;
            List<ConstraintHelper> ConstraintobjList = new List<ConstraintHelper>();
            string ConstraintTextVal, ContigencyTextVal;
            double ShiftfactorVal, ShadowPriceVal, ConstraintHourVal;
            DateTime currentDatetime;
            int ConstraintNumVal, HourVal;
            try
            {

                while (reader.Read())
                {
                    try
                    {
                        int hour = Convert.ToInt16(reader.GetValue(5));
                        int constraintNum = Convert.ToInt32(reader.GetValue(1));
                        constraintobj = new ConstraintHelper();
                        constraintobj.ConstraintNum = Convert.ToInt32(reader.GetValue(1));
                        ConstraintobjList = new List<ConstraintHelper>();

                        double heValue = reader.IsDBNull(4) ? 0 : (Convert.ToDouble(reader.GetValue(4)));

                        if (incChecked)
                        {
                            heValue = -1 * heValue;
                            ShiftfactorVal = -1 * Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);
                        }
                        else
                        {
                            heValue = heValue;
                            ShiftfactorVal = Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);
                        }


                        ConstraintNumVal = Convert.ToInt32(reader.GetValue(1));
                        ConstraintTextVal = GetConstraintName(reader.GetValue(2).ToString());
                        ContigencyTextVal = GetContingencyName(reader.GetValue(3).ToString());
                        HourVal = Convert.ToInt16(reader.GetValue(5));
                        ShadowPriceVal = Math.Round(Convert.ToDouble(reader.GetValue(6)), 2);
                        currentDatetime = reader.GetDateTime(7);


                        ConstraintHourVal = heValue;

                        constraintobj.ConstraintNum = ConstraintNumVal;
                        constraintobj.ConstraintText = ConstraintTextVal;
                        constraintobj.ContigencyText = ContigencyTextVal;
                        constraintobj.ShadowPrice = ShadowPriceVal;
                        constraintobj.ConstraintValue = ConstraintHourVal;
                        constraintobj.ShadowPrice = ShadowPriceVal;
                        constraintobj.hour = HourVal;
                        constraintobj.Shiftfactor = ShiftfactorVal;
                        constraintobj.MarketDateTime = currentDatetime;

                        ConstraintobjList.Add(constraintobj);

                        if (EndDate.Date.Equals(currentDatetime.Date))
                        {
                            if (AllConstraintDic.ContainsKey(ConstraintNumVal))
                                AllConstraintDic[ConstraintNumVal].Add(constraintobj);
                            else
                                AllConstraintDic.Add(ConstraintNumVal, ConstraintobjList);
                        }
                        else
                        {
                            Dictionary<DateTime, double> tempHash = new Dictionary<DateTime, double>();
                            if (RTORDA == "RT")
                            {
                                string keyText = ConstraintTextVal + "?split?" + ContigencyTextVal;
                                tempHash.Add(currentDatetime, reader.IsDBNull(6) ? 0 : Math.Abs(Convert.ToDouble(reader.GetValue(6))));
                                PreviousDaydic.Add(keyText, tempHash);
                            }

                            else
                            {
                                if (AllConstraintDic.ContainsKey(ConstraintNumVal))
                                    AllConstraintDic[ConstraintNumVal].Add(constraintobj);
                                else
                                    AllConstraintDic.Add(ConstraintNumVal, ConstraintobjList);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }




                }
                reader.Close();


                if (RTORDA == "RT")
                {
                    RTConstraintDic = CalculateRTRices(AllConstraintDic, PreviousDaydic);
                    AllConstraintDic = RTConstraintDic;
                }

                List<int> ConstraintNumberList = AllConstraintDic.Keys.ToList();

                foreach (var item in ConstraintNumberList)
                {
                    List<ConstraintHelper> Valueobj = AllConstraintDic[item];
                    for (int i = 0; i <= Valueobj.Count - 1; i++)
                    {

                        Model.Constraints constraint = new Model.Constraints();
                        int constraintNum = Valueobj.ElementAt(i).ConstraintNum;


                        if (constraintHash.ContainsKey(constraintNum))
                        {
                            constraint = constraintHash[constraintNum];
                        }
                        else
                        {
                            constraintHash.Add(constraintNum, constraint);
                        }

                        constraint.constraintNum = Valueobj.ElementAt(i).ConstraintNum;
                        constraint.monitoredName = Valueobj.ElementAt(i).ConstraintText;
                        constraint.contingName = Valueobj.ElementAt(i).ContigencyText;
                        constraint.shiftFactor = Valueobj.ElementAt(i).Shiftfactor;

                        double heValue = Valueobj.ElementAt(i).ConstraintValue;
                        int hour = Valueobj.ElementAt(i).hour;


                        if (RTORDA == "DA")
                        {
                            if (hour == 0)
                            {
                                constraint.he24Value = heValue;
                            }
                        }

                        if (RTORDA == "RT")
                            hour = hour + 1;


                        if (hour == 1)
                        {
                            constraint.he1Value = heValue;
                        }
                        else if (hour == 2)
                        {
                            constraint.he2Value = heValue;
                        }
                        else if (hour == 3)
                        {
                            constraint.he3Value = heValue;
                        }
                        else if (hour == 4)
                        {
                            constraint.he4Value = heValue;
                        }
                        else if (hour == 5)
                        {
                            constraint.he5Value = heValue;
                        }
                        else if (hour == 6)
                        {
                            constraint.he6Value = heValue;
                        }
                        else if (hour == 7)
                        {
                            constraint.he7Value = heValue;
                        }
                        else if (hour == 8)
                        {
                            constraint.he8Value = heValue;
                        }
                        else if (hour == 9)
                        {
                            constraint.he9Value = heValue;
                        }
                        else if (hour == 10)
                        {
                            constraint.he10Value = heValue;
                        }
                        else if (hour == 11)
                        {
                            constraint.he11Value = heValue;
                        }
                        else if (hour == 12)
                        {
                            constraint.he12Value = heValue;
                        }
                        else if (hour == 13)
                        {
                            constraint.he13Value = heValue;
                        }
                        else if (hour == 14)
                        {
                            constraint.he14Value = heValue;
                        }
                        else if (hour == 15)
                        {
                            constraint.he15Value = heValue;
                        }
                        else if (hour == 16)
                        {
                            constraint.he16Value = heValue;
                        }
                        else if (hour == 17)
                        {
                            constraint.he17Value = heValue;
                        }
                        else if (hour == 18)
                        {
                            constraint.he18Value = heValue;
                        }
                        else if (hour == 19)
                        {
                            constraint.he19Value = heValue;
                        }
                        else if (hour == 20)
                        {
                            constraint.he20Value = heValue;
                        }
                        else if (hour == 21)
                        {
                            constraint.he21Value = heValue;
                        }
                        else if (hour == 22)
                        {
                            constraint.he22Value = heValue;
                        }
                        else if (hour == 23)
                        {
                            constraint.he23Value = heValue;
                        }
                        //else if (hour == 24)
                        else if (hour == 24)
                        {
                            constraint.he24Value = heValue;
                        }


                    }





                }
            }
            catch (Exception ex)
            {
                //throw;
            }
        }
        /// <summary>
        /// Gets congestion.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        /// <param name="sourceNodeKey">The source node key.</param>
        /// <param name="sinkNodeKey">The sink node key.</param>
        /// <param name="hourCount">The hour count.</param>
        /// <param name="onlySourceValue">if set to <c>true</c> [only source value].</param>
        /// <param name="typ">The typ.</param>
        /// <param name="incChecked">if set to <c>true</c> [inc checked].</param>
        /// <returns></returns>
        /// 
        private Dictionary<int, List<ConstraintHelper>> CalculateRTRices(Dictionary<int, List<ConstraintHelper>> AllDataDic, Dictionary<string, Dictionary<DateTime, double>> PrevDayDic)
        {
            Dictionary<int, List<ConstraintHelper>> DataDicReturn = new Dictionary<int, List<ConstraintHelper>>();
            List<ConstraintHelper> DataListReturn;
            ConstraintHelper objReturn;

            string ConstraintText, ContigencyText;
            DateTime dateCounter;


            List<string> l1 = new List<string>();
            List<string> l2 = new List<string>();
            List<string> l3 = new List<string>();
            List<string> l4 = new List<string>();


            Dictionary<int, List<RTConstraintHelper>> hourMinuteDict = new Dictionary<int, List<RTConstraintHelper>>();
            List<int> ConstraintNumberList = AllDataDic.Keys.ToList();

            List<RTConstraintHelper> lstRTConstraintHelpers = new List<RTConstraintHelper>();
            try
            {
                foreach (var ConstraintNumber in ConstraintNumberList)
                {

                    var SingleConstraintObj = AllDataDic[ConstraintNumber];
                    hourMinuteDict = new Dictionary<int, List<RTConstraintHelper>>();
                    RTConstraintHelper RTConstraintHelperobj = new RTConstraintHelper();

                    for (int j = 0; j < SingleConstraintObj.Count; j++)
                    {

                        lstRTConstraintHelpers = new List<RTConstraintHelper>();
                        int Hourval = SingleConstraintObj.ElementAt(j).hour;
                        RTConstraintHelperobj = new RTConstraintHelper();
                        RTConstraintHelperobj.Hourval = Hourval;
                        RTConstraintHelperobj.Minute = SingleConstraintObj.ElementAt(j).MarketDateTime.Minute;
                        RTConstraintHelperobj.Seconds = SingleConstraintObj.ElementAt(j).MarketDateTime.Second;
                        RTConstraintHelperobj.ShadowPrice = SingleConstraintObj.ElementAt(j).ShadowPrice;

                        lstRTConstraintHelpers.Add(RTConstraintHelperobj);

                        if (!(hourMinuteDict.ContainsKey(Hourval)))
                        {
                            hourMinuteDict.Add(Hourval, lstRTConstraintHelpers);
                        }
                        else
                        {
                            hourMinuteDict[Hourval].Add(RTConstraintHelperobj);
                        }

                    }//single constraint

                    List<int> hourList = hourMinuteDict.Keys.ToList();
                    hourList = hourList.OrderByDescending(x => x).ToList();
                    foreach (var hour in hourList)
                    {
                        DataListReturn = new List<ConstraintHelper>();
                        objReturn = new ConstraintHelper(); ;

                        ConstraintText = SingleConstraintObj.ElementAt(0).ConstraintText;
                        ContigencyText = SingleConstraintObj.ElementAt(0).ContigencyText;
                        dateCounter = SingleConstraintObj.ElementAt(0).MarketDateTime;

                        objReturn.ConstraintNum = SingleConstraintObj.ElementAt(0).ConstraintNum;
                        objReturn.ConstraintText = SingleConstraintObj.ElementAt(0).ConstraintText;
                        objReturn.ContigencyText = SingleConstraintObj.ElementAt(0).ContigencyText;
                        objReturn.hour = hour;
                        objReturn.Shiftfactor = SingleConstraintObj.ElementAt(0).Shiftfactor;
                        objReturn.MarketDateTime = SingleConstraintObj.ElementAt(0).MarketDateTime;
                        objReturn.ConstraintValue = SingleConstraintObj.ElementAt(0).ConstraintValue;

                        var hourMinuteCollectionSP = hourMinuteDict[hour];

                        RTConstraintHelper int1Value = new RTConstraintHelper();
                        List<double> diffList = new List<double>();
                        List<double> timediffList = new List<double>();
                        l1.Clear(); l2.Clear(); l3.Clear(); l4.Clear();

                        double interval1, interval2, interval3, interval4, spvalue;
                        string key = "", allval = ""; ;
                        double finalsp = 0;
                        interval1 = interval2 = interval3 = interval4 = spvalue = 0;
                        hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();

                        int elementcount = hourMinuteCollectionSP.Count;
                        int lastmin = hourMinuteCollectionSP.ElementAt(elementcount - 1).Minute;

                        RTConstraintHelper tempobj;
                        if (objReturn.ConstraintText == "CONCHO_VRBS1_1/CONCHO-VRBS/69-69")
                        { }


                        if (lastmin < 55)
                        {
                            List<string> MissingPrintList = getMissingPrints(objReturn.MarketDateTime, hour, lastmin);

                            int cnt = 0;
                            while (cnt < MissingPrintList.Count)
                            {
                                string[] valuearray = MissingPrintList.ElementAt(cnt).Split('#');
                                tempobj = new RTConstraintHelper();
                                tempobj.Hourval = hour;
                                tempobj.Minute = Int32.Parse(valuearray[0]);
                                tempobj.Seconds = Int32.Parse(valuearray[1]);
                                tempobj.ShadowPrice = 0;

                                hourMinuteCollectionSP.Add(tempobj);
                                cnt++;
                            }

                        }
                        hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                        elementcount = hourMinuteCollectionSP.Count;
                        lastmin = hourMinuteCollectionSP.ElementAt(elementcount - 1).Minute;

                        while (lastmin < 55)
                        {
                            if (lastmin % 5 == 0)
                                lastmin = lastmin + 5;
                            else
                            {
                                do
                                    lastmin = lastmin + 1;
                                while ((lastmin % 5 != 0));
                            }
                            tempobj = new RTConstraintHelper();
                            tempobj.Hourval = hour;
                            tempobj.Seconds = 0;
                            tempobj.Minute = lastmin;
                            tempobj.ShadowPrice = 0;
                            hourMinuteCollectionSP.Add(tempobj);
                        }

                        for (int i = 0; i < hourMinuteCollectionSP.Count; i++)
                        {
                            int1Value = new RTConstraintHelper();
                            int1Value.Hourval = hourMinuteCollectionSP.ElementAt(i).Hourval;
                            int1Value.Minute = hourMinuteCollectionSP.ElementAt(i).Minute;
                            int1Value.Seconds = hourMinuteCollectionSP.ElementAt(i).Seconds;
                            int1Value.ShadowPrice = hourMinuteCollectionSP.ElementAt(i).ShadowPrice;

                            int currmin = int1Value.Minute;
                            int currsec = int1Value.Seconds;

                            key = allval = ""; ;
                            finalsp = 0;
                            if (i == 0)
                            {
                                key = ConstraintText + "?split?" + ContigencyText;
                                spvalue = getPreviousMinSP(hour, currmin, currsec, hourMinuteDict, key, dateCounter.ToString(), PrevDayDic);

                                finalsp = (currmin * 60 + currsec) * spvalue;
                                allval = hour + "#" + currmin + "#" + currsec + "?" + (currmin * 60 + currsec) + "#" + spvalue + "#" + finalsp;
                                l1.Add(allval);
                                interval1 = interval1 + finalsp;


                            }

                            int totalsec1, totalsec2, totaltime;
                            if (i > 0)
                            {
                                if (currmin > 0 && currmin <= 15)
                                {
                                    if (currmin == 15)
                                    {
                                        totalsec1 = currmin * 60;
                                        totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                        totaltime = totalsec1 - totalsec2;

                                    }
                                    else
                                    {
                                        totalsec1 = currmin * 60 + currsec;
                                        totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                        totaltime = totalsec1 - totalsec2;
                                    }

                                    spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                    finalsp = totaltime * spvalue;
                                    allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                    l1.Add(allval);
                                    interval1 = interval1 + finalsp;

                                }
                                if (currmin >= 15 && currmin <= 30)
                                {
                                    if (currmin == 30)
                                    {
                                        totalsec1 = currmin * 60;
                                        totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                        totaltime = totalsec1 - totalsec2;

                                    }
                                    else
                                    {
                                        if (currmin == 15)
                                        {
                                            totalsec2 = totalsec1 = 0;
                                            totaltime = currsec;
                                        }
                                        else
                                        {
                                            totalsec1 = currmin * 60 + currsec;
                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                            totaltime = totalsec1 - totalsec2;
                                        }
                                    }

                                    spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                    finalsp = totaltime * spvalue;
                                    allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                    l2.Add(allval);
                                    interval2 = interval2 + finalsp;

                                }
                                if (currmin >= 30 && currmin <= 45)
                                {
                                    if (currmin == 45)
                                    {
                                        totalsec1 = currmin * 60;
                                        totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                        totaltime = totalsec1 - totalsec2;

                                    }
                                    else
                                    {
                                        if (currmin == 30)
                                        {
                                            totalsec2 = totalsec1 = 0;
                                            totaltime = currsec;
                                        }
                                        else
                                        {
                                            totalsec1 = currmin * 60 + currsec;
                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                            totaltime = totalsec1 - totalsec2;
                                        }
                                    }

                                    spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                    finalsp = totaltime * spvalue;
                                    allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                    l3.Add(allval);
                                    interval3 = interval3 + finalsp;

                                }


                                if (currmin >= 45 && currmin <= 59)
                                {
                                    if (i == hourMinuteCollectionSP.Count - 1)
                                    {

                                        totalsec1 = currmin * 60 + currsec;
                                        totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                        totaltime = totalsec1 - totalsec2;

                                    }
                                    else
                                    {
                                        if (currmin == 45)
                                        {
                                            totalsec2 = totalsec1 = 0;
                                            totaltime = currsec;
                                        }
                                        else
                                        {
                                            totalsec1 = currmin * 60 + currsec;
                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                            totaltime = totalsec1 - totalsec2;
                                        }
                                    }

                                    spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                    finalsp = totaltime * spvalue;
                                    allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                    l4.Add(allval);
                                    interval4 = interval4 + finalsp;

                                    if (i == hourMinuteCollectionSP.Count - 1)
                                    {

                                        totalsec1 = currmin * 60 + currsec;
                                        totalsec2 = 60 * 60;
                                        totaltime = totalsec2 - totalsec1;

                                        spvalue = hourMinuteCollectionSP.ElementAt(i).ShadowPrice;
                                        finalsp = totaltime * spvalue;
                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                        l4.Add(allval);
                                        interval4 = interval4 + finalsp;

                                    }

                                }
                            }

                        }
                        interval1 = interval1 / 900;
                        interval2 = interval2 / 900;
                        interval3 = interval3 / 900;
                        interval4 = interval4 / 900;

                        spvalue = (interval1 + interval2 + interval3 + interval4) / 4;
                        objReturn.ShadowPrice = spvalue;

                        double Sensitivitydiff = objReturn.ConstraintValue;
                        objReturn.ConstraintValue = Sensitivitydiff * spvalue;

                        DataListReturn.Add(objReturn);

                        if (DataDicReturn.ContainsKey(ConstraintNumber))
                            DataDicReturn[ConstraintNumber].Add(objReturn);
                        else

                            DataDicReturn.Add(ConstraintNumber, DataListReturn);

                    }
                }

            }
            catch (Exception e)
            { }
            return DataDicReturn;
        }

        List<string> getMissingPrints(DateTime DateVal, int Hourval, int MinVal)
        {
            List<string> MinSecList = new List<string>();
            string MinuteSecond = "";

            try
            {
                SqlCommand mSelectSCEDPrintCommond = new SqlCommand();
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mSelectSCEDPrintCommond.CommandText = "Select distinct MarketMin, Second from NodeLMPMin where MarketDate = @DateVal and  Markethour=@Hourval and MarketMin > @MinVal";
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@DateVal", DateVal.Date);
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@Hourval", Hourval);
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@MinVal", MinVal);
                mSelectSCEDPrintCommond.Connection = VayuConnection;
                SqlDataReader reader = mSelectSCEDPrintCommond.ExecuteReader();
                while (reader.Read())
                {
                    MinuteSecond = reader.GetInt32(0) + "#" + reader.GetInt32(1);
                    MinSecList.Add(MinuteSecond);
                }
                reader.Close();

            }
            catch (Exception ae)
            { }
            return MinSecList;

        }
        double getPreviousMinSP(int hour, int min, int sec, Dictionary<int, List<RTConstraintHelper>> TotalhourMinuteDict, string key, string dateString, Dictionary<string, Dictionary<DateTime, double>> PrevDayConstraintDic)
        {
            double sp = 0;
            List<RTConstraintHelper> prevHourMinCollectionSP = null;

            try
            {
                if (hour == 0)
                {
                    if (PrevDayConstraintDic.ContainsKey(key))
                    {
                        var item2 = PrevDayConstraintDic[key];
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        DateTime dateTime10 = Convert.ToDateTime(dateString);
                        dateTime10 = dateTime10.AddMinutes(-5);
                        List<RTConstraintHelper> valueliest = new List<RTConstraintHelper>();
                        RTConstraintHelper obj;
                        foreach (var keyitem in item2)
                        {
                            int a = keyitem.Key.Date.CompareTo(dateTime10.Date);
                            if (keyitem.Key.Date.Equals(dateTime10.Date))
                            {
                                if (keyitem.Key.Hour == 23 && keyitem.Key.Minute > 54)
                                {
                                    obj = new RTConstraintHelper();
                                    obj.Hourval = 23;
                                    obj.Minute = keyitem.Key.Minute;
                                    obj.Seconds = keyitem.Key.Second;
                                    obj.ShadowPrice = keyitem.Value;
                                    valueliest.Add(obj);
                                }
                            }
                        }
                        valueliest = valueliest.OrderByDescending(x => x.Minute).ToList();

                        if (valueliest.Count > 0)
                            sp = valueliest.ElementAt(0).ShadowPrice;
                    }
                }
                if (TotalhourMinuteDict.ContainsKey(hour - 1))
                {
                    prevHourMinCollectionSP = TotalhourMinuteDict[hour - 1];
                    prevHourMinCollectionSP = prevHourMinCollectionSP.OrderBy(x => x.Minute).ToList();
                    int lastindex = prevHourMinCollectionSP.Count - 1;
                    //0 string s = prevHourMinCollectionSP.ElementAt(lastindex).Minute + ":" + prevHourMinCollectionSP.ElementAt(lastindex).Seconds; ;
                    sp = prevHourMinCollectionSP.ElementAt(lastindex).ShadowPrice;

                }
            }
            catch (Exception e)
            { }
            return sp;
        }

        private Model.Constraints GetCongestionRow(int marketKey, DateTime selectedDate, int sourceNodeKey, int sinkNodeKey, int hourCount, bool onlySourceValue, CongestionTypes typ, bool incChecked)
        {
            Model.Constraints daCongestion = GetCongestionRow(typ);

            try
            {
                //source
                Node[] DALmpPricesSource = RunLmp(marketKey, selectedDate.Date, selectedDate.Date.AddDays(1), typ == CongestionTypes.DACongestion);
                Node[] DAnodeinfoSource = DALmpPricesSource.Where(t => t.NodeId.Equals(sourceNodeKey)).ToArray();
                //sink
                Node[] DAnodeinfoSink = null;
                if (!onlySourceValue)
                {
                    Node[] DALmpPricesSink = RunLmp(marketKey, selectedDate.Date, selectedDate.Date.AddDays(1), typ == CongestionTypes.DACongestion);
                    DAnodeinfoSink = DALmpPricesSink.Where(t => t.NodeId.Equals(sinkNodeKey)).ToArray();
                }
                //DA sink congestion - source congestion
                if (marketKey == 1)
                {
                    for (int i = 1; i <= hourCount; i++)
                    {
                        LmpTimePrice[] nodehourpriceSource = DAnodeinfoSource[0].LmpTimePriceList.Where(x => x.MarketTime.Equals(selectedDate.Date.AddHours(i))).ToArray();
                        double sourceCong;
                        try
                        {
                            if (true)
                            {
                                if (incChecked)
                                    sourceCong = (-1) * nodehourpriceSource[0].Lmp.Congestion;
                                else
                                    sourceCong = nodehourpriceSource[0].Lmp.Congestion;
                            }
                        }
                        catch
                        {
                            sourceCong = 0;
                        }
                        double value = 0;
                        if (DAnodeinfoSink == null)
                        {
                            value = sourceCong;
                        }
                        else
                        {
                            LmpTimePrice[] nodehourpriceSink = DAnodeinfoSink[0].LmpTimePriceList.Where(x => x.MarketTime.Equals(selectedDate.Date.AddHours(i))).ToArray();
                            double sinkCong;
                            try
                            {
                                sinkCong = nodehourpriceSink[0].Lmp.Congestion;
                            }
                            catch
                            {
                                sinkCong = 0;
                            }
                            value = sinkCong - sourceCong;
                        }
                        if (value == 0)
                        {
                            daCongestion.GetType().GetProperty("he" + i + "Value").SetValue(daCongestion, null);
                        }
                        daCongestion.GetType().GetProperty("he" + i + "Value").SetValue(daCongestion, value);
                    }
                }
                if (marketKey == 9)
                {
                    for (int i = 1; i <= hourCount; i++)
                    {
                        LmpTimePrice[] nodehourpriceSource = DAnodeinfoSource[0].LmpTimePriceList.Where(x => x.MarketTime.Equals(selectedDate.Date.AddHours(i))).ToArray();
                        double sourceCong;
                        try
                        {
                            if (true)
                            {
                                if (incChecked)
                                    sourceCong = (-1) * nodehourpriceSource[0].Lmp.Price;
                                else
                                    sourceCong = nodehourpriceSource[0].Lmp.Price;
                            }
                        }
                        catch
                        {
                            sourceCong = 0;
                        }
                        double value = 0;
                        if (DAnodeinfoSink == null)
                        {
                            value = sourceCong;
                        }
                        else
                        {
                            LmpTimePrice[] nodehourpriceSink = DAnodeinfoSink[0].LmpTimePriceList.Where(x => x.MarketTime.Equals(selectedDate.Date.AddHours(i))).ToArray();
                            double sinkCong;
                            try
                            {
                                sinkCong = nodehourpriceSink[0].Lmp.Price;
                            }
                            catch
                            {
                                sinkCong = 0;
                            }
                            value = sinkCong - sourceCong;
                        }
                        if (value == 0)
                        {
                            daCongestion.GetType().GetProperty("he" + i + "Value").SetValue(daCongestion, null);
                        }
                        daCongestion.GetType().GetProperty("he" + i + "Value").SetValue(daCongestion, value);
                    }
                }
            }
            catch { }
            return daCongestion;
        }

        /// <summary>
        /// Gets the outage constraint list.
        /// </summary>
        /// <param name="StartChecked">if set to <c>true</c> [start checked].</param>
        /// <param name="OutageChecked">if set to <c>true</c> [outage checked].</param>
        /// <param name="ConstraintChecked">if set to <c>true</c> [constraint checked].</param>
        /// <param name="ShadowPriceValue">The shadow price value.</param>
        /// <param name="StartDate">The start date.</param>
        /// <returns></returns>
        private List<OutageConstraint> GetOutageConstraintList(bool StartChecked, bool OutageChecked, bool ConstraintChecked, string ShadowPriceValue, DateTime StartDate, string RTORDA, int marketkey)
        {
            if (StartChecked && OutageChecked)
            {
                List<OutageConstraint> outageConstraintList = new List<OutageConstraint>();
                SqlDataReader reader = null;

                if (RTORDA == "RT")
                {

                    mSelectStartCheckedConstraintsCommandRT.Parameters["@ShadowPriceValue"].Value = ShadowPriceValue;
                    mSelectStartCheckedConstraintsCommandRT.Parameters["@StartDate"].Value = StartDate.Date;
                    mSelectStartCheckedConstraintsCommandRT.Parameters["@EndDate"].Value = StartDate.AddDays(1).Date;
                    reader = mSelectStartCheckedConstraintsCommandRT.ExecuteReader();
                    while (reader.Read())
                    {
                        OutageConstraint outageConstraint = new OutageConstraint();
                        outageConstraint.ConstraintName = reader.GetString(0);
                        outageConstraint.constraintRTNum = Convert.ToString(reader.GetValue(1));
                        outageConstraint.ContingencyName = reader.GetString(2);
                        outageConstraint.MarketDateTime = reader.GetDateTime(3);
                        outageConstraintList.Add(outageConstraint);
                    }
                }
                //if (RTORDA == "DA")
                //{

                //    mSelectStartCheckedConstraintsCommand.Parameters["@ShadowPriceValue"].Value = ShadowPriceValue;
                //    mSelectStartCheckedConstraintsCommand.Parameters["@StartDate"].Value = StartDate.Date;
                //    mSelectStartCheckedConstraintsCommand.Parameters["@EndDate"].Value = StartDate.AddDays(1).Date;
                //     reader = mSelectStartCheckedConstraintsCommand.ExecuteReader();
                //    while (reader.Read())
                //    {
                //        OutageConstraint outageConstraint = new OutageConstraint();
                //        outageConstraint.ConstraintName = reader.GetString(0);
                //        outageConstraint.constraintRTNum = Convert.ToString(reader.GetValue(1));
                //        outageConstraint.ContingencyName = reader.GetString(2);
                //        outageConstraint.MarketDateTime = reader.GetDateTime(3);
                //        outageConstraintList.Add(outageConstraint);
                //    }
                //}


                reader.Close();
                return outageConstraintList;
            }
            else if (OutageChecked && !StartChecked)
            { // dollar-outge 


                List<OutageConstraint> outageConstraintList = new List<OutageConstraint>();
                mSelectOutageCheckedConstraintCommand.Parameters["@ShadowPriceValue"].Value = ShadowPriceValue;
                mSelectOutageCheckedConstraintCommand.Parameters["@StartDate"].Value = StartDate.Date;
                mSelectOutageCheckedConstraintCommand.Parameters["@EndDate"].Value = StartDate.AddDays(1).Date;
                SqlDataReader reader = mSelectOutageCheckedConstraintCommand.ExecuteReader();
                while (reader.Read())
                {
                    OutageConstraint outageConstraint = new OutageConstraint();
                    outageConstraint.ConstraintName = reader.GetString(0);
                    outageConstraint.constraintRTNum = Convert.ToString(reader.GetValue(1));
                    outageConstraint.ContingencyName = reader.GetString(2);
                    outageConstraint.MarketDateTime = reader.GetDateTime(3);
                    outageConstraintList.Add(outageConstraint);
                }
                reader.Close();
                return outageConstraintList;
            }
            else if (ConstraintChecked)
            {
                List<OutageConstraint> outageConstraintlist = new List<OutageConstraint>();
                mSelectConstraintCheckedCommand.Parameters["@StartDate"].Value = StartDate.AddDays(1).Date;
                mSelectConstraintCheckedCommand.Parameters["@EndDate"].Value = StartDate.AddDays(2).Date;
                mSelectConstraintCheckedCommand.Parameters["@ShadowPriceValue"].Value = ShadowPriceValue;
                SqlDataReader reader = mSelectConstraintCheckedCommand.ExecuteReader();
                while (reader.Read())
                {
                    OutageConstraint outageConstraint = new OutageConstraint();
                    outageConstraint.constraintRTNum = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                    outageConstraintlist.Add(outageConstraint);
                }
                return outageConstraintlist;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CongestionTypes
    {
        /// <summary>
        /// The rt congestion
        /// </summary>
        RTCongestion,
        /// <summary>
        /// The da congestion
        /// </summary>
        DACongestion,
        /// <summary>
        /// The dart congestion
        /// </summary>
        DARTCongestion
    }


    internal class ConstraintHelper
    {
        public string ConstraintText { get; set; }

        public string ContigencyText { get; set; }

        public int ConstraintNum { get; set; }

        public double ConstraintValue { get; set; }

        public double Shiftfactor { get; set; }

        public double ShadowPrice { get; set; }

        public int hour { get; set; }

        public DateTime MarketDateTime { get; set; }



        ///  public int Minute { get; set; }
        //  public int Seconds { get; set; }
        //  public double ShadowPrice { get; set; }
        //  public int Hourval { get; set; }

    }

    internal class RTConstraintHelper
    {
        public int Minute { get; set; }
        public int Seconds { get; set; }
        public double ShadowPrice { get; set; }
        public int Hourval { get; set; }

    }
}
