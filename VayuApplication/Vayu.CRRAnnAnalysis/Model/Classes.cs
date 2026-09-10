using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CRRPeriodCongestionLibrary;
using Vayu.DBLibrary;

namespace Vayu.CRRAnnAnalysis.Model
{
    class Classes
    {
    }
    public class SourceSinkState : SourceSinkData
    {
        /// <summary>
        /// Gets or sets the proc Crr data.
        /// </summary>
        /// <value>
        /// The proc Crr data.
        /// </value>
        public List<PeriodFTRProc> procCrrData { get; set; }
        /// <summary>
        /// Gets or sets the proc daily LMP data.
        /// </summary>
        /// <value>
        /// The proc daily LMP data.
        /// </value>
        public List<PeriodicLMPProc> procDailyLMPData { get; set; }
        /// <summary>
        /// Gets or sets the proc monthly LMP data.
        /// </summary>
        /// <value>
        /// The proc monthly LMP data.
        /// </value>
        public List<PeriodicLMPProc> procMonthlyLMPData { get; set; }

        //public string Source { get; set; }

        //public string Sink { get; set; }
    }
}
