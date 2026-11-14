using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Minio;

using Zagejmi.Application.Admin;
using Zagejmi.Application.Auth.Passwords;
using Zagejmi.Application.Auth.Tokens;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Images;
using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Application.Orders;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Application.Wallets;
using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Infrastructure.Auth.AccessTokens.Repositories;
using Zagejmi.Infrastructure.Auth.Passwords;
using Zagejmi.Infrastructure.Auth.Users.Repositories;
using Zagejmi.Infrastructure.Images.Processing;
using Zagejmi.Infrastructure.Images.Repositories;
using Zagejmi.Infrastructure.Images.Storage;
using Zagejmi.Infrastructure.Profiles.Repositories;
using Zagejmi.Infrastructure.Wallets;
using Zagejmi.Infrastructure.Wallets.Repositories;

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
        services.AddScoped<IPlayerReviewRepository, PlayerReviewRepository>();
        services.AddScoped<IServiceReviewRepository, ServiceReviewRepository>();
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

    /// <summary>
    ///     Adds the Goin ledger, the wallet rules (configuration section <c>Wallets</c>), orders (<c>Orders</c>, admins in
    ///     <c>Admin</c>) and the background jobs that expire bonus Goins and time out orders.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddZagejmiWallets(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WalletOptions>(configuration.GetSection(WalletOptions.SectionName));
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddHostedService<BonusExpiryService>();

        // Orders live next to the ledger (their money moves in the same database transaction).
        services.Configure<OrderOptions>(configuration.GetSection(OrderOptions.SectionName));
        services.Configure<AdminOptions>(configuration.GetSection(AdminOptions.SectionName));
        services.Configure<ReviewOptions>(configuration.GetSection(ReviewOptions.SectionName));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddHostedService<OrderTimeoutService>();

        return services;
    }
}
