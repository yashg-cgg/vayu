using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Vayu.WindServiceLibrary;

namespace Vayu.WindService
{
    /// <summary>
    /// PJM Wind Service
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant, MaxItemsInObjectGraph = int.MaxValue)]
    [CallbackBehavior(UseSynchronizationContext = true)]
    public class WindServer : IWindDataProvider
    {
        #region Private Members
        /// <summary>
        /// The ubscriber hash
        /// </summary>
        private static Dictionary<IWindDataCallback, string> sSubscriberHash = new Dictionary<IWindDataCallback, string>();
        /// <summary>
        /// The remove hash
        /// </summary>
        private static HashSet<IWindDataCallback> sRemoveHash = new HashSet<IWindDataCallback>();
        /// <summary>
        /// The timer
        /// </summary>
        private static System.Timers.Timer sTimer = new System.Timers.Timer();
        /// <summary>
        /// The wind total hash
        /// </summary>
        private static Dictionary<string, List<LatestWindData>> mWindTotalHash = new Dictionary<string, List<LatestWindData>>();
        /// <summary>
        /// The wind update hash
        /// </summary>
        private static Dictionary<string, DateTime> sWindUpdateHash = new Dictionary<string, DateTime>();
        #endregion

        /// <summary>
        /// The Sigma database connection
        /// </summary>
        SqlConnection VayuDBConnection;
        /// <summary>
        /// The select wind load realtime data
        /// </summary>
        SqlCommand mSelectWindLoadRTData;
        SqlCommand mSelectErcotWindLoadRTData;
        /// <summary>
        /// The select wind forecast realtime data
        /// </summary>
        SqlCommand mSelectWindForecastRTData;
        SqlCommand mSelectERcotWindForecastRTData;

        #region Public Object
        /// <summary>
        /// The lock object
        /// </summary>
        public static readonly object lockobj = new object();
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the database related object.
        /// </summary>
        private void InitializeDB()
        {
            try
            {
                VayuDBConnection = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());

                //mSelectWindLoadRTData = VayuDBConnection.CreateCommand();
                //mSelectWindLoadRTData.CommandText = "SELECT MarketDateTime, Value FROM Wind WHERE MarketDateTime >= @startDate AND MarketDateTime < @endDate";
                //mSelectWindLoadRTData.Parameters.AddWithValue("@startDate", "MarketDateTime");
                //mSelectWindLoadRTData.Parameters.AddWithValue("@endDate", "MarketDateTime");

                //mSelectWindForecastRTData = VayuDBConnection.CreateCommand();
                //mSelectWindForecastRTData.CommandText = "SELECT MarketDateTime, Value FROM WindForecast WHERE MarketDateTime >= @startDate AND MarketDateTime < @endDate";
                //mSelectWindForecastRTData.Parameters.AddWithValue("@startDate", "MarketDateTime");
                //mSelectWindForecastRTData.Parameters.AddWithValue("@endDate", "MarketDateTime");


                mSelectErcotWindLoadRTData = VayuDBConnection.CreateCommand();
                mSelectErcotWindLoadRTData.CommandText = "select a.MarketDateTime, a.Hour,a.RealTimevalue,a.RTSouth_Houston,a.RTWest,a.RTNorth, "
                        + " b.RTPANHANDLE,b.RTCOASTAL, b.RTNorth, b.RTSouth, b.RTWest from Vayu..WindPowerGenerationValue a "
                        + " left join   Vayu..WindPowerGenerationValueRegion b on a.MarketDateTime = b.MarketDateTime and a.Hour = b.Hour   where"
                        + " a.MarketDateTime >= @startDate AND a.MarketDateTime < @endDate order by a.MarketDateTime,a.Hour";
                mSelectErcotWindLoadRTData.Parameters.AddWithValue("@startDate", "MarketDateTime");
                mSelectErcotWindLoadRTData.Parameters.AddWithValue("@endDate", "MarketDateTime");

                mSelectERcotWindForecastRTData = VayuDBConnection.CreateCommand();
                // mSelectERcotWindForecastRTData.CommandText = "select MarketDateTime, Hour,Forecastvalue,ForeCastSouth_Houston,ForeCastWest,ForeCastNorth from ERCOT..WindPowerGenerationValue where MarketDateTime >= @startDate AND MarketDateTime < @endDate";
                mSelectERcotWindForecastRTData.CommandText = " select a.MarketDateTime, a.Hour,a.Forecastvalue,a.ForeCastSouth_Houston,a.ForeCastWest,a.ForeCastNorth, "
                        + " b.ForecastPANHANDLE,b.ForecastCOASTAL, b.ForecastNorth, b.ForecastSouth, b.ForecastWest from Vayu..WindPowerGenerationValue a "
                        + " join   Vayu..WindPowerGenerationValueRegion b on a.MarketDateTime = b.MarketDateTime and a.Hour = b.Hour and "
                        + " a.MarketDateTime >= @startDate AND a.MarketDateTime < @endDate order by a.MarketDateTime,a.Hour";
                mSelectERcotWindForecastRTData.Parameters.AddWithValue("@startDate", "MarketDateTime");
                mSelectERcotWindForecastRTData.Parameters.AddWithValue("@endDate", "MarketDateTime");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets the wind data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>temp Wind Hash</returns>
        private List<LatestWindData> GetWindData(DateTime startDate, DateTime endDate, int Marketkey)
        {
            lock (lockobj)
            {
                InitializeDB();

                try
                {
                    List<LatestWindData> tempWindHash = new List<LatestWindData>();

                    if (VayuDBConnection.State == ConnectionState.Closed)
                    {
                        VayuDBConnection.Open();
                    }
                    SqlDataReader rtReader = null;
                     
                        mSelectErcotWindLoadRTData.Parameters[0].Value = startDate;
                        mSelectErcotWindLoadRTData.Parameters[1].Value = endDate;
                        rtReader = mSelectErcotWindLoadRTData.ExecuteReader();
                     
                    while (rtReader.Read())
                    {
                         
                            DateTime date= rtReader.IsDBNull(0) ? new DateTime() : rtReader.GetDateTime(0);
                            int hour= rtReader.IsDBNull(1) ? 0 : rtReader.GetInt32(1);
                            tempWindHash.Add(new LatestWindData
                            {
                                WindType = "Actual",
                                MarketDate = date.AddHours(hour),
                                Value = rtReader.IsDBNull(2) ? 0.0 : Convert.ToDouble(rtReader.GetValue(2)),
                                RTSouth_Houston = rtReader.IsDBNull(3) ? 0.0 : Convert.ToDouble(rtReader.GetValue(3)),
                                RTWest = rtReader.IsDBNull(4) ? 0.0 : Convert.ToDouble(rtReader.GetValue(4)),
                                RTNorth = rtReader.IsDBNull(5) ? 0.0 : Convert.ToDouble(rtReader.GetValue(5)),
                                RTPANHANDLE= rtReader.IsDBNull(6) ? 0.0 : Convert.ToDouble(rtReader.GetValue(6)),
                                RTCOASTAL= rtReader.IsDBNull(7) ? 0.0 : Convert.ToDouble(rtReader.GetValue(7)),
                                RTNorthRegion= rtReader.IsDBNull(8) ? 0.0 : Convert.ToDouble(rtReader.GetValue(8)),
                                RTSouth= rtReader.IsDBNull(9) ? 0.0 : Convert.ToDouble(rtReader.GetValue(9)),
                                RTWestRegion= rtReader.IsDBNull(10) ? 0.0 : Convert.ToDouble(rtReader.GetValue(10))
                            });
                        
                    }
                    if (!rtReader.IsClosed)
                    {
                        rtReader.Close();
                    }

                    //
                    SqlDataReader forecastReader = null;
                     
                        mSelectERcotWindForecastRTData.Parameters[0].Value = startDate;
                        mSelectERcotWindForecastRTData.Parameters[1].Value = endDate;
                        forecastReader = mSelectERcotWindForecastRTData.ExecuteReader();
                    
                    while (forecastReader.Read())
                    {
                        
                            DateTime date=forecastReader.IsDBNull(0) ? new DateTime() : forecastReader.GetDateTime(0);
                            int hour=forecastReader.IsDBNull(1) ? 0 : forecastReader.GetInt32(1);
                            tempWindHash.Add(new LatestWindData
                            {
                                WindType = "Forecast",
                                MarketDate =date.AddHours(hour) ,
                                Value = forecastReader.IsDBNull(2) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(2)),
                                ForeCastSouth_Houston = forecastReader.IsDBNull(3) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(3)),
                                ForeCastWest = forecastReader.IsDBNull(4) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(4)),
                                ForeCastNorth = forecastReader.IsDBNull(5) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(5)),
                                ForecastPANHANDLE = forecastReader.IsDBNull(6) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(6)),
                                ForecastCOASTAL = forecastReader.IsDBNull(7) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(7)),
                                ForecastNorthRegion = forecastReader.IsDBNull(8) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(8)),
                                ForecastSouth = forecastReader.IsDBNull(9) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(9)),
                                ForecastWestRegion = forecastReader.IsDBNull(10) ? 0.0 : Convert.ToDouble(forecastReader.GetValue(10))
                            });
                        
                    }
                    if (!forecastReader.IsClosed)
                    {
                        forecastReader.Close();
                    }
                    tempWindHash.RemoveAll(a => a.Value == 0.0);

                    if (mWindTotalHash.ContainsKey(startDate.ToShortDateString() + "?" + endDate.ToShortDateString()))
                    {
                        mWindTotalHash.Remove(startDate.ToShortDateString() + "?" + endDate.ToShortDateString());
                    }

                    mWindTotalHash.Add(startDate.ToShortDateString() + "?" + endDate.ToShortDateString(), tempWindHash);

                    return tempWindHash;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new List<LatestWindData>();
                }
            }
        }
        /// <summary>
        /// Gets the subscriber ip address.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns>ip address</returns>
        private string GetSubscriberIpAddress(OperationContext operationContext)
        {
            return ((operationContext.IncomingMessageProperties as MessageProperties)[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;
        }
        /// <summary>
        /// Called when [timed event].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The ElapsedEventArgs instance containing the event data.</param>
        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            sTimer.Enabled = false;

            try
            {
                if (sSubscriberHash.Count > 0)
                {
                    //CallBackHelper();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            sTimer.Enabled = true;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the latest wind data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>Wind total Hash or temp Wind Hash</returns>
        public List<LatestWindData> GetLatestWindData(DateTime startDate, DateTime endDate, int Marketkey)
        {
             
                return GetWindData(startDate, endDate, Marketkey);

        }

        /// <summary>
        /// Subscribes the specified start date.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        public void Subscribe(DateTime startDate, DateTime endDate)
        {
            lock (lockobj)
            {
                try
                {
                    IWindDataCallback callBackItem = OperationContext.Current.GetCallbackChannel<IWindDataCallback>();
                    if (sSubscriberHash.ContainsKey(callBackItem))
                    {
                        sSubscriberHash.Remove(callBackItem);
                    }
                    else
                    {
                        sSubscriberHash.Add(callBackItem, startDate.ToShortDateString() + "?" + endDate.ToShortDateString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Calls the back helper.
        /// </summary>
        public void CallBackHelper()
        {
            lock (lockobj)
            {
                try
                {
                    InitializeDB();

                    List<string> keysList = mWindTotalHash.Keys.ToList<string>();

                    foreach (string sKey in keysList)
                    {
                        if (sWindUpdateHash.ContainsKey(sKey))
                        {
                            DateTime oldTime = sWindUpdateHash[sKey];

                            if ((DateTime.Now - oldTime).Minutes > 2)
                            {
                                mWindTotalHash[sKey] = GetWindData(DateTime.Parse(sKey.Split('?')[0]), DateTime.Parse(sKey.Split('?')[1]), 1);
                                sWindUpdateHash[sKey] = DateTime.Now;
                            }
                        }
                        else
                        {
                            mWindTotalHash[sKey] = GetWindData(DateTime.Parse(sKey.Split('?')[0]), DateTime.Parse(sKey.Split('?')[1]), 1);
                            sWindUpdateHash.Add(sKey, DateTime.Now);
                        }
                    }

                    foreach (IWindDataCallback callBackItem in sSubscriberHash.Keys)
                    {
                        try
                        {
                            List<LatestWindData> tempList = new List<LatestWindData>();

                            if (mWindTotalHash.ContainsKey(sSubscriberHash[callBackItem]))
                            {
                                tempList = mWindTotalHash[sSubscriberHash[callBackItem]];
                            }
                            else
                            {
                                string keyTemp = sSubscriberHash[callBackItem];

                                if (keyTemp.Split('?').Length == 2)
                                {
                                    tempList = GetLatestWindData(DateTime.Parse(keyTemp.Split('?')[0]), DateTime.Parse(keyTemp.Split('?')[1]), 1);
                                    mWindTotalHash.Add(keyTemp, tempList);
                                }
                            }
                            callBackItem.SetLatestWindData(tempList);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (!sRemoveHash.Contains(callBackItem))
                            {
                                sRemoveHash.Add(callBackItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    foreach (var item in sRemoveHash)
                    {
                        if (sSubscriberHash.ContainsKey(item))
                        {
                            sSubscriberHash.Remove(item);
                        }
                    }
                    sRemoveHash.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + "\t" + ex.Message);
                }
            }
        }

        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        public bool HeartBeat()
        {
            try
            {
                Console.WriteLine(DateTime.Now + "\t Heartbeat checked by \t" + GetSubscriberIpAddress(OperationContext.Current));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        public void Unsubscribe()
        {
            lock (lockobj)
            {
                try
                {
                    IWindDataCallback callBackItem = OperationContext.Current.GetCallbackChannel<IWindDataCallback>();

                    if (sSubscriberHash.ContainsKey(callBackItem))
                    {
                        sSubscriberHash.Remove(callBackItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        #endregion

        #region Internal Method
        /// <summary>
        /// Connects this instance.
        /// </summary>
        internal void Connect()
        {
            InitializeDB();

            NetTcpBinding binding = new NetTcpBinding();

            binding.OpenTimeout = new TimeSpan(0, 12, 0);
            binding.SendTimeout = new TimeSpan(0, 12, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 12, 0);
            binding.CloseTimeout = new TimeSpan(0, 12, 0);
            binding.Security.Mode = SecurityMode.None;

            ServiceThrottlingBehavior behavior = new ServiceThrottlingBehavior()
            {
                MaxConcurrentCalls = 10000,
                MaxConcurrentInstances = 10000,
                MaxConcurrentSessions = 10000
            };

            ServiceHost host = new ServiceHost(typeof(WindServer));

            host.Description.Behaviors.Add(behavior);
            host.AddServiceEndpoint(typeof(IWindDataProvider), binding, new Uri(Vayu.CommonAccessLibrary.ServiceConnections.GetWindService()));
            try
            {
                host.Open();
                sTimer.Enabled = true;
                sTimer.Interval = 40000;
                sTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                sTimer.Start();
                Console.WriteLine("Successfully opened port 8010 for WindService \t" + DateTime.Now);
                Console.Read();
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

    }
}
