using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.LoadGraphLibrary
{

    [ServiceContract()]
    public interface ILoadGraphCallback
    {
        [OperationContract(IsOneWay = true)]
        void SetGraph(string Zone, DateTime StartDate, DateTime EndDate, Dictionary<int, Dictionary<int, double>> hourHash, Dictionary<int, double> hour1Hash, Dictionary<int, double> hour2Hash, Dictionary<int, double> hour3Hash, Dictionary<int, double> hour4Hash, Dictionary<int, Dictionary<int, double>> hour5Hash, Dictionary<int, double> hour6Hash, Dictionary<int, double> hour7Hash, Dictionary<int, double> hour8Hash, Dictionary<int, double> hour9Hash, Dictionary<int, double> hour10Hash, Dictionary<int, double> hour11Hash, Dictionary<int, double> hour12Hash, Dictionary<int, double> hour13Hash, Dictionary<int, double> hour14Hash, Dictionary<int, double> hour15Hash);
    }
}