using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using Vayu.CRRNodalServiceLibrary.ERCOTNodalService;

namespace Vayu.CRRNodalServiceLibrary
{
    public class ErcotNodalClient
    {
        public ErcotNodalClient()
        {
        }

        public ERCOTNodalService.OperationsClient CreateErcotOperationsClient(CustomBinding myBinding, int submittype)
        {
            EndpointAddress epa = new EndpointAddress(new Uri("https://misapi.ercot.com/2007-08/Nodal/eEDS/EWS/"), EndpointIdentity.CreateDnsIdentity("misapi.ercot.com"));
            ERCOTNodalService.OperationsClient operationsClient = new ERCOTNodalService.OperationsClient(myBinding, epa);
            operationsClient.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;
            string serverCertFileName = string.Empty;
            String serverSigningCertFingerprint = string.Empty;
            String clientSigningCertFingerprint = "";
            try
            {
                if (submittype == 0)
                {
                    X509Certificate2 clientCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                    
                    //prod
                    string clientCertFileName = @"C:\ErcotTrader\CRR\Prod Digital\0809369485000$API_20200821RPALAVE.pfx";
                    string clientCertPassword = "VS4rd!g65pA0t";

                    clientCertFromFile.Import(clientCertFileName, clientCertPassword, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
                    clientSigningCertFingerprint = clientCertFromFile.Thumbprint;
                }

                clientSigningCertFingerprint = clientSigningCertFingerprint.ToUpper().Replace(" ", "");
                #region SerCertificate
                X509Certificate2 serverCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();

                //prod
                serverCertFileName = @"C:\ErcotTrader\ERCOT_NEW_API_public_key\misapi.cer";
                serverCertFromFile.Import(serverCertFileName);
                serverSigningCertFingerprint = serverCertFromFile.SerialNumber;
                #endregion
            }
            catch (Exception ex)
            {
            }
            serverSigningCertFingerprint = serverSigningCertFingerprint.ToUpper().Replace(" ", "");
            operationsClient.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, clientSigningCertFingerprint);
            operationsClient.ClientCredentials.ServiceCertificate.SetDefaultCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySerialNumber, serverSigningCertFingerprint);
            operationsClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;
            //
            return operationsClient;
        }
        public ERCOTNodalService.OperationsClient CreateErcotOperationsClientTest(CustomBinding myBinding, int submittype)
        {
            EndpointAddress epa = new EndpointAddress(new Uri("https://testmisapi.ercot.com/2007-08/Nodal/eEDS/EWS/"), EndpointIdentity.CreateDnsIdentity("testmisapi.ercot.com"));
            ERCOTNodalService.OperationsClient operationsClient = new ERCOTNodalService.OperationsClient(myBinding, epa);
            operationsClient.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;
            string serverCertFileName = string.Empty;
            String serverSigningCertFingerprint = string.Empty;
            String clientSigningCertFingerprint = "";
            try
            {
                if (submittype == 0)
                {
                    X509Certificate2 clientCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();

                    // test
                    string clientCertFileName = @"C:\ErcotTrader\Rajkumar\CRR\0809369485000$API_20200609rpalave.pfx";
                    string clientCertPassword = "$5MNy_poxS7Ty";

                    clientCertFromFile.Import(clientCertFileName, clientCertPassword, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
                    clientSigningCertFingerprint = clientCertFromFile.Thumbprint;
                }

                clientSigningCertFingerprint = clientSigningCertFingerprint.ToUpper().Replace(" ", "");
                #region SerCertificate
                X509Certificate2 serverCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();

                //prod
                //serverCertFileName = @"C:\ErcotTrader\ERCOT_NEW_API_public_key\misapi.cer";

                //test
                //serverCertFileName = @"C:\ErcotTrader\2019 Test Env\testmisapi.cer";

                serverCertFileName = @"C:\ErcotTrader\CRR\Test Mote\testmisapi.cer";
                serverCertFromFile.Import(serverCertFileName);
                serverSigningCertFingerprint = serverCertFromFile.SerialNumber;
                #endregion
            }
            catch (Exception ex)
            {
            }
            serverSigningCertFingerprint = serverSigningCertFingerprint.ToUpper().Replace(" ", "");
            operationsClient.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, clientSigningCertFingerprint);
            operationsClient.ClientCredentials.ServiceCertificate.SetDefaultCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySerialNumber, serverSigningCertFingerprint);
            operationsClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;
            //
            return operationsClient;
        }
    }
}
