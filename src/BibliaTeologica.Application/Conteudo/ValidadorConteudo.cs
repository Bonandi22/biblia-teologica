using System.Text.RegularExpressions;
using BibliaTeologica.Domain.Biblia;

namespace BibliaTeologica.Application.Conteudo;

/// <summary>
/// Valida registros de conteúdo já escolhidos para publicação (as 11 regras de formato.md).
/// Acumula todos os erros; cada mensagem traz arquivo e caminho.
/// </summary>
public static partial class ValidadorConteudo
{
    private const string ArquivoBibliografia = "bibliografia.yaml";
    private const int PrimeiraOrdemNovoTestamento = 40;

    private static readonly string[] Categorias = { "cumpridaEmCristo", "cumpridaNaHistoria", "promessaAguardada" };

    public static IReadOnlyList<string> Validar(ConjuntoRegistros c, IReadOnlyList<LivroCanonico> livros)
    {
        var erros = new List<string>();
        c = c with
        {
            Obras = c.Obras ?? Array.Empty<RegistroObra>(),
            Livros = c.Livros ?? Array.Empty<RegistroLivro>(),
            Estudos = c.Estudos ?? Array.Empty<RegistroEstudo>(),
        };
        var ordemPorSlug = livros.ToDictionary(l => l.Slug, l => l.Ordem, StringComparer.Ordinal);
        var obraIds = c.Obras.Where(o => !string.IsNullOrWhiteSpace(o.Id)).Select(o => o.Id!).ToHashSet(StringComparer.Ordinal);
        var estudoSlugs = c.Estudos.Select(e => SlugDoArquivo(e.Arquivo)).ToHashSet(StringComparer.Ordinal);

        var ctx = new Contexto(erros, ordemPorSlug, obraIds, estudoSlugs);

        ValidarObras(c.Obras, ctx);
        foreach (var livro in c.Livros)
            ValidarLivro(livro, ctx);
        foreach (var estudo in c.Estudos)
            ValidarEstudo(estudo, ctx);

        return erros;
    }

    private sealed record Contexto(
        List<string> Erros,
        IReadOnlyDictionary<string, int> OrdemPorSlug,
        HashSet<string> ObraIds,
        HashSet<string> EstudoSlugs)
    {
        public void Erro(string arquivo, string caminho, string mensagem) =>
            Erros.Add($"{arquivo}: {caminho}: {mensagem}");
    }

    // ---------- Obras (regras 1 e 9) ----------

    private static void ValidarObras(IReadOnlyList<RegistroObra> obras, Contexto ctx)
    {
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < obras.Count; i++)
        {
            var o = obras[i];
            var p = $"obras[{i}]";
            Obrigatorio(ctx, ArquivoBibliografia, $"{p}.id", o.Id);
            Obrigatorio(ctx, ArquivoBibliografia, $"{p}.autor", o.Autor);
            Obrigatorio(ctx, ArquivoBibliografia, $"{p}.titulo", o.Titulo);
            Obrigatorio(ctx, ArquivoBibliografia, $"{p}.detalhes", o.Detalhes);

            if (!string.IsNullOrWhiteSpace(o.Id) && !vistos.Add(o.Id))
                ctx.Erro(ArquivoBibliografia, $"{p}.id", $"id de obra duplicado '{o.Id}'.");

            if (!string.IsNullOrWhiteSpace(o.Url) && !o.Url.StartsWith("https://", StringComparison.Ordinal))
                ctx.Erro(ArquivoBibliografia, $"{p}.url", "a url da obra deve começar com https://.");
        }
    }

    // ---------- Livros ----------

    private static void ValidarLivro(RegistroLivro l, Contexto ctx)
    {
        var a = l.Arquivo;
        Obrigatorio(ctx, a, "livro", l.Livro);
        Obrigatorio(ctx, a, "resumo", l.Resumo);
        Obrigatorio(ctx, a, "quem", l.Quem);
        Obrigatorio(ctx, a, "paraQuem", l.ParaQuem);
        Obrigatorio(ctx, a, "porque", l.Porque);
        // Estes campos não têm lista de notas: aspas só passam com marcador [^n], que então fica sem nota (rejeitado).
        foreach (var (campo, valor) in new[] { ("resumo", l.Resumo), ("quem", l.Quem), ("paraQuem", l.ParaQuem), ("porque", l.Porque) })
        {
            ExigeOrigem(ctx, a, campo, valor, false);
            ValidarMarcadoresSemNota(valor, campo, a, ctx);
        }

        if (!string.IsNullOrWhiteSpace(l.Livro))
        {
            if (!ctx.OrdemPorSlug.ContainsKey(l.Livro))
                ctx.Erro(a, "livro", $"livro '{l.Livro}' não existe entre os livros canônicos.");
            if (SlugDoArquivo(a) != l.Livro)
                ctx.Erro(a, "livro", $"o slug do arquivo ('{SlugDoArquivo(a)}') difere do livro '{l.Livro}'.");
        }

        ValidarBloco(l.Panorama, "panorama", exigeNota: false, exigeAno: false, a, ctx);
        ValidarBloco(l.Contexto, "contexto", exigeNota: true, exigeAno: true, a, ctx);
        ValidarBloco(l.Mundo, "mundo", exigeNota: true, exigeAno: true, a, ctx);

        for (var i = 0; i < l.Profecias.Count; i++)
            ValidarProfecia(l.Profecias[i], $"profecias[{i}]", a, ctx);

        for (var i = 0; i < l.Citacoes.Count; i++)
            ValidarCitacao(l.Citacoes[i], $"citacoes[{i}]", l.Livro, a, ctx);
    }

    private static void ValidarBloco(RegistroBloco? b, string caminho, bool exigeNota, bool exigeAno, string a, Contexto ctx)
    {
        if (b is null)
        {
            ctx.Erro(a, caminho, "campo obrigatório ausente.");
            return;
        }

        Obrigatorio(ctx, a, $"{caminho}.explicacao", b.Explicacao);

        if (exigeNota && b.Notas.Count == 0)
            ctx.Erro(a, $"{caminho}.notas", "o bloco precisa de ao menos uma nota.");

        var textos = new List<string?> { b.Explicacao };
        ValidarItens(b.Personagens, $"{caminho}.personagens", true, false, a, ctx, textos);
        ValidarItens(b.Acontecimentos, $"{caminho}.acontecimentos", false, exigeAno, a, ctx, textos);

        ExigeOrigem(ctx, a, $"{caminho}.explicacao", b.Explicacao, false);

        ValidarNotasEMarcadores(b.Notas, textos, $"{caminho}.notas", a, ctx);
    }

    private static void ValidarItens(IReadOnlyList<RegistroItem> itens, string caminho, bool personagens, bool exigeAno, string a, Contexto ctx, List<string?> textos)
    {
        for (var i = 0; i < itens.Count; i++)
        {
            var it = itens[i];
            var p = $"{caminho}[{i}]";
            textos.Add(it.Texto);
            textos.Add(it.SaberMais);

            Obrigatorio(ctx, a, personagens ? $"{p}.nome" : $"{p}.quando", personagens ? it.Nome : it.Quando);
            Obrigatorio(ctx, a, $"{p}.texto", it.Texto);

            if (exigeAno && it.Ano is null)
                ctx.Erro(a, $"{p}.ano", "acontecimentos de contexto e mundo precisam de ano.");

            ValidarReferencias(it.Referencias, $"{p}.referencias", a, ctx);

            var temAspas = ComAspas(it.Texto) || ComAspas(it.SaberMais);
            var temFonte = it.Referencias.Count > 0
                || MarcadorRegex().IsMatch(it.Texto ?? string.Empty)
                || MarcadorRegex().IsMatch(it.SaberMais ?? string.Empty);
            if (temAspas && !temFonte)
                ctx.Erro(a, p, "texto com aspas precisa de nota [^n] ou de referencias no mesmo item.");

            if (!string.IsNullOrWhiteSpace(it.Estudo) && !ctx.EstudoSlugs.Contains(it.Estudo))
                ctx.Erro(a, $"{p}.estudo", $"o estudo '{it.Estudo}' não está publicado nesta importação.");
        }
    }

    private static void ValidarProfecia(RegistroProfecia pf, string p, string a, Contexto ctx)
    {
        Obrigatorio(ctx, a, $"{p}.anuncia", pf.Anuncia);
        ValidarReferenciaUnica(pf.Referencia, $"{p}.referencia", a, ctx);
        ValidarReferencias(pf.Cumprimento, $"{p}.cumprimento", a, ctx);
        var origemPf = !string.IsNullOrWhiteSpace(pf.Referencia) || pf.Cumprimento.Count > 0;
        ExigeOrigem(ctx, a, $"{p}.anuncia", pf.Anuncia, origemPf);
        ExigeOrigem(ctx, a, $"{p}.fatoHistorico", pf.FatoHistorico, origemPf);
        ValidarNotasEMarcadores(pf.Notas, new List<string?> { pf.Anuncia, pf.FatoHistorico }, $"{p}.notas", a, ctx, exigeUso: false);

        if (string.IsNullOrWhiteSpace(pf.Categoria) || !Categorias.Contains(pf.Categoria))
        {
            ctx.Erro(a, $"{p}.categoria", $"categoria inválida; use {string.Join(" | ", Categorias)}.");
            return;
        }

        if (pf.Categoria is "cumpridaEmCristo" or "promessaAguardada"
            && !pf.Cumprimento.Any(r => TemLivroDoNovoTestamento(r, ctx)))
            ctx.Erro(a, $"{p}.cumprimento", "precisa de ao menos uma referência do Novo Testamento (Mateus a Apocalipse).");

        if (pf.Categoria == "cumpridaNaHistoria" && string.IsNullOrWhiteSpace(pf.FatoHistorico))
            ctx.Erro(a, $"{p}.fatoHistorico", "obrigatório em profecias cumpridaNaHistoria.");
    }

    private static void ValidarCitacao(RegistroCitacao c, string p, string? livroDoArquivo, string a, Contexto ctx)
    {
        if (ValidarReferenciaUnica(c.Trecho, $"{p}.trecho", a, ctx, out var slug)
            && livroDoArquivo is not null && slug != livroDoArquivo)
            ctx.Erro(a, $"{p}.trecho", $"o trecho '{c.Trecho}' não pertence ao livro '{livroDoArquivo}'.");

        ValidarReferencias(c.CitadoEm, $"{p}.citadoEm", a, ctx);
        if (!c.CitadoEm.Any(r => TemLivroDoNovoTestamento(r, ctx)))
            ctx.Erro(a, $"{p}.citadoEm", "precisa de ao menos uma referência do Novo Testamento.");

        if (c.Notas.Count == 0)
            ctx.Erro(a, $"{p}.notas", "a citação precisa de ao menos uma nota, com a obra usada na conferência.");
        ExigeOrigem(ctx, a, $"{p}.comentario", c.Comentario, !string.IsNullOrWhiteSpace(c.Trecho) || c.CitadoEm.Count > 0);
        ValidarNotasEMarcadores(c.Notas, new List<string?> { c.Comentario }, $"{p}.notas", a, ctx, exigeUso: false);
    }

    // ---------- Estudos ----------

    private static void ValidarEstudo(RegistroEstudo e, Contexto ctx)
    {
        var a = e.Arquivo;
        Obrigatorio(ctx, a, "titulo", e.Titulo);
        Obrigatorio(ctx, a, "resumo", e.Resumo);
        Obrigatorio(ctx, a, "corpo", e.CorpoMarkdown);

        if (e.Livros.Count == 0)
            ctx.Erro(a, "livros", "campo obrigatório ausente.");
        for (var i = 0; i < e.Livros.Count; i++)
            if (!ctx.OrdemPorSlug.ContainsKey(e.Livros[i]))
                ctx.Erro(a, $"livros[{i}]", $"livro '{e.Livros[i]}' não existe entre os livros canônicos.");

        for (var i = 0; i < e.Relacionados.Count; i++)
            if (!ctx.EstudoSlugs.Contains(e.Relacionados[i]))
                ctx.Erro(a, $"relacionados[{i}]", $"o estudo '{e.Relacionados[i]}' não está publicado nesta importação.");

        if (e.Notas.Count == 0)
            ctx.Erro(a, "notas", "o estudo precisa de ao menos uma nota.");

        if (e.CorpoMarkdown is not null && HtmlBrutoRegex().IsMatch(e.CorpoMarkdown))
            ctx.Erro(a, "corpo", "o corpo não pode conter tag HTML bruta.");

        var paragrafos = ParagrafosRegex().Split(e.CorpoMarkdown ?? string.Empty);
        for (var i = 0; i < paragrafos.Length; i++)
            ExigeOrigem(ctx, a, $"corpo (parágrafo {i + 1})", paragrafos[i], false);

        ValidarNotasEMarcadores(e.Notas, new List<string?> { e.CorpoMarkdown }, "notas", a, ctx);
    }

    // ---------- Comuns ----------

    /// <summary>Regra 6: texto com aspas precisa de origem (marcador [^n] no próprio texto ou referência do item).</summary>
    private static void ExigeOrigem(Contexto ctx, string a, string caminho, string? texto, bool temOrigemDoItem)
    {
        if (ComAspas(texto) && !temOrigemDoItem && !MarcadorRegex().IsMatch(texto!))
            ctx.Erro(a, caminho, "texto com aspas precisa de origem: nota [^n] no mesmo trecho ou referência do item.");
    }

    private static void ValidarMarcadoresSemNota(string? texto, string caminho, string a, Contexto ctx)
    {
        if (texto is not null && MarcadorRegex().IsMatch(texto))
            ctx.Erro(a, caminho, "este campo não tem lista de notas; remova o marcador [^n].");
    }

    private static void ValidarNotasEMarcadores(IReadOnlyList<RegistroNota> notas, List<string?>? textos, string caminho, string a, Contexto ctx, bool exigeUso = true)
    {
        var numeros = new HashSet<int>();
        for (var i = 0; i < notas.Count; i++)
        {
            var n = notas[i];
            if (n.Numero < 1)
                ctx.Erro(a, $"{caminho}[{i}].numero", "o número da nota deve ser >= 1.");
            Obrigatorio(ctx, a, $"{caminho}[{i}].local", n.Local);
            if (!numeros.Add(n.Numero))
                ctx.Erro(a, $"{caminho}[{i}].numero", $"número de nota duplicado: {n.Numero}.");
            if (string.IsNullOrWhiteSpace(n.Obra) || !ctx.ObraIds.Contains(n.Obra))
                ctx.Erro(a, $"{caminho}[{i}].obra", $"a obra '{n.Obra}' não existe no catálogo.");
        }

        if (textos is null)
            return;

        var usados = new HashSet<int>();
        foreach (var t in textos.Where(t => t is not null))
            foreach (Match m in MarcadorRegex().Matches(t!))
                if (int.TryParse(m.Groups[1].Value, out var num))
                    usados.Add(num);

        foreach (var num in usados.Where(u => !numeros.Contains(u)).OrderBy(u => u))
            ctx.Erro(a, caminho, $"o marcador [^{num}] não tem nota correspondente.");
        if (!exigeUso)
            return;
        foreach (var num in numeros.Where(n => !usados.Contains(n)).OrderBy(n => n))
            ctx.Erro(a, caminho, $"a nota {num} não é usada em nenhum marcador [^{num}].");
    }

    private static void ValidarReferencias(IReadOnlyList<string> refs, string caminho, string a, Contexto ctx)
    {
        for (var i = 0; i < refs.Count; i++)
            ValidarReferenciaUnica(refs[i], $"{caminho}[{i}]", a, ctx);
    }

    private static void ValidarReferenciaUnica(string? texto, string caminho, string a, Contexto ctx) =>
        ValidarReferenciaUnica(texto, caminho, a, ctx, out _);

    private static bool ValidarReferenciaUnica(string? texto, string caminho, string a, Contexto ctx, out string slug)
    {
        slug = string.Empty;
        if (string.IsNullOrWhiteSpace(texto))
        {
            ctx.Erro(a, caminho, "referência obrigatória ausente.");
            return false;
        }

        if (!ReferenciaBiblica.TentarLer(texto, out slug) || !ctx.OrdemPorSlug.ContainsKey(slug))
        {
            ctx.Erro(a, caminho, $"referência bíblica inválida '{texto}'; use 'Livro cap[:vers][–fim]'.");
            slug = string.Empty;
            return false;
        }

        return true;
    }

    private static bool TemLivroDoNovoTestamento(string referencia, Contexto ctx) =>
        ReferenciaBiblica.TentarLer(referencia, out var slug)
        && ctx.OrdemPorSlug.TryGetValue(slug, out var ordem)
        && ordem >= PrimeiraOrdemNovoTestamento;

    private static void Obrigatorio(Contexto ctx, string arquivo, string caminho, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            ctx.Erro(arquivo, caminho, "campo obrigatório ausente.");
    }

    private static string SlugDoArquivo(string arquivo) => Path.GetFileNameWithoutExtension(arquivo);

    private static bool ComAspas(string? texto) =>
        texto is not null && texto.AsSpan().IndexOfAny('"', '“', '«') >= 0;

    [GeneratedRegex(@"\r?\n[ \t]*\r?\n")]
    private static partial Regex ParagrafosRegex();

    [GeneratedRegex(@"\[\^([0-9]+)\]")]
    private static partial Regex MarcadorRegex();

    // "<" imediatamente seguido de letra, "/" ou "!" (comentário); "a < b" não casa.
    [GeneratedRegex(@"<(?:/?[A-Za-z][^<>]*>|!--)")]
    private static partial Regex HtmlBrutoRegex();
}
