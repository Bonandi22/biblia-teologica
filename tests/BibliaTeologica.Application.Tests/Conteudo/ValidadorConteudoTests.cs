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
        new() { Id = id, Autor = "SBB", Titulo = "Bíblia ARC", Detalhes = "d", Url = null };

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
        Perfil = { new RegistroPerfilItem { Rotulo = "Origem", Texto = "Filho de Amoz.", Referencias = { "Isaías 1:1" } } },
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
            { Quando = "q", Ano = -701, Texto = "x.[^1]", Referencias = { "Isaías" } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("referencias"));
    }

    [Fact]
    public void Validar_Aceita_QuandoReferenciaComHifenEIntervalo()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem
            { Quando = "q", Ano = -701, Texto = "x.[^1]", Referencias = { "Isaías 36-37", "1 Pedro 2:22–25" } });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErro_QuandoReferenciaDeLivroForaDaLista()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem
            { Quando = "q", Ano = -701, Texto = "x.[^1]", Referencias = { "Obadias 1:1" } });

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

    // Regra 7: apontamentos sem categoria nem cumprimento
    [Fact]
    public void Validar_Aceita_QuandoProfeciaEPromessaSoComReferenciaEAnuncia()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroApontamento { Referencia = "Isaías 7:14", Anuncia = "a" });
        l.Promessas.Add(new RegistroApontamento { Referencia = "Isaías 65:17", Anuncia = "b" });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErro_QuandoPromessaSemAnunciaOuReferenciaInvalida()
    {
        var l = LivroValido();
        l.Promessas.Add(new RegistroApontamento { Referencia = "Isaías", Anuncia = " " });

        var erros = Validar(Conjunto(l));

        erros.Should().Contain(e => e.Contains(ArqLivro) && e.Contains("promessas[0].anuncia"));
        erros.Should().Contain(e => e.Contains(ArqLivro) && e.Contains("promessas[0].referencia"));
    }

    // Perfil do autor
    [Fact]
    public void Validar_RetornaErro_QuandoPerfilVazio()
    {
        var l = LivroValido();
        l.Perfil.Clear();

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("perfil") && e.Contains("ao menos um item"));
    }

    [Fact]
    public void Validar_RetornaErro_QuandoItemDoPerfilSemRotuloOuTexto()
    {
        var l = LivroValido();
        l.Perfil.Add(new RegistroPerfilItem());

        var erros = Validar(Conjunto(l));

        erros.Should().Contain(e => e.Contains("perfil[1].rotulo"));
        erros.Should().Contain(e => e.Contains("perfil[1].texto"));
    }

    [Fact]
    public void Validar_RetornaErro_QuandoReferenciaDoPerfilInvalida()
    {
        var l = LivroValido();
        l.Perfil[0].Referencias.Add("Isaías");

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("perfil[0].referencias[1]"));
    }

    [Fact]
    public void Validar_RetornaErro_QuandoTextoDoPerfilTemMarcadorDeNota()
    {
        var l = LivroValido();
        l.Perfil[0].Texto = "Filho de Amoz.[^1]";

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("perfil[0].texto") && e.Contains("[^n]"));
    }

    [Fact]
    public void Validar_RetornaErro_QuandoTextoDoPerfilTemAspasSemReferencia()
    {
        var l = LivroValido();
        l.Perfil[0].Texto = "Diz \"fala\".";
        l.Perfil[0].Referencias.Clear();

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("perfil[0].texto") && e.Contains("aspas"));
    }

    [Fact]
    public void Validar_Aceita_QuandoTextoDoPerfilTemAspasComReferencia()
    {
        var l = LivroValido();
        l.Perfil[0].Texto = "Diz \"fala\".";

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    // Regra 8
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoItemApontaParaEstudoNaoPublicado()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem { Quando = "q", Ano = -701, Texto = "x.[^1]", Estudo = "nao-existe" });

        Validar(Conjunto(l, EstudoValido())).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("nao-existe"));
    }

    [Fact]
    public void Validar_Aceita_QuandoItemApontaParaEstudoPublicado()
    {
        var l = LivroValido();
        l.Contexto!.Acontecimentos.Add(new RegistroItem { Quando = "q", Ano = -701, Texto = "x.[^1]", Estudo = "senaqueribe" });

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
    public void Validar_RetornaErro_QuandoCitadoEmVazio()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao { Trecho = "Isaías 40:3", Notas = { Nota() } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("citacoes[0].citadoEm"));
    }

    [Fact]
    public void Validar_AceitaCitacao_QuandoCitadoSoNoAntigoTestamento()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao
            { Trecho = "Isaías 2:2", CitadoEm = { "Miquéias 4:1" }, Notas = { Nota() } });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErro_QuandoCitadoEmApontaParaOProprioLivro()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao
            { Trecho = "Isaías 40:3", CitadoEm = { "Isaías 41:1", "Mateus 3:3" }, Notas = { Nota() } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("citacoes[0].citadoEm[0]") && e.Contains("próprio livro"));
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

    // Regra 6 ampliada
    [Fact]
    public void Validar_RetornaErro_QuandoResumoTemAspas()
    {
        var l = LivroValido();
        l.Resumo = "Diz \"fala\" aqui.";

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("resumo") && e.Contains("aspas"));
    }

    [Fact]
    public void Validar_RetornaErro_QuandoAnunciaTemAspasSemOrigem()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroApontamento { Referencia = null, Anuncia = "Diz \"fala\"." });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("profecias[0].anuncia") && e.Contains("aspas"));
    }

    [Fact]
    public void Validar_Aceita_QuandoAspasEmAnunciaComReferencia()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroApontamento { Referencia = "Isaías 7:14", Anuncia = "Diz \"virgem\"." });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErro_QuandoComentarioDaCitacaoTemAspasSemOrigem()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao { Trecho = null, Comentario = "Diz \"x\".", Notas = { Nota() } });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains("citacoes[0].comentario") && e.Contains("aspas"));
    }

    [Fact]
    public void Validar_Aceita_QuandoComentarioDaCitacaoTemAspasComMarcadorENota()
    {
        var l = LivroValido();
        l.Citacoes.Add(new RegistroCitacao
        {
            Trecho = "Isaías 40:3", CitadoEm = { "Mateus 3:3" }, Comentario = "Diz \"x\".[^1]", Notas = { Nota() },
        });

        Validar(Conjunto(l)).Should().BeEmpty();
    }

    [Fact]
    public void Validar_RetornaErro_QuandoParagrafoDoEstudoTemAspasSemMarcador()
    {
        var s = EstudoValido();
        s.CorpoMarkdown = "Primeiro com fonte.[^1]\n\nSegundo diz \"algo\" sem fonte.";

        Validar(Conjunto(estudo: s)).Should().Contain(e => e.Contains(ArqEstudo) && e.Contains("parágrafo 2") && e.Contains("aspas"));
    }

    [Fact]
    public void Validar_Aceita_QuandoParagrafoDoEstudoTemAspasComMarcador()
    {
        var s = EstudoValido();
        s.CorpoMarkdown = "Primeiro diz \"algo\".[^1]\n\nSegundo sem aspas.";

        Validar(Conjunto(estudo: s)).Should().BeEmpty();
    }

    // Regra 1 ampliada
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoPersonagemSemNomeOuTexto()
    {
        var l = LivroValido();
        l.Panorama!.Personagens.Add(new RegistroItem());

        var erros = Validar(Conjunto(l));

        erros.Should().Contain(e => e.Contains(ArqLivro) && e.Contains("panorama.personagens[0].nome"));
        erros.Should().Contain(e => e.Contains("panorama.personagens[0].texto"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoAcontecimentoSemQuando()
    {
        var l = LivroValido();
        l.Panorama!.Acontecimentos.Add(new RegistroItem { Texto = "x." });

        Validar(Conjunto(l)).Should().Contain(e => e.Contains(ArqLivro) && e.Contains("panorama.acontecimentos[0].quando"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoNotaSemLocalOuNumeroInvalido()
    {
        var l = LivroValido();
        l.Contexto!.Notas[0].Local = null;
        l.Contexto.Notas[0].Numero = 0;

        var erros = Validar(Conjunto(l));

        erros.Should().Contain(e => e.Contains(ArqLivro) && e.Contains("contexto.notas[0].local"));
        erros.Should().Contain(e => e.Contains(ArqLivro) && e.Contains("contexto.notas[0].numero"));
    }

    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoObraSemDetalhes()
    {
        var o = Obra();
        o.Detalhes = null;

        Validar(Conjunto(obras: o)).Should().Contain(e => e.Contains("bibliografia.yaml") && e.Contains("detalhes"));
    }

    // Regra 4 em profecias e citações
    [Fact]
    public void Validar_RetornaErroComArquivo_QuandoMarcadorEmApontamentoOuCitacaoSemNota()
    {
        var l = LivroValido();
        l.Profecias.Add(new RegistroApontamento { Referencia = "Isaías 7:14", Anuncia = "a.[^3]" });
        l.Promessas.Add(new RegistroApontamento { Referencia = "Isaías 65:17", Anuncia = "b.[^4]" });
        l.Citacoes.Add(new RegistroCitacao
            { Trecho = "Isaías 40:3", CitadoEm = { "Mateus 3:3" }, Comentario = "c.[^5]", Notas = { Nota() } });

        var erros = Validar(Conjunto(l));

        erros.Should().Contain(e => e.Contains("profecias[0].notas") && e.Contains("[^3]"));
        erros.Should().Contain(e => e.Contains("promessas[0].notas") && e.Contains("[^4]"));
        erros.Should().Contain(e => e.Contains("citacoes[0].notas") && e.Contains("[^5]"));
    }

    // Coleções nulas vindas do YAML
    [Fact]
    public void Validar_NaoLanca_QuandoColecoesNulas()
    {
        var l = LivroValido();
        l.Panorama!.Personagens = null!;
        l.Panorama.Acontecimentos = null!;
        l.Panorama.Notas = null!;
        l.Perfil = null!;
        l.Profecias = null!;
        l.Promessas = null!;
        l.Citacoes = null!;
        var s = EstudoValido();
        s.Livros = null!;
        s.Relacionados = null!;
        s.Notas = null!;
        var item = new RegistroItem { Nome = "n", Texto = "t", Referencias = null! };
        l.Contexto!.Personagens.Add(item);
        l.Profecias = new List<RegistroApontamento>
            { new() { Referencia = "Isaías 7:14", Anuncia = "a", Notas = null! } };

        var act = () => Validar(Conjunto(l, s));

        act.Should().NotThrow();
        act().Should().Contain(e => e.Contains(ArqEstudo) && e.Contains("livros"));
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
