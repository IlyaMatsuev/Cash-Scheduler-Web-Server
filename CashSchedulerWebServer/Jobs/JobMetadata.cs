using System;

namespace CashSchedulerWebServer.Jobs
{
    public class JobMetadata
    {
        public Guid JobId { get; private set; }
        public Type JobType { get; private set; }
        public string JobName { get; private set; }
        public string CronExpression { get; private set; }

        public JobMetadata(Type jobType, string jobName, string cronExpression)
        {
            JobId = Guid.NewGuid();
            JobType = jobType;
            JobName = jobName;
            CronExpression = cronExpression;
        }
    }
}
