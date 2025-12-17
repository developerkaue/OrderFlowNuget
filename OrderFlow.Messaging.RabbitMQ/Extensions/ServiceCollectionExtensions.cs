using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Messaging.Core.Configuration;
using OrderFlow.Messaging.Core.Extensions;
using OrderFlow.Messaging.RabbitMQ.Bus;
using OrderFlow.Messaging.RabbitMQ.Configuration;
using OrderFlow.Messaging.RabbitMQ.Connection;

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

            services.AddSingleton(new RetryOptions
            {
                MaxRetryAttempts = 3,
                InitialDelay = TimeSpan.FromSeconds(2),
            });

            services.AddMessagingCore();
            services.AddSingleton(options);

            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

            return services;
        }
    }
}
