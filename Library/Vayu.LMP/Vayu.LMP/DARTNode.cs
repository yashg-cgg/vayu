using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vayu.DBLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.LMP
{
    public static class DARTNode
    {
        public static ConcurrentDictionary<string, double> dictRTHash = new ConcurrentDictionary<string, double>();
        public static ConcurrentDictionary<string, double> dictDAHash = new ConcurrentDictionary<string, double>();
        public static ConcurrentDictionary<string, NodePriceLibrary.LMP> dictRTLmpHash = new ConcurrentDictionary<string, NodePriceLibrary.LMP>();
        public static ConcurrentDictionary<string, NodePriceLibrary.LMP> dictDALmpHash = new ConcurrentDictionary<string, NodePriceLibrary.LMP>();
        public static ConcurrentDictionary<int, Tuple<DateTime, DateTime>> dictAllDateHash = new ConcurrentDictionary<int, Tuple<DateTime, DateTime>>();
        
        public static ConcurrentDictionary<string, ConcurrentDictionary<int, double>> dictRTFiveMinHash = new ConcurrentDictionary<string, ConcurrentDictionary<int, double>>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketKey"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public static void GetAllFiveMinDarts(int marketKey, DateTime startDate, DateTime endDate)
        {
            DARTThread thread = new DARTThread(null, null, dictRTFiveMinHash, false, "fivemin", marketKey, startDate, endDate, null);
            thread.Run();
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="marketKey"></param>
       /// <param name="startDate"></param>
       /// <param name="endDate"></param>
        public static void GetAllDarts(int marketKey, DateTime startDate, DateTime endDate)
        {
            Tuple<DateTime, DateTime> tuple = null;
            if (dictAllDateHash.ContainsKey(marketKey))
            {
                tuple = dictAllDateHash[marketKey];
            }
            for (int i = 0; i < 2; i++)
            {
                bool found = false;
                if (i == 0)
                {
                    if (tuple != null && tuple.Item1 > startDate)
                    {
                        found = true;
                        endDate = tuple.Item1;
                    }
                }
                else
                {
                    if (tuple != null && tuple.Item2 < endDate)
                    {
                        found = true;
                        startDate = tuple.Item2;
                    }
                }
                if (!dictAllDateHash.ContainsKey(marketKey) || found)
                {
                    DARTThread range = new DARTThread(null, dictDAHash, null, true, "all", marketKey, startDate, endDate, null);
                    Thread thread = new Thread(new ThreadStart(range.Run));
                    thread.Start();
                    range = new DARTThread(null, dictRTHash, null, false, "all", marketKey, startDate, endDate, null);
                    Thread thread1 = new Thread(new ThreadStart(range.Run));
                    thread1.Start();
                    if (thread != null)
                    {
                        thread.Join();
                    }
                    if (thread1 != null)
                    {
                        thread1.Join();
                    }
                    if (!dictAllDateHash.ContainsKey(marketKey))
                    {
                        tuple = new Tuple<DateTime, DateTime>(startDate, endDate);
                        dictAllDateHash.TryAdd(marketKey, tuple);
                        break;
                    }
                    else
                    {
                        DateTime tempStartDate = startDate < tuple.Item1 ? startDate : tuple.Item1;
                        DateTime tempEndtDate = endDate > tuple.Item2 ? endDate : tuple.Item2;
                        tuple = new Tuple<DateTime, DateTime>(tempStartDate, tempEndtDate);
                        dictAllDateHash[marketKey] = tuple;
                    }
                }
                else if (startDate == DateTime.Today && i == 0)
                {
                    DARTThread range = new DARTThread(null, dictRTHash, null, false, "all", marketKey, startDate, endDate, null);
                    Thread thread1 = new Thread(new ThreadStart(range.Run));
                    thread1.Start();
                    if (thread1 != null)
                    {
                        thread1.Join();
                    }
                }
            }
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="rtNodeList"></param>
       /// <param name="daNodeList"></param>
        public static void GetDARTForHour(List<Node> rtNodeList, List<Node> daNodeList)
        {
            List<Node> sendDaList = new List<Node>();
            List<Node> sendRtList = new List<Node>();
            foreach (Node node in rtNodeList)
            {
                Node sendNode = new Node();
                sendNode.Market = node.Market;
                sendNode.NodeId = node.NodeId;
                sendNode.NodeName = node.NodeName;
                sendNode.PNodeId = node.PNodeId;
                List<LmpTimePrice> timePriceList = node.LmpTimePriceList;
                List<LmpTimePrice> sendTimePriceList = new List<LmpTimePrice>();
                foreach (LmpTimePrice timePrice in timePriceList)
                {
                    LmpTimePrice sendTimePrice = new LmpTimePrice();
                    sendTimePrice.MarketTime = timePrice.MarketTime;
                    NodePriceLibrary.LMP lmp = new NodePriceLibrary.LMP();
                    lmp.Price = double.NaN;
                    lmp.Congestion = double.NaN;
                    lmp.Loss = double.NaN;
                    sendTimePrice.Lmp = lmp;
                    string hourKey = sendTimePrice.MarketTime.ToString() + node.NodeId.ToString();
                    if (!dictRTLmpHash.ContainsKey(hourKey) || sendTimePrice.MarketTime > DateTime.Today)
                    {
                        sendTimePriceList.Add(sendTimePrice);
                    }
                }
                if (sendTimePriceList.Count > 0)
                {
                    sendNode.LmpTimePriceList = sendTimePriceList;
                    sendRtList.Add(sendNode);
                }
            }
            foreach (Node node in daNodeList)
            {
                Node sendNode = new Node();
                sendNode.Market = node.Market;
                sendNode.NodeId = node.NodeId;
                sendNode.NodeName = node.NodeName;
                sendNode.PNodeId = node.PNodeId;
                List<LmpTimePrice> timePriceList = node.LmpTimePriceList;
                List<LmpTimePrice> sendTimePriceList = new List<LmpTimePrice>();
                foreach (LmpTimePrice timePrice in timePriceList)
                {
                    LmpTimePrice sendTimePrice = new LmpTimePrice();
                    sendTimePrice.MarketTime = timePrice.MarketTime;
                    NodePriceLibrary.LMP lmp = new NodePriceLibrary.LMP();
                    lmp.Price = double.NaN;
                    lmp.Congestion = double.NaN;
                    lmp.Loss = double.NaN;
                    sendTimePrice.Lmp = lmp;
                    string hourKey = sendTimePrice.MarketTime.ToString() + node.NodeId.ToString();
                    if (!dictDALmpHash.ContainsKey(hourKey))
                    {
                        sendTimePriceList.Add(sendTimePrice);
                    }
                }
                if (sendTimePriceList.Count > 0)
                {
                    sendNode.LmpTimePriceList = sendTimePriceList;
                    sendDaList.Add(sendNode);
                }
            }
            Thread thread = null;
            Thread thread1 = null;
            if (sendDaList.Count > 0)
            {
                DARTThread range = new DARTThread(sendDaList, null, null, true, "hour", 0, DateTime.Today, DateTime.Today, dictDALmpHash);
                thread = new Thread(new ThreadStart(range.Run));
                thread.Start();
            }
            if (sendRtList.Count > 0)
            {
                DARTThread range = new DARTThread(sendRtList, null, null, false, "hour", 0, DateTime.Today, DateTime.Today, dictRTLmpHash);
                thread1 = new Thread(new ThreadStart(range.Run));
                thread1.Start();
            }
            if (thread != null)
            {
                thread.Join();
            }
            if (thread1 != null)
            {
                thread1.Join();
            }
            foreach (Node node in rtNodeList)
            {
                List<LmpTimePrice> timePriceList = node.LmpTimePriceList;
                foreach (LmpTimePrice timePrice in timePriceList)
                {
                    string key = timePrice.MarketTime.ToString() + node.NodeId.ToString();
                    if (dictRTLmpHash.ContainsKey(key))
                    {
                        timePrice.Lmp = dictRTLmpHash[key];
                    }
                }
            }
            foreach (Node node in daNodeList)
            {
                List<LmpTimePrice> timePriceList = node.LmpTimePriceList;
                foreach (LmpTimePrice timePrice in timePriceList)
                {
                    string key = timePrice.MarketTime.ToString() + node.NodeId.ToString();
                    if (dictDALmpHash.ContainsKey(key))
                    {
                        timePrice.Lmp = dictDALmpHash[key];
                    }
                }
            }
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="rtNodeList"></param>
       /// <param name="daNodeList"></param>
       /// <param name="startDate"></param>
       /// <param name="endDate"></param>
       /// <param name="onlyPrice"></param>
       /// <param name="marketKey"></param>
        public static void GetDartsForUptos(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, DateTime endDate, bool onlyPrice = true, int marketKey = 9)
        {
            DARTThread thread;
            if (onlyPrice)
            {
                thread = new DARTThread(daNodeList, dictDAHash, null, true, "uptos", marketKey, startDate, endDate, null, true);
                thread.Run();
                thread = new DARTThread(rtNodeList, dictRTHash, null, false, "uptos", marketKey, startDate, endDate, null, true);
                thread.Run();
            }
            else
            {
                thread = new DARTThread(daNodeList, dictDAHash, null, true, "uptos", marketKey, startDate, endDate, dictDALmpHash, false);
                thread.Run();
                new DARTThread(rtNodeList, dictRTHash, null, false, "uptos", marketKey, startDate, endDate, dictRTLmpHash, false).Run();
            }
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="nodeHash"></param>
       /// <param name="rtda"></param>
        public static void GetDartMarket(Dictionary<int, List<DateTime>> nodeHash, string rtda)
        {
            List<Node> rtNodeList = new List<Node>();
            List<Node> daNodeList = new List<Node>();
            List<int> nodeKeys = nodeHash.Keys.ToList<int>();
            foreach (int nodeKey in nodeKeys)
            {
                PricingNode node = DBAccess.GetNode(nodeKey);
                if (node == null)
                {
                    continue;
                }
                Node daNode = new Node();
                daNode.NodeId = nodeKey;
                daNode.PNodeId = node.ExternalNodeId;
                daNode.Market = node.MarketKey;
                Node rtNode = new Node();
                rtNode.NodeId = nodeKey;
                rtNode.PNodeId = node.ExternalNodeId;
                rtNode.Market = node.MarketKey;
                List<DateTime> marketDateList = nodeHash[nodeKey];
                List<TimePrice> daTimeList = new List<TimePrice>();
                List<TimePrice> rtTimeList = new List<TimePrice>();
                foreach (DateTime marketDate in marketDateList)
                {
                    string hourKey = marketDate.ToString() + nodeKey.ToString();
                    if (rtda == "da" || rtda == "both")
                    {
                        if (!dictDAHash.ContainsKey(hourKey))
                        {
                            TimePrice time = new TimePrice();
                            time.Price = double.NaN;
                            time.MarketTime = marketDate;
                            daTimeList.Add(time);
                        }
                    }
                    if (rtda == "rt" || rtda == "both")
                    {
                        if (DateTime.Today < marketDate)
                        {
                            double temp = 0;
                            dictRTHash.TryRemove(hourKey, out temp);
                        }
                        if (!dictRTHash.ContainsKey(hourKey))
                        {
                            TimePrice time = new TimePrice();
                            time.Price = double.NaN;
                            time.MarketTime = marketDate;
                            rtTimeList.Add(time);
                        }
                    }
                }
                if (daTimeList.Count > 0)
                {
                    daNode.TimePriceList = daTimeList;
                    daNodeList.Add(daNode);
                }
                if (rtTimeList.Count > 0)
                {
                    rtNode.TimePriceList = rtTimeList;
                    rtNodeList.Add(rtNode);
                }
            }
            Thread thread = null;
            Thread thread1 = null;
            if (daNodeList.Count > 0)
            {
                DARTThread range = new DARTThread(daNodeList, dictDAHash, null, true, "", 0, DateTime.Today, DateTime.Today, null);
                thread = new Thread(new ThreadStart(range.Run));
                thread.Start();
            }
            if (rtNodeList.Count > 0)
            {
                DARTThread range = new DARTThread(rtNodeList, dictRTHash, null, false, "", 0, DateTime.Today, DateTime.Today, null);
                thread1 = new Thread(new ThreadStart(range.Run));
                thread1.Start();
            }
            if (thread != null)
            {
                thread.Join();
            }
            if (thread1 != null)
            {
                thread1.Join();
            }
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="rtNodeList"></param>
       /// <param name="daNodeList"></param>
       /// <param name="startDate"></param>
       /// <param name="days"></param>
       /// <param name="onlyPrice"></param>
       /// <param name="fillDartList"></param>
        public static void GetDART(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, int days, bool onlyPrice, bool fillDartList)
        {
            GetDADART(rtNodeList, daNodeList, startDate, days, onlyPrice, fillDartList, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rtNodeList"></param>
        /// <param name="daNodeList"></param>
        /// <param name="startDate"></param>
        /// <param name="days"></param>
        /// <param name="onlyPrice"></param>
        /// <param name="fillDartList"></param>
        /// <param name="isOnlyDA"></param
        public static void GetDADART(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, int days, bool onlyPrice, bool fillDartList, bool isOnlyDA)
        {
            Console.WriteLine($"[START] GetDADART | startDate={startDate}, days={days}, onlyPrice={onlyPrice}, fillDartList={fillDartList}, isOnlyDA={isOnlyDA}");

            List<Node> sendRtList = new List<Node>();
            List<Node> sendDaList = new List<Node>();

            DateTime tempDate = SetToCST(startDate);
            Console.WriteLine($"Converted startDate to CST: {tempDate}");

            int startNum = isOnlyDA ? 1 : 0;

            for (int day = 0; day < days; day++)
            {
                Console.WriteLine($"--- Day Loop: day={day}, tempDate={tempDate} ---");

                for (int i = startNum; i < 2; i++)
                {
                    Console.WriteLine($"-- Market Loop: {(i == 0 ? "RT" : "DA")} --");

                    List<Node> nodeList = i == 0 ? rtNodeList : daNodeList;

                    foreach (Node tempNode in nodeList)
                    {
                        Console.WriteLine($"Processing Node: {tempNode.NodeId} ({tempNode.NodeName})");

                        Node node = new Node
                        {
                            Market = tempNode.Market,
                            NodeId = tempNode.NodeId,
                            NodeName = tempNode.NodeName,
                            PNodeId = tempNode.PNodeId
                        };

                        List<LmpTimePrice> daLmpTimeList = new List<LmpTimePrice>();
                        List<LmpTimePrice> rtLmpTimeList = new List<LmpTimePrice>();
                        List<TimePrice> daTimeList = new List<TimePrice>();
                        List<TimePrice> rtTimeList = new List<TimePrice>();

                        DateTime marketDate = tempDate;

                        for (int hour = 1; hour < 25; hour++)
                        {
                            marketDate = tempDate.AddHours(hour);
                            string hourKey = marketDate.ToString() + node.NodeId;

                            Console.WriteLine($"Hour={hour}, MarketDate={marketDate}, Key={hourKey}");

                            if (i == 1) // DA
                            {
                                if (onlyPrice)
                                {
                                    if (!dictDAHash.ContainsKey(hourKey))
                                    {
                                        Console.WriteLine($"[DA][MISS] Key not found: {hourKey}");

                                        daTimeList.Add(new TimePrice
                                        {
                                            MarketTime = marketDate,
                                            Price = double.NaN
                                        });
                                    }
                                }
                                else
                                {
                                    if (!dictDALmpHash.ContainsKey(hourKey))
                                    {
                                        Console.WriteLine($"[DA LMP][MISS] Key not found: {hourKey}");

                                        daLmpTimeList.Add(new LmpTimePrice
                                        {
                                            MarketTime = marketDate,
                                            Lmp = new NodePriceLibrary.LMP
                                            {
                                                Price = double.NaN,
                                                Loss = double.NaN,
                                                Congestion = double.NaN
                                            }
                                        });
                                    }
                                }
                            }
                            else // RT
                            {
                                if (DateTime.Today < marketDate)
                                {
                                    Console.WriteLine($"[RT][FUTURE DATA REMOVAL] {hourKey}");

                                    if (onlyPrice)
                                    {
                                        double temp = 0;
                                        dictRTHash.TryRemove(hourKey, out temp);
                                    }
                                    else
                                    {
                                        NodePriceLibrary.LMP temp = new NodePriceLibrary.LMP();
                                        dictRTLmpHash.TryRemove(hourKey, out temp);
                                    }
                                }

                                if (onlyPrice)
                                {
                                    if (!dictRTHash.ContainsKey(hourKey))
                                    {
                                        Console.WriteLine($"[RT][MISS] Key not found: {hourKey}");

                                        rtTimeList.Add(new TimePrice
                                        {
                                            MarketTime = marketDate,
                                            Price = double.NaN
                                        });
                                    }
                                }
                                else
                                {
                                    if (!dictRTLmpHash.ContainsKey(hourKey))
                                    {
                                        Console.WriteLine($"[RT LMP][MISS] Key not found: {hourKey}");

                                        rtLmpTimeList.Add(new LmpTimePrice
                                        {
                                            MarketTime = marketDate,
                                            Lmp = new NodePriceLibrary.LMP
                                            {
                                                Price = double.NaN,
                                                Loss = double.NaN,
                                                Congestion = double.NaN
                                            }
                                        });
                                    }
                                }
                            }
                        }

                        // Add to send lists
                        if (i == 0)
                        {
                            if ((onlyPrice && rtTimeList.Count > 0) || (!onlyPrice && rtLmpTimeList.Count > 0))
                            {
                                Console.WriteLine($"Adding RT node: {node.NodeId}");
                                node.TimePriceList = rtTimeList;
                                node.LmpTimePriceList = rtLmpTimeList;
                                sendRtList.Add(node);
                            }
                        }
                        else
                        {
                            if ((onlyPrice && daTimeList.Count > 0) || (!onlyPrice && daLmpTimeList.Count > 0))
                            {
                                Console.WriteLine($"Adding DA node: {node.NodeId}");
                                node.TimePriceList = daTimeList;
                                node.LmpTimePriceList = daLmpTimeList;
                                sendDaList.Add(node);
                            }
                        }
                    }
                }

                tempDate = tempDate.AddDays(1);
            }

            Console.WriteLine($"Prepared sendDaList={sendDaList.Count}, sendRtList={sendRtList.Count}");

            Thread thread = null;
            Thread thread1 = null;

            if (sendDaList.Count > 0)
            {
                Console.WriteLine("Starting DA thread...");

                DARTThread range = onlyPrice
                    ? new DARTThread(sendDaList, dictDAHash, null, true, "hour", 0, startDate, startDate.AddDays(days), null)
                    : new DARTThread(sendDaList, null, null, true, "hour", 0, startDate, startDate.AddDays(days), dictDALmpHash);

                thread = new Thread(new ThreadStart(range.Run));
                thread.Start();
            }

            if (sendRtList.Count > 0)
            {
                Console.WriteLine("Starting RT thread...");

                DARTThread range = onlyPrice
                    ? new DARTThread(sendRtList, dictRTHash, null, false, "hour", 0, startDate, startDate.AddDays(days), null)
                    : new DARTThread(sendRtList, null, null, false, "hour", 0, startDate, startDate.AddDays(days), dictRTLmpHash);

                thread1 = new Thread(new ThreadStart(range.Run));
                thread1.Start();
            }

            thread?.Join();
            thread1?.Join();

            Console.WriteLine("Threads completed.");

            if (fillDartList)
            {
                Console.WriteLine("Calling SetDaRtList...");
                SetDaRtList(startDate, days, onlyPrice, daNodeList, rtNodeList);
            }

            Console.WriteLine("[END] GetDADART");
        }
        // public static void GetDADART(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, int days, bool onlyPrice, bool fillDartList, bool isOnlyDA)
        // {

        //     List<Node> sendRtList = new List<Node>();
        //     List<Node> sendDaList = new List<Node>();
        //     DateTime tempDate = SetToCST(startDate);
        //     int startNum = isOnlyDA ? 1 : 0;
        //     for (int day = 0; day < days; day++)
        //     {
        //         for (int i = startNum; i < 2; i++)
        //         {
        //             List<Node> nodeList = i == 0 ? rtNodeList : daNodeList;
        //             foreach (Node tempNode in nodeList)
        //             {
        //                 Node node = new Node();
        //                 node.Market = tempNode.Market;
        //                 node.NodeId = tempNode.NodeId;
        //                 node.NodeName = tempNode.NodeName;
        //                 node.PNodeId = tempNode.PNodeId;
        //                 List<LmpTimePrice> daLmpTimeList = new List<LmpTimePrice>();
        //                 List<LmpTimePrice> rtLmpTimeList = new List<LmpTimePrice>();
        //                 List<TimePrice> daTimeList = new List<TimePrice>();
        //                 List<TimePrice> rtTimeList = new List<TimePrice>();
        //                 DateTime marketDate = tempDate;
        //                 for (int hour = 1; hour < 25; hour++)
        //                 {
        //                     marketDate = tempDate.AddHours(hour);
        //                     string hourKey = marketDate.ToString() + node.NodeId.ToString();
        //                     if (i == 1)
        //                     {
        //                         if (onlyPrice)
        //                         {
        //                             if (!dictDAHash.ContainsKey(hourKey))
        //                             {
        //                                 TimePrice time = new TimePrice();
        //                                 time.MarketTime = marketDate;
        //                                 time.Price = double.NaN;
        //                                 daTimeList.Add(time);
        //                             }
        //                         }
        //                         else
        //                         {
        //                             if (!dictDALmpHash.ContainsKey(hourKey))
        //                             {
        //                                 LmpTimePrice time = new LmpTimePrice();
        //                                 NodePriceLibrary.LMP lmp = new NodePriceLibrary.LMP();
        //                                 lmp.Price = double.NaN;
        //                                 lmp.Loss = double.NaN;
        //                                 lmp.Congestion = double.NaN;
        //                                 time.Lmp = lmp;
        //                                 time.MarketTime = marketDate;
        //                                 daLmpTimeList.Add(time);
        //                             }
        //                         }
        //                     }
        //                     else
        //                     {
        //                         if (DateTime.Today < marketDate)
        //                         {
        //                             if (onlyPrice)
        //                             {
        //                                 double temp = 0;
        //                                 dictRTHash.TryRemove(hourKey, out temp);
        //                             }
        //                             else
        //                             {
        //                                 NodePriceLibrary.LMP temp = new NodePriceLibrary.LMP();
        //                                 dictRTLmpHash.TryRemove(hourKey, out temp);
        //                             }
        //                         }
        //                         if (onlyPrice)
        //                         {
        //                             if (!dictRTHash.ContainsKey(hourKey))
        //                             {
        //                                 TimePrice time = new TimePrice();
        //                                 time.MarketTime = marketDate;
        //                                 time.Price = double.NaN;
        //                                 rtTimeList.Add(time);
        //                             }
        //                         }
        //                         else
        //                         {
        //                             if (!dictRTLmpHash.ContainsKey(hourKey))
        //                             {
        //                                 LmpTimePrice time = new LmpTimePrice();
        //                                 NodePriceLibrary.LMP lmp = new NodePriceLibrary.LMP();
        //                                 lmp.Price = double.NaN;
        //                                 lmp.Loss = double.NaN;
        //                                 lmp.Congestion = double.NaN;
        //                                 time.Lmp = lmp;
        //                                 time.MarketTime = marketDate;
        //                                 rtLmpTimeList.Add(time);
        //                             }
        //                         }
        //                     }
        //                 }
        //                 if (i == 0)
        //                 {
        //                     if (onlyPrice)
        //                     {
        //                         if (rtTimeList.Count > 0)
        //                         {
        //                             node.TimePriceList = rtTimeList;
        //                             sendRtList.Add(node);
        //                         }
        //                     }
        //                     else
        //                     {
        //                         if (rtLmpTimeList.Count > 0)
        //                         {
        //                             node.LmpTimePriceList = rtLmpTimeList;
        //                             sendRtList.Add(node);
        //                         }
        //                     }
        //                 }
        //                 else
        //                 {
        //                     if (onlyPrice)
        //                     {
        //                         if (daTimeList.Count > 0)
        //                         {
        //                             node.TimePriceList = daTimeList;
        //                             sendDaList.Add(node);
        //                         }
        //                     }
        //                     else
        //                     {
        //                         if (daLmpTimeList.Count > 0)
        //                         {
        //                             node.LmpTimePriceList = daLmpTimeList;
        //                             sendDaList.Add(node);
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //         tempDate = tempDate.AddDays(1);
        //     }


        //     Thread thread = null;
        //     Thread thread1 = null;
        //     if (sendDaList.Count > 0)
        //     {
        //         DARTThread range = null;
        //         if (onlyPrice)
        //         {
        //             range = new DARTThread(sendDaList, dictDAHash, null, true, "hour", 0, startDate, startDate.AddDays(days), null);
        //         }
        //         else
        //         {
        //             range = new DARTThread(sendDaList, null, null, true, "hour", 0, startDate, startDate.AddDays(days), dictDALmpHash);
        //         } 
        //         thread = new Thread(new ThreadStart(range.Run));
        //         thread.Start();
        //     }
        //     if (sendRtList.Count > 0)
        //     {
        //         DARTThread range = null;
        //         if (onlyPrice)
        //         {
        //             range = new DARTThread(sendRtList, dictRTHash, null, false, "hour", 0, startDate, startDate.AddDays(days), null);
        //         }
        //         else
        //         {
        //             range = new DARTThread(sendRtList, null, null, false, "hour", 0, startDate, startDate.AddDays(days), dictRTLmpHash);
        //         } 
        //         thread1 = new Thread(new ThreadStart(range.Run));
        //         thread1.Start();
        //     } 
        //     if (thread != null)
        //     {
        //         thread.Join();
        //     }
        //     if (thread1 != null)
        //     {
        //         thread1.Join();
        //     }
        //     if (fillDartList)
        //     {
        //         SetDaRtList(startDate, days, onlyPrice, daNodeList, rtNodeList);
        //     }
        // }
        ///// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="days"></param>
        /// <param name="onlyPrice"></param>
        /// <param name="daNodeList"></param>
        /// <param name="rtNodeList"></param>
        private static void SetDaRtList(DateTime startDate, int days, bool onlyPrice, List<Node> daNodeList, List<Node> rtNodeList)
        {
            DateTime tempDate = startDate;
            foreach (Node node in rtNodeList)
            {
                tempDate = startDate;
                node.TimePriceList = new List<TimePrice>();
                node.LmpTimePriceList = new List<LmpTimePrice>();
                for (int day = 0; day < days; day++)
                {
                    DateTime marketDate = tempDate;
                    for (int hour = 1; hour < 25; hour++)
                    {
                        marketDate = tempDate.AddHours(hour);
                        string hourKey = marketDate.ToString() + node.NodeId.ToString();
                        if (onlyPrice)
                        {
                            TimePrice time = new TimePrice();
                            time.Price = double.NaN;
                            time.MarketTime = marketDate;
                            if (dictRTHash.ContainsKey(hourKey))
                            {
                                time.Price = dictRTHash[hourKey];
                            }
                            node.TimePriceList.Add(time);
                        }
                        else
                        {
                            LmpTimePrice time = new LmpTimePrice();
                            NodePriceLibrary.LMP lmp = new NodePriceLibrary.LMP();
                            lmp.Price = double.NaN;
                            lmp.Congestion = double.NaN;
                            lmp.Loss = double.NaN;
                            time.MarketTime = marketDate;
                            if (dictRTLmpHash.ContainsKey(hourKey))
                            {
                                time.Lmp = dictRTLmpHash[hourKey];
                            }
                            node.LmpTimePriceList.Add(time);
                        }
                    }
                    tempDate = tempDate.AddDays(1);
                }
            }
            foreach (Node node in daNodeList)
            {
                tempDate = startDate;
                node.TimePriceList = new List<TimePrice>();
                node.LmpTimePriceList = new List<LmpTimePrice>();
                for (int day = 0; day < days; day++)
                {
                    DateTime marketDate = tempDate;
                    for (int hour = 1; hour < 25; hour++)
                    {
                        marketDate = tempDate.AddHours(hour);
                        string hourKey = marketDate.ToString() + node.NodeId.ToString();
                        if (onlyPrice)
                        {
                            TimePrice time = new TimePrice();
                            time.Price = double.NaN;
                            time.MarketTime = marketDate;
                            if (dictDAHash.ContainsKey(hourKey))
                            {
                                time.Price = dictDAHash[hourKey];
                            }
                            node.TimePriceList.Add(time);
                        }
                        else
                        {
                            LmpTimePrice time = new LmpTimePrice();
                            NodePriceLibrary.LMP lmp = new NodePriceLibrary.LMP();
                            lmp.Price = double.NaN;
                            lmp.Congestion = double.NaN;
                            lmp.Loss = double.NaN;
                            time.MarketTime = marketDate;
                            if (dictDALmpHash.ContainsKey(hourKey))
                            {
                                time.Lmp = dictDALmpHash[hourKey];
                            }
                            node.LmpTimePriceList.Add(time);
                        }
                    }
                    tempDate = tempDate.AddDays(1);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        private static DateTime SetToCST(DateTime datetime)
        {
            TimeZone zone = TimeZone.CurrentTimeZone;
            string zoneName = zone.StandardName;
            DateTime CSTLocal = datetime;

            //if (zone.StandardName.StartsWith("Mountain"))
            //{
            //    CSTLocal = datetime.AddHours(-1);
            //}
            //else if (zone.StandardName.StartsWith("East"))
            //{
            //    CSTLocal = datetime.AddHours(1);
            //}
            //else if (zone.StandardName.StartsWith("Pacific"))
            //{
            //    CSTLocal = datetime.AddHours(-2);
            //}

            DateTime dt = new DateTime(CSTLocal.Ticks, DateTimeKind.Unspecified);
            return dt;
        }
    }
}
