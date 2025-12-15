using System;
using System.Collections.Generic;
using System.Text;

namespace Orderflow.Messaging.Abstractions.Abstractions
{
    public interface IMessageBus : IPublisher, IConsumer
    {
    }
}
