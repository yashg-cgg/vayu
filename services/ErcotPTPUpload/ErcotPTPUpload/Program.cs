using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vayu.ErcotPTPUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            ERCOTPTPSubmitServer server = new ERCOTPTPSubmitServer();

            //GetErcotBids();
            try
            {
                //ErcotSubmissionLibrary.PTPBid bid = new ErcotSubmissionLibrary.PTPBid();
                //ErcotSubmissionLibrary.PTPBid[] bidArr = new ErcotSubmissionLibrary.PTPBid[2];
                //bid.PortfolioKey = 2002;
                //bid.Sink = "AEEC";
                //bid.Source = "AMISTAD_ALL";
                //bid.BidId = "2001_2";
                //bid.RequestID = "QTALLR." + DateTime.Today.AddDays(1).ToString("yyyyMMdd") + ".PTP." + bid.BidId + "." + bid.Source + "." + bid.Sink;
                //bid.date = DateTime.Today.AddDays(-1);
                //ErcotSubmissionLibrary.BidValues val = new ErcotSubmissionLibrary.BidValues();
                //ErcotSubmissionLibrary.BidValues[] valArr = new ErcotSubmissionLibrary.BidValues[1];
                //val.Hour = 2;  //AEEC	AMISTAD_ALL
                //val.MW = 1;
                //val.Price = 1;
                //valArr[0] = val;
                //bid.Bidvals = valArr;
                //bidArr[0] = bid;
                ////
                //ErcotSubmissionLibrary.PTPBid bid1 = new ErcotSubmissionLibrary.PTPBid();
                //bid1.PortfolioKey = 2001;
                //bid1.Source = "AEEC";
                //bid1.Sink = "AMISTAD_ALL";
                //bid1.BidId = "2001_1";
                //bid1.RequestID = "QTALLR." + DateTime.Today.AddDays(1).ToString("yyyyMMdd") + ".PTP." + bid1.BidId + "." + bid1.Source + "." + bid1.Sink;
                //bid1.date = DateTime.Today.AddDays(-1);
                //ErcotSubmissionLibrary.BidValues val1 = new ErcotSubmissionLibrary.BidValues();
                //ErcotSubmissionLibrary.BidValues[] valArr1 = new ErcotSubmissionLibrary.BidValues[1];
                //val1.Hour = 2;  //AEEC	AMISTAD_ALL
                //val1.MW = 1;
                //val1.Price = 1;
                //valArr1[0] = val;
                //bid1.Bidvals = valArr;
                //bidArr[1] = bid1;
                //
              //  server.CancelBids(bidArr, 0);               
               // server.StartTimer();
                server.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void GetErcotBids()
        {
            throw new NotImplementedException();
        }
    }
}
