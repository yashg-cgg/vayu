using System;
using System.Collections.Generic;

namespace Vayu.UTCRiskCopy.Model
{
    public interface IDataService
    {
        void InsertBids(List<Vayu.DBLibrary.Portfolio> PortfolioList, DateTime StartDate, string Market, int portfolioid);

        void InsertBidsToTest(List<Vayu.DBLibrary.Portfolio> PortfolioList, DateTime StartDate, string Market);
    }

    public class Path
    {
        public string ScheduleID { get; set; }
        public string BidStatus { get; set; }
        public int OasisID { get; set; }
        public DateTime EndMarketDateTime { get; set; }
        public double RequestedMW { get; set; }
        public double ClearedMW { get; set; }
        public int EndUserKey { get; set; }
        public int SourceNodeKey { get; set; }
        public int SinkNodeKey { get; set; }
        public string POR { get; set; }
        public string POD { get; set; }
        public string Source { get; set; }
        public string Sink { get; set; }
        public double Price { get; set; }
        public int PortfolioKey { get; set; }
        public DateTime SubmittedDateTime { get; set; }
        public string Comments { get; set; }

        public int Hour { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        public Path()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="fromPath">From path.</param>
        public Path(Path fromPath)
        {
            ScheduleID = fromPath.ScheduleID;
            BidStatus = fromPath.BidStatus;
            OasisID = fromPath.OasisID;
            EndMarketDateTime = fromPath.EndMarketDateTime;
            RequestedMW = fromPath.RequestedMW;
            ClearedMW = fromPath.ClearedMW;
            EndUserKey = fromPath.EndUserKey;
            SourceNodeKey = fromPath.SourceNodeKey;
            SinkNodeKey = fromPath.SinkNodeKey;
            POR = fromPath.POR;
            POD = fromPath.POD;
            Source = fromPath.Source;
            Sink = fromPath.Sink;
            Price = fromPath.Price;
            PortfolioKey = fromPath.PortfolioKey;
            SubmittedDateTime = fromPath.SubmittedDateTime;
            Comments = fromPath.Comments;

            Hour = fromPath.Hour;

            //BidId = fromPath.BidId;
            //SourceZone = fromPath.SourceZone;
            //SinkZone = fromPath.SinkZone;
            //AnalysisType = fromPath.AnalysisType;
            //Submit = fromPath.Submit;
            //MW = fromPath.MW;
            //Status = fromPath.Status;
            //Portfolio = fromPath.Portfolio;
            //Market = fromPath.Market;
            //AsBidRisk = fromPath.AsBidRisk;
            //AsBidMaxWin = fromPath.AsBidMaxWin;
            //AsBidRiskReward = fromPath.AsBidRiskReward;
            //AsBidSum = fromPath.AsBidSum;
            //AsBidWinPer = fromPath.AsBidWinPer;
            //MustTakeRisk = fromPath.MustTakeRisk;
            //MustTakeMaxWin = fromPath.MustTakeMaxWin;
            //MustTakeRiskReward = fromPath.MustTakeRiskReward;
            //MustTakeSum = fromPath.MustTakeSum;
            //MustTakeWinPer = fromPath.MustTakeWinPer;
            //AvgDa = fromPath.AvgDa;
            //MinDa = fromPath.MinDa;
            //MaxDa = fromPath.MaxDa;
            //AvgRt = fromPath.AvgRt;
            //MinRt = fromPath.MinRt;
            //MaxRt = fromPath.MaxRt;
            //AvgDart = fromPath.AvgDart;
            //MinDart = fromPath.MinDart;
            //MaxDart = fromPath.MaxDart;
            //ClearedPer = fromPath.ClearedPer;
            //Notional = fromPath.Notional;
            //IsUptos = fromPath.IsUptos;
            //MarketDateTime = fromPath.MarketDateTime;
            //RiskPath = fromPath.RiskPath;
            //PortfolioDate = fromPath.PortfolioDate;
            //Comments = fromPath.Comments;
            //SourcePNodeId = fromPath.SourcePNodeId;
            //SinkPNodeId = fromPath.SinkPNodeId;
            //AsBidAvgDart = fromPath.AsBidAvgDart;
        }


        //public void FireUpdate()
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs("MW"));
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
    }
}
