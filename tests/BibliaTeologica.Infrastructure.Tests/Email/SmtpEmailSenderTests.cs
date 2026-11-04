using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Email;
using Microsoft.Extensions.Options;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Email;

public class SmtpEmailSenderTests
{
    [Fact]
    public async Task EnviarAsync_NaoRegistraDestinatario_QuandoEnvioFalha()
    {
        var logger = new LoggerEspiao<SmtpEmailSender>();
        var opcoes = Options.Create(new EmailOptions
        {
            Smtp = new EmailOptions.SmtpOptions
            {
                Host = "127.0.0.1",
                Port = 1,
                User = "conta@teste.local",
                Password = "x",
                From = "conta@teste.local"
            }
        });

        var enviar = () => new SmtpEmailSender(opcoes, logger)
            .EnviarAsync("pessoa@teste.local", "Assunto", "<p>Corpo</p>", default);

        await enviar.Should().ThrowAsync<Exception>();
        logger.Mensagens.Should().NotBeEmpty();
        logger.Mensagens.Should().NotContain(m => m.Contains("pessoa@teste.local"));
    }
}
