using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.NodePriceFiveMinLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(CallbackContract = typeof(INodePriceFiveMinCallback), SessionMode = SessionMode.Required)]
    public interface INodePriceFiveMin
    {
        /// <summary>
        /// Subscribes the specified market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        [OperationContract]
        bool Subscribe(int market);
        /// <summary>
        /// Hearts the beat.
        /// </summary>
        [OperationContract]
        void HeartBeat();
        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Unsubscribe();
        /// <summary>
        /// Gets the nodes for market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        [OperationContract]
        Vayu.NodePriceLibrary.Node[] GetNodesForMarket(int market);
    }
}
