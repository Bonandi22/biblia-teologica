using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarSenhaDto(
    [property: Required] string SenhaAtual,
    [property: Required, MinLength(8)] string NovaSenha);
