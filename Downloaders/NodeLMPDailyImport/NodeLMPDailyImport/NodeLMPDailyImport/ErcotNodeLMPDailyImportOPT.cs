
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Data.SqlClient;
using System.ServiceModel.Description;

using Vayu.DBLibrary;
using Vayu.NodePriceLibrary;
using Vayu.NodeLMPLibrary;
using Vayu.CommonAccessLibrary;

namespace Vayu.NodeLMPDailyImport
{
    class ErcotNodeLMPDailyImportOPT
    {
        private static string sDartEndPoint = /*"net.tcp://20.168.249.173:9000/ISubscribe"*/ Vayu.CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress();
        Dictionary<long, string> validCRRNodeDict;
        private SqlConnection DBConnection;
        private SqlCommand mSelectMarketDateTime;
        private SqlCommand mSelectICEMarketDateTime;
        private SqlCommand mDeleteNYISONodeLMPDailyCommand;
        private SqlCommand mDeleteCaisoLMPDailyCommand;
        private SqlCommand mSelectSPPMarketDateTime;
        private DataTable mNodeLMPDT = new DataTable();
        private DataTable mNodeLMPNysioDT = new DataTable();
        private DataTable mNodeLMPErcotDT = new DataTable();
        private DataTable mUniqrecords = new DataTable();
        private LMPDatesHelper mLMPDatesHelper90;
        private Dictionary<int, string> mMarkets = new Dictionary<int, string>();
        List<PathHelper> validCRRPathList;
        public ErcotNodeLMPDailyImportOPT()
        {

            InitDB();
            IninMarket();
            mLMPDatesHelper90 = new LMPDatesHelper(DBConnection);
        }
        private void IninMarket()
        {
            mMarkets.Add(9, "ERCOT");
        }
        public void InitDB()
        {
            mSelectMarketDateTime = new SqlCommand();
            mSelectMarketDateTime.CommandText = "select MarketDateTime,PeakYN from MarketTime (nolock) where MarketDateTime between @startDate and @endDate and MarketKey = @marketkey order by MarketDateTime";
            mSelectMarketDateTime.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectMarketDateTime.Parameters.AddWithValue("@startDate", "startDate");
            mSelectMarketDateTime.Parameters.AddWithValue("@endDate", "endDate");           

            mNodeLMPDT.Columns.Add("SourceNodeKey", typeof(int));
            mNodeLMPDT.Columns.Add("SinkNodeKey", typeof(int));
            mNodeLMPDT.Columns.Add("MarketTypeCode", typeof(string));
            mNodeLMPDT.Columns.Add("MarketDate", typeof(DateTime));
            mNodeLMPDT.Columns.Add("ISOCode", typeof(string));
            mNodeLMPDT.Columns.Add("AvgPeakLMP", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgOffpeakLMP", typeof(decimal));
            mNodeLMPDT.Columns.Add("Avg24LMP", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakLoss", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgOffpeakLoss", typeof(decimal));
            mNodeLMPDT.Columns.Add("Avg24Loss", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakCong", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgOffpeakCong", typeof(decimal));
            mNodeLMPDT.Columns.Add("Avg24Cong", typeof(decimal));
            mNodeLMPDT.Columns.Add("OffpeakHours", typeof(decimal));
            mNodeLMPDT.Columns.Add("PeakHours", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakWDLMP", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakWELMP", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakWDLoss", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakWELoss", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakWDCong", typeof(decimal));
            mNodeLMPDT.Columns.Add("AvgPeakWECong", typeof(decimal));
            mNodeLMPDT.Columns.Add("PeakWDHours", typeof(decimal));
            mNodeLMPDT.Columns.Add("PeakWEHours", typeof(decimal));
            mNodeLMPDT.Columns.Add("UpdatedDdatetime", typeof(DateTime));           

            mUniqrecords.Columns.Add("NodeKey", typeof(int));
            mUniqrecords.Columns.Add("MarketTypeCode", typeof(string));
            mUniqrecords.Columns.Add("MarketDate", typeof(DateTime));
            mUniqrecords.Columns.Add("ISOCode", typeof(string));
            mUniqrecords.Columns.Add("AvgPeakLMP", typeof(decimal));
            mUniqrecords.Columns.Add("AvgOffpeakLMP", typeof(decimal));
            mUniqrecords.Columns.Add("Avg24LMP", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakLoss", typeof(decimal));
            mUniqrecords.Columns.Add("AvgOffpeakLoss", typeof(decimal));
            mUniqrecords.Columns.Add("Avg24Loss", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakCong", typeof(decimal));
            mUniqrecords.Columns.Add("AvgOffpeakCong", typeof(decimal));
            mUniqrecords.Columns.Add("Avg24Cong", typeof(decimal));
            mUniqrecords.Columns.Add("OffpeakHours", typeof(decimal));
            mUniqrecords.Columns.Add("PeakHours", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakWDLMP", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakWELMP", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakWDLoss", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakWELoss", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakWDCong", typeof(decimal));
            mUniqrecords.Columns.Add("AvgPeakWECong", typeof(decimal));
            mUniqrecords.Columns.Add("PeakWDHours", typeof(decimal));
            mUniqrecords.Columns.Add("PeakWEHours", typeof(decimal));
            mUniqrecords.Columns.Add("UpdatedDdatetime", typeof(DateTime));


            //
            mNodeLMPErcotDT.Columns.Add("SourceNodeKey", typeof(int));
            mNodeLMPErcotDT.Columns.Add("SinkNodeKey", typeof(int));
            mNodeLMPErcotDT.Columns.Add("MarketTypeCode", typeof(string));
            mNodeLMPErcotDT.Columns.Add("MarketDate", typeof(DateTime));
            mNodeLMPErcotDT.Columns.Add("AvgPeakLMP", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("AvgOffpeakLMP", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("Avg24LMP", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("AvgPeakLoss", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("AvgOffpeakLoss", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("Avg24Loss", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("AvgPeakCong", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("AvgOffpeakCong", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("Avg24Cong", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("AvgPeakWELMP", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("OffpeakHours", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("PeakHours", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("PeakWEHours", typeof(decimal));
            mNodeLMPErcotDT.Columns.Add("UpdatedDdatetime", typeof(DateTime));
           
            validCRRNodeDict = new Dictionary<long, string>();
        }

        internal void GetValidCRRNode()
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
                         
                        cmd.CommandText = "select a.NodeKey ,  b.NodeKey, a.nodename, b.nodename from Vayu..Node a cross join Vayu..Node b where a.MarketKey = 9 and a.NodeKey != b.NodeKey and a.NodeKey in (select distinct nodekey from Vayu..NodeDALMPH) and b.NodeKey in (select distinct nodekey from Vayu..NodeDALMPH) ";

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
        public void Run(DateTime startDate, DateTime endDate, int marketKey)
        {
            GetValidCRRNode();
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
                foreach (var operationDescription in pipeFactory.Endpoint.Contract.Operations)
                {
                    var dataContractBehavior = operationDescription.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                                    as DataContractSerializerOperationBehavior;
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    }
                }
                ILMP nodeProxy = pipeFactory.CreateChannel();
                HashSet<OldNewNodeInfo> hashInfo = GetMarketNodeDateHash(marketKey);
                while (startDate <= endDate)
                {
                    List<Node> daNodes = new List<Node>();
                    List<Node> rtNodes = new List<Node>();
                    DateTime finalstartdate = startDate;
                    DateTime finalendtdate = startDate.AddDays(1);
                    Console.WriteLine("Processing for Market Key : " + marketKey + " and Date : " + finalstartdate.ToShortDateString());
                    try
                    {
                        Node[] nodes = GetNodes(hashInfo, finalstartdate, finalendtdate, marketKey);
                        daNodes.AddRange(nodeProxy.GetPrice(nodes, true, false, false));
                        //rtNodes.AddRange(nodeProxy.GetPrice(nodes, false, false, false));
                        daNodes.AddRange(nodeProxy.GetAllPrice(marketKey, true, finalstartdate, finalendtdate, false));
                        // rtNodes.AddRange(nodeProxy.GetAllPrice(marketKey, false, finalstartdate, finalendtdate, false));
                        InsertLmps(marketKey, nodeProxy, finalstartdate, finalendtdate, daNodes.ToArray(), rtNodes.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                    startDate = startDate.AddDays(1);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private Node[] GetNodes(HashSet<OldNewNodeInfo> infoList, DateTime startDate, DateTime endDate, int marketKey)
        {
            
                DBAccess.GetNodeFromeExtId(Convert.ToInt32(57457), marketKey);//HB_NORTH
            Func<long, PricingNode> getPricingNode = null;
            if (marketKey == 1)
            {
                getPricingNode = (key) =>
                {
                    return DBAccess.GetNodeFromeExtId(Convert.ToInt64(key), marketKey);
                };
            }
            else
            {
                getPricingNode = (key) =>
                {
                    return DBAccess.GetNode(Convert.ToInt64(key), marketKey);
                };
            }
            List<Node> nodes = new List<Node>();
            Action<long> addNodes = (nodeID) =>
            {
                try
                {
                    PricingNode node = getPricingNode(nodeID);
                    if (node == null)
                        return;

                    Node n = new Node();
                    n.LmpTimePriceList = GetLmpTimePriceList(startDate, endDate);
                    n.NodeId = node.NodeKey;
                    n.PNodeId = node.ExternalNodeId;
                    n.Market = node.MarketKey;
                    n.TimePriceList = null;
                    n.NodeName = node.NodeName;
                    nodes.Add(n);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };
            foreach (OldNewNodeInfo info in infoList)
            {
                if (startDate.Date < info.UpdateDateTime.Date)
                {
                    if (marketKey == 1)
                        addNodes(info.NewNodeKey);
                    else
                        addNodes(info.NewNodeKey);
                }
                else
                    addNodes(info.OldNodeKey);
            }
            return nodes.ToArray();
        }
        private void InsertLmps(int marketKey, ILMP nodeProxy, DateTime finalstartdate, DateTime finalendtdate, Node[] daNodes, Node[] rtNodes)
        {

            if (marketKey == 9)
            {
                SaveLmpRTDAERCOT(daNodes, "DA", finalstartdate, finalendtdate, marketKey);
            }
        }

        private void SaveLmpRTDAERCOT(Node[] nodeHash, string MarketTypeCode, DateTime finalStartDate, DateTime finalEndDate, int marketKey)
        {
            try
            {
                string markename = "";
               List<DateTime> peakList = mLMPDatesHelper90.GetPeakDates(marketKey, finalStartDate, finalEndDate, false);
                List<DateTime> offpeakList = mLMPDatesHelper90.GetOffPeakDates(marketKey, finalStartDate, finalEndDate, false);
                List<DateTime> peakweList = mLMPDatesHelper90.GetPeakWEDates(marketKey, finalStartDate, finalEndDate, false);
                mNodeLMPErcotDT.Clear();
                mUniqrecords.Clear();
                foreach (PathHelper path in validCRRPathList)
                {

                    //if (path.SourceKey == 72049 && path.SinkKey == 57381)
                    {
                        DataRow nodeLmpRow = mNodeLMPErcotDT.NewRow();
                        SetDailyAverageErcot(marketKey, path, peakList, offpeakList, peakweList, nodeLmpRow, finalStartDate, false, MarketTypeCode, nodeHash);
                        string a = nodeLmpRow["SourceNodeKey"].ToString();
                        if (nodeLmpRow["SourceNodeKey"].ToString() != "")
                        {
                            //if (Convert.ToInt32(nodeLmpRow["OffpeakHours"]) > 0 && Convert.ToInt32(nodeLmpRow["PeakHours"]) > 0 && Convert.ToInt32(nodeLmpRow["PeakWEHours"]) > 0)
                                nodeLmpRow["UpdatedDdatetime"] = DateTime.Now;
                            mNodeLMPErcotDT.Rows.Add(nodeLmpRow);
                        }
                    }
                }
                int b = mNodeLMPErcotDT.Rows.Count;
                if (mNodeLMPErcotDT.Rows.Count > 0)
                {
                    //  ConcurrentDictionary<string, string> connection = DBConnectionHelper.ConnectionHelper.ConnectDB();
                    DBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

                    if (DBConnection.State == ConnectionState.Open)
                    {
                        DBConnection.Close();
                    }
                    DBConnection.Open();
                    if (mMarkets.ContainsKey(marketKey))
                    {
                        markename = mMarkets[marketKey];
                    }
                     
                    mDeleteNYISONodeLMPDailyCommand = new SqlCommand();
                    mDeleteNYISONodeLMPDailyCommand.CommandText = "Truncate table Vayu..NodeLMPDailysOPT_Test";
                    mDeleteNYISONodeLMPDailyCommand.Connection = DBConnection;
                    mDeleteNYISONodeLMPDailyCommand.ExecuteNonQuery();
                    SqlTransaction transaction = DBConnection.BeginTransaction();
                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        try
                        {
                            bkLmpH.DestinationTableName = "Vayu..NodeLMPDailysOPT_Test";
                            bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");//
                            bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");//
                            bkLmpH.ColumnMappings.Add("MarketTypeCode", "MarketTypeCode");//
                            bkLmpH.ColumnMappings.Add("MarketDate", "MarketDate");//
                            bkLmpH.ColumnMappings.Add("AvgPeakLMP", "AvgPeakLMP");//
                            bkLmpH.ColumnMappings.Add("AvgOffpeakLMP", "AvgOffpeakLMP");//
                            bkLmpH.ColumnMappings.Add("Avg24LMP", "Avg24LMP");//
                            bkLmpH.ColumnMappings.Add("AvgPeakLoss", "AvgPeakLoss");
                            bkLmpH.ColumnMappings.Add("AvgOffpeakLoss", "AvgOffpeakLoss");
                            bkLmpH.ColumnMappings.Add("Avg24Loss", "Avg24Loss");
                            bkLmpH.ColumnMappings.Add("AvgPeakCong", "AvgPeakCong");
                            bkLmpH.ColumnMappings.Add("AvgOffpeakCong", "AvgOffpeakCong");
                            bkLmpH.ColumnMappings.Add("Avg24Cong", "Avg24Cong");
                            bkLmpH.ColumnMappings.Add("AvgPeakWELMP", "AvgPeakWELMP");//
                            bkLmpH.ColumnMappings.Add("OffpeakHours", "OffpeakHours");//
                            bkLmpH.ColumnMappings.Add("PeakHours", "PeakHours");//
                            bkLmpH.ColumnMappings.Add("PeakWEHours", "PeakWEHours");//
                            bkLmpH.ColumnMappings.Add("UpdatedDdatetime", "UpdatedDdatetime");//
                            bkLmpH.WriteToServer(mNodeLMPErcotDT);
                            SqlCommand updatenodelmph = new SqlCommand("DeleteDupsDailyLMPImpsOPT", DBConnection, transaction);
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
                    if (DBConnection.State == ConnectionState.Open)
                    {
                        DBConnection.Close();
                    }
                    DBConnection.Open();
                    SqlTransaction transaction1 = DBConnection.BeginTransaction();
                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction1))
                    {
                        try
                        {
                            SqlCommand updatenodelmph = new SqlCommand("UpMergeNodeLMPDailysOPT", DBConnection, transaction1);
                            updatenodelmph.CommandType = CommandType.StoredProcedure;
                            updatenodelmph.CommandTimeout = 30000;
                            var upcount = updatenodelmph.ExecuteNonQuery();
                            transaction1.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction1.Rollback();

                        }
                    }
                    DBConnection.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
       
        private void SetDailyAverageErcot(int marketKey, PathHelper path, List<DateTime> peakList, List<DateTime> offpeakList, List<DateTime> peakweList, DataRow nodeLmpRow, DateTime finalStartDate, bool value, string MarketTypeCode1, Node[] nodeHash)
        {
            double? avgPeakLmp = null;
            double? avgPeakLoss = null;
            double? avgPeakCong = null;
            double? avgOffPeakLmp = null;
            double? avgPeakWELmp = null;
            double? avgOffPeakLoss = null;
            double? avg24LMP = null;
            double? avg24Loss = null;
            double? avgOffPeakCong = null;
            double? avg24Cong = null;
            int peakCount = 0;
            int offPeakCount = 0;
            int avgCount = 0;
            int peakWECount = 0;
            Node aNode = new Node();
            Node bNode = new Node();
            aNode = nodeHash.Where(x => x.NodeId == path.SourceKey).FirstOrDefault();
            bNode = nodeHash.Where(x => x.NodeId == path.SinkKey).FirstOrDefault();

            try
            {
                if (aNode != null)
                    if (bNode != null)
                    {
                        foreach (LmpTimePrice lmpTimePrice in aNode.LmpTimePriceList)
                        {

                            if (lmpTimePrice.Lmp == null)
                                continue;
                            LmpTimePrice sinkLmpTimePrice = new LmpTimePrice();
                            sinkLmpTimePrice = bNode.LmpTimePriceList.Where(x => x.MarketTime == lmpTimePrice.MarketTime).FirstOrDefault();
                            if (sinkLmpTimePrice.Lmp == null)
                                continue;

                            if ((!double.IsNaN(lmpTimePrice.Lmp.Price)) || (!double.IsNaN(lmpTimePrice.Lmp.Congestion)) || (!double.IsNaN(lmpTimePrice.Lmp.Loss)))
                            {
                                if (peakList.Contains(lmpTimePrice.MarketTime))
                                {
                                    // if ((!double.IsNaN(sinkLmpTimePrice.Lmp.Price)) || (!double.IsNaN(sinkLmpTimePrice.Lmp.Congestion)) || (!double.IsNaN(sinkLmpTimePrice.Lmp.Loss)))
                                    {
                                        if (avgPeakLmp == null && Convert.ToDouble(lmpTimePrice.Lmp.Price) != null)
                                        {
                                            avgPeakLmp = Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                        }
                                        else
                                        {
                                            avgPeakLmp += Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                        }
                                    }
                                    peakCount++;
                                }
                                else if (offpeakList.Contains(lmpTimePrice.MarketTime))
                                {
                                    if (avgOffPeakLmp == null && Convert.ToDouble(lmpTimePrice.Lmp.Price) != null)
                                    {
                                        avgOffPeakLmp = Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                    }
                                    else
                                    {
                                        avgOffPeakLmp += Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                    }
                                    offPeakCount++;
                                }
                                else if (peakweList.Contains(lmpTimePrice.MarketTime))
                                {
                                    if (avgPeakWELmp == null && Convert.ToDouble(lmpTimePrice.Lmp.Price) != null)
                                    {
                                        avgPeakWELmp = Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                    }
                                    else
                                    {
                                        avgPeakWELmp += Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                    }
                                    peakWECount++;
                                }
                                if (avg24LMP == null && Convert.ToDouble(lmpTimePrice.Lmp.Price) != null)
                                {
                                    avg24LMP = Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                }
                                else
                                {
                                    avg24LMP += Math.Max(0, (Convert.ToDouble(sinkLmpTimePrice.Lmp.Price - lmpTimePrice.Lmp.Price)));
                                }
                                avgCount++;
                            }
                        }

                       // if (offPeakCount > 0 || peakCount > 0 || peakWECount > 0)
                        {
                            nodeLmpRow["SourceNodeKey"] = Convert.ToInt32(aNode.NodeId);//57319
                            nodeLmpRow["SinkNodeKey"] = Convert.ToInt32(bNode.NodeId);//57319
                            if (value == true)
                            {
                                nodeLmpRow["MarketKey"] = marketKey;
                                if (marketKey == 7)
                                    nodeLmpRow["MarketTypeCode"] = MarketTypeCode1;

                            }
                            if (value == false)
                            {
                                nodeLmpRow["MarketTypeCode"] = MarketTypeCode1;
                            }
                            if (value == true)
                            {
                                nodeLmpRow["MarketDateTime"] = finalStartDate.ToShortDateString();
                            }
                            else
                            {
                                nodeLmpRow["MarketDate"] = finalStartDate.ToShortDateString();
                            }
                            nodeLmpRow["OffpeakHours"] = offPeakCount;
                            nodeLmpRow["PeakHours"] = peakCount;
                            nodeLmpRow["PeakWEHours"] = peakWECount;
                            nodeLmpRow["UpdatedDdatetime"] = DateTime.Now;
                            //peakWD
                            if (avgPeakLmp != null)
                            {
                                nodeLmpRow["AvgPeakLMP"] = avgPeakLmp / peakCount;
                            }
                            if (avgPeakLoss != null)
                            {
                                nodeLmpRow["AvgPeakLoss"] = avgPeakLoss / peakCount;
                            }
                            if (avgPeakCong != null)
                            {
                                nodeLmpRow["AvgPeakCong"] = avgPeakCong / peakCount;
                            }
                            //
                            //peakWD
                            if (avgPeakWELmp != null)
                            {
                                nodeLmpRow["AvgPeakWELMP"] = avgPeakWELmp / peakWECount;
                            }

                            //Offpeak
                            if (avgOffPeakLmp != null)
                            {
                                nodeLmpRow["AvgOffpeakLMP"] = avgOffPeakLmp / offPeakCount;
                            }
                            if (avgOffPeakLoss != null)
                            {
                                nodeLmpRow["AvgOffpeakLoss"] = avgOffPeakLoss / offPeakCount;
                            }
                            if (avgOffPeakCong != null)
                            {
                                nodeLmpRow["AvgOffpeakCong"] = avgOffPeakCong / offPeakCount;
                            }
                            //24hrs
                            if (avg24LMP != null)
                            {
                                nodeLmpRow["Avg24LMP"] = avg24LMP / avgCount;
                            }
                            if (avg24Loss != null)
                            {
                                nodeLmpRow["Avg24Loss"] = avg24Loss / avgCount;
                            }
                            if (avg24Cong != null)
                            {
                                nodeLmpRow["Avg24Cong"] = avg24Cong / avgCount;
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
            }
        }

        private List<LmpTimePrice> GetLmpTimePriceList(DateTime item, DateTime finaldate)
        {
            List<LmpTimePrice> tempList = new List<LmpTimePrice>();
            DateTime tempdate = item;
            while (tempdate < finaldate)
            {
                tempList.Add(new LmpTimePrice
                {
                    MarketTime = tempdate.AddHours(1)
                });
                tempdate = tempdate.AddHours(1);
            }
            return tempList;
        }
        private HashSet<OldNewNodeInfo> GetMarketNodeDateHash(int marketKey)
        {
            HashSet<OldNewNodeInfo> hashInfo = new HashSet<OldNewNodeInfo>();
            DBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            if (DBConnection.State == ConnectionState.Open)
            {
                DBConnection.Close();
            }
            DBConnection.Open();
            using (SqlConnection con = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.Connection = DBConnection;
                    switch (marketKey)
                    {

                        case 9:
                            cmd.CommandText = " select distinct CONVERT(VARCHAR(10), getdate(), 111),N1.Nodekey ,N1.Nodekey from Vayu..Node N1 " +
                                              " join Vayu..EESPathList EE ON N1.NodeKey=EE.SourceNodeKey where N1.MarketKey=9";
                            break;
                    }
                            
                    if (cmd.Connection.State == ConnectionState.Open)
                    {
                        cmd.Connection.Close();
                    }
                    cmd.Connection.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime date = reader.IsDBNull(0) ? new DateTime() : Convert.ToDateTime(reader.GetValue(0));
                        long oldNode = reader.IsDBNull(1) ? -1 : Convert.ToInt64(reader.GetValue(1));
                        long newNode = reader.IsDBNull(2) ? -1 : Convert.ToInt64(reader.GetValue(2));
                        if (oldNode <= 0 && newNode <= 0)
                            continue;
                        hashInfo.Add(new OldNewNodeInfo()
                        {
                            UpdateDateTime = date,
                            OldNodeKey = oldNode,
                            NewNodeKey = newNode
                        });
                    }
                    reader.Close();
                    cmd.Connection.Close();
                }
            }
            return hashInfo;
        }
    }
    public struct OldNewNodeInfo
    {
        public DateTime UpdateDateTime;
        public long OldNodeKey;
        public long NewNodeKey;
    }
    public class PathHelper
    {
        public long SourceKey { get; set; }
        public long SinkKey { get; set; }
    }
}
