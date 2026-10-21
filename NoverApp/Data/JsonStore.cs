using System.Text.Json;
using NoverApp.Core;

namespace NoverApp.Data;

// Liest und schreibt JSON. Schreiben ist atomar: erst temporaere Datei, dann umbenennen.
public static class JsonStore
{
    private static readonly JsonSerializerOptions Opt = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static T Load<T>(string pfad, T fallback) where T : notnull
    {
        try
        {
            if (!File.Exists(pfad)) return fallback;
            return JsonSerializer.Deserialize<T>(File.ReadAllText(pfad), Opt) ?? fallback;
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            Logger.Error($"Datei nicht lesbar ({pfad}): {ex.Message}");
            return fallback;
        }
    }

    public static void Save<T>(string pfad, T daten)
    {
        var tmp = pfad + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(daten, Opt));
        File.Move(tmp, pfad, true);
    }
}
