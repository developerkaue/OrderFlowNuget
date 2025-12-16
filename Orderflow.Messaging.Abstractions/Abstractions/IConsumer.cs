using System;
using System.Collections.Generic;
using System.Text;

namespace Orderflow.Messaging.Abstractions.Abstractions
{
    public interface IConsumer<TMessage> where TMessage : IMessage
    {
        Task ConsumeAsync(TMessage message, CancellationToken cancellationToken);
    }
}
