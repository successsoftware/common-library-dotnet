using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using SSS.AspNetCore.Extensions.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;

namespace SSS.AspNetCore.Extensions.Handlers
{
    public static class GlobalExceptionConfig
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder app,
            NamingStrategy namingStrategy = default,
            bool enableDiagnostics = false)
        {
            if (namingStrategy != default)
            {
                GlobalExceptionHandling.NamingPolicy = namingStrategy;
            }

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    Trace.TraceError("Global exception handling...");

                    var logger = LoggerFactory.Create(builder =>
                    {
#if DEBUG
                        builder.AddConsole();
#endif
                        builder.AddApplicationInsights();
                    }).CreateLogger(nameof(GlobalExceptionHandling));

                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();

                    logger.LogError("Error message @{ErrorMessage}", exceptionHandlerPathFeature?.Error?.Message);

                    if (enableDiagnostics)
                    {
                        Trace.TraceError(exceptionHandlerPathFeature?.Error?.Message);
                    }

                    var problemDetails = (exceptionHandlerPathFeature?.Error) switch
                    {
                        ArgumentNullException ex => ex.GetProblemDetails(),
                        ArgumentException ex => ex.GetProblemDetails(),
                        ValidationException ex => ex.GetProblemDetails(),
                        FluentValidation.ValidationException ex => ex.GetProblemDetails(),
                        ApplicationException ex => ex.GetProblemDetails(),
                        AggregateException ex => ex.GetProblemDetails(),
                        Refit.ApiException ex => ex.GetProblemDetails(),
                        BadRequestException ex => ex.GetProblemDetails(),
                        NotFoundException ex => ex.GetProblemDetails(),
                        Exception others => new ProblemDetails
                        {
                            Status = (int)HttpStatusCode.InternalServerError,
                            ErrorCode = nameof(HttpStatusCode.InternalServerError),
                            ErrorMessage = "An error occurred. Please try again later."
                        }
                    };

                    problemDetails.ErrorCode = problemDetails?.ErrorCode?.ToLower();

                    if (string.IsNullOrEmpty(problemDetails.TraceId))
                        problemDetails.TraceId = Activity.Current?.RootId ?? Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

                    if (problemDetails?.Status == (int)HttpStatusCode.InternalServerError)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                    if (problemDetails?.Status.GetValueOrDefault() == 0)
                    {
                        problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    }

                    context.Response.StatusCode = (int)problemDetails.Status;

                    context.Response.ContentType = "application/json";

                    if (enableDiagnostics)
                    {
                        Trace.WriteLine(problemDetails?.ToJSON(enableIndented: true));
                    }

                    await context.Response.WriteAsync(problemDetails?.ToJSON()).ConfigureAwait(false);
                });
            });

            return app;
        }
    }
}