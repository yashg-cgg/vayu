namespace Vayu.LTC_Portfolio.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PNL
    {
        /// <summary>
        /// Gets or sets the source sink.
        /// </summary>
        /// <value>
        /// The source sink.
        /// </value>
        public string SourceSink { get; set; }
        /// <summary>
        /// Gets or sets the da.
        /// </summary>
        /// <value>
        /// The da.
        /// </value>
        public double DA { get; set; }
        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public double Cost { get; set; }
        /// <summary>
        /// Gets or sets the dart.
        /// </summary>
        /// <value>
        /// The dart.
        /// </value>
        public double DART { get; set; }

        public double RT { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PNL"/> class.
        /// </summary>
        public PNL()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PNL"/> class.
        /// </summary>
        /// <param name="fromPnl">From PNL.</param>
        public PNL(PNL fromPnl)
        {
            SourceSink = fromPnl.SourceSink;
            DA = fromPnl.DA;
            Cost = fromPnl.Cost;
            DART = fromPnl.DART;
            RT = fromPnl.RT;
        }
    }
}
