using BibliaTeologica.Application.Livros;
using Microsoft.Extensions.DependencyInjection;

namespace BibliaTeologica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<ILivroQueryService, LivroQueryService>();
        return services;
    }
}
