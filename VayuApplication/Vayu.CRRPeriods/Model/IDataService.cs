using System;
using System.Collections.Generic;

namespace Vayu.CRRPeriods.Model
{
    public interface IDataService
    {
        //void GetData(Action<DataItem, Exception> callback);
        /// <summary>
        /// Gets the period data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetPeriodData(Action<List<FtrPeriod>, Exception> callback, string market);
    }
    /// <summary>
    /// 
    /// </summary>
    public class FtrPeriod
    {
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the period code.
        /// </summary>
        /// <value>
        /// The period code.
        /// </value>
        public string PeriodCode { get; set; }
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public string Market { get; set; }
        /// <summary>
        /// Gets or sets the name of the period.
        /// </summary>
        /// <value>
        /// The name of the period.
        /// </value>
        public string PeriodName { get; set; }
        /// <summary>
        /// Gets or sets the type of the period.
        /// </summary>
        /// <value>
        /// The type of the period.
        /// </value>
        public string PeriodType { get; set; }
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Gets or sets the peak hours.
        /// </summary>
        /// <value>
        /// The peak hours.
        /// </value>
        public int PeakHours { get; set; }
        /// <summary>
        /// Gets or sets the off peak HRS.
        /// </summary>
        /// <value>
        /// The off peak HRS.
        /// </value>
        public int OffPeakHrs { get; set; }
        /// <summary>
        /// Gets or sets the hours24.
        /// </summary>
        /// <value>
        /// The hours24.
        /// </value>
        public int Hours24 { get; set; }
        /// <summary>
        /// Gets or sets the credit date.
        /// </summary>
        /// <value>
        /// The credit date.
        /// </value>
        public DateTime CreditDate { get; set; }
        /// Gets or sets the PeakWE.
        /// </summary>
        /// <value>
        /// The PeakWE.
        public int PeakWE { get; set; }
    }
}
