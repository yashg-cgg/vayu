using System;
using System.Collections.Generic;

namespace Vayu.CRRTopTenParticipants.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Gets the top10 participant data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketKey">The market key.</param>
        /// <param name="month">The month.</param>
        /// <param name="number">The number.</param>
        void GetTop10ParticipantData(Action<List<DataItem>, Exception> callback, int MarketKey, string month, int number);
        /// <summary>
        /// Gets the top10 participant data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketKey">The market key.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        void GetTop10ParticipantData(Action<List<DataItem>, Exception> callback, int MarketKey, DateTime fromDate, DateTime toDate);
        /// <summary>
        /// Gets the period data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetPeriodData(Action<List<DateTime>, Exception> callback);
    }
}
