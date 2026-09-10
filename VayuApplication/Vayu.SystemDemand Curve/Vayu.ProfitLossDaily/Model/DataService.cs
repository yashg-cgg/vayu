using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonLibrary;
using Vayu.DBLibrary;

namespace Vayu.ProfitLossDaily.Model
{
    public class DataService : IDataService
    {
        #region Golbal Data Members

        private SqlConnection VayuDbConn; 

        private SqlCommand mSelectMarketCommand;

        private SqlCommand mSelectMWCommand;

        private SqlCommand mSelectPnlFeeCommand;
        private SqlCommand mSelectErcotPnlFeeCommand;
        private SqlCommand mSelectErcotExternalPnlFeeCommand;

        private SqlCommand mSelectLastFeeCommand;

        private SqlCommand mSelectPathDollarsCommand;

        private static SqlCommand cmdSelectExternalPortfolioName;

        private static Dictionary<int, PNLConstraints> sConstraintHash = new Dictionary<int, PNLConstraints>();

        private static Dictionary<int, string> dictPortfolioHash = new Dictionary<int, string>();

        private static readonly object lockobj = new object();

        public static Dictionary<string, double> sRTHash = new Dictionary<string, double>();

        public static Dictionary<string, double> sDAHash = new Dictionary<string, double>();


        #endregion

        #region OldCode
        #endregion
        private Dictionary<int, List<PNLConstraints>> mPathDetailHash = new Dictionary<int, List<PNLConstraints>>();

        private SqlCommand mSelectConstraintContingecyCommand;
        private SqlCommand mSelectSenstivityCommand;
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
        /// Fills the constraint hash.0
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="source">The source.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="hourHash">The hour hash.</param>
        private void FillConstraintHash(SqlDataReader reader, string source, string sink, Dictionary<DateTime, Dictionary<int, Dictionary<string, double>>> hourHashWithDate)
        {
            while (reader.Read())
            {
                int constraintNum = Convert.ToInt32(reader.GetValue(1));
                DateTime Date = Convert.ToDateTime(reader.GetDateTime(5));
                int hour = Convert.ToInt16(reader.GetValue(6));

                DateTime dateTime = Date.AddHours(hour);

                Dictionary<int, Dictionary<string, double>> constraintHash = new Dictionary<int, Dictionary<string, double>>();
                if (hourHashWithDate.ContainsKey(dateTime))
                {
                    constraintHash = hourHashWithDate[dateTime];
                    hourHashWithDate.Remove(dateTime);
                }

                Dictionary<string, double> sourceSinkHash = new Dictionary<string, double>();
                if (constraintHash.ContainsKey(constraintNum))
                {
                    sourceSinkHash = constraintHash[constraintNum];
                    constraintHash.Remove(constraintNum);
                }
                string sourceSinkKey = source + ":" + sink;
                double sensitivity = reader.IsDBNull(4) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                if (!sourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    sourceSinkHash.Add(sourceSinkKey, sensitivity);
                }
                constraintHash.Add(constraintNum, sourceSinkHash);
                hourHashWithDate.Add(dateTime, constraintHash);
                PNLConstraints constraint = new PNLConstraints();
                if (!sConstraintHash.ContainsKey(constraintNum))
                {
                    constraint.constraintNum = constraintNum;
                    constraint.monitoredName = GetConstraintName(reader.GetValue(2).ToString());
                    constraint.contingName = GetContingencyName(reader.GetValue(3).ToString());
                    sConstraintHash.Add(constraintNum, constraint);
                }
            }
        }
        /// <summary>
        /// Fills the exposure hash.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="mw">The mw.</param>
        /// <param name="pnl.HE">The in hour.</param>
        /// <param name="constraintHash">The constraint hash.</param>
        /// <param name="hourHash">The hour hash.</param>

        #endregion


        #region Public Methods

        public void loadDBCommands()
        {
            VayuDbConn = new SqlConnection( DBConnectionCredentials.GetERCOTDBConnection());
            mSelectMarketCommand = new SqlCommand();
            mSelectMarketCommand.CommandText = "select market from portfolio where portfoliokey = @portfoliokey";
            mSelectMarketCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mSelectMarketCommand.Connection = VayuDbConn;
            //
            mSelectMWCommand = new SqlCommand();
            mSelectMWCommand.CommandText = "select marketdatetime, clearedmw from clearedbids where marketdatetime between @start and @end"; //and portfoliokey = @portfoliokey";
            mSelectMWCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectMWCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectMWCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mSelectMWCommand.Connection = VayuDbConn;
            //
            mSelectPnlFeeCommand = new SqlCommand();
            mSelectPnlFeeCommand.CommandText = "select pnldate, fee, pnl,dollarscleared , mw from virtual_pnl where pnldate between @start and @end and portfoliokey = @portfoliokey";
            mSelectPnlFeeCommand.Parameters.AddWithValue("@start", "pnldate");
            mSelectPnlFeeCommand.Parameters.AddWithValue("@end", "pnldate");
            mSelectPnlFeeCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mSelectPnlFeeCommand.Connection = VayuDbConn;
            //

            mSelectErcotPnlFeeCommand = new SqlCommand();
            mSelectErcotPnlFeeCommand.CommandText = "select pnldate, fee, pnl,dollarscleared , mw from DailyPNL where pnldate between @start and @end and portfoliokey = @portfoliokey";
            mSelectErcotPnlFeeCommand.Parameters.AddWithValue("@start", "pnldate");
            mSelectErcotPnlFeeCommand.Parameters.AddWithValue("@end", "pnldate");
            mSelectErcotPnlFeeCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mSelectErcotPnlFeeCommand.Connection = VayuDbConn;

            //mSelectErcotExternalPnlFeeCommand
            mSelectErcotExternalPnlFeeCommand = new SqlCommand();
            mSelectErcotExternalPnlFeeCommand.CommandText = "select PnlDate, Pnl,DollarsCleared , mw from  Vayu..ExternalPnl where PnlDate between @start and @end and ParticipantID = @portfoliokey";
            mSelectErcotExternalPnlFeeCommand.Parameters.AddWithValue("@start", "pnldate");
            mSelectErcotExternalPnlFeeCommand.Parameters.AddWithValue("@end", "pnldate");
            mSelectErcotExternalPnlFeeCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mSelectErcotExternalPnlFeeCommand.Connection = VayuDbConn;
            //

            mSelectLastFeeCommand = new SqlCommand();
            mSelectLastFeeCommand.CommandText = "select PortfolioKey, PnlDate, fee from VIRTUAL_PNL where PnlDate = (select MAX(pnldate) from VIRTUAL_PNL where Fee <> 0 and PortfolioKey in " +
                                                "(select portfoliokey from Portfolio where tradetype = @tradetype and market = @market)) and fee <> 0 and " +
                                                "PortfolioKey  in (select portfoliokey from Portfolio where tradetype = @tradetype and market = @market)";
            mSelectLastFeeCommand.Parameters.AddWithValue("@tradetype", "tradetype");
            mSelectLastFeeCommand.Parameters.AddWithValue("@market", "market");
            mSelectLastFeeCommand.Connection = VayuDbConn;


            //

            mSelectPathDollarsCommand = new SqlCommand();

            mSelectPathDollarsCommand.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mSelectPathDollarsCommand.Parameters.AddWithValue("@sourceKey", "sourceKey");
            mSelectPathDollarsCommand.Parameters.AddWithValue("@sinkKey", "sinkKey");
            mSelectPathDollarsCommand.Connection = VayuDbConn;

            //
            mSelectConstraintContingecyCommand = new SqlCommand();
            mSelectConstraintContingecyCommand.Parameters.AddWithValue("@start", "MarkedDateTime");
            mSelectConstraintContingecyCommand.Parameters.AddWithValue("@end", "MarkedDateTime");
            mSelectConstraintContingecyCommand.Connection = VayuDbConn;

            //
            mSelectSenstivityCommand = new SqlCommand();
            mSelectSenstivityCommand.CommandText = "select NodeKey,Sensitivity, constraintrtnum from RTMasterVector (nolock) where ConstraintRTNum in " +
                                                   " (select ConstraintRTNum from  RTImpact(nolock)  where RTImpact.Date >= @start and RTImpact.Date <=@end)";
            mSelectSenstivityCommand.Parameters.AddWithValue("@start", "MarkedDateTime");
            mSelectSenstivityCommand.Parameters.AddWithValue("@end", "MarkedDateTime");
            mSelectSenstivityCommand.Connection = VayuDbConn;

            //
            cmdSelectExternalPortfolioName = new SqlCommand();
            cmdSelectExternalPortfolioName.CommandText = "select Participant from Company where Marketkey=9 and ParticipantID = @ParticipantID";
            cmdSelectExternalPortfolioName.Parameters.AddWithValue("@ParticipantID", "ParticipantID");
            cmdSelectExternalPortfolioName.Connection = VayuDbConn;
        }

        public string GetExternalPortfolioName(int portfolioKey)
        {
            if (dictPortfolioHash.ContainsKey(portfolioKey))
            {
                return dictPortfolioHash[portfolioKey];
            }
            VayuDbConn.Open();
            cmdSelectExternalPortfolioName.Parameters["@ParticipantID"].Value = portfolioKey;
            SqlDataReader reader = cmdSelectExternalPortfolioName.ExecuteReader();
            string account = null;
            while (reader.Read())
            {
                account = reader.GetString(0);
            }
            reader.Close();
            VayuDbConn.Close();
            if (account != null)
            {
                dictPortfolioHash.Add(portfolioKey, account);
            }
            return account;
        }
        /// <summary>
        /// Gets the PNL fee from database.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="product">The product.</param>
        /// <param name="portfolioKeyList">The portfolio key list.</param>
        /// <returns></returns>
        public Dictionary<DateTime, Dictionary<int, PnlFee>> GetPnlFee(DateTime startDate, DateTime endDate, string market, List<int> portfolioKeyList)
        {
            Dictionary<DateTime, Dictionary<int, PnlFee>> dateHash = new Dictionary<DateTime, Dictionary<int, PnlFee>>();
            if(VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
             if (market == "ERCOT")
            {
                mSelectMWCommand.CommandText = "select marketdatetime, clearedmw from clearedbids where marketdatetime between @start and @end and portfoliokey = @portfoliokey";
            }
            else if (market == "ERCOT External")
            {
                mSelectMWCommand.CommandText = "select DeliveryDate, MW from  Vayu..DAM60DAYPTPAWARDS where DeliveryDate between @start and @end and ParticipantID = @portfoliokey";
            }
            try
            {
                foreach (int portfolioKey in portfolioKeyList)
                {
                      if (market == "ERCOT")
                    {
                        mSelectErcotPnlFeeCommand.Parameters["@start"].Value = startDate;
                        mSelectErcotPnlFeeCommand.Parameters["@end"].Value = endDate.AddDays(0);
                        mSelectErcotPnlFeeCommand.Parameters["@portfoliokey"].Value = portfolioKey;
                        SqlDataReader reader = mSelectErcotPnlFeeCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            Dictionary<int, PnlFee> pnlFeeHash = new Dictionary<int, PnlFee>();
                            DateTime date = reader.GetDateTime(0);
                            PnlFee pnlFee = new PnlFee();
                            pnlFee.Fee = reader.IsDBNull(1) ? 0 : (double)reader.GetDecimal(1);
                            pnlFee.Pnl = reader.IsDBNull(2) ? 0 : (double)reader.GetDecimal(2);
                            pnlFee.DollarsCleared = reader.IsDBNull(3) ? 0 : (double)reader.GetDecimal(3);
                            pnlFee.MW = reader.IsDBNull(4) ? 0 : (double)reader.GetDecimal(4); ;
                            if (dateHash.ContainsKey(date))
                            {
                                pnlFeeHash = dateHash[date];
                                dateHash.Remove(date);
                            }
                            if (pnlFeeHash.ContainsKey(portfolioKey))
                            {
                                pnlFee.Fee += pnlFeeHash[portfolioKey].Fee;
                                pnlFee.Pnl += pnlFeeHash[portfolioKey].Pnl;
                                pnlFee.DollarsCleared += pnlFeeHash[portfolioKey].DollarsCleared;
                                pnlFee.MW += pnlFeeHash[portfolioKey].MW;
                                pnlFeeHash.Remove(portfolioKey);
                            }
                            pnlFeeHash.Add(portfolioKey, pnlFee);
                            dateHash.Add(date, pnlFeeHash);
                        }
                        reader.Close();
                        DateTime tempDt = endDate.AddDays(1);

                        reader.Close();

                    }
                    else if (market == "ERCOT External")
                    {
                        mSelectErcotExternalPnlFeeCommand.Parameters["@start"].Value = startDate;
                        mSelectErcotExternalPnlFeeCommand.Parameters["@end"].Value = endDate.AddDays(0);
                        mSelectErcotExternalPnlFeeCommand.Parameters["@portfoliokey"].Value = portfolioKey;
                        SqlDataReader reader = mSelectErcotExternalPnlFeeCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            Dictionary<int, PnlFee> pnlFeeHash = new Dictionary<int, PnlFee>();
                            DateTime date = reader.GetDateTime(0);
                            PnlFee pnlFee = new PnlFee();
                            pnlFee.Fee = 0;
                            pnlFee.Pnl = reader.IsDBNull(1) ? 0 : (double)reader.GetDecimal(1);
                            pnlFee.DollarsCleared = reader.IsDBNull(2) ? 0 : (double)reader.GetDecimal(2);
                            pnlFee.MW = reader.IsDBNull(3) ? 0 : (double)reader.GetDecimal(3);
                            if (dateHash.ContainsKey(date))
                            {
                                pnlFeeHash = dateHash[date];
                                dateHash.Remove(date);
                            }
                            if (pnlFeeHash.ContainsKey(portfolioKey))
                            {
                                pnlFee.Fee += pnlFeeHash[portfolioKey].Fee;
                                pnlFee.Pnl += pnlFeeHash[portfolioKey].Pnl;
                                pnlFee.DollarsCleared += pnlFeeHash[portfolioKey].DollarsCleared;
                                pnlFee.MW += pnlFeeHash[portfolioKey].MW;
                                pnlFeeHash.Remove(portfolioKey);
                            }
                            pnlFeeHash.Add(portfolioKey, pnlFee);
                            dateHash.Add(date, pnlFeeHash);
                        }
                        reader.Close();
                        DateTime tempDt = endDate.AddDays(1);

                        reader.Close();

                    }
                }
                VayuDbConn.Close();
                return dateHash;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <summary>
        /// Gets the PNL constraints.
        /// </summary>
        /// <param name="PNLList">The PNL list.</param>
        /// <returns></returns>
        public List<PNLConstraints> GetPNLConstraints(List<Pnl> PNLList, bool SortChecked, DateTime startDate, DateTime endDate, int marketKey)
        {
            loadDBCommands();
            Dictionary<int, Dictionary<string, double>> mwExposureHash = new Dictionary<int, Dictionary<string, double>>();
            List<List<PNLConstraints>> constraintList = new List<List<PNLConstraints>>();
            Dictionary<int, PNLConstraints> resultHash = new Dictionary<int, PNLConstraints>();
            List<PNLConstraints> ResultList = new List<PNLConstraints>();
            if (VayuDbConn.State == System.Data.ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            if (PNLList == null || PNLList.Count == 0)
            {
                return null;
            }
            var tempList = PNLList.GroupBy(x => new { x.Sink, x.Source, x.MarketDate });
            Dictionary<int, Dictionary<int, Dictionary<string, double>>> hourHash =
                                    new Dictionary<int, Dictionary<int, Dictionary<string, double>>>();
            foreach (var temp in tempList)
            {
                var key = temp.Key;
                if (SortChecked)
                {
                    if (marketKey == 1)
                    {
                        mSelectPathDollarsCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
                                       "(F.Sensitivity - C.Sensitivity) * (D.Impact/12), D.hour " +
                                       "FROM RTMasterConstraint as A " +
                                       "INNER JOIN RTMasterVector as C on A.constraintRTNum = C.constraintRTNum " +
                                       "INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum " +
                                       "INNER JOIN RTMasterVector as F on A.constraintRTNum = F.ConstraintRTNum " +
                                       "WHERE D.date = CONVERT(date, @realTimeDate) " +
                                       "AND C.NodeKey = @sourceKey and F.NodeKey = @sinkKey order by A.constraintRTNum desc";
                        mSelectPathDollarsCommand.Parameters["@realTimeDate"].Value = temp.Key.MarketDate;
                        mSelectPathDollarsCommand.Parameters["@sourceKey"].Value = DBAccess.GetNodeFromName(temp.Key.Source, 1).NodeKey;
                        mSelectPathDollarsCommand.Parameters["@sinkKey"].Value = DBAccess.GetNodeFromName(temp.Key.Sink, 1).NodeKey;

                    }
                    else if (marketKey == 9)
                    {
                        mSelectPathDollarsCommand.CommandText = "SELECT distinct A.shiftfactor, A.constraintRTNum, A.MonitoredText, A.ContingencyText, " +
               "(F.Sensitivity - C.Sensitivity) * (D.Impact/12), D.hour " +
               "FROM  Vayu..RTMasterConstraint as A " +
               "INNER JOIN  Vayu..RTMasterVector as C on A.constraintRTNum = C.constraintRTNum " +
               "INNER JOIN  Vayu..RTImpact as D on A.constraintRTNum = D.constraintRTNum " +
               "INNER JOIN  Vayu..RTMasterVector as F on A.constraintRTNum = F.ConstraintRTNum " +
               "WHERE D.date = CONVERT(date, @realTimeDate) " +
               "AND C.NodeKey = @sourceKey and F.NodeKey = @sinkKey order by A.constraintRTNum desc";

                        mSelectPathDollarsCommand.Parameters["@realTimeDate"].Value = temp.Key.MarketDate;
                        mSelectPathDollarsCommand.Parameters["@sourceKey"].Value = DBAccess.GetNodeFromName(temp.Key.Source, 9).NodeKey;
                        mSelectPathDollarsCommand.Parameters["@sinkKey"].Value = DBAccess.GetNodeFromName(temp.Key.Sink, 9).NodeKey;


                    }
                }
                else
                {
                    if (marketKey == 1)
                    {
                        mSelectPathDollarsCommand.CommandText = "select a.shiftfactor , a.constraintrtnum , a.MonitoredText , a.ContingencyText , sink.Sensitivity - source.Sensitivity  from rtmasterconstraint a " +
                                                        " join rtmastervector source on a.constraintrtnum = source.constraintrtnum join rtmastervector sink on  a.constraintrtnum = sink.constraintrtnum  " +
                                                        "  INNER JOIN RTImpact as D on A.constraintRTNum = D.constraintRTNum   WHERE D.date = CONVERT(date, @realTimeDate ) and source.NodeKey = @sourceKey and sink.NodeKey = @sinkKey ";
                        mSelectPathDollarsCommand.Parameters["@realTimeDate"].Value = temp.Key.MarketDate;
                        mSelectPathDollarsCommand.Parameters["@sourceKey"].Value = DBAccess.GetNodeFromName(temp.Key.Source, 1).NodeKey;
                        mSelectPathDollarsCommand.Parameters["@sinkKey"].Value = DBAccess.GetNodeFromName(temp.Key.Sink, 1).NodeKey;

                    }
                    else if (marketKey == 9)
                    {
                        mSelectPathDollarsCommand.CommandText = "select a.shiftfactor , a.constraintrtnum , a.MonitoredText , a.ContingencyText ,  (sink.Sensitivity - source.Sensitivity ) / nullif(a.shiftfactor , 0)  from  Vayu..rtmasterconstraint a (nolock)" +
                                " join  Vayu..rtmastervector source (nolock) on a.constraintrtnum = source.constraintrtnum join  Vayu..rtmastervector sink (nolock) on  a.constraintrtnum = sink.constraintrtnum  " +
                                "  INNER JOIN  Vayu..RTImpact as D (nolock) on A.constraintRTNum = D.constraintRTNum   WHERE D.date = CONVERT(date, @realTimeDate ) and source.NodeKey = @sourceKey and sink.NodeKey = @sinkKey ";
                        mSelectPathDollarsCommand.Parameters["@realTimeDate"].Value = temp.Key.MarketDate;
                        mSelectPathDollarsCommand.Parameters["@sourceKey"].Value = DBAccess.GetNodeFromName(temp.Key.Source, 9).NodeKey;
                        mSelectPathDollarsCommand.Parameters["@sinkKey"].Value = DBAccess.GetNodeFromName(temp.Key.Sink, 9).NodeKey;

                    }
                }



                SqlDataReader reader = mSelectPathDollarsCommand.ExecuteReader();

                if (SortChecked)
                    FillConstraintHash(reader, temp.Key.Source, temp.Key.Sink, hourHash);
                else
                    FillConstraintHash(reader, temp.Key.Source, temp.Key.Sink, hourHash, false);
                reader.Close();
            }
            Dictionary<int, Dictionary<int, Sensitivity>> nodeSensitivityHash = GetSensitivity(startDate, endDate.AddDays(-1), SortChecked, marketKey);
            Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel model = new ConstraintContingencyHistory.ViewModels.MainWindowViewModel();
            model.DAChecked = false;
            model.FromDate = startDate;
            model.DateRangeCheckBoxChecked = true;
            model.ThroDate = endDate.AddDays(-1);
            model.GetConstraints(9);

            Dictionary<double, PNLConstraints> exposureHash = new Dictionary<double, PNLConstraints>();
            if (nodeSensitivityHash.Count > 0)
            {
                foreach (Pnl pnl in PNLList)
                //  Parallel.ForEach(PNLList,( pnl)=>
                {
                    Dictionary<int, PNLConstraints> constraintHash = new Dictionary<int, PNLConstraints>();
                    constraintHash = resultHash;

                    try
                    {
                        lock (lockobj)
                        {
                            // if (hourHash.ContainsKey(pnl.HE))
                            {
                                int sinkNode = 0;

                                string sourceSinkKey = pnl.Source + ":" + pnl.Sink;

                                sinkNode = pnl.Sink == null ? 0 : DBAccess.GetNodeFromName(pnl.Sink, marketKey).NodeKey;
                                int sourceNode = DBAccess.GetNodeFromName(pnl.Source, marketKey).NodeKey;
                                Dictionary<int, Dictionary<string, double>> constraintExposureHash = null;
                                if (SortChecked)
                                {
                                    if (hourHash.ContainsKey(pnl.HE))
                                        constraintExposureHash = hourHash[pnl.HE];
                                }
                                else
                                    constraintExposureHash = hourHash[pnl.HE];

                                if (nodeSensitivityHash.ContainsKey(sourceNode) && nodeSensitivityHash.ContainsKey(sinkNode))
                                {
                                    Dictionary<int, Sensitivity> sourceSensitivityHash = nodeSensitivityHash[sourceNode];
                                    Dictionary<int, Sensitivity> sinkSensitivityHash = sinkNode == 0 ? null : nodeSensitivityHash[sinkNode];
                                    List<int> constraintKeys = sourceSensitivityHash.Keys.ToList<int>();

                                    foreach (int constraintKey in constraintKeys)
                                    {
                                        List<PNLConstraints> pathDetailList = new List<PNLConstraints>();
                                        if ((sinkNode == 0 || sinkSensitivityHash.ContainsKey(constraintKey)) && (sourceSensitivityHash.ContainsKey(constraintKey)))
                                        {
                                            Sensitivity sourceSensitivity = sourceSensitivityHash[constraintKey];
                                            Sensitivity sinkSensitivity = sinkNode == 0 ? null : sinkSensitivityHash[constraintKey];
                                            double diff = sinkNode == 0 ? sourceSensitivity.SensitivityValue : sinkSensitivity.SensitivityValue - sourceSensitivity.SensitivityValue;
                                            if (SortChecked)
                                            {
                                                //  diff *= sourceSensitivity.DollarImpact;
                                            }
                                            PNLConstraints constraint = new PNLConstraints();
                                            Dictionary<string, double> sourceSinkHash = null;
                                            if (constraintExposureHash != null)
                                            {
                                                if (SortChecked)
                                                {
                                                    if (constraintExposureHash.ContainsKey(constraintKey))
                                                        sourceSinkHash = constraintExposureHash[constraintKey];
                                                }
                                                else
                                                {
                                                    sourceSinkHash = constraintExposureHash[constraintKey];
                                                }
                                                if (sourceSinkHash != null && sourceSinkHash.Count > 0)
                                                {
                                                    if (sourceSinkHash.ContainsKey(sourceSinkKey))
                                                    {
                                                        double exposure = 0;
                                                        if (marketKey == 1)
                                                            exposure = sourceSinkHash[sourceSinkKey] * Convert.ToDouble(pnl.MW);
                                                        else if (marketKey == 9)
                                                        {
                                                            exposure = (sourceSinkHash[sourceSinkKey] / sourceSensitivity.ShiftFactor) * Convert.ToDouble(pnl.MW);

                                                        }
                                                        if (!constraintHash.ContainsKey(constraintKey))
                                                        {
                                                            constraint.constraintNum = constraintKey;
                                                            constraint.monitoredName = sConstraintHash[constraintKey].monitoredName;
                                                            constraint.contingName = sConstraintHash[constraintKey].contingName;
                                                            constraintHash.Add(constraintKey, constraint);
                                                        }
                                                        constraint = constraintHash[constraintKey];

                                                        if (mPathDetailHash.ContainsKey(constraint.constraintNum))
                                                        {
                                                            pathDetailList = mPathDetailHash[constraint.constraintNum];
                                                            mPathDetailHash.Remove(constraint.constraintNum);
                                                        }
                                                        if (pnl.HE == 1)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he1Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he1Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE1;
                                                                    }
                                                                    else
                                                                        constraint.he1Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE1;
                                                                }
                                                                else
                                                                    constraint.he1Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE1;
                                                                //  constraint.he1Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he1Value == null)
                                                                {
                                                                    constraint.he1Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he1Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he1Value += pnl.MW * diff;
                                                            }

                                                        }
                                                        else if (pnl.HE == 2)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he2Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he2Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE2;
                                                                    }
                                                                    else
                                                                        constraint.he2Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE2;
                                                                }
                                                                else
                                                                    constraint.he2Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE2;
                                                                //  constraint.he2Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he2Value == null)
                                                                {
                                                                    constraint.he2Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he2Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he2Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 3)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he3Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he3Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE3;
                                                                    }
                                                                    else
                                                                        constraint.he3Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE3;
                                                                }
                                                                else
                                                                    constraint.he3Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE3;
                                                                //  constraint.he3Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he3Value == null)
                                                                {
                                                                    constraint.he3Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he3Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he3Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 4)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he4Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he4Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE4;
                                                                    }
                                                                    else
                                                                        constraint.he4Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE4;
                                                                }
                                                                else
                                                                    constraint.he4Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE4;
                                                                //  constraint.he4Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he4Value == null)
                                                                {
                                                                    constraint.he4Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he4Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he4Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 5)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he5Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he5Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE5;
                                                                    }
                                                                    else
                                                                        constraint.he5Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE5;
                                                                }
                                                                else
                                                                    constraint.he5Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE5;
                                                                //  constraint.he5Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he5Value == null)
                                                                {
                                                                    constraint.he5Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he5Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he5Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 6)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he6Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he6Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE6;
                                                                    }
                                                                    else
                                                                        constraint.he6Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE6;
                                                                }
                                                                else
                                                                    constraint.he6Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE6;
                                                                //  constraint.he6Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he6Value == null)
                                                                {
                                                                    constraint.he6Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he6Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he6Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 7)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he7Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he7Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE7;
                                                                    }
                                                                    else
                                                                        constraint.he7Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE7;
                                                                }
                                                                else
                                                                    constraint.he7Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE7;
                                                                //  constraint.he7Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he7Value == null)
                                                                {
                                                                    constraint.he7Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he7Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he7Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 8)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he8Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he8Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE8;
                                                                    }
                                                                    else
                                                                        constraint.he8Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE8;
                                                                }
                                                                else
                                                                    constraint.he8Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE8;
                                                                //  constraint.he8Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he8Value == null)
                                                                {
                                                                    constraint.he8Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he8Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he8Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 9)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he9Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he9Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE9;
                                                                    }
                                                                    else
                                                                        constraint.he9Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE9;
                                                                }
                                                                else
                                                                    constraint.he9Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE9;
                                                                //  constraint.he9Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he9Value == null)
                                                                {
                                                                    constraint.he9Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he9Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he9Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 10)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he10Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he10Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE10;
                                                                    }
                                                                    else
                                                                        constraint.he10Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE10;
                                                                }
                                                                else
                                                                    constraint.he10Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE10;
                                                                //  constraint.he10Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he10Value == null)
                                                                {
                                                                    constraint.he10Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he10Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he10Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 11)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he11Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he11Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE11;
                                                                    }
                                                                    else
                                                                        constraint.he11Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE11;
                                                                }
                                                                else
                                                                    constraint.he11Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE11;
                                                                //  constraint.he11Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he11Value == null)
                                                                {
                                                                    constraint.he11Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he11Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he11Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 12)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he12Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he12Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE12;
                                                                    }
                                                                    else
                                                                        constraint.he12Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE12;
                                                                }
                                                                else
                                                                    constraint.he12Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE12;
                                                                //  constraint.he12Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he12Value == null)
                                                                {
                                                                    constraint.he12Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he12Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he12Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 13)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he13Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he13Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE13;
                                                                    }
                                                                    else
                                                                        constraint.he13Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE13;
                                                                }
                                                                else
                                                                    constraint.he13Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE13;
                                                                //  constraint.he13Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he13Value == null)
                                                                {
                                                                    constraint.he13Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he13Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he13Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 14)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he14Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he14Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE14;
                                                                    }
                                                                    else
                                                                        constraint.he14Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE14;
                                                                }
                                                                else
                                                                    constraint.he14Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE14;
                                                                //  constraint.he14Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he14Value == null)
                                                                {
                                                                    constraint.he14Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he14Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he14Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 15)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he15Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he15Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE15;
                                                                    }
                                                                    else
                                                                        constraint.he15Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE15;
                                                                }
                                                                else
                                                                    constraint.he15Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE15;
                                                                //  constraint.he15Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he15Value == null)
                                                                {
                                                                    constraint.he15Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he15Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he15Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 16)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he16Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he16Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE16;
                                                                    }
                                                                    else
                                                                        constraint.he16Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE16;
                                                                }
                                                                else
                                                                    constraint.he16Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE16;
                                                                // constraint.he16Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he16Value == null)
                                                                {
                                                                    constraint.he16Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he16Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he16Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 17)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he17Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he17Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE17;
                                                                    }
                                                                    else
                                                                        constraint.he17Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE17;
                                                                }
                                                                else
                                                                    constraint.he17Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE17;
                                                                //  constraint.he17Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he17Value == null)
                                                                {
                                                                    constraint.he17Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he17Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he17Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 18)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he18Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he18Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE18;
                                                                    }
                                                                    else
                                                                        constraint.he18Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE18;
                                                                }
                                                                else
                                                                    constraint.he18Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE18;
                                                                //  constraint.he18Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he18Value == null)
                                                                {
                                                                    constraint.he18Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he18Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he18Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 19)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he19Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he19Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE19;
                                                                    }
                                                                    else
                                                                        constraint.he19Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE19;
                                                                }
                                                                else
                                                                    constraint.he19Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE19;
                                                                //  constraint.he19Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he19Value == null)
                                                                {
                                                                    constraint.he19Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he19Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he19Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 20)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he20Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he20Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE20;
                                                                    }
                                                                    else
                                                                        constraint.he20Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE20;
                                                                }
                                                                else
                                                                    constraint.he20Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE20;
                                                                //  constraint.he20Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he20Value == null)
                                                                {
                                                                    constraint.he20Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he20Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he20Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 21)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he21Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he21Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE21;
                                                                    }
                                                                    else
                                                                        constraint.he21Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE21;
                                                                }
                                                                else
                                                                    constraint.he21Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE21;
                                                                //  constraint.he21Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he21Value == null)
                                                                {
                                                                    constraint.he21Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he21Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he21Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 22)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he22Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he22Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE22;
                                                                    }
                                                                    else
                                                                        constraint.he22Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE22;
                                                                }
                                                                else
                                                                    constraint.he22Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE22;
                                                                //  constraint.he22Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he22Value == null)
                                                                {
                                                                    constraint.he22Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he22Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he22Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 23)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he23Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he23Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE23;
                                                                    }
                                                                    else
                                                                        constraint.he23Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE23;
                                                                }
                                                                else
                                                                    constraint.he23Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE23;
                                                                //  constraint.he23Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he23Value == null)
                                                                {
                                                                    constraint.he23Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he23Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he23Value += pnl.MW * diff;
                                                            }
                                                        }
                                                        else if (pnl.HE == 24)
                                                        {
                                                            if (SortChecked)
                                                            {
                                                                if (constraint.he24Value == null)
                                                                {
                                                                    if (double.IsNaN(exposure))
                                                                    {
                                                                        constraint.he24Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE24;
                                                                    }
                                                                    else
                                                                        constraint.he24Value = diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE24;
                                                                }
                                                                else
                                                                    constraint.he24Value += diff * Convert.ToDouble(pnl.MW) * model.ConstraintList.Where(x => x.ConstraintText == constraint.monitoredName && x.ContingencyText == constraint.contingName).ToList().FirstOrDefault().HE24;
                                                                //  constraint.he24Value += exposure * diff;
                                                            }
                                                            else
                                                            {
                                                                if (constraint.he24Value == null)
                                                                {
                                                                    constraint.he24Value = 0;
                                                                }
                                                                if (marketKey == 1)
                                                                    constraint.he24Value += pnl.MW * diff * sourceSensitivity.ShiftFactor;
                                                                else if (marketKey == 9)
                                                                    constraint.he24Value += pnl.MW * diff;
                                                            }
                                                        }

                                                    }
                                                }
                                            }
                                            if (!mPathDetailHash.ContainsKey(constraint.constraintNum))
                                                mPathDetailHash.Add(constraint.constraintNum, pathDetailList);

                                        }

                                    }

                                }
                            }
                        }


                    }

                    catch (Exception ex)
                    {

                    }
                }
                //});
            }
            VayuDbConn.Close();
            foreach (var item in mPathDetailHash)
            {
                int constraintId = item.Key;
                List<PNLConstraints> pathDetailList = item.Value;
                PNLConstraints hourlyPathTot = new PNLConstraints();
                hourlyPathTot.he1Value = pathDetailList.Sum(a => a.he1Value);
                hourlyPathTot.he2Value = pathDetailList.Sum(a => a.he2Value);
                hourlyPathTot.he3Value = pathDetailList.Sum(a => a.he3Value);
                hourlyPathTot.he4Value = pathDetailList.Sum(a => a.he4Value);
                hourlyPathTot.he5Value = pathDetailList.Sum(a => a.he5Value);
                hourlyPathTot.he6Value = pathDetailList.Sum(a => a.he6Value);
                hourlyPathTot.he7Value = pathDetailList.Sum(a => a.he7Value);
                hourlyPathTot.he8Value = pathDetailList.Sum(a => a.he8Value);
                hourlyPathTot.he9Value = pathDetailList.Sum(a => a.he9Value);
                hourlyPathTot.he10Value = pathDetailList.Sum(a => a.he10Value);
                hourlyPathTot.he11Value = pathDetailList.Sum(a => a.he11Value);
                hourlyPathTot.he12Value = pathDetailList.Sum(a => a.he12Value);
                hourlyPathTot.he13Value = pathDetailList.Sum(a => a.he13Value);
                hourlyPathTot.he14Value = pathDetailList.Sum(a => a.he14Value);
                hourlyPathTot.he15Value = pathDetailList.Sum(a => a.he15Value);
                hourlyPathTot.he16Value = pathDetailList.Sum(a => a.he16Value);
                hourlyPathTot.he17Value = pathDetailList.Sum(a => a.he17Value);
                hourlyPathTot.he18Value = pathDetailList.Sum(a => a.he18Value);
                hourlyPathTot.he19Value = pathDetailList.Sum(a => a.he19Value);
                hourlyPathTot.he20Value = pathDetailList.Sum(a => a.he20Value);
                hourlyPathTot.he21Value = pathDetailList.Sum(a => a.he21Value);
                hourlyPathTot.he22Value = pathDetailList.Sum(a => a.he22Value);
                hourlyPathTot.he23Value = pathDetailList.Sum(a => a.he23Value);
                hourlyPathTot.he24Value = pathDetailList.Sum(a => a.he24Value);
                hourlyPathTot.monitoredName = "Total";
                pathDetailList.Add(hourlyPathTot);
            }

            List<PNLConstraints> exposureFinalList = exposureHash.Values.ToList<PNLConstraints>();
            foreach (PNLConstraints item in exposureFinalList)
            {
                if (item.he1Value == null)
                {
                    item.he1Value = 0;
                }
                if (item.he2Value == null)
                {
                    item.he2Value = 0;
                }
                if (item.he3Value == null)
                {
                    item.he3Value = 0;
                }
                if (item.he4Value == null)
                {
                    item.he4Value = 0;
                }
                if (item.he5Value == null)
                {
                    item.he5Value = 0;
                }
                if (item.he6Value == null)
                {
                    item.he6Value = 0;
                }
                if (item.he7Value == null)
                {
                    item.he7Value = 0;
                }
                if (item.he8Value == null)
                {
                    item.he8Value = 0;
                }
                if (item.he9Value == null)
                {
                    item.he9Value = 0;
                }
                if (item.he10Value == null)
                {
                    item.he10Value = 0;
                }
                if (item.he11Value == null)
                {
                    item.he11Value = 0;
                }
                if (item.he12Value == null)
                {
                    item.he12Value = 0;
                }
                if (item.he13Value == null)
                {
                    item.he13Value = 0;
                }
                if (item.he14Value == null)
                {
                    item.he14Value = 0;
                }
                if (item.he15Value == null)
                {
                    item.he15Value = 0;
                }
                if (item.he16Value == null)
                {
                    item.he16Value = 0;
                }
                if (item.he17Value == null)
                {
                    item.he17Value = 0;
                }
                if (item.he18Value == null)
                {
                    item.he18Value = 0;
                }
                if (item.he19Value == null)
                {
                    item.he19Value = 0;
                }
                if (item.he20Value == null)
                {
                    item.he20Value = 0;
                }
                if (item.he21Value == null)
                {
                    item.he21Value = 0;
                }
                if (item.he22Value == null)
                {
                    item.he22Value = 0;
                }
                if (item.he23Value == null)
                {
                    item.he23Value = 0;
                }
                if (item.he24Value == null)
                {
                    item.he24Value = 0;
                }
                item.Total = item.he1Value + item.he2Value + item.he3Value + item.he4Value + item.he5Value + item.he6Value + item.he7Value + item.he8Value + item.he9Value + item.he10Value + item.he11Value +

item.he12Value + item.he13Value +
                               item.he14Value + item.he15Value + item.he16Value + item.he17Value + item.he18Value + item.he19Value + item.he20Value + item.he21Value + item.he22Value + item.he23Value + item.he24Value;
            }
            List<PNLConstraints> lstConstraint = resultHash.Values.ToList<PNLConstraints>();
            return lstConstraint;
        }
        /// <summary>
        /// Gets the node hash.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetNodeHash()
        {
            SqlCommand cmd = VayuDbConn.CreateCommand();
            Dictionary<string, int> dic = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            cmd.CommandText = "select * from Node";
            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dic.Add(reader[1].ToString(), (int)reader.GetInt32(0));
            }
            reader.Close();
            cmd.Connection.Close();
            return dic;
        }

        internal double GetTotalMw(int portfolioKey)
        {
            try
            {
                loadDBCommands();
                DateTime Startdate = DateTime.Today.Date;
                DateTime EndDate = DateTime.Today.Date.AddDays(1);
                SqlCommand cmd = VayuDbConn.CreateCommand();
                cmd.CommandText = "select SUM(ClearedMW) from ClearedEES where PortfolioKey = @PortfolioKey and MarketDateTime > @Startdate and MarketDateTime <= @EndDate";
                cmd.Parameters.AddWithValue("@PortfolioKey", portfolioKey);
                cmd.Parameters.AddWithValue("@Startdate", Startdate);
                cmd.Parameters.AddWithValue("@EndDate", EndDate);
                cmd.Connection = VayuDbConn;
                if (VayuDbConn.State == ConnectionState.Closed)
                    VayuDbConn.Open();
                double port = Convert.ToDouble(cmd.ExecuteScalar());
                return port;
            }
            catch
            {

                return 0.0;
            }
        }
        public Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivity(DateTime startDate, DateTime endDate, bool IsDollar, int marketKey)
        {
            loadDBCommands();
            Dictionary<int, Dictionary<int, Sensitivity>> nodeHash = new Dictionary<int, Dictionary<int, Sensitivity>>();
            VayuDbConn.Open();
            Dictionary<int, Tuple<string, string, double, double>> constraintContingencyHash = new Dictionary<int, Tuple<string, string, double, double>>();
            if (marketKey == 9)
            {
                mSelectConstraintContingecyCommand.CommandText = " select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor, marketdatetime, dollarimpact " +
                                                 " from   Vayu..RTMasterConstraint as A (nolock) inner join  Vayu..RTImpact as B on A.ConstraintRTNum= B.ConstraintRTNum " +
                                                 " where B.Date >= @start and B.Date <= @end";

            }
            else if (marketKey == 1)
            {
                mSelectConstraintContingecyCommand.CommandText = " select distinct MonitoredText, ContingencyText, A.ConstraintRTNum, shiftfactor, marketdatetime, dollarimpact " +
                                 " from  RTMasterConstraint as A (nolock) inner join RTImpact as B on A.ConstraintRTNum= B.ConstraintRTNum " +
                                 " where B.Date >= @start and B.Date <= @end";

            }
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
            if (marketKey == 9)
            {
                mSelectSenstivityCommand.CommandText = "select NodeKey,Sensitivity, constraintrtnum from  Vayu..RTMasterVector (nolock) where ConstraintRTNum in " +
                                       " (select ConstraintRTNum from   Vayu..RTImpact(nolock)  where RTImpact.Date >= @start and RTImpact.Date <=@end)";

            }
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
                //  sensitivity.SensitivityValue = tuple.Item3 == 0 ? sensivityValue : sensivityValue / tuple.Item3;
                sensitivity.SensitivityValue = tuple.Item3 == 0 ? sensivityValue : sensivityValue;
                sensitivity.ID = constraintNum;
                sensitivity.Constraint = tuple.Item1;
                sensitivity.Contingency = tuple.Item2;
                sensitivity.ShiftFactor = tuple.Item3;
                sensitivity.DollarImpact = tuple.Item4;
                sensitivityHash.Add(constraintNum, sensitivity);
                nodeHash.Add(node, sensitivityHash);
            }
            reader.Close();
            VayuDbConn.Close();
            return nodeHash;
        }
        private void FillConstraintHash(SqlDataReader reader, string source, string sink,
                                     Dictionary<int, Dictionary<int, Dictionary<string, double>>> hourHash)
        {
            try
            {
                while (reader.Read())
                {
                    int hour = Convert.ToInt16(reader.GetValue(5));
                    Dictionary<int, Dictionary<string, double>> constraintHash = new Dictionary<int, Dictionary<string, double>>();
                    if (hourHash.ContainsKey(hour))
                    {
                        constraintHash = hourHash[hour];
                        hourHash.Remove(hour);
                    }
                    int constraintNum = Convert.ToInt32(reader.GetValue(1));
                    Dictionary<string, double> sourceSinkHash = new Dictionary<string, double>();
                    if (constraintHash.ContainsKey(constraintNum))
                    {
                        sourceSinkHash = constraintHash[constraintNum];
                        constraintHash.Remove(constraintNum);
                    }
                    string sourceSinkKey = source + ":" + sink;
                    double sensitivity = reader.IsDBNull(4) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                    if (!sourceSinkHash.ContainsKey(sourceSinkKey))
                    {
                        sourceSinkHash.Add(sourceSinkKey, sensitivity);
                    }
                    constraintHash.Add(constraintNum, sourceSinkHash);
                    hourHash.Add(hour, constraintHash);
                    PNLConstraints constraint = new PNLConstraints();
                    if (!sConstraintHash.ContainsKey(constraintNum))
                    {
                        constraint.constraintNum = constraintNum;
                        constraint.monitoredName = GetConstraintName(reader.GetValue(2).ToString());
                        constraint.contingName = GetContingencyName(reader.GetValue(3).ToString());
                        sConstraintHash.Add(constraintNum, constraint);
                    }
                }
            }
            catch (Exception ex)
            {


            }
        }

        private void FillConstraintHash(SqlDataReader reader, string source, string sink,
                             Dictionary<int, Dictionary<int, Dictionary<string, double>>> hourHash, bool dollarChk)
        {
            Dictionary<int, Dictionary<string, double>> exposureHash = new Dictionary<int, Dictionary<string, double>>();
            while (reader.Read())
            {
                for (int i = 1; i < 25; i++)
                {
                    int hour = i;
                    Dictionary<int, Dictionary<string, double>> constraintHash = new Dictionary<int, Dictionary<string, double>>();
                    if (hourHash.ContainsKey(hour))
                    {
                        constraintHash = hourHash[hour];
                        hourHash.Remove(hour);
                    }
                    int constraintNum = Convert.ToInt32(reader.GetValue(1));
                    Dictionary<string, double> sourceSinkHash = new Dictionary<string, double>();
                    if (constraintHash.ContainsKey(constraintNum))
                    {
                        sourceSinkHash = constraintHash[constraintNum];
                        constraintHash.Remove(constraintNum);
                    }
                    string sourceSinkKey = source + ":" + sink;
                    double sensitivity = reader.IsDBNull(4) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                    if (!sourceSinkHash.ContainsKey(sourceSinkKey))
                    {
                        sourceSinkHash.Add(sourceSinkKey, sensitivity);
                    }
                    constraintHash.Add(constraintNum, sourceSinkHash);
                    hourHash.Add(hour, constraintHash);
                    PNLConstraints constraint = new PNLConstraints();
                    if (!sConstraintHash.ContainsKey(constraintNum))
                    {
                        constraint.constraintNum = constraintNum;
                        constraint.monitoredName = GetConstraintName(reader.GetValue(2).ToString());
                        constraint.contingName = GetContingencyName(reader.GetValue(3).ToString());
                        sConstraintHash.Add(constraintNum, constraint);
                    }
                }
            }
        }

        #endregion
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
