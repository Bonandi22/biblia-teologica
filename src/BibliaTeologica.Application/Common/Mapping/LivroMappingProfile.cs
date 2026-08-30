using AutoMapper;
using BibliaTeologica.Application.Livros.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;

namespace BibliaTeologica.Application.Common.Mapping;

public class LivroMappingProfile : Profile
{
    public LivroMappingProfile()
    {
        CreateMap<Livro, LivroListItemDto>()
            .ForCtorParam(nameof(LivroListItemDto.Secao), o => o.MapFrom(l => l.Secao.ToString()))
            .ForCtorParam(nameof(LivroListItemDto.SecaoRotulo), o => o.MapFrom(l => l.Secao.Rotulo()));

        CreateMap<Livro, LivroDetailDto>()
            .ForCtorParam(nameof(LivroDetailDto.Secao), o => o.MapFrom(l => l.Secao.ToString()))
            .ForCtorParam(nameof(LivroDetailDto.SecaoRotulo), o => o.MapFrom(l => l.Secao.Rotulo()))
            .ForCtorParam(nameof(LivroDetailDto.Testamento), o => o.MapFrom(l => l.Testamento.ToString()))
            .ForCtorParam(nameof(LivroDetailDto.Ideias), o => o.MapFrom(l => l.Ideias));
    }
}
