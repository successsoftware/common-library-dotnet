using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Refit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SSS.AspNetCore.Extensions.Polly
{
    public class ResiliencePolicies
    {
        private const int MAX_TIMEOUT = 60;
        private const int NUMBER_OF_RETRIES = 5;

        private static readonly IEnumerable<TimeSpan> sleepDuration =
            Backoff.ConstantBackoff(TimeSpan.FromSeconds(2), NUMBER_OF_RETRIES);

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ResiliencePolicies>();

            var waitAndRetryPolicy = HttpPolicyExtensions
                                    .HandleTransientHttpError()
                                    .OrResult(res =>
                                        res.StatusCode == HttpStatusCode.Conflict ||
                                        res.StatusCode == HttpStatusCode.TooManyRequests)
                                    .WaitAndRetryAsync(sleepDuration, (resp, ctx) =>
                                    {
                                        Trace.TraceError(resp.Result == null ? resp.Exception.Message : resp.Result.ReasonPhrase, resp.Exception);

                                        logger.LogError("Error message @{ErrorMessage}", resp.Result == null ? resp.Exception.Message : resp.Result.ReasonPhrase);
                                    });

            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(MAX_TIMEOUT));

            var retryWithBackoffAndOverallTimeout = timeout.WrapAsync(waitAndRetryPolicy);

            return retryWithBackoffAndOverallTimeout;
        }

        public static async Task<T> ExecuteResilienceStrategy<T>(Func<Task<T>> action)
        {
            var fallbackPolicy = Policy<T>.Handle<ApiException>(ex => ex.StatusCode.TransientHttpStatusCodePredicate())
                                    .FallbackAsync(fallbackAction: async (ctx, cancellation, result) => await Task.FromResult(default(T)), onFallbackAsync: async (ex, ctx) =>
                                    {
                                        await Task.CompletedTask;

                                        if (!ctx.TryGetLogger(out var logger)) return;

                                        logger.LogError("Error message @{ErrorMessage}", ex.Exception.Message);

                                        Trace.TraceError(ex.Exception.Message, ex.Exception.InnerException);
                                    });

            return await fallbackPolicy.ExecuteAsync(async () => await action());
        }

        public static async Task ExecuteResilienceStrategy(Func<Task> action)
        {
            var fallbackPolicy = Policy.Handle<ApiException>(ex => ex.StatusCode.TransientHttpStatusCodePredicate())
                                    .FallbackAsync(async (ctx, cancellation, result) => await Task.CompletedTask, async (ex, ctx) =>
                                    {
                                        await Task.CompletedTask;

                                        if (!ctx.TryGetLogger(out var logger)) return;

                                        logger.LogError("Error message @{ErrorMessage}", ex.Message);

                                        Trace.TraceError(ex.Message, ex.InnerException);
                                    });

            await fallbackPolicy.ExecuteAsync(async () => await action());
        }
    }

    public static class PollyContextExtension
    {
        public static bool TryGetLogger(this Context context, out ILogger logger)
        {
            if (context.TryGetValue("logger", out var loggerObject) && loggerObject is ILogger theLogger)
            {
                logger = theLogger;
                return true;
            }

            logger = null;
            return false;
        }
    }
}