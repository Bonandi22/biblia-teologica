using BibliaTeologica.Application.Livros.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;

namespace BibliaTeologica.Application.Common.Mapping;

public static class LivroMapper
{
    public static LivroListItemDto ToListItem(Livro l)
        => new(l.Slug, l.Nome, l.Ordem, l.Secao.ToString(), l.Secao.Rotulo());

    public static LivroDetailDto ToDetail(Livro l)
        => new(
            l.Slug,
            l.Nome,
            l.Ordem,
            l.Secao.ToString(),
            l.Secao.Rotulo(),
            l.Testamento.ToString(),
            l.Quem,
            l.Porque,
            l.ParaQuem,
            l.Estrutura,
            l.Tema,
            l.Ideias,
            l.Versiculo);
}
