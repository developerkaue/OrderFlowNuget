using Orderflow.Messaging.Abstractions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Sample.Consumer
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private static int _attempts = 0;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task ConsumeAsync(OrderCreatedEvent message, CancellationToken cancellationToken)
        {
            _attempts++;

            _logger.LogInformation(
                "Tentativa {Attempt} - Processando pedido {OrderId}",
                _attempts,
                message.OrderId);

            if (_attempts < 3)
            {
                _logger.LogWarning("Falha simulada no processamento");
                throw new Exception("Simulated failure");
            }

            _logger.LogInformation("Pedido processado com sucesso!");
            return Task.CompletedTask;
        }
    }

}
