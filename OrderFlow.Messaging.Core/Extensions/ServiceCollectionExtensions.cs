using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Messaging.Core.Configuration;
using OrderFlow.Messaging.Core.Retry;
using OrderFlow.Messaging.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingCore(
            this IServiceCollection services,
            Action<MessagingOptions>? configure = null)
        {
            var options = new MessagingOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton(options.Retry);

            services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            services.AddSingleton<IRetryPolicy, PollyRetryPolicy>();

            return services;
        }
    }
}
