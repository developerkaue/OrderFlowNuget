using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Messaging.Core.Configuration;
using OrderFlow.Messaging.Core.Extensions;
using OrderFlow.Messaging.Core.Retry;
using OrderFlow.Messaging.Core.Serialization;
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

            RemoveExistingRegistrations<RabbitMqOptions>(services);
            RemoveExistingRegistrations<IMessageBus>(services);

            services.AddSingleton(options);

            services.AddSingleton(new RetryOptions
            {
                MaxRetryAttempts = options.RetryCount,
                InitialDelay = TimeSpan.FromSeconds(options.RetryDelaySeconds),
            });

            services.AddMessagingCore();
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

            services.AddSingleton<IMessageBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMqMessageBus>>();

                var connection = sp.GetRequiredService<IRabbitMqConnection>();
                var serializer = sp.GetRequiredService<IMessageSerializer>();
                var retryPolicy = sp.GetRequiredService<IRetryPolicy>();
                var busLogger = sp.GetRequiredService<ILogger<RabbitMqMessageBus>>();

                return new RabbitMqMessageBus(
                    connection,
                    serializer,
                    retryPolicy,
                    options,
                    sp,
                    busLogger
                );
            });

            return services;
        }

        private static void RemoveExistingRegistrations<T>(IServiceCollection services)
        {
            var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
        }
    }
}
