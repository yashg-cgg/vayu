using Vayu.NodePriceLibrary;
using System.ServiceModel;

namespace Vayu.NodePriceFiveMinLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface INodePriceFiveMinCallback
    {
        /// <summary>
        /// Sends the price.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        [OperationContract(IsOneWay = true)]
        void SendPrice(Node[] nodes);
    }
}
