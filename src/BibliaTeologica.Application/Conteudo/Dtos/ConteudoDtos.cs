namespace BibliaTeologica.Application.Conteudo.Dtos;

public sealed record NotaDto(int Numero, string Citacao, string? Url);

/// <summary>Personagem ou acontecimento; <c>Nome</c> fica vazio em acontecimento.</summary>
public sealed record ItemDto(
    string Nome,
    string Texto,
    string? SaberMais,
    string? EstudoSlug,
    IReadOnlyList<string> Referencias);

public sealed record AcontecimentoDto(string Quando, ItemDto Item);

public sealed record BlocoDto(
    string Explicacao,
    IReadOnlyList<ItemDto> Personagens,
    IReadOnlyList<AcontecimentoDto> Acontecimentos,
    IReadOnlyList<NotaDto> Notas);

public sealed record ProfeciaDto(
    string Referencia,
    string Anuncia,
    string Categoria,
    IReadOnlyList<string> Cumprimento,
    string? FatoHistorico,
    IReadOnlyList<NotaDto> Notas);

public sealed record CitacaoDto(
    string Trecho,
    IReadOnlyList<string> CitadoEm,
    string? Comentario,
    IReadOnlyList<NotaDto> Notas);

public sealed record LivroPanoramaDto(
    string LivroSlug,
    string Resumo,
    string Quem,
    string ParaQuem,
    string Porque,
    BlocoDto Panorama,
    BlocoDto Contexto,
    BlocoDto Mundo,
    IReadOnlyList<ProfeciaDto> Profecias,
    IReadOnlyList<CitacaoDto> Citacoes);

public sealed record EstudoResumoDto(string Slug, string Titulo, string Resumo);

public sealed record EstudoDto(
    string Slug,
    string Titulo,
    string Resumo,
    string CorpoMarkdown,
    IReadOnlyList<string> Livros,
    IReadOnlyList<EstudoResumoDto> Relacionados,
    IReadOnlyList<NotaDto> Notas);
