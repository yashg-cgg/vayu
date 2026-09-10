using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Vayu.CommonAccessLibrary;

namespace Vayu.NodePriceMonitor.Model
{
    public class DataService : IDataService
    {
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;
        /// <summary>
        /// The Vayu database connr
        /// </summary>


        #region SQL Commands

        /// <summary>
        /// The m select market command
        /// </summary>
        SqlCommand mSelectMarketCommand;
        /// <summary>
        /// The m select market node command
        /// </summary>
        private SqlCommand mSelectMarketNodeCommand;
        /// <summary>
        /// The m select da command
        /// </summary>
        private SqlCommand mSelectDACommand;
        /// <summary>
        /// The m select whadda command
        /// </summary>
        private SqlCommand mSelectWHADDACommand;
        /// <summary>
        /// The m select ercotda command
        /// </summary>
        private SqlCommand mSelectERCOTDACommand;
        /// <summary>
        /// The m select misort command
        /// </summary>
        private SqlCommand mSelectMISORTCommand;
        /// <summary>
        /// The m select PJMRT command
        /// </summary>
        private SqlCommand mSelectPJMRTCommand;
        /// <summary>
        /// The m select pjmwhadrt command
        /// </summary>
        private SqlCommand mSelectPJMWHADRTCommand;
        /// <summary>
        /// The m select misowhadrt command
        /// </summary>
        private SqlCommand mSelectMISOWHADRTCommand;
        /// <summary>
        /// The m select caisoda command
        /// </summary>
        private SqlCommand mSelectCAISODACommand;
        /// <summary>
        /// The m select caisort command
        /// </summary>
        private SqlCommand mSelectCAISORTCommand;
        /// <summary>
        /// The m select ercotrt command
        /// </summary>
        private SqlCommand mSelectERCOTRTCommand;
        /// <summary>
        /// The m select strategy command
        /// </summary>
        private SqlCommand mSelectStrategyCommand;
        /// <summary>
        /// The m insert strategy command
        /// </summary>
        private SqlCommand mInsertStrategyCommand;
        /// <summary>
        /// The m delete strategy command
        /// </summary>
        private SqlCommand mDeleteStrategyCommand;
        /// <summary>
        /// The m select pjmda command
        /// </summary>
        private SqlCommand mSelectPJMDACommand;

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectMarketCommand = new SqlCommand();
            mSelectMarketCommand.CommandText = "select Distinct MarketKey, MarketName from LmpMonitorNode (nolock) where marketname not in ('MISO','CAISO')";
            mSelectMarketCommand.Connection = VayuConnection;
            //
            mSelectMarketNodeCommand = new SqlCommand();
            mSelectMarketNodeCommand.CommandText = "Select * from LmpMonitorNode (nolock) Where MarketKey = @MarketKey and marketname not in ('MISO','CAISO')";
            mSelectMarketNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectMarketNodeCommand.Connection = VayuConnection;
            //
            mSelectERCOTDACommand = new SqlCommand();
            mSelectERCOTDACommand.CommandText = "select MarketDateTime, lmp from Vayu..NodeDALMPH where NodeKey = @nodekey and MarketDateTime > @start and MarketDateTime <= @end order by MarketDateTime";
            mSelectERCOTDACommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectERCOTDACommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectERCOTDACommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectERCOTDACommand.Connection = VayuConnection;
            //
            mSelectWHADDACommand = new SqlCommand();
            mSelectWHADDACommand.CommandText = "Select ad.MarketDateTime, WH.LMP-AD.LMP from NodeDALMPH WH Join NodeDALMPH AD on WH.MarketDateTime = AD.MarketDateTime " +
                                                "Where WH.NodeKey = 30 And AD.NodeKey = 21 And AD.MarketDateTime Between @start And @end " +
                                                "And WH.MarketDateTime Between @start And @end";
            mSelectWHADDACommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectWHADDACommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectWHADDACommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectWHADDACommand.Connection = VayuConnection;
            //
            mSelectERCOTRTCommand = new SqlCommand();
            mSelectERCOTRTCommand.CommandText = "select MarketDateTime, lmp from Vayu..NodeLMPH where NodeKey = @nodekey and MarketDateTime > @start and MarketDateTime <= @end order by MarketDateTime";
            mSelectERCOTRTCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectERCOTRTCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectERCOTRTCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectERCOTRTCommand.Connection = VayuConnection;
            //
            mSelectPJMRTCommand = new SqlCommand();
            mSelectPJMRTCommand.CommandText = "select MarketDateTime, lmp from PJM.NodeLMPH (nolock) where NodeKey = @nodekey and MarketDateTime > @start and MarketDateTime <= @end order by MarketDateTime";
            mSelectPJMRTCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectPJMRTCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectPJMRTCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectPJMRTCommand.Connection = VayuConnection;
            //
            mSelectMISORTCommand = new SqlCommand();
            mSelectMISORTCommand.CommandText = "select MarketDateTime, lmp from MISO.NodeLMPH where NodeKey = @nodekey and MarketDateTime > @start and MarketDateTime <= @end order by MarketDateTime";
            mSelectMISORTCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectMISORTCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectMISORTCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectMISORTCommand.Connection = VayuConnection;

            //
            mSelectMISOWHADRTCommand = new SqlCommand();
            mSelectMISOWHADRTCommand.CommandText = "Select ad.MarketDateTime, WH.LMP-AD.LMP from NodeLMPH WH Join NodeLMPH AD on WH.MarketDateTime = AD.MarketDateTime " +
                                                "Where WH.NodeKey = 30 And AD.NodeKey = 21 And AD.MarketDateTime Between @start And @end " +
                                                "And WH.MarketDateTime Between @start And @end";
            mSelectMISOWHADRTCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectMISOWHADRTCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectMISOWHADRTCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectMISOWHADRTCommand.Connection = VayuConnection;

            //
            mInsertStrategyCommand = new SqlCommand();
            mInsertStrategyCommand.CommandType = CommandType.Text;
            mInsertStrategyCommand.CommandText = "Insert into LmpStrategy (StrategyName, UserName, MarketKey, NodeKey, CurrentDate, CompareDate, HourType) " +
                                                 "Values (@StrategyName, @UserName, @MarketKey, @NodeKey, @CurrentDate, @CompareDate, @HourType)";
            mInsertStrategyCommand.Parameters.AddWithValue("@StrategyName", "StrategyName");
            mInsertStrategyCommand.Parameters.AddWithValue("@UserName", "UserName");
            mInsertStrategyCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mInsertStrategyCommand.Parameters.AddWithValue("@NodeKey", "NodeKey");
            mInsertStrategyCommand.Parameters.AddWithValue("@CurrentDate", "CurrentDate");
            mInsertStrategyCommand.Parameters.AddWithValue("@CompareDate", "CompareDate");
            mInsertStrategyCommand.Parameters.AddWithValue("@HourType", "HourType");
            mInsertStrategyCommand.Connection = VayuConnection;
            //
            mSelectStrategyCommand = new SqlCommand();
            mSelectStrategyCommand.CommandType = CommandType.Text;
            mSelectStrategyCommand.CommandText = "Select LmpStrategyId, StrategyName, UserName, MarketKey, NodeKey, CurrentDate, CompareDate, HourType From LmpStrategy (nolock) Where UserName = @UserName ";
            mSelectStrategyCommand.Parameters.AddWithValue("@UserName", "UserName");
            mSelectStrategyCommand.Connection = VayuConnection;
            //
            mDeleteStrategyCommand = new SqlCommand();
            mDeleteStrategyCommand.CommandType = CommandType.Text;
            mDeleteStrategyCommand.CommandText = "Delete From LmpStrategy Where UserName = @UserName And LmpStrategyId = @LmpStrategyId";
            mDeleteStrategyCommand.Parameters.AddWithValue("@UserName", "UserName");
            mDeleteStrategyCommand.Parameters.AddWithValue("@LmpStrategyId", "LmpStrategyId");
            mDeleteStrategyCommand.Connection = VayuConnection;
        }

        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetMarket(Action<List<MarketNode>, Exception> callback)
        {
            List<MarketNode> marketList = new List<MarketNode>();

            SqlDataReader reader = null;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            reader = mSelectMarketCommand.ExecuteReader();
            while (reader.Read())
            {
                MarketNode node = new MarketNode();
                node.MarketKey = int.Parse(reader[0].ToString());
                node.MarketName = reader.GetString(1);
                marketList.Add(node);
            }
            reader.Close();
            VayuConnection.Close();
            callback(marketList, null);
        }

        /// <summary>
        /// Gets the market node.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketKey">The market key.</param>
        public void GetMarketNode(Action<List<MarketNode>, Exception> callback, int MarketKey)
        {
            List<MarketNode> marketNodeList = new List<MarketNode>();
            SqlDataReader reader = null;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectMarketNodeCommand.Parameters["@MarketKey"].Value = MarketKey;
            reader = mSelectMarketNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                MarketNode node = new MarketNode();
                node.MarketKey = int.Parse(reader[0].ToString());
                node.MarketName = reader.GetString(1);
                node.NodeKey = int.Parse(reader[2].ToString());
                node.NodeName = reader.GetString(3);
                node.TempPrice = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                marketNodeList.Add(node);
            }
            reader.Close();
            VayuConnection.Close();
            callback(marketNodeList, null);
        }

        /// <summary>
        /// Gets the rt price.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="Date">The date.</param>
        /// <param name="Market">The market.</param>
        /// <param name="Hub">The hub.</param>
        public void GetRTPrice(Action<Dictionary<int, double>, Exception> callback, DateTime Date, int Market, int Hub)
        {
            Dictionary<int, double> RTHourHash = new Dictionary<int, double>();
            SqlDataReader reader = null;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            switch (Market)
            {

                case 9:
                    mSelectERCOTRTCommand.Parameters["@nodekey"].Value = Hub.ToString();
                    mSelectERCOTRTCommand.Parameters["@start"].Value = Date.AddMinutes(2);
                    mSelectERCOTRTCommand.Parameters["@end"].Value = Date.AddDays(1);  //marketDateTime.AddDays(1);
                    reader = mSelectERCOTRTCommand.ExecuteReader();
                    break;
            }
            while (reader.Read())
            {
                DateTime date = reader.GetDateTime(0);
                double lmp = (double)reader.GetDecimal(1);
                int hour = date.Hour == 0 ? 24 : date.Hour;

                RTHourHash.Add(hour, lmp);
            }
            reader.Close();
            VayuConnection.Close();
            callback(RTHourHash, null);
        }

        /// <summary>
        /// Gets the da price.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="Date">The date.</param>
        /// <param name="Market">The market.</param>
        /// <param name="Hub">The hub.</param>
        public void GetDAPrice(Action<Dictionary<int, double>, Exception> callback, DateTime Date, int Market, int Hub)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            Dictionary<int, double> DaHourHash = new Dictionary<int, double>();
            double daSum = 0;
            double daPeakSum = 0;
            double daOffPeakSum = 0;
            SqlDataReader reader = null;
            TimeZone zone = TimeZone.CurrentTimeZone; //DaylightChanges 
            DaylightTime time = zone.GetDaylightChanges(DateTime.Today.Year);//DaylightChanges
            string timediff = "01:00:00";//DaylightChanges
            bool mIsDST = zone.IsDaylightSavingTime(DateTime.Today.AddDays(1).AddHours(7));
            int lower = 7, upper = 23;
            if (!mIsDST)
            {
                lower = 8;
                upper = 24;
            }
            switch (Market)
            {
                case 9:
                    mSelectERCOTDACommand.Parameters["@nodekey"].Value = Hub.ToString();
                    mSelectERCOTDACommand.Parameters["@start"].Value = Date.AddMinutes(2);
                    mSelectERCOTDACommand.Parameters["@end"].Value = Date.AddDays(1);  //marketDateTime.AddDays(1);
                    reader = mSelectERCOTDACommand.ExecuteReader();
                    break;
            }

            while (reader.Read())
            {
                DateTime date = reader.GetDateTime(0);
                double lmp = (double)reader.GetDecimal(1);
                int hour = date.Hour == 0 ? 24 : date.Hour;
                if (Market == 1 || Market == 7)
                {
                    if (hour < 8 || hour == 24)
                    {
                        daOffPeakSum += lmp;
                    }
                    else
                    {
                        daPeakSum += lmp;
                    }
                }
                else if ((Convert.ToString(time.Delta) == timediff) && Market == 25463)
                {
                    if (hour <= lower || hour > upper)
                    {
                        daOffPeakSum += lmp;
                    }
                    else
                    {
                        daPeakSum += lmp;
                    }
                }
                else
                {
                    if (hour < 7 || hour > 22)
                    {
                        daOffPeakSum += lmp;
                    }
                    else
                    {
                        daPeakSum += lmp;
                    }
                }
                DaHourHash.Add(hour, lmp);
                daSum += lmp;
            }
            reader.Close();
            VayuConnection.Close();
            DaHourHash.Add(100, daSum / 24);
            DaHourHash.Add(200, daPeakSum / 16);
            DaHourHash.Add(300, daOffPeakSum / 8);
            callback(DaHourHash, null);
        }

        /// <summary>
        /// Inserts the strategy.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        public void InsertStrategy(Strategy strategy)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            try
            {
                mInsertStrategyCommand.Parameters["@StrategyName"].Value = strategy.StrategyName;
                mInsertStrategyCommand.Parameters["@UserName"].Value = strategy.UserName;
                mInsertStrategyCommand.Parameters["@MarketKey"].Value = strategy.MarketKey;
                mInsertStrategyCommand.Parameters["@NodeKey"].Value = strategy.NodeKey;
                mInsertStrategyCommand.Parameters["@CurrentDate"].Value = strategy.CurrentDate;
                mInsertStrategyCommand.Parameters["@CompareDate"].Value = strategy.CompareDate;
                mInsertStrategyCommand.Parameters["@HourType"].Value = strategy.HourType;
                mInsertStrategyCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
            }
            finally
            {
                VayuConnection.Close();
            }

        }

        /// <summary>
        /// Gets the strategy.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="username">The username.</param>
        public void GetStrategy(Action<List<Strategy>, Exception> callback, string username)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectStrategyCommand.Parameters["@UserName"].Value = username;
            SqlDataReader reader = mSelectStrategyCommand.ExecuteReader();
            List<Strategy> strategyList = new List<Strategy>();
            while (reader.Read())
            {
                strategyList.Add(new Strategy()
                {
                    NodePriceStrategyId = int.Parse(reader[0].ToString()),
                    StrategyName = reader[1].ToString(),
                    UserName = reader[2].ToString(),
                    MarketKey = int.Parse(reader[3].ToString()),
                    NodeKey = int.Parse(reader[4].ToString()),
                    CurrentDate = DateTime.Parse(reader[5].ToString()),
                    CompareDate = DateTime.Parse(reader[6].ToString()),
                    HourType = char.Parse(reader[7].ToString())
                });
            }
            VayuConnection.Close();
            callback(strategyList, null);
        }

        /// <summary>
        /// Deletes the strategy.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="NodePriceStrategyId">The node price strategy identifier.</param>
        public void DeleteStrategy(string username, int NodePriceStrategyId)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            try
            {
                mDeleteStrategyCommand.Parameters["@UserName"].Value = username;
                mDeleteStrategyCommand.Parameters["@LmpStrategyId"].Value = NodePriceStrategyId;
                mDeleteStrategyCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
            }
            finally
            {
                VayuConnection.Close();
            }
        }

        #endregion
    }
}
