using System;
using System.Collections.Generic;

namespace Vayu.LoadForcast.Model
{
    public interface IDataService
    {
        void GetAllForecastData(Action<List<LoadForecastData>, Exception> callback, DateTime? sdate, DateTime? edate, int Marketkey);
        void GetAllForecast7Data(Action<List<LoadForecastData>, Exception> callback, DateTime? sdate, DateTime? edate, int Marketkey);

        List<LoadForecastType> GetAllForecastList(int Marketkey);
    }
}
