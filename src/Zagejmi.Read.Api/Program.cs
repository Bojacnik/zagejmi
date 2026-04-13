using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using Serilog;

using Zagejmi.Read.Api.Components;
using Zagejmi.Read.Api.Ctx;
using Zagejmi.Read.Api.Services;

using _Imports = Zagejmi.Read.Client._Imports;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add HttpContextAccessor to access the current HttpContext from services
builder.Services.AddHttpContextAccessor();

// Register controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string basePath = AppContext.BaseDirectory;
    string readXml = Path.Combine(basePath, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    string contractsXml = Path.Combine(basePath, "Zagejmi.Contracts.xml");

    if (File.Exists(readXml))
    {
        options.IncludeXmlComments(readXml);
    }

    if (File.Exists(contractsXml))
    {
        options.IncludeXmlComments(contractsXml);
    }
});

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
builder.Services.AddScoped<ProfileStateService>();
builder.Services.AddScoped<ProfileProjectionApplier>();
builder.Services.Configure<RabbitMqProjectionOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<ProfileProjectionBootstrapService>();
builder.Services.AddHostedService<ProfileProjectionRabbitMqConsumerService>();

// Add custom AuthenticationStateProvider for Blazor InteractiveServer
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Zagejmi",
                "DataProtection")))
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });

builder.Services.AddAuthorization();

// Register DbContext for ZagejmiReadContext
builder.Services.AddDbContext<ZagejmiReadContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "read")));

// Read-side login is delegated to Write.Api; keep projection services for read features only.
WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    ZagejmiReadContext dbContext = scope.ServiceProvider.GetRequiredService<ZagejmiReadContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

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