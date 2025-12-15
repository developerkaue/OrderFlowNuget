using System;
using System.Collections.Generic;
using System.Text;

namespace Orderflow.Messaging.Abstractions.Abstractions
{
    public interface IMessage
    {
        Guid MessageId { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
