using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

/// <summary>Substituição completa: telefone ausente ou vazio apaga o número.</summary>
public record AtualizarPerfilDto([Required, MaxLength(80)] string NomeExibicao, [MaxLength(40)] string? Telefone = null);
