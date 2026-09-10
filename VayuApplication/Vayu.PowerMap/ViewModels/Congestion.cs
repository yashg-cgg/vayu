using System;

namespace Vayu.PowerMap.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class Congestion
    {
        /// <summary>
        /// Gets or sets the equipment.
        /// </summary>
        /// <value>
        /// The equipment.
        /// </value>
        public string Equipment { get; set; }
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public DateTime MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the shadow price.
        /// </summary>
        /// <value>
        /// The shadow price.
        /// </value>
        public double ShadowPrice { get; set; }
        /// <summary>
        /// Gets or sets the maximum load.
        /// </summary>
        /// <value>
        /// The maximum load.
        /// </value>
        public double? MaxLoad { get; set; }
        /// <summary>
        /// Gets or sets the name of the constraint.
        /// </summary>
        /// <value>
        /// The name of the constraint.
        /// </value>
        public string ConstraintName { get; set; }
        /// <summary>
        /// Gets or sets the name of the contingency.
        /// </summary>
        /// <value>
        /// The name of the contingency.
        /// </value>
        public string ContingencyName { get; set; }
    }
}
