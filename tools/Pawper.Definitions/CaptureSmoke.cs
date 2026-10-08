using System.Drawing;
using global::Grasshopper.Kernel;
using global::Grasshopper.Kernel.Parameters;
using global::Grasshopper.Kernel.Types;
using Pawper.Core;
using Pawper.Grasshopper;
using Rhino;
using Rhino.Geometry;
namespace Pawper.Definitions;
internal static class CaptureSmoke
{
    public static int Run(string root)
    {
        var doc = RhinoDoc.Create(null) ?? throw new Exception("Documento Rhino non disponibile.");
        var geometry = new Box(Plane.WorldXY, new Interval(0, 6.5), new Interval(-0.2, 0.2), new Interval(-0.4, 0)).ToBrep();
        doc.Objects.AddBrep(geometry);
        var view = doc.Views.ActiveView ?? doc.Views.Add("Pawper", Rhino.Display.DefinedViewportProjection.Perspective, new Rectangle(0, 0, 1000, 700), false);
        view.ActiveViewport.ZoomExtents();
        doc.NamedViews.Add("PawperDemo", view.ActiveViewportID);
        using var file = new Rhino.FileIO.File3dm(); file.Objects.AddBrep(geometry);
        file.Write(Path.Combine(root, "07-immagini-e-viste", "model.3dm"), 8);
        using var graph = new GH_Document(); graph.Enabled = true; GH_Document.EnableSolutions = true;
        var capture = new CaptureViewComponent(); capture.CreateAttributes(); graph.AddObject(capture, false);
        void Toggle(bool value) { var p = (Param_Boolean)capture.Params.Input[5]; p.PersistentData.Clear(); p.PersistentData.Append(new GH_Boolean(value)); graph.NewSolution(true, GH_SolutionMode.Silent); }
        foreach (var mode in new[] { "active", "named" })
        {
            if (mode == "named")
            {
                var name = (Param_String)capture.Params.Input[0]; name.PersistentData.Clear(); name.PersistentData.Append(new GH_String("PawperDemo"));
                var alpha = (Param_Boolean)capture.Params.Input[3]; alpha.PersistentData.Clear(); alpha.PersistentData.Append(new GH_Boolean(true));
            }
            Toggle(false); Toggle(true);
            var errors = capture.RuntimeMessages(GH_RuntimeMessageLevel.Error);
            if (global::Grasshopper.Instances.RunningHeadless)
            {
                if (!errors.Any(e => e.Contains("sessione grafica"))) throw new Exception("La cattura headless non è stata rifiutata in modo sicuro.");
                File.WriteAllText(Path.Combine(root, "capture-verification.json"), "{\"HeadlessGuard\":true,\"InteractiveCapture\":\"Requires manual verification in a visible Rhino viewport\"}");
                Console.WriteLine("CAPTURE GUARD OK: acquisizione headless rifiutata prima delle API grafiche native.");
                doc.Modified = false;
                doc.Dispose();
                return 0;
            }
            if (errors.Count > 0) throw new Exception(string.Join("; ", errors));
            var image = capture.Params.Output[0].VolatileData.AllData(true).OfType<WordGoo<ContentBlock>>().Select(x => x.Value).OfType<Pawper.Core.Image>().Single();
            var bytes = Convert.FromBase64String(image.DataBase64); WordEngine.ImageSize(bytes);
            File.WriteAllBytes(Path.Combine(root, "07-immagini-e-viste", "rhino-" + mode + ".png"), bytes);
            Console.WriteLine("CAPTURE OK " + mode + " " + bytes.Length);
        }
        File.WriteAllText(Path.Combine(root, "capture-verification.json"), "{\"Active\":true,\"Named\":true,\"Transparent\":true}");
        doc.Modified = false; doc.Dispose();
        return 0;
    }
}
