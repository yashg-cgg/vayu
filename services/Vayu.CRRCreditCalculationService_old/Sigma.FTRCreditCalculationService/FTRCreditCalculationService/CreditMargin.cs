using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.CRRCreditCalculationService
{
    public class CreditMargin
    {
        public string ClassType { get; set; }
        public double MW { get; set; }
        public int SourceNodeKey { get; set; }
        public int SinkNodeKey { get; set; }
    }
}
