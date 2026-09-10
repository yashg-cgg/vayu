using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.NodePriceService
{
    class Program
    {
        static void Main(string[] args)
        {
            LMPServer lmp = new LMPServer();
            
            lmp.FillSourceSinkHash();
            
            lmp.Connect();
        }
    }
}
