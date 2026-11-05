using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record ReenviarCodigoDto([Required, MaxLength(1024)] string TokenDesafio);
