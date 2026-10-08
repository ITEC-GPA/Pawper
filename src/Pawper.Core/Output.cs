using System.Collections.Immutable;
namespace Pawper.Core;
public sealed class RisingEdge
{
    bool previous;
    public bool Consume(bool current)
    {
        bool fire = current && !previous;
        previous = current;
        return fire;
    }
    public void Disarm() => previous = true;
}
public record SaveResult(string Path, bool Success, string Message);
public static class Output
{
    public static SaveResult Save(Document document, string path, bool overwrite = false)
    {
        string full = Path.GetFullPath(path);
        if (!full.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("L'output deve avere estensione DOCX.");
        if (File.Exists(full) && !overwrite)
            throw new IOException("File già esistente: abilitare Overwrite oppure cambiare nome.");
        var bytes = WordEngine.Build(document);
        var errors = WordEngine.Validate(bytes);
        if (errors.Length > 0)
            throw new InvalidDataException("Documento non valido: " + string.Join("\n", errors));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        string temp = Path.Combine(Path.GetDirectoryName(full)!, ".pawper-" + Guid.NewGuid() + ".tmp");
        try
        {
            using (var fs = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                fs.Write(bytes);
                fs.Flush(true);
            }
            if (File.Exists(full))
            {
                if (!overwrite)
                    throw new IOException("Destinazione creata durante il salvataggio.");
                File.Replace(temp, full, null);
            }
            else
                File.Move(temp, full);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
        return new(full, true, "Salvato; aggiornare i campi in Word se presenti.");
    }
    public static ImmutableArray<SaveResult> Batch(IReadOnlyList<Document> documents, IReadOnlyList<string> paths, bool overwrite = false)
    {
        if (documents.Count != paths.Count)
            throw new ArgumentException("Numero documenti e percorsi diverso.");
        var normalized = paths.Select(Path.GetFullPath).ToArray();
        if (normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Length)
            throw new ArgumentException("Percorsi di output duplicati nel batch.");
        if (normalized.Any(p => !p.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Tutti i percorsi devono essere DOCX.");
        if (!overwrite && normalized.Any(File.Exists))
            throw new IOException("Almeno una destinazione esiste già.");
        var results = new List<SaveResult>();
        for (int i = 0; i < documents.Count; i++)
        {
            try
            {
                results.Add(Save(documents[i], normalized[i], overwrite));
            }
            catch (Exception ex) { results.Add(new(normalized[i], false, ex.Message)); }
        }
        return results.ToImmutableArray();
    }
}
