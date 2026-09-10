using Vayu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Security.Permissions;
using Vayu.LoadGraphLibrary;
using Vayu.CommonAccessLibrary;
namespace Vayu.LoadGraphService
{
    /// <summary>
    /// Load Graph Service
    /// </summary>
    /// <seealso cref="Vayu.ILoadGraph" />
    public class LoadGraphServer : ILoadGraph
    {
        #region Private Members SqlCommand n Connection
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuDBConnection;
        
        /// <summary>
        /// The command select forecast command
        /// </summary>
        private SqlCommand cmdSelectForecastCommand;
        /// <summary>
        /// The command select cong forecast command
        /// </summary>
        private SqlCommand cmdSelectCongForecastCommand;

        //
        /// <summary>
        /// The command select cong forecast command
        /// </summary>
        private SqlCommand cmdSelectErcotForecastCommand;
        //
        /// <summary>
        /// The command select cong forecast command b
        /// </summary>
        private SqlCommand cmdSelectCongForecastCommandB;
        /// <summary>
        /// The command select bright forecast command
        /// </summary>
        private SqlCommand cmdSelectBrightForecastCommand;
        /// <summary>
        /// The command select east forecast command
        /// </summary>
        private SqlCommand cmdSelectEastForecastCommand;
        /// <summary>
        /// The command select rt forecast command
        /// </summary>
        private SqlCommand cmdSelectRTForecastCommand;

        /// <summary>
        /// The command select rt forecast command for Ercot
        /// </summary>
        private SqlCommand cmdSelectErcotRTCommand;


        /// <summary>
        /// The command select cong rt command
        /// </summary>
        private SqlCommand cmdSelectCongRTCommand;
        /// <summary>
        /// The command select cong rt command
        /// </summary>
        private SqlCommand cmdSelectCongRTCommandB;
        /// <summary>
        /// The command select bright rt command
        /// </summary>
        private SqlCommand cmdSelectBrightRTCommand;
        /// <summary>
        /// The command select east rt command
        /// </summary>
        private SqlCommand cmdSelectEastRTCommand;
        /// <summary>
        /// The command select da forecast command
        /// </summary>
        private SqlCommand cmdSelectDAForecastCommand;
        /// <summary>
        /// The command select cong da command
        /// </summary>
        private SqlCommand cmdSelectCongDACommand;
        /// <summary>
        /// The command select load command
        /// </summary>
        private SqlCommand cmdSelectLoadCommand;
        /// <summary>
        /// The command select PRT command
        /// </summary>
        private SqlCommand cmdSelectPRTCommand;
        /// <summary>
        /// The command select cong PRT command
        /// </summary>
        private SqlCommand cmdSelectCongPRTCommand;
        /// <summary>
        /// The command select cong PRT command
        /// </summary>
        private SqlCommand cmdSelectCongPRTCommandB;
        /// <summary>
        /// The command select bright PRT command
        /// </summary>
        private SqlCommand cmdSelectBrightPRTCommand;
        /// <summary>
        /// The command select east PRT command
        /// </summary>
        private SqlCommand cmdSelectEastPRTCommand;
        /// <summary>
        /// The command select tesla command
        /// </summary>
        private SqlCommand cmdSelectTeslaCommand;
        /// <summary>
        /// The command select cong tesla command
        /// </summary>
        private SqlCommand cmdSelectCongTeslaCommand;
        /// <summary>
        /// The command select cong tesla command
        /// </summary>
        private SqlCommand cmdSelectCongTeslaCommandB;
        /// <summary>
        /// The command select bright tesla command
        /// </summary>
        private SqlCommand cmdSelectBrightTeslaCommand;
        /// <summary>
        /// The command select east tesla command
        /// </summary>
        private SqlCommand cmdSelectEastTeslaCommand;
        /// <summary>
        /// The command select load reference command
        /// </summary>
        private SqlCommand cmdSelectLoadRefCommand;
        /// <summary>
        /// The command select wsi command
        /// </summary>
        private SqlCommand cmdSelectWsiCommand;
        /// <summary>
        /// The command select south command
        /// </summary>
        private SqlCommand cmdSelectSouthCommand;
        //
        /// <summary>
        /// The connection database data configuration
        /// </summary>
        private SqlConnection mConnectionDBDataConfig;
        /// <summary>
        /// The command select database configuration
        /// </summary>
        private SqlCommand cmdSelectDbConfig;
        //Cory zones
        /// <summary>
        /// The command select DOM cong forecast command
        /// </summary>
        private SqlCommand cmdSelectDomCongForecastCommand;
        /// <summary>
        /// The command select comed cong forecast command
        /// </summary>
        private SqlCommand cmdSelectComedCongForecastCommand;
        /// <summary>
        /// The command select DOM cong iso command
        /// </summary>
        private SqlCommand cmdSelectDomCongISOCommand;
        /// <summary>
        /// The command select comed cong iso command
        /// </summary>
        private SqlCommand cmdSelectComedCongISOCommand;
        /// <summary>
        /// The command select DOM cong PRT command
        /// </summary>
        private SqlCommand cmdSelectDomCongPRTCommand;
        /// <summary>
        /// The command select comed cong PRT command
        /// </summary>
        private SqlCommand cmdSelectComedCongPRTCommand;
        /// <summary>
        /// The command select DOM cong tesla command
        /// </summary>
        private SqlCommand cmdSelectDomCongTeslaCommand;
        /// <summary>
        /// The command select comed cong tesla command
        /// </summary>
        private SqlCommand cmdSelectComedCongTeslaCommand;
        /// <summary>
        /// The command select DOM cong wsi command
        /// </summary>
        private SqlCommand cmdSelectDomCongWSICommand;
        /// <summary>
        /// The command select comed cong wsi command
        /// </summary>
        private SqlCommand cmdSelectComedCongWSICommand;
        /// <summary>
        /// The command select DOM cong DTN command
        /// </summary>
        private SqlCommand cmdSelectDomCongDTNCommand;
        /// <summary>
        /// The command select comed cong DTN command
        /// </summary>
        private SqlCommand cmdSelectComedCongDTNCommand;
        /// <summary>
        /// The command select DOM cong da command
        /// </summary>
        private SqlCommand cmdSelectDomCongDACommand;
        /// <summary>
        /// The command select comed cong da command
        /// </summary>
        private SqlCommand cmdSelectComedCongDACommand;
        /// <summary>
        /// The command select command da command  For Ercot
        /// </summary>
        private SqlCommand cmdSelectErcotDACommand;

        #endregion

        #region Private Members
        /// <summary>
        /// The dictionary cache graph values hash
        /// </summary>
        private static Dictionary<string, HashValues> dictCacheGraphValuesHash = new Dictionary<string, HashValues>();
        /// <summary>
        /// The dictionary cache zone update hash
        /// </summary>
        private static Dictionary<string, DateTime> dictCacheZoneUpdateHash = new Dictionary<string, DateTime>();
        /// <summary>
        /// The dictionary load hash
        /// </summary>
        private static Dictionary<string, Load> dictLoadHash = new Dictionary<string, Load>();
        /// <summary>
        /// The dictionary cache user update hash
        /// </summary>
        private static Dictionary<ILoadGraphCallback, DateTime> dictCacheUserUpdateHash = new Dictionary<ILoadGraphCallback, DateTime>();

        /// <summary>
        /// The dictionary subscriber hash
        /// </summary>
        private static Dictionary<ILoadGraphCallback, object[]> dictSubscriberHash = new Dictionary<ILoadGraphCallback, object[]>();
        /// <summary>
        /// The dictionary subscriber ip adress hash
        /// </summary>
        private static Dictionary<ILoadGraphCallback, string> dictSubscriberIPAdressHash = new Dictionary<ILoadGraphCallback, string>();
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object lockObj = new object();
        /// <summary>
        /// The s timer
        /// </summary>
        private static Timer sTimer = null;
        /// <summary>
        /// The is DST
        /// </summary>
        private static bool sIsDST = false;
        /// <summary>
        /// The DST difference
        /// </summary>
        private int mDstDifference = 2;
        #endregion

        #region Public Methods
        /// <summary>
        /// Subscribes the load graph.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public bool SubscribeLoadGraph(string zone, DateTime startDate, DateTime endDate)
        {
            try
            {
                lock (lockObj)
                {
                    object[] parameters = new object[3];
                    parameters[0] = zone;
                    parameters[1] = startDate.Date;
                    parameters[2] = endDate.Date;
                    ILoadGraphCallback callback = OperationContext.Current.GetCallbackChannel<ILoadGraphCallback>();
                    string temPkey = zone + "@" + startDate.ToString() + "@" + endDate.ToString();
                    if (!dictCacheZoneUpdateHash.ContainsKey(temPkey))
                    {
                        dictCacheZoneUpdateHash.Add(temPkey, DateTime.Today);
                        dictCacheGraphValuesHash.Add(temPkey, null);
                    }
                    if (!dictSubscriberHash.ContainsKey(callback))
                    {
                        dictCacheUserUpdateHash.Add(callback, DateTime.Today);
                        dictSubscriberHash.Add(callback, parameters);
                        Console.WriteLine("Total Subscriber : {0}", dictSubscriberHash.Count);
                    }
                    else
                    {
                        if (!dictCacheUserUpdateHash.ContainsKey(callback))
                        {
                            dictCacheUserUpdateHash.Add(callback, DateTime.Today);
                        }
                        else
                        {
                            dictCacheUserUpdateHash.Remove(callback);
                            dictCacheUserUpdateHash.Add(callback, DateTime.Today);
                        }
                        dictSubscriberHash.Remove(callback);
                        dictSubscriberHash.Add(callback, parameters);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Subscribes the specified is algo.
        /// </summary>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        /// <returns></returns>
        public bool Subscribe(bool isAlgo)
        {
            return false;
        }

        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        public bool HeartBeat()
        {
            return true;
        }

        /// <summary>
        /// Unsubscribes the specified is algo.
        /// </summary>
        /// <param name="isAlgo">if set to <c>true</c> [is algo].</param>
        /// <returns></returns>
        public bool Unsubscribe(bool isAlgo)
        {
            lock (lockObj)
            {
                try
                {
                    ILoadGraphCallback callback = OperationContext.Current.GetCallbackChannel<ILoadGraphCallback>();
                    if (dictSubscriberHash.ContainsKey(callback))
                    {
                        dictCacheUserUpdateHash.Remove(callback);
                        dictSubscriberHash.Remove(callback);
                        Console.WriteLine("Total Subscriber : {0}", dictSubscriberHash.Count);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        public void Connect()
        {
            using (ServiceHost host = new ServiceHost(typeof(LoadGraphServer), new Uri("net.tcp://localhost:8003")))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.OpenTimeout = new TimeSpan(0, 5, 0);
                myBinding.SendTimeout = new TimeSpan(0, 5, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 5, 0);
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.MaxConnections = 1000;
                myBinding.MaxReceivedMessageSize = int.MaxValue;
                myBinding.MaxBufferPoolSize = int.MaxValue;
                var behavior = new ServiceThrottlingBehavior()
                {
                    MaxConcurrentCalls = 10000,
                    MaxConcurrentInstances = 10000,
                    MaxConcurrentSessions = 10000
                };
                host.Description.Behaviors.Add(behavior);
                host.AddServiceEndpoint(typeof(ILoadGraph), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine(DateTime.Now.ToString() + "    Successfully opened port 8003.");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the LoadGraphServer class.
        /// </summary>
        public LoadGraphServer()
        {
            InitDB();
            if (sTimer == null)
            {
                StartTimer();
            }
            if (dictLoadHash.Count <= 0)
            {
                FillLoadHash();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Fills the load hash.
        /// </summary>
        private void FillLoadHash()
        {
            if(VayuDBConnection.State==ConnectionState.Closed)
               VayuDBConnection.Open();
            
            SqlDataReader reader = cmdSelectLoadCommand.ExecuteReader();
            while (reader.Read())
            {
                string zone = reader.GetString(0).Trim();
                string market = reader.GetString(4);
                if (!zone.StartsWith("PJM"))
                {
                    zone = market + " " + zone;
                }
                Load load = new Load();
                load.Tesla = "";
                if (!reader.IsDBNull(1))
                {
                    load.Forecast = (int)reader.GetDecimal(1);
                }
                else
                {
                    load.Forecast = int.MaxValue;
                }
                load.Current = (int)reader.GetDecimal(2);
                
                if (market == "ERCOT")
                { cmdSelectLoadRefCommand.Parameters["@XRefName"].Value = "TESLA"; }
                else { cmdSelectLoadRefCommand.Parameters["@XRefName"].Value = "DayAhead"; }
                // cmdSelectLoadRefCommand.Parameters["@XRefName"].Value = "DayAhead";
                cmdSelectLoadRefCommand.Parameters["@LoadsKey"].Value = load.Current;
                var reader1 = cmdSelectLoadRefCommand.ExecuteReader();
                while (reader1.Read())
                {
                    load.DayAhead = reader1.GetString(0);
                }
                reader1.Close();
                 
                if (!dictLoadHash.ContainsKey(zone))
                    dictLoadHash.Add(zone, load);
            }
            reader.Close();
            VayuDBConnection.Close();
        }

        /// <summary>
        /// Initializes the database related object.
        /// </summary>
        private void InitDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            
            
            cmdSelectForecastCommand = new SqlCommand();
            cmdSelectForecastCommand.CommandText = "select marketdateTime, mw from loadforecasts where marketdatetime >= @START_DATE and " +
                                                 "loadforecasttypekey = @loadForecastTypeKey and marketdatetime < @END_DATE order by marketdatetime";
            cmdSelectForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectForecastCommand.Parameters.AddWithValue("@loadForecastTypeKey", "loadForecastTypeKey");
            cmdSelectForecastCommand.Connection = VayuDBConnection;
            //
            cmdSelectCongForecastCommand = new SqlCommand();
            cmdSelectCongForecastCommand.CommandText = "select a.MarketDateTime, a.MW + b.MW + c.MW - d.MW from LoadForecasts a, LoadForecasts b, LoadForecasts c, LoadForecasts d  " +
                "where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime " +
                "and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = d.MarketDateTime and a.LoadForecastTypeKey = 14 and b.LoadForecastTypeKey = 5 and c.LoadForecastTypeKey = 21 and d.LoadForecastTypeKey = 4 order by a.MarketDateTime";
            cmdSelectCongForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectCongForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectCongForecastCommand.Connection = VayuDBConnection;
            //Ercot

            cmdSelectErcotForecastCommand = new SqlCommand();
             cmdSelectErcotForecastCommand.CommandText = "select top 24 MarketDateTime,MW from LoadForecasts  where MarketDateTime >=@START_DATE and MarketDateTime<=@END_DATE and LoadForecastTypeKey=@LoadForecastTypeKey order by MarketDateTime";
            cmdSelectErcotForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectErcotForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectErcotForecastCommand.Parameters.AddWithValue("@LoadForecastTypeKey", "LoadForecastTypeKey");
            cmdSelectErcotForecastCommand.Connection = VayuDBConnection;
            //
            cmdSelectCongForecastCommandB = new SqlCommand();
            cmdSelectCongForecastCommandB.CommandText = "CongestionBForecastProc";
            cmdSelectCongForecastCommandB.CommandType = CommandType.StoredProcedure;
            cmdSelectCongForecastCommandB.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectCongForecastCommandB.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectCongForecastCommandB.Connection = VayuDBConnection;

            //

            cmdSelectBrightForecastCommand = new SqlCommand();
            cmdSelectBrightForecastCommand.CommandText = "select a.MarketDateTime, (a.MW/3) + ((b.MW + c.MW)/6) + d.MW + (2 * (e.MW + f.MW)) - g.MW from LoadForecasts a, LoadForecasts b, LoadForecasts c, LoadForecasts d, " +
                "LoadForecasts e, LoadForecasts f, LoadForecasts g where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "e.MarketDateTime >= @START_DATE and f.MarketDateTime >= @START_DATE and g.MarketDateTime  >= @START_DATE and a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and " +
                "c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and e.MarketDateTime < @END_DATE and f.MarketDateTime < @END_DATE and g.MarketDateTime < @END_DATE and " +
                "a.MarketDateTime = b.MarketDateTime and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and a.MarketDateTime = e.MarketDateTime and " +
                "a.MarketDateTime = f.MarketDateTime and a.MarketDateTime = g.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and " +
                "b.MarketDateTime = e.MarketDateTime and b.MarketDateTime = f.MarketDateTime and b.MarketDateTime = g.MarketDateTime and c.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = e.MarketDateTime and c.MarketDateTime = f.MarketDateTime and c.MarketDateTime = g.MarketDateTime and d.MarketDateTime = e.MarketDateTime and " +
                "d.MarketDateTime = f.MarketDateTime and d.MarketDateTime = g.MarketDateTime and e.MarketDateTime = f.MarketDateTime and e.MarketDateTime = g.MarketDateTime and " +
                "f.MarketDateTime = g.MarketDateTime and a.LoadForecastTypeKey = 9 and b.LoadForecastTypeKey = 3 and c.LoadForecastTypeKey = 2 and d.LoadForecastTypeKey = 9 and " +
                "e.LoadForecastTypeKey = 14 and f.LoadForecastTypeKey = 5 and g.LoadForecastTypeKey = 19 order by a.MarketDateTime";
            cmdSelectBrightForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectBrightForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectBrightForecastCommand.Connection = VayuDBConnection;
            //
            cmdSelectEastForecastCommand = new SqlCommand();
            cmdSelectEastForecastCommand.CommandText = "select a.MarketDateTime, a.MW - b.MW - c.MW + (d.MW/2) + (e.MW/2) from LoadForecasts a, LoadForecasts b, LoadForecasts c, LoadForecasts d, LoadForecasts e " +
                "where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "e.MarketDateTime >= @START_DATE and a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and " +
                "e.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and a.MarketDateTime = e.MarketDateTime and " +
                "b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and b.MarketDateTime = e.MarketDateTime and c.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = e.MarketDateTime and d.MarketDateTime = e.MarketDateTime and a.LoadForecastTypeKey = 19 and b.LoadForecastTypeKey = 16 and c.LoadForecastTypeKey = 22 and d.LoadForecastTypeKey = 6 and e.LoadForecastTypeKey = 21 order by a.MarketDateTime";
            cmdSelectEastForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectEastForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectEastForecastCommand.Connection = VayuDBConnection;
            //
            cmdSelectSouthCommand = new SqlCommand();
            cmdSelectSouthCommand.CommandText = "select a.MarketDateTime, a.MW - b.MW - c.MW from LoadForecasts a, LoadForecasts b, LoadForecasts c " +
                "where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and " +
                "a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime " +
                "and a.MarketDateTime = c.MarketDateTime and b.MarketDateTime = c.MarketDateTime and a.LoadForecastTypeKey = 10 and b.LoadForecastTypeKey = 1" +
                "and c.LoadForecastTypeKey = 7 order by a.MarketDateTime";
            cmdSelectSouthCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectSouthCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectSouthCommand.Connection = VayuDBConnection;
            //
            cmdSelectCongRTCommand = new SqlCommand();
            cmdSelectCongRTCommand.CommandText = "select a.MarketDateTime, a.MW + b.MW + c.MW - d.MW  from loadrt a, loadrt b, loadrt c, loadrt d " +
                "where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime " +
                "and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = d.MarketDateTime and a.loadskey = 14 and b.loadskey = 5 and c.loadskey = 21 and d.loadskey = 4 order by a.MarketDateTime";
            cmdSelectCongRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectCongRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectCongRTCommand.Connection = VayuDBConnection;
            //                
            cmdSelectCongRTCommandB = new SqlCommand();
            cmdSelectCongRTCommandB.CommandText = "Select a.MarketDateTime, (IsNull(a.MW,0) + Isnull(b.MW,0) + Isnull(c.MW,0)) + (1/4.0 * (Isnull(d.MW,0) + Isnull(e.MW,0) + Isnull(f.MW,0) + Isnull(g.MW,0) + Isnull(h.MW,0) + Isnull(i.MW,0))) - Isnull(j.MW,0) - (Isnull(k.MW,0) * (3/4.0)) - (1/4.0 * Isnull(l.MW,0)) - (3/4.0 * Isnull(m.MW,0)) - (1/2.0 * Isnull(n.MW,0)) " +
                                                "From loadrt a, loadrt b, loadrt c, loadrt d, loadrt e, loadrt f, loadrt g, loadrt h, loadrt i, loadrt j, loadrt k, loadrt l, loadrt m, loadrt n Where  a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE " +
                                                "And d.MarketDateTime  >= @START_DATE and e.MarketDateTime  >= @START_DATE and f.MarketDateTime  >= @START_DATE And g.MarketDateTime  >= @START_DATE and h.MarketDateTime  >= @START_DATE and i.MarketDateTime  >= @START_DATE And j.MarketDateTime  >= @START_DATE and k.MarketDateTime  >= @START_DATE and l.MarketDateTime  >= @START_DATE " +
                                                "And m.MarketDateTime  >= @START_DATE and n.MarketDateTime  >= @START_DATE and a.MarketDateTime < @END_DATE And b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE and d.MarketDateTime < @END_DATE And e.MarketDateTime < @END_DATE and f.MarketDateTime < @END_DATE and g.MarketDateTime < @END_DATE And h.MarketDateTime < @END_DATE and i.MarketDateTime < @END_DATE and j.MarketDateTime < @END_DATE " +
                                                "And k.MarketDateTime < @END_DATE and l.MarketDateTime < @END_DATE and m.MarketDateTime < @END_DATE And n.MarketDateTime < @END_DATE And a.MarketDateTime = b.MarketDateTime And b.MarketDateTime = c.MarketDateTime And c.MarketDateTime = d.MarketDateTime And d.MarketDateTime = e.MarketDateTime And e.MarketDateTime = f.MarketDateTime And f.MarketDateTime = g.MarketDateTime And g.MarketDateTime = h.MarketDateTime And h.MarketDateTime = i.MarketDateTime " +
                                                "And i.MarketDateTime = j.MarketDateTime And j.MarketDateTime = k.MarketDateTime And k.MarketDateTime = l.MarketDateTime And l.MarketDateTime = m.MarketDateTime And m.MarketDateTime = n.MarketDateTime And a.loadskey = 14 and b.loadskey = 5 and c.loadskey = 21 and d.loadskey = 11 and e.loadskey = 2 and f.loadskey = 9 and g.loadskey = 13 and h.loadskey = 17 and i.loadskey = 12 and j.loadskey = 4  and k.loadskey = 3 and l.loadskey = 6 and m.loadskey = 2207 and n.loadskey = 7 Order By a.MarketDateTime";
            cmdSelectCongRTCommandB.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectCongRTCommandB.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectCongRTCommandB.Connection = VayuDBConnection;
            //
            cmdSelectBrightRTCommand = new SqlCommand();
            cmdSelectBrightRTCommand.CommandText = "select a.MarketDateTime, (a.MW/3) + ((b.MW + c.MW)/6) + d.MW + (2 * (e.MW + f.MW)) - g.MW from loadrt a, loadrt b, loadrt c, loadrt d, " +
                "loadrt e, loadrt f, loadrt g where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "e.MarketDateTime >= @START_DATE and f.MarketDateTime >= @START_DATE and g.MarketDateTime  >= @START_DATE and a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and " +
                "c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and e.MarketDateTime < @END_DATE and f.MarketDateTime < @END_DATE and g.MarketDateTime < @END_DATE and " +
                "a.MarketDateTime = b.MarketDateTime and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and a.MarketDateTime = e.MarketDateTime and " +
                "a.MarketDateTime = f.MarketDateTime and a.MarketDateTime = g.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and " +
                "b.MarketDateTime = e.MarketDateTime and b.MarketDateTime = f.MarketDateTime and b.MarketDateTime = g.MarketDateTime and c.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = e.MarketDateTime and c.MarketDateTime = f.MarketDateTime and c.MarketDateTime = g.MarketDateTime and d.MarketDateTime = e.MarketDateTime and " +
                "d.MarketDateTime = f.MarketDateTime and d.MarketDateTime = g.MarketDateTime and e.MarketDateTime = f.MarketDateTime and e.MarketDateTime = g.MarketDateTime and " +
                "f.MarketDateTime = g.MarketDateTime and a.loadskey = 22 and b.loadskey = 3 and c.loadskey = 4 and d.loadskey = 21 and e.loadskey = 14 and f.loadskey = 5 and " +
                "g.loadskey = 19 order by a.MarketDateTime";
            cmdSelectBrightRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectBrightRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectBrightRTCommand.Connection = VayuDBConnection;
            //
            cmdSelectEastRTCommand = new SqlCommand();
            cmdSelectEastRTCommand.CommandText = "select a.MarketDateTime, a.MW - b.MW - c.MW + (d.MW/2) + (e.MW/2) from loadrt a, loadrt b, loadrt c, loadrt d, loadrt e " +
                "where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "e.MarketDateTime >= @START_DATE and a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and " +
                "e.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and a.MarketDateTime = e.MarketDateTime and " +
                "b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and b.MarketDateTime = e.MarketDateTime and c.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = e.MarketDateTime and d.MarketDateTime = e.MarketDateTime and a.loadskey = 19 and b.loadskey = 16 and c.loadskey = 22 and d.loadskey = 6 and e.loadskey = 21 order by a.MarketDateTime";
            cmdSelectEastRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectEastRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectEastRTCommand.Connection = VayuDBConnection;
            //
            cmdSelectRTForecastCommand = new SqlCommand();
            cmdSelectRTForecastCommand.CommandText = "select marketdateTime, mw from loadrt where marketdatetime > @START_DATE " +
                                                "and loadskey = @loadskey and marketdatetime <= @END_DATE  order by marketdatetime";
            cmdSelectRTForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectRTForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectRTForecastCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            cmdSelectRTForecastCommand.Connection = VayuDBConnection;
            // Ercot
            cmdSelectErcotRTCommand = new SqlCommand();
            cmdSelectErcotRTCommand.CommandText = "select marketdateTime, mw from Vayu..loadrt where marketdatetime > @START_DATE " +
                                                "and loadskey = @loadskey and marketdatetime <= @END_DATE  order by marketdatetime";
            cmdSelectErcotRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectErcotRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectErcotRTCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            cmdSelectErcotRTCommand.Connection = VayuDBConnection;
            //

            //
            cmdSelectDAForecastCommand = new SqlCommand();
            cmdSelectDAForecastCommand.CommandText = "select marketdateTime, value from dayaheadload where marketdatetime >= @START_DATE " +
                                                "and name = @name and marketdatetime < @END_DATE  order by marketdatetime";
            cmdSelectDAForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectDAForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectDAForecastCommand.Parameters.AddWithValue("@name", "name");
            cmdSelectDAForecastCommand.Connection = VayuDBConnection;
            // ERCOT
            cmdSelectErcotDACommand = new SqlCommand();
            cmdSelectErcotDACommand.CommandText = "select marketdateTime, value from Vayu..dayaheadload where marketdatetime >= @START_DATE " +
                                                "and name = @name and marketdatetime < @END_DATE  order by marketdatetime";
            cmdSelectErcotDACommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectErcotDACommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectErcotDACommand.Parameters.AddWithValue("@name", "name");
            cmdSelectErcotDACommand.Connection = VayuDBConnection;
            //
            //
            cmdSelectCongDACommand = new SqlCommand();
            cmdSelectCongDACommand.CommandText = "select a.MarketDateTime, a.value + b.value + c.value - d.value  from dayaheadload a, dayaheadload b, dayaheadload c, dayaheadload d " +
                "where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime " +
                "and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = d.MarketDateTime and a.name = ' and b.name = 5 and c.name = 21 and d.name = 4 order by a.MarketDateTime";
            cmdSelectCongDACommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectCongDACommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectCongDACommand.Connection = VayuDBConnection;
            //
            cmdSelectLoadCommand = new SqlCommand();
            cmdSelectLoadCommand.CommandText = "select LoadsName, loadForecastTypeKey, loadskey, l.marketkey, case when mk.Label = 'ERCOTTesting' then 'ERCOT' else mk.Label end as MarketLabel " +
                " from Loads l inner join Market mk on l.MarketKey = mk.MarketKey where l.MarketKey in (1,2,9) order by l.marketkey, loadsname ";
            cmdSelectLoadCommand.Connection = VayuDBConnection;
            //
            cmdSelectPRTCommand = new SqlCommand();
            cmdSelectPRTCommand.CommandText = "select MarketDateTime, LoadForecast from PRT where marketdatetime >= @START_DATE and marketdatetime < @END_DATE " +
                                                "and name = @name order by marketdatetime";
            cmdSelectPRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectPRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectPRTCommand.Parameters.AddWithValue("@name", "@name");
            cmdSelectPRTCommand.Connection = VayuDBConnection;
            //
            cmdSelectCongPRTCommand = new SqlCommand();
            cmdSelectCongPRTCommand.CommandText = "select a.MarketDateTime, a.LoadForecast + b.LoadForecast + c.LoadForecast - d.LoadForecast from PRT a, PRT b, PRT c, PRT d " +
                "where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime " +
                "and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = d.MarketDateTime and a.name = 'PEPCO' and b.name = 'BGE' and c.name = 'SouthernRegion' and d.name = 'APS' order by a.MarketDateTime";
            cmdSelectCongPRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectCongPRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectCongPRTCommand.Connection = VayuDBConnection;
            //
            cmdSelectCongPRTCommandB = new SqlCommand();
            cmdSelectCongPRTCommandB.CommandText = "Select a.MarketDateTime, (IsNull(a.LoadForecast,0) + Isnull(b.LoadForecast,0) + Isnull(c.LoadForecast,0)) + (1/4.0 * (Isnull(d.LoadForecast,0) + Isnull(e.LoadForecast,0) + Isnull(f.LoadForecast,0) + Isnull(g.LoadForecast,0) + Isnull(h.LoadForecast,0) + Isnull(i.LoadForecast,0))) - Isnull(j.LoadForecast,0) - (Isnull(k.LoadForecast,0) * (3/4.0)) - (1/4.0 * Isnull(l.LoadForecast,0)) - (3/4.0 * Isnull(m.LoadForecast,0)) - (1/2.0 * Isnull(n.LoadForecast,0)) " +
                                                 "From PRT a, PRT b, PRT c, PRT d , PRT e , PRT f, PRT g, PRT h, PRT i, PRT j, PRT k, PRT l, PRT m, PRT n Where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE " +
                                                 "And d.MarketDateTime  >= @START_DATE and e.MarketDateTime  >= @START_DATE and f.MarketDateTime  >= @START_DATE And g.MarketDateTime  >= @START_DATE and h.MarketDateTime  >= @START_DATE and i.MarketDateTime  >= @START_DATE " +
                                                 "And j.MarketDateTime  >= @START_DATE and k.MarketDateTime  >= @START_DATE and l.MarketDateTime  >= @START_DATE And m.MarketDateTime  >= @START_DATE and n.MarketDateTime  >= @START_DATE and a.MarketDateTime < @END_DATE " +
                                                 "And b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE and d.MarketDateTime < @END_DATE And e.MarketDateTime < @END_DATE and f.MarketDateTime < @END_DATE and g.MarketDateTime < @END_DATE " +
                                                 "And h.MarketDateTime < @END_DATE and i.MarketDateTime < @END_DATE and j.MarketDateTime < @END_DATE And k.MarketDateTime < @END_DATE and l.MarketDateTime < @END_DATE and m.MarketDateTime < @END_DATE And n.MarketDateTime < @END_DATE And a.MarketDateTime = b.MarketDateTime And b.MarketDateTime = c.MarketDateTime And c.MarketDateTime = d.MarketDateTime And d.MarketDateTime = e.MarketDateTime " +
                                                 "And e.MarketDateTime = f.MarketDateTime And f.MarketDateTime = g.MarketDateTime And g.MarketDateTime = h.MarketDateTime And h.MarketDateTime = i.MarketDateTime And i.MarketDateTime = j.MarketDateTime And j.MarketDateTime = k.MarketDateTime And k.MarketDateTime = l.MarketDateTime And l.MarketDateTime = m.MarketDateTime " +
                                                 "And m.MarketDateTime = n.MarketDateTime And a.Name = 'PEPCO' and b.Name = 'BGE' and c.Name = 'SouthernRegion' and d.Name = 'JCPL' and e.Name = 'AECO' and f.Name = 'DPL' and g.Name = 'PECO' And h.Name = 'PSEG' and i.Name = 'METED' and j.Name = 'APS' and k.Name = 'AEP' and l.Name = 'COMED' and m.Name = 'DUQ' and n.Name = 'DAYTON' Order By a.MarketDateTime";
            cmdSelectCongPRTCommandB.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectCongPRTCommandB.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectCongPRTCommandB.Connection = VayuDBConnection;
            //
            cmdSelectBrightPRTCommand = new SqlCommand();
            cmdSelectBrightPRTCommand.CommandText = "select a.MarketDateTime, (a.LoadForecast/3) + ((b.LoadForecast + c.LoadForecast)/6) + d.LoadForecast + " +
                "(2 * (e.LoadForecast + f.LoadForecast)) - g.LoadForecast from PRT a, PRT b, PRT c, PRT d, " +
                "PRT e, PRT f, PRT g where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and " +
                "e.MarketDateTime >= @START_DATE and f.MarketDateTime >= @START_DATE and g.MarketDateTime  >= @START_DATE and a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and " +
                "c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and e.MarketDateTime < @END_DATE and f.MarketDateTime < @END_DATE and g.MarketDateTime < @END_DATE and " +
                "a.MarketDateTime = b.MarketDateTime and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and a.MarketDateTime = e.MarketDateTime and " +
                "a.MarketDateTime = f.MarketDateTime and a.MarketDateTime = g.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and " +
                "b.MarketDateTime = e.MarketDateTime and b.MarketDateTime = f.MarketDateTime and b.MarketDateTime = g.MarketDateTime and c.MarketDateTime = d.MarketDateTime and " +
                "c.MarketDateTime = e.MarketDateTime and c.MarketDateTime = f.MarketDateTime and c.MarketDateTime = g.MarketDateTime and d.MarketDateTime = e.MarketDateTime and " +
                "d.MarketDateTime = f.MarketDateTime and d.MarketDateTime = g.MarketDateTime and e.MarketDateTime = f.MarketDateTime and e.MarketDateTime = g.MarketDateTime and " +
                "f.MarketDateTime = g.MarketDateTime and a.name = 'WesternRegionLoadForecast' and b.name = 'AEP' and c.name = 'APS' and d.name = 'SouthernRegion' and " +
                "e.name = 'PEPCO' and f.name = 'BGE' and g.name = 'MidAtlanticRegionLoadForecast' order by a.MarketDateTime";
            cmdSelectBrightPRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectBrightPRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectBrightPRTCommand.Connection = VayuDBConnection;
            //

            cmdSelectEastPRTCommand = new SqlCommand();
            cmdSelectEastPRTCommand.CommandText = "select  a.MarketDateTime, a.LoadForecast - b.LoadForecast - c.LoadForecast + (d.LoadForecast/2) + (e.LoadForecast/2) from PRT  a, PRT  b, PRT  c, PRT  d, PRT  e where a.MarketDateTime >= @START_DATE and b.MarketDateTime >= @START_DATE and c.MarketDateTime  >= @START_DATE and d.MarketDateTime  >= @START_DATE and e.MarketDateTime >= @START_DATE and a.MarketDateTime < @END_DATE and b.MarketDateTime < @END_DATE and c.MarketDateTime < @END_DATE  and d.MarketDateTime < @END_DATE and e.MarketDateTime < @END_DATE and a.MarketDateTime = b.MarketDateTime and a.MarketDateTime = c.MarketDateTime and a.MarketDateTime = d.MarketDateTime and a.MarketDateTime = e.MarketDateTime and b.MarketDateTime = c.MarketDateTime and b.MarketDateTime = d.MarketDateTime and b.MarketDateTime = e.MarketDateTime and c.MarketDateTime = d.MarketDateTime and c.MarketDateTime = e.MarketDateTime and d.MarketDateTime = e.MarketDateTime and a.name= 'MidAtlanticRegionLoadForecast' and b.name = 'PENELEC' and c.name = 'WesternRegionLoadForecast' and d.name = 'COMED' and e.name = 'SouthernRegion' order by a.MarketDateTime";
            cmdSelectEastPRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            cmdSelectEastPRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            cmdSelectEastPRTCommand.Connection = VayuDBConnection;
            //
            cmdSelectTeslaCommand = new SqlCommand();
            cmdSelectTeslaCommand.CommandText = "select tesla_time, forecast from TESLA where tesla_date = @START_DATE and area = @area order by tesla_time";
            cmdSelectTeslaCommand.Parameters.AddWithValue("@START_DATE", "tesla_date");
            cmdSelectTeslaCommand.Parameters.AddWithValue("@area", "area");
            cmdSelectTeslaCommand.Connection = VayuDBConnection;
            //
            cmdSelectCongTeslaCommand = new SqlCommand();
            cmdSelectCongTeslaCommand.CommandText = "select a.tesla_time, a.forecast + b.forecast + c.forecast - d.forecast from TESLA a, TESLA b, TESLA c, TESLA d " +
                "where a.tesla_date = @START_DATE and b.tesla_date = @START_DATE and c.tesla_date = @START_DATE  and d.tesla_date = @START_DATE and a.tesla_time = b.tesla_time " +
                "and a.tesla_time = c.tesla_time and a.tesla_time = d.tesla_time and b.tesla_time = c.tesla_time and b.tesla_time = d.tesla_time and " +
                "c.tesla_time = d.tesla_time and a.area = 'PJM PEP' and b.area = 'PJM BC' and c.area = 'PJM DOMIN' and d.area = 'PJM APS' order by a.tesla_time";
            cmdSelectCongTeslaCommand.Parameters.AddWithValue("@START_DATE", "tesla_date");
            cmdSelectCongTeslaCommand.Connection = VayuDBConnection;
            //
            cmdSelectCongTeslaCommandB = new SqlCommand();
            cmdSelectCongTeslaCommandB.CommandText = "Select a.tesla_time, (IsNull(a.forecast,0) + Isnull(b.forecast,0) + Isnull(c.forecast,0)) + (1/4.0 * (Isnull(d.forecast,0) + Isnull(e.forecast,0) + Isnull(f.forecast,0) + Isnull(g.forecast,0) + Isnull(h.forecast,0) + Isnull(i.forecast,0))) - Isnull(j.forecast,0) - (Isnull(k.forecast,0) * (3/4.0)) - (1/4.0 * Isnull(l.forecast,0)) - (3/4.0 * Isnull(m.forecast,0)) - (1/2.0 * Isnull(n.forecast,0)) " +
                                                   "From TESLA a, TESLA b, TESLA c, TESLA d, TESLA e, TESLA f, TESLA g, TESLA h, TESLA i, TESLA j, TESLA k, TESLA l, TESLA m, TESLA n " +
                                                   "Where a.tesla_date = @START_DATE and b.tesla_date = @START_DATE and c.tesla_date = @START_DATE  and d.tesla_date = @START_DATE and e.tesla_date = @START_DATE and f.tesla_date = @START_DATE and g.tesla_date = @START_DATE and h.tesla_date = @START_DATE and i.tesla_date = @START_DATE and j.tesla_date = @START_DATE and k.tesla_date = @START_DATE and l.tesla_date = @START_DATE and m.tesla_date = @START_DATE and n.tesla_date = @START_DATE " +
                                                   "and a.tesla_time = b.tesla_time and a.tesla_time = c.tesla_time and a.tesla_time = d.tesla_time and a.tesla_time = e.tesla_time and a.tesla_time = f.tesla_time and a.tesla_time = g.tesla_time and a.tesla_time = h.tesla_time and a.tesla_time = i.tesla_time and a.tesla_time = j.tesla_time and a.tesla_time = k.tesla_time and a.tesla_time = l.tesla_time and a.tesla_time = m.tesla_time and a.tesla_time = n.tesla_time " +
                                                   "And a.area = 'PJM PEP' and b.area = 'PJM BC' and c.area = 'PJM Southern' and d.area = 'PJM JC' and e.area = 'PJM AE' and f.area = 'PJM DPL' and g.area = 'PJM PE' And h.area = 'PJM PS' and i.area = 'PJM ME' and j.area = 'PJM APS' and k.area = 'PJM AEPOWER' and l.area = 'PJM COMED' and m.area = 'PJM DEOK' and n.area = 'PJM DAYTON Zone' Order By a.tesla_time";
            cmdSelectCongTeslaCommandB.Parameters.AddWithValue("@START_DATE", "tesla_date");
            cmdSelectCongTeslaCommandB.Connection = VayuDBConnection;
            //
            cmdSelectBrightTeslaCommand = new SqlCommand();
            cmdSelectBrightTeslaCommand.CommandText = "select a.tesla_time, (a.forecast/3) + ((b.forecast + c.forecast)/6) + d.forecast + " +
                                                    "(2 * (e.forecast + f.forecast)) - g.forecast from TESLA a, TESLA b, TESLA c, TESLA d, " +
                                                    "TESLA e, TESLA f, TESLA g where a.tesla_date = @START_DATE and b.tesla_date = @START_DATE and c.tesla_date = @START_DATE and d.tesla_date = @START_DATE and " +
                                                    "e.tesla_date = @START_DATE and f.tesla_date = @START_DATE and g.tesla_date = @START_DATE and " +
                                                    "a.area = 'PJM Western' and b.area = 'PJM AEPOWER' and c.area = 'PJM APS' and d.area = 'PJM Southern' and " +
                                                    "e.area = 'PJM PEP' and f.area = 'PJM BC' and g.area = 'PJM Mid-Atlantic' and " +
                                                    "a.tesla_time = b.tesla_time and a.tesla_time = c.tesla_time and a.tesla_time = d.tesla_time and a.tesla_time = e.tesla_time " +
                                                    "and a.tesla_time = f.tesla_time and a.tesla_time = g.tesla_time order by a.tesla_time";
            cmdSelectBrightTeslaCommand.Parameters.AddWithValue("@START_DATE", "tesla_date");
            cmdSelectBrightTeslaCommand.Connection = VayuDBConnection;
            //

            cmdSelectEastTeslaCommand = new SqlCommand();
            cmdSelectEastTeslaCommand.CommandText = "select a.tesla_time, a.forecast - b.forecast - c.forecast + (d.forecast/2) + (e.forecast/2) from TESLA a, TESLA b, TESLA c, TESLA d, TESLA e where a.tesla_date = @START_DATE and b.tesla_date = @START_DATE and c.tesla_date = @START_DATE and d.tesla_date = @START_DATE and e.tesla_date = @START_DATE and a.area = 'PJM Mid-Atlantic' and b.area = 'PJM PN' and c.area = 'PJM Western' and d.area = 'PJM COMED' and e.area = 'PJM Southern' and a.tesla_time = b.tesla_time and a.tesla_time = c.tesla_time and a.tesla_time = d.tesla_time and a.tesla_time = e.tesla_time";
            cmdSelectEastTeslaCommand.Parameters.AddWithValue("@START_DATE", "tesla_date");
            cmdSelectEastTeslaCommand.Connection = VayuDBConnection;

            //
            cmdSelectLoadRefCommand = new SqlCommand();
            cmdSelectLoadRefCommand.CommandText = "select LoadName from LoadXRefType where XRefName = @XRefName and LoadsKey = @LoadsKey";
            cmdSelectLoadRefCommand.Parameters.AddWithValue("@LoadsKey", "LoadsKey");
            cmdSelectLoadRefCommand.Parameters.AddWithValue("@XRefName", "XRefName");
            cmdSelectLoadRefCommand.Connection = VayuDBConnection;
            //
            cmdSelectWsiCommand = new SqlCommand();
            cmdSelectWsiCommand.CommandText = "select time, load_fcst from pjm_east_zonal_total_load_fcst where date = @date";
            cmdSelectWsiCommand.Parameters.AddWithValue("@date", "date");
            cmdSelectWsiCommand.Connection = VayuDBConnection;


            cmdSelectDomCongForecastCommand = VayuDBConnection.CreateCommand();
            cmdSelectDomCongForecastCommand.CommandText = "select aep.MarketDateTime ,(bc.MW+pep.MW-dom.MW)+((midat.MW-bc.MW-pep.MW-pn.MW)*0.5)-(0.5*aep.MW)+(0.35*(ap.MW))  " +
            " from LoadRT aep,LoadRT bc, LoadRT pep, LoadRT dom, LoadRT midat ,LoadRT pn,LoadRT ap " +
            " where bc.LoadsKey=5 and pep.LoadsKey=14 and dom.LoadsKey=8 and midat.LoadsKey=19     " +
            " and pn.LoadsKey=16 and ap.LoadsKey=4 and aep.LoadsKey=3                             " +
            " and bc.MarketDateTime>=@START_DATE and bc.MarketDateTime<@END_DATE               " +
            " and pep.MarketDateTime>=@START_DATE and pep.MarketDateTime<@END_DATE             " +
            " and aep.MarketDateTime>=@START_DATE and aep.MarketDateTime<@END_DATE             " +
            " and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE             " +
            " and pn.MarketDateTime>=@START_DATE and pn.MarketDateTime<@END_DATE               " +
            " and midat.MarketDateTime>=@START_DATE and midat.MarketDateTime<@END_DATE         " +
            " and ap.MarketDateTime>=@START_DATE and ap.MarketDateTime<@END_DATE " +
            " and aep.MarketDateTime=bc.MarketDateTime and aep.MarketDateTime=pep.MarketDateTime and aep.MarketDateTime=dom.MarketDateTime " +
            " and aep.MarketDateTime=midat.MarketDateTime and aep.MarketDateTime=pn.MarketDateTime and aep.MarketDateTime=ap.MarketDateTime " +
            " and bc.MarketDateTime=pep.MarketDateTime and bc.MarketDateTime=dom.MarketDateTime and bc.MarketDateTime=midat.MarketDateTime and bc.MarketDateTime=pn.MarketDateTime and bc.MarketDateTime=ap.MarketDateTime " +
            " and pep.MarketDateTime=dom.MarketDateTime and pep.MarketDateTime=midat.MarketDateTime and pep.MarketDateTime=pn.MarketDateTime and pep.MarketDateTime=ap.MarketDateTime " +
            " and dom.MarketDateTime=midat.MarketDateTime and dom.MarketDateTime=pn.MarketDateTime and dom.MarketDateTime=ap.MarketDateTime " +
            " and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime " +
            " and pn.MarketDateTime=ap.MarketDateTime";
            cmdSelectDomCongForecastCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectDomCongForecastCommand.Parameters.AddWithValue("@END_DATE", "");
            cmdSelectComedCongForecastCommand = VayuDBConnection.CreateCommand();

            cmdSelectComedCongForecastCommand.CommandText = "select ap.MarketDateTime ,(0.25*midat.MW)+(0.25*dom.MW)+(0.5*ap.MW)+(west.MW-ap.MW-comed.MW)-comed.MW from LoadRT midat ,LoadRT dom,LoadRT ap,LoadRT west, LoadRT comed " +
             " where  west.MarketDateTime >= @START_DATE and west.MarketDateTime < @END_DATE and comed.MarketDateTime >=@START_DATE and comed.MarketDateTime < @END_DATE and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE " +
             " and midat.MarketDateTime >= @START_DATE and midat.MarketDateTime < @END_DATE and ap.MarketDateTime >= @START_DATE and ap.MarketDateTime < @END_DATE and  west.LoadsKey=22 and comed.LoadsKey=6 and dom.LoadsKey=8 and midat.LoadsKey=19 and ap.LoadsKey=4 " +
             " and  midat.MarketDateTime = dom.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime and midat.MarketDateTime = west.MarketDateTime and midat.MarketDateTime=comed.MarketDateTime and  dom.MarketDateTime=ap.MarketDateTime and dom.MarketDateTime=west.MarketDateTime and dom.MarketDateTime=comed.MarketDateTime and " +
             " ap.MarketDateTime = west.MarketDateTime and ap.MarketDateTime = comed.MarketDateTime and west.MarketDateTime = comed.MarketDateTime";
            cmdSelectComedCongForecastCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectComedCongForecastCommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectDomCongISOCommand = VayuDBConnection.CreateCommand();
            cmdSelectDomCongISOCommand.CommandText = "select aep.MarketDateTime ,(bc.MW+pep.MW-dom.MW)+((midat.MW-bc.MW-pep.MW-pn.MW)*0.5)-(0.5*aep.MW)+(0.35*(ap.MW))  " +
            " from LoadForecasts aep,LoadForecasts bc, LoadForecasts pep, LoadForecasts dom, LoadForecasts midat ,LoadForecasts pn,LoadForecasts ap where bc.LoadForecastTypeKey=36 and pep.LoadForecastTypeKey=41 and dom.LoadForecastTypeKey=8 and midat.LoadForecastTypeKey=18 " +
            " and pn.LoadForecastTypeKey=43 and ap.LoadForecastTypeKey=2 and aep.LoadForecastTypeKey=3 and bc.MarketDateTime>=@START_DATE and bc.MarketDateTime<@END_DATE and pep.MarketDateTime>=@START_DATE and pep.MarketDateTime<@END_DATE " +
            " and aep.MarketDateTime>=@START_DATE and aep.MarketDateTime<@END_DATE and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE and pn.MarketDateTime>=@START_DATE and pn.MarketDateTime<@END_DATE " +
            " and midat.MarketDateTime>=@START_DATE and midat.MarketDateTime<@END_DATE and ap.MarketDateTime>=@START_DATE and ap.MarketDateTime<@END_DATE and aep.MarketDateTime=bc.MarketDateTime and aep.MarketDateTime=pep.MarketDateTime and aep.MarketDateTime=dom.MarketDateTime " +
            " and aep.MarketDateTime=midat.MarketDateTime and aep.MarketDateTime=pn.MarketDateTime and aep.MarketDateTime=ap.MarketDateTime  and bc.MarketDateTime=pep.MarketDateTime and bc.MarketDateTime=dom.MarketDateTime and bc.MarketDateTime=midat.MarketDateTime and bc.MarketDateTime=pn.MarketDateTime and bc.MarketDateTime=ap.MarketDateTime " +
            " and pep.MarketDateTime=dom.MarketDateTime and pep.MarketDateTime=midat.MarketDateTime and pep.MarketDateTime=pn.MarketDateTime and pep.MarketDateTime=ap.MarketDateTime and dom.MarketDateTime=midat.MarketDateTime and dom.MarketDateTime=pn.MarketDateTime and dom.MarketDateTime=ap.MarketDateTime " +
            " and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime and pn.MarketDateTime=ap.MarketDateTime ";
            cmdSelectDomCongISOCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectDomCongISOCommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectComedCongISOCommand = VayuDBConnection.CreateCommand();
            cmdSelectComedCongISOCommand.CommandText = "select ap.MarketDateTime ,(0.25*midat.MW)+(0.25*dom.MW)+(0.5*ap.MW)+(west.MW-ap.MW-comed.MW)-comed.MW " +
            " from LoadForecasts midat ,LoadForecasts dom,LoadForecasts ap,LoadForecasts west, LoadForecasts comed " +
            " where  west.MarketDateTime>=@START_DATE and west.MarketDateTime<@END_DATE and comed.MarketDateTime>=@START_DATE and comed.MarketDateTime<@END_DATE " +
            " and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE and midat.MarketDateTime>=@START_DATE and midat.MarketDateTime<@END_DATE " +
            " and ap.MarketDateTime>=@START_DATE and ap.MarketDateTime<@END_DATE and west.LoadForecastTypeKey=7 and comed.LoadForecastTypeKey=5 and dom.LoadForecastTypeKey=8 and midat.LoadForecastTypeKey=18 and ap.LoadForecastTypeKey=2  and " +
            " midat.MarketDateTime=dom.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime and midat.MarketDateTime=west.MarketDateTime and midat.MarketDateTime=comed.MarketDateTime and " +
            " dom.MarketDateTime=ap.MarketDateTime and dom.MarketDateTime=west.MarketDateTime and dom.MarketDateTime=comed.MarketDateTime and " +
            " ap.MarketDateTime =west.MarketDateTime and ap.MarketDateTime=comed.MarketDateTime and west.MarketDateTime=comed.MarketDateTime ";
            cmdSelectComedCongISOCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectComedCongISOCommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectDomCongPRTCommand = VayuDBConnection.CreateCommand();
            cmdSelectDomCongPRTCommand.CommandText = "select aep.MarketDateTime ,(bc.LoadForecast+pep.LoadForecast-dom.LoadForecast)+((midat.LoadForecast-bc.LoadForecast-pep.LoadForecast-pn.LoadForecast)*0.5)-(0.5*aep.LoadForecast)+(0.35*(ap.LoadForecast))  " +
            " from PRT aep,PRT bc, PRT pep, PRT dom, PRT midat ,PRT pn,PRT ap  where bc.MarketDateTime>=@START_DATE and bc.MarketDateTime<@END_DATE and pep.MarketDateTime>=@START_DATE and pep.MarketDateTime<@END_DATE " +
            " and aep.MarketDateTime>=@START_DATE and aep.MarketDateTime<@END_DATE and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE and pn.MarketDateTime>=@START_DATE and pn.MarketDateTime<@END_DATE " +
            " and midat.MarketDateTime>=@START_DATE and midat.MarketDateTime<@END_DATE and ap.MarketDateTime>=@START_DATE and ap.MarketDateTime<@END_DATE and bc.Name='BGE' and aep.Name='AEP' and dom.Name='METED' and midat.Name='MidAtlanticRegionLoadForecast' and pn.Name='PENELEC' and ap.Name='APS' and pep.Name ='PEPCO' " +
            " and aep.MarketDateTime=bc.MarketDateTime and aep.MarketDateTime=pep.MarketDateTime and aep.MarketDateTime=dom.MarketDateTime and aep.MarketDateTime=midat.MarketDateTime and aep.MarketDateTime=pn.MarketDateTime and aep.MarketDateTime=ap.MarketDateTime " +
            " and bc.MarketDateTime=pep.MarketDateTime and bc.MarketDateTime=dom.MarketDateTime and bc.MarketDateTime=midat.MarketDateTime and bc.MarketDateTime=pn.MarketDateTime and bc.MarketDateTime=ap.MarketDateTime " +
            " and pep.MarketDateTime=dom.MarketDateTime and pep.MarketDateTime=midat.MarketDateTime and pep.MarketDateTime=pn.MarketDateTime and pep.MarketDateTime=ap.MarketDateTime " +
            " and dom.MarketDateTime=midat.MarketDateTime and dom.MarketDateTime=pn.MarketDateTime and dom.MarketDateTime=ap.MarketDateTime and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime and pn.MarketDateTime=ap.MarketDateTime ";
            cmdSelectDomCongPRTCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectDomCongPRTCommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectComedCongPRTCommand = VayuDBConnection.CreateCommand();
            cmdSelectComedCongPRTCommand.CommandText = "select ap.MarketDateTime ,(0.25*midat.LoadForecast)+(0.25*dom.LoadForecast)+(0.5*ap.LoadForecast)+(west.LoadForecast-ap.LoadForecast-comed.LoadForecast)-comed.LoadForecast " +
            " from PRT midat ,PRT dom,PRT ap,PRT west, PRT comed where   west.MarketDateTime>=@START_DATE and west.MarketDateTime<@END_DATE and comed.MarketDateTime>=@START_DATE and comed.MarketDateTime<@END_DATE " +
            " and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE and midat.MarketDateTime>=@START_DATE and midat.MarketDateTime<@END_DATE and ap.MarketDateTime>=@START_DATE and ap.MarketDateTime<@END_DATE and " +
            " west.name='WesternRegionLoadForecast' and comed.name='COMED' and dom.name='METED' and midat.name='MidAtlanticRegionLoadForecast' and ap.name='APS'  and midat.MarketDateTime=dom.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime and midat.MarketDateTime=west.MarketDateTime and midat.MarketDateTime=comed.MarketDateTime " +
            " and  dom.MarketDateTime=ap.MarketDateTime and dom.MarketDateTime=west.MarketDateTime and dom.MarketDateTime=comed.MarketDateTime and ap.MarketDateTime =west.MarketDateTime and ap.MarketDateTime=comed.MarketDateTime and west.MarketDateTime=comed.MarketDateTime ";
            cmdSelectComedCongPRTCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectComedCongPRTCommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectDomCongTeslaCommand = VayuDBConnection.CreateCommand();
            cmdSelectDomCongTeslaCommand.CommandText = "select aep.tesla_time ,(bc.forecast+pep.forecast-dom.forecast)+((midat.forecast-bc.forecast-pep.forecast-pn.forecast)*0.5)-(0.5*aep.forecast)+(0.35*(ap.forecast)) " +
            " from Tesla aep,Tesla bc, Tesla pep, Tesla dom, Tesla midat ,Tesla pn,Tesla ap " +
            " where bc.tesla_date>=@START_DATE and bc.tesla_date<@END_DATE and pep.tesla_date>=@START_DATE and pep.tesla_date<@END_DATE " +
            " and aep.tesla_date>=@START_DATE and aep.tesla_date<@END_DATE and dom.tesla_date>=@START_DATE and dom.tesla_date<@END_DATE " +
            " and pn.tesla_date>=@START_DATE and pn.tesla_date<@END_DATE and midat.tesla_date>=@START_DATE and midat.tesla_date<@END_DATE " +
            " and ap.tesla_date>=@START_DATE and ap.tesla_date<@END_DATE and bc.area='PJM BC' and aep.area='PJM AE' and dom.area='PJM DOMIN' and midat.area='PJM Mid-Atlantic' and pn.area='PJM PN' and ap.area='PJM APS' and pep.area ='PJM PEP' " +
            " and aep.tesla_date=bc.tesla_date and aep.tesla_date=pep.tesla_date and aep.tesla_date=dom.tesla_date and aep.tesla_date=midat.tesla_date and aep.tesla_date=pn.tesla_date and aep.tesla_date=ap.tesla_date " +
            " and bc.tesla_date=pep.tesla_date and bc.tesla_date=dom.tesla_date and bc.tesla_date=midat.tesla_date and bc.tesla_date=pn.tesla_date and bc.tesla_date=ap.tesla_date and pep.tesla_date=dom.tesla_date and pep.tesla_date=midat.tesla_date and pep.tesla_date=pn.tesla_date " +
            " and pep.tesla_date=ap.tesla_date and dom.tesla_date=midat.tesla_date and dom.tesla_date=pn.tesla_date and dom.tesla_date=ap.tesla_date and midat.tesla_date=pn.tesla_date and midat.tesla_date=pn.tesla_date and midat.tesla_date=ap.tesla_date and pn.tesla_date=ap.tesla_date and " +
            " aep.tesla_time=bc.tesla_time and aep.tesla_time=pep.tesla_time and aep.tesla_time=dom.tesla_time and aep.tesla_time=midat.tesla_time and aep.tesla_time=pn.tesla_time and aep.tesla_time=ap.tesla_time " +
            " and bc.tesla_time=pep.tesla_time and bc.tesla_time=dom.tesla_time and bc.tesla_time=midat.tesla_time and bc.tesla_time=pn.tesla_time and bc.tesla_time=ap.tesla_time " +
            " and pep.tesla_time=dom.tesla_time and pep.tesla_time=midat.tesla_time and pep.tesla_time=pn.tesla_time and pep.tesla_time=ap.tesla_time " +
            " and dom.tesla_time=midat.tesla_time and dom.tesla_time=pn.tesla_time and dom.tesla_time=ap.tesla_time and midat.tesla_time=pn.tesla_time and midat.tesla_time=pn.tesla_time and midat.tesla_time=ap.tesla_time and pn.tesla_time=ap.tesla_time ";
            cmdSelectDomCongTeslaCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectDomCongTeslaCommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectComedCongTeslaCommand = VayuDBConnection.CreateCommand();
            cmdSelectComedCongTeslaCommand.CommandText = "select ap.tesla_time ,(0.25*midat.forecast)+(0.25*dom.forecast)+(0.5*ap.forecast)+(west.forecast-ap.forecast-comed.forecast)-comed.forecast " +
            " from Tesla midat ,Tesla dom,Tesla ap,Tesla west, Tesla comed where  west.tesla_date>=@START_DATE and west.tesla_date<@END_DATE and comed.tesla_date>=@START_DATE and comed.tesla_date<@END_DATE " +
            " and dom.tesla_date>=@START_DATE and dom.tesla_date<@END_DATE and midat.tesla_date>=@START_DATE and midat.tesla_date<@END_DATE and ap.tesla_date>=@START_DATE and ap.tesla_date<@END_DATE and " +
            " west.area='PJM Western' and comed.area='PJM COMED' and dom.area='PJM DOMIN' and midat.area='PJM Mid-Atlantic' and ap.area='PJM APS'  and  midat.tesla_date=dom.tesla_date and midat.tesla_date=ap.tesla_date and midat.tesla_date=west.tesla_date and midat.tesla_date=comed.tesla_date " +
            " and  dom.tesla_date=ap.tesla_date and dom.tesla_date=west.tesla_date and dom.tesla_date=comed.tesla_date and ap.tesla_date =west.tesla_date and ap.tesla_date=comed.tesla_date and west.tesla_date=comed.tesla_date and " +
            " midat.tesla_time=dom.tesla_time and midat.tesla_time=ap.tesla_time and midat.tesla_time=west.tesla_time and midat.tesla_time=comed.tesla_time and  dom.tesla_time=ap.tesla_time and dom.tesla_time=west.tesla_time and dom.tesla_time=comed.tesla_time and " +
            " ap.tesla_time =west.tesla_time and ap.tesla_time=comed.tesla_time and west.tesla_time=comed.tesla_time ";
            cmdSelectComedCongTeslaCommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectComedCongTeslaCommand.Parameters.AddWithValue("@END_DATE", "");
 

            cmdSelectDomCongDACommand = VayuDBConnection.CreateCommand();
            cmdSelectDomCongDACommand.CommandText = "select aep.MarketDateTime ,(cast(bc.Value as int)+cast(pep.Value as int)-cast(dom.Value as int))+((cast(midat.Value as int)   -cast(bc.Value as int)-cast(pep.Value as int)-cast(pn.Value as int))*0.5)-(0.5*cast(aep.Value as int))+(0.35*(cast(ap.Value as int)))  " +
            " from DayAheadLoad aep,DayAheadLoad bc, DayAheadLoad pep, DayAheadLoad dom, DayAheadLoad midat ,DayAheadLoad pn,DayAheadLoad ap " +
            " where bc.MarketDateTime>=@START_DATE and bc.MarketDateTime<@END_DATE and pep.MarketDateTime>=@START_DATE and pep.MarketDateTime<@END_DATE and aep.MarketDateTime>=@START_DATE and aep.MarketDateTime<@END_DATE " +
            " and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE and pn.MarketDateTime>=@START_DATE and pn.MarketDateTime<@END_DATE and midat.MarketDateTime>=@START_DATE and midat.MarketDateTime<@END_DATE " +
            " and ap.MarketDateTime>=@START_DATE and ap.MarketDateTime<@END_DATE and bc.Name='BGE' and aep.Name='AEP' and dom.Name='DOM' and midat.Name='Mid-Atlantic Region' and pn.Name='PENELEC' and ap.Name='AP' and pep.Name ='PEPCO' " +
            " and aep.MarketDateTime=bc.MarketDateTime and aep.MarketDateTime=pep.MarketDateTime and aep.MarketDateTime=dom.MarketDateTime and aep.MarketDateTime=midat.MarketDateTime and aep.MarketDateTime=pn.MarketDateTime and aep.MarketDateTime=ap.MarketDateTime " +
            " and bc.MarketDateTime=pep.MarketDateTime and bc.MarketDateTime=dom.MarketDateTime and bc.MarketDateTime=midat.MarketDateTime and bc.MarketDateTime=pn.MarketDateTime and bc.MarketDateTime=ap.MarketDateTime " +
            " and pep.MarketDateTime=dom.MarketDateTime and pep.MarketDateTime=midat.MarketDateTime and pep.MarketDateTime=pn.MarketDateTime and pep.MarketDateTime=ap.MarketDateTime and dom.MarketDateTime=midat.MarketDateTime and dom.MarketDateTime=pn.MarketDateTime " +
            " and dom.MarketDateTime=ap.MarketDateTime and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=pn.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime and pn.MarketDateTime=ap.MarketDateTime ";
            cmdSelectDomCongDACommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectDomCongDACommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectComedCongDACommand = VayuDBConnection.CreateCommand();
            cmdSelectComedCongDACommand.CommandText = "select ap.MarketDateTime ,(0.25* cast( midat.Value as int))+(0.25* cast(dom.Value as int))+(0.5*cast (ap.Value as int))+(cast ( west.Value as int)- cast(ap.Value as int)- cast(comed.Value as int))- cast(comed.Value as int) " +
            " from DayAheadLoad midat ,DayAheadLoad dom,DayAheadLoad ap,DayAheadLoad west, DayAheadLoad comed where  west.MarketDateTime>=@START_DATE and west.MarketDateTime<@END_DATE " +
            " and comed.MarketDateTime>=@START_DATE and comed.MarketDateTime<@END_DATE and dom.MarketDateTime>=@START_DATE and dom.MarketDateTime<@END_DATE and midat.MarketDateTime>=@START_DATE and midat.MarketDateTime<@END_DATE " +
            " and ap.MarketDateTime>=@START_DATE and ap.MarketDateTime<@END_DATE and  west.name='Western Region' and comed.name='CE' and dom.name='DOM' and midat.name='Mid-Atlantic Region' and ap.name='AP'  " +
            " and midat.MarketDateTime=dom.MarketDateTime and midat.MarketDateTime=ap.MarketDateTime and midat.MarketDateTime=west.MarketDateTime and midat.MarketDateTime=comed.MarketDateTime " +
            " and dom.MarketDateTime=ap.MarketDateTime and dom.MarketDateTime=west.MarketDateTime and dom.MarketDateTime=comed.MarketDateTime and ap.MarketDateTime =west.MarketDateTime and ap.MarketDateTime=comed.MarketDateTime and west.MarketDateTime=comed.MarketDateTime ";
            cmdSelectComedCongDACommand.Parameters.AddWithValue("@START_DATE", "");
            cmdSelectComedCongDACommand.Parameters.AddWithValue("@END_DATE", "");

            cmdSelectDbConfig = VayuDBConnection.CreateCommand();
            cmdSelectDbConfig.CommandText = "select * from DBDataConfig";
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        private void StartTimer()
        {
            try
            {
                sTimer = new System.Timers.Timer(30000) { Enabled = true };
                sTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Called when [timed event].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The ElapsedEventArgs instance containing the event data.</param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                sTimer.Enabled = false;
                if (dictSubscriberHash.Count > 0)
                {
                    CallbackClient();
                }
                sTimer.Enabled = true;
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Callbacks the client.
        /// </summary>
        private void CallbackClient()
        {
            lock (lockObj)
            {
                DateTime currentTime = DateTime.Now;
                foreach (ILoadGraphCallback item in dictSubscriberHash.Keys.ToList<ILoadGraphCallback>())
                {
                    object[] parameters = dictSubscriberHash[item];

                    try
                    {
                        HashValues hashValues = new HashValues();

                        if (!dictCacheZoneUpdateHash.ContainsKey(parameters[0] + "@" + parameters[1] + "@" + parameters[2]))
                        {
                            hashValues = GetLoads(parameters[0].ToString(), DateTime.Parse(parameters[1].ToString()), DateTime.Parse(parameters[2].ToString()), dictLoadHash[parameters[0].ToString()]);
                            dictCacheZoneUpdateHash.Add(parameters[0] + "@" + parameters[1] + "@" + parameters[2], currentTime);
                            dictCacheGraphValuesHash.Add(parameters[0] + "@" + parameters[1] + "@" + parameters[2], hashValues);

                            item.SetGraph(parameters[0].ToString(), DateTime.Parse(parameters[1].ToString()), DateTime.Parse(parameters[2].ToString()), hashValues.HourZeroHash, hashValues.Hour1Hash, hashValues.Hour2Hash, hashValues.Hour3Hash, hashValues.Hour4Hash, hashValues.HourFiveHash,
                                hashValues.Hour6Hash, hashValues.Hour7Hash, hashValues.Hour8Hash, hashValues.Hour9Hash, hashValues.Hour10Hash, hashValues.Hour11Hash, hashValues.Hour12Hash, hashValues.Hour13Hash, hashValues.Hour14Hash, hashValues.Hour15Hash);
                        }
                        else
                        {
                            TimeSpan diff = currentTime - dictCacheZoneUpdateHash[parameters[0] + "@" + parameters[1] + "@" + parameters[2]];
                            DateTime lastUpdated = dictCacheZoneUpdateHash[parameters[0] + "@" + parameters[1] + "@" + parameters[2]];

                            if (diff.Minutes > int.Parse(System.Configuration.ConfigurationManager.AppSettings["TimeDifference"].ToString()))
                            {
                                hashValues = GetLoads(parameters[0].ToString(), DateTime.Parse(parameters[1].ToString()), DateTime.Parse(parameters[2].ToString()), dictLoadHash[parameters[0].ToString()]);
                                if (!CompareGraphValues(dictCacheGraphValuesHash[parameters[0] + "@" + parameters[1] + "@" + parameters[2]], hashValues))
                                {
                                    dictCacheZoneUpdateHash.Remove(parameters[0] + "@" + parameters[1] + "@" + parameters[2]);
                                    dictCacheZoneUpdateHash.Add(parameters[0] + "@" + parameters[1] + "@" + parameters[2], currentTime);
                                    dictCacheGraphValuesHash.Remove(parameters[0] + "@" + parameters[1] + "@" + parameters[2]);
                                    dictCacheGraphValuesHash.Add(parameters[0] + "@" + parameters[1] + "@" + parameters[2], hashValues);
                                    lastUpdated = currentTime;
                                }
                            }

                            if (lastUpdated > dictCacheUserUpdateHash[item])
                            {
                                DateTime userTime = dictCacheUserUpdateHash[item];
                                dictCacheUserUpdateHash.Remove(item);
                                dictCacheUserUpdateHash.Add(item, currentTime);
                                if (!userTime.ToString("HH:mm:ss").Equals("00:00:00"))
                                {
                                    hashValues = dictCacheGraphValuesHash[parameters[0] + "@" + parameters[1] + "@" + parameters[2]];
                                    item.SetGraph(parameters[0].ToString(), DateTime.Parse(parameters[1].ToString()), DateTime.Parse(parameters[2].ToString()), hashValues.HourZeroHash, hashValues.Hour1Hash,
                                    hashValues.Hour2Hash, hashValues.Hour3Hash, hashValues.Hour4Hash, hashValues.HourFiveHash, hashValues.Hour6Hash, hashValues.Hour7Hash, hashValues.Hour8Hash,
                                    hashValues.Hour9Hash, hashValues.Hour10Hash, hashValues.Hour11Hash, hashValues.Hour12Hash, hashValues.Hour13Hash, hashValues.Hour14Hash, hashValues.Hour15Hash);
                                }
                            }
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        dictCacheUserUpdateHash.Remove(item);
                        dictSubscriberHash.Remove(item);
                        Console.WriteLine("Total Users: {0}", dictSubscriberHash.Count);
                    }
                    catch (Exception)
                    {
                        dictCacheUserUpdateHash.Remove(item);
                        dictSubscriberHash.Remove(item);
                        Console.WriteLine("Total Users: {0}", dictSubscriberHash.Count);
                    }
                }
            }
        }

        /// <summary>
        /// Compares the graph values.
        /// </summary>
        /// <param name="oldhourHash">The oldhour hash.</param>
        /// <param name="currenthourHash">The currenthour hash.</param>
        /// <returns>true or false</returns>
        private bool CompareGraphValues(HashValues oldhourHash, HashValues currenthourHash)
        {
            //if (oldhourHash.HourZeroHash.Count() != currenthourHash.HourZeroHash.Count())
            //{
            //    return false;
            //}
            if (!CompareMinuteValues(oldhourHash.HourZeroHash, currenthourHash.HourZeroHash))
            {
                return false;
            }
            //if (oldhourHash.HourFiveHash.Count() != currenthourHash.HourFiveHash.Count())
            //{
            //    return false;
            //}
            if (!CompareMinuteValues(oldhourHash.HourFiveHash, currenthourHash.HourFiveHash))
            {
                return false;
            }
            //if (oldhourHash.Hour1Hash.Count != currenthourHash.Hour1Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour1Hash, currenthourHash.Hour1Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour2Hash.Count != currenthourHash.Hour2Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour2Hash, currenthourHash.Hour2Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour3Hash.Count != currenthourHash.Hour3Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour3Hash, currenthourHash.Hour3Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour4Hash.Count != currenthourHash.Hour4Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour4Hash, currenthourHash.Hour4Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour6Hash.Count != currenthourHash.Hour6Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour6Hash, currenthourHash.Hour6Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour7Hash.Count != currenthourHash.Hour7Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour7Hash, currenthourHash.Hour7Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour8Hash.Count != currenthourHash.Hour8Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour8Hash, currenthourHash.Hour8Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour9Hash.Count != currenthourHash.Hour9Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour9Hash, currenthourHash.Hour9Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour10Hash.Count != currenthourHash.Hour10Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour10Hash, currenthourHash.Hour10Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour11Hash.Count != currenthourHash.Hour11Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour11Hash, currenthourHash.Hour11Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour12Hash.Count != currenthourHash.Hour12Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour12Hash, currenthourHash.Hour12Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour13Hash.Count != currenthourHash.Hour13Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour13Hash, currenthourHash.Hour13Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour14Hash.Count != currenthourHash.Hour14Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour14Hash, currenthourHash.Hour14Hash))
            {
                return false;
            }

            //if (oldhourHash.Hour15Hash.Count != currenthourHash.Hour15Hash.Count)
            //{
            //    return false;
            //}
            if (!CompareHourValues(oldhourHash.Hour15Hash, currenthourHash.Hour15Hash))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Compares the minute values.
        /// </summary>
        /// <param name="hourHash">The hour hash.</param>
        /// <param name="compareHourHash">The compare hour hash.</param>
        /// <returns>true or false</returns>
        private bool CompareMinuteValues(Dictionary<int, Dictionary<int, double>> hourHash, Dictionary<int, Dictionary<int, double>> compareHourHash)
        {
            if (hourHash.Count == compareHourHash.Count)
            {
                foreach (var pair in hourHash)
                {
                    Dictionary<int, double> value;
                    if (compareHourHash.TryGetValue(pair.Key, out value))
                    {
                        if (!CompareHourValues(pair.Value, value))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Compares the hour values.
        /// </summary>
        /// <param name="hourHash">The hour hash.</param>
        /// <param name="compareHourHash">The compare hour hash.</param>
        /// <returns>true or false</returns>
        private bool CompareHourValues(Dictionary<int, double> hourHash, Dictionary<int, double> compareHourHash)
        {
            if (hourHash.Count == compareHourHash.Count)
            {
                foreach (var pair in hourHash)
                {
                    double value;
                    if (compareHourHash.TryGetValue(pair.Key, out value))
                    {
                        if (value != pair.Value)
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        #endregion
        #region Public Methods

        /// <summary>
        /// Gets the load values.
        /// </summary>
        /// <param name="Zone">The zone.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <returns>Hash Values List</returns>
        [PrincipalPermission(SecurityAction.Demand, Authenticated = false)]
        public HashValues GetLoadValues(string Zone, DateTime startDate, DateTime EndDate)
        {
            DateTime currentTime = DateTime.Now;
            HashValues hashValuesList = new HashValues();
            

            if (!dictCacheZoneUpdateHash.ContainsKey(Zone + "@" + startDate.ToString() + "@" + EndDate.ToString()))
            {
                hashValuesList = GetLoads(Zone, startDate, EndDate, dictLoadHash[Zone]);
                dictCacheZoneUpdateHash.Add(Zone + "@" + startDate.ToString() + "@" + EndDate.ToString(), currentTime);
                dictCacheGraphValuesHash.Add(Zone + "@" + startDate.ToString() + "@" + EndDate.ToString(), hashValuesList);
            }
            else
            {
                TimeSpan diff = currentTime - dictCacheZoneUpdateHash[Zone + "@" + startDate.ToString() + "@" + EndDate.ToString()];
                if (diff.Minutes > int.Parse(System.Configuration.ConfigurationManager.AppSettings["TimeDifference"].ToString()))
                {
                    hashValuesList = GetLoads(Zone, startDate, EndDate, dictLoadHash[Zone]);
                    dictCacheZoneUpdateHash.Remove(Zone + "@" + startDate.ToString() + "@" + EndDate.ToString());
                    dictCacheZoneUpdateHash.Add(Zone + "@" + startDate.ToString() + "@" + EndDate.ToString(), currentTime);
                    dictCacheGraphValuesHash.Remove(Zone + "@" + startDate.ToString() + "@" + EndDate.ToString());
                    dictCacheGraphValuesHash.Add(Zone + "@" + startDate.ToString() + "@" + EndDate.ToString(), hashValuesList);
                }
                else
                {
                    hashValuesList = dictCacheGraphValuesHash[Zone + "@" + startDate.ToString() + "@" + EndDate.ToString()];
                }
            }
            return hashValuesList;
        }

        /// <summary>
        /// Gets the bulk data.
        /// </summary>
        /// <param name="Zone">The zone.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <returns>Hash Values List</returns>
        public Dictionary<DateTime, HashValues> GetBulkData(string Zone, DateTime startDate, DateTime EndDate)
        {
            Dictionary<DateTime, HashValues> hashValuesList = new Dictionary<DateTime, HashValues>();
            DateTime dt = startDate;
            if (Zone == "" | string.IsNullOrEmpty(Zone))
            {
                return null;
            }
            Load zoneload = dictLoadHash[Zone];
            while (dt <= EndDate)
            {
                hashValuesList.Add(dt, GetLoads(Zone, dt, EndDate, zoneload));
                dt = dt.AddDays(1);
            }

            return hashValuesList;
        }
        #endregion

        #region Private Method
        /// <summary>
        /// Gets the loads.
        /// </summary>
        /// <param name="Zone">The zone.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <param name="load">The load.</param>
        /// <returns>Hash Value</returns>
        private HashValues GetLoads(string Zone, DateTime startDate, DateTime EndDate, Load load)
        {
            HashValues hashValues = null;
            lock (lockObj)
            {
                sIsDST = DateTime.Now.IsDaylightSavingTime();
                if (sIsDST) mDstDifference = 1;
                try
                {
                    if (VayuDBConnection.State == ConnectionState.Open)
                    {
                        VayuDBConnection.Close();
                    }
                    VayuDBConnection.Open();
                    
                    
                    Dictionary<int, Dictionary<int, double>> hourHash = new Dictionary<int, Dictionary<int, double>>();
                    Dictionary<int, double> hour1Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour2Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour3Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour4Hash = new Dictionary<int, double>();
                    Dictionary<int, Dictionary<int, double>> hour5Hash = new Dictionary<int, Dictionary<int, double>>(); ;
                    Dictionary<int, double> hour6Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour7Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour8Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour9Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour10Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour11Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour12Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour13Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour14Hash = new Dictionary<int, double>();
                    Dictionary<int, double> hour15Hash = new Dictionary<int, double>();
                    SqlDataReader reader = null;
                    string[] arr = Zone.Split(' ');
                    string Market = arr[0];
                    if (Market == "ERCOT")
                    {
                        cmdSelectErcotRTCommand.Parameters["@START_DATE"].Value = DateTime.Today;
                        cmdSelectErcotRTCommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                        cmdSelectErcotRTCommand.Parameters["@loadskey"].Value = load.Current;
                        if (cmdSelectErcotRTCommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectErcotRTCommand.Connection.Open();
                        reader = cmdSelectErcotRTCommand.ExecuteReader();
                    }
                   
                    while (reader.Read())
                    {
                        DateTime hour = reader.GetDateTime(0);
                        double mw = (double)reader.GetDecimal(1);
                        Dictionary<int, double> minHash = new Dictionary<int, double>();
                        if (hourHash.ContainsKey(hour.Hour))
                        {
                            minHash = hourHash[hour.Hour];
                            hourHash.Remove(hour.Hour);
                        }
                        if (!minHash.ContainsKey(hour.Minute))
                            minHash.Add(hour.Minute, mw);
                        hourHash.Add(hour.Hour, minHash);
                    }
                    reader.Close();
                    for (int i = 0; i < 15; i++)
                    {
                        if (i == 0 && true)
                        {
                            if (Market == "ERCOT")
                            {
                                cmdSelectErcotForecastCommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                cmdSelectErcotForecastCommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                cmdSelectErcotForecastCommand.Parameters["@LoadForecastTypeKey"].Value = load.Forecast;
                                if (cmdSelectErcotForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                    cmdSelectErcotForecastCommand.Connection.Open();
                                reader = cmdSelectErcotForecastCommand.ExecuteReader();
                            }
                             
                        }
                       
                     
                        else if (i == 3)
                        {
                            if (Market == "ERCOT")
                            {
                                cmdSelectErcotDACommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                cmdSelectErcotDACommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                cmdSelectErcotDACommand.Parameters["@name"].Value = (load.DayAhead == "ERCOT Total") ? "Total" : load.DayAhead;
                                reader = cmdSelectErcotDACommand.ExecuteReader();
                            }
                            else
                            {
                                if (Zone.ToLower().Equals("comed congestion"))
                                {
                                    cmdSelectComedCongDACommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                    cmdSelectComedCongDACommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                    if (cmdSelectComedCongDACommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectComedCongDACommand.Connection.Open();
                                    reader = cmdSelectComedCongDACommand.ExecuteReader();
                                }
                                else if (Zone.ToLower().Equals("dom congestion"))
                                {
                                    cmdSelectDomCongDACommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                    cmdSelectDomCongDACommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                    if (cmdSelectDomCongDACommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectDomCongDACommand.Connection.Open();
                                    reader = cmdSelectDomCongDACommand.ExecuteReader();
                                }

                                else
                                {
                                    if (load != null && load.DayAhead != null)
                                    {
                                        cmdSelectDAForecastCommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                        cmdSelectDAForecastCommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                        if (load.DayAhead == "Total Load" && load.Tesla == "MISO Total")
                                        {
                                            cmdSelectDAForecastCommand.Parameters["@name"].Value = "Cleared Load";
                                        }
                                        else
                                        {
                                            if (load.DayAhead.ToLower().Equals("aps")) load.DayAhead = "AP";
                                            cmdSelectDAForecastCommand.Parameters["@name"].Value = load.DayAhead;
                                        }
                                        if (cmdSelectDAForecastCommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectDAForecastCommand.Connection.Open();
                                        reader = null; reader = cmdSelectDAForecastCommand.ExecuteReader();
                                    }
                                }
                            }
                        }
                        else if (i == 14)
                        {
                            if (Market == "ERCOT") //DA
                            {
                                cmdSelectErcotDACommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                cmdSelectErcotDACommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                cmdSelectErcotDACommand.Parameters["@name"].Value = (load.DayAhead == "ERCOT Total") ? "Total" : load.DayAhead;
                                reader = cmdSelectErcotDACommand.ExecuteReader();
                            }
                            else
                            {
                                if (Zone.ToLower().Equals("comed congestion"))
                                {
                                    cmdSelectComedCongDACommand.Parameters["@START_DATE"].Value = EndDate;
                                    cmdSelectComedCongDACommand.Parameters["@END_DATE"].Value = EndDate.AddDays(1);
                                    if (cmdSelectComedCongDACommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectComedCongDACommand.Connection.Open();
                                    reader = cmdSelectComedCongDACommand.ExecuteReader();
                                }
                                else if (Zone.ToLower().Equals("dom congestion"))
                                {
                                    cmdSelectDomCongDACommand.Parameters["@START_DATE"].Value = EndDate;
                                    cmdSelectDomCongDACommand.Parameters["@END_DATE"].Value = EndDate.AddDays(1);
                                    if (cmdSelectDomCongDACommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectDomCongDACommand.Connection.Open();
                                    reader = cmdSelectDomCongDACommand.ExecuteReader();
                                }
                                else
                                {
                                    if (load != null && load.DayAhead != null)
                                    {
                                        cmdSelectDAForecastCommand.Parameters["@START_DATE"].Value = EndDate;
                                        cmdSelectDAForecastCommand.Parameters["@END_DATE"].Value = EndDate.AddDays(1);
                                        if (load.DayAhead == "Total Load" && load.Tesla == "MISO Total")
                                        {
                                            cmdSelectDAForecastCommand.Parameters["@name"].Value = "Cleared Load";
                                        }
                                        else
                                        {
                                            cmdSelectDAForecastCommand.Parameters["@name"].Value = load.DayAhead;
                                        }
                                        if (cmdSelectDAForecastCommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectDAForecastCommand.Connection.Open();
                                        reader = null;
                                        reader = cmdSelectDAForecastCommand.ExecuteReader();
                                    }
                                }
                            }
                        }
                        else if (i == 4)
                        {
                            if (Market == "ERCOT")//RT
                            {
                                cmdSelectErcotRTCommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                cmdSelectErcotRTCommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                cmdSelectErcotRTCommand.Parameters["@loadskey"].Value = load.Current;
                                if (cmdSelectErcotRTCommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectErcotRTCommand.Connection.Open();
                                reader = cmdSelectErcotRTCommand.ExecuteReader();
                            }
                            else
                            {
                                DateTime startTime = DateTime.Parse(startDate.ToShortDateString() + " 00:00");
                                if (Zone == "PJM Congestion A")
                                {
                                    cmdSelectCongRTCommand.Connection = GetConnection(startTime);
                                    cmdSelectCongRTCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectCongRTCommand.Parameters["@END_DATE"].Value = DateTime.Parse(startTime.AddDays(1).ToShortDateString() + " 10:00");
                                    if (cmdSelectCongRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectCongRTCommand.Connection.Open();
                                    reader = null;
                                    reader = cmdSelectCongRTCommand.ExecuteReader();
                                }
                                else if (Zone == "PJM Congestion B")
                                {
                                    cmdSelectCongRTCommand.Connection = GetConnection(startTime);
                                    if (cmdSelectCongRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectCongRTCommand.Connection.Open();
                                    cmdSelectCongRTCommandB.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectCongRTCommandB.Parameters["@END_DATE"].Value = DateTime.Parse(startTime.AddDays(1).ToShortDateString() + " 10:00");
                                    if (cmdSelectCongRTCommandB.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectCongRTCommandB.Connection.Open();
                                    reader = null;
                                    reader = cmdSelectCongRTCommandB.ExecuteReader();
                                }
                                else if (Zone == "PJM Brighton")
                                {
                                    cmdSelectCongRTCommand.Connection = GetConnection(startTime);
                                    cmdSelectBrightRTCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectBrightRTCommand.Parameters["@END_DATE"].Value = DateTime.Parse(startTime.AddDays(1).ToShortDateString() + " 10:00");
                                    if (cmdSelectBrightRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectBrightRTCommand.Connection.Open();
                                    reader = null;
                                    reader = cmdSelectBrightRTCommand.ExecuteReader();
                                }
                                else if (Zone == "PJM East")
                                {
                                    cmdSelectEastRTCommand.Connection = GetConnection(startTime);
                                    cmdSelectEastRTCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectEastRTCommand.Parameters["@END_DATE"].Value = DateTime.Parse(startTime.AddDays(1).ToShortDateString() + " 10:00");
                                    if (cmdSelectEastRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectEastRTCommand.Connection.Open();
                                    reader = null;
                                    reader = cmdSelectEastRTCommand.ExecuteReader();
                                }
                                else if (Zone.ToLower().Equals("comed congestion"))
                                {
                                    cmdSelectComedCongForecastCommand.Connection = GetConnection(startTime);

                                    cmdSelectComedCongForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectComedCongForecastCommand.Parameters["@END_DATE"].Value = DateTime.Parse(startTime.AddDays(1).ToShortDateString() + " 10:00");
                                    if (cmdSelectComedCongForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectComedCongForecastCommand.Connection.Open();
                                    reader = cmdSelectComedCongForecastCommand.ExecuteReader();
                                }
                                else if (Zone.ToLower().Equals("dom congestion"))
                                {
                                    cmdSelectDomCongForecastCommand.Connection = GetConnection(startTime);
                                    if (cmdSelectDomCongForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectDomCongForecastCommand.Connection.Open();
                                    cmdSelectDomCongForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectDomCongForecastCommand.Parameters["@END_DATE"].Value = DateTime.Parse(startTime.AddDays(1).ToShortDateString() + " 10:00");
                                    if (cmdSelectDomCongForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectDomCongForecastCommand.Connection.Open();
                                    reader = cmdSelectDomCongForecastCommand.ExecuteReader();
                                }
                                else
                                {
                                    cmdSelectRTForecastCommand.Connection = GetConnection(startTime);
                                    if (cmdSelectRTForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectRTForecastCommand.Connection.Open();
                                    cmdSelectRTForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectRTForecastCommand.Parameters["@END_DATE"].Value = DateTime.Parse(startTime.AddDays(1).ToShortDateString() + " 10:00");
                                    cmdSelectRTForecastCommand.Parameters["@loadskey"].Value = load.Current;
                                    if (cmdSelectRTForecastCommand.Connection.State.Equals(ConnectionState.Closed)) cmdSelectRTForecastCommand.Connection.Open();
                                    reader = null; reader = cmdSelectRTForecastCommand.ExecuteReader();
                                }
                            }
                        }
                        else if (i == 5)
                        {
                            if (Market == "ERCOT")//DA
                            {
                                cmdSelectErcotDACommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                cmdSelectErcotDACommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                cmdSelectErcotDACommand.Parameters["@name"].Value = (load.DayAhead == "ERCOT Total") ? "Total" : load.DayAhead;
                                reader = cmdSelectErcotDACommand.ExecuteReader();
                            }
                            else
                            {
                                DateTime startTime = DateTime.Parse(startDate.ToShortDateString() + " 00:00"); // should be verified
                                if (Zone.ToLower().Equals("comed congestion"))
                                {
                                    cmdSelectComedCongDACommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectComedCongDACommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectComedCongDACommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectComedCongDACommand.Connection.Open();
                                    reader = cmdSelectComedCongDACommand.ExecuteReader();
                                }
                                else if (Zone.ToLower().Equals("dom congestion"))
                                {
                                    cmdSelectDomCongDACommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectDomCongDACommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectDomCongDACommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectDomCongDACommand.Connection.Open();
                                    reader = cmdSelectDomCongDACommand.ExecuteReader();
                                }
                                else
                                {
                                    if (load != null && load.DayAhead != null)
                                    {
                                        cmdSelectDAForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                        cmdSelectDAForecastCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                        if (load.DayAhead == "Total Load" && load.Tesla == "MISO Total")
                                        {
                                            cmdSelectDAForecastCommand.Parameters["@name"].Value = "Cleared Load";
                                        }
                                        else
                                        {
                                            cmdSelectDAForecastCommand.Parameters["@name"].Value = load.DayAhead;
                                        }
                                        if (cmdSelectDAForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                            cmdSelectDAForecastCommand.Connection.Open();
                                        reader = null; reader = cmdSelectDAForecastCommand.ExecuteReader();
                                    }
                                }
                            }
                        }
                        else if (i == 6 && (load == null || load.PRT != null))
                        {
                            DateTime startTime = DateTime.Parse(EndDate.ToShortDateString() + " 00:00");
                            if (Zone == "PJM Congestion A")
                            {
                                //cmdSelectCongPRTCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectCongPRTCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectCongPRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectCongPRTCommand.Connection.Open();
                                //reader = null; reader = cmdSelectCongPRTCommand.ExecuteReader();

                            }
                            else if (Zone == "PJM Congestion B")
                            {
                                //cmdSelectCongPRTCommandB.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectCongPRTCommandB.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectCongPRTCommandB.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectCongPRTCommandB.Connection.Open();
                                //reader = null; reader = cmdSelectCongPRTCommandB.ExecuteReader();

                            }
                            else if (Zone == "PJM Brighton")
                            {
                                //cmdSelectBrightPRTCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectBrightPRTCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectBrightPRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectBrightPRTCommand.Connection.Open();
                                //reader = null; reader = cmdSelectBrightPRTCommand.ExecuteReader();
                            }
                            else if (Zone == "PJM East")
                            {
                                //cmdSelectEastPRTCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectEastPRTCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectEastPRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectEastPRTCommand.Connection.Open();
                                //reader = null; reader = cmdSelectEastPRTCommand.ExecuteReader();
                            }
                            else if (Zone.ToLower().Equals("comed congestion"))
                            {
                                //cmdSelectComedCongPRTCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectComedCongPRTCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectComedCongPRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectComedCongPRTCommand.Connection.Open();
                                //reader = cmdSelectComedCongPRTCommand.ExecuteReader();
                            }
                            else if (Zone.ToLower().Equals("dom congestion"))
                            {
                                //cmdSelectDomCongPRTCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectDomCongPRTCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectDomCongPRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectDomCongPRTCommand.Connection.Open();
                                //reader = cmdSelectDomCongPRTCommand.ExecuteReader();
                            }
                            else
                            {
                                //cmdSelectPRTCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectPRTCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //cmdSelectPRTCommand.Parameters["@name"].Value = load.PRT;
                                //if (cmdSelectPRTCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectPRTCommand.Connection.Open();
                                //reader = null; reader = cmdSelectPRTCommand.ExecuteReader();
                            }
                        }
                        else if (i == 7 && (load == null || load.Tesla != ""))
                        {
                            DateTime startTime = DateTime.Parse(EndDate.ToShortDateString() + " 00:00");
                            if (Zone == "PJM Congestion A")
                            {
                                //cmdSelectCongTeslaCommand.Parameters["@START_DATE"].Value = startTime;
                                //if (cmdSelectCongTeslaCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectCongTeslaCommand.Connection.Open();
                                //reader = null; reader = cmdSelectCongTeslaCommand.ExecuteReader();

                            }
                            else if (Zone == "PJM Congestion B")
                            {
                                //cmdSelectCongTeslaCommandB.Parameters["@START_DATE"].Value = startTime;
                                //if (cmdSelectCongTeslaCommandB.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectCongTeslaCommandB.Connection.Open();
                                //reader = null; reader = cmdSelectCongTeslaCommandB.ExecuteReader();

                            }

                            else if (Zone == "PJM Brighton")
                            {
                                cmdSelectBrightTeslaCommand.Parameters["@START_DATE"].Value = startTime;
                                if (cmdSelectBrightTeslaCommand.Connection.State.Equals(ConnectionState.Closed))
                                    cmdSelectBrightTeslaCommand.Connection.Open();
                                reader = null; reader = cmdSelectBrightTeslaCommand.ExecuteReader();
                            }

                            else if (Zone == "PJM East")
                            {
                                //cmdSelectEastTeslaCommand.Parameters["@START_DATE"].Value = startTime;
                                //if (cmdSelectEastTeslaCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectEastTeslaCommand.Connection.Open();
                                //reader = null; reader = cmdSelectEastTeslaCommand.ExecuteReader();
                            }
                            else if (Zone.ToLower().Equals("comed congestion"))
                            {
                                //cmdSelectComedCongTeslaCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectComedCongTeslaCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectComedCongTeslaCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectComedCongTeslaCommand.Connection.Open();
                                //reader = cmdSelectComedCongTeslaCommand.ExecuteReader();
                            }
                            else if (Zone.ToLower().Equals("dom congestion"))
                            {
                                //cmdSelectDomCongTeslaCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectDomCongTeslaCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                //if (cmdSelectDomCongTeslaCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectDomCongTeslaCommand.Connection.Open();
                                //reader = cmdSelectDomCongTeslaCommand.ExecuteReader();
                            }
                            else
                            {
                                //cmdSelectTeslaCommand.Parameters["@START_DATE"].Value = startTime;
                                //cmdSelectTeslaCommand.Parameters["@area"].Value = load.Tesla;
                                //if (cmdSelectTeslaCommand.Connection.State.Equals(ConnectionState.Closed))
                                //    cmdSelectTeslaCommand.Connection.Open();
                                //reader = null; reader = cmdSelectTeslaCommand.ExecuteReader();
                            }
                        }
                        else if (i == 8 && (load == null || load.Forecast != int.MaxValue))
                        {
                            if (Market == "ERCOT")//Forecast
                            {
                                cmdSelectErcotForecastCommand.Parameters["@START_DATE"].Value = DateTime.Today;
                                cmdSelectErcotForecastCommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                                cmdSelectErcotForecastCommand.Parameters["@LoadForecastTypeKey"].Value = load.Forecast;
                                if (cmdSelectErcotForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                    cmdSelectErcotForecastCommand.Connection.Open();
                                reader = cmdSelectErcotForecastCommand.ExecuteReader();
                            }
                            else
                            {
                                DateTime startTime = DateTime.Parse(EndDate.ToShortDateString() + " 00:00");
                                if (Zone == "PJM Congestion A")
                                {
                                    cmdSelectCongForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectCongForecastCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectCongForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectCongForecastCommand.Connection.Open();
                                    reader = null; reader = cmdSelectCongForecastCommand.ExecuteReader();
                                }
                                else if (Zone == "PJM Congestion B")
                                {
                                    cmdSelectCongForecastCommandB.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectCongForecastCommandB.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectCongForecastCommandB.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectCongForecastCommandB.Connection.Open();
                                    reader = null; reader = cmdSelectCongForecastCommandB.ExecuteReader();
                                }
                                else if (Zone == "PJM Brighton")
                                {
                                    cmdSelectBrightForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectBrightForecastCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectBrightForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectBrightForecastCommand.Connection.Open();
                                    reader = null; reader = cmdSelectBrightForecastCommand.ExecuteReader();
                                }
                                else if (Zone == "PJM East")
                                {

                                    cmdSelectEastForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectEastForecastCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectEastForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectEastForecastCommand.Connection.Open();
                                    reader = null; reader = cmdSelectEastForecastCommand.ExecuteReader();
                                }
                                else if (Zone.ToLower().Equals("comed congestion"))
                                {
                                    cmdSelectComedCongISOCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectComedCongISOCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectComedCongISOCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectComedCongISOCommand.Connection.Open();
                                    reader = cmdSelectComedCongISOCommand.ExecuteReader();
                                }
                                else if (Zone.ToLower().Equals("dom congestion"))
                                {
                                    cmdSelectDomCongISOCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectDomCongISOCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    if (cmdSelectDomCongISOCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectDomCongISOCommand.Connection.Open();
                                    reader = cmdSelectDomCongISOCommand.ExecuteReader();
                                }
                                else
                                {
                                    cmdSelectForecastCommand.Parameters["@START_DATE"].Value = startTime;
                                    cmdSelectForecastCommand.Parameters["@END_DATE"].Value = startTime.AddDays(1);
                                    cmdSelectForecastCommand.Parameters["@loadforecasttypekey"].Value = load.Forecast;
                                    if (cmdSelectEastForecastCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectEastForecastCommand.Connection.Open();
                                    reader = null; reader = cmdSelectForecastCommand.ExecuteReader();
                                }
                            }
                        }
                        if ((i == 9 || i == 10 || i == 12 || i == 13) && (load == null || load.WSI != null))
                        {

                            if (Zone == "PJM Congestion A")
                            {
                                string commandStr = "select a.time, a.load_fcst + b.load_fcst + c.load_fcst - d.load_fcst from pjm_pep_load_fcst a, pjm_bc_load_fcst b, " +
                                        "pjm_domin_load_fcst c, pjm_aps_load_fcst d where a.date = @date and b.date = @date and c.date = @date and d.date = @date and a.time = b.time and a.time = c.time " +
                                        "and a.time = d.time";
                                if (i == 12 || i == 13)
                                {
                                    commandStr = commandStr.Replace("_load", "_dtn_load");
                                }
                                cmdSelectWsiCommand.CommandText = commandStr;
                                string dateStr = (i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd");
                                cmdSelectWsiCommand.Parameters["@date"].Value = Int32.Parse(dateStr);
                                try
                                {
                                    if (cmdSelectWsiCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectWsiCommand.Connection.Open();
                                    reader = null; reader = cmdSelectWsiCommand.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    reader.Close();
                                }

                            }
                            else if (Zone == "PJM Congestion B")
                            {
                                string commandStr = "Select a.time, (IsNull(a.load_fcst,0) + Isnull(b.load_fcst,0) + Isnull(c.load_fcst,0)) + (1/4.0 * (Isnull(d.load_fcst,0) + Isnull(e.load_fcst,0) + Isnull(f.load_fcst,0) + Isnull(g.load_fcst,0) + Isnull(h.load_fcst,0) + Isnull(i.load_fcst,0))) - Isnull(j.load_fcst,0) - (Isnull(k.load_fcst,0) * (3/4.0)) - (1/4.0 * Isnull(l.load_fcst,0)) - (3/4.0 * Isnull(m.load_fcst,0)) - (1/2.0 * Isnull(n.load_fcst,0)) " +
                                                    "From pjm_pep_load_fcst a, pjm_bc_load_fcst b, pjm_southern_load_fcst c, pjm_jc_load_fcst d,  pjm_ae_load_fcst e, pjm_dpl_load_fcst f, pjm_pe_load_fcst g, pjm_ps_load_fcst h, pjm_me_load_fcst i, pjm_aps_load_fcst j, pjm_aepower_load_fcst k, pjm_comed_load_fcst l, pjm_deok_load_fcst m, pjm_dayton_load_fcst n " +
                                                    "Where a.date = @date and b.date = @date and c.date = @date  and d.date = @date and e.date = @date and f.date = @date and g.date = @date and h.date = @date and i.date = @date and j.date = @date and k.date = @date and l.date = @date and m.date = @date and n.date = @date " +
                                                    "and a.time = b.time and a.time = c.time and a.time = d.time and a.time = e.time and a.time = f.time and a.time = g.time and a.time = h.time and a.time = i.time and a.time = j.time and a.time = k.time and a.time = l.time and a.time = m.time and a.time = n.time Order By a.date, a.time";

                                if (i == 12 || i == 13)
                                {
                                    commandStr = commandStr.Replace("_load", "_dtn_load");
                                }
                                cmdSelectWsiCommand.CommandText = commandStr;
                                string dateStr = (i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd");
                                cmdSelectWsiCommand.Parameters["@date"].Value = Int32.Parse(dateStr);
                                try
                                {
                                    if (cmdSelectWsiCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectWsiCommand.Connection.Open();
                                    reader = null; reader = cmdSelectWsiCommand.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    reader.Close();
                                }

                            }
                            else if (Zone == "PJM Brighton")
                            {
                                string commandStr = "select a.time, (a.load_fcst/3) + ((b.load_fcst + c.load_fcst)/6) + d.load_fcst + (2*(e.load_fcst + f.load_fcst)) - g.load_fcst from pjm_western_load_fcst a, pjm_aepower_load_fcst b, " +
                                        "pjm_aps_load_fcst c, pjm_southern_load_fcst d, pjm_pep_load_fcst e, pjm_bc_load_fcst f, pjm_midatlantic_load_fcst g where a.date = @date and b.date = @date and c.date = @date and d.date = @date " +
                                        "and e.date = @date and f.date = @date and g.date = @date and a.time = b.time and a.time = c.time and a.time = d.time and a.time = e.time and a.time = f.time and a.time = g.time";
                                if (i == 12 || i == 13)
                                {
                                    commandStr = commandStr.Replace("_load", "_dtn_load");
                                }
                                cmdSelectWsiCommand.CommandText = commandStr;
                                string dateStr = (i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd");
                                cmdSelectWsiCommand.Parameters["@date"].Value = Int32.Parse(dateStr);
                                try
                                {
                                    if (cmdSelectWsiCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectWsiCommand.Connection.Open();
                                    reader = null; reader = cmdSelectWsiCommand.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    reader.Close();
                                }
                            }
                            else if (Zone == "PJM East")
                            {

                                string commandStr = "select a.time, a.load_fcst - b.load_fcst - c.load_fcst + (d.load_fcst/2) + (e.load_fcst/2) from pjm_midatlantic_load_fcst a, pjm_pn_load_fcst b, pjm_western_load_fcst c, pjm_comed_load_fcst d, pjm_southern_load_fcst e where a.date = @date and b.date = @date and c.date = @date and d.date = @date and e.date = @date and a.time = b.time and a.time = c.time and a.time = d.time and a.time = e.time";
                                if (i == 12 || i == 13)
                                {
                                    commandStr = commandStr.Replace("_load", "_dtn_load");
                                }
                                cmdSelectWsiCommand.CommandText = commandStr;
                                string dateStr = (i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd");
                                cmdSelectWsiCommand.Parameters["@date"].Value = Int32.Parse(dateStr);
                                try
                                {
                                    if (cmdSelectWsiCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectWsiCommand.Connection.Open();
                                    reader = null; reader = cmdSelectWsiCommand.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    reader.Close();
                                }
                            }
                            else if (Zone == "PJM Western Region")
                            {
                                string loadTable = i == 12 || i == 13 ? load.WSI.Replace("_load", "_dtn_load") : load.WSI;
                                cmdSelectWsiCommand.CommandText = "select time, load_fcst from " + loadTable + " where date = @date";
                                string dateStr = (i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd");
                                cmdSelectWsiCommand.Parameters["@date"].Value = Int32.Parse(dateStr);
                                try
                                {
                                    if (cmdSelectWsiCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectWsiCommand.Connection.Open();
                                    reader = null; reader = cmdSelectWsiCommand.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    reader.Close();
                                }
                            }
                            else if (Zone == "MISO Total Load")
                            {
                                string loadTable = i == 12 || i == 13 ? load.WSI.Replace("_load", "_dtn_load") : load.WSI;
                                if (loadTable == "miso_total_rt_load_fcst")
                                {
                                    loadTable = "miso_composite_load_fcst";
                                }
                                cmdSelectWsiCommand.CommandText = "select time, load_fcst from " + loadTable + " where date = @date";
                                string dateStr = (i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd");
                                cmdSelectWsiCommand.Parameters["@date"].Value = Int32.Parse(dateStr);
                                try
                                {
                                    if (cmdSelectWsiCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectWsiCommand.Connection.Open();
                                    reader = null; reader = cmdSelectWsiCommand.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    reader.Close();
                                }
                            }
                            else if (Zone.ToLower().Equals("comed congestion"))
                            {
                                SqlCommand CommandTemp = null;
                                if (i < 12) CommandTemp = i == 9 || i == 10 ? cmdSelectComedCongWSICommand : null;
                                else if (i >= 12) CommandTemp = i == 12 || i == 13 ? cmdSelectComedCongDTNCommand : null;
                                try
                                {
                                    if (CommandTemp != null)
                                    {
                                        CommandTemp.Parameters[0].Value = int.Parse((i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd"));
                                        reader = CommandTemp.ExecuteReader();
                                    }
                                }
                                catch (Exception)
                                {
                                    reader.Close();
                                }
                            }
                            else if (Zone.ToLower().Equals("dom congestion"))
                            {
                                SqlCommand CommandTemp = null;
                                if (i < 12) CommandTemp = i == 9 || i == 10 ? cmdSelectComedCongWSICommand : null;
                                else if (i >= 12) CommandTemp = i == 12 || i == 13 ? cmdSelectComedCongDTNCommand : null;
                                try
                                {
                                    if (CommandTemp != null)
                                    {
                                        CommandTemp.Parameters[0].Value = int.Parse((i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd"));
                                        reader = CommandTemp.ExecuteReader();
                                    }
                                }
                                catch (Exception)
                                {
                                    reader.Close();
                                }
                            }
                            //
                            else
                            {
                                string loadTable = i == 12 || i == 13 ? load.WSI.Replace("_load", "_dtn_load") : load.WSI;
                                if (load.WSI == "pjm_western_load_fcst")
                                {
                                    string commandStr = "select a.time, a.load_fcst + b.load_fcst + c.load_fcst + d.load_fcst + e.load_fcst  + f.load_fcst from pjm_comed_load_fcst a, pjm_dayton_load_fcst b, " +
                                        "pjm_duquesne_load_fcst c, pjm_aepower_load_fcst d, pjm_aps_load_fcst e, pjm_atsi_load_fcst f where a.date = @date and b.date = @date and c.date = @date and d.date = @date and e.date = @date " +
                                        "and f.date = @date and a.time = b.time and a.time = c.time and a.time = d.time and a.time = e.time and a.time = f.time";
                                    if (i == 12 || i == 13)
                                    {
                                        commandStr = commandStr.Replace("_load", "_dtn_load");
                                    }
                                    cmdSelectWsiCommand.CommandText = commandStr;
                                }
                                else
                                {
                                    cmdSelectWsiCommand.CommandText = "select time, load_fcst from " + loadTable + " where date = @date";
                                }
                                string dateStr = (i == 9 || i == 12) ? DateTime.Today.ToString("yyyyMMdd") : EndDate.ToString("yyyyMMdd");
                                cmdSelectWsiCommand.Parameters["@date"].Value = Int32.Parse(dateStr);
                                try
                                {
                                    if (cmdSelectWsiCommand.Connection.State.Equals(ConnectionState.Closed))
                                        cmdSelectWsiCommand.Connection.Open();
                                    reader = cmdSelectWsiCommand.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    reader.Close();
                                }
                            }
                        }
                        if (i == 11 && Zone.ToLower().Contains("pjm south"))
                        {
                            cmdSelectSouthCommand.Parameters["@START_DATE"].Value = DateTime.Today;
                            cmdSelectSouthCommand.Parameters["@END_DATE"].Value = DateTime.Today.AddDays(1);
                            if (cmdSelectSouthCommand.Connection.State.Equals(ConnectionState.Closed))
                                cmdSelectSouthCommand.Connection.Open();
                            reader = cmdSelectSouthCommand.ExecuteReader();
                        }
                        if (!reader.IsClosed)
                        {
                            while (reader.Read())
                            {
                                int hour = 0;
                                if (i == 2 || i == 7)
                                {
                                    hour = reader.GetInt32(0);
                                }
                                else if (i == 9 || i == 10 || i == 12 || i == 13)
                                {
                                    hour = (int)reader.GetInt16(0);
                                    hour = (hour / 100) + 1;
                                }
                                else
                                {
                                    DateTime dateTime = reader.GetDateTime(0);
                                    hour = dateTime.Hour;
                                }
                                double mw = 0;
                                Type obj = reader.GetFieldType(1);
                                if (obj.Name == "Decimal")
                                {
                                    mw = (double)reader.GetDecimal(1);
                                }
                                else if (obj.Name == "Int32")
                                {
                                    mw = (double)reader.GetInt32(1);
                                }
                                else if (obj.Name == "String")
                                {
                                    mw = double.Parse(reader.GetString(1));
                                }
                                else
                                {
                                    mw = reader.GetDouble(1);
                                }
                                if (i == 0)
                                {
                                    if (Market == "ERCOT")
                                    {
                                        hour1Hash.Add(hour, mw);
                                    }
                                    else
                                    {
                                        hour1Hash.Add(hour - 1, mw);
                                    }
                                }
                                else if (i == 1)
                                {
                                    if (Zone.StartsWith("PJM"))
                                    {
                                        hour2Hash.Add(hour - 1, mw);
                                    }
                                    else if (Zone.StartsWith("MISO"))
                                    {
                                        hour2Hash.Add(hour - 1, mw);
                                    }
                                    else
                                    {
                                        hour2Hash.Add(hour, mw);
                                    }
                                }
                                else if (i == 2)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour3Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.Tesla.StartsWith("MISO"))
                                    {
                                        hour3Hash.Add(hour - 1, mw);
                                    }
                                    else
                                    {
                                        hour3Hash.Add(hour - 1, mw);
                                    }
                                }
                                else if (i == 3)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour4Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.DayAhead == "DOM" || load.DayAhead == "AEP" || load.DayAhead == "AP" || load.DayAhead == "DAY" || load.DayAhead == "CE" || load.DayAhead == "Total Load")
                                    {
                                        hour4Hash.Add(hour - 1, mw);
                                    }
                                    else
                                    {
                                        hour4Hash.Add(hour, mw);
                                    }
                                }
                                else if (i == 14)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour15Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && (load.DayAhead == "DOM" || load.DayAhead == "AEP" || load.DayAhead == "AP" || load.DayAhead == "DAY" || load.DayAhead == "CE" || load.DayAhead == "Total Load"))
                                    {
                                        hour15Hash.Add(hour - 1, mw);
                                    }
                                    else
                                    {
                                        hour15Hash.Add(hour, mw);
                                    }
                                }
                                else if (i == 4)
                                {
                                    DateTime hour5 = reader.GetDateTime(0);
                                    double mw5 = (double)reader.GetDecimal(1);
                                    Dictionary<int, double> min5Hash = new Dictionary<int, double>();
                                    int hourInt = hour5.Hour;
                                    if (hour5Hash.ContainsKey(hourInt))
                                    {
                                        min5Hash = hour5Hash[hourInt];
                                        if (!min5Hash.ContainsKey(hour5.Minute))
                                        {
                                            hour5Hash.Remove(hourInt);
                                        }
                                    }
                                    if (!min5Hash.ContainsKey(hour5.Minute))
                                    {
                                        min5Hash.Add(hour5.Minute, mw5);
                                        hour5Hash.Add(hourInt, min5Hash);
                                    }
                                }
                                else if (i == 5)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour6Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.DayAhead == "DOM" || load.DayAhead == "AEP" || load.DayAhead == "AP" || load.DayAhead == "DAY" || load.DayAhead == "CE" || load.DayAhead == "Total Load")
                                    {
                                        hour6Hash.Add(hour - 1, mw);
                                    }
                                    else
                                    {
                                        hour6Hash.Add(hour, mw);
                                    }
                                }
                                else if (i == 6)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour7Hash.Add(hour - 1, mw);
                                    }
                                    else if (Zone.StartsWith("PJM") || Zone.StartsWith("MISO"))
                                    {
                                        hour7Hash.Add(hour - 1, mw);
                                    }
                                    else
                                    {
                                        hour7Hash.Add(hour, mw);
                                    }
                                }
                                else if (i == 7)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour8Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.Tesla.StartsWith("MISO"))
                                    {
                                        hour8Hash.Add(hour - 1, mw);
                                    }
                                    else
                                    {
                                        hour8Hash.Add(hour - 1, mw);
                                    }
                                }
                                else if (i == 8)
                                {
                                    hour9Hash.Add(hour - 1, mw);
                                }
                                else if (i == 9)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour10Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.WSI.StartsWith("miso"))
                                    {
                                        hour10Hash.Add(hour - 2, mw);
                                    }
                                    else
                                    {
                                        hour10Hash.Add(hour - mDstDifference, mw);
                                    }
                                }
                                else if (i == 10)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour11Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.WSI.StartsWith("miso"))
                                    {
                                        hour11Hash.Add(hour - 2, mw);
                                    }
                                    else
                                    {
                                        hour11Hash.Add(hour - mDstDifference, mw);
                                    }
                                }
                                else if (i == 11)
                                {
                                    hour12Hash.Add(hour - 1, mw);
                                }
                                else if (i == 12)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour13Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.WSI.StartsWith("miso"))
                                    {
                                        hour13Hash.Add(hour - 2, mw);
                                    }
                                    else
                                    {
                                        hour13Hash.Add(hour - mDstDifference, mw);
                                    }
                                }
                                else if (i == 13)
                                {
                                    if (Zone.ToLower().Equals("comed congestion") || Zone.ToLower().Equals("dom congestion"))
                                    {
                                        hour14Hash.Add(hour - 1, mw);
                                    }
                                    else if (load != null && load.WSI.StartsWith("miso"))
                                    {
                                        hour14Hash.Add(hour - 2, mw);
                                    }
                                    else
                                    {
                                        hour14Hash.Add(hour - mDstDifference, mw);
                                    }
                                }
                            }
                            reader.Close();
                        }
                    }
                    VayuDBConnection.Close();

                    hashValues = new HashValues()
                    {
                        HourZeroHash = hourHash,
                        Hour1Hash = hour1Hash,
                        Hour2Hash = hour2Hash,
                        Hour3Hash = hour3Hash,
                        Hour4Hash = hour4Hash,
                        Hour6Hash = hour6Hash,
                        Hour7Hash = hour7Hash,
                        Hour8Hash = hour8Hash,
                        Hour9Hash = hour9Hash,
                        Hour10Hash = hour10Hash,
                        Hour11Hash = hour11Hash,
                        Hour12Hash = hour12Hash,
                        Hour13Hash = hour13Hash,
                        Hour14Hash = hour14Hash,
                        Hour15Hash = hour15Hash,
                        HourFiveHash = hour5Hash,
                        ZoneName = Zone
                    };
                }
                catch (Exception ex)
                {
                    //Task.Factory.StartNew(() => MainExceptionHelper(ex));
                    return new HashValues();
                }
            }
            return hashValues;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <returns>Vayu DataBase Connection</returns>
        private SqlConnection GetConnection(DateTime startDate)
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            return VayuDBConnection;
        }
        #endregion
    }
}
