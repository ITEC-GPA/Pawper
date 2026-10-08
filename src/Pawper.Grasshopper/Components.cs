using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Pawper.Core;
using Document = Pawper.Core.Document;
using ImageBlock = Pawper.Core.Image;

namespace Pawper.Grasshopper;
public abstract class WordComponent : GH_Component
{
    protected WordComponent(string name, string nick, string description, string category) : base(name, nick, description, "Pawper", category) { }
    public override Guid ComponentGuid => Identity.Id(GetType().Name);
    protected override Bitmap Icon => PawperIcons.Create(GetType().Name);
    protected sealed override void SolveInstance(IGH_DataAccess da)
    {
        try
        {
            Solve(da);
        }
        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message); }
    }
    protected abstract void Solve(IGH_DataAccess da);
    protected static T Val<T>(IGH_DataAccess da, int i) where T : class
    {
        WordGoo<T> value = null!;
        if (!da.GetData(i, ref value) || value?.Value == null)
            throw new ArgumentException("Collegare l'input " + (i + 1));
        return value.Value;
    }
    protected static T? Opt<T>(IGH_DataAccess da, int i) where T : class
    {
        WordGoo<T> value = null!;
        return da.GetData(i, ref value) ? value?.Value : null;
    }
    protected static ImmutableArray<T> List<T>(IGH_DataAccess da, int i) where T : class
    {
        var list = new List<WordGoo<T>>();
        da.GetDataList(i, list);
        return list.Select(x => x.Value).ToImmutableArray();
    }
    protected static string Text(IGH_DataAccess da, int i, string fallback = "")
    {
        string v = fallback;
        da.GetData(i, ref v);
        return v;
    }
    protected static double Num(IGH_DataAccess da, int i, double fallback = 0)
    {
        double v = fallback;
        da.GetData(i, ref v);
        return v;
    }
    protected static int Int(IGH_DataAccess da, int i, int fallback = 0)
    {
        int v = fallback;
        da.GetData(i, ref v);
        return v;
    }
    protected static bool Bool(IGH_DataAccess da, int i)
    {
        bool v = false;
        da.GetData(i, ref v);
        return v;
    }
    protected static void Out<T>(IGH_DataAccess da, int i, T value) where T : class => da.SetData(i, new WordGoo<T>(value));
    protected static void Block(IGH_DataAccess da, ContentBlock value) => Out<ContentBlock>(da, 0, value);
    protected static void BlockOutput(GH_OutputParamManager p) => p.AddParameter(new BlockParam(), "Block", "B", "Blocco del documento", GH_ParamAccess.item);
    protected static void DocOutput(GH_OutputParamManager p) => p.AddParameter(new DocumentParam(), "Document", "D", "Documento in memoria", GH_ParamAccess.item);
    protected static void DocInput(GH_InputParamManager p) => p.AddParameter(new DocumentParam(), "Document", "D", "Documento in memoria", GH_ParamAccess.item);
    protected static void BlocksInput(GH_InputParamManager p, string name = "Blocks") => p.AddParameter(new BlockParam(), name, "B", "Lista ordinata di blocchi", GH_ParamAccess.list);
    protected static void Optional(GH_InputParamManager p, int start = 0)
    {
        for (int i = start; i < p.ParamCount; i++)
            p[i].Optional = true;
    }
}
public sealed class NewDocumentComponent : WordComponent
{
    public NewDocumentComponent() : base("New Document", "New", "Crea un documento vuoto oppure carica una copia di DOCX/DOTX.", "Document") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Template path", "P", "Percorso DOCX/DOTX opzionale", GH_ParamAccess.item, "");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => DocOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        var file = Text(da, 0);
        Out(da, 0, string.IsNullOrWhiteSpace(file) ? Document.Empty : Document.Load(file));
    }
}
public sealed class ComposeComponent : WordComponent
{
    public ComposeComponent() : base("Compose Document", "Compose", "Aggiunge blocchi ordinati a una nuova copia del documento.", "Document") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        DocInput(p);
        BlocksInput(p);
        p.AddParameter(new StyleParam(), "Styles", "S", "Stili da aggiungere o aggiornare", GH_ParamAccess.list);
        p.AddParameter(new LayoutParam(), "Layout", "L", "Impostazioni pagina opzionali", GH_ParamAccess.item);
        Optional(p, 1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => DocOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        var d = Val<Document>(da, 0);
        var styles = List<Style>(da, 2);
        Out(da, 0, d.Append(List<ContentBlock>(da, 1)) with
        {
            Styles = (d.Styles.IsDefault ? ImmutableArray<Style>.Empty : d.Styles).AddRange(styles),
            Layout = Opt<PageLayout>(da, 3) ?? d.Layout
        });
    }
}
public sealed class TextRunComponent : WordComponent
{
    public TextRunComponent() : base("Text Run", "Run", "Testo formattato, collegamento, formula inline o campo.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Text", "T", "Testo", GH_ParamAccess.item, "");
        p.AddTextParameter("Font", "F", "Nome font", GH_ParamAccess.item, "Aptos");
        p.AddNumberParameter("Size", "S", "Dimensione in punti", GH_ParamAccess.item, 11);
        p.AddBooleanParameter("Bold", "B", "Grassetto", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Italic", "I", "Corsivo", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Underline", "U", "Sottolineato", GH_ParamAccess.item, false);
        p.AddTextParameter("Color", "C", "RGB senza #", GH_ParamAccess.item, "000000");
        p.AddTextParameter("Script", "V", "baseline, superscript, subscript", GH_ParamAccess.item, "baseline");
        p.AddTextParameter("Link", "L", "URL oppure #segnalibro", GH_ParamAccess.item, "");
        p.AddTextParameter("Math", "M", "Formula LaTeX inline opzionale", GH_ParamAccess.item, "");
        p.AddTextParameter("Field", "Q", "Codice campo opzionale", GH_ParamAccess.item, "");
        p.AddTextParameter("Style ID", "Y", "Stile carattere opzionale", GH_ParamAccess.item, "");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new RunParam(), "Run", "R", "Frammento formattato", GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {
        string? S(int i) => string.IsNullOrEmpty(Text(da, i)) ? null : Text(da, i);
        Out(da, 0, new TextRun(Text(da, 0), new(Text(da, 1), Num(da, 2), Bool(da, 3), Bool(da, 4), Bool(da, 5), Text(da, 6), Text(da, 7)), S(8), S(9), S(10), S(11)));
    }
}
public sealed class ParagraphComponent : WordComponent
{
    public ParagraphComponent() : base("Paragraph", "Paragraph", "Paragrafo con testo, run misti e stile Word.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Text", "T", "Testo iniziale", GH_ParamAccess.item, "");
        p.AddParameter(new RunParam(), "Runs", "R", "Frammenti aggiunti dopo il testo", GH_ParamAccess.list);
        p.AddTextParameter("Style ID", "S", "Identificatore stile Word", GH_ParamAccess.item, "Normal");
        p.AddTextParameter("Format", "F", "Opzioni JSON ParagraphFormat, vuoto per ereditare lo stile", GH_ParamAccess.item, "");
        p[1].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        var text = Text(da, 0);
        var runs = List<TextRun>(da, 1);
        if (text.Length > 0)
            runs = runs.Insert(0, new(text));
        Block(da, new Paragraph(runs, Text(da, 2), string.IsNullOrWhiteSpace(Text(da, 3)) ? null : Json.Read<ParagraphFormat>(Text(da, 3))));
    }
}
public sealed class HeadingComponent : WordComponent
{
    public HeadingComponent() : base("Heading", "Heading", "Titolo, sottotitolo o intestazione gerarchica.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Text", "T", "Testo", GH_ParamAccess.item);
        p.AddIntegerParameter("Level", "L", "0=Title; -1=Subtitle; 1..9=Heading", GH_ParamAccess.item, 1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        int l = Int(da, 1, 1);
        if (l is < -1 or > 9)
            throw new ArgumentException("Livello tra -1 e 9.");
        Block(da, Paragraph.Text(Text(da, 0), l == 0 ? "Title" : l == -1 ? "Subtitle" : "Heading" + l));
    }
}
public sealed class ListComponent : WordComponent
{
    public ListComponent() : base("List Item", "List", "Voce di elenco numerato o puntato multilivello.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Text", "T", "Voce", GH_ParamAccess.item);
        p.AddIntegerParameter("Level", "L", "Livello 0..8", GH_ParamAccess.item, 0);
        p.AddBooleanParameter("Bullet", "B", "Puntato anziché numerato", GH_ParamAccess.item, false);
        p.AddTextParameter("Group", "G", "Identità elenco; un nuovo nome riavvia la numerazione", GH_ParamAccess.item, "default");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new Paragraph([new(Text(da, 0))], ListLevel: Int(da, 1), Bullet: Bool(da, 2), ListKey: Text(da, 3)));
}
public sealed class StyleComponent : WordComponent
{
    public StyleComponent() : base("Word Style", "Style", "Crea o aggiorna uno stile Word per ID, distinto dal nome visualizzato.", "Styles") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("ID", "I", "ID univoco stile", GH_ParamAccess.item);
        p.AddTextParameter("Name", "N", "Nome visualizzato", GH_ParamAccess.item);
        p.AddTextParameter("Based on", "B", "Stile padre", GH_ParamAccess.item, "Normal");
        p.AddTextParameter("Run format", "R", "JSON RunFormat", GH_ParamAccess.item, "{}");
        p.AddTextParameter("Paragraph format", "P", "JSON ParagraphFormat", GH_ParamAccess.item, "{}");
        p.AddTextParameter("Type", "T", "paragraph, character, table", GH_ParamAccess.item, "paragraph");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new StyleParam(), "Style", "S", "Stile Word", GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da) => Out(da, 0, new Style(Text(da, 0), Text(da, 1), Text(da, 2), Json.Read<RunFormat>(Text(da, 3)), Json.Read<ParagraphFormat>(Text(da, 4)), Text(da, 5)));
}
public sealed class LayoutComponent : WordComponent
{
    public LayoutComponent() : base("Page Layout", "Layout", "Dimensioni e margini in millimetri; intestazioni e piè di pagina.", "Layout") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddNumberParameter("Width mm", "W", "Larghezza", GH_ParamAccess.item, 215.9);
        p.AddNumberParameter("Height mm", "H", "Altezza", GH_ParamAccess.item, 279.4);
        p.AddNumberParameter("Margin mm", "M", "Margine uniforme", GH_ParamAccess.item, 20);
        p.AddBooleanParameter("Landscape", "L", "Orientamento orizzontale", GH_ParamAccess.item, false);
        p.AddTextParameter("Header", "A", "Intestazione", GH_ParamAccess.item, "");
        p.AddTextParameter("Footer", "F", "Piè pagina; campi {{PAGE}}, {{NUMPAGES}}", GH_ParamAccess.item, "");
        p.AddTextParameter("Advanced", "O", "JSON PageLayout per prima pagina, pari e PageStart; vuoto per input standard", GH_ParamAccess.item, "");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new LayoutParam(), "Layout", "L", "Layout pagina", GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da) => Out(da, 0, Text(da, 6).Length > 0 ? Json.Read<PageLayout>(Text(da, 6)) : new PageLayout(Num(da, 0), Num(da, 1), Num(da, 2), Bool(da, 3), Text(da, 4), Text(da, 5)));
}
public sealed class SectionComponent : WordComponent
{
    public SectionComponent() : base("Section", "Section", "Inizia una nuova sezione con layout dedicato.", "Layout") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddParameter(new LayoutParam(), "Layout", "L", "Layout nuova sezione", GH_ParamAccess.item);
        BlocksInput(p);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new Section(Val<PageLayout>(da, 0), List<ContentBlock>(da, 1)));
}
public sealed class CellComponent : WordComponent
{
    public CellComponent() : base("Table Cell", "Cell", "Cella con blocchi, unioni e colore condizionale collegabile.", "Tables") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Text", "T", "Testo iniziale", GH_ParamAccess.item, "");
        BlocksInput(p);
        p.AddIntegerParameter("Column span", "C", "Colonne occupate", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Row span", "R", "Righe occupate; omettere le celle coperte nelle righe successive", GH_ParamAccess.item, 1);
        p.AddTextParameter("Fill", "F", "RGB opzionale, collegabile a una condizione", GH_ParamAccess.item, "");
        p.AddTextParameter("Alignment", "A", "left, center, right, both", GH_ParamAccess.item, "left");
        p[1].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new CellParam(), "Cell", "C", "Cella Word", GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {
        var b = List<ContentBlock>(da, 1);
        if (Text(da, 0).Length > 0)
            b = b.Insert(0, Paragraph.Text(Text(da, 0)));
        Out(da, 0, new Cell(b, Int(da, 2, 1), Int(da, 3, 1), Text(da, 4).Length > 0 ? Text(da, 4) : null, Text(da, 5)));
    }
}
public sealed class TableComponent : WordComponent
{
    public TableComponent() : base("Word Table", "Table", "Ogni ramo del Data Tree è una riga; ogni elemento è una cella. Ordine dei path Grasshopper preservato.", "Tables") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Rows", "R", "Data Tree di testo, numeri o Table Cell", GH_ParamAccess.tree);
        p.AddNumberParameter("Widths mm", "W", "Una larghezza per colonna; vuoto=uniforme", GH_ParamAccess.list);
        p.AddBooleanParameter("Header", "H", "Prima riga come intestazione ripetuta", GH_ParamAccess.item, true);
        p.AddTextParameter("Options", "O", "JSON TableOptions", GH_ParamAccess.item, "{}");
        p[1].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        if (!da.GetDataTree<IGH_Goo>(0, out var tree))
            return;
        var rows = tree.Branches.Select(branch => branch.Select(v => v is WordGoo<Cell> c ? c.Value : Cell.Text(v?.ToString() ?? "")).ToImmutableArray()).ToImmutableArray();
        var widths = new List<double>();
        da.GetDataList(1, widths);
        var o = Json.Read<TableOptions>(Text(da, 3));
        Block(da, new Table(rows, widths.ToImmutableArray(), Bool(da, 2), o.HeaderFill, o.StripeFill, o.BorderColor, o.PaddingMm, o.StyleId));
    }
}
public record TableOptions(string HeaderFill = "24445C", string StripeFill = "EEF3F6", string BorderColor = "CBD5DD", double PaddingMm = 2, string? StyleId = null);
public sealed class ImageComponent : WordComponent
{
    public ImageComponent() : base("Word Image", "Image", "Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.", "Images") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Path or Base64", "P", "Percorso immagine, oppure Base64 con formato", GH_ParamAccess.item);
        p.AddNumberParameter("Width mm", "W", "Larghezza", GH_ParamAccess.item, 100);
        p.AddTextParameter("Alt text", "A", "Testo alternativo", GH_ParamAccess.item, "");
        p.AddTextParameter("Caption", "C", "Didascalia opzionale", GH_ParamAccess.item, "");
        p.AddTextParameter("Format", "F", "Vuoto=percorso; png/jpeg=Base64", GH_ParamAccess.item, "");
        p.AddTextParameter("Alignment", "J", "left, center, right", GH_ParamAccess.item, "center");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        var f = Text(da, 4);
        var image = f.Length == 0 ? ImageBlock.Load(Text(da, 0), Num(da, 1), Text(da, 2)) : new ImageBlock(Text(da, 0), f, Num(da, 1), Alt: Text(da, 2));
        Block(da, image with
        {
            Caption = Text(da, 3).Length == 0 ? null : Text(da, 3),
            Alignment = Text(da, 5)
        });
    }
}
public sealed class EquationComponent : WordComponent
{
    public EquationComponent() : base("Word Equation", "Equation", "Formula LaTeX nel sottoinsieme documentato, convertita in OMML modificabile.", "Math") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("LaTeX", "L", "Formula", GH_ParamAccess.item);
        p.AddBooleanParameter("Inline", "I", "Formula inline", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Numbered", "N", "Aggiunge campo SEQ Equation", GH_ParamAccess.item, false);
        p.AddTextParameter("Bookmark", "B", "Segnalibro per REF opzionale", GH_ParamAccess.item, "");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        MathParser.Parse(Text(da, 0));
        Block(da, new Equation(Text(da, 0), Bool(da, 1), Text(da, 3).Length == 0 ? null : Text(da, 3), Bool(da, 2)));
    }
}
public sealed class FieldComponent : WordComponent
{
    public FieldComponent() : base("Word Field", "Field", "Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Instruction", "I", "Codice campo", GH_ParamAccess.item, "TOC \\o \"1-3\" \\h \\z \\u");
        p.AddTextParameter("Placeholder", "P", "Testo prima dell'aggiornamento", GH_ParamAccess.item, "Aggiornare il campo in Word");
        p.AddTextParameter("Style ID", "S", "Stile paragrafo", GH_ParamAccess.item, "Normal");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new Field(Text(da, 0), Text(da, 1), Text(da, 2)));
}
public sealed class BookmarkComponent : WordComponent
{
    public BookmarkComponent() : base("Bookmark", "Bookmark", "Segnalibro univoco intorno a una sequenza di blocchi.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Name", "N", "Lettera iniziale, max 40 caratteri alfanumerici/underscore", GH_ParamAccess.item);
        BlocksInput(p);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new Bookmark(Text(da, 0), List<ContentBlock>(da, 1)));
}
public sealed class BreakComponent : WordComponent
{
    public BreakComponent() : base("Page Break", "Break", "Interruzione di pagina.", "Layout") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new PageBreak());
}
public sealed class NoteComponent : WordComponent
{
    public NoteComponent() : base("Word Note", "Note", "Nota a piè di pagina o nota finale nativa.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Anchor", "A", "Testo nel corpo", GH_ParamAccess.item);
        p.AddTextParameter("Text", "T", "Testo nota", GH_ParamAccess.item);
        p.AddBooleanParameter("Endnote", "E", "Nota finale", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new Note(Text(da, 1), Text(da, 0), Bool(da, 2)));
}
public sealed class CommentComponent : WordComponent
{
    public CommentComponent() : base("Word Comment", "Comment", "Commento nativo associato al testo indicato.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Anchor", "A", "Testo nel corpo", GH_ParamAccess.item);
        p.AddTextParameter("Text", "T", "Commento", GH_ParamAccess.item);
        p.AddTextParameter("Author", "U", "Autore", GH_ParamAccess.item, "Pawper");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new Comment(Text(da, 0), Text(da, 1), Text(da, 2)));
}
public sealed class ControlComponent : WordComponent
{
    public ControlComponent() : base("Content Control", "Control", "Controllo contenuto Word a blocchi identificato da tag.", "Templates") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Tag", "T", "Identificatore per riempimento template", GH_ParamAccess.item);
        BlocksInput(p);
        p[1].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da) => Block(da, new ContentControl(Text(da, 0), List<ContentBlock>(da, 1)));
}
public sealed class TargetComponent : WordComponent
{
    public TargetComponent() : base("Template Target", "Target", "Definisce un riempimento di content control o segnalibro inline.", "Templates") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Name", "N", "Tag controllo o nome segnalibro", GH_ParamAccess.item);
        BlocksInput(p);
        p.AddBooleanParameter("Bookmark", "M", "True=segnalibro inline; False=content control", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new TargetParam(), "Target", "T", "Operazione di riempimento", GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da) => Out(da, 0, new TemplateTarget(Text(da, 0), List<ContentBlock>(da, 1), Bool(da, 2)));
}
public sealed class FillTemplateComponent : WordComponent
{
    public FillTemplateComponent() : base("Fill Template", "Fill", "Sostituisce placeholder anche divisi in run e riempie controlli/segnalibri.", "Templates") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        DocInput(p);
        p.AddTextParameter("Values", "V", "Oggetto JSON chiave:valore", GH_ParamAccess.item, "{}");
        p.AddParameter(new TargetParam(), "Targets", "T", "Riempimenti tipizzati", GH_ParamAccess.list);
        p.AddBooleanParameter("Strict", "S", "Errore per chiavi mancanti o non trovate", GH_ParamAccess.item, true);
        p[2].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => DocOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        var d = Val<Document>(da, 0);
        Out(da, 0, d with
        {
            Replacements = (d.Replacements ?? ImmutableDictionary<string, string>.Empty).SetItems(Json.Read<Dictionary<string, string>>(Text(da, 1))),
            Targets = (d.Targets.IsDefault ? ImmutableArray<TemplateTarget>.Empty : d.Targets).AddRange(List<TemplateTarget>(da, 2)),
            StrictPlaceholders = Bool(da, 3)
        });
    }
}
public sealed class RepeatComponent : WordComponent
{
    public RepeatComponent() : base("Repeat Blocks", "Repeat", "Ripete una sequenza sostituendo i placeholder per ogni record JSON.", "Templates") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        BlocksInput(p);
        p.AddTextParameter("Records", "R", "Array JSON di oggetti chiave:valore", GH_ParamAccess.item, "[]");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new BlockParam(), "Blocks", "B", "Blocchi nell'ordine record/blocco", GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da) => da.SetDataList(0, Composition.Repeat(List<ContentBlock>(da, 0), Json.Read<List<Dictionary<string, string>>>(Text(da, 1))).Select(x => new WordGoo<ContentBlock>(x)));
}
public sealed class ConditionalComponent : WordComponent
{
    public ConditionalComponent() : base("Conditional Blocks", "If", "Include i blocchi solo quando la condizione è vera.", "Templates") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddBooleanParameter("Condition", "C", "Condizione", GH_ParamAccess.item, false);
        BlocksInput(p);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new BlockParam(), "Blocks", "B", "Sequenza condizionale", GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da) => da.SetDataList(0, (Bool(da, 0) ? List<ContentBlock>(da, 1) : []).Select(x => new WordGoo<ContentBlock>(x)));
}
public sealed class MetadataComponent : WordComponent
{
    public MetadataComponent() : base("Document Metadata", "Metadata", "Proprietà standard e personalizzate del documento.", "Document") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        DocInput(p);
        p.AddTextParameter("Title", "T", "Titolo metadati", GH_ParamAccess.item, "");
        p.AddTextParameter("Author", "A", "Autore", GH_ParamAccess.item, "");
        p.AddTextParameter("Subject", "S", "Oggetto", GH_ParamAccess.item, "");
        p.AddTextParameter("Keywords", "K", "Parole chiave", GH_ParamAccess.item, "");
        p.AddTextParameter("Custom", "C", "JSON proprietà personalizzate testuali", GH_ParamAccess.item, "{}");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => DocOutput(p);
    protected override void Solve(IGH_DataAccess da) => Out(da, 0, Val<Document>(da, 0) with { Properties = new(Text(da, 1), Text(da, 2), Text(da, 3), Text(da, 4), Json.Read<Dictionary<string, string>>(Text(da, 5)).ToImmutableDictionary()) });
}
public sealed class ImportComponent : WordComponent
{
    public ImportComponent() : base("Import Content", "Import", "Importa blocchi DOCX selezionati; rimappa stili, elenchi, immagini e segnalibri. Vedere limiti nel manuale.", "Document") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Path", "P", "DOCX sorgente", GH_ParamAccess.item);
        p.AddIntegerParameter("Indices", "I", "Indici zero-based dei blocchi corpo, esclusa sectPr", GH_ParamAccess.list);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        var ids = new List<int>();
        da.GetDataList(1, ids);
        Block(da, new Import(Convert.ToBase64String(File.ReadAllBytes(Text(da, 0))), ids.ToImmutableArray()));
    }
}
public sealed class InspectorComponent : WordComponent
{
    public InspectorComponent() : base("Document Inspector", "Inspect", "Struttura, stili, metadati e validazione; non è un'anteprima impaginata.", "Output") { }
    protected override void RegisterInputParams(GH_InputParamManager p) => DocInput(p);
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Structure", "B", "Indice e contenuto dei blocchi", GH_ParamAccess.item);
        p.AddTextParameter("Styles", "S", "ID | nome visualizzato", GH_ParamAccess.list);
        p.AddTextParameter("Bookmarks", "M", "Nomi", GH_ParamAccess.list);
        p.AddTextParameter("Controls", "C", "Tag", GH_ParamAccess.list);
        p.AddTextParameter("Metadata", "P", "JSON metadati", GH_ParamAccess.item);
        p.AddTextParameter("Validation", "V", "Errori Open XML; vuoto=valido", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        var i = WordEngine.Inspect(Val<Document>(da, 0));
        da.SetData(0, i.Structure);
        da.SetDataList(1, i.Styles);
        da.SetDataList(2, i.Bookmarks);
        da.SetDataList(3, i.Controls);
        da.SetData(4, Json.Write(i.Properties));
        da.SetDataList(5, i.Validation);
    }
}
public abstract class TriggerComponent : WordComponent
{
    readonly RisingEdge edge = new(); bool fire;
    protected TriggerComponent(string name, string nick, string description, string category) : base(name, nick, description, category) { }
    protected override void BeforeSolveInstance()
    {
        fire = false;
    }
    protected bool Trigger(IGH_DataAccess da, int index)
    {
        if (da.Iteration == 0)
            fire = edge.Consume(Bool(da, index));
        return fire;
    }
    public override bool Read(GH_IReader reader)
    {
        edge.Disarm();
        return base.Read(reader);
    }
}
public sealed class SaveComponent : TriggerComponent
{
    public SaveComponent() : base("Save Word", "Save", "Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.", "Output") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        DocInput(p);
        p.AddTextParameter("Path", "P", "Destinazione DOCX", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "R", "Impulso esplicito False→True", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite", "O", "Sostituzione atomica di un file esistente", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Path", "P", "File scritto", GH_ParamAccess.item);
        p.AddTextParameter("Status", "S", "Esito", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        if (!Trigger(da, 2))
        {
            da.SetData(1, "In attesa di un nuovo impulso");
            return;
        }
        var result = Output.Save(Val<Document>(da, 0), Text(da, 1), Bool(da, 3));
        da.SetData(0, result.Path);
        da.SetData(1, result.Message);
    }
}
public sealed class BatchComponent : TriggerComponent
{
    public BatchComponent() : base("Save Word Batch", "Batch", "Ogni ramo contiene un documento e un percorso con lo stesso path; controlla collisioni prima della scrittura.", "Output") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddParameter(new DocumentParam(), "Documents", "D", "Un documento per ramo", GH_ParamAccess.tree);
        p.AddTextParameter("Paths", "P", "Un percorso per ramo con path corrispondente", GH_ParamAccess.tree);
        p.AddBooleanParameter("Run", "R", "Impulso False→True", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite", "O", "Sovrascrivi", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Results", "R", "Esiti per ramo originale", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        if (!Trigger(da, 2))
            return;
        if (!da.GetDataTree<WordGoo<Document>>(0, out var docs) || !da.GetDataTree<GH_String>(1, out var paths))
            return;
        if (docs.PathCount != paths.PathCount || !docs.Paths.Zip(paths.Paths).All(pair => pair.First.Indices.SequenceEqual(pair.Second.Indices)) || docs.Branches.Any(x => x.Count != 1) || paths.Branches.Any(x => x.Count != 1))
            throw new ArgumentException("Serve un documento e un percorso per ciascun ramo corrispondente. Documenti: " + string.Join(", ", docs.Paths) + "; percorsi: " + string.Join(", ", paths.Paths));
        var results = Output.Batch(docs.Branches.Select(x => x[0].Value).ToArray(), paths.Branches.Select(x => x[0].Value).ToArray(), Bool(da, 3));
        var tree = new GH_Structure<GH_String>();
        for (int i = 0; i < results.Length; i++)
            tree.Append(new GH_String(Json.Write(results[i])), docs.Paths[i]);
        da.SetDataTree(0, tree);
    }
}
public sealed class OpenResultComponent : TriggerComponent
{
    public OpenResultComponent() : base("Open Result", "Open", "Apre il DOCX nell'applicazione associata solo su comando esplicito.", "Output") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Path", "P", "DOCX esistente", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "R", "Impulso False→True", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Status", "S", "Esito", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        if (!Trigger(da, 1))
            return;
        string file = Path.GetFullPath(Text(da, 0));
        if (!File.Exists(file) || Path.GetExtension(file).ToLowerInvariant() != ".docx")
            throw new ArgumentException("Selezionare un DOCX esistente.");
        Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        da.SetData(0, "Documento aperto");
    }
}
public sealed class NumberComponent : WordComponent
{
    public NumberComponent() : base("Format Number", "Number", "Formatta un risultato con cultura, decimali e unità.", "Text") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddNumberParameter("Value", "V", "Valore finito", GH_ParamAccess.item);
        p.AddIntegerParameter("Decimals", "D", "0..15", GH_ParamAccess.item, 2);
        p.AddTextParameter("Unit", "U", "Unità", GH_ParamAccess.item, "");
        p.AddTextParameter("Culture", "C", "it-IT, en-US, ecc.", GH_ParamAccess.item, "it-IT");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Text", "T", "Numero formattato", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da) => da.SetData(0, Composition.Number(Num(da, 0), Int(da, 1, 2), Text(da, 2), Text(da, 3)));
}

