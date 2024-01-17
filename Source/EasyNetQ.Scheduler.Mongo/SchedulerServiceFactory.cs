using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyNetQ.Scheduler.Mongo
{
    public static class SchedulerServiceFactory
    {
        public static ISchedulerService CreateScheduler()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var serviceConfig = config.GetRequiredSection("SchedulerServiceConfiguration")
                .Get<SchedulerServiceConfiguration>();
            var schedulerRepositoryConfig = config.GetSection("ScheduleRepositoryConfiguration")
                .Get<ScheduleRepositoryConfiguration>();
            var bus = RabbitHutch.CreateBus(serviceConfig.RabbitHost, sr =>
            {
                if (serviceConfig.EnableLegacyConventions) sr.EnableLegacyConventions();
            });
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var logger = loggerFactory.CreateLogger<SchedulerService>();

            return new SchedulerService(
                bus,
                new ScheduleRepository(schedulerRepositoryConfig, () => DateTime.UtcNow),
                serviceConfig, logger);
        }
    }
}
