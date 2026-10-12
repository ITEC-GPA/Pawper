using Grasshopper.Kernel;
using Pawper.Core;
using Pawper.Grasshopper;

namespace Pawper.Definitions;

internal static class LoadingChecks
{
    internal static void Run(string examples)
    {
        // Exercise the actual startup callback: component construction alone skips it.
        var instruction = new PawperPriority().PriorityLoad();
        if (instruction != GH_LoadingInstruction.Proceed)
            throw new InvalidOperationException("PawperPriority non consente il caricamento.");
        var server = global::Grasshopper.Instances.ComponentServer;
        string shortName = server.GetCategoryShortName("Pawper");
        var icon = server.GetCategoryIcon("Pawper");
        if (shortName != "Paw" || server.GetCategorySymbolName("Pawper") != "P")
            throw new InvalidOperationException("Nome breve o simbolo della categoria non validi.");
        if (icon == null || icon.Width != 16 || icon.Height != 16)
            throw new InvalidOperationException("La categoria richiede un'icona 16 × 16.");
        Directory.CreateDirectory(examples);
        File.WriteAllText(Path.Combine(examples, "loading-verification.json"), Json.Write(new
        {
            Success = true,
            EntryPoint = "PawperPriority.PriorityLoad",
            Category = "Pawper",
            ShortName = shortName,
            Symbol = "P",
            IconWidth = icon.Width,
            IconHeight = icon.Height,
            Instruction = instruction.ToString(),
            SdkAssembly = typeof(GH_ComponentServer).Assembly.GetName().Version?.ToString(),
            VisibleRhinoStartupTested = false
        }));
        Console.WriteLine("LOADING OK: Pawper / Paw / P, categoria 16 × 16, PriorityLoad=Proceed");
    }
}
