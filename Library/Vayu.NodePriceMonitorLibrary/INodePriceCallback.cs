using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.NodePriceMonitorLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface INodePriceCallback
    {
        /// <summary>
        /// Sends the LMP print.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        [OperationContract(IsOneWay = true)]
        void SendLmpPrint(List<HourlyNodePriceDetails> hourlyPrices);
        /// <summary>
        /// Sends the LMP dispatch.
        /// </summary>
        /// <param name="hourlyPrices">The hourly prices.</param>
        [OperationContract(IsOneWay = true)]
        void SendLmpDispatch(List<HourlyNodePriceDetails> hourlyPrices);
    }
}
