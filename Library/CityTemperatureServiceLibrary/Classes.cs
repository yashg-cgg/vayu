using System;

namespace Vayu.CityTemperatureServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class WeatherLocation
    {
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; set; }
        /// <summary>
        /// Gets or sets the DTN code.
        /// </summary>
        /// <value>
        /// The DTN code.
        /// </value>
        public string DTNCode { get; set; }
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get; set; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public string Market { get; set; }
        /// <summary>
        /// Gets or sets the tesla table.
        /// </summary>
        /// <value>
        /// The tesla table.
        /// </value>
        public string TeslaTable { get; set; }
        /// <summary>
        /// Gets or sets the tesla frozen table.
        /// </summary>
        /// <value>
        /// The tesla frozen table.
        /// </value>
        public string TeslaFrozenTable { get; set; }
        /// <summary>
        /// Gets or sets the DTN table.
        /// </summary>
        /// <value>
        /// The DTN table.
        /// </value>
        public string DTNTable { get; set; }
        /// <summary>
        /// Gets or sets the DTN frozen table.
        /// </summary>
        /// <value>
        /// The DTN frozen table.
        /// </value>
        public string DTNFrozenTable { get; set; }
        /// <summary>
        /// Gets or sets the group label.
        /// </summary>
        /// <value>
        /// The group label.
        /// </value>
        public string GroupLabel { get; set; }
        /// <summary>
        /// Gets or sets the group display order.
        /// </summary>
        /// <value>
        /// The group display order.
        /// </value>
        public int GroupDisplayOrder { get; set; }
        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        /// <value>
        /// The display order.
        /// </value>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return City + "-" + State;
        }

        /// <summary>
        /// Hashes the string.
        /// </summary>
        /// <returns></returns>
        public string hashString()
        {
            return City + "-" + State + "-" + Market;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum WeatherCompareType
    {
        /// <summary>
        /// The current to frozen
        /// </summary>
        CurrentToFrozen,
        /// <summary>
        /// The compare date to frozen
        /// </summary>
        CompareDateToFrozen,
        /// <summary>
        /// The current to compare date
        /// </summary>
        CurrentToCompareDate
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ComparisionType
    {
        /// <summary>
        /// The frozen forecast
        /// </summary>
        FrozenForecast,
        /// <summary>
        /// The currentvs compare
        /// </summary>
        CurrentvsCompare,
        /// <summary>
        /// The forzen f CVS compare date
        /// </summary>
        ForzenFCvsCompareDate
    }

    /// <summary>
    /// 
    /// </summary>
    public class TimeSpanItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanItem"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        public TimeSpanItem(ChartTemperature x)
        {
            X = new TimeSpan(x.Hour, 0, 0);
            DX = x.Date;
            TempY = x.TemperatureValue;
            DewY = x.DewPoint;
            CloudY = x.CloudCover;
            PrecipY = x.Precip;
            WindY = x.WindSpeed;
        }

        /// <summary>
        /// Gets or sets the dx.
        /// </summary>
        /// <value>
        /// The dx.
        /// </value>
        public DateTime DX { get; set; }
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public TimeSpan X { get; set; }

        /// <summary>
        /// Gets or sets the temporary y.
        /// </summary>
        /// <value>
        /// The temporary y.
        /// </value>
        public double? TempY { get; set; }
        /// <summary>
        /// Gets or sets the dew y.
        /// </summary>
        /// <value>
        /// The dew y.
        /// </value>
        public double? DewY { get; set; }
        /// <summary>
        /// Gets or sets the cloud y.
        /// </summary>
        /// <value>
        /// The cloud y.
        /// </value>
        public double? CloudY { get; set; }
        /// <summary>
        /// Gets or sets the precip y.
        /// </summary>
        /// <value>
        /// The precip y.
        /// </value>
        public double? PrecipY { get; set; }
        /// <summary>
        /// Gets or sets the wind y.
        /// </summary>
        /// <value>
        /// The wind y.
        /// </value>
        public double? WindY { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AreaItem
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public double X { get; set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double Y { get; set; }
        /// <summary>
        /// Gets or sets the zero.
        /// </summary>
        /// <value>
        /// The zero.
        /// </value>
        public double Zero { get; set; }
    }
}
