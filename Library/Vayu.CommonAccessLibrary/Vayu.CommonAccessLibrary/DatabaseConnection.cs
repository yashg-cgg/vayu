using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Windows;

namespace Vayu.CommonAccessLibrary
{
    public class DatabaseConnection
    {
        //public static string VayuDbConnection()
        //{
        //    return "Data Source = ; Initial Catalog = Vayu; Persist Security Info = True; User Id = ; password = ; Connect Timeout = 100000; MultipleActiveResultSets=True";


        //}
        public static string GetConnectionStringFromAzure()
        {
            string ConnectionString = null;
            try
            {
                //To Authenticate Azure using Client ID and Client secret.
                // Authenticate using client ID and client secret
                string tenantId = "";
                string clientId = "";
                //string clientSecret = "";
                string clientSecret = "";

                var client = new SecretClient(new System.Uri("https://vyvault1.vault.azure.net/"), new ClientSecretCredential(tenantId, clientId, clientSecret));

                //Retrieve a secretName 
                string secretName = "DevUser";
                KeyVaultSecret secret = client.GetSecret(secretName);

                //Use the secret in you WPF application
                string secretValue = secret.Value;
                ConnectionString = "Data Source = ; Initial Catalog = Vayu; Persist Security Info = True; User Id =" + secretName + "; password = " + secretValue + "; Connect Timeout = 100000; MultipleActiveResultSets=True; Pooling=True";



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return ConnectionString;
        }
    }
}
