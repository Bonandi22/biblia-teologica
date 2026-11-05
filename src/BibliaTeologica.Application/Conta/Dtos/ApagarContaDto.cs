using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record ApagarContaDto([Required, MaxLength(128)] string Senha);
