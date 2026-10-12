using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawper.Core;

/// <summary>Small, explicitly documented text syntax; not a full Markdown implementation.</summary>
public static class StructuredText
{
    public static ImmutableArray<ContentBlock> Parse(string text, string listScope = "text")
    {
        var blocks = ImmutableArray.CreateBuilder<ContentBlock>();
        var paragraph = new List<string>();
        int paragraphLine = 1, group = 0; string? listKind = null;
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        void Flush()
        {
            if (paragraph.Count == 0) return;
            blocks.Add(new Paragraph(Inline(string.Join("\n", paragraph), paragraphLine)));
            paragraph.Clear();
        }
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i], trimmed = line.Trim();
            if (trimmed.Length == 0) { Flush(); listKind = null; continue; }
            if (trimmed.StartsWith("$$", StringComparison.Ordinal))
            {
                Flush(); listKind = null;
                int start = i;
                string latex;
                if (trimmed.Length > 4 && trimmed.EndsWith("$$", StringComparison.Ordinal)) latex = trimmed[2..^2].Trim();
                else if (trimmed == "$$")
                {
                    var formula = new List<string>();
                    while (++i < lines.Length && lines[i].Trim() != "$$") formula.Add(lines[i]);
                    if (i == lines.Length) throw Error(start + 1, "Manca la chiusura $$ della formula.");
                    latex = string.Join(" ", formula).Trim();
                }
                else throw Error(i + 1, "Usare $$ formula $$ oppure delimitatori $$ su righe separate.");
                ValidateMath(latex, start + 1);
                blocks.Add(new Equation(latex)); continue;
            }
            if (trimmed == "---") { Flush(); listKind = null; blocks.Add(new PageBreak()); continue; }
            var heading = Regex.Match(line, @"^(#{1,9})\s+(.+)$");
            if (heading.Success)
            {
                Flush(); listKind = null;
                blocks.Add(new Paragraph(Inline(heading.Groups[2].Value, i + 1), "Heading" + heading.Groups[1].Length)); continue;
            }
            var item = Regex.Match(line, @"^( *)(?:([-+])|([0-9]+)\.)\s+(.+)$");
            if (item.Success)
            {
                Flush();
                int spaces = item.Groups[1].Length;
                if (spaces % 2 != 0 || spaces / 2 > 8) throw Error(i + 1, "Negli elenchi usare due spazi per livello, da 0 a 8.");
                bool bullet = item.Groups[2].Success;
                string kind = bullet ? "bullet" : "number";
                if (listKind != kind)
                {
                    if (!bullet && item.Groups[3].Value != "1") throw Error(i + 1, "Un nuovo elenco numerato deve iniziare con 1.");
                    group++; listKind = kind;
                }
                blocks.Add(new Paragraph(Inline(item.Groups[4].Value, i + 1), ListLevel: spaces / 2, Bullet: bullet, ListKey: listScope + "/" + group)); continue;
            }
            if (trimmed.StartsWith("```", StringComparison.Ordinal) || trimmed.StartsWith('>') || trimmed.StartsWith('|') || trimmed.StartsWith("![", StringComparison.Ordinal) || Regex.IsMatch(trimmed, @"^#{10,}\s"))
                throw Error(i + 1, "Sintassi non supportata da Text Blocks. Usare i componenti dedicati per tabelle, immagini e contenuti avanzati.");
            listKind = null;
            if (paragraph.Count == 0) paragraphLine = i + 1;
            paragraph.Add(line);
        }
        Flush();
        return blocks.ToImmutable();
    }

    static ImmutableArray<TextRun> Inline(string text, int line)
    {
        var runs = ImmutableArray.CreateBuilder<TextRun>(); var buffer = new StringBuilder();
        bool bold = false, italic = false;
        void Flush()
        {
            if (buffer.Length == 0) return;
            runs.Add(new TextRun(buffer.ToString(), bold || italic ? new RunFormat(Bold: bold, Italic: italic) : null));
            buffer.Clear();
        }
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\\' && i + 1 < text.Length && "\\*$#[]".Contains(text[i + 1])) { buffer.Append(text[++i]); continue; }
            if (c == '*')
            {
                Flush();
                int count = 1; while (i + count < text.Length && text[i + count] == '*') count++;
                if (count > 3) throw Error(line, "Usare *, ** o *** per corsivo, grassetto o entrambi.");
                if (count >= 2) bold = !bold;
                if (count % 2 == 1) italic = !italic;
                i += count - 1; continue;
            }
            if (c == '$')
            {
                Flush(); int end = i + 1;
                while (end < text.Length && (text[end] != '$' || text[end - 1] == '\\')) end++;
                if (end == text.Length) throw Error(line, "Manca il $ di chiusura della formula inline.");
                string latex = text[(i + 1)..end]; ValidateMath(latex, line);
                runs.Add(new TextRun(Math: latex)); i = end; continue;
            }
            buffer.Append(c);
        }
        if (bold || italic) throw Error(line, "Formattazione non chiusa: controllare gli asterischi, oppure usare \\* per un asterisco letterale.");
        Flush();
        return runs.ToImmutable();
    }
    static void ValidateMath(string latex, int line)
    {
        if (string.IsNullOrWhiteSpace(latex)) throw Error(line, "Formula vuota.");
        try { MathParser.Parse(latex); } catch (ArgumentException ex) { throw Error(line, ex.Message); }
    }
    static ArgumentException Error(int line, string message) => new($"Riga {line}: {message}");
}

public static class SimpleInputs
{
    public static ImmutableDictionary<string, string> Values(IEnumerable<string> names, IEnumerable<string> values)
    {
        var keys = names.ToArray(); var data = values.ToArray();
        if (keys.Length != data.Length) throw new ArgumentException("Names e Values devono avere la stessa lunghezza; nessuna ripetizione implicita.");
        var result = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        for (int i = 0; i < keys.Length; i++)
        {
            string key = (keys[i] ?? "").Trim();
            if (key.StartsWith("{{", StringComparison.Ordinal) && key.EndsWith("}}", StringComparison.Ordinal)) key = key[2..^2].Trim();
            if (string.IsNullOrWhiteSpace(key) || key.IndexOfAny(['{', '}', '\r', '\n']) >= 0) throw new ArgumentException($"Nome non valido alla posizione {i + 1}.");
            if (!result.TryAdd(key, data[i] ?? "")) throw new ArgumentException("Nome duplicato: " + key);
        }
        return result.ToImmutable();
    }

    public static Document Fill(Document document, IEnumerable<string> names, IEnumerable<string> values, bool strict = true)
        => document with { Replacements = (document.Replacements ?? ImmutableDictionary<string, string>.Empty).SetItems(Values(names, values)), StrictPlaceholders = strict };

    public static Document CreateDocument(string title, IEnumerable<ContentBlock> blocks, Document? source = null, PageLayout? layout = null, IEnumerable<Style>? styles = null)
    {
        var document = source ?? Document.Empty;
        if (!string.IsNullOrWhiteSpace(title)) document = document.Append([Paragraph.Text(title, "Title")]);
        return document.Append(blocks) with
        {
            Layout = layout ?? document.Layout,
            Styles = (document.Styles.IsDefault ? [] : document.Styles).AddRange(styles ?? [])
        };
    }

    public static ParagraphFormat ParagraphSettings(string alignment = "left", double before = 0, double after = 6, double line = 1.15, bool keepNext = false)
    {
        alignment = alignment.Trim().ToLowerInvariant();
        if (alignment == "justified") alignment = "both";
        if (alignment is not ("left" or "center" or "right" or "both")) throw new ArgumentException("Alignment: left, center, right oppure both.");
        if (!double.IsFinite(before) || !double.IsFinite(after) || before < 0 || after < 0 || before > 1000 || after > 1000 || !double.IsFinite(line) || line <= 0 || line > 10)
            throw new ArgumentException("Spaziatura Before/After: 0..1000 punti; Line: maggiore di 0 e fino a 10.");
        return new(alignment, before, after, line, KeepNext: keepNext);
    }

    public static PageLayout Page(string format = "Letter", bool landscape = false, double marginMm = 20, string header = "", string footer = "")
    {
        var (width, height) = format.Trim().ToLowerInvariant() switch
        {
            "a4" => (210d, 297d), "a3" => (297d, 420d), "letter" => (215.9, 279.4), "legal" => (215.9, 355.6),
            _ => throw new ArgumentException("Format: A4, A3, Letter oppure Legal.")
        };
        if (!double.IsFinite(marginMm) || marginMm < 0 || 2 * marginMm >= Math.Min(width, height)) throw new ArgumentException("Margine non valido: deve lasciare spazio utile nella pagina.");
        return new(width, height, marginMm, landscape, header, footer);
    }

    public static Table FormatTable(ContentBlock block, string headerFill, string stripeFill, string border, double paddingMm = 2, string? styleId = null)
    {
        if (block is not Table table) throw new ArgumentException("Collegare una tabella a Table Format.");
        string Color(string text) => Regex.IsMatch(text, "^[0-9a-fA-F]{6}$") ? text.ToUpperInvariant() : throw new ArgumentException("Colore RGB non valido.");
        if (!double.IsFinite(paddingMm) || paddingMm < 0 || paddingMm > 50) throw new ArgumentException("Padding mm: da 0 a 50.");
        return table with { HeaderFill = Color(headerFill), StripeFill = Color(stripeFill), BorderColor = Color(border), PaddingMm = paddingMm, StyleId = string.IsNullOrWhiteSpace(styleId) ? table.StyleId : styleId };
    }
}
