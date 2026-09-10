using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CRRCreditLibrary;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Vayu.CancellationXmlLib;
using Vayu.CommonAccessLibrary;
namespace Vayu.CRRCreditCalculationService
{
    class CRRCreditCalculationServer : ICRRCredit
    {
        private static readonly object lockObj = new object();
        private SqlConnection mConnection90;
        //private SqlCommand mSelectSppRefPriceCommand;
        //private SqlCommand mSelectPjmWtdHisCongCommand;
        //private SqlCommand mSelectPjmPeriodCommand;
        //private SqlCommand mSelectMisoIsSeasonalCommand;
        //private SqlCommand mSelectMisoSeasonDaysCommand;
        //private SqlCommand mSelectCaisoPeriodDatesCommand;
        //private SqlCommand mSelectAllCaisoCreditMarginCommand;
        //private SqlCommand mSelectMaxCaisoCreditMarginCommand;
        //private static Dictionary<string, double> sSppRefPriceHash = new Dictionary<string, double>();
        DBService dbService;
        public CRRCreditCalculationServer()
        {
            dbService = new DBService();
        }

        public void Connect()
        {
            using (ServiceHost host = new ServiceHost(typeof(CRRCreditCalculationServer), new Uri("net.tcp://localhost:8008")))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.OpenTimeout = new TimeSpan(0, 360, 0);
                myBinding.SendTimeout = new TimeSpan(0, 360, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 360, 0);
                myBinding.CloseTimeout = new TimeSpan(0, 360, 0);
                myBinding.TransactionFlow = false;
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.MaxReceivedMessageSize = int.MaxValue;
                myBinding.MaxBufferPoolSize = int.MaxValue;
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.TransferMode = TransferMode.Streamed;
                myBinding.ReaderQuotas.MaxArrayLength = 5000000;
                host.AddServiceEndpoint(typeof(ICRRCredit), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine("Successfully opened port 8008 for Vayu.CRRCreditCalculationService");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        private void InitDB()
        {
            mConnection90 = new VayuDBConnection().GetInstance().GetSqlConnection();
        }

        public Dictionary<long, CreditResult> GetCredit(int marketKey, List<Credit> creditList)
        {
            InitDB();
            Dictionary<long, CreditResult> creditHash = new Dictionary<long, CreditResult>();
            
            return creditHash;
        }

        public string Cancel(int portfolio, string transaction, int round)
        {
            #region Old Code
            //lock (lockObj)
            //{
            //    StringBuilder bidText = new StringBuilder();
            //    bidText.AppendLine("<?xml version=\"1.0\"?>");
            //    bidText.AppendLine("<env:Envelope xmlns:mkt=\"http://eftr.pjm.com/ftr/xml\" xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            //    bidText.AppendLine("<env:Body>");
            //    bidText.AppendLine("<mkt:SubmitRequest>");
            //    bidText.AppendLine("<DeleteByTransaction>");
            //    bidText.AppendLine("<TransactionID>" + transaction + "</TransactionID>");
            //    bidText.AppendLine("</DeleteByTransaction>");
            //    bidText.AppendLine("</mkt:SubmitRequest>");
            //    bidText.AppendLine("</env:Body>");
            //    bidText.AppendLine("</env:Envelope>");
            //    NetworkCredential networkCred = DBService.GetNetworkCredentials(portfolio);
            //    HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("https://ftr.pjm.com/ftr/xml/submit");
            //    wrq.Method = "POST";
            //    wrq.ContentType = "text/xml";
            //    wrq.ContentLength = bidText.Length;
            //    wrq.Timeout = 240000;
            //    wrq.Credentials = networkCred;
            //    string tranId = "0";
            //    string error = "success";
            //    try
            //    {
            //        StreamWriter sw = default(StreamWriter);
            //        sw = new StreamWriter(wrq.GetRequestStream());
            //        sw.Write(bidText.ToString());
            //        sw.Close();
            //        Thread.Sleep(1000);
            //        HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
            //        StreamReader sr = new StreamReader(wrp.GetResponseStream());
            //        string xmlResponse = sr.ReadToEnd();
            //        Console.WriteLine("Cancel");
            //        Console.WriteLine(DateTime.Now + " : " + xmlResponse);
            //        sr.Close();
            //        XmlDataDocument xmlParser = new XmlDataDocument();
            //        xmlParser.LoadXml(xmlResponse);
            //        foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Text"))
            //        {
            //            if (error == "success")
            //            {
            //                error = xmlNode.InnerText; ;
            //            }
            //            else
            //            {
            //                error = error + "\n" + xmlNode.InnerText;
            //            }
            //        }
            //        Console.WriteLine("Error " + error);
            //        foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Success"))
            //        {
            //            tranId = xmlNode.InnerText;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        error = ex.Message;
            //        Console.WriteLine(DateTime.Now + " : " + ex);
            //    }
            //    if (!string.IsNullOrEmpty(transaction))
            //    {
            //        DBService.DeleteTransaction(transaction, round);
            //        DBService.UpdateInvalidFtrBids(transaction);
            //    }
            //    return error;
            //} 
            #endregion
            string status = CancelNew(portfolio, transaction, round);
            return status;
        }
        public string Post(CRR[] ftr, DateTime toDate, int portfolio, string market, string marketName, int round, string type)
        {
            //  round = 1;
            #region OldCode
            ////lock (lockObj)
            //{
            //    string tokenId = GetToken(portfolio);
            //    StringBuilder bidText = GetXmlStringBuilder(ftr, market, round);
            //    File.WriteAllText("ReqFile.xml", bidText.ToString());
            //    NetworkCredential networkCred = DBService.GetNetworkCredentials(portfolio);
            //    HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("https://ftrcenter.pjm.com/ftrcenter/xml/submit");
            //    wrq.Method = "POST";
            //    wrq.ContentType = "text/xml";
            //    wrq.ContentLength = bidText.Length;
            //    wrq.AllowWriteStreamBuffering = true;
            //    wrq.Headers.Set("Cookie", "pjmauth=" + tokenId);
            //    wrq.Timeout = 240000;
            //    wrq.Credentials = networkCred;
            //    string tranId = "0";
            //    string error = "success";
            //    try
            //    {
            //        StreamWriter sw = default(StreamWriter);
            //        sw = new StreamWriter(wrq.GetRequestStream());
            //        sw.Write(bidText.ToString());
            //        sw.Close();
            //        Thread.Sleep(1000);
            //        HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
            //        StreamReader sr = new StreamReader(wrp.GetResponseStream());
            //        string xmlResponse = sr.ReadToEnd();
            //        Console.WriteLine("Post");
            //        Console.WriteLine(DateTime.Now + " : " + xmlResponse);
            //        sr.Close();
            //        XmlDataDocument xmlParser = new XmlDataDocument();
            //        xmlParser.LoadXml(xmlResponse);
            //        foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Text"))
            //        {
            //            if (error == "success")
            //            {
            //                error = xmlNode.InnerText; ;
            //            }
            //            else
            //            {
            //                error = error + "\n" + xmlNode.InnerText;
            //            }
            //        }
            //        Console.WriteLine("Error " + error);
            //        foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Success"))
            //        {
            //            tranId = xmlNode.InnerText;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        error = ex.ToString();
            //        Console.WriteLine(DateTime.Now + " : " + ex);
            //    }

            //    if (tranId != "0")
            //    {
            //        DBService.SaveTransaction(tranId, marketName, round, type, 1);
            //        dbService.BulkUpdateStatus(ftr, "Valid", tranId);
            //        return tranId;
            //    }

            //    dbService.BulkUpdateStatus(ftr, "Invalid", tranId);
            //    return error;
            //} 
            #endregion
            string message = PostNew(ftr, toDate, portfolio, market, marketName, round, type);
            return message;
        }


        private StringBuilder GetXmlStringBuilder(CRR[] ftr, string market, int round)
        {
            StringBuilder bidText = new StringBuilder();
            bidText.AppendLine("<?xml version=\"1.0\"?>");
            bidText.AppendLine("<env:Envelope xmlns:mkt=\"http://eftr.pjm.com/ftr/xml\" xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            bidText.AppendLine("<env:Body>");
            bidText.AppendLine("<mkt:SubmitRequest>");
            string roundStr = round == 0 ? "" : " round=\"" + round + "\"";
            string marketStr = "market=\"" + market + "\"";
            bidText.AppendLine("<mkt:FTRQuotes " + marketStr + roundStr + ">");
            foreach (CRR ftrObj in ftr)
            {
                string tradeType = ftrObj.TradeType.ToUpper() == "BUY" ? "Buy" : "Sell";
                string classType = ftrObj.Class.ToUpper() == "OFFPEAK" ? "OffPeak" : "OnPeak";
                string hedgeType = ftrObj.Hedge.ToUpper() == "OBLIGATION" ? "Obligation" : "Option";
                bidText.AppendLine("<mkt:FTRQuote trade=\"" + tradeType + "\">");
                bidText.AppendLine("<Path sink=\"" + ftrObj.PathSink + "\" source=\"" + ftrObj.PathSource + "\"/>");
                bidText.AppendLine("<Class>" + classType + "</Class>");
                string period = ftrObj.Period == "ALL" ? "All" : ftrObj.Period.ToUpper();
                bidText.AppendLine("<Period>" + period + "</Period>");
                bidText.AppendLine("<Hedge>" + hedgeType + "</Hedge>");
                bidText.AppendLine("<MW>" + ftrObj.MW + "</MW>");
                bidText.AppendLine("<Price>" + Math.Round(ftrObj.Price, 2) + "</Price>");
                bidText.AppendLine("</mkt:FTRQuote>");
            }
            bidText.AppendLine("</mkt:FTRQuotes>");
            bidText.AppendLine("</mkt:SubmitRequest>");
            bidText.AppendLine("</env:Body>");
            bidText.AppendLine("</env:Envelope>");
            return bidText;
        }
        public string CreateSubmissionFile(CRR[] ftr, DateTime toDate, string portfolioName, string market, int round, string user, string type)
        {

            StringBuilder buildHelper = GetXmlStringBuilder(ftr, market, round);
            try
            {
                if (buildHelper.ToString().Length > 0)
                {
                    if (!Directory.Exists(@"D:\ISOFiles\CRRCreditCalculationService\" + user))
                    {
                        Directory.CreateDirectory(@"D:\ISOFiles\CRRCreditCalculationService\" + user);
                    }
                    TextWriter writer = new StreamWriter(@"D:\ISOFiles\CRRCreditCalculationService\" + user + "\\" + portfolioName + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xml");
                    writer.Write(buildHelper.ToString());
                    writer.Flush();
                    writer.Close();
                    return "Successfully created the file at " + @"D:\ISOFiles\CRRCreditCalculationService\" + user + "\\" + portfolioName + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xml";
                }
                return "Failed to create the file!!";
            }
            catch
            {
                return "Failed to create the file!!";
            }
        }

        public void Dispose()
        {
            if (dbService != null)
                dbService.OnDispose();
        }
        public string GetToken(int portfolio)
        {

            Dictionary<string, string> TokenResponseHash = new Dictionary<string, string>();

            NetworkCredential networkCred = DBService.GetNetworkCredentials(portfolio);
            HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("https://sso.pjm.com/access/authenticate/");
            wrq.ContentType = "application/json";
            wrq.Headers.Set("X-OpenAM-Username", networkCred.UserName);
            wrq.Headers.Set("X-OpenAM-Password", networkCred.Password);
            wrq.ContentLength = 0;
            wrq.ProtocolVersion = System.Net.HttpVersion.Version11;
            wrq.Method = "POST";
            wrq.Accept = "*/*";
            wrq.Timeout = 240000;
            //
            //StreamWriter sw = default(StreamWriter);
            //sw = new StreamWriter(wrq.GetRequestStream());
            //sw.Write(sw.ToString());
            //sw.Close();
            HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
            StreamReader sr = new StreamReader(wrp.GetResponseStream());
            string responseData = sr.ReadToEnd();
            string tokenId = string.Empty;
            TokenResponseHash = responseData.ToString().Remove(0, 1).Split(',').Select(value => value.Split(':')).ToDictionary(pair => pair[0].Replace("\"", ""), pair => pair[1].Replace("\"", ""));
            tokenId = TokenResponseHash["tokenId"];
            Console.WriteLine(DateTime.Now + " : " + tokenId);
            sr.Close();
            return tokenId;
        }
        private string GetXmlStringBuilderNew(CRR[] ftr, string market, int round)
        {
            SubmitRequestFTRQuotes pathMktQuotes;
            SubmitRequestFTRQuotesFTRQuote[] pathQuoteArrey;
            List<SubmitRequestFTRQuotesFTRQuote> pathQuotesList = new List<SubmitRequestFTRQuotesFTRQuote>();
            SubmitRequest subReq;
            EnvelopeBody envBdy;
            Envelope soapEnv = null;

            foreach (CRR ftrObj in ftr)
            {
                SubmitRequestFTRQuotesFTRQuotePath path = new SubmitRequestFTRQuotesFTRQuotePath();
                path.source = ftrObj.PathSource;
                path.sink = ftrObj.PathSink;
                SubmitRequestFTRQuotesFTRQuote pathQuote = new SubmitRequestFTRQuotesFTRQuote();
                pathQuote.Class = ftrObj.Class == "PEAK" ? "OnPeak" : "OffPeak";
                pathQuote.Hedge = ftrObj.Hedge == "OBLIGATION" ? "Obligation" : "Option";
                pathQuote.MW = (decimal)ftrObj.MW;
                pathQuote.Path = path;
                pathQuote.Period = ftrObj.Period == "ALL" ? "All" : ftrObj.Period.ToUpper();

                double price = Math.Round(ftrObj.Price, 2);
                pathQuote.Price = (decimal)price;
                pathQuote.trade = ftrObj.TradeType == "BUY" ? "Buy" : "Sell";
                pathQuotesList.Add(pathQuote);
            }
            if (pathQuotesList.Count > 0)
            {
                pathQuoteArrey = pathQuotesList.ToArray();
                pathMktQuotes = new SubmitRequestFTRQuotes();
                pathMktQuotes.FTRQuote = pathQuoteArrey;
                pathMktQuotes.market = market;
                pathMktQuotes.round = (byte)round;
                subReq = new SubmitRequest();
                subReq.FTRQuotes = pathMktQuotes;
                envBdy = new EnvelopeBody();
                envBdy.SubmitRequest = subReq;
                soapEnv = new Envelope();
                soapEnv.Body = envBdy;
            }

            XmlSerializer serialiseXml = new XmlSerializer(typeof(Envelope));
            TextWriter tw = new StreamWriter("Req.xml");
            serialiseXml.Serialize(tw, soapEnv);
            tw.Close();
            string xmlString = File.ReadAllText("Req.xml");

            return xmlString;
        }

        public string PostNew(CRR[] ftr, DateTime toDate, int portfolio, string market, string marketName, int round, string type)
        {
            //lock (lockObj)
            {

                string tokenId = GetToken(portfolio);
                string bidText = GetXmlStringBuilderNew(ftr, market, round);
                NetworkCredential networkCred = DBService.GetNetworkCredentials(portfolio);
                HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("https://ftrcenter.pjm.com/ftrcenter/xml/submit");
                wrq.Method = "POST";
                wrq.ContentType = "text/xml";
                wrq.ContentLength = bidText.Length;
                wrq.AllowWriteStreamBuffering = true;
                wrq.Headers.Set("Cookie", "pjmauth=" + tokenId);
                wrq.Timeout = 240000;
                wrq.Credentials = networkCred;
                string tranId = "0";
                string error = "success";
                try
                {
                    StreamWriter sw = default(StreamWriter);
                    sw = new StreamWriter(wrq.GetRequestStream());
                    sw.Write(bidText.ToString());
                    sw.Close();
                   

                    Thread.Sleep(1000);
                    HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
                    StreamReader sr = new StreamReader(wrp.GetResponseStream());
                    string xmlResponse = sr.ReadToEnd();
                    Console.WriteLine("Post");
                    Console.WriteLine(DateTime.Now + " : " + xmlResponse);
                    sr.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(xmlResponse);
                    foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Text"))
                    {
                        if (error == "success")
                        {
                            error = xmlNode.InnerText; ;
                        }
                        else
                        {
                            error = error + "\n" + xmlNode.InnerText;
                        }
                    }
                    Console.WriteLine("Error " + error);
                    foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Success"))
                    {
                        tranId = xmlNode.InnerText;
                    }
                }
                catch (Exception ex)
                {
                    error = ex.ToString();
                    Console.WriteLine(DateTime.Now + " : " + ex);
                }

                if (tranId != "0")
                {
                    DBService.SaveTransaction(tranId, marketName, round, type, 1);
                    dbService.BulkUpdateStatus(ftr, "Valid", tranId);
                    return tranId;
                }

                dbService.BulkUpdateStatus(ftr, "Invalid", tranId);
                return error;
            }
        }

        public string CancelNew(int portfolio, string transaction, int round)
        {
            lock (lockObj)
            {
                StringBuilder bidText = new StringBuilder();
                bidText.AppendLine("<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
                bidText.AppendLine("<Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                bidText.AppendLine("<SubmitRequest xmlns=\"http://eftr.pjm.com/ftr/xml\">");
                bidText.AppendLine("<DeleteByTransaction>");
                bidText.AppendLine("<TransactionID>" + transaction + "</TransactionID>");
                bidText.AppendLine("</DeleteByTransaction>");
                bidText.AppendLine("</SubmitRequest>");
                //bidText.AppendLine("</env:Envelope>");
                //
                Vayu.CancellationXmlLib.SubmitRequestDeleteByTransaction cancel = new Vayu.CancellationXmlLib.SubmitRequestDeleteByTransaction();
                cancel.TransactionID = Convert.ToInt32(transaction);
                Vayu.CancellationXmlLib.SubmitRequest req1 = new Vayu.CancellationXmlLib.SubmitRequest();
                req1.DeleteByTransaction = cancel;
                Vayu.CancellationXmlLib.Envelope env = new Vayu.CancellationXmlLib.Envelope();
                env.SubmitRequest = req1;
                try
                {
                    XmlSerializer cancelser = new XmlSerializer(typeof(Vayu.CancellationXmlLib.Envelope));
                    TextWriter tw = new StreamWriter("CancelReq.xml");
                    cancelser.Serialize(tw, cancelser);
                    tw.Close();
                }
                catch (Exception ex)
                {


                }

                // string bidText = File.ReadAllText("CancelReq.xml");
                //

                string tokenId = GetToken(portfolio);
                NetworkCredential networkCred = DBService.GetNetworkCredentials(portfolio);
                HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("https://ftrcenter.pjm.com/ftrcenter/xml/submit");
                wrq.Method = "POST";
                wrq.ContentType = "text/xml";
                wrq.ContentLength = bidText.Length;
                wrq.AllowWriteStreamBuffering = true;
                wrq.Headers.Set("Cookie", "pjmauth=" + tokenId);
                wrq.Timeout = 240000;
                wrq.Credentials = networkCred;
                string tranId = "0";
                string error = "success";
                //
                try
                {
                    StreamWriter sw = default(StreamWriter);
                    sw = new StreamWriter(wrq.GetRequestStream());
                    sw.Write(bidText.ToString());
                    sw.Close();
                    Thread.Sleep(1000);
                    HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
                    StreamReader sr = new StreamReader(wrp.GetResponseStream());
                    string xmlResponse = sr.ReadToEnd();
                    Console.WriteLine("Cancel");
                    Console.WriteLine(DateTime.Now + " : " + xmlResponse);
                    sr.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(xmlResponse);
                    foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Text"))
                    {
                        if (error == "success")
                        {
                            error = xmlNode.InnerText; ;
                        }
                        else
                        {
                            error = error + "\n" + xmlNode.InnerText;
                        }
                    }
                    Console.WriteLine("Error " + error);
                    foreach (XmlNode xmlNode in xmlParser.GetElementsByTagName("Success"))
                    {
                        tranId = xmlNode.InnerText;
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    Console.WriteLine(DateTime.Now + " : " + ex);
                }
                if (!string.IsNullOrEmpty(transaction))
                {
                    DBService.DeleteTransaction(transaction, round);
                    DBService.UpdateInvalidCRRBids(transaction);
                }
                return error;
            }
        }
    }
}
