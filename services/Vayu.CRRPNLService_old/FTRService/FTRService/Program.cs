using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.FTRService
{
    class Program
    {
        /// <summary>
        /// Create FTRServer class object and call Connect method.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            CRRServer ftrServer = new CRRServer();
            ftrServer.Connect();
        }
    }
}
