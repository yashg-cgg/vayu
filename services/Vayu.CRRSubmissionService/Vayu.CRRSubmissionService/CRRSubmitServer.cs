using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using Vayu.CRRSubmissionLibrary;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel.Channels;
using Vayu.CRRNodalServiceLibrary;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Globalization;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRSubmissionService
{
    class CRRSubmitServer : IBidCRRSubmit
    {
        public void Connect()
        {
            ServiceHost host = new ServiceHost(typeof(CRRSubmitServer), new Uri("net.tcp://localhost:8009"));
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.OpenTimeout = new TimeSpan(0, 360, 0);
                binding.SendTimeout = new TimeSpan(0, 360, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 360, 0);
                binding.CloseTimeout = new TimeSpan(0, 360, 0);
                binding.TransactionFlow = false;
                binding.Security.Mode = SecurityMode.None;
                binding.MaxReceivedMessageSize = long.MaxValue;
                binding.MaxBufferPoolSize = long.MaxValue;
                binding.MaxBufferSize = int.MaxValue;
                binding.TransferMode = TransferMode.Streamed;
                binding.ReaderQuotas.MaxArrayLength = 500000000;
                host.AddServiceEndpoint(typeof(IBidCRRSubmit), binding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": started ercot crr submission server 8009");
                    while (true)
                    {
                        Console.ReadLine();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool Subscribe(int traderid)
        {
            return true;
        }
        public bool Unsubscribe()
        {
            return true;
        }
        public bool HeartBeat()
        {
            Console.WriteLine(DateTime.Now + " HeartBeat");
            return true;
        }

        public string Post(CRRBid[] crrBids, DateTime toDate, int portfolio, string market, string marketName, int round, string type)
        {
            Console.WriteLine("Connected to Submit Method");
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": submitbids");
            string success = "";
            success = UploadCRR(crrBids, type);
            return success;
        }
        public string UploadCRR(CRRBid[] crrBid, string type)
        {
            string success = "Error";
            try
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": uploadptp");
                ErcotCRR crrUpload = new ErcotCRR();
                CRRBid[] bids = crrBid;// ((TimerArgs)data).bids;
                int submittype = Convert.ToInt32(type);// ((TimerArgs)data).submittype;
                string resp = crrUpload.Upload(bids, submittype);
                success = "Success";
                DataTable dt = CreateTable();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<root>" + resp + "</root>");
                string mridstr = string.Empty;
                foreach (XmlNode nodeitem in xmlDoc.GetElementsByTagName("ns1:CRR"))
                {
                    mridstr = nodeitem["ns1:mRID"].InnerText;
                    string statusstr = nodeitem["ns1:status"].InnerText;
                    Console.WriteLine("mrid: " + mridstr + " statusstr: " + statusstr);
                    string[] elems = mridstr.Split('.');
                    string bidid = elems[3];
                    dt.Rows.Add(bidid, null, null, null, null, null, statusstr, null, null, null, null);
                }
                UpdateDB(dt, bids[0].PortfolioKey, crrBid[0].Period);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "sucess";
        }

        public DataTable CreateTable()
        {
            DataTable newDT = new DataTable();
            newDT.Columns.Add("CRRBidsKey", typeof(int));
            newDT.Columns.Add("Source", typeof(string));
            newDT.Columns.Add("Sink", typeof(string));
            newDT.Columns.Add("PortfolioKey", typeof(int));
            newDT.Columns.Add("PeriodName", typeof(string));
            newDT.Columns.Add("PeriodKey", typeof(int));
            newDT.Columns.Add("Status", typeof(string));
            newDT.Columns.Add("PeriodType", typeof(string));
            newDT.Columns.Add("ClassType", typeof(string));
            newDT.Columns.Add("TradeType", typeof(string));
            newDT.Columns.Add("Auction", typeof(string));
            return newDT;
        }
        public StringBuilder CreateSubmissionFile(CRRBid[] crrBids, DateTime toDate, string portfolioName, string auctionName, int round, string user, string type)
        {
            DateTime startDate = new DateTime();
            string[] auctionArr = auctionName.Split(' ');
            string month = auctionArr[0].ToString();
            int year = Convert.ToInt32(auctionArr[1].ToString());
            DateTime endDate = new DateTime();
            startDate = new DateTime(year, DateTime.ParseExact(month, "MMM", CultureInfo.CreateSpecificCulture("en-GB")).Month, 1);
            auctionName = year + "." + month + "." + "Monthly." + auctionArr[2];
            StringBuilder buildHelper = GetXmlStringBuilder(crrBids, startDate, startDate.AddMonths(1).AddDays(-1), portfolioName, auctionName);
            return buildHelper;
        }
        private StringBuilder GetXmlStringBuilder(CRRBid[] crrBids, DateTime startDate, DateTime endDate, string portfolio, string AuctionSelectedItem)
        {
            StringBuilder bidText = new StringBuilder();
            try
            {
                bidText.AppendLine("<?xml version=\"1.0\"?>");
                bidText.AppendLine("<bids>");
                bidText.AppendLine("<title>");
                bidText.AppendLine("<portfolioName>" + portfolio + "</portfolioName>");
                bidText.AppendLine("<portfolioDescription>" + portfolio + "-DESC" + "</portfolioDescription>");
                bidText.AppendLine("<auctionName>" + AuctionSelectedItem + "</auctionName>");
                bidText.AppendLine("</title>");
                foreach (CRRBid crrObj in crrBids)
                {
                    bidText.AppendLine("<bid>");
                    bidText.Append("<bidID/>");
                    bidText.Append("<accountHolder>QENJRE</accountHolder>");
                    bidText.Append("<source>" + crrObj.PathSource + "</source>");
                    bidText.Append("<sink>" + crrObj.PathSink + "</sink>");
                    bidText.Append("<mw>" + crrObj.Bidvals[0].MW + "</mw>");
                    bidText.Append("<pricePerMW>" + crrObj.Bidvals[0].Price + "</pricePerMW>");
                    string classType = crrObj.Class.ToUpper() == "PEAKWD" ? "PeakWD" : crrObj.Class.ToUpper() == "PEAKWE" ? "PeakWE" : "Off-peak";
                    bidText.Append("<tou>" + classType + "</tou>");
                    string tradeType = crrObj.TradeType.ToUpper() == "BUY" ? "BUY" : "SELL";
                    bidText.Append("<type>" + tradeType + "</type>");
                    string hedgeType = crrObj.Hedge.ToUpper() == "OBL" ? "OBL" : "OPT";
                    bidText.Append("<hedgeType>" + hedgeType + "</hedgeType>");
                    bidText.Append("<startDate>" + startDate.ToString("yyyy-MM-dd") + "</startDate>");
                    bidText.Append("<endDate>" + endDate.ToString("yyyy-MM-dd") + "</endDate>");
                    bidText.AppendLine("</bid>");
                }
                bidText.AppendLine("</bids>");
            }
            catch
            {


            }
            return bidText;
        }
        public string Cancel(int portfolio, string transaction, int round)
        {
            string status = string.Empty;
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": submitbids");
            CancelCRR(portfolio, transaction, round);


            return status;
        }
        public void UpdateDB(DataTable dt, int portfoliokey, string periodName)
        {
            try
            {
                string status = "Valid";
                SqlConnection dbConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
                if (dbConnection.State == ConnectionState.Closed)
                    dbConnection.Open();
                string updatequery = "Update CRRBids Set Status='" + status + "' where PortfolioKey=" + portfoliokey + " and PeriodName='" + periodName + "' ";
                SqlCommand updateCommand = new SqlCommand(updatequery, dbConnection);
                int updatedrows = updateCommand.ExecuteNonQuery();
            }
            catch
            {

            }
        }
        public void CancelCRR(int Portfolio, string transaction, int round)
        {
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": cancelptp");
            ErcotCRR myCRRCancel = new ErcotCRR();
            string respstr = myCRRCancel.Cancel(Portfolio, transaction, round);
        }
    }
    public class TimerArgs
    {
        public CRRSubmissionLibrary.CRRBid[] bids;
        public ICRRSubmitResultCallback callback;
        public int submittype;
    }
}
