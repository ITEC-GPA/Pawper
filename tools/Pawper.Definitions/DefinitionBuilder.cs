using Pawper.Samples;
using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Text;
using global::Grasshopper;
using global::Grasshopper.Kernel;
using global::Grasshopper.Kernel.Data;
using global::Grasshopper.Kernel.Parameters;
using global::Grasshopper.Kernel.Types;
using GH_IO.Serialization;
using Pawper.Core;
using Pawper.Grasshopper;
using Document = Pawper.Core.Document;
using ImageBlock = Pawper.Core.Image;
namespace Pawper.Definitions;
public sealed class DefinitionBuilder
{
    GH_Document graph = new(); readonly List<GH_Component> nodes = []; readonly List<string> instructions = [];
    readonly Dictionary<int, int> positions = new(); string folder = ""; int id; int tableIndex;
    T Add<T>(T c, int column = 1) where T : GH_Component
    {
        c.CreateAttributes();
        int row = positions.GetValueOrDefault(column);
        positions[column] = row + 1;
        c.Attributes.Pivot = new PointF(150 + column * 330, 120 + row * 210);
        c.NickName = $"{nodes.Count + 1:00} {c.NickName}";
        graph.AddObject(c, false);
        nodes.Add(c);
        return c;
    }
    static void Set(GH_Component c, int index, object? value)
    {
        switch (c.Params.Input[index])
        {
            case Param_String p:
                p.PersistentData.Clear();
                p.PersistentData.Append(new GH_String(value?.ToString() ?? ""));
                break;
            case Param_Boolean p:
                p.PersistentData.Clear();
                p.PersistentData.Append(new GH_Boolean(Convert.ToBoolean(value)));
                break;
            case Param_Integer p:
                p.PersistentData.Clear();
                p.PersistentData.Append(new GH_Integer(Convert.ToInt32(value)));
                break;
            case Param_Number p:
                p.PersistentData.Clear();
                p.PersistentData.Append(new GH_Number(Convert.ToDouble(value)));
                break;
            default:
                throw new ArgumentException("Tipo input non gestito: " + c.Params.Input[index].Name);
        }
    }
    static void Link(GH_Component from, GH_Component to, int input, int output = 0) => to.Params.Input[input].AddSource(from.Params.Output[output]);
    RelativePathComponent PathNode(string relative)
    {
        var c = Add(new RelativePathComponent(), 0);
        Set(c, 0, relative);
        return c;
    }
    LayoutComponent Layout(PageLayout p)
    {
        var c = Add(new LayoutComponent(), 2);
        Set(c, 6, Json.Write(p));
        return c;
    }
    GH_Component Run(TextRun run)
    {
        var c = Add(new TextRunComponent(), 0);
        var f = run.Format ?? new();
        Set(c, 0, run.Text);
        Set(c, 1, f.Font ?? "Aptos");
        Set(c, 2, f.Size ?? 11);
        Set(c, 3, f.Bold);
        Set(c, 4, f.Italic);
        Set(c, 5, f.Underline);
        Set(c, 6, f.Color ?? "000000");
        Set(c, 7, f.Script ?? "baseline");
        Set(c, 8, run.Link ?? "");
        Set(c, 9, run.Math ?? "");
        Set(c, 10, run.Field ?? "");
        Set(c, 11, run.StyleId ?? "");
        return c;
    }
    QuickTableComponent QuickTable(object?[][] data, string[] headers, bool columns = false, bool strict = false,
        int[]? decimals = null, string[]? units = null, string culture = "it-IT", double[]? widths = null)
    {
        var c = Add(new QuickTableComponent(), 2);
        var values = (Param_GenericObject)c.Params.Input[0];
        // Deliberately sparse paths verify that order is path order, not contiguous indices.
        for (int r = 0; r < data.Length; r++)
        {
            var path = new GH_Path(2, 10 + r * 3);
            values.PersistentData.EnsurePath(path);
            foreach (var value in data[r])
            {
                IGH_Goo goo = value switch
                {
                    null => null!,
                    double n => new GH_Number(n),
                    int n => new GH_Integer(n),
                    _ => new GH_String(value.ToString())
                };
                values.PersistentData.Append(goo, path);
            }
        }
        foreach (var name in headers) ((Param_String)c.Params.Input[1]).PersistentData.Append(new GH_String(name));
        Set(c, 2, columns);
        var digits = (Param_Integer)c.Params.Input[3]; digits.PersistentData.Clear();
        foreach (var value in decimals ?? [2]) digits.PersistentData.Append(new GH_Integer(value));
        foreach (var unit in units ?? []) ((Param_String)c.Params.Input[4]).PersistentData.Append(new GH_String(unit));
        Set(c, 5, culture);
        foreach (var width in widths ?? TableSamples.Widths) ((Param_Number)c.Params.Input[6]).PersistentData.Append(new GH_Number(width));
        Set(c, 7, strict);
        return c;
    }
    TextTableComponent TextTable(string text, bool header, string separator, double[] widths)
    {
        var c = Add(new TextTableComponent(), 2);
        // Also exercise a multiline Panel's list-of-lines representation.
        var input = (Param_String)c.Params.Input[0]; input.PersistentData.Clear();
        foreach (var line in text.Split('\n')) input.PersistentData.Append(new GH_String(line));
        Set(c, 1, separator); Set(c, 2, header);
        foreach (var width in widths) ((Param_Number)c.Params.Input[3]).PersistentData.Append(new GH_Number(width));
        return c;
    }
    GH_Component BuildBlock(ContentBlock block)
    {
        switch (block)
        {
            case Paragraph p:
                if (p.ListLevel != null)
                {
                    var c = Add(new ListComponent());
                    Set(c, 0, string.Concat(p.Runs.Select(r => r.Text)));
                    Set(c, 1, p.ListLevel);
                    Set(c, 2, p.Bullet);
                    Set(c, 3, p.ListKey);
                    return c;
                }
                if (p.Runs.Length == 1 && p.Runs[0].Format == null && (p.StyleId is "Title" or "Subtitle" || p.StyleId.StartsWith("Heading")))
                {
                    var c = Add(new HeadingComponent());
                    Set(c, 0, p.Runs[0].Text);
                    Set(c, 1, p.StyleId == "Title" ? 0 : p.StyleId == "Subtitle" ? -1 : int.Parse(p.StyleId[7..]));
                    return c;
                }
                var para = Add(new ParagraphComponent());
                Set(para, 2, p.StyleId);
                if (p.Format != null)
                    Set(para, 3, Json.Write(p.Format));
                if (p.Runs.Length == 1 && p.Runs[0].Format == null && p.Runs[0].Link == null && p.Runs[0].Math == null && p.Runs[0].Field == null && p.Runs[0].StyleId == null)
                    Set(para, 0, p.Runs[0].Text);
                else
                    foreach (var run in p.Runs)
                        Link(Run(run), para, 1);
                return para;
            case Table t:
                if (id == 5)
                    return tableIndex++ switch
                    {
                        0 => QuickTable(TableSamples.Rows, TableSamples.Headers),
                        1 => QuickTable(TableSamples.Columns, TableSamples.ColumnHeaders, columns: true, decimals: [-1, 2, 3], units: ["", "kN", "m"], culture: "en-US"),
                        2 => TextTable(TableSamples.Tsv, true, "auto", [25, 90, 40]),
                        3 => TextTable(TableSamples.Csv, false, ";", [25, 90, 40]),
                        _ => throw new InvalidOperationException("Tabella dimostrativa non prevista.")
                    };
                if (id == 20)
                    return QuickTable(TableSamples.Rows, TableSamples.Headers);
                var rows = Add(new TableRowsComponent(), 2);
                var counts = (Param_Integer)rows.Params.Input[1];
                counts.PersistentData.Clear();
                foreach (var row in t.Rows)
                {
                    counts.PersistentData.Append(new GH_Integer(row.Length));
                    foreach (var cell in row)
                    {
                        var c = Add(new CellComponent());
                        Set(c, 2, cell.ColumnSpan);
                        Set(c, 3, cell.RowSpan);
                        Set(c, 4, cell.Fill ?? "");
                        Set(c, 5, cell.Alignment);
                        if (cell.Content.Length == 1 && cell.Content[0] is Paragraph cp && cp.Runs.Length == 1 && cp.Runs[0].Format == null)
                            Set(c, 0, cp.Runs[0].Text);
                        else
                            foreach (var b in cell.Content)
                                Link(BuildBlock(b), c, 1);
                        Link(c, rows, 0);
                    }
                }
                var table = Add(new TableComponent(), 3);
                Link(rows, table, 0);
                var widths = (Param_Number)table.Params.Input[1];
                if (!t.WidthsMm.IsDefault)
                    foreach (var width in t.WidthsMm)
                        widths.PersistentData.Append(new GH_Number(width));
                Set(table, 2, t.Header);
                Set(table, 3, Json.Write(new TableOptions(t.HeaderFill, t.StripeFill, t.BorderColor, t.PaddingMm, t.StyleId)));
                return table;
            case ImageBlock i:
                var image = Add(new ImageComponent());
                Set(image, 0, i.DataBase64);
                Set(image, 1, i.WidthMm);
                Set(image, 2, i.Alt);
                Set(image, 3, i.Caption ?? "");
                Set(image, 4, i.Format);
                Set(image, 5, i.Alignment);
                return image;
            case Equation e:
                var eq = Add(new EquationComponent());
                Set(eq, 0, e.Latex);
                Set(eq, 1, e.Inline);
                Set(eq, 2, e.Numbered);
                Set(eq, 3, e.BookmarkName ?? "");
                return eq;
            case Field f:
                var field = Add(new FieldComponent());
                Set(field, 0, f.Instruction);
                Set(field, 1, f.Placeholder);
                Set(field, 2, f.StyleId);
                return field;
            case PageBreak:
                return Add(new BreakComponent());
            case Section s:
                var section = Add(new SectionComponent(), 4);
                Link(Layout(s.Layout), section, 0);
                foreach (var b in s.Content)
                    Link(BuildBlock(b), section, 1);
                return section;
            case Bookmark b:
                var bm = Add(new BookmarkComponent(), 3);
                Set(bm, 0, b.Name);
                foreach (var x in b.Content)
                    Link(BuildBlock(x), bm, 1);
                return bm;
            case ContentControl c:
                var control = Add(new ControlComponent(), 3);
                Set(control, 0, c.Tag);
                foreach (var x in c.Content)
                    Link(BuildBlock(x), control, 1);
                return control;
            case Note n:
                var note = Add(new NoteComponent());
                Set(note, 0, n.Anchor);
                Set(note, 1, n.Text);
                Set(note, 2, n.Endnote);
                return note;
            case Comment c:
                var comment = Add(new CommentComponent());
                Set(comment, 0, c.Anchor);
                Set(comment, 1, c.Text);
                Set(comment, 2, c.Author);
                return comment;
            case Import i:
                var import = Add(new ImportComponent());
                Link(PathNode("../assets/source.docx"), import, 0);
                var indices = (Param_Integer)import.Params.Input[1];
                foreach (int index in i.BodyIndices)
                    indices.PersistentData.Append(new GH_Integer(index));
                return import;
            default:
                throw new NotSupportedException(block.GetType().Name);
        }
    }
    GH_Component BuildDocument(Document spec)
    {
        var create = Add(new NewDocumentComponent(), 4);
        if (spec.SourceBase64 != null)
            Link(PathNode(id == 15 ? "../assets/template.dotx" : "../assets/template.docx"), create, 0);
        var compose = Add(new ComposeComponent(), 5);
        compose.Params.Input[1].DataMapping = GH_DataMapping.Flatten;
        Link(create, compose, 0);
        if (id == 16)
        {
            Link(BuildBlock(Paragraph.Text("Schede degli elementi", "Title")), compose, 1);
            var repeat = Add(new RepeatComponent(), 3);
            Link(BuildBlock(Paragraph.Text("Elemento {{Id}}", "Heading2")), repeat, 0);
            Link(BuildBlock(Paragraph.Text("Lunghezza {{L}} m")), repeat, 0);
            Set(repeat, 1, "[{\"Id\":\"A\",\"L\":\"4.50\"},{\"Id\":\"B\",\"L\":\"6.00\"}]");
            Link(repeat, compose, 1);
            var conditional = Add(new ConditionalComponent(), 3);
            Set(conditional, 0, true);
            Link(BuildBlock(Paragraph.Text("Nota inclusa perché la condizione è vera.")), conditional, 1);
            Link(conditional, compose, 1);
            var falseCase = Add(new ConditionalComponent(), 3);
            Set(falseCase, 0, false);
            Link(BuildBlock(Paragraph.Text("Questa frase non deve comparire.")), falseCase, 1);
            Link(falseCase, compose, 1);
        }
        else
            foreach (var block in spec.Blocks)
                Link(BuildBlock(block), compose, 1);
        if (!spec.Styles.IsDefault)
            foreach (var style in spec.Styles)
            {
                var s = Add(new StyleComponent(), 3);
                Set(s, 0, style.Id);
                Set(s, 1, style.Name);
                Set(s, 2, style.BasedOn);
                Set(s, 3, Json.Write(style.Run ?? new()));
                Set(s, 4, Json.Write(style.Paragraph ?? new()));
                Set(s, 5, style.Type);
                Link(s, compose, 2);
            }
        if (spec.Layout != null)
            Link(Layout(spec.Layout), compose, 3);
        GH_Component current = compose;
        if (spec.Replacements != null || !spec.Targets.IsDefaultOrEmpty)
        {
            var fill = Add(new FillTemplateComponent(), 6);
            Link(current, fill, 0);
            Set(fill, 1, Json.Write(spec.Replacements ?? ImmutableDictionary<string, string>.Empty));
            Set(fill, 3, spec.StrictPlaceholders);
            if (!spec.Targets.IsDefault)
                foreach (var t in spec.Targets)
                {
                    var target = Add(new TargetComponent(), 5);
                    Set(target, 0, t.Name);
                    Set(target, 2, t.Bookmark);
                    foreach (var b in t.Content)
                        Link(BuildBlock(b), target, 1);
                    Link(target, fill, 2);
                }
            current = fill;
        }
        if (spec.Properties != null)
        {
            var m = spec.Properties;
            var metadata = Add(new MetadataComponent(), 6);
            Link(current, metadata, 0);
            Set(metadata, 1, m.Title);
            Set(metadata, 2, m.Author);
            Set(metadata, 3, m.Subject);
            Set(metadata, 4, m.Keywords);
            Set(metadata, 5, Json.Write(m.Custom ?? ImmutableDictionary<string, string>.Empty));
            current = metadata;
        }
        return current;
    }
    void Extras(GH_Component doc)
    {
        if (id == 5)
        {
            QuickTable([[1.0, 2.0], [3.0, 4.0]], [], strict: true, decimals: [1], units: ["mm"], widths: []);
            instructions.Add("Percorso rapido: collegare il Block di Quick Table o Text Table a Blocks di Compose. Non servono Table Cell o Table Rows per queste tabelle.");
            instructions.Add("Quick Table per righe: Data contiene {2;10} = [Luce, 6.5, m], {2;13} = [Carico, 12.3456, kN/m], {2;16} = [Dato mancante, null]. Headers = Elemento, Valore, Unità; Decimals = 2. I numeri sono allineati a destra. Le celle mancanti restano vuote.");
            instructions.Add("Quick Table per colonne: collegare tre liste ai tre ingressi di Entwine, poi Entwine a Data e Columns=True. Le liste sono [Trave A, Trave B, Mancante], [1234.567, 28.9], [4.1234, 3.8]. Headers = Elemento, Forza, Lunghezza; Decimals = [-1,2,3], Units = [vuoto,kN,m], Culture=en-US. Il file incluso memorizza già questi rami, senza richiedere altri componenti. Verificare 1,234.57 kN e 4.123 m nella prima riga.");
            instructions.Add("Il terzo Quick Table, scollegato da Compose, dimostra Headers vuoto, Widths vuoto, unità e decimali comuni a tutte le colonne, Strict=True. Mostra una matrice 2×2 con valori in mm. Collegarlo a Compose per includerlo; accorciare una riga per verificare il messaggio di errore.");
            instructions.Add("Text Table: copiare il contenuto di dati.tsv in un Panel e collegarlo a Text; Separator=auto e Header=True. È supportato sia un unico testo multilinea sia un Panel con una lista di righe. Rows permette di controllare che 001 conservi gli zeri e che Prima riga/seconda riga sia una sola cella. Per leggere il file usare Read File di Grasshopper con il percorso relativo risolto da Project Path.");
            instructions.Add("Secondo Text Table: dati.csv usa Separator=; e Header=False. La riga A è una riga di dati; il punto e virgola tra virgolette resta nella seconda cella. Si possono scegliere anche comma, tab e pipe. In auto un pareggio tra separatori produce un errore: scegliere quello effettivo del file.");
            instructions.Add("Controlli ed errori: collegare Rows a un Panel per vedere la griglia; non applicare Flatten a Data. Un numero di Headers inferiore alle colonne è un errore. Strict=False completa righe o colonne corte; Strict=True le rifiuta. I testi, compresi 001 e 12,50 incollati, non vengono convertiti in numeri. Per controllare i decimali fornire numeri reali a Quick Table. In Text Table chiudere le virgolette e raddoppiare quelle interne.");
        }
        if (id == 5)
            foreach (var culture in new[] { "it-IT", "en-US" })
            {
                var number = Add(new NumberComponent(), 0);
                Set(number, 0, 1234.567);
                Set(number, 1, culture == "it-IT" ? 2 : 3);
                Set(number, 2, "kN");
                Set(number, 3, culture);
                var p = Add(new ParagraphComponent());
                Link(number, p, 0);
            }
        if (id == 7)
        {
            var capture = Add(new CaptureViewComponent(), 0);
            capture.Locked = true;
            instructions.Add("Aprire il file incluso model.3dm in una finestra Rhino visibile. Sbloccare Capture Rhino View e portare Capture da False a True. Named view vuoto usa la vista attiva; per una vista nominata salvarla prima in Rhino e inserire il suo nome. Collegare l'output a Compose. L'acquisizione interattiva resta da collaudare manualmente; il rifiuto della cattura in modalità headless è stato verificato.");
            var fileImage = Add(new ImageComponent());
            Link(PathNode("../assets/diagram.png"), fileImage, 0);
            Set(fileImage, 2, "Schema da file");
        }
        if (id == 15)
        {
            var control = Add(new ControlComponent(), 3);
            Set(control, 0, "NuovoControllo");
            Link(BuildBlock(Paragraph.Text("Contenuto iniziale del controllo")), control, 1);
            instructions.Add("Content Control dimostra la creazione di un template. Il DOTX incluso contiene già Contenuti e il segnalibro Cliente.");
        }
        if (id == 19)
        {
            var batch = Add(new BatchComponent(), 8);
            var docs = new DocumentParam();
            docs.CreateAttributes();
            docs.Attributes.Pivot = new PointF(2150, 750);
            docs.NickName = "Tre documenti per ramo";
            var paths = PathNode("batch-live-1.docx");
            var values = (Param_String)paths.Params.Input[0];
            values.PersistentData.Clear();
            for (int k = 1; k <= 3; k++)
            {
                docs.PersistentData.Append(new WordGoo<Document>(Document.Empty.Append([Paragraph.Text("Report batch", "Title"), Paragraph.Text("Documento " + k)])), new GH_Path(k));
                values.PersistentData.Append(new GH_String($"batch-live-{k}.docx"), new GH_Path(k));
            }
            graph.AddObject(docs, false);
            batch.Params.Input[0].AddSource(docs);
            Link(paths, batch, 1);
            Set(batch, 2, false);
            instructions.Add("Batch: Documents e Paths hanno un elemento nei rami {1}, {2}, {3}. Run False→True genera tre file batch-live accanto alla definizione. Path discordanti o duplicati causano un errore prima della scrittura.");
        }
    }
    public static int Run(string root)
    {
        var server = Instances.ComponentServer;
        var load = typeof(GH_ComponentServer).GetMethod("LoadGHA", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
        var plugin = typeof(NewDocumentComponent).Assembly.Location;
        Console.WriteLine("Registering " + plugin);
        if (!(bool)load.Invoke(server, [new GH_ExternalFile(plugin), false])!)
            throw new InvalidOperationException("Caricamento Pawper fallito.");
        BrandingChecks.Run(root);
        var catalog = Json.Read<List<Catalog>>(File.ReadAllText(Path.Combine(root, "catalog.json")));
        var report = new List<object>();
        var coverage = new Dictionary<string, List<int>>();
        foreach (var entry in catalog)
        {
            var b = new DefinitionBuilder { id = entry.Id, folder = Path.Combine(root, $"{entry.Id:00}-{entry.Slug}") };
            var spec = Json.Read<Document>(File.ReadAllText(Path.Combine(b.folder, "document.json")));
            b.graph.FilePath = Path.Combine(b.folder, "example.gh");
            var output = b.BuildDocument(spec);
            b.Extras(output);
            var inspector = b.Add(new InspectorComponent(), 7);
            Link(output, inspector, 0);
            var save = b.Add(new SaveComponent(), 8);
            Link(output, save, 0);
            Link(b.PathNode("output.docx"), save, 1);
            Set(save, 2, false);
            var open = b.Add(new OpenResultComponent(), 9);
            Link(b.PathNode("output.docx"), open, 0);
            Set(open, 1, false);
            GH_Document.EnableSolutions = true;
            b.graph.Enabled = true;
            b.graph.NewSolution(false, GH_SolutionMode.Silent);
            var errors = b.nodes.SelectMany(c => c.RuntimeMessages(GH_RuntimeMessageLevel.Error).Select(e => c.Name + ": " + e)).ToArray();
            if (errors.Length > 0)
                throw new Exception($"Esempio {entry.Id}: " + string.Join("; ", errors));
            var goo = output.Params.Output[0].VolatileData.AllData(true).OfType<WordGoo<Document>>().FirstOrDefault() ?? throw new Exception("Nessun documento dall'esempio " + entry.Id);
            if (entry.Id is 5 or 20 && WordEngine.Inspect(goo.Value).Structure != WordEngine.Inspect(spec).Structure)
            {
                File.WriteAllText(Path.Combine(root, "../qa/table-actual.json"), Json.Write(goo.Value));
                File.WriteAllText(Path.Combine(root, "../qa/table-expected.json"), Json.Write(spec));
                throw new Exception("Le tabelle Grasshopper non coincidono con l'esempio del motore: " + entry.Id);
            }
            var bytes = WordEngine.Build(goo.Value);
            var validation = WordEngine.Validate(bytes);
            if (validation.Length > 0)
                throw new Exception(string.Join("\n", validation));
            File.WriteAllBytes(Path.Combine(b.folder, "grasshopper-verified.docx"), bytes);
            Console.WriteLine($"GH {entry.Id:00}: archive write");
            var archive = new GH_Archive();
            if (!archive.AppendObject(b.graph, "Definition") || !archive.WriteToFile(b.graph.FilePath, true, false))
                throw new Exception("Salvataggio GH fallito");
            Console.WriteLine($"GH {entry.Id:00}: archive read");
            var read = new GH_Archive();
            var reopened = new GH_Document();
            if (!read.ReadFromFile(b.graph.FilePath) || !read.ExtractObject(reopened, "Definition"))
                throw new Exception("Riapertura GH fallita");
            reopened.FilePath = b.graph.FilePath;
            reopened.Enabled = true;
            Console.WriteLine($"GH {entry.Id:00}: reopened solve");
            reopened.NewSolution(false, GH_SolutionMode.Silent);
            Console.WriteLine($"GH {entry.Id:00}: reopened solved");
            var reopenErrors = reopened.Objects.OfType<GH_Component>().SelectMany(c => c.RuntimeMessages(GH_RuntimeMessageLevel.Error)).ToArray();
            if (reopenErrors.Length > 0)
                throw new Exception("Errori dopo riapertura: " + string.Join("; ", reopenErrors));
            if (reopened.ObjectCount != b.graph.ObjectCount)
                throw new Exception("Oggetti persi durante la riapertura");
            var reopenedOutput = reopened.Objects.OfType<GH_Component>().Single(c => c.InstanceGuid == output.InstanceGuid);
            var reopenedValue = reopenedOutput.Params.Output[0].VolatileData.AllData(true).OfType<WordGoo<Document>>().Single().Value;
            Console.WriteLine($"GH {entry.Id:00}: reopened compare");
            if (WordEngine.Inspect(reopenedValue).Structure != WordEngine.Inspect(goo.Value).Structure)
            {
                File.WriteAllText(Path.Combine(root, "../qa/table-reopened.json"), Json.Write(reopenedValue));
                File.WriteAllText(Path.Combine(root, "../qa/table-before-reopen.json"), Json.Write(goo.Value));
                throw new Exception("Contenuto cambiato dopo riapertura: " + entry.Id);
            }
            if (entry.Id == 5)
            {
                var quick = reopened.Objects.OfType<QuickTableComponent>().First();
                Set(quick, 7, true);
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                if (!quick.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any()) throw new Exception("Strict non rifiuta i rami corti in Grasshopper");
                Set(quick, 7, false);
                var textTable = reopened.Objects.OfType<TextTableComponent>().First();
                var input = (Param_String)textTable.Params.Input[0];
                var saved = input.PersistentData.Duplicate();
                Set(textTable, 0, "A;B\n1;\"non chiuso"); Set(textTable, 1, ";");
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                if (!textTable.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any()) throw new Exception("CSV malformato non rifiutato in Grasshopper");
                input.PersistentData.Clear(); input.PersistentData.MergeStructure(saved); Set(textTable, 1, "auto");
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                if (quick.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any() || textTable.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any()) throw new Exception("Ripristino input non riuscito");
            }
            if (entry.Id == 1)
            {
                var testSave = reopened.Objects.OfType<SaveComponent>().Single();
                testSave.Params.Input[1].RemoveAllSources();
                string testFile = Path.GetFullPath(Path.Combine(root, "../qa/integration-save.docx"));
                Set(testSave, 1, testFile);
                Set(testSave, 3, true);
                Set(testSave, 2, false);
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                Set(testSave, 2, true);
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                if (!File.Exists(testFile))
                    throw new Exception("Save Word non ha scritto al fronte del trigger");
                var before = File.GetLastWriteTimeUtc(testFile);
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                if (File.GetLastWriteTimeUtc(testFile) != before)
                    throw new Exception("Save Word ha riscritto con trigger True costante");
            }
            if (entry.Id == 19)
            {
                var testBatch = reopened.Objects.OfType<BatchComponent>().Single();
                Set(testBatch, 3, true);
                Set(testBatch, 2, false);
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                Set(testBatch, 2, true);
                reopened.NewSolution(true, GH_SolutionMode.Silent);
                var results = testBatch.Params.Output[0].VolatileData.AllData(true).ToArray();
                if (results.Length != 3 || results.Any(x => !Json.Read<SaveResult>(x.ToString()!).Success))
                    throw new Exception("Batch Grasshopper non riuscito: " + string.Join("; ", testBatch.RuntimeMessages(GH_RuntimeMessageLevel.Error)) + " Results: " + string.Join("; ", results.Select(x => x.ToString())) + " Inputs: " + string.Join(" / ", testBatch.Params.Input.Select(x => x.VolatileData.PathCount + ":" + x.VolatileData.DataCount)));
            }
            foreach (var type in b.nodes.Select(n => n.GetType().Name).Distinct())
            {
                if (!coverage.ContainsKey(type))
                    coverage[type] = [];
                coverage[type].Add(entry.Id);
            }
            b.WriteGuide(entry);
            report.Add(new
            {
                entry.Id,
                Objects = b.graph.ObjectCount,
                Components = b.nodes.Count,
                Reopened = true,
                ContentPreserved = true,
                OpenXmlErrors = validation.Length
            });
            Console.WriteLine($"GH OK {entry.Id:00}: {b.graph.ObjectCount} objects");
            reopened.Dispose();
            b.graph.Dispose();
        }
        var implemented = typeof(NewDocumentComponent).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(WordComponent).IsAssignableFrom(t)).Select(t => t.Name).Order().ToArray();
        var missing = implemented.Where(t => !coverage.ContainsKey(t)).ToArray();
        if (missing.Length > 0)
            throw new Exception("Componenti senza esempi: " + string.Join(",", missing));
        File.WriteAllText(Path.Combine(root, "grasshopper-verification.json"), Json.Write(report));
        var matrix = new StringBuilder("# Matrice di copertura\n\nTutti i componenti pubblici compaiono nelle definizioni indicate. La cattura Rhino richiede verifica interattiva; è presente disabilitata nell'esempio 07.\n\n| Componente | Esempi |\n|---|---|\n");
        foreach (var type in implemented)
            matrix.AppendLine($"| {type} | {string.Join(", ", coverage[type].Select(i => i.ToString("00")))} |");
        matrix.AppendLine("\n| Funzionalità | Esempio |\n|---|---|");
        foreach (var e in catalog)
            foreach (var f in e.Features.Where(f => !f.EndsWith("Component")))
                matrix.AppendLine($"| {f} | {e.Id:00} |");
        File.WriteAllText(Path.Combine(root, "COVERAGE.md"), matrix.ToString());
        return 0;
    }
    void WriteGuide(Catalog entry)
    {
        var text = new StringBuilder($"# {entry.Id:00} {entry.Title}\n\n{entry.Purpose}\n\n## Prerequisiti e file\n\nRhino 8 con .NET 8 e Pawper installato. Conservare la cartella dentro examples, accanto ad assets.\n\n- `expected.docx`: risultato del motore indipendente.\n- `grasshopper-verified.docx`: risultato dei componenti della definizione.\n- `document.json`: specifica riproducibile.\n- `example.gh`: definizione binaria salvata, riaperta e ricalcolata in Rhino/Grasshopper.\n\n## Procedura\n\n1. Aprire example.gh e ricalcolare.\n2. Leggere Document Inspector: Validation deve essere vuoto.\n3. Modificare i valori degli input con il menu contestuale. I valori avanzati JSON sono già impostati.\n4. In Save Word portare Run su False e poi True. output.docx sarà creato accanto alla definizione. Per sostituire un file esistente abilitare Overwrite.\n5. Ricalcolare con Run ancora True: non deve esserci una nuova scrittura.\n6. In Open Result portare Run su False e poi True.\n7. In Word aggiornare i campi con F9.\n\n");
        foreach (var note in instructions)
            text.AppendLine(note + "\n");
        text.AppendLine("## Componenti e collegamenti\n\nI numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Blocks di Compose usa esplicitamente Flatten per comporre un solo documento da tutte le sorgenti; rimuoverlo per elaborare rami separati.\n");
        foreach (var c in nodes)
        {
            text.AppendLine($"### {c.NickName} — {c.Name}\n\n{c.Description}\n");
            foreach (var p in c.Params.Input)
            {
                if (p.SourceCount > 0)
                    text.AppendLine($"- **{p.Name}** ← " + string.Join(", ", p.Sources.Select(s => s.Attributes.GetTopLevel.DocObject.NickName + " / " + s.Name)));
                else
                {
                    string v = p switch
                    {
                        Param_String x => string.Join(" | ", x.PersistentData.AllData(true).Select(t => t.ToString())),
                        Param_Number x => string.Join(", ", x.PersistentData.AllData(true).Select(t => t.ToString())),
                        Param_Integer x => string.Join(", ", x.PersistentData.AllData(true).Select(t => t.ToString())),
                        Param_Boolean x => string.Join(", ", x.PersistentData.AllData(true).Select(t => t.ToString())),
                        Param_GenericObject x => string.Join("; ", x.PersistentData.Paths.Select(path => path + " = [" + string.Join(", ", x.PersistentData.get_Branch(path).Cast<IGH_Goo>().Select(g => g?.ToString() ?? "null")) + "]")),
                        _ => "opzionale / vuoto"
                    };
                    if (v.Length > 1200)
                        v = "Immagine Base64 incorporata; sostituibile tramite Image con un percorso.";
                    text.AppendLine($"- **{p.Name}** ({p.Access}): `{v.Replace("`", "'").Replace("\n", " ").Replace("\r", "")}`");
                }
            }
            text.AppendLine();
        }
        text.AppendLine("## Risultato e verifiche\n\nConfrontare il risultato con expected.docx e grasshopper-verified.docx. I documenti devono aprirsi senza riparazione. Testi, tabelle e formule sono modificabili; le immagini sono incorporate. I grafici sono immagini.\n\n## Errori comuni\n\n- Risorsa mancante: ripristinare examples/assets o correggere Project Path.\n- Stile mancante: usare l'ID di Inspector, non il nome tradotto.\n- File già esistente/bloccato: cambiare percorso o chiudere Word e abilitare Overwrite.\n- Nessun salvataggio: riportare Run a False prima di True.\n- Tabella errata: Quick Table usa righe o colonne secondo Columns; non appiattire Data. Word Table e Table Rows restano disponibili per celle complesse.\n- Formula rifiutata: consultare il sottoinsieme LaTeX nel manuale.\n");
        File.WriteAllText(Path.Combine(folder, "README.md"), text.ToString());
    }
    public record Catalog(int Id, string Slug, string Title, string Purpose, string[] Features);
}





