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

/// <summary>Profecia ou promessa: só aponta o trecho, sem categoria nem indicação de cumprimento.</summary>
public sealed record ApontamentoDto(
    string Referencia,
    string Anuncia,
    IReadOnlyList<NotaDto> Notas);

public sealed record PerfilItemDto(
    string Rotulo,
    string Texto,
    IReadOnlyList<string> Referencias);

public sealed record CitacaoDto(
    string Trecho,
    IReadOnlyList<string> CitadoEm,
    string? Comentario,
    IReadOnlyList<NotaDto> Notas);

public sealed record LivroPanoramaDto(
    string LivroSlug,
    string Resumo,
    IReadOnlyList<PerfilItemDto> Perfil,
    BlocoDto Panorama,
    BlocoDto Contexto,
    BlocoDto Mundo,
    IReadOnlyList<ApontamentoDto> Profecias,
    IReadOnlyList<ApontamentoDto> Promessas,
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
