using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.Actualvs7DayLoad.Model
{
    public class DataService : IDataService
    {
        private static SqlConnection VayuConnection;
        private static SqlCommand mSelectZoneCommand;
        private static SqlCommand mSelectLoadRefCommand;
        private static SqlCommand mSelectLoadForecastsCommand;
        private static SqlCommand mSelectErcotLoadForecastsCommand;
        private static SqlCommand mSelectRTLoadsCommand;
        private static SqlCommand mSelectErcotRTLoadsCommand;
        private static Dictionary<string, Vayu.LoadGraphLibrary.Load> sLoadHash = new Dictionary<string, Vayu.LoadGraphLibrary.Load>();
        private static void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectZoneCommand = VayuConnection.CreateCommand();
            mSelectZoneCommand.CommandText = "select LoadsName, loadForecastTypeKey, loadskey, l.marketkey, case when mk.Label = 'ERCOTTesting' then 'ERCOT' else mk.Label end as MarketLabel  from Loads l inner join Market mk on l.MarketKey = mk.MarketKey where l.MarketKey in (9) order by l.marketkey, loadsname ";

            mSelectLoadRefCommand = new SqlCommand();
            mSelectLoadRefCommand.CommandText = "select LoadName from LoadXRefType where XRefName = @XRefName and LoadsKey = @LoadsKey";
            mSelectLoadRefCommand.Parameters.AddWithValue("@LoadsKey", "LoadsKey");
            mSelectLoadRefCommand.Parameters.AddWithValue("@XRefName", "XRefName");

            mSelectLoadForecastsCommand = VayuConnection.CreateCommand();
            mSelectLoadForecastsCommand.CommandText = "select  MarketDateTime, MW  from FrozenLoadForecasts where  LoadForecastTypeKey= @loadforecasttypekey and MarketDateTime >=@startDate and MarketDateTime<= @endDate order by MarketDateTime";
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@startDate", "");
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@endDate", "");
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@loadforecasttypekey", "");

            mSelectErcotLoadForecastsCommand = VayuConnection.CreateCommand();
            mSelectErcotLoadForecastsCommand.CommandText = "select  CAST(MarketDateTime as date ) As MarketDateTime ,MAX(MW) as MW from FrozenLoadForecasts where  LoadForecastTypeKey= @loadforecasttypekey and MarketDateTime >=@startDate and MarketDateTime<= @endDate group by CAST(MarketDateTime as date)";
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@startDate", "");
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@endDate", "");
            mSelectLoadForecastsCommand.Parameters.AddWithValue("@loadforecasttypekey", "");

            mSelectRTLoadsCommand = VayuConnection.CreateCommand();
            mSelectRTLoadsCommand.CommandText = "select  top 1 MarketDateTime, max(MW) from LoadRT where  MarketDateTime >@startDate and MarketDateTime<= @endDate and LoadsKey=@loadskey group by LoadsKey,MarketDateTime  order by max(mw) desc";
            mSelectRTLoadsCommand.Parameters.AddWithValue("@startdate", "");
            mSelectRTLoadsCommand.Parameters.AddWithValue("@enddate", "");
            mSelectRTLoadsCommand.Parameters.AddWithValue("@loadskey", "");

            mSelectErcotRTLoadsCommand = VayuConnection.CreateCommand();
            mSelectErcotRTLoadsCommand.CommandText = "select  top 1 MarketDateTime, max(MW) from LoadRT where  MarketDateTime >@startDate and MarketDateTime<= @endDate and LoadsKey=@loadskey group by LoadsKey,MarketDateTime  order by max(mw) desc";
            mSelectErcotRTLoadsCommand.Parameters.AddWithValue("@startdate", "");
            mSelectErcotRTLoadsCommand.Parameters.AddWithValue("@enddate", "");
            mSelectErcotRTLoadsCommand.Parameters.AddWithValue("@loadskey", "");
        }
        public void GetLoadNames(Action<List<string>, Exception> callback)
        {
            loadDBCommands();
            try
            {
                if (sLoadHash.Count == 0)
                    FillLoadHash();
                callback(sLoadHash.Keys.OrderBy(a => a).ToList(), null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }
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
                using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                {
                    mSelectLoadRefCommand.Connection = con;
                    mSelectLoadRefCommand.Parameters["@XRefName"].Value = "DayAhead";
                    mSelectLoadRefCommand.Parameters["@LoadsKey"].Value = load.Current;
                    con.Open();
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

        public void GetLoadData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone, int Marketkey)
        {
            loadDBCommands();
            //int mKey = 1;
            if (Marketkey > 0)
            {
                if (true)
                {
                    try
                    {
                        Dictionary<string, List<LoadDataItem>> itemList = new Dictionary<string, List<LoadDataItem>>();
                        itemList.Add("ISO", GetLoadForecast(fromDate, toDate, zone, Marketkey));
                        itemList.Add("CURRENT", GetCurrentLoads(fromDate, toDate, zone, Marketkey));
                        if (itemList.ContainsKey("CURRENT"))
                        {
                            List<LoadDataItem> actualLoadList = itemList["CURRENT"].ToList();
                            List<LoadDataItem> actualAvg5DayList = calcMovingAvg(actualLoadList, 5).ToList();
                            List<LoadDataItem> actualAvg8DayList = calcMovingAvg(actualLoadList, 8).ToList();
                            List<LoadDataItem> actualAvg13DayList = calcMovingAvg(actualLoadList, 13).ToList();
                            List<LoadDataItem> actualAvg21DayList = calcMovingAvg(actualLoadList, 21).ToList();
                            actualAvg5DayList.RemoveAll(a => a == null);
                            actualAvg8DayList.RemoveAll(a => a == null);
                            actualAvg13DayList.RemoveAll(a => a == null);
                            actualAvg21DayList.RemoveAll(a => a == null);
                            itemList.Add("5DAYS-Avg", actualAvg5DayList);
                            itemList.Add("8DAYS-Avg", actualAvg8DayList);
                            itemList.Add("13DAYS-Avg", actualAvg13DayList);
                            itemList.Add("21DAYS-Avg", actualAvg21DayList);
                        }
                        callback(itemList, null);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        private LoadDataItem[] calcMovingAvg(List<LoadDataItem> dataList, int period)
        {
            try
            {
                dataList = dataList.OrderBy(a => a.MarketDateTime).ToList();
                LoadDataItem[] data = dataList.ToArray();
                double[] buffer = new double[period];
                LoadDataItem[] output = new LoadDataItem[dataList.Count];
                int current_index = 0;
                for (int i = 0; i < dataList.Count; i++)
                {
                    bool isbresk = false;
                    buffer[current_index] = Convert.ToDouble(data[i].LoadForecast / period);
                    double ma = 0;
                    for (int j = 0; j <= period; j++)
                    {
                        if (dataList.Count - i < period)
                            break;
                        if (j == period)
                        {
                            double avgma = ma / period;
                            if (i + period > dataList.Count)
                            {
                                isbresk = true;
                                break;
                            }
                            LoadDataItem item = new LoadDataItem();
                            item.LoadForecast = avgma;
                            item.MarketDateTime = data[i + period - 1].MarketDateTime;
                            output[i] = item;
                            continue;
                        }
                        else
                            ma += (double)data[j + i].LoadForecast;
                    }
                    if (isbresk)
                    {
                        break;
                    }
                    current_index = (current_index + 1) % period;
                }
                //string[] avgArr = new string[output.Length];
                //for (int i = 0; i < output.Length; i++)
                //{
                //    string avg = output[i].ToString();
                //    avgArr[i] = avg;
                //}

                return output;
            }
            catch (Exception ex)
            {

                return null;
            }


        }
        private static List<LoadDataItem> GetCurrentLoads(DateTime sDate, DateTime eDate, string zone, int mKey)
        {
            DateTime fromDate = sDate;
            DateTime toDate = DateTime.Today.AddDays(0);
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            if (mKey == 1 || mKey == 9)
            {
                while (fromDate < toDate)
                {
                    using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            if (mKey == 1)
                                cmd.CommandText = mSelectRTLoadsCommand.CommandText;
                            else if (mKey == 9)
                                cmd.CommandText = mSelectErcotRTLoadsCommand.CommandText;
                            cmd.Parameters.Add(new SqlParameter("@startDate", fromDate));
                            cmd.Parameters.Add(new SqlParameter("@endDate", fromDate.AddDays(1)));// DateTime.Today));
                            cmd.Parameters.Add(new SqlParameter("@loadskey", sLoadHash[zone].Current));

                            //con.Open();
                            IDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                itemList.Add(new LoadDataItem
                                {
                                    MarketDateTime = reader.IsDBNull(0) ? new DateTime() : Convert.ToDateTime(reader.GetDateTime(0).ToShortDateString()),
                                    LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                                });
                            }
                            if (!reader.IsClosed)
                                reader.Close();

                        }
                    }
                    fromDate = fromDate.AddDays(1);
                }
            }
            return itemList.OrderBy(a => a.MarketDateTime).ToList();
        }
        private List<LoadDataItem> GetLoadForecast(DateTime sDate, DateTime eDate, string zone, int mKey)
        {
            DateTime fromDate = sDate;
            DateTime toDate = eDate;
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            if (mKey == 1 || mKey == 9)
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (mKey == 1)
                            cmd.CommandText = mSelectLoadForecastsCommand.CommandText;
                        else if (mKey == 9)
                            cmd.CommandText = mSelectErcotLoadForecastsCommand.CommandText;

                        cmd.Parameters.Add(new SqlParameter("@loadforecasttypekey", sLoadHash[zone].Forecast));
                        cmd.Parameters.Add(new SqlParameter("@startDate", sDate));
                        cmd.Parameters.Add(new SqlParameter("@endDate", eDate));
                        //con.Open();
                        IDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            itemList.Add(new LoadDataItem
                            {
                                MarketDateTime = reader.IsDBNull(0) ? new DateTime() : Convert.ToDateTime(reader.GetDateTime(0).ToShortDateString()),
                                LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                            });
                        }
                        if (!reader.IsClosed)
                            reader.Close();

                    }
                }
            }
            return itemList.OrderBy(a => a.MarketDateTime).ToList();
        }
    }
}
