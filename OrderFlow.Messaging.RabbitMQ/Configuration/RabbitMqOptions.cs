using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.Messaging.RabbitMQ.Configuration
{
    public sealed class RabbitMqOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        public string Exchange { get; set; } = "orderflow.exchange";
        public string Queue { get; set; } = "orderflow.queue";
        public string DeadLetterQueue => $"{Queue}.dlq";
    }
}
