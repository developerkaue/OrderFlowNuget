using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Contracts.Events;
using OrderFlow.Contracts.Events.Contracts;
using OrderFlow.Messaging.Core.Extensions;
using OrderFlow.Messaging.RabbitMQ;
using OrderFlow.Messaging.RabbitMQ.Extensions;
using Polly;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddRabbitMqMessaging(options =>
        {
            options.Host = "rabbitmq";
            options.Port = 5672;
            options.Username = "guest";
            options.Password = "guest";

            options.ExchangeName = "orderflow.exchange";
            options.Queue = "orderflow.order-created.queue";
            options.RoutingKey = "order.created";

            options.RetryCount = 3;
            options.RetryDelaySeconds = 2;
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
