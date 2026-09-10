using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.LoadCurve.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private static SqlConnection VayuConnection;
        /// <summary>
        /// The m select zone command
        /// </summary>
        private static SqlCommand mSelectZoneCommand;
        /// <summary>
        /// The m select load reference command
        /// </summary>
        private static SqlCommand mSelectLoadRefCommand;
        /// <summary>
        /// The s load hash
        /// </summary>
        private static Dictionary<string, Vayu.LoadGraphLibrary.Load> sLoadHash = new Dictionary<string, Vayu.LoadGraphLibrary.Load>();

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
                {
                    FillLoadHash();
                }
                callback(sLoadHash.Keys.OrderBy(a => a).ToList(), null);
            }
            catch (Exception ex)
            {

                callback(null, ex);
            }


        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            mSelectZoneCommand = VayuConnection.CreateCommand();
            mSelectZoneCommand.CommandText = "select LoadsName, loadForecastTypeKey, loadskey,l.marketkey, case when mk.Label='ERCOTTesting' then 'ERCOT' else mk.Label end as MarketLabel from Loads l inner join Market mk on l.MarketKey = mk.MarketKey where l.MarketKey in (9) order by l.MarketKey, loadsname";
            mSelectLoadRefCommand = new SqlCommand();
            mSelectLoadRefCommand.CommandText = "select LoadName from LoadXRefType where XRefName = @XRefName and LoadsKey = @LoadsKey";
            mSelectLoadRefCommand.Parameters.AddWithValue("@LoadsKey", "LoadsKey");
            mSelectLoadRefCommand.Parameters.AddWithValue("@XRefName", "XRefName");

        }

        /// <summary>
        /// Fills the load hash.
        /// </summary>
        private void FillLoadHash()
        {
            if (sLoadHash.Count == 0)
            {
                try
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
                        using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                        {
                            mSelectLoadRefCommand.Connection = con;
                            mSelectLoadRefCommand.Parameters["@XRefName"].Value = "PRT";
                            mSelectLoadRefCommand.Parameters["@LoadsKey"].Value = load.Current;

                            SqlDataReader reader1 = mSelectLoadRefCommand.ExecuteReader();
                            while (reader1.Read())
                            {
                                load.PRT = reader1.GetString(0);
                            }
                            reader1.Close();
                            mSelectLoadRefCommand.Parameters["@XRefName"].Value = "TESLA";
                            mSelectLoadRefCommand.Parameters["@LoadsKey"].Value = load.Current;
                            reader1 = mSelectLoadRefCommand.ExecuteReader();
                            while (reader1.Read())
                            {
                                load.Tesla = reader1.GetString(0);
                            }
                            reader1.Close();
                            mSelectLoadRefCommand.Parameters["@XRefName"].Value = "DayAhead";
                            mSelectLoadRefCommand.Parameters["@LoadsKey"].Value = load.Current;
                            reader1 = mSelectLoadRefCommand.ExecuteReader();
                            while (reader1.Read())
                            {
                                load.DayAhead = reader1.GetString(0);
                            }
                            reader1.Close();
                            mSelectLoadRefCommand.Parameters["@XRefName"].Value = "WSI";
                            mSelectLoadRefCommand.Parameters["@LoadsKey"].Value = load.Current;
                            reader1 = mSelectLoadRefCommand.ExecuteReader();
                            while (reader1.Read())
                            {
                                load.WSI = reader1.GetString(0);
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
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// Gets the frozen loads data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="FrozenTime">The frozen time.</param>
        public void GetFrozenLoadsData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone, int FrozenTime)
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

        /// <summary>
        /// Gets the total loads.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="mKey">The m key.</param>
        /// <param name="frozenHour">The frozen hour.</param>
        /// <returns></returns>
        private Dictionary<string, List<LoadDataItem>> GetTotalLoads(DateTime fromDate, DateTime toDate, string zone, int mKey, int frozenHour)
        {
            Dictionary<string, List<LoadDataItem>> loads = new Dictionary<string, List<LoadDataItem>>();
            int noDays = (toDate.Date - fromDate.Date).Days;
            Dictionary<DateTime, DateTime> items = getDates(noDays, fromDate);
            if (items.Count > 0)
            {
                //foreach (var eachItem in items)
                //{
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "select t.MarketDate,t.ISOLoad,t.TeslaLoad, t.PRTLoad,t.WSILoad,t.DTNLoad from ZonalFrozenLoads t " +
                                       " where t.FreezeDate >=  DATEADD(DAY,-1, @startdate) and MarketDate between @startdate and @enddate " +
                                   "and MarketKey=@marketkey and FrozenHour=@frozenhour and zonename = @zonename order by marketdate";
                        cmd.Parameters.AddWithValue("@startdate", DateTime.Today); //eachItem.Key.AddDays(1));
                        cmd.Parameters.AddWithValue("@enddate", DateTime.Today.AddDays(1));
                        cmd.Parameters.AddWithValue("@marketkey", mKey);
                        cmd.Parameters.AddWithValue("@frozenhour", frozenHour);
                        cmd.Parameters.AddWithValue("@zonename", zone);
                        cmd.Connection.Open();
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
                                    loads.Add("FROZENISO", new List<LoadDataItem>{new LoadDataItem
                                        {
                                            LoadForecast =reader.IsDBNull(1)?0:Convert.ToDouble(reader.GetValue(1)),
                                            MarketDateTime = date
                                        }});
                                }

                                /*if (loads.ContainsKey("FROZENPRT"))
                                {
                                    loads["FROZENPRT"].Add(new LoadDataItem
                                    {
                                        LoadForecast = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3)),
                                        MarketDateTime = date
                                    });
                                }
                                else
                                {
                                    loads.Add("FROZENPRT", new List<LoadDataItem>{new  LoadDataItem
                                        {
                                            LoadForecast = reader.IsDBNull(3)?0:Convert.ToDouble(reader.GetValue(3)),
                                            MarketDateTime = date
                                        }});
                                }

                                if (loads.ContainsKey("FROZENWSI"))
                                {
                                    loads["FROZENWSI"].Add(new LoadDataItem
                                    {
                                        LoadForecast = reader.IsDBNull(4) ? 0 : Convert.ToDouble(reader.GetValue(4)),
                                        MarketDateTime = date
                                    });
                                }
                                else
                                {
                                    loads.Add("FROZENWSI", new List<LoadDataItem>{new LoadDataItem
                                        {
                                            LoadForecast = reader.IsDBNull(4)?0: Convert.ToDouble(reader.GetValue(4)),
                                            MarketDateTime = date
                                        }
                                        });
                                }

                                if (loads.ContainsKey("FROZENDTN"))
                                {
                                    loads["FROZENDTN"].Add(new LoadDataItem
                                    {
                                        LoadForecast = reader.IsDBNull(5) ? 0 : Convert.ToDouble(reader.GetValue(5)),
                                        MarketDateTime = date
                                    });

                                }
                                else
                                {
                                    loads.Add("FROZENDTN", new List<LoadDataItem>{ new  LoadDataItem
                                        {
                                            LoadForecast = reader.IsDBNull(5)?0: Convert.ToDouble(reader.GetValue(5)),
                                            MarketDateTime = date
                                        }});
                                }*/

                            }
                            catch (Exception)
                            {


                            }
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                }
                //}
            }
            return loads;
        }

        /// <summary>
        /// Gets the dates.
        /// </summary>
        /// <param name="noDays">The no days.</param>
        /// <param name="fromDate">From date.</param>
        /// <returns></returns>
        private Dictionary<DateTime, DateTime> getDates(int noDays, DateTime fromDate)
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

        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <returns></returns>
        private int GetMarketKey(string zone)
        {
            if (zone == null || zone == string.Empty)
            {
                return -1;
            }
            else
            {
                string[] zoneArray = zone.Split(' ');
                if (zoneArray[0].ToLower().Equals("miso"))
                {
                    return 2;
                }
                if (zoneArray[0].ToLower().Equals("pjm"))
                {
                    return 1;
                }
                if (zoneArray[0].ToLower().Equals("nyiso"))
                {
                    return 3;
                }
                if (zoneArray[0].ToLower().Equals("spp"))
                {
                    return 12;
                }
                else
                {
                    return 9;
                }

            }
        }

        #endregion
    }
}
