using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.DBLibrary
{ 
    /// <summary>
    /// 
    /// </summary>
    public class CRRBid
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the type of the trade.
        /// </summary>
        /// <value>
        /// The type of the trade.
        /// </value>
        public string TradeType { get; set; }
        /// <summary>
        /// Gets or sets the type of the class.
        /// </summary>
        /// <value>
        /// The type of the class.
        /// </value>
        public string ClassType { get; set; }
        /// <summary>
        /// Gets or sets the type of the hedge.
        /// </summary>
        /// <value>
        /// The type of the hedge.
        /// </value>
        public string HedgeType { get; set; }
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the period.
        /// </summary>
        /// <value>
        /// The name of the period.
        /// </value>
        public string PeriodName { get; set; }
        /// <summary>
        /// Gets or sets the period hours.
        /// </summary>
        /// <value>
        /// The period hours.
        /// </value>
        public int PeriodHours { get; set; }
        /// <summary>
        /// Gets or sets the m w1.
        /// </summary>
        /// <value>
        /// The m w1.
        /// </value>
        public double? MW1 { get; set; }
        /// <summary>
        /// Gets or sets the price1.
        /// </summary>
        /// <value>
        /// The price1.
        /// </value>
        public double? Price1 { get; set; }
        /// <summary>
        /// Gets or sets the m w2.
        /// </summary>
        /// <value>
        /// The m w2.
        /// </value>
        public double? MW2 { get; set; }
        /// <summary>
        /// Gets or sets the price2.
        /// </summary>
        /// <value>
        /// The price2.
        /// </value>
        public double? Price2 { get; set; }
        /// <summary>
        /// Gets or sets the m w3.
        /// </summary>
        /// <value>
        /// The m w3.
        /// </value>
        public double? MW3 { get; set; }
        /// <summary>
        /// Gets or sets the price3.
        /// </summary>
        /// <value>
        /// The price3.
        /// </value>
        public double? Price3 { get; set; }
        /// <summary>
        /// Gets or sets the m w4.
        /// </summary>
        /// <value>
        /// The m w4.
        /// </value>
        public double? MW4 { get; set; }
        /// <summary>
        /// Gets or sets the price4.
        /// </summary>
        /// <value>
        /// The price4.
        /// </value>
        public double? Price4 { get; set; }
        /// <summary>
        /// Gets or sets the m w5.
        /// </summary>
        /// <value>
        /// The m w5.
        /// </value>
        public double? MW5 { get; set; }
        /// <summary>
        /// Gets or sets the price5.
        /// </summary>
        /// <value>
        /// The price5.
        /// </value>
        public double? Price5 { get; set; }
        /// <summary>
        /// Gets or sets the m w6.
        /// </summary>
        /// <value>
        /// The m w6.
        /// </value>
        public double? MW6 { get; set; }
        /// <summary>
        /// Gets or sets the price6.
        /// </summary>
        /// <value>
        /// The price6.
        /// </value>
        public double? Price6 { get; set; }
        /// <summary>
        /// Gets or sets the credit.
        /// </summary>
        /// <value>
        /// The credit.
        /// </value>
        public double? Credit { get; set; }
        /// <summary>
        /// Gets or sets the reference price.
        /// </summary>
        /// <value>
        /// The reference price.
        /// </value>
        public double? RefPrice { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
        /// <summary>
        /// Gets or sets the source ext.
        /// </summary>
        /// <value>
        /// The source ext.
        /// </value>
        public long SourceExt { get; set; }
        /// <summary>
        /// Gets or sets the sink ext.
        /// </summary>
        /// <value>
        /// The sink ext.
        /// </value>
        public long SinkExt { get; set; }

        /// <summary>
        /// Gets or sets the source key.
        /// </summary>
        /// <value>
        /// The source key.
        /// </value>
        public int SourceKey { get; set; }
        /// <summary>
        /// Gets or sets the sink key.
        /// </summary>
        /// <value>
        /// The sink key.
        /// </value>
        public int SinkKey { get; set; }

        /// <summary>
        /// Gets or sets the tcrid.
        /// </summary>
        /// <value>
        /// The tcrid.
        /// </value>
        public int TCRID { get; set; }

        //
        /// <summary>
        /// Gets or sets the source Zone.
        /// </summary>
        /// <value>
        /// The source Zone.
        /// </value>
        public string SourceZone { get; set; }
        /// <summary>
        /// Gets or sets the sink Zone.
        /// </summary>
        /// <value>
        /// The sink Zone.
        /// </value>
        public string SinkZone { get; set; }


        /// <summary>
        /// The m market key
        /// </summary>
        public int mMarketKey;
        /// <summary>
        /// The m participant
        /// </summary>
        public string mParticipant;
        /// <summary>
        /// The month
        /// </summary>
        public DateTime month;
        /// <summary>
        /// The m period start date
        /// </summary>
        public DateTime mPeriodStartDate;

        public double? Peak { get; set; }
        public double? OffPeak { get; set; }
        public double? PeakWE { get; set; }
        public double? Total { get; set; }
        public string PortfolioName { get; set; }
        public double? Premium { get; set; }
        public double? PriceLastOBL { get; set; }
        public double? PriceLastOPT { get; set; }
        public double? PriceLastDiff { get; set; }
        public double? PricePrevOBL { get; set; }
        public double? PricePrevOPT { get; set; }
        public double? PricePrevDiff { get; set; }
        public double? MWOwnedOBL { get; set; }
        public double? MWOwnedOPT { get; set; }

        public double? MWSubmittedOBL { get; set; }
        public double? MWSubmittedOPT { get; set; }

        public double? MWClearedOBL { get; set; }
        public double? MWClearedOPT { get; set; }

        public double? MinDA { get; set; }
        public double? MaxDA { get; set; }
        public double? MedianDA45 { get; set; }
        public long SourceExternalid { get; set; }
        public long SinkExternalid { get; set; }
        public long SourceNodekey { get; set; }
        public long SinkNodekey { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="FTRBid"/> class.
        /// </summary>
        public CRRBid()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FTRBid"/> class.
        /// </summary>
        /// <param name="fromFtr">From FTR.</param>
        public CRRBid(CRRBid fromFtr)
        {
            ID = fromFtr.ID;
            Source = fromFtr.Source;
            Sink = fromFtr.Sink;
            TradeType = fromFtr.TradeType;
            ClassType = fromFtr.ClassType;
            HedgeType = fromFtr.HedgeType;
            PeriodKey = fromFtr.PeriodKey;
            PeriodName = fromFtr.PeriodName;
            PeriodHours = fromFtr.PeriodHours;
            MW1 = fromFtr.MW1;
            Price1 = fromFtr.Price1;
            MW2 = fromFtr.MW2;
            Price2 = fromFtr.Price2;
            MW3 = fromFtr.MW3;
            Price3 = fromFtr.Price3;
            MW4 = fromFtr.MW4;
            Price4 = fromFtr.Price4;
            MW5 = fromFtr.MW5;
            Price5 = fromFtr.Price5;
            MW6 = fromFtr.MW6;
            Price6 = fromFtr.Price6;
            Credit = fromFtr.Credit;
            RefPrice = fromFtr.RefPrice;
            Status = fromFtr.Status;
            SourceExt = fromFtr.SourceExt;
            SinkExt = fromFtr.SinkExt;
            SourceZone = fromFtr.SourceZone;
            SinkZone = fromFtr.SinkZone;
            SourceExternalid = fromFtr.SourceExternalid;
            SinkExternalid = fromFtr.SinkExternalid;
            SourceNodekey = fromFtr.SourceNodekey;
            SinkNodekey = fromFtr.SinkNodekey;
            SinkNodekey = fromFtr.SinkNodekey;
        }
    }
}
