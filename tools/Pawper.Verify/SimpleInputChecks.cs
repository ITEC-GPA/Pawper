using System.Collections.Immutable;
using DocumentFormat.OpenXml.Packaging;
using Pawper.Core;
using Pawper.Samples;
using W = DocumentFormat.OpenXml.Wordprocessing;
using M = DocumentFormat.OpenXml.Math;
namespace Pawper.Verify;
internal static class SimpleInputChecks
{
    internal static void Run(Action<string, Action> check, Action<string, Action> reject)
    {
        void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
        void Xml(Document d, Action<WordprocessingDocument> test)
        {
            byte[] bytes = WordEngine.Build(d); Assert(WordEngine.Validate(bytes).Length == 0, "Open XML non valido");
            using var stream = new MemoryStream(bytes); using var word = WordprocessingDocument.Open(stream, false); test(word);
        }
        check("Structured text native headings and mixed runs", () =>
        {
            var blocks = StructuredText.Parse(InputSamples.TextFor(2));
            Assert(blocks.Length == 3 && ((Paragraph)blocks[0]).StyleId == "Heading1", "Struttura errata");
            var runs = ((Paragraph)blocks[1]).Runs;
            Assert(runs.Any(r => r.Format is { Bold: true, Italic: true }) && runs.Any(r => r.Text.Contains('\n')), "Formattazione o a capo persi");
            Assert(string.Concat(((Paragraph)blocks[2]).Runs.Select(r => r.Text)) == "Simboli letterali: * e $.", "Escape errati");
            Xml(new(blocks), w => Assert(w.MainDocumentPart!.Document.Descendants<W.Bold>().Any(), "Grassetto non nativo"));
        });
        check("Structured text nine heading levels and blank input", () =>
        {
            var blocks = StructuredText.Parse(string.Join("\n", Enumerable.Range(1, 9).Select(n => new string('#', n) + " Titolo")));
            Assert(blocks.Cast<Paragraph>().Select(p => p.StyleId).SequenceEqual(Enumerable.Range(1, 9).Select(n => "Heading" + n)), "Livelli errati");
            Assert(StructuredText.Parse("\n \r\n").Length == 0, "Blocchi vuoti inattesi");
        });
        check("Structured text list restart nesting and isolated scopes", () =>
        {
            var blocks = StructuredText.Parse(InputSamples.TextFor(4), "a").OfType<Paragraph>().Where(p => p.ListLevel != null).ToArray();
            Assert(blocks.Length == 6 && blocks[1].ListLevel == 1 && blocks[0].Bullet, "Elenco errato");
            Assert(blocks[3].ListKey != blocks[5].ListKey, "Numerazione non riavviata");
            Assert(((Paragraph)StructuredText.Parse("1. A", "a")[0]).ListKey != ((Paragraph)StructuredText.Parse("1. A", "b")[0]).ListKey, "Elenchi contaminati");
            Xml(new(StructuredText.Parse(InputSamples.TextFor(4))), w => Assert(w.MainDocumentPart!.NumberingDefinitionsPart != null, "Numerazione Word assente"));
        });
        check("Structured text inline and display editable OMML", () => Xml(new(StructuredText.Parse(InputSamples.TextFor(8))), w => Assert(w.MainDocumentPart!.Document.Descendants<M.OfficeMath>().Count() == 3, "Attese tre formule native")));
        check("Structured text page break", () => Xml(new(StructuredText.Parse(InputSamples.TextFor(10))), w => Assert(w.MainDocumentPart!.Document.Descendants<W.Break>().Any(b => b.Type?.Value == W.BreakValues.Page), "Interruzione assente")));
        foreach (var pair in new Dictionary<string, string> { ["unclosed emphasis"] = "**testo", ["unclosed math"] = "$x", ["unclosed display"] = "$$\nx", ["unknown math"] = @"$\unknown{x}$", ["invalid indentation"] = " - X", ["invalid numbering start"] = "2. X", ["unsupported table"] = "| A | B |", ["unsupported image"] = "![x](file.png)" })
            reject("Structured text rejects " + pair.Key, () => StructuredText.Parse(pair.Value));
        check("Fill fields normalize names preserve branch and existing keys", () =>
        {
            var source = SimpleInputs.Fill(new([Paragraph.Text("{{A}} {{B}}")]), ["A"], ["uno"]);
            var result = SimpleInputs.Fill(source, [" {{B}} "], ["due"]);
            Assert(source.Replacements!.Count == 1 && result.Replacements!.Count == 2, "Mutazione del documento sorgente");
            Xml(result, w => Assert(w.MainDocumentPart!.Document.InnerText.Contains("uno due"), "Sostituzione errata"));
        });
        check("Fill fields lenient leaves missing placeholders and accepts empty values", () =>
            Xml(SimpleInputs.Fill(new([Paragraph.Text("{{A}} / {{B}}")]), ["A"], [""], false), w => Assert(w.MainDocumentPart!.Document.InnerText.Contains(" / {{B}}"), "Token non conservato")));
        reject("Fill fields strict missing value", () => WordEngine.Build(SimpleInputs.Fill(new([Paragraph.Text("{{A}} {{B}}")]), ["A"], ["uno"])));
        reject("Fill fields unequal lists", () => SimpleInputs.Values(["A", "B"], ["uno"]));
        reject("Fill fields duplicate normalized names", () => SimpleInputs.Values(["A", "{{A}}"], ["uno", "due"]));
        reject("Fill fields invalid name", () => SimpleInputs.Values(["{{A}"], ["uno"]));
        check("Quick document template and styles preserve source", () =>
        {
            var style = new Style("CustomBody", "Corpo personalizzato");
            var original = new Document([Paragraph.Text("Template")], Styles: [style], Layout: SimpleInputs.Page("Letter"));
            var bytes = WordEngine.Build(original);
            var source = new Document([], Convert.ToBase64String(bytes));
            var composed = SimpleInputs.CreateDocument("Titolo aggiunto", [Paragraph.Text("Corpo", "CustomBody")], source);
            Assert(source.Blocks.Length == 0 && composed.Blocks.Length == 2 && composed.Layout == null, "Sorgente o layout template modificato");
            Xml(composed, w => Assert(w.MainDocumentPart!.Document.InnerText.Contains("TemplateTitolo aggiuntoCorpo"), "Ordine o template perso"));
            var second = SimpleInputs.CreateDocument("", [], source, SimpleInputs.Page("A3", true), [new Style("Extra", "Extra")]);
            Xml(second, w => Assert(w.MainDocumentPart!.StyleDefinitionsPart!.Styles!.Elements<W.Style>().Any(s => s.StyleId == "Extra"), "Stile nuovo perso"));
        });
        check("Page presets native size and orientation", () =>
        {
            foreach (var format in new[] { "A4", "A3", "Letter", "Legal" }) foreach (bool landscape in new[] { false, true })
            {
                var layout = SimpleInputs.Page(format, landscape, 18, "Header", "{{PAGE}} / {{NUMPAGES}}");
                Xml(new([Paragraph.Text(format)], Layout: layout), w =>
                {
                    var size = w.MainDocumentPart!.Document.Descendants<W.PageSize>().Last();
                    Assert((size.Width!.Value > size.Height!.Value) == landscape, "Orientamento errato");
                    Assert(w.MainDocumentPart.FooterParts.Any(), "Footer perso");
                });
            }
        });
        reject("Page preset invalid name", () => SimpleInputs.Page("A5"));
        reject("Page preset invalid margins", () => SimpleInputs.Page("A4", marginMm: 105));
        check("Paragraph formats all alignments native and keep next", () =>
        {
            foreach (string align in new[] { "left", "center", "right", "both", "justified" })
                Xml(new([new Paragraph([new("Prova")], Format: SimpleInputs.ParagraphSettings(align, 3, 9, 1.5, true))]), w => Assert(w.MainDocumentPart!.Document.Descendants<W.KeepNext>().Any(), "KeepNext mancante"));
        });
        reject("Paragraph format invalid spacing", () => SimpleInputs.ParagraphSettings(line: double.NaN));
        reject("Paragraph format invalid alignment", () => SimpleInputs.ParagraphSettings("typo"));
        check("Table format immutable merged cells and header contrast", () =>
        {
            var table = new Table([[new Cell([Paragraph.Text("Titolo")], ColumnSpan: 2)], [Cell.Text("A"), Cell.Text("B")]]);
            foreach (var color in new[] { "DCEAF2", "24445C" })
            {
                var formatted = SimpleInputs.FormatTable(table, color, "F3F6F8", "D9D9D9", 2.5);
                Assert(table.PaddingMm == 2 && formatted.Rows == table.Rows, "Celle modificate");
                Xml(new([formatted]), w =>
                {
                    var row = w.MainDocumentPart!.Document.Descendants<W.TableRow>().First();
                    Assert(row.Descendants<W.Color>().First().Val == (color == "DCEAF2" ? "000000" : "FFFFFF"), "Contrasto errato");
                    Assert(row.Descendants<W.GridSpan>().First().Val?.Value == 2, "Cella unita persa");
                });
            }
        });
        reject("Table format rejects non table", () => SimpleInputs.FormatTable(Paragraph.Text("X"), "FFFFFF", "FFFFFF", "FFFFFF"));
        reject("Table format rejects invalid colors", () => SimpleInputs.FormatTable(TableSamples.RowTable(), "white", "FFFFFF", "FFFFFF"));
    }
}
