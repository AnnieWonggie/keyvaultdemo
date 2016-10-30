using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace mykeyvault
{
    class Program
    {
        

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            //Get the storage key as a secret in KeyVault
            var storageKey = await GetStorageKey();
            string storageAccountName = ConfigurationManager.AppSettings["storageAccountName"];
            var creds = new StorageCredentials(storageAccountName, storageKey);
            var storageAccount = new CloudStorageAccount(creds, true);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("samplequeue");
            await queue.CreateIfNotExistsAsync();
            await queue.AddMessageAsync(new CloudQueueMessage("Hello keyvault"));
        }

        private static async Task<string> GetStorageKey()
        {

            var client = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(GetAccessTokenAsync),
                new System.Net.Http.HttpClient());

            var vaultUrl = ConfigurationManager.AppSettings["vaultUrl"];

            var secret = await client.GetSecretAsync(vaultUrl, "storageAccountKey");

            return secret.Value;
        }

        
        private static async Task<string> GetAccessTokenAsync(
            string authority, 
            string resource, 
            string scope)
        {
            //clientID and clientSecret are obtained by registering 
            //the application in Azure AD
            var clientId = ConfigurationManager.AppSettings["clientId"];
            var clientSecret = ConfigurationManager.AppSettings["clientSecret"];

            var clientCredential = new ClientCredential(
                clientId,
                clientSecret);

            var context = new AuthenticationContext(
                authority, 
                TokenCache.DefaultShared);

            var result = await context.AcquireTokenAsync(
                resource, 
                clientCredential);

            return result.AccessToken;
        }       
    }
}
