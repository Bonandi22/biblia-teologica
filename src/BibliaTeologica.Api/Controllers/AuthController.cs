using BibliaTeologica.Api.Auth;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly CookieWriter _cookies;

    public AuthController(IAuthService auth, CookieWriter cookies)
    {
        _auth = auth;
        _cookies = cookies;
    }

    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var r = await _auth.LoginAsync(dto, Ip, ct);
        return r.Estado switch
        {
            EstadoLogin.Bloqueado => StatusCode(StatusCodes.Status423Locked, new ProblemDetails
            {
                Status = StatusCodes.Status423Locked,
                Title = "Conta bloqueada",
                Detail = "Demasiadas tentativas. Tenta mais tarde."
            }),
            EstadoLogin.Desafio2fa => Ok(new { desafio2fa = true, tokenDesafio = r.TokenDesafio }),
            _ => Unauthorized(new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Credenciais inválidas" })
        };
    }

    [HttpPost("login/2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> Login2fa(Verificar2faDto dto, CancellationToken ct)
        => Finalizar(await _auth.Verificar2faAsync(dto, Ip, ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var cru = Request.Cookies[CookieWriter.Refresh];
        if (string.IsNullOrEmpty(cru))
        {
            _cookies.Limpar(Response);
            return Unauthorized(new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Sessão expirada" });
        }
        return Finalizar(await _auth.RefreshAsync(cru, Ip, ct));
    }

    // Sem [Authorize], de propósito: com o acesso vencido, o 401 deixaria o refresh token válido
    // e a sessão viva depois de "Sair". Só revoga o refresh que veio no cookie, e o CSRF continua exigido.
    [HttpPost("logout")]
    [AllowAnonymous]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _auth.LogoutAsync(Request.Cookies[CookieWriter.Refresh], ct);
        _cookies.Limpar(Response);
        return NoContent();
    }

    [HttpPost("registar")]
    [AllowAnonymous]
    public async Task<IActionResult> Registar(RegistarMembroDto dto, CancellationToken ct)
    {
        var r = await _auth.RegistarMembroAsync(dto, ct);
        return r.Sucesso
            ? StatusCode(StatusCodes.Status201Created)
            : BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Registo inválido",
                Detail = string.Join(" ", r.Erros)
            });
    }

    [HttpGet("eu")]
    [Authorize]
    [PermitirSenhaPendente]
    public IActionResult Eu()
    {
        var email = User.FindFirst("email")?.Value ?? "";
        var nome = User.FindFirst("name")?.Value ?? "";
        var roles = User.FindAll("role").Select(c => c.Value).ToArray();
        var deveTrocarSenha = User.FindFirst("deve_trocar_senha")?.Value == "true";
        // Corpo "nu" (sem wrapper): o frontend faz http.get<Usuario>('/api/auth/eu') sem desembrulhar.
        return Ok(new { email, nomeExibicao = nome, roles, deveTrocarSenha });
    }

    private IActionResult Finalizar(ResultadoLoginDto r)
    {
        if (r.Estado != EstadoLogin.Autenticado || r.Tokens is null || r.Usuario is null)
        {
            _cookies.Limpar(Response);
            return r.Erros is { Count: > 0 }
                ? BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Erro",
                    Detail = string.Join(" ", r.Erros)
                })
                : Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Credenciais inválidas"
                });
        }

        _cookies.Escrever(Response, r.Tokens);
        return Ok(new
        {
            usuario = new
            {
                r.Usuario.Email,
                r.Usuario.NomeExibicao,
                r.Usuario.Roles,
                r.Usuario.DeveTrocarSenha
            }
        });
    }
}
