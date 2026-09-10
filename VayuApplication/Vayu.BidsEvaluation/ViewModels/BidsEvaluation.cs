using System;

namespace Vayu.BidsEvaluation.ViewModels
{
    public class BidsEvaluationDLY
    {
        public string PortfolioName { get; set; }
        public double? SubmittedCount { get; set; }
        public double? ClearedCount { get; set; }
        public double? TotalUnclearedBids { get; set; }
        public double? ClearedMW { get; set; }
        public double? RequestedMW { get; set; }
        public double? TotalUnclearedMW { get; set; }
        public double? PercentageMWCleared { get; set; }
        public DateTime MarketDate { get; set; }
    }
}
