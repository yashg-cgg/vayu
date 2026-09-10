using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Vayu.NodePriceFiveMinLibrary;
using Vayu.NodePriceLibrary;
using Vayu.CommonAccessLibrary;
namespace Vayu.NodePriceFiveMinService
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.INodePriceFiveMin" />
    [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue, InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class NodePriceFiveMinServer : INodePriceFiveMin
    {
        /// <summary>
        /// The Subscriber Dictionary
        /// </summary>
        private static Dictionary<INodePriceFiveMinCallback, int> sSubscriberHash = new Dictionary<INodePriceFiveMinCallback, int>();
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object lockObj = new object();
        /// <summary>
        /// Timer
        /// </summary>
        private static System.Timers.Timer sTimer = null;
        /// <summary>
        /// The Market Node Count Dictionary
        /// </summary>
        private static Dictionary<int, int> sMarketNodeCountHash = new Dictionary<int, int>();
        /// <summary>
        /// The Remover HashSet
        /// </summary>
        private static HashSet<INodePriceFiveMinCallback> sRemoverHash = new HashSet<INodePriceFiveMinCallback>();
        /// <summary>
        /// The Market Date Dictionary
        /// </summary>
        private static Dictionary<int, DateTime> sMarketDateHash = new Dictionary<int, DateTime>();
        /// <summary>
        /// The Node Hash cache
        /// </summary>
        Dictionary<int, Node[]> nodeHashCache = new Dictionary<int, Node[]>();
        /// <summary>
        /// The Market Data Dictionary
        /// </summary>
        Dictionary<int, bool> marketDataToSendHash = new Dictionary<int, bool>();
        /// <summary>
        /// The last market time
        /// </summary>
        Dictionary<int, DateTime> lastMarketTime = new Dictionary<int, DateTime>();

        /// <summary>
        /// Called when [timer event].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            if (sSubscriberHash.Count > 0)
            {
                CallBackClients();
            }
            sTimer.Enabled = true;
        }

        #region Private Methods

        /// <summary>
        /// Calls back clients.
        /// </summary>
        private void CallBackClients()
        {
            List<int> keys = new List<int>();
            lock (lockObj)
            {
                keys = sSubscriberHash.Values.Distinct().ToList();
            }
            Dictionary<int, Node[]> nodeHash = new Dictionary<int, Node[]>();
            foreach (int market in keys)
            {
                bool shouldF = ShouldFetchByMArket(market);
                Node[] nodes = null;

                try
                {
                    if (shouldF)
                        nodes = GetNodesForCache(market);
                    else
                        nodes = nodeHashCache[market];
                }
                catch { }

                if (nodes != null && nodes.Count() > 0)
                {
                    nodeHash.Add(market, nodes);
                }
            }
            if (nodeHashCache.Count == 0)
            {
                nodeHashCache = nodeHash;
                Parallel.ForEach(keys, a =>
                {
                    marketDataToSendHash.Add(a, true);
                });
            }
            else
            {
                Parallel.ForEach(nodeHash.Keys, a =>
                {
                    if (marketDataToSendHash.ContainsKey(a))
                    {
                        marketDataToSendHash.Remove(a);
                    }
                    marketDataToSendHash.Add(a, ShouldSendData(a, nodeHash[a]));
                });
            }
            lock (lockObj)
            {
                Parallel.ForEach(sSubscriberHash, cbItem =>
                {
                    try
                    {
                        if (marketDataToSendHash.ContainsKey(cbItem.Value))
                        {
                            if (marketDataToSendHash[cbItem.Value])
                            {
                                Console.WriteLine("Data sent to market key " + cbItem.Value + " number " + nodeHash[cbItem.Value].Length);
                                cbItem.Key.SendPrice(nodeHash[cbItem.Value]);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Data sent to market key " + cbItem.Value + " number " + nodeHash[cbItem.Value].Length);
                            cbItem.Key.SendPrice(nodeHash[cbItem.Value]);
                        }
                    }
                    catch (CommunicationObjectAbortedException ce)
                    {
                        if (!sRemoverHash.Contains(cbItem.Key))
                        {
                            Console.WriteLine(ce.Message);
                            sRemoverHash.Add(cbItem.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);

                    }
                });
                nodeHashCache = nodeHash;
            }
            foreach (INodePriceFiveMinCallback dleteItem in sRemoverHash)
            {
                try
                {
                    if (sSubscriberHash.ContainsKey(dleteItem))
                    {
                        sSubscriberHash.Remove(dleteItem);
                    }
                }
                catch
                {
                }
            }
            sRemoverHash.Clear();
        }

        /// <summary>
        /// Fetch by Market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        private bool ShouldFetchByMArket(int market)
        {
            try
            {
                if (nodeHashCache == null || nodeHashCache.Count == 0 || !nodeHashCache.ContainsKey(market))
                    return true;

                DateTime dt = GetLastDTByMArket(market);
                Node[] nodes = nodeHashCache[market];
                Node westHub = nodes.Where(x => x.NodeId == 30).FirstOrDefault();
                if (westHub == null || westHub.LmpTimePriceList == null || westHub.LmpTimePriceList.Max(x => x.MarketTime) < dt)
                    return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Gets the last date by market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public DateTime GetLastDTByMArket(int market)
        {
            try
            {

                SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection();
                if (market == 9)
                {
                    con = new VayuDBConnection().GetInstance().GetSqlConnection();
                }
                SqlCommand cmd = con.CreateCommand();
                 if (market == 9)
                    cmd.CommandText = "select top 1 DATEADD(HOUR , MarketHour , DATEADD(MINUTE , MarketMin , MarketDate)) as marketdatetime from NodeLMPMin where NodeKey = 57194 order by MarketDate desc , MarketHour desc , MarketMin desc ";

                cmd.Connection.Open();
                DateTime? dt = cmd.ExecuteScalar() as DateTime?;
                cmd.Connection.Close();
                return dt.GetValueOrDefault();
            }
            catch
            {
                return default(DateTime);
            }
        }

        private static DateTime GetErcotLatest(int market)
        {
            try
            {
                SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection();
                SqlCommand cmd = con.CreateCommand();
                  if (market == 9)
                    cmd.CommandText = "select top 1 DATEADD(HOUR , MarketHour , DATEADD(MINUTE , MarketMin , MarketDate)) as marketdatetime from Vayu..NodeLMPMin where NodeKey = 57194 order by MarketDate desc , MarketHour desc , MarketMin desc ";

                  if(cmd.Connection.State==System.Data.ConnectionState.Closed)
                              cmd.Connection.Open();


                DateTime? dt = cmd.ExecuteScalar() as DateTime?;
                cmd.Connection.Close();
                return dt.GetValueOrDefault();
            }
            catch { return default(DateTime); 
            }
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        private bool ShouldSendData(int p, Node[] nodes)
        {
            try
            {
                if (nodeHashCache[p].Count() != nodes.Count())
                {
                    return true;
                }
                bool toSend = false;
                for (int i = 0; i < 15; i++)
                {
                    if (nodeHashCache[p][i].LmpTimePriceList.Count == 0 || nodes[i].LmpTimePriceList.Count == 0)
                        continue;

                    if (nodeHashCache[p][i].LmpTimePriceList.Select(x => x.MarketTime).Max() < nodes[i].LmpTimePriceList.Select(x => x.MarketTime).Max())
                    {
                        toSend = true;
                        break;
                    }
                }
                return toSend;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the nodes for cache.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        private static Node[] GetNodesForCache(int market)
        {
            Console.WriteLine("Calling GetNodesForCache Method For Market " + market);
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
            ChannelFactory<ILMP> pipeFactory = new ChannelFactory<ILMP>(myBinding, new EndpointAddress(Vayu.CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress()));
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
            Node[] nodes = null;
            int counter = 0;
            while (nodes == null)
            {
                try
                {
                    DateTime dateStart = DateTime.Now;
                      if (market == 9)
                    {
                        Console.WriteLine("Getting All Five Min Prices  " + dateStart + " For Market " + market + "");
                        dateStart = GetErcotLatest(market);
                        nodes = nodeProxy.GetAllFiveMinPrice(market, dateStart, dateStart.AddMinutes(5), false);
                    }
                    if (nodes == null)
                        Console.WriteLine("Nodes are Null..");
                    else
                        Console.WriteLine("Node Count is " + nodes.Count());

                    if (nodes != null && nodes.Count() == 0)
                    {
                        nodes = null;
                    }
                    if (nodes == null)
                    {
                        Thread.Sleep(5000);
                        Console.WriteLine("Nodes did Not Null...");
                        counter++;
                    }
                    if (counter >= 5)
                        break;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + e.Message);
                    Thread.Sleep(5000);
                    break;
                }
            }
            try
            {
                Console.WriteLine("Got data from lmp service for market " + market + " it count : " + nodes.Count());
            }
            catch
            {
                //Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + e.Message);
            }
            return nodes;
        }

        /// <summary>
        /// Gets the subscriber ip address.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns></returns>
        private string GetSubscriberIpAddress(OperationContext operationContext)
        {
            return ((operationContext.IncomingMessageProperties as MessageProperties)[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the nodes for market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public Node[] GetNodesForMarket(int market)
        {
            nodeHashCache.Clear();
            Console.WriteLine("Calling GetNodesForMarket Method for Market " + market);
            if (nodeHashCache.Count == 0)
            {
                nodeHashCache.Add(market, GetNodesForCache(market));
            }
            else if (!nodeHashCache.ContainsKey(market))
            {
                Console.WriteLine("Prices Not Getting nodeHashCache Dictionary for Market " + market + " Calling GetNodesForCache Method ..");
                nodeHashCache.Add(market, GetNodesForCache(market));
            }
            return nodeHashCache[market];
        }
        /// <summary>
        /// Connects this instance.
        /// </summary>
        public void Connect()
        {
            sTimer = new System.Timers.Timer();
            OnTimerEvent(null, null);
            sTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            sTimer.Interval = 30000;
            sTimer.Start();
            using (ServiceHost host = new ServiceHost(typeof(NodePriceFiveMinServer), new Uri("net.tcp://localhost:8001")))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
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
                myBinding.Security.Mode = SecurityMode.None;
                host.AddServiceEndpoint(typeof(INodePriceFiveMin), myBinding, "ISubscribe");

                try
                {
                    host.Open();
                    Console.WriteLine("Successfully opened port 8001  Node Price Five Min Service");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + e.Message);
                }
            }
        }
        /// <summary>
        /// Subscribes this instance by Market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public bool Subscribe(int market)
        {
            lock (lockObj)
            {
                try
                {
                    INodePriceFiveMinCallback callback = OperationContext.Current.GetCallbackChannel<INodePriceFiveMinCallback>();
                    if (sSubscriberHash.ContainsKey(callback))
                    {
                        sSubscriberHash.Remove(callback);
                    }
                    sSubscriberHash.Add(callback, market);
                    Console.WriteLine("No. of Subscribers: " + sSubscriberHash.Count);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + e.Message);
                    return false;
                }
            }

        }
        /// <summary>
        /// Checks the connection is alive or not.
        /// </summary>
        public void HeartBeat()
        {
            Console.WriteLine(DateTime.Now.ToString() + "Heartbeat by " + GetSubscriberIpAddress(OperationContext.Current));
        }
        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Unsubscribe()
        {
            lock (lockObj)
            {
                try
                {
                    INodePriceFiveMinCallback callback = OperationContext.Current.GetCallbackChannel<INodePriceFiveMinCallback>();
                    if (sSubscriberHash.ContainsKey(callback))
                    {
                        sSubscriberHash.Remove(callback);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + e.Message);
                    return false;
                }
            }
        }

        #endregion
    }
}
