namespace BibliaTeologica.Application.Common.Resultados;

public record RefreshTokenInfo(Guid UserId, string Hash, DateTime ExpiraEm, DateTime? RevogadoEm)
{
    public bool Valido => RevogadoEm is null && ExpiraEm > DateTime.UtcNow;
}
