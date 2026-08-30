namespace BibliaTeologica.Application.Common.Interfaces;

public interface IUsuarioAtual
{
    Guid? Id { get; }
    string? Email { get; }
    bool EstaAutenticado { get; }
    bool EhAdmin { get; }
}
