using System.Collections.Immutable;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace Pawper.Core;

/// <summary>Whole-table construction independent of Grasshopper. No numeric inference from text.</summary>
public static class Tabular
{
    public static Table FromData(IEnumerable<IEnumerable<object?>> branches, IEnumerable<string>? headers = null,
        bool columns = false, IEnumerable<int>? decimals = null, IEnumerable<string>? units = null,
        string culture = "it-IT", IEnumerable<double>? widthsMm = null, bool strict = false)
    {
        var source = branches.Select(b => b.ToArray()).ToArray();
        var names = headers?.ToArray() ?? [];
        int width = columns ? source.Length : source.Select(b => b.Length).DefaultIfEmpty(0).Max();
        if (columns && source.Length > 0 && names.Length > 0 && names.Length != source.Length)
            throw new ArgumentException($"Headers: servono {source.Length} intestazioni, una per ramo colonna.");
        width = Math.Max(width, names.Length);
        if (width is < 1 or > 63) throw new ArgumentException("La tabella richiede da 1 a 63 colonne e almeno un dato o un'intestazione.");
        if (names.Length != 0 && names.Length != width)
            throw new ArgumentException($"Headers: servono {width} intestazioni, ricevute {names.Length}.");
        if (strict && (source.Select(b => b.Length).Distinct().Count() > 1 || !columns && source.Any(b => b.Length != width)))
            throw new ArgumentException("I rami hanno lunghezze diverse. Completare i dati o disattivare Strict.");
        int height = columns ? source.Select(b => b.Length).DefaultIfEmpty(0).Max() : source.Length;
        if (height == 0 && names.Length == 0) throw new ArgumentException("La tabella non contiene righe.");
        var digits = decimals?.ToArray() ?? [];
        var suffixes = units?.ToArray() ?? [];
        var widths = widthsMm?.ToImmutableArray() ?? [];
        CheckCount(digits.Length, width, "Decimals");
        CheckCount(suffixes.Length, width, "Units");
        if (digits.Any(d => d is < -1 or > 15)) throw new ArgumentException("Decimals: usare -1 (precisione originale) oppure 0..15.");
        if (!widths.IsEmpty && (widths.Length != width || widths.Any(w => !double.IsFinite(w) || w <= 0)))
            throw new ArgumentException($"Widths mm: servono {width} larghezze finite e positive, oppure lasciare vuoto.");
        var ci = CultureInfo.GetCultureInfo(culture);
        var rows = ImmutableArray.CreateBuilder<ImmutableArray<Cell>>();
        if (names.Length > 0) rows.Add(names.Select(n => Cell.Text(n ?? "")).ToImmutableArray());
        for (int r = 0; r < height; r++)
        {
            var row = ImmutableArray.CreateBuilder<Cell>();
            for (int c = 0; c < width; c++)
            {
                object? value = columns ? (r < source[c].Length ? source[c][r] : null) : (c < source[r].Length ? source[r][c] : null);
                int precision = digits.Length == 0 ? -1 : digits[digits.Length == 1 ? 0 : c];
                string unit = suffixes.Length == 0 ? "" : suffixes[suffixes.Length == 1 ? 0 : c];
                try { row.Add(FormatCell(value, precision, unit, ci)); }
                catch (ArgumentException ex) { throw new ArgumentException($"Riga {r + 1}, colonna {c + 1}: {ex.Message}", ex); }
            }
            rows.Add(row.ToImmutable());
        }
        return new Table(rows.ToImmutable(), widths, Header: names.Length > 0);
    }

    static void CheckCount(int count, int width, string name)
    {
        if (count != 0 && count != 1 && count != width)
            throw new ArgumentException($"{name}: usare un valore per tutte le colonne, {width} valori oppure lasciare vuoto.");
    }

    static Cell FormatCell(object? value, int decimals, string unit, CultureInfo culture)
    {
        if (value is null) return Cell.Text("");
        bool number = value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
        if (value is double d && !double.IsFinite(d) || value is float f && !float.IsFinite(f))
            throw new ArgumentException("Numero non finito (NaN o infinito).");
        if (number)
        {
            string text = ((IFormattable)value).ToString(decimals < 0 ? null : "N" + decimals, culture);
            if (!string.IsNullOrWhiteSpace(unit)) text += " " + unit;
            return Cell.Text(text) with { Alignment = "right" };
        }
        string content = value switch
        {
            string s => s,
            char c => c.ToString(),
            bool b => b ? "True" : "False",
            DateTime date => date.ToString(culture),
            DateTimeOffset date => date.ToString(culture),
            _ => throw new ArgumentException($"Tipo {value.GetType().Name} non tabellare. Convertirlo in testo; per celle complesse usare Word Table.")
        };
        return Cell.Text(content);
    }

    public sealed record TextResult(Table Table, ImmutableArray<ImmutableArray<string>> Rows, string Separator);

    public static TextResult FromText(string text, string separator = "auto", bool header = true, IEnumerable<double>? widthsMm = null)
    {
        text = text.TrimStart('\uFEFF');
        string delimiter = ResolveSeparator(text, separator);
        using var reader = new StringReader(text);
        using var parser = new TextFieldParser(reader) { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true, TrimWhiteSpace = false };
        parser.SetDelimiters(delimiter);
        var rows = ImmutableArray.CreateBuilder<ImmutableArray<string>>();
        try
        {
            while (!parser.EndOfData)
                if (parser.ReadFields() is { } fields) rows.Add(fields.ToImmutableArray());
        }
        catch (MalformedLineException ex)
        {
            throw new ArgumentException($"Testo delimitato non valido alla riga {ex.LineNumber}: controllare separatore e virgolette.", ex);
        }
        if (rows.Count == 0) throw new ArgumentException("Il testo della tabella è vuoto.");
        var parsed = rows.ToImmutable();
        var body = (header ? parsed.Skip(1) : parsed).Select(r => r.Cast<object?>());
        var table = FromData(body, header ? parsed[0] : null, widthsMm: widthsMm);
        return new(table, parsed, delimiter);
    }

    static string ResolveSeparator(string text, string separator)
    {
        string selected = separator.ToLowerInvariant() switch
        {
            "tab" or "tsv" or "\\t" => "\t", "comma" => ",", "semicolon" => ";", "pipe" => "|", _ => separator
        };
        if (!selected.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            if (selected.Length != 1 || selected[0] is '"' or '\r' or '\n')
                throw new ArgumentException("Separator: auto, tab, comma, semicolon, pipe oppure un singolo carattere diverso da virgolette e ritorno a capo.");
            return selected;
        }
        // Count separators only outside quoted fields in the first nonblank logical record.
        var counts = new Dictionary<char, int> { ['\t'] = 0, [';'] = 0, [','] = 0, ['|'] = 0 };
        bool quoted = false, started = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"')
            {
                if (quoted && i + 1 < text.Length && text[i + 1] == '"') { i++; continue; }
                quoted = !quoted; started = true;
            }
            else if (!quoted && c is '\r' or '\n') { if (started) break; }
            else
            {
                if (!char.IsWhiteSpace(c) || c == '\t') started = true;
                if (!quoted && counts.ContainsKey(c)) counts[c]++;
            }
        }
        int max = counts.Values.Max();
        if (max == 0) return "\t"; // A single-column table is valid.
        var best = counts.Where(c => c.Value == max).ToArray();
        if (best.Length != 1) throw new ArgumentException("Separatore ambiguo: scegliere tab, comma, semicolon o pipe esplicitamente.");
        return best[0].Key.ToString();
    }
}
