namespace Vayu.RT_Constraint_impact.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Constraints
    {
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public string MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Constraint { get; set; }
        /// <summary>
        /// Gets or sets the contingency.
        /// </summary>
        /// <value>
        /// The contingency.
        /// </value>
        public string Contingency { get; set; }
        /// <summary>
        /// Gets or sets the shadow price.
        /// </summary>
        /// <value>
        /// The shadow price.
        /// </value>
        public double? ShadowPrice { get; set; }
        /// <summary>
        /// Gets or sets the impact.
        /// </summary>
        /// <value>
        /// The impact.
        /// </value>
        public double? Impact { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Constraints"/> is color.
        /// </summary>
        /// <value>
        ///   <c>true</c> if color; otherwise, <c>false</c>.
        /// </value>
        public bool color { get; set; }
    }
}
