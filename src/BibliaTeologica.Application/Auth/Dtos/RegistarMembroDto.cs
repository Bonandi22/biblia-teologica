using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record RegistarMembroDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Senha,
    [Required, MaxLength(80)] string NomeExibicao);
