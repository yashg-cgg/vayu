using System.ServiceModel;

namespace Vayu.ErcotSubmissionLibrary
{
    //[ServiceContract(CallbackContract = typeof(ISubmitResultCallback), SessionMode = SessionMode.Required)]
    [ServiceContract(CallbackContract = typeof(ISubmitResultCallback))]
    // [ServiceContract]
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
