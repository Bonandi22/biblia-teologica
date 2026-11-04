using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Tests.Email;

internal sealed class LoggerEspiao<T> : ILogger<T>
{
    public readonly List<string> Mensagens = new();
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Fim();
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel level, EventId id, TState state, Exception? ex,
        Func<TState, Exception?, string> formatter) => Mensagens.Add(formatter(state, ex));
    private sealed class Fim : IDisposable { public void Dispose() { } }
}
