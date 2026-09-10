using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRTopTenParticipants.Model
{
    public class DataService : IDataService
    {

        private SqlConnection VayuConnection;
        private SqlCommand mSelectMonthCommand;

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            if (VayuConnection == null)
            {
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            }
            //
            if (mSelectMonthCommand == null)
            {
                mSelectMonthCommand = VayuConnection.CreateCommand();
            }
            mSelectMonthCommand.CommandText = "select distinct(FTR.month) from FTRPNL_REPORT FTR order by month";
            mSelectMonthCommand.Connection = VayuConnection;
        }

        /// <summary>
        /// Gets the period data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetPeriodData(Action<List<DateTime>, Exception> callback)
        {
            loadDBCommands();
            List<DateTime> datetimeList = new List<DateTime>();
            try
            {
                if (mSelectMonthCommand.Connection.State.Equals(ConnectionState.Closed))
                {
                    mSelectMonthCommand.Connection.Open();
                }
                IDataReader reader = mSelectMonthCommand.ExecuteReader();
                while (reader.Read())
                {
                    datetimeList.Add(reader.IsDBNull(0) ? DateTime.Now : Convert.ToDateTime(reader.GetValue(0)));
                }
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
                callback(datetimeList, null);
            }
            catch
            {
                callback(new List<DateTime>(), new ArgumentException("exception was raised"));
            }
            finally
            {
                if (mSelectMonthCommand.Connection.State.Equals(ConnectionState.Open))
                {
                    mSelectMonthCommand.Connection.Close();
                }
            }
        }
        /// <summary>
        /// Gets the top10 participant data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketKey">The market key.</param>
        /// <param name="month">The month.</param>
        /// <param name="number">The number.</param>
        public void GetTop10ParticipantData(Action<List<DataItem>, Exception> callback, int MarketKey, string month, int number)
        {
            if (VayuConnection == null)
            {
                loadDBCommands();
            }
            try
            {
                SqlCommand selectPNLTop10Command = VayuConnection.CreateCommand();
                selectPNLTop10Command.CommandText = "select top " + number + " FTR.Participant, sum(FTR.Cost) as Cost, sum(FTR.DAPrice) as DAPrice, sum(FTR.PNL) as PNL, " +
                            "sum(FTR.Monthly) as Monthly, sum(FTR.Annual) as Annual, sum(FTR.Q1) as Q1, sum(FTR.Q2) as Q2, sum(FTR.Q3) as Q3, sum(FTR.Q4) as Q4, " +
                            "sum(FTR.YR1) as YR1, sum(FTR.YR2) as YR2, sum(FTR.YR3) as YR3, sum(FTR.YRALL) as YRALL, sum(FTR.MW) as MW, " +
                            "C.company, max(FTR.ReportDate) from FTRPNL_REPORT FTR left join Company C on FTR.Participant = C.Participant and FTR.Marketkey = C.MarketKey where FTR.MarketKey = " +
                            MarketKey + "and FTR.month in (" + month + ")  group by FTR.Participant, C.Company order by PNL desc";
                if (selectPNLTop10Command.Connection.State.Equals(ConnectionState.Closed))
                {
                    selectPNLTop10Command.Connection.Open();
                }
                IDataReader reader = selectPNLTop10Command.ExecuteReader();
                List<DataItem> itemList = new List<DataItem>();
                int i = 0;
                while (reader.Read())
                {
                    itemList.Add(new DataItem()
                    {
                        Rank = i + 1,
                        Participant = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString(),
                        Cost = reader.IsDBNull(1) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(1))),
                        DAPrice = reader.IsDBNull(2) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(2))),
                        PNL = reader.IsDBNull(3) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(3))),
                        Monthly = reader.IsDBNull(4) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(4))),
                        Annual = reader.IsDBNull(5) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(5))),
                        Q1 = reader.IsDBNull(6) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(6))),
                        Q2 = reader.IsDBNull(7) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(7))),
                        Q3 = reader.IsDBNull(8) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(8))),
                        Q4 = reader.IsDBNull(9) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(9))),
                        YR1 = reader.IsDBNull(10) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(10))),
                        YR2 = reader.IsDBNull(11) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(11))),
                        YR3 = reader.IsDBNull(12) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(12))),
                        YRALL = reader.IsDBNull(13) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(13))),
                        MW = reader.IsDBNull(14) ? 0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(14))),
                        Company = reader.IsDBNull(15) ? "" : reader.GetValue(15).ToString(),
                        ReportDate = reader.IsDBNull(16) ? DateTime.Now : Convert.ToDateTime(reader.GetValue(16))
                    });
                    i++;
                }
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
                callback(itemList, null);
            }
            catch
            {
                callback(new List<DataItem>(), new ArgumentException("Soemthing is wrong"));
            }
            finally
            {
                if (VayuConnection.State.Equals(ConnectionState.Open))
                {
                    VayuConnection.Close();
                }
            }
        }
        /// <summary>
        /// Gets the top10 participant data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketKey">The market key.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        public void GetTop10ParticipantData(Action<List<DataItem>, Exception> callback, int MarketKey, DateTime fromDate, DateTime toDate)
        {
            loadDBCommands();
            Dictionary<DateTime, DateTime> datetimeList = GetDateList(fromDate, DateTime.Parse(toDate.Year + "-" + toDate.Month + "-" + toDate.Day + " 23:00:00"));
            try
            {
                List<List<DataItem>> itemList = new List<List<DataItem>>();
                foreach (var item in datetimeList)
                {
                    itemList.Add(GetMonthList(MarketKey, item.Key, item.Value));
                }
                Dictionary<string, DataItem> dataHash = new Dictionary<string, DataItem>();
                foreach (var lItem in itemList)
                {
                    foreach (var innerItem in lItem)
                    {
                        if (dataHash.ContainsKey(innerItem.Participant))
                        {
                            DataItem tempItem = dataHash[innerItem.Participant] as DataItem;
                            dataHash.Remove(innerItem.Participant);
                            tempItem.MW += innerItem.MW;
                            tempItem.Cost += innerItem.Cost;
                            tempItem.PNL += innerItem.PNL;
                            tempItem.Monthly += innerItem.Monthly;
                            tempItem.Annual += innerItem.Annual;
                            tempItem.Q1 += innerItem.Q1;
                            tempItem.Q2 += innerItem.Q2;
                            tempItem.Q3 += innerItem.Q3;
                            tempItem.Q4 += innerItem.Q4;
                            tempItem.YR1 += innerItem.YR1;
                            tempItem.YR2 += innerItem.YR2;
                            tempItem.YR3 += innerItem.YR3;
                            tempItem.YRALL += innerItem.YRALL;
                            tempItem.DAPrice += innerItem.DAPrice;
                            //tempItem.CostMonthly += innerItem.CostMonthly;
                            //tempItem.CostAnnual += innerItem.CostAnnual;
                            //tempItem.CostQ1 += innerItem.CostQ1;
                            //tempItem.CostQ2 += innerItem.CostQ2;
                            //tempItem.CostQ3 += innerItem.CostQ3;
                            //tempItem.CostQ4 += innerItem.CostQ4;
                            //tempItem.CostYR1 += innerItem.CostYR1;
                            //tempItem.CostYR2 += innerItem.CostYR2;
                            //tempItem.CostYR3 += innerItem.CostYR3;
                            //tempItem.CostYRALL += innerItem.CostYRALL;
                            tempItem.ReportDate = innerItem.ReportDate;
                            dataHash[innerItem.Participant] = tempItem;
                        }
                        else
                        {
                            dataHash.Add(innerItem.Participant, new DataItem
                            {
                                Participant = innerItem.Participant,
                                MW = innerItem.MW,
                                Cost = innerItem.Cost,
                                PNL = innerItem.PNL,
                                Monthly = innerItem.Monthly,
                                Annual = innerItem.Annual,
                                Q1 = innerItem.Q1,
                                Q2 = innerItem.Q2,
                                Q3 = innerItem.Q3,
                                Q4 = innerItem.Q4,
                                YR1 = innerItem.YR1,
                                YR2 = innerItem.YR2,
                                YR3 = innerItem.YR3,
                                YRALL = innerItem.YRALL,
                                //CostMonthly = innerItem.CostMonthly,
                                //CostAnnual = innerItem.CostAnnual,
                                //CostQ1 = innerItem.CostQ1,
                                //CostQ2 = innerItem.CostQ2,
                                //CostQ3 = innerItem.CostQ3,
                                //CostQ4 = innerItem.CostQ4,
                                //CostYR1 = innerItem.CostYR1,
                                //CostYR2 = innerItem.CostYR2,
                                //CostYR3 = innerItem.CostYR3,
                                //CostYRALL = innerItem.CostYRALL,
                                DAPrice = innerItem.DAPrice,
                                ReportDate = innerItem.ReportDate,
                                Company = innerItem.Company,
                            });
                        }
                    }
                }
                callback(dataHash.Values.ToList(), null);
            }
            catch
            {
                callback(null, new ArgumentException("Something went wrong"));
            }
            finally
            {
                if (VayuConnection.State.Equals(ConnectionState.Open))
                {
                    VayuConnection.Close();
                }
            }
        }
        /// <summary>
        /// Gets the month list.
        /// </summary>
        /// <param name="MarketKey">The market key.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        private List<DataItem> GetMonthList(int MarketKey, DateTime fromDate, DateTime toDate)
        {
            List<DataItem> helperList = new List<DataItem>();
            SqlCommand cmd = VayuConnection.CreateCommand();
            if (MarketKey == 9)
            {

                cmd.CommandText = "select top 10000 p.Participant,SUM(p.pnl) as pnl, SUM(p.Monthly) as monthly, SUM(p.Annual) as annual, SUM(p.Q1) as Q1, SUM(p.Q2) as Q2, " +
                            "SUM(p.Q3) as Q3, SUM(p.Q4) as Q4, SUM(p.YR1) as YR1, SUM(p.YR2) as YR2, SUM(p.YR3) as YR3, SUM(p.YRALL) as YRALL, sum(p.DAPrice) daprice,max(p.MW) as mw, " +
                            "c.Company, Max(p.Cost) as cost, cast(max(p.ReportDate) as Date) " +
                            "from Vayu..CRRDailyPNLReport p LEFT join company c on c.participant=p.participant and p.MarketKey=c.Marketkey  where p.Month between" + "'" +
                            fromDate.ToString("yyyy-MM-dd") + "'" + " and " + "'" + toDate.ToString("yyyy-MM-dd") + "'" +
                            " and p.MarketKey=" + MarketKey + " group by p.Participant,c.Company order by pnl  desc";
            }
            if (VayuConnection.State.Equals(ConnectionState.Closed))
            {
                VayuConnection.Open();
            }
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                helperList.Add(new DataItem
                {
                    Participant = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString(),
                    PNL = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(1))),
                    Monthly = reader.IsDBNull(2) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(2))),
                    Annual = reader.IsDBNull(3) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(3))),
                    Q1 = reader.IsDBNull(4) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(4))),
                    Q2 = reader.IsDBNull(5) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(5))),
                    Q3 = reader.IsDBNull(6) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(6))),
                    Q4 = reader.IsDBNull(7) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(7))),
                    YR1 = reader.IsDBNull(8) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(8))),
                    YR2 = reader.IsDBNull(9) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(9))),
                    YR3 = reader.IsDBNull(10) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(10))),
                    YRALL = reader.IsDBNull(11) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(11))),
                    MW = reader.IsDBNull(13) ? 0.0 : Convert.ToDouble(reader.GetValue(13)),
                    DAPrice = reader.IsDBNull(12) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(12))),
                    ReportDate = reader.IsDBNull(16) ? DateTime.Now : Convert.ToDateTime(reader.GetValue(16)),
                    Cost = reader.IsDBNull(15) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(15))),
                    Company = reader.IsDBNull(14) ? "" : reader.GetValue(14).ToString(),
                    //
                    //CostMonthly = reader.IsDBNull(17) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(17))),
                    //CostAnnual = reader.IsDBNull(18) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(18))),
                    //CostQ1 = reader.IsDBNull(19) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(19))),
                    //CostQ2 = reader.IsDBNull(20) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(20))),
                    //CostQ3 = reader.IsDBNull(21) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(21))),
                    //CostQ4 = reader.IsDBNull(22) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(22))),
                    //CostYR1 = reader.IsDBNull(23) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(23))),
                    //CostYR2 = reader.IsDBNull(24) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(24))),
                    //CostYR3 = reader.IsDBNull(25) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(25))),
                    //CostYRALL = reader.IsDBNull(26) ? 0.0 : Convert.ToDouble(Convert.ToInt32(reader.GetValue(26)))
                });
            }
            if (!reader.IsClosed)
            {
                reader.Close();
            }
            return helperList;
        }
        /// <summary>
        /// Gets the date list.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        private Dictionary<DateTime, DateTime> GetDateList(DateTime fromDate, DateTime toDate)
        {
            Dictionary<DateTime, DateTime> dateList = new Dictionary<DateTime, DateTime>();
            while (fromDate < toDate)
            {
                DateTime lstDate = DateTime.Parse(fromDate.Year + "-" + fromDate.Month + "-" + GetLastDay(fromDate));
                dateList.Add(fromDate, lstDate <= toDate ? lstDate : toDate);
                fromDate = fromDate.AddMonths(1);
            }
            return dateList;
        }
        /// <summary>
        /// Gets the last day.
        /// </summary>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        private int GetLastDay(DateTime toDate)
        {
            return DateTime.DaysInMonth(toDate.Year, toDate.Month);
        }
    }
}
