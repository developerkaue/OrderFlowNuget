using Microsoft.Extensions.Logging;
using Orderflow.Messaging.Abstractions.Abstractions;
using OrderFlow.Messaging.Core.Retry;
using OrderFlow.Messaging.Core.Serialization;
using OrderFlow.Messaging.RabbitMQ.Configuration;
using OrderFlow.Messaging.RabbitMQ.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;



namespace OrderFlow.Messaging.RabbitMQ.Bus
{
    public sealed class RabbitMqMessageBus : IMessageBus
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IMessageSerializer _serializer;
        private readonly IRetryPolicy _retryPolicy;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqMessageBus> _logger;
        private readonly IServiceProvider _serviceProvider;

        private static readonly HashSet<string> DeclaredQueues = new();


        private IModel _channel;

        public RabbitMqMessageBus(
            IRabbitMqConnection connection,
            IMessageSerializer serializer,
            IRetryPolicy retryPolicy,
            RabbitMqOptions options,
            IServiceProvider serviceProvider,
            ILogger<RabbitMqMessageBus> logger)
        {
            _connection = connection;
            _serializer = serializer;
            _retryPolicy = retryPolicy;
            _options = options;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _channel = _connection.CreateChannel();
        }

        public async Task PublishAsync<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            var messageId = Guid.NewGuid().ToString();
            var correlationId = Guid.NewGuid().ToString();

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = messageId;
            properties.CorrelationId = correlationId;
            properties.Headers = new Dictionary<string, object>();

            var body = _serializer.Serialize(message);

            var exchange = _options.ExchangeName;
            var routingKey = typeof(TMessage).Name;

            _logger.LogInformation(
                "Publishing message {MessageType} | MessageId={MessageId} | CorrelationId={CorrelationId}",
                typeof(TMessage).Name,
                messageId,
                correlationId
            );

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            await Task.CompletedTask;
        }


        public void Subscribe<TMessage, TConsumer>()
         where TMessage : IMessage
         where TConsumer : IConsumer<TMessage>
        {
            DeclareInfrastructureOnce<TMessage>();

            var queueName = $"{typeof(TMessage).Name}.queue";

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    await HandleMessageAsync<TMessage, TConsumer>(ea);

                    _channel.BasicAck(ea.DeliveryTag, false);

                    _logger.LogInformation(
                        "Message processed successfully {MessageType} | MessageId={MessageId}",
                        typeof(TMessage).Name,
                        ea.BasicProperties?.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Message failed permanently {MessageType} | MessageId={MessageId}",
                        typeof(TMessage).Name,
                        ea.BasicProperties?.MessageId);

                    _channel.BasicNack(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }

        private async Task HandleMessageAsync<TMessage, TConsumer>(BasicDeliverEventArgs eventArgs)
        where TMessage : IMessage
        where TConsumer : IConsumer<TMessage>
        {
            using var scope = _serviceProvider.CreateScope();

            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

            var message = _serializer.Deserialize<TMessage>(eventArgs.Body.ToArray());

            var messageId = eventArgs.BasicProperties?.MessageId;
            var correlationId = eventArgs.BasicProperties?.CorrelationId;

            _logger.LogInformation(
                "Consuming message {MessageType} | MessageId={MessageId} | CorrelationId={CorrelationId}",
                typeof(TMessage).Name,
                messageId,
                correlationId
            );

            await _retryPolicy.ExecuteAsync(() =>
                consumer.ConsumeAsync(message, CancellationToken.None));
        }

        private void DeclareInfrastructure<TMessage>()
        {
            var messageName = typeof(TMessage).Name;
            var queueName = $"{messageName}.queue";

            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            _channel.ExchangeDeclare(
                exchange: _options.DeadLetterExchange,
                type: ExchangeType.Direct,
                durable: true);

            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", _options.DeadLetterExchange }
            };

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);

            _channel.QueueBind(
                queue: queueName,
                exchange: _options.ExchangeName,
                routingKey: messageName);

            _channel.QueueDeclare(
                queue: $"{queueName}.dlq",
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: $"{queueName}.dlq",
                exchange: _options.DeadLetterExchange,
                routingKey: messageName);
        }


        private void DeclareInfrastructureOnce<TMessage>()
        {
            var queue = typeof(TMessage).Name;

            if (!DeclaredQueues.Add(queue))
                return;

            DeclareInfrastructure<TMessage>();
        }



    }
}
