using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.Outage_Constraint_History.Model
{
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;
        private SqlCommand mSelectConstraintCommand;
        private SqlCommand mSelecOutageHistoryCommand;
        private SqlCommand mSelecConstraintsHistoryCommand;
        public void loadDBCommands(string @StartDateTime, string @EndDateTime)
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectConstraintCommand = new SqlCommand();
            // mSelectConstraintCommand.CommandText = "  select distinct Outages, ConstraintText,MarketDateTime  from [PJM].[PredictedConstraints] where MarketDateTime>= '" + @StartDateTime + "' and  MarketDateTime< '" + @EndDateTime + "' order by Outages, ConstraintText";
            mSelectConstraintCommand.CommandText = "  	 SELECT distinct  b.Driver, b.constraintname FROM PJM_rt_outages a JOIN PJM.ConstraintOutages b " +
      " ON a.Equipment = b.Driver join PJM.Constraintrt c on b.constraintname=c.constrainttext " +
         " WHERE a.startDate  >= '" + @StartDateTime + "'  and a.startdate < '" + @EndDateTime + "'  and (a.enddate >= '" + @StartDateTime + "'  or a.enddate is null) " +
        " and (a.removeddate>='" + @EndDateTime + "' or a.removeddate is  null) ";
            mSelectConstraintCommand.Connection = VayuConnection;

            mSelecOutageHistoryCommand = new SqlCommand();
            mSelecOutageHistoryCommand.CommandText = "  	 SELECT distinct  b.Driver,  cast(startdate as date) FROM PJM_rt_outages a JOIN PJM.ConstraintOutages b " +
      " ON a.Equipment = b.Driver WHERE a.startDate  >= '" + @StartDateTime + "'  and a.startdate < '" + @EndDateTime + "'  and (a.enddate >= '" + @StartDateTime + "'  or a.enddate is null) " +
        " and (a.removeddate>='" + @EndDateTime + "' or a.removeddate is  null) " +
        " order by  b.Driver,  cast(startdate as date) ";
            mSelecOutageHistoryCommand.Connection = VayuConnection;

            mSelecConstraintsHistoryCommand = new SqlCommand();
            mSelecConstraintsHistoryCommand.CommandText = " SELECT distinct   constrainttext,cast(marketdatetime as date) FROM PJM.Constraintrt WHERE  marketdatetime>='" + @StartDateTime + "'  and marketdatetime<='" + @EndDateTime + "' order by   constrainttext,cast(marketdatetime as date)  ";
            mSelecConstraintsHistoryCommand.Connection = VayuConnection;
        }

        public void GetConstraints(Action<IEnumerable<ConstraintOutages>, Exception> callback)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<ConstraintOutages> mConstraintDetailsList = new List<ConstraintOutages>();
            SqlDataReader reader = null;
            try
            {
                reader = mSelectConstraintCommand.ExecuteReader();
                while (reader.Read())
                {
                    ConstraintOutages mConstraintDetails = new ConstraintOutages();
                    mConstraintDetails.Outages = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    mConstraintDetails.Constraint = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    mConstraintDetailsList.Add(mConstraintDetails);
                }
            }
            catch { }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            callback(mConstraintDetailsList, null);
        }
        public void GetOutagelist(Action<IEnumerable<OutagesDetails>, Exception> callback)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<OutagesDetails> mOutagesList = new List<OutagesDetails>();
            SqlDataReader reader = null;
            try
            {
                reader = mSelecOutageHistoryCommand.ExecuteReader();
                while (reader.Read())
                {
                    OutagesDetails mOutagesDetails = new OutagesDetails();
                    mOutagesDetails.Outages = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    mOutagesDetails.MarketDateTime = reader.GetDateTime(1);
                    mOutagesList.Add(mOutagesDetails);
                }
            }
            catch { }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            callback(mOutagesList, null);
        }


        public void GetConstraintlist(Action<IEnumerable<ConstraintDetails>, Exception> callback)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<ConstraintDetails> mConstraintList = new List<ConstraintDetails>();
            SqlDataReader reader = null;
            try
            {
                reader = mSelecConstraintsHistoryCommand.ExecuteReader();
                while (reader.Read())
                {
                    ConstraintDetails mConstraintDetails = new ConstraintDetails();
                    mConstraintDetails.Constraint = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    mConstraintDetails.MarketDateTime = reader.GetDateTime(1);
                    mConstraintList.Add(mConstraintDetails);
                }
            }
            catch { }
            finally
            {
                if (reader != null)
                    reader.Close();
                VayuConnection.Close();
            }
            callback(mConstraintList, null);
        }
    }
}
