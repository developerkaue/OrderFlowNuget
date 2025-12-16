using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Tests.Integration.TestConfiguration
{
    public static class RabbitMqTestOptions
    {
        public const string Host = "localhost";
        public const int Port = 5672;
        public const string Username = "guest";
        public const string Password = "guest";

        public const string Exchange = "orderflow.test.exchange";
        public const string Queue = "orderflow.test.queue";
    }
}
