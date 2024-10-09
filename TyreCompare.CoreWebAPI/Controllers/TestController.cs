using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TyreCompare.BCL;
using TyreCompare.DAL.EFCore;
using TyreCompare.DAL.Interfaces;
using TyreCompare.Log;
using System.Reflection;

namespace TyreCompare.API.Controllers;

[AllowAnonymous]
[ApiController]
[ApiVersion("0.5"), ApiVersion("1.0"), ApiVersion("1.5")]
[Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class TestController : Controller
{
    private ITyreCompareService DataAccessLayer;
    private IFileLogger Logger;

    public TestController(ITyreCompareService dataAccessLayer, IFileLogger logger)
    {
        DataAccessLayer = dataAccessLayer;
        Logger = logger;
    }

    [HttpGet]
    public IActionResult Test()
    {
        var message = "This is the current Test Controller.";
        return Ok(message);
    }

    [HttpGet]
    [MapToApiVersion("1.5")]
    public IActionResult Test_Test()
    {
        var message = "This is the testing Test Controller.";
        return Ok(message);
    }

    [HttpGet]
    [MapToApiVersion("0.5")]
    public IActionResult Test_Old()
    {
        var message = "This is the previous Test Controller.";
        return Ok(message);
    }

    [HttpGet]
    [Route("Version/{project}")]
    public IActionResult GetVersion([FromRoute] string project)
    {
        try
        {
            Assembly? assembly = null;
            string selectedProject = string.Empty;

            switch (project.ToUpper())
            {
                case "BCL":
                    selectedProject = "BCL";
                    assembly = typeof(TyreCompare.BCL.FileHelper).Assembly;
                    break;
                case "DAL":
                    selectedProject = "DAL";
                    assembly = typeof(TyreCompare.DAL.TyreCompareService).Assembly;
                    break;
                case "IMAGEPROCESSOR":
                    selectedProject = "ImageProcessor";
                    assembly = typeof(TyreCompare.ImageProcessor.ImageUtilities).Assembly;
                    break;
                case "LOG":
                    selectedProject = "Log";
                    assembly = typeof(TyreCompare.Log.AppLogger).Assembly;
                    break;
                case "MODELS":
                    selectedProject = "Models";
                    assembly = typeof(TyreCompare.Models.ITyre).Assembly;
                    break;
                case "API":
                default:
                    selectedProject = "API";
                    assembly = Assembly.GetEntryAssembly();
                    break;
            }

            var version = assembly?.GetName().Version;
            return Ok($"{selectedProject} project version: {version}");
        }
        catch (Exception ex)
        {
            return StatusCode((int)ex.GetAppropriateStatusCode(), ex.Message);
        }
    }

    [HttpGet]
    [Route("Log")]
    public IActionResult TestLogging()
    {
        try
        {
            Logger.Log(DateTime.UtcNow.ToString());
            return Ok($"Logged UTC DateTime at {Logger.GetCurrentLogPath()}");
        }
        catch (Exception ex)
        {
            return StatusCode((int)ex.GetAppropriateStatusCode(), ex.Message);
        }
    }

    [HttpGet]
    [Route("DbConnection")]
    public IActionResult TestDbConnection()
    {
        try
        {
            var message = DataAccessLayer.TestDbConnection();
            Logger.Log(message);
            return Ok(message);
        }
        catch (Exception ex)
        {
            Logger.Log("Cannot initialize DbConnection.");
            Logger.Log(ex);
            return StatusCode((int) ex.GetAppropriateStatusCode(), ex.Message);
        }
    }

    [HttpGet]
    [Route("AppSettings/{key}")]
    public IActionResult GetAppSettings([FromRoute] string key)
    {
        try
        {
            var value = ConfigHelper.GetConfigurationValue("AppSettings", key, "pa$$word");
            return Ok(value);
        }
        catch (Exception ex)
        {
            Logger.Log("Cannot get settings.");
            Logger.Log(ex);
            return StatusCode((int)ex.GetAppropriateStatusCode(), ex.Message);
        }
    }

    [HttpGet]
    [Route("ConnectionString")]
    public IActionResult GetConnectionString()
    {
        try
        {
            var connectionString = ConfigHelper.GetConfigurationValue("ConnectionStrings", "HostServerCS", "pa$$word");
            Logger.Log(connectionString);
            return Ok(connectionString);
        }
        catch (Exception ex)
        {
            Logger.Log("Cannot get connection string.");
            Logger.Log(ex);
            return StatusCode((int)ex.GetAppropriateStatusCode(), ex.Message);
        }
    }

    [HttpPost]
    [Route("ConnectionString")]
    public IActionResult TestConnectionString([FromBody] string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        { throw new ArgumentException("Connection string is empty"); }

        try
        {
            var dbContext = new TyreCompareContext_EFCore(connectionString);
            var repository = new TyreCompareRepository_EFCore(dbContext);
            var message = repository.TestDbConnection();
            Logger.Log(message);
            return Ok(message);
        }
        catch (Exception ex)
        {
            Logger.Log("Cannot initialize DbContext.");
            Logger.Log(ex);
            return StatusCode((int)ex.GetAppropriateStatusCode(), ex.Message);
        }
    }
}
