using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Pawper.Core;

public sealed partial class WordEngine
{
    internal static readonly XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    internal static XElement E(string n, params object?[] c) => new(w + n, c);
    internal static XAttribute A(string n, object v) => new(w + n, v);
    internal static XElement V(string n, object v) => E(n, A("val", v));
    static string N(double n) => n.ToString("0.###", CultureInfo.InvariantCulture);
    static int Tw(double mm) => checked((int)Math.Round(mm * 1440 / 25.4));
    WordprocessingDocument package = null!; MainDocumentPart main = null!;
    uint drawingId; int bookmarkId, noteId, commentId, numId; readonly HashSet<string> bookmarkNames = new();
    readonly Dictionary<string, int> lists = new(); PageLayout currentLayout = new();

    public static byte[] Build(Document spec)
    {
        using var stream = new MemoryStream();
        if (spec.SourceBase64 != null)
        {
            var bytes = Convert.FromBase64String(spec.SourceBase64);
            stream.Write(bytes);
            stream.Position = 0;
        }
        using (var doc = spec.SourceBase64 == null ? WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true) : WordprocessingDocument.Open(stream, true))
        {
            if (doc.DocumentType != WordprocessingDocumentType.Document)
                doc.ChangeDocumentType(WordprocessingDocumentType.Document);
            var engine = new WordEngine { package = doc, main = doc.MainDocumentPart ?? doc.AddMainDocumentPart() };
            engine.main.Document ??= new W.Document(new W.Body());
            engine.Render(spec);
        }
        return stream.ToArray();
    }
    void Render(Document spec)
    {
        var body = main.Document.Body ?? main.Document.AppendChild(new W.Body());
        drawingId = main.Document.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties>().Select(x => x.Id?.Value ?? 0).DefaultIfEmpty().Max();
        bookmarkId = main.Document.Descendants<W.BookmarkStart>().Select(x => int.TryParse(x.Id?.Value, out var id) ? id : 0).DefaultIfEmpty().Max();
        foreach (var b in main.Document.Descendants<W.BookmarkStart>())
            if (b.Name != null)
                bookmarkNames.Add(b.Name!);
        noteId = Math.Max(main.FootnotesPart?.Footnotes?.Elements<W.Footnote>().Select(x => (int)(x.Id?.Value ?? 0)).DefaultIfEmpty().Max() ?? 0, main.EndnotesPart?.Endnotes?.Elements<W.Endnote>().Select(x => (int)(x.Id?.Value ?? 0)).DefaultIfEmpty().Max() ?? 0);
        commentId = main.WordprocessingCommentsPart?.Comments?.Elements<W.Comment>().Select(x => int.TryParse(x.Id?.Value, out var id) ? id : 0).DefaultIfEmpty().Max() ?? 0;
        EnsureStyles(spec.Styles);
        var settings = main.DocumentSettingsPart ?? main.AddNewPart<DocumentSettingsPart>();
        settings.Settings ??= new W.Settings();
        settings.Settings.RemoveAllChildren<W.UpdateFieldsOnOpen>();
        settings.Settings.AddChild(new W.UpdateFieldsOnOpen { Val = true }, true);
        var compat = settings.Settings.GetFirstChild<W.Compatibility>();
        if (compat == null)
        {
            compat = new W.Compatibility();
            settings.Settings.AddChild(compat, true);
        }
        if (compat.GetFirstChild<W.DoNotExpandShiftReturn>() == null)
            compat.AddChild(new W.DoNotExpandShiftReturn(), true);
        var lastSection = body.Elements<W.SectionProperties>().LastOrDefault();
        lastSection?.Remove();
        currentLayout = spec.Layout ?? new();
        if (!spec.Blocks.IsDefault)
            foreach (var block in spec.Blocks)
                Append(body, RenderBlock(block));
        if (spec.Layout != null || lastSection == null || spec.Blocks.Any(x => x is Section))
            body.Append(new W.SectionProperties(LayoutXml(currentLayout).ToString()));
        else
            body.Append(lastSection);
        if (!spec.Targets.IsDefault)
            foreach (var target in spec.Targets)
                FillTarget(target);
        if (spec.Replacements != null)
            ReplaceText(spec.Replacements, spec.StrictPlaceholders);
        if (spec.Properties != null)
            SetMetadata(spec.Properties);
        main.Document.Save();
    }
    internal static void Append(OpenXmlCompositeElement parent, IEnumerable<XElement> items)
    {
        // Parsing through Body preserves the SDK's strongly typed elements for schema validation.
        var parsed = new W.Body(E("body", items).ToString());
        foreach (var child in parsed.ChildElements.ToArray())
        {
            child.Remove();
            parent.Append(child);
        }
    }
    IEnumerable<XElement> RenderBlock(ContentBlock block)
    {
        switch (block)
        {
            case Paragraph p:
                yield return ParagraphXml(p);
                break;
            case Table t:
                yield return TableXml(t);
                break;
            case Image i:
                yield return ImageXml(i);
                if (i.Caption != null)
                    yield return Caption("Figure", i.Caption);
                break;
            case Equation eq:
                var math = MathParser.Parse(eq.Latex);
                var pEq = E("p", E("pPr", V("jc", eq.Inline ? "left" : "center")), eq.Inline ? math : new XElement(MathParser.M + "oMathPara", math));
                if (eq.Numbered)
                {
                    pEq.Add(RunXml(new(" (")));
                    int? numberBookmark = eq.BookmarkName == null ? null : RegisterBookmark(eq.BookmarkName);
                    if (numberBookmark != null)
                        pEq.Add(E("bookmarkStart", A("id", numberBookmark), A("name", eq.BookmarkName!)));
                    pEq.Add(FieldXml("SEQ Equation \\* ARABIC", "1"));
                    if (numberBookmark != null)
                        pEq.Add(E("bookmarkEnd", A("id", numberBookmark)));
                    pEq.Add(RunXml(new(")")));
                }
                else if (eq.BookmarkName != null)
                    AddBookmark(pEq, eq.BookmarkName);
                yield return pEq;
                break;
            case PageBreak:
                yield return E("p", E("r", E("br", A("type", "page"))));
                break;
            case Field f:
                CheckStyle(f.StyleId);
                yield return E("p", E("pPr", V("pStyle", f.StyleId)), FieldXml(f.Instruction, f.Placeholder));
                break;
            case Bookmark b:
                var elements = b.Content.SelectMany(RenderBlock).ToList();
                if (elements.Count == 0)
                    elements.Add(E("p"));
                if (elements[0].Name != w + "p")
                    elements.Insert(0, E("p"));
                if (elements[^1].Name != w + "p")
                    elements.Add(E("p"));
                int bid = RegisterBookmark(b.Name);
                elements[0].Add(E("bookmarkStart", A("id", bid), A("name", b.Name)));
                elements[^1].Add(E("bookmarkEnd", A("id", bid)));
                // The start must precede the bookmarked contents (after pPr).
                var start = elements[0].Element(w + "bookmarkStart")!;
                start.Remove();
                var pr = elements[0].Element(w + "pPr");
                if (pr != null)
                    pr.AddAfterSelf(start);
                else
                    elements[0].AddFirst(start);
                foreach (var el in elements)
                    yield return el;
                break;
            case ContentControl c:
                yield return E("sdt", E("sdtPr", E("tag", A("val", c.Tag))), E("sdtContent", c.Content.SelectMany(RenderBlock)));
                break;
            case Section s:
                yield return E("p", E("pPr", LayoutXml(currentLayout)));
                currentLayout = s.Layout;
                foreach (var content in s.Content.SelectMany(RenderBlock))
                    yield return content;
                break;
            case Note n:
                yield return NoteXml(n);
                break;
            case Comment c:
                yield return CommentXml(c);
                break;
            case Import i:
                foreach (var imported in ImportXml(i))
                    yield return imported;
                break;
            default:
                throw new NotSupportedException($"Blocco non supportato: {block.GetType().Name}");
        }
    }
    static string Color(string value) => Regex.IsMatch(value, "^[0-9A-Fa-f]{6}$") ? value : throw new ArgumentException("Il colore deve essere RGB esadecimale a sei cifre.");
    static XElement RunProperties(RunFormat? f)
    {
        var x = E("rPr");
        if (f == null)
            return x;
        if (f.Font != null)
            x.Add(E("rFonts", A("ascii", f.Font), A("hAnsi", f.Font)));
        if (f.Bold)
            x.Add(E("b"));
        if (f.Italic)
            x.Add(E("i"));
        if (f.Color != null)
            x.Add(V("color", Color(f.Color)));
        if (f.Size != null)
        {
            if (f.Size is <= 0 or > 400)
                throw new ArgumentException("Dimensione font non valida.");
            x.Add(V("sz", N(f.Size.Value * 2)));
        }
        if (f.Underline)
            x.Add(V("u", "single"));
        if (f.Script != null)
        {
            if (f.Script is not ("superscript" or "subscript" or "baseline"))
                throw new ArgumentException("Script non valido.");
            x.Add(V("vertAlign", f.Script));
        }
        return x;
    }
    XElement RunXml(TextRun text)
    {
        if (text.Math != null)
            return MathParser.Parse(text.Math);
        if (text.Field != null)
            return FieldXml(text.Field, text.Text);
        var rp = RunProperties(text.Format);
        if (text.StyleId != null)
        {
            CheckStyle(text.StyleId);
            rp.AddFirst(V("rStyle", text.StyleId));
        }
        var run = E("r", rp);
        var lines = text.Text.Replace("\r\n", "\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                run.Add(E("br"));
            var tabs = lines[i].Split('\t');
            for (int j = 0; j < tabs.Length; j++)
            {
                if (j > 0)
                    run.Add(E("tab"));
                run.Add(E("t", new XAttribute(XNamespace.Xml + "space", "preserve"), tabs[j]));
            }
        }
        if (text.Link == null)
            return run;
        if (text.Link.StartsWith('#'))
            return E("hyperlink", A("anchor", text.Link[1..]), run);
        if (!Uri.TryCreate(text.Link, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http" or "mailto"))
            throw new ArgumentException("Collegamento non valido.");
        var rel = main.AddHyperlinkRelationship(uri, true);
        return E("hyperlink", new XAttribute(r + "id", rel.Id), run);
    }
    static XElement ParagraphProperties(ParagraphFormat? f, string? style = null, int? number = null, int level = 0)
    {
        var x = E("pPr");
        if (style != null)
            x.Add(V("pStyle", style));
        if (f?.KeepNext == true)
            x.Add(E("keepNext"));
        if (number != null)
            x.Add(E("numPr", V("ilvl", level), V("numId", number)));
        if (f == null)
            return x;
        if (f.Line <= 0 || f.Before < 0 || f.After < 0)
            throw new ArgumentException("Spaziatura non valida.");
        if (f.Alignment is not ("left" or "center" or "right" or "both"))
            throw new ArgumentException("Allineamento non valido.");
        x.Add(E("spacing", A("before", N(f.Before * 20)), A("after", N(f.After * 20)), A("line", N(f.Line * 240)), A("lineRule", "auto")));
        x.Add(E("ind", A("left", Tw(f.LeftMm)), A("right", Tw(f.RightMm)), A(f.FirstLineMm < 0 ? "hanging" : "firstLine", Math.Abs(Tw(f.FirstLineMm)))));
        x.Add(V("jc", f.Alignment));
        return x;
    }
    XElement ParagraphXml(Paragraph p)
    {
        CheckStyle(p.StyleId);
        if (p.ListLevel is < 0 or > 8)
            throw new ArgumentException("Livello elenco richiesto tra 0 e 8.");
        return E("p", ParagraphProperties(p.Format, p.StyleId, p.ListLevel == null ? null : Numbering(p.ListKey, p.Bullet), p.ListLevel ?? 0), p.Runs.Select(RunXml));
    }
    static XElement FieldXml(string instruction, string placeholder) => E("fldSimple", A("instr", " " + instruction + " "), A("dirty", "true"), E("r", E("t", placeholder)));
    XElement Caption(string category, string text) => E("p", E("pPr", V("pStyle", "Caption")), RunXml(new(category == "Figure" ? "Figura " : category + " ")), FieldXml("SEQ " + category + " \\* ARABIC", "1"), RunXml(new(" — " + text)));
    void CheckStyle(string id)
    {
        if (!main.StyleDefinitionsPart!.Styles!.Elements<W.Style>().Any(s => s.StyleId == id))
            throw new ArgumentException($"Stile '{id}' non trovato. Usa Inspector per gli identificatori disponibili.");
    }
    void EnsureStyles(ImmutableArray<Style> custom)
    {
        var part = main.StyleDefinitionsPart ?? main.AddNewPart<StyleDefinitionsPart>();
        part.Styles ??= new W.Styles();
        void Add(Style s, bool replace)
        {
            var old = part.Styles.Elements<W.Style>().FirstOrDefault(x => x.StyleId == s.Id);
            if (old != null && !replace)
                return;
            old?.Remove();
            if (s.Type is not ("paragraph" or "character" or "table"))
                throw new ArgumentException("Tipo stile non supportato.");
            var x = E("style", A("type", s.Type), A("styleId", s.Id), E("name", A("val", s.Name)));
            if (s.Id != "Normal" && s.Type == "paragraph")
                x.Add(V("basedOn", s.BasedOn));
            if (s.Type == "paragraph")
            {
                var pr = ParagraphProperties(s.Paragraph);
                if (s.Id.StartsWith("Heading") && int.TryParse(s.Id[7..], out int level) && level is >= 1 and <= 9)
                    pr.Add(V("outlineLvl", level - 1));
                x.Add(pr);
            }
            x.Add(RunProperties(s.Run));
            part.Styles.Append(new W.Style(x.ToString()));
        }
        Add(new("Normal", "Normal", Run: new("Aptos", 11, Color: "000000"), Paragraph: new()), false);
        Add(new("Title", "Title", Run: new(Size: 26, Bold: true, Color: "000000"), Paragraph: new(After: 14, KeepNext: true)), false);
        Add(new("Subtitle", "Subtitle", Run: new(Size: 14, Color: "000000"), Paragraph: new(After: 12, KeepNext: true)), false);
        Add(new("Caption", "Caption", Run: new(Size: 10, Color: "000000")), false);
        for (int i = 1; i <= 9; i++)
            Add(new("Heading" + i, "Heading " + i, Run: new(Size: Math.Max(11, 20 - i * 2), Bold: true, Color: "000000"), Paragraph: new(Before: 12, After: 6, KeepNext: true)), false);
        if (!custom.IsDefault)
            foreach (var s in custom)
                Add(s, true);
        part.Styles.Save();
    }
    int Numbering(string key, bool bullet)
    {
        key = (bullet ? "b:" : "n:") + key;
        if (lists.TryGetValue(key, out int id))
            return id;
        var part = main.NumberingDefinitionsPart ?? main.AddNewPart<NumberingDefinitionsPart>();
        part.Numbering ??= new W.Numbering();
        int abstractId = part.Numbering.Elements<W.AbstractNum>().Select(x => x.AbstractNumberId?.Value ?? 0).DefaultIfEmpty().Max() + 1;
        numId = part.Numbering.Elements<W.NumberingInstance>().Select(x => x.NumberID?.Value ?? 0).DefaultIfEmpty().Max() + 1;
        var abs = E("abstractNum", A("abstractNumId", abstractId), V("multiLevelType", "multilevel"));
        for (int level = 0; level < 9; level++)
            abs.Add(E("lvl", A("ilvl", level), V("start", 1), V("numFmt", bullet ? "bullet" : "decimal"), V("suff", "space"), V("lvlText", bullet ? "•" : string.Join(".", Enumerable.Range(1, level + 1).Select(n => "%" + n)) + "."), V("lvlJc", "left"), E("pPr", E("ind", A("left", 360 * (level + 1)), A("hanging", 240)))));
        var a = new W.AbstractNum(abs.ToString());
        var firstNum = part.Numbering.Elements<W.NumberingInstance>().FirstOrDefault();
        if (firstNum == null)
            part.Numbering.Append(a);
        else
            part.Numbering.InsertBefore(a, firstNum);
        part.Numbering.Append(new W.NumberingInstance(E("num", A("numId", numId), V("abstractNumId", abstractId)).ToString()));
        part.Numbering.Save();
        lists[key] = numId;
        return numId;
    }
    XElement LayoutXml(PageLayout page)
    {
        if (page.WidthMm <= 0 || page.HeightMm <= 0 || page.MarginMm < 0 || page.MarginMm * 2 >= Math.Min(page.WidthMm, page.HeightMm))
            throw new ArgumentException("Dimensioni pagina o margini non validi.");
        var section = E("sectPr");
        void H(string? text, string type, bool footer)
        {
            if (text == null)
                return;
            // Empty text is an explicit blank part, preventing unwanted inheritance.
            string id;
            var para = E("p", E("pPr", V("jc", footer ? "center" : "left")));
            var pieces = Regex.Split(text, "(\\{\\{PAGE\\}\\}|\\{\\{NUMPAGES\\}\\})");
            foreach (var piece in pieces)
                para.Add(piece is "{{PAGE}}" or "{{NUMPAGES}}" ? FieldXml(piece[2..^2], "1") : E("r", E("t", new XAttribute(XNamespace.Xml + "space", "preserve"), piece)));
            if (footer)
            {
                var part = main.AddNewPart<FooterPart>();
                part.Footer = new W.Footer(E("ftr", para).ToString());
                part.Footer.Save();
                id = main.GetIdOfPart(part);
            }
            else
            {
                var part = main.AddNewPart<HeaderPart>();
                part.Header = new W.Header(E("hdr", para).ToString());
                part.Header.Save();
                id = main.GetIdOfPart(part);
            }
            section.Add(E(footer ? "footerReference" : "headerReference", A("type", type), new XAttribute(r + "id", id)));
        }
        H(page.Header, "default", false);
        H(page.FirstHeader, "first", false);
        H(page.EvenHeader, "even", false);
        H(page.Footer, "default", true);
        H(page.FirstFooter, "first", true);
        H(page.EvenFooter, "even", true);
        if (page.EvenHeader != null || page.EvenFooter != null)
        {
            var settings = main.DocumentSettingsPart!.Settings!;
            if (settings.GetFirstChild<W.EvenAndOddHeaders>() == null)
                settings.AddChild(new W.EvenAndOddHeaders(), true);
        }
        double width = page.Landscape ? Math.Max(page.WidthMm, page.HeightMm) : Math.Min(page.WidthMm, page.HeightMm), height = page.Landscape ? Math.Min(page.WidthMm, page.HeightMm) : Math.Max(page.WidthMm, page.HeightMm);
        section.Add(E("type", A("val", "nextPage")), E("pgSz", A("w", Tw(width)), A("h", Tw(height)), A("orient", page.Landscape ? "landscape" : "portrait")), E("pgMar", A("top", Tw(page.MarginMm)), A("right", Tw(page.MarginMm)), A("bottom", Tw(page.MarginMm)), A("left", Tw(page.MarginMm)), A("header", 450), A("footer", 450), A("gutter", 0)));
        if (page.PageStart != null)
            section.Add(E("pgNumType", A("start", page.PageStart)));
        if (page.FirstHeader != null || page.FirstFooter != null)
            section.Add(E("titlePg"));
        return section;
    }
    int RegisterBookmark(string name)
    {
        if (!Regex.IsMatch(name, "^[A-Za-z][A-Za-z0-9_]{0,39}$") || !bookmarkNames.Add(name))
            throw new ArgumentException($"Segnalibro non valido o duplicato: {name}.");
        return ++bookmarkId;
    }
    void AddBookmark(XElement p, string name)
    {
        int id = RegisterBookmark(name);
        var start = E("bookmarkStart", A("id", id), A("name", name));
        var pr = p.Element(w + "pPr");
        if (pr == null)
            p.AddFirst(start);
        else
            pr.AddAfterSelf(start);
        p.Add(E("bookmarkEnd", A("id", id)));
    }
    public static ImmutableArray<string> Validate(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);
        return new OpenXmlValidator(FileFormatVersions.Office2019).Validate(doc).Take(100).Select(e => $"{e.Part?.Uri}: {e.Path?.XPath}: {e.Description}").ToImmutableArray();
    }
    public static Inspection Inspect(Document spec)
    {
        var bytes = Build(spec);
        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);
        var part = doc.MainDocumentPart!;
        var custom = doc.CustomFilePropertiesPart?.Properties?.Elements<DocumentFormat.OpenXml.CustomProperties.CustomDocumentProperty>().Where(x => x.Name != null).ToImmutableDictionary(x => x.Name!.Value!, x => x.InnerText);
        return new(part.StyleDefinitionsPart!.Styles!.Elements<W.Style>().Select(x => $"{x.StyleId} | {x.StyleName?.Val}").ToImmutableArray(), part.Document.Descendants<W.BookmarkStart>().Select(x => x.Name?.Value ?? "").ToImmutableArray(), part.Document.Descendants<W.Tag>().Select(x => x.Val?.Value ?? "").ToImmutableArray(), new(doc.PackageProperties.Title ?? "", doc.PackageProperties.Creator ?? "", doc.PackageProperties.Subject ?? "", doc.PackageProperties.Keywords ?? "", custom), Validate(bytes), string.Join("\n", part.Document.Body!.ChildElements.Select((b, i) => $"{i}: {b.LocalName} | {b.InnerText}")));
    }
}

