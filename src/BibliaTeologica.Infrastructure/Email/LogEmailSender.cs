using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Email;

public class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _log;

    public LogEmailSender(ILogger<LogEmailSender> log) => _log = log;

    public Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct)
    {
        _log.LogInformation("EMAIL (sink Dev) para {Para} | Assunto: {Assunto}\n{Corpo}",
            para, assunto, corpoHtml);
        return Task.CompletedTask;
    }
}
