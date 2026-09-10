using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.LoadGraphService
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadGraphServer server = new LoadGraphServer();
            server.Connect();
        }
    }
}
