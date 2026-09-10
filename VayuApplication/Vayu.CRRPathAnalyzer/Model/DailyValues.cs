namespace Vayu.CRRPathAnalyzer.Model
{
    public class DailyCrrValues
    {
        public int CrrAuctionKey { get; set; }
        public double LMPOnPeak { get; set; }
        public double LMPOffPeak { get; set; }
        public double Lmp24Hrs { get; set; }
        public int PeriodKey { get; set; }
        public int PeakHours { get; set; }
        public int OffpeakHours { get; set; }
        public int Hours24 { get; set; }
    }
}
