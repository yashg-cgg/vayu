using Vayu;
using Vayu.LatestConstraintsInformationLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Vayu.CommonAccessLibrary;

namespace Vayu.LatestConstraintInformationService
{
    /// <summary>
    /// Get Latest Constraints Information
    /// </summary>
    /// <seealso cref="Vayu.IConstraintInfoProvider" />
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant, MaxItemsInObjectGraph = int.MaxValue)]
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public partial class ConstrainInformationServer : IConstraintInfoProvider
    {
        #region Private Members
        /// <summary>
        /// The command select PJM market constraint
        /// </summary>
        private SqlCommand cmdSelectPjmMarketConstraint;
        /// <summary>
        /// The command select miso market constraint
        /// </summary>
        private SqlCommand cmdSelectMISOMarketConstraint;
        /// <summary>
        /// The command select caiso market constraint
        /// </summary>
        private SqlCommand cmdSelectCAISOMarketConstraint;
        /// <summary>
        /// The command select ercot market constraint
        /// </summary>
        private SqlCommand cmdSelectERCOTMarketConstraint;
        /// <summary>
        /// The command select nyiso market constraint
        /// </summary>
        private SqlCommand cmdSelectNYISOMarketConstraint;
        /// <summary>
        /// The command select SPP market constraint
        /// </summary>
        private SqlCommand cmdSelectSppMarketConstraint;

        private SqlCommand cmdSelectNSAConstraint;
        /// <summary>
        /// The dictionary constraint hash
        /// </summary>
        private static Dictionary<int, List<LatestConstraint>> dictConstraintHash = new Dictionary<int, List<LatestConstraint>>();
        /// <summary>
        /// The dictionary subscriber hash
        /// </summary>
        private static Dictionary<IConstraintInfoCallback, int> dictSubscriberHash = new Dictionary<IConstraintInfoCallback, int>();
        /// <summary>
        /// The hashset remover hash
        /// </summary>
        private static HashSet<IConstraintInfoCallback> hsRemoverHash = new HashSet<IConstraintInfoCallback>();
        /// <summary>
        /// The timer
        /// </summary>
        private static System.Timers.Timer sTimer = new System.Timers.Timer();
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object lockObj = new object();
        /// <summary>
        /// The start date
        /// </summary>
        private DateTime startDate;
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the database related object.
        /// </summary>
        private void LoadDB()
        {
            try
            { 
                // NSA Ercot
                cmdSelectNSAConstraint = Configuration.GetErcotDBCommand();
                 
                cmdSelectNSAConstraint.CommandText = " select distinct AC.MonitoredText ,AC.Contingency, AC.MarketDateTime,AC.MonitoredElementType, " +
                                                  " AC.ContingencyDesc,AC.MonitoredID1,AC.MonitoredID2,AC.RatingType,AC.RatingMW," +
                                                  " AC.PostCTGFlowMW,AC.PercentViolation,AC.DSTFlag,CG.SourceNodeKey,CG.SinkNodeKey,AC.FromKV,AC.ToKV,AC.ConstrainedSCEDLimitMW,AC.SCEDRatingMVA,AC.PostCTGFlowMVA, AC.FromStation,AC.ToStation,AC.FromKV,AC.ToKV " +
                                                  " from Vayu..ActiveConstraints AC left join Vayu..ConstraintGeo CG on AC.MonitoredText = CG.ConstraintText and AC.Contingency = CG.ContingencyText " +
                                                  " where AC.MarketDateTime between @startDate and @enddate order by MarketDateTime desc";


                cmdSelectNSAConstraint.Parameters.AddWithValue("@startDate", "");
                cmdSelectNSAConstraint.Parameters.AddWithValue("@enddate", "");
                 
                //ERCOT
                cmdSelectERCOTMarketConstraint = Configuration.GetErcotDBCommand();
                cmdSelectERCOTMarketConstraint.CommandText = " select distinct sc.ConstraintText ,sc.ContingencyText, sc.MarketDateTime, sc.ShadowPrice,sg.SourceNodeKey,sg.SinkNodeKey ,sg.ConstraintKV,sg.ContingencyKV, sg.Type " +
                " from Vayu..ConstraintRT sc left join Vayu..ConstraintGeo sg on sc.ConstraintText = sg.ConstraintText and sc.ContingencyText = sg.ContingencyText " +
                " where sc.MarketDateTime between @startDate and @enddate order by MarketDateTime desc ";
                cmdSelectERCOTMarketConstraint.Parameters.AddWithValue("@startDate", "");
                cmdSelectERCOTMarketConstraint.Parameters.AddWithValue("@enddate", "");
             }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Called when [timed event].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The System.Timers.ElapsedEventArgs instance containing the event data.</param>
        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            try
            {
                if (dictSubscriberHash.Count > 0)
                {
                    CallBackHelper();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            sTimer.Enabled = true;
        }
        /// <summary>
        /// Calls the back helper.
        /// </summary>
        private void CallBackHelper()
        {
            lock (lockObj)
            {
                try
                {
                    LoadDB();
                    foreach (int mKey in dictSubscriberHash.Values.Distinct())
                    {
                        if (dictConstraintHash.ContainsKey(mKey))
                        {
                            dictConstraintHash.Remove(mKey);
                        }
                        dictConstraintHash.Add(mKey, GetNSAConstraintList(mKey, DateTime.Now, true));//change Rajkumar GetConstraintList
                    }
                    foreach (IConstraintInfoCallback iCallbackItem in dictSubscriberHash.Keys)
                    {
                        try
                        {
                            Console.WriteLine("Sending constraints for market:" + dictSubscriberHash[iCallbackItem]);
                            iCallbackItem.SetConstraints(dictConstraintHash[dictSubscriberHash[iCallbackItem]]);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (!hsRemoverHash.Contains(iCallbackItem))
                            {
                                hsRemoverHash.Add(iCallbackItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            //new ApplicationLog.ApplicationLog(AppDomain.CurrentDomain.FriendlyName, "").UpdateError(System.Reflection.MethodInfo.GetCurrentMethod().Name, ex.Message);
                        }

                    }
                    foreach (var item in hsRemoverHash)
                    {
                        if (dictSubscriberHash.ContainsKey(item))
                        {
                            dictSubscriberHash.Remove(item);
                        }
                    }
                    hsRemoverHash.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + "\t" + ex.Message);
                }
            }
        }
        /// <summary>
        /// Gets the subscriber ip address.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns>Ip Address</returns>
        private string GetSubscriberIpAddress(OperationContext operationContext)
        {
            return ((operationContext.IncomingMessageProperties as MessageProperties)[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;
        }
        /// <summary>
        /// Gets the constraint list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="date">The date.</param>
        /// <param name="isAllDay">if set to <c>true</c> [is all day].</param>
        /// <returns>temp Constraint List</returns>
        private List<LatestConstraint> GetConstraintList(int marketKey, DateTime date, bool isAllDay)
        {
            lock (lockObj)
            {
                LoadDB();
                List<LatestConstraint> tempConstraintList = new List<LatestConstraint>();
                SqlCommand command = null;

                try
                {
                    switch (marketKey)
                    {
                         

                        case 9:
                            command = cmdSelectERCOTMarketConstraint;
                            break;

                        default:
                            return tempConstraintList;
                    }

                    if (isAllDay)
                    {
                        DateTime firstHour = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                        command.Parameters["@startDate"].Value = firstHour;
                        command.Parameters["@enddate"].Value = firstHour.AddDays(1).AddHours(-1);
                    }
                    else
                    {
                        command.Parameters["@startDate"].Value = date.AddHours(-1);
                        command.Parameters["@enddate"].Value = date;
                    }

                    if (command.Connection.State != ConnectionState.Open)
                        command.Connection.Open();

                    IDataReader reader = command.ExecuteReader();

                    if (marketKey == 12)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                tempConstraintList.Add(new LatestConstraint()
                                {
                                    ConstraintText = reader[0].ToString(),
                                    ContigencyText = reader[1].ToString(),
                                    MarketDate = Configuration.GetDate(reader[2]),
                                    ShadowPrice = Configuration.GetDouble(reader[3]),
                                    SourceNodeKey = Configuration.GetInt(reader[4]),
                                    SinkNodeKey = Configuration.GetInt(reader[5]),
                                    ConstraintKV = Configuration.GetDouble(reader[6]),
                                    ContingencyKV = Configuration.GetDouble(reader[7]),
                                    ConstraintType = (reader[8] ?? "").ToString(),
                                    MonitoredFacility = (reader[9] ?? "").ToString(),
                                    TLRLevel = (reader[10] ?? "").ToString(),
                                    State = (reader[11] ?? "").ToString()
                                });
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                tempConstraintList.Add(new LatestConstraint()
                                {
                                    ConstraintText = reader[0].ToString(),
                                    ContigencyText = reader[1].ToString(),
                                    MarketDate = Configuration.GetDate(reader[2]),
                                    ShadowPrice = Configuration.GetDouble(reader[3]),
                                    SourceNodeKey = Configuration.GetInt(reader[4]),
                                    SinkNodeKey = Configuration.GetInt(reader[5]),
                                    ConstraintKV = Configuration.GetDouble(reader[6]),
                                    ContingencyKV = Configuration.GetDouble(reader[7]),
                                    ConstraintType = (reader[8] ?? "").ToString()
                                });
                            }
                            catch { }
                        }
                    }

                    if (reader != null)
                        reader.Close();
                    if (date.ToString("yyyy-MM-dd") == DateTime.Today.ToString("yyyy-MM-dd"))
                    {
                        tempConstraintList.RemoveAll(a => a.MarketDate < DateTime.Now.AddHours(-2));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new List<LatestConstraint>();
                }
                finally
                {
                    if (command != null && command.Connection.State != ConnectionState.Closed)
                        command.Connection.Close();
                }

                return tempConstraintList;
            }
        }
        public List<LatestConstraint> GetNSAConstraintList(int marketKey, DateTime date, bool isAllDay)
        {
            lock (lockObj)
            {
                LoadDB();
                List<LatestConstraint> tempNSAConstraint = new List<LatestConstraint>();
                SqlCommand command = null;
                try
                {
                    switch (marketKey)
                    {
                         
                        case 9:
                            command = cmdSelectNSAConstraint;
                            break;
                        default:
                            tempNSAConstraint.ToList();
                            break;
                    }
                    if (isAllDay)
                    {
                        DateTime firstHour = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                        command.Parameters["@startDate"].Value = firstHour;
                        command.Parameters["@enddate"].Value = firstHour.AddDays(1).AddHours(-1);
                    }
                    else
                    {
                        command.Parameters["@startDate"].Value = date.AddMinutes(-10);
                        command.Parameters["@enddate"].Value = date;
                    }
                    if (command.Connection.State == ConnectionState.Closed)
                        command.Connection.Open();
                    IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            if (marketKey == 9)
                                tempNSAConstraint.Add(new LatestConstraint()
                                {

                                    ConstraintText = reader[0].ToString(),
                                    ContigencyText = reader[1].ToString(),
                                    MarketDate = Configuration.GetDate(reader[2].ToString()),
                                    ConstraintType = reader[3].ToString(),
                                    ContingencyDesc = reader[4].ToString(),
                                    MonitoredID1 = reader[5].ToString(),
                                    MonitoredID2 = reader[6].ToString(),
                                    RatingType = reader[7].ToString(),
                                    RatingMW = Configuration.GetDouble(reader[8].ToString()),
                                    PostCTGFlowMW = Configuration.GetDouble(reader[9].ToString()),
                                    PercentViolation = Configuration.GetDouble(reader[10].ToString()),
                                    DSTFlag = reader[11].ToString(),
                                    SourceNodeKey = Configuration.GetInt(reader[12]),
                                    SinkNodeKey = Configuration.GetInt(reader[13]),
                                    ConstraintKV = Configuration.GetDouble(reader[14]),
                                    ContingencyKV = Configuration.GetDouble(reader[15]),
                                    ConstrainedSCEDLimitMW = Configuration.GetDouble(reader[16]),
                                    SCEDRatingMVA = Configuration.GetDouble(reader[17]),
                                    PostCTGFlowMVA = Configuration.GetDouble(reader[18]),
                                    FromStation = reader[19].ToString(),
                                    ToStation = reader[20].ToString(),
                                    FromKV = Configuration.GetDouble(reader[21]),
                                    ToKV = Configuration.GetDouble(reader[22]),
                                });
                            else
                                tempNSAConstraint.Add(new LatestConstraint()
                                {
                                    ConstraintText = reader[0].ToString(),
                                    ContigencyText = reader[1].ToString(),
                                    MarketDate = Configuration.GetDate(reader[2]),
                                    ShadowPrice = Configuration.GetDouble(reader[3]),
                                    SourceNodeKey = Configuration.GetInt(reader[4]),
                                    SinkNodeKey = Configuration.GetInt(reader[5]),
                                    ConstraintKV = Configuration.GetDouble(reader[6]),
                                    ContingencyKV = Configuration.GetDouble(reader[7]),
                                    ConstraintType = (reader[8] ?? "").ToString()
                                });
                        }
                        catch
                        {

                        }
                    }
                    if (reader != null)
                        reader.Close();
                }
                catch
                {
                    return new List<LatestConstraint>();
                }
                finally
                {
                    if (command.Connection.State == ConnectionState.Open)
                        command.Connection.Close();
                }
                return tempNSAConstraint.ToList();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the latest constraint.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="date">The date.</param>
        /// <param name="isAllDay">if set to <c>true</c> [is all day].</param>
        /// <returns>Latest Constraint</returns>
        public List<LatestConstraint> GetLatestConstraint(int marketKey, DateTime date, bool isAllDay)
        {
            startDate = date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
            return GetConstraintList(marketKey, startDate, isAllDay);
        }
        /// <summary>
        /// Gets the nsa active constraint.
        /// </summary>
        /// <param name="Marketkey">The marketkey.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="IsAllDay">if set to <c>true</c> [is all day].</param>
        /// <returns></returns>
        public List<LatestConstraint> GetNSAActiveConstraint(int Marketkey, DateTime startDate, bool IsAllDay)
        {
            startDate = startDate.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
            return GetNSAConstraintList(Marketkey, startDate, IsAllDay);
        }
        /// <summary>
        /// Subscribes the specified market key.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        public void Subscribe(int marketKey)
        {
            lock (lockObj)
            {
                try
                {
                    IConstraintInfoCallback callbackItem = OperationContext.Current.GetCallbackChannel<IConstraintInfoCallback>();
                    if (dictSubscriberHash.ContainsKey(callbackItem))
                    {
                        dictSubscriberHash.Remove(callbackItem);
                    }
                    dictSubscriberHash.Add(callbackItem, marketKey);
                    Console.WriteLine("Total number of subscribers:\t" + dictSubscriberHash.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        /// <summary>
        /// Hearts the beat.
        /// </summary>
        public void HeartBeat()
        {
            try
            {
                Console.WriteLine(DateTime.Now + "\t HeartBeat checked by \t" + GetSubscriberIpAddress(OperationContext.Current));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
                    IConstraintInfoCallback callbackItem = OperationContext.Current.GetCallbackChannel<IConstraintInfoCallback>();
                    if (dictSubscriberHash.ContainsKey(callbackItem))
                    {
                        dictSubscriberHash.Remove(callbackItem);
                    }
                    Console.WriteLine("Total number of subscribers:\t" + dictSubscriberHash.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Subscribes the specified market key.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="dateTime">The date time.</param>
        public void Subscribe(int marketKey, DateTime dateTime)
        {
            Subscribe(marketKey);
        }
        #endregion
        /// <summary>
        /// Connects this instance.
        /// </summary>
        internal void Connect()
        {
            LoadDB();
            NetTcpBinding binding = new NetTcpBinding();
            binding.OpenTimeout = new TimeSpan(0, 12, 0);
            binding.SendTimeout = new TimeSpan(0, 12, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 12, 0);
            binding.Security.Mode = SecurityMode.None;
            binding.CloseTimeout = new TimeSpan(0, 12, 0);
            var behavior = new ServiceThrottlingBehavior()
            {
                MaxConcurrentCalls = 10000,
                MaxConcurrentInstances = 10000,
                MaxConcurrentSessions = 10000
            };
            ServiceHost host = new ServiceHost(typeof(ConstrainInformationServer));
            host.Description.Behaviors.Add(behavior);
            host.AddServiceEndpoint(typeof(IConstraintInfoProvider), binding, new Uri(ServiceConnections.GetLatestConstraintService()));
            try
            {
                host.Open();
                sTimer.Enabled = true;
                sTimer.Interval = 40000;
                sTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                sTimer.Start();
                Console.WriteLine("Successfully opened port 8002 for Latest Constraint Information Service \t" + DateTime.Now);
                Console.Read();
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}



