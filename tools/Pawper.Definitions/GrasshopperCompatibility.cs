using System.Reflection;
using Grasshopper.Kernel;

namespace Pawper.Definitions;

// Internal SDK access is confined to the isolated test host.
internal static class GrasshopperCompatibility
{
    internal static FieldInfo RequireField(Type type, string name, Type valueType)
    {
        var field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null || field.FieldType != valueType)
            throw Incompatible(type, name);
        return field;
    }

    internal static MethodInfo RequireMethod(Type type, string name, Type returnType, params Type[] parameters)
    {
        var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            binder: null, types: parameters, modifiers: null);
        if (method == null || method.ReturnType != returnType)
            throw Incompatible(type, name);
        return method;
    }

    static InvalidOperationException Incompatible(Type type, string member) => new(
        $"SDK non compatibile con il test host Pawper: {type.FullName}.{member} assente o con firma diversa " +
        $"in {type.Assembly.GetName().Name} {type.Assembly.GetName().Version}. " +
        "Aggiornare il test host per questa versione Rhino; il controllo riguarda API interne del generatore.");

    internal static void CreateIsolatedRegistry()
    {
        var instances = typeof(global::Grasshopper.Instances);
        // Validate both members before changing the registry.
        var headless = RequireField(instances, "m_headless", typeof(bool));
        var server = RequireField(instances, "m_comServer", typeof(GH_ComponentServer));
        headless.SetValue(null, true);
        server.SetValue(null, new GH_ComponentServer());
    }

    internal static void VerifyDiagnostics()
    {
        void Reject(Action action, string member)
        {
            try { action(); }
            catch (InvalidOperationException ex) when (ex.Message.Contains(member) && ex.Message.Contains("SDK non compatibile")) { return; }
            throw new InvalidOperationException("Diagnostica SDK non verificata: " + member);
        }
        Reject(() => RequireField(typeof(global::Grasshopper.Instances), "__PawperMissingField", typeof(bool)), "__PawperMissingField");
        Reject(() => RequireField(typeof(global::Grasshopper.Instances), "m_headless", typeof(string)), "m_headless");
        Reject(() => RequireMethod(typeof(GH_ComponentServer), "LoadGHA", typeof(bool), typeof(string)), "LoadGHA");
    }
}
