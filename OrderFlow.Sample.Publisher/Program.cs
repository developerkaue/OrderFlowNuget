using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Core.Extensions;
using OrderFlow.Messaging.RabbitMQ;
using OrderFlow.Messaging.RabbitMQ.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddRabbitMqMessaging(options =>
        {
            options.Host = "localhost";
            options.Port = 5672;
            options.Username = "guest";
            options.Password = "guest";

            options.ExchangeName = "orderflow.exchange";
            options.Queue = "orderflow.order-created.queue";
            options.RoutingKey = "order.created";
        });
    })
    .Build();

var bus = host.Services.GetRequiredService<IMessageBus>();

var orderEvent = new OrderCreatedEvent
{
    OrderId = Guid.NewGuid(),
    CreatedAt = DateTime.UtcNow
};

await bus.PublishAsync(orderEvent);

Console.WriteLine("OrderCreatedEvent published!");

await host.StopAsync();
