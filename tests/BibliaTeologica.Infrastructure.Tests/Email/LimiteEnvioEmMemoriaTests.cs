using AwesomeAssertions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Email;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace BibliaTeologica.Infrastructure.Tests.Email;

public class LimiteEnvioEmMemoriaTests
{
    private sealed class RelogioFalso : TimeProvider
    {
        private DateTimeOffset _agora = new(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _agora;
        public void Avancar(TimeSpan t) => _agora += t;
    }

    private static (LimiteEnvioEmMemoria Limite, RelogioFalso Relogio) Criar()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["EnvioEmail:MaxPorJanela"] = "3",
            ["EnvioEmail:JanelaMinutos"] = "15"
        }).Build();
        var relogio = new RelogioFalso();
        return (new LimiteEnvioEmMemoria(new MemoryCache(new MemoryCacheOptions()), config, relogio), relogio);
    }

    [Fact]
    public void TentarConsumir_RecusaQuartoEnvio_QuandoMesmoParNaJanela()
    {
        var (limite, _) = Criar();

        var resultados = Enumerable.Range(0, 4)
            .Select(_ => limite.TentarConsumir("a@x.com", FinalidadeEnvio.Cadastro))
            .ToArray();

        resultados.Should().Equal(true, true, true, false);
    }

    [Fact]
    public void TentarConsumir_Permite_QuandoFinalidadeDiferente()
    {
        var (limite, _) = Criar();
        for (var i = 0; i < 3; i++) limite.TentarConsumir("a@x.com", FinalidadeEnvio.Redefinicao);

        limite.TentarConsumir("a@x.com", FinalidadeEnvio.Redefinicao).Should().BeFalse();
        limite.TentarConsumir("a@x.com", FinalidadeEnvio.Login).Should().BeTrue();
    }

    [Fact]
    public void TentarConsumir_ContaJunto_QuandoEnderecoDifereSoEmCaixaEEspaco()
    {
        var (limite, _) = Criar();
        limite.TentarConsumir(" Maria@X.com ", FinalidadeEnvio.Cadastro);
        limite.TentarConsumir("maria@x.com", FinalidadeEnvio.Cadastro);
        limite.TentarConsumir("MARIA@X.COM", FinalidadeEnvio.Cadastro);

        limite.TentarConsumir("maria@x.com", FinalidadeEnvio.Cadastro).Should().BeFalse();
    }

    [Fact]
    public void TentarConsumir_LiberaDeNovo_QuandoJanelaPassa()
    {
        var (limite, relogio) = Criar();
        for (var i = 0; i < 3; i++) limite.TentarConsumir("a@x.com", FinalidadeEnvio.Cadastro);
        limite.TentarConsumir("a@x.com", FinalidadeEnvio.Cadastro).Should().BeFalse();

        relogio.Avancar(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1));

        limite.TentarConsumir("a@x.com", FinalidadeEnvio.Cadastro).Should().BeTrue();
    }
}
