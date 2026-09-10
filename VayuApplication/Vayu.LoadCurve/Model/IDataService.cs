using System;
using System.Collections.Generic;

namespace Vayu.LoadCurve.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Gets the load names.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetLoadNames(Action<List<String>, Exception> callback);
        /// <summary>
        /// Gets the frozen loads data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="FrozenTime">The frozen time.</param>
        void GetFrozenLoadsData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone, int FrozenTime);
    }
}
