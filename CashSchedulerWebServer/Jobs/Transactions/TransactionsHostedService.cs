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
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IJobFactory jobFactory;
        private readonly List<JobMetadata> jobsMetadata;

        private IScheduler Scheduler { get; set; }

        public TransactionsHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, List<JobMetadata> jobsMetadata)
        {
            this.schedulerFactory = schedulerFactory;
            this.jobFactory = jobFactory;
            this.jobsMetadata = jobsMetadata;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Scheduling the jobs");
            Scheduler = await schedulerFactory.GetScheduler();
            Scheduler.JobFactory = jobFactory;
            
            foreach (JobMetadata jobMetadata in jobsMetadata)
            {
                await Scheduler.ScheduleJob(CreateJob(jobMetadata), CreateTrigger(jobMetadata), cancellationToken);
                Console.WriteLine($"{jobMetadata.JobName} has been scheduled");
            }

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Shutting down the jobs");
            await Scheduler?.Shutdown(cancellationToken);
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
