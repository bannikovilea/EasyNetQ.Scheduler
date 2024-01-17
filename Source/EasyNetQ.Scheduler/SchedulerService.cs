using System;
using System.Transactions;
using EasyNetQ.Topology;
using System.Collections.Concurrent;
using EasyNetQ.ExternalScheduler;
using Microsoft.Extensions.Logging;

namespace EasyNetQ.Scheduler
{
    public interface ISchedulerService
    {
        void Start();
        void Stop();

    }

    public class SchedulerService : ISchedulerService
    {
        private const string schedulerSubscriptionId = "schedulerSubscriptionId";

        private readonly IBus bus;
        private readonly IScheduleRepository scheduleRepository;
        private readonly SchedulerServiceConfiguration configuration;
        private readonly ILogger<ScheduleRepository> log;

        private System.Threading.Timer publishTimer;
        private System.Threading.Timer purgeTimer;

        public SchedulerService(
            IBus bus,
            IScheduleRepository scheduleRepository,
            SchedulerServiceConfiguration configuration,
            ILogger<ScheduleRepository> log)
        {
            this.bus = bus;
            this.scheduleRepository = scheduleRepository;
            this.configuration = configuration;
            this.log = log;
        }

        public void Start()
        {
            log.LogDebug("Starting SchedulerService");
            bus.Subscribe<ScheduleMe>(schedulerSubscriptionId, OnMessage);
            bus.Subscribe<UnscheduleMe>(schedulerSubscriptionId, OnMessage);

            publishTimer = new System.Threading.Timer(OnPublishTimerTick, null, 0, configuration.PublishIntervalSeconds * 1000);
            purgeTimer = new System.Threading.Timer(OnPurgeTimerTick, null, 0, configuration.PurgeIntervalSeconds * 1000);
        }

        public void Stop()
        {
            log.LogDebug("Stopping SchedulerService");
            if (publishTimer != null)
            {
                publishTimer.Dispose();
            }
            if (purgeTimer != null)
            {
                purgeTimer.Dispose();
            }
            if (bus != null)
            {
                bus.Dispose();
            }
        }

        public void OnMessage(ScheduleMe scheduleMe)
        {
            log.LogDebug("Got Schedule Message");
            scheduleRepository.Store(scheduleMe);
        }

        public void OnMessage(UnscheduleMe unscheduleMe)
        {
            log.LogDebug("Got Unschedule Message");
            scheduleRepository.Cancel(unscheduleMe);
        }

        public void OnPublishTimerTick(object state)
        {
            try
            {
                if (!bus.IsConnected)
                {
                    log.LogInformation("Not connected");
                    return;
                }

                // Keep track of exchanges that have already been declared this tick
                var declaredExchanges = new ConcurrentDictionary<Tuple<string, string>, IExchange>();
                Func<Tuple<string, string>, IExchange> declareExchange = exchangeNameType =>
                {
                    log.LogDebug("Declaring exchange {0}, {1}", exchangeNameType.Item1, exchangeNameType.Item2);
                    return bus.Advanced.ExchangeDeclare(exchangeNameType.Item1, exchangeNameType.Item2);
                };

                using (var scope = new TransactionScope())
                {
                    var scheduledMessages = scheduleRepository.GetPending();

                    foreach (var scheduledMessage in scheduledMessages)
                    {
                        // Binding key fallback is only provided here for backwards compatibility, will be removed in the future
                        log.LogDebug("Publishing Scheduled Message with Routing Key: '{0}'", scheduledMessage.BindingKey);

                        var exchangeName = scheduledMessage.Exchange ?? scheduledMessage.BindingKey;
                        var exchangeType = scheduledMessage.ExchangeType ?? ExchangeType.Topic;

                        var exchange = declaredExchanges.GetOrAdd(new Tuple<string, string>(exchangeName, exchangeType), declareExchange);

                        var messageProperties = scheduledMessage.MessageProperties;

                        if (scheduledMessage.MessageProperties == null)
                            messageProperties = new MessageProperties { Type = scheduledMessage.BindingKey };

                        var routingKey = scheduledMessage.RoutingKey ?? scheduledMessage.BindingKey;

                        bus.Advanced.Publish(
                            exchange,
                            routingKey,
                            false,
                            messageProperties,
                            scheduledMessage.InnerMessage);
                    }

                    scope.Complete();
                }
            }
            catch (Exception exception)
            {
                log.LogError("Error in schedule poll\r\n{0}", exception);
            }
        }

        private void OnPurgeTimerTick(object state)
        {
            try
            {
                scheduleRepository.Purge();
            }
            catch (Exception exception)
            {
                log.LogError("Error in schedule purge\r\n{0}", exception);
            }
        }
    }
}
