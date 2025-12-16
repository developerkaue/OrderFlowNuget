using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Messaging.RabbitMQ.Extensions;
using OrderFlow.Sample.Consumer;
using OrderFlow.Contracts.Events;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqMessaging(options =>
{
    options.Host = "localhost";
    options.ExchangeName = "orderflow.exchange";
});

builder.Services.AddTransient<IConsumer<OrderCreatedEvent>, OrderCreatedConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
var bus = host.Services.GetRequiredService<IMessageBus>();
bus.Subscribe<OrderCreatedEvent, OrderCreatedConsumer>();

await host.RunAsync();
