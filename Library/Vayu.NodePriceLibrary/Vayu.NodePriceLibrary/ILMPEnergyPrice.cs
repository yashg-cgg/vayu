using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.NodePriceLibrary
{
    [ServiceContract]
    public interface ILMPEnergyPrice
    {
        [OperationContract]
        Dictionary<String, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> RTEnergyPrices(DateTime startDate, DateTime endDate);

        [OperationContract]
        Dictionary<String, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DAEnergyPrices(DateTime startDate, DateTime endDate);

        [OperationContract]
        Dictionary<String, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DARTEnergyPrices(DateTime startDate, DateTime endDate);
    }
}
