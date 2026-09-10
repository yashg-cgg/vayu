using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Vayu.CRRPeriodCongestionLibrary
{
    public class PeriodFTRProc
    {
        #region Properties

        /// <summary>
        /// Gets or sets the FTR auction start date.
        /// </summary>
        /// <value>
        /// The FTR auction start date.
        /// </value>
        public DateTime FTRAuctionStartDate { get; set; }
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the FTR on peak.
        /// </summary>
        /// <value>
        /// The FTR on peak.
        /// </value>
        public double? FTROnPeak { get; set; }
        /// <summary>
        /// Gets or sets the auction round.
        /// </summary>
        /// <value>
        /// The auction round.
        /// </value>
        public int AuctionRound { get; set; }
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the FTR off peak.
        /// </summary>
        /// <value>
        /// The FTR off peak.
        /// </value>
        public double? FTROffPeak { get; set; }
        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public string Class { get; set; }
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Gets or sets the type of the period.
        /// </summary>
        /// <value>
        /// The type of the period.
        /// </value>
        public string PeriodType { get; set; }
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

        #endregion


        public static List<PeriodFTRProc> GetSourceSinkList(DateTime startDate, DateTime endDate, int sourceNodeKey, int sinkNodeKey, int marketKey)
        {
            List<PeriodFTRProc> finalList = new List<PeriodFTRProc>();
            List<PeriodFTRProc> ftrList = PeriodFTRProcList.GetLatestFTRList(startDate, endDate, sourceNodeKey, sinkNodeKey, marketKey);
            IEnumerable<PeriodFTRProc> sourceList = ftrList.Where(x => x.PeriodType == "Monthly" && x.NodeKey == sourceNodeKey);
            IDictionary<int, PeriodFTRProc> sinkDic = ftrList.Where(x => x.PeriodType == "Monthly" && x.NodeKey == sinkNodeKey).ToDictionary(x => x.PeriodKey);
            foreach (var item in sourceList)
            {
                PeriodFTRProc source = new PeriodFTRProc();
                source.FTRAuctionStartDate = item.FTRAuctionStartDate;
                source.FTROnPeak = item.FTROnPeak;
                source.FTROffPeak = item.FTROffPeak;
                source.PeriodKey = item.PeriodKey;
                source.PeriodType = item.PeriodType;
                source.PeakHours = item.PeakHours;
                source.OffpeakHours = item.OffpeakHours;
                source.StartDate = item.StartDate;

                if (sinkDic.ContainsKey(item.PeriodKey))
                {
                    PeriodFTRProc sink = sinkDic[item.PeriodKey];
                    if (marketKey == 1)
                    {
                        source.FTROffPeak = sink.FTROffPeak - source.FTROffPeak;
                        source.FTROnPeak = sink.FTROnPeak - source.FTROnPeak;
                    }
                    else
                    {
                        source.FTROnPeak = source.FTROnPeak - sink.FTROnPeak;
                        source.FTROffPeak = source.FTROffPeak - sink.FTROffPeak;
                    }
                    source.FTROnPeak /= source.PeakHours;
                    source.FTROffPeak /= source.OffpeakHours;
                }

                finalList.Add(source);
            }
            return finalList;
        }


        public static bool GetHourType(string str)
        {
            return str == "ON" || str == "Peak" || str == "OnPeak";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PeriodFTRProcList
    {
        /// <summary>
        /// Gets the latest FTR list.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="sourceNodeKey">The source node key.</param>
        /// <param name="sinkNodeKey">The sink node key.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public static List<PeriodFTRProc> GetLatestFTRList(DateTime startDate, DateTime endDate, int sourceNodeKey, int sinkNodeKey, int marketKey)
        {
            Dictionary<string, PeriodFTRProc> procDic = new Dictionary<string, PeriodFTRProc>();
            List<PeriodFTRProc> list = new List<PeriodFTRProc>();
            DateTime endDt = new DateTime(endDate.AddMonths(1).Year, endDate.AddMonths(1).Month, 1);
            List<string> commandTexts = new List<string>();

            if (marketKey == 12 || marketKey == 7)
            {
                string marketName = "SPP";
                if (marketKey == 7)
                    marketName = "CAISO";

                commandTexts.Add("select a.FTRAuctionStartDate,NodeKey,Class,ShadowPrice,AuctionRound,p.PeriodKey , pe.StartDate, pe.PeriodType, pe.PeakHrs, pe.OffPeakHrs, 0 "
               + " from " + marketName + ".[FTRAuctionNodePrice] p "
               + " join " + marketName + ".FTRAuction a on p.FTRAuctionKey = a.FTRAuctionKey  "
               + " join Period pe on p.PeriodKey = pe.PeriodKey  "
               + " where pe.MarketKey = " + marketKey + " and pe.StartDate = a.FTRAuctionStartDate"
               + " and  a.FTRAuctionStartDate  between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDt.ToString("yyyy-MM-dd")
               + "' and a.FTRAuctionType = 'Monthly' and p.NodeKey in (" + sourceNodeKey + "," + sinkNodeKey + ")"
               + " order by a.FTRAuctionStartDate desc,AuctionRound desc");

                commandTexts.Add("select FTRAuctionStartDate,NodeKey,Class,ShadowPrice,AuctionRound,p.PeriodKey , p.StartDate, p.PeriodType, p.PeakHrs, p.OffPeakHrs, 0 "
               + " from " + marketName + ".[FTRAuctionNodePrice] f "
               + " join " + marketName + ".FTRAuction k on f.FTRAuctionKey = k.FTRAuctionKey "
               + " join Period p on f.PeriodKey = p.PeriodKey "
               + " where  k.FTRAuctionType = 'Annual' and NodeKey in (" + sourceNodeKey + "," + sinkNodeKey + ") and p.StartDate >= '" +
               startDate.ToString("yyyy-MM-dd") + "' and p.StartDate <=  '" + endDt.ToString("yyyy-MM-dd") + "' and p.MarketKey = " + marketKey);
            }
            else
                commandTexts.Add("exec GetAllFTRData '" + startDate.ToString("yyyy-MM-dd") + "' , '" +
                    endDt.ToString("yyyy-MM-dd") + "' , " + sourceNodeKey + " , " + sinkNodeKey + " ," + marketKey);

            Action<SqlDataReader> readOtherMarkets = (read) =>
            {
                string strKey = Utility.GetInt(read[1]).ToString() + Utility.GetInt(read[5]).ToString();
                PeriodFTRProc proc = null;
                if (procDic.ContainsKey(strKey))
                    proc = procDic[strKey];
                else
                {
                    proc = new PeriodFTRProc();
                    procDic.Add(strKey, proc);
                    proc.FTRAuctionStartDate = Utility.GetDateTime(read[0]);
                    proc.NodeKey = Utility.GetInt(read[1]);
                    proc.AuctionRound = Utility.GetInt(read[4]);
                    proc.PeriodKey = Utility.GetInt(read[5]);
                    proc.StartDate = Utility.GetDateTime(read[6]);
                    proc.PeriodType = read[7].ToString();
                    proc.PeakHours = Utility.GetInt(read[8]);
                    proc.OffpeakHours = Utility.GetInt(read[9]);
                }

                string className = read[2].ToString();
                if (className == "Peak" && !proc.FTROnPeak.HasValue)
                    proc.FTROnPeak = Utility.GetDouble(read[3]);
                else if (className == "Off-peak" && !proc.FTROffPeak.HasValue)
                    proc.FTROffPeak = Utility.GetDouble(read[3]);
            };

            Action<SqlDataReader> readPJMMarkets = (read) =>
            {
                string strKey = Utility.GetInt(read[1]).ToString() + Utility.GetInt(read[5]).ToString();
                if (procDic.ContainsKey(strKey))
                    return;

                PeriodFTRProc proc = new PeriodFTRProc();
                proc.FTRAuctionStartDate = Utility.GetDateTime(read[0]);
                proc.NodeKey = Utility.GetInt(read[1]);
                proc.Class = read[2].ToString();
                proc.FTROnPeak = Utility.GetDouble(read[3]);
                proc.AuctionRound = Utility.GetInt(read[4]);
                proc.PeriodKey = Utility.GetInt(read[5]);
                proc.StartDate = Utility.GetDateTime(read[6]);
                proc.PeriodType = read[7].ToString();
                proc.PeakHours = Utility.GetInt(read[8]);
                proc.OffpeakHours = Utility.GetInt(read[9]);
                proc.FTROffPeak = Utility.GetDouble(read[10]);
                procDic.Add(strKey, proc);
            };

            Action<SqlDataReader> readAction = null;
            if (marketKey == 1)
                readAction = readPJMMarkets;
            else
                readAction = readOtherMarkets;

            SqlCommand command = Utility.GetCommand(DB.TradingData);
            if (command.Connection.State != ConnectionState.Open)
                command.Connection.Open();

            try
            {
                foreach (var item in commandTexts)
                {
                    command.CommandText = item;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                        readAction(reader);
                    reader.Close();
                }
            }
            finally
            {
                if (command.Connection.State != ConnectionState.Closed)
                    command.Connection.Close();
            }

            list = procDic.Values.ToList();
            return list;
        }

        /// <summary>
        /// Gets the type of the hour.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public bool GetHourType(string str)
        {
            return str == "ON" || str == "Peak" || str == "OnPeak";
        }
    }
}
