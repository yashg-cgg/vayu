using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;

namespace Vayu.WCFPubSubBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="LISTENERCLASS">The type of the istenerclass.</typeparam>
    /// <typeparam name="ISERVICECONTRACT">The type of the servicecontract.</typeparam>
    /// <typeparam name="ICALLBACK">The type of the callback.</typeparam>
    public abstract class WCFListenerBase<LISTENERCLASS, ISERVICECONTRACT, ICALLBACK>  // T is ListenerServiceClass, U is ServiceContractInterface, V is CallBackInterface
    {
        /// <summary>
        /// Gets or sets the WCF listen port.
        /// </summary>
        /// <value>
        /// The WCF listen port.
        /// </value>
        protected static int WcfListenPort { get; set; }
        /// <summary>
        /// Gets or sets the local ip address.
        /// </summary>
        /// <value>
        /// The local ip address.
        /// </value>
        protected static string LocalIPAddress { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [b save to database].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [b save to database]; otherwise, <c>false</c>.
        /// </value>
        public static bool bSaveToDb { get; set; }
        /// <summary>
        /// Gets or sets the listen end point string.
        /// </summary>
        /// <value>
        /// The listen end point string.
        /// </value>
        public static string ListenEndPointString { get; set; }
        
        /// <summary>
        /// Gets or sets the Vayu. database connection.
        /// </summary>
        /// <value>
        /// The Vayu. database connection.
        /// </value>
        protected static SqlConnection DBConnection { get; set; }
        /// <summary>
        /// The lock object
        /// </summary>
        protected static readonly object lockObj = new object();
        /// <summary>
        /// Gets or sets the number subscribed.
        /// </summary>
        /// <value>
        /// The number subscribed.
        /// </value>
        protected static int numberSubscribed { get; set; }
        /// <summary>
        /// The dictionary algo subscribe list
        /// </summary>
        protected static Dictionary<ICALLBACK, string> dictAlgoSubscribeList;
        /// <summary>
        /// The dictionary trade application subscribe list
        /// </summary>
        protected static Dictionary<ICALLBACK, string> dictTradeAppSubscribeList;

        /// <summary>
        /// Initializes a new instance of the <see cref="WCFListenerBase{LISTENERCLASS, ISERVICECONTRACT, ICALLBACK}"/> class.
        /// </summary>
        protected WCFListenerBase()
        {
        }
        /// <summary>
        /// Initializes the configuration.
        /// </summary>
        protected virtual void InitConfig()
        {
            ListenEndPointString = "ISubscribe";
            numberSubscribed = 0;
            dictAlgoSubscribeList = new Dictionary<ICALLBACK, string>();
            dictTradeAppSubscribeList = new Dictionary<ICALLBACK, string>();
            try
            {
                WcfListenPort = Convert.ToInt32(ConfigurationManager.AppSettings["wcfListenPort"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("wcfListenPort is not an integer in config file, exiting");
                Environment.Exit(-1);
            }
            if (ConfigurationManager.AppSettings.Get("bSaveToDb") != null)
            {
                try
                {
                    bSaveToDb = bool.Parse(ConfigurationManager.AppSettings["bSaveToDb"]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("bSaveToDb is not a boolean in config file, exiting.");
                    Environment.Exit(-1);
                }
            }
            LocalIPAddress = getLocalIPAddress();
        }
        /// <summary>
        /// Loads the database.
        /// </summary>
        protected virtual void LoadDB()
        {
            if (bSaveToDb)
            {
                
                 DBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            }
        }

        /// <summary>
        /// Gets the local ip address.
        /// </summary>
        /// <returns></returns>
        protected string getLocalIPAddress()
        {
            System.Net.NetworkInformation.NetworkInterface[] myNetworkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            var result = myNetworkInterfaces.First(a => a.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet);
            var result2 = result.GetIPProperties().UnicastAddresses.First(a => a.DuplicateAddressDetectionState == System.Net.NetworkInformation.DuplicateAddressDetectionState.Preferred);
            return "net.tcp://" + result2.Address.ToString() + ":" + WcfListenPort + "/" + ListenEndPointString;
        }
        /// <summary>
        /// Connects this instance.
        /// </summary>
        public void Connect()
        {
            using (ServiceHost host = new ServiceHost(typeof(LISTENERCLASS), new Uri("net.tcp://localhost:" + WcfListenPort.ToString())))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.OpenTimeout = new TimeSpan(0, 12, 0);
                myBinding.SendTimeout = new TimeSpan(0, 12, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
                myBinding.Security.Mode = SecurityMode.None;
                host.AddServiceEndpoint(typeof(ISERVICECONTRACT), myBinding, ListenEndPointString);
                try
                {
                    host.Open();
                    Console.WriteLine("Successfully opened connection on net.tcp://localhost:" + WcfListenPort.ToString() + " .");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(DateTime.Now + " Could not listen on port, exiting. " + e.Message);
                    Environment.Exit(-1);
                }
            }
        }

        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        public bool HeartBeat()
        {
            return true;
        }
        /// <summary>
        /// Subscribes the specified is algo.
        /// </summary>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        /// <returns></returns>
        public bool Subscribe(bool isAlgo)
        {
            lock (lockObj)
            {
                try
                {
                    //Get the hashCode of the connecting app and store it as a connection
                    ICALLBACK callback = OperationContext.Current.GetCallbackChannel<ICALLBACK>();
                    string ipAddress = getSubscriberIPAddress(OperationContext.Current);
                    if (isAlgo)
                    {
                        if (!dictAlgoSubscribeList.ContainsKey(callback))
                        {
                            numberSubscribed++;
                            /* Commented lines show how to get the remote callback client's IP address */
                            //OperationContext context = OperationContext.Current;
                            //MessageProperties properties = context.IncomingMessageProperties;
                            //RemoteEndpointMessageProperty endpointProperty = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                            //Console.WriteLine("{0} {1} {2}", DateTime.Now.ToString(), context.ServiceSecurityContext.PrimaryIdentity.Name);
                            Console.WriteLine("Subscribed.   numberSubscribed: " + numberSubscribed + " , ip: " + ipAddress.PadRight(12));
                            dictAlgoSubscribeList.Add(callback, ipAddress);
                        }
                    }
                    else
                    {
                        if (!dictTradeAppSubscribeList.ContainsKey(callback))
                        {
                            numberSubscribed++;
                            /* Commented lines show how to get the remote callback client's IP address */
                            //OperationContext context = OperationContext.Current;
                            //MessageProperties properties = context.IncomingMessageProperties;
                            //RemoteEndpointMessageProperty endpointProperty = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                            //Console.WriteLine("{0} {1} {2}", DateTime.Now.ToString(), context.ServiceSecurityContext.PrimaryIdentity.Name);
                            Console.WriteLine("Subscribed.   numberSubscribed: " + numberSubscribed + " , ip: " + ipAddress.PadRight(12));
                            dictTradeAppSubscribeList.Add(callback, ipAddress);
                        }
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }
        /// <summary>
        /// Gets the subscriber ip address.
        /// </summary>
        /// <param name="opContext">The op context.</param>
        /// <returns></returns>
        private string getSubscriberIPAddress(OperationContext opContext)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties prop = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string ipAddress = endpoint.Address;
            return ipAddress;
        }
        /// <summary>
        /// Unsubscribes the specified is algo.
        /// </summary>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        /// <returns></returns>
        public bool Unsubscribe(bool isAlgo)
        {
            lock (lockObj)
            {
                try
                {
                    //remove any connection that is leaving
                    ICALLBACK callback = OperationContext.Current.GetCallbackChannel<ICALLBACK>();
                    if (isAlgo)
                    {
                        if (callback != null && dictAlgoSubscribeList.ContainsKey(callback))
                        {
                            numberSubscribed--;
                            Console.WriteLine("Unsubscribed. numberSubscribed: " + numberSubscribed + " , ip: " + dictAlgoSubscribeList[callback].PadLeft(12));
                            dictAlgoSubscribeList.Remove(callback);
                        }
                    }
                    else
                    {
                        if (callback != null && dictTradeAppSubscribeList.ContainsKey(callback))
                        {
                            numberSubscribed--;
                            Console.WriteLine("Unsubscribed. numberSubscribed: " + numberSubscribed + " , ip: " + dictTradeAppSubscribeList[callback].PadLeft(12));
                            dictTradeAppSubscribeList.Remove(callback);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

    }
}
