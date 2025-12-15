using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.Messaging.RabbitMQ.Connection
{
    public interface IRabbitMqConnection : IDisposable
    {
        IModel CreateChannel();
    }
}
