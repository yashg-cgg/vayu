using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.CRRService
{
    class Program
    {
        /// <summary>
        /// Create CRRServer class object and call Connect method.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            CRRServer CRRServer = new CRRServer();
            CRRServer.Connect();
        }
    }
}
