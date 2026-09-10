using System.Collections.Generic;

namespace Vayu.LoadGraphLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class Load
    {
        /// <summary>
        /// Gets or sets the forecast.
        /// </summary>
        /// <value>
        /// The forecast.
        /// </value>
        public int Forecast { get; set; }
        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public int Current { get; set; }
        /// <summary>
        /// Gets or sets the tesla.
        /// </summary>
        /// <value>
        /// The tesla.
        /// </value>
        public string Tesla { get; set; }
        /// <summary>
        /// Gets or sets the PRT.
        /// </summary>
        /// <value>
        /// The PRT.
        /// </value>
        public string PRT { get; set; }
        /// <summary>
        /// Gets or sets the day ahead.
        /// </summary>
        /// <value>
        /// The day ahead.
        /// </value>
        public string DayAhead { get; set; }
        /// <summary>
        /// Gets or sets the wsi.
        /// </summary>
        /// <value>
        /// The wsi.
        /// </value>
        public string WSI { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HashValues
    {
        /// <summary>
        /// Gets or sets the name of the zone.
        /// </summary>
        /// <value>
        /// The name of the zone.
        /// </value>
        public string ZoneName { get; set; }
        /// <summary>
        /// Gets or sets the hour zero hash.
        /// </summary>
        /// <value>
        /// The hour zero hash.
        /// </value>
        public Dictionary<int, Dictionary<int, double>> HourZeroHash { get; set; }
        /// <summary>
        /// Gets or sets the hour five hash.
        /// </summary>
        /// <value>
        /// The hour five hash.
        /// </value>
        public Dictionary<int, Dictionary<int, double>> HourFiveHash { get; set; }
        /// <summary>
        /// Gets or sets the hour1 hash.
        /// </summary>
        /// <value>
        /// The hour1 hash.
        /// </value>
        public Dictionary<int, double> Hour1Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour2 hash.
        /// </summary>
        /// <value>
        /// The hour2 hash.
        /// </value>
        public Dictionary<int, double> Hour2Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour3 hash.
        /// </summary>
        /// <value>
        /// The hour3 hash.
        /// </value>
        public Dictionary<int, double> Hour3Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour4 hash.
        /// </summary>
        /// <value>
        /// The hour4 hash.
        /// </value>
        public Dictionary<int, double> Hour4Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour6 hash.
        /// </summary>
        /// <value>
        /// The hour6 hash.
        /// </value>
        public Dictionary<int, double> Hour6Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour7 hash.
        /// </summary>
        /// <value>
        /// The hour7 hash.
        /// </value>
        public Dictionary<int, double> Hour7Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour8 hash.
        /// </summary>
        /// <value>
        /// The hour8 hash.
        /// </value>
        public Dictionary<int, double> Hour8Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour9 hash.
        /// </summary>
        /// <value>
        /// The hour9 hash.
        /// </value>
        public Dictionary<int, double> Hour9Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour10 hash.
        /// </summary>
        /// <value>
        /// The hour10 hash.
        /// </value>
        public Dictionary<int, double> Hour10Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour11 hash.
        /// </summary>
        /// <value>
        /// The hour11 hash.
        /// </value>
        public Dictionary<int, double> Hour11Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour12 hash.
        /// </summary>
        /// <value>
        /// The hour12 hash.
        /// </value>
        public Dictionary<int, double> Hour12Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour13 hash.
        /// </summary>
        /// <value>
        /// The hour13 hash.
        /// </value>
        public Dictionary<int, double> Hour13Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour14 hash.
        /// </summary>
        /// <value>
        /// The hour14 hash.
        /// </value>
        public Dictionary<int, double> Hour14Hash { get; set; }
        /// <summary>
        /// Gets or sets the hour15 hash.
        /// </summary>
        /// <value>
        /// The hour15 hash.
        /// </value>
        public Dictionary<int, double> Hour15Hash { get; set; }
    }
}
