using System;
using System.Collections.Generic;
using System.ServiceModel;
using Vayu.WCFServerClientBase;

namespace Vayu.NodePriceMonitorLibrary
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.IServer" />
    [ServiceContract(CallbackContract = typeof(INodePriceCallback), SessionMode = SessionMode.Required)]
    public interface INodePrice : IServer
    {
        /// <summary>
        /// Subscribes the LMP server.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="hub">The hub.</param>
        /// <param name="lmpDate">The LMP date.</param>
        /// <param name="tempnode">The tempnode.</param>
        /// <returns></returns>
        [OperationContract]
        bool SubscribeLMPServer(int market, int hub, DateTime lmpDate, string tempnode);
        /// <summary>
        /// Gets the LMP print.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="hub">The hub.</param>
        /// <param name="lmpDate">The LMP date.</param>
        /// <returns></returns>
        [OperationContract]
        List<HourlyNodePriceDetails> GetLmpPrint(int market, int hub, DateTime lmpDate);
    }
}
