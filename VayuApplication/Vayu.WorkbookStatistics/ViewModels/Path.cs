using System;
using System.ComponentModel;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class Path : INotifyPropertyChanged
    {
        public string Source { get; set; }

        public string SourceZone { get; set; }

        public string Sink { get; set; }

        public string SinkZone { get; set; }

        public string AnalysisType { get; set; }

        public DateTime MarketDateTime { get; set; }

        public double Price { get; set; }

        public double MW { get; set; }

        public string Status { get; set; }

        public string Portfolio { get; set; }

        public int PortfolioKey { get; set; }

        public string BidId { get; set; }

        public bool Submit { get; set; }

        public int Market { get; set; }

        public bool IsUptos { get; set; }

        public bool RiskPath { get; set; }

        public double? AsBidRisk { get; set; }

        public double? AsBidMaxWin { get; set; }

        public double? AsBidRiskReward { get; set; }
        /// <summary>
        /// Gets or sets as bid sum.
        /// </summary>
        /// <value>
        /// As bid sum.
        /// </value>
        public double? AsBidSum { get; set; }
        /// <summary>
        /// Gets or sets as bid win per.
        /// </summary>
        /// <value>
        /// As bid win per.
        /// </value>
        public double? AsBidWinPer { get; set; }
        /// <summary>
        /// Gets or sets as bid average dart.
        /// </summary>
        /// <value>
        /// As bid average dart.
        /// </value>
        public double? AsBidAvgDart { get; set; }
        /// <summary>
        /// Gets or sets the must take risk.
        /// </summary>
        /// <value>
        /// The must take risk.
        /// </value>
        public double? MustTakeRisk { get; set; }
        /// <summary>
        /// Gets or sets the must take maximum win.
        /// </summary>
        /// <value>
        /// The must take maximum win.
        /// </value>
        public double? MustTakeMaxWin { get; set; }
        /// <summary>
        /// Gets or sets the must take risk reward.
        /// </summary>
        /// <value>
        /// The must take risk reward.
        /// </value>
        public double? MustTakeRiskReward { get; set; }
        /// <summary>
        /// Gets or sets the must take sum.
        /// </summary>
        /// <value>
        /// The must take sum.
        /// </value>
        public double? MustTakeSum { get; set; }
        /// <summary>
        /// Gets or sets the must take win per.
        /// </summary>
        /// <value>
        /// The must take win per.
        /// </value>
        public double? MustTakeWinPer { get; set; }
        /// <summary>
        /// Gets or sets the average da.
        /// </summary>
        /// <value>
        /// The average da.
        /// </value>
        public double? AvgDa { get; set; }
        /// <summary>
        /// Gets or sets the minimum da.
        /// </summary>
        /// <value>
        /// The minimum da.
        /// </value>
        public double? MinDa { get; set; }
        /// <summary>
        /// Gets or sets the maximum da.
        /// </summary>
        /// <value>
        /// The maximum da.
        /// </value>
        public double? MaxDa { get; set; }
        /// <summary>
        /// Gets or sets the average rt.
        /// </summary>
        /// <value>
        /// The average rt.
        /// </value>
        public double? AvgRt { get; set; }
        /// <summary>
        /// Gets or sets the minimum rt.
        /// </summary>
        /// <value>
        /// The minimum rt.
        /// </value>
        public double? MinRt { get; set; }
        /// <summary>
        /// Gets or sets the maximum rt.
        /// </summary>
        /// <value>
        /// The maximum rt.
        /// </value>
        public double? MaxRt { get; set; }
        /// <summary>
        /// Gets or sets the average dart.
        /// </summary>
        /// <value>
        /// The average dart.
        /// </value>
        public double? AvgDart { get; set; }
        /// <summary>
        /// Gets or sets the minimum dart.
        /// </summary>
        /// <value>
        /// The minimum dart.
        /// </value>
        public double? MinDart { get; set; }
        /// <summary>
        /// Gets or sets the maximum dart.
        /// </summary>
        /// <value>
        /// The maximum dart.
        /// </value>
        public double? MaxDart { get; set; }
        /// <summary>
        /// Gets or sets the cleared per.
        /// </summary>
        /// <value>
        /// The cleared per.
        /// </value>
        public double? ClearedPer { get; set; }
        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>
        /// The notional.
        /// </value>
        public double? Notional { get; set; }
        /// <summary>
        /// Gets or sets the portfolio date.
        /// </summary>
        /// <value>
        /// The portfolio date.
        /// </value>
        public DateTime PortfolioDate { get; set; }
        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        public string Comments { get; set; }
        /// <summary>
        /// Gets or sets the source p node identifier.
        /// </summary>
        /// <value>
        /// The source p node identifier.
        /// </value>
        public long SourcePNodeId { get; set; }
        /// <summary>
        /// Gets or sets the sink p node identifier.
        /// </summary>
        /// <value>
        /// The sink p node identifier.
        /// </value>
        public long SinkPNodeId { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// 

        public string Deenergized { get; set; }
        public bool SourceDeenergized { get; set; }
        public bool SinkDeenergized { get; set; }

        public Path()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="fromPath">From path.</param>
        public Path(Path fromPath)
        {
            Source = fromPath.Source;
            SourceZone = fromPath.SourceZone;
            Sink = fromPath.Sink;
            SinkZone = fromPath.SinkZone;
            AnalysisType = fromPath.AnalysisType;
            Price = fromPath.Price;
            MW = fromPath.MW;
            Status = fromPath.Status;
            Portfolio = fromPath.Portfolio;
            PortfolioKey = fromPath.PortfolioKey;
            BidId = fromPath.BidId;
            Submit = fromPath.Submit;
            Market = fromPath.Market;
            AsBidRisk = fromPath.AsBidRisk;
            AsBidMaxWin = fromPath.AsBidMaxWin;
            AsBidRiskReward = fromPath.AsBidRiskReward;
            AsBidSum = fromPath.AsBidSum;
            AsBidWinPer = fromPath.AsBidWinPer;
            MustTakeRisk = fromPath.MustTakeRisk;
            MustTakeMaxWin = fromPath.MustTakeMaxWin;
            MustTakeRiskReward = fromPath.MustTakeRiskReward;
            MustTakeSum = fromPath.MustTakeSum;
            MustTakeWinPer = fromPath.MustTakeWinPer;
            AvgDa = fromPath.AvgDa;
            MinDa = fromPath.MinDa;
            MaxDa = fromPath.MaxDa;
            AvgRt = fromPath.AvgRt;
            MinRt = fromPath.MinRt;
            MaxRt = fromPath.MaxRt;
            AvgDart = fromPath.AvgDart;
            MinDart = fromPath.MinDart;
            MaxDart = fromPath.MaxDart;
            ClearedPer = fromPath.ClearedPer;
            Notional = fromPath.Notional;
            IsUptos = fromPath.IsUptos;
            MarketDateTime = fromPath.MarketDateTime;
            RiskPath = fromPath.RiskPath;
            PortfolioDate = fromPath.PortfolioDate;
            Comments = fromPath.Comments;
            SourcePNodeId = fromPath.SourcePNodeId;
            SinkPNodeId = fromPath.SinkPNodeId;
            AsBidAvgDart = fromPath.AsBidAvgDart;
        }


        public void FireUpdate()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("MW"));
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
