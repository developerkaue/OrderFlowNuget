using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Messaging.Core.Configuration;
using OrderFlow.Messaging.Core.Retry;
using OrderFlow.Messaging.Core.Serialization;

namespace OrderFlow.Messaging.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingCore(
            this IServiceCollection services)
        {
            services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();

            services.AddSingleton(new RetryOptions
            {
                MaxRetryAttempts = 3,
                InitialDelay = TimeSpan.FromSeconds(2),
            });

            services.AddSingleton<IRetryPolicy, PollyRetryPolicy>();

            return services;
        }
    }
}
