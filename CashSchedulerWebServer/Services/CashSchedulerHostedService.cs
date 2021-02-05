using System.Threading;
using System.Threading.Tasks;
using CashSchedulerWebServer.Jobs.Contracts;
using Microsoft.Extensions.Hosting;

namespace CashSchedulerWebServer.Services
{
    public class CashSchedulerHostedService : IHostedService
    {
        private IJobManager JobManager { get; }

        public CashSchedulerHostedService(IJobManager jobManager)
        {
            JobManager = jobManager;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await JobManager.ScheduleJobs(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await JobManager.UnScheduleJobs(cancellationToken);
        }
    }
}
