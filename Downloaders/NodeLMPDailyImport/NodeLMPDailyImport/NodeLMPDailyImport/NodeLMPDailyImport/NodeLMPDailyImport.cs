using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.SqlClient;
//using Sigma.NodeLMPLibrary;
using Sigma.NodePriceLibrary;
using System.Threading;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using Sigma.AppConfigHelper;
using Sigma.CommonAccessLibrary;
using Sigma.DBLibrary;
using Sigma.NodeLMPLibrary;
using Sigma.NodePriceLibrary;

//using TraderApp.DB;
using System.Collections.Concurrent;

namespace NodeLMPDailyImport
{
    class NodeLMPDailyImport
    {
        private static string sDartEndPoint = "net.tcp://64.147.124.37:7000/ISubscribe";
        
        private SqlConnection mConnection90;
        private SqlCommand mSelectMarketDateTime;
        private SqlCommand mSelectICEMarketDateTime;
        private SqlCommand mDeleteNYISONodeLMPDailyCommand;
        private SqlCommand mDeleteCaisoLMPDailyCommand;
        private SqlCommand mSelectSPPMarketDateTime;
        private DataTable mNodeLMPDT = new DataTable();
        private DataTable mNodeLMPNysioDT = new DataTable();
        private DataTable mUniqrecords = new DataTable();
        private DataTable mCaisoUniqrecords = new DataTable();
        private DataTable mCaisoNodeLMPDT = new DataTable();
        private LMPDatesHelper mLMPDatesHelper90;
        private Dictionary<int, string> mMarkets = new Dictionary<int, string>();
        public NodeLMPDailyImport()
        {
           
            InitDB();
            IninMarket();
            mLMPDatesHelper90 = new LMPDatesHelper(mConnection90);
        }
        private void IninMarket()
        {
            mMarkets.Add(1, "PJM");
            //mMarkets.Add(2, "MISO");
            //mMarkets.Add(7, "CAISO");
            //mMarkets.Add(12, "SPP");
        }
        public void InitDB()
        {
            mSelectMarketDateTime = new SqlCommand();
            mSelectMarketDateTime.CommandText = "select MarketDateTime,PeakYN from MarketTime (nolock) where MarketDateTime between @startDate and @endDate and MarketKey = @marketkey order by MarketDateTime";
            mSelectMarketDateTime.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectMarketDateTime.Parameters.AddWithValue("@startDate", "startDate");
            mSelectMarketDateTime.Parameters.AddWithValue("@endDate", "endDate");
            //
            mSelectSPPMarketDateTime = new SqlCommand();
            mSelectSPPMarketDateTime.CommandText = "select MarketDateTime,PeakYN from SPP.MarketDate (nolock) where MarketDateTime between @startDate and @endDate and MarketKey = @marketkey order by MarketDateTime";
            mSelectSPPMarketDateTime.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectSPPMarketDateTime.Parameters.AddWithValue("@startDate", "startDate");
            mSelectSPPMarketDateTime.Parameters.AddWithValue("@endDate", "endDate");
            //
            mSelectICEMarketDateTime = new SqlCommand();
            mSelectICEMarketDateTime.CommandText = "select MarketDateTime,PeakYN from icemarketdate (nolock) where MarketDateTime between @startDate and @endDate and MarketKey = @marketkey order by MarketDateTime";
            mSelectICEMarketDateTime.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectICEMarketDateTime.Parameters.AddWithValue("@startDate", "startDate");
            mSelectICEMarketDateTime.Parameters.AddWithValue("@endDate", "endDate");
            //
            mNodeLMPDT.Columns.Add("NodeKey", typeof(int));
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

            //
            mNodeLMPNysioDT.Columns.Add("NodeKey", typeof(int));
            mNodeLMPNysioDT.Columns.Add("MarketTypeCode", typeof(string));
            mNodeLMPNysioDT.Columns.Add("MarketDate", typeof(DateTime));
            mNodeLMPNysioDT.Columns.Add("AvgPeakLMP", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("AvgOffpeakLMP", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("Avg24LMP", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("AvgPeakLoss", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("AvgOffpeakLoss", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("Avg24Loss", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("AvgPeakCong", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("AvgOffpeakCong", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("Avg24Cong", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("OffpeakHours", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("PeakHours", typeof(decimal));
            mNodeLMPNysioDT.Columns.Add("UpdatedDdatetime", typeof(DateTime));
            //
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
            mCaisoNodeLMPDT.Columns.Add("NodeKey", typeof(int));
            mCaisoNodeLMPDT.Columns.Add("MarketKey", typeof(string));
            mCaisoNodeLMPDT.Columns.Add("MarketTypeCode", typeof(string));
            mCaisoNodeLMPDT.Columns.Add("MarketDateTime", typeof(DateTime));
            mCaisoNodeLMPDT.Columns.Add("AvgPeakLMP", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("AvgOffpeakLMP", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("Avg24LMP", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("AvgPeakLoss", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("AvgOffpeakLoss", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("Avg24Loss", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("AvgPeakCong", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("AvgOffpeakCong", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("Avg24Cong", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("OffpeakHours", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("PeakHours", typeof(decimal));
            mCaisoNodeLMPDT.Columns.Add("UpdatedDdatetime", typeof(DateTime));

            //
            mCaisoUniqrecords.Columns.Add("NodeKey", typeof(int));
            mCaisoUniqrecords.Columns.Add("MarketKey", typeof(string));
            mCaisoUniqrecords.Columns.Add("MarketTypeCode", typeof(string));
            mCaisoUniqrecords.Columns.Add("MarketDateTime", typeof(DateTime));
            mCaisoUniqrecords.Columns.Add("AvgPeakLMP", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("AvgOffpeakLMP", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("Avg24LMP", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("AvgPeakLoss", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("AvgOffpeakLoss", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("Avg24Loss", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("AvgPeakCong", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("AvgOffpeakCong", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("Avg24Cong", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("OffpeakHours", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("PeakHours", typeof(decimal));
            mCaisoUniqrecords.Columns.Add("UpdatedDdatetime", typeof(DateTime));
            //
            mDeleteCaisoLMPDailyCommand = new SqlCommand();
            mDeleteCaisoLMPDailyCommand.CommandText = "Truncate table [TradingData].[Caiso].[NodeLMPDaily_test]";
        }
        public void Run(DateTime startDate, DateTime endDate, int marketKey)
        {
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
                        rtNodes.AddRange(nodeProxy.GetPrice(nodes, false, false, false));
                        daNodes.AddRange(nodeProxy.GetAllPrice(marketKey, true, finalstartdate, finalendtdate, false));
                        rtNodes.AddRange(nodeProxy.GetAllPrice(marketKey, false, finalstartdate, finalendtdate, false));
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
            Func<long, PricingNode> getPricingNode = null;
            if (marketKey == 1)
            {
                getPricingNode = (key) =>
                {
                    return DBAccess.GetNodeFromeExtId(Convert.ToInt32(key));
                };
            }
            else
            {
                getPricingNode = (key) =>
                {
                    return DBAccess.GetNode(Convert.ToInt32(key));
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
                }
                else
                    addNodes(info.OldNodeKey);
            }
            return nodes.ToArray();
        }
        private void InsertLmps(int marketKey, ILMP nodeProxy, DateTime finalstartdate, DateTime finalendtdate, Node[] daNodes, Node[] rtNodes)
        {
            if ((marketKey == 12) || marketKey == 2 || marketKey == 1 || (marketKey == 7))
            {
                SaveLmpRTDA(daNodes, "DA", finalstartdate, finalendtdate, marketKey);
                SaveLmpRTDA(rtNodes, "RT", finalstartdate, finalendtdate, marketKey);
            }
        }
        private void SaveLmpRTDA(Node[] nodeHash, string MarketTypeCode, DateTime finalStartDate, DateTime finalEndDate, int marketKey)
        {
            try
            {
                string markename = "";
                List<DateTime> peakList = mLMPDatesHelper90.GetPeakDates(marketKey, finalStartDate, finalEndDate, false);
                mNodeLMPNysioDT.Clear();
                mUniqrecords.Clear();
                foreach (Node priceNode in nodeHash)
                {
                    DataRow nodeLmpRow = mNodeLMPNysioDT.NewRow();
                    SetDailyAverage(marketKey, priceNode, peakList, nodeLmpRow, finalStartDate, false, MarketTypeCode);
                    nodeLmpRow["UpdatedDdatetime"] = DateTime.Now;
                    mNodeLMPNysioDT.Rows.Add(nodeLmpRow);
                }
                int b = mNodeLMPNysioDT.Rows.Count;
              //  ConcurrentDictionary<string, string> connection = DBConnectionHelper.ConnectionHelper.ConnectDB();
                mConnection90 = new SqlConnection(Sigma.CommonAccessLibrary.DBConnectionCredentials.GetTradingDBConnection());

                if (mConnection90.State == ConnectionState.Open)
                {
                    mConnection90.Close();
                }
                mConnection90.Open();
                if (mMarkets.ContainsKey(marketKey))
                {
                    markename = mMarkets[marketKey];
                }
                mDeleteNYISONodeLMPDailyCommand = new SqlCommand();
                mDeleteNYISONodeLMPDailyCommand.CommandText = "Truncate table " + markename + ".NodeLMPDailyTest";
                mDeleteNYISONodeLMPDailyCommand.Connection = mConnection90;
                mDeleteNYISONodeLMPDailyCommand.ExecuteNonQuery();
                SqlTransaction transaction = mConnection90.BeginTransaction();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mConnection90, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkLmpH.DestinationTableName = "[TradingData].[" + markename + "].[NodeLMPDailyTest]";
                        bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                        bkLmpH.ColumnMappings.Add("MarketTypeCode", "MarketTypeCode");
                        bkLmpH.ColumnMappings.Add("MarketDate", "MarketDate");
                        bkLmpH.ColumnMappings.Add("AvgPeakLMP", "AvgPeakLMP");
                        bkLmpH.ColumnMappings.Add("AvgOffpeakLMP", "AvgOffpeakLMP");
                        bkLmpH.ColumnMappings.Add("Avg24LMP", "Avg24LMP");
                        bkLmpH.ColumnMappings.Add("AvgPeakLoss", "AvgPeakLoss");
                        bkLmpH.ColumnMappings.Add("AvgOffpeakLoss", "AvgOffpeakLoss");
                        bkLmpH.ColumnMappings.Add("Avg24Loss", "Avg24Loss");
                        bkLmpH.ColumnMappings.Add("AvgPeakCong", "AvgPeakCong");
                        bkLmpH.ColumnMappings.Add("AvgOffpeakCong", "AvgOffpeakCong");
                        bkLmpH.ColumnMappings.Add("Avg24Cong", "Avg24Cong");
                        bkLmpH.ColumnMappings.Add("OffpeakHours", "OffpeakHours");
                        bkLmpH.ColumnMappings.Add("PeakHours", "PeakHours");
                        bkLmpH.ColumnMappings.Add("UpdatedDdatetime", "UpdatedDdatetime");
                        bkLmpH.WriteToServer(mNodeLMPNysioDT);
                        SqlCommand updatenodelmph = new SqlCommand("[TradingData].[" + markename + "].[DeleteDups]", mConnection90, transaction);
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
                mConnection90.Close();
                if (mConnection90.State == ConnectionState.Open)
                {
                    mConnection90.Close();
                }
                mConnection90.Open();
                SqlTransaction transaction1 = mConnection90.BeginTransaction();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mConnection90, SqlBulkCopyOptions.TableLock, transaction1))
                {
                    try
                    {
                        SqlCommand updatenodelmph = new SqlCommand("[TradingData].[" + markename + "].[UpMergeNodeLMPDaily]", mConnection90, transaction1);
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
                mConnection90.Close();
            }
            catch (Exception ex)
            {
               
            }
        }
        private void SetDailyAverage(int marketKey, Node priceNode, List<DateTime> peakList, DataRow nodeLmpRow, DateTime finalStartDate, bool value, string MarketTypeCode1)
        {
            double? avgPeakLmp = null;
            double? avgPeakLoss = null;
            double? avgPeakCong = null;
            double? avgOffPeakLmp = null;
            double? avgOffPeakLoss = null;
            double? avg24LMP = null;
            double? avg24Loss = null;
            double? avgOffPeakCong = null;
            double? avg24Cong = null;
            int peakCount = 0;
            int offPeakCount = 0;
            int avgCount = 0;
            try
            {
                foreach (LmpTimePrice lmpTimePrice in priceNode.LmpTimePriceList)
                {
                    if (lmpTimePrice.Lmp == null)
                        continue;

                    if ((!double.IsNaN(lmpTimePrice.Lmp.Price)) || (!double.IsNaN(lmpTimePrice.Lmp.Congestion)) || (!double.IsNaN(lmpTimePrice.Lmp.Loss)))
                    {
                        if (peakList.Contains(lmpTimePrice.MarketTime))
                        {
                            if (avgPeakLmp == null && Convert.ToDouble(lmpTimePrice.Lmp.Price) != null)
                            {
                                avgPeakLmp = Convert.ToDouble(lmpTimePrice.Lmp.Price);
                            }
                            else
                            {
                                avgPeakLmp += Convert.ToDouble(lmpTimePrice.Lmp.Price);
                            }
                            if (avgPeakLoss == null && Convert.ToDouble(lmpTimePrice.Lmp.Loss) != null)
                            {
                                avgPeakLoss = Convert.ToDouble(lmpTimePrice.Lmp.Loss);
                            }
                            else
                            {
                                avgPeakLoss += Convert.ToDouble(lmpTimePrice.Lmp.Loss);
                            }
                            if (avgPeakCong == null && Convert.ToDouble(lmpTimePrice.Lmp.Congestion) != null)
                            {
                                avgPeakCong = Convert.ToDouble(lmpTimePrice.Lmp.Congestion);
                            }
                            else
                            {
                                avgPeakCong += Convert.ToDouble(lmpTimePrice.Lmp.Congestion);
                            }
                            peakCount++;
                        }
                        else
                        {

                            if (avgOffPeakLmp == null && Convert.ToDouble(lmpTimePrice.Lmp.Price) != null)
                            {
                                avgOffPeakLmp = Convert.ToDouble(lmpTimePrice.Lmp.Price);
                            }
                            else
                            {
                                avgOffPeakLmp += Convert.ToDouble(lmpTimePrice.Lmp.Price);
                            }

                            if (avgOffPeakLoss == null && Convert.ToDouble(lmpTimePrice.Lmp.Loss) != null)
                            {
                                avgOffPeakLoss = Convert.ToDouble(lmpTimePrice.Lmp.Loss);
                            }
                            else
                            {
                                avgOffPeakLoss += Convert.ToDouble(lmpTimePrice.Lmp.Loss);
                            }

                            if (avgOffPeakCong == null && Convert.ToDouble(lmpTimePrice.Lmp.Congestion) != null)
                            {
                                avgOffPeakCong = Convert.ToDouble(lmpTimePrice.Lmp.Congestion);
                            }
                            else
                            {
                                avgOffPeakCong += Convert.ToDouble(lmpTimePrice.Lmp.Congestion);
                            }
                            offPeakCount++;
                        }
                        if (avg24LMP == null && Convert.ToDouble(lmpTimePrice.Lmp.Price) != null)
                        {
                            avg24LMP = Convert.ToDouble(lmpTimePrice.Lmp.Price);
                        }
                        else
                        {
                            avg24LMP += Convert.ToDouble(lmpTimePrice.Lmp.Price);
                        }
                        if (avg24Loss == null && Convert.ToDouble(lmpTimePrice.Lmp.Loss) != null)
                        {
                            avg24Loss = Convert.ToDouble(lmpTimePrice.Lmp.Loss);
                        }
                        else
                        {
                            avg24Loss += Convert.ToDouble(lmpTimePrice.Lmp.Loss);
                        }
                        if (avg24Cong == null && Convert.ToDouble(lmpTimePrice.Lmp.Congestion) != null)
                        {
                            avg24Cong = Convert.ToDouble(lmpTimePrice.Lmp.Congestion);
                        }
                        else
                        {
                            avg24Cong += Convert.ToDouble(lmpTimePrice.Lmp.Congestion);
                        }
                        avgCount++;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            nodeLmpRow["NodeKey"] = Convert.ToInt32(priceNode.NodeId);
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
            nodeLmpRow["UpdatedDdatetime"] = DateTime.Now;
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
        //private IEnumerable<Node> GetOldMISOEmptyNodeList(DateTime startDt, DateTime endDateDt)
        //{
        //    List<Node> nodes = new List<Node>();
        //    SqlCommand cmd = ConfigHelper.
        //    cmd.CommandText = "select OldNodeKey from MISO.FTROldNewNodeMapping";
        //    cmd.Connection.Open();
        //    SqlDataReader reader = cmd.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        Node node = new Node();
        //        node.Market = 2;
        //        node.NodeId = ConfigHelper.GetInt(reader[0]);
        //        node.LmpTimePriceList = new List<LmpTimePrice>();
        //        DateTime stDate = startDt;
        //        DateTime edDate = endDateDt;
        //        while (stDate < edDate)
        //        {
        //            for (int hour = 1; hour < 25; hour++)
        //            {
        //                stDate = startDt.AddHours(hour);
        //                LmpTimePrice time = new LmpTimePrice();
        //                LMPLibrary2.LMP lmp = new LMPLibrary2.LMP();
        //                lmp.Price = double.NaN;
        //                lmp.Loss = double.NaN;
        //                lmp.Congestion = double.NaN;
        //                time.Lmp = lmp;
        //                time.MarketTime = stDate;
        //                node.LmpTimePriceList.Add(time);
        //            }
        //        }
        //        nodes.Add(node);
        //    }
        //    reader.Close();
        //    cmd.Connection.Close();
        //    return nodes;
        //}
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
         //   ConcurrentDictionary<string, string> connection = DBConnectionHelper.ConnectionHelper.ConnectDB();
            mConnection90 = new SqlConnection(Sigma.CommonAccessLibrary.DBConnectionCredentials.GetTradingDBConnection());
            if (mConnection90.State == ConnectionState.Open)
            {
                mConnection90.Close();
            }
            mConnection90.Open();
            using (SqlConnection con = new SqlConnection(mConnection90.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.Connection = mConnection90;
                    switch (marketKey)
                    {
                        case 1:
                            cmd.CommandText = "select UpdatedDate,oldnodekey,newnodekey from PJM.FTROldNewNodeMapping";
                            break;
                        //case 2:
                        //    cmd.CommandText = "select UpdatedDate,oldnodekey,newnodekey from MISO.FTROldNewNodeMapping";
                        //    break;
                        ///* case 7: cmd.CommandText = "select UpdatedDate,oldnodekey,newnodekey from PJM.FTROldNewNodeMapping";
                        //     break;*/
                        //default:
                            cmd.CommandText = "select UpdatedDate,oldnodekey,newnodekey from PJM.FTROldNewNodeMapping";
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
}

