using System;

namespace Vayu.LoadCurve.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class LoadDataItem
    {
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime { get; set; }
        /// <summary>
        /// Gets or sets the load forecast.
        /// </summary>
        /// <value>
        /// The load forecast.
        /// </value>
        public double? LoadForecast { get; set; }
    }
}
