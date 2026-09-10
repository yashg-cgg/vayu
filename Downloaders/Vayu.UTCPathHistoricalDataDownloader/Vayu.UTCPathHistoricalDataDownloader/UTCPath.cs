using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;

namespace Vayu.UTCPathHistoricalDataDownloader
{
    public class UTCPath
    {
        private SqlConnection VayuDbconn;

        SqlCommand _selectDANode = new SqlCommand();
        SqlCommand _selectRTNode = new SqlCommand();
        SqlCommand _selectPathListNode = new SqlCommand();

        SqlCommand _SelectPriceRT = new SqlCommand();
        SqlCommand _SelectPriceDA = new SqlCommand();
        SqlCommand _DeleteHistoricalData = new SqlCommand();

        private DataTable dtUTCPath;
        private DataRow drUTCPathRow = null;
        Dictionary<long, DateTime> dictDANode = new Dictionary<long, DateTime>();
        Dictionary<long, DateTime> dictRTNode = new Dictionary<long, DateTime>();

        Dictionary<long, DateTime> dictFinalNode = new Dictionary<long, DateTime>();
        Dictionary<long, DateTime> dictFinalRTsinkNode = new Dictionary<long, DateTime>(); 
        Dictionary<long, DateTime> dictFinalDANode = new Dictionary<long, DateTime>();
        Dictionary<long, DateTime> dictFinalDAsinkNode = new Dictionary<long, DateTime>();
        public UTCPath()
        {
            DBLoad();
            GetUTCDANode();
            GetUTCRTNode();
            GetPathList();
        }
        public void DBLoad()
        {
            VayuDbconn = new VayuDBConnection().GetInstance().GetSqlConnection();;

            dtUTCPath = new DataTable();

            dtUTCPath.Columns.Add("SourceNodeKey", typeof(long));
            dtUTCPath.Columns.Add("SinkNodeKey", typeof(long));
            dtUTCPath.Columns.Add("PathMinRT", typeof(DateTime));
            dtUTCPath.Columns.Add("PathMinDA", typeof(DateTime));

            _selectDANode = new SqlCommand();
            _selectDANode.CommandText = "Select NodeKey,MIN(MarketDateTime) from Vayu..NodeDALMPH group by NodeKey";
            _selectDANode.Connection = VayuDbconn;

            _selectRTNode = new SqlCommand();
            _selectRTNode.CommandText = "Select NodeKey,MIN(MarketDateTime) from Vayu..NodeLMPH group by NodeKey";
            _selectRTNode.Connection = VayuDbconn;

            _selectPathListNode = new SqlCommand();
            _selectPathListNode.CommandText = "Select distinct SourceNodeKey,SinkNodeKey from EESPathList";
            _selectPathListNode.Connection = VayuDbconn;

            _DeleteHistoricalData = new SqlCommand();
            _DeleteHistoricalData.CommandText = "truncate table Vayu..UTCPathHistoricData";
            _DeleteHistoricalData.Connection = VayuDbconn;
        }

        public void GetUTCDANode()
        {
            if (VayuDbconn.State == ConnectionState.Closed)
            {
                VayuDbconn.Open();
            }
            SqlDataReader reader = _selectDANode.ExecuteReader();
            while (reader.Read())
            {
                long NodeKey = Convert.ToInt64(reader.GetValue(0));
                DateTime MarketDateTime = Convert.ToDateTime(reader.GetDateTime(1));
                if (!dictDANode.ContainsKey(NodeKey))
                    dictDANode.Add(NodeKey, MarketDateTime);
            }
            reader.Close();
            VayuDbconn.Close();
        }
        public void GetUTCRTNode()
        {
            if (VayuDbconn.State == ConnectionState.Closed)
            {
                VayuDbconn.Open();
            }
            SqlDataReader reader = _selectRTNode.ExecuteReader();
            while (reader.Read())
            {
                long NodeKey = Convert.ToInt64(reader.GetValue(0));
                DateTime MarketDateTime = Convert.ToDateTime(reader.GetDateTime(1));
                if (!dictRTNode.ContainsKey(NodeKey))
                    dictRTNode.Add(NodeKey, MarketDateTime);
            }
            reader.Close();
            VayuDbconn.Close();
        }
        public void GetPathList()
        {
            dtUTCPath.Clear();
            if (VayuDbconn.State == ConnectionState.Closed)
            {
                VayuDbconn.Open();
            }
            DateTime RTDateTime = new DateTime(), DADateTime = new DateTime();
            SqlDataReader reader = _selectPathListNode.ExecuteReader();
            
            while (reader.Read())
            {
                long SourceNodekey = Convert.ToInt64(reader.GetValue(0));
                long SinkNodekey = Convert.ToInt64(reader.GetValue(1));
                {
                    //if (!dicteespathNode.ContainsKey(SourceNodekey) && !dicteespathNode.ContainsKey(SinkNodekey))
                    {
                        DateTime tempdate = new DateTime(), tempsordate = new DateTime(), tempsinkdate = new DateTime();
                        if (dictRTNode.ContainsKey(SourceNodekey) && dictRTNode.ContainsKey(SinkNodekey))
                        {
                            #region Source RT
                            DateTime sourcedate = dictRTNode[SourceNodekey];
                            if (dictFinalNode.Count > 0)
                            {
                                if (dictFinalNode.ContainsKey(SourceNodekey))
                                {
                                    tempdate = dictFinalNode[SourceNodekey];
                                    if (sourcedate > tempdate)//change
                                    {
                                        tempsordate = sourcedate;
                                        dictFinalNode.Remove(SourceNodekey);
                                        dictFinalNode.Add(SourceNodekey, tempsordate);
                                    }
                                    else
                                    {
                                        tempsordate = tempdate;
                                        dictFinalNode.Remove(SourceNodekey);
                                        dictFinalNode.Add(SourceNodekey, tempsordate);
                                    }
                                }
                                else
                                {
                                    tempsordate = sourcedate;
                                    dictFinalNode.Add(SourceNodekey, tempsordate);
                                }
                            }
                            else
                            {
                                tempsordate = sourcedate;
                                dictFinalNode.Add(SourceNodekey, tempsordate);
                            }
                            #endregion Source RT

                            #region Sink RT
                            DateTime sinkdate = dictRTNode[SinkNodekey];
                            if (dictFinalRTsinkNode.Count > 0)
                            {
                                if (dictFinalRTsinkNode.ContainsKey(SinkNodekey))
                                {
                                    tempdate = dictFinalRTsinkNode[SinkNodekey];
                                    if (sinkdate > tempdate)//change
                                    {
                                        tempsinkdate = sinkdate;
                                        dictFinalRTsinkNode.Remove(SinkNodekey);
                                        dictFinalRTsinkNode.Add(SinkNodekey, tempsinkdate);
                                    }
                                    else
                                    {
                                        tempsinkdate = tempdate;
                                        dictFinalRTsinkNode.Remove(SinkNodekey);
                                        dictFinalRTsinkNode.Add(SinkNodekey, tempsinkdate);
                                    }
                                }
                                else
                                {
                                    tempsinkdate = sinkdate;
                                    dictFinalRTsinkNode.Add(SinkNodekey, tempsinkdate);
                                }
                            }
                            else
                            {
                                tempsinkdate = sinkdate;
                                dictFinalRTsinkNode.Add(SinkNodekey, tempsinkdate);
                            }
                            #endregion Sink RT

                            if (tempsordate > tempsinkdate)//change
                            {
                                RTDateTime = tempsordate;
                            }
                            else
                            {
                                RTDateTime = tempsinkdate;
                            }
                        }


                        if (dictDANode.ContainsKey(SourceNodekey) && dictDANode.ContainsKey(SinkNodekey))
                        {

                            #region Source DA
                            DateTime sourcedate = dictDANode[SourceNodekey];
                            if (dictFinalDANode.Count > 0)
                            {
                                if (dictFinalDANode.ContainsKey(SourceNodekey))
                                {
                                    tempdate = dictFinalDANode[SourceNodekey];
                                    if (sourcedate > tempdate)//change
                                    {
                                        tempsordate = sourcedate;
                                        dictFinalDANode.Remove(SourceNodekey);
                                        dictFinalDANode.Add(SourceNodekey, tempsordate);
                                    }
                                    else
                                    {
                                        tempsordate = tempdate;
                                        dictFinalDANode.Remove(SourceNodekey);
                                        dictFinalDANode.Add(SourceNodekey, tempsordate);
                                    }
                                }
                                else
                                {
                                    tempsordate = sourcedate;
                                    dictFinalDANode.Add(SourceNodekey, tempsordate);
                                }
                            }
                            else
                            {
                                tempsordate = sourcedate;
                                dictFinalDANode.Add(SourceNodekey, tempsordate);
                            }
                            #endregion Source DA

                            #region Sink DA
                            DateTime sinkdate = dictDANode[SinkNodekey];
                            if (dictFinalDAsinkNode.Count > 0)
                            {
                                if (dictFinalDAsinkNode.ContainsKey(SinkNodekey))
                                {
                                    tempdate = dictFinalDAsinkNode[SinkNodekey];
                                    if (sinkdate > tempdate)//change
                                    {
                                        tempsinkdate = sinkdate;
                                        dictFinalDAsinkNode.Remove(SinkNodekey);
                                        dictFinalDAsinkNode.Add(SinkNodekey, tempsinkdate);
                                    }
                                    else
                                    {
                                        tempsinkdate = tempdate;
                                        dictFinalDAsinkNode.Remove(SinkNodekey);
                                        dictFinalDAsinkNode.Add(SinkNodekey, tempsinkdate);
                                    }
                                }
                                else
                                {
                                    tempsinkdate = sinkdate;
                                    dictFinalDAsinkNode.Add(SinkNodekey, tempsinkdate);
                                }
                            }
                            else
                            {
                                tempsinkdate = sinkdate;
                                dictFinalDAsinkNode.Add(SinkNodekey, tempsinkdate);
                            }
                            #endregion Sink DA

                            if (tempsordate > tempsinkdate)
                            {
                                DADateTime = tempsordate;
                            }
                            else
                            {
                                DADateTime = tempsinkdate;
                            }
                        }
                        drUTCPathRow = dtUTCPath.NewRow();
                        drUTCPathRow["SourceNodeKey"] = SourceNodekey;
                        drUTCPathRow["SinkNodeKey"] = SinkNodekey;
                        drUTCPathRow["PathMinRT"] = RTDateTime;
                        drUTCPathRow["PathMinDA"] = DADateTime;
                        dtUTCPath.Rows.Add(drUTCPathRow);
                    }
                }
            }
            reader.Close();
            VayuDbconn.Close();
            SaveDB(dtUTCPath);
        }
        public void SaveDB(DataTable pathdata)
        {
            int Count = pathdata.Rows.Count;
            if (VayuDbconn.State == ConnectionState.Closed)
            {
                VayuDbconn.Open();
            }
            _DeleteHistoricalData.Connection = VayuDbconn;
            _DeleteHistoricalData.ExecuteNonQuery();
            SqlTransaction transaction = VayuDbconn.BeginTransaction();
            using (SqlBulkCopy bkUTCPath = new SqlBulkCopy(VayuDbconn, SqlBulkCopyOptions.TableLock,transaction))
            {
                try
                {
                    bkUTCPath.DestinationTableName = "Vayu..UTCPathHistoricData";
                    bkUTCPath.BatchSize = 1500;
                    bkUTCPath.BulkCopyTimeout = 30000;
                    bkUTCPath.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                    bkUTCPath.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                    bkUTCPath.ColumnMappings.Add("PathMinRT", "PathMinRT");
                    bkUTCPath.ColumnMappings.Add("PathMinDA", "PathMinDA");
                    bkUTCPath.WriteToServer(pathdata);
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    transaction.Rollback();
                }
            }
            VayuDbconn.Close();
        }
    }
}
