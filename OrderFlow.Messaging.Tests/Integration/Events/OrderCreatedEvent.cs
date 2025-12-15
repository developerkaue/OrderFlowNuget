using Orderflow.Messaging.Abstractions.Abstractions;

namespace OrderFlow.Messaging.Tests.Integration.Events
{
    public sealed record OrderCreatedEvent(
        Guid OrderId,
        decimal Amount
    ) : IMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
