using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Identity;

/// <summary>Uma vez por dia apaga contas que nunca confirmaram o e-mail em 7 dias.</summary>
public sealed class LimpezaContasNaoConfirmadas : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);
    private static readonly TimeSpan Validade = TimeSpan.FromDays(7);

    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<LimpezaContasNaoConfirmadas> _log;

    public LimpezaContasNaoConfirmadas(IServiceScopeFactory scopes, ILogger<LimpezaContasNaoConfirmadas> log)
    {
        _scopes = scopes;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);
        try
        {
            do
            {
                await ExecutarUmaVezAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Encerramento normal.
        }
    }

    private async Task ExecutarUmaVezAsync(CancellationToken ct)
    {
        try
        {
            // IIdentityService é scoped: um scope por execução.
            using var scope = _scopes.CreateScope();
            var identity = scope.ServiceProvider.GetRequiredService<IIdentityService>();
            var apagadas = await identity.ApagarNaoConfirmadasAsync(DateTime.UtcNow - Validade, ct);
            _log.LogInformation("Limpeza de contas não confirmadas: {Apagadas} apagada(s).", apagadas);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Banco ainda sem migração, por exemplo: registra e tenta de novo no próximo ciclo.
            _log.LogError(ex, "Falha na limpeza de contas não confirmadas.");
        }
    }
}
