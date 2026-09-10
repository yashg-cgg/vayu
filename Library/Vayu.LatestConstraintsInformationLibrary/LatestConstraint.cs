using System;

namespace Vayu.LatestConstraintsInformationLibrary
{

    public class LatestConstraint
    {

        public int ConstraintKey { get; set; }

        public DateTime MarketDate { get; set; }

        public string ContigencyText { get; set; }
        /// <summary>
        /// Gets or sets the constraint text.
        /// </summary>
        /// <value>
        /// The constraint text.
        /// </value>
        public string ConstraintText { get; set; }
        /// <summary>
        /// Gets or sets the shadow price.
        /// </summary>
        /// <value>
        /// The shadow price.
        /// </value>
        public double ShadowPrice { get; set; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }
        /// <summary>
        /// Gets or sets the monitored facility.
        /// </summary>
        /// <value>
        /// The monitored facility.
        /// </value>
        public string MonitoredFacility { get; set; }
        /// <summary>
        /// Gets or sets the TLR level.
        /// </summary>
        /// <value>
        /// The TLR level.
        /// </value>
        public string TLRLevel { get; set; }

        /// <summary>
        /// Gets or sets the source node key.
        /// </summary>
        /// <value>
        /// The source node key.
        /// </value>
        public int SourceNodeKey { get; set; }
        /// <summary>
        /// Gets or sets the sink node key.
        /// </summary>
        /// <value>
        /// The sink node key.
        /// </value>
        public int SinkNodeKey { get; set; }
        /// <summary>
        /// Gets or sets the shadow price na n.
        /// </summary>
        /// <value>
        /// The shadow price na n.
        /// </value>
        public double ShadowPriceNaN { get; set; }
        /// <summary>
        /// Gets or sets the type of the constraint.
        /// </summary>
        /// <value>
        /// The type of the constraint.
        /// </value>
        public string ConstraintType { get; set; }
        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public int Duration { get; set; }
        /// <summary>
        /// Gets or sets the constraint kv.
        /// </summary>
        /// <value>
        /// The constraint kv.
        /// </value>
        public double ConstraintKV { get; set; }
        /// <summary>
        /// Gets or sets the contingency kv.
        /// </summary>
        /// <value>
        /// The contingency kv.
        /// </value>
        public double ContingencyKV { get; set; }

        public double MaxSHadowPrice { get; set; }
        /// <summary>
        /// Gets or sets the source zone.
        /// </summary>
        /// <value>
        /// The source zone.
        /// </value>
        public string SourceZone { get; set; }
        /// <summary>
        /// Gets or sets the sink zone.
        /// </summary>
        /// <value>
        /// The sink zone.
        /// </value>
        public string SinkZone { get; set; }
        //Add
        public string ContingencyDesc { get; set; }
        public string MonitoredID1 { get; set; }
        public string MonitoredID2 { get; set; }
        public string RatingType { get; set; }
        public double RatingMW { get; set; }
        public double PostCTGFlowMW { get; set; }
        public double PercentViolation { get; set; }
        public string DSTFlag { get; set; }
        public double ConstrainedSCEDLimitMW { get; set; }
        public double SCEDRatingMVA { get; set; }
        public double PostCTGFlowMVA { get; set; }

        public string FromStation { get; set; }
        public string ToStation { get; set; }
        public double FromKV { get; set; }
        public double ToKV { get; set; }
    }
}
