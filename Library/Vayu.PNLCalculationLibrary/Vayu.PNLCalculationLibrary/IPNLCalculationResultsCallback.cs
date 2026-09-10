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
    [ServiceContract]
    public interface IPNLCalculationResultsCallback
    {
         
        /// <summary>
        /// Sends the ercot results.
        /// </summary>
        /// <param name="hes">The hes.</param>
        [OperationContract(IsOneWay = true)]
        void SendErcotResults(HE[] hes);
    }
}
