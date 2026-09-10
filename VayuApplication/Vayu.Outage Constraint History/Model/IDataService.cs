using System;
using System.Collections.Generic;

namespace Vayu.Outage_Constraint_History.Model
{
    public interface IDataService
    {
        void loadDBCommands(string StartDate, string EndDate);
        void GetConstraints(Action<IEnumerable<ConstraintOutages>, Exception> callback);
        void GetOutagelist(Action<IEnumerable<OutagesDetails>, Exception> callback);
        void GetConstraintlist(Action<IEnumerable<ConstraintDetails>, Exception> callback);
    }
}
