using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.ProfitLossDaily.Model
{
    public interface IDataService
    {
        void loadDBCommands();

        Dictionary<DateTime, Dictionary<int, PnlFee>> GetPnlFee(DateTime startDate, DateTime endDate, string product, List<int> portfolioKeyList);

        List<PNLConstraints> GetPNLConstraints(List<Pnl> PNLList, bool SortChecked, DateTime startDate, DateTime endDate, int marketKey);
        string GetExternalPortfolioName(int portfolioKey);
    }
}
