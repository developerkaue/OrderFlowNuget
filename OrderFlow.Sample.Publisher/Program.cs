using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.RabbitMQ.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqMessaging(options =>
{
    options.Host = "localhost";
    options.ExchangeName = "orderflow.exchange";
});

var host = builder.Build();

var bus = host.Services.GetRequiredService<IMessageBus>();

while (true)
{
    var orderEvent = new OrderCreatedEvent
    {
        OrderId = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow
    };

    await bus.PublishAsync(orderEvent);

    Console.WriteLine($"Pedido publicado: {orderEvent.OrderId}");
    await Task.Delay(5000);
}
