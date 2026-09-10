using System;
using System.ServiceModel;
using System.Text;

namespace Vayu.CRRSubmissionLibrary
{
    [ServiceContract]
    public interface IBidCRRSubmit
    {
        [OperationContract]
        bool Subscribe(int traderid);
        [OperationContract]
        bool HeartBeat();
        [OperationContract]
        bool Unsubscribe();

        [OperationContract()]
        string Post(CRRBid[] crrBids, DateTime toDate, int portfolio, string market, string marketName, int round, string type);
        [OperationContract()]
        string Cancel(int portfolio, string transaction, int round);
        [OperationContract]
        StringBuilder CreateSubmissionFile(CRRBid[] crrBids, DateTime toDate, string portfolioName, string auctionName, int round, string user, string type);

        StringBuilder CreateAnnualSubmissionFile(CRRBid[] crrBids, DateTime toDate, string portfolioName, string auctionName, int round, string user, string type);
    }
}
