using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record Verificar2faDto(
    [Required] string TokenDesafio,
    [Required] string Codigo);
