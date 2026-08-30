using AutoMapper;
using AwesomeAssertions;
using BibliaTeologica.Application.Common.Mapping;
using Xunit;

namespace BibliaTeologica.Application.Tests.Common;

public class LivroMappingProfileTests
{
    [Fact]
    public void Configuracao_EhValida()
    {
        var config = new MapperConfiguration(c => c.AddProfile<LivroMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}
