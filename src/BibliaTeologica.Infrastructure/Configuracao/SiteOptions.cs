using BibliaTeologica.Application.Common.Interfaces;

namespace BibliaTeologica.Infrastructure.Configuracao;

public class SiteOptions : IConfiguracaoSite
{
    public const string Secao = "Site";
    private const string PadraoDev = "http://localhost:4300";

    public string? UrlPublica { get; set; }

    string IConfiguracaoSite.UrlPublica => (string.IsNullOrWhiteSpace(UrlPublica) ? PadraoDev : UrlPublica).TrimEnd('/');

    /// <summary>Falha na inicialização: em Production, sem a URL o link do e-mail apontaria para lugar errado.</summary>
    public void Validar(bool producao)
    {
        if (producao && string.IsNullOrWhiteSpace(UrlPublica))
            throw new InvalidOperationException("Configuração em falta: Site:UrlPublica.");
    }
}
