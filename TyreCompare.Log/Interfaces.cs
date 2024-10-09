namespace TyreCompare.Log;

public interface ILog
{
    Task<bool> Log(string message, string messageDetail = "");
    Task<bool> Log(Exception exception, string exceptionHeader = "");
}

public interface IDBLogger : ILog
{ }

public interface IFileLogger : ILog
{
    string GetCurrentLogPath();
}

public interface ILoggingService
{
    Task<bool> Log(string message, string messageDetail = "");
    Task<bool> Log(Exception exception, string exceptionHeader = "");
}