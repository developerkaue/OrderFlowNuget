using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Contracts.Events.Contracts;
using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Sample.Consumer.Consumers;

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
            options.DeadLetterExchange = "orderflow.exchange.dlx";
        });

        services.AddScoped<OrderCreatedConsumer>();
        services.AddSingleton<IMessageProcessedStore, InMemoryMessageProcessedStore>();
        services.AddHostedService<BackgroundServiceWrapper>();
    })
    .Build();



await host.RunAsync();


// Serviço vazio apenas para manter o host vivo
public sealed class BackgroundServiceWrapper : BackgroundService
{
    private readonly IServiceProvider _provider;

    public BackgroundServiceWrapper(IServiceProvider provider)
    {
        _provider = provider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _provider.CreateScope();

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        bus.Subscribe<OrderCreatedEvent, OrderCreatedConsumer>();

        return Task.CompletedTask;
    }
}
