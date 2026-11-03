using System.Text.Json;
using BibliaTeologica.Application.Conteudo.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;

namespace BibliaTeologica.Application.Conteudo;

/// <summary>
/// Converte registros já validados em documentos prontos para persistir (JSON dos DTOs).
/// Não valida: espera receber o resultado aprovado por <see cref="ValidadorConteudo"/>.
/// </summary>
public static class ConstrutorDocumentos
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<DocumentoConteudo> Construir(
        ConjuntoRegistros conjunto,
        IReadOnlyDictionary<string, string> titulosLivros)
    {
        var obras = conjunto.Obras.Where(o => o.Id is not null).ToDictionary(o => o.Id!, StringComparer.Ordinal);
        var estudosPorSlug = conjunto.Estudos.ToDictionary(SlugEstudo, StringComparer.Ordinal);
        var docs = new List<DocumentoConteudo>();

        foreach (var l in conjunto.Livros)
        {
            var slug = l.Livro!;
            var dto = new LivroPanoramaDto(
                slug, l.Resumo!, l.Quem!, l.ParaQuem!, l.Porque!,
                Bloco(l.Panorama, false, obras),
                Bloco(l.Contexto, true, obras),
                Bloco(l.Mundo, true, obras),
                l.Profecias.Select(p => new ProfeciaDto(
                    p.Referencia!, p.Anuncia!, p.Categoria!, p.Cumprimento.ToList(), p.FatoHistorico, Notas(p.Notas, obras))).ToList(),
                l.Citacoes.Select(c => new CitacaoDto(
                    c.Trecho!, c.CitadoEm.ToList(), c.Comentario, Notas(c.Notas, obras))).ToList());
            var titulo = titulosLivros.TryGetValue(slug, out var t) ? t : slug;
            docs.Add(DocumentoConteudo.Criar(TipoConteudo.Livro, slug, titulo, l.Resumo!, JsonSerializer.Serialize(dto, OpcoesJson)));
        }

        foreach (var e in conjunto.Estudos)
        {
            var slug = SlugEstudo(e);
            var relacionados = e.Relacionados
                .Select(r => estudosPorSlug[r])
                .Select(r => new EstudoResumoDto(SlugEstudo(r), r.Titulo!, r.Resumo!))
                .ToList();
            var dto = new EstudoDto(slug, e.Titulo!, e.Resumo!, e.CorpoMarkdown!, e.Livros.ToList(), relacionados, Notas(e.Notas, obras));
            docs.Add(DocumentoConteudo.Criar(TipoConteudo.Estudo, slug, e.Titulo!, e.Resumo!, JsonSerializer.Serialize(dto, OpcoesJson)));
        }

        return docs;
    }

    private static string SlugEstudo(RegistroEstudo e) => Path.GetFileNameWithoutExtension(e.Arquivo);

    private static BlocoDto Bloco(RegistroBloco? b, bool ordenarPorAno, IReadOnlyDictionary<string, RegistroObra> obras)
    {
        b ??= new RegistroBloco();
        var acontecimentos = ordenarPorAno
            ? b.Acontecimentos.OrderBy(a => a.Ano ?? int.MaxValue).ToList()
            : b.Acontecimentos;
        return new BlocoDto(
            b.Explicacao ?? string.Empty,
            b.Personagens.Select(Item).ToList(),
            acontecimentos.Select(a => new AcontecimentoDto(a.Quando!, Item(a))).ToList(),
            Notas(b.Notas, obras));
    }

    private static ItemDto Item(RegistroItem i) =>
        new(i.Nome ?? string.Empty, i.Texto!, i.SaberMais, i.Estudo, i.Referencias.ToList());

    private static List<NotaDto> Notas(IEnumerable<RegistroNota> notas, IReadOnlyDictionary<string, RegistroObra> obras) =>
        notas.Select(n =>
        {
            var o = obras[n.Obra!];
            return new NotaDto(n.Numero, $"{o.Autor}. {o.Titulo}. {o.Detalhes}, {n.Local}.", o.Url);
        }).ToList();
}
