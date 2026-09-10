using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.MarketViewService
{
    public class NodeChange
    {
        public long NodeKey { get; set; }
        public double Factor { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsOldNode { get; set; }
    }
    public class NodeComparer : IEqualityComparer<NodeChange>
    {
        public bool Equals(NodeChange x, NodeChange y)
        {
            return ((x.Factor == y.Factor) & (x.NodeKey == y.NodeKey) & (x.IsOldNode == y.IsOldNode) & (x.UpdatedDate == y.UpdatedDate));
        }
        public int GetHashCode(NodeChange obj)
        {
            string hashString = obj.UpdatedDate.ToString() + obj.NodeKey.ToString() + obj.IsOldNode.ToString() + obj.Factor.ToString();
            return hashString.GetHashCode();
        }
    }
}
