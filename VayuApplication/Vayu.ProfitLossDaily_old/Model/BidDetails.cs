using System;

namespace Vayu.ProfitLossDaily.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class BidDetails
    {
        /// <summary>
        /// Gets or sets the name of the portfolio.
        /// </summary>
        /// <value>
        /// The name of the portfolio.
        /// </value>
        public string PortfolioName { get; set; }
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public DateTime MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the he.
        /// </summary>
        /// <value>
        /// The he.
        /// </value>
        public int HE { get; set; }
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
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the inc decimal.
        /// </summary>
        /// <value>
        /// The inc decimal.
        /// </value>
        public string IncDEC { get; set; }
    }
}
