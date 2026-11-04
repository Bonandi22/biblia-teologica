using System.Net;
using System.Net.Mail;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BibliaTeologica.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _opts;
    private readonly ILogger<SmtpEmailSender> _log;

    public SmtpEmailSender(IOptions<EmailOptions> opts, ILogger<SmtpEmailSender> log)
    {
        _opts = opts.Value;
        _log = log;
    }

    public async Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct)
    {
        var s = _opts.Smtp;
#pragma warning disable SYSLIB0014 // System.Net.Mail.SmtpClient obsoleto — suficiente para o M2 (sink de dev por defeito)
        using var client = new SmtpClient(s.Host, s.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(s.User, s.Password)
        };
        using var msg = new MailMessage(s.From, para, assunto, corpoHtml) { IsBodyHtml = true };
        try
        {
            await client.SendMailAsync(msg, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Falha ao enviar e-mail.");
            throw;
        }
#pragma warning restore SYSLIB0014
    }
}
