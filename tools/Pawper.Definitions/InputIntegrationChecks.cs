using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Pawper.Core;
using Pawper.Grasshopper;
using Pawper.Samples;
using Document = Pawper.Core.Document;
namespace Pawper.Definitions;
internal static class InputIntegrationChecks
{
    internal static void Run(string examples)
    {
        string qa = Path.GetFullPath(Path.Combine(examples, "../qa/inputs")); Directory.CreateDirectory(qa);
        var checks = new List<string>();
        void Assert(bool valid, string message) { if (!valid) throw new Exception(message); }
        void Check(string name, Action action) { action(); checks.Add(name); Console.WriteLine("INPUT OK " + name); }
        var helper = typeof(TextBlocksComponent).Assembly.GetType("Pawper.Grasshopper.TextInputEditor")!;
        object? Call(string name, params object[] arguments) => helper.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments);
        using var graph = new GH_Document { Enabled = true };
        var text = new TextBlocksComponent(); text.CreateAttributes(); graph.AddObject(text, false);
        var input = (Param_String)text.Params.Input[0];
        var quick = new QuickDocumentComponent(); quick.CreateAttributes(); graph.AddObject(quick, false);
        quick.Params.Input[1].AddSource(text.Params.Output[0]);
        Check("editor applies persistent multiline input and reconstructs native blocks", () =>
        {
            Call("Apply", text, "# Titolo\nParagrafo **forte**"); graph.NewSolution(true, GH_SolutionMode.Silent);
            Assert(input.PersistentData.DataCount == 1, "Testo editor non persistente: " + input.PersistentData.DataCount + " valori: " + string.Join(" / ", input.PersistentData.AllData(true).Select(x => x.ToString())));
            Assert(text.Params.Output[0].VolatileData.DataCount == 2, "Blocchi inattesi");
            Assert(((Paragraph)text.Params.Output[0].VolatileData.AllData(true).OfType<WordGoo<ContentBlock>>().Last().Value).Runs.Any(r => r.Format?.Bold == true), "Grassetto perso");
        });
        Check("structured text reports malformed input and recovers", () =>
        {
            Call("Apply", text, "**non chiuso"); graph.NewSolution(true, GH_SolutionMode.Silent);
            Assert(text.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any(e => e.Contains("Riga 1")), "Errore riga assente");
            Call("Apply", text, "Valido"); graph.NewSolution(true, GH_SolutionMode.Silent);
            Assert(!text.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any(), "Errore non cancellato");
        });
        Check("text branches produce separate documents without duplication", () =>
        {
            input.PersistentData.Clear(); input.PersistentData.Append(new GH_String("1. A\n2. Uno"), new GH_Path(2)); input.PersistentData.Append(new GH_String("1. B\n2. Due"), new GH_Path(7));
            Assert(!(bool)Call("CanEdit", text, 0)!, "Editor deve proteggere rami distinti");
            graph.NewSolution(true, GH_SolutionMode.Silent);
            var docs = quick.Params.Output[0].VolatileData.AllData(true).OfType<WordGoo<Document>>().Select(d => d.Value).ToArray();
            Assert(docs.Length == 2 && docs.All(d => d.Blocks.Length == 2), "Data Tree appiattito");
            Assert(((Paragraph)docs[0].Blocks[0]).ListKey != ((Paragraph)docs[1].Blocks[0]).ListKey, "Elenchi di rami distinti condivisi");
            graph.NewSolution(true, GH_SolutionMode.Silent);
            Assert(quick.Params.Output[0].VolatileData.DataCount == 2, "Duplicazione al ricalcolo");
        });
        Check("editor does not overwrite connected input", () =>
        {
            var source = new Param_String(); source.CreateAttributes(); source.SetPersistentData(new GH_String("Sorgente")); graph.AddObject(source, false); input.AddSource(source);
            Assert(!(bool)Call("CanEdit", text, 0)!, "Editor disponibile con input collegato");
            bool rejected = false; try { Call("Apply", text, "Sovrascrittura"); } catch (TargetInvocationException) { rejected = true; }
            Assert(rejected && input.SourceCount == 1, "Sorgente scollegata"); input.RemoveAllSources(); graph.RemoveObject(source, false);
        });
        Check("format menus apply preset and alignment", () =>
        {
            foreach (var pair in new (GH_Component Component, string[] Values)[] { (new PagePresetComponent(), ["A4", "A3", "Letter", "Legal"]), (new ParagraphFormatComponent(), ["left", "center", "right", "both"]) })
            {
                pair.Component.CreateAttributes(); graph.AddObject(pair.Component, false);
                using var menu = new ContextMenuStrip(); Call("Choices", menu, pair.Component, 0, pair.Values);
                var parent = (ToolStripMenuItem)menu.Items[0];
                foreach (ToolStripMenuItem item in parent.DropDownItems)
                {
                    item.PerformClick(); graph.NewSolution(true, GH_SolutionMode.Silent);
                    Assert(((Param_String)pair.Component.Params.Input[0]).PersistentData.AllData(true).First().ToString() == item.Text, "Menu non applicato");
                    Assert(!pair.Component.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any(), "Formato rifiutato");
                }
            }
        });
        Check("editor dialogs render with multiline input and explicit apply cancel", () => RenderDialogs(qa));
        if (Directory.Exists(Path.Combine(examples, "../qa/before-inputs")))
        Check("twenty definitions from table release still resolve", () =>
        {
            string folder = Path.Combine(examples, "../qa/before-inputs"); int count = 0;
            foreach (string file in Directory.EnumerateFiles(folder, "example.gh", SearchOption.AllDirectories))
            {
                using var old = new GH_Document(); var archive = new GH_Archive();
                Assert(archive.ReadFromFile(file) && archive.ExtractObject(old, "Definition") && old.ObjectCount > 0 && archive.MessageCount(false, false, true) == 0, "Archivio precedente non leggibile: " + file); count++;
            }
            Assert(count == 20, "Backup precedente incompleto");
        });
        File.WriteAllText(Path.Combine(examples, "inputs-verification.json"), Json.Write(new { Passed = checks.Count, Checks = checks, DialogsRendered = true, InteractiveMouseKeyboardTested = false }));
    }
    internal static void RenderDialogs(string qa)
    {
        Directory.CreateDirectory(qa);
        void Assert(bool value, string error) { if (!value) throw new Exception(error); }
        var helper = typeof(TextBlocksComponent).Assembly.GetType("Pawper.Grasshopper.TextInputEditor")!;
        object? Call(string name, params object[] arguments) => helper.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments);
            foreach (string mode in new[] { "Text Blocks", "Text Table" })
            {
                string help = (string)helper.GetField(mode == "Text Blocks" ? "BlockHelp" : "TableHelp", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;
                using var form = (Form)Call("CreateDialog", mode, mode == "Text Blocks" ? InputSamples.TextFor(2) : TableSamples.Tsv, help)!;
                var box = form.Controls.OfType<TextBox>().Single();
                Assert(box.Multiline && box.AcceptsReturn && box.AcceptsTab && !box.WordWrap, "Editor non multilinea");
                Assert(((Button)form.AcceptButton!).DialogResult == DialogResult.OK && ((Button)form.CancelButton!).DialogResult == DialogResult.Cancel, "Pulsanti non distinti");
                form.CreateControl(); form.PerformLayout();
                using var bitmap = new Bitmap(form.Width, form.Height); form.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var origin = form.PointToScreen(Point.Empty); origin.Offset(-form.Left, -form.Top);
                    void DrawChildren(Control parent, Point offset)
                    {
                        foreach (Control child in parent.Controls)
                        {
                            var position = new Point(offset.X + child.Left, offset.Y + child.Top);
                            if (child.Width <= 0 || child.Height <= 0) continue;
                            using var surface = new Bitmap(child.Width, child.Height);
                            child.DrawToBitmap(surface, new Rectangle(Point.Empty, surface.Size));
                            graphics.DrawImageUnscaled(surface, position); DrawChildren(child, position);
                        }
                    }
                    DrawChildren(form, origin);
                }
                bitmap.Save(Path.Combine(qa, mode.Replace(' ', '-') + ".png"), ImageFormat.Png);
                Assert(box.Height > 300 && box.Width > 700, "Area testo troppo piccola");
            }
    }
}
