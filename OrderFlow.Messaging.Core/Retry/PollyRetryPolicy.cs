using Microsoft.Extensions.Logging;
using OrderFlow.Messaging.Core.Configuration;
using Polly;

namespace OrderFlow.Messaging.Core.Retry
{
    public sealed class PollyRetryPolicy : IRetryPolicy
    {
        private readonly AsyncPolicy _policy;

        public PollyRetryPolicy(
            RetryOptions options,
            ILogger<PollyRetryPolicy> logger)
        {
            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    options.MaxRetryAttempts,
                    attempt =>
                    {
                        if (!options.UseExponentialBackoff)
                            return options.InitialDelay;

                        return TimeSpan.FromSeconds(
                            Math.Pow(options.InitialDelay.TotalSeconds, attempt));
                    },
                    (exception, delay, attempt, _) =>
                    {
                        logger.LogWarning(
                            exception,
                            "Retry attempt {Attempt} after {Delay}",
                            attempt,
                            delay);
                    });
        }

        public Task ExecuteAsync(Func<Task> action,CancellationToken cancellationToken = default)=> _policy.ExecuteAsync(action);
    }
}
