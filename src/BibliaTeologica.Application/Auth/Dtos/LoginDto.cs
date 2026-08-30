using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Senha);
