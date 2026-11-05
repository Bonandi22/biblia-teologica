namespace BibliaTeologica.Application.Common.Interfaces;

/// <summary>Endereço público do site, vindo da configuração (nunca do cabeçalho Host).</summary>
public interface IConfiguracaoSite
{
    /// <summary>URL base sem barra final.</summary>
    string UrlPublica { get; }
}
