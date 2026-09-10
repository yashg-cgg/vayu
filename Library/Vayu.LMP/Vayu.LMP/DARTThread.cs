using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.LMP
{

    [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    public class DARTThread
    {
        private List<Node> lstPriceList = new List<Node>();
        private ConcurrentDictionary<string, double> dictPriceHash;
        private ConcurrentDictionary<string, NodePriceLibrary.LMP> dictLmpHash;
        private ConcurrentDictionary<string, ConcurrentDictionary<int, double>> dictFiveMinPriceHash;
        //private static string sDartEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress();

        private bool bIsDA;
        private string strType;
        private int Market;
        private DateTime StartDate;
        private DateTime EndDate;
        private bool onlyPrice;

        public DARTThread(List<Node> priceList, ConcurrentDictionary<string, double> priceHash, ConcurrentDictionary<string, ConcurrentDictionary<int, double>> fiveMinPriceHash, bool isDA, string type,
                                int market, DateTime start, DateTime end, ConcurrentDictionary<string, NodePriceLibrary.LMP> lmpHash, bool onlyPrice = true)
        {
            dictPriceHash = priceHash;
            dictFiveMinPriceHash = fiveMinPriceHash;
            lstPriceList = priceList;
            bIsDA = isDA;
            strType = type;
            Market = market;
            StartDate = start;
            EndDate = end;
            dictLmpHash = lmpHash;
            this.onlyPrice = onlyPrice;
        }
        public void Run()
        {
            Console.WriteLine($"[THREAD START] Type={strType}, IsDA={bIsDA}, Market={Market}, Start={StartDate}, End={EndDate}, onlyPrice={onlyPrice}");

            try
            {
                NetTcpBinding myBinding = new NetTcpBinding
                {
                    Security = { Mode = SecurityMode.None },
                    OpenTimeout = new TimeSpan(0, 30, 0),
                    SendTimeout = new TimeSpan(0, 12, 0),
                    ReceiveTimeout = new TimeSpan(0, 12, 0),
                    CloseTimeout = new TimeSpan(0, 12, 0),
                    TransactionFlow = false,
                    MaxReceivedMessageSize = int.MaxValue,
                    MaxBufferPoolSize = int.MaxValue,
                    MaxBufferSize = int.MaxValue,
                    TransferMode = TransferMode.Buffered
                };

                myBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;

                string endpoint = ServiceConnections.GetLMPServiceAddress();
                Console.WriteLine($"[WCF] Connecting to: {endpoint}");

                ChannelFactory<ILMP> pipeFactory =
                    new ChannelFactory<ILMP>(myBinding, new EndpointAddress(endpoint));

                foreach (var op in pipeFactory.Endpoint.Contract.Operations)
                {
                    var behavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                        as DataContractSerializerOperationBehavior;

                    if (behavior != null)
                        behavior.MaxItemsInObjectGraph = int.MaxValue;
                }

                ILMP nodeProxy = pipeFactory.CreateChannel();

                Node[] priceNodes = null;

                Console.WriteLine($"[WCF CALL] strType={strType}");

                if (strType == "all")
                {
                    priceNodes = nodeProxy.GetAllPrice(Market, bIsDA, StartDate, EndDate, true);
                }
                else if (strType == "uptos")
                {
                    priceNodes = nodeProxy.GetAllUptoPrice(Market, bIsDA, StartDate, EndDate, onlyPrice);
                }
                else if (strType == "fivemin")
                {
                    priceNodes = nodeProxy.GetAllFiveMinPrice(Market, StartDate.AddHours(-1), EndDate.AddHours(-1), true);
                }
                else
                {
                    Console.WriteLine($"[WCF CALL] Sending {lstPriceList.Count} nodes");
                    priceNodes = nodeProxy.GetPrice(lstPriceList.ToArray(), bIsDA, dictPriceHash != null, false);
                }

                if (priceNodes == null)
                {
                    Console.WriteLine("[ERROR] priceNodes is NULL");
                    return;
                }

                Console.WriteLine($"[WCF RESPONSE] Nodes received: {priceNodes.Length}");

                foreach (Node priceNode in priceNodes)
                {
                    Console.WriteLine($"[NODE] Processing NodeId={priceNode.NodeId}");

                    if (onlyPrice)
                    {
                        if (dictPriceHash != null || dictFiveMinPriceHash != null)
                        {
                            foreach (TimePrice time in priceNode.TimePriceList)
                            {
                                string priceKey = time.MarketTime.ToString() + priceNode.NodeId;

                                if (strType == "fivemin")
                                {
                                    DateTime hourDate = DateTime.Parse(
                                        $"{time.MarketTime.Month}/{time.MarketTime.Day}/{time.MarketTime.Year} {time.MarketTime.Hour}:00");

                                    priceKey = hourDate.AddHours(1) + priceNode.NodeId.ToString();

                                    if (!dictFiveMinPriceHash.ContainsKey(priceKey))
                                    {
                                        Console.WriteLine($"[5MIN][NEW KEY] {priceKey}");
                                        dictFiveMinPriceHash.TryAdd(priceKey, new ConcurrentDictionary<int, double>());
                                    }

                                    dictFiveMinPriceHash[priceKey].TryAdd(time.MarketTime.Minute, time.Price);
                                }
                                else
                                {
                                    if (!dictPriceHash.ContainsKey(priceKey))
                                    {
                                        Console.WriteLine($"[ADD] {priceKey} -> {time.Price}");
                                        dictPriceHash.TryAdd(priceKey, time.Price);
                                    }
                                    else if (time.MarketTime > DateTime.Today)
                                    {
                                        Console.WriteLine($"[UPDATE FUTURE] {priceKey} -> {time.Price}");
                                        dictPriceHash[priceKey] = time.Price;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("[WARNING] dictPriceHash and dictFiveMinPriceHash are NULL");

                            foreach (LmpTimePrice time in priceNode.LmpTimePriceList)
                            {
                                string priceKey = time.MarketTime.ToString() + priceNode.NodeId;

                                if (!dictLmpHash.ContainsKey(priceKey))
                                {
                                    Console.WriteLine($"[LMP ADD] {priceKey}");
                                    dictLmpHash.TryAdd(priceKey, time.Lmp);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (LmpTimePrice price2 in priceNode.LmpTimePriceList)
                        {
                            string priceKey = price2.MarketTime.ToString() + priceNode.NodeId;

                            if (dictLmpHash == null)
                            {
                                Console.WriteLine("[INIT] dictLmpHash was NULL");
                                dictLmpHash = new ConcurrentDictionary<string, NodePriceLibrary.LMP>();
                            }

                            if (!dictLmpHash.ContainsKey(priceKey))
                            {
                                Console.WriteLine($"[LMP ADD] {priceKey}");
                                dictLmpHash.TryAdd(priceKey, price2.Lmp);
                            }
                            else if (price2.MarketTime > DateTime.Today)
                            {
                                Console.WriteLine($"[LMP UPDATE FUTURE] {priceKey}");
                                dictLmpHash[priceKey] = price2.Lmp;
                            }
                        }
                    }
                }

                Console.WriteLine("[THREAD END SUCCESS]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        //public void Run()
        //{
        //    try
        //    {
        //        NetTcpBinding myBinding = new NetTcpBinding();
        //        myBinding.Security.Mode = SecurityMode.None;
        //        myBinding.OpenTimeout = new TimeSpan(0, 30, 0);
        //        myBinding.SendTimeout = new TimeSpan(0, 12, 0);
        //        myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
        //        myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
        //        myBinding.TransactionFlow = false;
        //        myBinding.MaxReceivedMessageSize = int.MaxValue;
        //        myBinding.MaxBufferPoolSize = int.MaxValue;
        //        myBinding.MaxBufferSize = int.MaxValue;
        //        myBinding.TransferMode = TransferMode.Buffered;
        //        myBinding.ReaderQuotas.MaxArrayLength = int.MaxValue; 
        //        ChannelFactory<ILMP> pipeFactory = new ChannelFactory<ILMP>(myBinding, new EndpointAddress(ServiceConnections.GetLMPServiceAddress()));
        //        foreach (var operationDescription in pipeFactory.Endpoint.Contract.Operations)
        //        {
        //            var dataContractBehavior = operationDescription.Behaviors[typeof(DataContractSerializerOperationBehavior)]
        //                            as DataContractSerializerOperationBehavior;
        //            if (dataContractBehavior != null)
        //            {
        //                dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;

        //            }
        //        }
        //        ILMP nodeProxy = pipeFactory.CreateChannel();
        //        Node[] priceNodes = null;
        //        if (strType == "all")
        //        {
        //            priceNodes = nodeProxy.GetAllPrice(Market, bIsDA, StartDate, EndDate, true);
        //        }
        //        else if (strType == "uptos")
        //        {
        //            priceNodes = nodeProxy.GetAllUptoPrice(Market, bIsDA, StartDate, EndDate, onlyPrice);
        //        }
        //        else if (strType == "fivemin")
        //        {
        //            priceNodes = nodeProxy.GetAllFiveMinPrice(Market, StartDate.AddHours(-1), EndDate.AddHours(-1), true);
        //        }
        //        else
        //        {
        //            priceNodes = nodeProxy.GetPrice(lstPriceList.ToArray<Node>(), bIsDA, dictPriceHash != null, false);
        //        }
        //        foreach (Node priceNode in priceNodes)
        //        {
        //            if (onlyPrice)
        //            {
        //                if (dictPriceHash != null || dictFiveMinPriceHash != null)
        //                {
        //                    foreach (TimePrice time in priceNode.TimePriceList)
        //                    {
        //                        if (strType == "fivemin")
        //                        {
        //                            DateTime hourDate = DateTime.Parse(time.MarketTime.Month + "/" + time.MarketTime.Day + "/" + time.MarketTime.Year + " " + time.MarketTime.Hour + ":00");
        //                            string priceKey = hourDate.AddHours(1).ToString() + priceNode.NodeId.ToString();
        //                            ConcurrentDictionary<int, double> minuteHash = new ConcurrentDictionary<int, double>();
        //                            if (dictFiveMinPriceHash.ContainsKey(priceKey))
        //                            {
        //                                minuteHash = dictFiveMinPriceHash[priceKey];
        //                            }
        //                            else
        //                            {
        //                                dictFiveMinPriceHash.TryAdd(priceKey, minuteHash);
        //                            }
        //                            minuteHash.TryAdd(time.MarketTime.Minute, time.Price);
        //                        }
        //                        else
        //                        {
        //                            string priceKey = time.MarketTime.ToString() + priceNode.NodeId.ToString();
        //                            if (!dictPriceHash.ContainsKey(priceKey))
        //                            {
        //                                dictPriceHash.TryAdd(priceKey, time.Price);
        //                            }
        //                            else if (time.MarketTime > DateTime.Today)
        //                            {
        //                                dictPriceHash[priceKey] = time.Price;
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (LmpTimePrice time in priceNode.LmpTimePriceList)
        //                    {
        //                        string priceKey = time.MarketTime.ToString() + priceNode.NodeId.ToString();
        //                        if (!dictLmpHash.ContainsKey(priceKey))
        //                        {
        //                            dictLmpHash.TryAdd(priceKey, time.Lmp);
        //                        }
        //                        else if (time.MarketTime > DateTime.Today)
        //                        {
        //                            dictLmpHash[priceKey] = time.Lmp;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                foreach (LmpTimePrice price2 in priceNode.LmpTimePriceList)
        //                {
        //                    string priceKey = price2.MarketTime.ToString() + priceNode.NodeId.ToString();
        //                    if (dictLmpHash == null)
        //                    {
        //                        dictLmpHash = new ConcurrentDictionary<string, NodePriceLibrary.LMP>();
        //                    }
        //                    if (!dictLmpHash.ContainsKey(priceKey))
        //                    {
        //                        dictLmpHash.TryAdd(priceKey, price2.Lmp);
        //                    }
        //                    else if (price2.MarketTime > DateTime.Today)
        //                    {
        //                        dictLmpHash[priceKey] = price2.Lmp;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}
    }
}
