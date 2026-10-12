using System.Collections.Immutable;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Pawper.Core;
public sealed partial class WordEngine
{
    XElement TableXml(Table table)
    {
        if (table.Rows.IsDefaultOrEmpty)
            throw new ArgumentException("La tabella non contiene righe.");
        int columns = table.Rows[0].Sum(c => c.ColumnSpan);
        if (columns == 0 || columns > 63 || table.PaddingMm < 0)
            throw new ArgumentException("Colonne o margini cella non validi.");
        var widths = table.WidthsMm.IsDefaultOrEmpty ? Enumerable.Repeat((currentLayout.Landscape ? Math.Max(currentLayout.WidthMm, currentLayout.HeightMm) : Math.Min(currentLayout.WidthMm, currentLayout.HeightMm)) / columns - 2 * currentLayout.MarginMm / columns, columns).ToArray() : table.WidthsMm.ToArray();
        if (widths.Length != columns || widths.Any(x => x <= 0))
            throw new ArgumentException("Serve una larghezza positiva per ogni colonna.");
        var pr = E("tblPr");
        if (table.StyleId != null)
        {
            CheckStyle(table.StyleId);
            pr.Add(V("tblStyle", table.StyleId));
        }
        pr.Add(E("tblW", A("w", Tw(widths.Sum())), A("type", "dxa")), E("tblBorders", new[] { "top", "left", "bottom", "right", "insideH", "insideV" }.Select(n => E(n, A("val", "single"), A("sz", 4), A("color", Color(table.BorderColor))))), E("tblLayout", A("type", "fixed")), E("tblCellMar", new[] { "top", "left", "bottom", "right" }.Select(n => E(n, A("w", Tw(table.PaddingMm)), A("type", "dxa")))));
        var xml = E("tbl", pr, E("tblGrid", widths.Select(x => E("gridCol", A("w", Tw(x))))));
        var active = new Dictionary<int, (int Left, int Span)>();
        for (int rowIndex = 0; rowIndex < table.Rows.Length; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            var tr = E("tr");
            if (table.Header && rowIndex == 0)
                tr.Add(E("trPr", E("tblHeader")));
            int col = 0, index = 0;
            while (col < columns)
            {
                bool continuing = active.TryGetValue(col, out var merge);
                Cell cell;
                if (continuing)
                {
                    cell = new([], merge.Span);
                    if (merge.Left == 1)
                        active.Remove(col);
                    else
                        active[col] = (merge.Left - 1, merge.Span);
                }
                else
                {
                    if (index >= row.Length)
                        cell = Cell.Text("");
                    else
                        cell = row[index++];
                    if (cell.ColumnSpan < 1 || cell.RowSpan < 1 || col + cell.ColumnSpan > columns || rowIndex + cell.RowSpan > table.Rows.Length)
                        throw new ArgumentException("Unione celle fuori dai limiti della tabella.");
                    for (int c = col + 1; c < col + cell.ColumnSpan; c++)
                        if (active.ContainsKey(c))
                            throw new ArgumentException("Unioni celle sovrapposte.");
                    if (cell.RowSpan > 1)
                        active[col] = (cell.RowSpan - 1, cell.ColumnSpan);
                }
                string fill = cell.Fill ?? (table.Header && rowIndex == 0 ? table.HeaderFill : rowIndex % 2 == 0 ? table.StripeFill : "FFFFFF");
                var tcPr = E("tcPr", E("tcW", A("w", Tw(widths.Skip(col).Take(cell.ColumnSpan).Sum())), A("type", "dxa")));
                if (cell.ColumnSpan > 1)
                    tcPr.Add(V("gridSpan", cell.ColumnSpan));
                if (continuing || cell.RowSpan > 1)
                    tcPr.Add(V("vMerge", continuing ? "continue" : "restart"));
                tcPr.Add(E("shd", A("val", "clear"), A("fill", Color(fill))), V("vAlign", "center"));
                var tc = E("tc", tcPr);
                if (cell.Content.Any(b => b is Section or Import))
                    throw new ArgumentException("Sezioni e importazioni non sono consentite nelle celle.");
                foreach (var b in cell.Content.SelectMany(RenderBlock))
                    tc.Add(b);
                if (tc.Elements().Last().Name != w + "p")
                    tc.Add(E("p"));
                foreach (var p in tc.Elements(w + "p"))
                {
                    var pp = p.Element(w + "pPr");
                    if (pp == null)
                    {
                        pp = E("pPr");
                        p.AddFirst(pp);
                    }
                    pp.Element(w + "jc")?.Remove();
                    pp.Add(V("jc", cell.Alignment));
                    if (table.Header && rowIndex == 0)
                        foreach (var run in p.Descendants(w + "r"))
                        {
                            var rp = run.Element(w + "rPr");
                            if (rp == null)
                            {
                                rp = E("rPr");
                                run.AddFirst(rp);
                            }
                            rp.Element(w + "color")?.Remove();
                            // Compare relative luminance: choose the higher contrast black/white text.
                            double Channel(int offset) { double c = Convert.ToInt32(fill.Substring(offset, 2), 16) / 255d; return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4); }
                            double luminance = 0.2126 * Channel(0) + 0.7152 * Channel(2) + 0.0722 * Channel(4);
                            rp.Add(V("color", luminance > 0.179 ? "000000" : "FFFFFF"));
                        }
                }
                tr.Add(tc);
                col += cell.ColumnSpan;
            }
            if (index < row.Length)
                throw new ArgumentException($"Riga {rowIndex + 1}: troppe celle rispetto alla griglia.");
            xml.Add(tr);
        }
        return xml;
    }
    XElement ImageXml(Image img)
    {
        var bytes = Convert.FromBase64String(img.DataBase64);
        var (pixelW, pixelH, format) = ImageSize(bytes);
        if (img.Format.ToLowerInvariant() is not ("png" or "jpg" or "jpeg") || (img.Format == "png") != (format == "png"))
            throw new ArgumentException("Immagine PNG/JPEG non coerente con il formato dichiarato.");
        if (img.WidthMm <= 0 || img.HeightMm is <= 0)
            throw new ArgumentException("Dimensioni immagine non valide.");
        double height = img.HeightMm ?? img.WidthMm * pixelH / pixelW;
        var part = main.AddImagePart(format == "png" ? ImagePartType.Png : ImagePartType.Jpeg);
        using (var stream = new MemoryStream(bytes))
            part.FeedData(stream);
        long cx = (long)Math.Round(img.WidthMm * 36000), cy = (long)Math.Round(height * 36000);
        uint id = ++drawingId;
        XNamespace wp = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing", a = "http://schemas.openxmlformats.org/drawingml/2006/main", pic = "http://schemas.openxmlformats.org/drawingml/2006/picture";
        var picture = new XElement(pic + "pic", new XElement(pic + "nvPicPr", new XElement(pic + "cNvPr", new XAttribute("id", id), new XAttribute("name", "Image " + id), new XAttribute("descr", img.Alt)), new XElement(pic + "cNvPicPr")), new XElement(pic + "blipFill", new XElement(a + "blip", new XAttribute(r + "embed", main.GetIdOfPart(part))), new XElement(a + "stretch", new XElement(a + "fillRect"))), new XElement(pic + "spPr", new XElement(a + "xfrm", new XElement(a + "off", new XAttribute("x", 0), new XAttribute("y", 0)), new XElement(a + "ext", new XAttribute("cx", cx), new XAttribute("cy", cy))), new XElement(a + "prstGeom", new XAttribute("prst", "rect"), new XElement(a + "avLst"))));
        var inline = new XElement(wp + "inline", new XElement(wp + "extent", new XAttribute("cx", cx), new XAttribute("cy", cy)), new XElement(wp + "docPr", new XAttribute("id", id), new XAttribute("name", "Image " + id), new XAttribute("descr", img.Alt)), new XElement(wp + "cNvGraphicFramePr", new XElement(a + "graphicFrameLocks", new XAttribute("noChangeAspect", 1))), new XElement(a + "graphic", new XElement(a + "graphicData", new XAttribute("uri", pic.NamespaceName), picture)));
        return E("p", E("pPr", img.Caption != null ? E("keepNext") : null, V("jc", img.Alignment)), E("r", E("drawing", inline)));
    }
    public static (double Width, double Height, string Format) ImageSize(byte[] b)
    {
        int I(int p) => System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(b.AsSpan(p, 4));
        if (b.Length >= 24 && b.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        {
            int x = I(16), y = I(20);
            if (x <= 0 || y <= 0)
                throw new ArgumentException("PNG non valido.");
            return (x, y, "png");
        }
        if (b.Length > 4 && b[0] == 255 && b[1] == 216)
        {
            int pos = 2;
            while (pos + 4 < b.Length)
            {
                if (b[pos++] != 255)
                    continue;
                int marker = b[pos++];
                if (marker == 255)
                {
                    pos--;
                    continue;
                }
                if (marker is 0xD8 or 0xD9 || marker is >= 0xD0 and <= 0xD7)
                    continue;
                int size = (b[pos] << 8) + b[pos + 1];
                if (size < 2 || pos + size > b.Length)
                    break;
                if (marker is 0xC0 or 0xC1 or 0xC2 && size >= 7)
                {
                    int h = (b[pos + 3] << 8) + b[pos + 4], width = (b[pos + 5] << 8) + b[pos + 6];
                    if (h > 0 && width > 0)
                        return (width, h, "jpeg");
                }
                pos += size;
            }
        }
        throw new ArgumentException("Il contenuto non è un PNG/JPEG valido.");
    }
    XElement NoteXml(Note n)
    {
        int id = ++noteId;
        var para = E("p", E("r", E(n.Endnote ? "endnoteRef" : "footnoteRef")), RunXml(new(" " + n.Text)));
        if (n.Endnote)
        {
            var part = main.EndnotesPart ?? main.AddNewPart<EndnotesPart>();
            part.Endnotes ??= new W.Endnotes();
            if (!part.Endnotes.HasChildren)
            {
                part.Endnotes.Append(new W.Endnote(E("endnote", A("id", -1), A("type", "separator"), E("p", E("r", E("separator")))).ToString()));
                part.Endnotes.Append(new W.Endnote(E("endnote", A("id", 0), A("type", "continuationSeparator"), E("p", E("r", E("continuationSeparator")))).ToString()));
            }
            part.Endnotes.Append(new W.Endnote(E("endnote", A("id", id), para).ToString()));
            part.Endnotes.Save();
        }
        else
        {
            var part = main.FootnotesPart ?? main.AddNewPart<FootnotesPart>();
            part.Footnotes ??= new W.Footnotes();
            if (!part.Footnotes.HasChildren)
            {
                part.Footnotes.Append(new W.Footnote(E("footnote", A("id", -1), A("type", "separator"), E("p", E("r", E("separator")))).ToString()));
                part.Footnotes.Append(new W.Footnote(E("footnote", A("id", 0), A("type", "continuationSeparator"), E("p", E("r", E("continuationSeparator")))).ToString()));
            }
            part.Footnotes.Append(new W.Footnote(E("footnote", A("id", id), para).ToString()));
            part.Footnotes.Save();
        }
        return E("p", RunXml(new(n.Anchor)), E("r", E(n.Endnote ? "endnoteReference" : "footnoteReference", A("id", id))));
    }
    XElement CommentXml(Comment c)
    {
        int id = ++commentId;
        var part = main.WordprocessingCommentsPart ?? main.AddNewPart<WordprocessingCommentsPart>();
        part.Comments ??= new W.Comments();
        part.Comments.Append(new W.Comment(E("comment", A("id", id), A("author", c.Author), E("p", E("r", E("t", c.Text)))).ToString()));
        part.Comments.Save();
        return E("p", E("commentRangeStart", A("id", id)), RunXml(new(c.Anchor)), E("commentRangeEnd", A("id", id)), E("r", E("commentReference", A("id", id))));
    }
    void SetMetadata(Metadata m)
    {
        package.PackageProperties.Title = m.Title;
        package.PackageProperties.Creator = m.Author;
        package.PackageProperties.Subject = m.Subject;
        package.PackageProperties.Keywords = m.Keywords;
        if (m.Custom == null)
            return;
        var part = package.CustomFilePropertiesPart ?? package.AddCustomFilePropertiesPart();
        part.Properties ??= new DocumentFormat.OpenXml.CustomProperties.Properties();
        int id = part.Properties.Elements<DocumentFormat.OpenXml.CustomProperties.CustomDocumentProperty>().Select(x => x.PropertyId?.Value ?? 1).DefaultIfEmpty(1).Max();
        foreach (var kv in m.Custom)
        {
            part.Properties.Elements<DocumentFormat.OpenXml.CustomProperties.CustomDocumentProperty>().FirstOrDefault(x => x.Name == kv.Key)?.Remove();
            part.Properties.Append(new DocumentFormat.OpenXml.CustomProperties.CustomDocumentProperty(new DocumentFormat.OpenXml.VariantTypes.VTLPWSTR(kv.Value)) { Name = kv.Key, PropertyId = ++id, FormatId = "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}" });
        }
        part.Properties.Save();
    }
}
