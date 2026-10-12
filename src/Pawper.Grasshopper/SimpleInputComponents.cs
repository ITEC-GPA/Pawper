using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Pawper.Core;
using Document = Pawper.Core.Document;

namespace Pawper.Grasshopper;

public sealed class TextBlocksComponent : WordComponent
{
    public TextBlocksComponent() : base("Text Blocks", "TextBlocks", "Titoli, paragrafi, elenchi e formule da un testo unico. Doppio clic per aprire l'editor; sottoinsieme documentato, non Markdown completo.", "Text") { }
    public override void CreateAttributes() => m_attributes = new TextEditorAttributes(this, TextInputEditor.BlockHelp);
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) { base.AppendAdditionalComponentMenuItems(menu); TextInputEditor.Append(menu, this, TextInputEditor.BlockHelp); }
    protected override void RegisterInputParams(GH_InputParamManager p) => p.AddTextParameter("Text", "T", "Testo multilinea o lista di righe nello stesso ramo. # titolo, **grassetto**, *corsivo*, - elenco, $formula$, $$formula$$, --- nuova pagina.", GH_ParamAccess.list, "Scrivi qui il testo.");
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new BlockParam(), "Blocks", "B", "Blocchi ordinati, uno per paragrafo/titolo/elemento/formula.", GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da)
    {
        var lines = new List<string>(); da.GetDataList(0, lines);
        var blocks = StructuredText.Parse(string.Join("\n", lines), InstanceGuid.ToString("N") + "/" + da.Iteration);
        da.SetDataList(0, blocks.Select(b => new WordGoo<ContentBlock>(b)));
        Message = blocks.Length + " blocchi";
    }
}

public sealed class QuickDocumentComponent : WordComponent
{
    public QuickDocumentComponent() : base("Quick Document", "QuickDoc", "Crea un documento con titolo, blocchi ordinati e impostazioni opzionali. Il salvataggio resta esplicito in Save Word.", "Document") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Title", "T", "Titolo iniziale; vuoto=nessun titolo aggiunto.", GH_ParamAccess.item, "");
        BlocksInput(p);
        p.AddParameter(new LayoutParam(), "Layout", "L", "Impostazioni pagina opzionali; collegare Page Preset.", GH_ParamAccess.item);
        p.AddTextParameter("Template path", "P", "DOCX/DOTX opzionale; titolo e blocchi vengono aggiunti in fondo alla copia.", GH_ParamAccess.item, "");
        p.AddParameter(new StyleParam(), "Styles", "S", "Stili aggiuntivi; gli ID del template sono disponibili senza collegamenti.", GH_ParamAccess.list);
        Optional(p);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => DocOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        string path = Text(da, 3);
        Out(da, 0, SimpleInputs.CreateDocument(Text(da, 0), List<ContentBlock>(da, 1), string.IsNullOrWhiteSpace(path) ? null : Document.Load(path), Opt<PageLayout>(da, 2), List<Style>(da, 4)));
    }
}

public sealed class FillFieldsComponent : WordComponent
{
    public FillFieldsComponent() : base("Fill Fields", "FillFields", "Compila placeholder con due liste parallele di nomi e valori, senza JSON. Mantiene la formattazione del primo run del token.", "Templates") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        DocInput(p);
        p.AddTextParameter("Names", "N", "Nomi sensibili alle maiuscole, con o senza {{parentesi}}. Vietati duplicati.", GH_ParamAccess.list);
        p.AddTextParameter("Values", "V", "Un valore testuale per nome. Usare Format Number per numeri e unità. Vuoto=stringa vuota.", GH_ParamAccess.list);
        p.AddBooleanParameter("Strict", "S", "True: segnala placeholder senza valore e nomi non presenti, alla validazione/scrittura. False: lascia i token non risolti.", GH_ParamAccess.item, true);
        p[1].Optional = p[2].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => DocOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        var names = new List<string>(); var values = new List<string>(); da.GetDataList(1, names); da.GetDataList(2, values);
        Out(da, 0, SimpleInputs.Fill(Val<Document>(da, 0), names, values, Bool(da, 3)));
    }
}

public sealed class ParagraphFormatComponent : WordComponent
{
    public ParagraphFormatComponent() : base("Paragraph Format", "ParaFormat", "Impostazioni del paragrafo con input diretti. Collegare Format a Paragraph oppure a Paragraph format di Word Style.", "Styles") { }
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) { base.AppendAdditionalComponentMenuItems(menu); TextInputEditor.Choices(menu, this, 0, ["left", "center", "right", "both"]); }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Alignment", "A", "left, center, right, both (giustificato); anche dal menu del componente.", GH_ParamAccess.item, "left");
        p.AddNumberParameter("Before pt", "B", "Spazio prima, punti.", GH_ParamAccess.item, 0);
        p.AddNumberParameter("After pt", "E", "Spazio dopo, punti.", GH_ParamAccess.item, 6);
        p.AddNumberParameter("Line", "L", "Interlinea: 1=singola, 1.5=una e mezza.", GH_ParamAccess.item, 1.15);
        p.AddBooleanParameter("Keep next", "K", "Mantieni insieme al paragrafo successivo.", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddTextParameter("Format", "F", "Formato serializzato compatibile con Paragraph e Word Style; non occorre modificarlo manualmente.", GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da) => da.SetData(0, Json.Write(SimpleInputs.ParagraphSettings(Text(da, 0), Num(da, 1), Num(da, 2), Num(da, 3), Bool(da, 4))));
}

public sealed class TableFormatComponent : WordComponent
{
    public TableFormatComponent() : base("Table Format", "TableFormat", "Colori e margini di una tabella completa. Preserva contenuti, celle unite e larghezze; restituisce una copia.", "Tables") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddParameter(new BlockParam(), "Table", "T", "Tabella prodotta da Quick Table, Text Table o Word Table.", GH_ParamAccess.item);
        p.AddColourParameter("Header", "H", "Sfondo intestazione; testo bianco/nero scelto per contrasto.", GH_ParamAccess.item, Color.FromArgb(36, 68, 92));
        p.AddColourParameter("Stripe", "S", "Sfondo righe alternate.", GH_ParamAccess.item, Color.FromArgb(238, 243, 246));
        p.AddColourParameter("Border", "B", "Colore bordi.", GH_ParamAccess.item, Color.FromArgb(203, 213, 221));
        p.AddNumberParameter("Padding mm", "P", "Margini interni delle celle, da 0 a 50 mm.", GH_ParamAccess.item, 2);
        p.AddTextParameter("Style ID", "I", "ID stile tabella opzionale; vuoto mantiene quello corrente. I colori diretti prevalgono sullo stile.", GH_ParamAccess.item, "");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        string ColorAt(int i) { var color = Color.Empty; if (!da.GetData(i, ref color) || color.A != 255) throw new ArgumentException("Usare colori opachi (Alpha=255)."); return $"{color.R:X2}{color.G:X2}{color.B:X2}"; }
        Block(da, SimpleInputs.FormatTable(Val<ContentBlock>(da, 0), ColorAt(1), ColorAt(2), ColorAt(3), Num(da, 4), Text(da, 5)));
    }
}

public sealed class PagePresetComponent : WordComponent
{
    public PagePresetComponent() : base("Page Preset", "PagePreset", "Pagina A4, A3, Letter o Legal con margini e intestazioni. Per impostazioni di sezione avanzate usare Page Layout.", "Layout") { }
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) { base.AppendAdditionalComponentMenuItems(menu); TextInputEditor.Choices(menu, this, 0, ["A4", "A3", "Letter", "Legal"]); }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Format", "F", "A4, A3, Letter o Legal; selezionabile anche dal menu del componente.", GH_ParamAccess.item, "A4");
        p.AddBooleanParameter("Landscape", "L", "Orientamento orizzontale.", GH_ParamAccess.item, false);
        p.AddNumberParameter("Margin mm", "M", "Margine uniforme su tutti i lati.", GH_ParamAccess.item, 20);
        p.AddTextParameter("Header", "H", "Testo intestazione.", GH_ParamAccess.item, "");
        p.AddTextParameter("Footer", "P", "Piè di pagina; {{PAGE}} e {{NUMPAGES}} creano campi da aggiornare in Word.", GH_ParamAccess.item, "");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new LayoutParam(), "Layout", "L", "Collegare a Quick Document, Compose o Word Section.", GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da) => Out(da, 0, SimpleInputs.Page(Text(da, 0), Bool(da, 1), Num(da, 2), Text(da, 3), Text(da, 4)));
}

internal static class TextInputEditor
{
    internal const string BlockHelp = "# Titolo · **grassetto** · *corsivo* · - elenco · 1. numerato · due spazi per sottolivello\r\nRiga vuota = nuovo paragrafo · $formula$ inline · $$formula$$ separata · --- nuova pagina · \\* e \\$ letterali";
    internal const string TableHelp = "Incolla da Excel (TAB) oppure CSV. Una riga per record; virgolette doppie per celle con separatori o ritorni a capo.\r\nSeparator=auto oppure scegli il separatore nell'input. Header=True usa la prima riga come intestazione.";
    internal static bool CanEdit(GH_Component component, int input = 0) => component.Params.Input[input] is Param_String p && p.SourceCount == 0 && p.PersistentData.PathCount <= 1;
    internal static void Append(ToolStripDropDown menu, GH_Component component, string help)
    {
        var item = new ToolStripMenuItem("Modifica testo…", null, (_, _) => Edit(component, help)) { Enabled = CanEdit(component), ToolTipText = "Disponibile se Text non è collegato e contiene al massimo un ramo. Annulla mantiene il testo precedente." };
        menu.Items.Add(item);
    }
    internal static void Choices(ToolStripDropDown menu, GH_Component c, int input, string[] choices)
    {
        var parent = new ToolStripMenuItem(c.Params.Input[input].Name) { Enabled = CanEdit(c, input) };
        string current = ((Param_String)c.Params.Input[input]).PersistentData.AllData(true).OfType<GH_String>().FirstOrDefault()?.Value ?? "";
        foreach (string value in choices)
            parent.DropDownItems.Add(new ToolStripMenuItem(value, null, (_, _) => { c.RecordUndoEvent("Imposta " + value); var parameter = (Param_String)c.Params.Input[input]; parameter.PersistentData.Clear(); parameter.SetPersistentData(new GH_String(value)); c.ExpireSolution(true); }) { Checked = string.Equals(current, value, StringComparison.OrdinalIgnoreCase) });
        menu.Items.Add(parent);
    }
    internal static void Edit(GH_Component c, string help)
    {
        if (!CanEdit(c)) return;
        var p = (Param_String)c.Params.Input[0];
        using var form = CreateDialog(c.Name, string.Join("\n", p.PersistentData.AllData(true).OfType<GH_String>().Select(t => t.Value)), help);
        if (form.ShowDialog() != DialogResult.OK) return;
        Apply(c, form.Controls.OfType<TextBox>().Single().Text);
    }
    internal static void Apply(GH_Component c, string text)
    {
        if (!CanEdit(c)) throw new InvalidOperationException("Modificare la sorgente collegata o i singoli rami dell'input.");
        c.RecordUndoEvent("Modifica testo Pawper");
        var parameter = (Param_String)c.Params.Input[0]; parameter.PersistentData.Clear(); parameter.SetPersistentData(new GH_String(text)); c.ExpireSolution(true);
    }
    internal static Form CreateDialog(string name, string text, string help)
    {
        var form = new Form { Text = "Pawper — " + name, Width = 820, Height = 590, MinimumSize = new Size(600, 400), StartPosition = FormStartPosition.CenterScreen, ShowInTaskbar = false };
        var font = new Font("Consolas", 11);
        form.Disposed += (_, _) => font.Dispose();
        var editor = new TextBox { Multiline = true, AcceptsReturn = true, AcceptsTab = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill, Font = font, Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n") };
        var hint = new Label { Text = help, Dock = DockStyle.Top, Height = 64, Padding = new Padding(10), AutoEllipsis = true };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
        var ok = new Button { Text = "Applica", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel); buttons.Controls.Add(ok); form.Controls.Add(editor); form.Controls.Add(hint); form.Controls.Add(buttons); form.AcceptButton = ok; form.CancelButton = cancel;
        return form;
    }
}
internal sealed class TextEditorAttributes(GH_Component owner, string help) : GH_ComponentAttributes(owner)
{
    public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (e.Button == MouseButtons.Left && Bounds.Contains(e.CanvasLocation) && TextInputEditor.CanEdit((GH_Component)Owner)) { TextInputEditor.Edit((GH_Component)Owner, help); return GH_ObjectResponse.Handled; }
        return base.RespondToMouseDoubleClick(sender, e);
    }
}
