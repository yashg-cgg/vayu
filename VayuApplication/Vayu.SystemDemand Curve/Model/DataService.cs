using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.SystemDemand_Curve.Model
{
    public class DataService : IDataService
    {
        #region Declaration 
        /// <summary>
        /// The load hash
        /// </summary>
        private static Dictionary<string, Vayu.LoadGraphLibrary.Load> sLoadHash = new Dictionary<string, Vayu.LoadGraphLibrary.Load>();

        #region SQL Commands

        /// <summary>
        /// The select zone command
        /// </summary>
        private static SqlCommand mSelectZoneCommand;
        //private static SqlCommand mSelectTeslaZoneCommand;
        /// <summary>
        /// The select PRT loads command
        /// </summary>
        private static SqlCommand mSelectPRTLoadsCommand;
        //private static SqlCommand mSelectTeslaLoadsCommand;
        /// <summary>
        /// The select da zone command
        /// </summary>
        private static SqlCommand mSelectDAZoneCommand;
        /// <summary>
        /// The m select iso zone command
        /// </summary>
        private static SqlCommand mSelectISOZoneCommand;
        /// <summary>
        /// The m select load reference command
        /// </summary>
        private static SqlCommand mSelectLoadRefCommand;
        /// <summary>
        /// The m connection tesla
        /// </summary>
        private static SqlConnection VayuConnection;
        /// <summary>
        /// The m select wsi command
        /// </summary>
        private static SqlCommand mSelectWsiCommand;
        /// <summary>
        /// The m select day ahead loads command
        /// </summary>
        private static SqlCommand mSelectDayAheadLoadsCommand;

        /// <summary>
        /// The m select day ahead loads command for Ercot
        /// </summary>
        private static SqlCommand mSelectErcotDayAheadLoadsCommand;

        /// <summary>
        /// The m select load forecasts command
        /// </summary>
        private static SqlCommand mSelectLoadForecastsCommand;
        //
        /// <summary>
        /// The m select load forecasts command for Ercot
        /// </summary>
        private static SqlCommand mSelectErcotLoadForecastsCommand;
        /// <summary>
        /// The m select load forecasts highest MW of the Day command for Ercot
        /// </summary>
        private static SqlCommand mSelectErcotLoadForecastsDayHighCommand;

        /// <summary>
        /// The m select rt loads command
        /// </summary>
        private static SqlCommand mSelectRTLoadsCommand;
        /// <summary>
        /// The m select rt loads command for Ercot
        /// </summary>
        private static SqlCommand mSelectErcotRTLoadsCommand;

        /// <summary>
        /// The m select genscape command
        /// </summary>
        private static SqlCommand mSelectGenscapeCommand;
        /// <summary>
        /// The m select PRT frozen loads command
        /// </summary>
        private static SqlCommand mSelectPRTFrozenLoadsCommand;
        //private static SqlCommand mSelectTeslaFrozenLoadsCommand;
        /// <summary>
        /// The m select wsi frozen loads command
        /// </summary>
        private static SqlCommand mSelectWSIFrozenLoadsCommand;
        /// <summary>
        /// The m select DTN frozen loads command
        /// </summary>
        private static SqlCommand mSelectDTNFrozenLoadsCommand;
        /// <summary>
        /// The m select iso frozen loads command
        /// </summary>
        private static SqlCommand mSelectISOFrozenLoadsCommand;

        private static SqlCommand mSelectISOFrozenUpdateTimeCommand;
        private static SqlCommand NetLoadCommand;
        private static SqlCommand FrozenNetLoadCommand;
        private static SqlCommand mSelectHourlyCommand;
        private static SqlCommand mSelectRTUpdateTimeCommand;
        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        public DataService()
        {
            loadDBCommands();
            FillLoadHash();
        }

        /// <summary>
        /// Gets the load names.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetLoadNames(Action<List<string>, Exception> callback)
        {
            loadDBCommands();
            try
            {
                if (sLoadHash.Count == 0)
                    FillLoadHash();
                sLoadHash.Remove("ERCOT East");
                sLoadHash.Remove("ERCOT Far West");
                sLoadHash.Remove("ERCOT South Central");
                sLoadHash.Remove("ERCOT North Central");
                callback(sLoadHash.Keys.OrderBy(a => a).ToList(), null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        /// <summary>
        /// Gets the load data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        public void GetLoadData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone)
        {
            loadDBCommands();
            DateTime NLDate = DateTime.Now.AddHours(-1);
            if (toDate <= NLDate)
            {
                NLDate = toDate;
            }
            int mKey = GetMarketKey(zone);
            if (mKey > 0)
            {
                if (true)
                {
                    try
                    {
                        Dictionary<string, List<LoadDataItem>> itemList = new Dictionary<string, List<LoadDataItem>>();
                        itemList.Add("DA", GetDALoads(fromDate, toDate, zone, mKey));
                        itemList.Add("ISO", GetISOLoads(fromDate, toDate, zone, mKey));
                        itemList.Add("CURRENT", GetHISOLoads(fromDate, toDate, zone, mKey));
                        itemList.Add("PEAK LOAD OF DAY", GetDayHighISOLoads(fromDate, toDate, zone, mKey));
                        // itemList.Add("Net Load", GetNetLoadData(fromDate, toDate, DateTime.Now.AddHours(-1), zone, mKey));
                        itemList.Add("Net Load", GetNetLoadData(fromDate, toDate, NLDate, zone, mKey));
                        itemList.Add("Frozen Net Load", GetFrozenNetLoadData(fromDate, toDate, mKey));
                        callback(itemList, null);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }


        //public void GetDayhighLoadData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone)
        //{
        //    loadDBCommands();
        //    int mKey = GetMarketKey(zone);
        //    if (mKey > 0)
        //    {
        //        if (true)
        //        {
        //            try
        //            {
        //                Dictionary<string, List<LoadDataItem>> itemList = new Dictionary<string, List<LoadDataItem>>();
        //                itemList.Add("PEAK LOAD OF DAY", GetDayHighISOLoads(fromDate, toDate, zone, mKey));
        //                callback(itemList, null);

        //            }
        //            catch (Exception ex)
        //            {

        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// Gets the frozen data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="FrozenTime">The frozen time.</param>
        public void GetFrozenData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone, int FrozenTime)
        {
            loadDBCommands();
            int mKey = GetMarketKey(zone);
            if (mKey > 0)
            {

                try
                {
                    callback(GetTotalLoads(fromDate, toDate, zone, mKey, FrozenTime), null);
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Private Methods



        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private static void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectZoneCommand = VayuConnection.CreateCommand();
            mSelectZoneCommand.CommandText = "select LoadsName, loadForecastTypeKey, loadskey, l.marketkey, case when mk.Label = 'ERCOTTesting' then 'ERCOT' else mk.Label end as MarketLabel  from Loads l inner join Market mk on l.MarketKey = mk.MarketKey where l.MarketKey in (9) order by l.marketkey, loadsname ";

            mSelectDAZoneCommand = VayuConnection.CreateCommand();
            mSelectDAZoneCommand.CommandText = "select distinct Name,marketkey from DayAheadLoad";

            //
            mSelectISOZoneCommand = VayuConnection.CreateCommand();
            mSelectISOZoneCommand.CommandText = "select distinct l.LoadsName,l.MarketKey from LoadForecasts f join Loads l on l.LoadForecastTypeKey=f.LoadForecastTypeKey";

            mSelectLoadRefCommand = new SqlCommand();
            mSelectLoadRefCommand.CommandText = "select LoadName from LoadXRefType where XRefName = @XRefName and LoadsKey = @LoadsKey";
            mSelectLoadRefCommand.Parameters.AddWithValue("@LoadsKey", "LoadsKey");
            mSelectLoadRefCommand.Parameters.AddWithValue("@XRefName", "XRefName");

            mSelectWsiCommand = VayuConnection.CreateCommand();
            mSelectWsiCommand.CommandText = "select date,time, load_fcst from  @loadTable  where date between @fromdate and @enddate Order By date, time";
            mSelectWsiCommand.Parameters.AddWithValue("@date", "date");

            mSelectDayAheadLoadsCommand = VayuConnection.CreateCommand();
            mSelectDayAheadLoadsCommand.CommandText = "select marketdateTime, value from dayaheadload where marketdatetime >= @STARTDATE and name = @name and marketdatetime < @ENDDATE  order by marketdatetime";
            mSelectDayAheadLoadsCommand.Parameters.AddWithValue("@startdate", "MARKETDATETIME");
            mSelectDayAheadLoadsCommand.Parameters.AddWithValue("@enddate", "MARKETDATETIME");
            mSelectDayAheadLoadsCommand.Parameters.AddWithValue("@name", "name");
            // Ercot
            mSelectErcotDayAheadLoadsCommand = VayuConnection.CreateCommand();
            mSelectErcotDayAheadLoadsCommand.CommandText = "select marketdateTime, value from Vayu..dayaheadload where marketdatetime >= @STARTDATE and name = @name and marketdatetime < @ENDDATE  order by marketdatetime";
            mSelectErcotDayAheadLoadsCommand.Parameters.AddWithValue("@startdate", "MARKETDATETIME");
            mSelectErcotDayAheadLoadsCommand.Parameters.AddWithValue("@enddate", "MARKETDATETIME");
            mSelectErcotDayAheadLoadsCommand.Parameters.AddWithValue("@name", "name");
            //

            mSelectLoadForecastsCommand = VayuConnection.CreateCommand();
            mSelectLoadForecastsCommand.CommandText = "select MarketDateTime,MW from LoadForecasts where LoadForecastTypeKey= @loadforecasttypekey and MarketDateTime between @startDate and @endDate order by MarketDateTime";
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@startDate", "");
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@endDate", "");
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@loadforecasttypekey", "");

            //Ercot
            mSelectErcotLoadForecastsCommand = VayuConnection.CreateCommand();
            mSelectErcotLoadForecastsCommand.CommandText = "select MarketDateTime,MW from Vayu..LoadForecasts where LoadForecastTypeKey= @loadforecasttypekey and MarketDateTime between @startDate and @endDate order by MarketDateTime";
            mSelectErcotLoadForecastsCommand.Parameters.AddWithValue("@startDate", "");
            mSelectErcotLoadForecastsCommand.Parameters.AddWithValue("@endDate", "");
            mSelectErcotLoadForecastsCommand.Parameters.AddWithValue("@loadforecasttypekey", "");

            mSelectErcotLoadForecastsDayHighCommand = VayuConnection.CreateCommand();
            mSelectErcotLoadForecastsDayHighCommand.CommandText = "WITH RankedLoadForecasts AS (SELECT MarketDateTime, MW, ROW_NUMBER() OVER (PARTITION BY CONVERT(DATE, MarketDateTime) ORDER BY MW DESC) AS RowNum FROM Vayu..LoadForecasts WHERE LoadForecastTypeKey = @loadforecasttypekey AND MarketDateTime >= @startDate AND MarketDateTime < DATEADD(DAY, DATEDIFF(DAY, 0, CAST(@endDate AS DATETIME)), 0)) SELECT MarketDateTime, MW FROM RankedLoadForecasts WHERE RowNum = 1 ORDER BY MarketDateTime;";
            mSelectErcotLoadForecastsDayHighCommand.Parameters.AddWithValue("@startDate", "");
            mSelectErcotLoadForecastsDayHighCommand.Parameters.AddWithValue("@endDate", "");
            mSelectErcotLoadForecastsDayHighCommand.Parameters.AddWithValue("@loadforecasttypekey", "");
            // mSelectErcotLoadForecastsDayHighCommand
            mSelectRTLoadsCommand = VayuConnection.CreateCommand();
            mSelectRTLoadsCommand.CommandText = "select MarketDateTime,MW from LoadRT where MarketDateTime between @startdate and @enddate and LoadsKey=@loadskey order by MarketDateTime ";
            mSelectRTLoadsCommand.Parameters.AddWithValue("@startdate", "");
            mSelectRTLoadsCommand.Parameters.AddWithValue("@enddate", "");
            mSelectRTLoadsCommand.Parameters.AddWithValue("@loadskey", "");
            //Ercot

            mSelectErcotRTLoadsCommand = VayuConnection.CreateCommand();
            mSelectErcotRTLoadsCommand.CommandText = "select MarketDateTime,MW from Vayu..LoadRT where MarketDateTime between @startdate and @enddate and LoadsKey=@loadskey order by MarketDateTime ";
            mSelectErcotRTLoadsCommand.Parameters.AddWithValue("@startdate", "");
            mSelectErcotRTLoadsCommand.Parameters.AddWithValue("@enddate", "");
            mSelectErcotRTLoadsCommand.Parameters.AddWithValue("@loadskey", "");
            //
            mSelectGenscapeCommand = VayuConnection.CreateCommand();
            mSelectGenscapeCommand.CommandText = "select MarketDateTime,MW from GenscapeForecasts where MarketDateTime between @startdate and @enddate and GenscapeForecastTypeKey=@GenscapeForecastTypeKey order by MarketDateTime ";
            mSelectGenscapeCommand.Parameters.AddWithValue("@startdate", "");
            mSelectGenscapeCommand.Parameters.AddWithValue("@enddate", "");
            mSelectGenscapeCommand.Parameters.AddWithValue("@GenscapeForecastTypeKey", "");

            mSelectHourlyCommand = VayuConnection.CreateCommand();
            mSelectHourlyCommand.CommandText = "select MarketDateTime,MW from LoadRTh where MarketDateTime between @startdate and @enddate and LoadsKey=@loadskey order by MarketDateTime ";
            mSelectHourlyCommand.Parameters.AddWithValue("@startdate", "");
            mSelectHourlyCommand.Parameters.AddWithValue("@enddate", "");
            mSelectHourlyCommand.Parameters.AddWithValue("@loadskey", "");

            mSelectPRTFrozenLoadsCommand = VayuConnection.CreateCommand();
            mSelectPRTFrozenLoadsCommand.CommandText = "select MarketDate,PRTLoad from ZonalFrozenLoads where MarketDate between @StartMarketDate and @EndMarketDate and ZoneName = @ZoneName and MarketKey=@MarketKey and FrozenHour=@FrozenHour Order by MarketDate";
            mSelectPRTFrozenLoadsCommand.Parameters.AddWithValue("@ZoneName", "ZoneName");
            mSelectPRTFrozenLoadsCommand.Parameters.AddWithValue("@StartMarketDate", "MarketDate");
            mSelectPRTFrozenLoadsCommand.Parameters.AddWithValue("@EndMarketDate", "MarketDate");
            mSelectPRTFrozenLoadsCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectPRTFrozenLoadsCommand.Parameters.AddWithValue("@FrozenHour", "FrozenHour");

            mSelectWSIFrozenLoadsCommand = VayuConnection.CreateCommand();
            mSelectWSIFrozenLoadsCommand.CommandText = "select MarketDate,WSILoad from ZonalFrozenLoads where MarketDate between @StartMarketDate and @EndMarketDate and ZoneName = @ZoneName and MarketKey=@MarketKey and FrozenHour=@FrozenHour Order by MarketDate";
            mSelectWSIFrozenLoadsCommand.Parameters.AddWithValue("@ZoneName", "ZoneName");
            mSelectWSIFrozenLoadsCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectWSIFrozenLoadsCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectWSIFrozenLoadsCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectWSIFrozenLoadsCommand.Parameters.AddWithValue("@FrozenHour", "FrozenHour");

            mSelectDTNFrozenLoadsCommand = VayuConnection.CreateCommand();
            mSelectDTNFrozenLoadsCommand.CommandText = "select MarketDate,DTNLoad from ZonalFrozenLoads where MarketDate between @StartMarketDate and @EndMarketDate and ZoneName = @ZoneName and MarketKey=@MarketKey and FrozenHour=@FrozenHour Order by MarketDate";
            mSelectDTNFrozenLoadsCommand.Parameters.AddWithValue("@ZoneName", "ZoneName");
            mSelectDTNFrozenLoadsCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectDTNFrozenLoadsCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectDTNFrozenLoadsCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectDTNFrozenLoadsCommand.Parameters.AddWithValue("@FrozenHour", "FrozenHour");

            mSelectISOFrozenLoadsCommand = VayuConnection.CreateCommand();
            mSelectISOFrozenLoadsCommand.CommandText = "select MarketDate,ISOLoad from ZonalFrozenLoads where MarketDate between @StartMarketDate and @EndMarketDate and ZoneName = @ZoneName and MarketKey=@MarketKey and FrozenHour=@FrozenHour Order by MarketDate";
            mSelectISOFrozenLoadsCommand.Parameters.AddWithValue("@ZoneName", "ZoneName");
            mSelectISOFrozenLoadsCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectISOFrozenLoadsCommand.Parameters.AddWithValue("@MarketDate", "MarketDate");
            mSelectISOFrozenLoadsCommand.Parameters.AddWithValue("@FrozenHour", "FrozenHour");

            mSelectISOFrozenUpdateTimeCommand = VayuConnection.CreateCommand();
            mSelectISOFrozenUpdateTimeCommand.CommandText = "SELECT TOP 1 FreezeDate FROM ZonalFrozenLoads WHERE MarketDate BETWEEN @StartMarketDate AND @EndMarketDate AND ZoneName = @ZoneName AND MarketKey =9 AND MarketDate = (SELECT MAX(MarketDate) FROM ZonalFrozenLoads WHERE MarketDate BETWEEN @StartMarketDate AND @EndMarketDate AND ZoneName = @ZoneName AND MarketKey =9) ORDER BY ISOLoad DESC;";
            mSelectISOFrozenUpdateTimeCommand.Parameters.AddWithValue("@ZoneName", "");
            mSelectISOFrozenUpdateTimeCommand.Parameters.AddWithValue("@StartMarketDate", "");
            mSelectISOFrozenUpdateTimeCommand.Parameters.AddWithValue("@EndMarketDate", "");

            mSelectRTUpdateTimeCommand = VayuConnection.CreateCommand();
            mSelectRTUpdateTimeCommand.CommandText = "SELECT TOP 1 filedatetime from Loadrt order by filedatetime desc ";


            NetLoadCommand = VayuConnection.CreateCommand();
            NetLoadCommand.CommandText = @"
SELECT 
    marketdatetime,net_load
FROM (
    SELECT 
        lr.marketdatetime,
        lr.mw - COALESCE(wp.Realtimevalue, 0) - COALESCE(sp.Realtimevalue, 0) AS net_load
    FROM 
        LoadRT lr
    LEFT JOIN 
        WindPowerGenerationValue wp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lr.marketdatetime AS DATE)) AS DATE)  ELSE CAST(lr.marketdatetime AS DATE) 
            END = CAST(wp.Marketdatetime AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN '24'  ELSE SUBSTRING(CONVERT(VARCHAR, lr.marketdatetime, 120), 12, 2) 
            END = wp.Hour
    LEFT JOIN 
        SolarPowerGenerationValue sp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lr.marketdatetime AS DATE)) AS DATE) ELSE CAST(lr.marketdatetime AS DATE) 
            END = CAST(sp.Marketdatetime AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN '24'  ELSE SUBSTRING(CONVERT(VARCHAR, lr.marketdatetime, 120), 12, 2) 
            END = sp.Hour
    WHERE 
        lr.marketdatetime BETWEEN @startDate AND @forecaststartDate
UNION ALL
    SELECT 
        lf.marketdatetime,
        lf.MW - COALESCE(wp.forecastvalue, 0) - COALESCE(sp.forecastvalue, 0) AS net_load
    FROM 
        loadforecasts lf
    LEFT JOIN 
        WindPowerGenerationValue wp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lf.marketdatetime AS DATE)) AS DATE) ELSE CAST(lf.marketdatetime AS DATE) 
            END = CAST(CONVERT(VARCHAR, wp.Marketdatetime, 120) AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN '24' ELSE SUBSTRING(CONVERT(VARCHAR, lf.marketdatetime, 120), 12, 2) 
            END = wp.Hour
    LEFT JOIN 
        SolarPowerGenerationValue sp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lf.marketdatetime AS DATE)) AS DATE)  ELSE CAST(lf.marketdatetime AS DATE) 
            END = CAST(CONVERT(VARCHAR, sp.Marketdatetime, 120) AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN '24' ELSE SUBSTRING(CONVERT(VARCHAR, lf.marketdatetime, 120), 12, 2) 
            END = sp.Hour
    WHERE 
        lf.loadforecasttypekey = @loadforecasttypekey
        AND lf.marketdatetime BETWEEN @forecaststartDate AND @endDate
) AS CombinedData;
";
            NetLoadCommand.Parameters.AddWithValue("@startDate", "");

            NetLoadCommand.Parameters.AddWithValue("@forecaststartDate", "");
            NetLoadCommand.Parameters.AddWithValue("@endDate", "");
            NetLoadCommand.Parameters.AddWithValue("@loadforecasttypekey", "");

            FrozenNetLoadCommand = VayuConnection.CreateCommand();
            FrozenNetLoadCommand.CommandText = @"SELECT 
    marketdate,net_load
FROM (
    SELECT 
        lr.marketdate,
        lr.ISOLoad - COALESCE(wp.ForecastValue, 0) - COALESCE(sp.ForecastValue, 0) AS net_load
    FROM 
        ZonalFrozenLoads lr
    LEFT JOIN 
        FrozenWindPowerGenerationValue wp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdate, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lr.marketdate AS DATE)) AS DATE)  ELSE CAST(lr.marketdate AS DATE) 
            END = CAST(wp.Marketdatetime AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdate, 120), 8) = '00:00:00' THEN '24'  ELSE SUBSTRING(CONVERT(VARCHAR, lr.marketdate, 120), 12, 2) 
            END = wp.Hour
    LEFT JOIN 
        FrozenSolarPowerGenerationValue sp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdate, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lr.marketdate AS DATE)) AS DATE) ELSE CAST(lr.marketdate AS DATE) 
            END = CAST(sp.Marketdatetime AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdate, 120), 8) = '00:00:00' THEN '24'  ELSE SUBSTRING(CONVERT(VARCHAR, lr.marketdate, 120), 12, 2) 
            END = sp.Hour
    WHERE 
        lr.marketdate BETWEEN @startDate AND @endDate and lr.ZoneName='ERCOT Total'

) AS CombinedData;";
            FrozenNetLoadCommand.Parameters.AddWithValue("@startDate", "");
            FrozenNetLoadCommand.Parameters.AddWithValue("@endDate", "");

        }
        /// <summary>
        /// Gets the da loads.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="mKey">The m key.</param>
        /// <returns></returns>
        private List<LoadDataItem> GetDALoads(DateTime fromDate, DateTime toDate, string zone, int mKey)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            if (sLoadHash[zone].DayAhead != null || sLoadHash[zone].Tesla != null)
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    var load = sLoadHash[zone];
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (mKey == 9)
                        {
                            cmd.CommandText = mSelectErcotDayAheadLoadsCommand.CommandText;
                            cmd.Parameters.Add(new SqlParameter("@name", (load.DayAhead == "Total Load") ? "Cleared Load" : (load.DayAhead == "ERCOT Total") ? "Total" : load.DayAhead));
                        }
                        else
                        {
                            cmd.CommandText = mSelectDayAheadLoadsCommand.CommandText;
                            // cmd.CommandText = mSelectDayAheadLoadsCommand.CommandText;

                            if (load.DayAhead.Contains("GENESE") || load.DayAhead.Contains("DUNWOD") || load.DayAhead.Contains("CAPITL") || load.DayAhead.Contains("CENTRL") || load.DayAhead.Contains("MILLWD") || load.DayAhead.Contains("MHK VL") || load.DayAhead.Contains("N.Y.C.") || load.DayAhead.Contains("HUD VL") || load.DayAhead.Contains("NORTH") || load.DayAhead.Contains("WEST") || load.DayAhead.Contains("LONGIL"))
                            {
                                load.DayAhead = load.DayAhead.Replace(" Zone", "");
                            }
                            cmd.Parameters.Add(new SqlParameter("@name", load.DayAhead == "Total Load" ? "Cleared Load" : load.DayAhead));
                        }

                        cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                        cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                        //if (con.State == ConnectionState.Closed)
                        //{
                        //    con.Open(); 
                        //}
                        IDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            itemList.Add(new LoadDataItem
                            {
                                MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                                LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                            });
                        }
                        if (!reader.IsClosed)
                            reader.Close();
                    }
                }
            }
            return itemList;
        }

        /// <summary>
        /// Gets the iso loads.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="mKey">The m key.</param>
        /// <returns></returns>
        private List<LoadDataItem> GetISOLoads(DateTime fromDate, DateTime toDate, string zone, int mKey)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    if (mKey == 9)
                    {
                        cmd.CommandText = mSelectErcotLoadForecastsCommand.CommandText;
                    }
                    else
                    {
                        cmd.CommandText = mSelectLoadForecastsCommand.CommandText;
                    }

                    cmd.Parameters.Add(new SqlParameter("@loadforecasttypekey", sLoadHash[zone].Forecast));
                    cmd.Parameters.Add(new SqlParameter("@startDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", toDate));
                    cmd.CommandTimeout = 3 * 10 * 1000;
                    //if (con.State == ConnectionState.Closed)
                    //{
                    //    con.Open();
                    //}
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
            return itemList;
        }

        public DateTime GetFrozenUpdateTimeLoads(DateTime fromDate, DateTime toDate, string zone)
        {
            DateTime item = DateTime.MinValue; // Initialize with a default value
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {

                    cmd.CommandText = mSelectISOFrozenUpdateTimeCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@StartMarketDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@EndMarketDate", toDate));
                    cmd.Parameters.Add(new SqlParameter("@ZoneName", zone));

                    cmd.CommandTimeout = 3 * 10 * 1000;
                    //if (con.State == ConnectionState.Closed)
                    //{
                    //    con.Open();
                    //}
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        item = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0);
                    }
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
            return item;
        }
        public DateTime GetRTUpdateTimeLoads()
        {
            DateTime item = DateTime.MinValue;
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = mSelectRTUpdateTimeCommand.CommandText;
                    cmd.CommandTimeout = 3 * 10 * 1000;
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        item = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0);
                    }
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
            return item;
        }

        private List<LoadDataItem> GetDayHighISOLoads(DateTime fromDate, DateTime toDate, string zone, int mKey)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    if (mKey == 9)
                    {
                        cmd.CommandText = mSelectErcotLoadForecastsDayHighCommand.CommandText;
                    }
                    //else { cmd.CommandText = mSelectLoadForecastsCommand.CommandText; }

                    cmd.Parameters.Add(new SqlParameter("@loadforecasttypekey", sLoadHash[zone].Forecast));
                    cmd.Parameters.Add(new SqlParameter("@startDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", toDate));
                    cmd.CommandTimeout = 3 * 10 * 1000;
                    //if (con.State == ConnectionState.Closed)
                    //{
                    //    con.Open();
                    //}
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
            return itemList;
        }

        private List<LoadDataItem> GetNetLoadData(DateTime fromDate, DateTime toDate, DateTime forcastfromDate, string zone, int key)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            using (SqlConnection connection = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    if (key == 9)
                    {
                        cmd.CommandText = NetLoadCommand.CommandText;
                    }
                    cmd.Parameters.Add(new SqlParameter("@loadforecasttypekey", sLoadHash[zone].Forecast));
                    cmd.Parameters.Add(new SqlParameter("@startDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@forecaststartDate", forcastfromDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", toDate));
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1))
                        });
                    }
                    reader.Close();
                }

            }
            return itemList;
        }

        private List<LoadDataItem> GetFrozenNetLoadData(DateTime fromDate, DateTime toDate, int key)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            using (SqlConnection connection = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    if (key == 9)
                    {
                        cmd.CommandText = FrozenNetLoadCommand.CommandText;
                    }

                    cmd.Parameters.Add(new SqlParameter("@startDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", toDate));
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1))
                        });
                    }
                    reader.Close();
                }

            }
            return itemList;

        }



        /// <summary>
        /// Gets the hiso loads.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="mKey">The m key.</param>
        /// <returns></returns>
        private static List<LoadDataItem> GetHISOLoads(DateTime fromDate, DateTime toDate, string zone, int mKey)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    if (mKey == 9)
                    {
                        cmd.CommandText = mSelectErcotRTLoadsCommand.CommandText;
                    }
                    else { cmd.CommandText = mSelectRTLoadsCommand.CommandText; }
                    //cmd.CommandText = mSelectRTLoadsCommand.CommandText;
                    cmd.Parameters.Add(new SqlParameter("@startDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", toDate));// DateTime.Today));
                    cmd.Parameters.Add(new SqlParameter("@loadskey", sLoadHash[zone].Current));
                    //if (con.State == ConnectionState.Closed)
                    //{
                    //    con.Open();
                    //}
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
            return itemList;
        }

        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <returns></returns>
        private int GetMarketKey(string zone)
        {
            if (zone == null || zone == string.Empty)
                return -1;
            else
            {
                string[] zoneArray = zone.Split(' ');
                return 9;
            }
        }

        /// <summary>
        /// Fills the load hash.
        /// </summary>
        private void FillLoadHash()
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlDataReader reader = mSelectZoneCommand.ExecuteReader();
            while (reader.Read())
            {
                string zone = reader.GetString(0).Trim();
                string market = reader.GetString(4);
                if (!zone.StartsWith("PJM"))
                {
                    zone = market + " " + zone;
                }
                Vayu.LoadGraphLibrary.Load load = new Vayu.LoadGraphLibrary.Load();
                //load.Tesla = "";
                if (!reader.IsDBNull(1))
                {
                    load.Forecast = (int)reader.GetDecimal(1);
                }
                else
                {
                    load.Forecast = int.MaxValue;
                }
                load.Current = (int)reader.GetDecimal(2);
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    mSelectLoadRefCommand.Connection = con;
                    if (market == "PJM")
                    {
                        mSelectLoadRefCommand.Parameters["@XRefName"].Value = "DayAhead";
                    }
                    else
                    {
                        mSelectLoadRefCommand.Parameters["@XRefName"].Value = "TESLA";
                    }
                    mSelectLoadRefCommand.Parameters["@LoadsKey"].Value = load.Current;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    SqlDataReader reader1 = mSelectLoadRefCommand.ExecuteReader();
                    while (reader1.Read())
                    {
                        load.DayAhead = reader1.GetString(0);
                    }
                    reader1.Close();
                    if (sLoadHash.ContainsKey(zone))
                        sLoadHash.Remove(zone);
                    sLoadHash.Add(zone, load);
                }
            }
            reader.Close();
            VayuConnection.Close();
        }

        /// <summary>
        /// Gets the total loads.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="mKey">The m key.</param>
        /// <param name="frozenHour">The frozen hour.</param>
        /// <returns></returns>
        Dictionary<string, List<LoadDataItem>> GetTotalLoads(DateTime fromDate, DateTime toDate, string zone, int mKey, int frozenHour)
        {
            Dictionary<string, List<LoadDataItem>> loads = new Dictionary<string, List<LoadDataItem>>();
            int noDays = (toDate.Date - fromDate.Date).Days;
            Dictionary<DateTime, DateTime> items = getDates(noDays, fromDate);
            if (items.Count > 0)
            {
                foreach (var eachItem in items)
                {
                    //int hr= DateTime.Now.Hour < 12 ? 4 : 5;
                    // DateTime dtfrozone = eachItem.Key.AddHours(hr);

                    if (mKey == 9)
                    {

                        frozenHour = 9;
                    }
                    using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "select t.MarketDate,t.ISOLoad,t.TeslaLoad, t.PRTLoad,t.WSILoad,t.DTNLoad from ZonalFrozenLoads t " +
                                            " where t.FreezeDate >=  DATEADD(DAY,-1,@startdate) and MarketDate between @startdate and  @enddate " +
                                        "and MarketKey=@marketkey and FrozenHour=@frozenhour and zonename = @zonename order by marketdate";
                            cmd.Parameters.AddWithValue("@startdate", eachItem.Key);//eachItem.Key

                            //cmd.Parameters.AddWithValue("@enddate", eachItem.Value.AddDays(noDays - 2));//toDate  eachItem.Value.AddDays(8)
                            cmd.Parameters.AddWithValue("@enddate", toDate.Date.AddDays(1));//toDate  eachItem.Value.AddDays(8)
                            cmd.Parameters.AddWithValue("@marketkey", mKey);
                            cmd.Parameters.AddWithValue("@frozenhour", frozenHour);
                            cmd.Parameters.AddWithValue("@zonename", zone);
                            //cmd.Parameters.AddWithValue("@FreezeDate", dtfrozone);
                            //cmd.Connection.Open();
                            IDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    DateTime date = reader.IsDBNull(0) ? DateTime.Today : Convert.ToDateTime(reader.GetValue(0));
                                    if (loads.ContainsKey("FROZENISO"))
                                    {
                                        loads["FROZENISO"].Add(new LoadDataItem
                                        {
                                            LoadForecast = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader.GetValue(1)),
                                            MarketDateTime = date
                                        });
                                    }
                                    else
                                    {
                                        loads.Add("FROZENISO", new List<LoadDataItem> {new LoadDataItem{
                            LoadForecast = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader.GetValue(1)),
                                MarketDateTime = date
                            } });
                                    }

                                }
                                catch (Exception)
                                {

                                }
                            }
                            reader.Close();
                            cmd.Connection.Close();
                        }
                    }
                }
            }

            return loads;
        }

        /// <summary>
        /// Gets the dates.
        /// </summary>
        /// <param name="noDays">The no days.</param>
        /// <param name="fromDate">From date.</param>
        /// <returns></returns>
        private static Dictionary<DateTime, DateTime> getDates(int noDays, DateTime fromDate)
        {
            Dictionary<DateTime, DateTime> dates = new Dictionary<DateTime, DateTime>();
            try
            {
                if (noDays > 1)
                {
                    for (int i = 0; i < noDays; i++)
                    {
                        if (i == 0)
                        {
                            dates.Add(fromDate, fromDate.AddDays(i + 1));
                        }
                        else
                        {
                            dates.Add(fromDate.AddDays(i), fromDate.AddDays(i + 1));
                        }
                    }
                }
                else
                {
                    dates.Add(fromDate, fromDate.AddDays(1));
                }
            }
            catch
            {
            }
            return dates;

        }

        #endregion
    }
}
