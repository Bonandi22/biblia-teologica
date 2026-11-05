using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Identity;

public class DescritorErrosPortuguesTests
{
    private readonly DescritorErrosPortugues _descritor = new();

    [Fact]
    public void Descrever_MensagemEmPortugues_QuandoSenhaFraca()
    {
        _descritor.PasswordTooShort(8).Description.Should().Be("A senha precisa ter pelo menos 8 caracteres.");
        _descritor.PasswordRequiresDigit().Description.Should().Be("A senha precisa ter pelo menos um número.");
        _descritor.PasswordRequiresUpper().Description.Should().Be("A senha precisa ter pelo menos uma letra maiúscula.");
        _descritor.PasswordRequiresLower().Description.Should().Be("A senha precisa ter pelo menos uma letra minúscula.");
    }
}
