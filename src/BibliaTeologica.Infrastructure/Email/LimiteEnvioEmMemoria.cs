using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace BibliaTeologica.Infrastructure.Email;

public sealed class LimiteEnvioEmMemoria : ILimiteEnvio
{
    private readonly IMemoryCache _cache;
    private readonly TimeProvider _tempo;
    private readonly int _max;
    private readonly TimeSpan _janela;
    private readonly object _trava = new();

    public LimiteEnvioEmMemoria(IMemoryCache cache, IConfiguration config, TimeProvider tempo)
    {
        _cache = cache;
        _tempo = tempo;
        _max = config.GetValue<int?>("EnvioEmail:MaxPorJanela") ?? 3;
        _janela = TimeSpan.FromMinutes(config.GetValue<int?>("EnvioEmail:JanelaMinutos") ?? 15);
    }

    public bool TentarConsumir(string chave, FinalidadeEnvio finalidade)
    {
        var id = $"{finalidade}:{chave.Trim().ToUpperInvariant()}";
        var agora = _tempo.GetUtcNow();

        lock (_trava)
        {
            var envios = _cache.TryGetValue(id, out List<DateTimeOffset>? existentes) && existentes is not null
                ? existentes
                : new List<DateTimeOffset>();

            envios.RemoveAll(t => t <= agora - _janela);
            if (envios.Count >= _max)
                return false;

            envios.Add(agora);
            // A janela é decidida pelos carimbos (TimeProvider); a expiração só libera a memória de pares inativos.
            _cache.Set(id, envios, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _janela });
            return true;
        }
    }
}
