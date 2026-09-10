namespace Vayu.VirtualBidSubmissionLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class VirtualBid
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public int Hour { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is inc.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is inc; otherwise, <c>false</c>.
        /// </value>
        public bool IsInc { get; set; }
        /// <summary>
        /// Gets or sets the segment.
        /// </summary>
        /// <value>
        /// The segment.
        /// </value>
        public int Segment { get; set; }
        /// <summary>
        /// Gets or sets the bid identifier.
        /// </summary>
        /// <value>
        /// The bid identifier.
        /// </value>
        public string BidId { get; set; }
    }
}
