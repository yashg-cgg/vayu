using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Channels;
//using System.ServiceModel.Security.Tokens;
//using System.Net.Security;
//using System.Net;
//using System.Security.Cryptography.X509Certificates;

namespace Vayu.ErcotPTPSubmissionLibrary
{
    public class AsymmetricCustomBinding
    {
        public AsymmetricCustomBinding()
        {
        }

        public CustomBinding CreateAsymmetricBinding()
        {
            CustomBinding myHttpBinding = new CustomBinding();
            AsymmetricSecurityBindingElement abe = (AsymmetricSecurityBindingElement)SecurityBindingElement.
                CreateMutualCertificateBindingElement(MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10);
            abe.EnableUnsecuredResponse = true;
            abe.IncludeTimestamp = false;
            abe.LocalServiceSettings.DetectReplays = false;
            abe.LocalClientSettings.DetectReplays = false;
            ((AsymmetricSecurityBindingElement)abe).AllowSerializedSigningTokenOnReply = true;
            myHttpBinding.Elements.Add(abe);

            TextMessageEncodingBindingElement element = new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);
            element.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            myHttpBinding.Elements.Add(element);

            HttpsTransportBindingElement httpsTransport = new HttpsTransportBindingElement();
            httpsTransport.MaxReceivedMessageSize = int.MaxValue;
            httpsTransport.MaxBufferSize = int.MaxValue;
            httpsTransport.MaxBufferPoolSize = int.MaxValue;
            httpsTransport.RequireClientCertificate = true;

            //httpsTransport.BypassProxyOnLocal = false; // these lines to use local proxy to capture xml on the wire
            //httpsTransport.UseDefaultWebProxy = true; // these lines
            //httpsTransport.ProxyAddress = new Uri("http://woakspc68:8888");
            //httpsTransport.RequireClientCertificate = true;
            myHttpBinding.Elements.Add(httpsTransport);
            return myHttpBinding;
        }


    }
}
