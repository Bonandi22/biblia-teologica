using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Email;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Email;

public class LogEmailSenderTests
{
    [Fact]
    public async Task EnviarAsync_LogaParaAssuntoECorpo()
    {
        var logger = new LoggerEspiao<LogEmailSender>();
        await new LogEmailSender(logger).EnviarAsync("a@b.c", "Código", "<b>123456</b>", default);

        logger.Mensagens.Should().ContainSingle()
            .Which.Should().Contain("a@b.c").And.Contain("Código").And.Contain("123456");
    }
}
