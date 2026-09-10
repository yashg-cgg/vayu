using System;
using System.Collections.Generic;

namespace Vayu.Actualvs7DayLoad.Model
{
    public interface IDataService
    {
        void GetLoadNames(Action<List<String>, Exception> callback);
        void GetLoadData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone, int Marketkey);
    }
}
