namespace EasyNetQ.Scheduler
{
    public class SchedulerServiceConfiguration
    {
        public int PublishIntervalSeconds { get; set; }
        public int PurgeIntervalSeconds { get; set; }
        public bool EnableLegacyConventions { get; set; }

        public string RabbitHost { get; set; }
    }
}
