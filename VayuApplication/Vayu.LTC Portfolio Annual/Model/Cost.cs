using System;
using System.Collections.Generic;

namespace Vayu.LTC_PortfolioAnnual.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ICloneable" />
    public class Cost : ICloneable
    {
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the ex node key.
        /// </summary>
        /// <value>
        /// The ex node key.
        /// </value>
        public int ExNodeKey { get; set; }
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
        public double PeakWE { set; get; }
        /// <summary>
        /// Gets or sets the da congestion list.
        /// </summary>
        /// <value>
        /// The da congestion list.
        /// </value>
        public List<DACongestion> DACongestionList { set; get; }

        public List<DACongestion> RTCongestionList { set; get; }
        /// <summary>
        /// The da hash
        /// </summary>
        public Dictionary<DateTime, DACongestion> daHash = new Dictionary<DateTime, DACongestion>();

        public Dictionary<DateTime, DACongestion> rtHash = new Dictionary<DateTime, DACongestion>();

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            Cost cc = this.MemberwiseClone() as Cost;
            if (this.DACongestionList != null)
                cc.DACongestionList = new List<DACongestion>();
            else
                return cc;
            if (this.RTCongestionList != null)
                cc.RTCongestionList = new List<DACongestion>();
            else
                return cc;

            foreach (var item in this.DACongestionList)
                cc.DACongestionList.Add(item.Clone() as DACongestion);
            foreach (var item in this.RTCongestionList)
                cc.RTCongestionList.Add(item.Clone() as DACongestion);

            return cc;
        }
    }
}
