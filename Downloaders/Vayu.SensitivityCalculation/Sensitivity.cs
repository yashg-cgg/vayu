
using Extreme;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using Vayu.NodePriceLibrary;
using Vayu.CommonAccessLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Timers;

namespace Vayu.SensitivityCalculation
{
    public class Sensitivity
    {
        private string mMarket;
        private SqlConnection VayuDBConnection;
        private SqlConnection mConnectionTest;
        private SqlCommand mDeleteErcotMasterConstraintCommand;
        private SqlCommand mSelectErcotConstraintMasterCommand;
        private SqlCommand mSelectErcotConstraintCountCommand;
        private SqlCommand mSelectConstraintCongestionCountCommand;
        private SqlCommand mSelectErcotConstraintCommand;
        private SqlCommand mSelectErcotConstraintIDCommand;
        private SqlCommand mSelectMarketDateTimeCommand;
        private SqlCommand mInsertErcotConstraintCommand;
        private SqlCommand mUpdateErcotConstraintCommand;
        private SqlCommand mDeleteErcotVectorCommand;
        private SqlCommand mSelectErcotSensitivityCommand;
        private SqlCommand mSelectErcotVectorCountCommand;
        private SqlCommand mDeleteErcotHourlyImpactCommand;
        private SqlCommand mInsertErcotHourlyImpactCommand;
        private SqlCommand mUpdateErcotImpactsCommand;
        private SqlCommand mSelectErcotMaxConstraintDateCommand;
        private SqlCommand mSelectPjmMaxConstraintCommand;
        private SqlCommand mSelectErcotNumFireCommand;
        private SqlCommand mSelectErcotOrphanVectorCommand;
        private SqlCommand mSelectErcotOrphanImpactCommand;
        private SqlCommand mDeleteErcotOrphanVectorCommand;
        private SqlCommand mDeleteErcotOrphanImpactCommand;
        private SqlCommand mSelectManualPJMConstraintImpactCommand;
        private SqlCommand mSelectManualPJMDatetimeCommand;
        private Dictionary<string, NodePriceLibrary.Node[]> mPriceHash = new Dictionary<string, NodePriceLibrary.Node[]>();
        private System.Timers.Timer mTimer = new System.Timers.Timer();
        private bool mFirstTime = true;
        private DateTime mStartDate;
        private DateTime mEndDate;
        private DataTable mVectorTable = new DataTable();
        private List<DateTime> mNotFoundDateTimeList = new List<DateTime>();
        private Dictionary<string, double> mSensitivityHash = new Dictionary<string, double>();
        private static string sDartEndPoint = ServiceConnections.GetLMPServiceAddress();
        private int runCount = 0;

        public Sensitivity(string market)
        {
            
            this.mMarket = market;
        }

        private void InitDB()
        {
            this.VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            
            this.mSelectErcotNumFireCommand = new SqlCommand();
            this.mSelectErcotNumFireCommand.CommandText = "select MarketDateTime, COUNT(*) from Vayu..ConstraintRT (nolock) where abs(shadowprice) > @shadowprice and MarketDateTime in (select MarketDateTime from Vayu..ConstraintRT (nolock) where ContingencyText = @ContingencyText and ConstraintText = @ConstraintText) and marketdatetime > (select MIN(marketdate) from Vayu..NodeLMPMin (nolock) where congestion is not null ) group by MarketDateTime order by MarketDateTime desc";
            this.mSelectErcotNumFireCommand.Parameters.AddWithValue("@ContingencyText", (object)"ContingencyText");
            this.mSelectErcotNumFireCommand.Parameters.AddWithValue("@ConstraintText", (object)"ConstraintText");
            this.mSelectErcotNumFireCommand.Parameters.AddWithValue("@shadowprice", (object)"shadowprice");
            this.mSelectErcotNumFireCommand.Connection = this.VayuDBConnection;
            
            this.mDeleteErcotVectorCommand = new SqlCommand();
            this.mDeleteErcotVectorCommand.CommandText = "delete Vayu..RTMasterVector where constraintrtnum = @constraintrtnum and source is null ";
            this.mDeleteErcotVectorCommand.Parameters.AddWithValue("@constraintrtnum", (object)"constraintrtnum");
            this.mDeleteErcotVectorCommand.Connection = this.VayuDBConnection;
            
            this.mDeleteErcotMasterConstraintCommand = new SqlCommand();
            this.mDeleteErcotMasterConstraintCommand.CommandText = "delete RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext";
            this.mDeleteErcotMasterConstraintCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mDeleteErcotMasterConstraintCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mDeleteErcotMasterConstraintCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotMaxConstraintDateCommand = new SqlCommand();
            this.mSelectErcotMaxConstraintDateCommand.CommandText = "select max(marketdatetime) from Vayu..RTMasterConstraint";
            this.mSelectErcotMaxConstraintDateCommand.Connection = this.VayuDBConnection;
            
            this.mDeleteErcotHourlyImpactCommand = new SqlCommand();
            this.mDeleteErcotHourlyImpactCommand.CommandText = "delete RTImpact where date =  Convert(Date, @Time) and hour = @Hour";
            this.mDeleteErcotHourlyImpactCommand.Parameters.AddWithValue("@Time", (object)"Time");
            this.mDeleteErcotHourlyImpactCommand.Parameters.AddWithValue("@Hour", (object)"Hour");
            this.mDeleteErcotHourlyImpactCommand.Connection = this.VayuDBConnection;
            
            this.mInsertErcotHourlyImpactCommand = new SqlCommand();
            this.mInsertErcotHourlyImpactCommand.CommandText = "INSERT INTO Vayu..RTImpact(ConstraintRTNum, Date, Hour, ShadowPrice) SELECT B.ConstraintRTNum, Convert(Date, @start) as Date, @Hour as Hour, abs(sum(A.ShadowPrice)) as ShadowPrice from Vayu..ConstraintRT as A inner join RTMasterConstraint as B on A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  where A.MarketDateTime >= @start and A.MarketDateTime < @end and ConstraintText <> 'None' group by ConstraintText, A.ContingencyText, B.ConstraintRTNum";
            this.mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@start", (object)"a.marketdatetime");
            this.mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@end", (object)"a.marketdatetime");
            this.mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@Hour", (object)"Hour");
            this.mInsertErcotHourlyImpactCommand.Connection = this.VayuDBConnection;
            
            this.mUpdateErcotImpactsCommand = new SqlCommand();
            this.mUpdateErcotImpactsCommand.CommandText = "Update RTImpact set impact = A.shadowprice*B.shiftfactor from RTImpact A inner join RTMasterConstraint B on A.ConstraintRTNum = B.ConstraintRTNum where date = Convert(Date, @Time) and hour = @Hour and B.Shiftfactor is not null ";
            this.mUpdateErcotImpactsCommand.Parameters.AddWithValue("@Time", (object)"Time");
            this.mUpdateErcotImpactsCommand.Parameters.AddWithValue("@Hour", (object)"Hour");
            this.mUpdateErcotImpactsCommand.Connection = this.VayuDBConnection;
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = "SELECT top 1 a.ConstraintRTNum, a.NodeKey, a.Sensitivity, a.SensitivityNormal FROM RTMasterVector a";
            sqlCommand.Connection = this.VayuDBConnection;
            new SqlDataAdapter() { SelectCommand = sqlCommand }.Fill(this.mVectorTable);
            
            this.mSelectErcotConstraintIDCommand = new SqlCommand();
            this.mSelectErcotConstraintIDCommand.CommandText = "select constraintrtnum, marketdatetime, numberconstraints from Vayu..RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext";
            this.mSelectErcotConstraintIDCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mSelectErcotConstraintIDCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mSelectErcotConstraintIDCommand.Connection = this.VayuDBConnection;
            this.mSelectPjmMaxConstraintCommand = new SqlCommand();
            this.mSelectPjmMaxConstraintCommand.CommandText = "SELECT max(ConstraintRTNum) from RTMasterConstraint";
            this.mSelectPjmMaxConstraintCommand.Connection = this.VayuDBConnection;
           
            this.mInsertErcotConstraintCommand = new SqlCommand();
            this.mInsertErcotConstraintCommand.CommandText = "insert RTMasterConstraint values (@constraintrtnum, @MarketDateTime, @MonitoredText, @ContingencyText, @ShadowPrice, @NumberConstraints, @ShiftFactor, 0, 1, @DollarImpact, @UpdateTime)";
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@constraintrtnum", (object)"constraintrtnum");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@MarketDateTime", (object)"MarketDateTime");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", (object)"MonitoredText");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", (object)"ContingencyText");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@ShadowPrice", (object)"ShadowPrice");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@NumberConstraints", (object)"NumberConstraints");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@ShiftFactor", (object)"ShiftFactor");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@DollarImpact", (object)"DollarImpact");
            this.mInsertErcotConstraintCommand.Parameters.AddWithValue("@UpdateTime", (object)"UpdateTime");
            this.mInsertErcotConstraintCommand.Connection = this.VayuDBConnection;
            
            this.mUpdateErcotConstraintCommand = new SqlCommand();
            this.mUpdateErcotConstraintCommand.CommandText = "update RTMasterConstraint set marketdatetime = @marketdatetime, shadowprice = @shadowprice, numberconstraints = @numberconstraints, shiftfactor = @shiftfactor, dollarimpact = @dollarimpact, updatetime = @updatetime where monitoredtext = @MonitoredText and contingencytext = @ContingencyText";
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", (object)"marketdatetime");
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shadowprice", (object)"shadowprice");
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@numberconstraints", (object)"numberconstraints");
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shiftfactor", (object)"shiftfactor");
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@dollarimpact", (object)"dollarimpact");
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@updatetime", (object)"updatetime");
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", (object)"MonitoredText");
            this.mUpdateErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", (object)"ContingencyText");
            this.mUpdateErcotConstraintCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotConstraintCommand = new SqlCommand();
            this.mSelectErcotConstraintCommand.CommandText = "select constrainttext, contingencytext, shadowprice from Vayu..constraintrt where marketdatetime = @marketdatetime and shadowprice is not null and shadowprice <> 0";
            this.mSelectErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", (object)"marketdatetime");
            this.mSelectErcotConstraintCommand.Connection = this.VayuDBConnection;
            
            this.mSelectMarketDateTimeCommand = new SqlCommand();
            this.mSelectMarketDateTimeCommand.CommandText = "select marketdatetime from MISO.constraintrt where constraintname = @constraintname and contingencyname = @contingencyname and marketdatetime > @marketdatetime order by marketdatetime";
            this.mSelectMarketDateTimeCommand.Parameters.AddWithValue("@constraintname", (object)"constraintname");
            this.mSelectMarketDateTimeCommand.Parameters.AddWithValue("@contingencyname", (object)"contingencyname");
            this.mSelectMarketDateTimeCommand.Parameters.AddWithValue("@marketdatetime", (object)"marketdatetime");
            this.mSelectMarketDateTimeCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotConstraintCountCommand = new SqlCommand();
            this.mSelectErcotConstraintCountCommand.CommandText = "select MarketDateTime, COUNT(*) a from Vayu..constraintrt where MarketDateTime > @start and MarketDateTime < @end and shadowprice <> 0 and shadowprice is not null group by marketdatetime   order by a, marketdatetime desc";
            this.mSelectErcotConstraintCountCommand.Parameters.AddWithValue("@start", (object)"marketdatetime");
            this.mSelectErcotConstraintCountCommand.Parameters.AddWithValue("@end", (object)"marketdatetime");
            this.mSelectErcotConstraintCountCommand.Connection = this.VayuDBConnection;
            
            this.mSelectConstraintCongestionCountCommand = new SqlCommand();
            this.mSelectConstraintCongestionCountCommand.CommandText = "select MarketDateTime from PJM.constraintrt where MarketDateTime <> @marketdatetime and shadowprice <> 0 and shadowprice is not null and constrainttext = @constraintname and contingencytext = @contingencyname order by marketdatetime desc";
            this.mSelectConstraintCongestionCountCommand.Parameters.AddWithValue("@marketdatetime", (object)"marketdatetime");
            this.mSelectConstraintCongestionCountCommand.Parameters.AddWithValue("@constraintname", (object)"constraintname");
            this.mSelectConstraintCongestionCountCommand.Parameters.AddWithValue("@contingencyname", (object)"contingencyname");
            this.mSelectConstraintCongestionCountCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotSensitivityCommand = new SqlCommand();
            this.mSelectErcotSensitivityCommand.CommandText = "select sensitivity from Vayu..RTMasterVector (NOLOCK) where nodekey = @nodekey and constraintrtnum = (select constraintrtnum from Vayu..RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext)";
            this.mSelectErcotSensitivityCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mSelectErcotSensitivityCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mSelectErcotSensitivityCommand.Parameters.AddWithValue("@nodekey", (object)"nodekey");
            this.mSelectErcotSensitivityCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotConstraintMasterCommand = new SqlCommand();
            this.mSelectErcotConstraintMasterCommand.CommandText = "select marketdatetime, monitoredtext, contingencytext, shadowprice, numberconstraints, shiftfactor, dollarimpact from Vayu..RTMasterConstraint where marketdatetime > dateadd(month, -8, getdate())";
            this.mSelectErcotConstraintMasterCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotVectorCountCommand = new SqlCommand();
            this.mSelectErcotVectorCountCommand.CommandText = "select count(*) from Vayu..RTMasterVector where constraintrtnum = (select  constraintrtnum from Vayu..RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext )";
            this.mSelectErcotVectorCountCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mSelectErcotVectorCountCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mSelectErcotVectorCountCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotOrphanVectorCommand = new SqlCommand();
            this.mSelectErcotOrphanVectorCommand.CommandText = "select distinct constraintrtnum from Vayu..RTMasterVector where ConstraintRTNum not in (select ConstraintRTNum from Vayu..RTMasterConstraint)";
            this.mSelectErcotOrphanVectorCommand.Connection = this.VayuDBConnection;
            
            this.mSelectErcotOrphanImpactCommand = new SqlCommand();
            this.mSelectErcotOrphanImpactCommand.CommandText = "select distinct constraintrtnum from RTImpact where ConstraintRTNum not in (select ConstraintRTNum from RTMasterConstraint)";
            this.mSelectErcotOrphanImpactCommand.Connection = this.VayuDBConnection;
            
            this.mDeleteErcotOrphanVectorCommand = new SqlCommand();
            this.mDeleteErcotOrphanVectorCommand.CommandText = "delete RTMasterVector where constraintrtnum = @constraintrtnum";
            this.mDeleteErcotOrphanVectorCommand.Parameters.AddWithValue("@constraintrtnum", (object)"constraintrtnum");
            this.mDeleteErcotOrphanVectorCommand.Connection = this.VayuDBConnection;
            
            this.mDeleteErcotOrphanImpactCommand = new SqlCommand();
            this.mDeleteErcotOrphanImpactCommand.CommandText = "delete RTImpact where constraintrtnum = @constraintrtnum";
            this.mDeleteErcotOrphanImpactCommand.Parameters.AddWithValue("@constraintrtnum", (object)"constraintrtnum");
            this.mDeleteErcotOrphanImpactCommand.Connection = this.VayuDBConnection;
            
            this.mSelectManualPJMConstraintImpactCommand = new SqlCommand();
            this.mSelectManualPJMConstraintImpactCommand.CommandText = "select distinct ConstraintText , ContingencyText from pjm.ConstraintRT where MarketDateTime = @EndDate ";
            this.mSelectManualPJMConstraintImpactCommand.Parameters.AddWithValue("@EndDate", (object)"EndDate");
            this.mSelectManualPJMConstraintImpactCommand.Connection = this.VayuDBConnection;
            this.mSelectManualPJMDatetimeCommand = new SqlCommand();
            this.mSelectManualPJMDatetimeCommand.CommandText = "select distinct cast(marketdatetime as date) as date from pjm.ConstraintRT where ConstraintText =@ConstraintText and ContingencyText=@ContingencyText";
            this.mSelectManualPJMDatetimeCommand.Parameters.AddWithValue("@ConstraintText", (object)"ConstraintText");
            this.mSelectManualPJMDatetimeCommand.Parameters.AddWithValue("@ContingencyText", (object)"ContingencyText");
            this.mSelectManualPJMDatetimeCommand.Connection = this.VayuDBConnection;
        }

        private void DeleteOrphans()
        {
            Console.WriteLine("Deleting Orphans");
            List<int> intList = new List<int>();
            if (this.VayuDBConnection.State == ConnectionState.Open )
            {
                this.VayuDBConnection.Close();
            }
            this.VayuDBConnection.Open();
            SqlDataReader sqlDataReader1 = this.mSelectErcotOrphanVectorCommand.ExecuteReader();
            while (sqlDataReader1.Read())
                intList.Add((int)sqlDataReader1.GetDecimal(0));
            sqlDataReader1.Close();
            SqlDataReader sqlDataReader2 =  this.mSelectErcotOrphanImpactCommand.ExecuteReader();
            while (sqlDataReader2.Read())
            {
                int num = (int)sqlDataReader2.GetDecimal(0);
                if (!intList.Contains(num))
                    intList.Add(num);
            }
            sqlDataReader2.Close();
            SqlCommand sqlCommand1 = this.mDeleteErcotOrphanVectorCommand;
            SqlCommand sqlCommand2 = this.mDeleteErcotOrphanImpactCommand;
            foreach (int num in intList)
            {
                sqlCommand1.Parameters["@constraintrtnum"].Value = (object)num;
                sqlCommand1.ExecuteNonQuery();
                sqlCommand2.Parameters["@constraintrtnum"].Value = (object)num;
                sqlCommand2.ExecuteNonQuery();
            }
            this.VayuDBConnection.Close();
            Console.WriteLine("Deleted Orphans");
        }

        public int CreateNewConID()
        {
            int num = -1;
            if (this.VayuDBConnection.State == ConnectionState.Closed)
                this.VayuDBConnection.Open();
            using (SqlDataReader sqlDataReader = this.mSelectPjmMaxConstraintCommand.ExecuteReader())
            {
                while (sqlDataReader.Read())
                {
                    if (sqlDataReader.IsDBNull(0))
                    {
                        num = 1;
                    }
                    else
                    {
                        num = Convert.ToInt32(sqlDataReader.GetValue(0));
                        ++num;
                    }
                }
            }
            return num;
        }

        private void InsertVector(
          List<Element> elementlist,
          double shiftfactor,
          int constraintID,
          string constraintName,
          string contingencyName)
        {
            this.mVectorTable.Rows.Clear();
            List<int> intList = new List<int>();
            string key = constraintName + "?" + contingencyName;
            DataTable table = this.mVectorTable.Clone();
            if (this.mMarket == "ERCOT")
            {
                foreach (Element element in elementlist)
                {
                    if (element.Nodekey != 57529)
                        ;
                    if (element.SensitivityHash.ContainsKey(key))
                    {
                        DataRow row = this.mVectorTable.NewRow();
                        row["ConstraintRTNum"] = (object)constraintID;
                        row["NodeKey"] = (object)element.Nodekey;
                        row["Sensitivity"] = Math.Abs(shiftfactor) <= 0.05 ? (object)0 : (!(this.mMarket == "ERCOT") ? (object)(element.SensitivityHash[key] / shiftfactor) : (object)element.SensitivityHash[key]);
                        row["SensitivityNormal"] = (object)0;
                        this.mVectorTable.Rows.Add(row);
                    }
                }
            }
            if (this.mVectorTable.Rows.Count == 0)
                return;
            if (this.VayuDBConnection.State == ConnectionState.Open )
            {
                this.VayuDBConnection.Close();
            }
            this.VayuDBConnection.Open();
            SqlCommand sqlCommand =  this.mDeleteErcotVectorCommand;
            sqlCommand.Parameters["@constraintrtnum"].Value = (object)constraintID;
            sqlCommand.ExecuteNonQuery();
            if (this.mMarket == "ERCOT")
            {
                List<int> ercotSensetivities = this.GetErcotSensetivities(constraintID);
                foreach (DataRow row in (InternalDataCollectionBase)this.mVectorTable.Rows)
                {
                    int int32 = Convert.ToInt32(row["NodeKey"]);
                    if (!ercotSensetivities.Contains(int32))
                        table.Rows.Add(row.ItemArray);
                }
            }
            try
            {
                Console.WriteLine(DateTime.Now.ToString() + " Inside Bulk Copy Vector");
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(this.VayuDBConnection);
                sqlBulkCopy.BulkCopyTimeout = 600;
                sqlBulkCopy.DestinationTableName =  "Vayu..RTMasterVector";
                if (this.VayuDBConnection.State == ConnectionState.Closed)
                    this.VayuDBConnection.Open();
                if (this.mMarket == "ERCOT")
                    sqlBulkCopy.WriteToServer(table);
                else
                    sqlBulkCopy.WriteToServer(this.mVectorTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex);
            }
            Console.WriteLine(DateTime.Now.ToString() + " Outside Bulk Copy Vector");
            this.VayuDBConnection.Close();
        }

        private List<int> GetErcotSensetivities(int constraintID)
        {
            List<int> intList = new List<int>();
            try
            {
                if (this.VayuDBConnection.State == ConnectionState.Closed)
                    this.VayuDBConnection.Open();
                SqlCommand command = this.VayuDBConnection.CreateCommand();
                command.Connection = this.VayuDBConnection;
                command.CommandText = "select distinct NodeKey from Vayu..RTMasterVector where ConstraintRTNum =" + (object)constraintID;
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    int int32 = Convert.ToInt32(sqlDataReader.GetValue(0));
                    intList.Add(int32);
                }
                this.VayuDBConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return intList;
        }

        private int InsertConstraint(
          List<ConstraintElement> constraintElementList,
          List<Element> elementList)
        {
            SqlCommand sqlCommand1 =this.mUpdateErcotConstraintCommand;
            SqlCommand sqlCommand2 =this.mInsertErcotConstraintCommand;
            SqlCommand sqlCommand3 = this.mSelectErcotConstraintIDCommand;
            try
            {
                foreach (ConstraintElement constraintElement in constraintElementList)
                {
                    if (this.VayuDBConnection.State == ConnectionState.Open )
                    {
                        this.VayuDBConnection.Close();
                    }
                    this.VayuDBConnection.Open();
                    int constraintID = 0;
                    DateTime dateTime = DateTime.MinValue;
                    sqlCommand3.Parameters["@monitoredtext"].Value = (object)constraintElement.Monitor;
                    sqlCommand3.Parameters["@contingencytext"].Value = (object)constraintElement.Contingency;
                    SqlDataReader sqlDataReader1 = sqlCommand3.ExecuteReader();
                    int num1 = 0;
                    while (sqlDataReader1.Read())
                    {
                        constraintID = Convert.ToInt32(sqlDataReader1.GetValue(0));
                        dateTime = sqlDataReader1.GetDateTime(1);
                        num1 = Convert.ToInt32(sqlDataReader1.GetValue(0));
                    }
                    sqlDataReader1.Close();
                    Console.WriteLine("Insert constraint num fire " + (object)num1 + " exist num fire " + (object)constraintElement.NumFiring);
                    if (num1 == 0 || num1 >= constraintElement.NumFiring && (num1 != constraintElement.NumFiring || !(dateTime > constraintElement.MarketDateTimeInterval)))
                    {
                        sqlCommand1.Parameters["@marketdatetime"].Value = (object)constraintElement.MarketDateTimeInterval;
                        sqlCommand1.Parameters["@MonitoredText"].Value = (object)constraintElement.Monitor;
                        sqlCommand1.Parameters["@ContingencyText"].Value = (object)constraintElement.Contingency;
                        sqlCommand1.Parameters["@shadowprice"].Value = (object)constraintElement.ShadowPrice;
                        sqlCommand1.Parameters["@numberconstraints"].Value = (object)constraintElement.NumFiring;
                        sqlCommand1.Parameters["@shiftfactor"].Value = (object)constraintElement.ShiftFactor;
                        sqlCommand1.Parameters["@dollarimpact"].Value = (object)constraintElement.Impact;
                        sqlCommand1.Parameters["@updatetime"].Value = (object)constraintElement.UpdateTime;
                        int num2 = sqlCommand1.ExecuteNonQuery();
                        if (this.mMarket != "ERCOT")
                        {
                            if (num2 == 0)
                            {
                                if (this.mMarket != "MISO")
                                {
                                    int newConId = this.CreateNewConID();
                                    sqlCommand2.Parameters["@constraintrtnum"].Value = (object)newConId;
                                }
                                sqlCommand2.Parameters["@MarketDateTime"].Value = (object)constraintElement.MarketDateTimeInterval;
                                sqlCommand2.Parameters["@MonitoredText"].Value = (object)constraintElement.Monitor;
                                sqlCommand2.Parameters["@ContingencyText"].Value = (object)constraintElement.Contingency;
                                sqlCommand2.Parameters["@ShadowPrice"].Value = (object)constraintElement.ShadowPrice;
                                sqlCommand2.Parameters["@NumberConstraints"].Value = (object)constraintElement.NumFiring;
                                sqlCommand2.Parameters["@ShiftFactor"].Value = (object)constraintElement.ShiftFactor;
                                sqlCommand2.Parameters["@DollarImpact"].Value = (object)constraintElement.Impact;
                                sqlCommand2.Parameters["@UpdateTime"].Value = (object)constraintElement.UpdateTime;
                                sqlCommand2.ExecuteNonQuery();
                                SqlDataReader sqlDataReader2 = sqlCommand3.ExecuteReader();
                                while (sqlDataReader2.Read())
                                    constraintID = Convert.ToInt32(sqlDataReader2.GetValue(0));
                                sqlDataReader2.Close();
                            }
                            this.InsertIntoRiskConstraints(constraintID, constraintElement.MarketDateTimeInterval.Date);
                        }
                        this.InsertVector(elementList, constraintElement.ShiftFactor, constraintID, constraintElement.Monitor, constraintElement.Contingency);
                        Console.WriteLine("Saved Vector " + (object)elementList.Count);
                        this.VayuDBConnection.Close();
                        this.VayuDBConnection.Close();
                    }
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
                List<int> intList = new List<int>();
                SqlCommand command1 = this.VayuDBConnection.CreateCommand();
                command1.CommandText = "select distinct ConstraintRTNum from RiskConstraints";
                command1.Connection = this.VayuDBConnection;
                if (this.VayuDBConnection.State == ConnectionState.Closed)
                    this.VayuDBConnection.Open();
                SqlDataReader sqlDataReader = command1.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    int int32 = Convert.ToInt32(sqlDataReader.GetValue(0));
                    intList.Add(int32);
                }
                sqlDataReader.Close();
                if (!intList.Contains(constraintID))
                {
                    SqlCommand command2 = this.VayuDBConnection.CreateCommand();
                    command2.CommandText = "insert into Riskconstraints values(@Date , @ConstraintRtNum , 'RISK')";
                    command2.Parameters.AddWithValue("@Date", (object)dateTime.Date);
                    command2.Parameters.AddWithValue("@ConstraintRtNum", (object)constraintID);
                    command2.Connection = this.VayuDBConnection;
                    command2.ExecuteNonQuery();
                    Console.WriteLine("Inserted New Constraint in RiskConstraints " + (object)constraintID);
                }
                this.VayuDBConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\tException while inserting data into RiskConstraints");
            }
        }

        private Dictionary<string, double> CalculateSensitivity(
          List<Congestion> congestionList,
          double congestionValue,
          long nodeKey)
        {
            Dictionary<string, double> dictionary = new Dictionary<string, double>();
            SqlCommand sqlCommand =  this.mSelectErcotSensitivityCommand;
            if (this.VayuDBConnection.State == ConnectionState.Open)
                this.VayuDBConnection.Close();
            this.VayuDBConnection.Open();
            double num1 = 0.0;
            double num2 = 0.0;
            string key1 = (string)null;
            foreach (Congestion congestion in congestionList)
            {
                bool flag = false;
                if (congestionList.Count > 1)
                {
                    string key2 = congestion.ConstraintName + congestion.ContingencyName + (object)nodeKey;
                    if (!this.mSensitivityHash.ContainsKey(key2))
                    {
                        sqlCommand.Parameters["@monitoredtext"].Value = (object)congestion.ConstraintName;
                        sqlCommand.Parameters["@contingencytext"].Value = (object)congestion.ContingencyName;
                        sqlCommand.Parameters["@nodekey"].Value = (object)nodeKey;
                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        while (sqlDataReader.Read())
                        {
                            double num3 = (double)sqlDataReader.GetDecimal(0);
                            this.mSensitivityHash.Add(key2, num3);
                        }
                        sqlDataReader.Close();
                    }
                    if (this.mSensitivityHash.ContainsKey(key2))
                    {
                        double num3 = this.mSensitivityHash[key2];
                        num1 += num3 * congestion.ShadowPrice;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    key1 = congestion.ConstraintName + "?" + congestion.ContingencyName;
                    num2 = congestion.ShadowPrice;
                }
            }
            if (num2 != 0.0)
            {
                double num3 = (congestionValue - num1) / num2;
                dictionary.Add(key1, num3);
            }
            return dictionary;
        }

        private void SaveSensitivity(
          List<Tuple<int, DateTime>> countList,
          List<List<string>> compareCongestionList)
        {
            this.mStartDate = DateTime.MinValue;
            List<ConstraintElement> constraintElementList1 = new List<ConstraintElement>();
            if (this.VayuDBConnection.State == ConnectionState.Open)
                this.VayuDBConnection.Close();
            this.VayuDBConnection.Open();
            SqlDataReader sqlDataReader1 =  this.mSelectErcotConstraintMasterCommand.ExecuteReader();
            while (sqlDataReader1.Read())
                constraintElementList1.Add(new ConstraintElement()
                {
                    MarketDateTimeInterval = sqlDataReader1.GetDateTime(0),
                    Monitor = sqlDataReader1.GetString(1),
                    Contingency = sqlDataReader1.GetString(2),
                    ShadowPrice = (double)sqlDataReader1.GetDecimal(3),
                    NumFiring = (int)sqlDataReader1.GetDecimal(4),
                    ShiftFactor = (double)sqlDataReader1.GetDecimal(5),
                    Score = 0.0,
                    ImpactRatio = 1.0,
                    Impact = (double)sqlDataReader1.GetDecimal(6),
                    UpdateTime = DateTime.Now
                });
            sqlDataReader1.Close();
            SqlCommand sqlCommand1 = this.mSelectErcotConstraintCommand;
            SqlCommand sqlCommand2 =  this.mSelectErcotVectorCountCommand;
            foreach (Tuple<int, DateTime> count in countList)
            {
                if (this.VayuDBConnection.State == ConnectionState.Open)
                    this.VayuDBConnection.Close();
                this.VayuDBConnection.Open();
                sqlCommand1.Parameters["@marketdatetime"].Value = (object)count.Item2;
                int num1 = 0;
                List<Congestion> congestionList = new List<Congestion>();
                SqlDataReader sqlDataReader2 = sqlCommand1.ExecuteReader();
                while (sqlDataReader2.Read())
                {
                    Congestion congestion = new Congestion();
                    congestion.ConstraintName = sqlDataReader2.GetString(0);
                    congestion.ContingencyName = sqlDataReader2.GetString(1);
                    congestion.ShadowPrice = sqlDataReader2.IsDBNull(2) ? 0.0 : (double)sqlDataReader2.GetDecimal(2);
                    if (congestion.ShadowPrice != 0.0)
                    {
                        congestionList.Add(congestion);
                        ++num1;
                    }
                }
                sqlDataReader2.Close();
                if (congestionList.Count != 0)
                {
                    bool flag = false;
                    foreach (List<string> compareCongestion in compareCongestionList)
                    {
                        if (compareCongestion.Count == congestionList.Count)
                        {
                            foreach (Congestion congestion in congestionList)
                            {
                                string str = congestion.ConstraintName + congestion.ContingencyName;
                                if (compareCongestion.Contains(str))
                                {
                                    flag = true;
                                }
                                else
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                            break;
                    }
                    if (!flag)
                    {
                        List<string> stringList = new List<string>();
                        foreach (Congestion congestion in congestionList)
                        {
                            string str = congestion.ConstraintName + congestion.ContingencyName;
                            stringList.Add(str);
                        }
                        compareCongestionList.Add(stringList);
                        foreach (Congestion congestion in congestionList)
                        {
                            int num2 = 0;
                            sqlCommand2.Parameters["@monitoredtext"].Value = (object)congestion.ConstraintName;
                            sqlCommand2.Parameters["@contingencytext"].Value = (object)congestion.ContingencyName;
                            SqlDataReader sqlDataReader3 = sqlCommand2.ExecuteReader();
                            while (sqlDataReader3.Read())
                                num2 = sqlDataReader3.GetInt32(0);
                            sqlDataReader3.Close();
                            if (num2 == 0)
                            {
                                flag = false;
                                break;
                            }
                            flag = true;
                        }
                        if (!(this.mMarket != "ERCOT") || !flag)
                        {
                            if (num1 > 1)
                            {
                                int num2 = 0;
                                foreach (Congestion congestion in congestionList)
                                {
                                    Congestion compCongestion = congestion;
                                    Dictionary<DateTime, int> dictionary = new Dictionary<DateTime, int>();
                                    int num3 = int.MaxValue;
                                    SqlCommand sqlCommand3 =  this.mSelectErcotNumFireCommand;
                                    sqlCommand3.Parameters["@ContingencyText"].Value = (object)compCongestion.ContingencyName;
                                    sqlCommand3.Parameters["@ConstraintText"].Value = (object)compCongestion.ConstraintName;
                                    sqlCommand3.Parameters["@shadowprice"].Value = (object)2;
                                    sqlCommand3.CommandTimeout = 600000;
                                    SqlDataReader sqlDataReader3 = sqlCommand3.ExecuteReader();
                                    while (sqlDataReader3.Read())
                                    {
                                        DateTime dateTime = sqlDataReader3.GetDateTime(0);
                                        int int32 = sqlDataReader3.GetInt32(1);
                                        if (this.mMarket == "ERCOT")
                                        {
                                            if (dateTime > DateTime.Parse("2018-06-18"))
                                                dictionary.Add(dateTime, int32);
                                        }
                                        else
                                            dictionary.Add(dateTime, int32);
                                    }
                                    sqlDataReader3.Close();
                                    sqlCommand3.Parameters["@shadowprice"].Value = (object)0;
                                    SqlDataReader sqlDataReader4 = sqlCommand3.ExecuteReader();
                                    while (sqlDataReader4.Read())
                                    {
                                        DateTime dateTime = sqlDataReader4.GetDateTime(0);
                                        int int32 = sqlDataReader4.GetInt32(1);
                                        if (dictionary.ContainsKey(dateTime))
                                        {
                                            int num4 = dictionary[dateTime];
                                            if (num4 == int32 && num3 > int32)
                                            {
                                                this.mStartDate = dateTime.AddMinutes(-5.0);
                                                this.mEndDate = dateTime;
                                                num3 = num4;
                                            }
                                        }
                                    }
                                    sqlDataReader4.Close();
                                    int index = constraintElementList1.FindIndex((Predicate<ConstraintElement>)(t => t.Monitor.Equals(compCongestion.ConstraintName) && t.Contingency.Equals(compCongestion.ContingencyName)));
                                    if (index != -1)
                                    {
                                        ConstraintElement constraintElement = constraintElementList1[index];
                                        if (constraintElement.NumFiring <= num3)
                                        {
                                            this.mStartDate = DateTime.MinValue;
                                            ++num2;
                                        }
                                        else if (this.mMarket != "ERCOT")
                                        {
                                            SqlCommand sqlCommand4 =  this.mDeleteErcotMasterConstraintCommand;
                                            sqlCommand4.Parameters["@contingencytext"].Value = (object)constraintElement.Contingency;
                                            sqlCommand4.Parameters["@monitoredtext"].Value = (object)constraintElement.Monitor;
                                            sqlCommand4.ExecuteNonQuery();
                                            this.VayuDBConnection.Close();
                                            this.VayuDBConnection.Close();
                                            return;
                                        }
                                    }
                                    else if (num3 < count.Item1 && num3 < 4)
                                        return;
                                }
                                if (num2 != num1 && num2 != congestionList.Count - 1)
                                    this.mNotFoundDateTimeList.Add(count.Item2);
                                if (num2 == num1 || num2 != congestionList.Count - 1)
                                {
                                    this.mStartDate = DateTime.MinValue;
                                    continue;
                                }
                            }
                            List<Element> elementList = new List<Element>();
                            try
                            {
                                NodePriceLibrary.Node[] nodeArray = (NodePriceLibrary.Node[])null;
                                if (this.mMarket == "ERCOT")
                                    nodeArray = this.GetErcotCongestions(count.Item2, count.Item2);
                                foreach (NodePriceLibrary.Node node in nodeArray)
                                {
                                    NodePriceLibrary.Node item = node;
                                    if (item.NodeId != 57529)
                                        ;
                                    Element element = new Element();
                                    element.Nodekey = item.NodeId;
                                    if (elementList.Where<Element>((Func<Element, bool>)(t => t.Nodekey.Equals(item.NodeId))).ToList<Element>().Count > 0)
                                    {
                                        int index = elementList.FindIndex((Predicate<Element>)(t => t.Nodekey.Equals(item.NodeId)));
                                        elementList.RemoveAt(index);
                                    }
                                    double congestion = (double)item.LmpTimePriceList[0].Lmp.Congestion;
                                    element.Congestion = congestion;
                                    element.SensitivityHash = this.CalculateSensitivity(congestionList, congestion, element.Nodekey);
                                    elementList.Add(element);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine((object)ex);
                                break;
                            }
                            if (elementList.Count<Element>() > 0)
                            {
                                List<ConstraintElement> constraintElementList2 = new List<ConstraintElement>();
                                double num2 = 0.0;
                                foreach (Congestion congestion in congestionList)
                                    num2 += congestion.ShadowPrice;
                                foreach (Congestion congestion in congestionList)
                                {
                                    double num3 = elementList.Min<Element>((Func<Element, double>)(p => p.Congestion));
                                    double num4 = elementList.Max<Element>((Func<Element, double>)(p => p.Congestion));
                                    double num5 = num4 != 0.0 || num3 != 0.0 ? Math.Abs(num4 - num3) / num2 * (congestion.ShadowPrice / num2) : 0.0;
                                    Console.WriteLine(congestion.ConstraintName + " shiftfactor " + (object)num5);
                                    ConstraintElement constraintElement = new ConstraintElement();
                                    constraintElement.MarketDateTimeInterval = count.Item2;
                                    constraintElement.Monitor = congestion.ConstraintName;
                                    constraintElement.Contingency = congestion.ContingencyName;
                                    constraintElement.ShadowPrice = congestion.ShadowPrice;
                                    constraintElement.NumFiring = congestionList.Count;
                                    constraintElement.ShiftFactor = num5;
                                    constraintElement.Score = 0.0;
                                    constraintElement.ImpactRatio = 1.0;
                                    constraintElement.Impact = congestion.ShadowPrice * num5;
                                    constraintElement.UpdateTime = DateTime.Now;
                                    constraintElementList1.Add(constraintElement);
                                    constraintElementList2.Add(constraintElement);
                                }
                                this.InsertConstraint(constraintElementList2, elementList);
                            }
                        }
                    }
                }
            }
        }

        private NodePriceLibrary.Node[] GetErcotCongestions(DateTime startDate, DateTime endDate)
        {
            List<NodePriceLibrary.Node> nodeList = new List<NodePriceLibrary.Node>();
            try
            {
                if (this.VayuDBConnection.State == ConnectionState.Open)
                    this.VayuDBConnection.Close();
                this.VayuDBConnection.Open();
                SqlCommand command = this.VayuDBConnection.CreateCommand();
                SqlCommand sqlCommand = command;
                string[] strArray = new string[8]
                {
          "select NodeKey , lmp , Congestion  from Vayu..NodeLMPMin where MarketDate = '",
          endDate.Date.ToString("yyyy-MM-dd"),
          "' and MarketHour = ",
          endDate.Hour.ToString(),
          " and MarketMin = ",
          null,
          null,
          null
                };
                int num = endDate.Minute;
                strArray[5] = num.ToString();
                strArray[6] = " and second = ";
                num = endDate.Second;
                strArray[7] = num.ToString();
                string str = string.Concat(strArray);
                sqlCommand.CommandText = str;
                command.Connection = this.VayuDBConnection;
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    NodePriceLibrary.Node node = new NodePriceLibrary.Node();
                    node.LmpTimePriceList=(new List<LmpTimePrice>());
                    node.NodeId=(sqlDataReader.IsDBNull(0) ? 0 : Convert.ToInt32(sqlDataReader.GetValue(0)));
                    LmpTimePrice lmpTimePrice = new LmpTimePrice();
                    lmpTimePrice.MarketTime=(startDate);
                    lmpTimePrice.Lmp=(new LMP());
                    lmpTimePrice.Lmp.Price = sqlDataReader.IsDBNull(1) ? double.NaN : Convert.ToDouble(sqlDataReader.GetValue(1));
                    lmpTimePrice.Lmp.Congestion = sqlDataReader.IsDBNull(1) ? double.NaN : Convert.ToDouble(sqlDataReader.GetValue(2));
                    node.LmpTimePriceList.Add(lmpTimePrice);
                    nodeList.Add(node);
                }
                this.VayuDBConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return nodeList.ToArray();
        }

        private void SaveSensitivityErcot(
          List<Tuple<int, DateTime>> countList,
          List<List<string>> compareCongestionList)
        {
            this.mStartDate = DateTime.MinValue;
            List<ConstraintElement> constraintElementList1 = new List<ConstraintElement>();
            if (this.VayuDBConnection.State == ConnectionState.Open)
                this.VayuDBConnection.Close();
            this.VayuDBConnection.Open();
            SqlDataReader sqlDataReader1 =  this.mSelectErcotMaxConstraintDateCommand.ExecuteReader();
            while (sqlDataReader1.Read())
                constraintElementList1.Add(new ConstraintElement()
                {
                    MarketDateTimeInterval = sqlDataReader1.GetDateTime(0),
                    Monitor = sqlDataReader1.GetString(1),
                    Contingency = sqlDataReader1.GetString(2),
                    ShadowPrice = (double)sqlDataReader1.GetDecimal(3),
                    NumFiring = (int)sqlDataReader1.GetDecimal(4),
                    ShiftFactor = (double)sqlDataReader1.GetDecimal(5),
                    Score = 0.0,
                    ImpactRatio = 1.0,
                    Impact = (double)sqlDataReader1.GetDecimal(6),
                    UpdateTime = DateTime.Now
                });
            sqlDataReader1.Close();
            SqlCommand sqlCommand1 = this.mSelectErcotConstraintCommand;
            SqlCommand sqlCommand2 = this.mSelectErcotVectorCountCommand;
            foreach (Tuple<int, DateTime> count in countList)
            {
                if (this.VayuDBConnection.State == ConnectionState.Open)
                    this.VayuDBConnection.Close();
                this.VayuDBConnection.Open();
                sqlCommand1.Parameters["@marketdatetime"].Value = (object)count.Item2;
                int num1 = 0;
                List<Congestion> congestionList = new List<Congestion>();
                SqlDataReader sqlDataReader2 = sqlCommand1.ExecuteReader();
                while (sqlDataReader2.Read())
                {
                    Congestion congestion = new Congestion();
                    congestion.ConstraintName = sqlDataReader2.GetString(0);
                    congestion.ContingencyName = sqlDataReader2.GetString(1);
                    congestion.ShadowPrice = sqlDataReader2.IsDBNull(2) ? 0.0 : (double)sqlDataReader2.GetDecimal(2);
                    if (congestion.ShadowPrice != 0.0)
                    {
                        congestionList.Add(congestion);
                        ++num1;
                    }
                }
                sqlDataReader2.Close();
                if (congestionList.Count != 0)
                {
                    bool flag = false;
                    foreach (List<string> compareCongestion in compareCongestionList)
                    {
                        if (compareCongestion.Count == congestionList.Count)
                        {
                            foreach (Congestion congestion in congestionList)
                            {
                                string str = congestion.ConstraintName + congestion.ContingencyName;
                                if (compareCongestion.Contains(str))
                                {
                                    flag = true;
                                }
                                else
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                            break;
                    }
                    if (!flag)
                    {
                        List<string> stringList = new List<string>();
                        foreach (Congestion congestion in congestionList)
                        {
                            string str = congestion.ConstraintName + congestion.ContingencyName;
                            stringList.Add(str);
                        }
                        compareCongestionList.Add(stringList);
                        foreach (Congestion congestion in congestionList)
                        {
                            int num2 = 0;
                            sqlCommand2.Parameters["@monitoredtext"].Value = (object)congestion.ConstraintName;
                            sqlCommand2.Parameters["@contingencytext"].Value = (object)congestion.ContingencyName;
                            SqlDataReader sqlDataReader3 = sqlCommand2.ExecuteReader();
                            while (sqlDataReader3.Read())
                                num2 = sqlDataReader3.GetInt32(0);
                            sqlDataReader3.Close();
                            if (num2 == 0)
                            {
                                flag = false;
                                break;
                            }
                            flag = true;
                        }
                        if (!flag)
                        {
                            if (num1 > 1)
                            {
                                int num2 = 0;
                                foreach (Congestion congestion in congestionList)
                                {
                                    Congestion compCongestion = congestion;
                                    Dictionary<DateTime, int> dictionary = new Dictionary<DateTime, int>();
                                    int num3 = int.MaxValue;
                                    SqlCommand sqlCommand3 =  this.mSelectErcotNumFireCommand;
                                    sqlCommand3.Parameters["@ContingencyText"].Value = (object)compCongestion.ContingencyName;
                                    sqlCommand3.Parameters["@ConstraintText"].Value = (object)compCongestion.ConstraintName;
                                    sqlCommand3.Parameters["@shadowprice"].Value = (object)2;
                                    sqlCommand3.CommandTimeout = 600000;
                                    SqlDataReader sqlDataReader3 = sqlCommand3.ExecuteReader();
                                    while (sqlDataReader3.Read())
                                    {
                                        DateTime dateTime = sqlDataReader3.GetDateTime(0);
                                        int int32 = sqlDataReader3.GetInt32(1);
                                        dictionary.Add(dateTime, int32);
                                    }
                                    sqlDataReader3.Close();
                                    sqlCommand3.Parameters["@shadowprice"].Value = (object)0;
                                    SqlDataReader sqlDataReader4 = sqlCommand3.ExecuteReader();
                                    while (sqlDataReader4.Read())
                                    {
                                        DateTime dateTime = sqlDataReader4.GetDateTime(0);
                                        int int32 = sqlDataReader4.GetInt32(1);
                                        if (dictionary.ContainsKey(dateTime))
                                        {
                                            int num4 = dictionary[dateTime];
                                            if (num4 == int32 && num3 > int32)
                                            {
                                                this.mStartDate = dateTime.AddMinutes(-5.0);
                                                this.mEndDate = dateTime;
                                                num3 = num4;
                                            }
                                        }
                                    }
                                    sqlDataReader4.Close();
                                    int index = constraintElementList1.FindIndex((Predicate<ConstraintElement>)(t => t.Monitor.Equals(compCongestion.ConstraintName) && t.Contingency.Equals(compCongestion.ContingencyName)));
                                    if (index != -1)
                                    {
                                        if (constraintElementList1[index].NumFiring <= num3)
                                        {
                                            this.mStartDate = DateTime.MinValue;
                                            ++num2;
                                        }
                                    }
                                    else if (num3 <= count.Item1 && num3 < 4)
                                        return;
                                }
                                if (num2 != num1 && num2 != congestionList.Count - 1)
                                    this.mNotFoundDateTimeList.Add(count.Item2);
                                if (num2 == num1 || num2 != congestionList.Count - 1)
                                {
                                    this.mStartDate = DateTime.MinValue;
                                    continue;
                                }
                            }
                            List<Element> elementList = new List<Element>();
                            try
                            {
                                foreach (NodePriceLibrary.Node node in this.RunLmp(count.Item2, count.Item2.AddMinutes(5.0)))
                                {
                                    NodePriceLibrary.Node item = node;
                                    Element element = new Element();
                                    element.Nodekey = item.NodeId;
                                    if (elementList.Where<Element>((Func<Element, bool>)(t => t.Nodekey.Equals(item.NodeId))).ToList<Element>().Count > 0)
                                    {
                                        int index = elementList.FindIndex((Predicate<Element>)(t => t.Nodekey.Equals(item.NodeId)));
                                        elementList.RemoveAt(index);
                                    }
                                    double congestion = (double)item.LmpTimePriceList[0].Lmp.Congestion;
                                    element.Congestion = congestion;
                                    element.SensitivityHash = this.CalculateSensitivity(congestionList, congestion, element.Nodekey);
                                    elementList.Add(element);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine((object)ex);
                                break;
                            }
                            if (elementList.Count<Element>() > 0)
                            {
                                List<ConstraintElement> constraintElementList2 = new List<ConstraintElement>();
                                double num2 = 0.0;
                                foreach (Congestion congestion in congestionList)
                                    num2 += congestion.ShadowPrice;
                                foreach (Congestion congestion in congestionList)
                                {
                                    double num3 = elementList.Min<Element>((Func<Element, double>)(p => p.Congestion));
                                    double num4 = elementList.Max<Element>((Func<Element, double>)(p => p.Congestion));
                                    double num5 = num4 != 0.0 || num3 != 0.0 ? Math.Abs(num4 - num3) / num2 * (congestion.ShadowPrice / num2) : 0.0;
                                    Console.WriteLine(congestion.ConstraintName + " shiftfactor " + (object)num5);
                                    ConstraintElement constraintElement = new ConstraintElement();
                                    constraintElement.MarketDateTimeInterval = count.Item2;
                                    constraintElement.Monitor = congestion.ConstraintName;
                                    constraintElement.Contingency = congestion.ContingencyName;
                                    constraintElement.ShadowPrice = congestion.ShadowPrice;
                                    constraintElement.NumFiring = congestionList.Count;
                                    constraintElement.ShiftFactor = num5;
                                    constraintElement.Score = 0.0;
                                    constraintElement.ImpactRatio = 1.0;
                                    constraintElement.Impact = congestion.ShadowPrice * num5;
                                    constraintElement.UpdateTime = DateTime.Now;
                                    constraintElementList1.Add(constraintElement);
                                    constraintElementList2.Add(constraintElement);
                                }
                                this.InsertConstraint(constraintElementList2, elementList);
                            }
                        }
                    }
                }
            }
        }

        public NodePriceLibrary.Node[] RunLmp(DateTime startDate, DateTime endDate)
        {
            string key = startDate.ToString() + endDate.ToString();
            if (this.mPriceHash.ContainsKey(key))
            {
                Console.WriteLine(DateTime.Now.ToString() + " Got prices " + (object)startDate + " " + (object)this.mPriceHash[key].Length);
                return this.mPriceHash[key];
            }
            NodePriceLibrary.Node[] nodeArray = (NodePriceLibrary.Node[])null;
            try
            {
                NetTcpBinding netTcpBinding = new NetTcpBinding();
                netTcpBinding.Security.Mode = SecurityMode.None;
                netTcpBinding.OpenTimeout = new TimeSpan(0, 30, 0);
                netTcpBinding.SendTimeout = new TimeSpan(0, 12, 0);
                netTcpBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                netTcpBinding.CloseTimeout = new TimeSpan(0, 12, 0);
                netTcpBinding.TransactionFlow = false;
                netTcpBinding.MaxReceivedMessageSize = (long)int.MaxValue;
                netTcpBinding.MaxBufferPoolSize = (long)int.MaxValue;
                netTcpBinding.MaxBufferSize = int.MaxValue;
                netTcpBinding.TransferMode = TransferMode.Buffered;
                netTcpBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                ILMP channel = new ChannelFactory<ILMP>((Binding)netTcpBinding, new EndpointAddress(Vayu.CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress())).CreateChannel();

                int num =  9;
                Console.WriteLine(DateTime.Now.ToString() + " Getting prices " + (object)startDate);
                nodeArray = channel.GetAllFiveMinPrice(num, startDate, endDate, false);
                if ( this.mMarket == "Ercot" && nodeArray.Length > 1000)
                    this.mPriceHash.Add(key, nodeArray);
                Console.WriteLine(DateTime.Now.ToString() + " Got prices " + (object)nodeArray.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex);
            }
            return nodeArray;
        }

        public void StartTimer()
        {
            this.OnTimerEvent((object)null, (ElapsedEventArgs)null);
            this.mTimer.Elapsed += new ElapsedEventHandler(this.OnTimerEvent);
            this.mTimer.Interval = 300000.0;
            this.mTimer.Start();
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }

        private void SaveHourlyImpact(DateTime startDate, DateTime endDate)
        {
            if (this.VayuDBConnection.State == ConnectionState.Open )
            {
                this.VayuDBConnection.Close();
            }
            this.VayuDBConnection.Open();
            SqlCommand sqlCommand1 = this.mDeleteErcotHourlyImpactCommand;
            SqlCommand sqlCommand2 = this.mInsertErcotHourlyImpactCommand;
            SqlCommand sqlCommand3 =  this.mUpdateErcotImpactsCommand;
            for (; startDate < endDate; startDate = startDate.AddHours(1.0))
            {
                Console.WriteLine(DateTime.Now.ToString() + " saving impact " + (object)startDate);
                DateTime date = startDate.Date;
                DateTime dateTime = startDate.AddHours(1.0);
                int num1;
                if (dateTime.Hour != 0)
                {
                    dateTime = startDate.AddHours(1.0);
                    num1 = dateTime.Hour;
                }
                else
                    num1 = 24;
                int num2 = num1;
                sqlCommand1.Parameters["@Time"].Value = (object)date;
                sqlCommand1.Parameters["@Hour"].Value = (object)num2;
                sqlCommand1.ExecuteNonQuery();
                sqlCommand2.Parameters["@start"].Value = (object)date.AddHours((double)(num2 - 1));
                sqlCommand2.Parameters["@Hour"].Value = (object)num2;
                sqlCommand2.Parameters["@end"].Value = (object)date.AddHours((double)num2);
                sqlCommand2.ExecuteNonQuery();
                sqlCommand3.Parameters["@Time"].Value = (object)date;
                sqlCommand3.Parameters["@Hour"].Value = (object)num2;
                sqlCommand3.ExecuteNonQuery();
            }
            this.VayuDBConnection.Close();
        }

        private List<Tuple<int, DateTime>> GetCountList(
          DateTime startDate,
          DateTime endDate)
        {
            if (this.VayuDBConnection.State == ConnectionState.Open)
                this.VayuDBConnection.Close();
            this.VayuDBConnection.Open();
            SqlCommand sqlCommand = this.mSelectErcotConstraintCountCommand;
            List<Tuple<int, DateTime>> tupleList = new List<Tuple<int, DateTime>>();
            if (this.mMarket == "ERCOT")
                sqlCommand.Parameters["@end"].Value = (object)endDate.AddMinutes(5.0);
            else
                sqlCommand.Parameters["@end"].Value = (object)endDate;
            sqlCommand.Parameters["@start"].Value = (object)startDate;
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                DateTime dateTime = sqlDataReader.GetDateTime(0);
                int int32 = sqlDataReader.GetInt32(1);
                tupleList.Add(new Tuple<int, DateTime>(int32, dateTime));
            }
            sqlDataReader.Close();
            this.VayuDBConnection.Close();
            return tupleList;
        }

        public void OnTimerEvent(object source, ElapsedEventArgs e)
        {
           
            
                if (!(this.mMarket == "ERCOT"))
                    return;
                this.CalculateErcotVectors();
            
        }

        private void CalculateErcotVectors()
        {
            this.mTimer.Enabled = false;
            this.InitDB();
            this.DeleteOrphans();
            if (this.VayuDBConnection.State == ConnectionState.Open)
                this.VayuDBConnection.Close();
            this.VayuDBConnection.Open();
            DateTime dateTime1 = DateTime.Today;
            SqlDataReader sqlDataReader1 = this.mSelectErcotMaxConstraintDateCommand.ExecuteReader();
            while (sqlDataReader1.Read())
                dateTime1 = sqlDataReader1.GetDateTime(0);
            this.mNotFoundDateTimeList = new List<DateTime>();
            List<Congestion> congestionList1 = new List<Congestion>();
            List<List<string>> compareCongestionList = new List<List<string>>();
            if (this.mStartDate == DateTime.MinValue)
            {
                this.mStartDate = dateTime1.AddHours(-1.0);
                this.mEndDate = DateTime.Now.AddHours(2.0);
            }
            Console.WriteLine(DateTime.Now.ToString() + " Start Date " + (object)this.mStartDate + " End Date " + (object)this.mEndDate);
            if (this.mFirstTime)
            {
                this.mFirstTime = false;
                this.mStartDate = dateTime1.AddHours(-360.0);
                this.SaveHourlyImpact(this.mStartDate, this.mEndDate);
            }
            this.SaveSensitivity(this.GetCountList(this.mStartDate, this.mEndDate), compareCongestionList);
            if (this.mStartDate != DateTime.MinValue)
            {
                this.mTimer.Enabled = true;
            }
            else
            {
                bool flag1 = false;
                Dictionary<DateTime, double> dictionary1 = new Dictionary<DateTime, double>();
                foreach (DateTime notFoundDateTime in this.mNotFoundDateTimeList)
                {
                    if (!flag1)
                    {
                        List<string> stringList1 = new List<string>();
                        Dictionary<DateTime, List<Congestion>> dictionary2 = new Dictionary<DateTime, List<Congestion>>();
                        Dictionary<DateTime, List<Congestion>> dictionary3 = new Dictionary<DateTime, List<Congestion>>();
                        List<Congestion> congestionList2 = new List<Congestion>();
                        this.mSelectErcotConstraintCommand.Parameters["@marketdatetime"].Value = (object)notFoundDateTime;
                        SqlDataReader sqlDataReader2 = this.mSelectErcotConstraintCommand.ExecuteReader();
                        while (sqlDataReader2.Read())
                        {
                            Congestion congestion = new Congestion();
                            congestion.ConstraintName = sqlDataReader2.GetString(0);
                            congestion.ContingencyName = sqlDataReader2.GetString(1);
                            congestion.ShadowPrice = sqlDataReader2.IsDBNull(2) ? 0.0 : (double)sqlDataReader2.GetDecimal(2);
                            if (congestion.ShadowPrice != 0.0)
                            {
                                congestionList2.Add(congestion);
                                stringList1.Add(congestion.ConstraintName + congestion.ContingencyName);
                                List<Congestion> congestionList3 = new List<Congestion>();
                                if (dictionary2.ContainsKey(notFoundDateTime))
                                {
                                    congestionList3 = dictionary2[notFoundDateTime];
                                    dictionary2.Remove(notFoundDateTime);
                                }
                                congestionList3.Add(congestion);
                                dictionary2.Add(notFoundDateTime, congestionList3);
                            }
                        }
                        sqlDataReader2.Close();
                        List<string> stringList2 = new List<string>();
                        foreach (Congestion congestion in congestionList2)
                        {
                            this.mSelectErcotConstraintIDCommand.Parameters["@monitoredtext"].Value = (object)congestion.ConstraintName;
                            this.mSelectErcotConstraintIDCommand.Parameters["@contingencytext"].Value = (object)congestion.ContingencyName;
                            SqlDataReader sqlDataReader3 = this.mSelectErcotConstraintIDCommand.ExecuteReader();
                            while (sqlDataReader3.Read())
                            {
                                string str = congestion.ConstraintName + congestion.ContingencyName;
                                stringList2.Add(str);
                            }
                            sqlDataReader3.Close();
                        }
                        if (stringList1.Count != 0)
                        {
                            if (stringList1.Count - stringList2.Count == 1)
                            {
                                flag1 = true;
                            }
                            else
                            {
                                this.mSelectConstraintCongestionCountCommand.Parameters["@marketdatetime"].Value = (object)notFoundDateTime;
                                this.mSelectConstraintCongestionCountCommand.Parameters["@constraintname"].Value = (object)congestionList2[0].ConstraintName;
                                this.mSelectConstraintCongestionCountCommand.Parameters["@contingencyname"].Value = (object)congestionList2[0].ContingencyName;
                                SqlDataReader sqlDataReader3 = this.mSelectConstraintCongestionCountCommand.ExecuteReader();
                                while (sqlDataReader3.Read())
                                {
                                    DateTime dateTime2 = sqlDataReader3.GetDateTime(0);
                                    this.mSelectErcotConstraintCommand.Parameters["@marketdatetime"].Value = (object)dateTime2;
                                    SqlDataReader sqlDataReader4 = this.mSelectErcotConstraintCommand.ExecuteReader();
                                    while (sqlDataReader4.Read())
                                    {
                                        Congestion congestion = new Congestion();
                                        congestion.ConstraintName = sqlDataReader4.GetString(0);
                                        congestion.ContingencyName = sqlDataReader4.IsDBNull(1) ? "" : sqlDataReader4.GetString(1);
                                        congestion.ShadowPrice = (double)sqlDataReader4.GetDecimal(2);
                                        if (stringList1.Contains(congestion.ConstraintName + congestion.ContingencyName))
                                        {
                                            List<Congestion> congestionList3 = new List<Congestion>();
                                            if (dictionary2.ContainsKey(dateTime2))
                                            {
                                                congestionList3 = dictionary2[dateTime2];
                                                dictionary2.Remove(dateTime2);
                                            }
                                            congestionList3.Add(congestion);
                                            dictionary2.Add(dateTime2, congestionList3);
                                        }
                                        else
                                        {
                                            List<Congestion> congestionList3 = new List<Congestion>();
                                            if (dictionary3.ContainsKey(dateTime2))
                                            {
                                                congestionList3 = dictionary3[dateTime2];
                                                dictionary3.Remove(dateTime2);
                                            }
                                            congestionList3.Add(congestion);
                                            dictionary3.Add(dateTime2, congestionList3);
                                        }
                                    }
                                    sqlDataReader4.Close();
                                }
                                sqlDataReader3.Close();
                                List<DateTime> list1 = dictionary2.Keys.ToList<DateTime>();
                                bool flag2 = true;
                                List<Congestion> congestionList4 = new List<Congestion>();
                                foreach (DateTime key in list1)
                                {
                                    if (flag2)
                                    {
                                        congestionList4 = dictionary2[key];
                                        flag2 = false;
                                    }
                                    else if (dictionary2[key].Count < congestionList4.Count)
                                        dictionary2.Remove(key);
                                }
                                List<DateTime> list2 = dictionary2.Keys.ToList<DateTime>();
                                Dictionary<int, List<Dictionary<DateTime, List<Congestion>>>> dictionary4 = new Dictionary<int, List<Dictionary<DateTime, List<Congestion>>>>();
                                foreach (DateTime key1 in list2)
                                {
                                    int key2 = 0;
                                    List<Dictionary<DateTime, List<Congestion>>> dictionaryList = new List<Dictionary<DateTime, List<Congestion>>>();
                                    if (dictionary3.ContainsKey(key1))
                                        key2 = dictionary3[key1].Count;
                                    if (dictionary4.ContainsKey(key2))
                                    {
                                        dictionaryList = dictionary4[key2];
                                        dictionary4.Remove(key2);
                                    }
                                    List<Congestion> congestionList3 = dictionary2[key1];
                                    dictionaryList.Add(new Dictionary<DateTime, List<Congestion>>()
                  {
                    {
                      key1,
                      congestionList3
                    }
                  });
                                    dictionary4.Add(key2, dictionaryList);
                                }
                                List<int> list3 = dictionary4.Keys.ToList<int>();
                                list3.Sort();
                                int num1 = 0;
                                double[,] numArray1 = new double[stringList1.Count, stringList1.Count];
                                List<Dictionary<DateTime, List<Congestion>>> dictionaryList1 = new List<Dictionary<DateTime, List<Congestion>>>();
                                foreach (int key in list3)
                                {
                                    foreach (Dictionary<DateTime, List<Congestion>> dictionary5 in dictionary4[key])
                                    {
                                        dictionaryList1.Add(dictionary5);
                                        ++num1;
                                        if (num1 == stringList1.Count)
                                            break;
                                    }
                                    if (num1 == stringList1.Count)
                                        break;
                                }
                                int index1 = 0;
                                List<DateTime> dateTimeList = new List<DateTime>();
                                foreach (Dictionary<DateTime, List<Congestion>> dictionary5 in dictionaryList1)
                                {
                                    foreach (DateTime key in dictionary5.Keys.ToList<DateTime>())
                                    {
                                        int index2 = 0;
                                        if (!dateTimeList.Contains(key))
                                            dateTimeList.Add(key);
                                        foreach (Congestion congestion in dictionary5[key])
                                        {
                                            numArray1[index1, index2] = congestion.ShadowPrice;
                                            ++index2;
                                        }
                                        ++index1;
                                    }
                                }
                                DenseMatrix denseMatrix = Matrix.Create(numArray1);
                                Matrix inverse;
                                try
                                {
                                    inverse = ((LinearOperator)denseMatrix).GetInverse();
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                                Dictionary<long, List<double>> dictionary6 = new Dictionary<long, List<double>>();
                                foreach (DateTime startDate in dateTimeList)
                                {
                                    NodePriceLibrary.Node[] nodeArray = (NodePriceLibrary.Node[])null;
                                      if (this.mMarket == "ERCOT")
                                        nodeArray = this.GetErcotCongestions(startDate, startDate.AddMinutes(5.0));
                                    foreach (NodePriceLibrary.Node node in nodeArray)
                                    {
                                        double congestion = (double)node.LmpTimePriceList[0].Lmp.Congestion;
                                        List<double> doubleList = new List<double>();
                                        if (dictionary6.ContainsKey(node.NodeId))
                                        {
                                            doubleList = dictionary6[node.NodeId];
                                            dictionary6.Remove(node.NodeId);
                                        }
                                        doubleList.Add(congestion);
                                        dictionary6.Add(node.NodeId, doubleList);
                                    }
                                }
                                List<long> list4 = dictionary6.Keys.ToList<long>();
                                double[] numArray2 = new double[stringList1.Count];
                                Dictionary<int, List<Element>> dictionary7 = new Dictionary<int, List<Element>>();
                                foreach (int key in list4)
                                {
                                    List<double> doubleList = dictionary6[key];
                                    int index2 = 0;
                                    foreach (double num2 in doubleList)
                                    {
                                        if (index2 < numArray2.Length)
                                            numArray2[index2] = num2;
                                        ++index2;
                                    }
                                    Vector vector = Matrix.Multiply((Vector)Vector.Create(numArray2), inverse);
                                    int num3 = 0;
                                    foreach (double num2 in doubleList)
                                    {
                                        Element element = new Element();
                                        element.Nodekey = key;
                                        List<Element> elementList = new List<Element>();
                                        if (dictionary7.ContainsKey(num3))
                                        {
                                            elementList = dictionary7[num3];
                                            dictionary7.Remove(num3);
                                        }
                                        element.Congestion = num2;
                                        List<Congestion> congestionList3 = dictionaryList1[0][dateTimeList[0]];
                                        element.SensitivityHash = new Dictionary<string, double>();
                                        if (num3 < vector.Length)
                                        {
                                            element.SensitivityHash.Add(congestionList3[num3].ConstraintName + "?" + congestionList3[num3].ContingencyName,num3);
                                            elementList.Add(element);
                                            dictionary7.Add(num3, elementList);
                                        }
                                        ++num3;
                                    }
                                }
                                int key3 = 0;
                                List<Congestion> congestionList5 = dictionaryList1[0][dateTimeList[0]];
                                double num4 = 0.0;
                                foreach (Congestion congestion in congestionList5)
                                    num4 += congestion.ShadowPrice;
                                foreach (Congestion congestion in congestionList5)
                                {
                                    List<ConstraintElement> constraintElementList = new List<ConstraintElement>();
                                    string str = congestion.ConstraintName + congestion.ContingencyName;
                                    if (!stringList2.Contains(str))
                                    {
                                        double num2 = dictionary7[key3].Min<Element>((Func<Element, double>)(p => p.Congestion));
                                        double num3 = dictionary7[key3].Max<Element>((Func<Element, double>)(p => p.Congestion));
                                        double num5 = num3 != 0.0 || num2 != 0.0 ? Math.Abs(num3 - num2) / num4 * (congestion.ShadowPrice / num4) : 0.0;
                                        Console.WriteLine(congestion.ConstraintName + " shiftfactor " + (object)num5);
                                        constraintElementList.Add(new ConstraintElement()
                                        {
                                            MarketDateTimeInterval = dateTimeList[0],
                                            Monitor = congestion.ConstraintName,
                                            Contingency = congestion.ContingencyName,
                                            ShadowPrice = congestion.ShadowPrice,
                                            NumFiring = dateTimeList.Count,
                                            ShiftFactor = num5,
                                            Score = 0.0,
                                            ImpactRatio = 1.0,
                                            Impact = congestion.ShadowPrice * num5,
                                            UpdateTime = DateTime.Now
                                        });
                                        flag1 = true;
                                    }
                                    this.InsertConstraint(constraintElementList, dictionary7[key3]);
                                    ++key3;
                                }
                            }
                        }
                    }
                    else
                        break;
                }
                this.VayuDBConnection.Close();
                this.mTimer.Enabled = true;
            }
        }

        private void CalculateVectors()
        {
            this.mTimer.Enabled = false;
            this.InitDB();
            this.DeleteOrphans();
            if (this.VayuDBConnection.State == ConnectionState.Open)
                this.VayuDBConnection.Close();
            this.VayuDBConnection.Open();
            DateTime dateTime1 = DateTime.Today;
            SqlDataReader sqlDataReader1 =  this.mSelectErcotMaxConstraintDateCommand.ExecuteReader();
            while (sqlDataReader1.Read())
                dateTime1 = sqlDataReader1.GetDateTime(0);
            this.mNotFoundDateTimeList = new List<DateTime>();
            List<Congestion> congestionList1 = new List<Congestion>();
            List<List<string>> compareCongestionList = new List<List<string>>();
            if (this.mStartDate == DateTime.MinValue)
            {
                this.mStartDate = dateTime1.AddHours(-1.0);
                this.mEndDate = DateTime.Now.AddHours(2.0);
            }
            Console.WriteLine(DateTime.Now.ToString() + " Start Date " + (object)this.mStartDate + " End Date " + (object)this.mEndDate);
            if (this.mFirstTime)
            {
                this.mFirstTime = false;
                this.mStartDate = dateTime1.AddHours(-30.0);
                this.mStartDate = DateTime.Parse("2016-07-18 13:30:00.000");
                this.mEndDate = DateTime.Parse("2016-07-18 13:35:00.000");
                this.SaveHourlyImpact(this.mStartDate, this.mEndDate);
            }
            else
                this.SaveHourlyImpact(dateTime1.AddHours(-30.0), this.mEndDate);
            this.SaveSensitivity(this.GetCountList(this.mStartDate, this.mEndDate), compareCongestionList);
            if (this.mStartDate != DateTime.MinValue)
            {
                this.mTimer.Enabled = true;
            }
            else
            {
                bool flag1 = false;
                Dictionary<DateTime, double> dictionary1 = new Dictionary<DateTime, double>();
                foreach (DateTime notFoundDateTime in this.mNotFoundDateTimeList)
                {
                    if (!flag1)
                    {
                        List<string> stringList1 = new List<string>();
                        Dictionary<DateTime, List<Congestion>> dictionary2 = new Dictionary<DateTime, List<Congestion>>();
                        Dictionary<DateTime, List<Congestion>> dictionary3 = new Dictionary<DateTime, List<Congestion>>();
                        List<Congestion> congestionList2 = new List<Congestion>();
                        this.mSelectErcotConstraintCommand.Parameters["@marketdatetime"].Value = (object)notFoundDateTime;
                        SqlDataReader sqlDataReader2 = this.mSelectErcotConstraintCommand.ExecuteReader();
                        while (sqlDataReader2.Read())
                        {
                            Congestion congestion = new Congestion();
                            congestion.ConstraintName = sqlDataReader2.GetString(0);
                            congestion.ContingencyName = sqlDataReader2.GetString(1);
                            congestion.ShadowPrice = sqlDataReader2.IsDBNull(2) ? 0.0 : (double)sqlDataReader2.GetDecimal(2);
                            if (congestion.ShadowPrice != 0.0)
                            {
                                congestionList2.Add(congestion);
                                stringList1.Add(congestion.ConstraintName + congestion.ContingencyName);
                                List<Congestion> congestionList3 = new List<Congestion>();
                                if (dictionary2.ContainsKey(notFoundDateTime))
                                {
                                    congestionList3 = dictionary2[notFoundDateTime];
                                    dictionary2.Remove(notFoundDateTime);
                                }
                                congestionList3.Add(congestion);
                                dictionary2.Add(notFoundDateTime, congestionList3);
                            }
                        }
                        sqlDataReader2.Close();
                        List<string> stringList2 = new List<string>();
                        foreach (Congestion congestion in congestionList2)
                        {
                            this.mSelectErcotConstraintIDCommand.Parameters["@monitoredtext"].Value = (object)congestion.ConstraintName;
                            this.mSelectErcotConstraintIDCommand.Parameters["@contingencytext"].Value = (object)congestion.ContingencyName;
                            SqlDataReader sqlDataReader3 = this.mSelectErcotConstraintIDCommand.ExecuteReader();
                            while (sqlDataReader3.Read())
                            {
                                string str = congestion.ConstraintName + congestion.ContingencyName;
                                stringList2.Add(str);
                            }
                            sqlDataReader3.Close();
                        }
                        if (stringList1.Count != 0)
                        {
                            if (stringList1.Count - stringList2.Count == 1)
                            {
                                flag1 = true;
                            }
                            else
                            {
                                this.mSelectConstraintCongestionCountCommand.Parameters["@marketdatetime"].Value = (object)notFoundDateTime;
                                this.mSelectConstraintCongestionCountCommand.Parameters["@constraintname"].Value = (object)congestionList2[0].ConstraintName;
                                this.mSelectConstraintCongestionCountCommand.Parameters["@contingencyname"].Value = (object)congestionList2[0].ContingencyName;
                                SqlDataReader sqlDataReader3 = this.mSelectConstraintCongestionCountCommand.ExecuteReader();
                                while (sqlDataReader3.Read())
                                {
                                    DateTime dateTime2 = sqlDataReader3.GetDateTime(0);
                                    this.mSelectErcotConstraintCommand.Parameters["@marketdatetime"].Value = (object)dateTime2;
                                    SqlDataReader sqlDataReader4 = this.mSelectErcotConstraintCommand.ExecuteReader();
                                    while (sqlDataReader4.Read())
                                    {
                                        Congestion congestion = new Congestion();
                                        congestion.ConstraintName = sqlDataReader4.GetString(0);
                                        congestion.ContingencyName = sqlDataReader4.IsDBNull(1) ? "" : sqlDataReader4.GetString(1);
                                        congestion.ShadowPrice = (double)sqlDataReader4.GetDecimal(2);
                                        if (stringList1.Contains(congestion.ConstraintName + congestion.ContingencyName))
                                        {
                                            List<Congestion> congestionList3 = new List<Congestion>();
                                            if (dictionary2.ContainsKey(dateTime2))
                                            {
                                                congestionList3 = dictionary2[dateTime2];
                                                dictionary2.Remove(dateTime2);
                                            }
                                            congestionList3.Add(congestion);
                                            dictionary2.Add(dateTime2, congestionList3);
                                        }
                                        else
                                        {
                                            List<Congestion> congestionList3 = new List<Congestion>();
                                            if (dictionary3.ContainsKey(dateTime2))
                                            {
                                                congestionList3 = dictionary3[dateTime2];
                                                dictionary3.Remove(dateTime2);
                                            }
                                            congestionList3.Add(congestion);
                                            dictionary3.Add(dateTime2, congestionList3);
                                        }
                                    }
                                    sqlDataReader4.Close();
                                }
                                sqlDataReader3.Close();
                                List<DateTime> list1 = dictionary2.Keys.ToList<DateTime>();
                                bool flag2 = true;
                                List<Congestion> congestionList4 = new List<Congestion>();
                                foreach (DateTime key in list1)
                                {
                                    if (flag2)
                                    {
                                        congestionList4 = dictionary2[key];
                                        flag2 = false;
                                    }
                                    else if (dictionary2[key].Count < congestionList4.Count)
                                        dictionary2.Remove(key);
                                }
                                List<DateTime> list2 = dictionary2.Keys.ToList<DateTime>();
                                Dictionary<int, List<Dictionary<DateTime, List<Congestion>>>> dictionary4 = new Dictionary<int, List<Dictionary<DateTime, List<Congestion>>>>();
                                foreach (DateTime key1 in list2)
                                {
                                    int key2 = 0;
                                    List<Dictionary<DateTime, List<Congestion>>> dictionaryList = new List<Dictionary<DateTime, List<Congestion>>>();
                                    if (dictionary3.ContainsKey(key1))
                                        key2 = dictionary3[key1].Count;
                                    if (dictionary4.ContainsKey(key2))
                                    {
                                        dictionaryList = dictionary4[key2];
                                        dictionary4.Remove(key2);
                                    }
                                    List<Congestion> congestionList3 = dictionary2[key1];
                                    dictionaryList.Add(new Dictionary<DateTime, List<Congestion>>()
                  {
                    {
                      key1,
                      congestionList3
                    }
                  });
                                    dictionary4.Add(key2, dictionaryList);
                                }
                                List<int> list3 = dictionary4.Keys.ToList<int>();
                                list3.Sort();
                                int num1 = 0;
                                double[,] numArray1 = new double[stringList1.Count, stringList1.Count];
                                List<Dictionary<DateTime, List<Congestion>>> dictionaryList1 = new List<Dictionary<DateTime, List<Congestion>>>();
                                foreach (int key in list3)
                                {
                                    foreach (Dictionary<DateTime, List<Congestion>> dictionary5 in dictionary4[key])
                                    {
                                        dictionaryList1.Add(dictionary5);
                                        ++num1;
                                        if (num1 == stringList1.Count)
                                            break;
                                    }
                                    if (num1 == stringList1.Count)
                                        break;
                                }
                                int index1 = 0;
                                List<DateTime> dateTimeList = new List<DateTime>();
                                foreach (Dictionary<DateTime, List<Congestion>> dictionary5 in dictionaryList1)
                                {
                                    foreach (DateTime key in dictionary5.Keys.ToList<DateTime>())
                                    {
                                        int index2 = 0;
                                        if (!dateTimeList.Contains(key))
                                            dateTimeList.Add(key);
                                        foreach (Congestion congestion in dictionary5[key])
                                        {
                                            numArray1[index1, index2] = congestion.ShadowPrice;
                                            ++index2;
                                        }
                                        ++index1;
                                    }
                                }
                                DenseMatrix denseMatrix = Matrix.Create(numArray1);
                                Matrix inverse;
                                try
                                {
                                    inverse = ((LinearOperator)denseMatrix).GetInverse();
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                                Dictionary<long, List<double>> dictionary6 = new Dictionary<long, List<double>>();
                                foreach (DateTime startDate in dateTimeList)
                                {
                                    foreach (NodePriceLibrary.Node node in this.RunLmp(startDate, startDate.AddMinutes(5.0)))
                                    {
                                        double congestion = (double)node.LmpTimePriceList[0].Lmp.Congestion;
                                        List<double> doubleList = new List<double>();
                                        if (dictionary6.ContainsKey(node.NodeId))
                                        {
                                            doubleList = dictionary6[node.NodeId];
                                            dictionary6.Remove(node.NodeId);
                                        }
                                        doubleList.Add(congestion);
                                        dictionary6.Add(node.NodeId, doubleList);
                                    }
                                }
                                List<long> list4 = dictionary6.Keys.ToList<long>();
                                double[] numArray2 = new double[stringList1.Count];
                                Dictionary<int, List<Element>> dictionary7 = new Dictionary<int, List<Element>>();
                                foreach (int key in list4)
                                {
                                    List<double> doubleList = dictionary6[key];
                                    int index2 = 0;
                                    foreach (double num2 in doubleList)
                                    {
                                        if (index2 < numArray2.Length)
                                            numArray2[index2] = num2;
                                        ++index2;
                                    }
                                    Vector vector = Matrix.Multiply((Vector)Vector.Create(numArray2), inverse);
                                    int num3 = 0;
                                    foreach (double num2 in doubleList)
                                    {
                                        Element element = new Element();
                                        element.Nodekey = key;
                                        List<Element> elementList = new List<Element>();
                                        if (dictionary7.ContainsKey(num3))
                                        {
                                            elementList = dictionary7[num3];
                                            dictionary7.Remove(num3);
                                        }
                                        element.Congestion = num2;
                                        List<Congestion> congestionList3 = dictionaryList1[0][dateTimeList[0]];
                                        element.SensitivityHash = new Dictionary<string, double>();
                                        if (num3 < vector.Length)
                                        {
                                            element.SensitivityHash.Add(congestionList3[num3].ConstraintName + "?" + congestionList3[num3].ContingencyName, num3);
                                            elementList.Add(element);
                                            dictionary7.Add(num3, elementList);
                                        }
                                        ++num3;
                                    }
                                }
                                int key3 = 0;
                                List<Congestion> congestionList5 = dictionaryList1[0][dateTimeList[0]];
                                double num4 = 0.0;
                                foreach (Congestion congestion in congestionList5)
                                    num4 += congestion.ShadowPrice;
                                foreach (Congestion congestion in congestionList5)
                                {
                                    List<ConstraintElement> constraintElementList = new List<ConstraintElement>();
                                    string str = congestion.ConstraintName + congestion.ContingencyName;
                                    if (!stringList2.Contains(str))
                                    {
                                        double num2 = dictionary7[key3].Min<Element>((Func<Element, double>)(p => p.Congestion));
                                        double num3 = dictionary7[key3].Max<Element>((Func<Element, double>)(p => p.Congestion));
                                        double num5 = num3 != 0.0 || num2 != 0.0 ? Math.Abs(num3 - num2) / num4 * (congestion.ShadowPrice / num4) : 0.0;
                                        Console.WriteLine(congestion.ConstraintName + " shiftfactor " + (object)num5);
                                        constraintElementList.Add(new ConstraintElement()
                                        {
                                            MarketDateTimeInterval = dateTimeList[0],
                                            Monitor = congestion.ConstraintName,
                                            Contingency = congestion.ContingencyName,
                                            ShadowPrice = congestion.ShadowPrice,
                                            NumFiring = dateTimeList.Count,
                                            ShiftFactor = num5,
                                            Score = 0.0,
                                            ImpactRatio = 1.0,
                                            Impact = congestion.ShadowPrice * num5,
                                            UpdateTime = DateTime.Now
                                        });
                                        flag1 = true;
                                    }
                                    this.InsertConstraint(constraintElementList, dictionary7[key3]);
                                    ++key3;
                                }
                            }
                        }
                    }
                    else
                        break;
                }
                this.VayuDBConnection.Close();
                this.VayuDBConnection.Close();
                this.mTimer.Enabled = true;
            }
        }

        public void SaveHourlyImpactManually(DateTime startDate, DateTime endDate, string market)
        {
            this.InitDB();
            if (this.VayuDBConnection.State == ConnectionState.Open )
            {
                this.VayuDBConnection.Close();
            }
            this.VayuDBConnection.Open();
            SqlCommand sqlCommand1 =  this.mDeleteErcotHourlyImpactCommand;
            SqlCommand sqlCommand2 =  this.mInsertErcotHourlyImpactCommand;
            SqlCommand sqlCommand3 =  this.mUpdateErcotImpactsCommand;
            for (; startDate < endDate; startDate = startDate.AddHours(1.0))
            {
                Console.WriteLine(DateTime.Now.ToString() + " saving impact " + (object)startDate);
                DateTime date = startDate.Date;
                DateTime dateTime = startDate.AddHours(1.0);
                int num1;
                if (dateTime.Hour != 0)
                {
                    dateTime = startDate.AddHours(1.0);
                    num1 = dateTime.Hour;
                }
                else
                    num1 = 24;
                int num2 = num1;
                sqlCommand1.Parameters["@Time"].Value = (object)date;
                sqlCommand1.Parameters["@Hour"].Value = (object)num2;
                sqlCommand1.ExecuteNonQuery();
                sqlCommand2.Parameters["@start"].Value = (object)date.AddHours((double)(num2 - 1));
                sqlCommand2.Parameters["@Hour"].Value = (object)num2;
                sqlCommand2.Parameters["@end"].Value = (object)date.AddHours((double)num2);
                sqlCommand2.ExecuteNonQuery();
                sqlCommand3.Parameters["@Time"].Value = (object)date;
                sqlCommand3.Parameters["@Hour"].Value = (object)num2;
                sqlCommand3.ExecuteNonQuery();
            }
            this.VayuDBConnection.Close();
        }
    }
}

