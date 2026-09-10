using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Vayu.ErcotPTPSubmissionLibrary.ERCOTNodalService;
using System.Security;

namespace Vayu.ErcotPTPSubmissionLibrary
{
    public class ErcotNodalClient
    {
        public ErcotNodalClient()
        {
        }

        public ERCOTNodalService.OperationsClient CreateErcotOperationsClient(CustomBinding myBinding, int submittype, string clientCertFileName="", string clientCertPassword="", string serverCertFileName="")
        { //change SP
            //EndpointAddress epa = new EndpointAddress(new Uri("https://misapi.ercot.com/2007-08/Nodal/eEDS/EWS/"), EndpointIdentity.CreateDnsIdentity("misapi.ercot.com"));
            EndpointAddress epa = new EndpointAddress(new Uri("https://testmisapi.ercot.com/NodalAPI/EWS/"), EndpointIdentity.CreateDnsIdentity("testmisapi.ercot.com"));
            ERCOTNodalService.OperationsClient operationsClient = new ERCOTNodalService.OperationsClient(myBinding, epa);
            operationsClient.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;
           // string serverCertFileName = string.Empty;
            String serverSigningCertFingerprint = string.Empty;
            String clientSigningCertFingerprint = "";
            try
            {
                if (submittype == 0)
                {
                     X509Certificate2 clientCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();  //SP Change

                    //prod
                     clientCertFileName = @"D:\VayuCertificate\Prod\APINEW\1189331712000$API_03172023MHANCOC.pfx";
                     clientCertPassword = "Wytq_#OIP!EMJ";

                     //clientCertFileName = @"D:\Q1Certificate\Digital\0809369482000$API_20200327mhancock.pfx";
                     //clientCertPassword = "kVmtS_cvFKXUZ";

                    //SP change
                    clientCertFromFile.Import(clientCertFileName, clientCertPassword, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
                    clientSigningCertFingerprint = clientCertFromFile.Thumbprint;
                }


                clientSigningCertFingerprint = clientSigningCertFingerprint.ToUpper().Replace(" ", "");
                #region SerCertificate
                X509Certificate2 serverCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();

                //prod
                serverCertFileName = @"D:\VayuCertificate\testmisapi_2025-2026\testmisapi.cer";
               // serverCertFileName = @"D:\Q1Certificate\ERCOT_NEW_API_public_key\misapi.cer";

                serverCertFromFile.Import(serverCertFileName);
                serverSigningCertFingerprint = serverCertFromFile.SerialNumber;
            }
            catch (Exception ex)
            {
            }

                #endregion
            serverSigningCertFingerprint = serverSigningCertFingerprint.ToUpper().Replace(" ", "");
            operationsClient.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, clientSigningCertFingerprint);
            Console.WriteLine("clientSigningCertFingerprint");
            operationsClient.ClientCredentials.ServiceCertificate.SetDefaultCertificate(StoreLocation.CurrentUser, StoreName.Root, X509FindType.FindBySerialNumber, serverSigningCertFingerprint);
            Console.WriteLine("serverSigningCertFingerprint");
            operationsClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;

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
                    string clientCertFileName = @"C:\ErcotTrader\2019 Test Env\ClientTestCertificate\ErcotClientTestCertificate.pfx";
                    string clientCertPassword = "taller123";

                    clientCertFromFile.Import(clientCertFileName, clientCertPassword, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
                    clientSigningCertFingerprint = clientCertFromFile.Thumbprint;
                }

                clientSigningCertFingerprint = clientSigningCertFingerprint.ToUpper().Replace(" ", "");
                #region SerCertificate
                X509Certificate2 serverCertFromFile = new System.Security.Cryptography.X509Certificates.X509Certificate2();

                //test
                serverCertFileName = @"C:\ErcotTrader\2019 Test Env\testmisapi.cer";

                // string clientCertPassword = "Taller123";
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

        public ERCOTNodalService.OperationsClient CreateErcotOperationsClient(CustomBinding myBinding, string clientCertFileName, string clientCertPassword, string serverCertFileName)
        {
            // Prod
            EndpointAddress epa = new EndpointAddress(new Uri("https://misapi.ercot.com/2007-08/Nodal/eEDS/EWS/"), EndpointIdentity.CreateDnsIdentity("misapi.ercot.com"));

            // Test
            //EndpointAddress epa = new EndpointAddress(new Uri("https://testmisapi.ercot.com/2007-08/Nodal/eEDS/EWS/"), EndpointIdentity.CreateDnsIdentity("testmisapi.ercot.com"));
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

        public HeaderType GetHeader(int submittype)
        {
            HeaderType myHeader = new HeaderType();
            if (submittype == 0)
            {
                myHeader.Source = "QTALLR";
                // myHeader.UserID = "API_12222017MHANCOC";
                //myHeader.Source = "QSE";
                myHeader.UserID = "API_mhancock2022032";
            }
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
