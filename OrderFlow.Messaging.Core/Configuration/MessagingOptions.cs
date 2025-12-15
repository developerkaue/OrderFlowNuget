using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Core.Configuration
{
    public sealed class MessagingOptions
    {
        public RetryOptions Retry { get; set; } = new();
    }
}
