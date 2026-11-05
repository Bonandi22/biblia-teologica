using System.Threading.Channels;
using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Email;

/// <summary>Fila em memória (singleton) e serviço que a esvazia, um e-mail de cada vez.</summary>
public sealed class FilaEmailEmMemoria : BackgroundService, IFilaEmail
{
    private readonly Channel<(string Para, string Assunto, string Corpo)> _canal =
        Channel.CreateUnbounded<(string, string, string)>();
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<FilaEmailEmMemoria> _log;

    public FilaEmailEmMemoria(IServiceScopeFactory scopes, ILogger<FilaEmailEmMemoria> log)
    {
        _scopes = scopes;
        _log = log;
    }

    public void Enfileirar(string para, string assunto, string corpoHtml) =>
        _canal.Writer.TryWrite((para, assunto, corpoHtml));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var (para, assunto, corpo) in _canal.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // IEmailSender é scoped: um scope por envio.
                    using var scope = _scopes.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                    await sender.EnviarAsync(para, assunto, corpo, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Sem destinatário nem corpo: podem conter dado pessoal, código ou link.
                    _log.LogError(ex, "Falha ao enviar e-mail da fila. Assunto: {Assunto}", assunto);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Encerramento normal.
        }
    }
}
