using System;
using System.Collections.Generic;

namespace Vayu.FuelMix.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {

        /// <summary>
        /// Gets the load data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>


        void GetFuelMixData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone);

    }
}
