using BibliaTeologica.Api.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Conta;
using BibliaTeologica.Application.Conta.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPut("email")]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> TrocarEmail(TrocarEmailDto dto, CancellationToken ct)
    {
        var r = await _conta.TrocarEmailAsync(Id, dto, ct);
        return r.Sucesso
            ? NoContent()
            : BadRequest(new ProblemDetails { Status = 400, Title = "Erro", Detail = string.Join(" ", r.Erros) });
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
