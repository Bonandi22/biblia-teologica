namespace BibliaTeologica.Application.Common.Resultados;

public record ResultadoSenha(bool Sucesso, bool Bloqueado, Guid? UserId);
