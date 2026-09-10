using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.NodePriceMonitorServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NodePriceServer nodepricemonitorServer = new NodePriceServer();
            nodepricemonitorServer.Connect();
        }
    }
}
