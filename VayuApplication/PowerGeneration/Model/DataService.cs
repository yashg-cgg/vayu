using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.WindServiceLibrary;

namespace Vayu.PowerGeneration.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.WindForecast.Model.IDataService" />
    public class DataService : IDataService
    {
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        public static SqlConnection VayuConnection;
        /// <summary>
        /// The select wind load rt data
        /// </summary>
        SqlCommand mSelectWindLoadRTData;
        SqlCommand mSelectSolarLoadRTData;
        /// <summary>
        /// The select wind forecast rt data
        /// </summary>
        SqlCommand mSelectWindForecastRTData;
        SqlCommand mSelectSolarForecastData;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        public DataService()
        {
            loadDBCommands();
        }

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectWindLoadRTData = VayuConnection.CreateCommand();
            mSelectWindLoadRTData.CommandText = "SELECT Value, MarketDateTime FROM Wind WHERE MarketDateTime BETWEEN @StartDate AND @EndDate";
            mSelectWindLoadRTData.Parameters.AddWithValue("@StartDate", "MarketDateTime");
            mSelectWindLoadRTData.Parameters.AddWithValue("@EndDate", "MarketDateTime");

            mSelectWindForecastRTData = VayuConnection.CreateCommand();
            mSelectWindForecastRTData.CommandText = "SELECT Value, MarketDateTime FROM WindForecast WHERE MarketDateTime BETWEEN @StartDate AND @EndDate";
            mSelectWindForecastRTData.Parameters.AddWithValue("@StartDate", "MarketDateTime");
            mSelectWindForecastRTData.Parameters.AddWithValue("@EndDate", "MarketDateTime");

            mSelectSolarLoadRTData = VayuConnection.CreateCommand();
            mSelectSolarLoadRTData.CommandText = "Select MarketDateTime,Hour,RealTimevalue from Vayu..SolarPowerGenerationValue where MarketDateTime >= @StartDate and MarketDateTime< @EndDate ";
            mSelectSolarLoadRTData.Parameters.AddWithValue("@StartDate", "MarketDateTime");
            mSelectSolarLoadRTData.Parameters.AddWithValue("@EndDate", "MarketDateTime");
            mSelectSolarLoadRTData.Connection = VayuConnection;

            mSelectSolarForecastData = VayuConnection.CreateCommand();
            mSelectSolarForecastData.CommandText = "Select MarketDateTime,Hour,Forecastvalue from Vayu..SolarPowerGenerationValue where MarketDateTime >= @StartDate and MarketDateTime< @EndDate";
            mSelectSolarForecastData.Parameters.AddWithValue("@StartDate", "MarketDateTime");
            mSelectSolarForecastData.Parameters.AddWithValue("@EndDate", "MarketDateTime");
            mSelectSolarForecastData.Connection = VayuConnection;
        }

        /// <summary>
        /// Gets the wind data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        public void GetWindData(Action<Dictionary<string, List<WindData>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone)
        {
            try
            {
                Dictionary<string, List<WindData>> WindDataList = new Dictionary<string, List<WindData>>();

                WindDataList.Add("Actual", GetLoadRT(fromDate, toDate));
                WindDataList.Add("Forecast", GetForecastRT(fromDate, toDate));

                callback(WindDataList, null);
            }
            catch
            { }
        }

        public void GetSolarData(Action<List<LatestWindData>, Exception> callback, DateTime fromDate, DateTime toDate, string zone)
        {
            List<LatestWindData> SolarDataList = new List<LatestWindData>();
            SolarDataList = GetSolarRT(fromDate, toDate);
            // SolarDataList.Add("Forecast", GetSolarForecast(fromDate, toDate));
            callback(SolarDataList, null);
        }
        /// <summary>
        /// Gets the load rt.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        private List<WindData> GetLoadRT(DateTime startDate, DateTime endDate)
        {
            List<WindData> lstWindDataRT = new List<WindData>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = mSelectWindLoadRTData.CommandText;
                    cmd.Parameters.Add("@startDate", startDate);
                    cmd.Parameters.Add("@endDate", endDate);
                    //con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lstWindDataRT.Add(new WindData
                        {
                            MarketDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            Value = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1))
                        });
                    }
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
            }

            return lstWindDataRT;
        }

        private List<LatestWindData> GetSolarRT(DateTime startDate, DateTime endDate)
        {
            List<LatestWindData> lstSolarDataRT = new List<LatestWindData>();
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = mSelectSolarLoadRTData.CommandText;
                        cmd.Parameters.Add("startDate", startDate);
                        cmd.Parameters.Add("endDate", endDate);
                        cmd.Connection = con;
                        //con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime date = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                            int hour = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            lstSolarDataRT.Add(new LatestWindData
                            {
                                WindType = "Actual",
                                MarketDate = date.AddHours(hour),
                                Value = reader.IsDBNull(2) ? 0.0 : Convert.ToDouble(reader.GetValue(2))
                            });
                        }
                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = mSelectSolarForecastData.CommandText;
                        cmd.Parameters.Add("startDate", startDate);
                        cmd.Parameters.Add("endDate", endDate);
                        cmd.Connection = con;
                        //con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime date = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                            int hour = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            lstSolarDataRT.Add(new LatestWindData
                            {
                                WindType = "Forecast",
                                MarketDate = date.AddHours(hour),
                                Value = reader.IsDBNull(2) ? 0.0 : Convert.ToDouble(reader.GetValue(2))
                            });
                        }
                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }
            }
            catch
            {

            }
            return lstSolarDataRT;
        }
        private List<LatestWindData> GetSolarForecast(DateTime startDate, DateTime endDate)
        {
            List<LatestWindData> lstSolarDataForecast = new List<LatestWindData>();
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = mSelectSolarForecastData.CommandText;
                        cmd.Parameters.Add("startDate", startDate);
                        cmd.Parameters.Add("endDate", endDate);
                        //con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime date = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                            int hour = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            lstSolarDataForecast.Add(new LatestWindData
                            {
                                MarketDate = date.AddHours(hour - 1),
                                Value = reader.IsDBNull(1) ? 0.0 : reader.GetDouble(1)
                            });
                        }
                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }
            }
            catch
            {

            }
            return lstSolarDataForecast;
        }
        /// <summary>
        /// Gets the forecast rt.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        private List<WindData> GetForecastRT(DateTime startDate, DateTime endDate)
        {
            List<WindData> lstWindForecastRT = new List<WindData>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = mSelectWindForecastRTData.CommandText;
                    cmd.Parameters.Add("@startDate", startDate);
                    cmd.Parameters.Add("@endDate", endDate);
                    //con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lstWindForecastRT.Add(new WindData
                        {
                            MarketDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            Value = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1))
                        });
                    }
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
            }

            return lstWindForecastRT;
        }


    }
}
