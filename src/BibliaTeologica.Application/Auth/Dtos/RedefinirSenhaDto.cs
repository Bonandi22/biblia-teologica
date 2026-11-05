using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

// Sem MinLength na senha: a política só roda depois de o token ser aceito, para não distinguir os casos.
public record RedefinirSenhaDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(2048)] string Token,
    [Required, MaxLength(128)] string NovaSenha);
