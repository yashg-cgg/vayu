using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Timers;

namespace Vayu.WCFServerClientBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="SUBSCRIBERCLASS">The type of the ubscriberclass.</typeparam>
    /// <typeparam name="ISERVICECONTRACT">The type of the servicecontract.</typeparam>
    public abstract class WCFServerBase<SUBSCRIBERCLASS, ISERVICECONTRACT>
        where ISERVICECONTRACT : IServer  // SUBSCRIBERCLASS is subscriber class, ISERVICECONTRACT is ServiceContract interface which must be of type IPublisher
    {
        #region Declaration

        /// <summary>
        /// The reconnect timer
        /// </summary>
        public System.Timers.Timer reconnectTimer;
        /// <summary>
        /// Gets or sets the reconnect timer interval secs.
        /// </summary>
        /// <value>
        /// The reconnect timer interval secs.
        /// </value>
        public int reconnectTimerIntervalSecs { get; set; }
        /// <summary>
        /// The m log file
        /// </summary>
        public TextWriter mLogFile;
        // proxySettingHash: set this up in the inheriting class to include other information possible like display elements to update
        /// <summary>
        /// The proxy setting hash
        /// </summary>
        public Dictionary<int, ProxySettingBase> proxySettingHash = new Dictionary<int, ProxySettingBase>();
        /// <summary>
        /// The proxy to index hash
        /// </summary>
        public Dictionary<ISERVICECONTRACT, int> proxyToIndexHash = new Dictionary<ISERVICECONTRACT, int>();
        /// <summary>
        /// The proxy end point URL to proxy index hash
        /// </summary>
        public Dictionary<string, int> proxyEndPointUrlToProxyIndexHash = new Dictionary<string, int>();
        /// <summary>
        /// The proxy to pipe factory hash
        /// </summary>
        protected Dictionary<ISERVICECONTRACT, DuplexChannelFactory<ISERVICECONTRACT>> proxyToPipeFactoryHash = new Dictionary<ISERVICECONTRACT, DuplexChannelFactory<ISERVICECONTRACT>>();
        /// <summary>
        /// The int to pipe factory hash
        /// </summary>
        public Dictionary<int, ISERVICECONTRACT> intToPipeFactoryHash = new Dictionary<int, ISERVICECONTRACT>();
        /// <summary>
        /// Gets or sets a value indicating whether this instance is trade algo.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is trade algo; otherwise, <c>false</c>.
        /// </value>
        public bool isTradeAlgo { get; set; }
        /// <summary>
        /// The subscribe task
        /// </summary>
        Task SubscribeTask;
        /// <summary>
        /// The re connerct all task
        /// </summary>
        Task reConnerctAllTask;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WCFServerBase{SUBSCRIBERCLASS, ISERVICECONTRACT}"/> class.
        /// </summary>
        public WCFServerBase()
        {
            reconnectTimerIntervalSecs = 3;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WCFServerBase{SUBSCRIBERCLASS, ISERVICECONTRACT}"/> class.
        /// </summary>
        /// <param name="myLogfile">My logfile.</param>
        public WCFServerBase(TextWriter myLogfile)
            : this()
        {
            mLogFile = myLogfile;
            proxySettingHash = new Dictionary<int, ProxySettingBase>();
        }

        #region Public Methods

        /// <summary>
        /// Starts the connect and subscribe all.
        /// </summary>
        public virtual void startConnectAndSubscribeAll()
        {
            reconnectTimer = new System.Timers.Timer(1000 * reconnectTimerIntervalSecs);
            reconnectTimer.Elapsed += new ElapsedEventHandler(ReconnectAndSubscribeAll);
            ReconnectAndSubscribeAll(null, null);
        }

        /// <summary>
        /// Subscribes all.
        /// </summary>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        public virtual void SubscribeAll(bool isAlgo)
        {
            foreach (int myProxyIndex in proxySettingHash.Keys)
            {
                if (proxySettingHash[myProxyIndex].enabled == true
                    && proxySettingHash[myProxyIndex].connectedSuccess == true
                    && proxySettingHash[myProxyIndex].subscribedSuccess == false)
                {
                    try
                    {
                        SubscribeSingle(myProxyIndex, isAlgo);
                        // success
                        proxySettingHash[myProxyIndex].subscribedSuccess = true;
                        SetSubscribedStatus(proxySettingHash[myProxyIndex].proxy, true);
                        if (mLogFile != null)
                        {
                            mLogFile.WriteLine(DateTime.Now.TimeOfDay.ToString() + " | Subscribe() to remote proxy " + proxySettingHash[myProxyIndex].endPointUrl + " succeeded.");
                            mLogFile.Flush();
                        }
                    }
                    catch (Exception ex)
                    {
                        //proxySettingHash[myProxyIndex].subscribedSuccess = false;
                        //Proxy_Faulted(proxySettingHash[myProxyIndex].proxy, null);  // this also starts the reconnect timer for us.
                        //SetSubscribedStatus(proxySettingHash[myProxyIndex].proxy, false);

                        if (mLogFile != null)
                        {
                            mLogFile.WriteLine(DateTime.Now.TimeOfDay.ToString() + " | Subscribe() to remote proxy " + proxySettingHash[myProxyIndex].endPointUrl + " failed.");
                            mLogFile.Flush();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Subscribes the single.
        /// </summary>
        /// <param name="myProxyIndex">Index of my proxy.</param>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        public virtual void SubscribeSingle(int myProxyIndex, bool isAlgo)
        {
            int i = 4;
            ((IServer)proxySettingHash[myProxyIndex].proxy).Subscribe(isAlgo);
        }
        /// <summary>
        /// Unsubscribes the and disconnect all.
        /// </summary>
        public void UnsubscribeAndDisconnectAll()
        {
            UnsubscribeAll();
            DisconnectAll();
        }
        /// <summary>
        /// Unsubscribes all.
        /// </summary>
        public virtual void UnsubscribeAll()
        {
            foreach (ISERVICECONTRACT myProxy in proxyToIndexHash.Keys)
            {
                UnsubscribeSingle(myProxy);
            }
        }
        /// <summary>
        /// Disconnects all.
        /// </summary>
        public virtual void DisconnectAll()
        {
            foreach (ISERVICECONTRACT myProxy in proxyToIndexHash.Keys)
            {
                AbortProxy(myProxy); // no need to reconnect, so call AbortProxy() here
            }
        }

        /// <summary>
        /// Aborts the proxy.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public virtual void AbortProxy(object sender) // this will disconnect & unsubscribe without attempting to reconnect
        {
            if (sender != null && sender.GetType() == typeof(ISERVICECONTRACT))
            {
                ISERVICECONTRACT myProxy = (ISERVICECONTRACT)sender;
                if (((ICommunicationObject)myProxy).State == CommunicationState.Opened)
                {
                    UnsubscribeSingle(myProxy); // aborted automatically implies unsubscribed
                }
                if (proxyToPipeFactoryHash.ContainsKey((ISERVICECONTRACT)sender))
                {
                    DuplexChannelFactory<ISERVICECONTRACT> pipeFactory = proxyToPipeFactoryHash[(ISERVICECONTRACT)myProxy];
                    if (pipeFactory != null)
                    {
                        pipeFactory.Abort();
                        pipeFactory.Close();
                        pipeFactory = null;
                    }
                }
                if (proxyToIndexHash.ContainsKey((ISERVICECONTRACT)sender))
                {
                    int myProxyIndex = proxyToIndexHash[(ISERVICECONTRACT)sender];
                    proxySettingHash[myProxyIndex].connectedSuccess = false;
                    proxySettingHash[myProxyIndex].subscribedSuccess = false;
                }
                SetConnectedStatus(myProxy, false); // display red when proxy connection lost
                myProxy = default(ISERVICECONTRACT);  // set proxy = null
            }
        }
        /// <summary>
        /// Unsubscribes the single.
        /// </summary>
        /// <param name="myProxy">My proxy.</param>
        public virtual void UnsubscribeSingle(ISERVICECONTRACT myProxy)
        {
            if (proxySettingHash[proxyToIndexHash[myProxy]].subscribedSuccess == true)
            {
                if (((ICommunicationObject)myProxy).State == CommunicationState.Opened)
                {
                    try
                    {
                        myProxy.Unsubscribe(isTradeAlgo);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                proxySettingHash[proxyToIndexHash[myProxy]].subscribedSuccess = false;
                SetSubscribedStatus(myProxy, false);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Reconnects the and subscribe all.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        public virtual void ReconnectAndSubscribeAll(object sender, ElapsedEventArgs e)
        {

            if (SubscribeTask != null && !SubscribeTask.IsCompleted)
                Task.WaitAll(SubscribeTask);
            if (reConnerctAllTask != null && !reConnerctAllTask.IsCompleted)
                Task.WaitAll(reConnerctAllTask);
            reConnerctAllTask = Task.Factory.StartNew(() =>
            {
                ReconnectAll(null, null);
            });

            SubscribeTask = Task.Factory.StartNew(() =>
            {
                SubscribeAll(isTradeAlgo);
            });
        }
        /// <summary>
        /// Reconnects all.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        public virtual void ReconnectAll(object sender, ElapsedEventArgs e)
        {
            if (reconnectTimer != null)
            {
                reconnectTimer.Stop();
            }
            foreach (int myProxyIndex in proxySettingHash.Keys)
            {
                if (proxySettingHash[myProxyIndex].enabled == true
                    && proxySettingHash[myProxyIndex].connectedSuccess == false)
                {
                    bool createProxySuccess = false;
                    if (proxySettingHash[myProxyIndex].newEndPointUrl == "")
                    {
                        string myEndPointAddress = proxySettingHash[myProxyIndex].endPointUrl;
                        isTradeAlgo = proxySettingHash[myProxyIndex].isTradeAlgo;
                        createProxySuccess = CreateProxy(myProxyIndex, myEndPointAddress);
                    }
                    else // a new proxy url has been saved to try to connect to
                    {
                        proxyEndPointUrlToProxyIndexHash.Remove(proxySettingHash[myProxyIndex].endPointUrl);
                        proxySettingHash[myProxyIndex].endPointUrl = proxySettingHash[myProxyIndex].newEndPointUrl;
                        proxySettingHash[myProxyIndex].newEndPointUrl = "";
                        if (!proxyEndPointUrlToProxyIndexHash.ContainsKey(proxySettingHash[myProxyIndex].endPointUrl))
                        {
                            // only add to hash, and only connect, if it is not a dupe
                            proxyEndPointUrlToProxyIndexHash.Add(proxySettingHash[myProxyIndex].endPointUrl, myProxyIndex);
                            createProxySuccess = CreateProxy(myProxyIndex, proxySettingHash[myProxyIndex].endPointUrl);
                        }
                    }
                    if (createProxySuccess)
                    {
                        proxySettingHash[myProxyIndex].connectedSuccess = true;
                    }
                    else
                    {
                        proxySettingHash[myProxyIndex].connectedSuccess = false;
                        proxySettingHash[myProxyIndex].subscribedSuccess = false;
                        if (mLogFile != null)
                        {
                            //logfile.WriteLine(DateTime.Now.TimeOfDay.ToString() + " Subscribe() to remote proxy " + myProxyEndPointAddress 
                            //      + " failed. Retrying in " + reconnectTimerIntervalSecs.ToString() + " seconds.");
                            mLogFile.Flush();
                        }
                        Proxy_Faulted(proxySettingHash[myProxyIndex].proxy, null);  // this also starts the reconnect timer for us.
                    }
                }
            }
        }
        /// <summary>
        /// Handles the Faulted event of the Proxy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void Proxy_Faulted(object sender, EventArgs e)
        {
            // use Proxy_Faulted when you want to attempt to reconnect, including when the actual channel faulted event occurs
            AbortProxy(sender);
            reconnectTimer.Start();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sends the dispatch alert.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        protected virtual string SendDispatchAlert(object obj)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties messageProperties = context.IncomingMessageProperties;
            MessageHeaders headers = context.IncomingMessageHeaders;
            string headerSourceUrl = "remote_unknown";
            if (headers.From != null)
            {
                headerSourceUrl = headers.From.Uri.ToString();
            }
            //if (mLogFile != null)
            //{
            //    mLogFile.WriteLine(DateTime.Now.TimeOfDay.ToString() + " | RemoteAlert, source: " + headerSourceUrl.PadLeft(6));
            //}
            //Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " | RemoteAlert, source: " + headerSourceUrl.PadLeft(6));
            return headerSourceUrl;
        }

        /// <summary>
        /// Creates the proxy.
        /// </summary>
        /// <param name="myProxyIndex">Index of my proxy.</param>
        /// <param name="myEndPointAddress">My end point address.</param>
        /// <returns></returns>
        protected bool CreateProxy(int myProxyIndex, string myEndPointAddress)
        {
            bool returnVal = false;
            try
            {
                NetTcpBinding myBinding = new NetTcpBinding(SecurityMode.None);
                myBinding.OpenTimeout = new TimeSpan(0, 0, 2);
                myBinding.SendTimeout = new TimeSpan(0, 1, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                DuplexChannelFactory<ISERVICECONTRACT> pipeFactory = new DuplexChannelFactory<ISERVICECONTRACT>(new InstanceContext(this), myBinding, new EndpointAddress(myEndPointAddress));
                ISERVICECONTRACT mNewProxy;
                if (intToPipeFactoryHash.ContainsKey(myProxyIndex) == null)
                {
                    mNewProxy = intToPipeFactoryHash[myProxyIndex];
                }
                else
                {
                    mNewProxy = (ISERVICECONTRACT)pipeFactory.CreateChannel();
                    ((ICommunicationObject)mNewProxy).Faulted += new EventHandler(Proxy_Faulted);
                }
                // pipefactory and proxy created here; we don't yet know if transmission works until we try to call a remote method such as Subscribe!!
                if (proxyToPipeFactoryHash.ContainsKey(mNewProxy))
                {
                    proxyToPipeFactoryHash.Remove(mNewProxy);
                }
                proxyToPipeFactoryHash.Add(mNewProxy, pipeFactory);
                if (proxyToIndexHash.ContainsKey(mNewProxy))
                {
                    proxyToIndexHash.Remove(mNewProxy);
                }
                proxyToIndexHash.Add(mNewProxy, myProxyIndex);
                if (proxyEndPointUrlToProxyIndexHash.ContainsKey(myEndPointAddress))
                {
                    proxyEndPointUrlToProxyIndexHash.Remove(myEndPointAddress);
                }
                proxyEndPointUrlToProxyIndexHash.Add(myEndPointAddress, myProxyIndex);
                proxySettingHash[myProxyIndex].proxy = mNewProxy;
                proxySettingHash[myProxyIndex].pipeFactory = pipeFactory;
                // pipefactory and proxy created here; we don't yet know if transmission works until we try to call a remote method such as Subscribe!!
                //SetConnectedStatus(mNewProxy, true); 
                returnVal = true;
            }
            catch (Exception ex)
            {
                if (!Debugger.IsAttached)
                {
                    if (mLogFile != null)
                    {
                        mLogFile.WriteLine(DateTime.Now.TimeOfDay.ToString() + " Failed to connect in CreateProxy(). Connect url: "
                            + ". Trying again in " + reconnectTimerIntervalSecs.ToString() + " secs."); // + " , message: " + ex.Message);
                    }
                }
                AbortProxy(proxySettingHash[myProxyIndex].proxy); // don't attempt to reconnect here, return false and let the caller decide
                returnVal = false;
            }
            return returnVal;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Sets the connected status.
        /// </summary>
        /// <param name="proxyIndex">Index of the proxy.</param>
        /// <param name="bConnected">if set to <c>true</c> [b connected].</param>
        protected abstract void SetConnectedStatus(ISERVICECONTRACT proxyIndex, bool bConnected);
        /// <summary>
        /// Sets the subscribed status.
        /// </summary>
        /// <param name="proxyIndex">Index of the proxy.</param>
        /// <param name="bSubscribed">if set to <c>true</c> [b subscribed].</param>
        protected abstract void SetSubscribedStatus(ISERVICECONTRACT proxyIndex, bool bSubscribed);

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public class ProxySettingBase
        {
            #region Declaration & Properties

            /// <summary>
            /// The pipe factory
            /// </summary>
            public DuplexChannelFactory<ISERVICECONTRACT> pipeFactory;
            /// <summary>
            /// Gets or sets the end point URL.
            /// </summary>
            /// <value>
            /// The end point URL.
            /// </value>
            public string endPointUrl { get; set; }
            /// <summary>
            /// Gets or sets the new end point URL.
            /// </summary>
            /// <value>
            /// The new end point URL.
            /// </value>
            public string newEndPointUrl { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="ProxySettingBase"/> is enabled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if enabled; otherwise, <c>false</c>.
            /// </value>
            public bool enabled { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [connected success].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [connected success]; otherwise, <c>false</c>.
            /// </value>
            public bool connectedSuccess { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [subscribed success].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [subscribed success]; otherwise, <c>false</c>.
            /// </value>
            public bool subscribedSuccess { get; set; }
            /// <summary>
            /// The proxy
            /// </summary>
            public ISERVICECONTRACT proxy;
            /// <summary>
            /// Gets or sets a value indicating whether this instance is trade algo.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is trade algo; otherwise, <c>false</c>.
            /// </value>
            public bool isTradeAlgo { get; set; }

            #endregion

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxySettingBase"/> class.
            /// </summary>
            /// <param name="myEndPointUrl">My end point URL.</param>
            /// <param name="myEnabled">if set to <c>true</c> [my enabled].</param>
            public ProxySettingBase(string myEndPointUrl, bool myEnabled)
            {
                endPointUrl = myEndPointUrl;
                enabled = myEnabled;

                newEndPointUrl = "";
                connectedSuccess = false;
                subscribedSuccess = false;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxySettingBase"/> class.
            /// </summary>
            /// <param name="myEndPointUrl">My end point URL.</param>
            /// <param name="myEnabled">if set to <c>true</c> [my enabled].</param>
            /// <param name="isTradeAlgo">if set to <c>true</c> [is trade algo].</param>
            public ProxySettingBase(string myEndPointUrl, bool myEnabled, bool isTradeAlgo)
            {
                endPointUrl = myEndPointUrl;
                enabled = myEnabled;
                this.isTradeAlgo = isTradeAlgo;

                newEndPointUrl = "";
                connectedSuccess = false;
                subscribedSuccess = false;
            }
        }
    }
}
