using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.TemperatureGraph.ViewModels;

namespace Vayu.TemperatureGraph.Model
{
    public class DataService : IDataService
    {
        private static SqlConnection VayuConnection;
        private static SqlCommand mSelectCityZoneCommand;
        private static SqlCommand mSelectTemperatureCommand;
        private static SqlCommand mSelectErcotTemperatureCommand;
        private static SqlCommand mSelectForecastTemperatureCommand;
        private static SqlCommand mSelectErcotForecastTemperatureCommand;
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            mSelectCityZoneCommand = VayuConnection.CreateCommand(); ;
            mSelectCityZoneCommand.CommandText = "select label,zone from dbo.WSIICAOCode where region='ERCOT' order by label";
            mSelectCityZoneCommand.Connection = VayuConnection;

            mSelectTemperatureCommand = VayuConnection.CreateCommand();
            mSelectTemperatureCommand.CommandText = "select min( b.Temperature) ,  max( b.Temperature), avg( b.Temperature), a.zone, a.label from WSIICAOCode a join  WSICurrent b on a.icaocode=b.icaocode where a.region='ERCOT'  and  marketdatetime>@startdate and marketdatetime<=@enddate  group by a.label,a.zone";
            mSelectTemperatureCommand.Parameters.AddWithValue("@startdate", "marketdatetime");
            mSelectTemperatureCommand.Parameters.AddWithValue("@enddate", "marketdatetime");
            mSelectTemperatureCommand.Connection = VayuConnection;

            //

            mSelectErcotTemperatureCommand = VayuConnection.CreateCommand();
            mSelectErcotTemperatureCommand.CommandText = "select min( b.Temperature) ,  max( b.Temperature), avg( b.Temperature), a.zone, a.label from Vayu..WSIICAOCode a join Vayu..WSICurrent b on a.icaocode=b.icaocode where a.region='Ercot'  and  marketdatetime>@startdate and marketdatetime<=@enddate  group by a.label,a.zone";
            mSelectErcotTemperatureCommand.Parameters.AddWithValue("@startdate", "marketdatetime");
            mSelectErcotTemperatureCommand.Parameters.AddWithValue("@enddate", "marketdatetime");
            mSelectErcotTemperatureCommand.Connection = VayuConnection;
            //
            mSelectForecastTemperatureCommand = VayuConnection.CreateCommand();
            mSelectForecastTemperatureCommand.CommandText = "select min( b.Temperature) ,  max( b.Temperature), avg( b.Temperature), a.zone, a.label from WSIICAOCode a join  WSIForecast b on a.icaocode=b.icaocode where a.region='ERCOT'  and  marketdatetime>@startdate and marketdatetime<=@enddate  group by a.label,a.zone";
            mSelectForecastTemperatureCommand.Parameters.AddWithValue("@startdate", "marketdatetime");
            mSelectForecastTemperatureCommand.Parameters.AddWithValue("@enddate", "marketdatetime");
            mSelectForecastTemperatureCommand.Connection = VayuConnection;

            //
            mSelectErcotForecastTemperatureCommand = VayuConnection.CreateCommand();
            mSelectErcotForecastTemperatureCommand.CommandText = "select min( b.Temperature) ,  max( b.Temperature), avg( b.Temperature), a.zone, a.label from WSIICAOCode a join  Vayu..WSIForecast b on a.icaocode=b.icaocode where a.region='Ercot'  and  marketdatetime>@startdate and marketdatetime<=@enddate group by a.label,a.zone";
            mSelectErcotForecastTemperatureCommand.Parameters.AddWithValue("@startdate", "marketdatetime");
            mSelectErcotForecastTemperatureCommand.Parameters.AddWithValue("@enddate", "marketdatetime");
            mSelectErcotForecastTemperatureCommand.Connection = VayuConnection;

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
        public void GetAllTemperatureData(Action<List<TemperatureData>, Exception> callback, DateTime? sdate, DateTime? edate, bool current, string Market)
        {
            List<TemperatureData> TemperatureDataList = new List<TemperatureData>();
            loadDBCommands();
            TemperatureDataList = DownloadTemperatureDataList(sdate, edate, current, Market);
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
    }
}
