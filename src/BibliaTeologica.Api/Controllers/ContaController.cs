using BibliaTeologica.Api.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Conta;
using BibliaTeologica.Application.Conta.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/conta")]
[Authorize]
public class ContaController : ControllerBase
{
    private readonly IContaService _conta;
    private readonly IUsuarioAtual _usuario;
    private readonly CookieWriter _cookies;

    public ContaController(IContaService conta, IUsuarioAtual usuario, CookieWriter cookies)
    {
        _conta = conta;
        _usuario = usuario;
        _cookies = cookies;
    }

    private Guid Id => _usuario.Id ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        var p = await _conta.ObterAsync(Id, ct);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPut]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> Atualizar(AtualizarPerfilDto dto, CancellationToken ct)
    {
        await _conta.AtualizarAsync(Id, dto, ct);
        return NoContent();
    }

    [HttpPost("email/solicitar")]
    [EnableRateLimiting("auth")]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> SolicitarTrocaEmail(SolicitarTrocaEmailDto dto, CancellationToken ct)
    {
        var r = await _conta.SolicitarTrocaEmailAsync(Id, dto, ct);
        if (r.Limite)
            return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Muitos envios",
                Detail = "Enviamos códigos demais para este endereço. Aguarde alguns minutos e tente de novo."
            });
        return r.Sucesso
            ? NoContent()
            : BadRequest(new ProblemDetails { Status = 400, Title = "Erro", Detail = string.Join(" ", r.Erros) });
    }

    [HttpPost("email/confirmar")]
    [EnableRateLimiting("auth")]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> ConfirmarTrocaEmail(ConfirmarTrocaEmailDto dto, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var r = await _conta.ConfirmarTrocaEmailAsync(Id, dto, ip, ct);
        if (r.Estado != EstadoLogin.Autenticado || r.Tokens is null)
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Erro",
                Detail = string.Join(" ", r.Erros ?? Array.Empty<string>())
            });

        _cookies.Escrever(Response, r.Tokens);
        return Ok(new
        {
            usuario = new
            {
                r.Usuario!.Email,
                r.Usuario.NomeExibicao,
                r.Usuario.Roles,
                r.Usuario.DeveTrocarSenha
            }
        });
    }

    [HttpPost("senha")]
    [PermitirSenhaPendente]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> TrocarSenha(TrocarSenhaDto dto, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var r = await _conta.TrocarSenhaAsync(Id, dto, ip, ct);
        if (r.Estado != EstadoLogin.Autenticado || r.Tokens is null)
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Erro",
                Detail = string.Join(" ", r.Erros ?? Array.Empty<string>())
            });

        _cookies.Escrever(Response, r.Tokens);
        return Ok(new
        {
            usuario = new
            {
                r.Usuario!.Email,
                r.Usuario.NomeExibicao,
                r.Usuario.Roles,
                r.Usuario.DeveTrocarSenha
            }
        });
    }
}
