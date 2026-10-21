using System.Text;
using System.Text.Json;
using NoverApp.Config;

namespace NoverApp.AI;

// Anbindung an Google Gemini (fuer Nover, schnelle Fragen).
public class GeminiProvider : IChatProvider
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(AppSettings.ApiZeitlimit) };

    public async Task<(bool Ok, string Text)> FrageAsync(
        string system, IReadOnlyList<(string Rolle, string Text)> verlauf, string modell)
    {
        var schluessel = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(schluessel))
            return (false, "Mir fehlt der Gemini-Schlüssel (GEMINI_API_KEY).");

        var daten = new
        {
            systemInstruction = new { parts = new[] { new { text = system } } },
            contents = verlauf.Select(m => new
            {
                role = m.Rolle == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Text } },
            }).ToList(),
            // Denkzeit aus, damit Antworten schnell kommen
            generationConfig = new { thinkingConfig = new { thinkingBudget = 0 }, maxOutputTokens = 4096 },
        };

        using var req = new HttpRequestMessage(HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{modell}:generateContent")
        {
            Content = new StringContent(JsonSerializer.Serialize(daten), Encoding.UTF8, "application/json"),
        };
        req.Headers.Add("x-goog-api-key", schluessel);

        return await ProviderHelper.Senden(Http, req, "Gemini",
            json => json.GetProperty("candidates")[0].GetProperty("content")
                .GetProperty("parts")[0].GetProperty("text").GetString() ?? "");
    }
}
