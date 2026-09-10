using System;
using System.Collections.Generic;

namespace Vayu.HourlyResourceOutgae.Model
{
    public interface IDataService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="zone"></param>
        void GetHourlyOutageData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone);
    }
}
