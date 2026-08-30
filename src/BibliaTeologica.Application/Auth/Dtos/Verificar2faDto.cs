using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record Verificar2faDto(
    [property: Required] string TokenDesafio,
    [property: Required] string Codigo);
