using System;

namespace Vayu.PerformanceReview.ViewModels
{
    public class Performance
    {
        public int Year { get; set; }
        public string Month { get; set; }
        public string QuantName { get; set; }
        public double? CRRGross { get; set; }
        public double? CRRFee { get; set; }
        public double? CRRNet { get; set; }
        public double? CRRMw { get; set; }
        public double? PTPGross { get; set; }
        public double? PTPFee { get; set; }
        public double? PTPNet { get; set; }
        //public double? DailyPNLFromStatement { get; set; }
        public double? PTPMW { get; set; }
        public double? PTPDollarsCleared { get; set; }
        public double? CompanyGross { get; set; }
        public double? CompanyFee { get; set; }
        public double? CompanyNet { get; set; }
        public double? CompanyMW { get; set; }
        public DateTime? DateFormat { get; set; }
        public string AccountName { get; set; }
    }
}
