using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Vayu.ErcotSubmissionLibrary
{
    [ServiceContract]
    public interface ISubmitResultCallback
    {
        [OperationContract(IsOneWay = true)]
        void SendResults(string[] s, string[] e);
    }
}
