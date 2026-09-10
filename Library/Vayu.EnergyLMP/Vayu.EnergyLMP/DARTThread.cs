using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Vayu.NodePriceLibrary;

namespace Vayu.EnergyLMP
{

    /// <summary>
    /// 
    /// </summary>
    public class DARTThread
    {
        #region Declaration

        /// <summary>
        /// The m price list
        /// </summary>
        private List<Node> mPriceList = new List<Node>();
        /// <summary>
        /// The m price hash
        /// </summary>
        private ConcurrentDictionary<string, double> mPriceHash;
        /// <summary>
        /// The m LMP hash
        /// </summary>
        private ConcurrentDictionary<string, Vayu.NodePriceLibrary.LMP> mLmpHash;
        /// <summary>
        /// The m five minimum price hash
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentDictionary<int, double>> mFiveMinPriceHash;
        /// <summary>
        /// The s dart end point
        /// </summary>
        private static string sDartEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress();

        /// <summary>
        /// The m is da
        /// </summary>
        private bool mIsDA;
        /// <summary>
        /// The m type
        /// </summary>
        private string mType;
        /// <summary>
        /// The m market
        /// </summary>
        private int mMarket;
        /// <summary>
        /// The m start
        /// </summary>
        private DateTime mStart;
        /// <summary>
        /// The m end
        /// </summary>
        private DateTime mEnd;
        private bool onlyprice;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DARTThread"/> class.
        /// </summary>
        /// <param name="priceList">The price list.</param>
        /// <param name="priceHash">The price hash.</param>
        /// <param name="fiveMinPriceHash">The five minimum price hash.</param>
        /// <param name="isDA">if set to <c>true</c> [is da].</param>
        /// <param name="type">The type.</param>
        /// <param name="market">The market.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="lmpHash">The LMP hash.</param>
        public DARTThread(List<Node> priceList, ConcurrentDictionary<string, double> priceHash, ConcurrentDictionary<string, ConcurrentDictionary<int, double>> fiveMinPriceHash, bool isDA, string type,
                                int market, DateTime start, DateTime end, ConcurrentDictionary<string, Vayu.NodePriceLibrary.LMP> lmpHash, bool onlyPrice = true)
        {
            mPriceHash = priceHash;
            mFiveMinPriceHash = fiveMinPriceHash;
            mPriceList = priceList;
            mIsDA = isDA;
            mType = type;
            mMarket = market;
            mStart = start;
            mEnd = end;
            mLmpHash = lmpHash;
            onlyprice = onlyPrice;
        }
        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
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
                Node[] priceNodes = null;
                if (mType == "all")
                {
                    priceNodes = nodeProxy.GetAllPrice(mMarket, mIsDA, mStart, mEnd, true);
                }
                else if (mType == "allLMPData")
                {
                    priceNodes = nodeProxy.GetAllPrice(mMarket, mIsDA, mStart, mEnd, false); // Only Price = False to get all LMP Data i.e. Price, Congestion and Loss
                }
                else if (mType == "uptos")
                {
                    priceNodes = nodeProxy.GetAllUptoPrice(mMarket, mIsDA, mStart, mEnd, true);
                }
                else if (mType == "fivemin")
                {
                    priceNodes = nodeProxy.GetAllFiveMinPrice(mMarket, mStart.AddHours(-1), mEnd.AddHours(-1), true);
                }
                else
                {
                    if (onlyprice)
                        priceNodes = nodeProxy.GetPrice(mPriceList.ToArray<Node>(), mIsDA, mPriceHash != null, false);
                    else
                        priceNodes = nodeProxy.GetPrice(mPriceList.ToArray<Node>(), mIsDA, false, false);
                }
                foreach (Node priceNode in priceNodes)
                {
                    if (mType != "allLMPData")
                    {
                        if (mPriceHash != null || mFiveMinPriceHash != null)
                        {
                            foreach (TimePrice time in priceNode.TimePriceList)
                            {
                                if (mType == "fivemin")
                                {
                                    DateTime hourDate = DateTime.Parse(time.MarketTime.Month + "/" + time.MarketTime.Day + "/" + time.MarketTime.Year + " " + time.MarketTime.Hour + ":00");
                                    string priceKey = hourDate.AddHours(1).ToString() + priceNode.NodeId.ToString();
                                    ConcurrentDictionary<int, double> minuteHash = new ConcurrentDictionary<int, double>();
                                    if (mFiveMinPriceHash.ContainsKey(priceKey))
                                    {
                                        minuteHash = mFiveMinPriceHash[priceKey];
                                    }
                                    else
                                    {
                                        mFiveMinPriceHash.TryAdd(priceKey, minuteHash);
                                    }
                                    minuteHash.TryAdd(time.MarketTime.Minute, time.Price);
                                }
                                else
                                {
                                    string priceKey = time.MarketTime.ToString() + priceNode.NodeId.ToString();
                                    if (!mPriceHash.ContainsKey(priceKey))
                                    {
                                        mPriceHash.TryAdd(priceKey, time.Price);
                                    }
                                    else if (time.MarketTime > DateTime.Today)
                                    {
                                        mPriceHash[priceKey] = time.Price;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (LmpTimePrice time in priceNode.LmpTimePriceList)
                            {
                                string priceKey = time.MarketTime.ToString() + priceNode.NodeId.ToString();

                                if (!mLmpHash.ContainsKey(priceKey))
                                {
                                    mLmpHash.TryAdd(priceKey, time.Lmp);
                                }
                                else if (time.MarketTime > DateTime.Today)
                                {
                                    mLmpHash[priceKey] = time.Lmp;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (priceNode.LmpTimePriceList != null)
                        {
                            foreach (LmpTimePrice time in priceNode.LmpTimePriceList)
                            {
                                string priceKey = time.MarketTime.ToString() + priceNode.NodeId.ToString();

                                if (!mLmpHash.ContainsKey(priceKey))
                                {
                                    mLmpHash.TryAdd(priceKey, time.Lmp);
                                }
                                else if (time.MarketTime > DateTime.Today)
                                {
                                    mLmpHash[priceKey] = time.Lmp;
                                }
                            }
                        }

                        #region commented code
                        //if ((mPriceHash != null && mPriceHash.Count != 0) || mFiveMinPriceHash != null)
                        //{
                        //    foreach (TimePrice time in priceNode.TimePriceList)
                        //    {
                        //        if (mType == "fivemin")
                        //        {
                        //            DateTime hourDate = DateTime.Parse(time.MarketTime.Month + "/" + time.MarketTime.Day + "/" + time.MarketTime.Year + " " + time.MarketTime.Hour + ":00");
                        //            string priceKey = hourDate.AddHours(1).ToString() + priceNode.NodeId.ToString();
                        //            ConcurrentDictionary<int, double> minuteHash = new ConcurrentDictionary<int, double>();
                        //            if (mFiveMinPriceHash.ContainsKey(priceKey))
                        //            {
                        //                minuteHash = mFiveMinPriceHash[priceKey];
                        //            }
                        //            else
                        //            {
                        //                mFiveMinPriceHash.TryAdd(priceKey, minuteHash);
                        //            }
                        //            minuteHash.TryAdd(time.MarketTime.Minute, time.Price);
                        //        }
                        //        else
                        //        {
                        //            string priceKey = time.MarketTime.ToString() + priceNode.NodeId.ToString();
                        //            if (!mPriceHash.ContainsKey(priceKey))
                        //            {
                        //                mPriceHash.TryAdd(priceKey, time.Price);
                        //            }
                        //            else if (time.MarketTime > DateTime.Today)
                        //            {
                        //                mPriceHash[priceKey] = time.Price;
                        //            }
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    foreach (LmpTimePrice time in priceNode.LmpTimePriceList)
                        //    {
                        //        string priceKey = time.MarketTime.ToString() + priceNode.NodeId.ToString();

                        //        if (!mLmpHash.ContainsKey(priceKey))
                        //        {
                        //            mLmpHash.TryAdd(priceKey, time.Lmp);
                        //        }
                        //        else if (time.MarketTime > DateTime.Today)
                        //        {
                        //            mLmpHash[priceKey] = time.Lmp;
                        //        }
                        //    }
                        //}
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
