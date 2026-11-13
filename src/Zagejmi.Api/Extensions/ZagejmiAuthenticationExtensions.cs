using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Zagejmi.Infrastructure.Auth.AccessTokens.Ctx;

namespace Zagejmi.Api.Extensions;

/// <summary>
///     Provides extension methods for configuring Zagejmi authentication.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    ///     Configures JWT authentication for the Zagejmi application using the provided configuration settings.
    /// </summary>
    /// <param name="services">Service collection to which authentication services will be added.</param>
    /// <param name="configuration">The configuration settings for JWT authentication.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddZagejmiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
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

                        AccessTokensContext dbContext = context.HttpContext.RequestServices
                            .GetRequiredService<AccessTokensContext>();

                        bool isRevoked = await dbContext.RevokedAccessTokens
                            .AsNoTracking()
                            .AnyAsync(x => x.JwtId == jti && x.ExpiresAt > DateTimeOffset.UtcNow);

                        if (isRevoked)
                        {
                            context.Fail("Token has been revoked.");
                        }
                    },
                };
            });

        return services;
    }
}