using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Salesforce
{
    [ExtendObjectType(Name = "Mutation")]
    public class SalesforceMutations
    {
        public Task<BugReport> ReportBug(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewBugReportInput bugReport)
        {
            return contextProvider.GetService<ISalesforceService>().CreateCase(new BugReport
            {
                Name = bugReport.Name,
                Email = bugReport.Email,
                Phone = bugReport.Phone,
                Subject = bugReport.Subject,
                Description = bugReport.Description
            });
        }
    }
}
