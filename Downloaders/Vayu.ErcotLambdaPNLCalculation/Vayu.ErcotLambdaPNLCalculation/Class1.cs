using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.ErcotLambdaPNLCalculation
{
    class PNLData
    {
        public DateTime date;
        public int hour;
        public int NodeKey;
        public string A_Source;
        public string A_Sink;
        public double A_MW;
        public double A_DA;
        public double? A_RT;
        public double? A_DART;
        public double? A_COST;
        public double? A_PNL;
        public double? A_REV;

        public string B_Source;
        public string B_Sink;
        public double B_MW;
        public double B_DA;
        public double? B_RT;
        public double? B_DART;
        public double? B_COST;
        public double? B_PNL;
        public double? B_REV;


        public double? Total_PNL;
        public double? Total_COST;
        public double? Total_REV;
    }
}
