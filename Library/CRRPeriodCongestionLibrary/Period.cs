using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Vayu.CRRPeriodCongestionLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class Period
    {
        /// <summary>
        /// The period key
        /// </summary>
        public int PeriodKey;
        /// <summary>
        /// The market key
        /// </summary>
        public int MarketKey;
        /// <summary>
        /// The period name
        /// </summary>
        public string PeriodName;
        /// <summary>
        /// The period type
        /// </summary>
        public string PeriodType;
        /// <summary>
        /// The start date
        /// </summary>
        public DateTime StartDate;
        /// <summary>
        /// The end date
        /// </summary>
        public DateTime EndDate;
        /// <summary>
        /// The peak hours
        /// </summary>
        public int PeakHours;
        /// <summary>
        /// The off peak hours
        /// </summary>
        public int OffPeakHours;
        /// <summary>
        /// The season
        /// </summary>
        public string Season;
        /// <summary>
        /// The period year
        /// </summary>
        public int PeriodYear;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{FTRPeriodCongestionLibrary.Period}" />
    public class PeriodList : List<Period>
    {
        #region Public Methods

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public static PeriodList GetList(int marketKey, DateTime? startDate = null, DateTime? endDate = null)
        {
            PeriodList periodList = new PeriodList();
            SqlCommand command = Utility.GetCommand(DB.TradingData);
            string commandTxt = "select PeriodKey,MarketKey,PeriodName,PeriodYear,StartDate,EndDate,PeakHrs,OffPeakHrs,PeriodType from Period where MarketKey = " + marketKey;

            if (startDate.HasValue)
                commandTxt += " and StartDate >= '" + startDate.Value.ToString("yyyy-MM-dd") + "'";

            if (endDate.HasValue)
                commandTxt += " and EndDate <= '" + endDate.Value.ToString("yyyy-MM-dd") + "' ";

            commandTxt += " order by StartDate ";
            command.CommandText = commandTxt;
            periodList.FillByCommand(command);
            return periodList;
        }

        /// <summary>
        /// Gets the missing list.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public static PeriodList GetMissingList(Strategy strategy, DateTime? startDate = null, DateTime? endDate = null)
        {
            SqlCommand command = Utility.GetCommand(DB.TradingData);
            string commandTxt = "select PeriodKey,MarketKey,PeriodName,PeriodYear,StartDate,EndDate,PeakHrs,OffPeakHrs,PeriodType " +
                " from Period where PeriodKey not in (select distinct PeriodKey from " + strategy.TableName + ") and  MarketKey = " + strategy.MarketID;

            if (startDate.HasValue)
                commandTxt += " and StartDate >= '" + startDate.Value.ToString("yyyy-MM-dd") + "'";

            if (endDate.HasValue)
                commandTxt += " and EndDate <= '" + endDate.Value.ToString("yyyy-MM-dd") + "' ";

            commandTxt += " order by StartDate ";
            command.CommandText = commandTxt;
            PeriodList periodList = new PeriodList();
            periodList.FillByCommand(command);
            return periodList;
        }

        /// <summary>
        /// Gets the daily update periods.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public static PeriodList GetDailyUpdatePeriods(int marketKey)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-11);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            DateTime endDate = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
            endDate = endDate.AddMonths(1).AddDays(-1);
            PeriodList periodList = GetList(marketKey, startDate, endDate);
            return periodList;
        }

        #endregion

        /// <summary>
        /// Fills the by command.
        /// </summary>
        /// <param name="command">The command.</param>
        private void FillByCommand(SqlCommand command)
        {
            try
            {
                if (command.Connection.State != System.Data.ConnectionState.Open)
                    command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    try
                    {
                        Period period = new Period();
                        int.TryParse(reader[0].ToString(), out period.PeriodKey);
                        int.TryParse(reader[1].ToString(), out period.MarketKey);
                        period.PeriodName = reader[2].ToString();
                        int.TryParse(reader[3].ToString(), out period.PeriodYear);
                        DateTime.TryParse(reader[4].ToString(), out period.StartDate);
                        DateTime.TryParse(reader[5].ToString(), out period.EndDate);
                        int.TryParse(reader[6].ToString(), out period.PeakHours);
                        int.TryParse(reader[7].ToString(), out period.OffPeakHours);
                        period.PeriodType = reader[8].ToString();
                        this.Add(period);
                    }
                    catch { }
                }

                reader.Close();
            }
            catch { }
            finally
            {
                if (command.Connection.State != System.Data.ConnectionState.Closed)
                    command.Connection.Close();
            }
        }
    }
}
