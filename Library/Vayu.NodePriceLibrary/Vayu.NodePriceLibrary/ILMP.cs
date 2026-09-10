using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.NodePriceLibrary
{
    [ServiceContract]
    public interface ILMP
    {
        [OperationContract]
        Node[] GetPrice(Node[] nodes, bool isDA, bool onlyPrice, bool isFiveMin);

        [OperationContract]
        Node[] GetAllPrice(int market, bool isDA, DateTime startDate, DateTime endDate, bool onlyPrice);

        [OperationContract]
        Node[] GetAllUptoPrice(int market, bool isDA, DateTime startDate, DateTime endDate, bool onlyPrice);

        [OperationContract]
        Node[] GetAllUptoPriceDateTime(int market, bool isDA, DateTime startDate, DateTime endDate, bool isFiveMin, bool onlyPrice);

        [OperationContract]
        Node[] GetAllFiveMinPrice(int market, DateTime startDate, DateTime endDate, bool onlyPrice, string screename = null);

        [OperationContract]
        Node[] GetAllUptoFiveMinPrice(int market, bool isDA, DateTime startDate, DateTime endDate);

        [OperationContract]
        Node[] GetVirtualLmps(List<Node> mNodeList, int market, bool isDA, DateTime startDate, DateTime endDate, bool isFiveMin);
    }
}
