using System;
using System.Collections.Generic;

namespace Vayu.ProfitLossDaily.Model
{
    public interface IDataService
    {
        void loadDBCommands();

        Dictionary<DateTime, Dictionary<int, PnlFee>> GetPnlFee(DateTime startDate, DateTime endDate, string product, List<int> portfolioKeyList);

        List<PNLConstraints> GetPNLConstraints(List<Pnl> PNLList, bool SortChecked, DateTime startDate, DateTime endDate, int marketKey, bool RTChecked);


        List<PNLConstraints> GetPNLConstraintsEOM(List<Pnl> PNLList, bool SortChecked, DateTime startDate, DateTime endDate, int marketKey);


        //Task<List<PNLConstraints>> GetPNLConstraintsEOM(List<Pnl> PNLList, bool SortChecked, DateTime startDate, DateTime endDate, int marketKey);

        //  IAsyncEnumerable <List<PNLConstraints>> GetPNLConstraintsEOM(List<Pnl> PNLList, bool SortChecked, DateTime startDate, DateTime endDate, int marketKey);

        string GetExternalPortfolioName(int portfolioKey);

        Dictionary<int, string> getNodeData();

        List<Pnl> getEMOPnl(DateTime startDate, DateTime endDate);
    }
}
