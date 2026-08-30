using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record RegistarMembroDto(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Senha,
    [property: Required, MaxLength(80)] string NomeExibicao);
