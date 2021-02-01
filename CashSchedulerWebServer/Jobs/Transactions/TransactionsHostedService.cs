using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Jobs.Transactions
{
    public class TransactionsHostedService : IHostedService
    {
        private ISchedulerFactory SchedulerFactory { get; }
        private IJobFactory JobFactory { get; }
        private List<JobMetadata> JobsMetadata { get; }

        private IScheduler Scheduler { get; set; }

        public TransactionsHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, List<JobMetadata> jobsMetadata)
        {
            SchedulerFactory = schedulerFactory;
            JobFactory = jobFactory;
            JobsMetadata = jobsMetadata;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Scheduling the jobs");
            Scheduler = await SchedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = JobFactory;
            
            foreach (var jobMetadata in JobsMetadata)
            {
                await Scheduler.ScheduleJob(CreateJob(jobMetadata), CreateTrigger(jobMetadata), cancellationToken);
                Console.WriteLine($"{jobMetadata.JobName} has been scheduled");
            }

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Shutting down the jobs");
            if (Scheduler != null)
            {
                await Scheduler.Shutdown(cancellationToken);                
            }
        }


        private ITrigger CreateTrigger(JobMetadata jobMetadata)
        {
            return TriggerBuilder.Create()
                .WithIdentity(jobMetadata.JobId.ToString())
                .WithCronSchedule(jobMetadata.CronExpression)
                .WithDescription(jobMetadata.JobName)
                .Build();
        }

        private IJobDetail CreateJob(JobMetadata jobMetadata)
        {
            return JobBuilder.Create(jobMetadata.JobType)
                .WithIdentity(jobMetadata.JobId.ToString())
                .WithDescription(jobMetadata.JobName)
                .Build();
        }
    }
}
