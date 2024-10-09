//using Microsoft.Extensions.Configuration;

namespace TyreCompare.Log;

public class Helper
{
    public static string GetLogFilePath(string logFolderPath)
    {
        var logFileName = GetLogFileName();
        var logFilePath = Path.Combine(logFolderPath, logFileName);
        return logFilePath;
    }

    public static string GetLogFileName()
    {
        string logFileName = @$"{DateTime.Today.Year}{DateTime.Today.Month}{DateTime.Today.Day}.txt";
        return logFileName;
    }

    public static string FormatMessageForLogFile(string message, string messageDetail = "")
    {
        var newMessage = $"\n=== [INFORMATION]: {DateTime.UtcNow} ===\n{message}\n{messageDetail}\n";
        return newMessage;
    }

    public static string FormatExceptionForLogFile(Exception ex, string exceptionHeader)
    {
        var newMessage = $"\n=== [EXCEPTION]: {DateTime.UtcNow} ===\n{exceptionHeader}{ex.Message}\n{ex.StackTrace}\n";
        return newMessage;
    }
}
