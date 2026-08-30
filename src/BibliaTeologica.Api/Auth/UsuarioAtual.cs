using System.Security.Claims;
using BibliaTeologica.Application.Common.Interfaces;

namespace BibliaTeologica.Api.Auth;

public class UsuarioAtual : IUsuarioAtual
{
    private readonly IHttpContextAccessor _acessor;

    public UsuarioAtual(IHttpContextAccessor acessor) => _acessor = acessor;

    private ClaimsPrincipal? User => _acessor.HttpContext?.User;

    public bool EstaAutenticado => User?.Identity?.IsAuthenticated ?? false;
    public bool EhAdmin => User?.IsInRole("Admin") ?? false;
    public string? Email => User?.FindFirstValue("email");

    public Guid? Id
    {
        get
        {
            var raw = User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}
