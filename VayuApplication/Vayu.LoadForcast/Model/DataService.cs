using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.LoadForcast.Model
{
    public class DataService : IDataService
    {
        private static SqlConnection VayuConnection;
        private static SqlCommand mSelectForecastCommand;
        private static SqlCommand mSelectLoadForecastTypeNameCommand;
        private static SqlCommand mSelectLoadForecastsCommand;
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectForecastCommand = VayuConnection.CreateCommand();
            mSelectForecastCommand.CommandText = "select max(mw),LoadForecastTypeKey from dbo.LoadForecasts where  marketdatetime>@startdate and marketdatetime<=@enddate and LoadForecastTypeKey in( select distinct LoadForecastTypeKey from dbo.LoadForecastType where marketkey=1 and LoadsKey in(select distinct loadskey from loads where marketkey=1 ) ) group by LoadForecastTypeKey";
            mSelectForecastCommand.Parameters.AddWithValue("@startdate", "marketdatetime");
            mSelectForecastCommand.Parameters.AddWithValue("@enddate", "marketdatetime");
            mSelectForecastCommand.Connection = VayuConnection;

            mSelectLoadForecastTypeNameCommand = VayuConnection.CreateCommand();
            mSelectLoadForecastTypeNameCommand.CommandText = "select distinct LoadForecastTypeKey , LoadForecastTypeName from dbo.LoadForecastType where marketkey=@Marketkey and LoadsKey in(select distinct loadskey from loads where marketkey=@Marketkey )";
            mSelectLoadForecastTypeNameCommand.Parameters.AddWithValue("@Marketkey", "marketkey");
            mSelectLoadForecastTypeNameCommand.Connection = VayuConnection;

            mSelectLoadForecastsCommand = VayuConnection.CreateCommand();
            mSelectLoadForecastsCommand.CommandText = "select  MW, LoadForecastTypeKey  from FrozenLoadForecasts where   MarketDateTime >=@startDate and MarketDateTime< @endDate order by MarketDateTime";
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@startDate", "");
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@endDate", "");
        }
        public void GetAllForecastData(Action<System.Collections.Generic.List<LoadForecastData>, Exception> callback, DateTime? sdate, DateTime? edate, int Marketkey)
        {
            List<LoadForecastData> LoadForecastList = new List<LoadForecastData>();
            loadDBCommands();
            LoadForecastList = DownloadLoadForecastList(sdate, edate, Marketkey);
            callback(LoadForecastList, null);
        }
        public void GetAllForecast7Data(Action<System.Collections.Generic.List<LoadForecastData>, Exception> callback, DateTime? sdate, DateTime? edate, int Marketkey)
        {
            List<LoadForecastData> LoadForecast7List = new List<LoadForecastData>();
            loadDBCommands();
            LoadForecast7List = DownloadLoadForecast7List(sdate, edate, Marketkey);
            callback(LoadForecast7List, null);
        }
        private List<LoadForecastData> DownloadLoadForecastList(DateTime? sdate, DateTime? edate, int Marketkey)
        {
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            SqlDataReader reader = null;
            List<LoadForecastData> LoadForecastDataList = new List<LoadForecastData>();
            try
            {
                if (Marketkey == 9)
                {
                    //mSelectForecastCommand = new SqlCommand();
                    mSelectForecastCommand.CommandText = " select max(mw),LoadForecastTypeKey from Vayu..LoadForecasts where  marketdatetime>@startdate and marketdatetime<=@enddate and " +
                                                         " LoadForecastTypeKey in( select distinct LoadForecastTypeKey from Vayu..LoadForecastType where marketkey=9 and " +
                                                         " LoadsKey in(select distinct loadskey from Vayu..loads where marketkey=9 ) ) group by LoadForecastTypeKey";
                    // mSelectForecastCommand.Connection = SigmaDbConn;
                }
                mSelectForecastCommand.Parameters["@startdate"].Value = sdate.Value;
                mSelectForecastCommand.Parameters["@enddate"].Value = edate.Value;
                reader = mSelectForecastCommand.ExecuteReader();
                while (reader.Read())
                {
                    LoadForecastData mTemperatureData = new LoadForecastData();
                    mTemperatureData.MW = (int)reader.GetDecimal(0);
                    mTemperatureData.LoadForecastKey = reader.IsDBNull(1) ? 0 : (int)reader.GetDecimal(1);
                    LoadForecastDataList.Add(mTemperatureData);
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
            return LoadForecastDataList;
        }

        private List<LoadForecastData> DownloadLoadForecast7List(DateTime? sdate, DateTime? edate, int Marketkey)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            SqlDataReader reader = null;
            List<LoadForecastData> LoadForecastData7List = new List<LoadForecastData>();
            try
            {
                if (Marketkey == 9)
                {
                    mSelectLoadForecastsCommand.CommandText = " select MAX(MW) AS MW, LoadForecastTypeKey  from Vayu..FrozenLoadForecasts where   MarketDateTime >=@startdate and " +
                                                              " MarketDateTime< @enddate group  by LoadForecastTypeKey";
                }
                mSelectLoadForecastsCommand.Parameters["@startdate"].Value = sdate.Value;
                mSelectLoadForecastsCommand.Parameters["@enddate"].Value = edate.Value;
                reader = mSelectLoadForecastsCommand.ExecuteReader();
                while (reader.Read())
                {
                    LoadForecastData mTemperature7Data = new LoadForecastData();
                    mTemperature7Data.MW = (int)reader.GetDecimal(0);
                    mTemperature7Data.LoadForecastKey = reader.IsDBNull(1) ? 0 : (int)reader.GetDecimal(1);
                    LoadForecastData7List.Add(mTemperature7Data);
                }
                reader.Close();
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
            return LoadForecastData7List;
        }
        public List<LoadForecastType> GetAllForecastList(int Marketkey)
        {
            loadDBCommands();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            SqlDataReader reader = null;
            List<LoadForecastType> LoadForecastTypeList = new List<LoadForecastType>();
            try
            {
                mSelectLoadForecastTypeNameCommand.Parameters["@Marketkey"].Value = Marketkey;
                reader = mSelectLoadForecastTypeNameCommand.ExecuteReader();
                while (reader.Read())
                {
                    LoadForecastType mLoadForecastTypeData = new LoadForecastType();
                    mLoadForecastTypeData.LoadForecastKey = reader.IsDBNull(0) ? 0 : (int)reader.GetDecimal(0);
                    mLoadForecastTypeData.LoadForecastTypeName = reader.GetString(1);
                    LoadForecastTypeList.Add(mLoadForecastTypeData);
                }
                reader.Close();

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
            return LoadForecastTypeList;
        }
    }
}
