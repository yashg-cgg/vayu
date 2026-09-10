using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using System.Xml.Serialization;
using Xml2CSharp;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotCRRMonthlyAuctionResultsDownload
{
    public class ErcotAnnualAuctionResultsDownload : ErcotMonthlyAuctionResultsDownload
    {
        SqlConnection DBConnection;
        SqlCommand mSelectCRRAuction;
        SqlCommand mSelectAllAuctionKeys;
        public virtual void DownloadAuunalUpdate(DateTime? processDate = null)
        {
            if (!processDate.HasValue)
                processDate = DateTime.Now.AddMonths(1);
            FillAllPeriodHash(processDate.Value);
            for (int i = 1; i < 2; i++)
            {
                int CRRAuctionKey = GetCRRAuctionKey(processDate.Value, i);
                Download(processDate.Value, CRRAuctionKey, i);
            }
        }
        private int GetCRRAuctionKey(DateTime date, int roundCount)
        {
            int CRRAuctionKey = -1;
            InitADB();
            try
            {
                if (mSelectCRRAuction.Connection.State == System.Data.ConnectionState.Closed)
                    mSelectCRRAuction.Connection.Open();

                mSelectCRRAuction.Parameters["@crrauctiontype"].Value = "Annual"; //"Annual";
                mSelectCRRAuction.Parameters["@startDate"].Value = date;
                mSelectCRRAuction.Parameters["@endDate"].Value = date.AddMonths(1);
                mSelectCRRAuction.Parameters["@auctionRound"].Value = roundCount;

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
        
        public ErcotAnnualAuctionResultsDownload()
        {
            InitADB();
        }
        private void InitADB()
        {
            DBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            mSelectCRRAuction = new SqlCommand();
            //mSelectCRRAuction.CommandText = "select CRRAuctionKey,CRRAuctionName,AuctionRound from  CRRAuction where CRRAuctiontype=@crrauctiontype and ResultsPostedDate>@startDate and ResultsPostedDate<@endDate and AuctionRound=@auctionRound";
           // mSelectCRRAuction.CommandText = "select CRRAuctionKey,CRRAuctionName  from CRRAuction where CRRAuctionType!='Monthly' and CRRAuctionName like '%2024%'";
            mSelectCRRAuction.CommandText = "select CRRAuctionKey, CRRAuctionName, AuctionRound  from CRRAuction where CRRAuctionKey = 96";
           mSelectCRRAuction.Parameters.AddWithValue("@crrauctiontype", "");
            mSelectCRRAuction.Parameters.AddWithValue("@startDate", "");
            mSelectCRRAuction.Parameters.AddWithValue("@endDate", "");
            mSelectCRRAuction.Parameters.AddWithValue("@auctionRound", "");
            mSelectCRRAuction.Connection = DBConnection;


            mSelectAllAuctionKeys = new SqlCommand();
            mSelectAllAuctionKeys.CommandText = "select CRRAuctionName,CRRAuctionKey from CRRAuction where CRRAuctionStartYear between 2022 and  2025";
            mSelectAllAuctionKeys.Connection = DBConnection;





        }
        private void Download(DateTime processDate, int CRRAuctionkey, int roundCount, bool updateAuctionResults = true, bool updateNodalPrices = true, bool updateOptionPrices=true)
        {

            Console.WriteLine("Deleteing Old Files.");
            string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\ERCOTAnnualAuction\");
            foreach (string filePath in filePaths)
                File.Delete(filePath);
            Console.WriteLine("Getting Files..");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=11203");
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
            string strAnnual = sr.ReadToEnd();
            sr.Close();
            string FileURL = string.Empty;
            strAnnual = strAnnual.Replace("\r\n", "").Trim();
            strAnnual = strAnnual.Substring(strAnnual.IndexOf("<b>Annual Auction Results</b></"));
            string[] splinelitRow = strAnnual.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 3; i < splinelitRow.Length; i+=3)
            {
                string auction= splinelitRow[i].Substring(splinelitRow[i].IndexOf("rpt")+ 3, splinelitRow[i].IndexOf("zip"));
                if (auction.Contains("20241st"))
                {
                    if (auction.Contains("20241st6AnnualAuctionSeq1"))
                        CRRAuctionkey = 155;


                    else if (auction.Contains("20241st6AnnualAuctionSeq2"))
                        CRRAuctionkey = 150;


                    else if (auction.Contains("20241st6AnnualAuctionSeq3"))
                        CRRAuctionkey = 145;

                    else if (auction.Contains("20241st6AnnualAuctionSeq4"))
                        CRRAuctionkey = 140;

                    else if (auction.Contains("20241st6AnnualAuctionSeq5"))
                        CRRAuctionkey = 135;

                    else if (auction.Contains("20241st6AnnualAuctionSeq6"))
                        CRRAuctionkey = 96;

                    else
                      {
                        break;
                    
                      }

                   

                    //FileURL = "https://mis.ercot.com/misdownload/servlets/mirDownload?mimic_duns=0809369482000&doclookupId=712349359";
                    FileURL = "https://mis.ercot.com/misdownload/servlets/mirDownload?" + splinelitRow[i+3].Substring(splinelitRow[i+3].IndexOf("mimic_duns"), splinelitRow[i+3].IndexOf("zip") - 45);
                    //CRRAuctionkey = 161;
                    string FileName = "D:\\ISOFiles\\ErcotCRRMonthlyAuctionResultsDownload\\ERCOTAnnualAuction\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                    FileDownloadRequest.ClientCertificates.Add(mCert);
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                    HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string fileDestName = FileName;
                    string folderFilename = @"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\ERCOTAnnualAuction\";
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
                    File.Delete(fileDestName);
                    string finalfileName = string.Empty; string nodalFile = string.Empty; string crrFile = string.Empty;
                    //string[] annualFiles = Directory.GetFiles(@"D:\ERCOTAnnualAuction\");
                    string[] annualFiles = Directory.GetFiles(@"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\ERCOTAnnualAuction\");

                    Dictionary<string, decimal> keydict = new Dictionary<string, decimal>();
                   


                   // keydict = GetCRRKey();
                    //AuctionBidsAndOffers_2024.1st6.AnnualAuction.Seq1_AUCTION.csv
                    decimal crrkey;
                    foreach (string fileItem in annualFiles)
                    {

                        //if (fileItem.Contains(".xml"))
                        //{
                        //    int length1 = fileItem.IndexOf("\\Common_")+8;
                        //    int length2 = fileItem.IndexOf("_AUCTION.xml");
                        //    string keyname = fileItem.Substring(length1, length2 - length1);
                        //}

                        foreach (var kvp in keydict)
                        {
                            if (fileItem.Contains(kvp.Key))
                            {
                                crrkey = kvp.Value; // Get the associated value
                                break; // Exit the loop after finding the first match
                            }
                        }


                        if (fileItem.Contains(("Common_MarketResults_")) && (fileItem.Contains("xml")))
                            finalfileName = fileItem;
                        if (fileItem.Contains("Common_SourceAndSinkShadowPrices_") && fileItem.Contains("xml"))
                            nodalFile = fileItem.ToString();
                        if (fileItem.Contains("Common_AuctionBidsAndOffers_") && fileItem.Contains("xml"))
                            crrFile = fileItem.ToString();
                    }
                    //  if (updateAuctionResults)
                    //  UpdateAnnualAuctionResult(finalfileName, CRRAuctionkey);

                    // if (updateNodalPrices)
                    //    UpdateAnnualNodalPrices(nodalFile, CRRAuctionkey, processDate);

                    if (updateOptionPrices)
                    {  //UpdateAnnualOptionPrices(crrFile, CRRAuctionkey, processDate);
                       UpdateOptionPricesAnnualy(crrFile, CRRAuctionkey, processDate, "OPT");
                        //UpdateOptionPricesAnnualy(crrFile, CRRAuctionkey, processDate, "OBL");
                    }

                    Console.WriteLine("Deleteing Files..");
                    filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotCRRMonthlyAuctionResultsDownload\ERCOTAnnualAuction");
                    foreach (string filePath in filePaths)
                        File.Delete(filePath);
                }
            }
        }
        public void InsertAnnualResult(DataTable annualDT)
        {
            if (DBConnection.State == ConnectionState.Open)
                DBConnection.Close();
            DBConnection.Open();
            SqlTransaction transaction = DBConnection.BeginTransaction();
            using (SqlBulkCopy bkAAuction = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    //bkAAuction.DestinationTableName = "[Vayu]..[CRRAnnualAuction]";
                    bkAAuction.DestinationTableName = "[Vayu]..[CRRAuctionResultstest]";
                    bkAAuction.ColumnMappings.Add("CRRAuctionkey", "CRRAuctionkey");
                    bkAAuction.ColumnMappings.Add("AuctionName", "AuctionName");
                    bkAAuction.ColumnMappings.Add("AuctionRound", "AuctionRound");
                    bkAAuction.ColumnMappings.Add("CRR_ID", "CRR_ID");
                    bkAAuction.ColumnMappings.Add("AccountHolder", "AccountHolder");
                    bkAAuction.ColumnMappings.Add("Hedge", "Hedge");
                    bkAAuction.ColumnMappings.Add("Bid", "Bid");
                    bkAAuction.ColumnMappings.Add("CRR", "CRR");
                    bkAAuction.ColumnMappings.Add("Sourcekey", "Sourcekey");
                    bkAAuction.ColumnMappings.Add("Sinkkey", "Sinkkey");
                    bkAAuction.ColumnMappings.Add("SourceName", "SourceName");
                    bkAAuction.ColumnMappings.Add("SinkName", "SinkName");
                    bkAAuction.ColumnMappings.Add("StartDate", "StartDate");
                    bkAAuction.ColumnMappings.Add("EndDate", "EndDate");
                    bkAAuction.ColumnMappings.Add("TimeUse", "TimeUse");
                    bkAAuction.ColumnMappings.Add("Bid24Hour", "Bid24Hour");
                    bkAAuction.ColumnMappings.Add("MW", "MW");
                    bkAAuction.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                    bkAAuction.ColumnMappings.Add("PeriodKey", "PeriodKey");
                    bkAAuction.WriteToServer(annualDT);
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                }
            }
            DBConnection.Close();
        }      
        private void UpdateAnnualAuctionResult(string fileName, int CRRAuctionkey)
        {
            int CRRKey = GetCRRAuctionKey(DateTime.Today, 1);
            mERCOTAuctionResultsDT.Clear();
            #region XML Data Parsing
            XmlSerializer serializer = new XmlSerializer(typeof(MarketResults));
            MarketResults mResult;
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                mResult = (MarketResults)serializer.Deserialize(reader);
            }
            int resultCount = mResult.Result.Count();
            foreach (var item in mResult.Result)
            {
                DateTime sDate= Convert.ToDateTime(item.StartDate);
                DateTime eDate= Convert.ToDateTime(item.EndDate);
                int diff = (eDate.Month - sDate.Month)+1;
                //drAuctionResult = mERCOTAuctionResultsDT.NewRow();
                int sourceNodekey = 0, sinkNodekey = 0;
                long crrid = Convert.ToInt64(item.CRR_ID);
                if (crrid == 0)
                    crrid = Convert.ToInt64(item.ORI_CRR_ID);
                if (NodeDictHash.ContainsKey(item.Source))
                    sourceNodekey = NodeDictHash[item.Source];
                if (NodeDictHash.ContainsKey(item.Sink))
                    sinkNodekey = NodeDictHash[item.Sink];
                eDate = sDate.AddMonths(1).AddDays(-1);
                for (int i = 1; i <= diff; i++)
                {
                    drAuctionResult = mERCOTAuctionResultsDT.NewRow();
                    drAuctionResult["CRRAuctionkey"] = CRRKey;
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
                   // drAuctionResult["StartDate"] = Convert.ToDateTime(item.StartDate);
                    drAuctionResult["StartDate"] = sDate;
                    //drAuctionResult["EndDate"] = Convert.ToDateTime(item.EndDate);
                    drAuctionResult["EndDate"] = eDate;
                    string month = sDate.ToString("MMM").ToUpper().Trim() + "_" + sDate.Year;
                    drAuctionResult["TimeUse"] = item.TimeOfUse;
                    drAuctionResult["Bid24Hour"] = item.Bid24Hour;
                    drAuctionResult["MW"] = Convert.ToDouble(item.MW);
                    drAuctionResult["ShadowPrice"] = Convert.ToDecimal(item.ShadowPrice);
                    drAuctionResult["PeriodKey"] = annualPeriodHash[month];
                    mERCOTAuctionResultsDT.Rows.Add(drAuctionResult);
                    sDate = sDate.AddMonths(i);
                    eDate = sDate.AddMonths(i).AddDays(-1);
                }
            }
            #endregion XML Data Parsing
            int totalcount = mERCOTAuctionResultsDT.Rows.Count;
            if (mERCOTAuctionResultsDT.Rows.Count > 0)
            {
                Console.WriteLine("Insert into DB result..");
                InsertAnnualResult(mERCOTAuctionResultsDT);
            }
        }
        public void UpdateAnnualNodalPrices(string fileName, int CRRAuctionkey, DateTime date)
        {
            //FillPeriodHash(date);
            FillAllPeriodHash(date);
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
                //List<ShadowPriceElement> spElement = shadowpriceList.Where(x => x.SourceSink == item.ToString()).ToList();
                var shadowPriceList = from spitem in shadowpriceList where spitem.SourceSink == item select spitem;
                var mPeriodList = from pItem in shadowPriceList orderby pItem.CalendarPeriod select pItem.CalendarPeriod;
                foreach (var mPeriod in mPeriodList.Distinct())
                {
                    drCRRNodal = mCRRNodalDT.NewRow();
                    drCRRNodal["CRRAuctionKey"] = CRRAuctionkey;
                    drCRRNodal["NodeKey"] = NodeDictHash[item.ToString()];
                    var spElement=from spItem in shadowPriceList where spItem.CalendarPeriod==mPeriod select spItem;
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
                        drCRRNodal["PeriodKey"] = annualPeriodHash[pName];//periodHash[pArr[0]];
                    }
                    mCRRNodalDT.Rows.Add(drCRRNodal); 
                }
            }
            int nodalCount = mCRRNodalDT.Rows.Count;
            if (mCRRNodalDT.Rows.Count > 1)
            {
                Console.WriteLine("Nodal Prices Insert into DB..");
                NodalPriceInsert(mCRRNodalDT);
               
            }
        }


        public void UpdateAnnualOptionPrices(string fileName, int CRRAuctionkey, DateTime date)
        {
            mOptionPriceDT.Clear();
            FillPeriodHash(date);
            string month = date.ToString("MMM").ToUpper();
            XmlSerializer serializer = new XmlSerializer(typeof(AuctionCRRs));
            AuctionCRRs mResult;
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                mResult = (AuctionCRRs)serializer.Deserialize(reader);
            }
            int count1 = mResult.Crr.Count();
            #region XML Data
            foreach (var item in mResult.Crr)
            {
                drAuctionResult = mOptionPriceDT.NewRow();
                int sourceNodekey = 0, sinkNodekey = 0;               
                if (NodeDictHash.ContainsKey(item.Source))
                    sourceNodekey = NodeDictHash[item.Source];
                if (NodeDictHash.ContainsKey(item.Sink))
                    sinkNodekey = NodeDictHash[item.Sink];
                drAuctionResult["CRRAuctionkey"] = CRRAuctionkey;             
                drAuctionResult["bidType"] = item.BidType;
               /// drAuctionResult["StartDate"] = Convert.ToDateTime(item.StartDate);
                //drAuctionResult["EndDate"] = Convert.ToDateTime(item.EndDate);
                drAuctionResult["hedgeType"] = item.HedgeType;

                if (item.Tou == "PeakWD")
                {
                    drAuctionResult["PeakWD"] = Convert.ToDouble(item.ShadowPrice);                   
                }
                if (item.Tou == "Off-peak")
                {
                    drAuctionResult["OffPeak"] = Convert.ToDouble(item.ShadowPrice);                   
                }
                if (item.Tou == "PeakWE")
                {
                    drAuctionResult["PeakWE"] = Convert.ToDouble(item.ShadowPrice);                    
                }

                drAuctionResult["Sourcekey"] = sourceNodekey;
                drAuctionResult["Sinkkey"] = sinkNodekey;
               
                mOptionPriceDT.Rows.Add(drAuctionResult);
            }
            #endregion XML Data
            int c = mOptionPriceDT.Rows.Count;
            if (mOptionPriceDT.Rows.Count > 0)
            {
                Console.WriteLine("Insert Option Price Insert into DB..");
                InsertOptionPrice(mOptionPriceDT);
            }
        }

        public void UpdateOptionPricesAnnualy(string fileName, int CRRAuctionkey, DateTime date, string HedgeType)
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
            optionPriceList = optionParseList.Where(a => a.HedgeType == HedgeType).OrderBy(a => a.Source).ThenBy(a => a.Sink).ToList();
            var nodeName = from item in optionPriceList where item.HedgeType == HedgeType orderby item.Source, item.Sink select new { item.Source, item.Sink };

            foreach (Crr item in optionParseList)
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
            List<string> sourcesingprice = new List<string>();
            foreach (var item in nodeName.Distinct())
            //Parallel.ForEach(nodeName, new ParallelOptions { MaxDegreeOfParallelism = 5}, item =>
            {
                DataRow droptionPrice;
                var npElement = optionPriceDic[item.Source + item.Sink];

                if (item.Source == "TORR_ALL" && item.Sink == "LARDVFTN_G5")
                {

                }
                else

                    continue;
                var bidpricepeakwd = npElement.Where(x => x.HedgeType == HedgeType && x.BidType == "BUY" && x.Tou == "PeakWD").Select(x => x.BidPrice);
                var bidpriceoffpeak = npElement.Where(x => x.HedgeType == HedgeType && x.BidType == "BUY" && x.Tou == "Off-peak").Select(x => x.BidPrice);
                var bidpricepeakwe = npElement.Where(x => x.HedgeType == HedgeType && x.BidType == "BUY" && x.Tou == "PeakWE").Select(x => x.BidPrice);
                var mwpeakwd = npElement.Where(x => x.HedgeType == HedgeType && x.BidType == "BUY" && x.Tou == "PeakWD").Select(x => x.MW);
                var mwoffpeak = npElement.Where(x => x.HedgeType == HedgeType && x.BidType == "BUY" && x.Tou == "Off-peak").Select(x => x.MW);
                var mwpeakwe = npElement.Where(x => x.HedgeType == HedgeType && x.BidType == "BUY" && x.Tou == "PeakWE").Select(x => x.MW);

                int maxcount = Math.Max(Math.Max(bidpricepeakwd.Count(), bidpriceoffpeak.Count()), bidpricepeakwe.Count());

                for (int j = 0; j < maxcount; j++)
                {
                    droptionPrice = mOptionPriceDT.NewRow();
                    droptionPrice["CRRAuctionKey"] = CRRAuctionkey;
                    droptionPrice["Sourcekey"] = NodeDictHash[item.Source.ToString()];
                    droptionPrice["Sinkkey"] = NodeDictHash[item.Sink.ToString()];
                    droptionPrice["PeriodKey"] = periodHash[month];
                    double sumpeakwd = 0;
                    double sumoffpeak = 0;
                    double sumpeakwe = 0;
                    for (int i = 0; i < npElement.ToList().Count(); i++)
                    {
                        string sp = npElement.ToList()[i].ShadowPrice;
                        string mwsp = npElement.ToList()[i].MW;
                        string bppm = npElement.ToList()[i].BidPrice;
                        if (npElement.ToList()[i].HedgeType == HedgeType && npElement.ToList()[i].BidType == "BUY")
                        {
                            if (npElement.ToList()[i].Tou == "PeakWD")
                            {
                                droptionPrice["PeakWD"] = Convert.ToDouble(npElement.ToList()[i].ShadowPrice);
                                sumpeakwd = sumpeakwd + Convert.ToDouble(npElement.ToList()[i].MW);
                                droptionPrice["PeakWDMW"] = sumpeakwd;
                            }
                            else if (npElement.ToList()[i].Tou == "Off-peak")
                            {
                                droptionPrice["OffPeak"] = Convert.ToDouble(npElement.ToList()[i].ShadowPrice);
                                sumoffpeak = sumoffpeak + Convert.ToDouble(npElement.ToList()[i].MW);
                                droptionPrice["OffPeakMW"] = sumoffpeak;
                            }
                            else if (npElement.ToList()[i].Tou == "PeakWE")
                            {
                                droptionPrice["PeakWE"] = Convert.ToDouble(npElement.ToList()[i].ShadowPrice.ToString());
                                sumpeakwe = sumpeakwe + Convert.ToDouble(npElement.ToList()[i].MW);
                                droptionPrice["PeakWEMW"] = sumpeakwe;
                            }
                            droptionPrice["BidType"] = npElement.ToList()[i].BidType.ToString();
                            droptionPrice["HedgeType"] = npElement.ToList()[i].HedgeType.ToString();
                        }
                    }
                    if (bidpricepeakwd.Count() > j)
                    {
                        droptionPrice["BidPricePeakWD"] = Convert.ToDouble(bidpricepeakwd.ToList()[j]);
                        droptionPrice["MWWD"] = Convert.ToDouble(mwpeakwd.ToList()[j]);
                    }
                    if (bidpriceoffpeak.Count() > j)
                    {
                        droptionPrice["BidPriceOffpeak"] = Convert.ToDouble(bidpriceoffpeak.ToList()[j]);
                        droptionPrice["MWOffP"] = Convert.ToDouble(mwoffpeak.ToList()[j]);
                    }
                    if (bidpricepeakwe.Count() > j)
                    {
                        droptionPrice["BidPricePeakWE"] = Convert.ToDouble(bidpricepeakwe.ToList()[j]);
                        droptionPrice["MWWE"] = Convert.ToDouble(mwpeakwe.ToList()[j]);
                    }
                    mOptionPriceDT.Rows.Add(droptionPrice);
                }
                //mOptionPriceDT.Rows.Add(droptionPrice);
            }
            //int optcount = mOptionPriceDT.Rows.Count;



            //if (mOptionPriceDT.Rows.Count > 0)
            //    InsertOptionPrice(mOptionPriceDT);
        }
        public Dictionary<string,decimal>GetCRRKey()
        {
            Dictionary<string, decimal> keyDictionary = new Dictionary<string, decimal>();

            mSelectAllAuctionKeys.Connection = DBConnection;
            DBConnection.Open();
            SqlDataReader dr = mSelectAllAuctionKeys.ExecuteReader();
            if(dr.HasRows)
            {
                while (dr.Read())
                { string key = dr.GetString(0);
                    decimal value = dr.GetDecimal(1);
                    //if(keyDictionary.ContainsValue)
                    keyDictionary.Add(key,value ); 
                
                }
              }
            return keyDictionary;


        }




    }
}
