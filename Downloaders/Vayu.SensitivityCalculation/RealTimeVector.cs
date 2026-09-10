//#define TEST
#define MANUAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Vayu.NodePriceLibrary;
using System.ServiceModel;
using System.ServiceModel.Description;
using Extreme.Mathematics.LinearAlgebra;
using Extreme.Mathematics;
using System.Timers;
using Vayu.CommonAccessLibrary;

namespace Vayu.SensitivityCalculation
{
    public class RealTimeVector
    {
        private string mMarket;
        private SqlConnection VayuDBConnection;
        
        private SqlCommand mDeleteErcotMasterConstraintCommand;
        private SqlCommand mSelectErcotConstraintMasterCommand;
        private SqlCommand mSelectErcotConstraintCountCommand;
        private SqlCommand mSelectConstraintCongestionCountCommand;
        private SqlCommand mSelectErcotConstraintCommand;
        private SqlCommand mSelectErcotConstraintIDCommand;
        private SqlCommand mInsertErcotConstraintCommand;
        private SqlCommand mUpdateErcotConstraintCommand;
        private SqlCommand mDeleteErcotVectorCommand;
        private SqlCommand mSelectErcotSensitivityCommand;
        private SqlCommand mSelectErcotVectorCountCommand;
        private SqlCommand mDeleteErcotHourlyImpactCommand;
        private SqlCommand mInsertErcotHourlyImpactCommand;
        private SqlCommand mUpdateErcotImpactsCommand;
        private SqlCommand mSelectErcotMaxConstraintDateCommand;
        private SqlCommand mSelectErcotMaxConstraintCommand;
        private SqlCommand mSelectErcotNumFireCommand;
        private SqlCommand mSelectErcotOrphanVectorCommand;
        private SqlCommand mSelectErcotOrphanImpactCommand;
        private SqlCommand mDeleteErcotOrphanVectorCommand;
        private SqlCommand mDeleteErcotOrphanImpactCommand;
        private SqlCommand mSelectManualErcotConstraintImpactCommand;
        private SqlCommand mSelectManualErcotDatetimeCommand;
        private Dictionary<string, Node[]> mPriceHash = new Dictionary<string, Node[]>();
        private System.Timers.Timer mTimer = new System.Timers.Timer();
        private bool mFirstTime = true;
        private DateTime mStartDate;
        private DateTime mEndDate;
        private DataTable mVectorTable = new DataTable();
        private List<DateTime> mNotFoundDateTimeList = new List<DateTime>();
        private Dictionary<string, double> mSensitivityHash = new Dictionary<string, double>();
        static string sDartEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress();
        int runCount = 0;

        public RealTimeVector(string market)
        {
            
            mMarket = market;
        }
        private void InitDB()
        {


            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            //
            mSelectErcotNumFireCommand = new SqlCommand();
            mSelectErcotNumFireCommand.CommandText = "select MarketDateTime, COUNT(*) from  ConstraintRT (nolock) where abs(shadowprice) > @shadowprice and MarketDateTime in (select MarketDateTime " +
                                                    "from  ConstraintRT (nolock) where ContingencyText = @ContingencyText and ConstraintText = @ConstraintText) and marketdatetime > (select MIN(marketdatetime) " +
                                                    "from  NodeLMP (nolock) where nodekey = 1) group by MarketDateTime order by MarketDateTime desc";
            mSelectErcotNumFireCommand.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
            mSelectErcotNumFireCommand.Parameters.AddWithValue("@ConstraintText", "ConstraintText");
            mSelectErcotNumFireCommand.Parameters.AddWithValue("@shadowprice", "shadowprice");
            mSelectErcotNumFireCommand.Connection = VayuDBConnection;

            //

            mDeleteErcotVectorCommand = new SqlCommand();
            mDeleteErcotVectorCommand.CommandText = "delete RTMasterVector where constraintrtnum = @constraintrtnum";
            mDeleteErcotVectorCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mDeleteErcotVectorCommand.Connection = VayuDBConnection;

            //

            mDeleteErcotMasterConstraintCommand = new SqlCommand();
            mDeleteErcotMasterConstraintCommand.CommandText = "delete RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext";
            mDeleteErcotMasterConstraintCommand.Parameters.AddWithValue("@monitoredtext", "monitoredtext");
            mDeleteErcotMasterConstraintCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
            mDeleteErcotMasterConstraintCommand.Connection = VayuDBConnection;


            mSelectErcotMaxConstraintDateCommand = new SqlCommand();
            mSelectErcotMaxConstraintDateCommand.CommandText = "select max(marketdatetime) from RTMasterConstraint";
            mSelectErcotMaxConstraintDateCommand.Connection = VayuDBConnection;

            //

            mDeleteErcotHourlyImpactCommand = new SqlCommand();
            mDeleteErcotHourlyImpactCommand.CommandText = "delete RTImpact where date =  Convert(Date, @Time) and hour = @Hour";
            mDeleteErcotHourlyImpactCommand.Parameters.AddWithValue("@Time", "Time");
            mDeleteErcotHourlyImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
            mDeleteErcotHourlyImpactCommand.Connection = VayuDBConnection;

            //

            mInsertErcotHourlyImpactCommand = new SqlCommand();
            mInsertErcotHourlyImpactCommand.CommandText = "INSERT INTO RTImpact(ConstraintRTNum, Date, Hour, ShadowPrice) " +
            "SELECT B.ConstraintRTNum, Convert(Date, @start) as Date, @Hour as Hour, abs(sum(A.ShadowPrice)) as ShadowPrice from pjm.ConstraintRT as A inner join " +
            "RTMasterConstraint as B on A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  where A.MarketDateTime >= @start and " +
            "A.MarketDateTime < @end and ConstraintText <> 'None' group by ConstraintText, A.ContingencyText, B.ConstraintRTNum";
            mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@start", "a.marketdatetime");
            mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@end", "a.marketdatetime");
            mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
            mInsertErcotHourlyImpactCommand.Connection = VayuDBConnection;



            //

            mUpdateErcotImpactsCommand = new SqlCommand();
            mUpdateErcotImpactsCommand.CommandText = "Update RTImpact set impact = A.shadowprice*B.shiftfactor from RTImpact A inner join RTMasterConstraint B on A.ConstraintRTNum = B.ConstraintRTNum " +
            "where date = Convert(Date, @Time) and hour = @Hour and B.Shiftfactor is not null ";
            mUpdateErcotImpactsCommand.Parameters.AddWithValue("@Time", "Time");
            mUpdateErcotImpactsCommand.Parameters.AddWithValue("@Hour", "Hour");
            mUpdateErcotImpactsCommand.Connection = VayuDBConnection;

            SqlCommand selectVectorCommand = new SqlCommand();
            selectVectorCommand.CommandText = "SELECT top 1 a.ConstraintRTNum, a.NodeKey, a.Sensitivity, a.SensitivityNormal FROM RTMasterVector a";
            selectVectorCommand.Connection = VayuDBConnection;

            //
            SqlDataAdapter sqladapter = new SqlDataAdapter();
            sqladapter.SelectCommand = selectVectorCommand;
            sqladapter.Fill(mVectorTable);
            //

            mSelectErcotConstraintIDCommand = new SqlCommand();
            mSelectErcotConstraintIDCommand.CommandText = "select constraintrtnum, marketdatetime, numberconstraints from RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext";
            mSelectErcotConstraintIDCommand.Parameters.AddWithValue("@monitoredtext", "monitoredtext");
            mSelectErcotConstraintIDCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
            mSelectErcotConstraintIDCommand.Connection = VayuDBConnection;

            mSelectErcotMaxConstraintCommand = new SqlCommand();
            mSelectErcotMaxConstraintCommand.CommandText = "SELECT max(ConstraintRTNum) from RTMasterConstraint";
            mSelectErcotMaxConstraintCommand.Connection = VayuDBConnection;



            mInsertErcotConstraintCommand = new SqlCommand();
            mInsertErcotConstraintCommand.CommandText = "insert RTMasterConstraint values (@constraintrtnum, @MarketDateTime, @MonitoredText, @ContingencyText, @ShadowPrice, @NumberConstraints, @ShiftFactor, " +
                "0, 1, @DollarImpact, @UpdateTime , 'Vector')";
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", "MonitoredText");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@ShadowPrice", "ShadowPrice");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@NumberConstraints", "NumberConstraints");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@ShiftFactor", "ShiftFactor");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@DollarImpact", "DollarImpact");
            mInsertErcotConstraintCommand.Parameters.AddWithValue("@UpdateTime", "UpdateTime");
            mInsertErcotConstraintCommand.Connection = VayuDBConnection;

            //

            mUpdateErcotConstraintCommand = new SqlCommand();
            mUpdateErcotConstraintCommand.CommandText = "update RTMasterConstraint set marketdatetime = @marketdatetime, shadowprice = @shadowprice, numberconstraints = @numberconstraints, " +
                "shiftfactor = @shiftfactor, dollarimpact = @dollarimpact, updatetime = @updatetime where monitoredtext = @MonitoredText and contingencytext = @ContingencyText";
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shadowprice", "shadowprice");
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@numberconstraints", "numberconstraints");
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shiftfactor", "shiftfactor");
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@dollarimpact", "dollarimpact");
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@updatetime", "updatetime");
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", "MonitoredText");
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
            mUpdateErcotConstraintCommand.Connection = VayuDBConnection;

            //
            mSelectErcotConstraintCommand = new SqlCommand();
            mSelectErcotConstraintCommand.CommandText = "select constrainttext, contingencytext, shadowprice from constraintrt where marketdatetime = @marketdatetime " +
                                                    "and shadowprice is not null and shadowprice <> 0";
            mSelectErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mSelectErcotConstraintCommand.Connection = VayuDBConnection;
            //
            mSelectErcotConstraintCountCommand = new SqlCommand();
            mSelectErcotConstraintCountCommand.CommandText = "select MarketDateTime, COUNT(*) a from constraintrt where MarketDateTime > @start and MarketDateTime <= @end and shadowprice <> 0 " +
                                                            "and shadowprice is not null group by marketdatetime   order by a, marketdatetime desc";
            mSelectErcotConstraintCountCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectErcotConstraintCountCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectErcotConstraintCountCommand.Connection = VayuDBConnection;
            //
            mSelectConstraintCongestionCountCommand = new SqlCommand();
            mSelectConstraintCongestionCountCommand.CommandText = "select MarketDateTime from constraintrt where MarketDateTime <> @marketdatetime and shadowprice <> 0" +
                                                  " and shadowprice is not null and constrainttext = @constraintname and contingencytext = @contingencyname order by marketdatetime desc";
            mSelectConstraintCongestionCountCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mSelectConstraintCongestionCountCommand.Parameters.AddWithValue("@constraintname", "constraintname");
            mSelectConstraintCongestionCountCommand.Parameters.AddWithValue("@contingencyname", "contingencyname");
            mSelectConstraintCongestionCountCommand.Connection = VayuDBConnection;
            //

            mSelectErcotSensitivityCommand = new SqlCommand();
            mSelectErcotSensitivityCommand.CommandText = "select sensitivity from RTMasterVector (NOLOCK) where nodekey = @nodekey and constraintrtnum = " +
                                                    "(select constraintrtnum from RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext)";
            mSelectErcotSensitivityCommand.Parameters.AddWithValue("@monitoredtext", "monitoredtext");
            mSelectErcotSensitivityCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
            mSelectErcotSensitivityCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectErcotSensitivityCommand.Connection = VayuDBConnection;


            mSelectErcotConstraintMasterCommand = new SqlCommand();
            mSelectErcotConstraintMasterCommand.CommandText = "select marketdatetime, monitoredtext, contingencytext, shadowprice, numberconstraints, shiftfactor, dollarimpact " +
                                                            "from RTMasterConstraint where marketdatetime > dateadd(month, -8, getdate())";
            mSelectErcotConstraintMasterCommand.Connection = VayuDBConnection;

            //
            mSelectErcotVectorCountCommand = new SqlCommand();
            mSelectErcotVectorCountCommand.CommandText = "select count(*) from RTMasterVector where constraintrtnum = (select constraintrtnum from RTMasterConstraint " +
                                                        "where monitoredtext = @monitoredtext and contingencytext = @contingencytext )";
            mSelectErcotVectorCountCommand.Parameters.AddWithValue("@monitoredtext", "monitoredtext");
            mSelectErcotVectorCountCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
            mSelectErcotVectorCountCommand.Connection = VayuDBConnection;


            mSelectErcotOrphanVectorCommand = new SqlCommand();
            mSelectErcotOrphanVectorCommand.CommandText = "select distinct constraintrtnum from RTMasterVector where ConstraintRTNum not in (select ConstraintRTNum from RTMasterConstraint)";
            mSelectErcotOrphanVectorCommand.Connection = VayuDBConnection;

            //
            mSelectErcotOrphanImpactCommand = new SqlCommand();
            mSelectErcotOrphanImpactCommand.CommandText = "select distinct constraintrtnum from RTImpact where ConstraintRTNum not in (select ConstraintRTNum from RTMasterConstraint)";
            mSelectErcotOrphanImpactCommand.Connection = VayuDBConnection;

            //
            mDeleteErcotOrphanVectorCommand = new SqlCommand();
            mDeleteErcotOrphanVectorCommand.CommandText = "delete RTMasterVector where constraintrtnum = @constraintrtnum";
            mDeleteErcotOrphanVectorCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mDeleteErcotOrphanVectorCommand.Connection = VayuDBConnection;

            //

            mDeleteErcotOrphanImpactCommand = new SqlCommand();
            mDeleteErcotOrphanImpactCommand.CommandText = "delete RTImpact where constraintrtnum = @constraintrtnum";
            mDeleteErcotOrphanImpactCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mDeleteErcotOrphanImpactCommand.Connection = VayuDBConnection;


            //
            mSelectManualErcotConstraintImpactCommand = new SqlCommand();
            mSelectManualErcotConstraintImpactCommand.CommandText = "select distinct ConstraintText , ContingencyText from ConstraintRT"
                                                                  + " where MarketDateTime = @EndDate ";
            // mSelectManualPJMConstraintImpactCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectManualErcotConstraintImpactCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectManualErcotConstraintImpactCommand.Connection = VayuDBConnection;

            //Manual
            mSelectManualErcotDatetimeCommand = new SqlCommand();
            mSelectManualErcotDatetimeCommand.CommandText = "select distinct cast(marketdatetime as date) as date from ConstraintRT where ConstraintText =@ConstraintText"
                                                           + " and ContingencyText=@ContingencyText";
            mSelectManualErcotDatetimeCommand.Parameters.AddWithValue("@ConstraintText", "ConstraintText");
            mSelectManualErcotDatetimeCommand.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
            mSelectManualErcotDatetimeCommand.Connection = VayuDBConnection;
        }
        private void DeleteOrphans()
        {
            Console.WriteLine("Deleting Orphans");
            List<int> orphanList = new List<int>();
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
            SqlDataReader reader =  mSelectErcotOrphanVectorCommand.ExecuteReader();
            while (reader.Read())
            {
                orphanList.Add((int)reader.GetDecimal(0));
            }
            reader.Close();
            reader =  mSelectErcotOrphanImpactCommand.ExecuteReader();
            while (reader.Read())
            {
                int rtConstraintNum = (int)reader.GetDecimal(0);
                if (!orphanList.Contains(rtConstraintNum))
                {
                    orphanList.Add(rtConstraintNum);
                }
            }
            reader.Close();
            SqlCommand sqlVectorCommand =  mDeleteErcotOrphanVectorCommand;
            SqlCommand sqlImpactCommand =  mDeleteErcotOrphanImpactCommand;
            foreach (int constraintRtNum in orphanList)
            {
                sqlVectorCommand.Parameters["@constraintrtnum"].Value = constraintRtNum;
                sqlVectorCommand.ExecuteNonQuery();
                sqlImpactCommand.Parameters["@constraintrtnum"].Value = constraintRtNum;
                sqlImpactCommand.ExecuteNonQuery();
            }
            VayuDBConnection.Close();
            Console.WriteLine("Deleted Orphans");
        }
        public int CreateNewConID()
        {
            int constraintId = -1;
            if (VayuDBConnection.State == ConnectionState.Closed)
            {
                VayuDBConnection.Open();
            }
            using (SqlDataReader reader = mSelectErcotMaxConstraintCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        constraintId = 1;
                    }
                    else
                    {
                        constraintId = Convert.ToInt32(reader.GetValue(0));
                        constraintId = constraintId + 1;
                    }
                }
            }
            return constraintId;
        }
        private void InsertVector(List<Element> elementlist, double shiftfactor, int constraintID, string constraintName, string contingencyName)
        {
            mVectorTable.Rows.Clear();
            List<int> keyList = new List<int>();
            string sensKey = constraintName + "?" + contingencyName;
            foreach (Element elm in elementlist)
            {
                if (!elm.SensitivityHash.ContainsKey(sensKey))
                {
                    continue;
                }
                DataRow row = mVectorTable.NewRow();
                row["ConstraintRTNum"] = constraintID;
                row["NodeKey"] = elm.Nodekey;
                if (Math.Abs(shiftfactor) > 0.05)
                {
                    row["Sensitivity"] = elm.SensitivityHash[sensKey] / shiftfactor;
                }
                else
                {
                    row["Sensitivity"] = 0;
                }
                row["SensitivityNormal"] = 0;
                mVectorTable.Rows.Add(row);
            }
            if (mVectorTable.Rows.Count == 0)
            {
                return;
            }
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
            SqlCommand deleteVectorCommand =  mDeleteErcotVectorCommand;
            deleteVectorCommand.Parameters["@constraintrtnum"].Value = constraintID;
            deleteVectorCommand.ExecuteNonQuery();
            try
            {
                Console.WriteLine(DateTime.Now + " Inside Bulk Copy Vector");


                SqlBulkCopy bulkCopy = new SqlBulkCopy(VayuDBConnection);
                bulkCopy.BulkCopyTimeout = 60 * 10;
                bulkCopy.DestinationTableName =  "RTMasterVector";

                bulkCopy.WriteToServer(mVectorTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine(DateTime.Now + " Outside Bulk Copy Vector");
            VayuDBConnection.Close();
        }
        private int InsertConstraint(List<ConstraintElement> constraintElementList, List<Element> elementList)
        {
            SqlCommand updateConstraintCommand = mUpdateErcotConstraintCommand;
            SqlCommand insertConstraintCommand = mInsertErcotConstraintCommand;
            SqlCommand selectConstraintIDCommand =  mSelectErcotConstraintIDCommand;
            try
            {
                foreach (ConstraintElement constraintElement in constraintElementList)
                {
                    if (VayuDBConnection.State == ConnectionState.Open)
                    {
                        VayuDBConnection.Close();
                    }
                    VayuDBConnection.Open();
                    int constraintID = 0;
                    DateTime marketDatetime = DateTime.MinValue;
                    selectConstraintIDCommand.Parameters["@monitoredtext"].Value = constraintElement.Monitor;
                    selectConstraintIDCommand.Parameters["@contingencytext"].Value = constraintElement.Contingency;
                    SqlDataReader reader = selectConstraintIDCommand.ExecuteReader();
                    int numFire = 0;
                    while (reader.Read())
                    {
                        constraintID = Convert.ToInt32(reader.GetValue(0));
                        marketDatetime = reader.GetDateTime(1);
                        numFire = Convert.ToInt32(reader.GetValue(0));
                    }
                    reader.Close();
                    Console.WriteLine("Insert constraint num fire " + numFire + " exist num fire " + constraintElement.NumFiring);
                    if ((numFire != 0) && ((numFire < constraintElement.NumFiring) || (numFire == constraintElement.NumFiring && marketDatetime > constraintElement.MarketDateTimeInterval)))
                    {
                        continue;
                    }
                    updateConstraintCommand.Parameters["@marketdatetime"].Value = constraintElement.MarketDateTimeInterval;
                    updateConstraintCommand.Parameters["@MonitoredText"].Value = constraintElement.Monitor;
                    updateConstraintCommand.Parameters["@ContingencyText"].Value = constraintElement.Contingency;
                    updateConstraintCommand.Parameters["@shadowprice"].Value = constraintElement.ShadowPrice;
                    updateConstraintCommand.Parameters["@numberconstraints"].Value = constraintElement.NumFiring;
                    updateConstraintCommand.Parameters["@shiftfactor"].Value = constraintElement.ShiftFactor;
                    updateConstraintCommand.Parameters["@dollarimpact"].Value = constraintElement.Impact;
                    updateConstraintCommand.Parameters["@updatetime"].Value = constraintElement.UpdateTime;
                    int count = updateConstraintCommand.ExecuteNonQuery();
                    if (count == 0)
                    {
                        if (mMarket != "MISO")
                        {
                            int newId = CreateNewConID();
                            insertConstraintCommand.Parameters["@constraintrtnum"].Value = newId;
                        }
                        insertConstraintCommand.Parameters["@MarketDateTime"].Value = constraintElement.MarketDateTimeInterval;
                        insertConstraintCommand.Parameters["@MonitoredText"].Value = constraintElement.Monitor;
                        insertConstraintCommand.Parameters["@ContingencyText"].Value = constraintElement.Contingency;
                        insertConstraintCommand.Parameters["@ShadowPrice"].Value = constraintElement.ShadowPrice;
                        insertConstraintCommand.Parameters["@NumberConstraints"].Value = constraintElement.NumFiring;
                        insertConstraintCommand.Parameters["@ShiftFactor"].Value = constraintElement.ShiftFactor;
                        insertConstraintCommand.Parameters["@DollarImpact"].Value = constraintElement.Impact;
                        insertConstraintCommand.Parameters["@UpdateTime"].Value = constraintElement.UpdateTime;
                        insertConstraintCommand.ExecuteNonQuery();
                        reader = selectConstraintIDCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            constraintID = Convert.ToInt32(reader.GetValue(0));
                        }
                        reader.Close();
                    }
                    InsertIntoRiskConstraints(constraintID, constraintElement.MarketDateTimeInterval.Date);
                    InsertVector(elementList, constraintElement.ShiftFactor, constraintID, constraintElement.Monitor, constraintElement.Contingency);
                    Console.WriteLine("Saved Vector " + elementList.Count);
                    VayuDBConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 1;
        }

        private void InsertIntoRiskConstraints(int constraintID, DateTime dateTime)
        {
            try
            {

                List<int> constraintNumList = new List<int>();
                SqlCommand cmd = VayuDBConnection.CreateCommand();
                cmd.CommandText = "select distinct ConstraintRTNum from RiskConstraints";
                cmd.Connection = VayuDBConnection;
                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    int constraintNum = Convert.ToInt32(rdr.GetValue(0));
                    constraintNumList.Add(constraintNum);
                }
                rdr.Close();
                if (!constraintNumList.Contains(constraintID))
                {

                    SqlCommand insertRiskConstraintsCmd = VayuDBConnection.CreateCommand();
                    insertRiskConstraintsCmd.CommandText = "insert into Riskconstraints values(@Date , @ConstraintRtNum , 'RISK')";

                    insertRiskConstraintsCmd.Parameters.AddWithValue("@Date", dateTime.Date);
                    insertRiskConstraintsCmd.Parameters.AddWithValue("@ConstraintRtNum", constraintID);
                    insertRiskConstraintsCmd.Connection = VayuDBConnection;
                    insertRiskConstraintsCmd.ExecuteNonQuery();
                    Console.WriteLine("Inserted New Constraint in RiskConstraints " + constraintID);
                }
                VayuDBConnection.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + '\t' + "Exception while inserting data into RiskConstraints");
            }
        }
        private Dictionary<string, double> CalculateSensitivity(List<Congestion> congestionList, double congestionValue, long nodeKey)
        {
            Dictionary<string, double> sensitivityHash = new Dictionary<string, double>(); ;
            SqlCommand selectSensitivityCommand = mSelectErcotSensitivityCommand;
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
            double sensitivityShadow = 0;
            double shadow = 0;
            string sensKey = null;
            foreach (Congestion congestion in congestionList)
            {
                bool found = false;
                if (congestionList.Count > 1)
                {
                    string key = congestion.ConstraintName + congestion.ContingencyName + nodeKey;
                    if (!mSensitivityHash.ContainsKey(key))
                    {
                        selectSensitivityCommand.Parameters["@monitoredtext"].Value = congestion.ConstraintName;
                        selectSensitivityCommand.Parameters["@contingencytext"].Value = congestion.ContingencyName;
                        selectSensitivityCommand.Parameters["@nodekey"].Value = nodeKey;
                        SqlDataReader reader = selectSensitivityCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            double tempSens = (double)reader.GetDecimal(0);
                            mSensitivityHash.Add(key, tempSens);
                        }
                        reader.Close();
                    }
                    if (mSensitivityHash.ContainsKey(key))
                    {
                        double tempSens = mSensitivityHash[key];
                        sensitivityShadow += (tempSens * congestion.ShadowPrice);
                        found = true;
                    }
                }
                if (!found)
                {
                    sensKey = congestion.ConstraintName + "?" + congestion.ContingencyName;
                    shadow = congestion.ShadowPrice;
                }
            }
            if (shadow != 0)
            {
                double sensitivity = (congestionValue - sensitivityShadow) / shadow;
                sensitivityHash.Add(sensKey, sensitivity);
            }
            return sensitivityHash;
        }
        private void SaveSensitivity(List<Tuple<int, DateTime>> countList, List<List<string>> compareCongestionList)
        {
            mStartDate = DateTime.MinValue;
            List<ConstraintElement> constraintList = new List<ConstraintElement>();
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
            SqlDataReader reader = mSelectErcotConstraintMasterCommand.ExecuteReader();
            while (reader.Read())
            {
                ConstraintElement constraint = new ConstraintElement();
                constraint.MarketDateTimeInterval = reader.GetDateTime(0);
                constraint.Monitor = reader.GetString(1);
                constraint.Contingency = reader.GetString(2);
                constraint.ShadowPrice = (double)reader.GetDecimal(3);
                constraint.NumFiring = (int)reader.GetDecimal(4);
                constraint.ShiftFactor = (double)reader.GetDecimal(5);
                constraint.Score = 0;
                constraint.ImpactRatio = 1;
                constraint.Impact = (double)reader.GetDecimal(6);
                constraint.UpdateTime = DateTime.Now;
                constraintList.Add(constraint);
            }
            reader.Close();
            SqlCommand selectConstraintCommand = mSelectErcotConstraintCommand;
            SqlCommand selectVectorCountCommand =mSelectErcotVectorCountCommand;
            foreach (Tuple<int, DateTime> countTuple in countList)
            {
                if (countTuple.Item1 != 1)
                {

                }
                if (VayuDBConnection.State == ConnectionState.Open)
                {
                    VayuDBConnection.Close();
                }
                VayuDBConnection.Open();
                selectConstraintCommand.Parameters["@marketdatetime"].Value = countTuple.Item2;
                int count = 0;
                List<Congestion> congestionList = new List<Congestion>();
                reader = selectConstraintCommand.ExecuteReader();
                while (reader.Read())
                {
                    Congestion tempCongestion = new Congestion();
                    tempCongestion.ConstraintName = reader.GetString(0);
                    tempCongestion.ContingencyName = reader.GetString(1);
                    tempCongestion.ShadowPrice = reader.IsDBNull(2) ? 0 : (double)reader.GetDecimal(2);
                    if (tempCongestion.ShadowPrice == 0)
                    {
                        continue;
                    }
                    if (tempCongestion.ConstraintName.Equals("Monitor     COMPANY LINE    230 KV  BRAMBLET-LOUDOUN4 2045A"))
                    {

                    }
                    congestionList.Add(tempCongestion);
                    count++;
                }
                reader.Close();
                if (congestionList.Count == 0)
                {
                    continue;
                }
                bool found = false;
                foreach (List<string> tempList in compareCongestionList)
                {

                    if (tempList.Count == congestionList.Count)
                    {
                        foreach (Congestion tempCongestion in congestionList)
                        {
                            string key = tempCongestion.ConstraintName + tempCongestion.ContingencyName;
                            if (tempList.Contains(key))
                            {
                                found = true;
                            }
                            else
                            {
                                found = false;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        break;
                    }
                }
                if (found)
                {
                    continue;
                }
                List<string> tempCongestionList = new List<string>();
                foreach (Congestion tempCongestion in congestionList)
                {
                    string key = tempCongestion.ConstraintName + tempCongestion.ContingencyName;
                    tempCongestionList.Add(key);
                }
                compareCongestionList.Add(tempCongestionList);
                foreach (Congestion tempCongestion in congestionList)
                {
                    int vectorCount = 0;
                    selectVectorCountCommand.Parameters["@monitoredtext"].Value = tempCongestion.ConstraintName;
                    selectVectorCountCommand.Parameters["@contingencytext"].Value = tempCongestion.ContingencyName;
                    reader = selectVectorCountCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        vectorCount = reader.GetInt32(0);
                    }
                    reader.Close();
                    if (vectorCount == 0)
                    {
                        found = false;
                        break;
                    }
                    else
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    continue;
                }
                if (count > 1)
                {
                    int compCount = 0;
                    foreach (Congestion compCongestion in congestionList)
                    {
                        Dictionary<DateTime, int> countHash = new Dictionary<DateTime, int>();
                        int minCount = int.MaxValue;
                        SqlCommand selectNumFireCountCommand =  mSelectErcotNumFireCommand;
                        selectNumFireCountCommand.Parameters["@ContingencyText"].Value = compCongestion.ContingencyName;
                        selectNumFireCountCommand.Parameters["@ConstraintText"].Value = compCongestion.ConstraintName;
                        selectNumFireCountCommand.Parameters["@shadowprice"].Value = 2;
                        selectNumFireCountCommand.CommandTimeout = 1000 * 60 * 10;
                        reader = selectNumFireCountCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime tempDate = reader.GetDateTime(0);
                            int tempCount = reader.GetInt32(1);
                            countHash.Add(tempDate, tempCount);
                        }
                        reader.Close();
                        selectNumFireCountCommand.Parameters["@shadowprice"].Value = 0;
                        reader = selectNumFireCountCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime tempDate = reader.GetDateTime(0);
                            int compareCount = reader.GetInt32(1);
                            if (countHash.ContainsKey(tempDate))
                            {
                                int tempCount = countHash[tempDate];
                                if (tempCount == compareCount)
                                {
                                    if (minCount > compareCount)
                                    {
                                        mStartDate = tempDate.AddMinutes(-5);
                                        mEndDate = tempDate;
                                        minCount = tempCount;
                                    }
                                }
                            }
                        }
                        reader.Close();
                        int index = constraintList.FindIndex(t => t.Monitor.Equals(compCongestion.ConstraintName) && t.Contingency.Equals(compCongestion.ContingencyName));
                        if (index != -1)
                        {
                            ConstraintElement compConstraint = constraintList[index];
                            if (compConstraint.NumFiring <= minCount)
                            {
                                mStartDate = DateTime.MinValue;
                                compCount++;
                            }
                            else
                            {
                                SqlCommand deleteMasterCommand =  mDeleteErcotMasterConstraintCommand;
                                deleteMasterCommand.Parameters["@contingencytext"].Value = compConstraint.Contingency;
                                deleteMasterCommand.Parameters["@monitoredtext"].Value = compConstraint.Monitor;
                                deleteMasterCommand.ExecuteNonQuery();
                                VayuDBConnection.Close();
                                return;
                            }
                        }
                        else if (minCount < countTuple.Item1 && minCount < 4)
                        {
                            // return;
                        }
                    }
                    if (compCount != count && compCount != congestionList.Count - 1)
                    {
                        mNotFoundDateTimeList.Add(countTuple.Item2);
                    }
                    if (compCount == count || compCount != congestionList.Count - 1)
                    {
                        mStartDate = DateTime.MinValue;
                        continue;
                    }
                }
                List<Element> elementList = new List<Element>();
                try
                {
                    Node[] priceNodes = RunLmp(countTuple.Item2, countTuple.Item2.AddMinutes(5));
                    foreach (Node item in priceNodes)
                    {
                        Element element = new Element();
                        element.Nodekey = item.NodeId;
                        List<Element> result = elementList.Where(t => t.Nodekey.Equals(item.NodeId)).ToList();
                        if (result.Count > 0)
                        {
                            int index = elementList.FindIndex(t => t.Nodekey.Equals(item.NodeId));
                            elementList.RemoveAt(index);
                        }
                        List<LmpTimePrice> RTTimePriceList = item.LmpTimePriceList;
                        double congestion = RTTimePriceList[0].Lmp.Congestion;
                        element.Congestion = congestion;
                        element.SensitivityHash = CalculateSensitivity(congestionList, congestion, element.Nodekey);
                        elementList.Add(element);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
                if (elementList.Count() > 0)
                {
                    List<ConstraintElement> sendConstraintList = new List<ConstraintElement>();
                    double totalShadowPrice = 0;
                    foreach (Congestion congestion in congestionList)
                    {
                        totalShadowPrice += congestion.ShadowPrice;
                    }
                    foreach (Congestion congestion in congestionList)
                    {
                        double minimum = elementList.Min(p => p.Congestion);
                        double maximum = elementList.Max(p => p.Congestion);
                        double shiftFactor = maximum == 0 && minimum == 0 ? 0 : (Math.Abs(maximum - minimum) / totalShadowPrice) * (congestion.ShadowPrice / totalShadowPrice);
                        Console.WriteLine(congestion.ConstraintName + " shiftfactor " + shiftFactor);
                        ConstraintElement constraint = new ConstraintElement();
                        constraint.MarketDateTimeInterval = countTuple.Item2;
                        constraint.Monitor = congestion.ConstraintName;
                        constraint.Contingency = congestion.ContingencyName;
                        constraint.ShadowPrice = congestion.ShadowPrice;
                        constraint.NumFiring = congestionList.Count;
                        constraint.ShiftFactor = shiftFactor;
                        constraint.Score = 0;
                        constraint.ImpactRatio = 1;
                        constraint.Impact = congestion.ShadowPrice * shiftFactor;
                        constraint.UpdateTime = DateTime.Now;
                        constraintList.Add(constraint);
                        sendConstraintList.Add(constraint);
                    }

                    InsertConstraint(sendConstraintList, elementList);
                }
            }
        }
        public Node[] RunLmp(DateTime startDate, DateTime endDate)
        {
            string key = startDate.ToString() + endDate.ToString();
            if (mPriceHash.ContainsKey(key))
            {
                Console.WriteLine(DateTime.Now + " Got prices " + startDate + " " + mPriceHash[key].Length);
                return mPriceHash[key];
            }
            Node[] priceNodes = null;
            try
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
                int marketKey =  9;
                Console.WriteLine(DateTime.Now + " Getting prices " + startDate);
                priceNodes = nodeProxy.GetAllFiveMinPrice(marketKey, startDate, endDate, false);
                if ((mMarket == "ERCOT" && priceNodes.Length > 10000))
                {
                    mPriceHash.Add(key, priceNodes);
                }
                Console.WriteLine(DateTime.Now + " Got prices " + priceNodes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return priceNodes;
        }
        public void StartTimer()
        {
            OnTimerEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            mTimer.Interval = 1000 * 60 * 5;// 120000;
            mTimer.Start();
            while (true) ;
        }
        private void SaveHourlyImpact(DateTime startDate, DateTime endDate)
        {

            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
            SqlCommand deleteHourlyImpactCommand = mDeleteErcotHourlyImpactCommand;
            SqlCommand insertHourlyImpactCommand = mInsertErcotHourlyImpactCommand;
            SqlCommand updateHourlyImpactCommand =  mUpdateErcotImpactsCommand;
            while (startDate < endDate)
            {
                Console.WriteLine(DateTime.Now + " saving impact " + startDate);
                DateTime date = startDate.Date;
                int hour = startDate.AddHours(1).Hour == 0 ? 24 : startDate.AddHours(1).Hour;
                deleteHourlyImpactCommand.Parameters["@Time"].Value = date;
                deleteHourlyImpactCommand.Parameters["@Hour"].Value = hour;
                deleteHourlyImpactCommand.ExecuteNonQuery();
                insertHourlyImpactCommand.Parameters["@start"].Value = date.AddHours(hour - 1);
                insertHourlyImpactCommand.Parameters["@Hour"].Value = hour;
                insertHourlyImpactCommand.Parameters["@end"].Value = date.AddHours(hour);
                insertHourlyImpactCommand.ExecuteNonQuery();
                updateHourlyImpactCommand.Parameters["@Time"].Value = date;
                updateHourlyImpactCommand.Parameters["@Hour"].Value = hour;
                updateHourlyImpactCommand.ExecuteNonQuery();
                startDate = startDate.AddHours(1);
            }

            VayuDBConnection.Close();
        }
        private List<Tuple<int, DateTime>> GetCountList(DateTime startDate, DateTime endDate)
        {
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
            SqlCommand selectConstraintCountCommand =  mSelectErcotConstraintCountCommand;
            List<Tuple<int, DateTime>> countList = new List<Tuple<int, DateTime>>();
            selectConstraintCountCommand.Parameters["@start"].Value = startDate;
            selectConstraintCountCommand.Parameters["@end"].Value = endDate;
            SqlDataReader reader = selectConstraintCountCommand.ExecuteReader();
            while (reader.Read())
            {
                DateTime foundMarketDateTime = reader.GetDateTime(0);
                int count = reader.GetInt32(1);
                countList.Add(new Tuple<int, DateTime>(count, foundMarketDateTime));
            }
            reader.Close();
            VayuDBConnection.Close();
            return countList;
        }
        public void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            InitDB();
            DeleteOrphans();
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
            VayuDBConnection.Open();
            DateTime maxDate = DateTime.Today;
            SqlDataReader reader =mSelectErcotMaxConstraintDateCommand.ExecuteReader();
            while (reader.Read())
            {
                maxDate = reader.GetDateTime(0);
            }
            mNotFoundDateTimeList = new List<DateTime>();
            List<Congestion> congestionList = new List<Congestion>();
            List<List<string>> compareCongestionList = new List<List<string>>();
            if (mStartDate == DateTime.MinValue)
            {
                mStartDate = maxDate.AddHours(-1);
                mEndDate = DateTime.Now.AddHours(2);
            }
            Console.WriteLine(DateTime.Now + " Start Date " + mStartDate + " End Date " + mEndDate);
#if MANUAL
            mFirstTime = true;
#else
#endif
            if (mFirstTime)
            {
                mFirstTime = false;
                mStartDate = maxDate.AddHours(-30);
#if MANUAL
                if (runCount == 0)
                {
                    List<DateTime> Impactdatetimelist = SaveHourlyImpactManually(mStartDate, mEndDate);
                    foreach (DateTime date in Impactdatetimelist)
                    {
                        mEndDate = date.AddDays(1).AddHours(1);
                        SaveHourlyImpact(date, mEndDate);
                    }
                }
#else
                SaveHourlyImpact(mStartDate, mEndDate);
#endif
            }
            else
            {
                SaveHourlyImpact(maxDate.AddHours(-30), mEndDate);
            }
            List<Tuple<int, DateTime>> countList = GetCountList(mStartDate, mEndDate);
            SaveSensitivity(countList, compareCongestionList);
            if (mStartDate != DateTime.MinValue)
            {
                mTimer.Enabled = true;
                return;
            }
            bool done = false;
            Dictionary<DateTime, double> sensitivityHash = new Dictionary<DateTime, double>();
            foreach (DateTime notFoundDateTime in mNotFoundDateTimeList)
            {
                // changed from miso to pjm
                if (done)
                {
                    break;
                }
                List<string> foundList = new List<string>();
                Dictionary<DateTime, List<Congestion>> foundCongestionHash = new Dictionary<DateTime, List<Congestion>>();
                Dictionary<DateTime, List<Congestion>> notFoundCongestionHash = new Dictionary<DateTime, List<Congestion>>();
                List<Congestion> notFoundCongestionList = new List<Congestion>();
                mSelectErcotConstraintCommand.Parameters["@marketdatetime"].Value = notFoundDateTime;
                reader = mSelectErcotConstraintCommand.ExecuteReader();
                while (reader.Read())
                {
                    Congestion tempCongestion = new Congestion();
                    tempCongestion.ConstraintName = reader.GetString(0);
                    tempCongestion.ContingencyName = reader.GetString(1);
                    tempCongestion.ShadowPrice = reader.IsDBNull(2) ? 0 : (double)reader.GetDecimal(2);
                    if (tempCongestion.ShadowPrice == 0)
                    {
                        continue;
                    }
                    notFoundCongestionList.Add(tempCongestion);
                    foundList.Add(tempCongestion.ConstraintName + tempCongestion.ContingencyName);
                    List<Congestion> addCongestionList = new List<Congestion>();
                    if (foundCongestionHash.ContainsKey(notFoundDateTime))
                    {
                        addCongestionList = foundCongestionHash[notFoundDateTime];
                        foundCongestionHash.Remove(notFoundDateTime);
                    }
                    addCongestionList.Add(tempCongestion);
                    foundCongestionHash.Add(notFoundDateTime, addCongestionList);
                }
                reader.Close();
                List<string> constraintNumList = new List<string>();
                foreach (Congestion congestion in notFoundCongestionList)
                {
                    //changed command from miso to pjm
                    mSelectErcotConstraintIDCommand.Parameters["@monitoredtext"].Value = congestion.ConstraintName;
                    mSelectErcotConstraintIDCommand.Parameters["@contingencytext"].Value = congestion.ContingencyName;
                    reader = mSelectErcotConstraintIDCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string key = congestion.ConstraintName + congestion.ContingencyName;
                        constraintNumList.Add(key);
                    }
                    reader.Close();
                }
                if (foundList.Count == 0)
                {
                    continue;
                }
                if (foundList.Count - constraintNumList.Count == 1)
                {
                    done = true;
                    continue;
                }
                mSelectConstraintCongestionCountCommand.Parameters["@marketdatetime"].Value = notFoundDateTime;
                mSelectConstraintCongestionCountCommand.Parameters["@constraintname"].Value = notFoundCongestionList[0].ConstraintName;
                mSelectConstraintCongestionCountCommand.Parameters["@contingencyname"].Value = notFoundCongestionList[0].ContingencyName;
                reader = mSelectConstraintCongestionCountCommand.ExecuteReader();
                while (reader.Read())
                {
                    DateTime foundMarketDateTime = reader.GetDateTime(0);
                    mSelectErcotConstraintCommand.Parameters["@marketdatetime"].Value = foundMarketDateTime;
                    SqlDataReader reader2 = mSelectErcotConstraintCommand.ExecuteReader();
                    while (reader2.Read())
                    {
                        Congestion congestion = new Congestion();
                        congestion.ConstraintName = reader2.GetString(0);
                        congestion.ContingencyName = reader2.IsDBNull(1) ? "" : reader2.GetString(1);
                        congestion.ShadowPrice = (double)reader2.GetDecimal(2);
                        if (foundList.Contains(congestion.ConstraintName + congestion.ContingencyName))
                        {
                            List<Congestion> addCongestionList = new List<Congestion>();
                            if (foundCongestionHash.ContainsKey(foundMarketDateTime))
                            {
                                addCongestionList = foundCongestionHash[foundMarketDateTime];
                                foundCongestionHash.Remove(foundMarketDateTime);
                            }
                            addCongestionList.Add(congestion);
                            foundCongestionHash.Add(foundMarketDateTime, addCongestionList);
                        }
                        else
                        {
                            List<Congestion> addCongestionList = new List<Congestion>();
                            if (notFoundCongestionHash.ContainsKey(foundMarketDateTime))
                            {
                                addCongestionList = notFoundCongestionHash[foundMarketDateTime];
                                notFoundCongestionHash.Remove(foundMarketDateTime);
                            }
                            addCongestionList.Add(congestion);
                            notFoundCongestionHash.Add(foundMarketDateTime, addCongestionList);
                        }
                    }
                    reader2.Close();
                }
                reader.Close();
                List<DateTime> foundKeyList = foundCongestionHash.Keys.ToList<DateTime>();
                bool first = true;
                List<Congestion> foundCongestionList = new List<Congestion>();
                foreach (DateTime foundDate in foundKeyList)
                {
                    if (first)
                    {
                        foundCongestionList = foundCongestionHash[foundDate];
                        first = false;
                    }
                    else
                    {
                        List<Congestion> tempCongestionList = foundCongestionHash[foundDate];
                        if (tempCongestionList.Count < foundCongestionList.Count)
                        {
                            foundCongestionHash.Remove(foundDate);
                        }
                    }
                }
                foundKeyList = foundCongestionHash.Keys.ToList<DateTime>();
                Dictionary<int, List<Dictionary<DateTime, List<Congestion>>>> countDateHash = new Dictionary<int, List<Dictionary<DateTime, List<Congestion>>>>();
                foreach (DateTime foundDate in foundKeyList)
                {
                    int keyCount = 0;
                    List<Dictionary<DateTime, List<Congestion>>> countDateList = new List<Dictionary<DateTime, List<Congestion>>>();
                    if (notFoundCongestionHash.ContainsKey(foundDate))
                    {
                        keyCount = notFoundCongestionHash[foundDate].Count;
                    }
                    if (countDateHash.ContainsKey(keyCount))
                    {
                        countDateList = countDateHash[keyCount];
                        countDateHash.Remove(keyCount);
                    }
                    List<Congestion> tempCongestionList = foundCongestionHash[foundDate];
                    Dictionary<DateTime, List<Congestion>> tempCongestionHash = new Dictionary<DateTime, List<Congestion>>();
                    tempCongestionHash.Add(foundDate, tempCongestionList);
                    countDateList.Add(tempCongestionHash);
                    countDateHash.Add(keyCount, countDateList);
                }
                List<int> countDateKeyList = countDateHash.Keys.ToList<int>();
                countDateKeyList.Sort();
                int dateCount = 0;
                double[,] components = new double[foundList.Count, foundList.Count];
                List<Dictionary<DateTime, List<Congestion>>> matrixList = new List<Dictionary<DateTime, List<Congestion>>>();
                foreach (int countDateKey in countDateKeyList)
                {
                    List<Dictionary<DateTime, List<Congestion>>> countDateList = countDateHash[countDateKey];
                    foreach (Dictionary<DateTime, List<Congestion>> tempCountHash in countDateList)
                    {
                        matrixList.Add(tempCountHash);
                        dateCount++;
                        if (dateCount == foundList.Count)
                        {
                            break;
                        }
                    }
                    if (dateCount == foundList.Count)
                    {
                        break;
                    }
                }
                int column = 0;
                List<DateTime> matrixDateTimeList = new List<DateTime>();
                foreach (Dictionary<DateTime, List<Congestion>> matrixDateHash in matrixList)
                {
                    List<DateTime> keyList = matrixDateHash.Keys.ToList<DateTime>();
                    foreach (DateTime key in keyList)
                    {
                        int row = 0;
                        if (!matrixDateTimeList.Contains(key))
                        {
                            matrixDateTimeList.Add(key);
                        }
                        List<Congestion> tempCongestionList = matrixDateHash[key];
                        foreach (Congestion tempCongestion in tempCongestionList)
                        {
                            components[column, row] = tempCongestion.ShadowPrice;
                            row++;
                        }
                        column++;
                    }
                }
                DenseMatrix m1 = Matrix.Create(components);
                Matrix m = null;
                try
                {
                    m = m1.GetInverse();
                }
                catch (Exception ex)
                {
                    continue;
                }
                Dictionary<long, List<double>> nodeCongestionHash = new Dictionary<long, List<double>>();
                foreach (DateTime key in matrixDateTimeList)
                {
                    Node[] priceNodes = RunLmp(key, key.AddMinutes(5));
                    foreach (Node item in priceNodes)
                    {
                        List<LmpTimePrice> RTTimePriceList = item.LmpTimePriceList;
                        double congestion = RTTimePriceList[0].Lmp.Congestion;
                        List<double> nodeCongestionList = new List<double>();
                        if (nodeCongestionHash.ContainsKey(item.NodeId))
                        {
                            nodeCongestionList = nodeCongestionHash[item.NodeId];
                            nodeCongestionHash.Remove(item.NodeId);
                        }
                        nodeCongestionList.Add(congestion);
                        nodeCongestionHash.Add(item.NodeId, nodeCongestionList);
                    }
                }
                List<long> nodeKeyList = nodeCongestionHash.Keys.ToList<long>();
                double[] components1 = new double[foundList.Count];
                Dictionary<int, List<Element>> elementHash = new Dictionary<int, List<Element>>();
                foreach (int nodeKey in nodeKeyList)
                {
                    List<double> nodeCongestionList = nodeCongestionHash[nodeKey];
                    int i = 0;
                    foreach (double nodeCongestion in nodeCongestionList)
                    {
                        if (i < components1.Length)
                        {
                            components1[i] = nodeCongestion;
                        }
                        i++;
                    }
                    DenseVector v1 = Vector.Create(components1);
                    Vector v = v1 * m;
                    i = 0;
                    foreach (double nodeCongestion in nodeCongestionList)
                    {
                        Element element = new Element();
                        element.Nodekey = nodeKey;
                        List<Element> elementList = new List<Element>();
                        if (elementHash.ContainsKey(i))
                        {
                            elementList = elementHash[i];
                            elementHash.Remove(i);
                        }
                        element.Congestion = nodeCongestion;
                        List<Congestion> vectorCongestionList = matrixList[0][matrixDateTimeList[0]];
                        element.SensitivityHash = new Dictionary<string, double>();
                        if (i < v.Length)
                        {
                            element.SensitivityHash.Add(vectorCongestionList[i].ConstraintName + "?" + vectorCongestionList[i].ContingencyName, v[i]);
                            elementList.Add(element);
                            elementHash.Add(i, elementList);
                        }
                        i++;
                    }
                }
                int num = 0;
                List<Congestion> sendCongestionList = matrixList[0][matrixDateTimeList[0]];
                double totalShadowPrice = 0;
                foreach (Congestion congestion in sendCongestionList)
                {
                    totalShadowPrice += congestion.ShadowPrice;
                }
                foreach (Congestion congestion in sendCongestionList)
                {
                    List<ConstraintElement> sendConstraintList = new List<ConstraintElement>();
                    string congKey = congestion.ConstraintName + congestion.ContingencyName;
                    if (!constraintNumList.Contains(congKey))
                    {
                        double minimum = elementHash[num].Min(p => p.Congestion);
                        double maximum = elementHash[num].Max(p => p.Congestion);
                        double shiftFactor = maximum == 0 && minimum == 0 ? 0 : (Math.Abs(maximum - minimum) / totalShadowPrice) * (congestion.ShadowPrice / totalShadowPrice);
                        Console.WriteLine(congestion.ConstraintName + " shiftfactor " + shiftFactor);
                        ConstraintElement constraint = new ConstraintElement();
                        constraint.MarketDateTimeInterval = matrixDateTimeList[0];
                        constraint.Monitor = congestion.ConstraintName;
                        constraint.Contingency = congestion.ContingencyName;
                        constraint.ShadowPrice = congestion.ShadowPrice;
                        constraint.NumFiring = matrixDateTimeList.Count;
                        constraint.ShiftFactor = shiftFactor;
                        constraint.Score = 0;
                        constraint.ImpactRatio = 1;
                        constraint.Impact = congestion.ShadowPrice * shiftFactor;
                        constraint.UpdateTime = DateTime.Now;
                        sendConstraintList.Add(constraint);
                        done = true;
                    }
                    InsertConstraint(sendConstraintList, elementHash[num]);
                    num++;
                }
            }
            VayuDBConnection.Close();
            VayuDBConnection.Close();
#if MANUAL
            runCount = 1;
#else
            mTimer.Enabled = true;
#endif
        }
        public List<DateTime> SaveHourlyImpactManually(DateTime StartDate, DateTime EndDate)
        {
            try
            {
                List<Congestion> ConstraintContingencyList = new List<Congestion>();
                List<DateTime> datetimeList = new List<DateTime>();
                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                // mSelectManualPJMConstraintImpactCommand.Parameters["@StartDate"].Value = StartDate;
                mSelectManualErcotConstraintImpactCommand.Parameters["@EndDate"].Value = EndDate;
                SqlDataReader reader = mSelectManualErcotConstraintImpactCommand.ExecuteReader();
                while (reader.Read())
                {
                    Congestion congestion = new Congestion();
                    congestion.ConstraintName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                    congestion.ContingencyName = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1));
                    ConstraintContingencyList.Add(congestion);
                }
                reader.Close();
                foreach (Congestion congestion in ConstraintContingencyList)
                {
                    mSelectManualErcotDatetimeCommand.Parameters["@ConstraintText"].Value = congestion.ConstraintName;
                    mSelectManualErcotDatetimeCommand.Parameters["@ContingencyText"].Value = congestion.ContingencyName;
                    SqlDataReader reader1 = mSelectManualErcotDatetimeCommand.ExecuteReader();
                    while (reader1.Read())
                    {
                        DateTime ImpactDate = reader1.IsDBNull(0) ? new DateTime() : Convert.ToDateTime(reader1.GetValue(0));
                        datetimeList.Add(ImpactDate.Date);
                    }
                    reader1.Close();
                }
                VayuDBConnection.Close();
                return datetimeList;
            }
            catch (Exception)
            {
                return null;
                // throw;
            }

        }
    }
}
