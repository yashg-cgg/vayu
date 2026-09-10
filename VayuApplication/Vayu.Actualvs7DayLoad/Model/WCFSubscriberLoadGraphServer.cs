using System;
using System.Collections.Generic;
using System.IO;
using Vayu.LoadGraphLibrary;
using Vayu.NodePriceMonitorLibrary;
using Vayu.WCFServerClientBase;

namespace Vayu.Actualvs7DayLoad.Model
{
    class WCFSubscriberLoadGraphServer : WCFServerBase<WCFSubscriberLMPServer, ILoadGraph>, ILoadGraphCallback
    {
        private ViewModels.MainWindowViewModel parentSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="WCFSubscriberLoadGraphServer"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="myLogWriter">My log writer.</param>
        public WCFSubscriberLoadGraphServer(ViewModels.MainWindowViewModel sender, TextWriter myLogWriter)
            : base(myLogWriter)
        {
            parentSender = sender;
        }

        /// <summary>
        /// Sets the connected status.
        /// </summary>
        /// <param name="myProxy">My proxy.</param>
        /// <param name="bRemoteConnected">if set to <c>true</c> [b remote connected].</param>
        protected override void SetConnectedStatus(ILoadGraph myProxy, bool bRemoteConnected)
        {
            int proxyIndex = proxyToIndexHash[myProxy];
            parentSender.SetConnectedStatusLoadGraphServer(proxyIndex, bRemoteConnected);
        }
        /// <summary>
        /// Sets the subscribed status.
        /// </summary>
        /// <param name="myProxy">My proxy.</param>
        /// <param name="bSubscribed">if set to <c>true</c> [b subscribed].</param>
        protected override void SetSubscribedStatus(ILoadGraph myProxy, bool bSubscribed)
        {
            SetConnectedStatus(myProxy, bSubscribed);
        }

        /// <summary>
        /// Subscribes the single.
        /// </summary>
        /// <param name="myProxyIndex">Index of my proxy.</param>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        public override void SubscribeSingle(int myProxyIndex, bool isAlgo)
        {
            string zone = ((WCFSubscriberLoadGraphServer.ProxySetting)proxySettingHash[myProxyIndex]).mZone;
            DateTime startdate = ((WCFSubscriberLoadGraphServer.ProxySetting)proxySettingHash[myProxyIndex]).mStartDate;
            DateTime enddate = ((WCFSubscriberLoadGraphServer.ProxySetting)proxySettingHash[myProxyIndex]).mEndDate;
            ((ILoadGraph)proxySettingHash[myProxyIndex].proxy).SubscribeLoadGraph(zone, startdate, enddate);
        }

        /// <summary>
        /// Sets the graph.
        /// </summary>
        /// <param name="Zone">The zone.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <param name="hourHash">The hour hash.</param>
        /// <param name="hour1Hash">The hour1 hash.</param>
        /// <param name="hour2Hash">The hour2 hash.</param>
        /// <param name="hour3Hash">The hour3 hash.</param>
        /// <param name="hour4Hash">The hour4 hash.</param>
        /// <param name="hour5Hash">The hour5 hash.</param>
        /// <param name="hour6Hash">The hour6 hash.</param>
        /// <param name="hour7Hash">The hour7 hash.</param>
        /// <param name="hour8Hash">The hour8 hash.</param>
        /// <param name="hour9Hash">The hour9 hash.</param>
        /// <param name="hour10Hash">The hour10 hash.</param>
        /// <param name="hour11Hash">The hour11 hash.</param>
        /// <param name="hour12Hash">The hour12 hash.</param>
        /// <param name="hour13Hash">The hour13 hash.</param>
        /// <param name="hour14Hash">The hour14 hash.</param>
        /// <param name="hour15Hash">The hour15 hash.</param>
        public void SetGraph(string Zone, DateTime StartDate, DateTime EndDate, Dictionary<int, Dictionary<int, double>> hourHash, Dictionary<int, double> hour1Hash, Dictionary<int, double> hour2Hash, Dictionary<int, double> hour3Hash, Dictionary<int, double> hour4Hash, Dictionary<int, Dictionary<int, double>> hour5Hash, Dictionary<int, double> hour6Hash, Dictionary<int, double> hour7Hash, Dictionary<int, double> hour8Hash, Dictionary<int, double> hour9Hash, Dictionary<int, double> hour10Hash, Dictionary<int, double> hour11Hash, Dictionary<int, double> hour12Hash, Dictionary<int, double> hour13Hash, Dictionary<int, double> hour14Hash, Dictionary<int, double> hour15Hash)
        {
            parentSender.SetGraph(Zone, StartDate, EndDate, hourHash, hour1Hash, hour2Hash, hour3Hash, hour4Hash, hour5Hash, hour6Hash, hour7Hash, hour8Hash, hour9Hash, hour10Hash, hour11Hash, hour12Hash, hour13Hash, hour14Hash, hour15Hash);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="WCFServerClientBase.WCFServerBase{Vayu.LoadStackCurve.Model.WCFSubscriberLMPServer,LoadServiceLibrary.ILoadGraph}.ProxySettingBase" />
        public class ProxySetting : ProxySettingBase
        {
            /// <summary>
            /// The m proxy
            /// </summary>
            public ILoadGraph mProxy;
            /// <summary>
            /// The m zone
            /// </summary>
            public string mZone;
            /// <summary>
            /// The m start date
            /// </summary>
            public DateTime mStartDate;
            /// <summary>
            /// The m end date
            /// </summary>
            public DateTime mEndDate;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxySetting"/> class.
            /// </summary>
            /// <param name="myEndPointUrl">My end point URL.</param>
            /// <param name="myEnabled">if set to <c>true</c> [my enabled].</param>
            /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
            /// <param name="zone">The zone.</param>
            /// <param name="startdate">The startdate.</param>
            /// <param name="enddate">The enddate.</param>
            public ProxySetting(string myEndPointUrl, bool myEnabled, bool isAlgo, string zone, DateTime startdate, DateTime enddate)
                : base(myEndPointUrl, myEnabled, isAlgo)
            {
                endPointUrl = myEndPointUrl;
                enabled = myEnabled;
                mZone = zone;
                mStartDate = startdate;
                mEndDate = enddate;

                newEndPointUrl = "";
                connectedSuccess = false;
            }
        }
    }

    public class WCFSubscriberLMPServer : WCFServerBase<WCFSubscriberLMPServer, INodePrice>, INodePriceCallback
    {
        //private LMPForm parentSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="WCFSubscriberLMPServer"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="myLogWriter">My log writer.</param>
        public WCFSubscriberLMPServer(object sender, System.IO.TextWriter myLogWriter)
            : base(myLogWriter)
        {
            //parentSender = sender;
        }

        /// <summary>
        /// Sets the connected status.
        /// </summary>
        /// <param name="myProxy">My proxy.</param>
        /// <param name="bRemoteConnected">if set to <c>true</c> [b remote connected].</param>
        protected override void SetConnectedStatus(INodePrice myProxy, bool bRemoteConnected)
        {
            int proxyIndex = proxyToIndexHash[myProxy];
        }
        /// <summary>
        /// Sets the subscribed status.
        /// </summary>
        /// <param name="myProxy">My proxy.</param>
        /// <param name="bSubscribed">if set to <c>true</c> [b subscribed].</param>
        protected override void SetSubscribedStatus(INodePrice myProxy, bool bSubscribed)
        {
            SetConnectedStatus(myProxy, bSubscribed);
        }
        /// <summary>
        /// Loads the LMP data.
        /// </summary>
        /// <param name="DA">The da.</param>
        /// <param name="hourHash">The hour hash.</param>
        public void LoadLMPData(Dictionary<int, double> DA, Dictionary<int, Dictionary<int, double>> hourHash)
        {
            //parentSender.LoadLMPData(DA, hourHash);
        }
        /// <summary>
        /// Updates the LMP data.
        /// </summary>
        /// <param name="hourHash">The hour hash.</param>
        public void UpdateLMPData(Dictionary<int, Dictionary<int, double>> hourHash)
        {
            //parentSender.UpdateLMPData(hourHash);
        }
        /// <summary>
        /// Update15s the sec LMP.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="price">The price.</param>
        public void Update15SecLMP(int hour, int minute, double price)
        {
            //parentSender.Update15SecLMP(hour, minute, price);
        }
        /// <summary>
        /// Update15s the sec ercot LMP.
        /// </summary>
        /// <param name="forecastBid">The forecast bid.</param>
        public void Update15SecErcotLMP(List<string> forecastBid)
        {
            //parentSender.Update15SecErcotLMP(forecastBid);
        }
        /// <summary>
        /// Subscribes the single.
        /// </summary>
        /// <param name="myProxyIndex">Index of my proxy.</param>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        public override void SubscribeSingle(int myProxyIndex, bool isAlgo)
        {
            string market = ((WCFSubscriberLMPServer.ProxySetting)proxySettingHash[myProxyIndex]).market;
            string minute = ((WCFSubscriberLMPServer.ProxySetting)proxySettingHash[myProxyIndex]).minute;
            string second = ((WCFSubscriberLMPServer.ProxySetting)proxySettingHash[myProxyIndex]).second;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="WCFServerClientBase.WCFServerBase{Vayu.LoadStackCurve.Model.WCFSubscriberLMPServer,NodePriceMonitorLibrary.INodePrice}.ProxySettingBase" />
        public class ProxySetting : ProxySettingBase
        {
            /// <summary>
            /// The proxy
            /// </summary>
            public INodePrice proxy;
            /// <summary>
            /// The market
            /// </summary>
            public string market;
            /// <summary>
            /// The minute
            /// </summary>
            public string minute;
            /// <summary>
            /// The second
            /// </summary>
            public string second;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxySetting"/> class.
            /// </summary>
            /// <param name="myEndPointUrl">My end point URL.</param>
            /// <param name="myEnabled">if set to <c>true</c> [my enabled].</param>
            /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
            /// <param name="myMarket">My market.</param>
            /// <param name="myMinute">My minute.</param>
            /// <param name="mySecond">My second.</param>
            public ProxySetting(string myEndPointUrl, bool myEnabled, bool isAlgo, string myMarket, string myMinute, string mySecond)
                : base(myEndPointUrl, myEnabled, isAlgo)
            {
                endPointUrl = myEndPointUrl;
                enabled = myEnabled;
                market = myMarket;
                minute = myMinute;
                second = mySecond;

                newEndPointUrl = "";
                connectedSuccess = false;
            }
        }

        /// <summary>
        /// Sends the LMP print.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        public void SendLmpPrint(List<HourlyNodePriceDetails> hourlyPrices)
        {
            //
        }

        /// <summary>
        /// Sends the LMP dispatch.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        public void SendLmpDispatch(List<HourlyNodePriceDetails> hourlyPrices)
        {
        }
    }
}
