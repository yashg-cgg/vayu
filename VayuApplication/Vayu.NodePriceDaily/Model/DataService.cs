using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.NodePriceDaily.Model
{
    public class DataService : IDataService
    {
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object lockObj = new object();

        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;

        #region SQL Commands

        /// <summary>
        /// The m select daily LMP command
        /// </summary>
        private SqlCommand mSelectDailyLMPCommand;
        /// <summary>
        /// The m select PJM daily LMP command
        /// </summary>
        private SqlCommand mSelectPjmDailyLMPCommand;
        /// <summary>
        /// The m select market date time command
        /// </summary>
        private SqlCommand mSelectMarketDateTimeCommand;
        /// <summary>
        /// The m select daily caiso LMP command
        /// </summary>
        private SqlCommand mSelectDailyCaisoLMPCommand;
        /// <summary>
        /// The m select nyiso LMP daily command
        /// </summary>
        private SqlCommand mSelectNyisoLmpDailyCommand;
        /// <summary>
        /// The m select SPP LMP daily command
        /// </summary>
        private SqlCommand mSelectSPPLmpDailyCommand;
        /// <summary>
        /// The m select SPP da daily command
        /// </summary>
        private SqlCommand mSelectSppDaDailyCommand;
        /// <summary>
        /// The m select SPP datetime command
        /// </summary>
        private SqlCommand mSelectSppDatetimeCommand;
        /// <summary>
        /// The m select miso daily LMP command
        /// </summary>
        private SqlCommand mSelectMisoDailyLmpCommand;
        /// <summary>
        /// The m select ercot daily LMP command
        /// </summary>
        private SqlCommand mSelectErcotDailyLmpCommand;

        #endregion

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectDailyLMPCommand = new SqlCommand();
            mSelectDailyLMPCommand.CommandText = "select nodekey, avgpeaklmp, avgoffpeaklmp, avg24lmp from nodelmpdailys (nolock) where marketkey = @marketkey and marketdatetime = @marketdatetime";
            mSelectDailyLMPCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectDailyLMPCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mSelectDailyLMPCommand.Connection = VayuConnection;
            //

            mSelectPjmDailyLMPCommand = new SqlCommand();
            mSelectPjmDailyLMPCommand.CommandText = "select nodekey, avgpeaklmp, avgoffpeaklmp, avg24lmp from pjm.nodelmpdailys (nolock) where markettypecode = @markettypecode and MarketDate = @MarketDate";
            mSelectPjmDailyLMPCommand.Parameters.AddWithValue("@markettypecode", "markettypecode");
            mSelectPjmDailyLMPCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectPjmDailyLMPCommand.Connection = VayuConnection;

            //

            mSelectDailyCaisoLMPCommand = new SqlCommand();
            mSelectDailyCaisoLMPCommand.CommandText = "select nodekey, avgpeaklmp, avgoffpeaklmp, avg24lmp from caiso.nodelmpdailys (nolock) where markettypecode = @markettypecode and MarketDate = @MarketDate";
            mSelectDailyCaisoLMPCommand.Parameters.AddWithValue("@markettypecode", "markettypecode");
            mSelectDailyCaisoLMPCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectDailyCaisoLMPCommand.Connection = VayuConnection;

            mSelectMarketDateTimeCommand = new SqlCommand();
            mSelectMarketDateTimeCommand.CommandText = "select peakyn from MarketTime (nolock)  where MarketKey = @marketkey and marketdatetime = @marketdatetime";
            mSelectMarketDateTimeCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectMarketDateTimeCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mSelectMarketDateTimeCommand.Connection = VayuConnection;
            //
            mSelectSppDatetimeCommand = new SqlCommand();
            mSelectSppDatetimeCommand.CommandText = "select peakYN  from spp.MarketDate (nolock) where marketdatetime = @marketdatetime";
            mSelectSppDatetimeCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mSelectSppDatetimeCommand.Connection = VayuConnection;
            //
            mSelectNyisoLmpDailyCommand = new SqlCommand();
            mSelectNyisoLmpDailyCommand.CommandText = "select nodekey ,avgpeaklmp, avgoffpeaklmp, avg24lmp  from NYISO.nodelmpdailys (nolock) where markettypecode = @markettypecode  and MarketDate = @MarketDate";
            mSelectNyisoLmpDailyCommand.Parameters.AddWithValue("@markettypecode", "markettypecode");
            mSelectNyisoLmpDailyCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectNyisoLmpDailyCommand.Connection = VayuConnection;
            //
            mSelectSPPLmpDailyCommand = new SqlCommand();
            mSelectSPPLmpDailyCommand.CommandText = "select nodekey ,avgpeaklmp, avgoffpeaklmp, avg24lmp  from SPP.nodelmpdailys (nolock) where  MarketDate = @MarketDate and  markettypecode = @markettypecode";
            mSelectSPPLmpDailyCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectSPPLmpDailyCommand.Parameters.AddWithValue("@markettypecode", "markettypecode");
            mSelectSPPLmpDailyCommand.Connection = VayuConnection;
            //
            mSelectSppDaDailyCommand = new SqlCommand();
            mSelectSppDaDailyCommand.CommandText = "select nodekey ,avgpeaklmp, avgoffpeaklmp, avg24lmp  from SPP.NodeDALMPDaily (nolock) where  MarketDate = @MarketDate";
            mSelectSppDaDailyCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectSppDaDailyCommand.Connection = VayuConnection;
            //
            mSelectMisoDailyLmpCommand = new SqlCommand();
            mSelectMisoDailyLmpCommand.CommandText = "select nodekey ,avgpeaklmp, avgoffpeaklmp, avg24lmp  from MISO.nodelmpdailys (nolock) where  MarketDate = @MarketDate and  markettypecode = @markettypecode";
            mSelectMisoDailyLmpCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectMisoDailyLmpCommand.Parameters.AddWithValue("@markettypecode", "markettypecode");
            mSelectMisoDailyLmpCommand.Connection = VayuConnection;
            //
            mSelectErcotDailyLmpCommand = new SqlCommand();
            mSelectErcotDailyLmpCommand.CommandText = "select nodekey ,avgpeaklmp, avgoffpeaklmp, avg24lmp   from nodelmpdailys (nolock) where  MarketDate = @MarketDate  and MarketTypeCode = @markettypecode";
            mSelectErcotDailyLmpCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectErcotDailyLmpCommand.Parameters.AddWithValue("@markettypecode", "markettypecode");
            mSelectErcotDailyLmpCommand.Connection = VayuConnection;
        }
        /// <summary>
        /// Determines whether [is peak day] [the specified market date].
        /// </summary>
        /// <param name="marketDate">The market date.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns>
        ///   <c>true</c> if [is peak day] [the specified market date]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPeakDay(DateTime marketDate, int marketKey)
        {
            loadDBCommands();
            string peakYN = null;
            lock (lockObj)
            {
                if (marketKey != 12)
                {
                    using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = mSelectMarketDateTimeCommand.CommandText;
                            cmd.Parameters.AddWithValue("@marketkey", marketKey);
                            var a = marketDate.AddHours(14);
                            cmd.Parameters.AddWithValue("@marketdatetime", marketDate.AddHours(14));
                            con.Open();
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                peakYN = reader.GetString(0);
                            }
                            reader.Close();
                            con.Close();
                        }
                    }
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = mSelectSppDatetimeCommand.CommandText;
                            cmd.Parameters.AddWithValue("@marketdatetime", marketDate.AddHours(14));
                            con.Open();
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                peakYN = reader.GetString(0);
                            }
                            reader.Close();
                            con.Close();
                        }
                    }
                }
                return peakYN.ToUpper() == "Y";
            }
        }
        /// <summary>
        /// Gets the daily LMP.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="marketDateTime">The market date time.</param>
        /// <param name="isDA">if set to <c>true</c> [is da].</param>
        /// <returns></returns>
        public Dictionary<int, DailyLMP> GetDailyLMP(int marketKey, DateTime marketDateTime, bool isDA)
        {
            loadDBCommands();
            lock (lockObj)
            {
                Dictionary<int, DailyLMP> dailyLmpHash = new Dictionary<int, DailyLMP>();
                SqlDataReader reader = null;
                if (marketKey == 3)
                {
                    if (isDA)
                    {
                        mSelectNyisoLmpDailyCommand.Parameters["@markettypecode"].Value = "DA";
                    }
                    else
                    {
                        mSelectNyisoLmpDailyCommand.Parameters["@markettypecode"].Value = "RT";
                    }
                    mSelectNyisoLmpDailyCommand.Parameters["@MarketDate"].Value = marketDateTime;
                    if (mSelectNyisoLmpDailyCommand.Connection.State.Equals(ConnectionState.Closed))
                    {
                        mSelectNyisoLmpDailyCommand.Connection.Open();
                    }
                    reader = mSelectNyisoLmpDailyCommand.ExecuteReader();
                }
                else if (marketKey == 9)
                {
                    if (isDA)
                    {
                        mSelectErcotDailyLmpCommand.Parameters["@markettypecode"].Value = "DA";
                    }
                    else
                    {
                        mSelectErcotDailyLmpCommand.Parameters["@markettypecode"].Value = "RT";
                    }

                    mSelectErcotDailyLmpCommand.Parameters["@MarketDate"].Value = marketDateTime;
                    if (mSelectErcotDailyLmpCommand.Connection.State.Equals(ConnectionState.Closed))
                    {
                        mSelectErcotDailyLmpCommand.Connection.Open();
                    }
                    reader = mSelectErcotDailyLmpCommand.ExecuteReader();
                }
                while (reader.Read())
                {
                    int nodeKey = (int)reader.GetDecimal(0);
                    DailyLMP dailyLmp = new DailyLMP();
                    if (marketKey == 1)
                    {
                        dailyLmp.Peak = reader.IsDBNull(1) ? (double?)null : Convert.ToDouble(reader.GetValue(1));
                        dailyLmp.OffPeak = reader.IsDBNull(2) ? (double?)null : Convert.ToDouble(reader.GetValue(2));
                        dailyLmp.Average = reader.IsDBNull(3) ? (double?)null : Convert.ToDouble(reader.GetValue(3));
                    }

                    else
                    {
                        dailyLmp.Peak = reader.IsDBNull(1) ? (double?)null : Convert.ToDouble(reader.GetValue(1));
                        dailyLmp.OffPeak = reader.IsDBNull(2) ? (double?)null : Convert.ToDouble(reader.GetValue(2));
                        dailyLmp.Average = reader.IsDBNull(3) ? (double?)null : Convert.ToDouble(reader.GetValue(3));
                    }
                    dailyLmpHash.Add(nodeKey, dailyLmp);
                }
                reader.Close();
                VayuConnection.Close();
                return dailyLmpHash;

            }
        }
    }
}
