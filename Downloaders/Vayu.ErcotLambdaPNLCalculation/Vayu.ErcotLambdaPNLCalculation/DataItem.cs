using System;

namespace Vayu.ErcotLambdaPNLCalculation
{
   
        public class DataItem
        {
            public int EnergyHourEnding { get; set; }

            public double SystemLambda { get; set; }

        }

        public class BetweenDataItem
        {
            public DateTime daDate { get; set; }
            public int EnergyHourEnding { get; set; }

            public double SystemLambda { get; set; }
        }
    
}