using Pawper.Core;

namespace Pawper.Samples;

internal static class TableSamples
{
    internal static object?[][] Rows => [["Luce", 6.5, "m"], ["Carico", 12.3456, "kN/m"], ["Dato mancante", null]];
    internal static string[] Headers => ["Elemento", "Valore", "Unità"];
    internal static double[] Widths => [70, 45, 40];
    internal static Table RowTable() => Tabular.FromData(Rows, Headers, decimals: [2], widthsMm: Widths);
    internal static object?[][] Columns => [["Trave A", "Trave B", "Mancante"], [1234.567, 28.9], [4.1234, 3.8]];
    internal static string[] ColumnHeaders => ["Elemento", "Forza", "Lunghezza"];
    internal static Table ColumnTable() => Tabular.FromData(Columns, ColumnHeaders, columns: true, decimals: [-1, 2, 3], units: ["", "kN", "m"], culture: "en-US", widthsMm: Widths);
    internal const string Tsv = "Codice\tDescrizione\tValore\n001\t\"Prima riga\nseconda riga\"\t12,50\n002\t\"Testo con \"\"virgolette\"\"\"\t\n003\tDato mancante";
    internal const string Csv = "A;\"Testo; con separatore\";12,50\nB;\"Virgolette \"\"OK\"\"\";\nC;Fine;0";
    internal static Table TextWithHeader() => Tabular.FromText(Tsv, widthsMm: [25, 90, 40]).Table;
    internal static Table TextWithoutHeader() => Tabular.FromText(Csv, ";", false, [25, 90, 40]).Table;
}
