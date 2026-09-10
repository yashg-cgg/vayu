using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Vayu.WCFPubSubBase
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface IPublisher
    {
        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool HeartBeat();
        /// <summary>
        /// Subscribes the specified is algo.
        /// </summary>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        /// <returns></returns>
        [OperationContract]
        bool Subscribe(bool isAlgo);
        /// <summary>
        /// Unsubscribes the specified is algo.
        /// </summary>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        /// <returns></returns>
        [OperationContract]
        bool Unsubscribe(bool isAlgo);

    }
}
