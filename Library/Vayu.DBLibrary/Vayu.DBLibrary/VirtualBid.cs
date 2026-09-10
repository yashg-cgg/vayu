using System.Collections.Generic;

namespace Vayu.DBLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class PriceMW
    {
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double? Price { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double? MW { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class VirtualBid
    {
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the segment.
        /// </summary>
        /// <value>
        /// The segment.
        /// </value>
        public int Segment { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is inc.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is inc; otherwise, <c>false</c>.
        /// </value>
        public bool IsInc { get; set; }
        /// <summary>
        /// Gets or sets the hour hash.
        /// </summary>
        /// <value>
        /// The hour hash.
        /// </value>
        public Dictionary<int, PriceMW> HourHash { get; set; }
    }
}
