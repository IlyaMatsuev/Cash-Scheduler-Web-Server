using System.Threading.Tasks;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Utils;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.Services.Salesforce
{
    public class SalesforceService : ISalesforceService
    {
        private ISalesforceApiWebService SalesforceWebService { get; }

        public SalesforceService(ISalesforceApiWebService salesforceWebService)
        {
            SalesforceWebService = salesforceWebService;
        }


        public Task<BugReport> CreateCase(BugReport bugReport)
        {
            ModelValidator.ValidateModelAttributes(bugReport);
            SalesforceWebService.CreateCase(new SfCase(bugReport));
            return Task.FromResult(bugReport);
        }
    }
}
