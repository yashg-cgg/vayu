using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Sigma.FTRCreditLibrary
{
    [ServiceContract]
    public interface IFTRCredit
    {
        [OperationContract()]
        Dictionary<long, CreditResult> GetCredit(int market, List<Credit> creditList);
        [OperationContract()]
        string Post(FTR[] ftr, DateTime toDate, int portfolio, string market, string marketName, int round, string type);
        [OperationContract()]
        string Cancel(int portfolio, string transaction, int round);
        [OperationContract]
        string CreateSubmissionFile(FTR[] ftr, DateTime toDate, string portfolioName, string market, int round, string user, string type);
    }
}
