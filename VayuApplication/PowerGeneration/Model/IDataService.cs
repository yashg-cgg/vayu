using System;
using System.Collections.Generic;
using Vayu.WindServiceLibrary;

namespace Vayu.PowerGeneration.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Gets the wind data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        void GetWindData(Action<Dictionary<string, List<WindData>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone);

        void GetSolarData(Action<List<LatestWindData>, Exception> callback, DateTime fromDate, DateTime toDate, string zone);
    }

    /// <summary>
    /// 
    /// </summary>
    public class WindData
    {
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public DateTime MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public double Value { get; set; }
    }
}
