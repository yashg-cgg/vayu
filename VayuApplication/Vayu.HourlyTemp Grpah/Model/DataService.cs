using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.HourlyTemp_Grpah.ViewModels;

namespace Vayu.HourlyTemp_Grpah.Model
{
    public class DataService : IDataService
    {
        private static SqlConnection VayuConnection;
        private static SqlCommand mSelectCityZoneCommand;
        private static SqlCommand mSelectNodeListCommand;
        private static SqlCommand mSelectTemperatureCommand;
        private static SqlCommand mSelectErcotTemperatureCommand;
        private static SqlCommand mSelectForecastTemperatureCommand;
        private static SqlCommand mSelectErcotForecastTemperatureCommand;
        private static SqlCommand mSelectErcotUnionTemperatureCommand;
        private static SqlCommand mSelectCityIcaoCodeCommand;
        private static SqlCommand mSelectCityIcaoCodeCommand1;
        private static SqlCommand mSelectCitiesNodesCommand;
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            mSelectCityZoneCommand = VayuConnection.CreateCommand();
            mSelectCityZoneCommand.CommandText = "select label,zone from dbo.WSIICAOCode where region='ERCOT' order by label";
            mSelectCityZoneCommand.Connection = VayuConnection;

            mSelectTemperatureCommand = VayuConnection.CreateCommand();
            mSelectTemperatureCommand.CommandText = "select min( b.Temperature) ,  max( b.Temperature), avg( b.Temperature), a.zone, a.label from WSIICAOCode a join  WSICurrent b on a.icaocode=b.icaocode where a.region='ERCOT'  and  marketdatetime>@startdate and marketdatetime<=@enddate  group by a.label,a.zone";
            mSelectTemperatureCommand.Parameters.AddWithValue("@startdate", "marketdatetime");
            mSelectTemperatureCommand.Parameters.AddWithValue("@enddate", "marketdatetime");
            mSelectTemperatureCommand.Connection = VayuConnection;

            mSelectForecastTemperatureCommand = VayuConnection.CreateCommand();
            mSelectForecastTemperatureCommand.CommandText = "select min( b.Temperature) ,  max( b.Temperature), avg( b.Temperature), a.zone, a.label from WSIICAOCode a join  WSIForecast b on a.icaocode=b.icaocode where a.region='ERCOT'  and  marketdatetime>@startdate and marketdatetime<=@enddate  group by a.label,a.zone";
            mSelectForecastTemperatureCommand.Parameters.AddWithValue("@startdate", "marketdatetime");
            mSelectForecastTemperatureCommand.Parameters.AddWithValue("@enddate", "marketdatetime");
            mSelectForecastTemperatureCommand.Connection = VayuConnection;

            //
            mSelectErcotForecastTemperatureCommand = VayuConnection.CreateCommand();

            mSelectNodeListCommand = VayuConnection.CreateCommand();
            mSelectNodeListCommand.CommandText = "select label,zone from dbo.WSIICAOCode where region='ERCOT' order by label";
            mSelectNodeListCommand.Connection = VayuConnection;

        }
        public void GetCityZoneNames(Action<List<CityZones>, Exception> callback, string Market)
        {
            loadDBCommands();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<CityZones> mCityzoneList = new List<CityZones>();
            try
            {
                SqlDataReader reader;

                mSelectCityZoneCommand.CommandText = "select label,zone from dbo.WSIICAOCode where region='Ercot' order by label";
                reader = mSelectCityZoneCommand.ExecuteReader();


                while (reader.Read())
                {
                    CityZones mCityZones = new CityZones();
                    mCityZones.City = reader.GetString(0);
                    mCityZones.Zone = reader.IsDBNull(1) ? null : reader.GetString(1);
                    mCityzoneList.Add(mCityZones);
                }
                reader.Close();
                VayuConnection.Close();
                callback(mCityzoneList, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }
        public void GetAllTemperatureDataList(Action<List<HourlyTemperatureData>, Exception> callback, DateTime? sdate, DateTime? edate, bool current, string icaocode, string Market)
        {
            List<HourlyTemperatureData> TemperatureDataList = new List<HourlyTemperatureData>();
            loadDBCommands();
            TemperatureDataList = GetAllTemperatureDataHourly(sdate, edate, current, icaocode, Market);
            callback(TemperatureDataList, null);
        }

        private List<TemperatureData> DownloadTemperatureDataList(DateTime? sdate, DateTime? edate, bool current, string Market)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            SqlDataReader reader = null;
            List<TemperatureData> TemperatureDataList = new List<TemperatureData>();
            try
            {

                DateTime startdate = sdate.Value;

                DateTime enddate = edate.Value.AddDays(4);
                while (startdate <= enddate)
                {
                    if (current)
                    {
                        mSelectErcotTemperatureCommand.Parameters["@startdate"].Value = startdate;
                        mSelectErcotTemperatureCommand.Parameters["@enddate"].Value = startdate.AddDays(1);
                        reader = mSelectErcotTemperatureCommand.ExecuteReader();

                    }
                    else
                    {
                        mSelectErcotForecastTemperatureCommand.Parameters["@startdate"].Value = startdate;
                        mSelectErcotForecastTemperatureCommand.Parameters["@enddate"].Value = startdate.AddDays(1);
                        reader = mSelectErcotForecastTemperatureCommand.ExecuteReader();

                    }
                    while (reader.Read())
                    {
                        TemperatureData mTemperatureData = new TemperatureData();
                        mTemperatureData.Min = reader.IsDBNull(0) ? 0 : (int)reader.GetValue(0);
                        mTemperatureData.Max = reader.IsDBNull(1) ? 0 : (int)reader.GetValue(1);
                        mTemperatureData.Avg = reader.IsDBNull(2) ? 0 : (int)reader.GetValue(2);
                        mTemperatureData.Zone = reader.IsDBNull(3) ? null : reader.GetString(3);
                        mTemperatureData.City = reader.GetString(4);
                        mTemperatureData.Date = startdate;
                        TemperatureDataList.Add(mTemperatureData);
                    }
                    reader.Close();
                    startdate = startdate.AddDays(1);
                    //TemperatureDataList.Add(mTemperatureData);
                }

            }
            catch
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            return TemperatureDataList;
        }

        private List<HourlyTemperatureData> GetAllTemperatureDataHourly(DateTime? sdate, DateTime? edate, bool current, string icaocode, string Market)
        {

            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            SqlDataReader reader = null;
            List<HourlyTemperatureData> TemperatureDataList = new List<HourlyTemperatureData>();
            List<HourlyTemperatureData> finalList = new List<HourlyTemperatureData>();
            try
            {
                if (DateTime.Today >= sdate.Value && DateTime.Today < edate.Value)
                {
                    mSelectErcotUnionTemperatureCommand = VayuConnection.CreateCommand();
                    mSelectErcotUnionTemperatureCommand.CommandText = @"SELECT
                                                                DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0) AS HourlyTimestamp,
                                                                AVG(Temperature) AS HourlyAverageTemperature,
	                                                            WSIICAOCode.LABEL,WSIICAOCode.Zone, 'N' as ForecastData
                                                            FROM
                                                                WSICurrent inner join WSIICAOCode on WSICurrent.ICAOCode = WSIICAOCode.ICAOCode
                                                            WHERE
                                                                MarketDateTime > '" + sdate + @"' AND MarketDateTime <= '" + DateTime.Now + @"' AND WSICurrent.ICAOCode = '" + icaocode + @"'
                                                            GROUP BY
                                                                DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0),
	                                                            WSIICAOCode.LABEL,WSIICAOCode.Zone,
	                                                            DATEPART(HOUR, MarketDateTime),
                                                                CAST(MarketDateTime AS DATE) 
	                                                            union 
                                                            SELECT 
                                                                DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0) AS HourlyTimestamp,
                                                                AVG(Temperature) AS HourlyAverageTemperature,
	                                                            WSIICAOCode.LABEL,WSIICAOCode.Zone, 'Y' as ForecastData
                                                            FROM
                                                                WSIForecast inner join WSIICAOCode on WSIForecast.ICAOCode = WSIICAOCode.ICAOCode
                                                            WHERE
                                                                MarketDateTime > '" + DateTime.Now + @"' AND MarketDateTime <= '" + edate.Value.AddDays(1) + @"' AND WSIForecast.ICAOCode = '" + icaocode + @"'
                                                            GROUP BY
                                                                DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0),
	                                                            WSIICAOCode.LABEL,WSIICAOCode.Zone,
	                                                            DATEPART(HOUR, MarketDateTime),
                                                                CAST(MarketDateTime AS DATE) 
                                                            ORDER BY
                                                                HourlyTimestamp, WSIICAOCode.LABEL";

                    reader = mSelectErcotUnionTemperatureCommand.ExecuteReader();
                }
                else if (DateTime.Today < sdate.Value)
                {
                    mSelectErcotForecastTemperatureCommand.CommandText = @"SELECT 
                                                                    DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0) AS HourlyTimestamp,
                                                                    AVG(Temperature) AS HourlyAverageTemperature,
	                                                                WSIICAOCode.LABEL,WSIICAOCode.Zone, 'Y' as ForecastData
                                                                FROM
                                                                    WSIForecast inner join WSIICAOCode on WSIForecast.ICAOCode = WSIICAOCode.ICAOCode
                                                                WHERE
                                                                    MarketDateTime > '" + sdate + @"' AND MarketDateTime <= '" + edate.Value.AddDays(1) + @"' AND WSIForecast.ICAOCode = '" + icaocode + @"'
                                                                GROUP BY
                                                                    DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0),
	                                                                WSIICAOCode.LABEL,WSIICAOCode.Zone,
	                                                                DATEPART(HOUR, MarketDateTime),
                                                                    CAST(MarketDateTime AS DATE) 
                                                                ORDER BY
                                                                    HourlyTimestamp, WSIICAOCode.LABEL";

                    reader = mSelectErcotForecastTemperatureCommand.ExecuteReader();
                }
                else if (DateTime.Today >= edate.Value)
                {
                    mSelectErcotTemperatureCommand = VayuConnection.CreateCommand();
                    mSelectErcotTemperatureCommand.CommandText = @"SELECT
                                                            DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0) AS HourlyTimestamp,
                                                            AVG(Temperature) AS HourlyAverageTemperature,
	                                                        WSIICAOCode.LABEL,WSIICAOCode.Zone,'N' as ForecastData
                                                        FROM
                                                            WSICurrent inner join WSIICAOCode on WSICurrent.ICAOCode = WSIICAOCode.ICAOCode
                                                        WHERE
                                                            MarketDateTime > '" + sdate + "' AND MarketDateTime <= '" + edate.Value.AddDays(1) + @"' AND WSICurrent.ICAOCode = '" + icaocode + @"'
                                                        GROUP BY
                                                            DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0),
	                                                        WSIICAOCode.LABEL,WSIICAOCode.Zone,
	                                                        DATEPART(HOUR, MarketDateTime),
                                                            CAST(MarketDateTime AS DATE) 
                                                         union 
                                                            SELECT 
                                                                DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0) AS HourlyTimestamp,
                                                                AVG(Temperature) AS HourlyAverageTemperature,
	                                                            WSIICAOCode.LABEL,WSIICAOCode.Zone, 'Y' as ForecastData
                                                            FROM
                                                                WSIForecast inner join WSIICAOCode on WSIForecast.ICAOCode = WSIICAOCode.ICAOCode
                                                            WHERE
                                                                MarketDateTime > '" + DateTime.Now + @"' AND MarketDateTime <= '" + edate.Value.AddDays(1) + @"' AND WSIForecast.ICAOCode = '" + icaocode + @"'
                                                            GROUP BY
                                                                DATEADD(HOUR, DATEDIFF(HOUR, 0, MarketDateTime), 0),
	                                                            WSIICAOCode.LABEL,WSIICAOCode.Zone,
	                                                            DATEPART(HOUR, MarketDateTime),
                                                                CAST(MarketDateTime AS DATE) 
                                                            ORDER BY
                                                                HourlyTimestamp, WSIICAOCode.LABEL";

                    reader = mSelectErcotTemperatureCommand.ExecuteReader();
                }
                HourlyTemperatureData mTemperatureData = new HourlyTemperatureData();
                DataTable dt = new DataTable();
                dt.Load(reader);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    mTemperatureData = new HourlyTemperatureData();
                    int pointingHour = Convert.ToDateTime(dt.Rows[i][0]).Hour;
                    mTemperatureData.Date = (DateTime)dt.Rows[i][0];
                    mTemperatureData.City = (string)dt.Rows[i][2];
                    mTemperatureData.Zone = (string)dt.Rows[i][3];
                    switch (pointingHour)
                    {
                        case 1: mTemperatureData.Hour1 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE1CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 2: mTemperatureData.Hour2 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE2CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 3: mTemperatureData.Hour3 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE3CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 4: mTemperatureData.Hour4 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE4CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 5: mTemperatureData.Hour5 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE5CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 6: mTemperatureData.Hour6 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE6CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 7: mTemperatureData.Hour7 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE7CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 8: mTemperatureData.Hour8 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE8CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 9: mTemperatureData.Hour9 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE9CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 10: mTemperatureData.Hour10 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE10CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 11: mTemperatureData.Hour11 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE11CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 12: mTemperatureData.Hour12 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE12CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 13: mTemperatureData.Hour13 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE13CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 14: mTemperatureData.Hour14 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE14CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 15: mTemperatureData.Hour15 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE15CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 16: mTemperatureData.Hour16 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE16CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 17: mTemperatureData.Hour17 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE17CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 18: mTemperatureData.Hour18 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE18CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 19: mTemperatureData.Hour19 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE19CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 20: mTemperatureData.Hour20 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE20CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 21: mTemperatureData.Hour21 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE21CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 22: mTemperatureData.Hour22 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE22CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;
                        case 23: mTemperatureData.Hour23 = (int)dt.Rows[i][1]; if (dt.Rows[i][4] != null) { mTemperatureData.IsHE23CurrentTemperature = dt.Rows[i][4].ToString() == "N"; } break;

                        case 0:
                            if (dt.Rows[i][4] != null) { mTemperatureData.IsHE24CurrentTemperature = dt.Rows[i][4].ToString() == "N"; }
                            mTemperatureData.Hour24 = (int)dt.Rows[i][1];
                            mTemperatureData.Date = Convert.ToDateTime(dt.Rows[i][0]).AddDays(-1);
                            break;
                    }
                    if (!(mTemperatureData.Date < sdate))
                        TemperatureDataList.Add(mTemperatureData);
                }
                var templist = TemperatureDataList.GroupBy(x => x.Date.Date).OrderBy(x => x.Key).ToList();

                HourlyTemperatureData singleDayData = new HourlyTemperatureData();
                foreach (var item in templist)
                {
                    singleDayData = new HourlyTemperatureData();
                    singleDayData.Date = item.Key;
                    foreach (var i in item)
                    {
                        if (i.Hour1 != null) { singleDayData.Hour1 = i.Hour1; singleDayData.IsHE1CurrentTemperature = i.IsHE1CurrentTemperature; }
                        if (i.Hour2 != null) { singleDayData.Hour2 = i.Hour2; singleDayData.IsHE2CurrentTemperature = i.IsHE2CurrentTemperature; }
                        if (i.Hour3 != null) { singleDayData.Hour3 = i.Hour3; singleDayData.IsHE3CurrentTemperature = i.IsHE3CurrentTemperature; }
                        if (i.Hour4 != null) { singleDayData.Hour4 = i.Hour4; singleDayData.IsHE4CurrentTemperature = i.IsHE4CurrentTemperature; }
                        if (i.Hour5 != null) { singleDayData.Hour5 = i.Hour5; singleDayData.IsHE5CurrentTemperature = i.IsHE5CurrentTemperature; }
                        if (i.Hour6 != null) { singleDayData.Hour6 = i.Hour6; singleDayData.IsHE6CurrentTemperature = i.IsHE6CurrentTemperature; }
                        if (i.Hour7 != null) { singleDayData.Hour7 = i.Hour7; singleDayData.IsHE7CurrentTemperature = i.IsHE7CurrentTemperature; }
                        if (i.Hour8 != null) { singleDayData.Hour8 = i.Hour8; singleDayData.IsHE8CurrentTemperature = i.IsHE8CurrentTemperature; }
                        if (i.Hour9 != null) { singleDayData.Hour9 = i.Hour9; singleDayData.IsHE9CurrentTemperature = i.IsHE9CurrentTemperature; }
                        if (i.Hour10 != null) { singleDayData.Hour10 = i.Hour10; singleDayData.IsHE10CurrentTemperature = i.IsHE10CurrentTemperature; }
                        if (i.Hour11 != null) { singleDayData.Hour11 = i.Hour11; singleDayData.IsHE11CurrentTemperature = i.IsHE11CurrentTemperature; }
                        if (i.Hour12 != null) { singleDayData.Hour12 = i.Hour12; singleDayData.IsHE12CurrentTemperature = i.IsHE12CurrentTemperature; }
                        if (i.Hour13 != null) { singleDayData.Hour13 = i.Hour13; singleDayData.IsHE13CurrentTemperature = i.IsHE13CurrentTemperature; }
                        if (i.Hour14 != null) { singleDayData.Hour14 = i.Hour14; singleDayData.IsHE14CurrentTemperature = i.IsHE14CurrentTemperature; }
                        if (i.Hour15 != null) { singleDayData.Hour15 = i.Hour15; singleDayData.IsHE15CurrentTemperature = i.IsHE15CurrentTemperature; }
                        if (i.Hour16 != null) { singleDayData.Hour16 = i.Hour16; singleDayData.IsHE16CurrentTemperature = i.IsHE16CurrentTemperature; }
                        if (i.Hour17 != null) { singleDayData.Hour17 = i.Hour17; singleDayData.IsHE17CurrentTemperature = i.IsHE17CurrentTemperature; }
                        if (i.Hour18 != null) { singleDayData.Hour18 = i.Hour18; singleDayData.IsHE18CurrentTemperature = i.IsHE18CurrentTemperature; }
                        if (i.Hour19 != null) { singleDayData.Hour19 = i.Hour19; singleDayData.IsHE19CurrentTemperature = i.IsHE19CurrentTemperature; }
                        if (i.Hour20 != null) { singleDayData.Hour20 = i.Hour20; singleDayData.IsHE20CurrentTemperature = i.IsHE20CurrentTemperature; }
                        if (i.Hour21 != null) { singleDayData.Hour21 = i.Hour21; singleDayData.IsHE21CurrentTemperature = i.IsHE21CurrentTemperature; }
                        if (i.Hour22 != null) { singleDayData.Hour22 = i.Hour22; singleDayData.IsHE22CurrentTemperature = i.IsHE22CurrentTemperature; }
                        if (i.Hour23 != null) { singleDayData.Hour23 = i.Hour23; singleDayData.IsHE23CurrentTemperature = i.IsHE23CurrentTemperature; }
                        if (i.Hour24 != null) { singleDayData.Hour24 = i.Hour24; singleDayData.IsHE24CurrentTemperature = i.IsHE24CurrentTemperature; }

                        if (singleDayData.Zone == null)
                        {
                            singleDayData.Zone = i.Zone;
                        }
                        if (singleDayData.City == null)
                        {
                            singleDayData.City = i.City;
                        }
                    }
                    finalList.Add(singleDayData);
                }
                foreach (HourlyTemperatureData data in finalList)
                {
                    data.CalculateMinMaxAvg();
                }
            }

            catch (Exception ex)
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            return finalList;
        }



        public List<Tuple<string, string>> GetCityIcaoCodeList(string Market)
        {
            SqlDataReader reader = null;
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            try
            {

                mSelectCityIcaoCodeCommand = VayuConnection.CreateCommand();
                mSelectCityIcaoCodeCommand.CommandText = "select distinct Label, Icaocode from WSIICAOCode where Region = '" + Market + "' ";
                reader = mSelectCityIcaoCodeCommand.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(Tuple.Create(reader.GetString(0), reader.GetString(1)));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            return list;

        }
        public List<string> GetCityIcaoCodeList1(string Market)
        {
            SqlDataReader reader = null;
            List<string> list = new List<string>();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            try
            {

                mSelectCityIcaoCodeCommand1 = VayuConnection.CreateCommand();
                mSelectCityIcaoCodeCommand1.CommandText = "select distinct Label from WSIICAOCode where Region = '" + Market + "' ";
                reader = mSelectCityIcaoCodeCommand1.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader.GetString(0));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            return list;

        }
        public Dictionary<int, string> GetCountyHashByNodeKey()
        {
            Dictionary<int, string> dictCountyHash = new Dictionary<int, string>();
            SqlDataReader reader = null;
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            try
            {
                mSelectCitiesNodesCommand = VayuConnection.CreateCommand();
                mSelectCitiesNodesCommand.CommandText = "select NodeKey, County from TblNodeZoneCounty where county is not null and county != ''  order by NodeName";
                reader = mSelectCitiesNodesCommand.ExecuteReader();
                while (reader.Read())
                {
                    dictCountyHash.Add(reader.GetInt32(0), reader.GetString(1));
                }
            }
            catch (Exception ex)
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            return dictCountyHash;
        }
    }
}
