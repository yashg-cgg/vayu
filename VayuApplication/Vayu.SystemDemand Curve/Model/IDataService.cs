using System;
using System.Collections.Generic;

namespace Vayu.SystemDemand_Curve.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Gets the load names.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetLoadNames(Action<List<String>, Exception> callback);
        /// <summary>
        /// Gets the load data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        void GetLoadData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone);
        /// <summary>
        /// Gets the frozen data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="FrozenTime">The frozen time.</param>
        void GetFrozenData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone, int FrozenTime);

        DateTime GetFrozenUpdateTimeLoads(DateTime fromDate, DateTime toDate, string zone);

        /// <summary>
        /// GetRTUpdateTimeLoads
        /// </summary>
        /// <returns></returns>
        DateTime GetRTUpdateTimeLoads();
    }
}
