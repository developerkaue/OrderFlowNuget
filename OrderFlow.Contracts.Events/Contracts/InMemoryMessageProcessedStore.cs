using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Contracts.Events.Contracts
{
    public sealed class InMemoryMessageProcessedStore : IMessageProcessedStore
    {
        private readonly HashSet<string> _processedMessages = new();
        private readonly object _lock = new();

        public Task<bool> HasBeenProcessedAsync(string messageId)
        {
            lock (_lock)
            {
                return Task.FromResult(_processedMessages.Contains(messageId));
            }
        }

        public Task MarkAsProcessedAsync(string messageId)
        {
            lock (_lock)
            {
                _processedMessages.Add(messageId);
            }

            return Task.CompletedTask;
        }
    }

}
