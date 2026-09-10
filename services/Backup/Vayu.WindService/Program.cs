using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.WindService
{
    class Program
    {
        /// <summary>
        /// Create PJMWindServer class object and call Connect method.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            new WindServer().Connect();
        }
    }
}
