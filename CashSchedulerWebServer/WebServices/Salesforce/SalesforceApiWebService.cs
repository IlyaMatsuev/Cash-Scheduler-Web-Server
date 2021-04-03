using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;
using CashSchedulerWebServer.WebServices.Salesforce.Wrappers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Tiny.RestClient;

namespace CashSchedulerWebServer.WebServices.Salesforce
{
    public class SalesforceApiWebService : ISalesforceApiWebService
    {
        private int MaxRecordsPerRequest => 200;
        private string IdField => "CashSchedulerId__c";
        private string ApiVersion => Configuration["WebServices:SalesforceApi:Version"];
        private string SObjectsEndpoint => $"services/data/{ApiVersion}/sobjects";
        private string SObjectsCompositeEndpoint => $"services/data/{ApiVersion}/composite/sobjects";
        private string SObjectsCustomWebServiceEndpoint => "services/apexrest/CashSchedulerWebService";
        private string WebToCaseEndpoint => "servlet/servlet.WebToCase";

        private TinyRestClient Client { get; }

        private IConfiguration Configuration { get; }

        public SalesforceApiWebService(IConfiguration configuration)
        {
            Configuration = configuration;
            Client = new TinyRestClient(new HttpClient(), Configuration["WebServices:SalesforceApi:Endpoint"]);
            ConfigureClient();
        }


        public Task<string> Login()
        {
            return Login(
                Configuration["WebServices:SalesforceApi:ClientId"],
                Configuration["WebServices:SalesforceApi:ClientSecret"],
                Configuration["WebServices:SalesforceApi:Username"],
                Configuration["WebServices:SalesforceApi:Password"],
                Configuration["WebServices:SalesforceApi:SecurityToken"]
            );
        }

        public async Task<string> Login(
            string clientId,
            string clientSecret,
            string username,
            string password,
            string securityToken)
        {
            var response = await Client.PostRequest("services/oauth2/token")
                .AddFormParameters(new Dictionary<string, string>
                {
                    {"grant_type", "password"},
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                    {"username", username},
                    {"password", password + securityToken}
                })
                .ExecuteAsync<Dictionary<string, string>>();
                return response["access_token"];
        }

        public async Task UpsertSObject(SfObject sObject)
        {
            string accessToken = await Login();
            var response = await Client.PatchRequest($"{SObjectsEndpoint}/{sObject.SObjectTypeName}/{IdField}/{sObject.Id}", sObject)
                .WithOAuthBearer(accessToken)
                .ExecuteAsHttpResponseMessageAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error during upserting an object: " + await response.Content.ReadAsStringAsync());
            }
        }

        public async Task UpsertSObjects(List<SfObject> sObjects)
        {
            string accessToken = await Login();
            List<List<SfObject>> chunks = sObjects
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / MaxRecordsPerRequest)
                .Select(x => x.Select(v => v.Value).ToList()).ToList();

            foreach (var chunk in chunks)
            {
                var response = await Client.PatchRequest(
                        $"{SObjectsCompositeEndpoint}/{sObjects.First().SObjectTypeName}/{IdField}",
                        new SfUpsertRecordListRequest<SfObject>(chunk)
                    )
                    .WithOAuthBearer(accessToken)
                    .ExecuteAsHttpResponseMessageAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error during upserting an object: " + await response.Content.ReadAsStringAsync());
                }
            }
        }

        public async Task DeleteSObject(SfObject sObject)
        {
            string accessToken = await Login();
            var response = await Client.DeleteRequest($"{SObjectsEndpoint}/{sObject.SObjectTypeName}/{IdField}/{sObject.Id}")
                .WithOAuthBearer(accessToken)
                .ExecuteAsHttpResponseMessageAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error during deleting an object: " + await response.Content.ReadAsStringAsync());
            }
        }

        public async Task DeleteSObjects(List<SfObject> sObjects)
        {
            string accessToken = await Login();
            var response = await Client.DeleteRequest($"{SObjectsCustomWebServiceEndpoint}")
                .AddQueryParameter("sobjecttypename", sObjects.First().SObjectTypeName)
                .AddQueryParameter("ids", string.Join(',', sObjects.Select(o => o.CashSchedulerId__c).ToList()))
                .WithOAuthBearer(accessToken)
                .ExecuteAsHttpResponseMessageAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error during deleting an object: " + await response.Content.ReadAsStringAsync());
            }
        }

        public void RunWithDelay(SfObject sObject, int delay, Action<ISalesforceApiWebService, SfObject> action)
        {
            Task.Delay(delay * 1000).ContinueWith(_ => action(this, sObject));
        }

        public Task CreateCase(SfCase sObject)
        {
            return Client.PostRequest(WebToCaseEndpoint)
                .AddFormParameters(new KeyValuePair<string, string>[]
                {
                    new ("encoding", "UTF-8"),
                    new ("orgid", Configuration["WebServices:SalesforceApi:OrgId"]),
                    new ("name", sObject.ContactName),
                    new ("email", sObject.Email),
                    new ("phone", sObject.Phone),
                    new ("subject", sObject.Subject),
                    new ("description", sObject.Description)
                })
                .ExecuteAsync();
        }


        private void ConfigureClient()
        {
            JsonFormatter defaultFormatter = (JsonFormatter) Client.Settings.Formatters.Default;
            defaultFormatter.JsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            defaultFormatter.JsonSerializer.DateFormatString = "yyyy-MM-dd";
        }
    }
}
