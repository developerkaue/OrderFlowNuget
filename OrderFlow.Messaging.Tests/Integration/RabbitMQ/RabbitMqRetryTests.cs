using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Messaging.Tests.Integration.RabbitMQ.Consumers;
using OrderFlow.Messaging.Tests.Integration.TestConfiguration;
using System;
using System.Collections.Generic;
using System.Text;
using OrderFlow.Contracts.Events.Contracts;

namespace OrderFlow.Messaging.Tests.Integration.RabbitMQ
{
    public class RabbitMqRetryTests
    {
        [Fact(DisplayName = "Retry message consumption on failure")]
        public async Task RetryMessageConsumptionOnFailure()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddRabbitMqMessaging(options =>
            {
                options.Host = RabbitMqTestOptions.Host;
                options.Port = RabbitMqTestOptions.Port;
                options.Username = RabbitMqTestOptions.Username;
                options.Password = RabbitMqTestOptions.Password;

                options.Exchange = "orderflow.retry.exchange";
                options.Queue = "orderflow.retry.queue";

                options.RetryCount = 3;
                options.RetryDelaySeconds = 1;
            });

            var provider = services.BuildServiceProvider();
            var bus = provider.GetRequiredService<IMessageBus>();

            var attemptCount = 0;
            var tcs = new TaskCompletionSource<bool>();

            // Act
            bus.Subscribe<OrderCreatedEvent, OrderCreatedRetryTestConsumer>();

            await bus.PublishAsync(new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });

            // Assert
            var completed = await Task.WhenAny(
                tcs.Task,
                Task.Delay(TimeSpan.FromSeconds(10)));

            Assert.True(
                completed == tcs.Task,
                "Message was not processed successfully after retries");

            Assert.True(tcs.Task.IsCompletedSuccessfully);

            Assert.True(
                attemptCount >= 3,
                $"Expected at least 3 attempts, but got {attemptCount}");
        }
    }
}
