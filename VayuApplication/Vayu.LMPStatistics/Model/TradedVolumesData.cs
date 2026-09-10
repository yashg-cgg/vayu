using System;

namespace Vayu.LMPStatistics.Model
{
    public class TradedVolumesData
    {
        public int NodeKey { get; set; }

        public string NodeName { get; set; }

        public DateTime MarketDateTime { get; set; }

        public string Hour { get; set; }

        public int RowNumber { get; set; }

        public double SourceVolume { get; set; }

        public double SinkVolume { get; set; }

        public double? D1 { set; get; }
        public double? D2 { set; get; }
        public double? D3 { set; get; }
        public double? D4 { set; get; }
        public double? D5 { set; get; }
        public double? D6 { set; get; }
        public double? D7 { set; get; }
        public double? D8 { set; get; }
        public double? D9 { set; get; }

        public double? D10 { set; get; }
        public double? D11 { set; get; }
        public double? D12 { set; get; }
        public double? D13 { set; get; }
        public double? D14 { set; get; }
        public double? D15 { set; get; }
        public double? D16 { set; get; }
        public double? D17 { set; get; }
        public double? D18 { set; get; }
        public double? D19 { set; get; }

        public double? D20 { set; get; }
        public double? D21 { set; get; }
        public double? D22 { set; get; }
        public double? D23 { set; get; }
        public double? D24 { set; get; }
        public double? D25 { set; get; }
        public double? D26 { set; get; }
        public double? D27 { set; get; }
        public double? D28 { set; get; }
        public double? D29 { set; get; }

        public double? D30 { set; get; }
        public double? D31 { set; get; }
    }
    public class sourcesink
    {
        public string Souce { get; set; }

        public string Sink { get; set; }
    }
}
