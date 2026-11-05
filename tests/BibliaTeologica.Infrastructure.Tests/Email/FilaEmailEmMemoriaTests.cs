using AwesomeAssertions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BibliaTeologica.Infrastructure.Tests.Email;

public class FilaEmailEmMemoriaTests
{
    private sealed class SenderFalso : IEmailSender
    {
        private int _chamadas;
        public bool FalharNaPrimeira { get; init; }
        public TaskCompletionSource<(string Para, string Assunto, string Corpo)> Recebido { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct)
        {
            if (Interlocked.Increment(ref _chamadas) == 1 && FalharNaPrimeira)
                throw new InvalidOperationException("falha simulada");
            Recebido.TrySetResult((para, assunto, corpoHtml));
            return Task.CompletedTask;
        }
    }

    private static FilaEmailEmMemoria Criar(SenderFalso sender)
    {
        var services = new ServiceCollection();
        services.AddScoped<IEmailSender>(_ => sender);
        return new FilaEmailEmMemoria(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            NullLogger<FilaEmailEmMemoria>.Instance);
    }

    [Fact]
    public async Task Enfileirar_EntregaAoSender_QuandoServicoRodando()
    {
        var sender = new SenderFalso();
        var fila = Criar(sender);
        await fila.StartAsync(CancellationToken.None);

        fila.Enfileirar("a@x.com", "Assunto", "<p>oi</p>");

        var recebido = await sender.Recebido.Task.WaitAsync(TimeSpan.FromSeconds(5));
        recebido.Should().Be(("a@x.com", "Assunto", "<p>oi</p>"));
        await fila.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Enfileirar_ContinuaEntregando_QuandoEnvioAnteriorLanca()
    {
        var sender = new SenderFalso { FalharNaPrimeira = true };
        var fila = Criar(sender);
        await fila.StartAsync(CancellationToken.None);

        fila.Enfileirar("a@x.com", "Primeiro", "1");
        fila.Enfileirar("b@x.com", "Segundo", "2");

        var recebido = await sender.Recebido.Task.WaitAsync(TimeSpan.FromSeconds(5));
        recebido.Assunto.Should().Be("Segundo");
        await fila.StopAsync(CancellationToken.None);
    }
}
