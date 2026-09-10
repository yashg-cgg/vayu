using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.SensitivityCalculation
{
    class Element
    {
        public long Nodekey;
        public double Congestion;
        public Dictionary<string, double> SensitivityHash;
    }
}
