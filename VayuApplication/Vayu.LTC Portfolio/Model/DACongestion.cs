using System;

namespace Vayu.LTC_Portfolio.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ICloneable" />
    public class DACongestion : ICloneable
    {
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime { get; set; }
        /// <summary>
        /// Gets or sets the peak.
        /// </summary>
        /// <value>
        /// The peak.
        /// </value>
        public double Peak { set; get; }
        /// <summary>
        /// Gets or sets the off peak.
        /// </summary>
        /// <value>
        /// The off peak.
        /// </value>
        public double OffPeak { set; get; }
        public double PeakWE { set; get; }
        /// <summary>
        /// Gets or sets the peak hours.
        /// </summary>
        /// <value>
        /// The peak hours.
        /// </value>
        public int PeakHours { set; get; }
        /// <summary>
        /// Gets or sets the off peak hours.
        /// </summary>
        /// <value>
        /// The off peak hours.
        /// </value>
        public int OffPeakHours { set; get; }
        public int PeakWEHours { set; get; }
        /// <summary>
        /// pnl
        /// </summary>
        public double pnl { set; get; }
        public double Peakpnl { set; get; }
        public double Offpnl { set; get; }

        public double PeakWEpnl { set; get; }
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
