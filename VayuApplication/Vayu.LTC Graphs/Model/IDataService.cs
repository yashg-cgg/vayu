using System;
using System.Collections.Generic;
using Vayu.DBLibrary;
using Vayu.LTC_Graphs.Design;

namespace Vayu.LTC_Graphs.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Gets the dart prices.
        /// </summary>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="from">From.</param>
        /// <param name="thro">The thro.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        Dictionary<Interval, List<LmpHelper>> GetDARTPrices(SourceSinkDetail sourceSink, DateTime from, DateTime thro, int marketKey, HourType CurrentHourType);
        //Dictionary<HourType, Dictionary<DateTime, double>> GetCRRPrice(SourceSinkNode sourceSink, DateTime start, DateTime end, int market);
        /// <summary>
        /// Gets all CRR data.
        /// </summary>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        Dictionary<HourType, Dictionary<int, CRR>> GetAllCRRData(SourceSinkDetail sourceSink, DateTime start, DateTime end, int market);
    }
}
