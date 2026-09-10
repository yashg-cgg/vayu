namespace Vayu.ConstraintSensitivityAlgorithm.Model
{
    public class Vector
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
        /// Gets or sets the source sensitivity.
        /// </summary>
        /// <value>
        /// The source sensitivity.
        /// </value>
        public double SourceSensitivity { get; set; }
        /// <summary>
        /// Gets or sets the sink sensitivity.
        /// </summary>
        /// <value>
        /// The sink sensitivity.
        /// </value>
        public double SinkSensitivity { get; set; }
        /// <summary>
        /// Gets or sets the sensitivity.
        /// </summary>
        /// <value>
        /// The sensitivity.
        /// </value>
        public double Sensitivity { get; set; }
    }
}
