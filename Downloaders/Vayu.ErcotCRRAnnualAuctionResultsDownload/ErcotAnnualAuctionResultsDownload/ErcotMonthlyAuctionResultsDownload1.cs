using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Data.SqlTypes;
using System.Timers;
using System.Globalization;
using Vayu.CertificateInfoLibrary;
using Vayu.CommonAccessLibrary;
using Xml2CSharp;
using System.Xml.Serialization;
using System.Collections;
using System.Threading.Tasks;
namespace Vayu.ErcotCRRMonthlyAuctionResultsDownload
{
    public class ErcotMonthlyAuctionResultsDownload1 /*: IDisposable*/
    {
        private Object thisLock = new Object();
        public X509Certificate2 mCert = new X509Certificate2();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        private SqlConnection DBConnection;
        SqlCommand mSelectCRRAuction;
        SqlCommand mSelectNodes;
        SqlCommand mSelectPeriodCmd;
        SqlCommand mSelectAnnualPeriodCmd;
        public Dictionary<string, int> NodeDictHash = new Dictionary<string, int>();
        protected Hashtable periodHash;
        protected Hashtable annualPeriodHash;
        private string finalfilename;
        public DataTable mERCOTAuctionResultsDT = new DataTable();
        public DataRow drAuctionResult;
        public DataTable mCRRNodalDT = new DataTable();
        public DataRow drCRRNodal;
        public DataTable mOptionPriceDT = new DataTable();
        public DataRow droptionPrice;
        public int CurrentAuctionRound { get; set; }
        public string CurrentAuctionName { get; set; }
        public ErcotMonthlyAuctionResultsDownload1()
        {
            userCertificateDetails = CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
            Console.WriteLine("Authonticated Successfully..");
            InitDB();
            GetNodes();
        }
        public virtual void DownloadUpdate(DateTime? processDate = null)
        {
            if (!processDate.HasValue)
                processDate = DateTime.Now.AddMonths(1);
            int CRRAuctionKey = GetCRRAuctionKey(processDate.Value);
            Download(processDate.Value, CRRAuctionKey);
            //DownloadHistoricalData(processDate.Value, CRRAuctionKey);
        }
        private void Download(DateTime processDate, int CRRAuctionkey, bool updateAuctionResults = true, bool updateNodalPrices = true, bool updateOptionPrices = true)
        {
            Console.WriteLine("Deleteing Old Files.");
            string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ERCOTAuction\");
            foreach (string filePath in filePaths)
                File.Delete(filePath);
            Console.WriteLine("Getting Files..");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=11201");
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.ConnectionLimit = 1;
            request.CookieContainer = new CookieContainer();
            request.Method = "GET";
            request.ClientCertificates.Add(mCert);
            request.Timeout = 1000000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = default(StreamReader);
            sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string strMonthly = sr.ReadToEnd();
            sr.Close();
            string FileURL = string.Empty;
            strMonthly = strMonthly.Replace("\r\n", "").Trim();
            strMonthly = strMonthly.Substring(strMonthly.IndexOf("<b>Monthly Auction Results</b></"));
            string[] splinelitRow = strMonthly.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 3; i < 4; i = 4)
            {
                ///FileURL = "http://mis.ercot.com/misdownload/servlets/mirDownload?mimic_duns=000000000&doclookupId=710223002";
                 FileURL = "https://mis.ercot.com/misdownload/servlets/mirDownload?" + splinelitRow[i].Substring(splinelitRow[i].IndexOf("mimic_duns"), splinelitRow[i].IndexOf("zip") - 45);
            }
            string FileName = @"D:\ISOFiles\ERCOTAuction\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
            // string FileName = @"D:\ISOFiles\ERCOTAuction\rpt.00011201.0000000000000000.20201217.100058790.JAN2021MonthlyCRRAuctionResults.zip";
            HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
            FileDownloadRequest.ClientCertificates.Add(mCert);
            FileDownloadRequest.Timeout = 100000;
            FileDownloadRequest.Method = "GET";
            HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
            string fileDestName = FileName;
            string folderFilename = @"D:\ISOFiles\ERCOTAuction\";
            string nodalFile = string.Empty; string optionFile = string.Empty;
            using (BinaryReader reader = new BinaryReader(FileDownloadresponse.GetResponseStream()))
            {
                using (FileStream fileStream = File.Open(fileDestName, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        byte[] buffer = new byte[2048];
                        int count = reader.Read(buffer, 0, buffer.Length);
                        while (count != 0)
                        {
                            writer.Write(buffer, 0, count);
                            writer.Flush();
                            count = reader.Read(buffer, 0, buffer.Length);
                        }
                        writer.Close();
                        reader.Close();
                    }
                }
            }
            Console.WriteLine("Extracting zip File..");
            XMLUnZipFile(fileDestName, folderFilename);
            string[] filesPath = Directory.GetFiles(@"D:\ISOFiles\ERCOTAuction\");
            foreach (string fileItem in filesPath)
            {
                //if (fileItem.Contains("Common_MarketResults_") && fileItem.Contains("xml"))
                //    finalfilename = fileItem.ToString();
                //if (fileItem.Contains("Common_SourceAndSinkShadowPrices_") && fileItem.Contains("xml"))
                //    nodalFile = fileItem.ToString();
                if (fileItem.Contains("Common_AuctionBidsAndOffers_") && fileItem.Contains("xml"))
                {
                    optionFile = fileItem.ToString();
                }
            }
            //if (updateAuctionResults)
            //    UpdateAuctionResult(finalfilename, CRRAuctionkey, processDate);
            //if (updateNodalPrices)
            //{
            //    UpdateNodalPrices(nodalFile, CRRAuctionkey, processDate);
            //}
            if (updateOptionPrices)
            {
                UpdateOptionPrices(optionFile, CRRAuctionkey, processDate);
            }
            Console.WriteLine("Deleteing Files..");
            filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\ERCOTAnnualAuction\");
            foreach (string filePath in filePaths)
                File.Delete(filePath);
        }
        public void InitDB()
        {
            DBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectCRRAuction = new SqlCommand();
            mSelectCRRAuction.Connection = DBConnection;
            mSelectCRRAuction.CommandText = "select CRRAuctionKey,CRRAuctionName,AuctionRound from CRRAuction where CRRAuctionStartYear=@CRRAuctionYear " +
                " and CRRAuctionStartMonth=@CRRAuctionMonth and CRRAuctionType = 'monthly'";
            mSelectCRRAuction.Parameters.AddWithValue("@CRRAuctionYear", "");
            mSelectCRRAuction.Parameters.AddWithValue("@CRRAuctionMonth", "");

            mSelectNodes = new SqlCommand();
            mSelectNodes.CommandText = "select distinct Nodekey,NodeName from Node";
            mSelectNodes.Connection = DBConnection;

            mSelectPeriodCmd = new SqlCommand();
            mSelectPeriodCmd.CommandText = "select PeriodKey,PeriodName from Period where  MarketKey = 9 and StartDate >= @startDate and EndDate <= @endDate ";
            mSelectPeriodCmd.Parameters.AddWithValue("@startDate", "");
            mSelectPeriodCmd.Parameters.AddWithValue("@endDate", "");
            mSelectPeriodCmd.Connection = DBConnection;

            mSelectAnnualPeriodCmd = new SqlCommand();
            mSelectAnnualPeriodCmd.CommandText = "select PeriodKey,PeriodName,PeriodYear from Period where  MarketKey = 9";
            mSelectAnnualPeriodCmd.Connection = DBConnection;

            mERCOTAuctionResultsDT.Columns.Add("CRRAuctionkey", typeof(int));
            mERCOTAuctionResultsDT.Columns.Add("AuctionName", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("AuctionRound", typeof(int));
            mERCOTAuctionResultsDT.Columns.Add("CRR_ID", typeof(long));
            mERCOTAuctionResultsDT.Columns.Add("AccountHolder", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("Hedge", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("Bid", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("CRR", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("Sourcekey", typeof(int));
            mERCOTAuctionResultsDT.Columns.Add("Sinkkey", typeof(int));
            mERCOTAuctionResultsDT.Columns.Add("SourceName", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("SinkName", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("StartDate", typeof(DateTime));
            mERCOTAuctionResultsDT.Columns.Add("EndDate", typeof(DateTime));
            mERCOTAuctionResultsDT.Columns.Add("TimeUse", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("Bid24Hour", typeof(string));
            mERCOTAuctionResultsDT.Columns.Add("MW", typeof(double));
            mERCOTAuctionResultsDT.Columns.Add("ShadowPrice", typeof(decimal));
            mERCOTAuctionResultsDT.Columns.Add("PeriodKey", typeof(int));


            mCRRNodalDT.Columns.Add("CRRAuctionKey", typeof(int));
            mCRRNodalDT.Columns.Add("NodeKey", typeof(int));
            mCRRNodalDT.Columns.Add("LMPOnPeak", typeof(double));
            mCRRNodalDT.Columns.Add("LMPOffPeak", typeof(double));
            mCRRNodalDT.Columns.Add("PeakWE", typeof(double));
            mCRRNodalDT.Columns.Add("PeriodKey", typeof(int));

            //DataColumn dc1 = new DataColumn();
            //dc1.ColumnName = "CRRAuctionKey";
            //dc1.DataType = typeof(int);

            //DataColumn dc2 = new DataColumn();
            //dc2.ColumnName = "Sourcekey";
            //dc2.DataType = typeof(int);

            //DataColumn dc3 = new DataColumn();
            //dc3.ColumnName = "Sinkkey";
            //dc3.DataType = typeof(int);











            mOptionPriceDT.Columns.Add("CRRAuctionKey", typeof(int));
            mOptionPriceDT.Columns.Add("Sourcekey", typeof(int));
            mOptionPriceDT.Columns.Add("Sinkkey", typeof(int));
            //mOptionPriceDT.Columns.Add(dc1);
            //mOptionPriceDT.Columns.Add(dc2);
            //mOptionPriceDT.Columns.Add(dc3);
            mOptionPriceDT.Columns.Add("PeakWD", typeof(double));
            //mOptionPriceDT.Columns.Add("PeakWDMW", typeof(double));
            mOptionPriceDT.Columns.Add("OffPeak", typeof(double));
            //mOptionPriceDT.Columns.Add("OffPeakMW", typeof(double));
            mOptionPriceDT.Columns.Add("PeakWE", typeof(double));
            //  mOptionPriceDT.Columns.Add("OffPeakMW", typeof(double));
            // mOptionPriceDT.Columns.Add("PeakWEMW", typeof(double));
            mOptionPriceDT.Columns.Add("BidType", typeof(string));
            mOptionPriceDT.Columns.Add("HedgeType", typeof(string));
            mOptionPriceDT.Columns.Add("StartDate", typeof(DateTime));
            mOptionPriceDT.Columns.Add("EndDate", typeof(DateTime));
            //mOptionPriceDT.Columns.Add("MW", typeof(double));
            // mOptionPriceDT.Columns.Add("PeriodKey", typeof(int));

            // mOptionPriceDT.PrimaryKey = new DataColumn[3] { dc1, dc2,dc3  };

            //DataColumn[] key = new DataColumn[3];
            //key[0] = DT.Columns["CRRAuctionKey"];
            //key[1] = DT.Columns["Sourcekey"];
            //key[2] = DT.Columns["Sinkkey"];
            //mOptionPriceDT.PrimaryKey = key;


            //mOptionPriceDT.PrimaryKey = new DataColumn[] { "CRRAuctionKey", "Sourcekey", 
            //    "Sinkkey", "PeakWD", "OffPeak", "PeakWE", "BidType", "HedgeType", "StartDate", 
            //    "EndDate" };
            //DataColumn[] keyColumns = new DataColumn[1];
            //keyColumns[0] = mOptionPriceDT.Columns["<CRRAuctionKey>, <Sourcekey>, <Sinkkey>, <StartDate>, <EndDate>"];
            //mOptionPriceDT.PrimaryKey = keyColumns;


        }
        public void UpdateAuctionResult(string fileName, int CRRAuctionkey, DateTime date)
        {
            mERCOTAuctionResultsDT.Clear();
            FillPeriodHash(date);
            string month = date.ToString("MMM").ToUpper();
            XmlSerializer serializer = new XmlSerializer(typeof(MarketResults));
            MarketResults mResult;
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                mResult = (MarketResults)serializer.Deserialize(reader);
            }
            int count1 = mResult.Result.Count();
            #region XML Data
            foreach (var item in mResult.Result)
            {
                drAuctionResult = mERCOTAuctionResultsDT.NewRow();
                int sourceNodekey = 0, sinkNodekey = 0;
                long crrid = Convert.ToInt64(item.CRR_ID);
                if (crrid == 0)
                    crrid = Convert.ToInt64(item.ORI_CRR_ID);
                if (NodeDictHash.ContainsKey(item.Source))
                    sourceNodekey = NodeDictHash[item.Source];
                if (NodeDictHash.ContainsKey(item.Sink))
                    sinkNodekey = NodeDictHash[item.Sink];
                drAuctionResult["CRRAuctionkey"] = CRRAuctionkey;
                drAuctionResult["AuctionName"] = CurrentAuctionName;
                drAuctionResult["AuctionRound"] = CurrentAuctionRound;
                drAuctionResult["CRR_ID"] = crrid;
                drAuctionResult["AccountHolder"] = item.AccountHolder;
                drAuctionResult["Hedge"] = item.HedgeType;
                drAuctionResult["Bid"] = item.BidType;
                drAuctionResult["CRR"] = item.CRRType;
                drAuctionResult["Sourcekey"] = sourceNodekey;
                drAuctionResult["Sinkkey"] = sinkNodekey;
                drAuctionResult["SourceName"] = item.Source;
                drAuctionResult["SinkName"] = item.Sink;
                drAuctionResult["StartDate"] = Convert.ToDateTime(item.StartDate);
                drAuctionResult["EndDate"] = Convert.ToDateTime(item.EndDate);
                drAuctionResult["TimeUse"] = item.TimeOfUse;
                drAuctionResult["Bid24Hour"] = item.Bid24Hour;
                drAuctionResult["MW"] = Convert.ToDouble(item.MW);
                drAuctionResult["ShadowPrice"] = Convert.ToDecimal(item.ShadowPrice);
                drAuctionResult["PeriodKey"] = periodHash[month];
                mERCOTAuctionResultsDT.Rows.Add(drAuctionResult);
            }
            #endregion XML Data
            int c = mERCOTAuctionResultsDT.Rows.Count;
            if (mERCOTAuctionResultsDT.Rows.Count > 0)
            {
                Console.WriteLine("Auction Result Insert into DB..");
                InsertIntoDB(mERCOTAuctionResultsDT);
            }
        }
        public void UpdateNodalPrices(string fileName, int CRRAuctionkey, DateTime date)
        {
            FillPeriodHash(date);
            XmlSerializer serializer = new XmlSerializer(typeof(ShadowPrices));
            ShadowPrices mNodalPriceData; List<ShadowPriceElement> shadowpriceList = new List<ShadowPriceElement>();
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                mNodalPriceData = (ShadowPrices)serializer.Deserialize(reader);
            }
            int count1 = mNodalPriceData.ShadowPrice.Count();
            mCRRNodalDT.Clear();
            shadowpriceList = mNodalPriceData.ShadowPrice;
            var nodeName = from item in shadowpriceList orderby item.SourceSink select item.SourceSink;
            foreach (var item in nodeName.Distinct())
            {
                drCRRNodal = mCRRNodalDT.NewRow();
                drCRRNodal["CRRAuctionKey"] = CRRAuctionkey;
                drCRRNodal["NodeKey"] = NodeDictHash[item.ToString()];
                //List<ShadowPriceElement> spElement = shadowpriceList.Where(x => x.SourceSink == item.ToString()).ToList();
                var spElement = from spitem in shadowpriceList where spitem.SourceSink == item select spitem;
                for (int i = 0; i < spElement.ToList().Count(); i++)
                {
                    string pName = spElement.ToList()[i].CalendarPeriod.ToString();
                    string[] pArr = pName.Split('_').ToArray();
                    if (spElement.ToList()[i].Tou == "PeakWD")
                    {
                        drCRRNodal["LMPOnPeak"] = Convert.ToDouble(spElement.ToList()[i].ShadowPrice);
                    }
                    if (spElement.ToList()[i].Tou == "Off-peak")
                    {
                        drCRRNodal["LMPOffPeak"] = Convert.ToDouble(spElement.ToList()[i].ShadowPrice);
                    }
                    if (spElement.ToList()[i].Tou == "PeakWE")
                    {
                        drCRRNodal["PeakWE"] = Convert.ToDouble(spElement.ToList()[i].ShadowPrice);
                    }
                    drCRRNodal["PeriodKey"] = periodHash[pArr[0]];
                }
                mCRRNodalDT.Rows.Add(drCRRNodal);
            }
            int nodalCount = mCRRNodalDT.Rows.Count;
            if (mCRRNodalDT.Rows.Count > 1)
            {
                Console.WriteLine("Nodal Prices Insert into DB..");
                NodalPriceInsert(mCRRNodalDT);
            }
        }
        public void UpdateOptionPrices(string fileName, int CRRAuctionkey, DateTime date)
        {
            FillPeriodHash(date);
            string month = date.ToString("MMM").ToUpper();
            XmlSerializer serializer = new XmlSerializer(typeof(AuctionCRRs));
            AuctionCRRs mOptionPriceData;
            List<Crr> optionPriceList = new List<Crr>(); List<Crr> optionParseList = new List<Crr>();
            Dictionary<string, List<Crr>> optionPriceDic = new Dictionary<string, List<Crr>>();
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                mOptionPriceData = (AuctionCRRs)serializer.Deserialize(reader);
            }
            int count = mOptionPriceData.Crr.Count();
            Console.WriteLine("Parsing Option Price File..");
            mOptionPriceDT.Clear();
            optionParseList = mOptionPriceData.Crr.ToList();
            optionPriceList = optionParseList.Where(a => a.HedgeType == "OPT").OrderBy(a => a.Source).ThenBy(a => a.Sink).ToList();
            var nodeName = from item in optionPriceList where item.HedgeType == "OPT" orderby item.Source, item.Sink select new { item.Source, item.Sink };
            foreach (Crr item in optionPriceList)
            {
                //
                List<Crr> objCrrlist = new List<Crr>();
                Crr objCrr = new Crr();
                objCrr.HedgeType = item.HedgeType;
                objCrr.MW = item.MW;
                objCrr.ShadowPrice = item.ShadowPrice;
                objCrr.Sink = item.Sink;
                objCrr.Source = item.Source;
                objCrr.StartDate = item.StartDate;
                objCrr.Tou = item.Tou;
                objCrr.BidPrice = item.BidPrice;
                objCrr.BidType = item.BidType;
                objCrr.EndDate = item.EndDate;
                // objCrr.PeriodKey = item.PeriodKey;
                if (optionPriceDic.ContainsKey(item.Source + item.Sink))
                {
                    objCrrlist = optionPriceDic[item.Source + item.Sink];
                    optionPriceDic.Remove(item.Source + item.Sink);
                }
                objCrrlist.Add(objCrr);
                optionPriceDic.Add(item.Source + item.Sink, objCrrlist);
            }
            foreach (var item in nodeName.Distinct())
            //Parallel.ForEach(nodeName, new ParallelOptions { MaxDegreeOfParallelism = 5}, item =>
            {
                var npElement = optionPriceDic[item.Source + item.Sink];
                var bidpricepeakwd = npElement.Where(x => x.HedgeType == "OPT" && x.BidType == "BUY" && x.Tou == "PeakWD").Select(x => x.BidPrice);
                var bidpriceoffpeak = npElement.Where(x => x.HedgeType == "OPT" && x.BidType == "BUY" && x.Tou == "Off-peak").Select(x => x.BidPrice);
                var bidpricepeakwe = npElement.Where(x => x.HedgeType == "OPT" && x.BidType == "BUY" && x.Tou == "PeakWE").Select(x => x.BidPrice);
                var mwpeakwd = npElement.Where(x => x.HedgeType == "OPT" && x.BidType == "BUY" && x.Tou == "PeakWD").Select(x => x.MW);
                var mwoffpeak = npElement.Where(x => x.HedgeType == "OPT" && x.BidType == "BUY" && x.Tou == "Off-peak").Select(x => x.MW);
                var mwpeakwe = npElement.Where(x => x.HedgeType == "OPT" && x.BidType == "BUY" && x.Tou == "PeakWE").Select(x => x.MW);

                int maxcount = Math.Max(Math.Max(bidpricepeakwd.Count(), bidpriceoffpeak.Count()), bidpricepeakwe.Count());

                for (int j = 0; j < maxcount; j++)
                {
                    droptionPrice = mOptionPriceDT.NewRow();
                    droptionPrice["CRRAuctionKey"] = CRRAuctionkey;
                    droptionPrice["Sourcekey"] = NodeDictHash[item.Source.ToString()];
                    droptionPrice["Sinkkey"] = NodeDictHash[item.Sink.ToString()];
                    //var npElement = optionPriceDic[item.Source + item.Sink];
                    double sumpeakwd = 0;
                    double sumoffpeak = 0;
                    double sumpeakwe = 0;
                    //   List<Crr> crrHelperList = optionPriceList.Where(a => a.Source == item.Source && a.Sink == item.Sink).ToList();
                    for (int i = 0; i < npElement.ToList().Count(); i++)
                    {
                        string sp = npElement.ToList()[i].ShadowPrice;
                        string mwsp = npElement.ToList()[i].MW;

                        if (npElement.ToList()[i].HedgeType == "OPT" && npElement.ToList()[i].BidType == "BUY")
                        {
                            if (npElement.ToList()[i].Tou == "PeakWD")
                            {
                                droptionPrice["PeakWD"] = Convert.ToDouble(npElement.ToList()[i].ShadowPrice);
                                sumpeakwd = sumpeakwd + Convert.ToDouble(npElement.ToList()[i].MW);
                                //droptionPrice["PeakWDMW"] = sumpeakwd;
                            }
                            else if (npElement.ToList()[i].Tou == "Off-peak")
                            {
                                droptionPrice["OffPeak"] = Convert.ToDouble(npElement.ToList()[i].ShadowPrice);
                                sumoffpeak = sumoffpeak + Convert.ToDouble(npElement.ToList()[i].MW);
                                // droptionPrice["OffPeakMW"] = sumoffpeak;
                            }
                            else if (npElement.ToList()[i].Tou == "PeakWE")
                            {
                                droptionPrice["PeakWE"] = Convert.ToDouble(npElement.ToList()[i].ShadowPrice.ToString());
                                sumpeakwe = sumpeakwe + Convert.ToDouble(npElement.ToList()[i].MW);
                                //droptionPrice["PeakWEMW"] = sumpeakwe;
                            }
                            droptionPrice["BidType"] = npElement.ToList()[i].BidType.ToString();
                            droptionPrice["HedgeType"] = npElement.ToList()[i].HedgeType.ToString();
                            droptionPrice["StartDate"] = npElement.ToList()[i].StartDate;
                            droptionPrice["EndDate"] = npElement.ToList()[i].EndDate;

                        }
                    }

                    //if (bidpricepeakwd.Count() > j)
                    //{
                    //    droptionPrice["BidPricePeakWD"] = Convert.ToDouble(bidpricepeakwd.ToList()[j]);
                    //   // droptionPrice["MWWD"] = Convert.ToDouble(mwpeakwd.ToList()[j]);
                    //}
                    //if (bidpriceoffpeak.Count() > j)
                    //{
                    //    droptionPrice["BidPriceOffpeak"] = Convert.ToDouble(bidpriceoffpeak.ToList()[j]);
                    //    //droptionPrice["MWOffP"] = Convert.ToDouble(mwoffpeak.ToList()[j]);
                    //}
                    //if (bidpricepeakwe.Count() > j)
                    //{
                    //    droptionPrice["BidPricePeakWE"] = Convert.ToDouble(bidpricepeakwe.ToList()[j]);
                    //    //droptionPrice["MWWE"] = Convert.ToDouble(mwpeakwe.ToList()[j]);
                    //}
                    mOptionPriceDT.Rows.Add(droptionPrice);
                }
            }
            //int optcount = mOptionPriceDT.Rows.Count;
            if (mOptionPriceDT.Rows.Count > 0)
                InsertOptionPrice(mOptionPriceDT);
        }

        public void UpdateHistoricalNodalPrices(string fileName, int CRRAuctionkey, DateTime date)
        {
            FillPeriodHash(date);
            XmlSerializer serializer = new XmlSerializer(typeof(HistoricalShadowPrices));
            HistoricalShadowPrices mNodalPriceData; List<SourceSinkData> shadowpriceList = new List<SourceSinkData>();
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                mNodalPriceData = (HistoricalShadowPrices)serializer.Deserialize(reader);
            }
            int count1 = mNodalPriceData.SourceSink.Count();
            mCRRNodalDT.Clear();
            shadowpriceList = mNodalPriceData.SourceSink;
            var nodeName = from item in shadowpriceList orderby item.SourceSink select item.SourceSink;
            foreach (var item in nodeName.Distinct())
            {
                drCRRNodal = mCRRNodalDT.NewRow();
                drCRRNodal["CRRAuctionKey"] = CRRAuctionkey;
                drCRRNodal["NodeKey"] = NodeDictHash[item.ToString()];
                //List<ShadowPriceElement> spElement = shadowpriceList.Where(x => x.SourceSink == item.ToString()).ToList();
                var spElement = from spitem in shadowpriceList where spitem.SourceSink == item select spitem;
                for (int i = 0; i < spElement.ToList().Count(); i++)
                {
                    string pName = spElement.ToList()[i].CalendarPeriod.ToString();
                    string[] pArr = pName.Split(' ').ToArray();
                    if (spElement.ToList()[i].Tou == "PeakWD")
                    {
                        drCRRNodal["LMPOnPeak"] = Convert.ToDouble(spElement.ToList()[i].ShadowPrice);
                    }
                    if (spElement.ToList()[i].Tou == "Off-peak")
                    {
                        drCRRNodal["LMPOffPeak"] = Convert.ToDouble(spElement.ToList()[i].ShadowPrice);
                    }
                    if (spElement.ToList()[i].Tou == "PeakWE")
                    {
                        drCRRNodal["PeakWE"] = Convert.ToDouble(spElement.ToList()[i].ShadowPrice);
                    }
                    drCRRNodal["PeriodKey"] = periodHash[pArr[0]];
                }
                mCRRNodalDT.Rows.Add(drCRRNodal);
            }
            int nodalCount = mCRRNodalDT.Rows.Count;
            if (mCRRNodalDT.Rows.Count > 1)
            {
                Console.WriteLine("Nodal Prices Insert into DB..");
                NodalPriceInsert(mCRRNodalDT);
            }
        }
        private int GetCRRAuctionKey(DateTime date)
        {
            int CRRAuctionKey = -1;
            try
            {
                if (mSelectCRRAuction.Connection.State == System.Data.ConnectionState.Closed)
                    mSelectCRRAuction.Connection.Open();

                mSelectCRRAuction.Parameters["@CRRAuctionYear"].Value = Convert.ToInt32(date.Year);
                mSelectCRRAuction.Parameters["@CRRAuctionMonth"].Value = Convert.ToInt32(date.Month);

                if (mSelectCRRAuction.Connection.State != System.Data.ConnectionState.Open)
                    mSelectCRRAuction.Connection.Open();

                SqlDataReader reader1 = mSelectCRRAuction.ExecuteReader();
                if (reader1.Read())
                {
                    CRRAuctionKey = (int)reader1.GetDecimal(0);
                    CurrentAuctionName = reader1[1].ToString();
                    CurrentAuctionRound = (int)reader1.GetDecimal(2);
                }
                reader1.Close();
                DBConnection.Close();
            }
            catch
            {

            }
            return CRRAuctionKey;
        }
        public void GetNodes()
        {
            try
            {
                if (mSelectNodes.Connection.State == System.Data.ConnectionState.Closed)
                    mSelectNodes.Connection.Open();
                SqlDataReader reader = mSelectNodes.ExecuteReader();
                while (reader.Read())
                {
                    int Nodekey = (int)reader[0];
                    string NodeName = (string)reader.GetString(1);
                    if (!NodeDictHash.ContainsKey(NodeName))
                        NodeDictHash.Add(NodeName, Nodekey);
                }
                reader.Close();
                DBConnection.Close();
            }
            catch
            {

            }
        }
        protected virtual void FillPeriodHash(DateTime date)
        {
            date = date.AddYears(-1);
            if (mSelectPeriodCmd.Connection.State != ConnectionState.Open)
                mSelectPeriodCmd.Connection.Open();

            DateTime start = new DateTime(2016, 06, 01);
            DateTime end = new DateTime(2018, 05, 31); ;
            periodHash = new Hashtable();
            if (date.Month > 5)
            {
                start = new DateTime(date.Year, 6, 1);
                end = new DateTime(date.Year + 1, 5, 31);
            }
            else
            {
                start = new DateTime(date.Year + 1 - 1, 6, 1);
                end = new DateTime(date.Year + 1, 5, 31);
            }

            mSelectPeriodCmd.Parameters["@startDate"].Value = start;
            mSelectPeriodCmd.Parameters["@endDate"].Value = end;
            SqlDataReader reader = mSelectPeriodCmd.ExecuteReader();

            while (reader.Read())
                periodHash[reader[1].ToString().ToUpper().Trim()] = (int)reader.GetValue(0);

            reader.Close();
            mSelectPeriodCmd.Connection.Close();
        }
        public static string XMLUnZipFile(string InputPathOfZipFile, string FolderFilename)
        {
            string strNewFile = "";
            try
            {
                if (File.Exists(InputPathOfZipFile))
                {
                    string baseDirectory = Path.GetDirectoryName(InputPathOfZipFile);
                    using (ZipInputStream ZipStream = new ZipInputStream(File.OpenRead(InputPathOfZipFile)))
                    {
                        ZipEntry theEntry;
                        while ((theEntry = ZipStream.GetNextEntry()) != null)
                        {
                            if (theEntry.IsFile)
                            {
                                if (theEntry.Name != "")
                                {
                                    strNewFile = @"" + baseDirectory + @"\" + theEntry.Name; //FolderFilename;
                                    if (File.Exists(strNewFile))
                                    {
                                        continue;
                                    }
                                    using (FileStream streamWriter = File.Create(strNewFile))
                                    {
                                        int size = 2048;
                                        byte[] data = new byte[2048];
                                        while (true)
                                        {
                                            size = ZipStream.Read(data, 0, data.Length);
                                            if (size > 0)
                                            {
                                                streamWriter.Write(data, 0, size);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        streamWriter.Close();
                                    }
                                }
                            }
                            else if (theEntry.IsDirectory)
                            {
                                string strNewDirectory = @"" + baseDirectory + @"\" + theEntry.Name;
                                if (!Directory.Exists(strNewDirectory))
                                {
                                    Directory.CreateDirectory(strNewDirectory);
                                }
                            }
                        }
                        ZipStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return strNewFile;
        }
        private void DownloadHistoricalData(DateTime processDate, int CRRAuctionkey, bool updateAuctionResults = true, bool updateNodalPrices = true)
        {
            Console.WriteLine("Getting Files...");
            string fileName = string.Empty;
            string[] files = Directory.GetFiles(@"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\SigmaTradingProjects\Historic CRR Auction\FileWork\");
            foreach (var item in files)
            {
                fileName = item;
            }
            string folderFilename = @"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\SigmaTradingProjects\Historic CRR Auction\FileWork\";
            XMLUnZipFile(fileName, folderFilename);
            File.Delete(fileName);
            string nodalFile = string.Empty;
            string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\SigmaTradingProjects\Historic CRR Auction\FileWork\");
            foreach (string fileItem in filePaths)
            {
                if (fileItem.Contains("Common_MarketResults_") && fileItem.Contains("xml"))
                    finalfilename = fileItem;
                if (fileItem.Contains("Common_SourceAndSinkShadowPrices_") && fileItem.Contains("xml"))
                    nodalFile = fileItem.ToString();
            }
            //if (updateAuctionResults)
            //    UpdateAuctionResult(finalfilename, CRRAuctionkey);
            if (updateNodalPrices)
            {
                UpdateHistoricalNodalPrices(nodalFile, CRRAuctionkey, processDate);
            }
            #region OLD
            //XmlSerializer serializer = new XmlSerializer(typeof(MarketResults));
            //MarketResults mResult;
            //using (XmlReader reader = XmlReader.Create(finalfilename))
            //{
            //    mResult = (MarketResults)serializer.Deserialize(reader);
            //}
            //int count1 = mResult.Result.Count();
            //#region XML Data
            //foreach (var item in mResult.Result)
            //{
            //    drAuctionResult = mERCOTAuctionResultsDT.NewRow();
            //    int sourceNodekey = 0, sinkNodekey = 0;
            //    long crrid = Convert.ToInt64(item.CRR_ID);
            //    if (crrid == 0)
            //        crrid = Convert.ToInt64(item.ORI_CRR_ID);
            //    if (NodeDictHash.ContainsKey(item.Source))
            //        sourceNodekey = NodeDictHash[item.Source];
            //    if (NodeDictHash.ContainsKey(item.Sink))
            //        sinkNodekey = NodeDictHash[item.Sink];
            //    drAuctionResult["CRRAuctionkey"] = CRRAuctionkey;
            //    drAuctionResult["AuctionName"] = CurrentAuctionName;
            //    drAuctionResult["AuctionRound"] = CurrentAuctionRound;
            //    drAuctionResult["CRR_ID"] = crrid;
            //    drAuctionResult["AccountHolder"] = item.AccountHolder;
            //    drAuctionResult["Hedge"] = item.HedgeType;
            //    drAuctionResult["Bid"] = item.BidType;
            //    drAuctionResult["CRR"] = item.CRRType;
            //    drAuctionResult["Sourcekey"] = sourceNodekey;
            //    drAuctionResult["Sinkkey"] = sinkNodekey;
            //    drAuctionResult["SourceName"] = item.Source;
            //    drAuctionResult["SinkName"] = item.Sink;
            //    drAuctionResult["StartDate"] = Convert.ToDateTime(item.StartDate);
            //    drAuctionResult["EndDate"] = Convert.ToDateTime(item.EndDate);
            //    drAuctionResult["TimeUse"] = item.TimeOfUse;
            //    drAuctionResult["Bid24Hour"] = item.Bid24Hour;
            //    drAuctionResult["MW"] = Convert.ToDouble(item.MW);
            //    drAuctionResult["ShadowPrice"] = Convert.ToDecimal(item.ShadowPrice);
            //    mERCOTAuctionResultsDT.Rows.Add(drAuctionResult);
            //}
            //#endregion XML Data
            //int c = mERCOTAuctionResultsDT.Rows.Count;
            //if (mERCOTAuctionResultsDT.Rows.Count > 0)
            //{
            //    Console.WriteLine("Insert into DB..");
            //    if (DBConnection.State == ConnectionState.Open)
            //    {
            //        DBConnection.Close();
            //    }
            //    DBConnection.Open();
            //    SqlTransaction transaction = DBConnection.BeginTransaction();
            //    using (SqlBulkCopy bkMAuction = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
            //    {
            //        try
            //        {
            //            bkMAuction.DestinationTableName = "[ERCOT]..[crrauctionresults_test]";
            //            bkMAuction.ColumnMappings.Add("CRRAuctionkey", "CRRAuctionkey");
            //            bkMAuction.ColumnMappings.Add("AuctionName", "AuctionName");
            //            bkMAuction.ColumnMappings.Add("AuctionRound", "AuctionRound");
            //            bkMAuction.ColumnMappings.Add("CRR_ID", "CRR_ID");
            //            bkMAuction.ColumnMappings.Add("AccountHolder", "AccountHolder");
            //            bkMAuction.ColumnMappings.Add("Hedge", "Hedge");
            //            bkMAuction.ColumnMappings.Add("Bid", "Bid");
            //            bkMAuction.ColumnMappings.Add("CRR", "CRR");
            //            bkMAuction.ColumnMappings.Add("Sourcekey", "Sourcekey");
            //            bkMAuction.ColumnMappings.Add("Sinkkey", "Sinkkey");
            //            bkMAuction.ColumnMappings.Add("SourceName", "SourceName");
            //            bkMAuction.ColumnMappings.Add("SinkName", "SinkName");
            //            bkMAuction.ColumnMappings.Add("StartDate", "StartDate");
            //            bkMAuction.ColumnMappings.Add("EndDate", "EndDate");
            //            bkMAuction.ColumnMappings.Add("TimeUse", "TimeUse");
            //            bkMAuction.ColumnMappings.Add("Bid24Hour", "Bid24Hour");
            //            bkMAuction.ColumnMappings.Add("MW", "MW");
            //            bkMAuction.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
            //            bkMAuction.WriteToServer(mERCOTAuctionResultsDT);
            //            transaction.Commit();
            //        }
            //        catch (Exception ex)
            //        {
            //            transaction.Rollback();
            //            //mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            //        }
            //    }
            //}
            #endregion OLD
            Console.WriteLine("Deleteing Files..");
            filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\SigmaTradingProjects\Historic CRR Auction\FileWork\");
            foreach (string filePath in filePaths)
                File.Delete(filePath);
            DBConnection.Close();
        }
        public void InsertIntoDB(DataTable monthlyDT)
        {
            if (DBConnection.State == ConnectionState.Closed)
                DBConnection.Open();
            SqlTransaction transaction = DBConnection.BeginTransaction();
            using (SqlBulkCopy bkMAuction = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkMAuction.DestinationTableName = "CRRAuctionResults";
                    bkMAuction.ColumnMappings.Add("CRRAuctionkey", "CRRAuctionkey");
                    bkMAuction.ColumnMappings.Add("AuctionName", "AuctionName");
                    bkMAuction.ColumnMappings.Add("AuctionRound", "AuctionRound");
                    bkMAuction.ColumnMappings.Add("CRR_ID", "CRR_ID");
                    bkMAuction.ColumnMappings.Add("AccountHolder", "AccountHolder");
                    bkMAuction.ColumnMappings.Add("Hedge", "Hedge");
                    bkMAuction.ColumnMappings.Add("Bid", "Bid");
                    bkMAuction.ColumnMappings.Add("CRR", "CRR");
                    bkMAuction.ColumnMappings.Add("Sourcekey", "Sourcekey");
                    bkMAuction.ColumnMappings.Add("Sinkkey", "Sinkkey");
                    bkMAuction.ColumnMappings.Add("SourceName", "SourceName");
                    bkMAuction.ColumnMappings.Add("SinkName", "SinkName");
                    bkMAuction.ColumnMappings.Add("StartDate", "StartDate");
                    bkMAuction.ColumnMappings.Add("EndDate", "EndDate");
                    bkMAuction.ColumnMappings.Add("TimeUse", "TimeUse");
                    bkMAuction.ColumnMappings.Add("Bid24Hour", "Bid24Hour");
                    bkMAuction.ColumnMappings.Add("MW", "MW");
                    bkMAuction.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                    bkMAuction.ColumnMappings.Add("PeriodKey", "PeriodKey");
                    bkMAuction.WriteToServer(monthlyDT);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            DBConnection.Close();
        }
        public void NodalPriceInsert(DataTable mNodalData)
        {
            if (DBConnection.State == ConnectionState.Closed)
                DBConnection.Open();
            SqlTransaction transaction = DBConnection.BeginTransaction();
            using (SqlBulkCopy BKNodalPrice = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    BKNodalPrice.DestinationTableName = "CRRAuctionNodePrice_test";
                    BKNodalPrice.ColumnMappings.Add("CRRAuctionKey", "CRRAuctionKey");
                    BKNodalPrice.ColumnMappings.Add("NodeKey", "NodeKey");
                    BKNodalPrice.ColumnMappings.Add("LMPOnPeak", "LMPOnPeak");
                    BKNodalPrice.ColumnMappings.Add("LMPOffPeak", "LMPOffPeak");
                    BKNodalPrice.ColumnMappings.Add("PeakWE", "PeakWE");
                    BKNodalPrice.ColumnMappings.Add("PeriodKey", "PeriodKey");
                    BKNodalPrice.WriteToServer(mNodalData);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            DBConnection.Close();
        }
        public void InsertOptionPrice(DataTable mOptionData)
        {
            Console.WriteLine("Insert into DB Option Pricess..");
            if (DBConnection.State == ConnectionState.Closed)
                DBConnection.Open();
            SqlTransaction transaction = DBConnection.BeginTransaction();
            using (SqlBulkCopy BKOptionPrice = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    BKOptionPrice.DestinationTableName = "CRRAuctionOptionPrice";
                    BKOptionPrice.ColumnMappings.Add("CRRAuctionKey", "CRRAuctionKey");
                    BKOptionPrice.ColumnMappings.Add("Sourcekey", "Sourcekey");
                    BKOptionPrice.ColumnMappings.Add("Sinkkey", "Sinkkey");
                    BKOptionPrice.ColumnMappings.Add("PeakWD", "PeakWD");
                    BKOptionPrice.ColumnMappings.Add("OffPeak", "OffPeak");
                    BKOptionPrice.ColumnMappings.Add("PeakWE", "PeakWE");
                    BKOptionPrice.ColumnMappings.Add("BidType", "BidType");
                    BKOptionPrice.ColumnMappings.Add("HedgeType", "HedgeType");
                    BKOptionPrice.ColumnMappings.Add("StartDate", "StartDate");
                    BKOptionPrice.ColumnMappings.Add("EndDate", "EndDate");
                    BKOptionPrice.WriteToServer(mOptionData);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            DBConnection.Close();
        }
        protected virtual void FillAllPeriodHash(DateTime date)
        {
            if (mSelectAnnualPeriodCmd.Connection.State == ConnectionState.Closed)
                mSelectAnnualPeriodCmd.Connection.Open();
            annualPeriodHash = new Hashtable();
            SqlDataReader reader = mSelectAnnualPeriodCmd.ExecuteReader();
            while (reader.Read())
                annualPeriodHash[reader[1].ToString().ToUpper().Trim() + "_" + reader[2].ToString()] = (int)reader.GetValue(0);
            reader.Close();
            mSelectAnnualPeriodCmd.Connection.Close();
        }
        void Dispose()
        {

        }
    }
}
