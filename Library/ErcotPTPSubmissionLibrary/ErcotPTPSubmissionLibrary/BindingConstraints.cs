using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;

namespace Vayu.ErcotPTPSubmissionLibrary
{
    public class BindingConstraints
    {
        public BindingConstraints()
        {
        }
        public ErcotPTPSubmissionLibrary.ERCOTNodalService.RequestMessage CreateRequestMessage(ErcotPTPSubmissionLibrary.ERCOTNodalService.RequestTypeMarketType myMarketType, DateTime operatingDate)
        {
            ErcotPTPSubmissionLibrary.ERCOTNodalService.HeaderType myHeader = new ErcotPTPSubmissionLibrary.ERCOTNodalService.HeaderType();
            ERCOTNodalService.RequestMessage myRequestMessage = new ERCOTNodalService.RequestMessage();
            ERCOTNodalService.ReplayDetectionType myReplayDetection = new ERCOTNodalService.ReplayDetectionType();
            ERCOTNodalService.RequestType myRequest = new ERCOTNodalService.RequestType();

            myHeader.Verb = ERCOTNodalService.HeaderTypeVerb.get;
            myHeader.Noun = "BindingConstraints";

            myReplayDetection.Nonce = new ERCOTNodalService.EncodedString();
            myReplayDetection.Nonce.Value = Convert.ToString(Guid.NewGuid());
            myReplayDetection.Created = new ERCOTNodalService.AttributedDateTime();
            myReplayDetection.Created.Value = Convert.ToString(String.Format("{0:s}", DateTime.Now));
            myHeader.ReplayDetection = myReplayDetection;

            myHeader.Revision = "1.0";
            myHeader.Source = "QWOAKS";
            myHeader.UserID = "API_mhancock2022032";
            myRequestMessage.Header = myHeader;

            // fill in the type of market and the date to get BindingConstraints
            myRequest.MarketType = myMarketType;
            myRequest.MarketTypeSpecified = true;
            myRequest.OperatingDate = operatingDate;
            myRequest.OperatingDateSpecified = true;

            myRequestMessage.Request = myRequest;
            return myRequestMessage;
        }
        public void Download()
        {
            try
            {
                /*  This callback handler instructs the current http connection context to accept any remote ssl certificate, to establish an https/ssl connection.
                 *  I had used an anoynmous method instead of a callback handler before, but it prevented Edit and Continue ability in Visual Studio when debugging, 
                 *  in case I wanted to change the output while debugging.
                 *  So I reverted the code back to a callback handler rather than an anonymous method.  -- CJ
                */
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

                ErcotPTPSubmissionLibrary.AsymmetricCustomBinding acb = new ErcotPTPSubmissionLibrary.AsymmetricCustomBinding();
                CustomBinding myBinding = acb.CreateAsymmetricBinding();

                ErcotPTPSubmissionLibrary.ErcotNodalClient ercotClient = new ErcotPTPSubmissionLibrary.ErcotNodalClient();
                ErcotPTPSubmissionLibrary.ERCOTNodalService.OperationsClient operationsClient = ercotClient.CreateErcotOperationsClient(myBinding, 0);

                ErcotPTPSubmissionLibrary.BindingConstraints bindingConstraints = new ErcotPTPSubmissionLibrary.BindingConstraints();
                ErcotPTPSubmissionLibrary.ERCOTNodalService.RequestMessage reqMessage = bindingConstraints.CreateRequestMessage(ErcotPTPSubmissionLibrary.ERCOTNodalService.RequestTypeMarketType.DAM, DateTime.Parse("2012-03-01T00:00:00-05:00"));
                ErcotPTPSubmissionLibrary.ERCOTNodalService.ResponseMessage respMessage = new ErcotPTPSubmissionLibrary.ERCOTNodalService.ResponseMessage();

                respMessage = operationsClient.MarketInfo(reqMessage);

                Console.WriteLine(String.Format("Header.Noun: {0}", respMessage.Header.Noun));
                Console.WriteLine(String.Format("ReplyCode: {0} ", respMessage.Reply.ReplyCode));
                if (respMessage.Reply.ReplyCode != "ERROR")
                {
                    //System.Diagnostics.Debug.WriteLine("replyCode: " + respMessage.Reply.ReplyCode + ", Payload.Any[0].OuterXml: " + respMessage.Payload.Any[0].OuterXml);
                }
                else
                {
                    // slightly easier error handling for debugging.
                    System.Diagnostics.Debug.WriteLine("replyCode: " + respMessage.Reply.ReplyCode + ", reason: " + respMessage.Reply.Error[0] + "");
                }
                string foo = "foo";
            }
            catch (Exception ex)
            {
                Console.WriteLine("foo");
            }
        }
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
