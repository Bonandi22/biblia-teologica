namespace BibliaTeologica.Application.Common.Resultados;

public record RefreshTokenInfo(Guid UserId, string Hash, DateTime ExpiraEm, DateTime? RevogadoEm, bool Rotacionado = false)
{
    public bool Valido => RevogadoEm is null && ExpiraEm > DateTime.UtcNow;
}
