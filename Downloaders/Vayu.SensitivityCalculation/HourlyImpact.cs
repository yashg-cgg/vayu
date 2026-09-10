//using Vayu.CommonAccessLibrary;
using Extreme;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using Vayu.NodePriceLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Timers;
using Vayu.CommonAccessLibrary;

namespace Vayu.SensitivityCalculation
{
    public class HourlyImpact
    {
        private string mMarket;
        private SqlConnection DBConnectionVayu;
        private SqlCommand mDeleteErcotMasterConstraintCommand;
        private SqlCommand mSelectErcotConstraintMasterCommand;
        private SqlCommand mSelectErcotConstraintCountCommand;
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
        private SqlCommand mSelectErcotNumFireCommand;
        private SqlCommand mSelectErcotOrphanVectorCommand;
        private SqlCommand mSelectErcotOrphanImpactCommand;
        private SqlCommand mDeleteErcotOrphanVectorCommand;
        private SqlCommand mDeleteErcotOrphanImpactCommand;
        private Dictionary<string, NodePriceLibrary.Node[]> mPriceHash = new Dictionary<string, NodePriceLibrary.Node[]>();
        private Timer mTimer = new Timer();
        private bool mFirstTime = true;
        private DateTime mStartDate;
        private DateTime mEndDate;
        private DataTable mVectorTable = new DataTable();
        private List<DateTime> mNotFoundDateTimeList = new List<DateTime>();
        private Dictionary<string, double> mSensitivityHash = new Dictionary<string, double>();
       

        public HourlyImpact(string market)
        {
            
            this.mMarket = market;
        }

        private void InitDB()
        {
            this.DBConnectionVayu = new VayuDBConnection().GetInstance().GetSqlConnection();
            
            
            this.mSelectErcotNumFireCommand = new SqlCommand();
            this.mSelectErcotNumFireCommand.CommandText = "select MarketDateTime, COUNT(*) from Vayu..ConstraintRT (nolock) where abs(shadowprice) > @shadowprice and MarketDateTime in (select MarketDateTime from Vayu..ConstraintRT (nolock) where ContingencyText = @ContingencyText and ConstraintText = @ConstraintText) and marketdatetime > (select MIN(marketdate) from Vayu..NodeLMPMin (nolock) where congestion is not null ) group by MarketDateTime order by MarketDateTime desc";
            this.mSelectErcotNumFireCommand.Parameters.AddWithValue("@ContingencyText", (object)"ContingencyText");
            this.mSelectErcotNumFireCommand.Parameters.AddWithValue("@ConstraintText", (object)"ConstraintText");
            this.mSelectErcotNumFireCommand.Parameters.AddWithValue("@shadowprice", (object)"shadowprice");
            this.mSelectErcotNumFireCommand.Connection = this.DBConnectionVayu;
             
            
            this.mDeleteErcotVectorCommand = new SqlCommand();
            this.mDeleteErcotVectorCommand.CommandText = "delete Vayu..RTMasterVector where constraintrtnum = @constraintrtnum and source is null ";
            this.mDeleteErcotVectorCommand.Parameters.AddWithValue("@constraintrtnum", (object)"constraintrtnum");
            this.mDeleteErcotVectorCommand.Connection = this.DBConnectionVayu;
             
            this.mDeleteErcotMasterConstraintCommand = new SqlCommand();
            this.mDeleteErcotMasterConstraintCommand.CommandText = "delete RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext";
            this.mDeleteErcotMasterConstraintCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mDeleteErcotMasterConstraintCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mDeleteErcotMasterConstraintCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectErcotMaxConstraintDateCommand = new SqlCommand();
            this.mSelectErcotMaxConstraintDateCommand.CommandText = "select max(marketdatetime) from Vayu..RTMasterConstraint";
            this.mSelectErcotMaxConstraintDateCommand.Connection = this.DBConnectionVayu;
             
            this.mDeleteErcotHourlyImpactCommand = new SqlCommand();
            this.mDeleteErcotHourlyImpactCommand.CommandText = "delete RTImpact where date =  Convert(Date, @Time) and hour = @Hour";
            this.mDeleteErcotHourlyImpactCommand.Parameters.AddWithValue("@Time", (object)"Time");
            this.mDeleteErcotHourlyImpactCommand.Parameters.AddWithValue("@Hour", (object)"Hour");
            this.mDeleteErcotHourlyImpactCommand.Connection = this.DBConnectionVayu;
             
            this.mInsertErcotHourlyImpactCommand = new SqlCommand();
            this.mInsertErcotHourlyImpactCommand.CommandText = "INSERT INTO Vayu..RTImpact(ConstraintRTNum, Date, Hour, ShadowPrice) SELECT B.ConstraintRTNum, Convert(Date, @start) as Date, @Hour as Hour, abs(sum(A.ShadowPrice)) as ShadowPrice from Vayu..ConstraintRT as A inner join RTMasterConstraint as B on A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  where A.MarketDateTime >= @start and A.MarketDateTime < @end and ConstraintText <> 'None' group by ConstraintText, A.ContingencyText, B.ConstraintRTNum";
            this.mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@start", (object)"a.marketdatetime");
            this.mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@end", (object)"a.marketdatetime");
            this.mInsertErcotHourlyImpactCommand.Parameters.AddWithValue("@Hour", (object)"Hour");
            this.mInsertErcotHourlyImpactCommand.Connection = this.DBConnectionVayu;
             
            this.mUpdateErcotImpactsCommand = new SqlCommand();
            this.mUpdateErcotImpactsCommand.CommandText = "Update RTImpact set impact = A.shadowprice*B.shiftfactor from RTImpact A inner join RTMasterConstraint B on A.ConstraintRTNum = B.ConstraintRTNum where date = Convert(Date, @Time) and hour = @Hour and B.Shiftfactor is not null ";
            this.mUpdateErcotImpactsCommand.Parameters.AddWithValue("@Time", (object)"Time");
            this.mUpdateErcotImpactsCommand.Parameters.AddWithValue("@Hour", (object)"Hour");
            this.mUpdateErcotImpactsCommand.Connection = this.DBConnectionVayu;
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = "SELECT top 1 a.ConstraintRTNum, a.NodeKey, a.Sensitivity, a.SensitivityNormal FROM RTMasterVector a";
            sqlCommand.Connection = this.DBConnectionVayu;
            new SqlDataAdapter() { SelectCommand = sqlCommand }.Fill(this.mVectorTable);
             
            this.mSelectErcotConstraintIDCommand = new SqlCommand();
            this.mSelectErcotConstraintIDCommand.CommandText = "select constraintrtnum, marketdatetime, numberconstraints from Vayu..RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext";
            this.mSelectErcotConstraintIDCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mSelectErcotConstraintIDCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mSelectErcotConstraintIDCommand.Connection = this.DBConnectionVayu;
             
             
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
            this.mInsertErcotConstraintCommand.Connection = this.DBConnectionVayu;
             
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
            this.mUpdateErcotConstraintCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectErcotConstraintCommand = new SqlCommand();
            this.mSelectErcotConstraintCommand.CommandText = "select constrainttext, contingencytext, shadowprice from Vayu..constraintrt where marketdatetime = @marketdatetime and shadowprice is not null and shadowprice <> 0";
            this.mSelectErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", (object)"marketdatetime");
            this.mSelectErcotConstraintCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectMarketDateTimeCommand = new SqlCommand();
            this.mSelectMarketDateTimeCommand.CommandText = "select marketdatetime from MISO.constraintrt where constraintname = @constraintname and contingencyname = @contingencyname and marketdatetime > @marketdatetime order by marketdatetime";
            this.mSelectMarketDateTimeCommand.Parameters.AddWithValue("@constraintname", (object)"constraintname");
            this.mSelectMarketDateTimeCommand.Parameters.AddWithValue("@contingencyname", (object)"contingencyname");
            this.mSelectMarketDateTimeCommand.Parameters.AddWithValue("@marketdatetime", (object)"marketdatetime");
            this.mSelectMarketDateTimeCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectErcotConstraintCountCommand = new SqlCommand();
            this.mSelectErcotConstraintCountCommand.CommandText = "select MarketDateTime, COUNT(*) a from Vayu..constraintrt where MarketDateTime > @start and MarketDateTime < @end and shadowprice <> 0 and shadowprice is not null group by marketdatetime   order by a, marketdatetime desc";
            this.mSelectErcotConstraintCountCommand.Parameters.AddWithValue("@start", (object)"marketdatetime");
            this.mSelectErcotConstraintCountCommand.Parameters.AddWithValue("@end", (object)"marketdatetime");
            this.mSelectErcotConstraintCountCommand.Connection = this.DBConnectionVayu;
            
             
            this.mSelectErcotSensitivityCommand = new SqlCommand();
            this.mSelectErcotSensitivityCommand.CommandText = "select sensitivity from Vayu..RTMasterVector (NOLOCK) where nodekey = @nodekey and constraintrtnum = (select constraintrtnum from Vayu..RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext)";
            this.mSelectErcotSensitivityCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mSelectErcotSensitivityCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mSelectErcotSensitivityCommand.Parameters.AddWithValue("@nodekey", (object)"nodekey");
            this.mSelectErcotSensitivityCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectErcotConstraintMasterCommand = new SqlCommand();
            this.mSelectErcotConstraintMasterCommand.CommandText = "select marketdatetime, monitoredtext, contingencytext, shadowprice, numberconstraints, shiftfactor, dollarimpact from Vayu..RTMasterConstraint where marketdatetime > dateadd(month, -8, getdate())";
            this.mSelectErcotConstraintMasterCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectErcotVectorCountCommand = new SqlCommand();
            this.mSelectErcotVectorCountCommand.CommandText = "select count(*) from Vayu..RTMasterVector where constraintrtnum = (select constraintrtnum from Vayu..RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext )";
            this.mSelectErcotVectorCountCommand.Parameters.AddWithValue("@monitoredtext", (object)"monitoredtext");
            this.mSelectErcotVectorCountCommand.Parameters.AddWithValue("@contingencytext", (object)"contingencytext");
            this.mSelectErcotVectorCountCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectErcotOrphanVectorCommand = new SqlCommand();
            this.mSelectErcotOrphanVectorCommand.CommandText = "select distinct constraintrtnum from Vayu..RTMasterVector where ConstraintRTNum not in (select ConstraintRTNum from Vayu..RTMasterConstraint)";
            this.mSelectErcotOrphanVectorCommand.Connection = this.DBConnectionVayu;
             
            this.mSelectErcotOrphanImpactCommand = new SqlCommand();
            this.mSelectErcotOrphanImpactCommand.CommandText = "select distinct constraintrtnum from RTImpact where ConstraintRTNum not in (select ConstraintRTNum from RTMasterConstraint)";
            this.mSelectErcotOrphanImpactCommand.Connection = this.DBConnectionVayu;
             
            this.mDeleteErcotOrphanVectorCommand = new SqlCommand();
            this.mDeleteErcotOrphanVectorCommand.CommandText = "delete RTMasterVector where constraintrtnum = @constraintrtnum";
            this.mDeleteErcotOrphanVectorCommand.Parameters.AddWithValue("@constraintrtnum", (object)"constraintrtnum");
            this.mDeleteErcotOrphanVectorCommand.Connection = this.DBConnectionVayu;
             
            this.mDeleteErcotOrphanImpactCommand = new SqlCommand();
            this.mDeleteErcotOrphanImpactCommand.CommandText = "delete RTImpact where constraintrtnum = @constraintrtnum";
            this.mDeleteErcotOrphanImpactCommand.Parameters.AddWithValue("@constraintrtnum", (object)"constraintrtnum");
            this.mDeleteErcotOrphanImpactCommand.Connection = this.DBConnectionVayu;
             
        }

        private void DeleteOrphans()
        {
            Console.WriteLine("Deleting Orphans");
            List<int> intList = new List<int>();
            if (this.DBConnectionVayu.State == ConnectionState.Open || this.DBConnectionVayu.State == ConnectionState.Open)
            {
                this.DBConnectionVayu.Close();
            }
            this.DBConnectionVayu.Open();
            SqlDataReader sqlDataReader1 =  this.mSelectErcotOrphanVectorCommand.ExecuteReader();
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
            SqlCommand sqlCommand1 =  this.mDeleteErcotOrphanVectorCommand;
            SqlCommand sqlCommand2 =  this.mDeleteErcotOrphanImpactCommand;
            foreach (int num in intList)
            {
                sqlCommand1.Parameters["@constraintrtnum"].Value = (object)num;
                sqlCommand1.ExecuteNonQuery();
                sqlCommand2.Parameters["@constraintrtnum"].Value = (object)num;
                sqlCommand2.ExecuteNonQuery();
            }
            this.DBConnectionVayu.Close();
            this.DBConnectionVayu.Close();
            Console.WriteLine("Deleted Orphans");
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
            if (this.DBConnectionVayu.State == ConnectionState.Open )
            {
                this.DBConnectionVayu.Close();
            }
            this.DBConnectionVayu.Open();
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
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(this.DBConnectionVayu);
                sqlBulkCopy.BulkCopyTimeout = 600;
                sqlBulkCopy.DestinationTableName =  "Vayu..RTMasterVector";
                if (this.DBConnectionVayu.State == ConnectionState.Closed)
                    this.DBConnectionVayu.Open();
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
            this.DBConnectionVayu.Close();
        }

        private List<int> GetErcotSensetivities(int constraintID)
        {
            List<int> intList = new List<int>();
            try
            {
                if (this.DBConnectionVayu.State == ConnectionState.Closed)
                    this.DBConnectionVayu.Open();
                SqlCommand command = this.DBConnectionVayu.CreateCommand();
                command.Connection = this.DBConnectionVayu;
                command.CommandText = "select distinct NodeKey from Vayu..RTMasterVector where ConstraintRTNum =" + (object)constraintID;
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    int int32 = Convert.ToInt32(sqlDataReader.GetValue(0));
                    intList.Add(int32);
                }
                this.DBConnectionVayu.Close();
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
            SqlCommand sqlCommand1 = this.mUpdateErcotConstraintCommand;
            SqlCommand sqlCommand2 = this.mInsertErcotConstraintCommand;
            SqlCommand sqlCommand3 =  this.mSelectErcotConstraintIDCommand;
            try
            {
                foreach (ConstraintElement constraintElement in constraintElementList)
                {
                    if (this.DBConnectionVayu.State == ConnectionState.Open)
                    {
                        this.DBConnectionVayu.Close();
                    }
                    this.DBConnectionVayu.Open();
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
                        
                        this.InsertVector(elementList, constraintElement.ShiftFactor, constraintID, constraintElement.Monitor, constraintElement.Contingency);
                        Console.WriteLine("Saved Vector " + (object)elementList.Count);
                        this.DBConnectionVayu.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 1;
        }


        private Dictionary<string, double> CalculateSensitivity(
          List<Congestion> congestionList,
          double congestionValue,
          long nodeKey)
        {
            Dictionary<string, double> dictionary = new Dictionary<string, double>();
            SqlCommand sqlCommand =  this.mSelectErcotSensitivityCommand;
            if (this.DBConnectionVayu.State == ConnectionState.Open)
                this.DBConnectionVayu.Close();
            this.DBConnectionVayu.Open();
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
            if (this.DBConnectionVayu.State == ConnectionState.Open)
                this.DBConnectionVayu.Close();
            this.DBConnectionVayu.Open();
            SqlDataReader sqlDataReader1 = this.mSelectErcotConstraintMasterCommand.ExecuteReader();
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
                if (this.DBConnectionVayu.State == ConnectionState.Open)
                    this.DBConnectionVayu.Close();
                this.DBConnectionVayu.Open();
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
                                    SqlCommand sqlCommand3 =  this.mSelectErcotNumFireCommand ;
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
                if (this.DBConnectionVayu.State == ConnectionState.Open)
                    this.DBConnectionVayu.Close();
                this.DBConnectionVayu.Open();
                SqlCommand command = this.DBConnectionVayu.CreateCommand();
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
                command.Connection = this.DBConnectionVayu;
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    NodePriceLibrary.Node node = new NodePriceLibrary.Node();
                    node.LmpTimePriceList = (new List<LmpTimePrice>());

                    node.NodeId = (sqlDataReader.IsDBNull(0) ? 0 : Convert.ToInt32(sqlDataReader.GetValue(0)));
                    LmpTimePrice lmpTimePrice = new LmpTimePrice();
                    lmpTimePrice.MarketTime = (startDate);
                    lmpTimePrice.Lmp = (new LMP());
                    lmpTimePrice.Lmp.Price = sqlDataReader.IsDBNull(1) ? double.NaN : Convert.ToDouble(sqlDataReader.GetValue(1));
                    lmpTimePrice.Lmp.Congestion = sqlDataReader.IsDBNull(1) ? double.NaN : Convert.ToDouble(sqlDataReader.GetValue(2));
                    node.LmpTimePriceList.Add(lmpTimePrice);
                    nodeList.Add(node);
                }
                this.DBConnectionVayu.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return nodeList.ToArray();
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
                ;
        }

        private void SaveHourlyImpact(DateTime startDate, DateTime endDate)
        {
            if (this.DBConnectionVayu.State == ConnectionState.Open)
            {
                this.DBConnectionVayu.Close();
            }
            this.DBConnectionVayu.Open();
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
            this.DBConnectionVayu.Close();
        }

        private List<Tuple<int, DateTime>> GetCountList(
          DateTime startDate,
          DateTime endDate)
        {
            if (this.DBConnectionVayu.State == ConnectionState.Open)
                this.DBConnectionVayu.Close();
            this.DBConnectionVayu.Open();
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
            this.DBConnectionVayu.Close();
            return tupleList;
        }

        public void OnTimerEvent(object source, ElapsedEventArgs e)
        {
           
            {
                if (!(this.mMarket == "ERCOT"))
                    return;
                this.CalculateErcotVectors();
            }
        }

        private void CalculateErcotVectors()
        {
            this.mTimer.Enabled = false;
            this.InitDB();
            this.DeleteOrphans();
            if (this.DBConnectionVayu.State == ConnectionState.Open)
                this.DBConnectionVayu.Close();
            this.DBConnectionVayu.Open();
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
                
                this.mTimer.Enabled = true;
            }
        }


        public void SaveHourlyImpactManually(DateTime startDate, DateTime endDate, string market)
        {
            this.InitDB();
            if (this.DBConnectionVayu.State == ConnectionState.Open)
            {
                this.DBConnectionVayu.Close();
            }
            this.DBConnectionVayu.Open();
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
            this.DBConnectionVayu.Close();
        }
    }
}

