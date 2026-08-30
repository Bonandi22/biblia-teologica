using System.ComponentModel;
using System.Reflection;

namespace BibliaTeologica.Domain.Enums;

public static class SecaoCanonicaExtensions
{
    public static string Rotulo(this SecaoCanonica secao)
    {
        var campo = typeof(SecaoCanonica).GetField(secao.ToString());
        return campo?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? secao.ToString();
    }

    public static Testamento Testamento(this SecaoCanonica secao)
        => secao <= SecaoCanonica.ProfetasMenores ? Enums.Testamento.Antigo : Enums.Testamento.Novo;

    public static SecaoCanonica FromRotulo(string rotulo)
    {
        if (string.IsNullOrWhiteSpace(rotulo))
            throw new ArgumentException("Rótulo de seção não pode ser vazio.", nameof(rotulo));

        foreach (var secao in Enum.GetValues<SecaoCanonica>())
            if (string.Equals(secao.Rotulo(), rotulo.Trim(), StringComparison.OrdinalIgnoreCase))
                return secao;

        throw new ArgumentException($"Seção canônica desconhecida: '{rotulo}'.", nameof(rotulo));
    }
}
