using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.MarketViewService
{
    [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    class PriceServer: ILMPMarketView
    {
        private SqlConnection vayuDBConnection;
        private SqlCommand cmdSelectAllErcotPriceRTCommand;
        private SqlCommand cmdSelectERCOTAllNodesFiveMinRTCommand;
        private SqlCommand cmdSelectErcotAllNodesDACommand;
        private SqlCommand cmdSelectPriceERCOTFiveMinRTCommand;
        private SqlCommand cmdSelectErcotPriceRTCommand;
        private SqlCommand cmdSelectNodeNameCommand;
        private SqlCommand cmdSelectErcotSourceSinkNodeCommand;

        private static Dictionary<long, List<long>> dictPnodeHash = new Dictionary<long, List<long>>();
        private static Dictionary<long, List<NodeChange>> dictOldNodeHash = new Dictionary<long, List<NodeChange>>();
        private static List<ExternalNodeMapping> lstExternalMappingList = new List<ExternalNodeMapping>();
        private static List<long> lstAggList = new List<long>();
        private static Dictionary<long, Node> dictPnodeNode = new Dictionary<long, Node>();

        private static Dictionary<int, Node> dictErcotSourceSinkHash = new Dictionary<int, Node>();
        private static Dictionary<int, Dictionary<int, double>> dictFactorHash = new Dictionary<int, Dictionary<int, double>>();

        public PriceServer()
        {
            LoadDB();
        }
        private void LoadDB()
        {
            vayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            //
            cmdSelectAllErcotPriceRTCommand = new SqlCommand();
            cmdSelectAllErcotPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                    "node.nodetypekey, Node.NodeName, congestion, loss From vayu..NodeLmph (NOLOCK) Join vayu..Node on Node.NodeKey = NodeLMPH.NodeKey Where " +
                                                    "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                    "READ COMMITTED";
            cmdSelectAllErcotPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectAllErcotPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectAllErcotPriceRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectAllErcotPriceRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectERCOTAllNodesFiveMinRTCommand = new SqlCommand();
            cmdSelectERCOTAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                             "node.nodetypekey, Node.NodeName, congestion, loss From vayu..NodeLmp (NOLOCK) Join vayu..Node on Node.NodeKey = vayu..NodeLMP.NodeKey  ";
            cmdSelectERCOTAllNodesFiveMinRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectErcotAllNodesDACommand = new SqlCommand();
            cmdSelectErcotAllNodesDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                    "node.nodetypekey, Node.NodeName, congestion, loss From vayu..NodeDALmph (NOLOCK) Join Vayu..Node on Node.NodeKey = Vayu..NodeDALMPH.NodeKey Where " +
                                                    "MarketDateTime >= @Start And MarketDateTime < @End SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectErcotAllNodesDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectErcotAllNodesDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectErcotAllNodesDACommand.Connection = vayuDBConnection;
            //
            cmdSelectPriceERCOTFiveMinRTCommand = new SqlCommand();
            cmdSelectPriceERCOTFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From vayu..NodeLmp (NOLOCK) Join vayu..Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(NodeKey) from vayu..Node where " +
                                              "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPriceERCOTFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPriceERCOTFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPriceERCOTFiveMinRTCommand.Parameters.Add("@externalnodeid", SqlDbType.BigInt);

            cmdSelectPriceERCOTFiveMinRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectErcotPriceRTCommand = new SqlCommand();
            cmdSelectErcotPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From vayu..NodeLmph (NOLOCK) Join vayu..Node on Node.NodeKey = vayu..NodeLMPH.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectErcotPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectErcotPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectErcotPriceRTCommand.Parameters.Add("@NodeKey", SqlDbType.Int);

            cmdSelectErcotPriceRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectNodeNameCommand = new SqlCommand();
            cmdSelectNodeNameCommand.CommandText = "select nodename from Node  where nodekey = @nodekey";
            cmdSelectNodeNameCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            cmdSelectNodeNameCommand.Connection = vayuDBConnection;
            //
            cmdSelectErcotSourceSinkNodeCommand = new SqlCommand();
            cmdSelectErcotSourceSinkNodeCommand.CommandText = "select src.SourceNodeKey, src.SourceName, n.ExternalNodeId as SourceExternalNodeID, n.NodeTypeKey as SourceNodeTypeKey, "
                                            + "n.Zone as SourceNodeZone, sink.SinkNodeKey, sink.SinkName, n2.ExternalNodeID as SinkExternalNodeID, "
                                            + "n2.NodeTypeKey as SinkNodeTypeKey, n2.Zone as SinkNodeZone "
                                            + "from Vayu..EESPathList src (NOLOCK) "
                                            + "inner join vayu..Node n (NOLOCK) on n.NodeKey = src.SourceNodeKey "
                                            + "inner join vayu..EESPathList sink (NOLOCK) on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey "
                                            + "inner join vayu..Node n2 (NOLOCK) on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = @market and n2.MarketKey = @market and src.MarketKey = @market "
                                            + "and sink.MarketKey = @market order by n.NodeName ";
            cmdSelectErcotSourceSinkNodeCommand.Parameters.AddWithValue("@market", "market");

            cmdSelectErcotSourceSinkNodeCommand.Connection = vayuDBConnection;
            //


        }

        public Node[] GetAllFiveMinPrice(int market, DateTime startDate, DateTime endDate, bool onlyPrice, string screename = null)
        {
            Console.WriteLine("Market View Called at"+DateTime.Now);
            Console.WriteLine("Getting Data for" + startDate +"And"+ endDate);
            Node[] nodes = GetPrices(market, 0, false, startDate, endDate, true, onlyPrice, screename);
            return nodes;
        }

        public void Connect()
        {
            using (ServiceHost host = new ServiceHost(typeof(PriceServer), new Uri("net.tcp://localhost:8015")))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.OpenTimeout = new TimeSpan(6, 0, 0);
                myBinding.SendTimeout = new TimeSpan(6, 0, 0);
                myBinding.ReceiveTimeout = new TimeSpan(6, 0, 0);
                myBinding.CloseTimeout = new TimeSpan(6, 0, 0);
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.TransactionFlow = false;
                myBinding.MaxReceivedMessageSize = int.MaxValue;
                myBinding.MaxBufferPoolSize = int.MaxValue;
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.TransferMode = TransferMode.Buffered;
                myBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                var behavior = new ServiceThrottlingBehavior()
                {
                    MaxConcurrentCalls = 10000,
                    MaxConcurrentInstances = 10000,
                    MaxConcurrentSessions = 1000
                };
                host.Description.Behaviors.Add(behavior);
                host.AddServiceEndpoint(typeof(ILMPMarketView), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine("Market View Price server successfully opened port 8015.");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void FillSourceSinkHash()
        {
            
            dictErcotSourceSinkHash = new Dictionary<int, Node>();
            if (vayuDBConnection.State == ConnectionState.Closed)
                vayuDBConnection.Open();
           
            cmdSelectErcotSourceSinkNodeCommand.Parameters["@Market"].Value = 9;
            SqlDataReader reader = cmdSelectErcotSourceSinkNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                Node sourcenode = new Node();
                sourcenode.NodeId = Convert.ToInt32(reader.GetValue(0));
                sourcenode.NodeName = reader.GetString(1);
                sourcenode.PNodeId = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetDecimal(2));
                sourcenode.Market = 9;
                Node sinknode = new Node();
                sinknode.NodeId = Convert.ToInt32(reader.GetValue(5));
                sinknode.NodeName = reader.GetString(6);
                sinknode.PNodeId = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetDecimal(7));
                sinknode.Market = 9;
                try
                {
                    if (!dictErcotSourceSinkHash.ContainsKey(sourcenode.NodeId))
                    {
                        dictErcotSourceSinkHash.Add(sourcenode.NodeId, sourcenode);
                    }
                }
                catch (Exception ex)
                {
                }
                try
                {
                    if (!dictErcotSourceSinkHash.ContainsKey(sinknode.NodeId))
                    {
                        dictErcotSourceSinkHash.Add(sinknode.NodeId, sinknode);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            reader.Close();
            vayuDBConnection.Close();
        }

        public Node[] GetPrices(int market, long passNodeId, bool isDA, DateTime startDate, DateTime endDate, bool isFiveMin, bool onlyPrice, string screen_name = null)
        {
            if (!isFiveMin)
            {
                startDate = startDate.AddHours(1);
                endDate = endDate.AddHours(1);
            }
            LoadDB();

            if (vayuDBConnection.State == ConnectionState.Closed)
            {
                vayuDBConnection.Open();
            }

            List<NodeChange> nodeFactorList = new List<NodeChange>();
            Dictionary<int, Dictionary<DateTime, LMP>> nodeHash = new Dictionary<int, Dictionary<DateTime, LMP>>();
            List<Node> nodeList = new List<Node>();
            Dictionary<int, String> nodeNameHash = new Dictionary<int, string>();
            NodeChange nodeChange = new NodeChange();
            nodeChange.NodeKey = passNodeId;
            nodeChange.Factor = 1;
            nodeFactorList.Add(nodeChange);
            SetPriceList(nodeFactorList, 0, market, startDate, endDate, isDA, isFiveMin, nodeHash, nodeList, nodeNameHash, onlyPrice, screen_name);
            if (dictOldNodeHash.ContainsKey(passNodeId) && !isFiveMin)
            {
                List<NodeChange> mapTupleList = dictOldNodeHash[passNodeId];
                SetPriceList(mapTupleList, passNodeId, market, startDate, endDate, isDA, isFiveMin, nodeHash, nodeList, nodeNameHash, onlyPrice);
            }
            if (market == 9 && passNodeId == 0)
            {
                foreach (ExternalNodeMapping externalNode in lstExternalMappingList)
                {
                    int index = (nodeList.FindIndex(item => item.NodeId.Equals(externalNode.NewNodeKey)));
                    if (index > 0)
                    {
                        if (onlyPrice)
                        {
                            Node nodeItem = new Node();
                            nodeItem.NodeId = externalNode.OldNodeKey;
                            nodeItem.TimePriceList = nodeList[index].TimePriceList;
                            nodeList.Add(nodeItem);
                        }
                        else
                        {
                            Node nodeItem = new Node();
                            nodeItem.NodeId = externalNode.OldNodeKey;
                            nodeItem.LmpTimePriceList = nodeList[index].LmpTimePriceList;
                            nodeList.Add(nodeItem);
                        }
                    }
                }
            }
            if (market == 1 || market == 9)
            {
                SetAggPrice(passNodeId, nodeHash, startDate, endDate, onlyPrice, isDA, isFiveMin, nodeList, nodeNameHash, market);
            }

            vayuDBConnection.Close();
            return nodeList.ToArray<Node>();
        }
        private void SetAggPrice(long nodeid, Dictionary<int, Dictionary<DateTime, LMP>> nodeHash, DateTime startDate, DateTime endDate, bool onlyPrice, bool isDA, bool isFiveMin, List<Node> nodeList, Dictionary<int, String> nodeNameHash, int market)
        {
            List<long> aggList = new List<long>();
            if (nodeid != 0)
            {
                aggList.Add(nodeid);
            }
            else
            {
                aggList = lstAggList;
            }
            foreach (int extId in aggList)
            {
                if (!dictPnodeNode.ContainsKey(extId))
                {
                    continue;
                }
                Node aggNode = dictPnodeNode[extId];
                if (!dictFactorHash.ContainsKey(extId))
                {
                    continue;
                }
                Dictionary<int, double> busHash = dictFactorHash[extId];
                List<int> busList = busHash.Keys.ToList<int>();
                List<NodeChange> nodeFactorList = new List<NodeChange>();
                foreach (int busId in busList)
                {
                    NodeChange nodeChange = new NodeChange();
                    nodeChange.NodeKey = (long)busId;
                    nodeChange.Factor = busHash[busId];
                    if (dictOldNodeHash.ContainsKey(busId) && !isFiveMin)
                    {
                        List<NodeChange> mapList = dictOldNodeHash[busId];
                        foreach (NodeChange map in mapList)
                        {
                            NodeChange tempNodeChange = new NodeChange();
                            tempNodeChange.NodeKey = map.NodeKey;
                            tempNodeChange.Factor = map.Factor * busHash[busId];
                            tempNodeChange.UpdatedDate = map.UpdatedDate;
                            tempNodeChange.IsOldNode = map.IsOldNode;
                            nodeFactorList.Add(tempNodeChange);
                        }
                    }
                    nodeFactorList.Add(nodeChange);
                }
                Dictionary<int, Dictionary<DateTime, LMP>> tempNodeHash = new Dictionary<int, Dictionary<DateTime, LMP>>();
                SetPriceList(nodeFactorList, aggNode.NodeId, market, startDate, endDate, isDA, isFiveMin, tempNodeHash, nodeList, nodeNameHash, onlyPrice);
            }
        }

        public void SetPriceList(List<NodeChange> nodeFactorList, long origNodeId, int market, DateTime startDate, DateTime endDate, bool isDA, bool isFiveMin,
                                   Dictionary<int, Dictionary<DateTime, LMP>> nodeHash, List<Node> nodeList, Dictionary<int, String> nodeNameHash, bool onlyPrice, string screenNameval = "marketview")
        {
            SqlDataReader reader = null;
            Dictionary<DateTime, LMP> origDateHash = new Dictionary<DateTime, LMP>();
            if (nodeHash != null && nodeHash.ContainsKey((int)origNodeId))
            {
                origDateHash = nodeHash[(int)origNodeId];
            }
            foreach (NodeChange nodeFactor in nodeFactorList)
            {
                double energyprice = 0;
                if (!isDA)
                {
                    energyprice = getEnergyPrice(startDate);
                }
                reader = GetLmpReader(isDA, market, isFiveMin, nodeFactor, startDate, endDate);
                while (reader.Read())
                {
                    int nodeId = (int)origNodeId != 0 ? (int)origNodeId : Convert.ToInt32(reader.GetValue(0));
                    long pNodeId = !reader.IsDBNull(1) ? Convert.ToInt64(reader.GetValue(1)) : 0;
                    DateTime marketDate = Convert.ToDateTime(reader.GetValue(3));
                    if (origDateHash.ContainsKey(marketDate))
                    {
                        continue;
                    }
                    double price = Convert.ToDouble(reader.GetValue(2));
                    string nodeName = reader.GetValue(5).ToString();
                    double congestion = !reader.IsDBNull(6) ? Convert.ToDouble(reader.GetValue(6)) : 0;
                    double loss = !reader.IsDBNull(7) ? Convert.ToDouble(reader.GetValue(7)) : 0;
                    Dictionary<DateTime, LMP> dateHash = new Dictionary<DateTime, LMP>();
                    if (nodeHash.ContainsKey(nodeId))
                    {
                        dateHash = nodeHash[nodeId];
                        nodeHash.Remove(nodeId);
                    }
                    LMP lmp = new LMP();
                    if (!nodeFactor.IsOldNode && nodeFactor.UpdatedDate < startDate && nodeFactor.UpdatedDate.Year > 2005)
                    {
                        lmp.Price = price;// * nodeFactor.Factor;
                        lmp.Congestion = congestion;// * nodeFactor.Factor;
                        lmp.Loss = loss;// * nodeFactor.Factor;
                        if (!isDA)
                            lmp.EnergyPrice = energyprice;

                    }
                    else
                    {
                        lmp.Price = price * nodeFactor.Factor;
                        lmp.Congestion = congestion * nodeFactor.Factor;
                        lmp.Loss = loss * nodeFactor.Factor;
                        lmp.EnergyPrice = energyprice * nodeFactor.Factor;
                        if (!isDA)
                            lmp.EnergyPrice = energyprice;
                    }

                    if (dateHash.ContainsKey(marketDate))
                    {
                        if (nodeFactor.Factor < 1 && origNodeId != 0)
                        {
                            if ((lmp.Price != 0 && lmp.Price != double.NaN))
                            {
                                lmp.Price += dateHash[marketDate].Price;
                                lmp.Congestion += dateHash[marketDate].Congestion;
                                lmp.Loss += dateHash[marketDate].Loss;
                                lmp.EnergyPrice += dateHash[marketDate].EnergyPrice;
                            }
                        }
                        dateHash[marketDate] = lmp;
                    }
                    else
                    {
                        dateHash.Add(marketDate, lmp);
                    }
                    nodeHash.Add(nodeId, dateHash);
                    if (!nodeNameHash.ContainsKey(nodeId))
                    {
                        if (origNodeId != 0)
                        {
                            vayuDBConnection.Open();
                            cmdSelectNodeNameCommand.Parameters["@nodekey"].Value = origNodeId;
                            SqlDataReader reader1 = cmdSelectNodeNameCommand.ExecuteReader();
                            while (reader1.Read())
                            {
                                nodeNameHash.Add((int)origNodeId, reader1.GetString(0));
                            }
                            reader1.Close();
                            vayuDBConnection.Close();
                        }
                        else
                        {
                            nodeNameHash.Add(nodeId, nodeName);
                        }
                    }
                    if (dictPnodeHash.ContainsKey(pNodeId))
                    {
                        List<long> nodes = dictPnodeHash[pNodeId];
                        foreach (int tempNodeid in nodes)
                        {
                            if (tempNodeid != nodeId)
                            {
                                Dictionary<DateTime, LMP> dateHash1 = new Dictionary<DateTime, LMP>();
                                if (nodeHash.ContainsKey(tempNodeid))
                                {
                                    dateHash1 = nodeHash[tempNodeid];
                                    nodeHash.Remove(tempNodeid);
                                }
                                if (dateHash.ContainsKey(marketDate))
                                {
                                    dateHash1.Remove(marketDate);
                                }
                                LMP lmp1 = new LMP();
                                lmp1.Price = price;
                                lmp1.Congestion = congestion;
                                lmp1.Loss = loss;
                                lmp1.EnergyPrice = energyprice;
                                dateHash1.Add(marketDate, lmp1);
                                nodeHash.Add(tempNodeid, dateHash1);
                                if (!nodeNameHash.ContainsKey(tempNodeid))
                                {
                                    nodeNameHash.Add(tempNodeid, nodeName);
                                }
                            }
                        }
                    }
                }
                reader.Close();
            }
            List<int> nodeKeyList = nodeHash.Keys.ToList<int>();
            foreach (int nodeKey in nodeKeyList)
            {
                Node node = new Node();
                node.NodeId = nodeKey;
                node.Market = market;
                if (nodeNameHash.ContainsKey(nodeKey))
                {
                    node.NodeName = nodeNameHash[nodeKey];
                }
                else
                {
                    node.NodeName = "";
                }
                node.PNodeId = 0;
                Dictionary<DateTime, LMP> dateHash = nodeHash[nodeKey];
                List<TimePrice> timePriceList = new List<TimePrice>();
                List<LmpTimePrice> lmpTimePriceList = new List<LmpTimePrice>();
                List<DateTime> dateKeyList = dateHash.Keys.ToList<DateTime>();
                foreach (DateTime date in dateKeyList)
                {
                    LMP lmp = dateHash[date];
                    if (onlyPrice)
                    {
                        TimePrice timePrice = new TimePrice();
                        timePrice.MarketTime = date;
                        timePrice.Price = lmp.Price;
                        timePriceList.Add(timePrice);
                    }
                    else
                    {
                        LmpTimePrice timePrice = new LmpTimePrice();
                        timePrice.MarketTime = date;
                        timePrice.Lmp = lmp;
                        lmpTimePriceList.Add(timePrice);
                    }
                }
                if (onlyPrice)
                {
                    node.TimePriceList = timePriceList;
                }
                else
                {
                    node.LmpTimePriceList = lmpTimePriceList;
                }
                nodeList.Add(node);
            }
        }

        public double getEnergyPrice(DateTime timeValue)
        {
            double energyprice = 0;
            int counter = 0;
            DateTime dtval1 = timeValue.AddMinutes(0);
            DateTime dtval2 = timeValue.AddMinutes(4);

            if (vayuDBConnection.State == ConnectionState.Closed)
            {
                vayuDBConnection.Open();

            }
            SqlCommand cmd = vayuDBConnection.CreateCommand();

            cmd.CommandText = "select SystemLambda from  Vayu..RTSystemLambda where DeliveryDate between '" + dtval1 + "' and '" + dtval2 + "';";
            cmd.Connection = vayuDBConnection;
            SqlDataReader reader;

            while (counter < 7)
            {
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                        energyprice = !reader.IsDBNull(0) ? Convert.ToDouble(reader.GetValue(0)) : 0;
                    counter = 7;
                    break;
                }
                else
                {
                    counter++;
                    Thread.Sleep(15 * 1000);//15 second
                }
                reader.Close();
            }

            return energyprice;
        }

        private SqlDataReader GetLmpReader(bool isDA, int market, bool isFiveMin, NodeChange nodeFactor, DateTime startDate, DateTime endDate)
        {
            SqlDataReader reader = null;
            
                if (market == 9)
                {
                        if (isFiveMin)
                        {

                            //
                            cmdSelectERCOTAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,  DATEADD(HOUR , MarketHour , DATEADD(MINUTE , MarketMin , MarketDate)) as marketdatetime , " +
                                         "node.nodetypekey, node.Nodename, 0, 0 From Vayu..NodeLmpMin (NOLOCK) Join Vayu..Node on Node.NodeKey = NodeLMPMin.NodeKey Where " +
                                           "MarketDate >= '" + startDate.Date.ToString() + "' " +
                                           " SET TRANSACTION ISOLATION LEVEL READ COMMITTED";

                            reader = cmdSelectERCOTAllNodesFiveMinRTCommand.ExecuteReader();
                        }
                    
                }
            
            return reader;
        }

    }
    class ExternalNodeMapping
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the external identifier.
        /// </summary>
        /// <value>
        /// The external identifier.
        /// </value>
        public long ExternalID { get; set; }
        /// <summary>
        /// Gets or sets the old node key.
        /// </summary>
        /// <value>
        /// The old node key.
        /// </value>
        public int OldNodeKey { get; set; }
        /// <summary>
        /// Gets or sets the new node key.
        /// </summary>
        /// <value>
        /// The new node key.
        /// </value>
        public int NewNodeKey { get; set; }
        #endregion
    }
}
