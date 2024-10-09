using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TyreCompare.API.CustomMiddleware;
using TyreCompare.API.HelperClasses;
using TyreCompare.BCL;
using TyreCompare.DAL;
using TyreCompare.DAL.Interfaces;
using TyreCompare.DAL.EFCore;
using TyreCompare.Log;
using ILog = TyreCompare.Log.ILog;
using TyreCompare.DAL.Dapper;
using Microsoft.AspNetCore.Mvc.Versioning;
using TyreCompare.DAL.BaseClasses;

public class Program
{
    public static void Main(string[] args)
    {

        // Create web builder
        var webAppBuilder = WebApplication.CreateBuilder(args);
        var fileLogPath = ConfigHelper.GetLogFolderPath();
        var startupLogger = new FileLogger(fileLogPath);
        try
        {
            // Service Builder
            webAppBuilder.Services.AddApiVersioning(options =>
            {
                // Latest: 1.0 | Previous: 0.5 | Testing: 1.5
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ReportApiVersions = true;
            });
            webAppBuilder.Services.AddScoped<EntryMiddleware>();
            webAppBuilder.Services.AddScoped<IExceptionFilter, ExceptionFilter>();
            webAppBuilder.Services.AddScoped<ITyreCompareService, TyreCompareService>();
            webAppBuilder.Services.AddScoped<ILoggingService, LoggingService>();
            webAppBuilder.Services.AddScoped<ILog, AppLogger>();
            webAppBuilder.Services.AddScoped<IDBLogger, DBLogger>();
            webAppBuilder.Services.AddSingleton<IFileLogger, FileLogger>(x => new FileLogger(fileLogPath));
            webAppBuilder.Services.AddSingleton<ICacheService, MemoryCacheService>();
            webAppBuilder.AddTyreCompareContexts();
            webAppBuilder.Services.AddJwtAuthentication();
            webAppBuilder.Services.AddAuthorization();
            webAppBuilder.Services.AddControllers(options => options.Filters.Add(new TypeFilterAttribute(typeof(ExceptionFilter))));
            webAppBuilder.Services.AddCors(options => options.AddPolicy("DefaultCors", policy =>
            {
                policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("isauthenticated");
            }));
            webAppBuilder.Services.AddLogging(x => x.AddConsole());
            webAppBuilder.Services.AddMemoryCache();
            var webApp = webAppBuilder.Build();

            // Middleware Pipeline
            webApp.UseMiddleware<EntryMiddleware>();
            webApp.UseCors("DefaultCors");
            webApp.UseAuthentication();
            webApp.UseAuthorization();
            webApp.UseHttpLogging();
            webApp.MapControllers();

            webApp.Run();
        }
        catch (Exception ex)
        {
            startupLogger.Log(ex);
            throw;
        }
    }
}
