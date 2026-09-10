using System;

namespace Vayu.CRRTopTenParticipants.Model
{
    public class DataItem
    {
        public int Rank { get; set; }
        public string Participant { get; set; }
        public string Company { get; set; }
        public double? PNL { get; set; }
        public double? Monthly { get; set; }
        public double? Annual { get; set; }
        public double? Q1 { get; set; }
        public double? Q2 { get; set; }
        public double? Q3 { get; set; }
        public double? Q4 { get; set; }
        public double? YR1 { get; set; }
        public double? YR2 { get; set; }
        public double? YR3 { get; set; }
        public double? YRALL { get; set; }
        public double? Cost { get; set; }
        //public double? CostMonthly { get; set; }
        //public double? CostQ1 { get; set; }
        //public double? CostQ2 { get; set; }
        //public double? CostQ3 { get; set; }
        //public double? CostQ4 { get; set; }
        //public double? CostAnnual { get; set; }
        //public double? CostYR1 { get; set; }
        //public double? CostYR2 { get; set; }
        //public double? CostYR3 { get; set; }
        //public double? CostYRALL { get; set; }
        public double? DAPrice { get; set; }
        public double? MW { get; set; }
        public DateTime ReportDate { get; set; }
    }
}
