using System;

namespace Vayu.ProfitLossDaily
{
    /// <summary>
    /// 
    /// </summary>
    public class Pnl
    {
        /// <summary>
        /// Gets or sets the name of the portfolio.
        /// </summary>
        /// <value>
        /// The name of the portfolio.
        /// </value>
        public string PortfolioName { get; set; }
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public DateTime MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the he.
        /// </summary>
        /// <value>
        /// The he.
        /// </value>
        public int HE { get; set; }
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
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the da.
        /// </summary>
        /// <value>
        /// The da.
        /// </value>
        public double DA { get; set; }
        /// <summary>
        /// Gets or sets the rt.
        /// </summary>
        /// <value>
        /// The rt.
        /// </value>
        public double? RT { get; set; }
        /// <summary>
        /// Gets or sets the dart.
        /// </summary>
        /// <value>
        /// The dart.
        /// </value>
        public double? DART { get; set; }
        /// <summary>
        /// Gets or sets the inc decimal.
        /// </summary>
        /// <value>
        /// The inc decimal.
        /// </value>
        public string IncDEC { get; set; }
        /// <summary>
        /// Gets or sets the PNL value.
        /// </summary>
        /// <value>
        /// The PNL value.
        /// </value>
        public double? PnlValue { get; set; }
        /// <summary>
        /// Gets or sets the pay collect.
        /// </summary>
        /// <value>
        /// The pay collect.
        /// </value>
        public double PayCollect { get; set; }
        /// <summary>
        /// Gets or sets the fee.
        /// </summary>
        /// <value>
        /// The fee.
        /// </value>
        public double? Fee { get; set; }
        /// <summary>
        /// Gets or sets the net PNL.
        /// </summary>
        /// <value>
        /// The net PNL.
        /// </value>
        public double NetPnl { get; set; }
        /// <summary>
        /// Gets or sets the cumm PNL.
        /// </summary>
        /// <value>
        /// The cumm PNL.
        /// </value>
        public double CummPnl { get; set; }
        /// <summary>
        /// Gets or sets the source zone.
        /// </summary>
        /// <value>
        /// The source zone.
        /// </value>
        public string SourceZone { get; set; }
        /// <summary>
        /// Gets or sets the sink zone.
        /// </summary>
        /// <value>
        /// The sink zone.
        /// </value>
        public string SinkZone { get; set; }

        public double DACong { get; set; }

        public double RTCong { get; set; }

        public double DALoss { get; set; }

        public double RTLoss { get; set; }

        public double DACongTot { get; set; }

        public double RTCongTot { get; set; }

        public double DALossTot { get; set; }

        public double RTLossTot { get; set; }
        public double? Revenu { get; set; }


    }

    /// <summary>
    /// 
    /// </summary>
    public class Coordinates
    {
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public object Hour { get; set; }
        /// <summary>
        /// Gets or sets the PNL.
        /// </summary>
        /// <value>
        /// The PNL.
        /// </value>
        public double PNL { get; set; }
        /// <summary>
        /// Gets or sets the portfolio.
        /// </summary>
        /// <value>
        /// The portfolio.
        /// </value>
        public string Portfolio { get; set; }

        public double RT { get; set; }

        public double DA { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class XValues
    {
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public object Hour { get; set; }
        /// <summary>
        /// Gets or sets the portfolio.
        /// </summary>
        /// <value>
        /// The portfolio.
        /// </value>
        public string Portfolio { get; set; }

    }
}
