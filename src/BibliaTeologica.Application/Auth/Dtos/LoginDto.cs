using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record LoginDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(128)] string Senha);
