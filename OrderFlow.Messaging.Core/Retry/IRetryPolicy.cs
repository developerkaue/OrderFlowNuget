using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Core.Retry
{
    public interface IRetryPolicy
    {
        Task ExecuteAsync(Func<Task> action,CancellationToken cancellationToken = default);
    }
}
