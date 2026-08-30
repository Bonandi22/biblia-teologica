using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record RegistarMembroDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Senha,
    [Required, MaxLength(80)] string NomeExibicao);
