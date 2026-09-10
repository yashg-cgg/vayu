using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Vayu.OutageConstraintMapping.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The m connection
        /// </summary>
        private SqlConnection mConnection;
        /// <summary>
        /// The m outage command
        /// </summary>
        private SqlCommand mOutageCommand;
        /// <summary>
        /// The m constraint command
        /// </summary>
        private SqlCommand mConstraintCommand;
        /// <summary>
        /// The m contigency command
        /// </summary>
        private SqlCommand mContigencyCommand;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the outage data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="rangeChecked">if set to <c>true</c> [range checked].</param>
        /// <param name="IsStartChecked">if set to <c>true</c> [is start checked].</param>
        /// <returns></returns>
        public List<OutageData> GetOutageData(DateTime startDate, DateTime endDate, bool rangeChecked, bool IsStartChecked)
        {
            List<OutageData> OutageList = new List<OutageData>();
            try
            {
                using (mConnection = new SqlConnection("Data Source = ; Initial Catalog = NewTrading; Persist Security Info = True; User Id = ; password = ; Connect Timeout = 100000; MultipleActiveResultSets=True"))
                {
                    using (mOutageCommand = mConnection.CreateCommand())
                    {
                        mOutageCommand.Connection.Open();
                        if (rangeChecked)
                        {
                            if (IsStartChecked)
                            {
                                mOutageCommand.CommandText = "SELECT DISTINCT b.Driver, b.Source FROM PJM_rt_outages a JOIN PJM.ConstraintOutages b " +
                                                                "ON a.Equipment = b.Driver"
                                                                + " WHERE a.startDate >= @Startdate and a.startdate < @EndDate and (a.enddate >= @StartDate or a.enddate is null)"
                                                                + " and (a.removeddate>=@EndDate or a.removeddate is  null) ORDER BY b.Driver";
                                mOutageCommand.Parameters.AddWithValue("@StartDate", startDate);
                                mOutageCommand.Parameters.AddWithValue("@EndDate", endDate);
                            }
                            else
                            {
                                mOutageCommand.CommandText = "SELECT DISTINCT b.Driver, b.Source FROM PJM_rt_outages a JOIN PJM.ConstraintOutages b " +
                                                                "ON a.Equipment = b.Driver"
                                                                + " WHERE a.startdate <= @EndDate and (a.enddate >= @StartDate or a.enddate is null) and "
                                                                + "(a.removeddate>=@EndDate or a.removeddate is  null)"
                                                                + " ORDER BY b.Driver";
                                mOutageCommand.Parameters.AddWithValue("@StartDate", startDate);
                                mOutageCommand.Parameters.AddWithValue("@EndDate", endDate);
                            }
                        }
                        else
                        {
                            mOutageCommand.CommandText = "SELECT DISTINCT a.Equipment, b.Source FROM PJM_rt_outages a JOIN PJM.ConstraintOutages b ON a.Equipment = b.Driver"
                                                            + " ORDER BY a.Equipment";
                        }
                        SqlDataReader reader = mOutageCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            OutageList.Add(new OutageData
                            {
                                DriverName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)),
                                Source = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1))
                            });
                            //OutageList.Add(reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)));
                        }
                        reader.Close();
                        mOutageCommand.Connection.Close();
                    }
                }
                return OutageList;
            }
            catch (Exception ex)
            {
                return null;
                //throw;
            }
        }
        /// <summary>
        /// The list of constarint family
        /// </summary>
        public List<Tuple<String, String, String>> listOfConstarintFamily = new List<Tuple<String, String, String>>();

        /// <summary>
        /// Constraints the family list.
        /// </summary>
        /// <returns></returns>
        public List<Tuple<String, String, String>> ConstraintFamilyList()
        {
            return listOfConstarintFamily;
        }

        /// <summary>
        /// Gets the constraint data.
        /// </summary>
        /// <param name="SelectedOutages">The selected outages.</param>
        /// <param name="shadowPrice">The shadow price.</param>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetConstraintData(List<string> SelectedOutages, int shadowPrice)
        {
            try
            {
                listOfConstarintFamily.Clear();
                Dictionary<string, List<string>> ConstraintDict = new Dictionary<string, List<string>>();
                List<string> ConstraintList = new List<string>();

                foreach (string selectedOutage in SelectedOutages)
                {
                    using (mConnection = new SqlConnection("Data Source = ; Initial Catalog = NewTrading; Persist Security Info = True; User Id = ; password = ; Connect Timeout = 100000; MultipleActiveResultSets=True"))
                    {
                        using (mConstraintCommand = mConnection.CreateCommand())
                        {
                            mConstraintCommand.Connection.Open();
                            //mConstraintCommand.CommandText = "select * from pjm.ConstraintOutages where Driver like @Driver order by constraintname";
                            //mConstraintCommand.CommandText = "select a.monitoredtext,b.monitoredtext from rtmasterconstraint a join rtfamily b on a.constraintrtnum=b.constraintrtnum" +
                            //                                " where abs(a.shadowprice) > @shadowprice and a.monitoredtext in(select constraintname from pjm.ConstraintOutages where Driver like @Driver)";
                            mConstraintCommand.CommandText = "select a.monitoredtext,b.monitoredtext,c.Driver,c.Source from rtmasterconstraint a join rtfamily b on a.constraintrtnum=b.constraintrtnum" +
                                                             " and abs(a.shadowprice) > @shadowprice join pjm.ConstraintOutages c on c.ConstraintName=a.MonitoredText where Driver like @Driver";
                            mConstraintCommand.Parameters.AddWithValue("@Driver", selectedOutage);
                            mConstraintCommand.Parameters.AddWithValue("@shadowprice", shadowPrice);
                            SqlDataReader reader = mConstraintCommand.ExecuteReader();
                            while (reader.Read())
                            {
                                ConstraintList = new List<string>();
                                string ConstraintName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                                string ConstraintFamily = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1));
                                string constraintDriverName = reader.IsDBNull(2) ? "" : Convert.ToString(reader.GetValue(2));
                                string constraintSource = reader.IsDBNull(3) ? "" : Convert.ToString(reader.GetValue(3));
                                ConstraintList.Add(ConstraintName);
                                if (!ConstraintDict.ContainsKey(ConstraintFamily))
                                {
                                    ConstraintDict.Add(ConstraintFamily, ConstraintList);

                                    listOfConstarintFamily.Add(new Tuple<String, String, String>(ConstraintFamily, constraintSource, constraintDriverName));
                                }
                                else
                                {
                                    List<string> templist = ConstraintDict[ConstraintFamily];
                                    templist.Add(ConstraintName);
                                    ConstraintDict[ConstraintFamily] = templist;
                                }
                                //ConstraintList.Add(new Constraint
                                //{
                                //    ConstraintName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)),
                                //});
                                //ConstraintList.Add(reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0)));
                            }
                            reader.Close();
                            mConstraintCommand.Connection.Close();
                        }
                    }
                }
                return ConstraintDict;
            }
            catch (Exception)
            {
                // throw;
                return null;
            }
        }

        /// <summary>
        /// Gets the contigency data.
        /// </summary>
        /// <param name="SelectedConstraint">The selected constraint.</param>
        /// <returns></returns>
        public List<Contingency> GetContigencyData(string SelectedConstraint)
        {
            try
            {
                List<Contingency> ContingencyList = new List<Contingency>();
                using (mConnection = mConnection = new SqlConnection("Data Source = ; Initial Catalog = NewTrading; Persist Security Info = True; User Id = ; password = ; Connect Timeout = 100000; MultipleActiveResultSets=True"))
                {
                    using (mContigencyCommand = mConnection.CreateCommand())
                    {
                        if (mConnection.State == ConnectionState.Closed)
                        {
                            mConnection.Open();
                        }
                        mContigencyCommand.CommandText = "select distinct contingency from pjm.ConstraintOutages where constraintname like @constraintname and contingency is not null order by contingency ";
                        mContigencyCommand.Parameters.AddWithValue("@constraintname", SelectedConstraint);
                        SqlDataReader reader = mContigencyCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            Contingency contingency = new Contingency();
                            contingency.ContingencyName = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                            ContingencyList.Add(contingency);
                        }
                        reader.Close();
                        mConnection.Close();
                    }
                }
                return ContingencyList;
            }
            catch (Exception e)
            {
                return null;
                //throw;
            }

        }

        #endregion
    }
}
