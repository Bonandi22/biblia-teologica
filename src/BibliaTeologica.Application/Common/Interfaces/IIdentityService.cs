using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Resultados;

namespace BibliaTeologica.Application.Common.Interfaces;

public record ResultadoRedefinicao(Guid? UserId, bool TokenInvalido, IReadOnlyList<string> Erros);

public interface IIdentityService
{
    Task<ResultadoSenha> VerificarSenhaAsync(string email, string senha, bool lockoutOnFailure, CancellationToken ct);
    Task<Guid?> ResolverIdPorEmailAsync(string email, CancellationToken ct);
    Task<bool> EstaBloqueadoAsync(Guid userId, CancellationToken ct);
    Task<string> GerarTokenEmailAsync(Guid userId, CancellationToken ct);
    Task<bool> VerificarTokenEmailAsync(Guid userId, string codigo, CancellationToken ct);
    Task RegistarFalhaAsync(Guid userId, CancellationToken ct);
    Task ResetarFalhasAsync(Guid userId, CancellationToken ct);
    Task<UsuarioDto?> ObterUsuarioAsync(Guid userId, CancellationToken ct);
    Task<string?> ObterEmailAsync(Guid userId, CancellationToken ct);
    Task<DateTime?> ObterCriadoEmAsync(Guid userId, CancellationToken ct);
    /// <summary>Roda a política de senha sem criar nada, para validar antes de olhar se o e-mail existe.</summary>
    Task<IdentityResultDto> ValidarSenhaAsync(string senha, CancellationToken ct);
    Task<ResultadoCriacao> CriarMembroAsync(NovoMembro dados, CancellationToken ct);
    Task<string> GerarTokenRedefinicaoAsync(Guid userId, CancellationToken ct);
    Task<ResultadoRedefinicao> RedefinirSenhaAsync(string email, string token, string novaSenha, CancellationToken ct);
    Task<IdentityResultDto> TrocarSenhaAsync(Guid userId, string atual, string nova, CancellationToken ct);
    /// <summary>Confere a senha de quem já está logado; cada falha conta para o bloqueio.</summary>
    Task<ResultadoSenha> ConferirSenhaAsync(Guid userId, string senha, CancellationToken ct);
    Task<string> GerarCodigoTrocaEmailAsync(Guid userId, string novoEmail, CancellationToken ct);
    /// <summary>Confere o código ligado ao endereço e troca e-mail e nome de utilizador. Falhas contam para o bloqueio.</summary>
    Task<IdentityResultDto> ConfirmarTrocaEmailAsync(Guid userId, string novoEmail, string codigo, CancellationToken ct);
    Task AtualizarNomeAsync(Guid userId, string nomeExibicao, CancellationToken ct);
    Task LimparDeveTrocarSenhaAsync(Guid userId, CancellationToken ct);
}
