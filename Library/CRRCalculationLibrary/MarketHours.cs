using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Vayu.CRRCalculationLibrary
{ 
    public class MarketHours
    {
        
        public int Year { get; set; }
        
        public int TotalHoursOnpeak { get; set; }
         
        public int TotalHoursOffpeak { get; set; }
         
        public Dictionary<int, int> Onpeakhash { get; set; }
        
        public Dictionary<int, int> Offpeakhash { get; set; }
        
        public Dictionary<int, int> OnpeakQuarter { get; set; }
         
        public Dictionary<int, int> OffpeakQuarter { get; set; }
    }
}