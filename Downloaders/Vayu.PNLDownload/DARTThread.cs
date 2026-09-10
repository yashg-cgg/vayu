using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Vayu.PNLCalculationLibrary;
using System.ServiceModel;
using System.Data.SqlClient;
using Vayu.NodePriceLibrary;
using System.Threading;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;

namespace Vayu.PNLDownload
{
    /// <summary>
    /// 
    /// </summary>
    class DARTThread
    {
        #region Private Members
        /// <summary>
        /// The price list
        /// </summary>
        private List<Node> mPriceList = new List<Node>();
        /// <summary>
        /// The price hash
        /// </summary>
        private Dictionary<string, double> mPriceHash;
        /// <summary>
        /// The dart end point
        /// </summary>
        private static string sDartEndPoint = CommonAccessLibrary.ServiceConnections.GetLMPServiceAddress();

        /// <summary>
        /// The m is da
        /// </summary>
        private bool mIsDA; 
        #endregion

        /// <summary>
        /// Initializes a new instance of the DARTThread class.
        /// </summary>
        /// <param name="priceList">The price list.</param>
        /// <param name="priceHash">The price hash.</param>
        /// <param name="isDA">if set to <c>true</c> [is da].</param>
        public DARTThread(List<Node> priceList, Dictionary<string, double> priceHash, bool isDA)
        {
            mPriceHash = priceHash;
            mPriceList = priceList;
            mIsDA = isDA;
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
                Node[] rtNodes = nodeProxy.GetPrice(mPriceList.ToArray<Node>(), mIsDA,true,false);
                foreach (Node rtNode in rtNodes)
                {
                    foreach (TimePrice time in rtNode.TimePriceList)
                    {
                        if (!double.IsNaN(time.Price))
                        {
                            string rtKey = time.MarketTime.ToString() + rtNode.NodeId.ToString();
                            mPriceHash.Add(rtKey, time.Price);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
