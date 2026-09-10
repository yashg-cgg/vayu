namespace Vayu.WorkbookStatistics.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Sensitivity
    {
        /// <summary>
        /// Gets or sets the sensitivity value.
        /// </summary>
        /// <value>
        /// The sensitivity value.
        /// </value>
        public double SensitivityValue { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }
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
        /// Gets or sets the shift factor.
        /// </summary>
        /// <value>
        /// The shift factor.
        /// </value>
        public double ShiftFactor { get; set; }
        /// <summary>
        /// Gets or sets the dollar impact.
        /// </summary>
        /// <value>
        /// The dollar impact.
        /// </value>
        public double DollarImpact { get; set; }
        /// <summary>
        /// Gets or sets the type of the risk.
        /// </summary>
        /// <value>
        /// The type of the risk.
        /// </value>
        public string RiskType { get; set; }
    }
}
