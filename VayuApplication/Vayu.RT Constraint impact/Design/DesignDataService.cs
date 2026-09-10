using System;
using Vayu.RT_Constraint_impact.Model;

namespace Vayu.RT_Constraint_impact.Design
{
    public class DesignDataService : IDataService
    {
        public void loadDBCommands()
        {
            throw new NotImplementedException();
        }

        public void GetConstraints(Action<System.Collections.Generic.List<ViewModels.Constraints>, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void GetConstraints(Action<System.Collections.Generic.List<ViewModels.Constraints>, Exception> callback, string selectedMarket, DateTime MarketDate, bool IsHourly)
        {
            throw new NotImplementedException();
        }
    }
}
