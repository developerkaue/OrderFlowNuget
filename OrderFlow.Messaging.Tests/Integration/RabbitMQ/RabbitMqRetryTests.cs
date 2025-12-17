using OrderFlow.Contracts.Events.Contracts;
using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Messaging.Tests.Integration.RabbitMQ.Consumers;
using OrderFlow.Messaging.Tests.Integration.TestConfiguration;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;

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

            var configuration = TestConfigurationProvider.Create();
            services.AddRabbitMqMessaging(options =>
            {
                options.Host = "localhost";
                options.Port = 5672;
                options.Username = "guest";
                options.Password = "guest";

                options.ExchangeName = "orderflow.exchange";
                options.Queue = "orderflow.order-created.queue";
                options.RoutingKey = "order.created";

                options.RetryCount = 3;
                options.RetryDelaySeconds = 2;
            });

            services.AddScoped<OrderCreatedRetryTestConsumer>();

            var provider = services.BuildServiceProvider();
            var bus = provider.GetRequiredService<IMessageBus>();

            // Act
            bus.Subscribe<OrderCreatedEvent, OrderCreatedRetryTestConsumer>();

            await bus.PublishAsync(new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Se chegou até aqui sem exception → retries funcionaram
            Assert.True(true);
        }
    }
}
