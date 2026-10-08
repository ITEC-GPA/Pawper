using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pawper.Core;

public static class Json
{
    public static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true, Converters = { new ImmutableArrayConverterFactory(), new JsonStringEnumConverter() } };
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Read<T>(string json) => JsonSerializer.Deserialize<T>(json, Options) ?? throw new ArgumentException("Dati JSON vuoti.");
}
internal sealed class ImmutableArrayConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>);
    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options) => (JsonConverter)Activator.CreateInstance(typeof(ArrayConverter<>).MakeGenericType(type.GetGenericArguments()[0]))!;
    sealed class ArrayConverter<T> : JsonConverter<ImmutableArray<T>>
    {
        public override ImmutableArray<T> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => (JsonSerializer.Deserialize<T[]>(ref reader, options) ?? []).ToImmutableArray();
        public override void Write(Utf8JsonWriter writer, ImmutableArray<T> value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value.IsDefault ? [] : value.ToArray(), options);
    }
}
public record RunFormat(string? Font = null, double? Size = null, bool Bold = false, bool Italic = false, bool Underline = false, string? Color = null, string? Script = null);
public record TextRun(string Text = "", RunFormat? Format = null, string? Link = null, string? Math = null, string? Field = null, string? StyleId = null);
public record ParagraphFormat(string Alignment = "left", double Before = 0, double After = 6, double Line = 1.15, double LeftMm = 0, double RightMm = 0, double FirstLineMm = 0, bool KeepNext = false);
public record Style(string Id, string Name, string BasedOn = "Normal", RunFormat? Run = null, ParagraphFormat? Paragraph = null, string Type = "paragraph");
public record PageLayout(double WidthMm = 215.9, double HeightMm = 279.4, double MarginMm = 20, bool Landscape = false,
    string? Header = null, string? Footer = null, string? FirstHeader = null, string? FirstFooter = null, string? EvenHeader = null, string? EvenFooter = null, int? PageStart = null);
public record Metadata(string Title = "", string Author = "", string Subject = "", string Keywords = "", ImmutableDictionary<string, string>? Custom = null);
public record Document(ImmutableArray<ContentBlock> Blocks, string? SourceBase64 = null, ImmutableArray<Style> Styles = default,
    PageLayout? Layout = null, Metadata? Properties = null, ImmutableDictionary<string, string>? Replacements = null,
    ImmutableArray<TemplateTarget> Targets = default, bool StrictPlaceholders = true)
{
    public static Document Empty => new([]);
    public Document Append(IEnumerable<ContentBlock> blocks) => this with { Blocks = Blocks.AddRange(blocks) };
    public static Document Load(string path)
    {
        if (!new[] { ".docx", ".dotx" }.Contains(Path.GetExtension(path).ToLowerInvariant()))
            throw new ArgumentException("Sono supportati DOCX e DOTX.");
        return new([], Convert.ToBase64String(File.ReadAllBytes(path)));
    }
}
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Paragraph), "paragraph")]
[JsonDerivedType(typeof(Table), "table")]
[JsonDerivedType(typeof(Image), "image")]
[JsonDerivedType(typeof(Equation), "equation")]
[JsonDerivedType(typeof(Section), "section")]
[JsonDerivedType(typeof(Field), "field")]
[JsonDerivedType(typeof(Bookmark), "bookmark")]
[JsonDerivedType(typeof(Note), "note")]
[JsonDerivedType(typeof(Comment), "comment")]
[JsonDerivedType(typeof(PageBreak), "break")]
[JsonDerivedType(typeof(Import), "import")]
[JsonDerivedType(typeof(ContentControl), "control")]
public abstract record ContentBlock;
public record Paragraph(ImmutableArray<TextRun> Runs, string StyleId = "Normal", ParagraphFormat? Format = null, int? ListLevel = null, bool Bullet = false, string ListKey = "default") : ContentBlock
{
    public static Paragraph Text(string text, string style = "Normal") => new([new(text)], style);
}
public record Cell(ImmutableArray<ContentBlock> Content, int ColumnSpan = 1, int RowSpan = 1, string? Fill = null, string Alignment = "left")
{
    public static Cell Text(string value) => new([Paragraph.Text(value)]);
}
public record Table(ImmutableArray<ImmutableArray<Cell>> Rows, ImmutableArray<double> WidthsMm = default, bool Header = true,
    string HeaderFill = "24445C", string StripeFill = "EEF3F6", string BorderColor = "CBD5DD", double PaddingMm = 2, string? StyleId = null) : ContentBlock;
public record Image(string DataBase64, string Format = "png", double WidthMm = 100, double? HeightMm = null, string Alt = "", string? Caption = null, string Alignment = "center") : ContentBlock
{
    public static Image Load(string path, double width = 100, string alt = "") => new(Convert.ToBase64String(File.ReadAllBytes(path)), Path.GetExtension(path).TrimStart('.').ToLowerInvariant(), width, null, alt);
}
public record Equation(string Latex, bool Inline = false, string? BookmarkName = null, bool Numbered = false) : ContentBlock;
public record Section(PageLayout Layout, ImmutableArray<ContentBlock> Content) : ContentBlock;
public record Field(string Instruction, string Placeholder = "Aggiornare il campo in Word", string StyleId = "Normal") : ContentBlock;
public record Bookmark(string Name, ImmutableArray<ContentBlock> Content) : ContentBlock;
public record Note(string Text, string Anchor = "Nota", bool Endnote = false) : ContentBlock;
public record Comment(string Anchor, string Text, string Author = "Pawper") : ContentBlock;
public record PageBreak : ContentBlock;
public record Import(string SourceBase64, ImmutableArray<int> BodyIndices) : ContentBlock;
public record ContentControl(string Tag, ImmutableArray<ContentBlock> Content) : ContentBlock;
public record TemplateTarget(string Name, ImmutableArray<ContentBlock> Content, bool Bookmark = false);
public record Inspection(ImmutableArray<string> Styles, ImmutableArray<string> Bookmarks, ImmutableArray<string> Controls, Metadata Properties, ImmutableArray<string> Validation, string Structure);

public static class Composition
{
    public static ImmutableArray<ContentBlock> Repeat(IEnumerable<ContentBlock> blocks, IEnumerable<IReadOnlyDictionary<string, string>> rows)
        => rows.SelectMany(row => blocks.Select(b => ReplaceBlock(b, row))).ToImmutableArray();
    public static ContentBlock ReplaceBlock(ContentBlock b, IReadOnlyDictionary<string, string> values)
    {
        string R(string s)
        {
            foreach (var v in values)
                s = s.Replace("{{" + v.Key + "}}", v.Value);
            return s;
        }
        ImmutableArray<ContentBlock> B(ImmutableArray<ContentBlock> a) => a.Select(x => ReplaceBlock(x, values)).ToImmutableArray();
        return b switch
        {
            Paragraph p => p with { Runs = p.Runs.Select(r => r with { Text = R(r.Text), Math = r.Math is null ? null : R(r.Math) }).ToImmutableArray() },
            Table t => t with { Rows = t.Rows.Select(row => row.Select(c => c with { Content = B(c.Content) }).ToImmutableArray()).ToImmutableArray() },
            Equation e => e with { Latex = R(e.Latex) },
            Section s => s with { Content = B(s.Content) },
            Bookmark m => m with { Name = R(m.Name), Content = B(m.Content) },
            ContentControl c => c with { Tag = R(c.Tag), Content = B(c.Content) },
            Image i => i with { Alt = R(i.Alt), Caption = i.Caption is null ? null : R(i.Caption) },
            Note n => n with { Text = R(n.Text), Anchor = R(n.Anchor) },
            Comment c => c with { Text = R(c.Text), Anchor = R(c.Anchor) },
            _ => b
        };
    }
    public static string Number(double value, int decimals = 2, string unit = "", string culture = "it-IT")
    {
        if (!double.IsFinite(value) || decimals is < 0 or > 15)
            throw new ArgumentException("Numero o decimali non validi.");
        return value.ToString("N" + decimals, System.Globalization.CultureInfo.GetCultureInfo(culture)) + (string.IsNullOrWhiteSpace(unit) ? "" : " " + unit);
    }
}
