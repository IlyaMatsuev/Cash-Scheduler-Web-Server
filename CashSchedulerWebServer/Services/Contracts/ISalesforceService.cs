using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ISalesforceService
    {
        Task<BugReport> CreateCase(BugReport bugReport);
    }
}
