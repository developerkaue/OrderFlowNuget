using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Core.Serialization
{
    public interface IMessageSerializer
    {
        byte[] Serialize<T>(T message);
        T Deserialize<T>(byte[] payload);
    }
}
