namespace BibliaTeologica.Infrastructure.Identity;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? RevogadoEm { get; set; }
    public string? SubstituidoPorHash { get; set; }
    public string? CriadoPorIp { get; set; }
}
