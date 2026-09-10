using System;
using System.Collections.Generic;

namespace Vayu.NodePriceDaily.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Gets the daily LMP.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="marketDateTime">The market date time.</param>
        /// <param name="isDA">if set to <c>true</c> [is da].</param>
        /// <returns></returns>
        Dictionary<int, DailyLMP> GetDailyLMP(int marketKey, DateTime marketDateTime, bool isDA);
        /// <summary>
        /// Determines whether [is peak day] [the specified market date].
        /// </summary>
        /// <param name="marketDate">The market date.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns>
        ///   <c>true</c> if [is peak day] [the specified market date]; otherwise, <c>false</c>.
        /// </returns>
        bool IsPeakDay(DateTime marketDate, int marketKey);
    }
}
