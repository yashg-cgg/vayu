using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vayu.PNLCalculationLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class PNL
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the da.
        /// </summary>
        /// <value>
        /// The da.
        /// </value>
        public double DA { get; set; }
        /// <summary>
        /// Gets or sets the rt.
        /// </summary>
        /// <value>
        /// The rt.
        /// </value>
        public double RT { get; set; }
        /// <summary>
        /// Gets or sets the PNL value.
        /// </summary>
        /// <value>
        /// The PNL value.
        /// </value>
        public double PnlValue { get; set; }
        /// <summary>
        /// Gets or sets the collect.
        /// </summary>
        /// <value>
        /// The collect.
        /// </value>
        public double Collect { get; set; }
    }
}
