using System;
using System.Collections.Generic;
using Vayu.TemperatureGraph.ViewModels;

namespace Vayu.TemperatureGraph.Model
{
    public interface IDataService
    {
        void GetCityZoneNames(Action<List<CityZones>, Exception> callback, string Market);
        void GetAllTemperatureData(Action<List<TemperatureData>, Exception> callback, DateTime? sdate, DateTime? edate, bool current, string Market);
    }
}
