using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Tests.Integration.TestConfiguration
{
    public static class TestConfigurationProvider
    {
        public static IConfiguration Create()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RabbitMq:Host"] = "localhost",
                    ["RabbitMq:Port"] = "5672",
                    ["RabbitMq:Username"] = "guest",
                    ["RabbitMq:Password"] = "guest",

                    ["RabbitMq:ExchangeName"] = "orderflow.exchange",
                    ["RabbitMq:Queue"] = "orderflow.test.queue",
                    ["RabbitMq:RoutingKey"] = "order.created",

                    ["RabbitMq:RetryCount"] = "3",
                    ["RabbitMq:RetryDelaySeconds"] = "2"
                })
                .Build();
        }
    }
}
