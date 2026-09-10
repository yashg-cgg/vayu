using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Vayu.PNLDownload
{
    class Program
    {
        static void Main(string[] args)
        {

            VirtualPnlDownload virtDownload = new VirtualPnlDownload();
            //virtDownload.InitDB();
            ////virtDownload.GetVirtualPnl();

            //virtDownload.GetUptosPnl("PJM");   //uncomment
            virtDownload.GetUptosPnl("ERCOT");

           // virtDownload.GetExternalPnl("ERCOT External");

            //FeeDownload feeDownload = new FeeDownload();  //uncomment for PJM
            //feeDownload.UpdateFees();

            //PJMReserveRatesDownload reserveRatesDownload = new PJMReserveRatesDownload();
            //reserveRatesDownload.UpdateFees();
        }
    }
}

