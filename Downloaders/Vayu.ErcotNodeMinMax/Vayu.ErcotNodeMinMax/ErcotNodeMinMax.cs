using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace  Vayu.ErcotNodeMinMax
{
    public class ErcotNodeMinMax
    {
        List<PathHelper> validCRRPathList;
        Dictionary<long, string> validCRRNodeDict;
        DataTable dtMinMax = new DataTable();
        DataRow drMinMax = null;
        SqlConnection DBConnection = new SqlConnection();
        public ErcotNodeMinMax()
        {
            INDB();
            dtMinMax = new DataTable();
            dtMinMax.Columns.Add("SourceName", typeof(string));
            dtMinMax.Columns.Add("SinkName", typeof(string));
            dtMinMax.Columns.Add("MinDA", typeof(long));
            dtMinMax.Columns.Add("MaxDA", typeof(long));
            dtMinMax.Columns.Add("TimeOfUse", typeof(string));
            GetValidCRRNode();
            CreatePath();
        }

        private void INDB()
        {
            DBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
        }
        
        private void CreatePath()
        {
            dtMinMax.Clear();
            int i = 0;
            List<long> validNodeList = validCRRNodeDict.Keys.ToList();
            foreach (PathHelper item1 in validCRRPathList)
            {
                try
                {
                    //PeakWD
                    using (SqlConnection connectionDB = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = connectionDB.CreateCommand())
                        {
                            if (connectionDB.State == System.Data.ConnectionState.Closed)
                                connectionDB.Open();
                            cmd.Connection = connectionDB;
                            cmd.CommandText = " select f.MinDA, f.MAxDA, c.NodeName as SourceName, d.NodeName as SinkName  from " +
                                " ( select min(b.AvgPeakLMP- a.AvgPeakLMP)as MinDA , max(b.AvgPeakLMP- a.AvgPeakLMP) as MAxDA, a.NodeKey as source " +
                                " , b.NodeKey as sink from NodeLMPDailys   a join NodeLMPDailys  b on a.NodeKey!=b.NodeKey   and a.MarketDate=b.MarketDate" +
                                "  where a.MarketTypeCode='da'  and b.MarketTypeCode='da' and  a.NodeKey=" + item1.SourceKey + " and b.NodeKey =" + item1.SinkKey + "  and  " +
                                " a.AvgPeakLMP is not null   and b.AvgPeakLMP is not null  group by a.NodeKey, b.NodeKey) f join node " +
                                " c on f.source=c.NodeKey join node d   on f.sink=d.NodeKey   ";
                            cmd.CommandTimeout = 30000;
                            SqlDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                i++;
                                Console.WriteLine(i);
                                drMinMax = dtMinMax.NewRow();
                                drMinMax["SourceName"] = Convert.ToString(rdr.GetValue(2));
                                drMinMax["SinkName"] = Convert.ToString(rdr.GetValue(3));
                                drMinMax["MinDA"] = Convert.ToInt64(rdr.GetValue(0));
                                drMinMax["MaxDA"] = Convert.ToInt64(rdr.GetValue(1));
                                drMinMax["TimeOfUse"] = "PeakWD";
                                dtMinMax.Rows.Add(drMinMax);
                            }
                            rdr.Close();
                            connectionDB.Close();
                        }
                    }

                    //OffPeak
                    using (SqlConnection connectionDB = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = connectionDB.CreateCommand())
                        {
                            if (connectionDB.State == System.Data.ConnectionState.Closed)
                                connectionDB.Open();
                            cmd.Connection = connectionDB;
                            cmd.CommandText = " select f.MinDA, f.MAxDA, c.NodeName as SourceName, d.NodeName as SinkName  from " +
                               " ( select min(b.AvgOffpeakLMP- a.AvgOffpeakLMP)as MinDA , max(b.AvgOffpeakLMP- a.AvgOffpeakLMP) as MAxDA, a.NodeKey as source " +
                               " , b.NodeKey as sink from NodeLMPDailys   a join NodeLMPDailys  b on a.NodeKey!=b.NodeKey   and a.MarketDate=b.MarketDate" +
                               "  where a.MarketTypeCode='da'  and b.MarketTypeCode='da' and  a.NodeKey=" + item1.SourceKey + " and b.NodeKey =" + item1.SinkKey + "  and  " +
                               " a.AvgOffpeakLMP is not null   and b.AvgOffpeakLMP is not null  group by a.NodeKey, b.NodeKey) f join node " +
                               " c on f.source=c.NodeKey join node d   on f.sink=d.NodeKey   ";
                            cmd.CommandTimeout = 30000;
                            SqlDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                i++;
                                Console.WriteLine(i);
                                drMinMax = dtMinMax.NewRow();
                                drMinMax["SourceName"] = Convert.ToString(rdr.GetValue(2));
                                drMinMax["SinkName"] = Convert.ToString(rdr.GetValue(3));
                                drMinMax["MinDA"] = Convert.ToInt64(rdr.GetValue(0));
                                drMinMax["MaxDA"] = Convert.ToInt64(rdr.GetValue(1));
                                drMinMax["TimeOfUse"] = "OffPeak";
                                dtMinMax.Rows.Add(drMinMax);
                            }
                            rdr.Close();
                            connectionDB.Close();
                        }
                    }

                    //PeakWE
                    using (SqlConnection connectionDB = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = connectionDB.CreateCommand())
                        {
                            if (connectionDB.State == System.Data.ConnectionState.Closed)
                                connectionDB.Open();
                            cmd.Connection = connectionDB;
                            cmd.CommandText = " select f.MinDA, f.MAxDA, c.NodeName as SourceName, d.NodeName as SinkName  from " +
                                 " ( select min(b.AvgPeakWELMP- a.AvgPeakWELMP)as MinDA , max(b.AvgPeakWELMP- a.AvgPeakWELMP) as MAxDA, a.NodeKey as source " +
                                 " , b.NodeKey as sink from NodeLMPDailys   a join NodeLMPDailys  b on a.NodeKey!=b.NodeKey   and a.MarketDate=b.MarketDate" +
                                 "  where a.MarketTypeCode='da'  and b.MarketTypeCode='da' and  a.NodeKey=" + item1.SourceKey + " and b.NodeKey =" + item1.SinkKey + "  and  " +
                                 " a.AvgPeakWELMP is not null   and b.AvgPeakWELMP is not null  group by a.NodeKey, b.NodeKey) f join node " +
                                 " c on f.source=c.NodeKey join node d   on f.sink=d.NodeKey   ";
                            cmd.CommandTimeout = 30000;
                            SqlDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                i++;
                                Console.WriteLine(i);
                                drMinMax = dtMinMax.NewRow();
                                drMinMax["SourceName"] = Convert.ToString(rdr.GetValue(2));
                                drMinMax["SinkName"] = Convert.ToString(rdr.GetValue(3));
                                drMinMax["MinDA"] = Convert.ToInt64(rdr.GetValue(0));
                                drMinMax["MaxDA"] = Convert.ToInt64(rdr.GetValue(1));
                                drMinMax["TimeOfUse"] = "PeakWE";
                                dtMinMax.Rows.Add(drMinMax);
                            }
                            rdr.Close();
                            connectionDB.Close();
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }


            if (DBConnection.State == ConnectionState.Closed)
                DBConnection.Open();
            SqlTransaction transaction = DBConnection.BeginTransaction();
            using (SqlBulkCopy BKOptionPrice = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    BKOptionPrice.DestinationTableName = "Vayu..ErcotNodeMinMaxTest";
                    BKOptionPrice.ColumnMappings.Add("SourceName", "SourceName");
                    BKOptionPrice.ColumnMappings.Add("SinkName", "SinkName");
                    BKOptionPrice.ColumnMappings.Add("MinDA", "MinDA");
                    BKOptionPrice.ColumnMappings.Add("MaxDA", "MaxDA");
                    BKOptionPrice.ColumnMappings.Add("TimeOfUse", "TimeOfUse");
                    BKOptionPrice.WriteToServer(dtMinMax);

                    SqlCommand updatenodelmph = new SqlCommand("[UpMergeNodeMinMax]", DBConnection, transaction);
                    updatenodelmph.CommandType = CommandType.StoredProcedure;
                    updatenodelmph.CommandTimeout = 30000;
                    var upcount = updatenodelmph.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            DBConnection.Close();


        }
       
        private void GetValidCRRNode()
        {
            Console.WriteLine("Getting All Valid Nodes..");
            try
            {
                using (SqlConnection connectionDB = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = connectionDB.CreateCommand())
                    {
                        validCRRNodeDict = new Dictionary<long, string>();
                        validCRRPathList = new List<PathHelper>();
                        if (connectionDB.State == System.Data.ConnectionState.Closed)
                            connectionDB.Open();
                        cmd.Connection = connectionDB;
                        cmd.CommandText = "select distinct a.Sourcekey as source , a.Sinkkey as sink , SourceName , SinkName from CRRAuctionResults a "
                                          + "join node n1 on a.Sourcekey = n1.NodeKey "
                                          + "join node n2 on a.Sinkkey = n2.NodeKey ";
                        cmd.CommandTimeout = 30000;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            long sourceKey = Convert.ToInt64(rdr.GetValue(0));
                            long sinkKey = Convert.ToInt64(rdr.GetValue(1));
                            string sourceName = Convert.ToString(rdr.GetValue(2));
                            string sinkName = Convert.ToString(rdr.GetValue(3));
                            if (!validCRRNodeDict.ContainsKey(sourceKey))
                                validCRRNodeDict.Add(sourceKey, sourceName);
                            if (!validCRRNodeDict.ContainsKey(sinkKey))
                                validCRRNodeDict.Add(sinkKey, sinkName);
                            PathHelper path = new PathHelper();
                            path.SourceKey = sourceKey;
                            path.SinkKey = sinkKey;
                            validCRRPathList.Add(path);
                        }
                        rdr.Close();
                        connectionDB.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

    }

}
