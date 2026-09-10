using System.Collections.Generic;
using Vayu.NodePriceLibrary;

namespace Vayu.LMPStatistics.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    class LMPList
    {
        /// <summary>
        /// Gets or sets the da list.
        /// </summary>
        /// <value>
        /// The da list.
        /// </value>
        public List<Node> DAList { get; set; }
        /// <summary>
        /// Gets or sets the rt list.
        /// </summary>
        /// <value>
        /// The rt list.
        /// </value>
        public List<Node> RTList { get; set; }
        /// <summary>
        /// Gets or sets the dart list.
        /// </summary>
        /// <value>
        /// The dart list.
        /// </value>
        public List<Node> DARTList { get; set; }
    }
}
