using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.NodePriceFiveMinService
{
    class Program
    {
        static void Main(string[] args)
        {
            NodePriceFiveMinServer  Server = new NodePriceFiveMinServer();
            Server.Connect();
        }
    }
}
