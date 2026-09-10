using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Vayu.ErcotCRRMonthlyAuctionResultsDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            //ErcotMonthlyAuctionResultsDownload mErcotMonthlyAuctionResultsDownload = new ErcotMonthlyAuctionResultsDownload();
            //mErcotMonthlyAuctionResultsDownload.DownloadUpdate(new DateTime(2024, 07, 01));//First run (main)

            //ErcotMonthlyAuctionResultsDownload1 mErcotMonthlyAuctionResultsDownload1 = new ErcotMonthlyAuctionResultsDownload1();
            //mErcotMonthlyAuctionResultsDownload1.DownloadUpdate(new DateTime(2024, 07, 01));// secondly should run


            ErcotAnnualAuctionResultsDownload ercotAnnualAuctionResultsDownload = new ErcotAnnualAuctionResultsDownload();
            
            
            ercotAnnualAuctionResultsDownload.DownloadAuunalUpdate();




            //loaded only prices to old table
            // ErcotMonthlyAuctionResultsDownload1 ErcotMonthlyAuctionResultsDownload1 = new ErcotMonthlyAuctionResultsDownload1();
            //ErcotMonthlyAuctionResultsDownload1.DownloadUpdate(2021,02,03));
            //ErcotMonthlyAuctionResultsDownload1.DownloadUpdate(new DateTime(DateTime.Today.Year, DateTime.Today.Month + 1, 01));
            //ErcotAnnualAuctionResultsDownload mErcotAnnualAuctionResultsDownload = new ErcotAnnualAuctionResultsDownload();
            //mErcotAnnualAuctionResultsDownload.DownloadAuunalUpdate(new DateTime(2019, 01, 01));

        }
    }
}
