using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Zagejmi.Infrastructure.Auth.AccessTokens.Ctx;
using Zagejmi.Infrastructure.Auth.Users;
using Zagejmi.Infrastructure.Images.Ctx;
using Zagejmi.Infrastructure.Profiles.Ctx;

namespace Zagejmi.Infrastructure.Extensions;

// TODO: Make configurable connection string name and schema names for migrations history tables

/// <summary>
///     Provides extension methods for configuring and adding the Zagejmi database contexts to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the Zagejmi database contexts (ZagejmiUsersContext, ProfilesContext, AccessTokensContext, and BlobImagesContext) to the service
    ///     collection using the specified configuration to retrieve the connection string. Each context is configured to use
    ///     PostgreSQL with a specific migrations history table for each context. This method ensures that the database
    ///     contexts are properly registered and available for dependency injection throughout the application.
    /// </summary>
    /// <param name="services">Service collection to which the database contexts will be added.</param>
    /// <param name="configuration">Configuration to retrieve the connection string.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddZagejmiDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        // Factories, not plain scoped contexts: a Blazor Server circuit is one long-lived DI scope, and several
        // components (top bar, page, popup) query at the same time. Repositories create a short-lived context per
        // operation from these factories. AddDbContextFactory also registers each context as scoped for the
        // migrations and the JWT revocation check.
        services.AddDbContextFactory<ZagejmiUsersContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "users")));

        services.AddDbContextFactory<ProfilesContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "profiles")));

        services.AddDbContextFactory<AccessTokensContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "access_tokens")));

        services.AddDbContextFactory<BlobImagesContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "blob_images")));

        return services;
    }

    /// <summary>
    ///     Applies pending EF Core migrations for every Zagejmi database context.
    /// </summary>
    /// <param name="services">The root service provider.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task MigrateZagejmiDatabasesAsync(this IServiceProvider services)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        IServiceProvider provider = scope.ServiceProvider;

        await provider.GetRequiredService<ZagejmiUsersContext>().Database.MigrateAsync();
        await provider.GetRequiredService<ProfilesContext>().Database.MigrateAsync();
        await provider.GetRequiredService<AccessTokensContext>().Database.MigrateAsync();
        await provider.GetRequiredService<BlobImagesContext>().Database.MigrateAsync();
    }
}
