using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarEmailDto(
    [property: Required, EmailAddress] string NovoEmail,
    [property: Required] string Senha);
