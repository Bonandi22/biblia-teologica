using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record LoginDto(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Senha);
