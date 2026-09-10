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
    class ErcotNodeMedian45
    {
        List<PathHelper> validCRRPathList = new List<PathHelper>();
        Dictionary<long, string> validCRRNodeDict;
        DataTable dtMinMax = new DataTable();
        DataRow drMinMax = null;
        SqlConnection VayuDBConnection = new SqlConnection();
        Dictionary<string, long> mNodeHash = new Dictionary<string, long>();
        Dictionary<long, string> mNodeHash1 = new Dictionary<long, string>();
        private readonly object balanceLock = new object();
        public ErcotNodeMedian45()
        {
            INDB();
            DateTime sDate = DateTime.Today.AddDays(0);
            DateTime eDate = DateTime.Today.AddDays(1);
            GetNodes();
            GetValidCRRNode();

            while (sDate <= eDate)
            {
                Console.WriteLine(sDate);
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

        private void GetDailys(DateTime sDate)
        {
            Dictionary<string, Double> dictDailysPeakWD = new Dictionary<string, Double>();
            Dictionary<string, Double> dictDailysOffPeak = new Dictionary<string, Double>();
            Dictionary<string, Double> dictDailysPeakWE = new Dictionary<string, Double>();
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
                        cmd.CommandText = " select NodeKey, AvgPeakLMP, MarketDate from NodeLMPDailys where MarketDate >= '" + sDate.AddDays(-45) + "' and MarketDate <= '" + sDate + "'  " +
                            "and MarketTypeCode = 'da' and AvgPeakLMP is not null order by MarketDate";
                        cmd.CommandTimeout = 30000;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(1))
                            {
                                dictDailysPeakWD.Add(rdr.GetValue(0).ToString() + ":" + Convert.ToDateTime(rdr.GetValue(2)).ToString(), Convert.ToDouble(rdr.GetValue(1)));
                            }

                        }
                        rdr.Close();
                        connectionDB.Close();
                    }

                    using (SqlCommand cmd = connectionDB.CreateCommand())
                    {
                        if (connectionDB.State == System.Data.ConnectionState.Closed)
                            connectionDB.Open();
                        cmd.Connection = connectionDB;
                        cmd.CommandText = " select NodeKey, AvgOffpeakLMP, MarketDate from NodeLMPDailys where MarketDate >= '" + sDate.AddDays(-45) + "' and MarketDate <= '" + sDate + "'  " +
                            "and MarketTypeCode = 'da' and AvgOffpeakLMP is not null order by MarketDate";
                        cmd.CommandTimeout = 30000;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(1))
                            {
                                dictDailysOffPeak.Add(rdr.GetValue(0).ToString() + ":" + Convert.ToDateTime(rdr.GetValue(2)).ToString(), Convert.ToDouble(rdr.GetValue(1)));
                            }

                        }
                        rdr.Close();
                        connectionDB.Close();
                    }
                    using (SqlCommand cmd = connectionDB.CreateCommand())
                    {
                        if (connectionDB.State == System.Data.ConnectionState.Closed)
                            connectionDB.Open();
                        cmd.Connection = connectionDB;
                        cmd.CommandText = " select NodeKey, AvgPeakWELMP, MarketDate from NodeLMPDailys where MarketDate >= '" + sDate.AddDays(-45) + "' and MarketDate <= '" + sDate + "'  " +
                            "and MarketTypeCode = 'da' and AvgPeakWELMP is not null order by MarketDate";
                        cmd.CommandTimeout = 30000;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(1))
                            {
                                dictDailysPeakWE.Add(rdr.GetValue(0).ToString() + ":" + Convert.ToDateTime(rdr.GetValue(2)).ToString(), Convert.ToDouble(rdr.GetValue(1)));
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
                if(item1.SourceKey== 57320 && item1.SinkKey== 57811)
                {

                }
                


                string keypeak = item1.SourceKey + ":" + item1.SinkKey + ":PeakWD";
                string source = "";
                string sink = "";
                double median = double.MaxValue;
                if (mNodeHash1.ContainsKey(item1.SourceKey))
                {
                    source = mNodeHash1[item1.SourceKey];
                }
                if (mNodeHash1.ContainsKey(item1.SinkKey))
                {
                    sink = mNodeHash1[item1.SinkKey];
                }
                List<double> medianlistpeak = new List<double>();
                List<double> medianlistoffpeak = new List<double>();
                List<double> medianlistpeakwe = new List<double>();
                DateTime futureDate = sDate;
                for (DateTime date = sDate.AddDays(-45); date < futureDate; date = date.AddDays(1.0))
                {

                    string sourcekey = item1.SourceKey + ":" + date.ToString();
                    string sinkkey = item1.SinkKey + ":" + date.ToString();
                    if (dictDailysPeakWD.ContainsKey(sourcekey) && dictDailysPeakWD.ContainsKey(sinkkey))
                    {
                        medianlistpeak.Add(dictDailysPeakWD[sinkkey] - dictDailysPeakWD[sourcekey]);
                    }

                    if (dictDailysOffPeak.ContainsKey(sourcekey) && dictDailysOffPeak.ContainsKey(sinkkey))
                    {
                        medianlistoffpeak.Add(dictDailysOffPeak[sinkkey] - dictDailysOffPeak[sourcekey]);
                    }

                    if (dictDailysOffPeak.ContainsKey(sourcekey) && dictDailysOffPeak.ContainsKey(sinkkey))
                    {
                        medianlistpeakwe.Add(dictDailysOffPeak[sinkkey] - dictDailysOffPeak[sourcekey]);
                    }

                }
                if (medianlistpeak.Count > 0)
                {
                    drMinMax = dtMinMax.NewRow();
                    drMinMax["SourceName"] = source;
                    drMinMax["SinkName"] = sink;
                    drMinMax["Median45"] = Math.Round(GetMedian(medianlistpeak), 2);
                    drMinMax["TimeOfUse"] = "PeakWD";
                    drMinMax["UpdatedDateTime"] = DateTime.Now;
                    dtMinMax.Rows.Add(drMinMax);
                }
                if (medianlistoffpeak.Count > 0)
                {
                    drMinMax = dtMinMax.NewRow();
                    drMinMax["SourceName"] = source;
                    drMinMax["SinkName"] = sink;
                    drMinMax["Median45"] = Math.Round(GetMedian(medianlistoffpeak), 2);
                    drMinMax["TimeOfUse"] = "OFF-PEAK";
                    drMinMax["UpdatedDateTime"] = DateTime.Now;
                    dtMinMax.Rows.Add(drMinMax);
                }
                if (medianlistpeakwe.Count > 0)
                {
                    drMinMax = dtMinMax.NewRow();
                    drMinMax["SourceName"] = source;
                    drMinMax["SinkName"] = sink;
                    drMinMax["Median45"] = Math.Round(GetMedian(medianlistpeakwe), 2);
                    drMinMax["TimeOfUse"] = "PeakWE";
                    drMinMax["UpdatedDateTime"] = DateTime.Now;
                    dtMinMax.Rows.Add(drMinMax);
                }
            }


            if (VayuDBConnection.State == ConnectionState.Closed)
                VayuDBConnection.Open();

            SqlCommand mDeleteLMPDailyCommand = new SqlCommand();
            mDeleteLMPDailyCommand.CommandText = "Truncate table Vayu..ErcotNodeMedian45_Test";
            mDeleteLMPDailyCommand.CommandTimeout = 30000;
            mDeleteLMPDailyCommand.Connection = VayuDBConnection;
            mDeleteLMPDailyCommand.ExecuteNonQuery();
            SqlTransaction transaction = VayuDBConnection.BeginTransaction();
            using (SqlBulkCopy BKOptionPrice = new SqlBulkCopy(VayuDBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    BKOptionPrice.DestinationTableName = "Vayu..ErcotNodeMedian45_Test";
                    BKOptionPrice.ColumnMappings.Add("SourceName", "SourceName");
                    BKOptionPrice.ColumnMappings.Add("SinkName", "SinkName");
                    BKOptionPrice.ColumnMappings.Add("Median45", "Median45");
                    BKOptionPrice.ColumnMappings.Add("TimeOfUse", "TimeOfUse");
                    BKOptionPrice.ColumnMappings.Add("UpdatedDateTime", "UpdatedDateTime");
                    BKOptionPrice.WriteToServer(dtMinMax);

                    SqlCommand updatenodelmph = new SqlCommand("[UpMergeErcotNodeMedian45]", VayuDBConnection, transaction);
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
            VayuDBConnection.Close();
        }
        private void INDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            dtMinMax = new DataTable();
            dtMinMax.Columns.Add("SourceName", typeof(string));
            dtMinMax.Columns.Add("SinkName", typeof(string));
            dtMinMax.Columns.Add("Median45", typeof(double));
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

        public double GetMedian(IEnumerable<double> source)
        {
            double[] temp = source.ToArray();
            Array.Sort(temp);
            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                double a = temp[count / 2 - 1];
                double b = temp[count / 2];
                return (a + b) / 2;
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }
    }
}
