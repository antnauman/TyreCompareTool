namespace TyreCompare.Log;

public class FileLogger : IFileLogger
{
    private static SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    private string LogFilePath { get; set; }

    public FileLogger(string logFolderPath)
    {
        LogFilePath = Helper.GetLogFilePath(logFolderPath);
    }

    public async Task<bool> Log(string message, string messageDetail = "")
    {
        await Semaphore.WaitAsync();
        try
        {
            string loggableMessage = Helper.FormatMessageForLogFile(message, messageDetail);
            await File.AppendAllTextAsync(LogFilePath, loggableMessage);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<bool> Log(Exception exception, string exceptionHeader = "")
    {
        await Semaphore.WaitAsync();
        try
        {
            string loggableMessage = Helper.FormatExceptionForLogFile(exception, exceptionHeader);
            await File.AppendAllTextAsync(LogFilePath, loggableMessage);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public string GetCurrentLogPath()
    {
        return LogFilePath;
    }
}
