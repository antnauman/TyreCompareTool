namespace TyreCompare.Log;

public class AppLogger : ILog
{
    private readonly IDBLogger DbLogger;
    private readonly IFileLogger FileLogger;

    public AppLogger (IDBLogger dbLogger, IFileLogger fileLogger)
    {
        DbLogger = dbLogger;
        FileLogger = fileLogger;
    }

    public async Task<bool> Log(string message, string messageDetail = "")
    {
        try
        {
            await FileLogger.Log(message, messageDetail);
            await DbLogger.Log(message, messageDetail);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> Log(Exception exception, string exceptionHeader = "")
    {
        try
        {
            await FileLogger.Log(exception, exceptionHeader);
            await DbLogger.Log(exception, exceptionHeader);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}