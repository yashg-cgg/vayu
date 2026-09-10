using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.SensitivityCalculation
{
    class ConstraintElement
    {
        public DateTime MarketDateTimeInterval { get; set; }
        public string Monitor { get; set; }
        public string Contingency { get; set; }
        public double ShadowPrice { get; set; }
        public int NumFiring { get; set; }
        public double ShiftFactor { get; set; }
        public double Score { get; set; }
        public double ImpactRatio { get; set; }
        public double Impact { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
