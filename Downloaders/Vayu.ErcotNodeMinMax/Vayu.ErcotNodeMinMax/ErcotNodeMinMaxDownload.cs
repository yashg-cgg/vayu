using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;

namespace  Vayu.ErcotNodeMinMax
{
    public class ErcotNodeMinMaxDownload
    {
        List<PathHelper> validCRRPathList = new List<PathHelper>();
        Dictionary<long, string> validCRRNodeDict;
        DataTable dtMinMax = new DataTable();
        DataRow drMinMax = null;
        SqlConnection mDBConnection = new SqlConnection();
        Dictionary<string, MinMax> dictPathPeakWD = new Dictionary<string, MinMax>();
        Dictionary<string, MinMax> dictPathOffPeak = new Dictionary<string, MinMax>();
        Dictionary<string, MinMax> dictPathPeakWE = new Dictionary<string, MinMax>();
        Dictionary<string, long> mNodeHash = new Dictionary<string, long>();
        Dictionary<long, string> mNodeHash1 = new Dictionary<long, string>();
        public ErcotNodeMinMaxDownload()
        {
            INDB();
            DateTime sDate = DateTime.Today.AddDays(-5);
            DateTime eDate = DateTime.Today.AddDays(1);
            GetNodes();
            GetValidCRRNode();

            while (sDate <= eDate)
            {
                Console.WriteLine(sDate);
                GetAllData();
                GetDailys(sDate);
                sDate = sDate.AddDays(1);
            }
        }

        private void GetNodes()
        {
            mNodeHash = new Dictionary<string, long>();
            mNodeHash1 = new Dictionary<long, string>();
            try
            {
                using (SqlConnection connectionDB = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = connectionDB.CreateCommand())
                    {
                        if (connectionDB.State == System.Data.ConnectionState.Closed)
                            connectionDB.Open();
                        cmd.Connection = connectionDB;
                        cmd.CommandText = " select nodekey, Nodename from Node where MarketKey = 9  ";
                        cmd.CommandTimeout = 30000;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(1))
                            {
                                mNodeHash.Add(rdr.GetValue(1).ToString(), Convert.ToInt64(rdr.GetValue(0)));

                                mNodeHash1.Add(Convert.ToInt64(rdr.GetValue(0)), rdr.GetValue(1).ToString());
                            }
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

        private void GetAllData()
        {

            // select SourceName, SinkName from ErcotNodeMinMax where TimeOfUse = 'Off-peak'
            //select SourceName, SinkName from ErcotNodeMinMax where TimeOfUse = 'PeakWE'

            dictPathPeakWD = new Dictionary<string, MinMax>();
            dictPathOffPeak = new Dictionary<string, MinMax>();
            dictPathPeakWE = new Dictionary<string, MinMax>();
            try
            {
                using (SqlConnection connectionDB = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = connectionDB.CreateCommand())
                    {
                        if (connectionDB.State == System.Data.ConnectionState.Closed)
                            connectionDB.Open();
                        cmd.Connection = connectionDB;
                        cmd.CommandText = " select SourceName, SinkName, MinDa,MaxDA, TimeOfuse from ErcotNodeMinMax ";
                        cmd.CommandTimeout = 30000;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string sourcename = rdr.GetString(0);
                            string sinkname = rdr.GetString(1);


                            string key = mNodeHash[sourcename] + ":" + mNodeHash[sinkname] + ":" + Convert.ToString(rdr.GetValue(4)).ToString();
                            if (Convert.ToString(rdr.GetValue(4)) == "PeakWD")
                            {
                                MinMax mMinmax = new MinMax();
                                mMinmax.MinDA = Convert.ToDouble(rdr.GetValue(2));
                                mMinmax.MaxDA = Convert.ToDouble(rdr.GetValue(3));
                                dictPathPeakWD.Add(key, mMinmax);
                            }

                            if (Convert.ToString(rdr.GetValue(4)) == "OffPeak")
                            {
                                MinMax mMinmax = new MinMax();
                                mMinmax.MinDA = Convert.ToDouble(rdr.GetValue(2));
                                mMinmax.MaxDA = Convert.ToDouble(rdr.GetValue(3));
                                dictPathOffPeak.Add(key, mMinmax);
                            }

                            if (Convert.ToString(rdr.GetValue(4)) == "PeakWE")
                            {
                                MinMax mMinmax = new MinMax();
                                mMinmax.MinDA = Convert.ToDouble(rdr.GetValue(2));
                                mMinmax.MaxDA = Convert.ToDouble(rdr.GetValue(3));
                                dictPathPeakWE.Add(key, mMinmax);
                            }

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

        private void GetDailys(DateTime sDate)
        {
            Dictionary<long, Double> dictDailysPeakWD = new Dictionary<long, Double>();
            Dictionary<long, Double> dictDailysOffPeak = new Dictionary<long, Double>();
            Dictionary<long, Double> dictDailysPeakWE = new Dictionary<long, Double>();

            dtMinMax.Clear();

            try
            {
                using (SqlConnection connectionDB = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = connectionDB.CreateCommand())
                    {
                        if (connectionDB.State == System.Data.ConnectionState.Closed)
                            connectionDB.Open();
                        cmd.Connection = connectionDB;
                        cmd.CommandText = " select NodeKey, AvgPeakLMP, AvgOffpeakLMP, AvgPeakWELMP from NodeLMPDailys where MarketDate = '" + sDate + "'and MarketTypeCode = 'da'   ";
                        cmd.CommandTimeout = 30000;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(1))
                            {
                                dictDailysPeakWD.Add(Convert.ToInt64(rdr.GetValue(0)), Convert.ToDouble(rdr.GetValue(1)));
                            }
                            if (!rdr.IsDBNull(2))
                            {
                                dictDailysOffPeak.Add(Convert.ToInt64(rdr.GetValue(0)), Convert.ToDouble(rdr.GetValue(2)));
                            }
                            if (!rdr.IsDBNull(3))
                            {
                                dictDailysPeakWE.Add(Convert.ToInt64(rdr.GetValue(0)), Convert.ToDouble(rdr.GetValue(3)));
                            }
                        }
                        rdr.Close();
                        connectionDB.Close();
                    }

                }


            }
            catch (Exception ex)
            {

            }


            foreach (PathHelper item1 in validCRRPathList)
            {
                //Peak
                string keypeak = item1.SourceKey + ":" + item1.SinkKey + ":PeakWD";
                string source = "";
                string sink = "";

                double minpeak = double.MaxValue;
                double maxpeak = double.MinValue;

                double minpeakoff = double.MaxValue;
                double maxpeakoff = double.MinValue;

                double minpeakwe = double.MaxValue;
                double maxpeakwe = double.MinValue;
                //if (mNodeHash.ContainsValue(item1.SourceKey))
                //{
                //    source = mNodeHash.FirstOrDefault(a => a.Value == Convert.ToInt64(item1.SourceKey)).Key;
                //}
                //if (mNodeHash.ContainsValue(item1.SinkKey))
                //{
                //    sink = mNodeHash.FirstOrDefault(a => a.Value == Convert.ToInt64(item1.SinkKey)).Key;
                //}
                if (mNodeHash1.ContainsKey(item1.SourceKey))
                {
                    source = mNodeHash1[item1.SourceKey];
                }
                if (mNodeHash1.ContainsKey(item1.SinkKey))
                {
                    sink = mNodeHash1[item1.SinkKey];
                }
                //SourceName='TGCCS_ALL' and SinkName='SC2SES_UNIT1'
                if (source == "TGCCS_ALL" && sink == "SC2SES_UNIT1")
                {

                }
                if (dictPathPeakWD.ContainsKey(keypeak))
                {
                    minpeak = Convert.ToDouble(dictPathPeakWD[keypeak].MinDA);
                    maxpeak = Convert.ToDouble(dictPathPeakWD[keypeak].MaxDA);
                }
                if (dictDailysPeakWD.ContainsKey(item1.SinkKey) && dictDailysPeakWD.ContainsKey(item1.SourceKey))
                {
                    double daspread = dictDailysPeakWD[item1.SinkKey] - dictDailysPeakWD[item1.SourceKey];
                    if (daspread < minpeak)
                    {
                        minpeak = daspread;
                    }
                    if (daspread > maxpeak)
                    {
                        maxpeak = daspread;
                    }
                    drMinMax = dtMinMax.NewRow();


                    drMinMax["SourceName"] = source;
                    drMinMax["SinkName"] = sink;
                    drMinMax["MinDA"] = Math.Round(minpeak, 2);
                    drMinMax["MaxDA"] = Math.Round(maxpeak, 2);
                    drMinMax["TimeOfUse"] = "PeakWD";
                    drMinMax["UpdatedDateTime"] = DateTime.Now;

                    dtMinMax.Rows.Add(drMinMax);
                }

                //Offpeak
                string keyoffpeak = item1.SourceKey + ":" + item1.SinkKey + ":OffPeak";

                if (dictPathOffPeak.ContainsKey(keyoffpeak))
                {
                    minpeakoff = Convert.ToDouble(dictPathOffPeak[keyoffpeak].MinDA);
                    maxpeakoff = Convert.ToDouble(dictPathOffPeak[keyoffpeak].MaxDA);
                }
                if (dictDailysOffPeak.ContainsKey(item1.SinkKey) && dictDailysOffPeak.ContainsKey(item1.SourceKey))
                {
                    double daspread = dictDailysOffPeak[item1.SinkKey] - dictDailysOffPeak[item1.SourceKey];
                    if (daspread < minpeakoff)
                    {
                        minpeakoff = daspread;
                    }
                    if (daspread > maxpeakoff)
                    {
                        maxpeakoff = daspread;
                    }
                    drMinMax = dtMinMax.NewRow();

                    drMinMax["SourceName"] = source;
                    drMinMax["SinkName"] = sink;
                    drMinMax["MinDA"] = Math.Round(minpeakoff, 2);
                    drMinMax["MaxDA"] = Math.Round(maxpeakoff, 2);
                    drMinMax["TimeOfUse"] = "OffPeak";
                    drMinMax["UpdatedDateTime"] = DateTime.Now;
                    dtMinMax.Rows.Add(drMinMax);
                }

                //PeakWE
                string keypeajwe = item1.SourceKey + ":" + item1.SinkKey + ":PeakWE";

                if (dictPathPeakWE.ContainsKey(keypeajwe))
                {
                    minpeakwe = Convert.ToDouble(dictPathPeakWE[keypeajwe].MinDA);
                    maxpeakwe = Convert.ToDouble(dictPathPeakWE[keypeajwe].MaxDA);
                }
                if (dictDailysPeakWE.ContainsKey(item1.SinkKey) && dictDailysPeakWE.ContainsKey(item1.SourceKey))
                {
                    double daspread = dictDailysPeakWE[item1.SinkKey] - dictDailysPeakWE[item1.SourceKey];
                    if (daspread < minpeakwe)
                    {
                        minpeakwe = daspread;
                    }
                    if (daspread > maxpeakwe)
                    {
                        maxpeakwe = daspread;
                    }
                    drMinMax = dtMinMax.NewRow();
                    drMinMax["SourceName"] = source;
                    drMinMax["SinkName"] = sink;
                    drMinMax["MinDA"] = Math.Round(minpeakwe, 2);
                    drMinMax["MaxDA"] = Math.Round(maxpeakwe, 2);
                    drMinMax["TimeOfUse"] = "PeakWE";
                    drMinMax["UpdatedDateTime"] = DateTime.Now;
                    dtMinMax.Rows.Add(drMinMax);
                }
            }


            if (mDBConnection.State == ConnectionState.Closed)
                mDBConnection.Open();

            SqlCommand mDeleteLMPDailyCommand = new SqlCommand();
            mDeleteLMPDailyCommand.CommandText = "Truncate table  Vayu..ErcotNodeMinMaxTest";
            mDeleteLMPDailyCommand.CommandTimeout = 30000;
            mDeleteLMPDailyCommand.Connection = mDBConnection;
            mDeleteLMPDailyCommand.ExecuteNonQuery();
            SqlTransaction transaction = mDBConnection.BeginTransaction();
            using (SqlBulkCopy BKOptionPrice = new SqlBulkCopy(mDBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    BKOptionPrice.DestinationTableName = "Vayu..ErcotNodeMinMaxTest";
                    BKOptionPrice.ColumnMappings.Add("SourceName", "SourceName");
                    BKOptionPrice.ColumnMappings.Add("SinkName", "SinkName");
                    BKOptionPrice.ColumnMappings.Add("MinDA", "MinDA");
                    BKOptionPrice.ColumnMappings.Add("MaxDA", "MaxDA");
                    BKOptionPrice.ColumnMappings.Add("TimeOfUse", "TimeOfUse");
                    BKOptionPrice.ColumnMappings.Add("UpdatedDateTime", "UpdatedDateTime");
                    BKOptionPrice.WriteToServer(dtMinMax);

                    SqlCommand updatenodelmph = new SqlCommand("[UpMergeNodeMinMax]", mDBConnection, transaction);
                    updatenodelmph.CommandType = CommandType.StoredProcedure;
                    updatenodelmph.CommandTimeout = 300000;
                    var upcount = updatenodelmph.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            mDBConnection.Close();
        }
        private void INDB()
        {
            mDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            dtMinMax = new DataTable();
            dtMinMax.Columns.Add("SourceName", typeof(string));
            dtMinMax.Columns.Add("SinkName", typeof(string));
            dtMinMax.Columns.Add("MinDA", typeof(double));
            dtMinMax.Columns.Add("MaxDA", typeof(double));
            dtMinMax.Columns.Add("TimeOfUse", typeof(string));
            dtMinMax.Columns.Add("UpdatedDateTime", typeof(DateTime));

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

    internal class MinMax
    {
        public string SourceName { get; set; }
        public string SinkName { get; set; }
        public double MinDA { get; set; }
        public double MaxDA { get; set; }

    }
}
