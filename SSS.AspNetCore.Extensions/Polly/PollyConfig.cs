using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace SSS.AspNetCore.Extensions.Polly
{
    public static class PollyConfig
    {
        public static IServiceCollection AddHttpClientWithPolly(this IServiceCollection services, Uri baseUri, string clientName)
        {
            var loggerFactory = services.BuildServiceProvider().CreateScope().ServiceProvider.GetService<ILoggerFactory>();

            var builder = services.AddHttpClient(clientName, c => c.BaseAddress = baseUri);

            builder.AddPolicyHandler(ResiliencePolicies.GetRetryPolicy(loggerFactory));

            return services;
        }
    }
}
