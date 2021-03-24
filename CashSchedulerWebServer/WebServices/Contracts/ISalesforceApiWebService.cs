using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.WebServices.Contracts
{
    public interface ISalesforceApiWebService
    {
        Task<string> Login();

        Task<string> Login(
            string clientId,
            string clientSecret,
            string username,
            string password,
            string securityToken
        );

        Task UpsertSObject(SfObject sObject);

        Task UpsertSObjects(List<SfObject> sObjects);

        Task DeleteSObject(SfObject sObject);

        Task DeleteSObjects(List<SfObject> sObjects);

        void RunWithDelay(SfObject sObject, int delay, Action<ISalesforceApiWebService, SfObject> action);

        Task CreateCase(SfCase sObject);
    }
}
