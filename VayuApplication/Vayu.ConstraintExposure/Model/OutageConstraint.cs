using System;

namespace Vayu.ConstraintExposure.Model
{
    /// <summary>
    /// 
    /// </summary>
    class OutageConstraint
    {
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
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime { get; set; }
        /// <summary>
        /// Gets or sets the constraint rt number.
        /// </summary>
        /// <value>
        /// The constraint rt number.
        /// </value>
        public string constraintRTNum { get; set; }
    }
}
