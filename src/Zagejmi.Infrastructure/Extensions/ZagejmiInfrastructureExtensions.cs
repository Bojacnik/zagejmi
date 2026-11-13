using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Minio;

using Zagejmi.Application.Auth.Passwords;
using Zagejmi.Application.Auth.Tokens;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Images;
using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Infrastructure.Auth.AccessTokens;
using Zagejmi.Infrastructure.Auth.Passwords;
using Zagejmi.Infrastructure.Auth.Users;
using Zagejmi.Infrastructure.Images.Processing;
using Zagejmi.Infrastructure.Images.Repositories;
using Zagejmi.Infrastructure.Minio;
using Zagejmi.Infrastructure.Profiles.Repositories;

namespace Zagejmi.Infrastructure.Extensions;

/// <summary>
///     Registers infrastructure implementations of application abstractions (repositories, hashing, object storage).
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    ///     Adds user and profile persistence and password hashing services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddZagejmiUsers(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

        return services;
    }

    /// <summary>
    ///     Adds image metadata persistence and MinIO-backed image storage. Reads the <c>Minio</c> configuration section
    ///     (<c>Endpoint</c>, <c>AccessKey</c>, <c>SecretKey</c>, <c>UseSsl</c>).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddZagejmiImages(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection minio = configuration.GetSection("Minio");
        string endpoint = minio["Endpoint"] ?? "localhost:9000";
        string accessKey = minio["AccessKey"] ?? throw new InvalidOperationException("Minio:AccessKey is not configured.");
        string secretKey = minio["SecretKey"] ?? throw new InvalidOperationException("Minio:SecretKey is not configured.");
        bool useSsl = minio.GetValue<bool>("UseSsl");

        services.AddSingleton<IMinioClient>(_ => new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSsl)
            .Build());

        services.AddScoped<IImageStorage, MinioImageStorage>();
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();
        services.AddScoped<ImageUploader>();
        services.AddScoped<IImageBlobRepository, ImageBlobRepository>();

        return services;
    }
}
