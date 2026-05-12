using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Text;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Services.Identity;

namespace ADH.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? "default_secret_key_change_me"))
            };
        })
        .AddCookie("Cookies")
        .AddOpenIdConnect("OpenIdConnect", options =>
        {
            options.Authority = configuration["Authentication:Oidc:Authority"];
            options.ClientId = configuration["Authentication:Oidc:ClientId"];
            options.ClientSecret = configuration["Authentication:Oidc:ClientSecret"];
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
        })
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"] ?? "dummy";
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "dummy";
        });

        services.AddScoped<IJwtService, JwtService>();
        
        return services;
    }
}
