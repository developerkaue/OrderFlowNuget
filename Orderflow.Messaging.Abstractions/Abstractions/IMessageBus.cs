using System;
using System.Collections.Generic;
using System.Text;

namespace Orderflow.Messaging.Abstractions.Abstractions
{
    public interface IMessageBus
    {
        Task PublishAsync<TMessage>(TMessage message)
        where TMessage : IMessage;

        void Subscribe<TMessage, TConsumer>()
            where TMessage : IMessage
            where TConsumer : IConsumer<TMessage>;
    }
}
