using System;
using System.Collections.Generic;

namespace Vayu.CongestionVolatilityIndex.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
        void GetCongestionVolatility(Action<List<CongestionVolatility>, Exception> callback, int marketKey, DateTime fromDate, DateTime throDate, string Type, bool HourlyChecked, bool FourhourlyChecked, bool DailyChecked);
        List<string> FillSourceSinkHash(int Marketkey);
        List<string> GetUptosNode(int Marketkey);
    }
}
