using System;
using System.Collections.Generic;
using System.Linq;

namespace Vayu.CRRPeriodCongestionLibrary
{

    public class PeriodInfo
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
        /// <summary>
        /// Gets or sets the FTR auction start date.
        /// </summary>
        /// <value>
        /// The FTR auction start date.
        /// </value>
        public DateTime FTRAuctionStartDate { get; set; }
        /// <summary>
        /// Gets or sets the FTR on peak.
        /// </summary>
        /// <value>
        /// The FTR on peak.
        /// </value>
        public double FTROnPeak { get; set; }
        /// <summary>
        /// Gets or sets the auction round.
        /// </summary>
        /// <value>
        /// The auction round.
        /// </value>
        public int AuctionRound { get; set; }
        /// <summary>
        /// Gets or sets the FTR off peak.
        /// </summary>
        /// <value>
        /// The FTR off peak.
        /// </value>
        public double FTROffPeak { get; set; }
        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public string Class { get; set; }

        #endregion

        public static List<PeriodInfo> GetList(DateTime startDate, DateTime endDate, int sourceNodeKey, int sinkNodeKey, int marketKey)
        {
            List<PeriodFTRProc> ftrList = PeriodFTRProcList.GetLatestFTRList(startDate, endDate, sourceNodeKey, sinkNodeKey, marketKey);
            List<PeriodicLMPProc> lmpList = PeriodicLMPProc.GetList(startDate, endDate, sourceNodeKey, sinkNodeKey, marketKey, "");
            List<PeriodInfo> infoList = new List<PeriodInfo>();

            foreach (var item in lmpList)
            {
                PeriodInfo info = new PeriodInfo();

                info.MarketTypeCode = item.MarketTypeCode;
                info.NodeKey = item.NodeKey;
                info.PeakLMP = item.PeakLMP;
                info.OffPeakLMP = item.OffPeakLMP;
                info.StartDate = item.StartDate;
                info.EndDate = item.EndDate;
                info.PeakHours = item.PeakHours;
                info.OffpeakHours = item.OffpeakHours;
                info.PeriodName = item.PeriodName;
                info.PeriodYear = item.PeriodYear;
                info.PeriodKey = item.PeriodKey;
                info.PeriodType = item.PeriodType;
                infoList.Add(info);
            }

            Dictionary<Key, PeriodInfo> infoDic = infoList.ToDictionary(x => new Key() { NodeKey = x.NodeKey, PeriodKey = x.PeriodKey });

            foreach (var item in ftrList)
            {
                Key key = new Key() { NodeKey = item.NodeKey, PeriodKey = item.PeriodKey, TypeCode = item.Class };
                PeriodInfo pinfo = infoDic[key];
                pinfo.FTROnPeak = item.FTROnPeak.GetValueOrDefault();
                pinfo.FTROffPeak = item.FTROffPeak.GetValueOrDefault();
                pinfo.AuctionRound = item.AuctionRound;
                pinfo.FTRAuctionStartDate = item.FTRAuctionStartDate;
            }

            return infoList;
        }
    }
}
