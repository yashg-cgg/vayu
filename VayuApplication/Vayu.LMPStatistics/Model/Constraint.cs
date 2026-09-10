using System;

namespace Vayu.LMPStatistics.Model
{
    public class Constraint
    {
        public int CnstRTNum { get; set; }
        public DateTime ConstraintDate { get; set; }
        public string ConstraintText { get; set; }
        public string ContingencyText { get; set; }
        public double Sensitivity { get; set; }
        public double? Price { get; set; }
        public double? Impact { get; set; }
        public string TopHours { get; set; }
        public int? Count { get; set; }
        public DateTime? MaxConstraintDate { get; set; }
    }
}
