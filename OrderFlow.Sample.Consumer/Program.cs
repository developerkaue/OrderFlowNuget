using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Core.Extensions;
using OrderFlow.Messaging.RabbitMQ;
using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Sample.Consumer.Consumers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
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

            options.RetryCount = 3;
            options.RetryDelaySeconds = 2;
        });

        services.AddScoped<OrderCreatedConsumer>();

        services.AddHostedService(provider =>
        {
            var bus = provider.GetRequiredService<IMessageBus>();

            bus.Subscribe<OrderCreatedEvent, OrderCreatedConsumer>();

            return new BackgroundServiceWrapper();
        });
    })
    .Build();

await host.RunAsync();


// Serviço vazio apenas para manter o host vivo
sealed class BackgroundServiceWrapper : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Delay(Timeout.Infinite, stoppingToken);
}
