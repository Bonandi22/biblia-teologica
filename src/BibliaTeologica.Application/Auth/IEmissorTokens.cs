using BibliaTeologica.Application.Auth.Dtos;

namespace BibliaTeologica.Application.Auth;

public interface IEmissorTokens
{
    Task<ResultadoLoginDto> EmitirAsync(Guid userId, string? ip, CancellationToken ct);
}
