using Microsoft.Extensions.Logging;
using OrderFlow.Messaging.Core.Retry;
using OrderFlow.Messaging.Core.Serialization;
using OrderFlow.Messaging.RabbitMQ.Configuration;
using OrderFlow.Messaging.RabbitMQ.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Orderflow.Messaging.Abstractions.Abstractions;



namespace OrderFlow.Messaging.RabbitMQ.Bus
{
    public sealed class RabbitMqMessageBus : IMessageBus
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IMessageSerializer _serializer;
        private readonly IRetryPolicy _retryPolicy;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqMessageBus> _logger;

        public RabbitMqMessageBus(
            IRabbitMqConnection connection,
            IMessageSerializer serializer,
            IRetryPolicy retryPolicy,
            RabbitMqOptions options,
            ILogger<RabbitMqMessageBus> logger)
        {
            _connection = connection;
            _serializer = serializer;
            _retryPolicy = retryPolicy;
            _options = options;
            _logger = logger;
        }

        public async Task PublishAsync<T>(
            T message,
            CancellationToken cancellationToken = default)
            where T : class, IMessage
        {
            await _retryPolicy.ExecuteAsync(() =>
            {
                using var channel = _connection.CreateChannel();

                channel.ExchangeDeclare(
                    exchange: _options.Exchange,
                    type: ExchangeType.Topic,
                    durable: true);

                var body = _serializer.Serialize(message);

                var props = channel.CreateBasicProperties();
                props.Persistent = true;

                channel.BasicPublish(
                    exchange: _options.Exchange,
                    routingKey: typeof(T).Name,
                    basicProperties: props,
                    body: body);

                _logger.LogInformation(
                    "Message {MessageId} published",
                    message.MessageId);

                return Task.CompletedTask;
            }, cancellationToken);
        }

        public void Subscribe<T>(
            Func<T, Task> handler,
            CancellationToken cancellationToken = default)
            where T : class, IMessage
        {
            var channel = _connection.CreateChannel();

            channel.ExchangeDeclare(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true);

            channel.QueueDeclare(
                queue: _options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", _options.DeadLetterQueue }
                });

            channel.QueueBind(
                queue: _options.Queue,
                exchange: _options.Exchange,
                routingKey: typeof(T).Name);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (_, args) =>
            {
                try
                {
                    var message = _serializer.Deserialize<T>(args.Body.ToArray());

                    await _retryPolicy.ExecuteAsync(() =>
                        handler(message), cancellationToken);

                    channel.BasicAck(args.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message processing failed");

                    channel.BasicNack(
                        args.DeliveryTag,
                        multiple: false,
                        requeue: false);
                }
            };

            channel.BasicConsume(
                queue: _options.Queue,
                autoAck: false,
                consumer: consumer);
        }
    }
}
