using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.Messaging.RabbitMQ.Configuration
{
    public sealed class RabbitMqOptions
    {
        public string Host { get; set; } = "rabbitmq";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        public string Exchange { get; set; } = "orderflow.exchange";
        public string Queue { get; set; } = "orderflow.queue";
        public string RoutingKey { get; set; } = "order.created";
        public string DeadLetterQueue => $"{Queue}.dlq";
        public string DeadLetterExchange { get; set; } = "orderflow.dlx";
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
        public string ExchangeName { get; set; } = "orderflow.exchange";


    }
}
