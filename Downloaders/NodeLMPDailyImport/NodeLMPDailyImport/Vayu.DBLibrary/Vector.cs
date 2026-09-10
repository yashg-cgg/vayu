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
    public class Vector
    {
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the sensitivity.
        /// </summary>
        /// <value>
        /// The sensitivity.
        /// </value>
        public double Sensitivity { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class VectorKeyInfo
    {
        /// <summary>
        /// Gets or sets the database text.
        /// </summary>
        /// <value>
        /// The database text.
        /// </value>
        public string DBText { get; set; }
        /// <summary>
        /// Gets or sets the display text.
        /// </summary>
        /// <value>
        /// The display text.
        /// </value>
        public string DisplayText { get; set; }
    }
}
