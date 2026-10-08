using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Pawper.Core;
public sealed partial class WordEngine
{
    IEnumerable<OpenXmlPartRootElement> Roots()
    {
        yield return main.Document;
        foreach (var p in main.HeaderParts)
            if (p.Header != null)
                yield return p.Header;
        foreach (var p in main.FooterParts)
            if (p.Footer != null)
                yield return p.Footer;
        if (main.FootnotesPart?.Footnotes != null)
            yield return main.FootnotesPart.Footnotes;
        if (main.EndnotesPart?.Endnotes != null)
            yield return main.EndnotesPart.Endnotes;
    }
    void ReplaceText(IReadOnlyDictionary<string, string> values, bool strict)
    {
        var seen = new HashSet<string>();
        foreach (var root in Roots())
        {
            foreach (var para in root.Descendants<W.Paragraph>())
            {
                // A match cannot cross a hyperlink, field, tab, break, or nested paragraph boundary.
                var groups = new List<List<W.Text>>();
                var group = new List<W.Text>();
                foreach (var child in para.ChildElements)
                {
                    if (child is W.Run run && run.ChildElements.All(x => x is W.RunProperties or W.Text))
                        group.AddRange(run.Elements<W.Text>());
                    else
                    {
                        if (group.Count > 0)
                            groups.Add(group);
                        group = [];
                    }
                }
                if (group.Count > 0)
                    groups.Add(group);
                foreach (var texts in groups)
                {
                    string source = string.Concat(texts.Select(x => x.Text));
                    var offsets = new List<int>();
                    int offset = 0;
                    foreach (var t in texts)
                    {
                        offsets.Add(offset);
                        offset += t.Text.Length;
                    }
                    foreach (Match match in Regex.Matches(source, @"\{\{([^{}]+)\}\}").Cast<Match>().Reverse())
                    {
                        string key = match.Groups[1].Value;
                        if (!values.TryGetValue(key, out var replacement))
                        {
                            if (strict)
                                throw new ArgumentException($"Valore mancante per {{{{{key}}}}}.");
                            continue;
                        }
                        seen.Add(key);
                        int a = offsets.FindLastIndex(x => x <= match.Index), b = offsets.FindLastIndex(x => x < match.Index + match.Length);
                        string prefix = texts[a].Text[..(match.Index - offsets[a])], suffix = texts[b].Text[(match.Index + match.Length - offsets[b])..];
                        texts[a].Text = prefix + replacement + (a == b ? suffix : "");
                        texts[a].Space = SpaceProcessingModeValues.Preserve;
                        for (int j = a + 1; j <= b; j++)
                        {
                            texts[j].Text = j == b ? suffix : "";
                            texts[j].Space = SpaceProcessingModeValues.Preserve;
                        }
                    }
                }
            }
            root.Save();
        }
        if (strict)
            foreach (var key in values.Keys)
                if (!seen.Contains(key))
                    throw new ArgumentException($"Placeholder non trovato: {{{{{key}}}}}.");
    }
    void FillTarget(TemplateTarget target)
    {
        if (target.Bookmark)
        {
            var start = main.Document.Descendants<W.BookmarkStart>().SingleOrDefault(b => b.Name == target.Name) ?? throw new ArgumentException("Segnalibro non trovato: " + target.Name);
            var end = main.Document.Descendants<W.BookmarkEnd>().SingleOrDefault(b => b.Id == start.Id) ?? throw new ArgumentException("Segnalibro incompleto.");
            if (start.Parent == end.Parent && start.Parent is W.Paragraph paragraph)
            {
                var nodes = target.Content.SelectMany(RenderBlock).ToList();
                if (nodes.Count != 1 || nodes[0].Name != w + "p")
                    throw new ArgumentException("Un segnalibro inline accetta un solo paragrafo; usare un content control a blocchi per tabelle e immagini.");
                var cursor = start.NextSibling();
                while (cursor != null && cursor != end)
                {
                    var next = cursor.NextSibling();
                    cursor.Remove();
                    cursor = next;
                }
                var replacement = new W.Paragraph(nodes[0].ToString());
                foreach (var child in replacement.ChildElements.Where(x => x is not W.ParagraphProperties).ToArray())
                {
                    child.Remove();
                    paragraph.InsertBefore(child, end);
                }
            }
            else
                throw new ArgumentException("La sostituzione di segnalibri richiede in questa versione un intervallo nello stesso paragrafo. Per intervalli a blocchi usare content control.");
        }
        else
        {
            var targets = main.Document.Descendants<W.SdtElement>().Where(s => s.SdtProperties?.GetFirstChild<W.Tag>()?.Val == target.Name).ToArray();
            if (targets.Length == 0)
                throw new ArgumentException("Content control non trovato: " + target.Name);
            foreach (var targetElement in targets)
            {
                if (targetElement is W.SdtBlock block)
                {
                    block.SdtContentBlock ??= new W.SdtContentBlock();
                    block.SdtContentBlock.RemoveAllChildren();
                    Append(block.SdtContentBlock, target.Content.SelectMany(RenderBlock));
                }
                else if (targetElement is W.SdtRun run)
                {
                    var content = target.Content.SelectMany(RenderBlock).ToList();
                    if (content.Count != 1 || content[0].Name != w + "p")
                        throw new ArgumentException("Un content control inline accetta un solo paragrafo.");
                    run.SdtContentRun ??= new W.SdtContentRun();
                    run.SdtContentRun.RemoveAllChildren();
                    var para = new W.Paragraph(content[0].ToString());
                    foreach (var child in para.ChildElements.Where(x => x is not W.ParagraphProperties).ToArray())
                    {
                        child.Remove();
                        run.SdtContentRun.Append(child);
                    }
                }
                else
                    throw new ArgumentException("Content control di riga/cella non supportato: usare un controllo a blocchi.");
            }
        }
    }
    IEnumerable<XElement> ImportXml(Import import)
    {
        using var stream = new MemoryStream(Convert.FromBase64String(import.SourceBase64));
        using var source = WordprocessingDocument.Open(stream, false);
        var src = source.MainDocumentPart!;
        var candidates = src.Document.Body!.ChildElements.Where(x => x is not W.SectionProperties).ToArray();
        if (import.BodyIndices.IsDefaultOrEmpty)
            throw new ArgumentException("Selezionare almeno un indice da importare.");
        if (import.BodyIndices.Any(i => i < 0 || i >= candidates.Length))
            throw new ArgumentException("Indice di importazione fuori intervallo.");
        var blocks = import.BodyIndices.Select(i => XElement.Parse(candidates[i].OuterXml)).ToArray();
        if (blocks.SelectMany(b => b.DescendantsAndSelf()).Any(e => e.Name == w + "sectPr" || e.Name == w + "footnoteReference" || e.Name == w + "endnoteReference" || e.Name == w + "commentReference" || e.Name == w + "altChunk"))
            throw new NotSupportedException("Importazione selettiva: paragrafi, tabelle, immagini, link, elenchi ed equazioni supportati; sezioni, note, commenti e altChunk richiedono una conversione preventiva.");
        string prefix = "I" + (main.StyleDefinitionsPart!.Styles!.ChildElements.Count + 1) + "_";
        var styles = src.StyleDefinitionsPart?.Styles?.Elements<W.Style>().Select(s => XElement.Parse(s.OuterXml)).ToArray() ?? [];
        var styleIds = styles.ToDictionary(x => (string)x.Attribute(w + "styleId")!, x => prefix + (string)x.Attribute(w + "styleId")!);
        foreach (var style in styles)
        {
            style.SetAttributeValue(w + "styleId", styleIds[(string)style.Attribute(w + "styleId")!]);
            style.Attribute(w + "default")?.Remove();
        }
        var numMap = new Dictionary<string, string>();
        var numberPart = main.NumberingDefinitionsPart ?? main.AddNewPart<NumberingDefinitionsPart>();
        numberPart.Numbering ??= new W.Numbering();
        var sourceNumbering = src.NumberingDefinitionsPart?.Numbering;
        if (sourceNumbering?.Elements<W.NumberingPictureBullet>().Any() == true)
            throw new NotSupportedException("Importazione di elenchi con immagini puntate non supportata.");
        if (sourceNumbering != null)
        {
            int absId = numberPart.Numbering.Elements<W.AbstractNum>().Select(a => a.AbstractNumberId?.Value ?? 0).DefaultIfEmpty().Max(), nextNum = numberPart.Numbering.Elements<W.NumberingInstance>().Select(a => a.NumberID?.Value ?? 0).DefaultIfEmpty().Max();
            var absMap = new Dictionary<int, int>();
            foreach (var abs in sourceNumbering.Elements<W.AbstractNum>())
            {
                var copy = (W.AbstractNum)abs.CloneNode(true);
                absMap[abs.AbstractNumberId!.Value] = ++absId;
                copy.AbstractNumberId = absId;
                var xml = XElement.Parse(copy.OuterXml);
                RemapStyles(xml, styleIds);
                var first = numberPart.Numbering.Elements<W.NumberingInstance>().FirstOrDefault();
                if (first == null)
                    numberPart.Numbering.Append(new W.AbstractNum(xml.ToString()));
                else
                    numberPart.Numbering.InsertBefore(new W.AbstractNum(xml.ToString()), first);
            }
            foreach (var num in sourceNumbering.Elements<W.NumberingInstance>())
            {
                var copy = (W.NumberingInstance)num.CloneNode(true);
                numMap[num.NumberID!.Value.ToString()] = (++nextNum).ToString();
                copy.NumberID = nextNum;
                copy.AbstractNumId!.Val = absMap[copy.AbstractNumId.Val!.Value];
                numberPart.Numbering.Append(copy);
            }
            numberPart.Numbering.Save();
        }
        foreach (var style in styles)
        {
            RemapStyles(style, styleIds);
            RemapNumbers(style, numMap);
            main.StyleDefinitionsPart.Styles.Append(new W.Style(style.ToString()));
        }
        main.StyleDefinitionsPart.Styles.Save();
        var relationMap = new Dictionary<string, string>();
        var bookmarks = new Dictionary<string, (int Id, string Name)>();
        foreach (var block in blocks)
            foreach (var start in block.DescendantsAndSelf(w + "bookmarkStart"))
            {
                string old = (string)start.Attribute(w + "id")!;
                string name = prefix + (string)start.Attribute(w + "name")!;
                name = name[..Math.Min(40, name.Length)];
                int id = RegisterBookmark(name);
                bookmarks[old] = (id, name);
            }
        foreach (var block in blocks)
        {
            RemapStyles(block, styleIds);
            RemapNumbers(block, numMap);
            foreach (var a in block.DescendantsAndSelf().Attributes().Where(a => a.Name.Namespace == r).ToArray())
            {
                if (!relationMap.TryGetValue(a.Value, out var newId))
                {
                    var hyperlink = src.HyperlinkRelationships.FirstOrDefault(x => x.Id == a.Value);
                    if (hyperlink != null)
                        newId = main.AddHyperlinkRelationship(hyperlink.Uri, hyperlink.IsExternal).Id;
                    else
                    {
                        var related = src.GetPartById(a.Value);
                        if (related is not ImagePart)
                            throw new NotSupportedException("L'importazione supporta immagini incorporate e link, non oggetti OLE/grafici incorporati.");
                        var copied = main.AddPart(related);
                        newId = main.GetIdOfPart(copied);
                    }
                    relationMap[a.Value] = newId;
                }
                a.Value = newId;
            }
            foreach (var e in block.DescendantsAndSelf())
            {
                if (e.Name.LocalName == "docPr")
                    e.SetAttributeValue("id", ++drawingId);
                if (e.Name == w + "bookmarkStart" || e.Name == w + "bookmarkEnd")
                {
                    string id = (string)e.Attribute(w + "id")!;
                    if (!bookmarks.TryGetValue(id, out var replacement))
                        throw new ArgumentException("La selezione include solo parte di un segnalibro.");
                    e.SetAttributeValue(w + "id", replacement.Id);
                    if (e.Name == w + "bookmarkStart")
                        e.SetAttributeValue(w + "name", replacement.Name);
                }
            }
            // Internal REF/PAGEREF and anchor remapping by original bookmark name.
            var nameMap = blocks.SelectMany(b => b.DescendantsAndSelf(w + "bookmarkStart")).Select(b => (string?)b.Attribute(w + "name")).Where(n => n != null).ToArray();
            foreach (var field in block.DescendantsAndSelf().Attributes(w + "instr"))
                field.Value = RewriteReferences(field.Value, prefix, bookmarkNames);
            foreach (var field in block.Descendants(w + "instrText"))
                field.Value = RewriteReferences(field.Value, prefix, bookmarkNames);
            foreach (var anchor in block.DescendantsAndSelf().Attributes(w + "anchor"))
                if (bookmarkNames.Contains(prefix + anchor.Value))
                    anchor.Value = prefix + anchor.Value;
            yield return block;
        }
    }
    static string RewriteReferences(string field, string prefix, HashSet<string> names) => Regex.Replace(field, @"\b(REF|PAGEREF)\s+([A-Za-z][A-Za-z0-9_]*)", m => names.Contains(prefix + m.Groups[2].Value) ? m.Groups[1].Value + " " + prefix + m.Groups[2].Value : m.Value);
    static void RemapStyles(XElement root, Dictionary<string, string> ids)
    {
        foreach (var e in root.DescendantsAndSelf().Where(e => new[] { "pStyle", "rStyle", "tblStyle", "basedOn", "next", "link", "styleLink", "numStyleLink" }.Contains(e.Name.LocalName)))
        {
            var a = e.Attribute(w + "val");
            if (a != null && ids.TryGetValue(a.Value, out var id))
                a.Value = id;
        }
    }
    static void RemapNumbers(XElement root, Dictionary<string, string> ids)
    {
        foreach (var e in root.Descendants(w + "numId"))
        {
            var a = e.Attribute(w + "val");
            if (a != null && ids.TryGetValue(a.Value, out var id))
                a.Value = id;
        }
    }
}
