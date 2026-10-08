using System.Drawing;
using System.Drawing.Imaging;
using Grasshopper.Kernel;
using Pawper.Core;
using Rhino;
using Rhino.Display;
namespace Pawper.Grasshopper;
public sealed class RelativePathComponent : WordComponent
{
    public RelativePathComponent() : base("Project Path", "Path", "Risolve un percorso rispetto al file .gh salvato, per esempi portabili.", "Document") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Relative path", "R", "Percorso relativo alla definizione", GH_ParamAccess.item, "output.docx");
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Full path", "P", "Percorso assoluto risolto", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        var file = OnPingDocument()?.FilePath;
        if (string.IsNullOrEmpty(file))
            throw new ArgumentException("Salvare prima la definizione Grasshopper.");
        da.SetData(0, Path.GetFullPath(Text(da, 0), Path.GetDirectoryName(file)!));
    }
}
public sealed class CaptureViewComponent : TriggerComponent
{
    Pawper.Core.Image? cached;
    public CaptureViewComponent() : base("Capture Rhino View", "Capture", "Acquisisce la vista attiva o nominata su impulso; ripristina la vista originale.", "Images") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Named view", "N", "Vuoto=vista attiva; altrimenti nome della vista salvata", GH_ParamAccess.item, "");
        p.AddIntegerParameter("Width pixels", "W", "Larghezza 64..8192", GH_ParamAccess.item, 1600);
        p.AddIntegerParameter("Height pixels", "H", "Altezza 64..8192", GH_ParamAccess.item, 1000);
        p.AddBooleanParameter("Transparent", "T", "Sfondo trasparente", GH_ParamAccess.item, false);
        p.AddNumberParameter("Width mm", "M", "Larghezza in Word", GH_ParamAccess.item, 150);
        p.AddBooleanParameter("Capture", "C", "Impulso False→True", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => BlockOutput(p);
    protected override void Solve(IGH_DataAccess da)
    {
        if (Trigger(da, 5))
        {
            if (global::Grasshopper.Instances.RunningHeadless)
                throw new InvalidOperationException("La cattura richiede una sessione grafica di Rhino con una vista visibile. L'esecuzione headless non dispone di un contesto grafico affidabile.");
            int width = Int(da, 1), height = Int(da, 2);
            if (width is < 64 or > 8192 || height is < 64 or > 8192)
                throw new ArgumentException("Risoluzione tra 64 e 8192 pixel.");
            string name = Text(da, 0);
            bool transparent = Bool(da, 3);
            double mm = Num(da, 4);
            Exception? failure = null;
            RhinoApp.InvokeOnUiThread((Action)(() =>
            {
                try
                {
                    var doc = RhinoDoc.ActiveDoc ?? throw new InvalidOperationException("Nessun documento Rhino attivo.");
                    var view = doc.Views.ActiveView ?? throw new InvalidOperationException("Nessuna vista attiva.");
                    using var original = new Rhino.DocObjects.ViewportInfo(view.ActiveViewport);
                    try
                    {
                        if (name.Length > 0)
                        {
                            int index = doc.NamedViews.FindByName(name);
                            if (index < 0)
                                throw new ArgumentException("Vista nominata non trovata: " + name);
                            doc.NamedViews.Restore(index, view.ActiveViewport);
                        }
                        var capture = new ViewCapture { Width = width, Height = height, TransparentBackground = transparent, DrawAxes = false, DrawGrid = false, DrawGridAxes = false, ScaleScreenItems = false };
                        using var bitmap = capture.CaptureToBitmap(view) ?? throw new InvalidOperationException("Acquisizione vista non disponibile.");
                        using var memory = new MemoryStream();
                        bitmap.Save(memory, ImageFormat.Png);
                        cached = new Pawper.Core.Image(Convert.ToBase64String(memory.ToArray()), WidthMm: mm, Alt: name.Length == 0 ? "Vista Rhino attiva" : name);
                    }
                    finally { view.ActiveViewport.SetViewProjection(original, false); view.Redraw(); }
                }
                catch (Exception ex) { failure = ex; }
            }));
            if (failure != null)
                throw failure;
        }
        if (cached != null)
            Block(da, cached);
        else
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Premere Capture per acquisire una vista.");
    }
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
        if (cached != null)
            writer.SetString("CachedImage", Json.Write(cached));
        return base.Write(writer);
    }
    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
        if (reader.ItemExists("CachedImage"))
            cached = Json.Read<Pawper.Core.Image>(reader.GetString("CachedImage"));
        return base.Read(reader);
    }
}
public sealed class TableRowsComponent : WordComponent
{
    public TableRowsComponent() : base("Table Rows", "Rows", "Organizza una lista ordinata di celle in righe con lunghezze esplicite.", "Tables") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddParameter(new CellParam(), "Cells", "C", "Celle nell'ordine riga/colonna", GH_ParamAccess.list);
        p.AddIntegerParameter("Counts", "N", "Celle esplicite per riga, escluse quelle coperte da unioni verticali", GH_ParamAccess.list);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddParameter(new CellParam(), "Rows", "R", "Un ramo per riga", GH_ParamAccess.tree);
    protected override void Solve(IGH_DataAccess da)
    {
        var cells = List<Cell>(da, 0);
        var counts = new List<int>();
        da.GetDataList(1, counts);
        if (counts.Any(n => n < 0) || counts.Sum() != cells.Length)
            throw new ArgumentException("La somma dei conteggi deve coincidere con le celle.");
        var tree = new global::Grasshopper.Kernel.Data.GH_Structure<WordGoo<Cell>>();
        int index = 0;
        for (int row = 0; row < counts.Count; row++)
        {
            var path = new global::Grasshopper.Kernel.Data.GH_Path(row);
            tree.EnsurePath(path);
            for (int i = 0; i < counts[row]; i++)
                tree.Append(new WordGoo<Cell>(cells[index++]), path);
        }
        da.SetDataTree(0, tree);
    }
}
