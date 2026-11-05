using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Resultados;

namespace BibliaTeologica.Application.Auth;

public enum ResultadoReenvio { Aceito, DesafioInvalido, Limite }

public interface IAuthService
{
    Task<ResultadoLoginDto> LoginAsync(LoginDto dto, string? ip, CancellationToken ct);
    Task<ResultadoLoginDto> Verificar2faAsync(Verificar2faDto dto, string? ip, CancellationToken ct);
    Task<ResultadoLoginDto> RefreshAsync(string refreshTokenCru, string? ip, CancellationToken ct);
    Task LogoutAsync(string? refreshTokenCru, CancellationToken ct);
    Task<ResultadoCadastro> RegistarMembroAsync(RegistarMembroDto dto, CancellationToken ct);
    Task<ResultadoReenvio> ReenviarCodigoAsync(string tokenDesafio, CancellationToken ct);
}
