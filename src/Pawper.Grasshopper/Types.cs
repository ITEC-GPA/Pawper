using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Pawper.Core;

namespace Pawper.Grasshopper;
internal static class Identity
{
    public static Guid Id(string name) => StableIds.Values.TryGetValue(name, out var id)
        ? id : new(MD5.HashData(Encoding.UTF8.GetBytes("Pawper.0.1/" + name)));
}
public sealed class WordGoo<T> : GH_Goo<T> where T : class
{
    public WordGoo()
    {
    }
    public WordGoo(T value) => Value = value;
    public override bool IsValid => Value != null;
    public override string TypeName => "Word " + typeof(T).Name;
    public override string TypeDescription => "Valore documentale immutabile Pawper";
    public override IGH_Goo Duplicate() => Value == null ? new WordGoo<T>() : new WordGoo<T>(Json.Read<T>(Json.Write(Value)));
    public override string ToString() => Value switch { Document d => $"Documento ({d.Blocks.Length} blocchi)", Paragraph p => string.Concat(p.Runs.Select(x => x.Text)), TextRun r => r.Text, Style s => $"{s.Id} | {s.Name}", null => "Vuoto", _ => TypeName };
    public override bool Write(GH_IWriter writer)
    {
        if (Value != null)
            writer.SetString("Data", Json.Write(Value));
        return true;
    }
    public override bool Read(GH_IReader reader)
    {
        if (reader.ItemExists("Data"))
            Value = Json.Read<T>(reader.GetString("Data"));
        return true;
    }
    public override bool CastFrom(object source)
    {
        if (source is T t)
        {
            Value = t;
            return true;
        }
        if (source is WordGoo<T> g)
        {
            Value = g.Value;
            return true;
        }
        return false;
    }
}
public abstract class WordParam<T> : GH_PersistentParam<WordGoo<T>> where T : class
{
    protected WordParam() : base(new GH_InstanceDescription("Word " + typeof(T).Name, typeof(T).Name, "Dati tipizzati Pawper; ordine preservato.", "Pawper", "Parameters")) { }
    public override Guid ComponentGuid => Identity.Id("Param/" + typeof(T).Name);
    protected override Bitmap Icon => PawperIcons.Create(typeof(T).Name);
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    protected override GH_GetterResult Prompt_Singular(ref WordGoo<T> value) => GH_GetterResult.cancel;
    protected override GH_GetterResult Prompt_Plural(ref List<WordGoo<T>> values) => GH_GetterResult.cancel;
}
public sealed class DocumentParam : WordParam<Document>
{
}
public sealed class BlockParam : WordParam<ContentBlock>
{
}
public sealed class RunParam : WordParam<TextRun>
{
}
public sealed class StyleParam : WordParam<Style>
{
}
public sealed class LayoutParam : WordParam<PageLayout>
{
}
public sealed class CellParam : WordParam<Cell>
{
}
public sealed class TargetParam : WordParam<TemplateTarget>
{
}
public sealed class AssemblyInfo : GH_AssemblyInfo
{
    public override string Name => "Pawper";
    public override Bitmap Icon => PawperIcons.Create("Pawper");
    public override string Description => "Documenti Word parametrici e modificabili, senza automazione Office.";
    public override Guid Id => Identity.Id("Assembly");
    public override string AuthorName => "Pawper contributors";
    public override string AuthorContact => "";
    public override string AssemblyVersion => "0.1.0";
}
