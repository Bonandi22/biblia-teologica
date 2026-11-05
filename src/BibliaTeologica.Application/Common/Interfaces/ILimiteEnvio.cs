namespace BibliaTeologica.Application.Common.Interfaces;

public enum FinalidadeEnvio
{
    Login,
    Cadastro,
    Redefinicao,
    TrocaEmail,
    Aviso,
    /// <summary>Avisos de segurança (senha ou e-mail alterado, conta apagada): só saem após ação autenticada e não dividem limite com o que um anônimo dispara.</summary>
    AvisoSeguranca
}

/// <summary>Limite de envios de e-mail por par (endereço normalizado, finalidade).</summary>
public interface ILimiteEnvio
{
    /// <summary>Consome uma vaga na janela; devolve false quando o limite já foi atingido.</summary>
    bool TentarConsumir(string chave, FinalidadeEnvio finalidade);
}
