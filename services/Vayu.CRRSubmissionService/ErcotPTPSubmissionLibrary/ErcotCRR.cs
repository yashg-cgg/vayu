using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CRRSubmissionLibrary;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using Vayu.CRRNodalServiceLibrary.ERCOTNodalService;
using ews_2007_06;
namespace Vayu.CRRNodalServiceLibrary
{
    public class ErcotCRR
    {
        public Vayu.CRRNodalServiceLibrary.ERCOTNodalService.RequestMessage CreateRequestMessage(CRRBid[] bids, int submittype)
        {
            int totalBids = bids.Length;
            DateTime tdate = DateTime.Today.AddDays(1);
            BidSet myBidSet = new BidSet();
            myBidSet.tradingDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            myBidSet.Items = new object[totalBids];
            myBidSet.BidsetItemsElementName = new BidsetItemsChoiceType[totalBids];
            CRR[] mCRRObligation = new CRR[totalBids];
            for (int i = 0; i < totalBids; i++)
            {
                mCRRObligation[i] = new CRR();
                mCRRObligation[i].startTime = DateTime.Parse(FormattedDate(tdate));
                mCRRObligation[i].startTimeSpecified = true;
                mCRRObligation[i].endTime = DateTime.Parse(FormattedDate(tdate.AddDays(1)));
                mCRRObligation[i].endTimeSpecified = true;
                mCRRObligation[i].marketType = "DAM";
                mCRRObligation[i].crrId = bids[i].ID.ToString();
                mCRRObligation[i].source = bids[i].PathSource;
                mCRRObligation[i].sink = bids[i].PathSink;
                mCRRObligation[i].crrAccountHolderId = "XTALLR";
                mCRRObligation[i].CapacitySchedule = new CapacitySchedule();
                int totalHour = 1;// bids[i].Bidvals.Length;
                mCRRObligation[i].CapacitySchedule.TmPoint = new TmPoint[1];
                mCRRObligation[i].MinimumReservationPrice = new MinimumReservationPrice[1];
                double MW = 0;               

                for (int j = 0; j < 1; j++)
                {
                    TmPoint mTmPoint = new TmPoint();
                    mTmPoint.time = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour - 1)));
                    mTmPoint.ending = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour)));
                    mTmPoint.endingSpecified = true;
                    if (bids[i].Bidvals[j].MW < 1)
                        MW = 1;
                    else
                        MW = bids[i].Bidvals[j].MW;
                    mTmPoint.value1 = new decimal(MW);
                    mTmPoint.value1Specified = true;
                    mCRRObligation[i].CapacitySchedule.TmPoint[j] = mTmPoint;
                   // MinimumPrice myMinimumPrice = new MinimumPrice();
                    MinimumReservationPrice myMinimumPrice = new MinimumReservationPrice();
                    myMinimumPrice.startTime = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour - 1)));
                    myMinimumPrice.endTime = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour)));
                    myMinimumPrice.price = new decimal(bids[i].Bidvals[j].Price);
                    mCRRObligation[i].MinimumReservationPrice[j] = myMinimumPrice;
                }
                myBidSet.Items[i] = mCRRObligation[i];
                myBidSet.BidsetItemsElementName[i] = BidsetItemsChoiceType.CRR;
            }

            string bidSetXml = null;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ews_2007_06.BidSet));
                XmlQualifiedName qualified = new XmlQualifiedName("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                XmlSerializerNamespaces serializerns = new XmlSerializerNamespaces(new XmlQualifiedName[] { qualified });
                bidSetXml = XmlUtil.Serialize(serializer, Encoding.ASCII, serializerns, true, myBidSet);
                bidSetXml = bidSetXml.Replace("<BidSet>", "<BidSet xmlns=\"http://www.ercot.com/schema/2007-06/nodal/ews\">");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            XmlDocument mxmldoc = new XmlDocument();
            XElement xelem = XElement.Parse(bidSetXml, LoadOptions.SetBaseUri);
            mxmldoc.Load(xelem.CreateReader());
            RequestMessage reqMessage = new RequestMessage();
            reqMessage.Payload = new PayloadType();
            reqMessage.Payload.Any = new XmlElement[] { mxmldoc.DocumentElement };
            try
            {
                XmlSerializer mserializer = new XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces xmlnamespace = new XmlSerializerNamespaces();
                string xml = XmlUtil.Serialize(mserializer, Encoding.ASCII, xmlnamespace, true, reqMessage);
                System.Diagnostics.Debug.WriteLine(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            HeaderType myHeaderType = new HeaderType();
            ReplayDetectionType myReplayDetectionType = new ReplayDetectionType();
            RequestType myRequestType = new RequestType();
            myHeaderType.Verb = HeaderTypeVerb.create;
            myHeaderType.Noun = "BidSet";

            myReplayDetectionType.Nonce = new EncodedString();
            myReplayDetectionType.Nonce.Value = Convert.ToString(Guid.NewGuid());
            myReplayDetectionType.Created = new AttributedDateTime();
            myReplayDetectionType.Created.Value = Convert.ToString(String.Format("{0:s}", DateTime.Now));
            myHeaderType.ReplayDetection = myReplayDetectionType;
            myHeaderType.Revision = "1.0";
            if (submittype == 0)
            {
                myHeaderType.Source = "XTALLR";
                myHeaderType.UserID = "API_20200609rpalave";
            }
            myHeaderType.MessageID = DateTime.Now.ToString("MMddHHmmss");
            reqMessage.Header = myHeaderType;
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, reqMessage);
                System.Diagnostics.Debug.WriteLine(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
            }
            return reqMessage;
        }
        public string Upload(CRRBid[] bids, int submittype)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
                CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
                ErcotNodalClient ercotClient = new ErcotNodalClient();
                OperationsClient operationsClient = ercotClient.CreateErcotOperationsClientTest(myClientRequestBinding, submittype);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Console.WriteLine(ServicePointManager.SecurityProtocol.ToString());
                RequestMessage reqMessage = CreateRequestMessage(bids, submittype);
                ResponseMessage respMessage = new ResponseMessage();
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, reqMessage);
                    respMessage = operationsClient.MarketTransactions(reqMessage);
                    if(respMessage.Reply.ReplyCode=="OK")
                    {
                        return "Success";
                    }
                    else
                    {
                        return "Error";
                    }
                }
                catch (Exception ex)
                {
                     Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
            }
            return null;
        }
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
        public string FormattedDate(DateTime d)
        {
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            bool isDST = curTimeZone.IsDaylightSavingTime(d);
            if (isDST == true)
            {
                return d.ToString("yyyy-MM-ddTHH:mm:ss-05:00");
            }
            else
            {
                return d.ToString("yyyy-MM-ddTHH:mm:ss-06:00");
            }
        }
        public string Cancel(int Portfolio,string transaction,int round)
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
            AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
            CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
            ErcotNodalClient ercotClient = new ErcotNodalClient();
            OperationsClient operationsClient = new OperationsClient();// ercotClient.CreateErcotOperationsClientTest(myClientRequestBinding, submittype);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.WriteLine(ServicePointManager.SecurityProtocol.ToString());

            RequestMessage reqMessage = CreateCancelRequestMessage(Portfolio, transaction, round);
            ResponseMessage respMessage = new ResponseMessage();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces serializerns = new XmlSerializerNamespaces();
                string xmlstr = XmlUtil.Serialize(serializer, Encoding.ASCII, serializerns, true, reqMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
            }
            try
            {
                respMessage = operationsClient.MarketTransactions(reqMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if(respMessage.Reply.ReplyCode=="OK")
            {
                return "Success";
            }
            else
            {
                return "Error";
            }
        }
        public Vayu.CRRNodalServiceLibrary.ERCOTNodalService.RequestMessage CreateCancelRequestMessage(int poertfolio, string trasaction, int round)
        {
            string bidSetXml = string.Empty;
            string[] requestedID = new string[trasaction.Length];
            requestedID[0] = trasaction;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ews_2007_06.BidSet));
                XmlQualifiedName xmlQField = new XmlQualifiedName("","http://www.ercot.com/schema/2007-06/nodal/ews");
                XmlSerializerNamespaces serializerns = new XmlSerializerNamespaces(new XmlQualifiedName[] { xmlQField });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            XmlDocument doc = new XmlDocument();
            RequestMessage reqMessage = new RequestMessage();
            reqMessage.Payload = new PayloadType();
            reqMessage.Payload.Any = new XmlElement[] { doc.DocumentElement };
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                XmlQualifiedName xmlQField = new XmlQualifiedName("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                XmlSerializerNamespaces serializerns = new XmlSerializerNamespaces(new XmlQualifiedName[] { xmlQField });
                string xmlstr = XmlUtil.Serialize(serializer, Encoding.ASCII, serializerns, true, reqMessage);
                System.Diagnostics.Debug.WriteLine(xmlstr);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            HeaderType myHeader = new HeaderType();
            ReplayDetectionType myReplayDetectionType = new ReplayDetectionType();
            RequestType myRequest = new RequestType();

            myHeader.Verb = HeaderTypeVerb.cancel;
            myHeader.Noun = "BidSet";

            myReplayDetectionType.Nonce = new EncodedString();
            myReplayDetectionType.Nonce.Value = Convert.ToString(Guid.NewGuid());
            myReplayDetectionType.Created = new AttributedDateTime();
            myReplayDetectionType.Created.Value = Convert.ToString(String.Format("{0:s}", DateTime.Now));
            myHeader.ReplayDetection = myReplayDetectionType;

            myHeader.Revision = "1.0";
            myHeader.Source = "QTALLR";
            myHeader.UserID = "API_20200609rpalave";
            myHeader.MessageID = DateTime.Now.ToString("MMddHHmmss");
            reqMessage.Header = myHeader;
            myRequest.ID = requestedID;
            reqMessage.Request = myRequest;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces serializerns=new XmlSerializerNamespaces();
                string xmlstr=XmlUtil.Serialize(serializer,Encoding.ASCII,serializerns,true,reqMessage);
                System.Diagnostics.Debug.WriteLine(xmlstr);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return reqMessage;
        }
    }
}
