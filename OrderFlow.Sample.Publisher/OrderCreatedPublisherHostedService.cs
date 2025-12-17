using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Contracts.Events.Contracts;

public class OrderCreatedPublisherHostedService : BackgroundService
{
    private readonly IMessageBus _bus;
    private readonly ILogger<OrderCreatedPublisherHostedService> _logger;

    public OrderCreatedPublisherHostedService(
        IMessageBus bus,
        ILogger<OrderCreatedPublisherHostedService> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // pequeno delay opcional só para garantir startup completo
        await Task.Delay(2000, stoppingToken);

        var orderEvent = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        await _bus.PublishAsync(orderEvent);

        _logger.LogInformation("OrderCreatedEvent published successfully");
    }
}
