using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.LatestConstraintInformationService
{
    class Program
    {
        static void Main(string[] args)
        {
            ConstrainInformationServer  Server = new ConstrainInformationServer();
            Server.GetNSAConstraintList(9, DateTime.Today, false);
            new ConstrainInformationServer().Connect();
        }
    }
}
