using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.EnergyPriceService
{
    public class NodeChange
    {
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public long NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the factor.
        /// </summary>
        /// <value>
        /// The factor.
        /// </value>
        public double Factor { get; set; }
        /// <summary>
        /// Gets or sets the updated date.
        /// </summary>
        /// <value>
        /// The updated date.
        /// </value>
        public DateTime UpdatedDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is old node.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is old node; otherwise, <c>false</c>.
        /// </value>
        public bool IsOldNode { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEqualityComparer{NodePriceService.NodeChange}" />
    public class NodeComparer : IEqualityComparer<NodeChange>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(NodeChange x, NodeChange y)
        {
            return ((x.Factor == y.Factor) & (x.NodeKey == y.NodeKey) & (x.IsOldNode == y.IsOldNode) & (x.UpdatedDate == y.UpdatedDate));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(NodeChange obj)
        {
            string hashString = obj.UpdatedDate.ToString() + obj.NodeKey.ToString() + obj.IsOldNode.ToString() + obj.Factor.ToString();
            return hashString.GetHashCode();
        }
    }

    public class ErcotEnergyPriceHelper
    {
        public int hour { get; set; }
        public int Minute { get; set; }
        public int Seconds { get; set; }
        public double EnergyPrice { get; set; }

        public double MaxLoad { get; set; }
    }

    public class ErcotEnergyPriceHourwise
    {
        public double hourEP  {get; set;}
        
    }
}
