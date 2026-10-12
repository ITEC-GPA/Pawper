using Pawper.Core;
namespace Pawper.Samples;
public static class InputSamples
{
    public static string TextFor(int id) => id switch
    {
        2 => "# Testo da un unico input\nUn paragrafo con **grassetto**, *corsivo* e ***entrambi***.\nQuesta riga resta nello stesso paragrafo.\n\nSimboli letterali: \\* e \\$.",
        4 => "# Elenchi da testo\n- Primo punto\n  - Sottopunto\n+ Secondo punto\n\n1. Prima fase\n2. Seconda fase\n\n1. Nuovo elenco numerato",
        8 => "# Formule da testo\nLa tensione vale $\\sigma=\\frac{N}{A}$ nel modello dimostrativo.\n\n$$x_1^2+\\sqrt{a}$$\n\n$$\n\\sum_{i=1}^{n}{x_i}\n$$",
        10 => "---\n# Pagina da testo strutturato\nTre trattini isolati creano un'interruzione di pagina nativa.",
        _ => ""
    };
}
