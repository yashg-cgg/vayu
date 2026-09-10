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
            mSelectCRRAuction.CommandText = "select CRRAuctionKey, CRRAuctionName, AuctionRound  from CRRAuction where CRRAuctionKey = 215";
           mSelectCRRAuction.Parameters.AddWithValue("@crrauctiontype", "");
            mSelectCRRAuction.Parameters.AddWithValue("@startDate", "");
            mSelectCRRAuction.Parameters.AddWithValue("@endDate", "");
            mSelectCRRAuction.Parameters.AddWithValue("@auctionRound", "");
            mSelectCRRAuction.Connection = DBConnection;
           
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
            for (int i = 0; i < splinelitRow.Length; i+=3)
            {
                string auction = splinelitRow[i].Substring(splinelitRow[i].IndexOf("rpt") + 3, splinelitRow[i].IndexOf("zip"));
                if (auction.Contains("2026"))
                {
                    //FileURL = "https://mis.ercot.com/misdownload/servlets/mirDownload?mimic_duns=0809369482000&doclookupId=712349359";
                    FileURL = "https://mis.ercot.com/misdownload/servlets/mirDownload?" + splinelitRow[i+3].Substring(splinelitRow[i+3].IndexOf("mimic_duns"), splinelitRow[i+3].IndexOf("zip") - 45);
                    CRRAuctionkey = 215;
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

                    foreach (string fileItem in annualFiles)
                    {
                        if (fileItem.Contains(("Common_MarketResults_")) && (fileItem.Contains("xml")))
                            finalfileName = fileItem;
                        if (fileItem.Contains("Common_SourceAndSinkShadowPrices_") && fileItem.Contains("xml"))
                            nodalFile = fileItem.ToString();
                        if (fileItem.Contains("Common_AuctionBidsAndOffers_") && fileItem.Contains("xml"))
                            crrFile = fileItem.ToString();
                    }
                    if (updateAuctionResults)
                        UpdateAnnualAuctionResult(finalfileName, CRRAuctionkey);
                    //if (updateNodalPrices)
                    //    UpdateAnnualNodalPrices(nodalFile, CRRAuctionkey, processDate);
                    //if (updateOptionPrices)
                    //    UpdateAnnualOptionPrices(crrFile, CRRAuctionkey, processDate);
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
                    bkAAuction.DestinationTableName = "[Vayu]..[CRRAnnualAuctionResult]";
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
                    if(sDate.Year==2025)
                    {

                    }
                    sDate = sDate.AddMonths(1);
                    eDate = sDate.AddMonths(1).AddDays(-1);
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
                drAuctionResult["StartDate"] = Convert.ToDateTime(item.StartDate);
                drAuctionResult["EndDate"] = Convert.ToDateTime(item.EndDate);
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




    }
}
