using System.Text.Json;
using NoverApp.Core;

namespace NoverApp.AI;

// Gemeinsame Schnittstelle aller KI-Anbieter.
public interface IChatProvider
{
    Task<(bool Ok, string Text)> FrageAsync(
        string system, IReadOnlyList<(string Rolle, string Text)> verlauf, string modell);
}

// Gemeinsamer Versand und Fehlerbehandlung fuer alle Anbieter.
public static class ProviderHelper
{
    public static async Task<(bool Ok, string Text)> Senden(
        HttpClient http, HttpRequestMessage req, string name, Func<JsonElement, string> lese)
    {
        try
        {
            using var antwort = await http.SendAsync(req);
            var body = await antwort.Content.ReadAsStringAsync();
            if (!antwort.IsSuccessStatusCode)
            {
                Logger.Error($"{name} HTTP {(int)antwort.StatusCode}");
                return (false, $"{name} hat abgelehnt (Code {(int)antwort.StatusCode}). Prüf Schlüssel, Modell und Limits.");
            }
            using var doc = JsonDocument.Parse(body);
            return (true, lese(doc.RootElement).Trim());
        }
        catch (TaskCanceledException)
        {
            Logger.Error($"{name}: Zeitüberschreitung");
            return (false, $"{name} antwortet zu langsam. Probier es nochmal.");
        }
        catch (HttpRequestException ex)
        {
            Logger.Error($"{name}: {ex.Message}");
            return (false, $"Ich kann {name} gerade nicht erreichen. Prüf deine Internetverbindung.");
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or JsonException or IndexOutOfRangeException)
        {
            Logger.Error($"{name}: unerwartete Antwort: {ex.Message}");
            return (false, $"Die Antwort von {name} sah komisch aus. Probier es nochmal.");
        }
    }
}
