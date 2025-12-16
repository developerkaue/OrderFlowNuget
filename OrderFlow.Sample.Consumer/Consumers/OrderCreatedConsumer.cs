using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Contracts.Events.Contracts;

namespace OrderFlow.Sample.Consumer.Consumers
{
    public sealed class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private static int _attempts = 0;

        public async Task ConsumeAsync(OrderCreatedEvent message, CancellationToken cancellationToken)
        {
            _attempts++;

            Console.WriteLine($"Attempt {_attempts}");
            Console.WriteLine($"OrderId: {message.OrderId}");

            if (_attempts < 3)
            {
                Console.WriteLine("Simulated failure");
                throw new Exception("Failing on purpose to test retry");
            }

            Console.WriteLine("Order processed successfully");
            Console.WriteLine($"OrderId: {message.OrderId}");
            Console.WriteLine($"CreatedAt: {message.CreatedAt}");

            await Task.CompletedTask;
        }
    }

}
