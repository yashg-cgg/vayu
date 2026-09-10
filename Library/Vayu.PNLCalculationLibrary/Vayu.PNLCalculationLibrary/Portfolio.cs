using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vayu.PNLCalculationLibrary
{
     
    public class Portfolio
    {
        
        public int ID { get; set; }
        
        public PNL[] Pnls { get; set; }
    }
}
