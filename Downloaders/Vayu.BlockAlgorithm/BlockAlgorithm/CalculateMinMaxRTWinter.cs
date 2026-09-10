using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Vayu.NodePriceLibrary;
using Vayu.DBLibrary;
using Vayu.LMP;
using Vayu.BlockAlgorithmLibraryNamespace;
using System.Data;
using System.Configuration;
using System.ServiceModel;
using Vayu.CommonAccessLibrary;

namespace Vayu.BlockAlgorithmNamespace
{
    //Dec-Jan-Feb ---> Winter
    //Mar-Apr-May ---> Spring
    //Jun-July-Aug-Sept ----> Summer
    //Oct - Nov ---> Fall
    public class CalculateMinMaxRTWinter
    {
        private SqlConnection mConnection;
        private SqlCommand mSelectPathsCommand;
        private SqlCommand mSelectWinterPathsCommand;
        private SqlCommand mSelectMaxDateCommand;
        private SqlCommand cmdDeleteCommand;
        private List<SourceSink> mNodeList = new List<SourceSink>();
        private DataTable mDtCalculateMinMaxRTWinter = new DataTable();
        private DataRow dr = null;
        private Dictionary<int, int> mDicWinter = new Dictionary<int, int>();
        //private List<BlockAlgoMinMax> mSeasonsList = new List<BlockAlgoMinMax>();
        private Dictionary<string, BlockAlgoMinMax> mSeasonsList = new Dictionary<string, BlockAlgoMinMax>();
        private SqlCommand mSelectWinterCommand;
        private SqlCommand mSelectSpringCommand;
        private SqlCommand mSelectSummerCommand;
        private SqlCommand mSelectFallCommand;
        public CalculateMinMaxRTWinter()
        {
            InitDB();
            GetEESPaths();
            DateTime mStartDate = DateTime.Today.AddMonths(-2).AddYears(-5);
            DateTime mEndDate = DateTime.Today;
            while (mStartDate <= mEndDate)
            {
                GetHashTables(mStartDate);
                GetDartValues(mStartDate);
                mStartDate = mStartDate.AddDays(1);
                Console.WriteLine("Done so far :" + mStartDate);
            }
        }

        private void GetHashTables(DateTime mStartDate)
        {
            mSeasonsList = new Dictionary<string, BlockAlgoMinMax>();
            string key = "";
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            mConnection.Open();
            SqlDataReader reader = null;
            if ((mStartDate.Month == 1) || (mStartDate.Month == 2) || (mStartDate.Month == 12))
            {
                reader = mSelectWinterCommand.ExecuteReader();
            }
            if ((mStartDate.Month == 3) || (mStartDate.Month == 4) || (mStartDate.Month == 5))
            {
                reader = mSelectSpringCommand.ExecuteReader();
            }
            if ((mStartDate.Month == 6) || (mStartDate.Month == 7) || (mStartDate.Month == 8)  )
            {
                reader = mSelectSummerCommand.ExecuteReader();
            }
            if ((mStartDate.Month == 9)||(mStartDate.Month == 10) || (mStartDate.Month == 11))
            {
                reader = mSelectFallCommand.ExecuteReader();
            }
            while (reader.Read())
            {
                BlockAlgoMinMax mBlockAlgoMinMax = new BlockAlgoMinMax();
                key = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[0]).GetValueOrDefault() + ":" + Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[1]).GetValueOrDefault();
                //mBlockAlgoMinMax.SourceNodeKey = CommonAccessLibrary.CommonDataConversions.GetInt(reader[0]).GetValueOrDefault();
                //mBlockAlgoMinMax.SinkNodeKey = CommonAccessLibrary.CommonDataConversions.GetInt(reader[1]).GetValueOrDefault();
                mBlockAlgoMinMax.RTMin = Convert.ToDouble(reader.GetDecimal(2));
                mBlockAlgoMinMax.RTMax = Convert.ToDouble(reader.GetDecimal(3));
                mBlockAlgoMinMax.RTMinDate = Convert.ToDateTime(reader.GetDateTime(4));
                mBlockAlgoMinMax.RTMaxDate = Convert.ToDateTime(reader.GetDateTime(5));
                mSeasonsList.Add(key, mBlockAlgoMinMax);
            }
        }

        private DateTime GetMaxDate()
        {
            DateTime returndate = new DateTime();
            mConnection.Open();
            SqlDataReader reader = mSelectMaxDateCommand.ExecuteReader();
            while (reader.Read())
            {
                DateTime date1 = reader.GetDateTime(0);
                DateTime date2 = reader.GetDateTime(1);
                if (date1 > date2)
                    returndate = date1;
                else
                    returndate = date2;
            }
            reader.Close();
            return returndate;
        }
        public void GetDartValues(DateTime mDate)
        {
            List<Node> sourceList = GetUptosSource();
            List<Node> sinklist = GetUptosSink();

            DARTNode.GetDartsForUptos(sourceList, sinklist, mDate, mDate.AddDays(1));
            mDtCalculateMinMaxRTWinter.Clear();
            foreach (SourceSink sourceSink in mNodeList)
            {
                double dart = 0.0;
                int hour = 0;
                double min = double.MaxValue;
                double max = double.MinValue;
                DateTime? mindate = null;
                DateTime? maxdate = null;
                string key = sourceSink.SourceNodeKey + ":" + sourceSink.SinkNodeKey;
                if (mSeasonsList.ContainsKey(key))
                {
                    min = Convert.ToDouble(mSeasonsList[key].RTMin);
                    max = Convert.ToDouble(mSeasonsList[key].RTMax);
                    mindate = Convert.ToDateTime(mSeasonsList[key].RTMinDate);
                    maxdate = Convert.ToDateTime(mSeasonsList[key].RTMaxDate);
                }

                for (int row = 0; row <= 23; row++)
                {
                    hour = row + 1;
                    DateTime tempDate = mDate.Date;
                    tempDate = hour == 24 ? tempDate.AddDays(1) : tempDate.AddHours(hour);
                    string sourceKey = tempDate.ToString() + sourceSink.SourceNodeKey.ToString();
                    string sinkKey = tempDate.ToString() + sourceSink.SinkNodeKey.ToString();
                    if (DARTNode.dictRTHash.ContainsKey(sourceKey) && DARTNode.dictDAHash.ContainsKey(sourceKey) &&
                        DARTNode.dictRTHash.ContainsKey(sinkKey) && DARTNode.dictDAHash.ContainsKey(sinkKey) &&
                        !double.IsNaN(DARTNode.dictRTHash[sinkKey]) && !double.IsNaN(DARTNode.dictDAHash[sinkKey]) &&
                        !double.IsNaN(DARTNode.dictRTHash[sourceKey]) && !double.IsNaN(DARTNode.dictDAHash[sourceKey]))
                    {
                        double sourceDa = DARTNode.dictDAHash[sourceKey];
                        double sourceRt = DARTNode.dictRTHash[sourceKey];
                        double sinkDa = DARTNode.dictDAHash[sinkKey];
                        double sinkRt = DARTNode.dictRTHash[sinkKey];
                        double daSpread = (sinkDa - sourceDa);
                        double rtSpread = (sinkRt - sourceRt);
                        dart = rtSpread - daSpread;
                        if (rtSpread < min)
                        {
                            min = rtSpread;
                            //mindate = tempDate;
                        }
                        if (rtSpread > max)
                        {
                            max = rtSpread;
                            //maxdate = tempDate;
                        }
                        if (tempDate < mindate)
                        {
                            mindate = tempDate;
                        }
                        if (tempDate > maxdate)
                        {
                            maxdate = tempDate;
                        }
                    }

                }
                if (mindate.HasValue && maxdate.HasValue)
                {
                    dr = mDtCalculateMinMaxRTWinter.NewRow();
                    dr["SourceNodeKey"] = sourceSink.SourceNodeKey;
                    dr["SinkNodeKey"] = sourceSink.SinkNodeKey;
                    dr["RTMin"] = min;
                    dr["RTMax"] = max;
                    dr["RTMinDate"] = mindate.Value;
                    dr["RTMaxDate"] = maxdate.Value;
                    dr["InsertedDate"] = DateTime.Now;
                    mDtCalculateMinMaxRTWinter.Rows.Add(dr);
                }

            }
            if (mDtCalculateMinMaxRTWinter.Rows.Count > 0)
                SaveInDB(mDtCalculateMinMaxRTWinter, mDate);

        }
        private List<Node> GetUptosSink()
        {
            List<Node> nodeList = new List<Node>();
            Console.WriteLine("Getting All Valid Sinks");
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        Node node = new Node();
                        cmd.Connection = con;
                        cmd.CommandText = "SELECT  DISTINCT   SinkNodeKey , SinkName , SinkNodeId    from EESPathList where  marketkey = 1 order by SinkNodeKey  ";
                        cmd.Connection.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            node.NodeId = Convert.ToInt32(rdr.GetValue(0));
                            node.NodeName = rdr.GetValue(1).ToString();
                            node.PNodeId = Convert.ToInt32(rdr.GetValue(2));
                            nodeList.Add(node);
                        }
                        rdr.Close();
                        cmd.Connection.Close();
                    }
                }
                return nodeList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private List<Node> GetUptosSource()
        {
            List<Node> nodeList = new List<Node>();
            Console.WriteLine("Getting All Valid Sources");
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        Node node = new Node();
                        cmd.Connection = con;
                        cmd.CommandText = "SELECT  DISTINCT  SourceNodeKey , SourceName , SourceNodeId    from EESPathList where  marketkey = 1 order by SourceNodeKey  ";
                        cmd.Connection.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            node.NodeId = Convert.ToInt32(rdr.GetValue(0));
                            node.NodeName = rdr.GetValue(1).ToString();
                            node.PNodeId = Convert.ToInt32(rdr.GetValue(2));
                            nodeList.Add(node);
                        }
                        rdr.Close();
                        cmd.Connection.Close();
                    }
                }
                return nodeList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        private void SaveInDB(DataTable tempDt, DateTime mStartDate)
        {
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            mConnection.Open();
            cmdDeleteCommand.ExecuteNonQuery();
            string storedproc = "";
            if ((mStartDate.Month == 1) || (mStartDate.Month == 2) || (mStartDate.Month == 12))
            {
                storedproc = "[PJM].[UpMergeBlockAlgo_Winter]";
            }
            if ((mStartDate.Month == 3) || (mStartDate.Month == 4) || (mStartDate.Month == 5))
            {
                storedproc = "[PJM].[UpMergeBlockAlgo_Spring]";
            }
            if ((mStartDate.Month == 6) || (mStartDate.Month == 7) || (mStartDate.Month == 8))
            {
                storedproc = "[PJM].[UpMergeBlockAlgo_Summer]";
            }
            if ((mStartDate.Month == 9)||(mStartDate.Month == 10) || (mStartDate.Month == 11))
            {
                storedproc = "[PJM].[UpMergeBlockAlgo_Fall]";
            }
            SqlTransaction transaction = mConnection.BeginTransaction();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkLmpH.DestinationTableName = "[dbo].[BlockAlgoWinterTest]";
                    bkLmpH.BatchSize = 15000;
                    bkLmpH.BulkCopyTimeout = 30000;
                    bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                    bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                    bkLmpH.ColumnMappings.Add("RTMin", "RTMin");
                    bkLmpH.ColumnMappings.Add("RTMax", "RTMax");
                    bkLmpH.ColumnMappings.Add("RTMinDate", "RTMinDate");
                    bkLmpH.ColumnMappings.Add("RTMaxDate", "RTMaxDate");
                    bkLmpH.ColumnMappings.Add("InsertedDate", "InsertedDate");
                    bkLmpH.WriteToServer(tempDt);
                    SqlCommand cmdUpdatenodelmph = new SqlCommand(storedproc, mConnection, transaction);
                    cmdUpdatenodelmph.CommandType = CommandType.StoredProcedure;
                    cmdUpdatenodelmph.CommandTimeout = 30000;
                    cmdUpdatenodelmph.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            mConnection.Close();
        }

        private void GetEESPaths()
        {
            mNodeList = new List<SourceSink>();
            List<int> uniqueList = new List<int>();
            mConnection.Open();
            SqlDataReader reader = mSelectPathsCommand.ExecuteReader();
            while (reader.Read())
            {
                SourceSink mSourceSink = new SourceSink();
                mSourceSink.SourceNodeKey = Convert.ToInt32(reader.GetValue(0));
                mSourceSink.SinkNodeKey = Convert.ToInt32(reader.GetValue(1));
                mNodeList.Add(mSourceSink);
            }
            reader.Close();
            mConnection.Close();

        }
        private void InitDB()
        {
            mConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectPathsCommand = new SqlCommand();
            mSelectPathsCommand.CommandText = "SELECT DISTINCT SourceNodeKey, SinkNodeKey from EESPathList where marketkey = 9 order by sourcenodekey, sinknodekey";
            mSelectPathsCommand.Connection = mConnection;

            mDtCalculateMinMaxRTWinter = new DataTable();
            mDtCalculateMinMaxRTWinter.Columns.Add("SourceNodeKey", typeof(string));
            mDtCalculateMinMaxRTWinter.Columns.Add("SinkNodeKey", typeof(string));
            mDtCalculateMinMaxRTWinter.Columns.Add("RTMin", typeof(double));
            mDtCalculateMinMaxRTWinter.Columns.Add("RTMax", typeof(double));
            mDtCalculateMinMaxRTWinter.Columns.Add("RTMinDate", typeof(DateTime));
            mDtCalculateMinMaxRTWinter.Columns.Add("RTMaxDate", typeof(DateTime));
            mDtCalculateMinMaxRTWinter.Columns.Add("InsertedDate", typeof(DateTime));

            cmdDeleteCommand = new SqlCommand();
            cmdDeleteCommand.CommandText = "truncate table BlockAlgoWinterTest";
            cmdDeleteCommand.Connection = mConnection;

            mSelectWinterPathsCommand = new SqlCommand();
            mSelectWinterPathsCommand.CommandText = "select distinct SourceNodeKey,  SinkNodeKey from BlockAlgoWinteruptos ";
            mSelectWinterPathsCommand.Connection = mConnection;

            mSelectMaxDateCommand = new SqlCommand();
            mSelectMaxDateCommand.CommandText = "select max(RTMinDateDate) ,max(RTMaxDateDate) from BlockAlgoFalluptos  a inner join BlockAlgoSpringuptos  b on a.SourceNodeKey=b.SourceNodeKey and a.SinkNodeKey=b.SinkNodeKey   inner join BlockAlgoSummeruptos  c on c.SourceNodeKey=a.SourceNodeKey and c.SinkNodeKey=a.SinkNodeKey   inner join BlockAlgoWinteruptos  d on d.SourceNodeKey=a.SourceNodeKey and d.SinkNodeKey=a.SinkNodeKey   CROSS APPLY (SELECT max(d) RTMinDateDate FROM (VALUES (a.RTMinDate), (b.RTMinDate),(c.RTMinDate), (d.RTMinDate)) AS MI(d)) MI  CROSS APPLY (SELECT MAX(e) RTMaxDateDate FROM (VALUES (a.RTMaxDate),(b.RTMaxDate),(c.RTMaxDate),(d.RTMaxDate)) AS MI(e)) MX";
            mSelectMaxDateCommand.Connection = mConnection;

            mSelectWinterCommand = new SqlCommand();
            //mSelectWinterCommand.CommandText = "select distinct SourceNodeKey,SinkNodeKey,RTMin,RTMax,RTMinDate,RTMaxDate from  dbo.BlockAlgoWinteruptos  ";
            mSelectWinterCommand.CommandText = "select distinct a.SourceNodeKey,a.SinkNodeKey,RTMin,RTMax,RTMinDate,RTMaxDate from  BlockAlgoWinterUptos a join EESPathList b on a.SourceNodeKey=b.SourceNodeKey and a.SinkNodeKey=b.SinkNodeKey";//
            mSelectWinterCommand.Connection = mConnection;

            mSelectSpringCommand = new SqlCommand();
            //mSelectSpringCommand.CommandText = "select distinct SourceNodeKey,SinkNodeKey,RTMin,RTMax ,RTMinDate,RTMaxDate from  dbo.BlockAlgoSpringuptos ";
            mSelectSpringCommand.CommandText = "select distinct a.SourceNodeKey,a.SinkNodeKey,RTMin,RTMax,RTMinDate,RTMaxDate from  BlockAlgoSpringUptos a join EESPathList b on a.SourceNodeKey=b.SourceNodeKey and a.SinkNodeKey=b.SinkNodeKey";
            mSelectSpringCommand.Connection = mConnection;

            mSelectSummerCommand = new SqlCommand();
            //mSelectSummerCommand.CommandText = "select distinct SourceNodeKey,SinkNodeKey,RTMin,RTMax ,RTMinDate,RTMaxDate from  dbo.BlockAlgoSummeruptos";
            mSelectSummerCommand.CommandText = "select distinct a.SourceNodeKey,a.SinkNodeKey,RTMin,RTMax,RTMinDate,RTMaxDate from  BlockAlgoSummerUptos a join EESPathList b on a.SourceNodeKey=b.SourceNodeKey and a.SinkNodeKey=b.SinkNodeKey";
            mSelectSummerCommand.Connection = mConnection;

            mSelectFallCommand = new SqlCommand();
            //mSelectFallCommand.CommandText = "select distinct SourceNodeKey,SinkNodeKey,RTMin,RTMax,RTMinDate,RTMaxDate from  dbo.BlockAlgoFalluptos ";
            mSelectFallCommand.CommandText = "select distinct a.SourceNodeKey,a.SinkNodeKey,RTMin,RTMax,RTMinDate,RTMaxDate from  BlockAlgoFallUptos a join EESPathList b on a.SourceNodeKey=b.SourceNodeKey and a.SinkNodeKey=b.SinkNodeKey";
            mSelectFallCommand.Connection = mConnection;
        }
    }
    public class BlockAlgoMinMax
    {
        //public int SourceNodeKey { get; set; }
        //public int SinkNodeKey { get; set; }
        public double RTMin { get; set; }
        public double RTMax { get; set; }
        public DateTime RTMinDate { get; set; }
        public DateTime RTMaxDate { get; set; }
    }
}
