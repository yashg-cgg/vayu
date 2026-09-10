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

namespace Vayu.NodePriceService
{
    [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    class LMPServer : ILMP
    {
        #region Private Members
        
        private SqlConnection vayuDBConnection;
         
        private SqlCommand cmdSelectNyisoAllNodesRTCommand;
         
        private SqlCommand cmdSelectPJMAllNodesRTCommand;
         
        private SqlCommand cmdSelectNyisoAllNodesFiveMinRTCommand;
         
        private SqlCommand cmdSelectPJMAllNodesFiveMinRTCommand;
         
        private SqlCommand cmdSelectNyisoPriceRTCommand;
         
        private SqlCommand cmdSelectPJMPriceRTCommand;
         
        private SqlCommand cmdSelectMISOAllNodesRTCommand;
         
        private SqlCommand cmdSelectMISOPriceRTCommand;
         
        private SqlCommand cmdSelectNyisoAllNodesDACommand;
         
        private SqlCommand cmdSelectNyisoPriceDACommand;
         
        private SqlCommand cmdSelectMappingCommand;
         
        private SqlCommand cmdSelectPnodeNodeCommand;
         
        private SqlCommand cmdSelectPricePJMRTCommand;
         
        private SqlCommand cmdSelectPricePJMFiveMinRTCommand;
         
        private SqlCommand cmdSelectPriceMISOFiveMinRTCommand;
         
        private SqlCommand cmdSelectPriceNYISOFiveMinRTCommand;
         
        private SqlCommand cmdSelectPriceSPPFiveMinRTCommand;
         
        private SqlCommand cmdSelectPriceCAISOFiveMinRTCommand;
         
        private SqlCommand cmdSelectPriceERCOTFiveMinRTCommand;
         
        private SqlCommand cmdSelectPricePJMDACommand;
         
        private SqlCommand cmdSelectPriceRTCommand;
         
        private SqlCommand cmdSelectPriceDACommand;
         
        private SqlCommand cmdSelectErcotPriceRTCommand;
          
        private SqlCommand cmdSelectSPPPriceRTCommand;
         
        private SqlCommand cmdSelectSPPAllNodesRTCommand;
         
        private SqlCommand cmdSelectErcotPriceDACommand;
         
        private SqlCommand cmdSelectSPPPriceDACommand;
         
        private SqlCommand cmdSelectCaisoPriceRTCommand;
         
        private SqlCommand cmdSelectCaisoPriceDACommand;
         
        private SqlCommand cmdSelectFactorCommand;
         
        private SqlCommand cmdSelectAllPriceRTCommand;
         
        private SqlCommand cmdSelectAllPriceMISORTCommand;
         
        private SqlCommand cmdSelectAllFiveMinPriceRTCommand;
         
        private SqlCommand cmdSelectAllFiveMinPriceMisoRTCommand;
         
        private SqlCommand cmdSelectAllErcotPriceRTCommand;
         
        private SqlCommand cmdSelectErcotAllNodesDACommand;
         
        private SqlCommand cmdSelectSPPAllNodesDACommand;
         
        private SqlCommand cmdSelectAllPriceDACommand;
         
        private SqlCommand cmdSelectCaisoAllNodesRTCommand;
         
        private SqlCommand cmdSelectCaisoAllNodesDACommand;
         
        private SqlCommand cmdSelectSourceSinkNodeCommand;
         
        private SqlCommand cmdSelectExternalNodeCommand;
         
        private SqlCommand cmdSelectAggregateNodeCommand;
         
        private SqlCommand cmdSelectAggregrateNodeWithPricingCommand;
         
        private SqlCommand cmdSelectNodeFromPNodeCommand;
         
        private SqlCommand cmdSelectPNodeCommand;
         
        private SqlCommand cmdSelectAllOldMisoNodeCommand;
         
        private SqlCommand cmdSelectAllOldCaisoNodeCommand;
         
        private SqlCommand cmdSelectNewMisoNodeCommand;
         
        private SqlCommand cmdSelectAllNewMisoNodeCommand;
         
        private SqlCommand cmdSelectOldMisoNodeCommand;
         
        private SqlCommand cmdSelectMISOAllNodesDACommand;
         
        private SqlCommand cmdSelectMISOPriceDACommand;
         
        private SqlCommand cmdSelectCAISOAllNodesFiveMinRTCommand;
         
        private SqlCommand cmdSelectSPPAllNodesFiveMinRTCommand;
         
        private SqlCommand cmdSelectERCOTAllNodesFiveMinRTCommand;
         
        private SqlCommand cmdSelectMISOAllNodesFiveMinRTCommand;
         
        private SqlCommand cmdSelectNodeNameCommand;

        private SqlCommand cmdSelectErcotSourceSinkNodeCommand;

        
        private static Dictionary<long, Node> dictPnodeNode = new Dictionary<long, Node>();
           
        private static Dictionary<int, Dictionary<int, double>> dictFactorHash = new Dictionary<int, Dictionary<int, double>>();
         
        private static Dictionary<int, Node> dictPJMSourceSinkHash = new Dictionary<int, Node>();
          
        private static Dictionary<int, Node> dictErcotSourceSinkHash = new Dictionary<int, Node>();
         
        private static Dictionary<long, List<long>> dictPnodeHash = new Dictionary<long, List<long>>();
         
        private static Dictionary<long, List<NodeChange>> dictOldNodeHash = new Dictionary<long, List<NodeChange>>();
          
        private static List<ExternalNodeMapping> lstExternalMappingList = new List<ExternalNodeMapping>();
         
        private static List<long> lstAggList = new List<long>();
        #endregion
#if Local
        string TableName = "TestNode";
#else
     
        string TableName = "Node";
#endif

        #region Public Methods
         
        public LMPServer()
        {
            LoadDB();
        }
         
        public void SetRemapNode()
        {
            vayuDBConnection.Open();
            for (int i = 2; i < 4; i++)
            {
                List<long> nodeList = new List<long>();
                SqlCommand command = cmdSelectAllNewMisoNodeCommand;
                if (i == 1)
                {
                    command = cmdSelectAllOldMisoNodeCommand;
                }
                else if (i == 2)
                {
                    command = cmdSelectAllNewMisoNodeCommand;
                    command.CommandText = cmdSelectAllNewMisoNodeCommand.CommandText.Replace("miso.", "pjm.");
                }
                else if (i == 3)
                {
                    command = cmdSelectAllOldMisoNodeCommand;
                    command.CommandText = cmdSelectAllOldMisoNodeCommand.CommandText.Replace("miso.", "pjm.");
                }
                else if (i == 4)
                {
                    command = cmdSelectAllNewMisoNodeCommand;
                    command.CommandText = cmdSelectAllNewMisoNodeCommand.CommandText.Replace("pjm.", "caiso.");
                }
                else if (i == 5)
                {
                    command = cmdSelectAllOldMisoNodeCommand;
                    command.CommandText = cmdSelectAllOldMisoNodeCommand.CommandText.Replace("pjm.", "caiso.");
                }
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    nodeList.Add(Convert.ToInt64(reader.GetValue(0)));
                }
                reader.Close();
                command = cmdSelectOldMisoNodeCommand;
                if (i == 1)
                {
                    command = cmdSelectNewMisoNodeCommand;
                }
                else if (i == 2)
                {
                    command = cmdSelectOldMisoNodeCommand;
                    command.CommandText = cmdSelectOldMisoNodeCommand.CommandText.Replace("miso.", "pjm.");
                }
                else if (i == 3)
                {
                    command = cmdSelectNewMisoNodeCommand;
                    command.CommandText = cmdSelectNewMisoNodeCommand.CommandText.Replace("miso.", "pjm.");
                }
                else if (i == 4)
                {
                    command = cmdSelectOldMisoNodeCommand;
                    command.CommandText = cmdSelectOldMisoNodeCommand.CommandText.Replace("pjm.", "caiso.");
                }
                else if (i == 5)
                {
                    command = cmdSelectNewMisoNodeCommand;
                    command.CommandText = cmdSelectNewMisoNodeCommand.CommandText.Replace("pjm.", "caiso.");
                }
                foreach (long nodeKey in nodeList)
                {
                    long oldNodeKey = nodeKey;
                    while (oldNodeKey != 0)
                    {
                        List<NodeChange> nodeChangeList = new List<NodeChange>();
                        List<NodeChange> TempnodeChangeList = new List<NodeChange>();
                        if (dictOldNodeHash.ContainsKey(nodeKey))
                        {
                            nodeChangeList = dictOldNodeHash[nodeKey];
                            dictOldNodeHash.Remove(nodeKey);
                        }
                        command.Parameters["@oldnodekey"].Value = oldNodeKey;
                        reader = command.ExecuteReader();
                        oldNodeKey = 0;
                        while (reader.Read())
                        {
                            oldNodeKey = Convert.ToInt64(reader.GetValue(0));
                            double factor = i == 0 || i == 2 || i == 4 ? 1 : (double)reader.GetDecimal(1);
                            DateTime updatedDate = reader.GetDateTime(2);
                            NodeChange nodeChange = new NodeChange();
                            nodeChange.NodeKey = oldNodeKey;
                            nodeChange.Factor = factor;
                            nodeChange.IsOldNode = i == 0 || i == 2 || i == 4;
                            nodeChange.UpdatedDate = updatedDate;

                            if (nodeChangeList.Any(x => x.NodeKey == oldNodeKey && x.UpdatedDate == updatedDate &&
                                x.IsOldNode == nodeChange.IsOldNode))
                            {
                                oldNodeKey = 0;
                                break;
                            }

                            nodeChangeList.Add(nodeChange);
                        }
                        reader.Close();
                        dictOldNodeHash.Add(nodeKey, nodeChangeList);
                    }
                }
            }
            vayuDBConnection.Close();
            NodeComparer comp = new NodeComparer();
            Dictionary<long, List<NodeChange>> tempdictOldNodeHash = new Dictionary<long, List<NodeChange>>();

            foreach (var item in dictOldNodeHash)
            {
                List<NodeChange> nodeChangeList = item.Value.Distinct(comp).ToList();
                tempdictOldNodeHash.Add(item.Key, nodeChangeList);
            }

            dictOldNodeHash = tempdictOldNodeHash;
        }
         
        public Node[] GetAllFiveMinPrice(int market, DateTime startDate, DateTime endDate, bool onlyPrice, string screename = null)
        {
            Node[] nodes = GetPrices(market, 0, false, startDate, endDate, true, onlyPrice,screename);
            return nodes;
        }
         
        public Node[] GetAllUptoFiveMinPrice(int market, bool isDA, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date.AddHours(1);
            endDate = endDate.Date;
            return GetAllUptoPriceDateTime(market, isDA, startDate, endDate, false, true);
        }
         
        public Node[] GetPrice(Node[] nodes, bool isDA, bool onlyPrice, bool isFiveMin)
        {
            foreach (Node node in nodes)
            {
                Dictionary<DateTime, double> priceHash = new Dictionary<DateTime, double>();
                Dictionary<DateTime, LMP> lmpHash = new Dictionary<DateTime, LMP>();
                DateTime start = onlyPrice ? node.TimePriceList.OrderBy(kvp => kvp.MarketTime).First().MarketTime.AddHours(-1) :
                                            node.LmpTimePriceList.OrderBy(kvp => kvp.MarketTime).First().MarketTime.AddHours(-1);
                DateTime end = onlyPrice ? node.TimePriceList.OrderBy(kvp => kvp.MarketTime).Last().MarketTime :
                                            node.LmpTimePriceList.OrderBy(kvp => kvp.MarketTime).Last().MarketTime;
                Node[] returnNodes = node.Market == 1 ? GetPrices(node.Market, node.PNodeId, isDA, start, end, isFiveMin, onlyPrice) :
                                                            GetPrices(node.Market, node.NodeId, isDA, start, end, isFiveMin, onlyPrice);
                if (returnNodes.Length > 0)
                {
                    if (onlyPrice)
                    {
                        foreach (TimePrice item in returnNodes[0].TimePriceList)
                        {
                            if (!priceHash.ContainsKey(item.MarketTime))
                            {
                                priceHash.Add(item.MarketTime, item.Price);
                            }
                        }
                        foreach (TimePrice timePrice in node.TimePriceList)
                        {
                            if (priceHash.ContainsKey(timePrice.MarketTime))
                            {
                                timePrice.Price = priceHash[timePrice.MarketTime];
                            }
                        }
                    }
                    else
                    {
                        foreach (LmpTimePrice item in returnNodes[0].LmpTimePriceList)
                        {
                            if (!lmpHash.ContainsKey(item.MarketTime))
                            {
                                lmpHash.Add(item.MarketTime, item.Lmp);
                            }
                        }
                        foreach (LmpTimePrice timePrice in node.LmpTimePriceList)
                        {
                            if (lmpHash.ContainsKey(timePrice.MarketTime))
                            {
                                timePrice.Lmp = lmpHash[timePrice.MarketTime];
                            }
                        }
                    }
                }
            }
            return nodes;
        }
         
        public void SetAggregate()
        {
            List<long> aggList = new List<long>();
            LoadDB();
            vayuDBConnection.Open();
            SqlDataReader reader = cmdSelectAggregateNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int externalNodeId = (int)reader.GetDecimal(0);
                lstAggList.Add(externalNodeId);
            }
            reader.Close();
            reader = cmdSelectAggregrateNodeWithPricingCommand.ExecuteReader();
            while (reader.Read())
            {
                int externalNodeId = (int)reader.GetDecimal(0);
                lstAggList.Remove(externalNodeId);
            }
            reader.Close();
            //foreach (int extNodeId in lstAggList)
            //{
            //    cmdSelectNodeFromPNodeCommand.Parameters["@externalnodeid"].Value = extNodeId;
            //    reader = cmdSelectNodeFromPNodeCommand.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        aggList.Add(extNodeId);
            //        break;
            //    }
            //    reader.Close();
            //}
            //lstAggList = aggList;
            reader = cmdSelectFactorCommand.ExecuteReader();
            while (reader.Read())
            {
                int aggregateId = (int)reader.GetDecimal(0);
                if (!lstAggList.Contains(aggregateId))
                {
                    continue;
                }
                int busId = (int)reader.GetDecimal(1);
                double factor = (double)reader.GetDecimal(2);
                Dictionary<int, double> busHash = new Dictionary<int, double>();
                if (dictFactorHash.ContainsKey(aggregateId))
                {
                    busHash = dictFactorHash[aggregateId];
                    dictFactorHash.Remove(aggregateId);
                }
                if (!busHash.ContainsKey(busId))
                {
                    busHash.Add(busId, factor);
                }
                dictFactorHash.Add(aggregateId, busHash);
            }
            reader.Close();
            vayuDBConnection.Close();
        }
         
        public void MapExtNodes()
        {
            vayuDBConnection.Open();
            cmdSelectPnodeNodeCommand.Parameters["@MarketKey"].Value = 1;
            SqlDataReader reader = cmdSelectPnodeNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                long pnode = Convert.ToInt64(reader.GetValue(0));
                int nodekey = Convert.ToInt32(reader.GetValue(1));
                string nodeName = reader.GetString(2);
                Node node = new Node();
                node.Market = 1;
                node.NodeId = nodekey;
                node.NodeName = nodeName;
                node.PNodeId = pnode;
                if (!dictPnodeNode.ContainsKey(pnode))
                {
                    dictPnodeNode.Add(pnode, node);
                }
                else
                {
                    if (dictPnodeNode[pnode].NodeId > nodekey)
                    {
                        dictPnodeNode[pnode] = node;
                    }
                }
            }
            reader.Close();
            vayuDBConnection.Close();
        }
         
        public void SetExternalNodes()
        {
            if (lstExternalMappingList.Count == 0)
            {
                lstExternalMappingList = new List<ExternalNodeMapping>();
                vayuDBConnection.Open();
                SqlDataReader reader = cmdSelectExternalNodeCommand.ExecuteReader();
                while (reader.Read())
                {
                    long externalNodeKey = Convert.ToInt64(reader.GetValue(0));
                    int nodekey = Convert.ToInt32(reader.GetValue(1));
                    if (lstExternalMappingList.Exists(t => t.ExternalID.Equals(externalNodeKey)))
                    {
                        int index = lstExternalMappingList.FindIndex(item => item.ExternalID.Equals(externalNodeKey));
                        lstExternalMappingList[index].NewNodeKey = nodekey;
                    }
                    else
                    {
                        ExternalNodeMapping externalNodeMapping = new ExternalNodeMapping();
                        externalNodeMapping.ExternalID = externalNodeKey;
                        externalNodeMapping.OldNodeKey = nodekey;
                        lstExternalMappingList.Add(externalNodeMapping);
                    }
                }
                vayuDBConnection.Close();
            }
        }
         
        public void SetExternalNodeHash()
        {
            if (dictPnodeHash.Count == 0)
            {
                vayuDBConnection.Open();
                SqlDataReader reader = cmdSelectPNodeCommand.ExecuteReader();
                while (reader.Read())
                {
                    int nodeKey = Convert.ToInt32(reader.GetValue(0));
                    long pNodeKey = Convert.ToInt64(reader.GetValue(1));
                    List<long> nodeList = new List<long>();
                    if (dictPnodeHash.ContainsKey(pNodeKey))
                    {
                        nodeList = dictPnodeHash[pNodeKey];
                        dictPnodeHash.Remove(pNodeKey);
                    }
                    nodeList.Add(nodeKey);
                    dictPnodeHash.Add(pNodeKey, nodeList);
                }
                reader.Close();
                vayuDBConnection.Close();
            }
        }
         
        public Node[] GetAllPrice(int market, bool isDA, DateTime startDate, DateTime endDate, bool onlyPrice)
        {
            return GetPrices(market, 0, isDA, startDate, endDate, false, onlyPrice);
        }
         
        public Node[] GetAllUptoPriceDateTime(int market, bool isDA, DateTime startDateTime, DateTime endDateTime, bool isFiveMin, bool onlyPrice)
        {
            List<int> nodeKeyList = new List<int>();
            if(vayuDBConnection.State == ConnectionState.Open)
            {
                vayuDBConnection.Close();
            }
            vayuDBConnection.Open();
            Dictionary<int, Dictionary<DateTime, double>> nodeHash = new Dictionary<int, Dictionary<DateTime, double>>();
            Dictionary<int, Node> sourceSinkHash = new Dictionary<int, Node>();
            List<Node> nodeList = new List<Node>();
            if (market == 9)
            {
                sourceSinkHash = dictErcotSourceSinkHash;
            }
            foreach (KeyValuePair<int, Node> item in sourceSinkHash)
            {
                item.Value.TimePriceList = new List<TimePrice>();
                item.Value.LmpTimePriceList = new List<LmpTimePrice>();
                for (DateTime datehour = startDateTime; datehour <= endDateTime; datehour = datehour.AddHours(1))
                {
                    if (onlyPrice)
                    {
                        TimePrice darttp = new TimePrice();
                        darttp.MarketTime = datehour;
                        item.Value.TimePriceList.Add(darttp);
                    }
                    else
                    {
                        LmpTimePrice darttp2 = new LmpTimePrice();
                        darttp2.MarketTime = datehour;
                        item.Value.LmpTimePriceList.Add(darttp2);
                    }
                }
            }
            if (isDA)
            {
                Node[] daNodes = GetPrice(sourceSinkHash.Values.ToArray(), isDA, onlyPrice, isFiveMin);
                return daNodes;
            }
            Node[] rtNodes = GetPrice(sourceSinkHash.Values.ToArray(), isDA, onlyPrice, isFiveMin);
            return rtNodes;
        }
         
        public Node[] GetAllUptoPrice(int market, bool isDA, DateTime startDate, DateTime endDate, bool onlyPrice)
        {
            startDate = startDate.Date.AddHours(1);
            endDate = endDate.Date;
            return GetAllUptoPriceDateTime(market, isDA, startDate, endDate, false, onlyPrice);
        }
         
        public void FillSourceSinkHash()
        {
            //LoadDB();
            if (dictPJMSourceSinkHash.Count > 0)
            {
                return;
            }
            dictPJMSourceSinkHash = new Dictionary<int, Node>();
            dictErcotSourceSinkHash = new Dictionary<int, Node>();
            //vayuDBConnection.Open();
            if (vayuDBConnection.State == ConnectionState.Closed)
                vayuDBConnection.Open();
            cmdSelectSourceSinkNodeCommand.Parameters["@Market"].Value = 1;
            SqlDataReader reader = cmdSelectSourceSinkNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                Node sourcenode = new Node();
                sourcenode.NodeId = Convert.ToInt32(reader.GetValue(0));
                sourcenode.NodeName = reader.GetString(1);
                sourcenode.PNodeId = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetDecimal(2));
                sourcenode.Market = 1;
                Node sinknode = new Node();
                sinknode.NodeId = Convert.ToInt32(reader.GetValue(5));
                sinknode.NodeName = reader.GetString(6);
                sinknode.PNodeId = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetDecimal(7));
                sinknode.Market = 1;
                try
                {
                    if (!dictPJMSourceSinkHash.ContainsKey(sourcenode.NodeId))
                    {
                        dictPJMSourceSinkHash.Add(sourcenode.NodeId, sourcenode);
                    }
                }
                catch (Exception ex)
                {
                }
                try
                {
                    if (!dictPJMSourceSinkHash.ContainsKey(sinknode.NodeId))
                    {
                        dictPJMSourceSinkHash.Add(sinknode.NodeId, sinknode);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            reader.Close();
            reader = null;
            vayuDBConnection.Close();
            
            vayuDBConnection.Open();
            cmdSelectErcotSourceSinkNodeCommand.Parameters["@Market"].Value = 9;
            reader = cmdSelectErcotSourceSinkNodeCommand.ExecuteReader();
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
         
        public void Connect()
        {
            using (ServiceHost host = new ServiceHost(typeof(LMPServer), new Uri("net.tcp://localhost:8000")))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.OpenTimeout = new TimeSpan(0, 30, 0);
                myBinding.SendTimeout = new TimeSpan(0, 12, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
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
                host.AddServiceEndpoint(typeof(ILMP), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine("LMP server successfully opened port 8000.");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
         
        public void ReplaceInvaliedBusNodes()
        {
            List<long> unpricedBusList = new List<long>();
            Dictionary<int, int> aggsForUnpricedBusHash = new Dictionary<int, int>();
           
            if (vayuDBConnection.State == ConnectionState.Closed)
                vayuDBConnection.Open();
            SqlCommand cmd = vayuDBConnection.CreateCommand();
            cmd.CommandText = "select distinct buspnodeid from PJMAggregateFactors where BusPnodeId in "
                             + "(select distinct ExternalNodeID from " + TableName + " where NodeTypeKey = 5 and NodeKey not in"
                             + "(select distinct NodeKey from PJM.NodeDALMPH) ) and Factor < 1";
            cmd.Connection = vayuDBConnection;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                long busNodeId = Convert.ToInt64(rdr.GetValue(0));
                unpricedBusList.Add(busNodeId);
            }
            rdr.Close();
            SqlCommand cmd1 = vayuDBConnection.CreateCommand();
            cmd1.CommandText = "select distinct aggregatepnodeid , BusPnodeId from PJMAggregateFactors where BusPnodeId in ("
                                + "select distinct buspnodeid from PJMAggregateFactors where BusPnodeId in"
                                + "(select distinct ExternalNodeID from " + TableName + " where NodeTypeKey = 5 and NodeKey not in (select distinct NodeKey from PJM.NodeDALMPH) )and Factor < 1 )"
                                + "and AggregatePnodeId in (select  distinct ExternalNodeID from " + TableName + " where NodeKey in (select distinct NodeKey from PJM.NodeDALMPH))"
                                + "and Factor = 1";
            cmd1.Connection = vayuDBConnection;
            SqlDataReader rdr1 = cmd1.ExecuteReader();
            while (rdr1.Read())
            {
                int aggPnodeId = Convert.ToInt32(rdr1.GetValue(0));
                int busPnodeId = Convert.ToInt32(rdr1.GetValue(1));
                aggsForUnpricedBusHash.Add(busPnodeId, aggPnodeId);
            }
            rdr1.Close();
            vayuDBConnection.Close();
            Dictionary<int, Dictionary<int, double>> tempdictFactorHash = new Dictionary<int, Dictionary<int, double>>(dictFactorHash);
            foreach (var item in tempdictFactorHash)
            {
                int aggNodeId = item.Key;
                Dictionary<int, double> busHash = item.Value;
                Dictionary<int, double> tempbusHash = new Dictionary<int, double>(busHash);
                foreach (var item1 in tempbusHash)
                {
                    int busNodekey = item1.Key;
                    double factor = item1.Value;
                    if (aggsForUnpricedBusHash.ContainsKey(busNodekey))
                    {
                        busHash.Remove(busNodekey);
                        int tempAggPnode = aggsForUnpricedBusHash[busNodekey];
                        busHash.Add(tempAggPnode, factor);
                    }
                }
                dictFactorHash[aggNodeId] = busHash;
            }
            foreach (var item in aggsForUnpricedBusHash)
            {
                int aggKey = item.Key;
                int busKey = item.Value;
                if (!dictFactorHash.ContainsKey(busKey))
                {
                    Dictionary<int, double> tempBusHash = new Dictionary<int, double>();
                    tempBusHash.Add(busKey, 1);
                    dictFactorHash.Add(aggKey, tempBusHash);
                }
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
                /*if (nodeFactor.UpdatedDate != DateTime.MinValue && ((nodeFactor.UpdatedDate < startDate && nodeFactor.IsOldNode) || (nodeFactor.UpdatedDate > endDate && !nodeFactor.IsOldNode)))
                {
                    continue;
                }
                if (nodeFactor.UpdatedDate != DateTime.MinValue && nodeFactor.UpdatedDate > startDate && nodeFactor.IsOldNode && nodeFactor.UpdatedDate < endDate)
                {
                    endDate = nodeFactor.UpdatedDate;
                }
                if (nodeFactor.UpdatedDate != DateTime.MinValue && nodeFactor.UpdatedDate < endDate && !nodeFactor.IsOldNode && nodeFactor.UpdatedDate > startDate)
                {
                    startDate = nodeFactor.UpdatedDate;
                }*/
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

        public Node[] GetPrices(int market, long passNodeId, bool isDA, DateTime startDate, DateTime endDate, bool isFiveMin, bool onlyPrice ,string screen_name=null)
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
            if (market == 1 && passNodeId == 0)
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
        #endregion

        #region Private Methods
       
        private void LoadDB()
        {
            //vayuDBConnection = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            vayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            cmdSelectNodeNameCommand = new SqlCommand();
            cmdSelectNodeNameCommand.CommandText = "select nodename from " + TableName + "  where nodekey = @nodekey";
            cmdSelectNodeNameCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            cmdSelectNodeNameCommand.Connection = vayuDBConnection;
           
            cmdSelectSourceSinkNodeCommand = new SqlCommand();
            cmdSelectSourceSinkNodeCommand.CommandText = "select src.SourceNodeKey, src.SourceName, n.ExternalNodeId as SourceExternalNodeID, n.NodeTypeKey as SourceNodeTypeKey, "
                                            + "n.Zone as SourceNodeZone, sink.SinkNodeKey, sink.SinkName, n2.ExternalNodeID as SinkExternalNodeID, "
                                            + "n2.NodeTypeKey as SinkNodeTypeKey, n2.Zone as SinkNodeZone "
                                            + "from EESPathList src (NOLOCK) "
                                            + "inner join Node n (NOLOCK) on n.NodeKey = src.SourceNodeKey "
                                            + "inner join EESPathList sink (NOLOCK) on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey "
                                            + "inner join Node n2 (NOLOCK) on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = @market and n2.MarketKey = @market and src.MarketKey = @market "
                                            + "and sink.MarketKey = @market order by n.NodeName ";
            cmdSelectSourceSinkNodeCommand.Parameters.AddWithValue("@market", "market");
            cmdSelectSourceSinkNodeCommand.Connection = vayuDBConnection;
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

            ////
            cmdSelectPnodeNodeCommand = new SqlCommand();
            cmdSelectPnodeNodeCommand.CommandText = "select externalnodeid, nodekey, nodename from " + TableName + " where MarketKey = @MarketKey";
            cmdSelectPnodeNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            cmdSelectPnodeNodeCommand.Connection = vayuDBConnection;

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
            cmdSelectPricePJMRTCommand = new SqlCommand();
            cmdSelectPricePJMRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From pjm.NodeLmph (NOLOCK) Join Node on Node.NodeKey = NodeLMPH.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(NodeKey) from " + TableName + " where " +
                                              "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPricePJMRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPricePJMRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPricePJMRTCommand.Parameters.Add("@externalnodeid", SqlDbType.Int);
            cmdSelectPricePJMRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectPricePJMFiveMinRTCommand = new SqlCommand();
            cmdSelectPricePJMFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From pjm.NodeLmp (NOLOCK) Join Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(NodeKey) from " + TableName + " where " +
                                              "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPricePJMFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPricePJMFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPricePJMFiveMinRTCommand.Parameters.Add("@externalnodeid", SqlDbType.Int);
            cmdSelectPricePJMFiveMinRTCommand.Connection = vayuDBConnection;


            //
            cmdSelectPriceMISOFiveMinRTCommand = new SqlCommand();
            cmdSelectPriceMISOFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From miso.NodeLmp (NOLOCK) Join Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(NodeKey) from " + TableName + " where " +
                                              "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPriceMISOFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPriceMISOFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPriceMISOFiveMinRTCommand.Parameters.Add("@externalnodeid", SqlDbType.Int);
            cmdSelectPriceMISOFiveMinRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectPriceNYISOFiveMinRTCommand = new SqlCommand();
            cmdSelectPriceNYISOFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From nyiso.NodeLmp (NOLOCK) Join Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(NodeKey) from " + TableName + "where " +
                                              "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPriceNYISOFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPriceNYISOFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPriceNYISOFiveMinRTCommand.Parameters.Add("@externalnodeid", SqlDbType.Int);
            cmdSelectPriceNYISOFiveMinRTCommand.Connection = vayuDBConnection;
            //


            //
            cmdSelectPriceSPPFiveMinRTCommand = new SqlCommand();
            cmdSelectPriceSPPFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From SPP.NodeLmp (NOLOCK) Join Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(NodeKey) from " + TableName + "where " +
                                              "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPriceSPPFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPriceSPPFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPriceSPPFiveMinRTCommand.Parameters.Add("@externalnodeid", SqlDbType.Int);
            cmdSelectPriceSPPFiveMinRTCommand.Connection = vayuDBConnection;

            //
            cmdSelectPriceCAISOFiveMinRTCommand = new SqlCommand();
            cmdSelectPriceCAISOFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From caiso.NodeLmp (NOLOCK) Join Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(NodeKey) from " + TableName + " where " +
                                              "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPriceCAISOFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPriceCAISOFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPriceCAISOFiveMinRTCommand.Parameters.Add("@externalnodeid", SqlDbType.Int);
            cmdSelectPriceCAISOFiveMinRTCommand.Connection = vayuDBConnection;

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
            cmdSelectPriceRTCommand = new SqlCommand();
            cmdSelectPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                            "node.nodetypekey, node.Nodename, congestion, loss From miso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = NodeLMPH.NodeKey Where " +
                                              "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @externalnodeid SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPriceRTCommand.Parameters.Add("@externalnodeid", SqlDbType.BigInt);
            cmdSelectPriceRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectAllFiveMinPriceRTCommand = new SqlCommand();
            cmdSelectAllFiveMinPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                "node.nodetypekey, node.Nodename, congestion, loss From pjm.NodeLmp (NOLOCK) Join Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey order by marketdatetime SET TRANSACTION ISOLATION LEVEL " +
                                                "READ COMMITTED";
            cmdSelectAllFiveMinPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectAllFiveMinPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectAllFiveMinPriceRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectAllFiveMinPriceRTCommand.Connection = vayuDBConnection;

            //
            cmdSelectAllFiveMinPriceMisoRTCommand = new SqlCommand();
            cmdSelectAllFiveMinPriceMisoRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                "node.nodetypekey, node.Nodename, congestion, loss From miso.NodeLmp (NOLOCK) Join Node on Node.NodeKey = NodeLMP.NodeKey Where " +
                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey order by marketdatetime SET TRANSACTION ISOLATION LEVEL " +
                                                "READ COMMITTED";
            cmdSelectAllFiveMinPriceMisoRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectAllFiveMinPriceMisoRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectAllFiveMinPriceMisoRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectAllFiveMinPriceMisoRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectAllPriceRTCommand = new SqlCommand();
            cmdSelectAllPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                "node.nodetypekey, node.Nodename, congestion, loss From pjm.NodeLmph (NOLOCK) Join Node on Node.NodeKey = NodeLMPH.NodeKey Where " +
                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                "READ COMMITTED";
            cmdSelectAllPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectAllPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectAllPriceRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectAllPriceRTCommand.Connection = vayuDBConnection;

            //
            //
            cmdSelectAllPriceMISORTCommand = new SqlCommand();
            cmdSelectAllPriceMISORTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                "node.nodetypekey, node.Nodename, congestion, loss From miso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = NodeLMPH.NodeKey Where " +
                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                "READ COMMITTED";
            cmdSelectAllPriceMISORTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectAllPriceMISORTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectAllPriceMISORTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectAllPriceMISORTCommand.Connection = vayuDBConnection;
            //
            cmdSelectAllErcotPriceRTCommand = new SqlCommand();
            cmdSelectAllErcotPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                    "node.nodetypekey, Node.NodeName, congestion, loss From vayu..NodeLmph (NOLOCK) Join vayu..Node on Node.NodeKey = NodeLMPH.NodeKey Where " +
                                                    "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                    "READ COMMITTED";
            cmdSelectAllErcotPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectAllErcotPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectAllErcotPriceRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            //cmdSelectAllErcotPriceRTCommand.Connection = SigmaDBConnection;
            cmdSelectAllErcotPriceRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectSPPPriceRTCommand = new SqlCommand();
            cmdSelectSPPPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                   "node.nodetypekey, node.Nodename, congestion, loss From SPP.NodeLmph (NOLOCK) Join Node on Node.NodeKey = SPP.NodeLMPh.NodeKey Where " +
                                                   "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectSPPPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectSPPPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectSPPPriceRTCommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            cmdSelectSPPPriceRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectNyisoPriceRTCommand = new SqlCommand();
            cmdSelectNyisoPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                   "node.nodetypekey, node.Nodename, congestion, loss From Nyiso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = Nyiso.NodeLMPh.NodeKey Where " +
                                                   "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectNyisoPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectNyisoPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);

            cmdSelectNyisoPriceRTCommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            cmdSelectNyisoPriceRTCommand.Connection = vayuDBConnection;

            //
            //
            cmdSelectPJMPriceRTCommand = new SqlCommand();
            cmdSelectPJMPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                   "node.nodetypekey, node.Nodename, congestion, loss From pjm.NodeLmph (NOLOCK) Join Node on Node.NodeKey = pjm.NodeLMPh.NodeKey Where " +
                                                   "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(nodekey) from " + TableName + " where externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPJMPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPJMPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPJMPriceRTCommand.Parameters.Add("@externalnodeid", SqlDbType.BigInt);
            cmdSelectPJMPriceRTCommand.Connection = vayuDBConnection;

            //
            cmdSelectMISOPriceRTCommand = new SqlCommand();
            cmdSelectMISOPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                   "node.nodetypekey, node.Nodename, congestion, loss From miso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = miso.NodeLMPh.NodeKey Where " +
                                                   "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectMISOPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectMISOPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectMISOPriceRTCommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            cmdSelectMISOPriceRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectSPPAllNodesRTCommand = new SqlCommand();
            cmdSelectSPPAllNodesRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                                "node.nodetypekey, Node.NodeName, congestion, loss From SPP.NodeLmph (NOLOCK) Join Node on Node.NodeKey = spp.NodeLMPh.NodeKey Where " +
                                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                                "READ COMMITTED";
            cmdSelectSPPAllNodesRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectSPPAllNodesRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectSPPAllNodesRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectSPPAllNodesRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectNyisoAllNodesRTCommand = new SqlCommand();
            cmdSelectNyisoAllNodesRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                               "node.nodetypekey, Node.NodeName, congestion, loss From Nyiso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = Nyiso.NodeLMPh.NodeKey Where " +
                                                               "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                               "READ COMMITTED";
            cmdSelectNyisoAllNodesRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectNyisoAllNodesRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectNyisoAllNodesRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectNyisoAllNodesRTCommand.Connection = vayuDBConnection;

            //
            cmdSelectPJMAllNodesRTCommand = new SqlCommand();
            cmdSelectPJMAllNodesRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                               "node.nodetypekey, Node.NodeName, congestion, loss From pjm.NodeLmph (NOLOCK) Join Node on Node.NodeKey = pjm.NodeLMPh.NodeKey Where " +
                                                               "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                               "READ COMMITTED";
            cmdSelectPJMAllNodesRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPJMAllNodesRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPJMAllNodesRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectPJMAllNodesRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectMISOAllNodesRTCommand = new SqlCommand();
            cmdSelectMISOAllNodesRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                               "node.nodetypekey, Node.NodeName, congestion, loss From miso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = miso.NodeLMPh.NodeKey Where " +
                                                               "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL " +
                                                               "READ COMMITTED";
            cmdSelectMISOAllNodesRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectMISOAllNodesRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectMISOAllNodesRTCommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectMISOAllNodesRTCommand.Connection = vayuDBConnection;

            //
            cmdSelectSPPAllNodesFiveMinRTCommand = new SqlCommand();
            cmdSelectSPPAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                                "node.nodetypekey, Node.NodeName, congestion, loss From SPP.NodeLmp (NOLOCK) Join Node on Node.NodeKey = spp.NodeLMP.NodeKey Where " +
                                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = 12 SET TRANSACTION ISOLATION LEVEL " +
                                                                "READ COMMITTED";
            cmdSelectSPPAllNodesFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectSPPAllNodesFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectSPPAllNodesFiveMinRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectNyisoAllNodesFiveMinRTCommand = new SqlCommand();
            cmdSelectNyisoAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                                "node.nodetypekey, Node.NodeName, congestion, loss From Nyiso.NodeLmp (NOLOCK) Join Node on Node.NodeKey = Nyiso.NodeLMP.NodeKey Where " +
                                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = 3 SET TRANSACTION ISOLATION LEVEL " +
                                                                "READ COMMITTED";
            cmdSelectNyisoAllNodesFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectNyisoAllNodesFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectNyisoAllNodesFiveMinRTCommand.Connection = vayuDBConnection;

            //
            //
            cmdSelectPJMAllNodesFiveMinRTCommand = new SqlCommand();
            cmdSelectPJMAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                                "node.nodetypekey, Node.NodeName, congestion, loss From pjm.NodeLmp (NOLOCK) Join Node on Node.NodeKey = pjm.NodeLMP.NodeKey Where " +
                                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = 1 SET TRANSACTION ISOLATION LEVEL " +
                                                                "READ COMMITTED";
            cmdSelectPJMAllNodesFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPJMAllNodesFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPJMAllNodesFiveMinRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectERCOTAllNodesFiveMinRTCommand = new SqlCommand();
            cmdSelectERCOTAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                             "node.nodetypekey, Node.NodeName, congestion, loss From vayu..NodeLmp (NOLOCK) Join vayu..Node on Node.NodeKey = vayu..NodeLMP.NodeKey  ";
            //cmdSelectERCOTAllNodesFiveMinRTCommand.Connection = SigmaDBConnection;
            cmdSelectERCOTAllNodesFiveMinRTCommand.Connection = vayuDBConnection;

            //
            cmdSelectCAISOAllNodesFiveMinRTCommand = new SqlCommand();
            cmdSelectCAISOAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                             "node.nodetypekey, Node.NodeName, congestion, loss From caiso.nodelmp (NOLOCK) Join Node on Node.NodeKey = caiso.nodelmp.NodeKey Where " +
                                                             "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = 7 SET TRANSACTION ISOLATION LEVEL " +
                                                             "READ COMMITTED";
            cmdSelectCAISOAllNodesFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectCAISOAllNodesFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectCAISOAllNodesFiveMinRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectMISOAllNodesFiveMinRTCommand = new SqlCommand();
            cmdSelectMISOAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                                "node.nodetypekey, Node.NodeName, congestion, loss From miso.NodeLmp (NOLOCK) Join Node on Node.NodeKey = miso.NodeLMP.NodeKey Where " +
                                                                "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = 2 SET TRANSACTION ISOLATION LEVEL " +
                                                                "READ COMMITTED";
            cmdSelectMISOAllNodesFiveMinRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectMISOAllNodesFiveMinRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectMISOAllNodesFiveMinRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectPricePJMDACommand = new SqlCommand();
            cmdSelectPricePJMDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,MarketDateTime, " +
                                            "node.nodetypekey,Node.NodeName, congestion, loss From pjm.NodeDALmph da Join Node on Node.NodeKey = da.NodeKey Where " +
                                            "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = (select min(nodekey) from " + TableName + " where " +
                                            "externalnodeid = @externalnodeid) SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPricePJMDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPricePJMDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPricePJMDACommand.Parameters.Add("@externalnodeid", SqlDbType.BigInt);

            cmdSelectPricePJMDACommand.Connection = vayuDBConnection;
            //
            cmdSelectPriceDACommand = new SqlCommand();
            cmdSelectPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,MarketDateTime, " +
                                            "node.nodetypekey, Node.NodeName, congestion, loss From pjm.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = NodeDALMPH.NodeKey Where " +
                                            "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @externalnodeid SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectPriceDACommand.Parameters.Add("@externalnodeid", SqlDbType.Int);
            cmdSelectPriceDACommand.Connection = vayuDBConnection;
            //
            //cmdSelectAllPJMPriceDACommand = vayuDBConnection.CreateCommand();
            //cmdSelectAllPJMPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
            //                                        "node.nodetypekey, Node.NodeName, congestion, loss From pjm.NodeDALmph da (NOLOCK) Join Node on Node.NodeKey = da.NodeKey Where " +
            //                                        "MarketDateTime >= @Start And MarketDateTime < @End SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            //cmdSelectAllPJMPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            //cmdSelectAllPJMPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            //cmdSelectAllPJMPriceDACommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            //
            cmdSelectAllPriceDACommand = new SqlCommand();
            cmdSelectAllPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                    "node.nodetypekey, Node.NodeName, congestion, loss From pjm.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = NodeDALMPH.NodeKey Where " +
                                                    "MarketDateTime >= @Start And MarketDateTime < @End And MarketKey = @MarketKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectAllPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectAllPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectAllPriceDACommand.Parameters.Add("@MarketKey", SqlDbType.Int);
            cmdSelectAllPriceDACommand.Connection = vayuDBConnection;
            //
            cmdSelectCaisoAllNodesRTCommand = new SqlCommand();
            cmdSelectCaisoAllNodesRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                "node.nodetypekey, node.Nodename, congestion, loss From caiso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = caiso.NodeLMPH.NodeKey Where " +
                                                "MarketDateTime >= @Start And MarketDateTime < @End SET TRANSACTION ISOLATION LEVEL " +
                                                "READ COMMITTED";
            cmdSelectCaisoAllNodesRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectCaisoAllNodesRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectCaisoAllNodesRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectCaisoAllNodesDACommand = new SqlCommand();
            cmdSelectCaisoAllNodesDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                    "node.nodetypekey, Node.NodeName, congestion, loss From caiso.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = caiso.NodeDALMPH.NodeKey Where " +
                                                    "MarketDateTime >= @Start And MarketDateTime < @End SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectCaisoAllNodesDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectCaisoAllNodesDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectCaisoAllNodesDACommand.Connection = vayuDBConnection;
            //
            cmdSelectErcotAllNodesDACommand = new SqlCommand();
            cmdSelectErcotAllNodesDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                    "node.nodetypekey, Node.NodeName, congestion, loss From vayu..NodeDALmph (NOLOCK) Join Vayu..Node on Node.NodeKey = Vayu..NodeDALMPH.NodeKey Where " +
                                                    "MarketDateTime >= @Start And MarketDateTime < @End SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectErcotAllNodesDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectErcotAllNodesDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            //cmdSelectErcotAllNodesDACommand.Connection = SigmaDBConnection;
            cmdSelectErcotAllNodesDACommand.Connection = vayuDBConnection;
            //
            cmdSelectSPPAllNodesDACommand = new SqlCommand();
            cmdSelectSPPAllNodesDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                                    "node.nodetypekey, Node.NodeName, congestion, loss From SPP.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = SPP.NodeDALMPH.NodeKey Where " +
                                                    "MarketDateTime >= @Start And MarketDateTime < @End order by MarketDateTime SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectSPPAllNodesDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectSPPAllNodesDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectSPPAllNodesDACommand.Connection = vayuDBConnection;
            //
            cmdSelectNyisoAllNodesDACommand = new SqlCommand();
            cmdSelectNyisoAllNodesDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                        "node.nodetypekey, Node.NodeName, congestion, loss From Nyiso.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = Nyiso.NodeDALMPH.NodeKey Where " +
                                        "MarketDateTime >= @Start And MarketDateTime < @End order by MarketDateTime SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectNyisoAllNodesDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectNyisoAllNodesDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectNyisoAllNodesDACommand.Connection = vayuDBConnection;

            //

            cmdSelectMISOAllNodesDACommand = new SqlCommand();
            cmdSelectMISOAllNodesDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
                                        "node.nodetypekey, Node.NodeName, congestion, loss From miso.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = miso.NodeDALMPH.NodeKey Where " +
                                        "MarketDateTime >= @Start And MarketDateTime < @End order by MarketDateTime SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectMISOAllNodesDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectMISOAllNodesDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectMISOAllNodesDACommand.Connection = vayuDBConnection;

            cmdSelectCaisoPriceDACommand = new SqlCommand();
            cmdSelectCaisoPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,MarketDateTime, " +
                                            "node.nodetypekey, Node.NodeName, congestion, loss From caiso.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = caiso.NodeDALMPH.NodeKey Where " +
                                            "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectCaisoPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectCaisoPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectCaisoPriceDACommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            cmdSelectCaisoPriceDACommand.Connection = vayuDBConnection;
            //
            cmdSelectErcotPriceDACommand = new SqlCommand();
            cmdSelectErcotPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,MarketDateTime, " +
                                            "node.nodetypekey, Node.NodeName, congestion, loss From Vayu..NodeDALmph (NOLOCK) Join Vayu..Node on Node.NodeKey = Vayu..NodeDALMPH.NodeKey Where " +
                                            "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectErcotPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectErcotPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectErcotPriceDACommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            // cmdSelectErcotPriceDACommand.Connection = SigmaDBConnection;
            cmdSelectErcotPriceDACommand.Connection = vayuDBConnection;
            //   
            cmdSelectSPPPriceDACommand = new SqlCommand();
            cmdSelectSPPPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,MarketDateTime, " +
                                            "node.nodetypekey, Node.NodeName, congestion, loss From SPP.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = SPP.NodeDALMPH.NodeKey Where " +
                                            "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            cmdSelectSPPPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            cmdSelectSPPPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            cmdSelectSPPPriceDACommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            cmdSelectSPPPriceDACommand.Connection = vayuDBConnection;
            //
            //cmdSelectNyisoPriceDACommand = new SqlCommand();
            //cmdSelectNyisoPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,MarketDateTime, " +
            //                    "node.nodetypekey, Node.NodeName, congestion, loss From Nyiso.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = Nyiso.NodeDALMPH.NodeKey Where " +
            //                    "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            //cmdSelectNyisoPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            //cmdSelectNyisoPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            //cmdSelectNyisoPriceDACommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            //cmdSelectNyisoPriceDACommand.Connection = vayuDBConnection;

            ////
            //cmdSelectMISOPriceDACommand = new SqlCommand();
            //cmdSelectMISOPriceDACommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,MarketDateTime, " +
            //                    "node.nodetypekey, Node.NodeName, congestion, loss From miso.NodeDALmph (NOLOCK) Join Node on Node.NodeKey = miso.NodeDALMPH.NodeKey Where " +
            //                    "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            //cmdSelectMISOPriceDACommand.Parameters.Add("@Start", SqlDbType.DateTime);
            //cmdSelectMISOPriceDACommand.Parameters.Add("@End", SqlDbType.DateTime);
            //cmdSelectMISOPriceDACommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            //cmdSelectMISOPriceDACommand.Connection = vayuDBConnection;
            //
            //cmdSelectCaisoPriceRTCommand = new SqlCommand();
            //cmdSelectCaisoPriceRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP, MarketDateTime, " +
            //                                "node.nodetypekey, node.Nodename, congestion, loss From caiso.NodeLmph (NOLOCK) Join Node on Node.NodeKey = caiso.NodeLMPH.NodeKey Where " +
            //                                  "MarketDateTime >= @Start And MarketDateTime < @End And Node.NodeKey = @NodeKey SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            //cmdSelectCaisoPriceRTCommand.Parameters.Add("@Start", SqlDbType.DateTime);
            //cmdSelectCaisoPriceRTCommand.Parameters.Add("@End", SqlDbType.DateTime);
            //cmdSelectCaisoPriceRTCommand.Parameters.Add("@NodeKey", SqlDbType.Int);
            //cmdSelectCaisoPriceRTCommand.Connection = vayuDBConnection;
            //
            cmdSelectFactorCommand = new SqlCommand();
            cmdSelectFactorCommand.CommandText = "select AggregatePNodeID, BusPNodeID, Factor from PJMAggregateFactors";
            cmdSelectFactorCommand.Connection = vayuDBConnection;
            //
            cmdSelectExternalNodeCommand = new SqlCommand();
            cmdSelectExternalNodeCommand.CommandText = "select ExternalNodeID, NodeKey from  " + TableName + " (NOLOCK) where ExternalNodeID in( select ExternalNodeID from " + TableName + " where MarketKey=1 group by ExternalNodeID having " +
                                                        "(count(ExternalNodeID)>1)) order by ExternalNodeID";
            cmdSelectExternalNodeCommand.Connection = vayuDBConnection;
            //
            cmdSelectAggregateNodeCommand = new SqlCommand();
            cmdSelectAggregateNodeCommand.CommandText = "select distinct AggregatePNodeID from dbo.PJMAggregateFactors (NOLOCK)";
            cmdSelectAggregateNodeCommand.Connection = vayuDBConnection;
            //
            cmdSelectAggregrateNodeWithPricingCommand = new SqlCommand();
            cmdSelectAggregrateNodeWithPricingCommand.CommandText = "select distinct externalnodeid from " + TableName + " (nolock) where nodekey in (select distinct Nodekey from pjm.NodeDALMPH(nolock) where nodekey in ( " +
                                                                    "select MIN(nodekey) from " + TableName + " (nolock) where ExternalNodeID in ( select distinct AggregatePNodeID from dbo.PJMAggregateFactors) group by ExternalNodeID))";
            cmdSelectAggregrateNodeWithPricingCommand.Connection = vayuDBConnection;
            //
            cmdSelectNodeFromPNodeCommand = new SqlCommand();
            cmdSelectNodeFromPNodeCommand.CommandText = "select nodekey " + TableName + " (NOLOCK) where externalnodeid = @externalnodeid";
            cmdSelectNodeFromPNodeCommand.Parameters.AddWithValue("@externalnodeid", "externalnodeid");
            cmdSelectNodeFromPNodeCommand.Connection = vayuDBConnection;
            //
            cmdSelectPNodeCommand = new SqlCommand();
            cmdSelectPNodeCommand.Connection = vayuDBConnection;
            cmdSelectPNodeCommand.CommandText = "select nodekey,externalnodeid, MarketKey from " + TableName + " (NOLOCK) where ExternalNodeID in " +
                                                "( select ExternalNodeID from " + TableName + " where ExternalNodeID is not null and ExternalNodeID <>0 group by ExternalNodeID having COUNT(NodeKey)>1)";
            cmdSelectPNodeCommand.Connection = vayuDBConnection;
            //
            //cmdSelectAllOldMisoNodeCommand = new SqlCommand();
            //cmdSelectAllOldMisoNodeCommand.CommandText = "select oldnodekey from miso.FTROldNewNodeMapping (NOLOCK)";
            //cmdSelectAllOldMisoNodeCommand.Connection = vayuDBConnection;
            //
            //cmdSelectAllOldCaisoNodeCommand = new SqlCommand();
            //cmdSelectAllOldCaisoNodeCommand.CommandText = "select OldNodeKey from CAISO.FTROldNewNodeMapping (Nolock)";
            //cmdSelectAllOldCaisoNodeCommand.Connection = vayuDBConnection;
            //
            //cmdSelectNewMisoNodeCommand = new SqlCommand();
            //cmdSelectNewMisoNodeCommand.CommandText = "select newnodekey, ratio, updateddate from miso.FTROldNewNodeMapping (NOLOCK) where oldnodekey = @oldnodekey and newnodekey <> @oldnodekey";
            //cmdSelectNewMisoNodeCommand.Parameters.AddWithValue("@oldnodekey", "oldnodekey");
            //cmdSelectNewMisoNodeCommand.Connection = vayuDBConnection;
            //
            //cmdSelectAllNewMisoNodeCommand = new SqlCommand();
            //cmdSelectAllNewMisoNodeCommand.CommandText = "select newnodekey from miso.FTROldNewNodeMapping (NOLOCK)";
            //cmdSelectAllNewMisoNodeCommand.Connection = vayuDBConnection;
            ////
            //cmdSelectOldMisoNodeCommand = new SqlCommand();
            //cmdSelectOldMisoNodeCommand.CommandText = "select oldnodekey, ratio, updateddate from miso.FTROldNewNodeMapping (NOLOCK) where newnodekey = @oldnodekey and oldnodekey <> @oldnodekey";
            //cmdSelectOldMisoNodeCommand.Parameters.AddWithValue("@oldnodekey", "oldnodekey");
            //cmdSelectOldMisoNodeCommand.Connection = vayuDBConnection;
        }
        
        private SqlDataReader GetLmpReader(bool isDA, int market, bool isFiveMin, NodeChange nodeFactor, DateTime startDate, DateTime endDate)
        {
            SqlDataReader reader = null;
            if (isDA)
            {
                 if (market == 9)
                {
                    if (nodeFactor.NodeKey != 0)
                    {
                        cmdSelectErcotPriceDACommand.Parameters["@NodeKey"].Value = nodeFactor.NodeKey;
                        cmdSelectErcotPriceDACommand.Parameters["@Start"].Value = startDate;
                        cmdSelectErcotPriceDACommand.Parameters["@end"].Value = endDate;
                        reader = cmdSelectErcotPriceDACommand.ExecuteReader();
                    }
                    else
                    {
                        cmdSelectErcotAllNodesDACommand.Parameters["@Start"].Value = startDate;
                        cmdSelectErcotAllNodesDACommand.Parameters["@End"].Value = endDate;
                        reader = cmdSelectErcotAllNodesDACommand.ExecuteReader();
                    }
                } 
                else
                {
                    if (nodeFactor.NodeKey != 0)
                    {
                        if (market != 1)
                        {
                            cmdSelectPriceDACommand.Parameters["@externalnodeid"].Value = nodeFactor.NodeKey;
                            cmdSelectPriceDACommand.Parameters["@Start"].Value = startDate;
                            cmdSelectPriceDACommand.Parameters["@end"].Value = endDate;
                            reader = cmdSelectPriceDACommand.ExecuteReader();
                        }
                        else
                        {
                            cmdSelectPricePJMDACommand.Parameters["@externalnodeid"].Value = nodeFactor.NodeKey;
                            cmdSelectPricePJMDACommand.Parameters["@Start"].Value = startDate;
                            cmdSelectPricePJMDACommand.Parameters["@end"].Value = endDate;
                            reader = cmdSelectPricePJMDACommand.ExecuteReader();
                        }
                    }
                    else
                    {
                        if (market != 1)
                        {
                            cmdSelectAllPriceDACommand.Parameters["@Start"].Value = startDate;
                            cmdSelectAllPriceDACommand.Parameters["@End"].Value = endDate;
                            cmdSelectAllPriceDACommand.Parameters["@MarketKey"].Value = market;
                            reader = cmdSelectAllPriceDACommand.ExecuteReader();
                        }
                        else
                        {
                            cmdSelectAllPJMPriceDACommand.Parameters["@Start"].Value = startDate;
                            cmdSelectAllPJMPriceDACommand.Parameters["@End"].Value = endDate;
                            reader = cmdSelectAllPJMPriceDACommand.ExecuteReader();
                        }
                    }
                }
            }
            else
            {
                if (market == 9)
                {
                    if (nodeFactor.NodeKey != 0)
                    {
                        if (isFiveMin)
                        {

                            cmdSelectPriceERCOTFiveMinRTCommand.Parameters["@externalnodeid"].Value = nodeFactor.NodeKey;
                            cmdSelectPriceERCOTFiveMinRTCommand.Parameters["@Start"].Value = startDate;
                            cmdSelectPriceERCOTFiveMinRTCommand.Parameters["@end"].Value = endDate;
                            reader = cmdSelectPriceERCOTFiveMinRTCommand.ExecuteReader();
                        }
                        else
                        {
                            cmdSelectErcotPriceRTCommand.Parameters["@NodeKey"].Value = nodeFactor.NodeKey;
                            cmdSelectErcotPriceRTCommand.Parameters["@Start"].Value = startDate;
                            cmdSelectErcotPriceRTCommand.Parameters["@end"].Value = endDate;
                            reader = cmdSelectErcotPriceRTCommand.ExecuteReader();
                        }
                    }
                    else
                    {
                        if (isFiveMin)
                        {

                            //
                            cmdSelectERCOTAllNodesFiveMinRTCommand.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED Select Node.NodeKey, node.ExternalNodeID, LMP,  DATEADD(HOUR , MarketHour , DATEADD(MINUTE , MarketMin , MarketDate)) as marketdatetime , " +
                                         "node.nodetypekey, node.Nodename, 0, 0 From Vayu..NodeLmpMin (NOLOCK) Join Vayu..Node on Node.NodeKey = NodeLMPMin.NodeKey Where " +
                                           "MarketDate = '" + startDate.Date.ToString() + "' And MarketHour =  " + startDate.Hour.ToString() + " and MarketMin = " + startDate.Minute.ToString() + " " +
                                           " SET TRANSACTION ISOLATION LEVEL READ COMMITTED";

                            reader = cmdSelectERCOTAllNodesFiveMinRTCommand.ExecuteReader();
                        }
                        else
                        {
                            cmdSelectAllErcotPriceRTCommand.Parameters["@Start"].Value = startDate;
                            cmdSelectAllErcotPriceRTCommand.Parameters["@End"].Value = endDate;
                            cmdSelectAllErcotPriceRTCommand.Parameters["@MarketKey"].Value = market;
                            reader = cmdSelectAllErcotPriceRTCommand.ExecuteReader();
                        }
                    }
                }
                else if (market == 2)
                {
                    if (nodeFactor.NodeKey != 0)
                    {
                        if (isFiveMin)
                        {
                            cmdSelectPriceMISOFiveMinRTCommand.Parameters["@externalnodeid"].Value = nodeFactor.NodeKey;
                            cmdSelectPriceMISOFiveMinRTCommand.Parameters["@Start"].Value = startDate;
                            cmdSelectPriceMISOFiveMinRTCommand.Parameters["@end"].Value = endDate;
                            reader = cmdSelectPriceMISOFiveMinRTCommand.ExecuteReader();
                        }
                        else
                        {
                            cmdSelectMISOPriceRTCommand.Parameters["@NodeKey"].Value = nodeFactor.NodeKey;
                            cmdSelectMISOPriceRTCommand.Parameters["@Start"].Value = startDate;
                            cmdSelectMISOPriceRTCommand.Parameters["@end"].Value = endDate;
                            reader = cmdSelectMISOPriceRTCommand.ExecuteReader();
                        }
                    }
                    else
                    {
                        if (isFiveMin)
                        {
                            cmdSelectMISOAllNodesFiveMinRTCommand.Parameters["@Start"].Value = startDate;
                            cmdSelectMISOAllNodesFiveMinRTCommand.Parameters["@End"].Value = endDate;
                            reader = cmdSelectMISOAllNodesFiveMinRTCommand.ExecuteReader();
                        }
                        else
                        {
                            cmdSelectMISOAllNodesRTCommand.Parameters["@Start"].Value = startDate;
                            cmdSelectMISOAllNodesRTCommand.Parameters["@End"].Value = endDate;
                            cmdSelectMISOAllNodesRTCommand.Parameters["@MarketKey"].Value = market;
                            reader = cmdSelectMISOAllNodesRTCommand.ExecuteReader();
                        }
                    }
                }
            }
            return reader;
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
        #endregion

        #region Struct
        public struct SummaryType
        {
          
            public double price { get; set; }
           
            public DateTime dates { get; set; }
        }
        #endregion


        #region Pivate Members
        
        private static bool sFirstTime;
         
        private SqlCommand cmdSelectAllPJMPriceDACommand;
        #endregion

        #region Public Methods
         
        public Node[] GetAllUptoPriceByNodeID(int[] sourceIDs, int market, bool isDA, DateTime startDate, DateTime endDate, bool onlyPrice)
        {
            FillHash(sourceIDs, sourceIDs, market);
            return GetAllUptoPriceDateTime(market, isDA, startDate, endDate, false, onlyPrice);
        }
        public void FillHash(int[] sourceIDs, int[] sinkIDs, int market)
        {
            if (sFirstTime)
            {
                return;
            }
            string sourceQuery = string.Empty;
            string sinkQuery = string.Empty;
            for (int i = 0; i < sourceIDs.Length; i++)
            {
                sourceQuery += sourceIDs[i].ToString() + ",";
            }
            for (int i = 0; i < sinkIDs.Length; i++)
            {
                sinkQuery += sinkIDs[i].ToString() + ",";
            }
            sinkQuery = sinkQuery.TrimEnd(',');
            sourceQuery = sourceQuery.TrimEnd(',');
            cmdSelectSourceSinkNodeCommand = new SqlCommand();
            cmdSelectSourceSinkNodeCommand.CommandText = "select src.SourceNodeKey, src.SourceName, n.ExternalNodeId as SourceExternalNodeID, n.NodeTypeKey as SourceNodeTypeKey, "
                                            + "n.Zone as SourceNodeZone, sink.SinkNodeKey, sink.SinkName, n2.ExternalNodeID as SinkExternalNodeID, "
                                            + "n2.NodeTypeKey as SinkNodeTypeKey, n2.Zone as SinkNodeZone "
                                            + "from EESPathList src (NOLOCK) "
                                            + "inner join Node n (NOLOCK) on n.NodeKey = src.SourceNodeKey "
                                            + "inner join EESPathList sink (NOLOCK) on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey "
                                            + "inner join Node n2 (NOLOCK) on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = @market and n2.MarketKey = @market and src.MarketKey = @market "
                                            + "and sink.MarketKey = @market and src.SourceNodeKey in (" + sourceQuery + ")  and sink.SinkNodeKey in (" + sinkQuery + ") order by n.NodeName ";
            cmdSelectSourceSinkNodeCommand.Parameters.AddWithValue("@market", market);
            cmdSelectSourceSinkNodeCommand.Connection = vayuDBConnection;
            dictPJMSourceSinkHash.Clear();
            FillSourceSinkHash();
            sFirstTime = true;
        }
        public Node[] GetVirtualLmps(List<Node> mNodeList, int market, bool isDa, DateTime startDate, DateTime endDate, bool isFiveMin)
        {
            Dictionary<DateTime, double> priceHash = new Dictionary<DateTime, double>();
            Node[] nodes = mNodeList.ToArray();
            List<TimePrice> timePriseList = new List<TimePrice>();
            // Node[] priceNodes = new Node();
            foreach (Node node in nodes)
            {
                timePriseList = node.TimePriceList;
                Node[] returnNodes = GetPrices(node.Market, node.PNodeId, isDa, startDate, endDate, isFiveMin, true);

                if (returnNodes.Length > 0)
                {
                    int length = returnNodes.Length;
                    foreach (TimePrice item in returnNodes[0].TimePriceList)
                    {
                        if (!priceHash.ContainsKey(item.MarketTime))
                        {
                            priceHash.Add(item.MarketTime, item.Price);
                        }
                    }
                    for (int i = 0; i < length; i++)
                    {
                        node.TimePriceList = returnNodes[i].TimePriceList;
                    }
                    foreach (TimePrice timePrice in node.TimePriceList)
                    {
                        if (priceHash.ContainsKey(timePrice.MarketTime))
                        {
                            timePrice.Price = priceHash[timePrice.MarketTime];
                        }
                    }


                }
            }
            return nodes;
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
        internal void FillErcotSourceSinkHash()
        {
            if (dictPJMSourceSinkHash.Count > 0)
            {
                return;
            }
            dictPJMSourceSinkHash = new Dictionary<int, Node>();
            dictErcotSourceSinkHash = new Dictionary<int, Node>();
            vayuDBConnection.Open();
            cmdSelectSourceSinkNodeCommand.Parameters["@Market"].Value = 1;
            SqlDataReader reader = cmdSelectSourceSinkNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                Node sourcenode = new Node();
                sourcenode.NodeId = Convert.ToInt32(reader.GetValue(0));
                sourcenode.NodeName = reader.GetString(1);
                sourcenode.PNodeId = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetDecimal(2));
                sourcenode.Market = 1;
                Node sinknode = new Node();
                sinknode.NodeId = Convert.ToInt32(reader.GetValue(5));
                sinknode.NodeName = reader.GetString(6);
                sinknode.PNodeId = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetDecimal(7));
                sinknode.Market = 1;
                try
                {
                    if (!dictPJMSourceSinkHash.ContainsKey(sourcenode.NodeId))
                    {
                        dictPJMSourceSinkHash.Add(sourcenode.NodeId, sourcenode);
                    }
                }
                catch (Exception ex)
                {
                }
                try
                {
                    if (!dictPJMSourceSinkHash.ContainsKey(sinknode.NodeId))
                    {
                        dictPJMSourceSinkHash.Add(sinknode.NodeId, sinknode);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            reader.Close();
            vayuDBConnection.Close();
        }
        #endregion
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
