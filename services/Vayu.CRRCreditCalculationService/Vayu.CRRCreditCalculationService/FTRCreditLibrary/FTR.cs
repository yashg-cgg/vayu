using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigma.FTRCreditLibrary
{
    public class FTR
    {
        public string Trade { get; set; }
        public string PathSource { get; set; }
        public string PathSink { get; set; }
        public string Class { get; set; }
        public string Period { get; set; }
        public string Hedge { get; set; }
        public double MW { get; set; }
        public double Price { get; set; }
        public string TradeType { get; set; }
        public int ID { get; set; }
    }
}
