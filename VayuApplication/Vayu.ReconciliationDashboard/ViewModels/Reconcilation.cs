using System;

namespace Vayu.ReconciliationDashboard.ViewModels
{
    public class ReconcilationMTLY
    {
        public string Month { get; set; }
        public double? Gross { get; set; }
        public double? MonthGrossISO { get; set; }
        public double? MonthFeeISO { get; set; }
        public double? Fee { get; set; }
        public double? PNL { get; set; }
        public double? ISOMiscellaneousCharges { get; set; }
        public double? MonthlyPNLFromStatement { get; set; }
        public double? MonthlyRunningTotal { get; set; }
        public double? MonthlyRunningTotal1 { get; set; }
        public double? RTM { get; set; }
        public double? DAM { get; set; }
        public double? FeePerMW { get; set; }
        public double? MWs { get; set; }
        public double? DollarsCleared { get; set; }
        public double? Net { get; set; }
        public DateTime DateFormat { get; set; }
    }
    public class ReconcilationDLY
    {
        public string Date { get; set; }
        public double? DailyGross { get; set; }
        public double? DailyGrossISO { get; set; }
        public double? DailyFee { get; set; }
        public double? DailyFeeISO { get; set; }
        public double? DailyPNL { get; set; }
        public double? ISOMiscellaneousCharges { get; set; }
        //public double? DailyPNLFromStatement { get; set; }
        public double? DailyRunningTotal { get; set; }
        public double? RTM { get; set; }
        public double? DAM { get; set; }
        public double? DailyFeePerMW { get; set; }
        public double? DollarsCleared { get; set; }
        public double? MWs { get; set; }
        public DateTime DateFormat { get; set; }
        public double? Net { get; set; }
    }
    public class Reconcilation
    {
        public string Period { get; set; }
        public double? Gross { get; set; }
        public double? Fee { get; set; }
        public double? PNL { get; set; }
        public double? ISOMiscellaneousCharges { get; set; }
        public double? PNLFromStatement { get; set; }
        public double? RunningTotal { get; set; }
    }
}
