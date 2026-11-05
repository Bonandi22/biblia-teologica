using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

/// <param name="Site">Campo invisível no formulário: só um robô o preenche.</param>
public record EsqueciSenhaDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [MaxLength(200)] string? Site);
