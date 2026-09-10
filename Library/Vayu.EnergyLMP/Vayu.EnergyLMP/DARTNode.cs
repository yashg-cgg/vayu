using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vayu.DBLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.EnergyLMP
{
    /// <summary>
    /// 
    /// </summary>
    public static class DARTNode
    {
        /// <summary>
        /// The s rt hash
        /// </summary>
        public static ConcurrentDictionary<string, double> sRTHash = new ConcurrentDictionary<string, double>();
        /// <summary>
        /// The s da hash
        /// </summary>
        public static ConcurrentDictionary<string, double> sDAHash = new ConcurrentDictionary<string, double>();
        /// <summary>
        /// The s rt LMP hash
        /// </summary>
        public static ConcurrentDictionary<string, Vayu.NodePriceLibrary.LMP> sRTLmpHash = new ConcurrentDictionary<string, Vayu.NodePriceLibrary.LMP>();
        /// <summary>
        /// The s da LMP hash
        /// </summary>
        public static ConcurrentDictionary<string, Vayu.NodePriceLibrary.LMP> sDALmpHash = new ConcurrentDictionary<string, Vayu.NodePriceLibrary.LMP>();
        /// <summary>
        /// The s all date hash
        /// </summary>
        public static ConcurrentDictionary<int, Tuple<DateTime, DateTime>> sAllDateHash = new ConcurrentDictionary<int, Tuple<DateTime, DateTime>>();
        /// <summary>
        /// The s rt five minimum hash
        /// </summary>
        public static ConcurrentDictionary<string, ConcurrentDictionary<int, double>> sRTFiveMinHash = new ConcurrentDictionary<string, ConcurrentDictionary<int, double>>();

        /// <summary>
        /// Gets all five minimum darts.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        public static void GetAllFiveMinDarts(int marketKey, DateTime startDate, DateTime endDate)
        {
            DARTThread thread = new DARTThread(null, null, sRTFiveMinHash, false, "fivemin", marketKey, startDate, endDate, null);
            thread.Run();
        }
        /// <summary>
        /// Gets all darts.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        public static void GetAllDarts(int marketKey, DateTime startDate, DateTime endDate)
        {
            Tuple<DateTime, DateTime> tuple = null;
            if (sAllDateHash.ContainsKey(marketKey))
            {
                tuple = sAllDateHash[marketKey];
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
                if (!sAllDateHash.ContainsKey(marketKey) || found)
                {
                    DARTThread range = new DARTThread(null, sDAHash, null, true, "all", marketKey, startDate, endDate, null);
                    Thread thread = new Thread(new ThreadStart(range.Run));
                    thread.Start();
                    range = new DARTThread(null, sRTHash, null, false, "all", marketKey, startDate, endDate, null);
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
                    if (!sAllDateHash.ContainsKey(marketKey))
                    {
                        tuple = new Tuple<DateTime, DateTime>(startDate, endDate);
                        sAllDateHash.TryAdd(marketKey, tuple);
                        break;
                    }
                    else
                    {
                        DateTime tempStartDate = startDate < tuple.Item1 ? startDate : tuple.Item1;
                        DateTime tempEndtDate = endDate > tuple.Item2 ? endDate : tuple.Item2;
                        tuple = new Tuple<DateTime, DateTime>(tempStartDate, tempEndtDate);
                        sAllDateHash[marketKey] = tuple;
                    }
                }
                else if (startDate == DateTime.Today && i == 0)
                {
                    DARTThread range = new DARTThread(null, sRTHash, null, false, "all", marketKey, startDate, endDate, null);
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
        /// Gets the dart for hour.
        /// </summary>
        /// <param name="rtNodeList">The rt node list.</param>
        /// <param name="daNodeList">The da node list.</param>
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
                    Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                    lmp.Price = double.NaN;
                    lmp.Congestion = double.NaN;
                    lmp.Loss = double.NaN;
                    sendTimePrice.Lmp = lmp;
                    string hourKey = sendTimePrice.MarketTime.ToString() + node.NodeId.ToString();
                    if (!sRTLmpHash.ContainsKey(hourKey) || sendTimePrice.MarketTime > DateTime.Today)
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
                    Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                    lmp.Price = double.NaN;
                    lmp.Congestion = double.NaN;
                    lmp.Loss = double.NaN;
                    sendTimePrice.Lmp = lmp;
                    string hourKey = sendTimePrice.MarketTime.ToString() + node.NodeId.ToString();
                    if (!sDALmpHash.ContainsKey(hourKey))
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
                DARTThread range = new DARTThread(sendDaList, null, null, true, "hour", 0, DateTime.Today, DateTime.Today, sDALmpHash);
                thread = new Thread(new ThreadStart(range.Run));
                thread.Start();
            }
            if (sendRtList.Count > 0)
            {
                DARTThread range = new DARTThread(sendRtList, null, null, false, "hour", 0, DateTime.Today, DateTime.Today, sRTLmpHash);
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
                    if (sRTLmpHash.ContainsKey(key))
                    {
                        timePrice.Lmp = sRTLmpHash[key];
                    }
                }
            }
            foreach (Node node in daNodeList)
            {
                List<LmpTimePrice> timePriceList = node.LmpTimePriceList;
                foreach (LmpTimePrice timePrice in timePriceList)
                {
                    string key = timePrice.MarketTime.ToString() + node.NodeId.ToString();
                    if (sDALmpHash.ContainsKey(key))
                    {
                        timePrice.Lmp = sDALmpHash[key];
                    }
                }
            }
        }
        /// <summary>
        /// Gets the darts for uptos.
        /// </summary>
        /// <param name="rtNodeList">The rt node list.</param>
        /// <param name="daNodeList">The da node list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        public static void GetDartsForUptos(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, DateTime endDate, int marketKey = 9)
        {
            DARTThread range = new DARTThread(daNodeList, sDAHash, null, true, "uptos", marketKey, startDate, endDate, null);
            range.Run();
            range = new DARTThread(rtNodeList, sRTHash, null, false, "uptos", marketKey, startDate, endDate, null);
            range.Run();
        }
        /// <summary>
        /// Gets the dart market.
        /// </summary>
        /// <param name="nodeHash">The node hash.</param>
        /// <param name="rtda">The rtda.</param>
        public static void GetDartMarket(Dictionary<int, List<DateTime>> nodeHash, string rtda, int market, bool onlyPrice = true)
        {
            List<Node> rtNodeList = new List<Node>();
            List<Node> daNodeList = new List<Node>();
            List<int> nodeKeys = nodeHash.Keys.ToList<int>();
            sDALmpHash.Clear();
            sRTLmpHash.Clear();
            foreach (int nodeKey in nodeKeys)
            {
                PricingNode node = DBAccess.GetNode(nodeKey, market);
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
                List<LmpTimePrice> daLMPTimePriceList = new List<LmpTimePrice>();
                List<LmpTimePrice> rtLMPTimePriceList = new List<LmpTimePrice>();
                foreach (DateTime marketDate in marketDateList)
                {
                    if (onlyPrice)
                    {
                        string hourKey = marketDate.ToString() + nodeKey.ToString();
                        if (rtda == "da" || rtda == "both")
                        {
                            if (!sDAHash.ContainsKey(hourKey))
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
                                sRTHash.TryRemove(hourKey, out temp);
                            }
                            if (!sRTHash.ContainsKey(hourKey))
                            {
                                TimePrice time = new TimePrice();
                                time.Price = double.NaN;
                                time.MarketTime = marketDate;
                                rtTimeList.Add(time);
                            }
                        }
                    }
                    else
                    {
                        string hourKey = marketDate.ToString() + nodeKey.ToString();
                        if (rtda == "da" || rtda == "both")
                        {
                            if (!sDAHash.ContainsKey(hourKey))
                            {
                                LmpTimePrice time = new LmpTimePrice();
                                //  time.Lmp. = double.NaN;
                                time.MarketTime = marketDate;
                                daLMPTimePriceList.Add(time);
                            }
                            else
                            {
                                LmpTimePrice time = new LmpTimePrice();
                                //  time.Lmp. = double.NaN;
                                time.MarketTime = marketDate;
                                daLMPTimePriceList.Add(time);
                            }
                        }
                        if (rtda == "rt" || rtda == "both")
                        {
                            if (DateTime.Today < marketDate)
                            {
                                double temp = 0;
                                sRTHash.TryRemove(hourKey, out temp);
                            }
                            if (!sRTHash.ContainsKey(hourKey))
                            {
                                LmpTimePrice time = new LmpTimePrice();
                                // time.Price = double.NaN;
                                time.MarketTime = marketDate;
                                rtLMPTimePriceList.Add(time);
                            }
                            else
                            {
                                LmpTimePrice time = new LmpTimePrice();
                                // time.Price = double.NaN;
                                time.MarketTime = marketDate;
                                rtLMPTimePriceList.Add(time);
                            }
                        }
                    }
                }
                if (onlyPrice)
                {
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
                else
                {
                    if (daLMPTimePriceList.Count > 0)
                    {
                        daNode.LmpTimePriceList = daLMPTimePriceList;
                        daNodeList.Add(daNode);
                    }
                    if (rtLMPTimePriceList.Count > 0)
                    {
                        rtNode.LmpTimePriceList = rtLMPTimePriceList;
                        rtNodeList.Add(rtNode);
                    }
                }
            }
            Thread thread = null;
            Thread thread1 = null;
            if (onlyPrice)
            {
                if (daNodeList.Count > 0)
                {
                    DARTThread range = new DARTThread(daNodeList, sDAHash, null, true, "", 0, DateTime.Today, DateTime.Today, null);
                    thread = new Thread(new ThreadStart(range.Run));
                    thread.Start();
                }
                if (rtNodeList.Count > 0)
                {
                    DARTThread range = new DARTThread(rtNodeList, sRTHash, null, false, "", 0, DateTime.Today, DateTime.Today, null);
                    thread1 = new Thread(new ThreadStart(range.Run));
                    thread1.Start();
                }
            }
            else
            {
                if (daNodeList.Count > 0)
                {
                    DARTThread range = new DARTThread(daNodeList, null, null, true, "", 0, DateTime.Today, DateTime.Today, sDALmpHash, false);
                    thread = new Thread(new ThreadStart(range.Run));
                    thread.Start();
                }
                if (rtNodeList.Count > 0)
                {
                    DARTThread range = new DARTThread(rtNodeList, null, null, false, "", 0, DateTime.Today, DateTime.Today, sRTLmpHash, false);
                    thread1 = new Thread(new ThreadStart(range.Run));
                    thread1.Start();
                }
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
        /// Gets the dart.
        /// </summary>
        /// <param name="rtNodeList">The rt node list.</param>
        /// <param name="daNodeList">The da node list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="days">The days.</param>
        /// <param name="onlyPrice">if set to <c>true</c> [only price].</param>
        /// <param name="fillDartList">if set to <c>true</c> [fill dart list].</param>



        public static void GetDART(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, int days, bool onlyPrice, bool fillDartList)
        {
            GetDADART(rtNodeList, daNodeList, startDate, days, onlyPrice, fillDartList, false);
        }
        /// <summary>
        /// Gets the dadart.
        /// </summary>
        /// <param name="rtNodeList">The rt node list.</param>
        /// <param name="daNodeList">The da node list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="days">The days.</param>
        /// <param name="onlyPrice">if set to <c>true</c> [only price].</param>
        /// <param name="fillDartList">if set to <c>true</c> [fill dart list].</param>
        /// <param name="isOnlyDA">if set to <c>true</c> [is only da].</param>
        public static void GetDADART(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, int days, bool onlyPrice, bool fillDartList, bool isOnlyDA)
        {
            List<Node> sendRtList = new List<Node>();
            List<Node> sendDaList = new List<Node>();
            DateTime tempDate = SetToCST(startDate);
            int startNum = isOnlyDA ? 1 : 0;
            for (int day = 0; day < days; day++)
            {
                for (int i = startNum; i < 2; i++)
                {
                    List<Node> nodeList = i == 0 ? rtNodeList : daNodeList;
                    foreach (Node tempNode in nodeList)
                    {
                        Node node = new Node();
                        node.Market = tempNode.Market;
                        node.NodeId = tempNode.NodeId;
                        node.NodeName = tempNode.NodeName;
                        node.PNodeId = tempNode.PNodeId;
                        List<LmpTimePrice> daLmpTimeList = new List<LmpTimePrice>();
                        List<LmpTimePrice> rtLmpTimeList = new List<LmpTimePrice>();
                        List<TimePrice> daTimeList = new List<TimePrice>();
                        List<TimePrice> rtTimeList = new List<TimePrice>();
                        DateTime marketDate = tempDate;
                        for (int hour = 1; hour < 25; hour++)
                        {
                            marketDate = tempDate.AddHours(hour);
                            string hourKey = marketDate.ToString() + node.NodeId.ToString();
                            if (i == 1)
                            {
                                if (onlyPrice)
                                {
                                    if (!sDAHash.ContainsKey(hourKey))
                                    {
                                        TimePrice time = new TimePrice();
                                        time.MarketTime = marketDate;
                                        time.Price = double.NaN;
                                        daTimeList.Add(time);
                                    }
                                }
                                else
                                {
                                    if (!sDALmpHash.ContainsKey(hourKey))
                                    {
                                        LmpTimePrice time = new LmpTimePrice();
                                        Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                                        lmp.Price = double.NaN;
                                        lmp.Loss = double.NaN;
                                        lmp.Congestion = double.NaN;
                                        time.Lmp = lmp;
                                        time.MarketTime = marketDate;
                                        daLmpTimeList.Add(time);
                                    }
                                }
                            }
                            else
                            {
                                if (DateTime.Today < marketDate)
                                {
                                    if (onlyPrice)
                                    {
                                        double temp = 0;
                                        sRTHash.TryRemove(hourKey, out temp);
                                    }
                                    else
                                    {
                                        Vayu.NodePriceLibrary.LMP temp = new Vayu.NodePriceLibrary.LMP();
                                        sRTLmpHash.TryRemove(hourKey, out temp);
                                    }
                                }
                                if (onlyPrice)
                                {
                                    if (!sRTHash.ContainsKey(hourKey))
                                    {
                                        TimePrice time = new TimePrice();
                                        time.MarketTime = marketDate;
                                        time.Price = double.NaN;
                                        rtTimeList.Add(time);
                                    }
                                }
                                else
                                {
                                    if (!sRTLmpHash.ContainsKey(hourKey))
                                    {
                                        LmpTimePrice time = new LmpTimePrice();
                                        Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                                        lmp.Price = double.NaN;
                                        lmp.Loss = double.NaN;
                                        lmp.Congestion = double.NaN;
                                        time.Lmp = lmp;
                                        time.MarketTime = marketDate;
                                        rtLmpTimeList.Add(time);
                                    }
                                }
                            }
                        }
                        if (i == 0)
                        {
                            if (onlyPrice)
                            {
                                if (rtTimeList.Count > 0)
                                {
                                    node.TimePriceList = rtTimeList;
                                    sendRtList.Add(node);
                                }
                            }
                            else
                            {
                                if (rtLmpTimeList.Count > 0)
                                {
                                    node.LmpTimePriceList = rtLmpTimeList;
                                    sendRtList.Add(node);
                                }
                            }
                        }
                        else
                        {
                            if (onlyPrice)
                            {
                                if (daTimeList.Count > 0)
                                {
                                    node.TimePriceList = daTimeList;
                                    sendDaList.Add(node);
                                }
                            }
                            else
                            {
                                if (daLmpTimeList.Count > 0)
                                {
                                    node.LmpTimePriceList = daLmpTimeList;
                                    sendDaList.Add(node);
                                }
                            }
                        }
                    }
                }
                tempDate = tempDate.AddDays(1);
            }
            Thread[] threads = new Thread[2];
            if (sendDaList.Count > 0)
            {
                DARTThread range = null;
                if (onlyPrice)
                {
                    range = new DARTThread(sendDaList, sDAHash, null, true, "hour", 0, startDate, startDate.AddDays(days), null);
                }
                else
                {
                    range = new DARTThread(sendDaList, null, null, true, "hour", 0, startDate, startDate.AddDays(days), sDALmpHash);
                }
                threads[0] = new Thread(new ThreadStart(range.Run));
                threads[0].Start();
            }
            if (sendRtList.Count > 0)
            {
                DARTThread range = null;
                if (onlyPrice)
                {
                    range = new DARTThread(sendRtList, sRTHash, null, false, "hour", 0, startDate, startDate.AddDays(days), null);
                }
                else
                {
                    range = new DARTThread(sendRtList, null, null, false, "hour", 0, startDate, startDate.AddDays(days), sRTLmpHash);
                }
                threads[1] = new Thread(new ThreadStart(range.Run));
                threads[1].Start();
            }
            if (threads[0] != null)
            {
                threads[0].Join();
            }
            if (threads[1] != null)
            {
                threads[1].Join();
            }
            if (fillDartList)
            {
                SetDaRtList(startDate, days, onlyPrice, daNodeList, rtNodeList);
            }
        }

        public static void GetDADARTForExternal(List<Node> rtNodeList, List<Node> daNodeList, DateTime startDate, int days, bool onlyPrice, bool fillDartList, bool isOnlyDA)
        {
            List<Node> sendRtList = new List<Node>();
            List<Node> sendDaList = new List<Node>();
            startDate = startDate.AddMonths(-2);
            DateTime tempDate = SetToCST(startDate);
            int startNum = isOnlyDA ? 1 : 0;
            for (int day = 0; day < days; day++)
            {
                for (int i = startNum; i < 2; i++)
                {
                    List<Node> nodeList = i == 0 ? rtNodeList : daNodeList;
                    foreach (Node tempNode in nodeList)
                    {
                        Node node = new Node();
                        node.Market = tempNode.Market;
                        node.NodeId = tempNode.NodeId;
                        node.NodeName = tempNode.NodeName;
                        node.PNodeId = tempNode.PNodeId;
                        List<LmpTimePrice> daLmpTimeList = new List<LmpTimePrice>();
                        List<LmpTimePrice> rtLmpTimeList = new List<LmpTimePrice>();
                        List<TimePrice> daTimeList = new List<TimePrice>();
                        List<TimePrice> rtTimeList = new List<TimePrice>();
                        DateTime marketDate = tempDate;
                        for (int hour = 1; hour < 25; hour++)
                        {
                            marketDate = tempDate.AddHours(hour);
                            string hourKey = marketDate.ToString() + node.NodeId.ToString();
                            if (i == 1)
                            {
                                if (onlyPrice)
                                {
                                    if (!sDAHash.ContainsKey(hourKey))
                                    {
                                        TimePrice time = new TimePrice();
                                        time.MarketTime = marketDate;
                                        time.Price = double.NaN;
                                        daTimeList.Add(time);
                                    }
                                }
                                else
                                {
                                    if (!sDALmpHash.ContainsKey(hourKey))
                                    {
                                        LmpTimePrice time = new LmpTimePrice();
                                        Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                                        lmp.Price = double.NaN;
                                        lmp.Loss = double.NaN;
                                        lmp.Congestion = double.NaN;
                                        time.Lmp = lmp;
                                        time.MarketTime = marketDate;
                                        daLmpTimeList.Add(time);
                                    }
                                }
                            }
                            else
                            {
                                if (DateTime.Today < marketDate)
                                {
                                    if (onlyPrice)
                                    {
                                        double temp = 0;
                                        sRTHash.TryRemove(hourKey, out temp);
                                    }
                                    else
                                    {
                                        Vayu.NodePriceLibrary.LMP temp = new Vayu.NodePriceLibrary.LMP();
                                        sRTLmpHash.TryRemove(hourKey, out temp);
                                    }
                                }
                                if (onlyPrice)
                                {
                                    if (!sRTHash.ContainsKey(hourKey))
                                    {
                                        TimePrice time = new TimePrice();
                                        time.MarketTime = marketDate;
                                        time.Price = double.NaN;
                                        rtTimeList.Add(time);
                                    }
                                }
                                else
                                {
                                    if (!sRTLmpHash.ContainsKey(hourKey))
                                    {
                                        LmpTimePrice time = new LmpTimePrice();
                                        Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                                        lmp.Price = double.NaN;
                                        lmp.Loss = double.NaN;
                                        lmp.Congestion = double.NaN;
                                        time.Lmp = lmp;
                                        time.MarketTime = marketDate;
                                        rtLmpTimeList.Add(time);
                                    }
                                }
                            }
                        }
                        if (i == 0)
                        {
                            if (onlyPrice)
                            {
                                if (rtTimeList.Count > 0)
                                {
                                    node.TimePriceList = rtTimeList;
                                    sendRtList.Add(node);
                                }
                            }
                            else
                            {
                                if (rtLmpTimeList.Count > 0)
                                {
                                    node.LmpTimePriceList = rtLmpTimeList;
                                    sendRtList.Add(node);
                                }
                            }
                        }
                        else
                        {
                            if (onlyPrice)
                            {
                                if (daTimeList.Count > 0)
                                {
                                    node.TimePriceList = daTimeList;
                                    sendDaList.Add(node);
                                }
                            }
                            else
                            {
                                if (daLmpTimeList.Count > 0)
                                {
                                    node.LmpTimePriceList = daLmpTimeList;
                                    sendDaList.Add(node);
                                }
                            }
                        }
                    }
                }
                tempDate = tempDate.AddDays(1);
            }
            Thread[] threads = new Thread[2];
            if (sendDaList.Count > 0)
            {
                DARTThread range = null;
                if (onlyPrice)
                {
                    range = new DARTThread(sendDaList, sDAHash, null, true, "hour", 0, startDate, startDate.AddDays(days), null);
                }
                else
                {
                    range = new DARTThread(sendDaList, null, null, true, "hour", 0, startDate, startDate.AddDays(days), sDALmpHash);
                }
                threads[0] = new Thread(new ThreadStart(range.Run));
                threads[0].Start();
            }
            if (sendRtList.Count > 0)
            {
                DARTThread range = null;
                if (onlyPrice)
                {
                    range = new DARTThread(sendRtList, sRTHash, null, false, "hour", 0, startDate, startDate.AddDays(days), null);
                }
                else
                {
                    range = new DARTThread(sendRtList, null, null, false, "hour", 0, startDate, startDate.AddDays(days), sRTLmpHash);
                }
                threads[1] = new Thread(new ThreadStart(range.Run));
                threads[1].Start();
            }
            if (threads[0] != null)
            {
                threads[0].Join();
            }
            if (threads[1] != null)
            {
                threads[1].Join();
            }
            if (fillDartList)
            {
                SetDaRtList(startDate, days, onlyPrice, daNodeList, rtNodeList);
            }
        }
        /// <summary>
        /// Sets the da rt list.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="days">The days.</param>
        /// <param name="onlyPrice">if set to <c>true</c> [only price].</param>
        /// <param name="daNodeList">The da node list.</param>
        /// <param name="rtNodeList">The rt node list.</param>
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
                            if (sRTHash.ContainsKey(hourKey))
                            {
                                time.Price = sRTHash[hourKey];
                            }
                            node.TimePriceList.Add(time);
                        }
                        else
                        {
                            LmpTimePrice time = new LmpTimePrice();
                            Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                            lmp.Price = double.NaN;
                            lmp.Congestion = double.NaN;
                            lmp.Loss = double.NaN;
                            time.MarketTime = marketDate;
                            if (sRTLmpHash.ContainsKey(hourKey))
                            {
                                time.Lmp = sRTLmpHash[hourKey];
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
                            if (sDAHash.ContainsKey(hourKey))
                            {
                                time.Price = sDAHash[hourKey];
                            }
                            node.TimePriceList.Add(time);
                        }
                        else
                        {
                            LmpTimePrice time = new LmpTimePrice();
                            Vayu.NodePriceLibrary.LMP lmp = new Vayu.NodePriceLibrary.LMP();
                            lmp.Price = double.NaN;
                            lmp.Congestion = double.NaN;
                            lmp.Loss = double.NaN;
                            time.MarketTime = marketDate;
                            if (sDALmpHash.ContainsKey(hourKey))
                            {
                                time.Lmp = sDALmpHash[hourKey];
                            }
                            node.LmpTimePriceList.Add(time);
                        }
                    }
                    tempDate = tempDate.AddDays(1);
                }
            }
        }
        /// <summary>
        /// Sets to CST.
        /// </summary>
        /// <param name="datetime">The datetime.</param>
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

        public static void GetAllDartsAndLMP(int marketKey, DateTime startDate, DateTime endDate)
        {
            Tuple<DateTime, DateTime> tuple = null;
            if (sAllDateHash.ContainsKey(marketKey))
            {
                tuple = sAllDateHash[marketKey];
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
                if (!sAllDateHash.ContainsKey(marketKey) || found)
                {
                    DARTThread range = new DARTThread(null, sDAHash, null, true, "allLMPData", marketKey, startDate, endDate, sDALmpHash);
                    //DARTThread range = new DARTThread(null, null, null, true, "allLMPData", marketKey, startDate, endDate, sDALmpHash);

                    Thread thread = new Thread(new ThreadStart(range.Run));
                    thread.Start();
                    range = new DARTThread(null, sRTHash, null, false, "allLMPData", marketKey, startDate, endDate, sRTLmpHash);
                    //range = new DARTThread(null, null, null, false, "allLMPData", marketKey, startDate, endDate, sRTLmpHash);

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
                    if (!sAllDateHash.ContainsKey(marketKey))
                    {
                        tuple = new Tuple<DateTime, DateTime>(startDate, endDate);
                        sAllDateHash.TryAdd(marketKey, tuple);
                        break;
                    }
                    else
                    {
                        DateTime tempStartDate = startDate < tuple.Item1 ? startDate : tuple.Item1;
                        DateTime tempEndtDate = endDate > tuple.Item2 ? endDate : tuple.Item2;
                        tuple = new Tuple<DateTime, DateTime>(tempStartDate, tempEndtDate);
                        sAllDateHash[marketKey] = tuple;
                    }
                }
                else if (startDate == DateTime.Today && i == 0)
                {
                    DARTThread range = new DARTThread(null, sRTHash, null, false, "all", marketKey, startDate, endDate, sRTLmpHash);
                    //DARTThread range = new DARTThread(null, null, null, false, "all", marketKey, startDate, endDate, sRTLmpHash);

                    Thread thread1 = new Thread(new ThreadStart(range.Run));
                    thread1.Start();
                    if (thread1 != null)
                    {
                        thread1.Join();
                    }
                }
            }
        }
    }
}
