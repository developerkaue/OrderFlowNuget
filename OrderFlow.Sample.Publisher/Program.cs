using OrderFlow.Messaging.RabbitMQ.Extensions;

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

        services.AddHostedService<OrderCreatedPublisherHostedService>();
    })
    .Build();

await host.StartAsync();

Console.WriteLine("Host started. Publishing event...");

await Task.Delay(2000);

Console.WriteLine("OrderCreatedEvent published!");

await host.StopAsync();

Console.WriteLine("Host stopped.");
 