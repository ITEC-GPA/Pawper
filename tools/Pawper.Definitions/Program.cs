using System.Reflection;
using System.Runtime.CompilerServices;
namespace Pawper.Definitions;
public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        Console.WriteLine("Starting definitions generator");
        RhinoInside.Resolver.Initialize();
        Console.WriteLine("Rhino.Inside resolver ready");
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => { var name = new AssemblyName(e.Name).Name; foreach (var folder in new[] { @"C:\Program Files\Rhino 8\Plug-ins\Grasshopper", AppContext.BaseDirectory }) foreach (var ext in new[] { ".dll", ".gha" }) { string file = Path.Combine(folder, name + ext); if (File.Exists(file)) return Assembly.LoadFrom(file); } return null; };
        try
        {
            if (args.Contains("--loading-check")) return CheckLoading(args);
            if (args.Contains("--editor-render")) return RenderEditors(args);
            return Run(args);
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int CheckLoading(string[] args)
    {
        // Isolate category registration from the user's installed add-ons and Rhino UI.
        var instances = typeof(global::Grasshopper.Instances);
        instances.GetField("m_headless", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, true);
        instances.GetField("m_comServer", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, new global::Grasshopper.Kernel.GH_ComponentServer());
        LoadingChecks.Run(Path.GetFullPath(args[0]));
        return 0;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int RenderEditors(string[] args)
    {
        InputIntegrationChecks.RenderDialogs(Path.GetFullPath(Path.Combine(args[0], "../qa/inputs")));
        Console.WriteLine("Editor controls rendered without opening windows");
        return 0;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int Run(string[] args)
    {
        Console.WriteLine("Starting Rhino native host");
        using var core = new Rhino.Runtime.InProcess.RhinoCore(["/NOSPLASH", "/NOTEMPLATE"], Rhino.Runtime.InProcess.WindowStyle.Hidden);
        Console.WriteLine("Rhino ready " + Rhino.RhinoApp.Version);
        return RunGrasshopper(args);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int RunGrasshopper(string[] args)
    {
        if (args.Contains("--inspect"))
        {
            var instanceType = typeof(global::Grasshopper.Instances);
            foreach (var f in instanceType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                Console.WriteLine("FIELD " + f);
            foreach (var p in instanceType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                Console.WriteLine("PROP " + p + " SET " + p.CanWrite);
            foreach (var type in new[] { typeof(global::Grasshopper.Kernel.GH_ComponentServer), typeof(global::Grasshopper.Kernel.GH_DocumentIO), typeof(global::Grasshopper.Kernel.GH_AssemblyInfo), typeof(global::Grasshopper.Kernel.GH_ExternalFile) })
            {
                Console.WriteLine(type.FullName);
                foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(m => m.Name.Contains("Load") || m.Name.Contains("Save") || m.Name.Contains("Register")))
                    Console.WriteLine(m);
                foreach (var c in type.GetConstructors())
                    Console.WriteLine(c);
            }
            return 0;
        }
        // Test harness only: use an isolated component registry instead of loading the user's installed add-ons.
        Console.WriteLine("Creating isolated Grasshopper registry");
        var instances = typeof(global::Grasshopper.Instances);
        instances.GetField("m_headless", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, true);
        instances.GetField("m_comServer", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, new global::Grasshopper.Kernel.GH_ComponentServer());
        // Generic persistent inputs resolve GH_Number/GH_String by type name during archive reads.
        // Register the SDK's own types, without discovering any of the user's third-party plugins.
        var ghAssembly = typeof(global::Grasshopper.Kernel.Types.GH_Number).Assembly;
        var parse = typeof(global::Grasshopper.Kernel.GH_ComponentServer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(m => m.Name == "ParseGHA" && m.GetParameters().Length == 3);
        parse.Invoke(global::Grasshopper.Instances.ComponentServer, [ghAssembly, ghAssembly.Location, global::Grasshopper.Kernel.GH_LoadingMechanism.Direct]);
        Console.WriteLine("Grasshopper registry ready");
        if (args.Contains("--capture-test")) return CaptureSmoke.Run(Path.GetFullPath(args[0]));
        return DefinitionBuilder.Run(Path.GetFullPath(args.Length > 0 ? args[0] : "examples"));
    }
}
