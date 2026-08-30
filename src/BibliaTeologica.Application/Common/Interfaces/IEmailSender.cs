namespace BibliaTeologica.Application.Common.Interfaces;

public interface IEmailSender
{
    Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct);
}
