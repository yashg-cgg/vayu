//#define test

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;

using ews_2007_06;

using Vayu.ErcotPTPSubmissionLibrary.ERCOTNodalService;
using Vayu.ErcotPTPSubmissionLibrary;
using Vayu.ErcotSubmissionLibrary;

namespace Vayu.ErcotPTPSubmissionLibrary
{
    public class ErcotPTP
    {
        public ErcotPTP()
        {
        }

        public RequestMessage CreateRequestMessage1() // todo: can we delete this unused method?
        {
            // set up the  Request body objects
            PTPObligation myPTPObligation = new PTPObligation();


            myPTPObligation.startTime = DateTime.Parse("2011-12-09T00:00:00-06:00");
            myPTPObligation.startTimeSpecified = true;
            myPTPObligation.endTime = DateTime.Parse("2011-12-09T00:00:00-06:00");
            myPTPObligation.endTimeSpecified = true;
            myPTPObligation.marketType = "DAM";
            myPTPObligation.source = "SDSES_PUN3";
            myPTPObligation.sink = "BUCHAN_ALL_8";
            myPTPObligation.bidId = "987_38";

            myPTPObligation.CapacitySchedule = new TmSchedule();
            myPTPObligation.CapacitySchedule.TmPoint = new TmPoint[2];
            TmPoint myTmPoint1 = new TmPoint();
            myTmPoint1.time = DateTime.Parse("2011-12-09T15:00:00-06:00");
            myTmPoint1.ending = DateTime.Parse("2011-12-09T16:00:00.000-06:00");
            myTmPoint1.endingSpecified = true;
            myTmPoint1.value1 = 1;
            myTmPoint1.value1Specified = true;
            myPTPObligation.CapacitySchedule.TmPoint[0] = myTmPoint1;

            TmPoint myTmPoint2 = new TmPoint();
            myTmPoint2.time = DateTime.Parse("2011-12-09T16:00:00-06:00");
            myTmPoint2.ending = DateTime.Parse("2011-12-09T17:00:00.000-06:00");
            myTmPoint2.endingSpecified = true;
            myTmPoint2.value1 = 1;
            myTmPoint2.value1Specified = true;
            myPTPObligation.CapacitySchedule.TmPoint[1] = myTmPoint2;




            MaximumPrice myMaximumPrice1 = new MaximumPrice();
            myMaximumPrice1.startTime = DateTime.Parse("2011-12-09T15:00:00-06:00");
            myMaximumPrice1.endTime = DateTime.Parse("2011-12-09T16:00:00.000-06:00");
            myMaximumPrice1.price = new decimal(1.08);
            myPTPObligation.MaximumPrice = new MaximumPrice[2];
            myPTPObligation.MaximumPrice[0] = myMaximumPrice1;

            MaximumPrice myMaximumPrice2 = new MaximumPrice();
            myMaximumPrice2.startTime = DateTime.Parse("2011-12-09T16:00:00.000-06:00");
            myMaximumPrice2.endTime = DateTime.Parse("2011-12-09T17:00:00.000-06:00");
            myMaximumPrice2.price = new decimal(1.08);
            myPTPObligation.MaximumPrice[1] = myMaximumPrice2;




            PTPObligation myPTPObligation1 = new PTPObligation();

            myPTPObligation1.startTime = DateTime.Parse("2011-12-09T00:00:00-06:00");
            myPTPObligation1.startTimeSpecified = true;
            myPTPObligation1.endTime = DateTime.Parse("2011-12-09T00:00:00-06:00");
            myPTPObligation1.endTimeSpecified = true;
            myPTPObligation1.marketType = "DAM";
            myPTPObligation1.source = "SDSES_UNIT4";
            myPTPObligation1.sink = "INKS_INKS_G1";
            myPTPObligation1.bidId = "989_99";

            myPTPObligation1.CapacitySchedule = new TmSchedule();
            myPTPObligation1.CapacitySchedule.TmPoint = new TmPoint[2];
            // TmPoint myTmPoint1 = new TmPoint();
            myTmPoint1.time = DateTime.Parse("2011-12-09T15:00:00-06:00");
            myTmPoint1.ending = DateTime.Parse("2011-12-09T16:00:00.000-06:00");
            myTmPoint1.endingSpecified = true;
            myTmPoint1.value1 = 1;
            myTmPoint1.value1Specified = true;
            myPTPObligation1.CapacitySchedule.TmPoint[0] = myTmPoint1;

            // TmPoint myTmPoint2 = new TmPoint();
            myTmPoint2.time = DateTime.Parse("2011-12-09T16:00:00-06:00");
            myTmPoint2.ending = DateTime.Parse("2011-12-09T17:00:00.000-06:00");
            myTmPoint2.endingSpecified = true;
            myTmPoint2.value1 = 1;
            myTmPoint2.value1Specified = true;
            myPTPObligation1.CapacitySchedule.TmPoint[1] = myTmPoint2;

            // MaximumPrice myMaximumPrice1 = new MaximumPrice();
            myMaximumPrice1.startTime = DateTime.Parse("2011-12-09T15:00:00-06:00");
            myMaximumPrice1.endTime = DateTime.Parse("2011-12-09T16:00:00.000-06:00");
            myMaximumPrice1.price = new decimal(1.08);
            myPTPObligation1.MaximumPrice = new MaximumPrice[2];
            myPTPObligation1.MaximumPrice[0] = myMaximumPrice1;

            // MaximumPrice myMaximumPrice2 = new MaximumPrice();
            myMaximumPrice2.startTime = DateTime.Parse("2011-12-09T16:00:00.000-06:00");
            myMaximumPrice2.endTime = DateTime.Parse("2011-12-09T17:00:00.000-06:00");
            myMaximumPrice2.price = new decimal(1.08);
            myPTPObligation1.MaximumPrice[1] = myMaximumPrice2;


            BidSet myBidset = new BidSet();
            myBidset.tradingDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");


            myBidset.Items = new Object[2];
            myBidset.BidsetItemsElementName = new BidsetItemsChoiceType[2];
            myBidset.Items[0] = myPTPObligation;
            // for each bid type object included in Bidset, have to name the object type in the BidsetItemsChoiceType array.
            myBidset.BidsetItemsElementName[0] = BidsetItemsChoiceType.PTPObligation;
            myBidset.Items[1] = myPTPObligation1;
            myBidset.BidsetItemsElementName[1] = BidsetItemsChoiceType.PTPObligation;
            string bidSetXml = null;
            XmlDocument xmldoc = new XmlDocument();

            try
            {
                XmlSerializer payloadSerializer = new XmlSerializer(typeof(ews_2007_06.BidSet));


                XmlQualifiedName qn = new XmlQualifiedName("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces(new XmlQualifiedName[] { qn });
                // XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                // ns.Add(String.Empty,String.Empty); 
                // ns.Add("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                bidSetXml = XmlUtil.Serialize(payloadSerializer, Encoding.ASCII, ns, true, myBidset);
                // bidSetXml = bidSetXml.Replace("<BidSet>", "<BidSet xmlns=\"http://www.ercot.com/schema/2007-06/nodal/ews\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.ercot.com/schema/2007-06/nodal/ews ErcotTransactions.xsd\">");
                bidSetXml = bidSetXml.Replace("<BidSet>", "<BidSet xmlns=\"http://www.ercot.com/schema/2007-06/nodal/ews\">");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("foo");
            }

            XmlDocument doc = new XmlDocument();
            // doc.LoadXml(bidSetXml);
            XElement xelem = XElement.Parse(bidSetXml, LoadOptions.SetBaseUri);
            doc.Load(xelem.CreateReader());
            RequestMessage myRequestMessage = new RequestMessage();
            myRequestMessage.Payload = new PayloadType();
            myRequestMessage.Payload.Any = new XmlElement[] { doc.DocumentElement };
            // myRequestMessage.Payload.Any = new XmlElement[1];
            // myRequestMessage.Payload.Any[0] = doc.DocumentElement;
            try
            {
                //XmlSerializer serializer = new XmlSerializer(typeof(ews_2007_06.BidSet)); //, new Type[] { typeof(PTPObligation), typeof(Bid), typeof(TmSchedule),typeof(TmPoint) });
                XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage)); //, new Type[] { typeof(BidSet), typeof(PTPObligation), typeof(Bid), typeof(TmSchedule), typeof(TmPoint) });
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, myRequestMessage);
                System.Diagnostics.Debug.WriteLine(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("foo");
            }

            // set up the service client message objects
            HeaderType myHeader = new HeaderType();
            ReplayDetectionType myReplayDetection = new ReplayDetectionType();
            RequestType myRequest = new RequestType();

            myHeader.Verb = HeaderTypeVerb.create;
            myHeader.Noun = "BidSet";

            myReplayDetection.Nonce = new EncodedString();
            myReplayDetection.Nonce.Value = Convert.ToString(Guid.NewGuid());
            myReplayDetection.Created = new AttributedDateTime();
            myReplayDetection.Created.Value = Convert.ToString(String.Format("{0:s}", DateTime.Now));
            myHeader.ReplayDetection = myReplayDetection;

            myHeader.Revision = "1.0";
            myHeader.Source = "QWOAKS"; //QENJRE
            myHeader.UserID = "API_mhancock2022032";
            myHeader.MessageID = "231232456";
            myRequestMessage.Header = myHeader;

            try
            {
                //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(BidSet), new Type[]{typeof(PTPObligation)});
                //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(RequestMessage), new Type[] { typeof(PayloadType) });
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                // ns.Add("", "");
                string xml = XmlUtil.Serialize(x, Encoding.ASCII, ns, true, myRequestMessage);
                System.Diagnostics.Debug.Write(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("foo");
            }
            return myRequestMessage;
        }

        public string Upload1(string Userpath, string password, string serverPath) // todo: can we delete this unused method?
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
                CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
                ErcotNodalClient ercotClient = new ErcotPTPSubmissionLibrary.ErcotNodalClient();
                OperationsClient operationsClient = ercotClient.CreateErcotOperationsClient(myClientRequestBinding, 0, Userpath, password, serverPath);
                RequestMessage reqMessage = CreateRequestMessage1();
                ResponseMessage respMessage = new ResponseMessage();

                try
                {
                    //XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage), new Type[] { typeof(BidSet), typeof(PTPObligation), typeof(Bid), typeof(TmSchedule), typeof(TmPoint) });
                    XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    // ns.Add("", "");
                    string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, reqMessage);
                    System.Diagnostics.Debug.WriteLine(xml);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("foo");
                }
                // respMessage = operationsClient.MarketInfo(reqMessage);
                respMessage = operationsClient.MarketTransactions(reqMessage);
                System.Diagnostics.Debug.Write(respMessage.Header.ToString());
                System.Diagnostics.Debug.Write(" ");
                System.Diagnostics.Debug.Write(respMessage.Reply.ToString());
                return respMessage.Payload.Any[0].InnerXml;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }


        public Vayu.ErcotPTPSubmissionLibrary.ERCOTNodalService.RequestMessage CreateRequestMessage(PTPBid[] bids, int submittype,string username)
        {
            // set up the  Request body objects
            int totalbids = bids.Length;
            //       DateTime tdate = DateTime.Today.AddDays(1); // todo: which trading date should be specified in the submit sheet (in a config tab) rather than hard coded.  How does a trader submit 2 days in advance?
            DateTime tdate = DateTime.Today.AddDays(1);
            BidSet myBidset = new BidSet();
            myBidset.tradingDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"); // todo: which trading date should be specified in the submit sheet (in a config tab) rather than hard coded.  How does a trader submit 2 days in advance?
            myBidset.Items = new Object[totalbids];
            myBidset.BidsetItemsElementName = new BidsetItemsChoiceType[totalbids];

            PTPObligation[] myPTPObligation = new PTPObligation[totalbids];
            for (int i = 0; i < totalbids; i++)
            {
                myPTPObligation[i] = new PTPObligation();

                // myPTPObligation[i].startTime = DateTime.Parse(tdate.ToString("yyyy-MM-ddT00:00:00-06:00"));
                myPTPObligation[i].startTime = DateTime.Parse(FormattedDate(tdate));
                myPTPObligation[i].startTimeSpecified = true;
                // myPTPObligation[i].endTime = DateTime.Parse(tdate.AddDays(1).ToString("yyyy-MM-ddT00:00:00-06:00"));
                myPTPObligation[i].endTime = DateTime.Parse(FormattedDate(tdate.AddDays(1)));
                myPTPObligation[i].endTimeSpecified = true;
                myPTPObligation[i].marketType = "DAM";
                myPTPObligation[i].source = bids[i].Source;
                myPTPObligation[i].sink = bids[i].Sink;
                myPTPObligation[i].bidId = bids[i].BidId;

                myPTPObligation[i].CapacitySchedule = new TmSchedule();

                int totalhours = bids[i].Bidvals.Length;

                myPTPObligation[i].CapacitySchedule.TmPoint = new TmPoint[totalhours];
                myPTPObligation[i].MaximumPrice = new MaximumPrice[totalhours];

                //  for (int j = 0; j < totalhours; j++)
                double mw = 0;
                for (int j = 0; j < 1; j++)
                {
                    TmPoint myTmPoint = new TmPoint();
                    // myTmPoint.time = DateTime.Parse(tdate.AddHours(bids[i].Bidvals[j].Hour - 1).ToString("yyyy-MM-ddTHH:mm:ss-06:00"));
                    // myTmPoint.ending = DateTime.Parse(tdate.AddHours(bids[i].Bidvals[j].Hour).ToString("yyyy-MM-ddTHH:mm:ss-06:00"));
                    myTmPoint.time = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour - 1)));
                    myTmPoint.ending = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour)));
                    myTmPoint.endingSpecified = true;
                    if (bids[i].Bidvals[j].MW < 1)
                        mw = 1;
                    else
                        mw = bids[i].Bidvals[j].MW;
                    myTmPoint.value1 = new decimal(mw);
                    myTmPoint.value1Specified = true;
                    myPTPObligation[i].CapacitySchedule.TmPoint[j] = myTmPoint;

                    MaximumPrice myMaximumPrice = new MaximumPrice();
                    // myMaximumPrice.startTime = DateTime.Parse(tdate.AddHours(bids[i].Bidvals[j].Hour - 1).ToString("yyyy-MM-ddTHH:mm:ss-06:00"));
                    // myMaximumPrice.endTime = DateTime.Parse(tdate.AddHours(bids[i].Bidvals[j].Hour).ToString("yyyy-MM-ddTHH:mm:ss-06:00"));
                    myMaximumPrice.startTime = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour - 1)));
                    myMaximumPrice.endTime = DateTime.Parse(FormattedDate(tdate.AddHours(bids[i].Bidvals[j].Hour)));
                    myMaximumPrice.price = new decimal(bids[i].Bidvals[j].Price);
                    myPTPObligation[i].MaximumPrice[j] = myMaximumPrice;
                }
                myBidset.Items[i] = myPTPObligation[i];
                myBidset.BidsetItemsElementName[i] = BidsetItemsChoiceType.PTPObligation;
            }

            string bidSetXml = null;
            XmlDocument xmldoc = new XmlDocument();

            try
            {
                XmlSerializer payloadSerializer = new XmlSerializer(typeof(ews_2007_06.BidSet));

                XmlQualifiedName qn = new XmlQualifiedName("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces(new XmlQualifiedName[] { qn });
                // XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                // ns.Add(String.Empty,String.Empty); 
                // ns.Add("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                bidSetXml = XmlUtil.Serialize(payloadSerializer, Encoding.ASCII, ns, true, myBidset);
                // bidSetXml = bidSetXml.Replace("<BidSet>", "<BidSet xmlns=\"http://www.ercot.com/schema/2007-06/nodal/ews\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.ercot.com/schema/2007-06/nodal/ews ErcotTransactions.xsd\">");
                bidSetXml = bidSetXml.Replace("<BidSet>", "<BidSet xmlns=\"http://www.ercot.com/schema/2007-06/nodal/ews\">");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            XmlDocument doc = new XmlDocument();
            // doc.LoadXml(bidSetXml);
            XElement xelem = XElement.Parse(bidSetXml, LoadOptions.SetBaseUri);
            doc.Load(xelem.CreateReader());
            RequestMessage myRequestMessage = new RequestMessage();
            myRequestMessage.Payload = new PayloadType();
            myRequestMessage.Payload.Any = new XmlElement[] { doc.DocumentElement };
            // myRequestMessage.Payload.Any = new XmlElement[1];
            // myRequestMessage.Payload.Any[0] = doc.DocumentElement;
            try
            {
                //XmlSerializer serializer = new XmlSerializer(typeof(ews_2007_06.BidSet)); //, new Type[] { typeof(PTPObligation), typeof(Bid), typeof(TmSchedule),typeof(TmPoint) });
                XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage)); //, new Type[] { typeof(BidSet), typeof(PTPObligation), typeof(Bid), typeof(TmSchedule), typeof(TmPoint) });
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, myRequestMessage);
                System.Diagnostics.Debug.WriteLine(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // set up the service client message objects
            HeaderType myHeader = new HeaderType();
            ReplayDetectionType myReplayDetection = new ReplayDetectionType();
            RequestType myRequest = new RequestType();

            myHeader.Verb = HeaderTypeVerb.create;
            myHeader.Noun = "BidSet";

            myReplayDetection.Nonce = new EncodedString();
            myReplayDetection.Nonce.Value = Convert.ToString(Guid.NewGuid());
            myReplayDetection.Created = new AttributedDateTime();
            myReplayDetection.Created.Value = Convert.ToString(String.Format("{0:s}", DateTime.Now));
            myHeader.ReplayDetection = myReplayDetection;

            myHeader.Revision = "1.0";
            if (submittype == 0)
            {
                myHeader.Source = "QENJRE";

                #region comment
                //myHeader.Source = "QSE";
                // myHeader.UserID = "API_12222017MHANCOC";
                #endregion

#if test
                myHeader.UserID = "API_04122019PIYUSHD";
#else
                //myHeader.UserID = "API_mhancock2022032";
                myHeader.UserID = username;
#endif

            }

            #region comment

            //if (submittype == 0)
            //{
            //    //myHeader.Source = "QWOAKS";
            //    //myHeader.UserID = "API_WOQSE";
            //}
            //else if (submittype == 1)  // used to be submittype = 1 was QWOAK2 until 1/30/2011
            //{
            //    myHeader.Source = "QWOAK1";
            //    myHeader.UserID = "API_QWOAK1";
            //}
            //else if (submittype == 2)
            //{
            //    myHeader.Source = "QWOAK2";
            //    myHeader.UserID = "API_QWOAK2";
            //}
            //else if (submittype == 3)
            //{
            //    myHeader.Source = "QWOAK3";
            //    myHeader.UserID = "API_QWOAK3";
            //}
            //else if (submittype == 4)
            //{
            //    myHeader.Source = "QWOAK4";
            //    myHeader.UserID = "API_QWOAK4";
            //}
            //else if (submittype == 5) // new Cobalt certs
            //{
            //    myHeader.Source = "QCOBAL";
            //    myHeader.UserID = "API_QCOBAL";
            //}
            //else if (submittype == 6)
            //{
            //    myHeader.Source = "QCOBA1";
            //    myHeader.UserID = "API_QCOBA1";
            //}
            //else if (submittype == 7)
            //{
            //    myHeader.Source = "QCOBA2";
            //    myHeader.UserID = "API_QCOBA2";
            //}
            //else if (submittype == 8)
            //{
            //    myHeader.Source = "QCOBA3";
            //    myHeader.UserID = "API_QCOBA3";
            //}
            //else if (submittype == 9)
            //{
            //    myHeader.Source = "QCOBA4";
            //    myHeader.UserID = "API_QCOBA4";
            //}

            #endregion

            myHeader.MessageID = DateTime.Now.ToString("MMddHHmmss");
            myRequestMessage.Header = myHeader;

            try
            {
                //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(BidSet), new Type[]{typeof(PTPObligation)});
                //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(RequestMessage), new Type[] { typeof(PayloadType) });
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                // ns.Add("", "");
                string xml = XmlUtil.Serialize(x, Encoding.ASCII, ns, true, myRequestMessage);
                System.Diagnostics.Debug.Write(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
            }
            return myRequestMessage;
        }

        public RequestMessage CreateCancelRequestMessage(PTPBid[] bids, int submittype)
        {
            // set up the  Request body objects
            int totalbids = bids.Length;
            DateTime tdate = DateTime.Today.AddDays(1); // todo: which trading date should be specified in the submit sheet (in a config tab) rather than hard coded.  How does a trader submit 2 days in advance?

            string[] idArr = new string[totalbids];
            try
            {
                for (int i = 0; i < totalbids; i++)
                {
                    idArr[i] = bids[i].RequestID;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string bidSetXml = null;
            XmlDocument xmldoc = new XmlDocument();

            try
            {
                XmlSerializer payloadSerializer = new XmlSerializer(typeof(ews_2007_06.BidSet));

                XmlQualifiedName qn = new XmlQualifiedName("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces(new XmlQualifiedName[] { qn });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            XmlDocument doc = new XmlDocument();

            RequestMessage myRequestMessage = new RequestMessage();
            myRequestMessage.Payload = new PayloadType();
            myRequestMessage.Payload.Any = new XmlElement[] { doc.DocumentElement };

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage)); //, new Type[] { typeof(BidSet), typeof(PTPObligation), typeof(Bid), typeof(TmSchedule), typeof(TmPoint) });
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "http://www.ercot.com/schema/2007-06/nodal/ews");
                string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, myRequestMessage);
                System.Diagnostics.Debug.WriteLine(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // set up the service client message objects
            HeaderType myHeader = new HeaderType();
            ReplayDetectionType myReplayDetection = new ReplayDetectionType();
            RequestType myRequest = new RequestType();

            myHeader.Verb = HeaderTypeVerb.cancel;
            myHeader.Noun = "BidSet";

            myReplayDetection.Nonce = new EncodedString();
            myReplayDetection.Nonce.Value = Convert.ToString(Guid.NewGuid());
            myReplayDetection.Created = new AttributedDateTime();
            myReplayDetection.Created.Value = Convert.ToString(String.Format("{0:s}", DateTime.Now));
            myHeader.ReplayDetection = myReplayDetection;

            myHeader.Revision = "1.0";
            if (submittype == 0)
            {
                myHeader.Source = "QENJRE";

#if test
                myHeader.UserID = "API_04122019PIYUSHD";
#else
                myHeader.UserID = "API_03172023MHANCOC";
#endif
            }

            myHeader.MessageID = DateTime.Now.ToString("MMddHHmmss");
            myRequestMessage.Header = myHeader;
            myRequest.ID = idArr;
            myRequestMessage.Request = myRequest;

            try
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                string xml = XmlUtil.Serialize(x, Encoding.ASCII, ns, true, myRequestMessage);
                System.Diagnostics.Debug.Write(xml);
                System.Diagnostics.Debug.WriteLine(null);
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
            }
            return myRequestMessage;
        }

        public string Upload(PTPBid[] bids, int submittype,string Userpath,string password,string serverPath,string username)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
                CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
                ErcotNodalClient ercotClient = new ErcotPTPSubmissionLibrary.ErcotNodalClient();

#if test
                OperationsClient operationsClient = ercotClient.CreateErcotOperationsClientTest(myClientRequestBinding, submittype);
#else
                OperationsClient operationsClient = ercotClient.CreateErcotOperationsClient(myClientRequestBinding, submittype,Userpath,password,serverPath);
#endif

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                Console.WriteLine(ServicePointManager.SecurityProtocol.ToString());

                RequestMessage reqMessage = CreateRequestMessage(bids, submittype, username);
                ResponseMessage respMessage = new ResponseMessage();
                
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, reqMessage);
                    File.WriteAllText("Test.xml", xml);
                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine(ex.Message);
                    Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
                }
                respMessage = operationsClient.MarketTransactions(reqMessage);

                if (respMessage.Reply.ReplyCode == "OK")
                {
                    return "Success";
                }
                else
                {
                    return "Error";
                }

                //System.Diagnostics.Debug.Write(respMessage.Header.ToString());
                //System.Diagnostics.Debug.Write(" ");
                //System.Diagnostics.Debug.Write(respMessage.Reply.ToString());
                //return respMessage.Payload.Any[0].InnerXml;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
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

        public string Cancel(PTPBid[] bids, int submittype, string Userpath, string password, string serverPath)
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
            AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
            CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
            ErcotNodalClient ercotClient = new ErcotPTPSubmissionLibrary.ErcotNodalClient();

#if test
            OperationsClient operationsClient = ercotClient.CreateErcotOperationsClientTest(myClientRequestBinding, submittype);
#else
            OperationsClient operationsClient = ercotClient.CreateErcotOperationsClient(myClientRequestBinding, submittype, Userpath, password, serverPath);
#endif

            RequestMessage reqMessage = CreateCancelRequestMessage(bids, submittype);
            ResponseMessage respMessage = new ResponseMessage();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RequestMessage));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                string xml = XmlUtil.Serialize(serializer, Encoding.ASCII, ns, true, reqMessage);
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

            if (respMessage.Reply.ReplyCode == "OK")
            {
                return "Success";
            }
            else
            {
                return "Error";
            }

            // System.Diagnostics.Debug.Write(respMessage.Header.ToString());
            // System.Diagnostics.Debug.Write(" ");
            // System.Diagnostics.Debug.Write(respMessage.Reply.ToString());
            //return respMessage.Payload.Items;
            // respMessage.Payload
        }
    }
}
