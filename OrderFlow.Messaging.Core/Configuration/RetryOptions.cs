using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Messaging.Core.Configuration
{
    public sealed class RetryOptions
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(5);
        public bool UseExponentialBackoff { get; set; } = true;
    }
}
