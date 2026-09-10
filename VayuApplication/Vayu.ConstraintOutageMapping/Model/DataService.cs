using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.ConstraintOutageMapping.ViewModels;
namespace Vayu.ConstraintOutageMapping.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The m connection
        /// </summary>
        private SqlConnection VayuConnection;
        /// <summary>
        /// The m constraints command
        /// </summary>
        private SqlCommand mConstraintsCommand;
        /// <summary>
        /// The m constraint command
        /// </summary>
        private SqlCommand mConstraintCommand;
        private SqlCommand mAllConstraintCommand;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the constraint list.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<Constraints> GetConstraintList(DateTime startDate, DateTime endDate)
        {
            List<Constraints> ConstraintList = new List<Constraints>();
            try
            {
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (mConstraintsCommand = VayuConnection.CreateCommand())
                    {
                        // mConstraintsCommand.Connection.Open();

                        {
                            mConstraintsCommand.CommandText = "Select distinct ConstraintText from  ConstraintRT where MarketDateTime >= @StartDate and MarketDateTime<@EndDate and ConstraintText !='None'";
                        }
                        // mConstraintsCommand.CommandText = "Select distinct ConstraintText, ContingencyText from PJM.ConstraintRT where MarketDateTime >@StartDate and MarketDateTime<@EndDate";
                        mConstraintsCommand.Parameters.AddWithValue("@StartDate", startDate);
                        mConstraintsCommand.Parameters.AddWithValue("@EndDate", endDate);
                        SqlDataReader reader = mConstraintsCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            ConstraintList.Add(new Constraints
                            {
                                ConstraintName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)),
                                // ContingencyText = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1))
                            });

                        }
                        reader.Close();
                        mConstraintsCommand.Connection.Close();
                    }
                }

                return ConstraintList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the contingency list.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="constraintText">The constraint text.</param>
        /// <returns></returns>
        public List<string> GetContingencyList(DateTime startDate, DateTime endDate, string constraintText)
        {
            List<string> ContingencyTextList = new List<string>();
            try
            {
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (mConstraintsCommand = VayuConnection.CreateCommand())
                    {
                        // mConstraintsCommand.Connection.Open();

                        {
                            mConstraintsCommand.CommandText = "Select distinct ContingencyText from  ConstraintRT where ConstraintText=@ConstraintText and MarketDateTime >@StartDate and MarketDateTime<@EndDate";
                        }
                        mConstraintsCommand.Parameters.AddWithValue("@ConstraintText", constraintText);
                        mConstraintsCommand.Parameters.AddWithValue("@StartDate", startDate);
                        mConstraintsCommand.Parameters.AddWithValue("@EndDate", endDate);

                        SqlDataReader reader = mConstraintsCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            Constraints con = new Constraints();
                            ContingencyTextList.Add(reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)));

                        }
                        reader.Close();
                        mConstraintsCommand.Connection.Close();
                    }
                }

                return ContingencyTextList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the outage data.
        /// </summary>
        /// <param name="Selectedconstraint">The selectedconstraint.</param>
        /// <param name="SelectedContingency">The selected contingency.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="Iscurrent">if set to <c>true</c> [iscurrent].</param>
        /// <param name="IsStartChecked">if set to <c>true</c> [is start checked].</param>
        /// <returns></returns>
        public List<ConstraintOutages> GetOutageData(string Selectedconstraint, string SelectedContingency, DateTime startDate, DateTime endDate, bool Iscurrent, bool IsStartChecked, bool isAllConstaintchecked)
        {
            List<ConstraintOutages> ConstraintList = new List<ConstraintOutages>();
            try
            {
                if (Iscurrent)
                {
                    using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        SqlDataReader reader = null;
                        if (isAllConstaintchecked)
                        {
                            using (mAllConstraintCommand = VayuConnection.CreateCommand())
                            {
                                // mAllConstraintCommand.Connection.Open();

                                {
                                    mAllConstraintCommand.CommandText = " SELECT DISTINCT b.Driver,A.OutageIdentifier , A.EquipmentFromStationName as FromSub, A.EquipmentToStationName as ToSub,'Zone', A.EquipmentType, A.VoltageLevel, A.OutageStatus, " +
                                                                           " A.ActualStartDate as ActualStart,A.ActualEndDate as ActualEnd, DATEDIFF(day, a.ActualStartDate, a.ActualEndDate) as PlannedDuration,  " +
                                                                           "  A.OutageType,b.ConstraintName,b.Contingency FROM  Ercot_rt_outages a JOIN  ConstraintOutages  " +
                                                                           "  b ON CONCAT(a.EquipmentName, '_', a.EquipmentFromStationName, '_', EquipmentType) = b.Driver  " +
                                                                           " WHERE(a.PlannedStartDate >= @startDate  and a.PlannedStartDate <= @endDate)  and(a.PlannedEndtDate >= @endDate or a.PlannedEndtDate is null)";
                                }
                                mAllConstraintCommand.Parameters.AddWithValue("@StartDate", startDate);
                                mAllConstraintCommand.Parameters.AddWithValue("@EndDate", endDate);
                                reader = mAllConstraintCommand.ExecuteReader();
                            }
                        }
                        else
                        {
                            using (mConstraintCommand = VayuConnection.CreateCommand())
                            {
                                // mConstraintCommand.Connection.Open();

                                {
                                    mConstraintCommand.CommandText = " SELECT DISTINCT b.Driver,A.OutageIdentifier , A.EquipmentFromStationName as FromSub, A.EquipmentToStationName as ToSub,'Zone', A.EquipmentType, A.VoltageLevel, A.OutageStatus, " +
                                                                           " A.ActualStartDate as ActualStart,A.ActualEndDate as ActualEnd, DATEDIFF(day, a.ActualStartDate, a.ActualEndDate) as PlannedDuration,  " +
                                                                           "  A.OutageType,b.ConstraintName,b.Contingency FROM  Ercot_rt_outages a JOIN  ConstraintOutages  " +
                                                                           "  b ON CONCAT(a.EquipmentName, '_', a.EquipmentFromStationName, '_', EquipmentType) = b.Driver  " +
                                                                           " WHERE a.PlannedStartDate <=  @endDate and (a.PlannedEndtDate >= @startDate or a.PlannedEndtDate is null)  " +
                                                                           " and b.ConstraintName=@Constraint " +
                                                                           " and b.Contingency=@Contingency";
                                }
                                mConstraintCommand.Parameters.AddWithValue("@Constraint", Selectedconstraint);
                                mConstraintCommand.Parameters.AddWithValue("@Contingency", SelectedContingency);
                                mConstraintCommand.Parameters.AddWithValue("@StartDate", startDate);
                                mConstraintCommand.Parameters.AddWithValue("@EndDate", endDate);
                                reader = mConstraintCommand.ExecuteReader();
                            }
                        }
                        while (reader.Read())
                        {
                            ConstraintOutages mConstraintOutages = new ConstraintOutages();
                            // ConstraintList.Add(new ConstraintOutages
                            {
                                mConstraintOutages.OutageName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                                mConstraintOutages.TicketID = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1));
                                mConstraintOutages.Branch = reader.IsDBNull(2) ? "" : Convert.ToString(reader.GetValue(2));
                                mConstraintOutages.ToBranch = reader.IsDBNull(3) ? "" : Convert.ToString(reader.GetValue(3));
                                mConstraintOutages.Zone = reader.IsDBNull(4) ? "" : Convert.ToString(reader.GetValue(4));
                                mConstraintOutages.EquipmentType = reader.IsDBNull(5) ? "" : Convert.ToString(reader.GetValue(5));
                                mConstraintOutages.Voltage = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));
                                mConstraintOutages.OutageStatus = reader.IsDBNull(7) ? "" : Convert.ToString(reader.GetValue(7));
                                mConstraintOutages.Startdate = reader.IsDBNull(8) ? new DateTime() : Convert.ToDateTime(reader.GetValue(8));
                                DateTime? dt = null;
                                mConstraintOutages.Enddate = reader.IsDBNull(9) ? dt : Convert.ToDateTime(reader.GetValue(9));
                                mConstraintOutages.OutageDuration = reader.IsDBNull(10) ? 0 : Convert.ToInt32(reader.GetValue(10));
                                mConstraintOutages.OutageType = reader.IsDBNull(11) ? "" : Convert.ToString(reader.GetValue(11));
                                mConstraintOutages.ConstraintName = reader.IsDBNull(12) ? "" : Convert.ToString(reader.GetValue(12));
                                mConstraintOutages.Contingency = reader.IsDBNull(13) ? "" : Convert.ToString(reader.GetValue(13));
                            }
                            ConstraintList.Add(mConstraintOutages);
                        }
                        reader.Close();
                        if (isAllConstaintchecked)
                            mAllConstraintCommand.Connection.Close();
                        else
                            mConstraintCommand.Connection.Close();
                    }
                }
                else
                {
                    using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        SqlDataReader reader = null;
                        if (isAllConstaintchecked)
                        {
                            using (mAllConstraintCommand = VayuConnection.CreateCommand())
                            {
                                //   mAllConstraintCommand.Connection.Open();
                                mAllConstraintCommand.CommandText = " SELECT DISTINCT b.Driver,A.TicketID , A.Branch as FromSub, A.ToBranch as ToSub,A.Zone, A.EquipmentType, A.Voltage, A.OutageStatus,   " +
                                                                    " A.startdate as startdate,A.enddate as ActualEnd, DATEDIFF(day, a.startdate,a.enddate) as PlannedDuration, A.OutageType,b.ConstraintName, " +
                                                                    " b.Contingency   FROM PJM_rt_outages a  " +
                                                                    " JOIN PJM.ConstraintOutages b ON a.Equipment = b.Driver    " +
                                                                    " WHERE ( a.startdate >= @startDate and a.StartDate <= @endDate) and (a.enddate >= @endDate or a.enddate is null) ";
                                mAllConstraintCommand.Parameters.AddWithValue("@StartDate", startDate);
                                mAllConstraintCommand.Parameters.AddWithValue("@EndDate", endDate);
                                reader = mAllConstraintCommand.ExecuteReader();
                            }
                        }
                        else
                        {
                            using (mConstraintCommand = VayuConnection.CreateCommand())
                            {
                                // mConstraintCommand.Connection.Open();
                                mConstraintCommand.CommandText = "SELECT DISTINCT b.Driver,A.TicketID , A.Branch as FromSub, A.ToBranch as ToSub,A.Zone, " +
                                                                    "A.EquipmentType, A.Voltage, A.OutageStatus, A.startdate as startdate,A.enddate as ActualEnd, " +
                                                                    "DATEDIFF(day, a.startdate,a.enddate) as PlannedDuration, A.OutageType ,b.ConstraintName,b.Contingency " +
                                                                    " FROM PJM_rt_outages a JOIN PJM.ConstraintOutages b " +
                                                                    "ON a.Equipment = b.Driver " +
                                                                    "WHERE b.ConstraintName=@Constraint and b.Contingency=@Contingency " +
                                                                    "and a.startdate <= @EndDate and (a.enddate >= @StartDate or a.enddate is null)";
                                mConstraintCommand.Parameters.AddWithValue("@Constraint", Selectedconstraint);
                                mConstraintCommand.Parameters.AddWithValue("@Contingency", SelectedContingency);
                                mConstraintCommand.Parameters.AddWithValue("@StartDate", startDate);
                                mConstraintCommand.Parameters.AddWithValue("@EndDate", endDate);
                                reader = mConstraintCommand.ExecuteReader();
                            }
                        }
                        while (reader.Read())
                        {
                            ConstraintOutages mConstraintOutages = new ConstraintOutages();
                            // ConstraintList.Add(new ConstraintOutages
                            {
                                mConstraintOutages.OutageName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                                mConstraintOutages.TicketID = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1));
                                mConstraintOutages.Branch = reader.IsDBNull(2) ? "" : Convert.ToString(reader.GetValue(2));
                                mConstraintOutages.ToBranch = reader.IsDBNull(3) ? "" : Convert.ToString(reader.GetValue(3));
                                mConstraintOutages.Zone = reader.IsDBNull(4) ? "" : Convert.ToString(reader.GetValue(4));
                                mConstraintOutages.EquipmentType = reader.IsDBNull(5) ? "" : Convert.ToString(reader.GetValue(5));
                                mConstraintOutages.Voltage = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));
                                mConstraintOutages.OutageStatus = reader.IsDBNull(7) ? "" : Convert.ToString(reader.GetValue(7));
                                mConstraintOutages.Startdate = reader.IsDBNull(8) ? new DateTime() : Convert.ToDateTime(reader.GetValue(8));
                                mConstraintOutages.Enddate = reader.IsDBNull(9) ? new DateTime() : Convert.ToDateTime(reader.GetValue(9));
                                mConstraintOutages.OutageDuration = reader.IsDBNull(10) ? 0 : Convert.ToInt32(reader.GetValue(10));
                                mConstraintOutages.OutageType = reader.IsDBNull(11) ? "" : Convert.ToString(reader.GetValue(11));
                                mConstraintOutages.ConstraintName = reader.IsDBNull(12) ? "" : Convert.ToString(reader.GetValue(12));
                                mConstraintOutages.Contingency = reader.IsDBNull(13) ? "" : Convert.ToString(reader.GetValue(13));
                            }
                            ConstraintList.Add(mConstraintOutages);
                        }
                        reader.Close();
                        if (isAllConstaintchecked)
                            mAllConstraintCommand.Connection.Close();
                        else
                            mConstraintCommand.Connection.Close();
                    }
                }
                return ConstraintList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all outage data.
        /// </summary>
        /// <param name="Selectedconstraint">The selectedconstraint.</param>
        /// <param name="SelectedContingency">The selected contingency.</param>
        /// <returns></returns>
        public List<ConstraintOutages> GetAllOutageData(string Selectedconstraint, string SelectedContingency, bool isAllConstaintchecked, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                List<ConstraintOutages> ConstraintList = new List<ConstraintOutages>();
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    SqlDataReader reader = null;
                    if (isAllConstaintchecked)
                    {
                        using (mAllConstraintCommand = VayuConnection.CreateCommand())
                        {
                            // mAllConstraintCommand.Connection.Open();

                            {
                                mAllConstraintCommand.CommandText = " SELECT DISTINCT b.Driver,A.OutageIdentifier , A.EquipmentFromStationName as FromSub, A.EquipmentToStationName as ToSub,'Zone', A.EquipmentType, A.VoltageLevel, A.OutageStatus,  " +
                                                    " A.ActualStartDate as ActualStart,A.ActualEndDate as ActualEnd, DATEDIFF(day, a.ActualStartDate, a.ActualEndDate) as PlannedDuration,  " +
                                                    " A.OutageType,b.ConstraintName,b.Contingency  " +
                                                    " FROM  Ercot_rt_outages a JOIN  ConstraintOutages b ON CONCAT(a.EquipmentName, '_', a.EquipmentFromStationName, '_', EquipmentType) = b.Driver  " +
                                                    " where(a.PlannedStartDate >= @StartDate and a.PlannedStartDate <= @EndDate)  " +
                                                    " and(a.PlannedEndtDate > @EndDate or a.PlannedEndtDate is null)";
                            }
                            mAllConstraintCommand.Parameters.AddWithValue("@StartDate", StartDate);
                            mAllConstraintCommand.Parameters.AddWithValue("@EndDate", EndDate);
                            reader = mAllConstraintCommand.ExecuteReader();
                        }
                    }
                    else
                    {
                        using (mConstraintCommand = VayuConnection.CreateCommand())
                        {
                            //  mConstraintCommand.Connection.Open();

                            {
                                mConstraintCommand.CommandText = " SELECT DISTINCT b.Driver,A.OutageIdentifier , A.EquipmentFromStationName as FromSub, A.EquipmentToStationName as ToSub,'Zone', A.EquipmentType, A.VoltageLevel, A.OutageStatus,  " +
                                                    " A.ActualStartDate as ActualStart,A.ActualEndDate as ActualEnd, DATEDIFF(day, a.ActualStartDate, a.ActualEndDate) as PlannedDuration,  " +
                                                    " A.OutageType,b.ConstraintName,b.Contingency  " +
                                                    " FROM  Ercot_rt_outages a JOIN  ConstraintOutages b ON CONCAT(a.EquipmentName, '_', a.EquipmentFromStationName, '_', EquipmentType) = b.Driver  " +
                                                                    "WHERE " +
                                                                    "b.ConstraintName=@SelectedConstraint " +
                                                                    "and b.Contingency=@Contingency";
                            }
                            mConstraintCommand.Parameters.AddWithValue("@SelectedConstraint", Selectedconstraint);
                            mConstraintCommand.Parameters.AddWithValue("@Contingency", SelectedContingency);
                            reader = mConstraintCommand.ExecuteReader();
                        }
                    }
                    while (reader.Read())
                    {
                        ConstraintOutages mConstraintOutages = new ConstraintOutages();
                        // ConstraintList.Add(new ConstraintOutages
                        {
                            mConstraintOutages.OutageName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                            mConstraintOutages.TicketID = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1));
                            mConstraintOutages.Branch = reader.IsDBNull(2) ? "" : Convert.ToString(reader.GetValue(2));
                            mConstraintOutages.ToBranch = reader.IsDBNull(3) ? "" : Convert.ToString(reader.GetValue(3));
                            mConstraintOutages.Zone = reader.IsDBNull(4) ? "" : Convert.ToString(reader.GetValue(4));
                            mConstraintOutages.EquipmentType = reader.IsDBNull(5) ? "" : Convert.ToString(reader.GetValue(5));
                            mConstraintOutages.Voltage = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));
                            mConstraintOutages.OutageStatus = reader.IsDBNull(7) ? "" : Convert.ToString(reader.GetValue(7));
                            mConstraintOutages.Startdate = reader.IsDBNull(8) ? new DateTime() : Convert.ToDateTime(reader.GetValue(8));
                            mConstraintOutages.Enddate = reader.IsDBNull(9) ? new DateTime() : Convert.ToDateTime(reader.GetValue(9));
                            mConstraintOutages.OutageDuration = reader.IsDBNull(10) ? 0 : Convert.ToInt32(reader.GetValue(10));
                            mConstraintOutages.OutageType = reader.IsDBNull(11) ? "" : Convert.ToString(reader.GetValue(11));
                            mConstraintOutages.ConstraintName = reader.IsDBNull(12) ? "" : Convert.ToString(reader.GetValue(12));
                            mConstraintOutages.Contingency = reader.IsDBNull(13) ? "" : Convert.ToString(reader.GetValue(13));
                        }
                        ConstraintList.Add(mConstraintOutages);
                    }
                    reader.Close();
                    if (isAllConstaintchecked)
                        mAllConstraintCommand.Connection.Close();
                    else
                        mConstraintCommand.Connection.Close();
                }
                return ConstraintList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
