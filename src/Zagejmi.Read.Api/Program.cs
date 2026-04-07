using System;
using System.IO;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Zagejmi.Read.Api.Components;
using Zagejmi.Read.Api.Services;

using _Imports = Zagejmi.Read.Client._Imports;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add HttpContextAccessor to access the current HttpContext from services
builder.Services.AddHttpContextAccessor();

// Register controllers
builder.Services.AddControllers();

// Register HttpClient factory
builder.Services.AddHttpClient();

// Get Write.Api base address from configuration
string? writeApiBaseAddress = builder.Configuration["WriteApi:BaseAddress"];
if (string.IsNullOrWhiteSpace(writeApiBaseAddress))
{
    throw new InvalidOperationException(
        "Write API base address is not configured. Please ensure 'WriteApi:BaseAddress' is set in appsettings.json");
}

if (!writeApiBaseAddress.EndsWith('/'))
{
    writeApiBaseAddress += "/";
}

// Register the WriteAPI named HttpClient
builder.Services.AddHttpClient("WriteAPI", client => { client.BaseAddress = new Uri(writeApiBaseAddress); });

builder.Services.AddScoped<SidebarStateService>();

// Add custom AuthenticationStateProvider for Blazor InteractiveServer
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add DbContext for ZagejmiContext
/*
builder.Services.AddDbContext<ZagejmiReadContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    */

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Zagejmi",
                "DataProtection")))
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

// Map controllers for API endpoints
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

app.Run();