using System;

namespace Vayu.CityTemperatureServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class Temperature
    {
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
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double Longitude { get; set; }
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double Latitude { get; set; }
        /// <summary>
        /// Gets or sets the climate date.
        /// </summary>
        /// <value>
        /// The climate date.
        /// </value>
        public DateTime ClimateDate { get; set; }
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public string Market { get; set; }
        /// <summary>
        /// Gets or sets the temporary value.
        /// </summary>
        /// <value>
        /// The temporary value.
        /// </value>
        public int TempVal { get; set; }
        /// <summary>
        /// Gets or sets the precipitation.
        /// </summary>
        /// <value>
        /// The precipitation.
        /// </value>
        public int Precipitation { get; set; }
        /// <summary>
        /// Gets or sets the cloud cover.
        /// </summary>
        /// <value>
        /// The cloud cover.
        /// </value>
        public int CloudCover { get; set; }
        /// <summary>
        /// Gets or sets the dew point.
        /// </summary>
        /// <value>
        /// The dew point.
        /// </value>
        public int DewPoint { get; set; }
        /// <summary>
        /// Gets or sets the wind speed.
        /// </summary>
        /// <value>
        /// The wind speed.
        /// </value>
        public int WindSpeed { get; set; }
        /// <summary>
        /// Gets or sets the relative humidity.
        /// </summary>
        /// <value>
        /// The relative humidity.
        /// </value>
        public int RelativeHumidity { get; set; }
        /// <summary>
        /// Gets or sets the wind direction.
        /// </summary>
        /// <value>
        /// The wind direction.
        /// </value>
        public int WindDirection { get; set; }

        /// <summary>
        /// Gets or sets the humidity.
        /// </summary>
        /// <value>
        /// The humidity.
        /// </value>
        public double Humidity { get; set; }
        /// <summary>
        /// Gets or sets the rain.
        /// </summary>
        /// <value>
        /// The rain.
        /// </value>
        public double Rain { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the clouds.
        /// </summary>
        /// <value>
        /// The clouds.
        /// </value>
        public double Clouds { get; set; }
        public double WindGust { get; set; }

        public double MinTemp { get; set; }
        public double MaxTemp { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChartTemperature
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public int Hour { get; set; }
        /// <summary>
        /// Gets or sets the temperature value.
        /// </summary>
        /// <value>
        /// The temperature value.
        /// </value>
        public double? TemperatureValue { get; set; }
        /// <summary>
        /// Gets or sets the cloud cover.
        /// </summary>
        /// <value>
        /// The cloud cover.
        /// </value>
        public double? CloudCover { get; set; }
        /// <summary>
        /// Gets or sets the dew point.
        /// </summary>
        /// <value>
        /// The dew point.
        /// </value>
        public double? DewPoint { get; set; }
        /// <summary>
        /// Gets or sets the precip.
        /// </summary>
        /// <value>
        /// The precip.
        /// </value>
        public double? Precip { get; set; }
        /// <summary>
        /// Gets or sets the wind speed.
        /// </summary>
        /// <value>
        /// The wind speed.
        /// </value>
        public double? WindSpeed { get; set; }
    }
}
