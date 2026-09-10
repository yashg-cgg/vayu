using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.NodeLMPLibrary
{
    public class LMPDatesHelper
    {
        #region Declaration & Properties

        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;

        /// <summary>
        /// Gets or sets the m select ice market date time command.
        /// </summary>
        /// <value>
        /// The m select ice market date time command.
        /// </value>
        protected SqlCommand mSelectICEMarketDateTimeCommand { get; set; }
        /// <summary>
        /// Gets or sets the m select SPP market date time command.
        /// </summary>
        /// <value>
        /// The m select SPP market date time command.
        /// </value>
        protected SqlCommand mSelectSPPMarketDateTimeCommand { get; set; }
        /// <summary>
        /// Gets or sets the m select market date time command.
        /// </summary>
        /// <value>
        /// The m select market date time command.
        /// </value>
        protected SqlCommand mSelectMarketDateTimeCommand { get; set; }

        /// <summary>
        /// Gets or sets the timing session.
        /// </summary>
        /// <value>
        /// The timing session.
        /// </value>
        public MarketTimingSession TimingSession { get; set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        protected virtual void loadDBCommands()
        {
            mSelectMarketDateTimeCommand.CommandText = "select MarketDateTime,PeakYN,MarketHour from MarketTime where MarketDateTime > @startDate and MarketDateTime <= @endDate and MarketKey = @marketkey order by MarketDateTime";
            InitializeCommonParameteres(mSelectMarketDateTimeCommand);

        }

        /// <summary>
        /// Initializes the common parameteres.
        /// </summary>
        /// <param name="command">The command.</param>
        protected void InitializeCommonParameteres(SqlCommand command)
        {
            command.Parameters.AddWithValue("@marketkey", "marketkey");
            command.Parameters.AddWithValue("@startDate", "startDate");
            command.Parameters.AddWithValue("@endDate", "endDate");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="LMPDatesHelper"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public LMPDatesHelper(SqlConnection connection = null)
        {
            VayuConnection = connection;
            if (VayuConnection == null)
            {
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            }
            mSelectMarketDateTimeCommand = new SqlCommand();
            mSelectMarketDateTimeCommand.Connection = VayuConnection;
            loadDBCommands();
            TimingSession = new MarketTimingSession(this);
        }

        /// <summary>
        /// Sets the common parameter values.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="marketKey">The market key.</param>
        public virtual void SetCommonParameterValues(SqlCommand command, DateTime startDateTime, DateTime endDateTime, int marketKey)
        {
            command.Parameters["@startDate"].Value = startDateTime;
            command.Parameters["@endDate"].Value = endDateTime;
            command.Parameters["@marketkey"].Value = marketKey;
        }

        /// <summary>
        /// Gets the market timings.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="isIce">if set to <c>true</c> [is ice].</param>
        /// <returns></returns>
        public List<MarketTime> GetMarketTimings(int marketKey, DateTime startDateTime, DateTime endDateTime, bool isIce = true)
        {
            IDataReader reader = null;
            List<MarketTime> marketTimeList = new List<MarketTime>();
            if (VayuConnection == null || startDateTime.Date == DateTime.MinValue.Date || endDateTime.Date == DateTime.MinValue.Date)
            {
                return marketTimeList;
            }
            try
            {
                if (VayuConnection.State == System.Data.ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }

                SetCommonParameterValues(mSelectMarketDateTimeCommand, startDateTime, endDateTime, marketKey);
                reader = mSelectMarketDateTimeCommand.ExecuteReader();

                while (reader.Read())
                {
                    MarketTime mTime = new MarketTime();
                    mTime.MarketDateTime = reader.GetDateTime(0);
                    mTime.PeakYN = reader.GetString(1);
                    mTime.marketHour = (int)reader.GetDecimal(2);

                    if ((marketKey == 2 && isIce) || (marketKey == 12))
                    {
                        mTime.IndexMarketHour = mTime.marketHour + 1;
                    }
                    else
                    {
                        mTime.IndexMarketHour = mTime.marketHour;
                    }
                    marketTimeList.Add(mTime);
                }
            }
            catch (Exception ex)
            {
            } //TODO:
            finally
            {
                reader.Close();
                VayuConnection.Close();
            }
            return marketTimeList;
        }

        /// <summary>
        /// Gets the peak dates.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="isIce">if set to <c>true</c> [is ice].</param>
        /// <returns></returns>
        public List<DateTime> GetPeakDates(int marketKey, DateTime startDateTime, DateTime endDateTime, bool isIce = false)
        {
            return GetPeakOffPeakDates(marketKey, startDateTime, endDateTime, "Y", isIce);
        }

        /// <summary>
        /// Gets the off peak dates.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="isIce">if set to <c>true</c> [is ice].</param>
        /// <returns></returns>
        public List<DateTime> GetOffPeakDates(int marketKey, DateTime startDateTime, DateTime endDateTime, bool isIce = false)
        {
            return GetPeakOffPeakDates(marketKey, startDateTime, endDateTime, "N", isIce);
        }
        public List<DateTime> GetPeakWEDates(int marketKey, DateTime startDateTime, DateTime endDateTime, bool isIce = false)
        {
            return GetPeakWEPeakDates(marketKey, startDateTime, endDateTime, "W", isIce);
        }

        /// <summary>
        /// Gets the market by label.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns></returns>
        public Market GetMarketByLabel(string label)
        {
            Market market = new Market();
            if (string.IsNullOrEmpty(label))
            {
                return null;
            }
            if (label.Equals("ercot", StringComparison.InvariantCultureIgnoreCase))
            {
                label = "ERCOTTesting";
            }
            SqlCommand command = new SqlCommand();
            command.CommandText = "select * from market where Label = @label";
            command.Parameters.AddWithValue("@label", label ?? "");
            command.Connection = VayuConnection;
            if (VayuConnection.State == System.Data.ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            try
            {
                IDataReader reader = command.ExecuteReader();
                if (reader == null)
                {
                    return market;
                }
                if (reader.Read())
                {
                    market.MarketKey = (decimal)reader["MarketKey"];
                    market.Label = (string)reader["Label"];
                    market.Timezone = (string)reader["Timezone"];
                    market.MarketTimeObservesDST = (bool)reader["MarketTimeObservesDST"];
                    if (DBNull.Value != reader["HEIntervalMinute"])
                    {
                        market.HEIntervalMinute = (int?)reader["HEIntervalMinute"];
                    }
                    if (DBNull.Value != reader["MinutesInInterval"])
                    {
                        market.MinutesInInterval = (int?)reader["MinutesInInterval"];
                    }
                }
                reader.Close();
            }
            catch (Exception ex) { }
            finally
            {
                VayuConnection.Close();
            }
            return market;
        }

        #endregion

        /// <summary>
        /// Gets the peak off peak dates.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="peakOffPeak">The peak off peak.</param>
        /// <param name="isIce">if set to <c>true</c> [is ice].</param>
        /// <returns></returns>
        private List<DateTime> GetPeakOffPeakDates(int marketKey, DateTime startDateTime, DateTime endDateTime, string peakOffPeak, bool isIce = false)
        {
            List<MarketTime> marketTimingList = GetMarketTimings(marketKey, startDateTime, endDateTime, isIce);
            List<DateTime> peakOffPeakList = marketTimingList.Where(x => x.PeakYN == peakOffPeak).Select(x => x.MarketDateTime).ToList();
            return peakOffPeakList;
        }
        private List<DateTime> GetPeakWEPeakDates(int marketKey, DateTime startDateTime, DateTime endDateTime, string peakOffPeak, bool isIce = false)
        {
            List<MarketTime> marketTimingList = GetMarketTimings(marketKey, startDateTime, endDateTime, isIce);
            List<DateTime> peakOffPeakList = marketTimingList.Where(x => x.PeakYN == peakOffPeak).Select(x => x.MarketDateTime).ToList();
            return peakOffPeakList;
        }

        /// <summary>
        /// 
        /// </summary>
        public class MarketTimingSession
        {
            #region Declaration

            /// <summary>
            /// The parent
            /// </summary>
            private LMPDatesHelper parent;
            /// <summary>
            /// The session timings
            /// </summary>
            private List<MarketTime> sessionTimings;
            /// <summary>
            /// The off peak timings hash
            /// </summary>
            private Hashtable offPeakTimingsHash;
            /// <summary>
            /// The on peak timings hash
            /// </summary>
            private Hashtable onPeakTimingsHash;

            /// <summary>
            /// The off peak count hash
            /// </summary>
            private Hashtable offPeakCountHash;
            /// <summary>
            /// The on peak count hash
            /// </summary>
            private Hashtable onPeakCountHash;

            #endregion

            #region Public Methods

            /// <summary>
            /// Initializes a new instance of the <see cref="MarketTimingSession"/> class.
            /// </summary>
            /// <param name="helper">The helper.</param>
            public MarketTimingSession(LMPDatesHelper helper)
            {
                parent = helper;
                offPeakTimingsHash = new Hashtable();
                onPeakTimingsHash = new Hashtable();
                offPeakCountHash = new Hashtable();
                onPeakCountHash = new Hashtable();
                sessionTimings = new List<MarketTime>();
            }

            /// <summary>
            /// Initializes the specified label.
            /// </summary>
            /// <param name="label">The label.</param>
            /// <param name="startDateTime">The start date time.</param>
            /// <param name="endDateTime">The end date time.</param>
            /// <param name="isIce">if set to <c>true</c> [is ice].</param>
            public void Initialize(string label, DateTime startDateTime, DateTime endDateTime, bool isIce = true)
            {
                Market market = parent.GetMarketByLabel(label);
                if (market == null)
                {
                    return;
                }
                Initialize((int)market.MarketKey, startDateTime, endDateTime, isIce);
            }

            /// <summary>
            /// Initializes the specified market key.
            /// </summary>
            /// <param name="marketKey">The market key.</param>
            /// <param name="startDateTime">The start date time.</param>
            /// <param name="endDateTime">The end date time.</param>
            /// <param name="isIce">if set to <c>true</c> [is ice].</param>
            public void Initialize(int marketKey, DateTime startDateTime, DateTime endDateTime, bool isIce = true)
            {
                if (marketKey == 0)
                {
                    return;
                }
                while (startDateTime <= endDateTime)
                {
                    DateTime nextDay = startDateTime.AddDays(1);
                    List<MarketTime> timings = parent.GetMarketTimings(marketKey, startDateTime, nextDay, isIce);
                    sessionTimings.AddRange(timings);
                    startDateTime = nextDay;
                }

                for (int i = 0; i < sessionTimings.Count; i++)
                {
                    if (sessionTimings[i].PeakYN.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                    {
                        onPeakTimingsHash[sessionTimings[i].MarketDateTime] = sessionTimings[i];
                        onPeakCountHash[sessionTimings[i].MarketDateTime.Hour] = sessionTimings[i];
                    }
                    else
                    {
                        offPeakTimingsHash[sessionTimings[i].MarketDateTime] = sessionTimings[i];
                        offPeakCountHash[sessionTimings[i].MarketDateTime.Hour] = sessionTimings[i];
                    }
                }
            }

            /// <summary>
            /// Determines whether [is off peak] [the specified time].
            /// </summary>
            /// <param name="time">The time.</param>
            /// <returns>
            ///   <c>true</c> if [is off peak] [the specified time]; otherwise, <c>false</c>.
            /// </returns>
            public bool IsOffPeak(DateTime time)
            {
                if (offPeakTimingsHash.ContainsKey(time))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Determines whether [is on peak] [the specified time].
            /// </summary>
            /// <param name="time">The time.</param>
            /// <returns>
            ///   <c>true</c> if [is on peak] [the specified time]; otherwise, <c>false</c>.
            /// </returns>
            public bool IsOnPeak(DateTime time)
            {
                if (onPeakTimingsHash.ContainsKey(time))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Determines whether [is off peak] [the specified index].
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>
            ///   <c>true</c> if [is off peak] [the specified index]; otherwise, <c>false</c>.
            /// </returns>
            public bool IsOffPeak(int index)
            {
                if (offPeakCountHash.ContainsKey(index))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Determines whether [is on peak] [the specified index].
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>
            ///   <c>true</c> if [is on peak] [the specified index]; otherwise, <c>false</c>.
            /// </returns>
            public bool IsOnPeak(int index)
            {
                if (onPeakCountHash.ContainsKey(index))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion
        }
    }
}
