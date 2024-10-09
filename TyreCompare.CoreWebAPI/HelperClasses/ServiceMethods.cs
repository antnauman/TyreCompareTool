using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TyreCompare.BCL;
using TyreCompare.DAL.Dapper;
using TyreCompare.DAL.EFCore;
using TyreCompare.DAL.Interfaces;

namespace TyreCompare.API.HelperClasses;

public static class ServiceMethods
{
    public static void AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(ConfigureAuthenticationOptionsMethod)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, ConfigureJwtOptionsMethod);
    }

    private static void ConfigureAuthenticationOptionsMethod(AuthenticationOptions options)
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }

    private static void ConfigureJwtOptionsMethod(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(JwtHelper.GetJwtKey())
        };

        options.Events = new JwtBearerEvents();
        options.Events.OnTokenValidated = (context) =>
        {
            context.Response.Headers.Add("isauthenticated", "true");
            return Task.CompletedTask;
        };

        options.Events.OnAuthenticationFailed = (context) =>
        {
            context.Response.Headers.Add("isauthenticated", "false");
            return Task.CompletedTask;
        };

        options.Events.OnChallenge = (context) =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = JsonConvert.SerializeObject(new { Message = "You are not logged in." });
            return context.Response.WriteAsync(result);
        };
    }

    public static void AddTyreCompareContexts(this WebApplicationBuilder webAppBuilder)
    {
        var connectionString = webAppBuilder.Configuration["ConnectionStrings:HostServerCS"];
        var repositoryType = webAppBuilder.Configuration.GetValue("AppSettings:RepositoryType", "EFCore");

        switch (repositoryType)
        {
            case "Dapper":
                webAppBuilder.Services.AddScoped<ITyreCompareRepository, TyreCompareRepository_Dapper>(provider =>
                { return new TyreCompareRepository_Dapper(connectionString); });
                webAppBuilder.Services.AddScoped<ILoggingRepository, LoggingRepository_Dapper>(provider =>
                { return new LoggingRepository_Dapper(connectionString); });
                break;
            case "EFCore":
            default:
                webAppBuilder.Services.AddDbContext<TyreCompareContext_EFCore>(option =>
                    option.UseSqlServer(connectionString), ServiceLifetime.Transient);
                webAppBuilder.Services.AddScoped<ITyreCompareRepository, TyreCompareRepository_EFCore>();
                webAppBuilder.Services.AddScoped<ILoggingRepository, LoggingRepository_EFCore>();
                break;
        }
    }
}