using System;

namespace Vayu.WindServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class LatestWindData
    {
        /// <summary>
        /// Gets or sets the type of the wind.
        /// </summary>
        /// <value>
        /// The type of the wind.
        /// </value>
        public string WindType { get; set; }
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public DateTime MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public double Value { get; set; }

        public double RTSouth_Houston { get; set; }
        public double RTWest { get; set; }
        public double RTNorth { get; set; }
        public double RTSouth { get; set; }
        public double ForeCastSouth_Houston { get; set; }
        public double ForeCastWest { get; set; }
        public double ForeCastNorth { get; set; }

        public double RTPANHANDLE { get; set; }
        public double ForecastPANHANDLE { get; set; }
        public double RTCOASTAL { get; set; }
        public double ForecastCOASTAL { get; set; }
        public double ForecastSouth { get; set; }
        public double RTWestRegion { get; set; }
        public double ForecastWestRegion { get; set; }
        public double RTNorthRegion { get; set; }
        public double ForecastNorthRegion { get; set; }
    }
}
