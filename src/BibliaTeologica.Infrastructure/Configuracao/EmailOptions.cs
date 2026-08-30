namespace BibliaTeologica.Infrastructure.Configuracao;

public class EmailOptions
{
    public const string Secao = "Email";
    public string Sink { get; set; } = "Dev";           // "Dev" | "Smtp"
    public string Remetente { get; set; } = "nao-responder@biblia-teologica.local";
    public SmtpOptions Smtp { get; set; } = new();

    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
    }
}
