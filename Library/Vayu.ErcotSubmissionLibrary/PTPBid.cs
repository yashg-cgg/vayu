using System;

namespace Vayu.ErcotSubmissionLibrary
{

    public class BidValues
    {
        public int Hour;
        public double MW;
        public double Price;
    }

    public class PTPBid
    {
        public String BidId;
        public string Source;
        public string Sink;
        public BidValues[] Bidvals;
        public int PortfolioKey;
        public string RequestID;
        public DateTime date;
    }
}
