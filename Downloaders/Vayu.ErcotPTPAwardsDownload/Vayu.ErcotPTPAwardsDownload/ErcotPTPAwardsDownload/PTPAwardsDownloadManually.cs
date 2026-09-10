using Microsoft.Web.Services3;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;
using Xml2CSharp;

namespace Vayu.ErcotPTPAwardsDownload
{
  public  class PTPAwardsDownloadManually
    {

        private SqlConnection mConnection;
        private SqlCommand insertBidsommand;
        private SqlCommand cmdDeleteXmlBidsInfoCommand;
        private SqlCommand insertClearedBidsommand;
        private SqlCommand cmdSelectPTPBids;
        X509Certificate2 mCert = new X509Certificate2();
        System.Timers.Timer mTimer = new System.Timers.Timer();
        SoapContext requestContext = null;

        public PTPAwardsDownloadManually()
        {
            Init();
            //  StartTimer();
            ParseXMLMeathod();
        } 

        public void ParseXMLMeathod()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Root));
            //var x = (AwardedPTPObligation)serializer.Deserialize("04182018_AwardedPTPObligation_DAM");
            Root awardPTP;

            using (XmlReader reader = XmlReader.Create("08152019_AwardedPTPObligation_DAM.xml")) // 05262018_AwardedPTPObligation_DAM
            {
                awardPTP = (Root)serializer.Deserialize(reader);
            }
            int count = awardPTP.AwardSet.AwardedPTPObligation.Count;
            List<BidsInfo> bidsInfo = new List<BidsInfo>();
            foreach (var item in awardPTP.AwardSet.AwardedPTPObligation)
            {
                BidsInfo tempBidsInfo = new BidsInfo();
                tempBidsInfo.AwardedMw = Convert.ToDouble(item.AwardedMW);
                tempBidsInfo.BidID = item.BidId;
                tempBidsInfo.EndTime = Convert.ToDateTime(item.EndTime);
                tempBidsInfo.Price = Convert.ToDouble(item.Price);
                tempBidsInfo.User = item.Qse;
                tempBidsInfo.Sink = item.Sink;
                tempBidsInfo.Source = item.Source;
                tempBidsInfo.StartTime = Convert.ToDateTime(item.StartTime);
                tempBidsInfo.TradingDate = Convert.ToDateTime(item.TradingDate);
                bidsInfo.Add(tempBidsInfo);
            }
            int count1 = bidsInfo.Count;
            // List<ClearedBidsInfo> clearedBisInfo = GetBids(bidsInfo);
            if (bidsInfo.Count > 0)
            {
                SaveBidInfo(bidsInfo);
                List<ClearedBidsInfo> clearedBisInfo = GetBids(bidsInfo);
                SaveClearedBids(clearedBisInfo);
                //SaveClearedBids(clearedBisInfo);
            }
        }

        public void SaveBidInfo(List<BidsInfo> bidsInfo)
        {
            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            mConnection.Open();
            cmdDeleteXmlBidsInfoCommand = new SqlCommand();
            cmdDeleteXmlBidsInfoCommand.CommandText = "truncate table  ercot..XmlBidsInfo";
            cmdDeleteXmlBidsInfoCommand.Connection = mConnection;
            cmdDeleteXmlBidsInfoCommand.ExecuteNonQuery();

            insertBidsommand = new SqlCommand();
            insertBidsommand.CommandText = "INSERT INTO ercot..XmlBidsInfo values (@AwardedMw,@BidID,@EndTime,@Price,@UserName,@Sink,@Source,@StartTime,@TradingDate)";
            insertBidsommand.Parameters.AddWithValue("@AwardedMw", "AwardedMw");
            insertBidsommand.Parameters.AddWithValue("@BidID", "BidID");
            insertBidsommand.Parameters.AddWithValue("@EndTime", "EndTime");
            insertBidsommand.Parameters.AddWithValue("@Price", "Price");
            insertBidsommand.Parameters.AddWithValue("@UserName", "UserName");
            insertBidsommand.Parameters.AddWithValue("@Sink", "Sink");
            insertBidsommand.Parameters.AddWithValue("@Source", "Source");
            insertBidsommand.Parameters.AddWithValue("@StartTime", "StartTime");
            insertBidsommand.Parameters.AddWithValue("@TradingDate", "TradingDate");
            //insertBidsommand.Parameters.AddWithValue("@ClearedMW", "");
            insertBidsommand.Connection = mConnection;
            for (int i = 0; i < bidsInfo.Count; i++)
            {
                insertBidsommand.Parameters["@AwardedMw"].Value = bidsInfo[i].AwardedMw;
                insertBidsommand.Parameters["@BidID"].Value = bidsInfo[i].BidID;
                insertBidsommand.Parameters["@EndTime"].Value = bidsInfo[i].EndTime;
                insertBidsommand.Parameters["@Price"].Value = bidsInfo[i].Price;
                insertBidsommand.Parameters["@UserName"].Value = bidsInfo[i].User;
                insertBidsommand.Parameters["@Sink"].Value = bidsInfo[i].Sink;
                insertBidsommand.Parameters["@Source"].Value = bidsInfo[i].Source;
                insertBidsommand.Parameters["@StartTime"].Value = bidsInfo[i].StartTime;
                insertBidsommand.Parameters["@TradingDate"].Value = bidsInfo[i].TradingDate;
                insertBidsommand.ExecuteNonQuery();
            }
            mConnection.Close();

        }

        public List<ClearedBidsInfo> GetBids(List<BidsInfo> bidsInfo)
        {
            List<ClearedBidsInfo> clearedBisInfo = new List<ClearedBidsInfo>();
            SqlDataReader rdr = null;
            if (mConnection.State == ConnectionState.Closed)
                mConnection.Open();
            cmdSelectPTPBids = new SqlCommand();

            cmdSelectPTPBids.CommandText = "select n1.NodeKey as sourceKey, n2.NodeKey as sinkKey ,b.AwardedMw,b.EndTime,b.BidID from  ercot..XmlBidsInfo b " +
                                         "join node n1 on b.Source = n1.nodename  join Node n2  on b.Sink = n2.NodeName  where n1.MarketKey = 9 and n2.MarketKey = 9";
            //cmdSelectPTPBids.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
            //cmdSelectPTPBids.Parameters.AddWithValue("@Source", "Source");
            //cmdSelectPTPBids.Parameters.AddWithValue("@Sink", "Sink");
            //cmdSelectPTPBids.Parameters.AddWithValue("@Price", "Price");
            cmdSelectPTPBids.Connection = mConnection;

            //for (int i = 0; i < bidsInfo.Count; i++)
            //{
            //    cmdSelectPTPBids.Parameters["@EndMarketDateTime"].Value = bidsInfo[i].TradingDate;
            //    cmdSelectPTPBids.Parameters["@Source"].Value = bidsInfo[i].Source;
            //    cmdSelectPTPBids.Parameters["@Sink"].Value = bidsInfo[i].Sink;
            //    cmdSelectPTPBids.Parameters["@Price"].Value = bidsInfo[i].Price;
            rdr = cmdSelectPTPBids.ExecuteReader();
            while (rdr.Read())
            {
                ClearedBidsInfo tempclearedBisInfo = new ClearedBidsInfo();
                double mw = Convert.ToDouble(rdr.GetValue(2));
                if (mw > 0)
                {
                    tempclearedBisInfo.SourceNodeKey = Convert.ToInt32(rdr.GetValue(0));
                    tempclearedBisInfo.SinkNodeKey = Convert.ToInt32(rdr.GetValue(1));
                    tempclearedBisInfo.ClearedMW = mw;
                    tempclearedBisInfo.MarketDateTime = Convert.ToDateTime(rdr.GetValue(3));
                    string x = Convert.ToString(rdr.GetValue(4));
                    string[] y = x.Split('_');
                    if (rdr.IsDBNull(4))
                    {

                    }
                    //  tempclearedBisInfo.PortfolioKey = rdr.IsDBNull(4) ? 2011 : Convert.ToInt32(y[0]);
                    tempclearedBisInfo.PortfolioKey = 2011;
                    tempclearedBisInfo.BidId = Convert.ToString(rdr.GetValue(4));
                    // tempclearedBisInfo.PortfolioKey = 2001;

                    clearedBisInfo.Add(tempclearedBisInfo);
                }
            }

            rdr.Close();
            // }

            mConnection.Close();
            return clearedBisInfo;
        }
        public void SaveClearedBids(List<ClearedBidsInfo> clearedBisInfo)
        {
            List<ClearedBidsInfo> clearedList2011 = new List<ClearedBidsInfo>();
            List<ClearedBidsInfo> actualPortfolioList = new List<ClearedBidsInfo>();

            if (mConnection.State == ConnectionState.Open)
            {
                mConnection.Close();
            }
            mConnection.Open();
            insertClearedBidsommand = new SqlCommand();
            insertClearedBidsommand.CommandText = "INSERT ercot..clearedees_2011 SELECT @SourceNodeKey, @SinkNodeKey, @ClearedMW, @MarketDateTime, null, null, @PortfolioKey , @bidId"
                                                    + " where not exists (SELECT PortfolioKey from ercot..clearedees_2011 where SourceNodeKey = @SourceNodeKey and "
                                                    + " SinkNodeKey = @SinkNodeKey and ClearedMW = @ClearedMW and PortfolioKey = @PortfolioKey "
                                                    + " and MarketDateTime = @MarketDateTime) ";
            insertClearedBidsommand.Parameters.AddWithValue("@SourceNodeKey", "SourceNodeKey");
            insertClearedBidsommand.Parameters.AddWithValue("@SinkNodeKey", "SinkNodeKey");
            insertClearedBidsommand.Parameters.AddWithValue("@ClearedMW", "ClearedMW");
            insertClearedBidsommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            insertClearedBidsommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            insertClearedBidsommand.Parameters.AddWithValue("@BidId", "BidId");
            insertClearedBidsommand.Connection = mConnection;
            for (int i = 0; i < clearedBisInfo.Count; i++)
            {
                insertClearedBidsommand.Parameters["@SourceNodeKey"].Value = clearedBisInfo[i].SourceNodeKey;
                insertClearedBidsommand.Parameters["@SinkNodeKey"].Value = clearedBisInfo[i].SinkNodeKey;
                insertClearedBidsommand.Parameters["@ClearedMW"].Value = clearedBisInfo[i].ClearedMW;
                insertClearedBidsommand.Parameters["@MarketDateTime"].Value = clearedBisInfo[i].MarketDateTime;
                insertClearedBidsommand.Parameters["@PortfolioKey"].Value = clearedBisInfo[i].PortfolioKey;
                insertClearedBidsommand.Parameters["@BidId"].Value = clearedBisInfo[i].BidId;
                insertClearedBidsommand.ExecuteNonQuery();
            }
        }

        private void Init()
        {
            mConnection = new SqlConnection("Data Source = 10.10.2.21,49222; Initial Catalog = ERCOT; Persist Security Info = True; User Id = AryaTrade; password = Arya@1234; Connect Timeout = 100000; MultipleActiveResultSets=True");
            //string certFilePath = @"C:\Ercot\ERCOTCerts\WOAKSQSEnew.cer"; C:\ErcotTrader\Piyush
            string certFilePath = @"C:\ErcotTrader\Piyush\\SubmitType1\\0809369482000$API_20180405piyushd.pfx";
            mCert.Import(certFilePath, "QDGLI##PI50cg", X509KeyStorageFlags.MachineKeySet);

        }

        private void StartTimer()
        {
            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 360000;
            mTimer.Enabled = true;
            while (Console.Read() != 'q') ;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //GetErcotClearedBidsDownload();
            DownloadBids();
        }
        public string DownloadBids()
        {
            //if (timerindex >= 40)
            //{
            //    timerindex = 0;
            //}
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": DownloadBids");

            DownloadPTP();

            return "Downloaded Successfully";
        }

        private void DownloadPTP()
        {
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": Downloadedptp");
            //MarketLibraryErcot.ErcotPTP myPTPUpload = new MarketLibraryErcot.ErcotPTP();
            //PTPBid[] bids = ((TimerArgs)data).bids;
            //int submittype = ((TimerArgs)data).submittype;
            //string resp = myPTPUpload.Download();
        }

        


   





        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {

            return true;
        }
    }

    public class ClearedBidsInfo
    {
        //public int ClearedEESKey { get; set; }
        public int SinkNodeKey { get; set; }
        public int SourceNodeKey { get; set; }
        public double ClearedMW { get; set; }
        public DateTime MarketDateTime { get; set; }
        public int OasisID { get; set; }
        public int ScheduledID { get; set; }
        public int PortfolioKey { get; set; }
        public string BidId { get; set; }

    }

    public class BidsInfo
    {
        public double AwardedMw { get; set; }
        public string BidID { get; set; }
        public DateTime EndTime { get; set; }
        public double Price { get; set; }
        public String User { get; set; }
        public String Sink { get; set; }
        public String Source { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime TradingDate { get; set; }
    }
}
