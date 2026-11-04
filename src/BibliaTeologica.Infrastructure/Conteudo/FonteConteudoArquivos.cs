using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Infrastructure.Configuracao;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BibliaTeologica.Infrastructure.Conteudo;

public sealed partial class FonteConteudoArquivos : IFonteConteudo
{
    private const long LimiteBytes = 1024 * 1024;
    private const int CabecalhoBytes = 16 * 1024;

    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private readonly string _pasta;
    private readonly ILogger<FonteConteudoArquivos> _logger;

    public FonteConteudoArquivos(IOptions<ConteudoOptions> options, ILogger<FonteConteudoArquivos> logger)
    {
        var pasta = options.Value.Pasta;
        _pasta = Path.IsPathRooted(pasta) ? pasta : Path.Combine(AppContext.BaseDirectory, pasta);
        _logger = logger;
    }

    public async Task<ConjuntoRegistros> LerAsync(bool incluirRascunhos, CancellationToken ct)
    {
        var obras = new List<RegistroObra>();
        var livros = new List<RegistroLivro>();
        var estudos = new List<RegistroEstudo>();

        var bib = Path.Combine(_pasta, "bibliografia.yaml");
        if (Elegivel(bib))
        {
            var texto = await LerTextoAsync(bib, ct);
            var lista = Desserializar<List<RegistroObra>>(texto, bib) ?? new();
            VerificarNulos(lista, bib);
            obras.AddRange(lista);
        }

        foreach (var arq in Listar("livros", "*.yaml"))
        {
            if (!Incluir(arq, incluirRascunhos)) continue;
            var texto = await LerTextoAsync(arq, ct);
            var livro = Desserializar<RegistroLivro>(texto, arq)
                        ?? throw new ConteudoInvalidoException($"{Path.GetFileName(arq)}: arquivo vazio.");
            VerificarNulos(livro, arq);
            livro.Arquivo = Path.GetFileName(arq);
            livros.Add(livro);
        }

        foreach (var arq in Listar("estudos", "*.md"))
        {
            if (!Incluir(arq, incluirRascunhos)) continue;
            var texto = await LerTextoAsync(arq, ct);
            var (front, corpo) = SepararFrontmatter(texto, arq);
            var estudo = Desserializar<RegistroEstudo>(front, arq)
                         ?? throw new ConteudoInvalidoException($"{Path.GetFileName(arq)}: frontmatter vazio.");
            VerificarNulos(estudo, arq);
            estudo.Arquivo = Path.GetFileName(arq);
            estudo.CorpoMarkdown = corpo;
            estudos.Add(estudo);
        }

        return new ConjuntoRegistros(obras, livros, estudos);
    }

    /// <summary>
    /// Decide se o arquivo entra, lendo só o início e sem lançar: rascunho com defeito
    /// (grande demais, ilegível, frontmatter quebrado) é ignorado. Sem estado identificável, entra.
    /// </summary>
    private bool Incluir(string arquivo, bool incluirRascunhos)
    {
        if (incluirRascunhos) return true;
        string inicio;
        try
        {
            using var fs = new FileStream(arquivo, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buf = new byte[CabecalhoBytes];
            var n = fs.Read(buf, 0, buf.Length);
            inicio = new UTF8Encoding(false).GetString(buf, 0, n).TrimStart('﻿').Replace("\r\n", "\n").Replace('\r', '\n');
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return true;
        }

        if (arquivo.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            if (!inicio.StartsWith("---\n", StringComparison.Ordinal)) return true;
            var fim = inicio.IndexOf("\n---", 4, StringComparison.Ordinal);
            inicio = fim < 0 ? inicio[4..] : inicio[4..fim];
        }
        var m = EstadoRegex().Match(inicio);
        var rascunho = m.Success && m.Groups[1].Value == "rascunho";
        if (rascunho && !incluirRascunhos)
        {
            _logger.LogWarning("Rascunho ignorado: {Arquivo}", Path.GetFileName(arquivo));
            return false;
        }
        return true;
    }

    private IEnumerable<string> Listar(string subpasta, string padrao)
    {
        var dir = Path.Combine(_pasta, subpasta);
        if (!Directory.Exists(dir)) return Array.Empty<string>();
        return Directory.EnumerateFiles(dir, padrao, SearchOption.TopDirectoryOnly)
            .Where(Elegivel)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
    }

    private bool Elegivel(string caminho)
    {
        var info = new FileInfo(caminho);
        if (!info.Exists) return false;
        if (info.LinkTarget is not null)
        {
            _logger.LogWarning("Link simbólico ignorado: {Arquivo}", info.Name);
            return false;
        }
        return true;
    }

    private static async Task<string> LerTextoAsync(string caminho, CancellationToken ct)
    {
        var nome = Path.GetFileName(caminho);
        if (new FileInfo(caminho).Length > LimiteBytes)
            throw new ConteudoInvalidoException($"{nome}: arquivo maior que 1 MB.");
        try
        {
            var bytes = await File.ReadAllBytesAsync(caminho, ct);
            var texto = new UTF8Encoding(false).GetString(bytes).TrimStart('﻿');
            return texto.Replace("\r\n", "\n").Replace('\r', '\n');
        }
        catch (IOException e)
        {
            throw new ConteudoInvalidoException($"{nome}: não foi possível ler o arquivo ({e.Message}).");
        }
    }

    private static (string Frontmatter, string Corpo) SepararFrontmatter(string texto, string arquivo)
    {
        var linhas = texto.Split('\n');
        if (linhas.Length == 0 || linhas[0].TrimEnd() != "---")
            throw new ConteudoInvalidoException($"{Path.GetFileName(arquivo)}: frontmatter ausente (esperado '---' na primeira linha).");
        for (var i = 1; i < linhas.Length; i++)
        {
            if (linhas[i].TrimEnd() != "---") continue;
            return (string.Join('\n', linhas[1..i]), string.Join('\n', linhas[(i + 1)..]).Trim('\n'));
        }
        throw new ConteudoInvalidoException($"{Path.GetFileName(arquivo)}: frontmatter sem '---' de fechamento.");
    }

    private static T? Desserializar<T>(string yaml, string arquivo)
    {
        try
        {
            return Yaml.Deserialize<T>(yaml);
        }
        catch (YamlException e)
        {
            throw new ConteudoInvalidoException($"{Path.GetFileName(arquivo)}: YAML inválido ({e.Message}).");
        }
    }

    /// <summary>Elemento nulo dentro de lista YAML vira erro do arquivo, não NullReferenceException adiante.</summary>
    private static void VerificarNulos(object? obj, string arquivo)
    {
        var erro = AcharNulo(obj, "");
        if (erro is not null)
            throw new ConteudoInvalidoException($"{Path.GetFileName(arquivo)}: elemento nulo em lista ({erro}).");
    }

    private static string? AcharNulo(object? obj, string caminho)
    {
        if (obj is null || obj is string || obj.GetType().IsPrimitive) return null;
        if (obj is IList lista)
        {
            for (var i = 0; i < lista.Count; i++)
            {
                var caminhoItem = $"{caminho}[{i}]";
                if (lista[i] is null) return caminhoItem;
                var r = AcharNulo(lista[i], caminhoItem);
                if (r is not null) return r;
            }
            return null;
        }
        if (obj.GetType().Namespace?.StartsWith("BibliaTeologica") != true) return null;
        foreach (var p in obj.GetType().GetProperties())
        {
            if (p.GetIndexParameters().Length > 0) continue;
            var r = AcharNulo(p.GetValue(obj), caminho.Length == 0 ? p.Name : $"{caminho}.{p.Name}");
            if (r is not null) return r;
        }
        return null;
    }

    [GeneratedRegex(@"^estado:\s*[""']?(\w+)", RegexOptions.Multiline)]
    private static partial Regex EstadoRegex();
}
