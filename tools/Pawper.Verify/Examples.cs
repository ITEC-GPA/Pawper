using Pawper.Samples;
using System.Collections.Immutable;
using Pawper.Core;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using W = DocumentFormat.OpenXml.Wordprocessing;
namespace Pawper.Verify;
public record Example(int Id, string Slug, string Title, string Purpose, Document Document, string[] Features);
public static class Examples
{
    public static string Root = "";
    public static ImmutableArray<ContentBlock> B(params ContentBlock[] b) => b.ToImmutableArray();
    public static Paragraph P(string t, string style = "Normal") => Paragraph.Text(t, style);
    static Document D(params ContentBlock[] b) => new(B(b));
    static Table SimpleTable() => TableSamples.RowTable();
    static Image Picture(string format = "png") => Image.Load(Path.Combine(Root, "assets", "diagram." + format), 120, "Schema di una trave appoggiata con carico distribuito") with { Caption = "Schema geometrico dimostrativo" };
    public static List<Example> Create(string root)
    {
        Root = root;
        Directory.CreateDirectory(Path.Combine(root, "assets"));
        var all = new List<Example>();
        void Add(int id, string slug, string title, string purpose, Document doc, params string[] features) => all.Add(new(id, slug, title, purpose, doc, features));
        Add(1, "primo-documento", "Primo documento", "Creare, comporre e salvare un documento senza Word installato.", D(P("Primo documento", "Title"), P("Questo documento è stato generato con Pawper. Il testo rimane modificabile in Microsoft Word.")), "NewDocumentComponent", "ComposeComponent", "SaveComponent", "OpenResultComponent", "InspectorComponent", "RelativePathComponent");
        Add(2, "testo-formattato", "Testo formattato", "Combinare frammenti con proprietà tipografiche diverse.", D(P("Testo e formattazione", "Title"), new Paragraph([new("Normale "), new("grassetto ", new(Bold: true)), new("corsivo ", new(Italic: true)), new("sottolineato ", new(Underline: true)), new("colore ", new(Color: "24445C")), new("x"), new("2", new(Script: "superscript")), new(" H"), new("2", new(Script: "subscript")), new("O")]), new Paragraph([new("Paragrafo con rientro e spaziatura.\nSeconda riga con un'interruzione esplicita.")], Format: new("both", Before: 6, After: 12, LeftMm: 5, FirstLineMm: 5)), P("Testo piccolo e grande"), new Paragraph([new("Aptos 10 ", new("Aptos", 10)), new("Arial 16", new("Arial", 16))])), "TextRunComponent", "ParagraphComponent");
        var custom = new Style("ProjectBody", "Testo progetto", Run: new("Aptos", 11, Color: "24445C"), Paragraph: new(After: 8));
        Add(3, "stili-e-titoli", "Stili e titoli", "Usare identificatori indipendenti dalla lingua di Word e tutti i livelli di titolo.", new Document(B(P("Stili del progetto", "Title"), P("Sottotitolo della relazione", "Subtitle"), P("Questo paragrafo applica ProjectBody.", "ProjectBody")).AddRange(Enumerable.Range(1, 9).Select(i => P("Intestazione livello " + i, "Heading" + i))), Styles: [custom]), "StyleComponent", "HeadingComponent");
        Add(4, "elenchi", "Elenchi multilivello", "Numerare voci, annidare livelli, usare punti e riavviare un elenco.", D(P("Elenchi", "Title"), new Paragraph([new("Prima voce")], ListLevel: 0), new Paragraph([new("Sottovoce")], ListLevel: 1), new Paragraph([new("Dettaglio")], ListLevel: 2), new Paragraph([new("Seconda voce")], ListLevel: 0), new Paragraph([new("Punto principale")], ListLevel: 0, Bullet: true), new Paragraph([new("Punto secondario")], ListLevel: 1, Bullet: true), new Paragraph([new("Nuovo elenco")], ListLevel: 0, ListKey: "second")), "ListComponent");
        Add(5, "tabella-numerica", "Tabelle intere da dati e testo", "Creare tabelle con un solo componente da righe, colonne, testo Excel e CSV.", D(
            P("Tabelle intere da dati e testo", "Title"), P("Quattro modi per trasformare dati tabellari in tabelle Word modificabili. I valori mancanti restano vuoti e i codici mantengono gli zeri iniziali."),
            P("Dati organizzati per righe", "Heading1"), SimpleTable(),
            P("Dati organizzati per colonne", "Heading1"), TableSamples.ColumnTable(),
            new PageBreak(), P("Testo copiato da Excel", "Heading1"), TableSamples.TextWithHeader(),
            P("CSV senza intestazione", "Heading1"), TableSamples.TextWithoutHeader(),
            P("Italiano: " + Composition.Number(1234.567, 2, "kN", "it-IT")), P("English: " + Composition.Number(1234.567, 3, "kN", "en-US"))),
            "QuickTableComponent", "TextTableComponent", "NumberComponent", "quick-table-rows", "quick-table-columns", "quick-table-numeric-culture-units", "quick-table-no-header-strict", "table-missing-values", "table-preview", "text-table-tsv-auto", "text-table-csv-explicit-no-header", "text-table-quoted-multiline", "text-table-leading-zeros");
        string tableFolder = Path.Combine(root, "05-tabella-numerica");
        Directory.CreateDirectory(tableFolder);
        File.WriteAllText(Path.Combine(tableFolder, "dati.tsv"), TableSamples.Tsv);
        File.WriteAllText(Path.Combine(tableFolder, "dati.csv"), TableSamples.Csv);
        Add(6, "celle-e-unioni", "Celle e unioni", "Unire righe e colonne e inserire contenuti formattati nelle celle.", D(P("Tabella composta", "Title"), new Table([[new Cell(B(P("Verifiche")), 2), Cell.Text("Esito")], [new Cell(B(P("Gruppo A")), RowSpan: 2), Cell.Text("Resistenza"), new Cell(B(P("OK")), Fill: "DDEEDD", Alignment: "center")], [Cell.Text("Deformazione"), new Cell(B(new Paragraph([new("ATTENZIONE", new(Bold: true))])), Fill: "FFF0CC")], [new Cell(B(P("Schema")), 2), new Cell(B(Picture() with { WidthMm = 35, Caption = null }))]], [40, 65, 50])), "CellComponent", "table-rowspan", "table-colspan", "conditional-fill", "rich-cells");
        Add(7, "immagini-e-viste", "Immagini e viste Rhino", "Incorporare PNG e JPEG, mantenere le proporzioni e acquisire viste attive o nominate.", D(P("Immagini del progetto", "Title"), Picture(), Picture("jpg") with
        {
            WidthMm = 80,
            Caption = "Versione JPEG",
            Alignment = "left"
        }), "ImageComponent", "CaptureViewComponent", "image-base64", "image-file", "image-alt", "image-caption");
        Add(8, "equazioni-base", "Equazioni modificabili", "Scrivere formule inline e separate con frazioni, radici, indici ed esponenti.", D(P("Equazioni native", "Title"), new Paragraph([new("La relazione "), new(Math: @"\sigma=\frac{N}{A}"), new(" è una formula inline modificabile.")]), new Equation(@"x_1^2+\sqrt[3]{a}=\frac{b}{c}"), new Equation(@"\alpha+\beta+\gamma=\pi", Inline: true)), "EquationComponent", "equation-inline", "equation-fraction", "equation-root", "equation-scripts", "equation-greek");
        Add(9, "equazioni-avanzate", "Equazioni avanzate", "Sommatorie, integrali, prodotti, matrici e riferimenti alle equazioni.", D(P("Formule strutturate", "Title"), new Equation(@"\sum_{i=1}^{n}{x_i}", BookmarkName: "EqSum", Numbered: true), new Equation(@"\int_0^L{q(x)}\,dx"), new Equation(@"\prod_{i=1}^{n}{a_i}"), new Equation(@"A=\begin{pmatrix}a&b\\c&d\end{pmatrix}"), new Equation(@"B=\begin{bmatrix}1&0\\0&1\end{bmatrix}"), new Equation(@"C=\begin{matrix}1&2\\3&4\end{matrix}"), new Field("REF EqSum \\h", "Riferimento da aggiornare")), "equation-sum", "equation-integral", "equation-matrix", "equation-numbering", "FieldComponent");
        Add(10, "pagine-e-sezioni", "Pagine e sezioni", "Gestire pagina iniziale, pari/dispari, numeri pagina e orientamenti diversi.", D(P("Relazione con sezioni", "Title"), P("Prima pagina con intestazione dedicata."), new PageBreak(), P("Seconda pagina", "Heading1"), P("Verifica intestazioni pari e numerazione."), new Section(new(210, 297, 20, true, Header: "Sezione orizzontale", Footer: "Pagina {{PAGE}}", PageStart: 1), B(P("Tabella orizzontale", "Heading1"), SimpleTable()))) with
        {
            Layout = new(210, 297, 20, Header: "Intestazione dispari", Footer: "Pagina {{PAGE}} di {{NUMPAGES}}", FirstHeader: "Copertina", FirstFooter: "", EvenHeader: "Intestazione pari", EvenFooter: "Pagina {{PAGE}}")
        }, "LayoutComponent", "SectionComponent", "BreakComponent", "layout-first-even", "page-fields");
        Add(11, "sommari-e-didascalie", "Sommari e didascalie", "Creare campi Word per sommario ed elenchi figure/tabelle, da aggiornare in Word.", D(P("Indici del documento", "Title"), new Field("TOC \\o \"1-3\" \\h \\z \\u"), P("Figure", "Heading1"), new Field("TOC \\h \\z \\c \"Figure\""), P("Tabelle", "Heading1"), new Field("TOC \\h \\z \\c \"Table\""), new PageBreak(), P("Risultati", "Heading1"), Picture() with
        {
            WidthMm = 90
        }, new Paragraph([new("Tabella "), new("1", Field: "SEQ Table \\* ARABIC"), new(" — Dati di ingresso")], "Caption"), SimpleTable()), "field-toc", "field-list-figures", "field-list-tables", "field-caption");
        Add(12, "link-e-segnalibri", "Link e segnalibri", "Collegamenti esterni e interni e riferimenti al testo o alla pagina.", D(P("Navigazione", "Title"), new Paragraph([new("Documentazione Rhino", Link: "https://developer.rhino3d.com/")]), new Paragraph([new("Vai ai risultati", Link: "#Risultati")]), new Bookmark("Risultati", B(P("Risultati verificati", "Heading1"))), new Field("REF Risultati \\h"), new Field("PAGEREF Risultati \\h")), "BookmarkComponent", "hyperlink-external", "hyperlink-internal", "field-ref", "field-pageref");
        Add(13, "metadati", "Metadati", "Impostare proprietà standard e personalizzate e ispezionare il documento.", D(P("Proprietà del progetto", "Title"), P("Le proprietà sono memorizzate nel pacchetto DOCX.")) with
        {
            Properties = new("Scheda progetto", "Studio dimostrativo", "Documentazione parametrica", "Pawper; report", new Dictionary<string, string> { { "Codice", "PRJ-001" }, { "Revisione", "A" } }.ToImmutableDictionary())
        }, "MetadataComponent", "metadata-custom");
        var template = D(P("Template dimostrativo", "Title"), new Paragraph([new("Progetto: {{Pro", new(Bold: true)), new("getto}}", new(Italic: true))]), P("Cliente: {{Cliente}}"), P("Ripetizione: {{Progetto}}")) with
        {
            Styles = [custom],
            Layout = new(Header: "Progetto {{Progetto}}", Footer: "Cliente {{Cliente}}")
        };
        var templateBytes = WordEngine.Build(template);
        File.WriteAllBytes(Path.Combine(root, "assets", "template.docx"), templateBytes);
        Add(14, "template-testuale", "Template con placeholder", "Sostituire token divisi tra run nel corpo, nelle intestazioni e nei piè di pagina.", new Document([], Convert.ToBase64String(templateBytes), Replacements: new Dictionary<string, string> { { "Progetto", "Edificio dimostrativo" }, { "Cliente", "Cliente esempio" } }.ToImmutableDictionary()).Append(B(P("Aggiunta con stile del template", "ProjectBody"))), "FillTemplateComponent", "template-docx", "placeholder-split", "placeholder-repeated", "template-style", "template-header-footer");
        var control = D(P("Template strutturato", "Title"), new ContentControl("Contenuti", B(P("Da riempire"))), new Bookmark("Cliente", B(P("Nome cliente"))));
        var controlBytes = WordEngine.Build(control);
        using (var ms = new MemoryStream())
        {
            ms.Write(controlBytes);
            ms.Position = 0;
            using (var doc = WordprocessingDocument.Open(ms, true))
            {
                doc.ChangeDocumentType(WordprocessingDocumentType.Template);
            }
            File.WriteAllBytes(Path.Combine(root, "assets", "template.dotx"), ms.ToArray());
        }
        var dotx = File.ReadAllBytes(Path.Combine(root, "assets", "template.dotx"));
        Add(15, "template-strutturato", "Content control e segnalibri", "Riempire un DOTX con blocchi diversi e sostituire un segnalibro inline.", new([], Convert.ToBase64String(dotx), Targets: [new("Contenuti", B(P("Dati del progetto", "Heading1"), SimpleTable(), Picture() with { WidthMm = 80 })), new("Cliente", B(P("Cliente dimostrativo")), true)]), "ControlComponent", "TargetComponent", "template-dotx", "target-blocks", "target-bookmark");
        var repeat = Composition.Repeat(B(P("Elemento {{Id}}", "Heading2"), P("Lunghezza {{L}} m")), new[] { new Dictionary<string, string> { { "Id", "A" }, { "L", "4.50" } }, new Dictionary<string, string> { { "Id", "B" }, { "L", "6.00" } } });
        Add(16, "ripetizioni-e-condizioni", "Ripetizioni e condizioni", "Ripetere contenuti per record e includere sezioni solo quando richiesto.", D(P("Schede degli elementi", "Title")).Append(repeat).Append(B(P("Nota inclusa perché la condizione è vera."))), "RepeatComponent", "ConditionalComponent");
        Add(17, "note-e-commenti", "Note e commenti", "Note a piè di pagina, note finali e commenti modificabili.", D(P("Annotazioni", "Title"), new Note("Nota a piè di pagina dimostrativa.", "Dato con approfondimento"), new Note("Nota finale dimostrativa.", "Metodo di riferimento", true), new Comment("Testo da verificare", "Confermare questo dato prima della consegna.", "Revisore")), "NoteComponent", "CommentComponent", "note-foot", "note-end");
        var importDoc = D(P("Documento sorgente", "Heading1"), P("Stile da importare", "ProjectBody"), new Paragraph([new("Elenco sorgente")], ListLevel: 0), Picture() with
        {
            WidthMm = 70
        }, new Bookmark("Origine", B(P("Segnalibro sorgente"))), new Field("REF Origine \\h")) with
        {
            Styles = [custom]
        };
        var importBytes = WordEngine.Build(importDoc);
        File.WriteAllBytes(Path.Combine(root, "assets", "source.docx"), importBytes);
        Add(18, "importazione", "Importazione selettiva", "Importare due volte gli stessi blocchi rimappando stili, numerazioni, risorse e segnalibri.", D(P("Composizione da più documenti", "Title"), new Import(Convert.ToBase64String(importBytes), [0, 1, 2, 3, 4, 5, 6]), new Import(Convert.ToBase64String(importBytes), [1, 2, 3])), "ImportComponent", "import-styles", "import-numbering", "import-images", "import-bookmarks");
        Add(19, "batch", "Generazione batch", "Un documento per ramo del Data Tree, con percorsi distinti e controllo delle collisioni.", D(P("Report batch", "Title"), P("Questo è il documento base. La definizione produce tre documenti con nomi distinti.")), "BatchComponent", "batch-paths", "branch-isolation", "save-trigger");
        Add(20, "relazione-completa", "Relazione tecnica completa", "Combinare copertina, sommario, dati, immagini, equazioni, note e sezioni.", D(P("Relazione tecnica dimostrativa", "Title"), P("Progetto di una trave appoggiata", "Subtitle"), P("Esempio completo di documentazione parametrica generata da Grasshopper."), new PageBreak(), P("Sommario", "Heading1"), new Field("TOC \\o \"1-3\" \\h \\z \\u"), P("Dati del modello", "Heading1"), SimpleTable(), P("Schema e risultati", "Heading1"), Picture(), new Equation(@"M_{max}=\frac{qL^2}{8}", BookmarkName: "Momento", Numbered: true), P("La formula e i dati hanno finalità dimostrative e non costituiscono una verifica strutturale."), new Note("Il modello rappresenta una trave semplicemente appoggiata.", "Ipotesi di modello"), new Section(new(210, 297, 20, true, Header: "Appendice", Footer: "Pagina {{PAGE}}"), B(P("Appendice tabellare", "Heading1"), SimpleTable()))) with
        {
            Layout = new(210, 297, 20, Header: "Pawper — Relazione tecnica", Footer: "Pagina {{PAGE}} di {{NUMPAGES}}"),
            Properties = new("Relazione tecnica dimostrativa", "Pawper", "Esempio completo", "relazione;Grasshopper")
        }, "integrated-report");
        all[2] = all[2] with
        {
            Document = all[2].Document.Append(B(new Paragraph([new TextRun("Stile carattere Evidenza", StyleId: "EmphasisProject")]), SimpleTable() with
            {
                StyleId = "ProjectTable"
            })) with
            {
                Styles = [custom, new Style("EmphasisProject", "Evidenza", Run: new(Italic: true), Type: "character"), new Style("ProjectTable", "Tabella progetto", Run: new(Font: "Aptos", Size: 11), Type: "table")]
            },
            Features = all[2].Features.Concat(new[] { "style-character", "style-table" }).ToArray()
        };
        return all;
    }
}


