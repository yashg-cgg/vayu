using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.RenewableOutageGraph.Model
{
    public class DataService : IDataService
    {
        private static SqlConnection VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

        public List<RenewableOutage> GetRenewableOutage(DateTime? sdate, DateTime? edate)
        {
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            SqlDataReader reader = null;
            List<RenewableOutage> RenewableOutageList = new List<RenewableOutage>();
            using (SqlCommand mSelectRenewableOutageCommand = VayuConnection.CreateCommand())
            {
                mSelectRenewableOutageCommand.CommandText = "SELECT solar + wind AS sum, datetime FROM FuelMix WHERE datetime BETWEEN @StartDate AND DATEADD(SECOND, -1, DATEADD(DAY, 1, @EndDate))";
                mSelectRenewableOutageCommand.Parameters.AddWithValue("@StartDate", sdate);
                mSelectRenewableOutageCommand.Parameters.AddWithValue("@EndDate", edate);

                reader = mSelectRenewableOutageCommand.ExecuteReader();

                while (reader.Read())
                {
                    RenewableOutage renewableOutage = new RenewableOutage();
                    renewableOutage.MW = Convert.ToDouble(reader["sum"]);
                    renewableOutage.Date = Convert.ToDateTime(reader["datetime"]);

                    RenewableOutageList.Add(renewableOutage);
                }
            }

            return RenewableOutageList;
        }
    }
}
