using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record ConfirmarTrocaEmailDto(
    [Required, EmailAddress, MaxLength(256)] string NovoEmail,
    [Required, MaxLength(10)] string Codigo);
