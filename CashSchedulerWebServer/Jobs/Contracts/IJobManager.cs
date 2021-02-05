using System.Threading;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Jobs.Contracts
{
    public interface IJobManager
    {
        Task ScheduleJobs(CancellationToken cancellationToken);
        Task UnScheduleJobs(CancellationToken cancellationToken);
    }
}
