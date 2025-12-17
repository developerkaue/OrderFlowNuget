using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Contracts.Events.Contracts
{
    public interface IMessageProcessedStore
    {
        Task<bool> HasBeenProcessedAsync(string messageId);
        Task MarkAsProcessedAsync(string messageId);
    }
}
