using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identity;
    private readonly ITokenService _token;
    private readonly IRefreshTokenStore _store;
    private readonly IEmailSender _email;
    private readonly IEmissorTokens _emissor;
    private readonly IFilaEmail _fila;
    private readonly ILimiteEnvio _limite;
    private readonly ILogger<AuthService> _log;

    public AuthService(IIdentityService identity, ITokenService token, IRefreshTokenStore store,
        IEmailSender email, IEmissorTokens emissor, IFilaEmail fila, ILimiteEnvio limite,
        ILogger<AuthService> log)
    {
        _fila = fila;
        _limite = limite;
        _identity = identity;
        _token = token;
        _store = store;
        _email = email;
        _emissor = emissor;
        _log = log;
    }

    public async Task<ResultadoLoginDto> LoginAsync(LoginDto dto, string? ip, CancellationToken ct)
    {
        var r = await _identity.VerificarSenhaAsync(dto.Email, dto.Senha, lockoutOnFailure: true, ct);
        if (r.Bloqueado) return new ResultadoLoginDto(EstadoLogin.Bloqueado);
        if (!r.Sucesso || r.UserId is null) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        var codigo = await _identity.GerarTokenEmailAsync(r.UserId.Value, ct);
        await _email.EnviarAsync(dto.Email, "Código de acesso",
            $"<p>O seu código de acesso é <strong>{codigo}</strong>. Expira em poucos minutos.</p>", ct);

        return new ResultadoLoginDto(EstadoLogin.Desafio2fa, TokenDesafio: _token.GerarDesafioLogin(r.UserId.Value));
    }

    public async Task<ResultadoLoginDto> Verificar2faAsync(Verificar2faDto dto, string? ip, CancellationToken ct)
    {
        var userId = _token.ValidarDesafioLogin(dto.TokenDesafio);
        if (userId is null) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        if (!await _identity.VerificarTokenEmailAsync(userId.Value, dto.Codigo, ct))
        {
            await _identity.RegistarFalhaAsync(userId.Value, ct);
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);
        }

        await _identity.ResetarFalhasAsync(userId.Value, ct);
        return await _emissor.EmitirAsync(userId.Value, ip, ct);
    }

    public async Task<ResultadoLoginDto> RefreshAsync(string refreshTokenCru, string? ip, CancellationToken ct)
    {
        var hash = _token.Hash(refreshTokenCru);
        var info = await _store.ObterAsync(hash, ct);
        if (info is null) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        // Só token já rotacionado indica roubo. Revogado por logout ou troca de senha é só sessão encerrada.
        if (info.Rotacionado)
        {
            _log.LogWarning("Refresh token reutilizado para {UserId} — a revogar cadeia.", info.UserId);
            await _store.RevogarCadeiaAsync(info.UserId, ct);
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);
        }
        if (!info.Valido) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        // O refresh é rotacionado aqui (novo hash guardado). NÃO usamos o _emissor
        // (guardaria um 2º refresh). Montamos o resultado localmente.
        var (novoCru, novoHash, novoExp) = _token.GerarRefreshToken();
        await _store.RotacionarAsync(hash, info.UserId, novoHash, novoExp, ip, ct);

        var usuario = await _identity.ObterUsuarioAsync(info.UserId, ct)
            ?? throw new InvalidOperationException("Utilizador não encontrado ao emitir tokens.");
        var access = _token.GerarAccessToken(info.UserId, usuario.Email, usuario.NomeExibicao,
            usuario.Roles, usuario.DeveTrocarSenha);

        return new ResultadoLoginDto(EstadoLogin.Autenticado, Usuario: usuario,
            Tokens: new TokensGerados(access, DateTime.UtcNow.AddMinutes(15), novoCru, novoExp,
                Guid.NewGuid().ToString("N")));
    }

    public async Task LogoutAsync(string? refreshTokenCru, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(refreshTokenCru))
            await _store.RevogarAsync(_token.Hash(refreshTokenCru), ct);
    }

    public async Task<ResultadoCadastro> RegistarMembroAsync(RegistarMembroDto dto, CancellationToken ct)
    {
        // Robô: responde igual a um cadastro comum, sem criar nem enviar nada.
        if (!string.IsNullOrWhiteSpace(dto.Site)) return DesafioFalso();

        if (!dto.AceitouTermo)
            return Recusa("É preciso aceitar o termo de proteção de dados para criar a conta.");
        if (!Telefone.TentarNormalizar(dto.Telefone, out var telefone))
            return Recusa("O telefone é inválido. Use de 8 a 15 dígitos, com + opcional no início.");

        var senha = await _identity.ValidarSenhaAsync(dto.Senha, ct);
        if (!senha.Sucesso) return new ResultadoCadastro(null, senha.Erros);

        var email = dto.Email.Trim();
        var criacao = await _identity.CriarMembroAsync(
            new NovoMembro(email, dto.Senha, dto.NomeExibicao.Trim(), telefone,
                TermoPrivacidade.VersaoAtual, DateTime.UtcNow), ct);

        if (criacao.JaExiste)
        {
            if (_limite.TentarConsumir(email, FinalidadeEnvio.Aviso))
            {
                var (assunto, corpo) = ModelosEmail.CadastroExistente();
                _fila.Enfileirar(email, assunto, corpo);
            }
            return DesafioFalso();
        }

        if (criacao.UserId is not { } userId) return new ResultadoCadastro(null, criacao.Erros);

        var codigo = await _identity.GerarTokenEmailAsync(userId, ct);
        if (_limite.TentarConsumir(email, FinalidadeEnvio.Cadastro))
        {
            var (assunto, corpo) = ModelosEmail.Codigo(codigo);
            _fila.Enfileirar(email, assunto, corpo);
        }
        return new ResultadoCadastro(_token.GerarDesafioLogin(userId), Array.Empty<string>());
    }

    // Id aleatório que não existe: o token tem a forma certa, mas o código nunca confere.
    private ResultadoCadastro DesafioFalso()
        => new(_token.GerarDesafioLogin(Guid.NewGuid()), Array.Empty<string>());

    private static ResultadoCadastro Recusa(string erro) => new(null, new[] { erro });
}
