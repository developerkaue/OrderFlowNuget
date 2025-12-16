using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Tests.Integration.RabbitMQ.Consumers;
using OrderFlow.Messaging.Tests.Integration.TestConfiguration;

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
            services.AddRabbitMqMessaging(options =>
            {
                options.Host = RabbitMqTestOptions.Host;
                options.Port = RabbitMqTestOptions.Port;
                options.Username = RabbitMqTestOptions.Username;
                options.Password = RabbitMqTestOptions.Password;
                options.Exchange = RabbitMqTestOptions.Exchange;
                options.Queue = RabbitMqTestOptions.Queue;
            });

            var provider = services.BuildServiceProvider();
            var bus = provider.GetRequiredService<IMessageBus>();

            var tcs = new TaskCompletionSource<bool>();

            // Act
            bus.Subscribe<OrderCreatedEvent, OrderCreatedTestConsumer>();

            await bus.PublishAsync(new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });

            // Assert
            var completed = await Task.WhenAny(
                tcs.Task,
                Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.True(
                completed == tcs.Task,
                "Message was not consumed within timeout");

            Assert.True(tcs.Task.IsCompletedSuccessfully);
        }
    }
}
