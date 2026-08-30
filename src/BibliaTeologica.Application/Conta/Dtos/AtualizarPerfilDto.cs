using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record AtualizarPerfilDto([property: Required, MaxLength(80)] string NomeExibicao);
