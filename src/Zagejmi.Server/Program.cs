using MassTransit;
using Microsoft.EntityFrameworkCore;
using Zagejmi.Application.CommandHandlers.People;
using Zagejmi.Components;
using Zagejmi.Infrastructure.Ctx;

namespace Zagejmi;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

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

        builder.Services.AddDbContext<ZagejmiContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

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