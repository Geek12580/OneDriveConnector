using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;
using NUnit.Framework;
using File = Microsoft.Graph.File;

namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            Console.WriteLine("dfdsfds");
            var scopes = new[] { "User.Read" };

// Multi-tenant apps can use "common",
// single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "common";

// Value from app registration
            var clientId = "a61b936f-0407-482f-bbaf-056bdfa89d63";

// using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

// Callback function that receives the user prompt
// Prompt contains the generated device code that you must
// enter during the auth process in the browser
            Func<DeviceCodeInfo, CancellationToken, Task> callback = (code, cancellation) => {
                Console.WriteLine(code.Message);
                return Task.FromResult(0);
            };

// https://learn.microsoft.com/dotnet/api/azure.identity.devicecodecredential
            var deviceCodeCredential = new DeviceCodeCredential(
                callback, tenantId, clientId, options);

            var graphClient = new GraphServiceClient(deviceCodeCredential, scopes);
            
            
            var drive = await graphClient.Me.Drive.Request().GetAsync();
            Assert.Pass();
        }

        [Test]
        public async Task Test2()
        {
            // The client credentials flow requires that you request the
// /.default scope, and preconfigure your permissions on the
// app registration in Azure. An administrator must grant consent
// to those permissions beforehand.
            var scopes = new[] { "https://graph.microsoft.com/.default" };

// Multi-tenant apps can use "common",
// single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "4909be41-dd2f-4054-8bd3-971f424ab277";

// Values from app registration
            var clientId = "a61b936f-0407-482f-bbaf-056bdfa89d63";
            var clientSecret = "AoS8Q~~b5XiO0G3bSvaMlGZlh5LXtAvsNKrLWbE1";

// using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

// https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
            
            Uri uri = new Uri("https://0qnyh-my.sharepoint.com/personal/shenwangzeng_0qnyh_onmicrosoft_com");
            var site = await graphClient.Sites.GetByPath(uri.AbsolutePath, uri.Host).Request().WithMaxRetry(3)
                .Select(c => c.Id).GetAsync().ConfigureAwait(false);
            var siteId = site.Id;
            
            ISiteDrivesCollectionPage drives = await graphClient.Sites[siteId].Drives
                .Request()
                .GetAsync();
            
            foreach (Drive drive1 in drives)
            {
                string driverId = drive1.Id;
                var driveItem = new DriveItem
                {
                    Name = "test9.csv",
                    File = new File(),
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"@microsoft.graph.conflictBehavior", "rename"}
                    }
                };

                await graphClient.Drives[driverId].Root.Children
                    .Request()
                    .AddAsync(driveItem);
                
                var children = await graphClient.Drives[driverId].Root.Children.Request().GetAsync();
                string driveItemId = children[0].Id;

                IDriveItemRequestBuilder driveItemRequestBuilder = graphClient.Drives[driverId].Root;
                // driveItemRequestBuilder = driveItemRequestBuilder.ItemWithPath("test/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/Sql.txt");
                driveItemRequestBuilder = driveItemRequestBuilder.ItemWithPath("test/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15");
                driveItem = await graphClient.Drives[driverId].Root.ItemWithPath("test/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15").Request().GetAsync();
                DriveItem driveItem1= await graphClient.Drives[driverId].Items[driveItem.Id].ItemWithPath("16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/Sql.txt").Request().GetAsync();
                driveItemRequestBuilder = driveItemRequestBuilder.ItemWithPath("16/17/18/19/20/21/22/23/24/25/26/27/28/29/30");

                // children = await driveItemRequestBuilder.Children.Request().GetAsync();
                var folder = await driveItemRequestBuilder.Request().GetAsync();
                var folderId = folder.Id;
                Expression<Func<DriveItem, object>> selectExpression = c => new { c.Folder, c.Name, c.Size, c.LastModifiedDateTime };
                IDriveItemChildrenCollectionPage driveItemChildrenCollectionPage = null;
                driveItemRequestBuilder = graphClient.Drives[driverId].Items[folderId];
                while (true)
                {
                    if (driveItemChildrenCollectionPage == null)
                    {
                        driveItemChildrenCollectionPage = driveItemRequestBuilder.Children.Request().WithMaxRetry(3).
                            Select(selectExpression).GetAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    else
                    {
                        driveItemChildrenCollectionPage = driveItemRequestBuilder.Children
                            .Request(driveItemChildrenCollectionPage.NextPageRequest.QueryOptions).WithMaxRetry(3).GetAsync().
                            ConfigureAwait(false).GetAwaiter().GetResult(); ;
                    }
                    if (driveItemChildrenCollectionPage.NextPageRequest == null)
                    {
                        break;
                    }
                }
                
                // children = await driveItemRequestBuilder.Children.Request().GetAsync();
                
                driveItemRequestBuilder = driveItemRequestBuilder.ItemWithPath("17/18/19/20/21/22/23/24");
                driveItemRequestBuilder = driveItemRequestBuilder.ItemWithPath("25/26/27/28/29/30/Sql.txt");
                var test = await driveItemRequestBuilder.Content.Request().GetAsync();
                StreamReader reader12 = new StreamReader(test);
                string text12 = reader12.ReadToEnd();
                
                
                children = await graphClient.Drives[driverId].Items[driveItemId].Children.Request().GetAsync();
                
                await ReadSharedFiles(graphClient, driverId);
                // await ReadRecentFiles(graphClient, id);
                await ReadFollowingFiles(graphClient, driverId);

                await graphClient.Drives[driverId].Items[driveItemId].CreateUploadSession().Request(new List<Option>() { new HeaderOption("@microsoft.graph.conflictBehavior","replace") }).PostAsync();
                
                using Stream stream1 = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(@"The contents of the file goes here."));
                await graphClient.Drives[driverId].Items[driveItemId].Content
                    .Request()
                    .PutAsync<DriveItem>(stream1);
                
                Stream stream = await graphClient.Drives[driverId].Items[driveItemId].Content.Request().WithMaxRetry(3).GetAsync(completionOption: HttpCompletionOption.ResponseHeadersRead);
                StreamReader reader = new StreamReader(stream);
                string text = reader.ReadToEnd();
                // await graphClient.Drives[id].Items[driveItemId].Request().DeleteAsync();
                Assert.Pass();
            }
            // GraphServiceClient graphClient = new GraphServiceClient( authProvider );
         
            Assert.Pass();
        }

        private static async Task ReadSharedFiles(GraphServiceClient graphClient, string id)
        {
            var shared = await graphClient.Drives[id].SharedWithMe().Request().GetAsync();
            string sharedItemId = shared[0].Id;
            Stream stream11 = await graphClient.Drives[id].Items[sharedItemId].Content.Request().WithMaxRetry(3)
                .GetAsync(completionOption: HttpCompletionOption.ResponseHeadersRead);
            StreamReader reader1 = new StreamReader(stream11);
            string text1 = reader1.ReadToEnd();
            Console.WriteLine();
        }

        private static async Task ReadRecentFiles(GraphServiceClient graphClient, string id)
        {
            var recent = await graphClient.Drives[id].Recent().Request().GetAsync();
            string recentItemId = recent[1].Id;
            Stream stream11 = await graphClient.Drives[id].Items[recentItemId].Content.Request().WithMaxRetry(3)
                .GetAsync(completionOption: HttpCompletionOption.ResponseHeadersRead);
            StreamReader reader1 = new StreamReader(stream11);
            string text1 = reader1.ReadToEnd();
            Console.WriteLine();
        }
        
        private static async Task ReadFollowingFiles(GraphServiceClient graphClient, string id)
        {
            var following = await graphClient.Drives[id].Following.Request().GetAsync();
            string recentItemId = following[0].Id;
            Stream stream11 = await graphClient.Drives[id].Items[recentItemId].Content.Request().WithMaxRetry(3)
                .GetAsync(completionOption: HttpCompletionOption.ResponseHeadersRead);
            StreamReader reader1 = new StreamReader(stream11);
            string text1 = reader1.ReadToEnd();
            Console.WriteLine();
        }
    }
}