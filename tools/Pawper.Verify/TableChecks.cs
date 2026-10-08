using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using Pawper.Core;
using Pawper.Samples;
using W = DocumentFormat.OpenXml.Wordprocessing;
namespace Pawper.Verify;

internal static class TableChecks
{
    internal static void Run(Action<string, Action> check, Action<string, Action> reject)
    {
        static void Assert(bool ok, string message) { if (!ok) throw new Exception(message); }
        static string CellText(Table t, int row, int col) => string.Concat(t.Rows[row][col].Content.OfType<Paragraph>().SelectMany(p => p.Runs).Select(r => r.Text));
        check("Whole table rows, nulls, ragged padding and alignment", () =>
        {
            var t = TableSamples.RowTable();
            Assert(t.Header && t.Rows.Length == 4 && t.Rows.All(r => r.Length == 3), "Griglia errata");
            Assert(CellText(t, 1, 1) == "6,50" && CellText(t, 2, 1) == "12,35", "Arrotondamento errato");
            Assert(CellText(t, 3, 1) == "" && CellText(t, 3, 2) == "" && t.Rows[1][1].Alignment == "right", "Valori mancanti o allineamento errati");
        });
        check("Whole table columns, per-column units and culture", () =>
        {
            var t = TableSamples.ColumnTable();
            Assert(CellText(t, 1, 1) == "1,234.57 kN" && CellText(t, 1, 2) == "4.123 m", "Unità o cultura errate");
            Assert(CellText(t, 3, 0) == "Mancante" && CellText(t, 3, 1) == "", "Trasposizione errata");
        });
        check("Whole table no header, global units, precision and strict", () =>
        {
            var t = Tabular.FromData([[1.2, 3.45], [2.3, 4.5]], strict: true, decimals: [1], units: ["mm"]);
            Assert(!t.Header && t.Rows.Length == 2 && CellText(t, 0, 0) == "1,2 mm", "Modalità senza intestazione errata");
            var original = Tabular.FromData([[1.23456789]]);
            Assert(CellText(original, 0, 0) == "1,23456789", "Precisione originale persa");
        });
        check("Whole table text is never parsed as number", () =>
        {
            var t = Tabular.FromData([["001", "12,50", 12.5m]], decimals: [0], units: ["kN"]);
            Assert(CellText(t, 0, 0) == "001" && CellText(t, 0, 1) == "12,50" && CellText(t, 0, 2).EndsWith(" kN"), "Conversione implicita dei testi");
        });
        check("Whole table grid uses all rows and header width", () =>
        {
            var t = Tabular.FromData([["A"], ["B", 2, 3]]);
            Assert(t.Rows.All(r => r.Length == 3) && CellText(t, 1, 2) == "3", "Prima riga corta tronca la griglia");
            var h = Tabular.FromData([["A"]], ["A", "B"]);
            Assert(h.Rows[1].Length == 2 && CellText(h, 1, 1) == "", "Intestazione più larga non gestita");
        });
        check("Whole table header only and empty branches", () =>
        {
            Assert(Tabular.FromData([], ["A", "B"]).Rows.Length == 1, "Intestazione sola rifiutata");
            var t = Tabular.FromData([[], [1, 2]], ["A", "B"]);
            Assert(t.Rows.Length == 3 && CellText(t, 1, 1) == "", "Ramo vuoto perso");
            var c = Tabular.FromData([[], [1, 2]], ["A", "B"], columns: true);
            Assert(CellText(c, 2, 0) == "" && CellText(c, 2, 1) == "2", "Colonna vuota persa");
        });
        check("CSV and TSV quoted fields, multiline, leading zeros", () =>
        {
            var t = TableSamples.TextWithHeader();
            Assert(CellText(t, 1, 0) == "001" && CellText(t, 1, 1).Replace("\r", "") == "Prima riga\nseconda riga", "Campo multilinea o zeri persi");
            Assert(CellText(t, 2, 1) == "Testo con \"virgolette\"" && CellText(t, 2, 2) == "", "Quote o vuoti errati");
            var c = TableSamples.TextWithoutHeader();
            Assert(!c.Header && CellText(c, 0, 1) == "Testo; con separatore", "Header False o quote errati");
        });
        check("Text table separator variants and BOM", () =>
        {
            foreach (var (sep, name) in new[] { ("\t", "tab"), (",", "comma"), (";", "semicolon"), ("|", "pipe"), (":", ":") })
            {
                string text = "\uFEFFA" + sep + "B\r\n1" + sep + "2\r\n";
                var t = Tabular.FromText(text, name);
                Assert(t.Separator == sep && CellText(t.Table, 1, 1) == "2", "Separatore errato " + name);
                if (sep != ":") Assert(Tabular.FromText(text).Separator == sep, "Auto errato " + name);
            }
        });
        check("Text table auto ignores quoted separators", () =>
        {
            var t = Tabular.FromText("\"Header, with, commas\";Value\nA;1,50");
            Assert(t.Separator == ";" && CellText(t.Table, 1, 1) == "1,50", "Virgole decimali interpretate come colonne");
        });
        check("Text table single column, blank lines and quoted empty record", () =>
        {
            var t = Tabular.FromText("\nHeader\n\n001\n\"\"\n").Table;
            Assert(t.Rows.Length == 3 && CellText(t, 1, 0) == "001" && CellText(t, 2, 0) == "", "Righe vuote errate");
        });
        check("Whole tables native editable XML and repeatable headers", () =>
        {
            Table[] tables = [TableSamples.RowTable(), TableSamples.ColumnTable(), TableSamples.TextWithHeader(), TableSamples.TextWithoutHeader()];
            var bytes = WordEngine.Build(Document.Empty.Append(tables));
            Assert(WordEngine.Validate(bytes).IsEmpty, "Schema Open XML non valido");
            using var memory = new MemoryStream(bytes); using var doc = WordprocessingDocument.Open(memory, false);
            Assert(doc.MainDocumentPart!.Document.Descendants<W.Table>().Count() == 4, "Tabelle native mancanti");
            Assert(doc.MainDocumentPart.Document.Descendants<W.TableHeader>().Count() == 3, "Intestazioni ripetute errate");
        });
        check("Whole table input is snapshotted and culture is independent", () =>
        {
            object?[][] data = [[1234.5]]; var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                var t = Tabular.FromData(data, culture: "en-US", decimals: [2]); data[0][0] = 0;
                Assert(CellText(t, 0, 0) == "1,234.50", "Stato condiviso o dipendenza dalla cultura host");
            }
            finally { CultureInfo.CurrentCulture = previous; }
        });
        reject("Whole table ragged strict rejects", () => Tabular.FromData([[1, 2], [3]], strict: true));
        reject("Whole table short row against header strict rejects", () => Tabular.FromData([[1]], ["A", "B"], strict: true));
        reject("Whole table column header mismatch rejects", () => Tabular.FromData([[1, 2]], ["A", "B"], columns: true));
        reject("Whole table extra data columns reject", () => Tabular.FromData([[1, 2, 3]], ["A", "B"]));
        reject("Whole table decimals mismatch rejects", () => Tabular.FromData([[1, 2, 3]], decimals: [1, 2]));
        reject("Whole table unit mismatch rejects", () => Tabular.FromData([[1, 2, 3]], units: ["m", "s"]));
        reject("Whole table bad precision rejects", () => Tabular.FromData([[1]], decimals: [16]));
        reject("Whole table NaN rejects", () => Tabular.FromData([[double.NaN]]));
        reject("Whole table invalid widths reject", () => Tabular.FromData([[1]], widthsMm: [double.PositiveInfinity]));
        reject("Whole table unsupported objects reject", () => Tabular.FromData([[new object()]]));
        reject("Whole table empty rejects", () => Tabular.FromData([]));
        reject("Whole table invalid culture rejects", () => Tabular.FromData([[1]], culture: "!invalid!"));
        reject("Text table empty rejects", () => Tabular.FromText("  \n"));
        reject("Text table unclosed quotes reject", () => Tabular.FromText("A;B\n1;\"unclosed", ";"));
        reject("Text table wider than header rejects", () => Tabular.FromText("A;B\n1;2;3"));
        reject("Text table ambiguous separator rejects", () => Tabular.FromText("A,B;C\n1,2;3"));
        reject("Text table invalid delimiter rejects", () => Tabular.FromText("A;B", "wrong"));
    }
}
