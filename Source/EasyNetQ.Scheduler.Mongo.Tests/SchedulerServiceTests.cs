﻿using EasyNetQ.Topology;
using NSubstitute;
using Xunit;
using System;
using Microsoft.Extensions.Logging;

namespace EasyNetQ.Scheduler.Mongo.Tests
{
    public class SchedulerServiceTests
    {
        public SchedulerServiceTests()
        {
            bus = Substitute.For<IBus>();
            advancedBus = Substitute.For<IAdvancedBus>();
            var logger = Substitute.For<ILogger<SchedulerService>>();

            bus.IsConnected.Returns(true);
            bus.Advanced.Returns(advancedBus);

            scheduleRepository = Substitute.For<IScheduleRepository>();

            schedulerService = new SchedulerService(
                bus,
                scheduleRepository,
                new SchedulerServiceConfiguration
                    {
                        HandleTimeoutInterval = TimeSpan.FromSeconds(1),
                        PublishInterval = TimeSpan.FromSeconds(1),
                        SubscriptionId = "Scheduler",
                        PublishMaxSchedules = 2,
                        EnableLegacyConventions = false
                    },
                logger);
        }

        private SchedulerService schedulerService;
        private IBus bus;
        private IAdvancedBus advancedBus;
        private IScheduleRepository scheduleRepository;

        [Fact]
        public void Should_get_pending_scheduled_messages_and_update_them()
        {
            var id = Guid.NewGuid();
            scheduleRepository.GetPending().Returns(new Schedule
                {
                    Id = id,
                    BindingKey = "msg1"
                });

            schedulerService.OnPublishTimerTick(null);

            scheduleRepository.Received().MarkAsPublished(id);
            advancedBus.Received().Publish(
                Arg.Any<IExchange>(),
                Arg.Is<string>("msg1"),
                Arg.Any<bool>(),
                Arg.Any<MessageProperties>(),
                Arg.Any<byte[]>());
        }


        [Fact]
        public void Should_handle_publish_timeout_()
        {
            schedulerService.OnHandleTimeoutTimerTick(null);
            scheduleRepository.Received().HandleTimeout();
        }
    }
}

// ReSharper restore InconsistentNaming
