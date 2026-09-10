using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Vayu.ErcotSubmissionLibrary
{
     
    [ServiceContract(CallbackContract = typeof(ISubmitResultCallback))]
   
    public interface IBidSubmit
    {
        [OperationContract]
        bool Subscribe(int traderid);
        [OperationContract]
        bool HeartBeat();
        [OperationContract]
        bool Unsubscribe();
        [OperationContract]
        string SubmitBids(PTPBid[] b, int submittype);
        [OperationContract]
        string CancelBids(PTPBid[] b, int submittype);
    }
}
