using TyreCompare.Log;
using ILogging = TyreCompare.Log.ILog;

namespace TyreCompare.API.HelperClasses;

public class EntryMiddleware : IMiddleware
{
    private IFileLogger Logger { get; set; }

    public EntryMiddleware(IFileLogger logger)
    {
        Logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate nextRequest)
    {
        try
        {
            await nextRequest(context);
        }
        catch (Exception ex)
        {
            Logger.Log("Exception in Middleware");
            Logger.Log(ex);
        }
    }
}
