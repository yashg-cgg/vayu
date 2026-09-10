using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.CRRPNLDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            CRRPNLdownloadMain objcrr = new CRRPNLdownloadMain();
            objcrr.CRRPNLCalculationMain();
            //to download for ISO2 only
            //CRRPNLdownloadISO crrPNLCalculationiso = new CRRPNLdownloadISO();
            //crrPNLCalculationiso.CRRPNLCalculationMain();
        }
    }
}
