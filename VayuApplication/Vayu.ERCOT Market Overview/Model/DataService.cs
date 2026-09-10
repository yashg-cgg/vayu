using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.ERCOT_Market_Overview.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.Admin.Model.IDataService" />
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;

        private SqlCommand mSelectDAVolumesCommand;
        private SqlCommand mSelectDAVolumesNodeCommand;
        private SqlCommand mSelectDAVolumesBetweenCommand;
        private SqlCommand mSelectRTEnergyCommand;
        private SqlCommand mSelectDAEnergyCommand;
        private SqlCommand mSelectDAEnergyCommandBetween;
        private SqlCommand mSelectRTEnergyCommandBetween;
        public void LoadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();


            mSelectDAVolumesCommand = new SqlCommand();
            mSelectDAVolumesCommand.CommandText = "select a.NodeKey,a.DELIVERYDATE,a.HOURENDING,a.STL_PNT,a.TOTAL_PTP_OBL_AWARDED_SOURCE,a.TOTAL_PTP_OBL_AWARDED_SINK,a.DSTFlag,b.Zone,c.FuelSource " +
                "from DAMPTPObligation a inner join Node b on a.nodekey=b.NodeKey left join ErcotNodeFuelSource c on b.NodeKey=c.NodeKey where DeliveryDate=@Deliverydate order by DeliveryDate, HOURENDING, STL_PNT";
            mSelectDAVolumesCommand.Parameters.AddWithValue("@Deliverydate", "Deliverydate");
            mSelectDAVolumesCommand.Connection = VayuConnection;

            mSelectDAVolumesNodeCommand = new SqlCommand();
            mSelectDAVolumesNodeCommand.CommandText = "select a.NodeKey,a.DELIVERYDATE,a.HOURENDING,a.STL_PNT,a.TOTAL_PTP_OBL_AWARDED_SOURCE,a.TOTAL_PTP_OBL_AWARDED_SINK,a.DSTFlag,b.Zone,c.FuelSource " +
                "from DAMPTPObligation a inner join Node b on a.nodekey=b.NodeKey left join ErcotNodeFuelSource c on b.NodeKey=c.NodeKey where DeliveryDate=@Deliverydate and STL_PNT= @STL_PNT order by DeliveryDate, HOURENDING, STL_PNT";
            mSelectDAVolumesNodeCommand.Parameters.AddWithValue("@Deliverydate", "Deliverydate");
            mSelectDAVolumesNodeCommand.Parameters.AddWithValue("@STL_PNT", "STL_PNT");
            mSelectDAVolumesNodeCommand.Connection = VayuConnection;

            mSelectDAVolumesBetweenCommand = new SqlCommand();
            mSelectDAVolumesBetweenCommand.CommandText = "select a.NodeKey,a.DELIVERYDATE,a.HOURENDING,a.STL_PNT,a.TOTAL_PTP_OBL_AWARDED_SOURCE,a.TOTAL_PTP_OBL_AWARDED_SINK,a.DSTFlag,b.Zone,c.FuelSource " +
                "from DAMPTPObligation a inner join Node b on a.nodekey=b.NodeKey left join ErcotNodeFuelSource c on b.NodeKey=c.NodeKey where DeliveryDate Between @Deliverydate and @Deliverydate1 and STL_PNT= @STL_PNT order by DeliveryDate, HOURENDING, STL_PNT";
            mSelectDAVolumesBetweenCommand.Parameters.AddWithValue("@Deliverydate", "Deliverydate");
            mSelectDAVolumesBetweenCommand.Parameters.AddWithValue("@Deliverydate1", "Deliverydate");
            mSelectDAVolumesBetweenCommand.Parameters.AddWithValue("@STL_PNT", "STL_PNT");
            mSelectDAVolumesBetweenCommand.Connection = VayuConnection;

            mSelectRTEnergyCommand = new SqlCommand();
            mSelectRTEnergyCommand.CommandText = "select ROW_NUMBER() Over(Order by (DeliveryDate) )  AS DeliveryDate , SystemLambda from Vayu.[dbo].[RTSystemLambdaHourly] " +
                "where deliverydate >= @Deliverydate and deliverydate < @Deliverydate1  order by deliverydate";
            mSelectRTEnergyCommand.Parameters.AddWithValue("@Deliverydate", "Deliverydate");
            mSelectRTEnergyCommand.Parameters.AddWithValue("@Deliverydate1", "Deliverydate");
            mSelectRTEnergyCommand.Connection = VayuConnection;

            mSelectDAEnergyCommand = new SqlCommand();
            mSelectDAEnergyCommand.CommandText = "select ROW_NUMBER() Over(Order by (DeliveryDate) ) AS DeliveryDate, SystemLambda from DAMSystemLambda " +
                "where deliverydate > @Deliverydate and deliverydate <= @Deliverydate1 and DSTFlag='N' order by DeliveryDate";
            mSelectDAEnergyCommand.Parameters.AddWithValue("@Deliverydate", "Deliverydate");
            mSelectDAEnergyCommand.Parameters.AddWithValue("@Deliverydate1", "Deliverydate");
            mSelectDAEnergyCommand.Connection = VayuConnection;

            mSelectDAEnergyCommandBetween = new SqlCommand();
            mSelectDAEnergyCommandBetween.CommandText = "select  DeliveryDate, SystemLambda from DAMSystemLambda " +
                "where deliverydate > @Deliverydate and deliverydate <= @Deliverydate1 and DSTFlag='N' order by DeliveryDate";
            mSelectDAEnergyCommandBetween.Parameters.AddWithValue("@Deliverydate", "Deliverydate");
            mSelectDAEnergyCommandBetween.Parameters.AddWithValue("@Deliverydate1", "Deliverydate");
            mSelectDAEnergyCommandBetween.Connection = VayuConnection;//mSelectDAEnergyCommandBetween

            mSelectRTEnergyCommandBetween = new SqlCommand();
            mSelectRTEnergyCommandBetween.CommandText = "select  DeliveryDate, SystemLambda from RTSystemLambdaHourly " +
                "where deliverydate >= @Deliverydate and deliverydate < @Deliverydate1 order by DeliveryDate";
            mSelectRTEnergyCommandBetween.Parameters.AddWithValue("@Deliverydate", "Deliverydate");
            mSelectRTEnergyCommandBetween.Parameters.AddWithValue("@Deliverydate1", "Deliverydate");
            mSelectRTEnergyCommandBetween.Connection = VayuConnection;
        }

        public List<DAVolumes> GetDAVolumes(DateTime date)
        {
            LoadDBCommands();
            List<DAVolumes> mDAVolumesList = new List<DAVolumes>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectDAVolumesCommand.Parameters["@Deliverydate"].Value = date;
            SqlDataReader reader = mSelectDAVolumesCommand.ExecuteReader();
            while (reader.Read())
            {
                DAVolumes mDAVolumes = new DAVolumes();
                mDAVolumes.NodeKey = (int)reader.GetValue(0);
                mDAVolumes.DeliveryYDate = reader.GetDateTime(1);
                mDAVolumes.HourEnding = (int)reader.GetDecimal(2);
                mDAVolumes.STLPNT = reader.GetString(3);
                mDAVolumes.TOTAL_PTP_OBL_AWARDED_SOURCE = Convert.ToDouble(reader.GetValue(4));
                mDAVolumes.TOTAL_PTP_OBL_AWARDED_SINK = Convert.ToDouble(reader.GetValue(5));
                mDAVolumes.DSTFlag = reader.GetString(6);
                mDAVolumes.Zone = (reader.IsDBNull(7)) ? "" : reader.GetString(7);
                mDAVolumes.Fuelsource = (reader.IsDBNull(8)) ? "" : reader.GetString(8);


                mDAVolumesList.Add(mDAVolumes);
            }
            reader.Close();
            VayuConnection.Close();
            return mDAVolumesList;
        }

        public List<DataItem> GetRTEnergy(DateTime date)
        {
            LoadDBCommands();
            List<DataItem> vRTEnergyList = new List<DataItem>();
            DateTime startDate = date;
            DateTime endDate = date.AddDays(1);
            if (VayuConnection.State == ConnectionState.Open)
                VayuConnection.Close();
            VayuConnection.Open();
            mSelectRTEnergyCommand.Parameters["@Deliverydate"].Value = startDate;
            mSelectRTEnergyCommand.Parameters["@Deliverydate1"].Value = endDate;
            SqlDataReader reader = mSelectRTEnergyCommand.ExecuteReader();
            while (reader.Read())
            {
                DataItem vRTEnergy = new DataItem();

                vRTEnergy.EnergyHourEnding = Convert.ToInt32(reader.GetValue(0));
                vRTEnergy.SystemLambda = Convert.ToDouble(reader.GetValue(1));
                vRTEnergyList.Add(vRTEnergy);
            }
            return vRTEnergyList;
        }

        public List<DataItem> GetDAEnergy(DateTime date)
        {
            LoadDBCommands();
            List<DataItem> vDAEnergyList = new List<DataItem>();
            DateTime startDate = date;
            DateTime endDate = date.AddDays(1);
            if (VayuConnection.State == ConnectionState.Open)
                VayuConnection.Close();
            VayuConnection.Open();
            mSelectDAEnergyCommand.Parameters["@Deliverydate"].Value = startDate;
            mSelectDAEnergyCommand.Parameters["@Deliverydate1"].Value = endDate;
            SqlDataReader reader = mSelectDAEnergyCommand.ExecuteReader();
            while (reader.Read())
            {
                DataItem vRTEnergy = new DataItem();

                vRTEnergy.EnergyHourEnding = Convert.ToInt32(reader.GetValue(0));
                vRTEnergy.SystemLambda = Convert.ToDouble(reader.GetValue(1));
                vDAEnergyList.Add(vRTEnergy);
            }
            return vDAEnergyList;
        }

        public List<BetweenDataItem> GetDAEnergyBetween(DateTime sdate, DateTime edate)
        {
            LoadDBCommands();
            List<BetweenDataItem> vDAEnergyList = new List<BetweenDataItem>();
            DateTime startDate = sdate;
            DateTime endDate = edate.AddDays(1);
            if (VayuConnection.State == ConnectionState.Open)
                VayuConnection.Close();
            VayuConnection.Open();
            mSelectDAEnergyCommandBetween.Parameters["@Deliverydate"].Value = startDate;
            mSelectDAEnergyCommandBetween.Parameters["@Deliverydate1"].Value = endDate;
            SqlDataReader reader = mSelectDAEnergyCommandBetween.ExecuteReader();
            while (reader.Read())
            {
                BetweenDataItem vRTEnergy = new BetweenDataItem();
                DateTime tempDate = reader.GetDateTime(0);
                //vRTEnergy.daDate = reader.GetDateTime(0);
                if (tempDate.Hour == 0)
                {
                    vRTEnergy.daDate = tempDate.AddDays(-1);
                    vRTEnergy.EnergyHourEnding = 24;
                }
                else
                {
                    vRTEnergy.daDate = tempDate;
                    vRTEnergy.EnergyHourEnding = vRTEnergy.daDate.Hour;

                }
                vRTEnergy.SystemLambda = Convert.ToDouble(reader.GetValue(1));
                vDAEnergyList.Add(vRTEnergy);
            }
            return vDAEnergyList;
        }

        public List<BetweenDataItem> GetRTEnergyBetween(DateTime sdate, DateTime edate)
        {
            LoadDBCommands();
            List<BetweenDataItem> vDAEnergyList = new List<BetweenDataItem>();
            DateTime startDate = sdate;
            DateTime endDate = edate.AddDays(1);
            if (VayuConnection.State == ConnectionState.Open)
                VayuConnection.Close();
            VayuConnection.Open();
            mSelectRTEnergyCommandBetween.Parameters["@Deliverydate"].Value = startDate;
            mSelectRTEnergyCommandBetween.Parameters["@Deliverydate1"].Value = endDate;
            SqlDataReader reader = mSelectRTEnergyCommandBetween.ExecuteReader();
            while (reader.Read())
            {
                BetweenDataItem vRTEnergy = new BetweenDataItem();
                DateTime tempDate = reader.GetDateTime(0);
                //vRTEnergy.daDate = reader.GetDateTime(0);
                if (tempDate.Hour == 23)
                {
                    vRTEnergy.daDate = tempDate.AddDays(-1).AddHours(1);
                    vRTEnergy.EnergyHourEnding = 24;
                }
                else
                {
                    vRTEnergy.daDate = tempDate.AddHours(1);
                    vRTEnergy.EnergyHourEnding = vRTEnergy.daDate.Hour;

                }
                vRTEnergy.SystemLambda = Convert.ToDouble(reader.GetValue(1));
                vDAEnergyList.Add(vRTEnergy);
            }
            return vDAEnergyList;
        }

        public List<DAVolumes> GetDAVolumesBetween(DateTime date, DateTime date1, string Stlpnt)
        {

            LoadDBCommands();
            List<DAVolumes> mDAVolumesList = new List<DAVolumes>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectDAVolumesBetweenCommand.Parameters["@Deliverydate"].Value = date;
            mSelectDAVolumesBetweenCommand.Parameters["@Deliverydate1"].Value = date1;
            mSelectDAVolumesBetweenCommand.Parameters["@STL_PNT"].Value = Stlpnt;
            SqlDataReader reader = mSelectDAVolumesBetweenCommand.ExecuteReader();
            while (reader.Read())
            {
                DAVolumes mDAVolumes = new DAVolumes();
                mDAVolumes.NodeKey = (int)reader.GetValue(0);
                mDAVolumes.DeliveryYDate = reader.GetDateTime(1);
                mDAVolumes.HourEnding = (int)reader.GetDecimal(2);
                mDAVolumes.STLPNT = reader.GetString(3);
                mDAVolumes.TOTAL_PTP_OBL_AWARDED_SOURCE = Convert.ToDouble(reader.GetValue(4));
                mDAVolumes.TOTAL_PTP_OBL_AWARDED_SINK = Convert.ToDouble(reader.GetValue(5));
                mDAVolumes.DSTFlag = reader.GetString(6);
                mDAVolumes.Zone = (reader.IsDBNull(7)) ? "" : reader.GetString(7);
                mDAVolumes.Fuelsource = (reader.IsDBNull(8)) ? "" : reader.GetString(8);


                mDAVolumesList.Add(mDAVolumes);
            }
            reader.Close();
            VayuConnection.Close();
            return mDAVolumesList;
        }

        public List<string> GetValidPTPList(DateTime StartDate)
        {
            LoadDBCommands();
            List<string> validPTPs = new List<string>();

            string sql = @"
        SELECT DISTINCT nd.NodeName
        FROM Vayu..NodeDALMPH n
        JOIN Vayu..EESPathList e ON n.NodeKey = e.SourceNodeKey
        JOIN Vayu..Node nd ON n.NodeKey = nd.NodeKey
        WHERE n.MarketDateTime = @Date
          AND e.SourceNodeKey IS NOT NULL";

            using (SqlCommand cmd = new SqlCommand(sql, VayuConnection))
            {
                // Match parameter name to query
                cmd.Parameters.AddWithValue("@Date", StartDate);

                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Column index 0 is NodeName from SELECT
                        validPTPs.Add(reader.GetString(0));
                    }
                }

                VayuConnection.Close();
            }

            return validPTPs;
        }


    }
}
