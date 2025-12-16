using OrderFlow.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Tests.Integration.RabbitMQ.Consumers
{
    public class OrderCreatedTestConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly TaskCompletionSource<bool> _tcs;

        public OrderCreatedTestConsumer(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public Task ConsumeAsync(OrderCreatedEvent message, CancellationToken cancellationToken)
        {
            _tcs.TrySetResult(true);
            return Task.CompletedTask;
        }
    }
}
