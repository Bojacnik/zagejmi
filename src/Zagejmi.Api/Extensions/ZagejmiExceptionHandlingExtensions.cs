using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Zagejmi.Api.Extensions;

/// <summary>
///     Provides extension methods for configuring global exception handling middleware in the ASP.NET Core application.
/// </summary>
public static class ExceptionHandlingApplicationBuilderExtensions
{
    private static readonly Action<ILogger, PathString, Exception?> LogUnhandledExceptionProcessingPath =
        LoggerMessage.Define<PathString>(
            LogLevel.Error,
            new EventId(1, nameof(LogUnhandledExceptionProcessingPath)),
            "Unhandled exception processing {Path}");

    /// <summary>
    ///     Registers global exception handling middleware that catches unhandled exceptions,
    ///     logs them, and returns a standardized <c>application/problem+json</c> 500 response.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder" /> instance to configure.</param>
    /// <returns>The configured <see cref="IApplicationBuilder" /> instance.</returns>
    public static IApplicationBuilder UseZagejmiExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                // Retrieve the exception details from the context features
                Exception? exception = context.Features
                    .Get<IExceptionHandlerFeature>()?
                    .Error;

                // Create a logger instance to log the exception details for debugging and monitoring purposes
                ILogger logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");

                if (exception is not null)
                {
                    LogUnhandledExceptionProcessingPath(logger, context.Request.Path, exception);
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        title = "Unexpected server error",
                        status = StatusCodes.Status500InternalServerError,
                        detail = "An unexpected error occurred while processing the request.",
                    });
            });
        });

        return app;
    }
}
