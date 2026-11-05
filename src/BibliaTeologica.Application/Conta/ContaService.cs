using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Application.Conta.Dtos;

namespace BibliaTeologica.Application.Conta;

public class ContaService : IContaService
{
    private readonly IIdentityService _identity;
    private readonly IEmissorTokens _emissor;
    private readonly IRefreshTokenStore _store;
    private readonly IFilaEmail _fila;
    private readonly ILimiteEnvio _limite;

    public ContaService(IIdentityService identity, IEmissorTokens emissor, IRefreshTokenStore store,
        IFilaEmail fila, ILimiteEnvio limite)
    {
        _fila = fila;
        _limite = limite;
        _identity = identity;
        _emissor = emissor;
        _store = store;
    }

    public async Task<PerfilDto?> ObterAsync(Guid userId, CancellationToken ct)
    {
        var u = await _identity.ObterUsuarioAsync(userId, ct);
        if (u is null) return null;
        var criadoEm = await _identity.ObterCriadoEmAsync(userId, ct) ?? DateTime.UtcNow;
        return new PerfilDto(u.Email, u.NomeExibicao, u.Roles, criadoEm);
    }

    public Task AtualizarAsync(Guid userId, AtualizarPerfilDto dto, CancellationToken ct)
        => _identity.AtualizarNomeAsync(userId, dto.NomeExibicao, ct);

    public async Task<IdentityResultDto> SolicitarTrocaEmailAsync(Guid userId, SolicitarTrocaEmailDto dto, CancellationToken ct)
    {
        var senha = await _identity.ConferirSenhaAsync(userId, dto.Senha, ct);
        if (!senha.Sucesso) return IdentityResultDto.Falha("Senha incorreta.");

        var novoEmail = dto.NovoEmail.Trim();
        // Um só consumo para os dois ramos: o limite responde igual com o endereço livre ou em uso.
        if (!_limite.TentarConsumir(novoEmail, FinalidadeEnvio.TrocaEmail))
            return IdentityResultDto.LimiteEsgotado();

        // Os dois ramos terminam na fila, sem esperar o SMTP: o tempo não revela se o endereço está em uso.
        var dono = await _identity.ResolverIdPorEmailAsync(novoEmail, ct);
        var (assunto, corpo) = dono is null
            ? ModelosEmail.TrocaEmailCodigo(await _identity.GerarCodigoTrocaEmailAsync(userId, novoEmail, ct))
            : ModelosEmail.EmailEmUso();
        _fila.Enfileirar(novoEmail, assunto, corpo);
        return IdentityResultDto.Ok();
    }

    public async Task<ResultadoLoginDto> ConfirmarTrocaEmailAsync(Guid userId, ConfirmarTrocaEmailDto dto, string? ip, CancellationToken ct)
    {
        var antigo = await _identity.ObterEmailAsync(userId, ct);
        var r = await _identity.ConfirmarTrocaEmailAsync(userId, dto.NovoEmail, dto.Codigo, ct);
        if (!r.Sucesso)
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas, Erros: r.Erros);

        await _store.RevogarCadeiaAsync(userId, ct);
        if (antigo is not null && _limite.TentarConsumir(antigo, FinalidadeEnvio.Aviso))
        {
            var (assunto, corpo) = ModelosEmail.EmailAlterado();
            _fila.Enfileirar(antigo, assunto, corpo);
        }
        return await _emissor.EmitirAsync(userId, ip, ct);
    }

    public async Task<ResultadoLoginDto> TrocarSenhaAsync(Guid userId, TrocarSenhaDto dto, string? ip, CancellationToken ct)
    {
        var r = await _identity.TrocarSenhaAsync(userId, dto.SenhaAtual, dto.NovaSenha, ct);
        if (!r.Sucesso)
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas, Erros: r.Erros);

        // Antes de emitir: encerra as outras sessões e preserva a nova, desta sessão.
        await _store.RevogarCadeiaAsync(userId, ct);
        return await _emissor.EmitirAsync(userId, ip, ct);
    }
}
