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
using System.Threading.Tasks;
using System.Timers;
using Vayu.CommonAccessLibrary;
namespace Vayu.WeatherForecastDownload
{
    public class ErcotWeatherForecast
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
        System.Timers.Timer sTimer = new System.Timers.Timer();
        public ErcotWeatherForecast()
        {
            LoadDB();
            LoadCity();
            sTimer = new System.Timers.Timer();
            OnTimerEvent(null, null);
            sTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            // sTimer.Interval = 3660000;
            //sTimer.Interval = 3600000;
            sTimer.Interval = 120000;
            sTimer.Start();
            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') { }
        }
        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            try
            {
                foreach (var item in lstWeatherList)
                {
                    Download(item.CityNo, item.LABEL, item.ICAOCode);
                }
            }
            catch (Exception ex)
            {
            }
            sTimer.Enabled = true;
        }
        public void LoadDB()
        {
            
 
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();;

            cmdSelectCityNoCommand = new SqlCommand();
            cmdSelectCityNoCommand.CommandText = "Select cityNo, Label, ICAOCode from WSIICAOCode where Region='ERCOT' order by ICAOCode asc";
            cmdSelectCityNoCommand.Connection = VayuDBConnection;

            cmdSelectPrevForecastCommand = new SqlCommand();
            cmdSelectPrevForecastCommand.CommandText = "select Temperature,tempmin,tempmax  from WSIForecast where MarketDateTime=@MarketDateTime and ICAOCode=@ICAOCode";
            cmdSelectPrevForecastCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime"); 
            cmdSelectPrevForecastCommand.Parameters.AddWithValue("@ICAOCode", "ICAOCode");
            cmdSelectPrevForecastCommand.Connection = VayuDBConnection;

            cmdInsertWeatherCommand = new SqlCommand(); 
 

            cmdSelectCityNoCommand = new SqlCommand();
            cmdSelectCityNoCommand.CommandText = "Select cityNo, Label, ICAOCode from WSIICAOCode where Region='ERCOT' order by ICAOCode asc";
            cmdSelectCityNoCommand.Connection = VayuDBConnection;

            cmdSelectPrevForecastCommand = new SqlCommand();
            cmdSelectPrevForecastCommand.CommandText = "select Temperature,tempmin,tempmax  from WSIForecast where MarketDateTime=@MarketDateTime and ICAOCode=@ICAOCode";
            cmdSelectPrevForecastCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            //cmdSelectPrevForecastCommand.Parameters.AddWithValue("@Hour", "Hour");
            cmdSelectPrevForecastCommand.Parameters.AddWithValue("@ICAOCode", "ICAOCode");
            cmdSelectPrevForecastCommand.Connection = VayuDBConnection;

            cmdInsertWeatherCommand = new SqlCommand();
            //"IF Not Exists(Select 'x' From WSIForecast Where ICAOCode = @ICAOCode And MarketDateTime = @MarketDateTime) " +
 
            cmdInsertWeatherCommand.CommandText = "Insert into WSIForecast(ICAOCode, MarketDateTime, Temperature,Humidity, Rain,Clouds,Description ,Dewpoint, HeatIndex, WindChill, WindSpeed, WindDirection, CloudCover, POP, Precipitation,Windgust,tempmin,tempmax) Values (@ICAOCode, @MarketDateTime, @Temperature,@Humidity, @Rain,@Clouds,@Description, Null, Null,  Null,  @WindSpeed,  Null,  Null,  Null,  Null,@Windgust,@tempmin,@tempmax)";
            cmdInsertWeatherCommand.CommandType = CommandType.Text;
            cmdInsertWeatherCommand.Parameters.AddWithValue("@ICAOCode", "ICAOCode");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            //cmdInsertWeatherCommand.Parameters.AddWithValue("@Hour", "Hour");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Temperature", "Temperature");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Humidity", "Humidity");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Rain", "Rain");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Clouds", "Clouds");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Description", "Description");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@WindSpeed", "WindSpeed");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@Windgust", "Windgust");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@tempmin", "tempmin");
            cmdInsertWeatherCommand.Parameters.AddWithValue("@tempmax", "tempmax");
 
            cmdInsertWeatherCommand.Connection = VayuDBConnection;

             cmdUpdateWeatherCommand = new SqlCommand();
            cmdUpdateWeatherCommand.CommandText = "update WSIForecast set Temperature=@Temperature, Humidity=@Humidity, Rain=@Rain, Clouds=@Clouds, Description=@Description, Windgust=@Windgust, tempmin=@tempmin, tempmax=@tempmax where ICAOCode=@ICAOCode and MarketDateTime=@MarketDateTime ";
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@ICAOCode", "ICAOCode");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@Temperature", "Temperature");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@Humidity", "Humidity");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@Rain", "Rain");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@Clouds", "Clouds");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@Description", "Description");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@WindSpeed", "WindSpeed");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@Windgust", "Windgust");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@tempmin", "tempmin");
            cmdUpdateWeatherCommand.Parameters.AddWithValue("@tempmax", "tempmax");
 
 
            cmdUpdateWeatherCommand.Connection = VayuDBConnection;
 
  
        }

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
                mweather.LABEL = reader.GetString(1).Trim();
                mweather.ICAOCode = reader.GetString(2).Trim();
                lstWeatherList.Add(mweather);
            }
            reader.Close();
 
            VayuDBConnection.Close();
 
             
 
        } 
        private void Download(int cityno, string cityname, string Icaocode)
        {
            try
            {

                string a = "api.openweathermap.org/data/2.5/forecast?q=" + cityno + "," + "us&mode=xml";
                // HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("http://api.openweathermap.org/data/2.5/forecast?id=" + cityno + "," + "us&appid=62b73943bcbc8ebc912f49490ee1123a");
                HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("http://api.openweathermap.org/data/2.5/forecast?q=" + cityname + "," + "us&appid=62b73943bcbc8ebc912f49490ee1123a");
                wrq.Method = "POST";
                wrq.ContentType = "text/xml";
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
                sr = new StreamReader(wrp.GetResponseStream());
                string xmlResponse = sr.ReadToEnd();
                sr.Close();
                wrp.Close();
                JObject obj = JObject.Parse(xmlResponse);
                var list = obj["list"];
                if (list == null)
                {

                }
                else
                {
                    foreach (var item in list)
                    {
                        string[] mainrain = null;
                        string desc = "";
                        double rain = 0;
                        double clouds = 0;
                        var dt = item["dt"];
                        double dtime = Convert.ToDouble(((Newtonsoft.Json.Linq.JValue)(dt)).Value);
                        DateTime datetime = UnixTimeStampToDateTime(dtime);
                        var main = (item["main"].First).First;
                        string[] mainhum = (item["main"].ToString()).Replace("\n", "").Replace("\t", "").Replace("\r", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string[] maindec = (item["weather"].ToString()).Replace("\n", "").Replace("\t", "").Replace("\r", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string[] WindData = (item["wind"].ToString()).Replace("\n", "").Replace("\t", "").Replace("\r", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string[] windSpeed = WindData[0].Split(new string[] { "speed" }, StringSplitOptions.RemoveEmptyEntries);
                        string[] windGust = WindData[2].Split(new string[] { "gust" }, StringSplitOptions.RemoveEmptyEntries);
                        double Wspeed = Convert.ToDouble(windSpeed[1].Replace(":", "").Replace(",", "").Replace("\"", ""));
                        double Wgust = Convert.ToDouble(windGust[1].Replace(":", "").Replace(",", "").Replace("\"", "").Replace("}", ""));
                        string[] temp_min = mainhum[2].Split(new string[] { "temp_min" }, StringSplitOptions.RemoveEmptyEntries);
                        double tempmin = Convert.ToDouble(temp_min[1].Replace(":", "").Replace(",", "").Replace("\"", ""));
                        string[] temp_max = mainhum[3].Split(new string[] { "temp_max" }, StringSplitOptions.RemoveEmptyEntries);
                        double tempmax = Convert.ToDouble(temp_max[1].Replace(":", "").Replace(",", "").Replace("\"", ""));

                        //    string[] mainrain = (item["rain"].ToString()).Replace("\n", "").Replace("\t", "").Replace("\r", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (!xmlResponse.Contains("rain"))
                        {
                            rain = double.NaN;
                        }
                        else
                        {
                            rain = 0;
                        }
                        var rainn = item["rain"];
                        if (xmlResponse.Contains("rain"))
                        {
                            if (item ["rain"] != null)
                            {
                                try
                                {
                                    mainrain = (item["rain"].ToString()).Replace("\n", "").Replace("\t", "").Replace("\r", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }

                            }
                        }
                        string[] mainclouds = (item["clouds"].ToString()).Replace("\n", "").Replace("\t", "").Replace("\r", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string[] maincloud = mainclouds[0].Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("}", "").Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        clouds = Convert.ToDouble(maincloud[1]);
                        //if (mainrain[0] == "{}")
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
                        string[] splitdec = maindec[2].Split(new string[] { "description" }, StringSplitOptions.RemoveEmptyEntries);
                        desc = splitdec[1].Replace(":", "").Replace(",", "").Replace("\"", "").Trim();
                        double temp = Convert.ToDouble(((Newtonsoft.Json.Linq.JValue)(main)).Value);
                        string[] splithum = mainhum[7].Split(new string[] { "humidity" }, StringSplitOptions.RemoveEmptyEntries);
                        double hum = Convert.ToDouble(splithum[1].Replace(":", "").Replace(",", "").Replace("\"", ""));

                        double ftemp = 1.8 * (temp - 273) + 32;
                        double Fmin = 1.8 * (tempmin - 273) + 32;
                        double Fmax = 1.8 * (tempmax - 273) + 32;
                        //double ftemp = ((temp - 273.15) * (1.80)) + 32;
                        InsertUpdate(Icaocode, datetime, ftemp, hum, rain, clouds, desc, Wspeed, Wgust, Fmin, Fmax);
 
                        if (VayuDBConnection.State == ConnectionState.Open)
                        {
                            VayuDBConnection.Close();
                        }
                        VayuDBConnection.Open();
 
                        
 
                        cmdSelectPrevForecastCommand.Parameters["@MarketDateTime"].Value = datetime.AddHours(-3);
                        cmdSelectPrevForecastCommand.Parameters["@ICAOCode"].Value = Icaocode;
                        SqlDataReader reader = cmdSelectPrevForecastCommand.ExecuteReader();
                        if (reader.HasRows)
                        {
                            int prev3 = 0;
                            double prevmin = 0, prevmax = 0;
                            while (reader.Read())
                            {
                                prev3 = (int)reader.GetValue(0);
                                prevmin = (double)reader.GetDecimal(1);
                                prevmax = (double)reader.GetDecimal(2);
                            }
 
 
                            VayuDBConnection.Close();
 
                            if (prev3 == 0)
                            {
                                prev3 = (int)ftemp - 2;
                            }
                            if (prevmin == 0)
                            {
                                prevmin = (int)Fmin - 2;
                            }
                            if (prevmax == 0)
                            {
                                prevmax = (int)Fmax - 2;
                            }
                            double temp1 = Math.Round(((2 * prev3) + ftemp) / 3); //MAin
                            double temp1min = Math.Round(((2 * prevmin) + Fmin) / 3);
                            double temp1max = Math.Round(((2 * prevmax) + Fmax) / 3);
                            InsertUpdate(Icaocode, datetime.AddHours(-2), temp1, hum, rain, clouds, desc, Wspeed, Wgust, temp1min, temp1max);
                            double temp2 = Math.Round((prev3 + (2 * ftemp)) / 3);
                            double temp2min = Math.Round((prevmin + (2 * Fmin)) / 3);
                            double temp2max = Math.Round((prevmax + (2 * Fmax)) / 3);//MAin
                            InsertUpdate(Icaocode, datetime.AddHours(-1), temp2, hum, rain, clouds, desc, Wspeed, Wgust, temp2min, temp2max);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void InsertUpdate(string Icaocode, DateTime datetime, double temp, double humidity, double rain, double clouds, string desc, double Wspeed, double Wgust, double tempmin, double tempmax)
        {
 
            if (VayuDBConnection.State == ConnectionState.Open)
            {
                VayuDBConnection.Close();
            }
            VayuDBConnection.Open();
  
            cmdUpdateWeatherCommand.Parameters["@ICAOCode"].Value = Icaocode;
            cmdUpdateWeatherCommand.Parameters["@MarketDateTime"].Value = datetime;
            cmdUpdateWeatherCommand.Parameters["@Temperature"].Value = temp;
            cmdUpdateWeatherCommand.Parameters["@Humidity"].Value = humidity;
            cmdUpdateWeatherCommand.Parameters["@Rain"].Value = rain;
            cmdUpdateWeatherCommand.Parameters["@Clouds"].Value = clouds;
            cmdUpdateWeatherCommand.Parameters["@Description"].Value = desc;
            cmdUpdateWeatherCommand.Parameters["@WindSpeed"].Value = Convert.ToInt32(Wspeed);
            cmdUpdateWeatherCommand.Parameters["@Windgust"].Value = Wgust;
            cmdUpdateWeatherCommand.Parameters["@tempmin"].Value = tempmin;
            cmdUpdateWeatherCommand.Parameters["@tempmax"].Value = tempmax;
            try
            {
                int count = cmdUpdateWeatherCommand.ExecuteNonQuery();
                Console.WriteLine("Data Downloading...." + count + "Updated");
                if (count <= 0) 
                {
                    cmdInsertWeatherCommand.Parameters["@ICAOCode"].Value = Icaocode;
                    cmdInsertWeatherCommand.Parameters["@MarketDateTime"].Value = datetime;
                    cmdInsertWeatherCommand.Parameters["@Temperature"].Value = temp;
                    cmdInsertWeatherCommand.Parameters["@Humidity"].Value = humidity;
                    cmdInsertWeatherCommand.Parameters["@Rain"].Value = rain;
                    cmdInsertWeatherCommand.Parameters["@Clouds"].Value = clouds;
                    cmdInsertWeatherCommand.Parameters["@Description"].Value = desc;
                    cmdInsertWeatherCommand.Parameters["@WindSpeed"].Value = Wspeed;
                    cmdInsertWeatherCommand.Parameters["@Windgust"].Value = Wgust;
                    cmdInsertWeatherCommand.Parameters["@tempmin"].Value = tempmin;
                    cmdInsertWeatherCommand.Parameters["@tempmax"].Value = tempmax;
                    int result2 = cmdInsertWeatherCommand.ExecuteNonQuery();

                    Console.WriteLine("Data Downloading...." + result2 + "Inserted");

                }
            }
            catch (Exception ex)
            {

                throw;
            }
           

            VayuDBConnection.Close();
  }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        } 
    }
}
