using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Vayu.CommonAccessLibrary;

namespace Vayu.GenscapeContingencyUpdate
{
    /// <summary>
    /// 
    /// </summary>
    class ErcotContingencyUpdate
    {
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection VayuDbConn;
        /// <summary>
        /// The dt con
        /// </summary>
        DataTable dtCon = new DataTable();
        System.Timers.Timer sTimer = new System.Timers.Timer();
        DateTime mDate = new DateTime();
        /// <summary>
        /// Initializes a new instance of the <see cref="ErcotContingencyUpdate"/> class.
        /// </summary>
        public ErcotContingencyUpdate()
        {
            sTimer = new System.Timers.Timer();
            OnTimerEvent(null, null);
            sTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            sTimer.Interval = 300000;
            sTimer.Start();
            Console.WriteLine("Press \'q\' to quit.");
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
            
        }
        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            try
            {
                 DateTime sDate = DateTime.Today.AddDays(0);
                DateTime eDate = DateTime.Today.AddDays(1);
                while (sDate <= eDate)
                {
                    Console.WriteLine(sDate);
                    UpdateContingencyForMultipleDays(sDate);
                    sDate = sDate.AddDays(1);
                }
            }
            catch (Exception ex)
            {
            }
            sTimer.Enabled = true;
        }
        /// <summary>
        /// Gets the trading database connection.
        /// </summary>
        void GetDBConnection()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();

        }
        /// <summary>
        /// Use this function if want to update contingency Multiple Days
        /// </summary>
        public void UpdateContingencyForMultipleDays(DateTime dateTime)
        {
            try
            {
                GetDBConnection();
                dtCon.Rows.Clear();
                if (VayuDbConn.State == ConnectionState.Open)
                {
                    VayuDbConn.Close();
                }
                VayuDbConn.Open();
                SqlCommand mGetConstraintsCommand = new SqlCommand();
                mGetConstraintsCommand.Connection = VayuDbConn;
                mGetConstraintsCommand.CommandText = "select DISTINCT  ConstraintText, ContingencyText  from ConstraintRT WHERE MarketDateTime > '" + dateTime + "'  AND MarketDateTime <= '" + dateTime.AddDays(1) + "' "
                +" Union"
                + " select distinct MonitoredText, Contingency from ActiveConstraints WHERE MarketDateTime > '" + dateTime + "'  AND MarketDateTime <= '" + dateTime.AddDays(1) + "' ";
                SqlDataAdapter da = new SqlDataAdapter(mGetConstraintsCommand.CommandText, VayuDbConn);
                da.Fill(dtCon);
                VayuDbConn.Close();
                dtCon = dtCon.DefaultView.ToTable( /*distinct*/ true);
                for (int j = 0; j < dtCon.Rows.Count; j++)
                {
                    string ConstraintText = dtCon.Rows[j]["ConstraintText"].ToString();
                    string Equipment = "";
                    string ContingencyText = "";
                    string ConstraintName, Reason, Driver, Contingency;
                    ConstraintName = Reason = Driver = Contingency = string.Empty;
                    string[] valuearray, finalvalue;

                    valuearray = ConstraintText.Split('/');
                    finalvalue = valuearray[1].Split('-');
                    SqlCommand mGetContingencyCommand;

                    if (VayuDbConn.State == ConnectionState.Open)
                    {
                        VayuDbConn.Close();
                    }
                    VayuDbConn.Open();
                    mGetContingencyCommand = new SqlCommand();
                    if (finalvalue[0].Length > 0 && finalvalue[1].Length > 0)
                    {
                        mGetContingencyCommand.CommandText = "select CONCAT(EquipmentName , '_', EquipmentFromStationName,'_' ,EquipmentType ) as equipmentname  from Ercot_rt_outages where (EquipmentFromStationName like '%" + finalvalue[0] + "%' or EquipmentToStationName like '%" + finalvalue[1] + "%') " +
                            " and PlannedStartDate<= '" + dateTime + "' and PlannedEndtDate>= '" + dateTime + "'";

                        mGetContingencyCommand.Connection = VayuDbConn;

                        SqlDataReader rdr = mGetContingencyCommand.ExecuteReader();

                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                ConstraintName = ConstraintText;

                                Reason = "Outage";

                                Driver = rdr.GetValue(0).ToString();

                                Contingency = dtCon.Rows[j]["ContingencyText"].ToString();


                                if (Contingency != ContingencyText)
                                {
                                    if (Contingency != "")
                                    {
                                        // Insert new record

                                        if (VayuDbConn.State != ConnectionState.Open)
                                        {
                                            VayuDbConn.Open();
                                        }
                                        //VayuDbConn.Open();

                                        SqlCommand mInsertContingencyCommand = new SqlCommand();
                                        mInsertContingencyCommand.CommandText = "IF NOT EXISTS (SELECT * FROM ConstraintOutages WHERE ConstraintName = '" + ConstraintName + "' AND driver ='" + Driver + "' AND Contingency ='" + Contingency + "')"
                                            + "INSERT INTO ConstraintOutages(ConstraintName, Reason, Driver, Source, Contingency, InsertedDate)"
                                       + " VALUES ('" + ConstraintName + "', '" + Reason + "', '" + Driver + "', 'Input', '" + Contingency + "', '" + DateTime.Now + "') ";
                                        mInsertContingencyCommand.Connection = VayuDbConn;

                                        mInsertContingencyCommand.ExecuteNonQuery();
                                        //VayuDbConn.Close();
                                    }
                                    else
                                    {
                                        // Update Contingency

                                        if (VayuDbConn.State == ConnectionState.Open)
                                        {
                                            VayuDbConn.Close();
                                        }
                                        VayuDbConn.Open();

                                        SqlCommand cmdUpdateContingencyCommand = new SqlCommand();
                                        cmdUpdateContingencyCommand.CommandText = "UPDATE ConstraintOutages SET Contingency = '" + ContingencyText + "' "
                                                                                + " WHERE ConstraintName = '" + ConstraintName + "' "
                                                                                + " AND Driver = '" + Driver + "' "
                                                                                + " AND Source = 'Input' ";
                                        cmdUpdateContingencyCommand.Connection = VayuDbConn;

                                        cmdUpdateContingencyCommand.ExecuteNonQuery();
                                        VayuDbConn.Close();
                                    }
                                }
                            }

                            
                        }
                    }
                   
                }
                VayuDbConn.Close();
            }

            catch (Exception ex)
            {

            }
        }

    }
}
