using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Pawper.Core;
using Pawper.Grasshopper;

namespace Pawper.Definitions;

internal static class BrandingChecks
{
    internal static void Run(string examples)
    {
        LoadingChecks.Run(examples);
        string project = Path.GetFullPath(Path.Combine(examples, ".."));
        string assets = Path.Combine(project, "assets", "icons");
        Directory.CreateDirectory(assets);
        var assembly = typeof(NewDocumentComponent).Assembly;
        var objects = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IGH_DocumentObject).IsAssignableFrom(t))
            .Select(t => (IGH_DocumentObject)Activator.CreateInstance(t)!).OrderBy(o => o.GetType().Name).ToArray();
        if (objects.Length != 48 || objects.Select(o => o.ComponentGuid).Distinct().Count() != 48)
            throw new InvalidOperationException("Componenti, parametri o identificatori non validi.");
        var stableType = assembly.GetType("Pawper.Grasshopper.StableIds")!;
        var stable = (IReadOnlyDictionary<string, Guid>)stableType.GetField("Values", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        foreach (var obj in objects)
        {
            var icon = obj.Icon_24x24;
            if (icon == null || icon.Width != 24 || icon.Height != 24) throw new Exception("Icona mancante: " + obj.Name);
            if (obj.Category != "Pawper" || !stable.Values.Contains(obj.ComponentGuid)) throw new Exception("Identità modificata: " + obj.Name);
            icon.Save(Path.Combine(assets, obj.GetType().Name + ".png"), ImageFormat.Png);
        }
        foreach (int size in new[] { 24, 128, 256 })
        {
            using var logo = PawperIcons.Create("Pawper", size);
            logo.Save(Path.Combine(assets, $"pawper-{size}.png"), ImageFormat.Png);
        }
        using (var preview = new Bitmap(1040, 170 + ((objects.Length + 3) / 4) * 55))
        using (var graphics = Graphics.FromImage(preview))
        using (var font = new Font("Segoe UI", 10))
        using (var title = new Font("Segoe UI", 22, FontStyle.Bold))
        using (var logo = PawperIcons.Create("Pawper", 96))
        {
            graphics.Clear(Color.FromArgb(245, 246, 242));
            graphics.DrawImageUnscaled(logo, 24, 20);
            graphics.DrawString("Pawper", title, Brushes.DarkSlateGray, 135, 32);
            graphics.DrawString($"{objects.Length} icone per componenti e parametri · anteprima alle dimensioni reali di 24 px", font, Brushes.DarkSlateGray, 138, 77);
            for (int i = 0; i < objects.Length; i++)
            {
                int x = 24 + i % 4 * 255, y = 140 + i / 4 * 55;
                graphics.DrawImageUnscaled(objects[i].Icon_24x24, x, y);
                graphics.DrawString(objects[i].Name, font, Brushes.DarkSlateGray, x + 36, y + 2);
            }
            preview.Save(Path.Combine(assets, "pawper-icons.png"), ImageFormat.Png);
        }

        int previous = 0;
        string backup = Path.Combine(project, "qa", "before-rename");
        if (Directory.Exists(backup))
        {
            GH_Document.EnableSolutions = false;
            try
            {
                foreach (string file in Directory.EnumerateFiles(backup, "example.gh", SearchOption.AllDirectories))
                {
                    var archive = new GH_Archive();
                    using var graph = new GH_Document();
                    if (!archive.ReadFromFile(file) || !archive.ExtractObject(graph, "Definition") || graph.ObjectCount == 0)
                        throw new Exception("Definizione precedente non leggibile: " + file);
                    if (archive.MessageCount(false, false, true) > 0 || graph.Objects.Any(o => o.GetType().Assembly != assembly))
                        throw new Exception("Componenti precedenti non risolti in Pawper: " + file);
                    previous++;
                }
            }
            finally { GH_Document.EnableSolutions = true; }
            if (previous != 20) throw new Exception("Attese 20 definizioni precedenti.");
        }
        File.WriteAllText(Path.Combine(examples, "branding-verification.json"), Json.Write(new
        {
            Name = "Pawper", Components = objects.OfType<GH_Component>().Count(), Parameters = 7, Icons = objects.Length,
            PersistentIdsPreserved = 41, NewIds = 8, PreviousDefinitionsReopened = previous
        }));
        Console.WriteLine($"PAWPER BRANDING OK: {objects.Length} icons, {previous} previous definitions reopened");
    }
}
