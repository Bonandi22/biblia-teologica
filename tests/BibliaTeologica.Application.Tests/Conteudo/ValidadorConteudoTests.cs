using AwesomeAssertions;
using BibliaTeologica.Application.Conteudo;
using Xunit;

namespace BibliaTeologica.Application.Tests.Conteudo;

public class ValidadorConteudoTests
{
    private static readonly IReadOnlyList<LivroCanonico> Livros = new[]
    {
        new LivroCanonico("isaias", 23), new LivroCanonico("2-reis", 12),
        new LivroCanonico("miqueias", 33), new LivroCanonico("mateus", 40),
        new LivroCanonico("1-pedro", 59), new LivroCanonico("salmos", 19),
    };

    private const string ArqLivro = "content/livros/isaias.yaml";
    private const string ArqEstudo = "content/estudos/senaqueribe.md";

    private static RegistroObra Obra(string id = "arc") =>
        new() { Id = id, Autor = "SBB", Titulo = "Bíblia ARC", Url = null };

    private static RegistroNota Nota(int n = 1, string obra = "arc") =>
        new() { Numero = n, Obra = obra, Local = "Isaías 36:1" };

    private static RegistroBloco BlocoValido() => new()
    {
        Explicacao = "Texto do bloco.[^1]",
        Notas = { Nota() },
    };

    private static RegistroLivro LivroValido() => new()
    {
        Arquivo = ArqLivro,
        Estado = "revisado",
        Livro = "isaias",
        Resumo = "r",
        Quem = "q",
        ParaQuem = "pq",
        Porque = "p",
        Panorama = new RegistroBloco { Explicacao = "Texto." },
        Contexto = BlocoValido(),
        Mundo = BlocoValido(),
    };

    private static RegistroEstudo EstudoValido() => new()
    {
        Arquivo = ArqEstudo,
        Estado = "revisado",
        Titulo = "Senaqueribe",
        Resumo = "r",
        Livros = { "isaias" },
        CorpoMarkdown = "Corpo do estudo.[^1]",
        Notas = { Nota() },
    };

    private static ConjuntoRegistros Conjunto(
        RegistroLivro? livro = null, RegistroEstudo? estudo = null, params RegistroObra[] obras) =>
        new(obras.Length > 0 ? obras : new[] { Obra() },
            livro is null ? Array.Empty<RegistroLivro>() : new[] { livro },
            estudo is null ? Array.Empty<RegistroEstudo>() : new[] { estudo });

    private static IReadOnlyList<string> Validar(ConjuntoRegistros c) => ValidadorConteudo.Validar(c, Livros);

    [Fact]
    public void Validar_RetornaVazio_QuandoConteudoValido()
    {
        Validar(Conjunto(LivroValido(), EstudoValido())).Should().BeEmpty();
    }

    // Regra 1
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoCampoObrigatorioAusente()
    {
        var l = LivroValido();
        l.Resumo = " ";

        Validar(Conjunto(l)).Should().ContainSingle(e => e.Contains(ArqLivro) && e.Contains("resumo"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoSlugDoArquivoDifereDoLivro()
    {
        var l = LivroValido();
        l.Arquivo = "content/livros/jeremias.yaml";

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("jeremias.yaml") && e.Contains("livro"));
    }

    // Regra 2
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoLivroInexistente()
    {
        var l = LivroValido();
        l.Livro = "hezequias";
        l.Arquivo = "content/livros/hezequias.yaml";

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("hezequias.yaml") && e.Contains("livro"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoEstudoCitaLivroInexistente()
    {
        var s = EstudoValido();
        s.Livros.Add("hezequias");

        Validar(Conjunto(estudo: s)).Should().Contain(e => e.Contains(ArqEstudo) && e.Contains("livros[1]"));
    }

    // Regra 3
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoReferenciaMalFormada()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem
            { Ano = -701, Texto = "x.[^1]", Referencias = { "Isaías" } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("referencias"));
    }

    [Fact]
    public void Validar_Aceita_QuandoReferenciaComHifenEIntervalo()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem
            { Ano = -701, Texto = "x.[^1]", Referencias = { "Isaías 36-37", "1 Pedro 2:22–25" } });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErro_QuandoReferenciaDeLivroForaDaLista()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem
            { Ano = -701, Texto = "x.[^1]", Referencias = { "Obadias 1:1" } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("Obadias 1:1"));
    }

    // Regra 4
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoMarcadorSemNota()
    {
        var l = LivroValido();
        l.Contexto!.Explicacao = "Texto.[^1][^2]";

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("[^2]"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoNotaNaoUsada()
    {
        var l = LivroValido();
        l.Contexto!.Notas.Add(Nota(2));

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("nota 2"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoNumeroDeNotaDuplicado()
    {
        var l = LivroValido();
        l.Contexto!.Notas.Add(Nota(1));

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("duplicado"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoObraDaNotaNaoExisteNoCatalogo()
    {
        var l = LivroValido();
        l.Contexto!.Notas[0].Obra = "inexistente";

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("inexistente"));
    }

    [Fact]
    public void Validar_Aceita_QuandoMarcadorEmTextoDeItemDoBloco()
    {
        var l = LivroValido();
        l.Contexto!.Explicacao = "Texto sem nota.";
        l.Contexto.Personagens.Add(new RegistroItem { Nome = "Ezequias", Texto = "Rei.[^1]" });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    // Regra 5
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoContextoSemNota()
    {
        var l = LivroValido();
        l.Contexto = new RegistroBloco { Explicacao = "Sem nota." };

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("contexto") && e.Contains("ao menos uma nota"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoEstudoSemNota()
    {
        var s = EstudoValido();
        s.Notas.Clear();
        s.CorpoMarkdown = "Corpo.";

        Validar(Conjunto(estudo: s)).Should().Contain(e => e.Contains(ArqEstudo) && e.Contains("ao menos uma nota"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoAcontecimentoSemAnoNoMundo()
    {
        var l = LivroValido();
        l.Mundo!.Acontecimentos.Add(new RegistroItem { Quando = "Em 701 a.C.", Texto = "x.[^1]" });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("mundo.acontecimentos[0].ano"));
    }

    [Fact]
    public void Validar_Aceita_QuandoAcontecimentoDoPanoramaSemAno()
    {
        var l = LivroValido();
        l.Panorama!.Acontecimentos.Add(new RegistroItem { Quando = "Em 701 a.C.", Texto = "x." });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    // Regra 6
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoAspasSemNotaNemReferencia()
    {
        var l = LivroValido();
        l.Panorama!.Personagens.Add(new RegistroItem { Nome = "Ezequias", Texto = "Disse \"confia\" no Senhor." });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("aspas"));
    }

    [Theory]
    [InlineData("Disse “confia” no Senhor.")]
    [InlineData("Disse «confia» no Senhor.")]
    public void Validar_RetornaErro_QuandoAspasTipograficasSemFonte(string texto)
    {
        var l = LivroValido();
        l.Panorama!.Personagens.Add(new RegistroItem { Nome = "E", Texto = texto });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("aspas"));
    }

    [Fact]
    public void Validar_Aceita_QuandoAspasComReferencias()
    {
        var l = LivroValido();
        l.Panorama!.Personagens.Add(new RegistroItem
            { Nome = "Ezequias", Texto = "Disse \"confia\".", Referencias = { "Isaías 36:1" } });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    // Regra 7
    [Theory]
    [InlineData("cumpridaEmCristo")]
    [InlineData("promessaAguardada")]
    public void Validar_RetornaErroComArquivo_QuandoCumprimentoSemReferenciaDoNovoTestamento(string categoria)
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroProfecia
            { Referencia = "Isaías 7:14", Anuncia = "a", Categoria = categoria, Cumprimento = { "2 Reis 19:35" } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("profecias[0].cumprimento"));
    }

    [Fact]
    public void Validar_Aceita_QuandoCumprimentoComReferenciaDoNovoTestamento()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroProfecia
            { Referencia = "Isaías 7:14", Anuncia = "a", Categoria = "cumpridaEmCristo", Cumprimento = { "Mateus 1:22–23" } });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoCumpridaNaHistoriaSemFatoHistorico()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroProfecia
            { Referencia = "Isaías 37:7", Anuncia = "a", Categoria = "cumpridaNaHistoria" });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("fatoHistorico"));
    }

    [Fact]
    public void Validar_Aceita_QuandoCumpridaNaHistoriaComFatoHistorico()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroProfecia
            { Referencia = "Isaías 37:7", Anuncia = "a", Categoria = "cumpridaNaHistoria", FatoHistorico = "Senaqueribe morreu em 681 a.C." });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoCategoriaDeProfeciaDesconhecida()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroProfecia { Referencia = "Isaías 7:14", Anuncia = "a", Categoria = "outra" });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("categoria"));
    }

    // Regra 8
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoItemApontaParaEstudoNaoPublicado()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem { Ano = -701, Texto = "x.[^1]", Estudo = "nao-existe" });

        Validar(Conjunto(l, EstudoValido())).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("nao-existe"));
    }

    [Fact]
    public void Validar_Aceita_QuandoItemApontaParaEstudoPublicado()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem { Ano = -701, Texto = "x.[^1]", Estudo = "senaqueribe" });

        Validar(Conjunto(l, EstudoValido())).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoRelacionadoNaoPublicado()
    {
        var s = EstudoValido();
        s.Relacionados.Add("fantasma");

        Validar(Conjunto(estudo: s)).Should().Contain(e => e.Contains(ArqEstudo) && e.Contains("fantasma"));
    }

    // Regra 9
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoUrlDeObraNaoHttps()
    {
        var o = Obra();
        o.Url = "http://exemplo.com";

        Validar(Conjunto(obras: o)).Should().Contain(e => e.Contains("bibliografia.yaml") && e.Contains("https://"));
    }

    [Fact]
    public void Validar_Aceita_QuandoUrlDeObraHttps()
    {
        var o = Obra();
        o.Url = "https://exemplo.com";

        Validar(Conjunto(obras: o)).Should().BeEmpty();
    }

    // Regra 10
    [Theory]
    [InlineData("Corpo <script>alert(1)</script>.[^1]")]
    [InlineData("Corpo <b>forte</b>.[^1]")]
    [InlineData("Corpo <img src=x>.[^1]")]
    public void Validar_RetornaErroComArquivo_QuandoCorpoDoEstudoTemHtmlBruto(string corpo)
    {
        var s = EstudoValido();
        s.CorpoMarkdown = corpo;

        Validar(Conjunto(estudo: s)).Should().Contain(e => e.Contains(ArqEstudo) && e.Contains("HTML"));
    }

    [Fact]
    public void Validar_Aceita_QuandoCorpoDoEstudoTemMenorQueSemTag()
    {
        var s = EstudoValido();
        s.CorpoMarkdown = "Se a < b e b > c, então sim.[^1]";

        Validar(Conjunto(estudo: s)).Should().BeEmpty();
    }

    // Regra 11
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoTrechoDaCitacaoNaoPertenceAoLivro()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao
            { Trecho = "Salmos 22:1", CitadoEm = { "Mateus 27:46" }, Notas = { Nota() } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("citacoes[0].trecho"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoCitadoEmSemNovoTestamento()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao
            { Trecho = "Isaías 40:3", CitadoEm = { "Miquéias 5:2" }, Notas = { Nota() } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("citacoes[0].citadoEm"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoCitacaoSemNota()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao { Trecho = "Isaías 40:3", CitadoEm = { "Mateus 3:3" } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("citacoes[0].notas"));
    }

    [Fact]
    public void Validar_Aceita_QuandoCitacaoCompleta()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao
            { Trecho = "Isaías 40:3", CitadoEm = { "Mateus 3:3" }, Notas = { Nota() } });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_AcumulaTodosOsErros_QuandoVariosArquivosInvalidos()
    {
        var l = LivroValido();
        l.Resumo = null;
        var s = EstudoValido();
        s.Titulo = null;
        var o = Obra();
        o.Url = "http://x";

        var erros = Validar(Conjunto(l, s, o));

        erros.Should().Contain(e => e.Contains(ArqLivro));
        erros.Should().Contain(e => e.Contains(ArqEstudo));
        erros.Should().Contain(e => e.Contains("bibliografia.yaml"));
    }
}
