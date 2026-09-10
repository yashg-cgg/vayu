using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.DBLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class SourceSinkData
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public PricingNode Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public PricingNode Sink { get; set; }

       // public string AnalysisType { get; set; }

     
    }

    /// <summary>
    /// 
    /// </summary>
    public class SourceSinkDetail
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public NodeDetail Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public NodeDetail Sink { get; set; }
    }
}
