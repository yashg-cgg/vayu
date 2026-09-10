using System.ServiceModel;

namespace Vayu.CRRSubmissionLibrary
{
    [ServiceContract]
    public interface ICRRSubmitResultCallback
    {
        [OperationContract(IsOneWay = true)]
        void SendResults(string[] s, string[] e);
    }
}
