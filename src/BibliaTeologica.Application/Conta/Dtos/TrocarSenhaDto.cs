using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarSenhaDto(
    [Required, MaxLength(128)] string SenhaAtual,
    [Required, MinLength(8), MaxLength(128)] string NovaSenha);
