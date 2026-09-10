using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Timers;
using Vayu.CityTemperatureServiceLibrary;

namespace CityTemperatureService
{
    /// <summary>
    /// Get City Temperature
    /// </summary>
    /// <seealso cref="CityTemperatureServiceLibrary.ITemperatureService" />
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant, MaxItemsInObjectGraph = int.MaxValue, UseSynchronizationContext = false)]
    public class TemperatureServer : ITemperatureService
    {
        #region Private Members
        /// <summary>
        /// The alpha database connection
        /// </summary>
        private static string alphaDBConnection = Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection();
        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object lockObj = new object();
        /// <summary>
        /// The city table list
        /// </summary>
        private static List<CityTableCode> sCityTableList = new List<CityTableCode>();
        /// <summary>
        /// The temporary callback hash
        /// </summary>
        private static Dictionary<ITemperatureCallback, DateTime> sTempCallbackHash = new Dictionary<ITemperatureCallback, DateTime>();
        /// <summary>
        /// The remover hash
        /// </summary>
        private List<ITemperatureCallback> mRemoverHash = new List<ITemperatureCallback>();
        /// <summary>
        /// The call back timer
        /// </summary>
        private Timer mCallBackTimer = new Timer();
        #endregion

        /// <summary>
        /// Initializes a new instance of the TemperatureServer class.
        /// </summary>
        public TemperatureServer()
        {
            if (sCityTableList.Count == 0)
            {
                FillCityTableHash();
            }
        }
        /// <summary>
        /// Connects this instance.
        /// </summary>
        internal void Connect()
        {
            //string portNo = Vayu.CommonAccessLibrary.ServiceConnections.GetTemperatureService();
            ServiceHost host = new ServiceHost(this.GetType(), new Uri(Vayu.CommonAccessLibrary.ServiceConnections.GetTemperatureService()));
            NetTcpBinding binding = new NetTcpBinding
            {
                //MaxReceivedMessageSize = int.MaxValue,
                Security = new NetTcpSecurity { Mode = SecurityMode.None },
                OpenTimeout = new TimeSpan(0, 25, 0),
                CloseTimeout = new TimeSpan(0, 25, 0),
                SendTimeout = new TimeSpan(0, 25, 0),
                ReceiveTimeout = new TimeSpan(0, 25, 0),
                // MaxConnections = int.MaxValue
            };
            var behavior = new ServiceThrottlingBehavior()
            {
                MaxConcurrentCalls = 10000,
                MaxConcurrentInstances = 10000,
                MaxConcurrentSessions = 1000
            };
            host.Description.Behaviors.Add(behavior);
            host.AddServiceEndpoint(typeof(ITemperatureService), binding, "");
            try
            {
                host.Open();
                mCallBackTimer.Interval = new TimeSpan(0, 5, 0).TotalMilliseconds;
                mCallBackTimer.Elapsed += mCallBackTimer_Elapsed;
                mCallBackTimer.Enabled = true;
                mCallBackTimer.Start();

                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": started Temperature Service on port 8011");
                Console.Read();
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + " : " + ex.Message); ;
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the CallBackTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The ElapsedEventArgs instance containing the event data.</param>
        void mCallBackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mCallBackTimer.Enabled = false;
            lock (lockObj)
            {
                if (sTempCallbackHash.Count > 0)
                {
                    CallBackClients();
                }
            }
            mCallBackTimer.Enabled = true;
        }

        #region Private Methods
        /// <summary>
        /// Calls the back clients.
        /// </summary>
        private void CallBackClients()
        {
            List<Temperature> temperatureList = GetCurrentTemperatures(sTempCallbackHash.Values.Max(), sTempCallbackHash.Values.Min());
            lock (lockObj)
            {
                foreach (ITemperatureCallback item in sTempCallbackHash.Keys)
                {
                    try
                    {
                        item.SendTemperature(temperatureList.Where(a => a.ClimateDate.ToShortDateString() == sTempCallbackHash[item].ToShortDateString()).ToList());
                    }
                    catch (CommunicationException)
                    {
                        if (!mRemoverHash.Contains(item))
                        {
                            mRemoverHash.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                foreach (ITemperatureCallback item in mRemoverHash)
                {
                    try
                    {
                        if (sTempCallbackHash.ContainsKey(item))
                        {
                            sTempCallbackHash.Remove(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Gets the current temperatures.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>Current Temperature List</returns>
        private List<Temperature> GetCurrentTemperatures(DateTime fromDate, DateTime endDate, string market = "ERCOT", bool IsCelsius = false, bool IsKmph = false, bool Isknots = false)
        {
            List<Temperature> tempList = new List<Temperature>();
            try
            {
                if (fromDate == endDate)
                {
                    endDate = endDate.AddDays(1);
                }
                using (SqlConnection con = new SqlConnection(alphaDBConnection))
                {
                    try
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            
                                cmd.CommandText = "select w.label,MarketDateTime,w.Region,Temperature,CloudCover,DewPoint,Precipitation,WindSpeed," +
                  " WindDirection , w.Latitude,w.Logitude,w.StateName,  Humidity, Rain,Clouds,Description from Vayu..WSICurrent ww join WSIICAOCode w on ww.ICAOCode=w.ICAOCode where " +
                  " MarketDateTime >= @startdate and MarketDateTime < @enddate and w.Region in ('ercot') order by w.LABEL asc";
                            

                            cmd.Parameters.AddWithValue("@startdate", fromDate);
                            cmd.Parameters.AddWithValue("@enddate", endDate);
                            cmd.Connection.Open();
                            System.Data.IDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    Temperature temp = new Temperature
                                    {
                                        City = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString(),
                                        ClimateDate = reader.IsDBNull(1) ? new DateTime() : Convert.ToDateTime(reader.GetValue(1)),
                                        Market = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString(),
                                        TempVal = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
                                        CloudCover = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                                        DewPoint = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5)),
                                        Precipitation = reader.IsDBNull(6) ? 0 : Convert.ToInt32(6),
                                        WindSpeed = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7)),
                                        //RelativeHumidity = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8)),
                                        WindDirection = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8)),
                                        Latitude = reader.IsDBNull(9) ? 0 : Convert.ToDouble(reader.GetValue(9)),
                                        Longitude = reader.IsDBNull(10) ? 0 : Convert.ToDouble(reader.GetValue(10)),
                                        State = reader.IsDBNull(11) ? "" : reader.GetValue(11).ToString(),
                                        // Newly Added
                                        Humidity = reader.IsDBNull(12) ? 0 : Convert.ToDouble(reader.GetValue(12)),
                                        Rain = reader.IsDBNull(13) ? 0 : Convert.ToDouble(reader.GetValue(13)),
                                        Clouds = reader.IsDBNull(14) ? 0 : Convert.ToDouble(reader.GetValue(14)),
                                        Description = reader.IsDBNull(15) ? "" : reader.GetValue(15).ToString(),
                                        //WindGust = reader.IsDBNull(16) ? 0 : Convert.ToDouble(reader.GetValue(16)),
                                        //MinTemp = reader.IsDBNull(17) ? 0 : Convert.ToDouble(reader.GetValue(17)),
                                        //MaxTemp = reader.IsDBNull(18) ? 0 : Convert.ToDouble(reader.GetValue(18))
                                    };

                                     
                                    if (IsCelsius)
                                    {
                                        temp.TempVal = Convert.ToInt32((temp.TempVal - 32) * 5) / 9;
                                        temp.MinTemp = Convert.ToInt32((temp.MinTemp - 32) * 5) / 9;
                                        temp.MaxTemp = Convert.ToInt32((temp.MaxTemp - 32) * 5) / 9;
                                    }
                                    if (IsKmph)
                                    {
                                        temp.WindSpeed = Convert.ToInt32(temp.WindSpeed * 1.60934);   
                                    }
                                    else if (Isknots)
                                    {
                                        temp.WindSpeed = Convert.ToInt32(temp.WindSpeed * 0.868976);

                                    }
                                    tempList.Add(temp);
                                    
                                        
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            reader.Close();
                            cmd.Connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (con.State.Equals(System.Data.ConnectionState.Open))
                            con.Close();
                    }
                }
                #region Commented Code
                //if (sCityTableList.Count == 0)
                //{
                //    FillCityTableHash();
                //}
                //tempList.RemoveAll(a => a.ClimateDate < DateTime.Now.AddHours(-1) && a.ClimateDate > DateTime.Now.AddHours(1)); 
                #endregion
                tempList.RemoveAll(a => a.City == "" && a.Market == "");
                #region Commented Code
                //System.Threading.Tasks.Parallel.ForEach(sCityTableList, a =>
                //{
                //    foreach (var item in tempList.Where(k => k.City.ToLower().StartsWith(a.City.ToLower())))
                //    {
                //        item.Latitude = a.Latitude;
                //        item.Longitude = a.Longitude;
                //    }
                //});
                //tempList.RemoveAll(a => a.Latitude == 0 && a.Longitude == 0); 
                #endregion
                tempList = tempList.OrderBy(a => a.City).ThenBy(p => p.ClimateDate).ToList();
                //tempList = tempList.OrderBy(a => a.ClimateDate).ThenBy(p => p.City).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return tempList;
        }
        /// <summary>
        /// Gets the forecast temperatures.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>Forecast Temperature List</returns>
        private List<Temperature> GetForecastTemperatures(DateTime fromDate, DateTime endDate, string market, bool IsCelsius, bool IsKmph, bool Isknots) //, bool Isknots
        {
             
            List<Temperature> tempList = new List<Temperature>();
            try
            {
                if (fromDate == endDate)
                {
                    endDate = endDate.AddDays(1);
                }
                using (SqlConnection con = new SqlConnection(alphaDBConnection))
                {
                    try
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {

                            
                                cmd.CommandText = "select w.label,MarketDateTime,w.Region,Temperature,CloudCover,DewPoint,Precipitation,WindSpeed," +
                "WindDirection , w.Latitude,w.Logitude,w.StateName, Humidity, Rain,Clouds,Description,Windgust,tempmin,tempmax from Vayu..WSIForecast ww join WSIICAOCode w on ww.ICAOCode=w.ICAOCode where " +
                 " MarketDateTime >= '2022 - 09 - 14' and MarketDateTime < '2022 - 09 - 15' and w.Region in ('ercot') order by w.LABEL asc ";
                            

                            //cmd.Parameters.AddWithValue("@startdate", fromDate);
                            //cmd.Parameters.AddWithValue("@enddate", endDate);
                            cmd.Connection.Open();
                            System.Data.IDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    Temperature temp = new Temperature
                                    {
                                        City = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString(),
                                        ClimateDate = reader.IsDBNull(1) ? new DateTime() : Convert.ToDateTime(reader.GetValue(1)),
                                        Market = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString(),
                                        CloudCover = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                                        DewPoint = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5)),
                                        //Precipitation = reader.IsDBNull(6) ? 0 : Convert.ToInt32(6),
                                        WindSpeed = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7)),
                                        //RelativeHumidity = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8)),
                                        WindDirection = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8)),
                                        Latitude = reader.IsDBNull(9) ? 0 : Convert.ToDouble(reader.GetValue(9)),
                                        Longitude = reader.IsDBNull(10) ? 0 : Convert.ToDouble(reader.GetValue(10)),
                                        State = reader.IsDBNull(11) ? "" : reader.GetValue(11).ToString(),
                                        //Newly Added
                                        Humidity = reader.IsDBNull(12) ? 0 : Convert.ToDouble(reader.GetValue(12)),
                                        Rain = reader.IsDBNull(13) ? 0 : Convert.ToDouble(reader.GetValue(13)),
                                        Clouds = reader.IsDBNull(14) ? 0 : Convert.ToDouble(reader.GetValue(14)),
                                        Description = reader.IsDBNull(15) ? "" : reader.GetValue(15).ToString(),
                                        WindGust = reader.IsDBNull(16) ? 0 : Convert.ToDouble(reader.GetValue(16)),
                                        TempVal = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
                                        MinTemp = reader.IsDBNull(17) ? 0 : Convert.ToDouble(reader.GetValue(17)),
                                        MaxTemp = reader.IsDBNull(18) ? 0 : Convert.ToDouble(reader.GetValue(18))
                                    };

                                    //int hour = CommonAccessLibrary.CommonDataConversions.GetInt(reader[12]).GetValueOrDefault();
                                    //temp.ClimateDate = temp.ClimateDate.AddHours(hour);
                                    if (IsCelsius)
                                    {
                                        temp.TempVal = Convert.ToInt32((temp.TempVal - 32) * 5) / 9;
                                        temp.MinTemp = Convert.ToInt32((temp.MinTemp - 32) * 5) / 9;
                                        temp.MaxTemp = Convert.ToInt32((temp.MaxTemp - 32) * 5) / 9;
                                    }
                                    #region For WindSpeed mph to kmph and knots conversion
                                    if (IsKmph) //IsKmph
                                    {
                                        temp.WindSpeed = Convert.ToInt32(temp.WindSpeed * 1.60934);
                                    }
                                    else if (Isknots)
                                    {
                                        temp.WindSpeed = Convert.ToInt32(temp.WindSpeed * 0.868976);

                                    }
                                     
                                    #endregion
                                    tempList.Add(temp);

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            reader.Close();
                            cmd.Connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (con.State.Equals(System.Data.ConnectionState.Open))
                            con.Close();
                    }
                }
                #region Commented Code
                //if (sCityTableList.Count == 0)
                //{
                //    FillCityTableHash();
                //}
                //tempList.RemoveAll(a => a.ClimateDate < DateTime.Now.AddHours(-1) && a.ClimateDate > DateTime.Now.AddHours(1)); 
                #endregion
                tempList.RemoveAll(a => a.City == "" && a.Market == "");
                #region Commented Code
                //System.Threading.Tasks.Parallel.ForEach(sCityTableList, a =>
                //{
                //    foreach (var item in tempList.Where(k => k.City.ToLower().StartsWith(a.City.ToLower())))
                //    {
                //        item.Latitude = a.Latitude;
                //        item.Longitude = a.Longitude;
                //    }
                //});
                //tempList.RemoveAll(a => a.Latitude == 0 && a.Longitude == 0); 
                #endregion
                tempList = tempList.OrderBy(a => a.City).ThenBy(p => p.ClimateDate).ToList();
                //tempList = tempList.OrderBy(a => a.ClimateDate).ThenBy(p => p.City).ToList();
                if (tempList.Exists(X => X.City == "PECOS"))
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return tempList;
        }
        /// <summary>
        /// Gets the temperatures.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>Temperature List</returns>
        private List<Temperature> GetTemperatures(DateTime fromDate, DateTime endDate)
        {
            List<Temperature> tempList = new List<Temperature>();
            try
            {
                if (fromDate == endDate)
                {
                    endDate = endDate.AddDays(1);
                }
                using (SqlConnection con = new SqlConnection(alphaDBConnection))
                {
                    try
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            #region Commented Code
                            //cmd.CommandText = "select label,DATEADD(hour,Hour, cast (Date as nvarchar)),w.Region,Temperature,CloudCover,DewPoint,Precipitation,WindSpeed," +
                            //"WindDirection , w.Latitude,w.Logitude,w.StateName  from WSIForecast ww join WSIICAOCode w on ww.ICAOCode=w.ICAOCode where" +
                            //"Date >= @startdate and Date < @enddate and w.Region in ('pjm','miso','ercot','nyiso','spp','caiso')"; 
                            #endregion
                            cmd.CommandText = "select City,DATEADD(hour,markettime, cast (marketdate as nvarchar)),w.Region,Temperature,CloudCover,DewPoint,Precip,WindSpeed,RelativeHumidity, " +
                            " WindDirection , w.Latitude,w.Logitude,w.StateName  from Wsi_Weather ww join WSIICAOCode w on ww.City = w.LABEL and ww.StateName = w. StateName where" +
                            " MarketDate >= @startdate and MarketDate <@enddate and ww.Region in ('pjm','miso','ercot','nyiso','spp','caiso') ";
                            //cmd.CommandText = "select City,DATEADD(hour,markettime, cast (marketdate as nvarchar)),Region,Temperature,CloudCover,DewPoint,Precip,WindSpeed,RelativeHumidity," +
                            // " WindDirection  from Wsi_Weather where MarketDate >= @startdate and MarketDate <@enddate and Region in ('pjm','miso','ercot','nyiso','spp','caiso')";
                            cmd.Parameters.AddWithValue("@startdate", fromDate);
                            cmd.Parameters.AddWithValue("@enddate", endDate);
                            cmd.Connection.Open();
                            System.Data.IDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    tempList.Add(new Temperature
                                    {
                                        City = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString(),
                                        ClimateDate = reader.IsDBNull(1) ? new DateTime() : Convert.ToDateTime(reader.GetValue(1)),
                                        Market = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString(),
                                        TempVal = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
                                        CloudCover = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                                        DewPoint = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5)),
                                        //Precipitation = reader.IsDBNull(6) ? 0 : Convert.ToInt32(6),
                                        WindSpeed = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7)),
                                        //RelativeHumidity = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8)),
                                        WindDirection = reader.IsDBNull(9) ? 0 : Convert.ToInt32(reader.GetValue(9)),
                                        Latitude = reader.IsDBNull(10) ? 0 : Convert.ToDouble(reader.GetValue(10)),
                                        Longitude = reader.IsDBNull(11) ? 0 : Convert.ToDouble(reader.GetValue(11)),
                                        State = reader.IsDBNull(12) ? "" : reader.GetValue(12).ToString()
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            reader.Close();
                            cmd.Connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (con.State.Equals(System.Data.ConnectionState.Open))
                            con.Close();
                    }
                }
                #region Commented Code
                //if (sCityTableList.Count == 0)
                //{
                //    FillCityTableHash();
                //}
                //tempList.RemoveAll(a => a.ClimateDate < DateTime.Now.AddHours(-1) && a.ClimateDate > DateTime.Now.AddHours(1)); 
                #endregion
                tempList.RemoveAll(a => a.City == "" && a.Market == "");

                #region Commented Code
                //System.Threading.Tasks.Parallel.ForEach(sCityTableList, a =>
                //{
                //    foreach (var item in tempList.Where(k => k.City.ToLower().StartsWith(a.City.ToLower())))
                //    {
                //        item.Latitude = a.Latitude;
                //        item.Longitude = a.Longitude;
                //    }
                //});
                //tempList.RemoveAll(a => a.Latitude == 0 && a.Longitude == 0); 
                #endregion

                tempList = tempList.OrderBy(a => a.ClimateDate).ThenBy(p => p.City).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return tempList;
        }
        /// <summary>
        /// Fills the city table hash.
        /// </summary>
        private void FillCityTableHash()
        {
            sCityTableList.Clear();
            using (SqlConnection con = new SqlConnection(alphaDBConnection))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "select LABEL,Region,Logitude,latitude from WSIICAOCode order by LABEL";
                        cmd.Connection.Open();
                        System.Data.IDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            try
                            {
                                sCityTableList.Add(new CityTableCode
                                {
                                    City = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString(),
                                    Market = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString(),
                                    Longitude = reader.IsDBNull(2) ? 0 : Convert.ToDouble(reader.GetValue(2)),
                                    Latitude = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3)),
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns>Ip Address</returns>
        private string GetIPAddress(OperationContext operationContext)
        {
            if (operationContext == null)
                return "";
            MessageProperties msp = operationContext.IncomingMessageProperties;
            return (msp[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Subscribes the specified date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public bool Subscribe(DateTime date)
        {
            if (date == new DateTime())
                return false;
            try
            {
                lock (lockObj)
                {
                    try
                    {
                        ITemperatureCallback callback = OperationContext.Current.GetCallbackChannel<ITemperatureCallback>();
                        if (callback != null)
                        {
                            try
                            {
                                if (sTempCallbackHash.ContainsKey(callback))
                                {
                                    sTempCallbackHash.Remove(callback);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            sTempCallbackHash.Add(callback, date);
                            Console.WriteLine("Subscribed by {0}", GetIPAddress(OperationContext.Current));
                            Console.WriteLine("Total subscribers: {0}", sTempCallbackHash.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Gets the city temperatures.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="isRange">if set to <c>true</c> [is range].</param>
        /// <returns>City Temperature List</returns>

                                                  //DateTime fromDate, DateTime toDate, bool isRange = false, string market = "ERCOT", bool? IsCelsius = false, bool? IsKmph = false, bool? Isknots = false
        public List<Temperature> GetCityTemperatures(DateTime fromDate, DateTime toDate, bool isRange, string market, bool IsCelsius, bool IsKmph, bool Isknots)
        {
            if (sCityTableList.Count == 0)
            {
                FillCityTableHash();
            }
            if (isRange)
            {
                List<Temperature> lstCurrentData = GetForecastTemperatures(fromDate, toDate, market, IsCelsius, IsKmph, Isknots);
                return lstCurrentData;
            }
            else
            {
                List<Temperature> lstCurrentData = GetCurrentTemperatures(fromDate, toDate, market, IsCelsius, IsKmph, Isknots);
                if (lstCurrentData.Count == 0)
                {
                    return GetForecastTemperatures(fromDate, toDate, market, IsCelsius, IsKmph, Isknots);
                }
                else
                {
                    return lstCurrentData;
                }
            }

        }
        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        public bool HeartBeat()
        {
            Console.WriteLine(DateTime.Now.ToShortDateString() + "\t" + DateTime.Now.ToShortTimeString() + "HeartBeat by {0}", GetIPAddress(OperationContext.Current));
            return true;
        }

        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        public void Unsubscribe()
        {
            lock (lockObj)
            {
                try
                {
                    ITemperatureCallback callback = OperationContext.Current.GetCallbackChannel<ITemperatureCallback>();
                    if (callback != null)
                    {
                        try
                        {
                            if (sTempCallbackHash.ContainsKey(callback))
                            {
                                sTempCallbackHash.Remove(callback);
                            }
                            Console.WriteLine("Unsubscribed by {0}", GetIPAddress(OperationContext.Current));
                            Console.WriteLine("Total subscribers: {0}", sTempCallbackHash.Count);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        

        #endregion
        
    }
}
