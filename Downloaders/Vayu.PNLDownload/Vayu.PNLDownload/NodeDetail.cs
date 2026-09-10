using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vayu.PNLDownload
{
    /// <summary>
    /// define properties
    /// </summary>
    public class NodeDetail
    {
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the p node.
        /// </summary>
        /// <value>
        /// The p node.
        /// </value>
        public long PNode { get; set; }
    }
}
