using System;
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zagejmi.Api.Services;
using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Handlers.Service;
using Zagejmi.Application.Handlers.User;
using Zagejmi.Infrastructure.Auth;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Utility;

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
    public static void Main(string[] args)
    {
        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Configure routing to use lowercase URLs for consistency
        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        // 1. Add services to the container.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                myAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("http://localhost:5271", "https://localhost:7014")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAntiforgery();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(Read.Api.Controllers.UserProfilesController).Assembly);

        // Add Swagger/OpenAPI services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            string basePath = AppContext.BaseDirectory;
            string writeXml = Path.Combine(basePath, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            string contractsXml = Path.Combine(basePath, "Zagejmi.Contracts.xml");

            if (File.Exists(writeXml)) options.IncludeXmlComments(writeXml);

            if (File.Exists(contractsXml)) options.IncludeXmlComments(contractsXml);
        });

        // Use Npgsql for PostgreSQL / TimescaleDB
        builder.Services.AddDbContext<ZagejmiContext>(options => options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "write")));

        // Read-side DbContext (merged)
        builder.Services.AddDbContext<ZagejmiReadContext>(options => options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "read")));

        // Configure data protection to persist keys in a cross-platform location
        string keyStorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zagejmi",
            "DataProtection");

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyStorePath))
            .SetApplicationName("ZagejmiShared");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        string? jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                        if (string.IsNullOrWhiteSpace(jti))
                        {
                            context.Fail("Token is missing jti claim.");
                            return;
                        }

                        ZagejmiContext dbContext = context.HttpContext.RequestServices
                            .GetRequiredService<ZagejmiContext>();

                        bool isRevoked = await dbContext.RevokedAccessTokens
                            .AsNoTracking()
                            .AnyAsync(x => x.JwtId == jti && x.ExpiresAtUtc > DateTimeOffset.UtcNow);

                        if (isRevoked) context.Fail("Token has been revoked.");
                    },
                };
            });

        // Register the event store
        builder.Services.AddScoped<IEventStore, EventStore>();
        builder.Services.Configure<RabbitMqProjectionOptions>(builder.Configuration.GetSection("RabbitMq"));
        builder.Services.AddScoped<IStoredEventPublisher, RabbitMqStoredEventPublisher>();

        // Register Mapper
        builder.Services.AddScoped<IMapper, SimpleMapper>();

        // Register abstractions and implementations
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEmailVerificationTokenService, EmailVerificationTokenService>();
        builder.Services.AddScoped<IEmailVerificationLinkFactory, EmailVerificationLinkFactory>();
        builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
        builder.Services.AddScoped<IProfileImageStorage, FileSystemProfileImageStorage>();

        // Read-side services (merged)
        builder.Services.AddScoped<ProfileProjectionApplier>();
        builder.Services.AddScoped<ServiceProjectionApplier>();
        builder.Services.AddScoped<ProfileProjectionRepairService>();
        builder.Services.AddScoped<AssociateProjectionRepairService>();
        builder.Services.AddScoped<ReadOnlyProfileImageStorage>();
        builder.Services.AddScoped<ReadOnlyServiceImageStorage>();
        builder.Services.AddScoped<UserReadRepository>();
        builder.Services.Configure<Read.Api.Services.RabbitMqProjectionOptions>(
            builder.Configuration.GetSection("RabbitMq"));
        builder.Services.AddHostedService<ProfileProjectionBootstrapService>();
        builder.Services.AddHostedService<ProfileProjectionRabbitMqConsumerService>();

        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
        builder.Services.Configure<EmailVerificationOptions>(builder.Configuration.GetSection("EmailVerification"));

        // Register Command Handlers
        builder.Services
            .AddScoped<IHandlerRequest<UserCreateCommand, (? Result, Failure? Error)>, UserCreateCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserCreateProfileCommand, (? Result, Failure? Error)>,
                UserCreateProfileCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserUpdateProfileImageCommand, (? Result, Failure? Error)>,
                UserUpdateProfileImageCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserRemoveProfileImageCommand, (? Result, Failure? Error)>,
                UserRemoveProfileImageCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserUpdateProfileBannerCommand, (? Result, Failure? Error)>,
                UserUpdateProfileBannerCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserCreateServiceCommand, (? Result, Failure? Error)>,
                UserCreateServiceCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserLoginCommand, (? Result, Failure? Error)>, UserLoginCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserSendEmailVerificationCommand, (? Result, Failure? Error)>,
                UserSendEmailVerificationCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserConfirmEmailVerificationCommand, (? Result, Failure? Error)>,
                UserConfirmEmailVerificationCommandHandler>();

        builder.Services.AddScoped<SidebarStateService>();

        // 2. Build the application.
        WebApplication app = builder.Build();

        // 3. Configure the HTTP request pipeline.

        // Enable Swagger middleware only in development environment
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("./swagger/v1/swagger.json", "Zagejmi.Api v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
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
        app.UseHsts();
        app.UseHttpsRedirection();

        // Add CORS middleware BEFORE authentication
        app.UseCors(myAllowSpecificOrigins);

        // Add authentication and authorization to the pipeline
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapControllers();

        app.Run();
    }
}