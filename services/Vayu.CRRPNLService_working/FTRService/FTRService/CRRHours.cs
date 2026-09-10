using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.FTRService
{
    class CRRHours
    {
        /// <summary>
        /// Gets or sets the peak.
        /// </summary>
        /// <value>
        /// The peak.
        /// </value>
        public int Peak { get; set; }
        /// <summary>
        /// Gets or sets the peakWE.
        /// </summary>
        /// <value>
        /// The peakWE.
        /// </value>
        public int PeakWE { get; set; }

        /// <summary>
        /// Gets or sets the off peak.
        /// </summary>
        /// <value>
        /// The off peak.
        /// </value>
        public int OffPeak { get; set; }
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public int Total { get; set; }
    }

}
