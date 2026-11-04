using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record Verificar2faDto(
    [Required, MaxLength(1024)] string TokenDesafio,
    [Required, MaxLength(10)] string Codigo);
