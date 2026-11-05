namespace BibliaTeologica.Application.Common.Resultados;

/// <summary>Dados já validados para criar um membro. <paramref name="Telefone"/> vem normalizado.</summary>
public record NovoMembro(
    string Email,
    string Senha,
    string NomeExibicao,
    string? Telefone,
    string TermoVersao,
    DateTime TermoAceitoEm);

public record ResultadoCriacao(Guid? UserId, bool JaExiste, IReadOnlyList<string> Erros);
