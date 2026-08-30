using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarEmailDto(
    [Required, EmailAddress] string NovoEmail,
    [Required] string Senha);
