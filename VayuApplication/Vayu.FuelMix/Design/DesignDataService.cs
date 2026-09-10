using System;
using System.Collections.Generic;
using Vayu.FuelMix.Model;

namespace Vayu.FuelMix.Design
{
    public class DesignDataService : IDataService
    {
        public void GetFuelMixData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone)
        {
            throw new NotImplementedException();
        }
    }
}
