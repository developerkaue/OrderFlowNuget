using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Messaging.Tests.Integration.Events;
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
            bus.Subscribe<OrderCreatedEvent>(message =>
            {
                tcs.TrySetResult(true);
                return Task.CompletedTask;
            });

            await bus.PublishAsync(
                new OrderCreatedEvent(Guid.NewGuid(), 150));

            // Assert
            var completed = await Task.WhenAny(
                tcs.Task,
                Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.True(
                completed == tcs.Task,
                "Message was not consumed within timeout");
        }
    }
}
