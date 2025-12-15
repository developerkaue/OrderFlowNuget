using System;
using System.Collections.Generic;
using System.Text;

namespace Orderflow.Messaging.Abstractions.Abstractions
{
    public interface IConsumer
    {
        void Subscribe<T>(Func<T, Task> handler, CancellationToken cancellationToken = default)where T : class, IMessage;
    }
}
