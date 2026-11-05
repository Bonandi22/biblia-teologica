using Microsoft.AspNetCore.Identity;

namespace BibliaTeologica.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string NomeExibicao { get; set; } = string.Empty;
    public bool DeveTrocarSenha { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? TermoAceitoEm { get; set; }
    public string? TermoVersao { get; set; }
}
