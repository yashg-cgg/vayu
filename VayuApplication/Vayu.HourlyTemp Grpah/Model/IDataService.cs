using System;
using System.Collections.Generic;
using Vayu.HourlyTemp_Grpah.ViewModels;

namespace Vayu.HourlyTemp_Grpah.Model
{
    public interface IDataService
    {
        void GetCityZoneNames(Action<List<CityZones>, Exception> callback, string Market);
        List<Tuple<string, string>> GetCityIcaoCodeList(string Market);
        List<string> GetCityIcaoCodeList1(string Market);
        void GetAllTemperatureDataList(Action<List<HourlyTemperatureData>, Exception> callback, DateTime? sdate, DateTime? edate, bool current, string icaocode, string Market);
        Dictionary<int, string> GetCountyHashByNodeKey();
    }
}
