namespace BibliaTeologica.Infrastructure.Configuracao;

public class EmailOptions
{
    public const string Secao = "Email";
    public string Sink { get; set; } = "Dev";           // "Dev" | "Smtp"
    public string Remetente { get; set; } = "nao-responder@biblia-teologica.local";
    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>Falha na inicialização: em Production, sem e-mail real ninguém recebe o código 2FA.</summary>
    public void Validar(bool producao)
    {
        var usaSmtp = string.Equals(Sink, "Smtp", StringComparison.OrdinalIgnoreCase);
        if (producao && !usaSmtp)
            throw new InvalidOperationException("Email:Sink precisa ser Smtp em Production.");
        if (!usaSmtp) return;

        var faltando = new (string Nome, string Valor)[]
            {
                ("Host", Smtp.Host), ("User", Smtp.User), ("Password", Smtp.Password), ("From", Smtp.From)
            }
            .Where(c => string.IsNullOrWhiteSpace(c.Valor))
            .Select(c => $"Email:Smtp:{c.Nome}")
            .ToList();
        if (faltando.Count > 0)
            throw new InvalidOperationException($"Configuração de e-mail em falta: {string.Join(", ", faltando)}.");
    }

    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
    }
}
