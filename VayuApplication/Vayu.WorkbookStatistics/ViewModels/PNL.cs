using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class PNL
    {
        public string SourceSink { get; set; }

        public double DA { get; set; }

        public double RT { get; set; }

        public double DART { get; set; }

        public PNL()
        {
        }

        public PNL(PNL fromPnl)
        {
            SourceSink = fromPnl.SourceSink;
            DA = fromPnl.DA;
            RT = fromPnl.RT;
            DART = fromPnl.DART;
        }
    }
}
