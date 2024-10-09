using Microsoft.AspNetCore.Mvc;
using TyreCompare.Log;

namespace TyreCompare.API.Controllers;

[ApiController]
[ApiVersion("0.5"), ApiVersion("1.0"), ApiVersion("1.5")]
[Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class LogController : Controller
{
    private IFileLogger Logger;

    public LogController(IFileLogger logger)
    {
        Logger = logger;
    }

    [HttpGet]
    public IActionResult GetLogFileNames()
    {
        var logFolder = Path.GetDirectoryName(Logger.GetCurrentLogPath());
        var logFileNames = Directory.GetFiles(logFolder).ToList().Select(x => Path.GetFileName(x));

        return Ok(logFileNames);
    }

    [HttpGet("{fileName}")]
    public IActionResult GetLogFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        { throw new ArgumentException("Log file name is invalid."); }

        var logFolder = Path.GetDirectoryName(Logger.GetCurrentLogPath());
        var logFilePath = Path.Combine(logFolder, fileName);
        var fileData = System.IO.File.ReadAllBytes(logFilePath);

        return File(fileData, "text/plain", fileName);
    }
}
