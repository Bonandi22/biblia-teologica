using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Configuracao;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Configuracao;

public class EmailOptionsTests
{
    private const string SenhaDeTeste = "senha-que-nao-pode-vazar";

    private static EmailOptions SmtpCompleto() => new()
    {
        Sink = "Smtp",
        Smtp = new EmailOptions.SmtpOptions
        {
            Host = "smtp.gmail.com",
            Port = 587,
            User = "conta@gmail.com",
            Password = SenhaDeTeste,
            From = "conta@gmail.com"
        }
    };

    [Theory]
    [InlineData("Dev")]
    [InlineData("")]
    [InlineData("Outro")]
    public void Validar_Lanca_QuandoProducaoSemSinkSmtp(string sink)
    {
        var opts = SmtpCompleto();
        opts.Sink = sink;

        var acao = () => opts.Validar(producao: true);

        acao.Should().Throw<InvalidOperationException>().WithMessage("*Email:Sink*");
    }

    [Theory]
    [InlineData("Host")]
    [InlineData("User")]
    [InlineData("Password")]
    [InlineData("From")]
    public void Validar_Lanca_QuandoSmtpIncompleto(string campo)
    {
        var opts = SmtpCompleto();
        typeof(EmailOptions.SmtpOptions).GetProperty(campo)!.SetValue(opts.Smtp, " ");

        var acao = () => opts.Validar(producao: false);

        acao.Should().Throw<InvalidOperationException>().WithMessage($"*Email:Smtp:{campo}*");
    }

    [Fact]
    public void Validar_NaoExpoeASenha_QuandoSmtpIncompleto()
    {
        var opts = SmtpCompleto();
        opts.Smtp.Host = "";

        var acao = () => opts.Validar(producao: true);

        acao.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().NotContain(SenhaDeTeste);
    }

    [Theory]
    [InlineData("Smtp")]
    [InlineData("smtp")]
    public void Validar_Aceita_QuandoProducaoComSmtpCompleto(string sink)
    {
        var opts = SmtpCompleto();
        opts.Sink = sink;

        var acao = () => opts.Validar(producao: true);

        acao.Should().NotThrow();
    }

    [Fact]
    public void Validar_Aceita_QuandoDesenvolvimentoComSinkDev()
    {
        var acao = () => new EmailOptions { Sink = "Dev" }.Validar(producao: false);

        acao.Should().NotThrow();
    }
}
