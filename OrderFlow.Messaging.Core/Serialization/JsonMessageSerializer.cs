using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace OrderFlow.Messaging.Core.Serialization
{
    public sealed class JsonMessageSerializer : IMessageSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public byte[] Serialize<T>(T message)
            => JsonSerializer.SerializeToUtf8Bytes(message, Options);

        public T Deserialize<T>(byte[] payload)
            => JsonSerializer.Deserialize<T>(payload, Options)!;
    }
}
