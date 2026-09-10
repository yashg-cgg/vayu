using System;
using System.Collections.Generic;

namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.PowerMap.Outage" />
    public class OutageCorrelationModel : Outage
    {
        /// <summary>
        /// Gets or sets the outagetimes.
        /// </summary>
        /// <value>
        /// The outagetimes.
        /// </value>
        public List<OutageTimes> outagetimes { get; set; }
        /// <summary>
        /// Gets or sets the constraintgroup.
        /// </summary>
        /// <value>
        /// The constraintgroup.
        /// </value>
        public List<ConstraintsGroup> constraintgroup { get; set; }
        /// <summary>
        /// Gets or sets the timestring.
        /// </summary>
        /// <value>
        /// The timestring.
        /// </value>
        public string timestring { get; set; }
        /// <summary>
        /// Gets or sets the stationlevelhash.
        /// </summary>
        /// <value>
        /// The stationlevelhash.
        /// </value>
        public Dictionary<int, List<string>> stationlevelhash { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OutageTimes
    {
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.PowerMap.Constraintobj" />
    public class ConstraintsGroup : Constraintobj
    {
        /// <summary>
        /// Gets or sets the constraint occurrences.
        /// </summary>
        /// <value>
        /// The constraint occurrences.
        /// </value>
        public int ConstraintOccurrences { get; set; }
        /// <summary>
        /// The distancemiles
        /// </summary>
        private double _distancemiles = double.NaN;
        /// <summary>
        /// Gets or sets the distancemiles.
        /// </summary>
        /// <value>
        /// The distancemiles.
        /// </value>
        public double distancemiles
        {
            get { return _distancemiles; }
            set { _distancemiles = value; }
        }
        /// <summary>
        /// The busaway
        /// </summary>
        private double _busaway = double.NaN;
        /// <summary>
        /// Gets or sets the busaway.
        /// </summary>
        /// <value>
        /// The busaway.
        /// </value>
        public double busaway
        {
            get { return _busaway; }
            set { _busaway = value; }
        }
    }
}
