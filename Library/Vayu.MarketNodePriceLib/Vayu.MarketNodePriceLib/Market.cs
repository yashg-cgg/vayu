namespace Vayu.MarketNodePriceLib
{
    public class Market
    {
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public decimal MarketKey { get; set; }
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the timezone.
        /// </summary>
        /// <value>
        /// The timezone.
        /// </value>
        public string Timezone { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [market time observes DST].
        /// </summary>
        /// <value>
        /// <c>true</c> if [market time observes DST]; otherwise, <c>false</c>.
        /// </value>
        public bool MarketTimeObservesDST { get; set; }
        /// <summary>
        /// Gets or sets the he interval minute.
        /// </summary>
        /// <value>
        /// The he interval minute.
        /// </value>
        public int? HEIntervalMinute { get; set; }
        /// <summary>
        /// Gets or sets the minutes in interval.
        /// </summary>
        /// <value>
        /// The minutes in interval.
        /// </value>
        public int? MinutesInInterval { get; set; }
    }
}
