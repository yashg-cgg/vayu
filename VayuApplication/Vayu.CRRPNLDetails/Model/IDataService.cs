using System;
using System.Collections.Generic;
using Vayu.CRRCalculationLibrary;

namespace Vayu.CRRPNLDetails.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Gets the ercot account holders.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetERCOTAccountHolders(Action<List<string>, Exception> callback);
        /// <summary>
        /// Gets the market participants.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="marketKey">The market key.</param>
        void GetMarketParticipants(Action<List<string>, Exception> callback, int marketKey);

        List<string> Get6MonthAuctionDate(string year);
        List<int> GetAnnualAuctionKey(string round, string year);
        /// <summary>
        /// Gets the date list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetDateList(Action<List<DateTime>, Exception> callback, int marketKey);


        Dictionary<int, PeriodDays> GetPeriodDays(string SelectedMarket);

        PeriodHours GetPeriodHours(DateTime startDate, DateTime endDate, int? marketKey);

        List<SourceSink> GetFTRs(int marketkey, List<string> accounts, DateTime period);

        List<SourceSink> Get6MonthsCRRs(int key, List<string> accounts, DateTime period, string rounds);
    }
}
