namespace BibliaTeologica.Infrastructure.Configuracao;

public class AdminSeedOptions
{
    public const string Secao = "AdminSeed";
    public List<AdminSeedEntrada> Entradas { get; set; } = new();
}

public class AdminSeedEntrada
{
    public string Email { get; set; } = string.Empty;
    public string SenhaInicial { get; set; } = string.Empty;
}
