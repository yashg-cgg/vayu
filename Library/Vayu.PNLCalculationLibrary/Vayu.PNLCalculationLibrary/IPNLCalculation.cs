using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Vayu.PNLCalculationLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPNLCalculationResultsCallback), SessionMode = SessionMode.Required)]
    public interface IPNLCalculation
    {
        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Subscribe();
        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool HeartBeat();
        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Unsubscribe();
     }
}
