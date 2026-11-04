using BibliaTeologica.Application.Conteudo;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface IFonteConteudo
{
    /// <exception cref="ConteudoInvalidoException">Arquivo a importar ilegível ou com YAML inválido.</exception>
    Task<ConjuntoRegistros> LerAsync(bool incluirRascunhos, CancellationToken ct);
}
