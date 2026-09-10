using System;

namespace Vayu.NodeSensitivityAnalysis.Model
{
    public class Constraint
    {
        public DateTime ConstraintDate { get; set; }
        public string ConstraintText { get; set; }
        public string ContingencyText { get; set; }
        public double Sensitivity { get; set; }
        public double? Price { get; set; }
        public int? SinkNodekey { get; set; }
        public int? SourceNodekey { get; set; }
    }
}
