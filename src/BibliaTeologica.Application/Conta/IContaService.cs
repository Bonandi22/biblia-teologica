using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Application.Conta.Dtos;

namespace BibliaTeologica.Application.Conta;

public interface IContaService
{
    Task<PerfilDto?> ObterAsync(Guid userId, CancellationToken ct);
    Task AtualizarAsync(Guid userId, AtualizarPerfilDto dto, CancellationToken ct);
    Task<IdentityResultDto> TrocarEmailAsync(Guid userId, TrocarEmailDto dto, CancellationToken ct);
    Task<ResultadoLoginDto> TrocarSenhaAsync(Guid userId, TrocarSenhaDto dto, string? ip, CancellationToken ct);
}
