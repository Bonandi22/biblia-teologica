using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Email;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Email;

public class LogEmailSenderTests
{
    private sealed class LoggerEspiao<T> : ILogger<T>
    {
        public readonly List<string> Mensagens = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Fim();
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? ex,
            Func<TState, Exception?, string> formatter) => Mensagens.Add(formatter(state, ex));
        private sealed class Fim : IDisposable { public void Dispose() { } }
    }

    [Fact]
    public async Task EnviarAsync_LogaParaAssuntoECorpo()
    {
        var logger = new LoggerEspiao<LogEmailSender>();
        await new LogEmailSender(logger).EnviarAsync("a@b.c", "Código", "<b>123456</b>", default);

        logger.Mensagens.Should().ContainSingle()
            .Which.Should().Contain("a@b.c").And.Contain("Código").And.Contain("123456");
    }
}
