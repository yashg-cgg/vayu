using System;
using System.Collections.Generic;
using System.IO;
using Vayu.NodePriceMonitorLibrary;
using Vayu.WCFServerClientBase;

namespace Vayu.NodePriceMonitor.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="WCFServerClientBase.WCFServerBase{Vayu.NodePriceMonitor.ViewModel.SubscriberLMPMonitor,NodePriceMonitorLibrary.INodePrice}" />
    /// <seealso cref="Vayu.INodePriceCallback" />
    public class SubscriberLMPMonitor : WCFServerBase<SubscriberLMPMonitor, INodePrice>, INodePriceCallback
    {
        /// <summary>
        /// The parent
        /// </summary>
        private MainWindowViewModel parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriberLMPMonitor"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="myLogWriter">My log writer.</param>
        public SubscriberLMPMonitor(MainWindowViewModel sender, TextWriter myLogWriter)
            : base(myLogWriter)
        {
            parent = sender;
        }
        /// <summary>
        /// Sends the LMP print.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        public void SendLmpPrint(List<HourlyNodePriceDetails> hourlyPrices)
        {
            parent.SendLmpPrint(hourlyPrices);
        }

        /// <summary>
        /// Sends the LMP dispatch.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        public void SendLmpDispatch(List<HourlyNodePriceDetails> hourlyPrices)
        {
            parent.SendLmpDispatch(hourlyPrices);
        }

        /// <summary>
        /// Sets the connected status.
        /// </summary>
        /// <param name="myProxy">My proxy.</param>
        /// <param name="bRemoteConnected">if set to <c>true</c> [b remote connected].</param>
        protected override void SetConnectedStatus(INodePrice myProxy, bool bRemoteConnected)
        {
            int proxyIndex = proxyToIndexHash[myProxy];
            parent.SetConnectedStatusLMPServer(proxyIndex, bRemoteConnected);
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
        /// Subscribes the single.
        /// </summary>
        /// <param name="myProxyIndex">Index of my proxy.</param>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        public override void SubscribeSingle(int myProxyIndex, bool isAlgo)
        {
            int market = ((SubscriberLMPMonitor.ProxySetting)proxySettingHash[myProxyIndex]).mMarket;
            int hub = ((SubscriberLMPMonitor.ProxySetting)proxySettingHash[myProxyIndex]).mHub;
            DateTime lmpdate = ((SubscriberLMPMonitor.ProxySetting)proxySettingHash[myProxyIndex]).mNodePriceDate;
            string tempnode = ((SubscriberLMPMonitor.ProxySetting)proxySettingHash[myProxyIndex]).mTempNode;
            ((INodePrice)proxySettingHash[myProxyIndex].proxy).SubscribeLMPServer(market, hub, lmpdate, tempnode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="WCFServerClientBase.WCFServerBase{Vayu.NodePriceMonitor.ViewModel.SubscriberLMPMonitor,NodePriceMonitorLibrary.INodePrice}.ProxySettingBase" />
        public class ProxySetting : ProxySettingBase
        {
            /// <summary>
            /// The m proxy
            /// </summary>
            public INodePrice mProxy;
            /// <summary>
            /// The m market
            /// </summary>
            public int mMarket;
            /// <summary>
            /// The m hub
            /// </summary>
            public int mHub;
            /// <summary>
            /// The m node price date
            /// </summary>
            public DateTime mNodePriceDate;
            /// <summary>
            /// The m temporary node
            /// </summary>
            public string mTempNode;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxySetting"/> class.
            /// </summary>
            /// <param name="myEndPointUrl">My end point URL.</param>
            /// <param name="myEnabled">if set to <c>true</c> [my enabled].</param>
            /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
            /// <param name="market">The market.</param>
            /// <param name="hub">The hub.</param>
            /// <param name="lmpDate">The LMP date.</param>
            /// <param name="tempNode">The temporary node.</param>
            public ProxySetting(string myEndPointUrl, bool myEnabled, bool isAlgo, int market, int hub, DateTime lmpDate, string tempNode)
                : base(myEndPointUrl, myEnabled, isAlgo)
            {
                endPointUrl = myEndPointUrl;
                enabled = myEnabled;
                mMarket = market;
                mHub = hub;
                mNodePriceDate = lmpDate;
                mTempNode = tempNode;

                newEndPointUrl = "";
                connectedSuccess = false;
            }
        }
    }
}
