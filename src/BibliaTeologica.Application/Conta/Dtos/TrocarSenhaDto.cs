using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarSenhaDto(
    [Required] string SenhaAtual,
    [Required, MinLength(8)] string NovaSenha);
