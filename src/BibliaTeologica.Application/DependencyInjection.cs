using BibliaTeologica.Application.Livros;
using Microsoft.Extensions.DependencyInjection;

namespace BibliaTeologica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILivroQueryService, LivroQueryService>();
        services.AddScoped<Auth.IEmissorTokens, Auth.EmissorTokens>();
        services.AddScoped<Auth.IAuthService, Auth.AuthService>();
        services.AddScoped<Conta.IContaService, Conta.ContaService>();
        return services;
    }
}
