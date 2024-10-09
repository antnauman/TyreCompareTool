using TyreCompare.DAL.Interfaces;
using TyreCompare.Log;

namespace TyreCompare.DAL;

public class LoggingService : ILoggingService
{
    private readonly ILoggingRepository LoggingRepository;

    public LoggingService(ILoggingRepository loggingRepository)
    {
        LoggingRepository = loggingRepository;
    }

    public async Task<bool> Log(string message, string messageDetail = "")
    {
        if (string.IsNullOrWhiteSpace(message))
        { message = "Log Message was not provided."; }

        var logObject = new Models.Log()
        {
            LogLevel = "Information",
            Message = message,
            Trace = messageDetail,
            CreatedDate = DateTime.UtcNow
        };

        return await LoggingRepository.AddLog(logObject);
    }

    public async Task<bool> Log(Exception exception, string exceptionHeader = "")
    {
        if (exception == null)
        { new Exception("Log Exception was not provided."); }

        var logObject = new Models.Log()
        {
            LogLevel = "Exception",
            Message = exceptionHeader + exception.Message,
            Trace = exception.StackTrace,
            CreatedDate = DateTime.UtcNow
        };

        return await LoggingRepository.AddLog(logObject);
    }
}
