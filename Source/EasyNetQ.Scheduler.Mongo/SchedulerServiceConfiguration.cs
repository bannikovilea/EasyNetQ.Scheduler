using System;
using System.Configuration;

namespace EasyNetQ.Scheduler.Mongo
{
    public class SchedulerServiceConfiguration : ISchedulerServiceConfiguration
    {
        public bool EnableLegacyConventions { get; set; }
        public string SubscriptionId { get; set; }
        public TimeSpan PublishInterval { get; set; }
        public TimeSpan HandleTimeoutInterval { get; set; }
        public int PublishMaxSchedules { get; set; }
        public string RabbitHost { get; set; }
    }
}
