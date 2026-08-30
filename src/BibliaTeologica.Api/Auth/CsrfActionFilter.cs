using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliaTeologica.Api.Auth;

public class CsrfActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> Seguros = new(StringComparer.OrdinalIgnoreCase)
        { "GET", "HEAD", "OPTIONS", "TRACE" };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var req = context.HttpContext.Request;
        if (!Seguros.Contains(req.Method))
        {
            var cookie = req.Cookies[CookieWriter.Csrf];
            var header = req.Headers["X-CSRF-Token"].ToString();
            if (string.IsNullOrEmpty(cookie) || !string.Equals(cookie, header, StringComparison.Ordinal))
            {
                context.Result = new ObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "CSRF",
                    Detail = "Token CSRF em falta ou inválido."
                }) { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }
        }
        await next();
    }
}
