using System;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zagejmi.Api.Components;
using Zagejmi.Api.Extensions;
using Zagejmi.Api.Filters;
using Zagejmi.Api.Services;
using Zagejmi.Application.Auth.Tokens;
using Zagejmi.Application.Auth.Users.CreateUser;
using Zagejmi.Application.Auth.Users.DeleteUser;
using Zagejmi.Application.Auth.Users.EmailVerification;
using Zagejmi.Application.Auth.Users.LoginUser;
using Zagejmi.Application.Auth.Users.ResendVerificationEmail;
using Zagejmi.Application.Auth.Users.SetDefaultProfile;
using Zagejmi.Application.Auth.Users.SetGhostMode;
using Zagejmi.Application.Auth.Users.VerifyEmail;
using Zagejmi.Application.Profiles.AddPlayerService;
using Zagejmi.Application.Profiles.CreatePlayerProfile;
using Zagejmi.Application.Profiles.DeletePlayerService;
using Zagejmi.Application.Profiles.GetPlayerDetail;
using Zagejmi.Application.Profiles.GetUserProfiles;
using Zagejmi.Application.Profiles.SearchPlayers;
using Zagejmi.Application.Profiles.SearchServices;
using Zagejmi.Application.Profiles.SetPlayerBanner;
using Zagejmi.Application.Profiles.SetProfileImage;
using Zagejmi.Application.Profiles.UpdatePlayerService;
using Zagejmi.Application.Wallets.ExpireBonuses;
using Zagejmi.Application.Wallets.GetWallet;
using Zagejmi.Application.Wallets.GetWalletHistory;
using Zagejmi.Application.Wallets.GrantReward;
using Zagejmi.Infrastructure.Extensions;

namespace Zagejmi.Api;

/// <summary>
///     The main entry point for the Zagejmi.Api application.
///     This class is responsible for configuring and running the web application, including setting up services,
///     middleware, and the HTTP request pipeline.
/// </summary>
public static class Program
{
    /// <summary>
    ///     The main method that configures and runs the web application.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    /// <returns>A task that completes when the application shuts down.</returns>
    public static async Task Main(string[] args)
    {
        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // The app is Czech-native: dates, numbers and currency use cs-CZ everywhere (incl. Blazor circuits).
        CultureInfo czech = CultureInfo.GetCultureInfo("cs-CZ");
        CultureInfo.DefaultThreadCurrentCulture = czech;
        CultureInfo.DefaultThreadCurrentUICulture = czech;

        // Configure routing to use lowercase URLs for consistency
        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        // 1. Add services to the container.
        // Ghost mode is enforced for every controller action; enums are exchanged as strings ("Female", "Feminine").
        builder.Services
            .AddControllers(options => options.Filters.Add<GhostModeFilter>())
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        string[] allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                myAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        builder.Services.AddHttpContextAccessor();

        // Add Swagger
        builder.Services.AddZagejmiSwagger();

        // Add db contexts
        builder.Services.AddZagejmiDbContext(builder.Configuration);

        // Configure data protection to persist keys. "DataProtection:KeysPath" overrides the default location
        // (used by the Docker image, where the app user can only write to its home/volumes).
        string keyStorePath = builder.Configuration["DataProtection:KeysPath"]
                              ?? Path.Combine(
                                  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                  "Zagejmi",
                                  "DataProtection");

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyStorePath))
            .SetApplicationName("Zagejmi");

        // This is a custom extension method that configures authentication services for the application.
        builder.Services.AddZagejmiAuthentication(builder.Configuration);
        builder.Services.AddAuthorization();

        // This is a service that provides functionality for generating and validating tokens used for authentication and authorization.
        builder.Services.AddScoped<ITokenService, TokenService>();

        // This is a custom extension method that configures email services for the application.
        builder.Services.AddZagejmiEmail(builder.Configuration);

        // Infrastructure implementations of application abstractions
        builder.Services.AddZagejmiUsers();
        builder.Services.AddZagejmiImages(builder.Configuration);
        builder.Services.AddZagejmiWallets(builder.Configuration);
        builder.Services.AddSingleton(TimeProvider.System);

        // Application use-case handlers
        builder.Services.AddScoped<CreateUserHandler>();
        builder.Services.AddScoped<DeleteUserHandler>();
        builder.Services.AddScoped<LoginUserHandler>();
        builder.Services.AddScoped<SetDefaultProfileHandler>();
        builder.Services.AddScoped<SetGhostModeHandler>();
        builder.Services.AddScoped<CreatePlayerProfileHandler>();
        builder.Services.AddScoped<GetUserProfilesHandler>();
        builder.Services.AddScoped<SearchPlayersHandler>();
        builder.Services.AddScoped<SearchServicesHandler>();
        builder.Services.AddScoped<GetPlayerDetailHandler>();
        builder.Services.AddScoped<SetProfileImageHandler>();
        builder.Services.AddScoped<SetPlayerBannerHandler>();
        builder.Services.AddScoped<AddPlayerServiceHandler>();
        builder.Services.AddScoped<UpdatePlayerServiceHandler>();
        builder.Services.AddScoped<DeletePlayerServiceHandler>();
        builder.Services.AddScoped<EmailVerificationMailer>();
        builder.Services.AddScoped<VerifyEmailHandler>();
        builder.Services.AddScoped<ResendVerificationEmailHandler>();
        builder.Services.AddScoped<GrantRewardHandler>();
        builder.Services.AddScoped<ExpireBonusesHandler>();
        builder.Services.AddScoped<GetWalletHandler>();
        builder.Services.AddScoped<GetWalletHistoryHandler>();

        // Blazor (interactive server) UI
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

        // HTTP clients used by the Blazor pages. "WriteAPI" is a leftover name from the read/write split and now
        // points back at this same application.
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient(
            "WriteAPI",
            client => client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5178/"));

        // This is a service that manages the state of the sidebar in the application, allowing for dynamic updates and interactions.
        builder.Services.AddScoped<SidebarStateService>();

        // Signed-in user's profiles and the currently active one (Zákazník/Hráč), one instance per circuit.
        builder.Services.AddScoped<ActiveProfileService>();

        // Balance of the active profile's wallet, shown in the top bar and refreshed on profile switches.
        builder.Services.AddScoped<WalletStateService>();

        // Builds the WebApplication instance, which represents the configured web application and its services.
        WebApplication app = builder.Build();

        // Enable Swagger middleware and auto-apply migrations only in development environment
        if (app.Environment.IsDevelopment())
        {
            await app.Services.MigrateZagejmiDatabasesAsync();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.json", "Zagejmi.Api v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.UseZagejmiExceptionHandling();

        // Enable HSTS (HTTP Strict Transport Security) in non-development environments to enforce secure connections
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // Redirect HTTP requests to HTTPS to ensure secure communication
        app.UseHttpsRedirection();

        // Add routing middleware to the pipeline to enable endpoint routing
        app.UseRouting();

        // Add CORS middleware BEFORE authentication
        app.UseCors(myAllowSpecificOrigins);

        // Add authentication and authorization to the pipeline
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        // Map controller routes to handle incoming HTTP requests
        app.MapControllers();

        // Map static assets and the Blazor UI
        app.MapStaticAssets();

        // The JWT lives in browser storage, so the server cannot see it on a full page load. [Authorize] on a page
        // would otherwise turn every reload/forceLoad of e.g. /profile into an empty 401. Pages are protected by
        // AuthorizeRouteView + RedirectToLogin once the interactive circuit can read the token; API controllers keep
        // their own [Authorize].
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AllowAnonymous();

        // Start the application and listen for incoming requests
        await app.RunAsync();
    }
}