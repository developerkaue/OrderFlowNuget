using OrderFlow.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Tests.Integration.RabbitMQ.Consumers
{
    public class OrderCreatedRetryTestConsumer : IConsumer<OrderCreatedEvent>
    {
        private static int _attemptCount = 0;
        private readonly TaskCompletionSource<bool> _tcs;

        public OrderCreatedRetryTestConsumer(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public Task ConsumeAsync(OrderCreatedEvent message, CancellationToken cancellationToken)
        {
            _attemptCount++;

            if (_attemptCount < 3)
                throw new Exception("Simulated processing failure");

            _tcs.TrySetResult(true);
            return Task.CompletedTask;
        }
    }
}
