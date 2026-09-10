using System;

namespace Vayu.NodeLMPLibrary
{
    /// <summary>
    /// MarketTime
    /// </summary>
    public class MarketTime
    {
        /// <summary>
        /// The market date time
        /// </summary>
        internal DateTime marketDateTime;
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime
        {
            get { return marketDateTime; }
            set { marketDateTime = value; }
        }

        /// <summary>
        /// The peak yn
        /// </summary>
        internal string peakYN;
        /// <summary>
        /// Gets or sets the peak yn.
        /// </summary>
        /// <value>
        /// The peak yn.
        /// </value>
        public string PeakYN
        {
            get { return peakYN; }
            set { peakYN = value; }
        }

        /// <summary>
        /// The market hour
        /// </summary>
        internal int marketHour;
        /// <summary>
        /// Gets or sets the market hour.
        /// </summary>
        /// <value>
        /// The market hour.
        /// </value>
        public int MarketHour
        {
            get { return marketHour; }
            set { marketHour = value; }
        }

        /// <summary>
        /// The index market hour
        /// </summary>
        private int indexMarketHour;
        /// <summary>
        /// Gets or sets the index market hour.
        /// </summary>
        /// <value>
        /// The index market hour.
        /// </value>
        internal int IndexMarketHour
        {
            get { return indexMarketHour; }
            set { indexMarketHour = value; }
        }
    }
}
