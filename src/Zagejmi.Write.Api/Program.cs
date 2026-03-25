using System;
using System.IO;

using MassTransit;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zagejmi.Contracts.Util;
using Zagejmi.Write.Application.EventStore;
using Zagejmi.Write.Domain.Auth;
using Zagejmi.Write.Domain.Profile;
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
        builder.Services.AddDbContext<ZagejmiContext>(options => options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(@"D:\temp-keys"))
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

        builder.Services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<ZagejmiContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(10);

                // Use Postgres for the outbox to match the main DbContext
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                // Reverting to the explicit host configuration
                cfg.Host(
                    "localhost",
                    "/",
                    h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                /*
                cfg.ReceiveEndpoint("user-created-events", e =>
                {
                    e.ConfigureConsumer<UserProjection>(context);
                });
                */

                cfg.ConfigureEndpoints(context);
            });
        });

        // Register the generic EventStore implementation
        builder.Services.AddScoped<IEventStore<User, Guid>, EventStore<User, Guid>>();
        builder.Services.AddScoped<IEventStore<Profile, Guid>, EventStore<Profile, Guid>>();

        // Register Mapper
        builder.Services.AddScoped<IMapper, SimpleMapper>();

        // 2. Build the application.
        WebApplication app = builder.Build();

        // 3. Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Zagejmi.Write.Api v1");
                options.RoutePrefix = string.Empty; // Set to empty string to serve at root
            });
        }

        app.UseExceptionHandler("/Error");
        app.UseHsts();
        app.UseHttpsRedirection();

        app.UseCors(myAllowSpecificOrigins);

        // Add authentication and authorization to the pipeline
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapControllers();

        app.Run();
    }
}