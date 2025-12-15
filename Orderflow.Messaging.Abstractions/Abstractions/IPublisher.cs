using System;
using System.Collections.Generic;
using System.Text;

namespace Orderflow.Messaging.Abstractions.Abstractions
{
    public interface IPublisher
    {
        Task PublishAsync<T>(T message,CancellationToken cancellationToken = default)where T : class, IMessage;
            
    }
}
