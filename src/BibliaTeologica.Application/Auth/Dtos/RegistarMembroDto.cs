using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

/// <param name="Site">Campo invisível no formulário: só um robô o preenche.</param>
public record RegistarMembroDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Senha,
    [Required, MaxLength(80)] string NomeExibicao,
    bool AceitouTermo,
    [MaxLength(40)] string? Telefone,
    [MaxLength(200)] string? Site);
