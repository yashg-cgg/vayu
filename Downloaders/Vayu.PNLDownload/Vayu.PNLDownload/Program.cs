using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.IO;

namespace Vayu.PNLDownload
{
    class Program
    {
        static void Main(string[] args)
        {

            VirtualPnlDownload virtDownload = new VirtualPnlDownload();
            
            virtDownload.GetUptosPnl("ERCOT");
            FeeDownload feeDownload = new FeeDownload();
            feeDownload.UpdateFees();

        }


    }
}

