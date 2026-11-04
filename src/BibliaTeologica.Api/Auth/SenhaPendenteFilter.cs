using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliaTeologica.Api.Auth;

/// <summary>Marca o endpoint autenticado que continua disponível antes da troca da senha inicial.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PermitirSenhaPendenteAttribute : Attribute;

/// <summary>Bloqueia endpoints autenticados até a senha inicial ser trocada. O guard do Angular é só UX.</summary>
public class SenhaPendenteFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.FindFirst("deve_trocar_senha")?.Value != "true") return;

        var metadados = context.ActionDescriptor.EndpointMetadata;
        var exigeLogin = metadados.OfType<IAuthorizeData>().Any() && !metadados.OfType<IAllowAnonymous>().Any();
        if (!exigeLogin || metadados.OfType<PermitirSenhaPendenteAttribute>().Any()) return;

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Troca de senha pendente",
            Detail = "Troque a senha inicial para continuar."
        }) { StatusCode = StatusCodes.Status403Forbidden };
    }
}
