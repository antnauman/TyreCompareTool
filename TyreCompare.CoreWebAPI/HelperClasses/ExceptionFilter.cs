using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TyreCompare.BCL;
using ILog = TyreCompare.Log.ILog;

namespace TyreCompare.API.CustomMiddleware;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ILog Logger;
    public ExceptionFilter(ILog logger)
    { Logger = logger; }

    public void OnException(ExceptionContext context)
    {
        var username = context.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        string exceptionHeader = "Anonymous | ";
        if (username != null && controllerActionDescriptor != null)
        {
            var controllerName = controllerActionDescriptor.ControllerName;
            var actionName = controllerActionDescriptor.ActionName;
            exceptionHeader = $"{username} | {controllerName} - {actionName}() | ";
        }
        Logger.Log(context.Exception, exceptionHeader).Wait();
        context.Result = new ObjectResult(context.Exception.Message)
        { StatusCode = (int)context.Exception.GetAppropriateStatusCode() };

        context.ExceptionHandled = true;
    }
}
