namespace BibliaTeologica.Application.Livros.Dtos;

public record LivroDetailDto(
    string Slug,
    string Nome,
    int Ordem,
    string Secao,
    string SecaoRotulo,
    string Testamento,
    string Quem,
    string Porque,
    string ParaQuem,
    string Estrutura,
    string Tema,
    IReadOnlyList<string> Ideias,
    string Versiculo);
