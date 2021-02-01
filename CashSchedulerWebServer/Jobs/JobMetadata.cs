using System;

namespace CashSchedulerWebServer.Jobs
{
    public class JobMetadata
    {
        public Guid JobId { get; }
        public Type JobType { get; }
        public string JobName { get; }
        public string CronExpression { get; }

        public JobMetadata(Type jobType, string jobName, string cronExpression)
        {
            JobId = Guid.NewGuid();
            JobType = jobType;
            JobName = jobName;
            CronExpression = cronExpression;
        }
    }
}
