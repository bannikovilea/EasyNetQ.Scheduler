using System.Configuration;

namespace EasyNetQ.Scheduler
{
    public class ScheduleRepositoryConfiguration
    {
        public string ProviderName
        {
            get => string.IsNullOrEmpty(providerName) ? "Microsoft.Data.SqlClient" : providerName;
            set => providerName = value;
        }

        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }

        public short PurgeBatchSize { get; set; }
        public int MaximumScheduleMessagesToReturn = 100;

        /// <summary>
        /// The number of days after a schedule item triggers before it is purged.
        /// </summary>
        public int PurgeDelayDays { get; set; }

        /// <summary>
        /// Allows to create a 'discriminator' for different environment (like a VHOST for rabbitMQ, or to use the same database for different branches, environments or developer machines)
        /// </summary>
        public string InstanceName = "";

        private string providerName;
    }
}
