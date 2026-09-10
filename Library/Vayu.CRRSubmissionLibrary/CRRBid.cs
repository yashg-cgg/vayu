namespace Vayu.CRRSubmissionLibrary
{

    public class CRRBid
    {
        public string Trade { get; set; }
        public string PathSource { get; set; }
        public string PathSink { get; set; }
        public string Class { get; set; }
        public string Period { get; set; }
        public string Hedge { get; set; }
        public BidValues[] Bidvals;
        public string TradeType { get; set; }
        public int ID { get; set; }
        public int PortfolioKey;
    }
    public class BidValues
    {
        public int Hour;
        public double MW;
        public double Price;
    }
}
