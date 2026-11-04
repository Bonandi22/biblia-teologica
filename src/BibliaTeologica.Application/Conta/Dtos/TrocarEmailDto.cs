using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarEmailDto(
    [Required, EmailAddress, MaxLength(256)] string NovoEmail,
    [Required, MaxLength(128)] string Senha);
