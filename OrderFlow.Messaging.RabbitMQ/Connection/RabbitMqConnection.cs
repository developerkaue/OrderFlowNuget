using OrderFlow.Messaging.RabbitMQ.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace OrderFlow.Messaging.RabbitMQ.Connection
{
    public sealed class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly object _lock = new();

        public RabbitMqConnection(RabbitMqOptions options)
        {
            _factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password,
                DispatchConsumersAsync = true
            };
        }

        public IModel CreateChannel()
        {
            EnsureConnection();
            return _connection!.CreateModel();
        }

        private void EnsureConnection()
        {
            if (_connection is { IsOpen: true })
                return;

            lock (_lock)
            {
                if (_connection is { IsOpen: true })
                    return;

                var retries = 10;

                while (retries-- > 0)
                {
                    try
                    {
                        _connection = _factory.CreateConnection();
                        return;
                    }
                    catch
                    {
                        Thread.Sleep(2000);
                    }
                }

                throw new InvalidOperationException(
                    "Could not connect to RabbitMQ after multiple attempts.");
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
