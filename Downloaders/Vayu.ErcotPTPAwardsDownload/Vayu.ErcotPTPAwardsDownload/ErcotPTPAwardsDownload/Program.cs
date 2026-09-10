

using Vayu.CertificateInfoLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;
using Vayu.MarketLibraryErcot;
using Vayu.MarketLibraryErcot.ERCOTNodalService;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotPTPAwardsDownload
{
    internal class Program
    {
        //private static void Main(string[] args) => new Program.DownloadBids().GetRequestFile(DateTime.Today);
        static void Main(string[] args)
        {
            DownloadBids bids = new DownloadBids();
            bids.GetRequestFile1(DateTime.Today.AddDays(1));
        }

        public class DownloadBids
        {
            private SqlConnection vConnection;
            private SqlCommand vInsertBidsommand;
            private SqlCommand vDeleteXmlBidsInfoCommand;
            private SqlCommand vSelectPTPBids;
            private SqlCommand vInsertClearedBidsommand;
            private DataTable dtXmlBidsInfo = new DataTable();
            private DataRow drXmlBidsInfo = (DataRow)null;
            private DataTable dtClearedBids = new DataTable();
            private DataRow drClearedBids = (DataRow)null;
            private DataTable dtMainSplitBids = new DataTable();
            private DataRow drMainSplitBids = (DataRow)null;
            private DateTime vMarketDate = DateTime.Today.AddDays(1);
            private CertificateHeler vUserCertificateDetails = (CertificateHeler)null;
            private CertificateHeler vServerCertificateDetails = (CertificateHeler)null;
            private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
            {
                return true;
            }

            public void GetRequestFile1(DateTime today)
            {
                DateTime date = Convert.ToDateTime(System.Configuration.ConfigurationSettings.AppSettings["MarketDate"]);
                if (date != DateTime.MinValue)
                {
                    vMarketDate = date;
                }
                Init();
                Console.WriteLine("Market Date: " + vMarketDate);
                bool exists = ChkFOrExixtingBIds(vMarketDate);
                //if (exists)
                //{
                //    Console.WriteLine("Bids Already Present for " + marketDate + "Please delete Manually and Insert");
                //    return;
                //}
                vUserCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI"); //ProdClientAPI  ErcotSubmission
                vServerCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdServerCert");//ProdServerCert  ErcotDownloadersServer
                TimeZone curTimeZone = TimeZone.CurrentTimeZone;
                List<DateTime> marketDateTimeList = new List<DateTime>();
                try
                {
                    {
                        {
                            AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
                            CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
                            ErcotNodalClient ercotclient = new ErcotNodalClient();
#if test
                            OperationsClient operationsClient = ercotclient.CreateErcotOperationsClientTest(myClientRequestBinding, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path);
#else
                            OperationsClient operationsClient = ercotclient.CreateErcotOperationsClient(myClientRequestBinding, vUserCertificateDetails.Path, vUserCertificateDetails.Password, vServerCertificateDetails.Path);
#endif
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            RequestMessage vRequestMessage = new RequestMessage();
                            ResponseMessage vRespMessage = new ResponseMessage();
                            PayloadType vPayloadType = new PayloadType();
                            ReplayDetectionType vReplayDetection = new ReplayDetectionType();
                            RequestType vRequest = new RequestType();
                            HeaderType vHeader = new HeaderType();
                            vHeader.Source = "QENJRE";
#if test
                            myHeader.UserID = "API_04122019PIYUSHD";
#else
                            //myHeader.UserID = "API_20180405piyushd";
                            vHeader.UserID = vUserCertificateDetails.UserName;
#endif
                            vHeader.Verb = HeaderTypeVerb.get;
                            vHeader.Noun = "AwardedPTPObligation";
                            vReplayDetection.Nonce = new EncodedString();
                            vReplayDetection.Nonce.Value = Convert.ToString(Guid.NewGuid());
                            vReplayDetection.Created = new AttributedDateTime();
                            vReplayDetection.Created.Value = Convert.ToString(string.Format("{0:s}", DateTime.Now));
                            vHeader.ReplayDetection = vReplayDetection;
                            vHeader.Revision = "1.0";
                            vRequestMessage.Header = vHeader;
                            vRequest.TradingDateSpecified = true;
                            vPayloadType.format = "XML";
                            vRequestMessage.Payload = vPayloadType;
                            vRequestMessage.Request = vRequest;
                            bool isDST = curTimeZone.IsDaylightSavingTime(DateTime.Now);
                            //DateTime tempDate = DateTime.Today.AddDays(0);
                            vRequest.TradingDate = vMarketDate;
                            vRequest.StartTime = vMarketDate; //DateTime.Today.AddDays(-1);
                            vRequest.EndTime = vMarketDate.AddDays(0); // DateTime.Today;
                            XmlSerializer ser = new XmlSerializer(typeof(RequestMessage));
                            TextWriter writer = new StreamWriter("Req.xml");
                            ser.Serialize(writer, vRequestMessage);
                            try
                            {
                                vRespMessage = operationsClient.MarketInfo(vRequestMessage);
                                string respString = vRespMessage.ToString();
                                if (vRespMessage.Payload.ItemsElementName[0].ToString().ToLower() == "compressed")
                                {
                                    string encoadedString = vRespMessage.Payload.Items[0].ToString();
                                    byte[] base64String = Convert.FromBase64String((string)vRespMessage.Payload.Items[0]);
                                    MemoryStream memStream = new MemoryStream(base64String);
                                    GZipStream zipStream = new GZipStream(memStream, CompressionMode.Decompress);
                                    byte[] outputStream = new byte[10000000];
                                    zipStream.Read(outputStream, 0, outputStream.Length);
                                    memStream.Close();
                                    string xmlResponse = System.Text.ASCIIEncoding.ASCII.GetString(outputStream);
                                    File.WriteAllText("Resp.xml", xmlResponse);
                                    File.WriteAllText("RespTest.owl", xmlResponse);
                                    XmlDataDocument vXmlParser = new XmlDataDocument();
                                    vXmlParser.LoadXml(xmlResponse);
                                    List<BidsInfo> bidList = new List<BidsInfo>();
                                    if (vXmlParser.DocumentElement.LocalName == "AwardSet")
                                    {
                                        string str2 = "<Root>" + vXmlParser.DocumentElement.InnerXml + "</Root>";
                                        System.IO.File.WriteAllText("InnerResp.xml", str2);
                                        XmlDocument vXmlDocument = new XmlDocument();
                                        vXmlDocument.LoadXml(str2);
                                        DateTime vDateTime = DateTime.MinValue;
                                        foreach (XmlNode childNode1 in vXmlDocument.ChildNodes)
                                        {
                                            if (childNode1.LocalName == "Root")
                                            {
                                                foreach (XmlNode childNode2 in childNode1.ChildNodes)
                                                {
                                                    if (childNode2.LocalName.ToLower() == "tradingdate")
                                                        vDateTime = Convert.ToDateTime(childNode2.InnerText);
                                                    if (childNode2.LocalName == "AwardedPTPObligation")
                                                    {
                                                        Program.BidsInfo bidsInfo2 = new Program.BidsInfo();
                                                        foreach (XmlNode childNode3 in childNode2.ChildNodes)
                                                        {
                                                            bidsInfo2.TradingDate = vDateTime;
                                                            if (childNode3.LocalName.ToLower() == "qse")
                                                            {
                                                                string str3 = childNode3.InnerText.ToString();
                                                                bidsInfo2.User = str3;
                                                            }
                                                            if (childNode3.LocalName.ToLower() == "starttime")
                                                            {
                                                                DateTime dateTime3 = Convert.ToDateTime(childNode3.InnerText);
                                                                bidsInfo2.StartTime = dateTime3;
                                                            }
                                                            if (childNode3.LocalName.ToLower() == "endtime")
                                                            {
                                                                DateTime dateTime3 = Convert.ToDateTime(childNode3.InnerText);
                                                                bidsInfo2.EndTime = dateTime3;
                                                            }
                                                            if (childNode3.LocalName.ToLower() == "awardedmw")
                                                            {
                                                                double num = Convert.ToDouble(childNode3.InnerText);
                                                                bidsInfo2.AwardedMw = num;
                                                            }
                                                            if (childNode3.LocalName.ToLower() == "source")
                                                            {
                                                                string str3 = childNode3.InnerText.ToString();
                                                                bidsInfo2.Source = str3;
                                                            }
                                                            if (childNode3.LocalName.ToLower() == "sink")
                                                            {
                                                                string str3 = childNode3.InnerText.ToString();
                                                                bidsInfo2.Sink = str3;
                                                            }
                                                            if (childNode3.LocalName.ToLower() == "price")
                                                            {
                                                                double num = Convert.ToDouble(childNode3.InnerText);
                                                                bidsInfo2.Price = num;
                                                            }
                                                            if (childNode3.LocalName.ToLower() == "bidid")
                                                            {
                                                                string str3 = childNode3.InnerText.ToString();
                                                                bidsInfo2.BidID = str3;
                                                            }
                                                        }
                                                        bidList.Add(bidsInfo2);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (bidList.Count > 0)
                                    {
                                        Console.WriteLine("BidList Count: " + (object)bidList.Count + "for Date: " + (object)bidList[0].TradingDate);
                                        TruncateTables();
                                        SaveBidInfo(bidList);
                                        GetBids(bidList);
                                        Split();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                   
                }
            }
        
            private bool ChkFOrExixtingBIds(DateTime marketDate)
            {
                bool flag = false;
                using (SqlConnection sqlConnection = new SqlConnection(vConnection.ConnectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.Connection = sqlConnection;
                        command.CommandText = "select COUNT(*) from clearedees_temp where MarketDateTime > '" + marketDate.ToString("yyyy-MM-dd") + "' and MarketDateTime <= '" + marketDate.AddDays(1.0).ToString("yyyy-MM-dd") + "'";
                        if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                            flag = true;
                    }
                    sqlConnection.Close();
                }
                return flag;
            }

            private void SplitBidsNew()
            {
                List<int> portfolioKeys = GetPortfolioKeys();
                using (SqlConnection sqlConnection = new SqlConnection(vConnection.ConnectionString))
                {
                    foreach (int num in portfolioKeys)
                    {
                        sqlConnection.Open();
                        using (SqlCommand command = sqlConnection.CreateCommand())
                        {
                            command.Connection = sqlConnection;
                            command.CommandTimeout = 60000;
                            command.CommandText = " insert into clearedeesActual_temp  select a.SourceNodeKey , a.SinkNodeKey , b.RequestedMW , a.MarketDateTime , null , null , b.PortfolioKey , a.BidID  from clearedees_2011_temp a join ErcotPTPBids b on a.SourceNodeKey = b.SourceNodeKey and a.SinkNodeKey = b.SinkNodeKey and  a.MarketDateTime = b.EndMarketDateTime    where  b.EndMarketDateTime > '" + vMarketDate.ToString("yyyy-MM-dd") + "' and b.EndMarketDateTime <= '" + vMarketDate.AddDays(1.0).ToString("yyyy-MM-dd") + "' and BidStatus = 'Valid'  and a.bidid like '" + (object)num + "%' and b.PortfolioKey = " + (object)num;
                            command.ExecuteNonQuery();
                        }
                        sqlConnection.Close();
                    }
                }
                using (SqlConnection sqlConnection = new SqlConnection(vConnection.ConnectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.Connection = sqlConnection;
                        command.CommandTimeout = 60000;
                        command.CommandText = " insert into clearedees_temp  select a.SourceNodeKey , a.SinkNodeKey , (b.ClearedMW - a.ClearedMW) , a.MarketDateTime , null , null , b.PortfolioKey , a.BidID   from clearedeesActual_temp a join clearedees_2011_temp b   on a.SourceNodeKey = b.SourceNodeKey and a.SinkNodeKey = b.SinkNodeKey and a.MarketDateTime = b.MarketDateTime and a.BidID = b.BidID   group by  a.SourceNodeKey , a.SinkNodeKey , (b.ClearedMW - a.ClearedMW) , a.MarketDateTime  , b.PortfolioKey , a.BidID having (b.ClearedMW - a.ClearedMW) > 0 ";
                        command.ExecuteNonQuery();
                    }
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.Connection = sqlConnection;
                        command.CommandTimeout = 60000;
                        command.CommandText = " insert into clearedees_temp  select SourceNodeKey , SinkNodeKey , ClearedMW , MarketDateTime , OasisID , ScheduleID , PortfolioKey , BidID from clearedeesActual_temp ";
                        command.ExecuteNonQuery();
                    }
                    sqlConnection.Close();
                }
            }

            private void Split()
            {
                Console.WriteLine("Split Bids Started...");
                List<Program.DownloadBids.SubmittedBids> source1 = new List<Program.DownloadBids.SubmittedBids>();
                SqlCommand sqlCommand1 = new SqlCommand();
                sqlCommand1.CommandText = "select SourceNodeKey,SinkNodeKey,RequestedMW,endMarketDateTime, PortfolioKey,ScheduleID from ErcotPTPBids where   endMarketDateTime> '" + vMarketDate.ToString("yyyy-MM-dd") + "'   and endMarketDateTime<= '" + vMarketDate.AddDays(1.0).ToString("yyyy-MM-dd") + "'  and PortfolioKey not in (3012,3022) and BidStatus ='Valid' order by endMarketDateTime";
                sqlCommand1.Connection = vConnection;
                if (vConnection.State == ConnectionState.Closed)
                    vConnection.Open();
                SqlDataReader sqlDataReader1 = sqlCommand1.ExecuteReader();
                while (sqlDataReader1.Read())
                    source1.Add(new Program.DownloadBids.SubmittedBids()
                    {
                        SourceNodeKey = Convert.ToInt32(sqlDataReader1.GetValue(0)),
                        SinkNodeKey = Convert.ToInt32(sqlDataReader1.GetValue(1)),
                        RequestedMW = Convert.ToDouble(sqlDataReader1.GetValue(2)),
                        MarketDateTime = sqlDataReader1.GetDateTime(3),
                        PortfolioKey = Convert.ToInt32(sqlDataReader1.GetValue(4)),
                        ScheduleID = sqlDataReader1.GetString(5)
                    });
                sqlDataReader1.Close();
                vConnection.Close();
                List<Program.DownloadBids.SubmittedBids> source2 = new List<Program.DownloadBids.SubmittedBids>();
                SqlCommand sqlCommand2 = new SqlCommand();
                sqlCommand2.CommandText = "Select N1.NodeKey SourceNodeKey,N2.NodeKey SinkNodeKey,XB.AwardedMw,XB.EndTime,XB.BidID from XmlBidsInfo_temp XB join Node N1 on XB.Source=N1.NodeName join Node N2 on XB.Sink=N2.NodeName  where XB.EndTime>'" + vMarketDate.ToString("yyyy-MM-dd") + "' and XB.EndTime<='" + vMarketDate.AddDays(1.0).ToString("yyyy-MM-dd") + "' and XB.AwardedMw > 0  order by XB.EndTime ";
                sqlCommand2.Connection = vConnection;
                if (vConnection.State == ConnectionState.Closed)
                    vConnection.Open();
                SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
                while (sqlDataReader2.Read())
                {
                    Program.DownloadBids.SubmittedBids websiteBids = new Program.DownloadBids.SubmittedBids();
                    websiteBids.SourceNodeKey = Convert.ToInt32(sqlDataReader2.GetValue(0));
                    websiteBids.SinkNodeKey = Convert.ToInt32(sqlDataReader2.GetValue(1));
                    websiteBids.RequestedMW = Convert.ToDouble(sqlDataReader2.GetValue(2));
                    websiteBids.MarketDateTime = sqlDataReader2.GetDateTime(3);
                    websiteBids.ScheduleID = sqlDataReader2.GetString(4);
                    
                    if (!source1.Exists((Predicate<Program.DownloadBids.SubmittedBids>)(x => x.SourceNodeKey == websiteBids.SourceNodeKey && x.SinkNodeKey 
                    == websiteBids.SinkNodeKey && x.MarketDateTime == websiteBids.MarketDateTime && x.ScheduleID == websiteBids.ScheduleID)))
                    {
                        source2.AddRange((IEnumerable<Program.DownloadBids.SubmittedBids>)SetRiskBids(websiteBids, 0.0));
                    }
                    else
                    {
                        Program.DownloadBids.SubmittedBids submittedBids = source1.Where<Program.DownloadBids.SubmittedBids>((Func<Program.DownloadBids.SubmittedBids, bool>)(x => x.SourceNodeKey == websiteBids.SourceNodeKey && x.SinkNodeKey == websiteBids.SinkNodeKey && x.MarketDateTime == websiteBids.MarketDateTime && x.ScheduleID == websiteBids.ScheduleID)).FirstOrDefault<Program.DownloadBids.SubmittedBids>();
                        if (websiteBids.RequestedMW <= submittedBids.RequestedMW)
                        {
                            source2.Add(new Program.DownloadBids.SubmittedBids()
                            {
                                SourceNodeKey = submittedBids.SourceNodeKey,
                                SinkNodeKey = submittedBids.SinkNodeKey,
                                RequestedMW = websiteBids.RequestedMW,
                                MarketDateTime = submittedBids.MarketDateTime,
                                PortfolioKey = submittedBids.PortfolioKey,
                                ScheduleID = submittedBids.ScheduleID
                            });
                        }
                        else
                        {
                            source2.AddRange((IEnumerable<Program.DownloadBids.SubmittedBids>)SetRiskBids(new Program.DownloadBids.SubmittedBids()
                            {
                                SourceNodeKey = submittedBids.SourceNodeKey,
                                SinkNodeKey = submittedBids.SinkNodeKey,
                                RequestedMW = websiteBids.RequestedMW - submittedBids.RequestedMW,
                                MarketDateTime = submittedBids.MarketDateTime,
                                PortfolioKey = 3011,
                                ScheduleID = submittedBids.ScheduleID
                            }, submittedBids.RequestedMW));
                            source2.Add(new Program.DownloadBids.SubmittedBids()
                            {
                                SourceNodeKey = submittedBids.SourceNodeKey,
                                SinkNodeKey = submittedBids.SinkNodeKey,
                                RequestedMW = submittedBids.RequestedMW,
                                MarketDateTime = submittedBids.MarketDateTime,
                                PortfolioKey = submittedBids.PortfolioKey,
                                ScheduleID = submittedBids.ScheduleID
                            });
                        }
                    }
                }
                sqlDataReader2.Close();
                vConnection.Close();
                Console.WriteLine("Split Bids Completed");
                dtMainSplitBids = new DataTable();
                dtMainSplitBids.Columns.Add("SourceNodeKey");
                dtMainSplitBids.Columns.Add("SinkNodeKey");
                dtMainSplitBids.Columns.Add("ClearedMW");
                dtMainSplitBids.Columns.Add("MarketDateTime");
                dtMainSplitBids.Columns.Add("PortfolioKey");
                dtMainSplitBids.Columns.Add("BidID");
                List<Program.DownloadBids.SubmittedBids> list = source2.Distinct<Program.DownloadBids.SubmittedBids>().ToList<Program.DownloadBids.SubmittedBids>();
                for (int index = 0; index < list.Count; ++index)
                {
                    drMainSplitBids = dtMainSplitBids.NewRow();
                    drMainSplitBids["SourceNodeKey"] = (object)list[index].SourceNodeKey;
                    drMainSplitBids["SinkNodeKey"] = (object)list[index].SinkNodeKey;
                    drMainSplitBids["ClearedMW"] = (object)list[index].RequestedMW;
                    drMainSplitBids["MarketDateTime"] = (object)list[index].MarketDateTime;
                    drMainSplitBids["PortfolioKey"] = (object)list[index].PortfolioKey;
                    drMainSplitBids["BidID"] = (object)list[index].ScheduleID;
                    dtMainSplitBids.Rows.Add(drMainSplitBids);
                }
                InsertSplitBidsInfo(dtMainSplitBids);
            }

            private List<Program.DownloadBids.SubmittedBids> SetRiskBids(
              Program.DownloadBids.SubmittedBids subBids,
              double underMW)
            {
                List<Program.DownloadBids.SubmittedBids> submittedBidsList = new List<Program.DownloadBids.SubmittedBids>();
                double num1 = 0.0;
                double num2 = 0.0;
                bool flag = true;
                subBids.RequestedMW = Math.Round(subBids.RequestedMW, 1);
                underMW = Math.Round(underMW, 1);
                for (double num3 = 0.0; num3 < subBids.RequestedMW; num3 = Math.Round(num3 + 0.1, 1))
                {
                    if (flag)
                    {
                        if (num2 >= underMW)
                            flag = false;
                        num2 = Math.Round(num2 + 0.1, 1);
                    }
                    else
                    {
                        num1 = Math.Round(num1 + 0.1, 1);
                        flag = true;
                    }
                }
                submittedBidsList.Add(new Program.DownloadBids.SubmittedBids()
                {
                    SourceNodeKey = subBids.SourceNodeKey,
                    SinkNodeKey = subBids.SinkNodeKey,
                    MarketDateTime = subBids.MarketDateTime,
                    ScheduleID = subBids.ScheduleID,
                    RequestedMW = num1,
                    PortfolioKey = 3011
                });
                submittedBidsList.Add(new Program.DownloadBids.SubmittedBids()
                {
                    SourceNodeKey = subBids.SourceNodeKey,
                    SinkNodeKey = subBids.SinkNodeKey,
                    MarketDateTime = subBids.MarketDateTime,
                    ScheduleID = subBids.ScheduleID,
                    RequestedMW = num2,
                    PortfolioKey = 3011
                });
                return submittedBidsList;
            }

            private void InsertSplitBidsInfo(DataTable dtMainSplitBids)
            {
                int count = dtMainSplitBids.Rows.Count;
                try
                {
                    if (vConnection.State == ConnectionState.Closed)
                        vConnection.Open();
                    vDeleteXmlBidsInfoCommand = new SqlCommand();
                    vDeleteXmlBidsInfoCommand.CommandText = "truncate table  Vayu..ClearedEES_temp";
                    vDeleteXmlBidsInfoCommand.Connection = vConnection;
                    vDeleteXmlBidsInfoCommand.ExecuteNonQuery();
                    SqlTransaction sqlTransaction = vConnection.BeginTransaction();
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(vConnection, SqlBulkCopyOptions.TableLock, sqlTransaction))
                    {
                        sqlBulkCopy.DestinationTableName = "Vayu..ClearedEES_temp";
                        sqlBulkCopy.BatchSize = 2000;
                        sqlBulkCopy.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                        sqlBulkCopy.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                        sqlBulkCopy.ColumnMappings.Add("ClearedMW", "ClearedMW");
                        sqlBulkCopy.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                        sqlBulkCopy.ColumnMappings.Add("PortfolioKey", "PortfolioKey");
                        sqlBulkCopy.ColumnMappings.Add("BidID", "BidID");
                        sqlBulkCopy.BulkCopyTimeout = 420000;
                        sqlBulkCopy.WriteToServer(dtMainSplitBids);
                        SqlCommand sqlCommand = new SqlCommand("[UpMergeMain]", vConnection, sqlTransaction);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandTimeout = 420000;
                        sqlCommand.ExecuteNonQuery();
                        sqlTransaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            private List<int> GetPortfolioKeys()
            {
                List<int> intList = new List<int>();
                try
                {
                    if (vConnection.State == ConnectionState.Closed)
                        vConnection.Open();
                    SqlCommand command = vConnection.CreateCommand();
                    command.CommandText = " SELECT PORTFOLIO_ID FROM Vayu..Portfolio WHERE HUB = 'ERCOT'  and ACCOUNT='A17'";
                    SqlDataReader sqlDataReader = command.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        int int32 = Convert.ToInt32(sqlDataReader.GetValue(0));
                        intList.Add(int32);
                    }
                    sqlDataReader.Close();
                    vConnection.Close();
                }
                catch (Exception ex)
                {
                }
                return intList;
            }

            public void SaveBidInfo(List<Program.BidsInfo> bidsInfo)
            {
                if (vConnection.State == ConnectionState.Open)
                    vConnection.Close();
                vConnection.Open();
                vDeleteXmlBidsInfoCommand = new SqlCommand();
                vDeleteXmlBidsInfoCommand.CommandText = "truncate table  Vayu..XmlBidsInfo_temp";
                vDeleteXmlBidsInfoCommand.Connection = vConnection;
                vDeleteXmlBidsInfoCommand.ExecuteNonQuery();
                dtXmlBidsInfo = new DataTable();
                dtXmlBidsInfo.Columns.Add("AwardedMw");
                dtXmlBidsInfo.Columns.Add("BidID");
                dtXmlBidsInfo.Columns.Add("EndTime");
                dtXmlBidsInfo.Columns.Add("Price");
                dtXmlBidsInfo.Columns.Add("UserName");
                dtXmlBidsInfo.Columns.Add("Sink");
                dtXmlBidsInfo.Columns.Add("Source");
                dtXmlBidsInfo.Columns.Add("StartTime");
                dtXmlBidsInfo.Columns.Add("TradingDate");
                for (int index = 0; index < bidsInfo.Count; ++index)
                {
                    drXmlBidsInfo = dtXmlBidsInfo.NewRow();
                    drXmlBidsInfo["AwardedMw"] = (object)bidsInfo[index].AwardedMw;
                    drXmlBidsInfo["BidID"] = (object)bidsInfo[index].BidID;
                    drXmlBidsInfo["EndTime"] = (object)bidsInfo[index].EndTime;
                    drXmlBidsInfo["Price"] = (object)bidsInfo[index].Price;
                    drXmlBidsInfo["UserName"] = (object)bidsInfo[index].User;
                    drXmlBidsInfo["Sink"] = (object)bidsInfo[index].Sink;
                    drXmlBidsInfo["Source"] = (object)bidsInfo[index].Source;
                    drXmlBidsInfo["StartTime"] = (object)bidsInfo[index].StartTime;
                    drXmlBidsInfo["TradingDate"] = (object)bidsInfo[index].TradingDate;
                    dtXmlBidsInfo.Rows.Add(drXmlBidsInfo);
                }
                InsertXmlBidsInfo(dtXmlBidsInfo);
            }

            private void InsertXmlBidsInfo(DataTable dtXmlBidsInfo)
            {
                if (vConnection.State == ConnectionState.Open)
                    vConnection.Close();
                vConnection.Open();
                SqlTransaction externalTransaction = vConnection.BeginTransaction();
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(vConnection, SqlBulkCopyOptions.TableLock, externalTransaction))
                {
                    sqlBulkCopy.DestinationTableName = "Vayu..XmlBidsInfo_temp";
                    sqlBulkCopy.BatchSize = 2000;
                    sqlBulkCopy.ColumnMappings.Add("AwardedMw", "AwardedMw");
                    sqlBulkCopy.ColumnMappings.Add("BidID", "BidID");
                    sqlBulkCopy.ColumnMappings.Add("EndTime", "EndTime");
                    sqlBulkCopy.ColumnMappings.Add("Price", "Price");
                    sqlBulkCopy.ColumnMappings.Add("UserName", "UserName");
                    sqlBulkCopy.ColumnMappings.Add("Sink", "Sink");
                    sqlBulkCopy.ColumnMappings.Add("Source", "Source");
                    sqlBulkCopy.ColumnMappings.Add("StartTime", "StartTime");
                    sqlBulkCopy.ColumnMappings.Add("TradingDate", "TradingDate");
                    sqlBulkCopy.BulkCopyTimeout = 420000;
                    sqlBulkCopy.WriteToServer(dtXmlBidsInfo);
                    externalTransaction.Commit();
                }
            }

            public List<Program.ClearedBidsInfo> GetBids(List<Program.BidsInfo> bidsInfo)
            {
                List<Program.ClearedBidsInfo> clearedBidsInfoList = new List<Program.ClearedBidsInfo>();
                if (vConnection.State == ConnectionState.Closed)
                    vConnection.Open();
                vSelectPTPBids = new SqlCommand();
                vSelectPTPBids.CommandText = "select n1.NodeKey as sourceKey, n2.NodeKey as sinkKey ,b.AwardedMw,b.EndTime,b.BidID from  Vayu..XmlBidsInfo_temp b join node n1 on b.Source = n1.nodename  join Node n2  on b.Sink = n2.NodeName  where n1.MarketKey = 9 and n2.MarketKey = 9";
                vSelectPTPBids.Connection = vConnection;
                SqlDataReader sqlDataReader = vSelectPTPBids.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    Program.ClearedBidsInfo clearedBidsInfo = new Program.ClearedBidsInfo();
                    double num = Convert.ToDouble(sqlDataReader.GetValue(2));
                    if (num > 0.0)
                    {
                        clearedBidsInfo.SourceNodeKey = Convert.ToInt32(sqlDataReader.GetValue(0));
                        clearedBidsInfo.SinkNodeKey = Convert.ToInt32(sqlDataReader.GetValue(1));
                        clearedBidsInfo.ClearedMW = num;
                        clearedBidsInfo.MarketDateTime = Convert.ToDateTime(sqlDataReader.GetValue(3));
                        string str = Convert.ToString(sqlDataReader.GetValue(4));
                        clearedBidsInfo.BidID = sqlDataReader.GetValue(4).ToString();
                        str.Split('_');
                        clearedBidsInfo.PortfolioKey = 3011;
                        clearedBidsInfoList.Add(clearedBidsInfo);
                    }
                }
                sqlDataReader.Close();
                vConnection.Close();
                return clearedBidsInfoList;
            }

            public void SaveClearedBids(List<Program.ClearedBidsInfo> clearedBisInfo)
            {
                if (vConnection.State == ConnectionState.Open)
                    vConnection.Close();
                vConnection.Open();
                dtClearedBids = new DataTable();
                dtClearedBids.Columns.Add("SourceNodeKey");
                dtClearedBids.Columns.Add("SinkNodeKey");
                dtClearedBids.Columns.Add("ClearedMW");
                dtClearedBids.Columns.Add("MarketDateTime");
                dtClearedBids.Columns.Add("PortfolioKey");
                dtClearedBids.Columns.Add("BidID");
                for (int index = 0; index < clearedBisInfo.Count; ++index)
                {
                    drClearedBids = dtClearedBids.NewRow();
                    drClearedBids["SourceNodeKey"] = (object)clearedBisInfo[index].SourceNodeKey;
                    drClearedBids["SinkNodeKey"] = (object)clearedBisInfo[index].SinkNodeKey;
                    drClearedBids["ClearedMW"] = (object)clearedBisInfo[index].ClearedMW;
                    drClearedBids["MarketDateTime"] = (object)clearedBisInfo[index].MarketDateTime;
                    drClearedBids["PortfolioKey"] = (object)clearedBisInfo[index].PortfolioKey;
                    drClearedBids["BidID"] = (object)clearedBisInfo[index].BidID;
                    dtClearedBids.Rows.Add(drClearedBids);
                }
                InsertClearedBidsommandDB(dtClearedBids);
            }

            private void InsertClearedBidsommandDB(DataTable dtClearedBids)
            {
                if (vConnection.State == ConnectionState.Open)
                    vConnection.Close();
                vConnection.Open();
                vDeleteXmlBidsInfoCommand = new SqlCommand();
                vDeleteXmlBidsInfoCommand.CommandText = "truncate table  clearedees2011tempSP";
                vDeleteXmlBidsInfoCommand.Connection = vConnection;
                vDeleteXmlBidsInfoCommand.ExecuteNonQuery();
                SqlTransaction sqlTransaction = vConnection.BeginTransaction();
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(vConnection, SqlBulkCopyOptions.TableLock, sqlTransaction))
                {
                    sqlBulkCopy.DestinationTableName = "Vayu..clearedees2011tempSP";
                    sqlBulkCopy.BatchSize = 2000;
                    sqlBulkCopy.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                    sqlBulkCopy.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                    sqlBulkCopy.ColumnMappings.Add("ClearedMW", "ClearedMW");
                    sqlBulkCopy.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                    sqlBulkCopy.ColumnMappings.Add("PortfolioKey", "PortfolioKey");
                    sqlBulkCopy.ColumnMappings.Add("BidID", "BidID");
                    sqlBulkCopy.BulkCopyTimeout = 420000;
                    sqlBulkCopy.WriteToServer(dtClearedBids);
                    SqlCommand sqlCommand = new SqlCommand("[UpMergeBidsTest]", vConnection, sqlTransaction);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = 420000;
                    sqlCommand.ExecuteNonQuery();
                    sqlTransaction.Commit();
                }
            }

            private void Init() => vConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            private void TruncateTables()
            {
                using (SqlConnection sqlConnection = new SqlConnection(vConnection.ConnectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.Connection = sqlConnection;
                        command.CommandText = "truncate table clearedees_2011_temp ";
                        command.ExecuteNonQuery();
                    }
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.Connection = sqlConnection;
                        command.CommandText = "truncate table clearedeesActual_temp";
                        command.ExecuteNonQuery();
                    }
                    sqlConnection.Close();
                }
            }

            private class SubmittedBids
            {
                public int SourceNodeKey { get; set; }

                public int SinkNodeKey { get; set; }

                public double RequestedMW { get; set; }

                public DateTime MarketDateTime { get; set; }

                public int PortfolioKey { get; set; }

                public string ScheduleID { get; set; }
            }
        }

        public class ClearedBidsInfo
        {
            public int SinkNodeKey { get; set; }

            public int SourceNodeKey { get; set; }

            public double ClearedMW { get; set; }

            public DateTime MarketDateTime { get; set; }

            public int OasisID { get; set; }

            public int ScheduledID { get; set; }

            public int PortfolioKey { get; set; }

            public string BidID { get; set; }
        }

        public class BidsInfo
        {
            public double AwardedMw { get; set; }

            public string BidID { get; set; }

            public DateTime EndTime { get; set; }

            public double Price { get; set; }

            public string User { get; set; }

            public string Sink { get; set; }

            public string Source { get; set; }

            public DateTime StartTime { get; set; }

            public DateTime TradingDate { get; set; }
        }
    }
}
