//#define DA
#define FifteenMins
//#define FiveMins
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vayu.ErcotnodeDAlmpDownload
{
    class Program
    {
        static void Main(string[] args)
        {

           ERCOTNodeDALmpDownload objShortTermReports = new ERCOTNodeDALmpDownload();
#if DA
#endif
#if FifteenMins
            //  Ercot15MinsDownload download = new Ercot15MinsDownload();
#endif
#if FiveMins
           FiveMinsLmpDownload download = new FiveMinsLmpDownload();
#endif
        }
    }
}
