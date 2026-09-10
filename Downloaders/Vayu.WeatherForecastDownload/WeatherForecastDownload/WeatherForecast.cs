using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Vayu.CommonAccessLibrary;
namespace Vayu.WeatherForecastDownload
{
    /// <summary>
    /// Weather Properties
    /// </summary>
    public class Weather
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the city no.
        /// </summary>
        /// <value>
        /// The city no.
        /// </value>
        public int CityNo { get; set; }
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string LABEL { get; set; }
        /// <summary>
        /// Gets or sets the icao code.
        /// </summary>
        /// <value>
        /// The icao code.
        /// </value>
        public string ICAOCode { get; set; } 
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    class WeatherForecast
    {
        #region Private Members
        /// <summary>
        /// The Sigma database connection
        /// </summary>
 
        private SqlConnection VayuDBConnection;
 
        /// <summary>
        /// The command select city no command
        /// </summary>
        private SqlCommand cmdSelectCityNoCommand;
        /// <summary>
        /// The command select previous forecast command
        /// </summary>
        private SqlCommand cmdSelectPrevForecastCommand;
        /// <summary>
        /// The command update weather command
        /// </summary>
        private SqlCommand cmdUpdateWeatherCommand;
        /// <summary>
        /// The command insert weather command
        /// </summary>
        private SqlCommand cmdInsertWeatherCommand;
        /// <summary>
        /// The LST weather list
        /// </summary>
        private List<Weather> lstWeatherList; 
        #endregion

        /// <summary>
        /// The s timer
        /// </summary>
        System.Timers.Timer sTimer = new System.Timers.Timer();

        /// <summary>
        /// Initializes a new instance of the WeatherForecast class.
        /// </summary>
        public WeatherForecast()
        {
            
            
        }

        #region Private Methods
       
       
        #endregion

        #region Public Metods
        /// <summary>
        /// Unixes the time stamp to date time.
        /// </summary>
        /// <param name="unixTimeStamp">The unix time stamp.</param>
        /// <returns>DateTime</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        } 
        #endregion
    }
}
