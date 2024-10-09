namespace TyreCompare.Log;

public class DBLogger : IDBLogger
{
    private readonly ILoggingService LoggingService;

    public DBLogger(ILoggingService loggingService)
    {
        LoggingService = loggingService;
    }

    public Task<bool> Log(string message, string messageDetail = "")
    {
        return LoggingService.Log(message, messageDetail);
    }

    public Task<bool> Log(Exception exception, string exceptionHeader = "")
    {
        return LoggingService.Log(exception, exceptionHeader);
    }
}
