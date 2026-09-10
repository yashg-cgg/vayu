using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.CRRCreditLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface ICRRCredit
    {
        [OperationContract()]
        Dictionary<long, CreditResult> GetCredit(int market, List<Credit> creditList);
        [OperationContract()]
        string Post(CRR[] CRR, DateTime toDate, int portfolio, string market, string marketName, int round, string type);
        [OperationContract()]
        string Cancel(int portfolio, string transaction, int round);
        [OperationContract]
        string CreateSubmissionFile(CRR[] CRR, DateTime toDate, string portfolioName, string market, int round, string user, string type);
    }
}

