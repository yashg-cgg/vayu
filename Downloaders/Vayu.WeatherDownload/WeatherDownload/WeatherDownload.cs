using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading;
using System.Timers;
using Vayu.CommonAccessLibrary;
namespace Vayu.WeatherDownload
{
    /// <summary>
    /// define Weather Properties
    /// </summary>
    public class Weather
    {
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
    }

    /// <summary>
    /// Download Weather
    /// </summary>
    class WeatherDownload
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
        /// The command insert weather command
        /// </summary>
        private SqlCommand cmdInsertWeatherCommand;
        /// <summary>
        /// The LST weather list
        /// </summary>
        private List<Weather> lstWeatherList; 
        #endregion

        /// <summary>
        /// timer
        /// </summary>
        System.Timers.Timer sTimer = new System.Timers.Timer();

        /// <summary>
        /// Initializes a new instance of the WeatherDownload class.
        /// </summary>
        public WeatherDownload()
        {
            LoadDB();
            LoadCity();
            sTimer = new System.Timers.Timer();
            OnTimerEvent(null, null);
            sTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            sTimer.Interval = 3600000;
            sTimer.Start();
            Console.WriteLine("Press \'q\' to quit.");
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }

        #region Private Methods
        /// <summary>
        /// Loads the database related object.
        /// </summary>
        private void LoadDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();;

            cmdSelectCityNoCommand = new SqlCommand();
            cmdSelectCityNoCommand.CommandText = "Select cityNo, Label, ICAOCode from WSIICAOCode";
            cmdSelectCityNoCommand.Connection = VayuDBConnection;
// d095c189a148182b96c6c34129c140cacf917da5
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();;

            cmdSelectCityNoCommand = new SqlCommand();
            cmdSelectCityNoCommand.CommandText = "Select cityNo, Label, ICAOCode from WSIICAOCode";
            cmdSelectCityNoCommand.Connection = VayuDBConnection;
// d095c189a148182b96c6c34129c140cacf917da5
// d095c189a148182b96c6c34129c140cacf917da5

            cmdInsertWeatherCommand = new SqlCommand();
            cmdInsertWeatherCommand.CommandText = "IF Not Exists(Select 'x' From WSICurrent Where ICAOCode = @ICAOCode And MarketDateTime = @MarketDateTime )" +
                                "Insert into WSICurrent(ICAOCode, MarketDateTime, Temperature, Humidity, Rain,Clouds,Description,Dewpoint, HeatIndex, WindChill, WindSpeed, WindDirection, CloudCover, POP, Precipitation) Values (@ICAOCode, @MarketDateTime, @Temperature,@Humidity, @Rain,@Clouds,@Description, Null, Null,  Null,  Null,  Null,  Null,  Null,  Null)";
            cmdInsertWeatherCommand.CommandType = CommandType.Text;
            cmdInsertWeatherCommand.Parameters.AddWithValue("@ICAOCode", "ICAOCode");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Temperature", "Temperature");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Humidity", "Humidity");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Rain", "Rain");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Clouds", "Clouds");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Description", "Description");
 
            cmdInsertWeatherCommand.Connection = VayuDBConnection;
// d095c189a148182b96c6c34129c140cacf917da5
        }

        /// <summary>
        /// Called when [timer event].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The ElapsedEventArgs instance containing the event data.</param>
        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            try
            {
                foreach (var item in lstWeatherList)
                {
                    //if (item.ICAOCode == "KDCA")
                    Download(item.CityNo, item.LABEL, item.ICAOCode);
                }
            }
            catch (Exception ex)
            {
            }
            sTimer.Enabled = true;
        }

        /// <summary>
        /// Downloads the Weather.
        /// </summary>
        /// <param name="cityno">The cityno.</param>
        /// <param name="cityname">The cityname.</param>
        /// <param name="Icaocode">The icaocode.</param>
        private void Download(int cityno, string cityname, string Icaocode)
        {
            try
            {
                #region
                //HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("http://api.openweathermap.org/data/2.5/weather?q=" + cityno + "us&appid=62b73943bcbc8ebc912f49490ee1123a");

                //HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("http://api.openweathermap.org/data/2.5/weather?id=" + cityno + "us&appid=62b73943bcbc8ebc912f49490ee1123a");
                HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("http://api.openweathermap.org/data/2.5/weather?q=" + cityname + " " + "," + "us&appid=62b73943bcbc8ebc912f49490ee1123a");
                wrq.Method = "POST";
                wrq.ContentType = "gzip";
                wrq.Headers.Add("SOAPAction", "text/html");
                wrq.Timeout = 500000;
                StreamReader sr = default(StreamReader);
                StreamWriter sw = default(StreamWriter);
                sw = new StreamWriter(wrq.GetRequestStream());
                sw.Close();
                HttpWebResponse wrp = null;
                try
                {
                    wrp = (HttpWebResponse)wrq.GetResponse();
                }
                catch (Exception ex)
                {
                    return;
                }
                #endregion
                sr = new StreamReader(wrp.GetResponseStream());
                string xmlResponse = sr.ReadToEnd();
                if (xmlResponse.Contains("rain"))
                {

                }
                sr.Close();
                wrp.Close();
                double rain = 0;
                double clouds = 0;
                string[] mainrain = null;

                object o = JsonConvert.SerializeObject(xmlResponse);

                JObject obj = JObject.Parse(xmlResponse);

                var main = ((obj["main"].First).First).ToString().Replace("{", "").Replace("}", "");
                double temp = Convert.ToDouble(main);
                double ftemp = 1.8 * (temp - 273) + 32;
                var dt = ((obj["dt"].Parent).First).ToString().Replace("{", "").Replace("}", ""); 
                double dtime = Convert.ToDouble(dt);
                DateTime datetime = UnixTimeStampToDateTime(dtime);
                var humidity = (((((obj["main"].Parent).First).First).Next).Next).First.ToString().Replace("{", "").Replace("}", "");
                var descrip = ((((obj["weather"].First).First).Next).Next).First.ToString().Replace("{", "").Replace("}", "");
                var clouds1 = (obj["clouds"].First).First.ToString().Replace("{", "").Replace("}", "");
                if (!xmlResponse.Contains("rain"))
                {
                    rain = double.NaN;
                }
                else
                {
                    rain = 0;
                }
                try
                {
                    mainrain = (obj["rain"].ToString()).Replace("\n", "").Replace("\t", "").Replace("\r", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch
                {

                }
                if (mainrain == null)
                {
                    rain = 0;
                }
                else
                {
                    if (mainrain[0] == "{}")
                    {
                        rain = 0;
                    }
                    else
                    {
                        string[] mainrain1 = mainrain[0].Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("{", "").Replace("}", "").Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        rain = Convert.ToDouble(mainrain1[1].Trim());
                    }
                }
 
                if (VayuDBConnection.State == ConnectionState.Open)
                {
                    VayuDBConnection.Close();
                }
                VayuDBConnection.Open();
 
                var fullfivemin = datetime.Minute / 5;
                DateTime result = new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, fullfivemin * 5, 0);
                cmdInsertWeatherCommand.Parameters["@ICAOCode"].Value = Icaocode;
                cmdInsertWeatherCommand.Parameters["@MarketDateTime"].Value = result;
                cmdInsertWeatherCommand.Parameters["@Temperature"].Value = ftemp;
                cmdInsertWeatherCommand.Parameters["@Humidity"].Value = Convert.ToDouble(humidity);
                cmdInsertWeatherCommand.Parameters["@Rain"].Value = Convert.ToDouble(rain);
                cmdInsertWeatherCommand.Parameters["@Clouds"].Value = Convert.ToDouble(clouds1);
                cmdInsertWeatherCommand.Parameters["@Description"].Value = descrip;
                int result1 = cmdInsertWeatherCommand.ExecuteNonQuery();
                Console.WriteLine(result1 + "inserted");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception", ex);

            }
        }

        /// <summary>
        /// Loads the city.
        /// </summary>
        private void LoadCity()
        {
 
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
  
 
            SqlDataReader reader = cmdSelectCityNoCommand.ExecuteReader();
            lstWeatherList = new List<Weather>();
            while (reader.Read())
            {
                Weather mweather = new Weather();
                mweather.CityNo = (int)reader.GetValue(0);
                mweather.LABEL = reader.GetString(1);
                mweather.ICAOCode = reader.GetString(2);
                lstWeatherList.Add(mweather);
            }
            reader.Close();

            VayuDBConnection.Close();  
        } 
        #endregion

        #region Public Method
        /// <summary>
        /// Unixes the time stamp to date time.
        /// </summary>
        /// <param name="unixTimeStamp">The unix time stamp.</param>
        /// <returns>datatable DateTime</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        } 
        #endregion
    }
}
