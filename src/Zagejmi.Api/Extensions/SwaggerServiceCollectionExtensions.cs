using System;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Zagejmi.Api.Extensions;

// TODO: Make more configurable

/// <summary>
///     Provides extension methods for configuring Swagger services in the Zagejmi.Api application.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    ///     Configures and adds Swagger services to the service collection, enabling API documentation and exploration for the
    ///     Zagejmi.Api application. This method sets up the necessary services for generating Swagger/OpenAPI documentation,
    ///     including XML comments from the executing assembly and the Zagejmi.Contracts assembly, if available. It ensures
    ///     that the API endpoints are properly documented and accessible through the Swagger UI, facilitating easier testing
    ///     and integration for developers and clients.
    /// </summary>
    /// <param name="services">The service collection to which the Swagger services will be added.</param>
    /// <returns>The updated service collection with Swagger services configured.</returns>
    public static IServiceCollection AddZagejmiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            string basePath = AppContext.BaseDirectory;
            string writeXml = Path.Combine(basePath, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            string contractsXml = Path.Combine(basePath, "Zagejmi.Contracts.xml");

            if (File.Exists(writeXml))
            {
                options.IncludeXmlComments(writeXml);
            }

            if (File.Exists(contractsXml))
            {
                options.IncludeXmlComments(contractsXml);
            }
        });

        return services;
    }
}