using System;

namespace Vayu.WorkbookStatistics.Model
{
    public class DeenergizedNode
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public DateTime MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public int Hours { get; set; }
        /// <summary>
        /// Gets or sets the flag.
        /// </summary>
        /// <value>
        /// The flag.
        /// </value>
        public string Flag { get; set; }
    }
}
