using System;

namespace Vayu.CongestionVolatilityIndex.Model
{
    public class CongestionVolatility
    {
        public string ConstraintText { get; set; }

        public string ContingencyText { get; set; }

        public string MonitoredFacility { get; set; }

        public DateTime ConstraintDate { get; set; }

        public double? HE1 { get; set; }

        public double? HE2 { get; set; }

        public double? HE3 { get; set; }

        public double? HE4 { get; set; }

        public double? HE5 { get; set; }

        public double? HE6 { get; set; }

        public double? HE7 { get; set; }

        public double? HE8 { get; set; }

        public double? HE9 { get; set; }

        public double? HE10 { get; set; }

        public double? HE11 { get; set; }

        public double? HE12 { get; set; }

        public double? HE13 { get; set; }

        public double? HE14 { get; set; }

        public double? HE15 { get; set; }

        public double? HE16 { get; set; }

        public double? HE17 { get; set; }

        public double? HE18 { get; set; }

        public double? HE19 { get; set; }

        public double? HE20 { get; set; }

        public double? HE21 { get; set; }

        public double? HE22 { get; set; }

        public double? HE23 { get; set; }

        public double? HE24 { get; set; }

        public double? Price { get; set; }
        public double? Avg { get; set; }

        public bool Is15Min { get; set; }

        public double? MaxLoad { get; set; }

        public int? SourceNodekey { get; set; }

        public int? SinkNodekey { get; set; }

        public string SourceZone { get; set; }

        public string SinkZone { get; set; }

        public double? TotalHr { get; set; }

        public double? value { get; set; }
        public double? HE { get; set; }
    }
}
