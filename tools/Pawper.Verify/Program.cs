using System.Collections.Immutable;
using System.IO.Compression;
using System.Xml.Linq;
using Pawper.Core;
using Pawper.Verify;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

string root = Path.GetFullPath(args.Length > 0 ? args[0] : "examples");
Directory.CreateDirectory(root);
var examples = Examples.Create(root);
int passed = 0;
var failures = new List<string>();
void Check(string name, Action action) { try { action(); Console.WriteLine("PASS " + name); passed++; } catch (Exception ex) { failures.Add(name + ": " + ex.Message); Console.WriteLine("FAIL " + name + ": " + ex.Message); } }
void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
void Reject(string name, Action action) => Check(name, () => { bool rejected = false; try { action(); } catch { rejected = true; } Assert(rejected, "Errore atteso non sollevato"); });
foreach (var e in examples)
{
    string folder = Path.Combine(root, $"{e.Id:00}-{e.Slug}");
    Directory.CreateDirectory(folder);
    File.WriteAllText(Path.Combine(folder, "document.json"), Json.Write(e.Document));
    Check($"Example {e.Id:00} {e.Slug}", () => { var result = Output.Save(e.Document, Path.Combine(folder, "expected.docx"), true); Assert(result.Success, result.Message); var roundtrip = Json.Read<Document>(Json.Write(e.Document)); Assert(WordEngine.Validate(WordEngine.Build(roundtrip)).Length == 0, "Serializzazione non valida"); });
}
Check("Exactly 20 examples", () => Assert(examples.Count == 20, "Numero esempi errato"));
Check("Isolated document branches", () => { var a = Document.Empty; var b = a.Append([Examples.P("B")]); var c = a.Append([Examples.P("C")]); Assert(a.Blocks.Length == 0 && b.Blocks.Length == 1 && c.Blocks.Length == 1, "Rami modificati"); Assert(!WordEngine.Inspect(b).Structure.Contains("C"), "Contaminazione rami"); });
Check("Idempotent recompute", () => { var d = examples[3].Document; var a = WordEngine.Inspect(d); var b = WordEngine.Inspect(d); Assert(a.Structure == b.Structure, "Ricalcolo diverso"); });
Check("Rising edge and reload disarm", () => { var t = new RisingEdge(); Assert(!t.Consume(false) && t.Consume(true) && !t.Consume(true) && !t.Consume(false) && t.Consume(true), "Trigger errato"); t.Disarm(); Assert(!t.Consume(true), "Riavvio armato"); Assert(!t.Consume(false) && t.Consume(true), "Riarmo errato"); });
Check("Split placeholder and format preservation", () => { using var m = new MemoryStream(WordEngine.Build(examples[13].Document)); using var d = WordprocessingDocument.Open(m, false); Assert(d.MainDocumentPart!.Document.InnerText.Contains("Edificio dimostrativo"), "Testo non sostituito"); Assert(!d.MainDocumentPart.Document.InnerText.Contains("{{"), "Placeholder residuo"); Assert(d.MainDocumentPart.Document.Descendants<W.Bold>().Any(), "Grassetto perso"); Assert(d.MainDocumentPart.HeaderParts.First().Header!.InnerText.Contains("Edificio dimostrativo"), "Header non sostituito"); Assert(d.MainDocumentPart.StyleDefinitionsPart!.Styles!.Elements<W.Style>().Any(s => s.StyleId == "ProjectBody"), "Stile perso"); });
Check("Repeated split tokens in one paragraph", () => { var d = new Document([new Paragraph([new("{{A"), new("}} / {{A}} {{B}}")])], Replacements: new Dictionary<string, string> { { "A", "uno" }, { "B", "due" } }.ToImmutableDictionary()); Assert(WordEngine.Inspect(d).Structure.Contains("uno / uno due"), "Sostituzione multipla errata"); });
Check("Editable OMML and embedded media", () => { using var m = new MemoryStream(WordEngine.Build(examples[19].Document)); using var d = WordprocessingDocument.Open(m, false); Assert(d.MainDocumentPart!.Document.Descendants<DocumentFormat.OpenXml.Math.OfficeMath>().Any(), "OMML assente"); Assert(d.MainDocumentPart.ImageParts.Any(), "Immagine non incorporata"); });
Check("Native annotations", () => { using var m = new MemoryStream(WordEngine.Build(examples[16].Document)); using var d = WordprocessingDocument.Open(m, false); Assert(d.MainDocumentPart!.FootnotesPart != null && d.MainDocumentPart.EndnotesPart != null && d.MainDocumentPart.WordprocessingCommentsPart != null, "Parti note mancanti"); });
Reject("Unsupported LaTeX rejects", () => MathParser.Parse(@"\unknown{x}"));
Reject("Malformed LaTeX rejects", () => MathParser.Parse(@"\frac{a}{"));
Reject("Missing style rejects", () => WordEngine.Build(new([Examples.P("X", "MissingStyle")])));
Reject("Empty table rejects", () => WordEngine.Build(new([new Table([])])));
Reject("Missing placeholder rejects", () => WordEngine.Build(new([Examples.P("{{Missing}}")], Replacements: ImmutableDictionary<string, string>.Empty)));
Reject("Unused placeholder key rejects", () => WordEngine.Build(new([Examples.P("X")], Replacements: new Dictionary<string, string> { { "Y", "Z" } }.ToImmutableDictionary())));
Reject("Duplicate bookmarks reject", () => WordEngine.Build(new([new Bookmark("Same", [Examples.P("A")]), new Bookmark("Same", [Examples.P("B")])])));
Reject("Duplicate batch paths reject before writing", () => Output.Batch([Document.Empty, Document.Empty], [Path.Combine(root, "collision.docx"), Path.Combine(root, "COLLISION.docx")], true));
Check("Batch output per document", () => { var d = examples[18].Document; var folder = Path.Combine(root, "19-batch"); var results = Output.Batch(Enumerable.Range(1, 3).Select(i => d.Append([Examples.P("Documento " + i)])).ToArray(), Enumerable.Range(1, 3).Select(i => Path.Combine(folder, $"batch-{i}.docx")).ToArray(), true); Assert(results.All(x => x.Success), string.Join(";", results.Select(x => x.Message))); });
Check("Overwrite protection", () => { var path = Path.Combine(root, "01-primo-documento", "expected.docx"); byte[] before = File.ReadAllBytes(path); bool failed = false; try { Output.Save(Document.Empty, path); } catch (IOException) { failed = true; } Assert(failed && before.SequenceEqual(File.ReadAllBytes(path)), "File sovrascritto"); });
Check("Locked file preserves original", () => { var path = Path.Combine(root, "01-primo-documento", "expected.docx"); byte[] before = File.ReadAllBytes(path); bool failed = false; using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) { try { Output.Save(Document.Empty, path, true); } catch (IOException) { failed = true; } } Assert(failed && before.SequenceEqual(File.ReadAllBytes(path)), "File bloccato modificato"); });
TableChecks.Run(Check, Reject);
SimpleInputChecks.Run(Check, Reject);
File.WriteAllText(Path.Combine(root, "catalog.json"), Json.Write(examples.Select(e => new { e.Id, e.Slug, e.Title, e.Purpose, e.Features }).ToArray()));
var report = new { Passed = passed, Failed = failures.Count, Failures = failures, Examples = examples.Count };
File.WriteAllText(Path.Combine(root, "verification.json"), Json.Write(report));
Console.WriteLine(Json.Write(report));
return failures.Count == 0 ? 0 : 1;
