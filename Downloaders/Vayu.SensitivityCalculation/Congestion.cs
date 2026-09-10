using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.SensitivityCalculation
{
    public class Congestion
    {
        public string ConstraintName { get; set; }
        public string ContingencyName { get; set; }
        public double ShadowPrice { get; set; }
    }
}
