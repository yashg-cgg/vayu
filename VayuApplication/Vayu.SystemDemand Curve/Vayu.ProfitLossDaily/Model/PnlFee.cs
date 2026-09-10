using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.ProfitLossDaily.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PnlFee
    {
        /// <summary>
        /// Gets or sets the PNL.
        /// </summary>
        /// <value>
        /// The PNL.
        /// </value>
        public double Pnl { get; set; }
        /// <summary>
        /// Gets or sets the fee.
        /// </summary>
        /// <value>
        /// The fee.
        /// </value>
        public double Fee { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        public double DollarsCleared { get; set; }
    }
}
