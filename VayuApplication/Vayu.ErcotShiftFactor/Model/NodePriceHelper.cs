using System;

namespace Vayu.ErcotShiftFactor.Model
{
    public class NodePriceHelper
    {
        public string NodeName { get; set; }
        public DateTime MktDateTime { get; set; }
        public double LMP { get; set; }
        public double Congestion { get; set; }
        public double Loss { get; set; }
        public string Zone { get; set; }
    }
}
