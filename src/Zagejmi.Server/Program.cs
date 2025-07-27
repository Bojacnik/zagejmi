using System.Text;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using Zagejmi.Application;
using Zagejmi.Application.CommandHandlers.People;
using Zagejmi.Components;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Events.EventStore;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Persistance;
using Zagejmi.Infrastructure.Repository.Person;
using Zagejmi.Infrastructure.EventBus;
using Zagejmi.Infrastructure.EventStore;
using Zagejmi.Components.Services;


namespace Zagejmi;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddDbContext<ZagejmiContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services
            .AddScoped<IEventBusProducer<Person, Guid>, EventBusProducer>()
            .AddScoped<IEventStore<Person, Guid>, EventStore<Person, Guid>>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IRepositoryPersonWrite, RepositoryPersonWrite>()
            .AddScoped<IMediator, Mediator>()
            .AddScoped<IServiceUser, ServiceUser>();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
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
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

        builder.Services.AddMassTransit(x =>
        {
            // Add your consumers
            x.AddConsumer<HandlerPersonCreate>();

            x.AddEntityFrameworkOutbox<ZagejmiContext>(o =>
            {
                // How often the outbox delivery service polls for new messages
                o.QueryDelay = TimeSpan.FromSeconds(10);

                o.UsePostgres();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }
}