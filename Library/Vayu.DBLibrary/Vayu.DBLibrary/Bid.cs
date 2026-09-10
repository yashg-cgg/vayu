using System;

namespace Vayu.DBLibrary
{

    /// <summary>
    /// 
    /// </summary>
    public class Bid
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public int Source { get; set; }

        public long SourcePnodeId { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public int Sink { get; set; }

        public long SinkPnodeId { get; set; }
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
        /// Gets or sets the segment.
        /// </summary>
        /// <value>
        /// The segment.
        /// </value>
        public int Segment { get; set; }
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime { get; set; }
        /// <summary>
        /// Gets or sets the bid identifier.
        /// </summary>
        /// <value>
        /// The bid identifier.
        /// </value>
        public string BidId { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
        /// <summary>
        /// Gets or sets the portfolio key.
        /// </summary>
        /// <value>
        /// The portfolio key.
        /// </value>
        public int PortfolioKey { get; set; }
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public int Market { get; set; }
        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public string File { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is uptos.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is uptos; otherwise, <c>false</c>.
        /// </value>
        public bool IsUptos { get; set; }
        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        public string Comments { get; set; }

        /// <summary>
        /// Gets the node group.
        /// </summary>
        /// <value>
        /// The node group.
        /// </value>
        public string NodeGroup { get { return Source.ToString() + " " + Sink.ToString(); } }
    }
}
