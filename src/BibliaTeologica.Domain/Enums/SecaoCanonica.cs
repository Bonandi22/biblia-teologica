using System.ComponentModel;

namespace BibliaTeologica.Domain.Enums;

public enum SecaoCanonica
{
    [Description("PENTATEUCO")] Pentateuco = 1,
    [Description("LIVROS HISTÓRICOS")] LivrosHistoricos = 2,
    [Description("LIVROS POÉTICOS")] LivrosPoeticos = 3,
    [Description("PROFETAS MAIORES")] ProfetasMaiores = 4,
    [Description("PROFETAS MENORES")] ProfetasMenores = 5,
    [Description("EVANGELHOS")] Evangelhos = 6,
    [Description("ATOS")] Atos = 7,
    [Description("CARTAS PAULINAS")] CartasPaulinas = 8,
    [Description("CARTAS GERAIS")] CartasGerais = 9,
    [Description("APOCALIPSE")] Apocalipse = 10
}
