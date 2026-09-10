using System;
using System.Collections.Generic;

namespace Vayu.CRRAuction.Model
{ /// <summary>
  /// 
  /// </summary>
    public interface IDataService
    {
        // void GetData(Action<DataItem, Exception> callback);
        /// <summary>
        /// Gets the auction data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="market">The market.</param>
        void GetAuctionData(Action<List<FtrAuctionType>, Exception> callback, string market);
    }
    /// <summary>
    /// 
    /// </summary>
    public class FtrAuctionType
    {
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public string Market { get; set; }
        /// <summary>
        /// Gets or sets the FTR auction key.
        /// </summary>
        /// <value>
        /// The FTR auction key.
        /// </value>
        public int FTRAuctionKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the FTR auction.
        /// </summary>
        /// <value>
        /// The name of the FTR auction.
        /// </value>
        public string FTRAuctionName { get; set; }
        /// <summary>
        /// Gets or sets the auction start date.
        /// </summary>
        /// <value>
        /// The auction start date.
        /// </value>
        public DateTime AuctionStartDate { get; set; }
        /// <summary>
        /// Gets or sets the auction end date.
        /// </summary>
        /// <value>
        /// The auction end date.
        /// </value>
        public DateTime AuctionEndDate { get; set; }
        /// <summary>
        /// Gets or sets the auction round.
        /// </summary>
        /// <value>
        /// The auction round.
        /// </value>
        public int AuctionRound { get; set; }
        /// <summary>
        /// Gets or sets the result posted date.
        /// </summary>
        /// <value>
        /// The result posted date.
        /// </value>
        public DateTime ResultPostedDate { get; set; }
        /// <summary>
        /// Gets or sets the credit posted date.
        /// </summary>
        /// <value>
        /// The credit posted date.
        /// </value>
        public DateTime CreditPostedDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is old.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is old; otherwise, <c>false</c>.
        /// </value>
        public bool IsOld { get; set; }
        /// <summary>
        /// Gets or sets the FTR auction period.
        /// </summary>
        /// <value>
        /// The FTR auction period.
        /// </value>
        public string FTRAuctionPeriod { get; set; }
    }
}
