using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.RT_Constraint_impact.Model
{

    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection VayuConnection;
        /// <summary>
        /// The m select five minimum constraints
        /// </summary>
        private SqlCommand mSelectFiveMinConstraints;
        /// <summary>
        /// The m select hourly constraints
        /// </summary>
        private SqlCommand mSelectHourlyConstraints;
        /// <summary>
        /// The m select sum constraint five minimum
        /// </summary>
        private SqlCommand mSelectSumConstraintFiveMin;

        #endregion

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectFiveMinConstraints = new SqlCommand();
            mSelectFiveMinConstraints.CommandText = "Select A.marketdatetime, A.constrainttext, A.contingencytext, A.shadowprice, A.shadowprice*B.shiftfactor as impact " +
                                             " from ConstraintRT A left join RTMasterConstraint B On A.constrainttext = B.monitoredtext And A.contingencytext = B.contingencytext " +
                                             " Where A.marketdatetime >= @StartDateTime and A.marketdatetime < @EndDateTime and A.ConstraintText!='none' order by A.marketdatetime";
            mSelectFiveMinConstraints.Parameters.AddWithValue("@StartDateTime", "StartDateTime");
            mSelectFiveMinConstraints.Parameters.AddWithValue("@EndDateTime", "EndDateTime");
            mSelectFiveMinConstraints.Connection = VayuConnection;
            //
            mSelectHourlyConstraints = new SqlCommand();
            mSelectHourlyConstraints.CommandText = "select date, hour, monitoredtext, contingencytext, a.shadowprice/12, impact/12 from RTImpact a inner join RTMasterConstraint b on a.constraintrtnum = b.constraintrtnum where date = @StartDateTime order by hour asc";
            mSelectHourlyConstraints.Parameters.AddWithValue("@StartDateTime", "StartDateTime");
            mSelectHourlyConstraints.Connection = VayuConnection;
            //
            mSelectSumConstraintFiveMin = new SqlCommand();
            mSelectSumConstraintFiveMin.CommandText = "select date,hour+1,constraints,contingency,sum(shadowprice)/@intervals, sum(impact)/@intervals from ( Select CONVERT(date,A.marketdatetime) as date,DATEPART(HH,A.marketdatetime) as hour , " +
                                                      " A.constrainttext as constraints, A.contingencytext as contingency, A.shadowprice as shadowprice, A.shadowprice*B.shiftfactor as impact from ConstraintRT A left join " +
                                                      " RTMasterConstraint B On A.constrainttext = B.monitoredtext And A.contingencytext = B.contingencytext Where A.marketdatetime >= @StartDateTime AND A.marketdatetime <@EndDateTime " +
                                                      " and A.ConstraintText!='none' group by A.constrainttext, A.contingencytext,A.marketdatetime,A.shadowprice,b.shiftfactor ) " +
                                                      " as b group by constraints,contingency,date,hour";
            mSelectSumConstraintFiveMin.Parameters.AddWithValue("@StartDateTime", "StartDateTime");
            mSelectSumConstraintFiveMin.Parameters.AddWithValue("@EndDateTime", "EndDateTime");
            mSelectSumConstraintFiveMin.Parameters.AddWithValue("@intervals", "intervals");
            mSelectSumConstraintFiveMin.Connection = VayuConnection;
            //
        }

        /// <summary>
        /// Gets the constraints.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketDate">The market date.</param>
        /// <param name="IsHourly">if set to <c>true</c> [is hourly].</param>
        public void GetConstraints(Action<List<ViewModels.Constraints>, Exception> callback, string selectedMarket, DateTime MarketDate, bool IsHourly)
        {
            List<ViewModels.Constraints> ConstraintsList = new List<ViewModels.Constraints>();
            double? ImpactNull = null;
            if (IsHourly)
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                if (selectedMarket == "ERCOT")
                {
                    mSelectHourlyConstraints.CommandText = "select date, hour, monitoredtext, contingencytext, a.shadowprice/12, impact/12 from Vayu..RTImpact a inner join Vayu..RTMasterConstraint b on a.constraintrtnum = b.constraintrtnum where date = @StartDateTime order by hour asc";
                }
                mSelectHourlyConstraints.Parameters["@StartDateTime"].Value = MarketDate.Date;
                SqlDataReader reader = mSelectHourlyConstraints.ExecuteReader();
                bool colorHourly = false;
                int CompareHourly = -1;
                while (reader.Read())
                {
                    int selectedHourly = Convert.ToDateTime(reader.GetValue(0)).AddHours(Convert.ToInt16(reader.GetValue(1))).Hour;
                    if (CompareHourly == -1)
                    {
                        CompareHourly = selectedHourly;
                    }
                    if (CompareHourly != selectedHourly)
                    {
                        if (colorHourly == true)
                        {
                            colorHourly = false;
                        }
                        else
                        {
                            colorHourly = true;
                        }
                        CompareHourly = selectedHourly;
                    }


                    ViewModels.Constraints constraint = new ViewModels.Constraints();
                    if (Convert.ToDateTime(reader.GetValue(0)).AddHours(Convert.ToInt16(reader.GetValue(1))) == MarketDate.AddDays(1))
                    {
                        constraint.MarketDate = Convert.ToDateTime(reader.GetValue(0)).ToString("yyyy-MM-dd 24:00");
                    }
                    else
                    {
                        constraint.MarketDate = Convert.ToDateTime(reader.GetValue(0)).AddHours(Convert.ToInt16(reader.GetValue(1))).ToString("yyyy-MM-dd HH:00");
                    }
                    constraint.Constraint = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                    constraint.Contingency = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                    constraint.ShadowPrice = Math.Abs(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0));
                    constraint.Impact = reader.IsDBNull(5) ? ImpactNull : Math.Abs(Math.Round(Convert.ToDouble(reader.GetValue(5)), 0));
                    constraint.color = colorHourly;
                    ConstraintsList.Add(constraint);
                }
                reader.Close();

                //if market date not today
                int lastHour = 24;
                DateTime currentTime = DateTime.Now;
                int currentHour = Convert.ToInt32(currentTime.ToString("HH"));
                currentHour = currentHour + 1;

                if (MarketDate.Date == currentTime.Date) { lastHour = currentHour; } else { lastHour = 24; }

                for (int i = 0; i < 24; i++)
                {
                    string checkDate = MarketDate.AddHours(i + 1).ToString("yyyy-MM-dd HH:00");
                    bool ContainsHour = ConstraintsList.Any(Item => Item.MarketDate == checkDate);
                    if (!ContainsHour)
                    {
                        mSelectSumConstraintFiveMin.Parameters["@StartDateTime"].Value = MarketDate.AddHours(i);
                        mSelectSumConstraintFiveMin.Parameters["@EndDateTime"].Value = MarketDate.AddHours(i + 1);
                        if (i == currentHour && MarketDate.Date == currentTime.Date)
                        {
                            int currentMinute = Convert.ToInt32(currentTime.ToString("mm"));
                            double currentElapsedInterval = currentMinute / 5;
                            currentElapsedInterval = Math.Floor(currentElapsedInterval);
                            mSelectSumConstraintFiveMin.Parameters["@intervals"].Value = currentElapsedInterval;
                        }
                        else
                        {
                            mSelectSumConstraintFiveMin.Parameters["@intervals"].Value = 12;
                        }
                        reader = mSelectSumConstraintFiveMin.ExecuteReader();
                        while (reader.Read())
                        {
                            int selectedHourly = Convert.ToDateTime(reader.GetValue(0)).AddHours(Convert.ToInt16(reader.GetValue(1))).Hour;
                            if (CompareHourly == -1)
                            {
                                CompareHourly = selectedHourly;
                            }
                            if (CompareHourly != selectedHourly)
                            {
                                if (colorHourly == true)
                                {
                                    colorHourly = false;
                                }
                                else
                                {
                                    colorHourly = true;
                                }
                                CompareHourly = selectedHourly;
                            }
                            ViewModels.Constraints constraint = new ViewModels.Constraints();
                            constraint.MarketDate = Convert.ToDateTime(reader.GetValue(0)).AddHours(Convert.ToInt16(reader.GetValue(1))).ToString("yyyy-MM-dd HH:00");
                            constraint.Constraint = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                            constraint.Contingency = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                            constraint.ShadowPrice = Math.Abs(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0));
                            constraint.Impact = reader.IsDBNull(5) ? ImpactNull : Math.Abs(Math.Round(Convert.ToDouble(reader.GetValue(5)), 0));
                            constraint.color = colorHourly;
                            ConstraintsList.Add(constraint);
                        }
                        reader.Close();
                    }
                }
                VayuConnection.Close();
            }
            else
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                if (selectedMarket == "ERCOT")
                {
                    mSelectFiveMinConstraints.CommandText = "Select A.marketdatetime, A.constrainttext, A.contingencytext, A.shadowprice, A.shadowprice*B.shiftfactor as impact " +
                                 " from Vayu..ConstraintRT A left join Vayu..RTMasterConstraint B On A.constrainttext = B.monitoredtext And A.contingencytext = B.contingencytext " +
                                 " Where A.marketdatetime >= @StartDateTime and A.marketdatetime < @EndDateTime and A.ConstraintText!='none' order by A.marketdatetime";
                }
                mSelectFiveMinConstraints.Parameters["@StartDateTime"].Value = MarketDate;
                mSelectFiveMinConstraints.Parameters["@EndDateTime"].Value = MarketDate.AddDays(1);
                SqlDataReader reader = mSelectFiveMinConstraints.ExecuteReader();
                bool color = false;
                int CompareHour = -1;
                while (reader.Read())
                {
                    int selectedHour = Convert.ToDateTime(reader.GetValue(0)).Hour;
                    if (CompareHour == -1)
                    {
                        CompareHour = selectedHour;
                    }
                    if (CompareHour != selectedHour)
                    {
                        if (color == true)
                        {
                            color = false;
                        }
                        else
                        {
                            color = true;
                        }
                        CompareHour = selectedHour;
                    }
                    ViewModels.Constraints constraint = new ViewModels.Constraints();
                    constraint.MarketDate = Convert.ToDateTime(reader.GetValue(0)).ToString("yyyy-MM-dd HH:mm"); ;
                    constraint.Constraint = reader.GetValue(1).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                    constraint.Contingency = reader.GetValue(2).ToString().Replace("Contingency", "").TrimStart();
                    constraint.ShadowPrice = Math.Abs(Math.Round(Convert.ToDouble(reader.GetValue(3)), 0));
                    constraint.Impact = reader.IsDBNull(4) ? ImpactNull : Math.Abs(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0));
                    constraint.color = color;
                    ConstraintsList.Add(constraint);
                }
                reader.Close();
                VayuConnection.Close();
            }
            callback(ConstraintsList, null);
        }
    }
}
