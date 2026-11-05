namespace BibliaTeologica.Application.Common.Interfaces;

public enum FinalidadeEnvio
{
    Login,
    Cadastro,
    Redefinicao,
    TrocaEmail,
    Aviso
}

/// <summary>Limite de envios de e-mail por par (endereço normalizado, finalidade).</summary>
public interface ILimiteEnvio
{
    /// <summary>Consome uma vaga na janela; devolve false quando o limite já foi atingido.</summary>
    bool TentarConsumir(string chave, FinalidadeEnvio finalidade);
}
