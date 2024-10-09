using System.Net;

namespace TyreCompare.BCL;

public static class ExceptionHelper
{
    public static HttpStatusCode GetAppropriateStatusCode(this Exception exception)
    {
        if (exception is ArgumentNullException)
        {
            return HttpStatusCode.BadRequest;
        }
        if (exception is ArgumentException)
        {
            return HttpStatusCode.BadRequest;
        }
        if (exception is InvalidOperationException)
        {
            return HttpStatusCode.BadRequest;
        }
        if (exception is NotSupportedException)
        {
            return HttpStatusCode.BadRequest;
        }
        if (exception is UnauthorizedAccessException)
        {
            return HttpStatusCode.Unauthorized;
        }
        if (exception is KeyNotFoundException)
        {
            return HttpStatusCode.NotFound;
        }
        if (exception is NotImplementedException)
        {
            return HttpStatusCode.NotImplemented;
        }
        if (exception is TimeoutException)
        {
            return HttpStatusCode.RequestTimeout;
        }

        return HttpStatusCode.InternalServerError;
    }
}
