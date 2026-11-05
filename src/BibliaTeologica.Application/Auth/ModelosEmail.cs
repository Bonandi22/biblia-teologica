namespace BibliaTeologica.Application.Auth;

/// <summary>Textos dos e-mails. Só interpolar códigos gerados e URLs já codificadas.</summary>
public static class ModelosEmail
{
    public static (string Assunto, string Corpo) Codigo(string codigo) =>
        ("Seu código de acesso",
         $"<p>O seu código de acesso é <strong>{codigo}</strong>. Expira em 5 minutos.</p>");

    public static (string Assunto, string Corpo) Redefinicao(string link) =>
        ("Redefinição de senha",
         $"<p>Para definir uma nova senha, use o link abaixo. Ele vale por 1 hora.</p><p><a href=\"{link}\">Redefinir senha</a></p>" +
         "<p>Se não foi você, ignore esta mensagem.</p>");

    public static (string Assunto, string Corpo) SenhaAlterada() =>
        ("Sua senha foi alterada",
         "<p>A senha da sua conta foi alterada. Se não foi você, redefina a senha imediatamente.</p>");

    public static (string Assunto, string Corpo) TrocaEmailCodigo(string codigo) =>
        ("Confirme o novo e-mail",
         $"<p>Para confirmar o novo e-mail, use o código <strong>{codigo}</strong>. Expira em 5 minutos.</p>");

    public static (string Assunto, string Corpo) EmailEmUso() =>
        ("Tentativa de uso do seu e-mail",
         "<p>Alguém tentou associar este e-mail a outra conta, mas ele já está em uso. " +
         "Se não foi você, ignore esta mensagem.</p>");

    public static (string Assunto, string Corpo) EmailAlterado() =>
        ("O e-mail da sua conta foi alterado",
         "<p>O e-mail da sua conta foi alterado. Se não foi você, entre em contato conosco.</p>");

    public static (string Assunto, string Corpo) ContaApagada() =>
        ("Sua conta foi apagada",
         "<p>Sua conta e os dados associados foram apagados. Obrigado por ter passado por aqui.</p>");
}
