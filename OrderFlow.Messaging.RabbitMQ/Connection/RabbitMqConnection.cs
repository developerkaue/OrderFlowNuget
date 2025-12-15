using OrderFlow.Messaging.RabbitMQ.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace OrderFlow.Messaging.RabbitMQ.Connection
{
    public sealed class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly IConnection _connection;

        public RabbitMqConnection(RabbitMqOptions options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
        }

        public IModel CreateChannel() => _connection.CreateModel();

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
