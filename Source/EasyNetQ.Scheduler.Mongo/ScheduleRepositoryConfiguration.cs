using System;

namespace EasyNetQ.Scheduler.Mongo
{
    public class ScheduleRepositoryConfiguration : IScheduleRepositoryConfiguration
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public TimeSpan DeleteTimeout { get; set; }
        public TimeSpan PublishTimeout { get; set; }
    }
}
