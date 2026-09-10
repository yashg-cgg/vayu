using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.CRRSubmissionService
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CRRSubmitServer server = new CRRSubmitServer();
                server.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
