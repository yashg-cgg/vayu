using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;
using Vayu.NodePriceLibrary;
using Vayu.LMP;
using System.Data;


namespace Vayu.ErcotDAAnalyser
{
    public class ErcotDAAnalyser
    {
        private SqlConnection mConnection;
        private SqlCommand mSelectPathsCommand;
        private SqlCommand mSelectPreviousDayCommand;
        private SqlCommand mDeleteCommand;
        private List<SourceSink> mNodeList = new List<SourceSink>();
        private DateTime stStartDate, stEndDate;
        private DataTable mDtErcotDAAnalyser = new DataTable();
        private Dictionary<string, ErcotDAMinMax> mPreviousdayList = new Dictionary<string, ErcotDAMinMax>();
        private DataRow dr = null;
        public ErcotDAAnalyser()
        {
            stStartDate = DateTime.Today.AddDays(-15);
            stEndDate = DateTime.Today.AddDays(1);
            InitDB();
            GetEESPathList();

            while (stStartDate <= stEndDate)
            {
                GetPreviouDayData(stStartDate);
                GetDARTValues();
                stStartDate = stStartDate.AddDays(1);
            }
        }

        private void GetPreviouDayData(DateTime stStartDate)
        {
            mPreviousdayList = new Dictionary<string, ErcotDAMinMax>();
            string key = "";
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            mConnection.Open();
            SqlDataReader reader = null;
            mSelectPreviousDayCommand.Parameters["@MarketDate"].Value = stStartDate.AddDays(-1);
            reader = mSelectPreviousDayCommand.ExecuteReader();

            while (reader.Read())
            {
                ErcotDAMinMax mBlockAlgoMinMax = new ErcotDAMinMax();
                key = CommonAccessLibrary.CommonDataConversions.GetInt(reader[0]).GetValueOrDefault() + ":" + CommonAccessLibrary.CommonDataConversions.GetInt(reader[1]).GetValueOrDefault();
                mBlockAlgoMinMax.MinDA = Convert.ToDouble(reader.GetDecimal(2));
                mBlockAlgoMinMax.MaxDA = Convert.ToDouble(reader.GetDecimal(3));
                mBlockAlgoMinMax.TotalDA = Convert.ToDouble(reader.GetDecimal(4));
                mPreviousdayList.Add(key, mBlockAlgoMinMax);
            }
        }

        private void GetDARTValues()
        {
            List<Node> sourceList = GetUptosSource();
            List<Node> sinklist = GetUptosSink();

            DARTNode.GetDartsForUptos(null, null, stStartDate, stStartDate.AddDays(1), true, 9);
            mDtErcotDAAnalyser.Clear();
            foreach (SourceSink sourceSink in mNodeList)
            {
                double dart = 0.0;
                int hour = 0;
                double min = double.MaxValue;
                double max = double.MinValue;
                double? prevmin = 0.0;
                double? prevmax = 0.0;
                double? prevtotal = 0.0;
                double? fprevmin = 0.0;
                double? fprevmax = 0.0;
                double? fprevtotal = 0.0;
                double datotal = 0.0;
                string key = sourceSink.SourceNodeKey + ":" + sourceSink.SinkNodeKey;
                if (mPreviousdayList.ContainsKey(key))
                {
                    prevmin = Convert.ToDouble(mPreviousdayList[key].MinDA);
                    prevmax = Convert.ToDouble(mPreviousdayList[key].MaxDA);
                    prevtotal = Convert.ToDouble(mPreviousdayList[key].TotalDA);
                }

                for (int row = 0; row <= 23; row++)
                {
                    hour = row + 1;
                    DateTime tempDate = stStartDate.Date;
                    tempDate = hour == 24 ? tempDate.AddDays(1) : tempDate.AddHours(hour);
                    string sourceKey = tempDate.ToString() + sourceSink.SourceNodeKey.ToString();
                    string sinkKey = tempDate.ToString() + sourceSink.SinkNodeKey.ToString();
                    if (DARTNode.dictDAHash.ContainsKey(sourceKey) &&
                        DARTNode.dictDAHash.ContainsKey(sinkKey) &&
                        !double.IsNaN(DARTNode.dictDAHash[sinkKey]) &&
                         !double.IsNaN(DARTNode.dictDAHash[sourceKey]))
                    {
                        double sourceDa = DARTNode.dictDAHash[sourceKey];
                        double sinkDa = DARTNode.dictDAHash[sinkKey];
                        double daSpread = (sinkDa - sourceDa);
                        datotal += daSpread;
                        if (daSpread < min)
                        {
                            min = daSpread;
                        }
                        if (daSpread > max)
                        {
                            max = daSpread;
                        }
                    }
                }
                if (prevmin != 0)
                    fprevmin = ((min - prevmin) / Math.Abs(prevmin.Value)) * 100;
                if (prevmax != 0)
                    fprevmax = ((max - prevmax) / Math.Abs(prevmax.Value)) * 100;
                if (prevtotal != 0)
                    fprevtotal = ((datotal - prevtotal) / Math.Abs(prevtotal.Value)) * 100;
                //  if (mindate.HasValue && maxdate.HasValue)
                {
                    dr = mDtErcotDAAnalyser.NewRow();
                    dr["SourceNodeKey"] = sourceSink.SourceNodeKey;
                    dr["SinkNodeKey"] = sourceSink.SinkNodeKey;
                    dr["MinDA"] = Math.Round(min, 2);
                    dr["MaxDA"] = Math.Round(max, 2);
                    dr["TotalDA"] = Math.Round(datotal, 2);
                    dr["PerChangeInMinDA"] = Math.Round(fprevmin.Value, 2);
                    dr["PerChangeInMaxDA"] = Math.Round(fprevmax.Value, 2);
                    dr["PerChangeInTotalDA"] = Math.Round(fprevtotal.Value, 2);
                    dr["MarketDate"] = stStartDate;
                    dr["InsertedDate"] = DateTime.Now;
                    mDtErcotDAAnalyser.Rows.Add(dr);
                }

            }
            if (mDtErcotDAAnalyser.Rows.Count > 0)
                SaveInDB(mDtErcotDAAnalyser, stStartDate);
        }

        private void SaveInDB(DataTable mDtErcotDAAnalyser, DateTime stStartDate)
        {
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            mConnection.Open();
            mDeleteCommand.ExecuteNonQuery();
            SqlTransaction transaction = mConnection.BeginTransaction();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkLmpH.DestinationTableName = "[dbo].[ErcotDAAnalysertest]";
                    bkLmpH.BatchSize = 20000;
                    bkLmpH.BulkCopyTimeout = 30000;
                    bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                    bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                    bkLmpH.ColumnMappings.Add("MinDA", "MinDA");
                    bkLmpH.ColumnMappings.Add("MaxDA", "MaxDA");
                    bkLmpH.ColumnMappings.Add("TotalDA", "TotalDA");
                    bkLmpH.ColumnMappings.Add("PerChangeInMinDA", "PerChangeInMinDA");
                    bkLmpH.ColumnMappings.Add("PerChangeInMaxDA", "PerChangeInMaxDA");
                    bkLmpH.ColumnMappings.Add("PerChangeInTotalDA", "PerChangeInTotalDA");
                    bkLmpH.ColumnMappings.Add("MarketDate", "MarketDate");
                    bkLmpH.ColumnMappings.Add("InsertedDate", "InsertedDate");
                    bkLmpH.WriteToServer(mDtErcotDAAnalyser);
                    SqlCommand cmdUpdatenodelmph = new SqlCommand("[dbo].[UpMergeErcotDAAnalyser]", mConnection, transaction);
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

        private void GetEESPathList()
        {
            mNodeList = new List<SourceSink>();
            List<int> uniqueList = new List<int>();
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
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
            mSelectPathsCommand.CommandText = "select DISTINCT A.NodeKey As SourceNodeKey, C.NodeKey As sinkNodekey from [dbo].[EESPathList] B inner join Node A on A.Nodename=B.SourceNodeName " +
                                              " INNER JOIN Node C ON B.SinkNodeName=C.NodeName ";
            mSelectPathsCommand.Connection = mConnection;

            mSelectPreviousDayCommand = new SqlCommand();
            mSelectPreviousDayCommand.CommandText = "select * from ErcotDAAnalyser where MarketDate=@MarketDate";
            mSelectPreviousDayCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectPreviousDayCommand.Connection = mConnection;

            mDeleteCommand = new SqlCommand();
            mDeleteCommand.CommandText = "Truncate table ErcotDAAnalysertest";
            mDeleteCommand.Connection = mConnection;

            mDtErcotDAAnalyser = new DataTable();
            mDtErcotDAAnalyser.Columns.Add("SourceNodeKey", typeof(int));
            mDtErcotDAAnalyser.Columns.Add("SinkNodeKey", typeof(int));
            mDtErcotDAAnalyser.Columns.Add("MinDA", typeof(double));
            mDtErcotDAAnalyser.Columns.Add("MaxDA", typeof(double));
            mDtErcotDAAnalyser.Columns.Add("TotalDA", typeof(double));
            mDtErcotDAAnalyser.Columns.Add("PerChangeInMinDA", typeof(double));
            mDtErcotDAAnalyser.Columns.Add("PerChangeInMaxDA", typeof(double));
            mDtErcotDAAnalyser.Columns.Add("PerChangeInTotalDA", typeof(double));
            mDtErcotDAAnalyser.Columns.Add("MarketDate", typeof(DateTime));
            mDtErcotDAAnalyser.Columns.Add("InsertedDate", typeof(DateTime));


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
                        cmd.CommandText = "SELECT  DISTINCT  SourceNodeKey , SourceName   from EESPathList where  marketkey = 9 order by SourceNodeKey  ";
                        
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            node.NodeId = Convert.ToInt32(rdr.GetValue(0));
                            node.NodeName = rdr.GetValue(1).ToString();
                            //  node.PNodeId = Convert.ToInt32(rdr.GetValue(2));
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
                        cmd.CommandText = "SELECT  DISTINCT   SinkNodeKey , SinkName     from EESPathList where  marketkey = 9 order by SinkNodeKey  ";
                        
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            node.NodeId = Convert.ToInt32(rdr.GetValue(0));
                            node.NodeName = rdr.GetValue(1).ToString();
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
    }
}
