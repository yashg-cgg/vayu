
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Vayu.WCFServerClientBase;

namespace Vayu.LoadGraphLibrary
{


    [ServiceContract(CallbackContract = typeof(ILoadGraphCallback), SessionMode = SessionMode.Required)]
    public interface ILoadGraph : IServer
    {

        [OperationContract]
        bool SubscribeLoadGraph(string zone, DateTime StartDate, DateTime EndDate);

        [OperationContract]
        HashValues GetLoadValues(string zone, DateTime StartDate, DateTime EndDate);

        [OperationContract]
        Dictionary<DateTime, HashValues> GetBulkData(string zone, DateTime StartDate, DateTime EndDate);
    }
}
