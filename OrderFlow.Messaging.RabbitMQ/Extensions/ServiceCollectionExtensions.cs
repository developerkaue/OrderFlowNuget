using Orderflow.Messaging.Abstractions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Messaging.Core.Extensions;
using OrderFlow.Messaging.RabbitMQ.Bus;
using OrderFlow.Messaging.RabbitMQ.Configuration;
using OrderFlow.Messaging.RabbitMQ.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.Messaging.RabbitMQ.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMqMessaging(
            this IServiceCollection services,
            Action<RabbitMqOptions> configure)
        {
            var options = new RabbitMqOptions();
            configure(options);

            services.AddSingleton(options);
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

            services.AddMessagingCore();
            services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

            return services;
        }
    }
}
