using System.Collections.Immutable;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Pawper.Core;

namespace Pawper.Grasshopper;

public sealed class QuickTableComponent : WordComponent
{
    public QuickTableComponent() : base("Quick Table", "QuickTable", "Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.", "Tables") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Data", "D", "Un ramo per riga; con Columns=True un ramo per colonna. I path mantengono l'ordine Grasshopper, senza Flatten.", GH_ParamAccess.tree);
        p.AddTextParameter("Headers", "H", "Intestazioni separate, una per colonna; vuoto=nessuna intestazione. La prima riga dei dati non viene consumata.", GH_ParamAccess.list);
        p.AddBooleanParameter("Columns", "C", "False: rami=righe. True: rami=colonne, utile per liste di risultati unite con Entwine.", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Decimals", "N", "-1=precisione originale; 0..15=decimali. Un valore per tutti i numeri oppure uno per colonna.", GH_ParamAccess.list, -1);
        p.AddTextParameter("Units", "U", "Unità applicate solo ai numeri. Una per tutte le colonne oppure una per colonna; stringa vuota=nessuna.", GH_ParamAccess.list);
        p.AddTextParameter("Culture", "L", "Cultura numerica, per esempio it-IT o en-US. I testi non vengono interpretati come numeri.", GH_ParamAccess.item, "it-IT");
        p.AddNumberParameter("Widths mm", "W", "Una larghezza per colonna; vuoto=uniforme in base alla pagina.", GH_ParamAccess.list);
        p.AddBooleanParameter("Strict", "S", "False: completa i rami corti con celle vuote. True: rifiuta lunghezze diverse.", GH_ParamAccess.item, false);
        p[0].Optional = p[1].Optional = p[4].Optional = p[6].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        BlockOutput(p);
        p.AddTextParameter("Rows", "R", "Anteprima testuale: un ramo per riga, inclusa l'intestazione.", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        da.GetDataTree<IGH_Goo>(0, out var tree);
        var headers = new List<string>(); var decimals = new List<int>(); var units = new List<string>(); var widths = new List<double>();
        da.GetDataList(1, headers); da.GetDataList(3, decimals); da.GetDataList(4, units); da.GetDataList(6, widths);
        var data = tree == null ? [] : tree.Branches.Select(b => b.Select(v => v?.ScriptVariable()).ToArray()).ToArray();
        var table = Tabular.FromData(data, headers, Bool(da, 2), decimals, units, Text(da, 5), widths, Bool(da, 7));
        // A whole input tree creates one block. Do not inherit a sparse source path,
        // which would reorder this block after paragraphs when Compose is flattened.
        var blocks = new GH_Structure<WordGoo<ContentBlock>>();
        blocks.Append(new WordGoo<ContentBlock>(table), new GH_Path(0));
        da.SetDataTree(0, blocks);
        da.SetDataTree(1, TablePreview.Rows(table));
        Message = $"{table.Rows.Length - (table.Header ? 1 : 0)} × {table.Rows[0].Length}";
        if (data.Select(b => b.Length).Distinct().Count() > 1)
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Rami di lunghezza diversa: le posizioni mancanti sono celle vuote. Strict=True le rifiuta.");
    }
}

public sealed class TextTableComponent : WordComponent
{
    public TextTableComponent() : base("Text Table", "TextTable", "Tabella intera da testo incollato da Excel (TAB), CSV o righe di un Panel. Gestisce virgolette e celle multilinea.", "Tables") { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Text", "T", "Testo multilinea oppure lista di righe nello stesso ramo. Per file usare Read File di Grasshopper. Nessun accesso automatico agli appunti.", GH_ParamAccess.list);
        p.AddTextParameter("Separator", "S", "auto, tab, comma, semicolon, pipe oppure un carattere. Auto usa il primo record; in caso di ambiguità scegliere esplicitamente.", GH_ParamAccess.item, "auto");
        p.AddBooleanParameter("Header", "H", "La prima riga è l'intestazione ripetuta sulle pagine successive.", GH_ParamAccess.item, true);
        p.AddNumberParameter("Widths mm", "W", "Una larghezza per colonna; vuoto=uniforme.", GH_ParamAccess.list);
        p[3].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        BlockOutput(p);
        p.AddTextParameter("Rows", "R", "Anteprima testuale, un ramo per riga inclusa l'intestazione; preserva celle vuote e zeri iniziali.", GH_ParamAccess.tree);
        p.AddTextParameter("Separator", "S", "Separatore effettivo, TAB per la tabulazione.", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        var text = new List<string>(); var widths = new List<double>();
        da.GetDataList(0, text); da.GetDataList(3, widths);
        var result = Tabular.FromText(string.Join("\n", text), Text(da, 1), Bool(da, 2), widths);
        Block(da, result.Table);
        da.SetDataTree(1, TablePreview.Rows(result.Table));
        da.SetData(2, result.Separator == "\t" ? "TAB" : result.Separator);
        Message = $"{result.Table.Rows.Length - (result.Table.Header ? 1 : 0)} × {result.Table.Rows[0].Length}";
        if (result.Rows.Select(r => r.Length).Distinct().Count() > 1)
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Le righe corte sono completate con celle vuote; le righe più larghe dell'intestazione sono rifiutate.");
    }
}

internal static class TablePreview
{
    internal static GH_Structure<GH_String> Rows(Table table)
    {
        var tree = new GH_Structure<GH_String>();
        for (int r = 0; r < table.Rows.Length; r++)
            foreach (var cell in table.Rows[r])
                tree.Append(new GH_String(string.Concat(cell.Content.OfType<Paragraph>().SelectMany(p => p.Runs).Select(t => t.Text))), new GH_Path(r));
        return tree;
    }
}
