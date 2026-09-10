using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Vayu.CRRPeriodCongestionLibrary
{

    public class PeriodicLMPProc
    {
        #region Properties

        /// <summary>
        /// Gets or sets the market type code.
        /// </summary>
        /// <value>
        /// The market type code.
        /// </value>
        public string MarketTypeCode { get; set; }
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the peak LMP.
        /// </summary>
        /// <value>
        /// The peak LMP.
        /// </value>
        public double? PeakLMP { get; set; }
        /// <summary>
        /// Gets or sets the off peak LMP.
        /// </summary>
        /// <value>
        /// The off peak LMP.
        /// </value>
        public double? OffPeakLMP { get; set; }
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Gets or sets the peak hours.
        /// </summary>
        /// <value>
        /// The peak hours.
        /// </value>
        public int PeakHours { get; set; }
        /// <summary>
        /// Gets or sets the offpeak hours.
        /// </summary>
        /// <value>
        /// The offpeak hours.
        /// </value>
        public int OffpeakHours { get; set; }
        /// <summary>
        /// Gets or sets the name of the period.
        /// </summary>
        /// <value>
        /// The name of the period.
        /// </value>
        public string PeriodName { get; set; }
        /// <summary>
        /// Gets or sets the period year.
        /// </summary>
        /// <value>
        /// The period year.
        /// </value>
        public int PeriodYear { get; set; }
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the type of the period.
        /// </summary>
        /// <value>
        /// The type of the period.
        /// </value>
        public string PeriodType { get; set; }

        #endregion


        public string GetKey(string periodKey)
        {
            if ("daily".Equals(periodKey, StringComparison.InvariantCultureIgnoreCase))
                return StartDate.ToString();
            else
                return PeriodKey.ToString();
        }


        public static List<PeriodicLMPProc> GetList(DateTime startDate, DateTime endDate, int sourceNodeKey, int sinkNodeKey, int marketKey, string period)
        {
            List<PeriodicLMPProc> dartList = new List<PeriodicLMPProc>();
            List<PeriodicLMPProc> fdartList = new List<PeriodicLMPProc>();
            string procName = "GetPeriodicDART";
            if (period == "Daily")
            {
                procName = "GetPeriodicDARTDaily";
                while (startDate <= endDate.AddDays(1))
                {
                    dartList = GetDartList(procName, startDate, startDate.AddMonths(1).AddDays(-1), sourceNodeKey, sinkNodeKey, marketKey, period);
                    fdartList.AddRange(dartList);
                    startDate = startDate.AddMonths(1);
                }
            }
            dartList = GetDartList(procName, startDate, endDate, sourceNodeKey, sinkNodeKey, marketKey, period);
            return fdartList;
        }

        private static List<PeriodicLMPProc> GetDartList(string procName, DateTime startDate, DateTime endDate, int sourceNodeKey, int sinkNodeKey, int marketKey, string period)
        {
            List<PeriodicLMPProc> dartList = new List<PeriodicLMPProc>();
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            endDate = (new DateTime(endDate.Year, endDate.Month, 1)).AddMonths(1).AddDays(-1);

            SqlCommand cmd = Utility.GetCommand(CRRPeriodCongestionLibrary.DB.TradingData);
            cmd.CommandText = " exec " + procName + " '" + startDate.ToString("yyyy-MM-dd") + "' , '" +
                endDate.ToString("yyyy-MM-dd") + "' , " + sourceNodeKey + "," + sinkNodeKey + " , " + marketKey;

            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    PeriodicLMPProc pdart = new PeriodicLMPProc();
                    pdart.MarketTypeCode = reader[0].ToString();
                    pdart.NodeKey = Utility.GetInt(reader[1]);
                    pdart.PeakLMP = Utility.GetNullDouble(reader[2]);
                    pdart.OffPeakLMP = Utility.GetNullDouble(reader[3]);
                    pdart.StartDate = Utility.GetDateTime(reader[4]);
                    pdart.EndDate = Utility.GetDateTime(reader[5]);
                    pdart.PeakHours = Utility.GetInt(reader[6]);
                    pdart.OffpeakHours = Utility.GetInt(reader[7]);
                    pdart.PeriodName = reader[8].ToString();
                    pdart.PeriodYear = Utility.GetInt(reader[9]);
                    pdart.PeriodKey = Utility.GetInt(reader[10]);
                    pdart.PeriodType = reader[11].ToString();
                    dartList.Add(pdart);
                }

                reader.Close();
            }
            catch { }
            finally
            {
                if (cmd.Connection.State != ConnectionState.Closed)
                    cmd.Connection.Close();
            }
            return dartList;
        }

        /// <summary>
        /// Gets the source sink list.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="sourceNodeKey">The source node key.</param>
        /// <param name="sinkNodeKey">The sink node key.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public static List<PeriodicLMPProc> GetSourceSinkList(DateTime startDate, DateTime endDate, int sourceNodeKey, int sinkNodeKey, int marketKey, string period)
        {
            List<PeriodicLMPProc> lmpList = GetList(startDate, endDate, sourceNodeKey, sinkNodeKey, marketKey, period);

            List<PeriodicLMPProc> finalList = new List<PeriodicLMPProc>();
            IEnumerable<PeriodicLMPProc> sourceList = lmpList.Where(x => x.NodeKey == sourceNodeKey);
            IDictionary<string, PeriodicLMPProc> sinkDic = null;
            try
            {
                if (period == "Daily")
                {
                    if (marketKey == 7 || marketKey == 12)
                        sinkDic = lmpList.Where(x => x.NodeKey == sinkNodeKey).ToDictionary(x => x.MarketTypeCode + x.StartDate + x.PeriodType);
                    else
                        sinkDic = lmpList.Where(x => x.NodeKey == sinkNodeKey).ToDictionary(x => x.MarketTypeCode + x.StartDate);
                }
                else
                    if (marketKey == 7 || marketKey == 12)
                    sinkDic = lmpList.Where(x => x.NodeKey == sinkNodeKey).ToDictionary(x => x.MarketTypeCode + x.StartDate + x.PeriodType + x.PeriodType);
                else
                    sinkDic = lmpList.Where(x => x.NodeKey == sinkNodeKey).ToDictionary(x => x.MarketTypeCode + x.PeriodKey);
            }
            catch (Exception ex)
            {
            }

            foreach (var item in sourceList)
            {
                PeriodicLMPProc proc = new PeriodicLMPProc();

                proc.MarketTypeCode = item.MarketTypeCode;
                proc.NodeKey = item.NodeKey;
                proc.PeakLMP = item.PeakLMP;
                proc.OffPeakLMP = item.OffPeakLMP;
                proc.StartDate = item.StartDate;
                proc.EndDate = item.EndDate;
                proc.PeakHours = item.PeakHours;
                proc.OffpeakHours = item.OffpeakHours;
                proc.PeriodName = item.PeriodName;
                proc.PeriodYear = item.PeriodYear;
                proc.PeriodKey = item.PeriodKey;
                proc.PeriodType = item.PeriodType;

                string priceKey = string.Empty;
                if (period == "Daily")
                {
                    if (marketKey == 7 || marketKey == 12)
                        priceKey = item.MarketTypeCode + item.StartDate + item.PeriodType;
                    else
                        priceKey = item.MarketTypeCode + item.StartDate;
                }
                else
                {
                    if (marketKey == 7 || marketKey == 12)
                        priceKey = item.MarketTypeCode + item.StartDate + item.PeriodType;
                    else
                        priceKey = item.MarketTypeCode + item.PeriodKey;
                }

                if (sinkDic.ContainsKey(priceKey))
                {
                    PeriodicLMPProc sink = sinkDic[priceKey];
                    if (proc.PeakLMP.HasValue && sink.PeakLMP.HasValue)
                    {
                        proc.PeakLMP = sink.PeakLMP - proc.PeakLMP;
                        //proc.PeakLMP /= proc.PeakHours;
                    }

                    if (proc.OffPeakLMP.HasValue && sink.OffPeakLMP.HasValue)
                    {
                        proc.OffPeakLMP = sink.OffPeakLMP - proc.OffPeakLMP;
                        //proc.OffPeakLMP /= proc.OffpeakHours;
                    }
                }

                finalList.Add(proc);
            }



            return finalList;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct FTRKey
    {
        /// <summary>
        /// The node key
        /// </summary>
        public int NodeKey;
        /// <summary>
        /// The period key
        /// </summary>
        public int PeriodKey;
    }
}
