using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigma.FTRCreditLibrary
{
    public class Credit
    {
        public long ID { get; set; }
        public int SourceNodeKey { get; set; }
        public int SinkNodeKey { get; set; }
        public string Type { get; set; }
        public string HedgeType { get; set; }
        public string PeriodName { get; set; }
        public int PeriodKey { get; set; }
        public int PeriodHours { get; set; }
        public string ClassType { get; set; }
        public List<CreditPriceMW> PriceMWList { get; set; }
    }
}
