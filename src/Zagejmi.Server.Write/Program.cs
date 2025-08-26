using System.Text;
using LanguageExt;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Zagejmi.Server.Application.CommandHandlers;
using Zagejmi.Server.Application.CommandHandlers.User;
using Zagejmi.Server.Application.CommandHandlers.People;
using Zagejmi.Server.Application.Commands.User;
using Zagejmi.Server.Application.Commands.Person;
using Zagejmi.Server.Application.EventStore;
using Zagejmi.Server.Domain.Auth;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure;
using Zagejmi.Server.Infrastructure.Auth;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Infrastructure.EventStore;
using Zagejmi.Server.Infrastructure.Repository.User;
using Zagejmi.Server.Infrastructure.Repository.People;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.Server.Infrastructure.Projections;
using Zagejmi.SharedKernel.Util;

namespace Zagejmi.Server.Write;

public static class Program
{
    public static void Main(string[] args)
    {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // 1. Add services to the container.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
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
            //x.AddConsumer<UserProjection>();

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
                cfg.Host("localhost", "/", h =>
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

        // Register application services
        builder.Services.AddScoped<IRepositoryUserWrite, RepositoryUserWrite>();
        builder.Services.AddScoped<IRepositoryPersonWrite, RepositoryPersonWrite>();
        builder.Services.AddScoped<IHandlerRequest<CommandUserCreate, Either<Failure, Guid>>, HandlerUserCreate>();
        builder.Services.AddScoped<IHandlerRequest<CommandPersonCreate, Either<Failure, Guid>>, HandlerPersonCreate>();
        builder.Services.AddScoped<IHandlerRequest<CommandUserLogin, Either<Failure, string>>, HandlerUserLogin>();
        builder.Services.AddScoped<IHashHandler, HashHandler>();
        
        // Register the generic EventStore implementation
        builder.Services.AddScoped<IEventStore<User, Guid>, EventStore<User, Guid>>();
        builder.Services.AddScoped<IEventStore<Person, Guid>, EventStore<Person, Guid>>();

        // Register Mapper
        builder.Services.AddScoped<IMapper, Mapper>();

        // 2. Build the application.
        WebApplication app = builder.Build();

        // 3. Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Zagejmi.Server.Write v1");
                options.RoutePrefix = string.Empty; // Set to empty string to serve at root
            });
        }

        app.UseExceptionHandler("/Error");
        app.UseHsts();
        app.UseHttpsRedirection();

        app.UseCors(MyAllowSpecificOrigins);

        // Add authentication and authorization to the pipeline
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapControllers();

        app.Run();
    }
}
