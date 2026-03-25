using System;
using System.IO;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Zagejmi.Read.Api.Components;
using Zagejmi.Read.Api.Services;
using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Projections;

using _Imports = Zagejmi.Read.Client._Imports;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add HttpContextAccessor to access the current HttpContext from services
builder.Services.AddHttpContextAccessor();

// Correctly register HttpClient for server-side rendering.
builder.Services.AddHttpClient();

// Configure the named HttpClient for the write-side API
builder.Services.AddHttpClient("WriteAPI", client => { client.BaseAddress = new Uri("http://localhost:5178"); });

builder.Services.AddScoped<SidebarStateService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add DbContext for ZagejmiContext
builder.Services.AddDbContext<ZagejmiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"D:\temp-keys"))
    .SetApplicationName("ZagejmiShared");

// Add simple cookie-based Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
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

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add Minimal API endpoint for UserProjection
app.MapGet(
    "/api/users/{username}",
    [Authorize] async (string username, ZagejmiContext context) =>
    {
        UserProjection? user = await context.UserProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == username);

        return user == null ? Results.NotFound() : Results.Ok(user);
    });

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

app.Run();