using System;
using System.ServiceModel;

namespace Vayu.NodePriceLibrary
{
    [ServiceContract]
    public interface ILMPMarketView
    {
        [OperationContract]
        Node[] GetAllFiveMinPrice(int market, DateTime startDate, DateTime endDate, bool onlyPrice, string screename = null);
    }
}
