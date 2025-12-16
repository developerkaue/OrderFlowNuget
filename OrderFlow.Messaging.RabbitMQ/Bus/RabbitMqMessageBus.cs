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
            var exchange = _options.ExchangeName;
            var routingKey = typeof(TMessage).Name;

            _channel.ExchangeDeclare(
                exchange: exchange,
                type: ExchangeType.Direct,
                durable: true);

            var body = _serializer.Serialize(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

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
            var messageName = typeof(TMessage).Name;
            var exchangeName = _options.ExchangeName;
            var queueName = $"{messageName}.queue";
            var routingKey = messageName;

            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true);

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                await HandleMessageAsync<TMessage, TConsumer>(ea);
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
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

                var message = _serializer.Deserialize<TMessage>(eventArgs.Body.ToArray());

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await consumer.ConsumeAsync(message, CancellationToken.None);
                });

                        _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
            }
            catch
            {
                // Se falhar mesmo após retries
                _channel.BasicNack(eventArgs.DeliveryTag, false, requeue: false);
                throw;
            }
        }


    }
}
