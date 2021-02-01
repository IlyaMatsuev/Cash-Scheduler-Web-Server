using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace CashSchedulerWebServer.Jobs
{
    public class JobFactory : IJobFactory
    {
        private IServiceProvider ServiceProvider { get; }

        public JobFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }


        public IJob NewJob(TriggerFiredBundle triggerFiredBundle, IScheduler scheduler)
        {
            var scope = ServiceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService(triggerFiredBundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}
