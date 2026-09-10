using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.FuelMix.Model
{

    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private static SqlConnection VayuConnection;

        #region SQL Commands
        private static SqlCommand mSelectSolarCommand;
        private static SqlCommand mSelectWindCommand;
        private static SqlCommand mSelectHydroCommand;
        private static SqlCommand mSelectPowerStorageCommand;
        private static SqlCommand mSelectNaturalGasCommand;
        private static SqlCommand mSelectCoalandLigniteCommand;
        private static SqlCommand mSelectNuclearCommand;
        private static SqlCommand mSelectOtherCommand;
        private static SqlCommand mSelectTotalCommand;

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        public DataService()
        {
            loadDBCommands();
        }


        public void GetFuelMixData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone)
        {
            loadDBCommands();



            if (true)
            {
                try
                {
                    Dictionary<string, List<LoadDataItem>> itemList = new Dictionary<string, List<LoadDataItem>>();
                    itemList.Add("Solar", GetSolarFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Wind", GetWindFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Hydro", GetHydroFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Power Storage", GetPowerStorageFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Natural Gas", GetNaturalGasFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Coal and Lignite", GetCoalandLigniteFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Nuclear", GetNuclearFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Other", GetOtherFromFuelMix(fromDate, toDate, zone));
                    itemList.Add("Total", GetTotalFromFuelMix(fromDate, toDate, zone));
                    callback(itemList, null);
                }
                catch (Exception ex)
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

            //
            mSelectSolarCommand = VayuConnection.CreateCommand();
            mSelectSolarCommand.CommandText = "select DateTime, Solar from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectSolarCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectSolarCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectWindCommand = VayuConnection.CreateCommand();
            mSelectWindCommand.CommandText = "select DateTime, Wind from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectWindCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectWindCommand.Parameters.AddWithValue("@endDate", "endDate");
            //
            mSelectHydroCommand = VayuConnection.CreateCommand();
            mSelectHydroCommand.CommandText = "select DateTime, Hydro from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectHydroCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectHydroCommand.Parameters.AddWithValue("@endDate", "endDate");
            //

            mSelectPowerStorageCommand = VayuConnection.CreateCommand();
            mSelectPowerStorageCommand.CommandText = "select DateTime, PowerStorage from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectPowerStorageCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectPowerStorageCommand.Parameters.AddWithValue("@endDate", "endDate");
            //

            mSelectNaturalGasCommand = VayuConnection.CreateCommand();
            mSelectNaturalGasCommand.CommandText = "select DateTime, NaturalGas from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectNaturalGasCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectNaturalGasCommand.Parameters.AddWithValue("@endDate", "endDate");
            //mSelectSouthRenewableLoadsCommand

            mSelectCoalandLigniteCommand = VayuConnection.CreateCommand();
            mSelectCoalandLigniteCommand.CommandText = "select DateTime, Coalandlignite from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectCoalandLigniteCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectCoalandLigniteCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectNuclearCommand = VayuConnection.CreateCommand();
            mSelectNuclearCommand.CommandText = "select DateTime, Nuclear from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectNuclearCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectNuclearCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectOtherCommand = VayuConnection.CreateCommand();
            mSelectOtherCommand.CommandText = "select DateTime, Other from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectOtherCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectOtherCommand.Parameters.AddWithValue("@endDate", "endDate");

            mSelectTotalCommand = VayuConnection.CreateCommand();
            mSelectTotalCommand.CommandText = "select DateTime, (Solar+Wind+Hydro+PowerStorage+Other+NaturalGas+CoalandLignite+Nuclear) as Total  from FuelMix where DateTime> @startDate and DateTime<=@endDate order by DateTime";
            mSelectTotalCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectTotalCommand.Parameters.AddWithValue("@endDate", "endDate");
        }

        private List<LoadDataItem> GetSolarFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {

                    cmd.CommandText = mSelectSolarCommand.CommandText;


                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        //int hour = reader.GetInt32(1);
                        //DateTime sDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                        //DateTime hDate =sDate.AddHours(hour);
                        itemList.Add(new LoadDataItem
                        {

                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }
        private List<LoadDataItem> GetWindFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {

                    cmd.CommandText = mSelectWindCommand.CommandText;


                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {

                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }
        private List<LoadDataItem> GetHydroFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    cmd.CommandText = mSelectHydroCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }

        private List<LoadDataItem> GetPowerStorageFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    cmd.CommandText = mSelectPowerStorageCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }
        private List<LoadDataItem> GetNaturalGasFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    cmd.CommandText = mSelectNaturalGasCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }

        private List<LoadDataItem> GetCoalandLigniteFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    cmd.CommandText = mSelectCoalandLigniteCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }

        private List<LoadDataItem> GetNuclearFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    cmd.CommandText = mSelectNuclearCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }

        private List<LoadDataItem> GetOtherFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    cmd.CommandText = mSelectOtherCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }

        private List<LoadDataItem> GetTotalFromFuelMix(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    cmd.CommandText = mSelectTotalCommand.CommandText;

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                }
            }
            return itemList;
        }


        #endregion
    }
}
