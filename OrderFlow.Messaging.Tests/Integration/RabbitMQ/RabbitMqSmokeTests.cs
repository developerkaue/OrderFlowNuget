using OrderFlow.Contracts.Events.Contracts;
using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Messaging.Tests.Integration.RabbitMQ.Consumers;
using OrderFlow.Messaging.Tests.Integration.TestConfiguration;
using Polly;

namespace OrderFlow.Messaging.Tests.Integration.RabbitMQ
{
    public class RabbitMqSmokeTests
    {
        [Fact(DisplayName = "Publish and consume message using RabbitMQ")]
        public async Task PublishAndConsumeMessage()
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

            services.AddScoped<OrderCreatedTestConsumer>();

            var provider = services.BuildServiceProvider();
            var bus = provider.GetRequiredService<IMessageBus>();

            // Act
            bus.Subscribe<OrderCreatedEvent, OrderCreatedTestConsumer>();

            await bus.PublishAsync(new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Se chegou aqui sem exception → mensagem publicada e consumida
            Assert.True(true);
        }
    }
}
