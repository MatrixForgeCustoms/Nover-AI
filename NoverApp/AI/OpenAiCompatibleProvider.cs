using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NoverApp.Config;

namespace NoverApp.AI;

// Fuer alle Anbieter im OpenAI-Format (z.B. Groq, fuer Roms 1.3).
public class OpenAiCompatibleProvider(string name, string url, string envName) : IChatProvider
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(AppSettings.ApiZeitlimit) };

    public async Task<(bool Ok, string Text)> FrageAsync(
        string system, IReadOnlyList<(string Rolle, string Text)> verlauf, string modell)
    {
        var schluessel = Environment.GetEnvironmentVariable(envName);
        if (string.IsNullOrWhiteSpace(schluessel))
            return (false, $"Mir fehlt der {name}-Schlüssel ({envName}).");

        var nachrichten = new List<object> { new { role = "system", content = system } };
        nachrichten.AddRange(verlauf.Select(m => (object)new
        {
            role = m.Rolle == "assistant" ? "assistant" : "user",
            content = m.Text,
        }));

        var daten = new { model = modell, messages = nachrichten, max_tokens = 2048, reasoning_effort = "low" };

        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(daten), Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", schluessel);

        return await ProviderHelper.Senden(Http, req, name,
            json => json.GetProperty("choices")[0].GetProperty("message")
                .GetProperty("content").GetString() ?? "");
    }
}
