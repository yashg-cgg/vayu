using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.NodeSensitivityAnalysis.Model
{
    public class DataService : IDataService
    {
        #region SQL Commands

        /// <summary>
        /// The m select PJM distinct constraint command
        /// </summary>
        SqlCommand mSelectPJMDistinctConstraintCmd;
        SqlCommand mSelectERCOTDistinctConstraintCmd;
        /// <summary>
        /// The m select pjmda distinct constraint command
        /// </summary>
        SqlCommand mSelectPJMDADistinctConstraintCmd;

        SqlCommand mSelectERCOTDAContingencyCmd;

        /// <summary>
        /// The m select PJM contingency command
        /// </summary>
        SqlCommand mSelectPJMContingencyCmd;
        SqlCommand mSelectERCOTContingencyCmd;
        /// <summary>
        /// The m select pjmda contingency command
        /// </summary>
        SqlCommand mSelectPJMDAContingencyCmd;
        /// <summary>
        /// The m select PJM historic constraint data
        /// </summary>
        SqlCommand mSelectPJMHistoricConstraintData;
        SqlCommand mSelectERCOTHistoricConstraintData;
        SqlCommand mSelectDAERCOTMHistoricConstraintData;
        SqlCommand mSelectErcotDADistinctConstraintCmd;
        private SqlCommand mSelectloadDailyCommand;
        private SqlCommand mSelectloadHourlyCommand;

        private SqlCommand mSelectZoneCommand;

        private SqlCommand mGetMaxShadowPriceCommand;
        #endregion

        /// <summary>
        /// The m average hash
        /// </summary>
        private Dictionary<string, Dictionary<DateTime, double>> mAvgHash = null;
        public Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
        #region Public Methods

        public void GetConstraintData(Action<System.Collections.Generic.List<Constraint>, Exception> callback, int marketKey, bool isDa, DateTime fromDate, DateTime? throDate = null)
        {
            List<Constraint> tempList = FillAvgHash(marketKey, isDa, fromDate, throDate);

            callback(tempList.OrderByDescending(a => a.Price).ThenByDescending(a => a.ConstraintDate).ToList(), null);
        }

        public List<Constraint> FillAvgHashByService(int marketKey, bool isDa, DateTime fromDate, DateTime? throDate)
        {

            List<Constraint> tempList = new List<Constraint>();
            ConstraintHelper helper = new ConstraintHelper();
            IConstraintInfoProvider chelper = helper.GetInstance();
            List<LatestConstraint> constraintList = null;

            throDate = (throDate.HasValue ? throDate.Value.AddDays(1) : fromDate.AddDays(1));

            if (isDa)
                fromDate = fromDate.AddHours(1);

            if (isDa)
                constraintList = chelper.GetConstraintDA(marketKey, Configuration.GetAbsoluteDate(fromDate), Configuration.GetAbsoluteDate(throDate.Value));
            else
                constraintList = chelper.GetConstraintRT(marketKey, Configuration.GetAbsoluteDate(fromDate), Configuration.GetAbsoluteDate(throDate.Value), false);

            mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in constraintList)
            {
                try
                {
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;
                    if (contingency == "DGT_HOC8")
                    {
                        if (constraint == "GG_TAP91_1/GG-GG/138-138")
                        {

                        }
                    }

                    DateTime mktDate;
                    if (isDa)
                        mktDate = item.MarketDate.AddHours(-1);
                    else
                        mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    int sourceNodekey = item.SourceNodeKey;
                    int sinkNodekey = item.SinkNodeKey;
                    string constName = constraint + "?" + contingency;

                    Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new Constraint
                        {
                            ConstraintDate = mktDate,
                            ContingencyText = contingency,
                            ConstraintText = constraint,
                            SourceNodekey = sourceNodekey,
                            SinkNodekey = sinkNodekey,

                        };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (!isDa)
                            {
                                if (!double.IsNaN(item.ShadowPriceNaN))
                                    hourValues[mktDate] += hourValues[mktDate] + Math.Abs(item.ShadowPrice);
                            }
                        }
                        else
                        {
                            hourValues.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        }
                    }
                    else
                    {
                        Dictionary<DateTime, double> tempHash = new Dictionary<DateTime, double>();
                        tempHash.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        mAvgHash.Add(keyText, tempHash);
                    }
                    if (tempList.Contains(constraintItem))
                        tempList.Remove(constraintItem);
                    tempList.Add(constraintItem);
                }
                catch
                {
                    continue;
                }
            }
            return tempList;
        }

        internal List<Constraint> GetErcotSensitivitiesData(bool isDA, string constraintName, string contingencyName, string sourcenode, string sinknode, DateTime date)
        {
            List<Constraint> senSitivityList1 = new List<Constraint>();

            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Connection = con;
                        if (sourcenode != null && sinknode == null)
                        {
                            if (isDA)
                            {
                                cmd.CommandText =
" select b.MonitoredText , b.ContingencyText , a.Sensitivity  " +
 "from DAMasterVector a join DAMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey   " +
 "where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "' and c.NodeName='" + sourcenode + "'";


                            }
                            else
                            {
                                cmd.CommandText = "select b.MonitoredText , b.ContingencyText , a.Sensitivity " +
     "from RTMasterVector_new a join RTMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey  " +
    " where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "' and c.NodeName='" + sourcenode + "'";
                            }
                        }
                        else
                        {

                            if (isDA)
                            {
                                cmd.CommandText = " WITH cte AS(select b.MonitoredText , b.ContingencyText , a.Sensitivity " +
                              " from DAMasterVector a join DAMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey  " +
                              " where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "' and c.NodeName='" + sourcenode + "'), cte2 AS " +
                                "(select b.MonitoredText , b.ContingencyText , a.Sensitivity " +
                               "from DAMasterVector a join DAMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey  " +
                              " where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "' and c.NodeName='" + sinknode + "' )  " +
                               " SELECT  c.MonitoredText, c.ContingencyText , c2.Sensitivity -c.Sensitivity as Sensitivity  " +
                               "FROM cte c FULL JOIN cte2 c2 ON c.MonitoredText = c2.MonitoredText  and c.ContingencyText=c2.ContingencyText";
                            }
                            else
                            {
                                cmd.CommandText = cmd.CommandText = " WITH cte AS(select b.MonitoredText , b.ContingencyText , a.Sensitivity " +
                               " from RTMasterVector_new a join RTMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey  " +
                               " where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "' and c.NodeName='" + sourcenode + "'), cte2 AS " +
                                 "(select b.MonitoredText , b.ContingencyText , a.Sensitivity " +
                                "from RTMasterVector_new a join RTMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey  " +
                               " where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "' and c.NodeName='" + sinknode + "' )  " +
                                " SELECT  c.MonitoredText, c.ContingencyText , c2.Sensitivity -c.Sensitivity as Sensitivity  " +
                                "FROM cte c FULL JOIN cte2 c2 ON c.MonitoredText = c2.MonitoredText  and c.ContingencyText=c2.ContingencyText";
                            }

                        }

                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        SqlDataReader rdr1 = cmd.ExecuteReader();
                        while (rdr1.Read())
                        {
                            Constraint helper = new Constraint();
                            helper.ConstraintDate = date;
                            helper.ConstraintText = rdr1.IsDBNull(0) ? "" : rdr1.GetValue(0).ToString();
                            helper.ContingencyText = rdr1.IsDBNull(1) ? "" : rdr1.GetValue(1).ToString();
                            helper.Sensitivity = rdr1.IsDBNull(2) ? 0 : Convert.ToDouble(rdr1.GetValue(2));
                            senSitivityList1.Add(helper);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return senSitivityList1;
        }

        private List<Constraint> FillAvgHash(int marketKey, bool isDa, DateTime fromDate, DateTime? throDate)
        {
            return FillAvgHashByService(marketKey, isDa, fromDate, throDate);

            List<Constraint> tempList = new List<Constraint>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (marketKey == 9)
                        {
                            cmd.CommandText = isDa ? "select ConstraintName,ContingencyName,MarketDateTime,MarginalValue from MarginalValueDA(nolock)  where MarketKey=9 and MarketDateTime>=@start and MarketDateTime<@end "
                                                   : "select ConstraintText,ContingencyText,MarketDateTime,ShadowPrice from ercot..ConstraintRT(nolock) where MarketDateTime>=@start and MarketDateTime<@end ";
                        }

                        cmd.Parameters.AddWithValue("@start", isDa ? fromDate.AddHours(1) : fromDate);
                        cmd.Parameters.AddWithValue("@end", isDa ? throDate.HasValue ? throDate.Value.AddDays(1) : fromDate.AddDays(1).AddHours(1) : (throDate.HasValue ? throDate.Value.AddDays(1) : fromDate.AddDays(1).AddHours(1)));
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        IDataReader reader = cmd.ExecuteReader();
                        mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                        while (reader.Read())
                        {
                            try
                            {
                                string constraint = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString();
                                string contingency = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString();
                                DateTime mktDate = reader.IsDBNull(2) ? new DateTime() : Convert.ToDateTime(reader.GetValue(2));
                                if (mktDate.Hour == 0 && isDa)
                                    mktDate = mktDate.AddMinutes(-1);
                                double prc = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3));
                                string keyText = constraint + "?split?" + contingency;
                                Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                                if (constraintItem == null)
                                {
                                    constraintItem = new Constraint { ConstraintDate = mktDate, ContingencyText = contingency, ConstraintText = constraint };
                                }
                                if (mAvgHash.ContainsKey(keyText))
                                {
                                    Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                                    if (hourValues.ContainsKey(mktDate))
                                    {
                                        if (!reader.IsDBNull(3))
                                            hourValues[mktDate] += hourValues[mktDate] + Math.Abs((Convert.ToDouble(reader.GetValue(3))));
                                    }
                                    else
                                    {
                                        hourValues.Add(mktDate, reader.IsDBNull(3) ? 0 : Math.Abs(Convert.ToDouble(reader.GetValue(3))));
                                    }
                                }
                                else
                                {
                                    Dictionary<DateTime, double> tempHash = new Dictionary<DateTime, double>();
                                    tempHash.Add(mktDate, reader.IsDBNull(3) ? 0 : Math.Abs(Convert.ToDouble(reader.GetValue(3))));
                                    mAvgHash.Add(keyText, tempHash);
                                }
                                if (tempList.Contains(constraintItem))
                                    tempList.Remove(constraintItem);
                                tempList.Add(constraintItem);
                            }
                            catch
                            {
                            }
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                }
                catch
                {
                }
            }

            return tempList;
        }

    }

}
#endregion