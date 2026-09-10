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
using Vayu.CommonAccessLibrary;

namespace Vayu.BlockAlgorithmNamespace
{
    public class CalculateMinMaxRT
    {
        private SqlConnection mConnection;
        private SqlCommand mSelectPathsCommand;
        private SqlCommand cmdDeleteCommand;
        private List<Node> mRTFilteredSortedList = new List<Node>();
        private Dictionary<string, List<Node>[, ,]> mCachedNodeListHash = new Dictionary<string, List<Node>[, ,]>();
        private List<PricingNode> SourceNodeList;
        private List<PricingNode> SinkNodeList;
        private Dictionary<string,NodePriceLibrary.LMP> nodeIdLMPHHash = new Dictionary<string, NodePriceLibrary.LMP>();
        private DateTime StartDate = new DateTime(2013, 01, 01);
        private DateTime EndDate = DateTime.Today.AddDays(-1);
        private Dictionary<string, List<Node>> mHourlyPivotHash = new Dictionary<string, List<Node>>();
        private List<SourceSink> NodeList = new List<SourceSink>();
        private DataTable mDtCalculateMinMaxRT = new DataTable();
        private DataRow dr = null;
        public CalculateMinMaxRT()
        {
            InitDB();
            DBAccess.GetSourceSinkNodeList(
              (item1, error) =>
              {
                  SourceNodeList = item1.Item1;
                  SinkNodeList = item1.Item2;
              }, "PJM", "UPTO");
            GetEESPaths();
            mDtCalculateMinMaxRT.Clear();
            Console.WriteLine("Total Path :" + NodeList.Count);
            int i = 0;
            foreach (SourceSink item in NodeList)
            {
                AccessData(item);
                Console.WriteLine("Done so Far :" + i++);
            }

        }

        private void SaveInDB(DataTable tempDt)
        {
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            mConnection.Open();
            cmdDeleteCommand.ExecuteNonQuery();
            SqlTransaction transaction = mConnection.BeginTransaction();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkLmpH.DestinationTableName = "[dbo].[BlockAlgoMinMaxAllTest]";
                    bkLmpH.ColumnMappings.Add("SourceName", "SourceName");
                    bkLmpH.ColumnMappings.Add("SinkName", "SinkName");
                    bkLmpH.ColumnMappings.Add("RTMinAll", "RTMinAll");
                    bkLmpH.ColumnMappings.Add("RTMaxAll", "RTMaxAll");
                    bkLmpH.WriteToServer(tempDt);
                    SqlCommand cmdUpdatenodelmph = new SqlCommand("[dbo].[PJMDeleteDupsBlockAlgoall]", mConnection, transaction);
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
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            try
            {
                mConnection.Open();
                SqlTransaction transaction2 = mConnection.BeginTransaction();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mConnection, SqlBulkCopyOptions.TableLock, transaction2))
                {
                    try
                    {
                        if (tempDt.Rows.Count > 0)
                        {
                            SqlCommand cmdUpdateNodeLmpMin = new SqlCommand("[PJM].[UpMergeBlockAlgoall]", mConnection, transaction2);
                            cmdUpdateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                            cmdUpdateNodeLmpMin.ExecuteNonQuery();
                            transaction2.Commit();
                            mConnection.Close();
                            tempDt.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void GetEESPaths()
        {
            NodeList = new List<SourceSink>();
            List<int> uniqueList = new List<int>();
            Dictionary<int, Node> rtHash = new Dictionary<int, Node>();
            Dictionary<int, Node> daHash = new Dictionary<int, Node>();
            mConnection.Open();
            //mSelectPathsCommand.Parameters["@marketdate"].Value = EndDate;
            SqlDataReader reader = mSelectPathsCommand.ExecuteReader();
            while (reader.Read())
            {
                SourceSink mSourceSink = new SourceSink();
                mSourceSink.SourceNodeKey = Convert.ToInt32(reader.GetValue(0));
                mSourceSink.SinkNodeKey = Convert.ToInt32(reader.GetValue(1));
                NodeList.Add(mSourceSink);
                if (!uniqueList.Contains(mSourceSink.SourceNodeKey))
                {
                    uniqueList.Add(mSourceSink.SourceNodeKey);
                }
                if (!uniqueList.Contains(mSourceSink.SinkNodeKey))
                {
                    uniqueList.Add(mSourceSink.SinkNodeKey);
                }
            }
            reader.Close();
            mConnection.Close();
            List<int> dartList = new List<int>();
            foreach (SourceSink sourceSink in NodeList)
            {
                if (!dartList.Contains(sourceSink.SourceNodeKey))
                {
                    dartList.Add(sourceSink.SourceNodeKey);
                }
                if (!dartList.Contains(sourceSink.SinkNodeKey))
                {
                    dartList.Add(sourceSink.SinkNodeKey);
                }
            }

        }
        private void AccessData(SourceSink SourceSinkitem)
        {
            string sourceSinkKey = SourceSinkitem.SourceNodeKey.ToString() + ":" + SourceSinkitem.SinkNodeKey.ToString();
            mRTFilteredSortedList = new List<Node>();
            string existingPathKey = "";
            SourceSinkData sourceSinkData = new SourceSinkData();
            sourceSinkData.Source = SourceNodeList.FirstOrDefault(x => x.NodeKey == SourceSinkitem.SourceNodeKey);
            sourceSinkData.Sink = SinkNodeList.FirstOrDefault(x => x.NodeKey == SourceSinkitem.SinkNodeKey);
            try
            {
                if (CachedNodeListHashCheckKeyExists(sourceSinkData, out existingPathKey))
                {
                    DateTime cachedEndDateRT = mCachedNodeListHash[existingPathKey][0, 1, 0][0].LmpTimePriceList.Max(x => x.MarketTime);
                    DateTime cachedStartDateRT = mCachedNodeListHash[existingPathKey][0, 1, 0][0].LmpTimePriceList.Min(x => x.MarketTime);
                }
                List<Node> lmpList = GetDarts(sourceSinkData);
                if (lmpList == null)
                {
                    return;
                }
                if (!CachedNodeListHashCheckKeyExists(sourceSinkData, out existingPathKey))
                {
                    mCachedNodeListHash.Add(existingPathKey, new List<Node>[2, 3, 2] { { { null, null }, { lmpList, null }, { null, null } },
                        { { null, null }, { null, null }, { null, null } } });
                }
                else // already exists in cache, update it
                {
                    mCachedNodeListHash[existingPathKey][0, 1, 0] = lmpList;
                }

                mRTFilteredSortedList = new List<Node>();
                try
                {
                    foreach (Node node in mCachedNodeListHash[sourceSinkKey][0, 1, 0])
                    {
                        mRTFilteredSortedList.Add(new Node(node)); // new Node(node) is a deep copy of node object.
                    }
                }
                catch (Exception ex)
                {

                }
                Sort(mRTFilteredSortedList);
                if (mHourlyPivotHash.Keys.Contains("RT"))
                {
                    mHourlyPivotHash.Remove("RT");
                }
                mHourlyPivotHash.Add("RT", mRTFilteredSortedList);
                SetHourlyPivotList(mHourlyPivotHash);
            }catch(Exception ex)
            {

            }
        }
        private void SetHourlyPivotList(Dictionary<string, List<Node>> hourlyPivotList)
        {
            if (hourlyPivotList == null)
            {
                return;
            }
            List<HourlyPivotData> hourlyPivotData = new List<HourlyPivotData>();
            string rowType;
            DateTime LoadDate = DateTime.MinValue;
            try
            {
                string source = "";
                string sink = "";
                foreach (var nodeList in hourlyPivotList)
                {
                    rowType = nodeList.Key;
                    int counter = 0;
                    foreach (var item in nodeList.Value)
                    {
                        int compareCounter = nodeList.Value.Count == 3 ? 2 : 0;
                        if (counter == 0 && compareCounter == 2)
                        {
                            counter++;
                            source = item.NodeName;
                            continue;
                        }
                        if (counter == 1 && compareCounter == 2)
                        {
                            counter++;
                            sink = item.NodeName;
                            continue;
                        }
                        var itemlist = item.LmpTimePriceList.OrderBy(i => i.MarketTime).ToArray();
                        DateTime dateCounter = DateTime.Now.Date;
                        if (itemlist.FirstOrDefault() != null)
                        {
                            dateCounter = itemlist.FirstOrDefault().MarketTime.Date;
                        }
                        double total = 0;
                        double avgcounter = 0;
                        HourlyPivotData hpdata = new HourlyPivotData();
                        foreach (var hourPrice in itemlist)
                        {
                            if ((counter == 0 && nodeList.Value.Count > 1))
                            {
                                break;
                            }
                            if ((counter == 1 && nodeList.Value.Count > 1))
                            {
                                break;
                            }
                            if (hourPrice.MarketTime.AddMinutes(-1).Date > dateCounter)
                            {
                                hpdata.Total = (total);
                                hpdata.Average = (total) / avgcounter;
                                avgcounter = total = 0;
                                hpdata.Date = dateCounter;
                                hpdata.DateDisplay = dateCounter;
                                hpdata.RowDay = dateCounter.Date.ToString("ddd");
                                hourlyPivotData.Add(hpdata);
                                hpdata = new HourlyPivotData();
                                dateCounter = hourPrice.MarketTime.AddMinutes(-1).Date;
                            }
                            if (hourPrice.MarketTime.AddMinutes(-1).Date == dateCounter)
                            {
                                hpdata.Date = hourPrice.MarketTime.AddMinutes(-1).Date;
                                hpdata.DateDisplay = hourPrice.MarketTime.AddMinutes(-1).Date;
                                hpdata.RowDay = dateCounter.Date.ToString("ddd");
                                hpdata.RowType = rowType;
                                hpdata.RowDisplayType = rowType;
                            }
                            string hour = "HE" + hourPrice.MarketTime.Hour.ToString();
                            double? price = null;
                            if (hour.Equals("HE0"))
                            {
                                hour = "HE24";
                            }
                            if (!hourPrice.Lmp.Price.Equals(double.NaN))
                            {
                                price = hourPrice.Lmp.Price;
                                total += (double)price;
                                avgcounter++;
                            }
                            hpdata.GetType().GetProperty(hour).SetValue(hpdata, price, null);
                        }
                        counter++;
                        if (hpdata != null)
                        {
                            hpdata.Total = total;
                            hpdata.Average = total / avgcounter;
                            avgcounter = total = 0;
                            hourlyPivotData.Add(hpdata);
                            hpdata = null;
                        }
                    }
                    hourlyPivotData.RemoveAll(item => (item.Total == null && item.Average == null));
                }
                if (hourlyPivotData.Where(a => a.RowType == null).Count() <= 0)
                {
                    List<HourlyPivotData> sortTempHourlyPivotList = (from t in hourlyPivotData
                                                                     orderby t.Date descending, t.RowType.Length descending, t.RowType ascending, t.RowName descending
                                                                     select t).ToList();
                    if (sortTempHourlyPivotList.Count > 0)
                    {
                        double[] hourCountList = new double[24];
                        HourlyPivotData totalHourlySummaryPivot = new HourlyPivotData();
                        totalHourlySummaryPivot.RowDisplayType = "Total";
                        totalHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        totalHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;

                        DateTime? date = sortTempHourlyPivotList[0].Date;
                        string rowtype = sortTempHourlyPivotList[0].RowDisplayType;
                        sortTempHourlyPivotList[0].RowDay = date.Value.ToString("ddd");
                        double? sumWin = null;
                        double? sumHourCount = null;
                        double? sumHourTotal = null;

                        HourlyPivotData minHourlySummaryPivot = new HourlyPivotData();
                        minHourlySummaryPivot.RowDisplayType = "Min";
                        minHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        minHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;
                        HourlyPivotData maxHourlySummaryPivot = new HourlyPivotData();
                        maxHourlySummaryPivot.RowDisplayType = "Max";
                        maxHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        maxHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;
                        var mintotal = sortTempHourlyPivotList.Min(p => p.Total.GetValueOrDefault());
                        var maxtotal = sortTempHourlyPivotList.Max(p => p.Total.GetValueOrDefault());
                        minHourlySummaryPivot.Total = mintotal;
                        maxHourlySummaryPivot.Total = maxtotal;
                        var list1 = sortTempHourlyPivotList.Where(e => e.Total == mintotal);
                        for (int i = 1; i < sortTempHourlyPivotList.Count + 1; i++)
                        {
                            if (i < sortTempHourlyPivotList.Count)
                            {
                                if (sortTempHourlyPivotList[i].RowDisplayType == rowtype && (date == sortTempHourlyPivotList[i].Date || sortTempHourlyPivotList[i].Date == null))
                                {
                                    sortTempHourlyPivotList[i].RowDisplayType = string.Empty;
                                }
                                else
                                {
                                    rowtype = sortTempHourlyPivotList[i].RowDisplayType;
                                }
                                if (sortTempHourlyPivotList[i].Date == date)
                                {
                                    sortTempHourlyPivotList[i].RowDay = date.Value.ToString("ddd");
                                    sortTempHourlyPivotList[i].DateDisplay = null;
                                }
                                else
                                {
                                    date = sortTempHourlyPivotList[i].Date;
                                    sortTempHourlyPivotList[i].RowDay = date.Value.ToString("ddd");
                                }
                            }
                            Action<int> winRow = (hour) =>
                            {
                                if (sumWin == null && sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    if (sortTempHourlyPivotList[i - 1].HourDataList[hour] > 0)
                                    {
                                        sumWin = 1;
                                    }
                                }
                                else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    if (sortTempHourlyPivotList[i - 1].HourDataList[hour] > 0)
                                    {
                                        sumWin++;
                                    }
                                }
                                if (sumHourTotal == null && sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    sumHourTotal = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    sumHourCount = 1;
                                }
                                else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    sumHourTotal += sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    sumHourCount++;
                                }
                                if (totalHourlySummaryPivot.HourDataList[hour] == null && sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    totalHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    minHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    maxHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    hourCountList[hour] = 1;
                                }
                                else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    totalHourlySummaryPivot.HourDataList[hour] += sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    if (minHourlySummaryPivot.HourDataList[hour] > sortTempHourlyPivotList[i - 1].HourDataList[hour])
                                    {
                                        minHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    }
                                    if (maxHourlySummaryPivot.HourDataList[hour] < sortTempHourlyPivotList[i - 1].HourDataList[hour])
                                    {
                                        maxHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    }
                                    hourCountList[hour]++;
                                }

                            };
                            for (int j = 0; j < 24; j++)
                            {
                                winRow(j);
                            }
                        } //for loop
                        double sumTotal = 0;
                        double sumCount = 0;
                        double sumMax = -1000;
                        double sumMin = 1000;

                        Action<int> hourlySumm = (hour) =>
                        {
                            if (totalHourlySummaryPivot.HourDataList[hour] != null)
                            {
                                sumTotal += (double)totalHourlySummaryPivot.HourDataList[hour];
                                sumCount++;
                                if (sumMax < (double)maxHourlySummaryPivot.HourDataList[hour])
                                {
                                    sumMax = (double)maxHourlySummaryPivot.HourDataList[hour];
                                }
                                if (minHourlySummaryPivot.HourDataList[hour].HasValue)
                                {
                                    if (sumMin > (double)minHourlySummaryPivot.HourDataList[hour])
                                    {
                                        sumMin = (double)minHourlySummaryPivot.HourDataList[hour];
                                    }
                                }
                            }
                        };
                        dr = mDtCalculateMinMaxRT.NewRow();
                        dr["SourceName"] = source;
                        dr["SinkName"] = sink;
                        dr["RTMinAll"] = minHourlySummaryPivot.HourDataList.Min(x => x.Value);
                        dr["RTMaxAll"] = maxHourlySummaryPivot.HourDataList.Max(x => x.Value);
                        mDtCalculateMinMaxRT.Rows.Add(dr);
                        if (mDtCalculateMinMaxRT.Rows.Count > 0)
                            SaveInDB(mDtCalculateMinMaxRT);
                    }

                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
        private void Sort(List<Node> mRTFilteredSortedList)
        {
            List<Node> rtList = mRTFilteredSortedList;

            if (rtList.Count == 0)
            {
                return;
            }
            foreach (Node node in rtList)
            {
                List<LmpTimePrice> timePriceList = node.LmpTimePriceList.OrderBy(x => x.MarketTime).ToList<LmpTimePrice>();
                node.LmpTimePriceList = timePriceList;
            }
        }
        public bool CachedNodeListHashCheckKeyExists(SourceSinkData srcSinkData, out string existingPathKey)
        {
            bool keyExists = false;
            string testKey1 = srcSinkData.Sink == null ? srcSinkData.Source.NodeKey.ToString() : srcSinkData.Source.NodeKey.ToString() + ":" + srcSinkData.Sink.NodeKey.ToString();
            string testKey2 = srcSinkData.Sink == null ? srcSinkData.Source.NodeKey.ToString() : srcSinkData.Sink.NodeKey.ToString() + ":" + srcSinkData.Source.NodeKey.ToString();
            existingPathKey = testKey1;
            if (mCachedNodeListHash.ContainsKey(testKey1))
            {
                keyExists = true;
                existingPathKey = testKey1;
            }
            else if (mCachedNodeListHash.ContainsKey(testKey2))
            {
                keyExists = true;
                existingPathKey = testKey2;
            }
            return keyExists;
        }
        private void InitDB()
        {
            //mConnection = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            mConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectPathsCommand = new SqlCommand();
            // mSelectPathsCommand.CommandText = "SELECT DISTINCT SourceNodeKey, SinkNodeKey from EESPathList where marketkey = 1 order by sourcenodekey, sinknodekey";
            //mSelectPathsCommand.CommandText = "SELECT distinct SourceNodeKey, SinkNodeKey from BlockAlgorithmResults  where marketdate=@marketdate order by SourceNodeKey";
            //mSelectPathsCommand.CommandText = "SELECT distinct SourceNodeKey, SinkNodeKey from BlockAlgorithmResults  where SourceNodeKey in (select nodekey from node where marketkey=1 and NodeKey >= " + ConfigurationManager.AppSettings.Get("sKey") + " and  NodeKey <= " + ConfigurationManager.AppSettings.Get("eKey") + ")";
            //mSelectPathsCommand.CommandText = "SELECT distinct SourceNodeKey, SinkNodeKey from BlockAlgorithmResults  where SourceNodeKey in (select nodekey from node where marketkey=1 and NodeKey = " + item + ")  and sinknodekey  in (select nodekey from node where marketkey=1 and nodename not in ( select sinkname from BlockAlgoMinMaxComplete where sourcename in (select nodename from node where NodeKey =  "
            //    + item + " )))";
            // " SourceNodeKey in (" + ConfigurationManager.AppSettings.Get("Key")+")";
            //mSelectPathsCommand.Parameters.AddWithValue("@marketdate", "marketdate");
           // mSelectPathsCommand.CommandText = "SELECT distinct SourceNodeKey, SinkNodeKey from EESPathList  where SourceNodeKey in (select nodekey from node where marketkey=1 and NodeKey >= " + ConfigurationManager.AppSettings.Get("sKey") + " and  NodeKey <= " + ConfigurationManager.AppSettings.Get("eKey") + ")";
            mSelectPathsCommand.CommandText = "select  b.NodeKey,c.NodeKey from eespathlist a  join node b on a.SourceNodeName=b.NodeName join Node c on a.SinkNodeName=c.NodeName except select  b.NodeKey,c.NodeKey from BlockAlgoMinMaxall a  join node b on a.SourceName=b.NodeName join Node c on a.SinkName=c.NodeName order by  b.NodeKey,c.NodeKey desc";
            mSelectPathsCommand.Connection = mConnection;
            mDtCalculateMinMaxRT = new DataTable();
            mDtCalculateMinMaxRT.Columns.Add("SourceName", typeof(string));
            mDtCalculateMinMaxRT.Columns.Add("SinkName", typeof(string));
            mDtCalculateMinMaxRT.Columns.Add("RTMinAll", typeof(double));
            mDtCalculateMinMaxRT.Columns.Add("RTMaxAll", typeof(double));

            cmdDeleteCommand = new SqlCommand();
            cmdDeleteCommand.CommandText = "truncate table BlockAlgoMinMaxAllTest";
            cmdDeleteCommand.Connection = mConnection;
        }
        private List<Node> GetDarts(SourceSinkData sourceSinkData)
        {
            List<Node> lmpList = new List<Node>();
            try
            {
                lmpList = GetChartData(StartDate, EndDate, sourceSinkData.Source, sourceSinkData.Sink);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            return lmpList;
        }
        public List<Node> GetChartData(DateTime startDate, DateTime endDate, PricingNode sourcePricingNode, PricingNode sinkPricingNode)
        {
            List<Node> rtNodeList = new List<Node>();
            List<Node> daNodeList = new List<Node>();
            Node daSourceNode = new Node();
            daSourceNode.Market = sourcePricingNode.MarketKey;
            daSourceNode.NodeId = sourcePricingNode.NodeKey;
            daSourceNode.PNodeId = sourcePricingNode.ExternalNodeId;
            daSourceNode.NodeName = sourcePricingNode.NodeName;
            List<LmpTimePrice> timePriceList = new List<LmpTimePrice>();
            TimeSpan ts = endDate.AddDays(1) - startDate;
            int daysSpan = (int)Math.Ceiling(ts.TotalDays);
            for (int d = 0; d < daysSpan; d++)
            {
                DateTime tempDate = startDate.Date.AddDays(d);
                for (int h = 1; h < 25; h++)
                {
                    LmpTimePrice tp1 = new LmpTimePrice();
                    tp1.MarketTime = tempDate.AddHours(h);
                    timePriceList.Add(tp1);
                }
            }
            daSourceNode.LmpTimePriceList = timePriceList;
            Node rtSourceNode = new Node();
            rtSourceNode.Market = sourcePricingNode.MarketKey;
            rtSourceNode.NodeId = sourcePricingNode.NodeKey;
            rtSourceNode.PNodeId = sourcePricingNode.ExternalNodeId;
            rtSourceNode.NodeName = sourcePricingNode.NodeName;
            timePriceList = new List<LmpTimePrice>();
            for (int d = 0; d < daysSpan; d++)
            {
                DateTime tempDate = startDate.Date.AddDays(d);
                for (int h = 1; h < 25; h++)
                {
                    LmpTimePrice tp1 = new LmpTimePrice();
                    tp1.MarketTime = tempDate.AddHours(h);
                    timePriceList.Add(tp1);
                }
            }
            rtSourceNode.LmpTimePriceList = timePriceList;
            rtNodeList.Add(rtSourceNode);
            daNodeList.Add(daSourceNode);
            if (sinkPricingNode != null)
            {
                Node daSinkNode = new Node();
                daSinkNode.Market = sinkPricingNode.MarketKey;
                daSinkNode.NodeId = sinkPricingNode.NodeKey;
                daSinkNode.PNodeId = sinkPricingNode.ExternalNodeId;
                daSinkNode.NodeName = sinkPricingNode.NodeName;
                timePriceList = new List<LmpTimePrice>();
                for (int d = 0; d < daysSpan; d++)
                {
                    DateTime tempDate = startDate.Date.AddDays(d);
                    for (int h = 1; h < 25; h++)
                    {
                        LmpTimePrice tp1 = new LmpTimePrice();
                        tp1.MarketTime = tempDate.AddHours(h);
                        timePriceList.Add(tp1);
                    }
                }
                daSinkNode.LmpTimePriceList = timePriceList;
                Node rtSinkNode = new Node();
                rtSinkNode.Market = sinkPricingNode.MarketKey;
                rtSinkNode.NodeId = sinkPricingNode.NodeKey;
                rtSinkNode.PNodeId = sinkPricingNode.ExternalNodeId;
                rtSinkNode.NodeName = sinkPricingNode.NodeName;
                timePriceList = new List<LmpTimePrice>();
                for (int d = 0; d < daysSpan; d++)
                {
                    DateTime tempDate = startDate.Date.AddDays(d);
                    for (int h = 1; h < 25; h++)
                    {
                        LmpTimePrice tp1 = new LmpTimePrice();
                        tp1.MarketTime = tempDate.AddHours(h);
                        timePriceList.Add(tp1);
                    }
                }
                rtSinkNode.LmpTimePriceList = timePriceList;
                rtNodeList.Add(rtSinkNode);
                daNodeList.Add(daSinkNode);
            }
            DARTNode.GetDART(rtNodeList, daNodeList, startDate, ts.Days, false, true);
            try
            {
                for (int j = 0; j < 2; j++)
                {
                    Node spreadNodeTimePrices = new Node();
                    spreadNodeTimePrices.NodeName = sourcePricingNode.NodeName + " -> " + sinkPricingNode.NodeName;
                    spreadNodeTimePrices.LmpTimePriceList = new List<LmpTimePrice>();
                    LmpTimePrice tpSource = new LmpTimePrice();
                    LmpTimePrice tpSink = new LmpTimePrice();
                    LmpTimePrice tpSpread = new LmpTimePrice();
                    List<Node> nodeLMPsList = j == 0 ? daNodeList : rtNodeList;
                    for (int i = 0; i < nodeLMPsList[0].LmpTimePriceList.Count; i++)
                    {
                        tpSpread = new LmpTimePrice();
                        tpSpread.Lmp = new NodePriceLibrary.LMP();
                        tpSpread.MarketTime = nodeLMPsList[0].LmpTimePriceList[i].MarketTime;
                        if (nodeLMPsList[0].LmpTimePriceList[i] != null
                            && nodeLMPsList[0].LmpTimePriceList[i].MarketTime != null
                            && nodeLMPsList[1].LmpTimePriceList[i] != null
                            && nodeLMPsList[1].LmpTimePriceList[i].MarketTime != null)
                        {
                            tpSpread.Lmp.Price = nodeLMPsList[1].LmpTimePriceList[i].Lmp.Price - nodeLMPsList[0].LmpTimePriceList[i].Lmp.Price;
                            tpSpread.Lmp.Congestion = nodeLMPsList[1].LmpTimePriceList[i].Lmp.Congestion - nodeLMPsList[0].LmpTimePriceList[i].Lmp.Congestion;
                            tpSpread.Lmp.Loss = nodeLMPsList[1].LmpTimePriceList[i].Lmp.Loss - nodeLMPsList[0].LmpTimePriceList[i].Lmp.Loss;
                        }
                        else
                        {
                            tpSpread.Lmp.Price = double.NaN;
                            tpSpread.Lmp.Congestion = double.NaN;
                            tpSpread.Lmp.Loss = double.NaN;
                        }
                        spreadNodeTimePrices.LmpTimePriceList.Add(tpSpread);
                    }
                    foreach (Node node1LMPs in nodeLMPsList)
                    {
                        foreach (LmpTimePrice time in node1LMPs.LmpTimePriceList)
                        {
                            if (!double.IsNaN(time.Lmp.Price))
                            {
                                string marketDateTimeNodeIdKey = time.MarketTime.ToString() + node1LMPs.NodeId.ToString();
                                if (!nodeIdLMPHHash.ContainsKey(marketDateTimeNodeIdKey))
                                {
                                    nodeIdLMPHHash.Add(marketDateTimeNodeIdKey, time.Lmp);
                                }
                            }
                        }
                    }
                    if (j == 0)
                    {
                        daNodeList = nodeLMPsList.ToList<Node>();
                        daNodeList.Add(spreadNodeTimePrices);
                    }
                    else
                    {
                        rtNodeList = nodeLMPsList.ToList<Node>();
                        rtNodeList.Add(spreadNodeTimePrices);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return rtNodeList;
        }
    }
    public class HourlyPivotData
    {
        /// <summary>
        /// The m hour data list
        /// </summary>
        private List<double?> mHourDataList;
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime? Date { get; set; }
        /// <summary>
        /// Gets or sets the date display.
        /// </summary>
        /// <value>
        /// The date display.
        /// </value>
        public DateTime? DateDisplay { get; set; }
        /// <summary>
        /// Gets or sets the type of the row.
        /// </summary>
        /// <value>
        /// The type of the row.
        /// </value>
        public string RowType { get; set; }
        /// <summary>
        /// Gets or sets the display type of the row.
        /// </summary>
        /// <value>
        /// The display type of the row.
        /// </value>
        public string RowDisplayType { get; set; }
        /// <summary>
        /// Gets or sets the name of the row.
        /// </summary>
        /// <value>
        /// The name of the row.
        /// </value>
        public string RowName { get; set; }
        /// <summary>
        /// Gets or sets the row day.
        /// </summary>
        /// <value>
        /// The row day.
        /// </value>
        public string RowDay { get; set; }

        /// <summary>
        /// The m total
        /// </summary>
        private double? mTotal;
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public double? Total
        {
            get
            {
                return mTotal;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    mTotal = null;
                }
                else
                {
                    mTotal = value;
                }
            }
        }
        //private double? _MaxLoad;

        //public double? MaxLoad
        //{
        //    get { return _MaxLoad; }
        //    set
        //    {
        //        if (value == 0 || value.Equals(double.NaN))
        //        {
        //            _MaxLoad = null;
        //        }
        //        else
        //            _MaxLoad = value;
        //    }
        //}
        /// <summary>
        /// The m average
        /// </summary>
        private double? mAverage;
        /// <summary>
        /// Gets or sets the average.
        /// </summary>
        /// <value>
        /// The average.
        /// </value>
        public double? Average
        {
            get
            {
                return mAverage;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    mAverage = null;
                }
                else
                {
                    mAverage = value;
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HourlyPivotData"/> class.
        /// </summary>
        public HourlyPivotData()
        {
            mHourDataList = new List<double?>();
            for (int i = 0; i < 24; i++)
            {
                mHourDataList.Add(null);
            }
        }
        /// <summary>
        /// Gets or sets the hour data list.
        /// </summary>
        /// <value>
        /// The hour data list.
        /// </value>
        public List<double?> HourDataList
        {
            get
            {
                return mHourDataList;
            }
            set
            {
                mHourDataList = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e1.
        /// </summary>
        /// <value>
        /// The h e1.
        /// </value>
        public double? HE1
        {
            get
            {
                return mHourDataList[0];
            }
            set
            {
                mHourDataList[0] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e2.
        /// </summary>
        /// <value>
        /// The h e2.
        /// </value>
        public double? HE2
        {
            get
            {
                return mHourDataList[1];
            }
            set
            {
                mHourDataList[1] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e3.
        /// </summary>
        /// <value>
        /// The h e3.
        /// </value>
        public double? HE3
        {
            get
            {
                return mHourDataList[2];
            }
            set
            {
                mHourDataList[2] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e4.
        /// </summary>
        /// <value>
        /// The h e4.
        /// </value>
        public double? HE4
        {
            get
            {
                return mHourDataList[3];
            }
            set
            {
                mHourDataList[3] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e5.
        /// </summary>
        /// <value>
        /// The h e5.
        /// </value>
        public double? HE5
        {
            get
            {
                return mHourDataList[4];
            }
            set
            {
                mHourDataList[4] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e6.
        /// </summary>
        /// <value>
        /// The h e6.
        /// </value>
        public double? HE6
        {
            get
            {
                return mHourDataList[5];
            }
            set
            {
                mHourDataList[5] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e7.
        /// </summary>
        /// <value>
        /// The h e7.
        /// </value>
        public double? HE7
        {
            get
            {
                return mHourDataList[6];
            }
            set
            {
                mHourDataList[6] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e8.
        /// </summary>
        /// <value>
        /// The h e8.
        /// </value>
        public double? HE8
        {
            get
            {
                return mHourDataList[7];
            }
            set
            {
                mHourDataList[7] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e9.
        /// </summary>
        /// <value>
        /// The h e9.
        /// </value>
        public double? HE9
        {
            get
            {
                return mHourDataList[8];
            }
            set
            {
                mHourDataList[8] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e10.
        /// </summary>
        /// <value>
        /// The h e10.
        /// </value>
        public double? HE10
        {
            get
            {
                return mHourDataList[9];
            }
            set
            {
                mHourDataList[9] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e11.
        /// </summary>
        /// <value>
        /// The h e11.
        /// </value>
        public double? HE11
        {
            get
            {
                return mHourDataList[10];
            }
            set
            {
                mHourDataList[10] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e12.
        /// </summary>
        /// <value>
        /// The h e12.
        /// </value>
        public double? HE12
        {
            get
            {
                return mHourDataList[11];
            }
            set
            {
                mHourDataList[11] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e13.
        /// </summary>
        /// <value>
        /// The h e13.
        /// </value>
        public double? HE13
        {
            get
            {
                return mHourDataList[12];
            }
            set
            {
                mHourDataList[12] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e14.
        /// </summary>
        /// <value>
        /// The h e14.
        /// </value>
        public double? HE14
        {
            get
            {
                return mHourDataList[13];
            }
            set
            {
                mHourDataList[13] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e15.
        /// </summary>
        /// <value>
        /// The h e15.
        /// </value>
        public double? HE15
        {
            get
            {
                return mHourDataList[14];
            }
            set
            {
                mHourDataList[14] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e16.
        /// </summary>
        /// <value>
        /// The h e16.
        /// </value>
        public double? HE16
        {
            get
            {
                return mHourDataList[15];
            }
            set
            {
                mHourDataList[15] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e17.
        /// </summary>
        /// <value>
        /// The h e17.
        /// </value>
        public double? HE17
        {
            get
            {
                return mHourDataList[16];
            }
            set
            {
                mHourDataList[16] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e18.
        /// </summary>
        /// <value>
        /// The h e18.
        /// </value>
        public double? HE18
        {
            get
            {
                return mHourDataList[17];
            }
            set
            {
                mHourDataList[17] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e19.
        /// </summary>
        /// <value>
        /// The h e19.
        /// </value>
        public double? HE19
        {
            get
            {
                return mHourDataList[18];
            }
            set
            {
                mHourDataList[18] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e20.
        /// </summary>
        /// <value>
        /// The h e20.
        /// </value>
        public double? HE20
        {
            get
            {
                return mHourDataList[19];
            }
            set
            {
                mHourDataList[19] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e21.
        /// </summary>
        /// <value>
        /// The h e21.
        /// </value>
        public double? HE21
        {
            get
            {
                return mHourDataList[20];
            }
            set
            {
                mHourDataList[20] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e22.
        /// </summary>
        /// <value>
        /// The h e22.
        /// </value>
        public double? HE22
        {
            get
            {
                return mHourDataList[21];
            }
            set
            {
                mHourDataList[21] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e23.
        /// </summary>
        /// <value>
        /// The h e23.
        /// </value>
        public double? HE23
        {
            get
            {
                return mHourDataList[22];
            }
            set
            {
                mHourDataList[22] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e24.
        /// </summary>
        /// <value>
        /// The h e24.
        /// </value>
        public double? HE24
        {
            get
            {
                return mHourDataList[23];
            }
            set
            {
                mHourDataList[23] = value;
            }
        }
    }
  public  class Dates
    {
        public DateTime sDateTime { get; set; }
        public DateTime eDateTime { get; set; }
    }

}
