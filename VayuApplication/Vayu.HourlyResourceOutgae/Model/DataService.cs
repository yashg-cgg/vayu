using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.HourlyResourceOutgae.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private static SqlConnection VayuConnection;



        #region SQL Commands

        private static SqlCommand mSelectSouthOutageLoadsCommand;
        private static SqlCommand mSelectSouthRenewableLoadsCommand;
        private static SqlCommand mSelectNorthOutageLoadsCommand;
        private static SqlCommand mSelectNorthRenewableLoadsCommand;
        private static SqlCommand mSelectWestOutageLoadsCommand;
        private static SqlCommand mSelectWestRenewableLoadsCommand;
        private static SqlCommand mSelectHoustonOutageLoadsCommand;
        private static SqlCommand mSelectHoustonRenewableLoadsCommand;
        private static SqlCommand mSelectTotalOutageLoadsCommand;
        private static SqlCommand mSelectTotalRenewableLoadsCommand;

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


        public void GetHourlyOutageData(Action<Dictionary<string, List<LoadDataItem>>, Exception> callback, DateTime fromDate, DateTime toDate, string zone)
        {
            loadDBCommands();

            if (true)
            {
                try
                {
                    Dictionary<string, List<LoadDataItem>> itemList = new Dictionary<string, List<LoadDataItem>>();
                    itemList.Add("South", GetSouthLoad(fromDate, toDate, zone));
                    itemList.Add("North", GetNorthLoad(fromDate, toDate, zone));
                    itemList.Add("West", GetWestLoad(fromDate, toDate, zone));
                    itemList.Add("Houston", GetHoustonLoad(fromDate, toDate, zone));
                    itemList.Add("Total", GetTotalLoad(fromDate, toDate, zone));
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
            //mSelectWestOutageLoadsCommand
            mSelectSouthOutageLoadsCommand = VayuConnection.CreateCommand();
            mSelectSouthOutageLoadsCommand.CommandText = "select Date, HourEnding, TotalResourceMWZoneSouth from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectSouthOutageLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectSouthOutageLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectNorthOutageLoadsCommand = VayuConnection.CreateCommand();
            mSelectNorthOutageLoadsCommand.CommandText = "select Date, HourEnding, TotalResourceMWZoneNorth from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectNorthOutageLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectNorthOutageLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");
            //
            mSelectWestOutageLoadsCommand = VayuConnection.CreateCommand();
            mSelectWestOutageLoadsCommand.CommandText = "select Date, HourEnding, TotalResourceMWZoneWest from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectWestOutageLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectWestOutageLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");
            //

            mSelectHoustonOutageLoadsCommand = VayuConnection.CreateCommand();
            mSelectHoustonOutageLoadsCommand.CommandText = "select Date, HourEnding, TotalResourceMWZoneHouston from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectHoustonOutageLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectHoustonOutageLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");
            //

            mSelectTotalOutageLoadsCommand = VayuConnection.CreateCommand();
            mSelectTotalOutageLoadsCommand.CommandText = "select Date, HourEnding, TotalResourceMWZoneSouth + TotalResourceMWZoneNorth + TotalResourceMWZoneWest + TotalResourceMWZoneHouston from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectTotalOutageLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectTotalOutageLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");
            //mSelectSouthRenewableLoadsCommand

            mSelectSouthRenewableLoadsCommand = VayuConnection.CreateCommand();
            mSelectSouthRenewableLoadsCommand.CommandText = "select Date, HourEnding, TotalIRRMWZoneSouth from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectSouthRenewableLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectSouthRenewableLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectNorthRenewableLoadsCommand = VayuConnection.CreateCommand();
            mSelectNorthRenewableLoadsCommand.CommandText = "select Date, HourEnding, TotalIRRMWZoneNorth from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectNorthRenewableLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectNorthRenewableLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectWestRenewableLoadsCommand = VayuConnection.CreateCommand();
            mSelectWestRenewableLoadsCommand.CommandText = "select Date, HourEnding, TotalIRRMWZoneWest from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectWestRenewableLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectWestRenewableLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectHoustonRenewableLoadsCommand = VayuConnection.CreateCommand();
            mSelectHoustonRenewableLoadsCommand.CommandText = "select Date, HourEnding, TotalIRRMWZoneHouston from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectHoustonRenewableLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectHoustonRenewableLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
            mSelectTotalRenewableLoadsCommand = VayuConnection.CreateCommand();
            mSelectTotalRenewableLoadsCommand.CommandText = "select Date, HourEnding, TotalIRRMWZoneSouth + TotalIRRMWZoneNorth + TotalIRRMWZoneWest + TotalIRRMWZoneHouston from Hourlyoutages where Date> @startDate and Date<=@endDate order by Date, HourEnding";
            mSelectTotalRenewableLoadsCommand.Parameters.AddWithValue("@startDate", "startDate");
            mSelectTotalRenewableLoadsCommand.Parameters.AddWithValue("@endDate", "endDate");

            //
        }

        private List<LoadDataItem> GetSouthLoad(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())//mSelectSouthRenewableLoadsCommand
                {
                    if (zone == "Outages")
                    {
                        cmd.CommandText = mSelectSouthOutageLoadsCommand.CommandText;

                    }
                    else
                    {
                        cmd.CommandText = mSelectSouthRenewableLoadsCommand.CommandText;
                    }

                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int hour = reader.GetInt32(1);
                        DateTime sDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                        DateTime hDate = sDate.AddHours(hour);
                        itemList.Add(new LoadDataItem
                        {

                            MarketDateTime = hDate,
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(2)),
                        });
                    }

                }

            }



            return itemList;

        }//mSelectNorthOutageLoadsCommand

        private List<LoadDataItem> GetNorthLoad(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    if (zone == "Outages")
                    {
                        cmd.CommandText = mSelectNorthOutageLoadsCommand.CommandText;

                    }
                    else
                    {
                        cmd.CommandText = mSelectNorthRenewableLoadsCommand.CommandText;
                    }
                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));

                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int hour = reader.GetInt32(1);
                        DateTime sDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                        DateTime hDate = sDate.AddHours(hour);
                        itemList.Add(new LoadDataItem
                        {

                            MarketDateTime = hDate,
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(2)),
                        });
                    }

                }

            }



            return itemList;

        }//mSelectWestOutageLoadsCommand

        private List<LoadDataItem> GetWestLoad(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    if (zone == "Outages")
                    {
                        cmd.CommandText = mSelectWestOutageLoadsCommand.CommandText;

                    }
                    else
                    {
                        cmd.CommandText = mSelectWestRenewableLoadsCommand.CommandText;
                    }
                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));

                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int hour = reader.GetInt32(1);
                        DateTime sDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                        DateTime hDate = sDate.AddHours(hour);
                        itemList.Add(new LoadDataItem
                        {

                            MarketDateTime = hDate,
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(2)),
                        });
                    }

                }

            }


            return itemList;

        }

        private List<LoadDataItem> GetHoustonLoad(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    if (zone == "Outages")
                    {
                        cmd.CommandText = mSelectHoustonOutageLoadsCommand.CommandText;

                    }
                    else
                    {
                        cmd.CommandText = mSelectHoustonRenewableLoadsCommand.CommandText;
                    }
                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));

                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int hour = reader.GetInt32(1);
                        DateTime sDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                        DateTime hDate = sDate.AddHours(hour);
                        itemList.Add(new LoadDataItem
                        {

                            MarketDateTime = hDate,
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(2)),
                        });
                    }

                }

            }

            return itemList;

        }
        private List<LoadDataItem> GetTotalLoad(DateTime fromDate, DateTime toDate, string zone)
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    if (zone == "Outages")
                    {
                        cmd.CommandText = mSelectTotalOutageLoadsCommand.CommandText;

                    }
                    else
                    {
                        cmd.CommandText = mSelectTotalRenewableLoadsCommand.CommandText;
                    }
                    cmd.Parameters.Add(new SqlParameter("@startdate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", toDate));

                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int hour = reader.GetInt32(1);
                        DateTime sDate = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0);
                        DateTime hDate = sDate.AddHours(hour);
                        itemList.Add(new LoadDataItem
                        {

                            MarketDateTime = hDate,
                            LoadForecast = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(2)),
                        });
                    }

                }

            }

            return itemList;

        }


        #endregion
    }
}
