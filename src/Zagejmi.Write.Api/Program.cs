using System;
using System.IO;

using LanguageExt;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Shared.Util;
using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Application.Handlers.User;
using Zagejmi.Write.Infrastructure.Auth;
using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.EventStore;

namespace Zagejmi.Write.Api;

/// <summary>
///     The main entry point for the Zagejmi.Write.Api application.
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
        builder.Services.AddControllers();

        // Add Swagger/OpenAPI services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Use Npgsql for PostgreSQL / TimescaleDB
        builder.Services.AddDbContext<ZagejmiWriteContext>(options => options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure data protection to persist keys in a cross-platform location
        string keyStorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zagejmi",
            "DataProtection");

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyStorePath))
            .SetApplicationName("ZagejmiShared");

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/api/user/logout";
                options.AccessDeniedPath = "/";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Domain = "localhost";
                options.Cookie.Path = "/";
            });

        // Register the event store
        builder.Services.AddScoped<IEventStore, EventStore>();

        // Register Mapper
        builder.Services.AddScoped<IMapper, SimpleMapper>();

        // Register abstractions and implementations
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Register Command Handlers
        builder.Services
            .AddScoped<IHandlerRequest<UserCreateCommand, Either<Failure, Guid>>, UserCreateCommandHandler>();
        builder.Services
            .AddScoped<IHandlerRequest<UserLoginCommand, Either<Failure, string>>, UserLoginCommandHandler>();

        // 2. Build the application.
        WebApplication app = builder.Build();

        // 3. Configure the HTTP request pipeline.

        // Enable Swagger middleware only in development environment
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("./swagger/v1/swagger.json", "Zagejmi.Write.Api v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseExceptionHandler("/Error");
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