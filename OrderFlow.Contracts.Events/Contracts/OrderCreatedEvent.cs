using Orderflow.Messaging.Abstractions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Contracts.Events.Contracts
{
    public class OrderCreatedEvent : IMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid OrderId { get; set; }
    }
}
