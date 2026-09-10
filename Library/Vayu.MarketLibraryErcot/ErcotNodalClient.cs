using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Vayu.MarketLibraryErcot.ERCOTNodalService;
using System.Net;

namespace Vayu.MarketLibraryErcot
{
    public class ErcotNodalClient
    {
        public ErcotNodalClient()
        {
        }

        public ERCOTNodalService.OperationsClient CreateErcotOperationsClient(CustomBinding myBinding, int submittype)
        {
           // EndpointAddress epa = new EndpointAddress(new Uri("https://misapi.ercot.com/2007-08/Nodal/eEDS/EWS/"),  EndpointIdentity.CreateDnsIdentity("misapi.ercot.com"));
            EndpointAddress epa = new EndpointAddress(new Uri("https://misapi.ercot.com/NodalAPI/EWS/"),  EndpointIdentity.CreateDnsIdentity("misapi.ercot.com"));
            ERCOTNodalService.OperationsClient operationsClient = new ERCOTNodalService.OperationsClient(myBinding, epa);
            operationsClient.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;

            String clientSigningCertFingerprint = "";
            if (submittype == 0)
            {
                X509Certificate2 clientCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                string clientCertFileName = @"D:\VayuCertificate\Prod\API\1189331712000$API_03172023MHANCOC.pfx";
                string clientCertPassword = "Ol0hT$6fqOX_8";
                clientCertFromFile.Import(clientCertFileName, clientCertPassword, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
                clientSigningCertFingerprint = clientCertFromFile.Thumbprint;
            }


            clientSigningCertFingerprint = clientSigningCertFingerprint.ToUpper().Replace(" ", "");
            #region SerCertificate
            X509Certificate2 serverCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();
            string serverCertFileName = @"D:\VayuCertificate\Prod\ERCOT_CA.cer";
            // string clientCertPassword = "Taller123";
            serverCertFromFile.Import(serverCertFileName);
            String serverSigningCertFingerprint = serverCertFromFile.SerialNumber;
            #endregion
            serverSigningCertFingerprint = serverSigningCertFingerprint.ToUpper().Replace(" ", "");
            operationsClient.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, clientSigningCertFingerprint);
            operationsClient.ClientCredentials.ServiceCertificate.SetDefaultCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySerialNumber, serverSigningCertFingerprint);
            operationsClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;
            //
            return operationsClient;
        }

        public ERCOTNodalService.OperationsClient CreateErcotOperationsClient(CustomBinding myBinding, string clientCertFileName, string clientCertPassword, string serverCertFileName)
        {

            EndpointAddress epa = new EndpointAddress(new Uri("https://misapi.ercot.com/NodalAPI/EWS/"), EndpointIdentity.CreateDnsIdentity("misapi.ercot.com"));
            ERCOTNodalService.OperationsClient operationsClient = new ERCOTNodalService.OperationsClient(myBinding, epa);
            try
            {

                operationsClient.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;

                X509Certificate2 clientCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                clientCertFromFile.Import(clientCertFileName, clientCertPassword, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
                operationsClient.ClientCredentials.ClientCertificate.Certificate = clientCertFromFile;

                X509Certificate2 serverCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                serverCertFromFile.Import(serverCertFileName);
                operationsClient.ClientCredentials.ServiceCertificate.DefaultCertificate = serverCertFromFile;
                operationsClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;
                operationsClient.ClientCredentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return operationsClient;
        }

        public ERCOTNodalService.OperationsClient CreateErcotOperationsClientTest(CustomBinding myBinding, string clientCertFileName, string clientCertPassword, string serverCertFileName)
        {
            EndpointAddress epa = new EndpointAddress(new Uri("https://testmisapi.ercot.com/NodalAPI/EWS/"), EndpointIdentity.CreateDnsIdentity("testmisapi.ercot.com"));
            ERCOTNodalService.OperationsClient operationsClient = new ERCOTNodalService.OperationsClient(myBinding, epa);
            try
            {
                operationsClient.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;

                X509Certificate2 clientCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();

                //clientCertFileName = @"C:\ErcotTrader\2019 Test Env\ClientTestCertificate\ErcotClientTestCertificate.pfx";
                //clientCertPassword = "taller123";
                clientCertFileName = @"D:\VayuCertificate\Prod\API\1189331712000$API_03172023MHANCOC.pfx";
                clientCertPassword = "Ol0hT$6fqOX_8";

                clientCertFromFile.Import(clientCertFileName, clientCertPassword, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
                operationsClient.ClientCredentials.ClientCertificate.Certificate = clientCertFromFile;

                X509Certificate2 serverCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();

                //serverCertFileName = @"C:\ErcotTrader\2019 Test Env\testmisapi.cer";
                serverCertFileName = @"D:\VayuCertificate\Prod\misapi.cer";

                serverCertFromFile.Import(serverCertFileName);
                operationsClient.ClientCredentials.ServiceCertificate.DefaultCertificate = serverCertFromFile;
                operationsClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;
                operationsClient.ClientCredentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return operationsClient;
        }


        public HeaderType GetHeader(int submittype)
        {
            HeaderType myHeader = new HeaderType();
            
            //else if (submittype == 1)
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
            //else if (submittype == 5)
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
            return myHeader;
        }
    }
}
