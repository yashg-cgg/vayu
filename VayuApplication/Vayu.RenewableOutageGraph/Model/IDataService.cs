using System;
using System.Collections.Generic;

namespace Vayu.RenewableOutageGraph.Model
{
    public interface IDataService
    {
        List<RenewableOutage> GetRenewableOutage(DateTime? sdate, DateTime? edate);
    }
}
