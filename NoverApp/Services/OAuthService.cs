using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using NoverApp.Config;

namespace NoverApp.Services;

public record OAuthUser(string Id, string Email);

// Login ueber Google und GitHub (OAuth 2.0, Authorization Code Flow).
// Client-ID und Secret kommen aus Umgebungsvariablen, nie aus dem Code.
public class OAuthService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(AppSettings.ApiZeitlimit) };
    private readonly Dictionary<string, (string Provider, DateTime Ablauf)> _states = new();
    private readonly object _lock = new();

    public string Redirect(string provider) =>
        $"http://localhost:{AppSettings.Port}/api/oauth/{provider}/callback";

    private static (string? Id, string? Secret) Keys(string provider) => provider switch
    {
        "google" => (Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"), Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")),
        "github" => (Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID"), Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET")),
        _ => (null, null),
    };

    // Liefert die Adresse des Anbieters oder null, wenn der Login nicht eingerichtet ist.
    public string? Start(string provider)
    {
        var (id, secret) = Keys(provider);
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret)) return null;

        var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        lock (_lock) _states[state] = (provider, DateTime.UtcNow.AddMinutes(10));
        var redirect = Uri.EscapeDataString(Redirect(provider));
        var cid = Uri.EscapeDataString(id);

        return provider == "google"
            ? $"https://accounts.google.com/o/oauth2/v2/auth?client_id={cid}&redirect_uri={redirect}&response_type=code&scope=openid%20email%20profile&state={state}"
            : $"https://github.com/login/oauth/authorize?client_id={cid}&redirect_uri={redirect}&scope=user%3Aemail&state={state}";
    }

    // Prueft den state-Wert gegen Manipulation und Wiederverwendung.
    public bool CheckState(string state, string provider)
    {
        lock (_lock)
        {
            if (!_states.TryGetValue(state, out var eintrag)) return false;
            _states.Remove(state);
            return eintrag.Provider == provider && eintrag.Ablauf > DateTime.UtcNow;
        }
    }

    public async Task<OAuthUser?> FinishAsync(string provider, string code)
    {
        var (id, secret) = Keys(provider);
        var token = provider == "google"
            ? await GoogleTokenAsync(code, id!, secret!)
            : await GitHubTokenAsync(code, id!, secret!);
        if (token is null) return null;

        return provider == "google" ? await GoogleUserAsync(token) : await GitHubUserAsync(token);
    }

    private async Task<string?> GoogleTokenAsync(string code, string id, string secret)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code, ["client_id"] = id, ["client_secret"] = secret,
                ["redirect_uri"] = Redirect("google"), ["grant_type"] = "authorization_code",
            }),
        };
        var json = await SendAsync(req);
        return json?.GetProperty("access_token").GetString();
    }

    private async Task<string?> GitHubTokenAsync(string code, string id, string secret)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code, ["client_id"] = id, ["client_secret"] = secret,
                ["redirect_uri"] = Redirect("github"),
            }),
        };
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var json = await SendAsync(req);
        return json?.GetProperty("access_token").GetString();
    }

    private async Task<OAuthUser?> GoogleUserAsync(string token)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var json = await SendAsync(req);
        if (json is null) return null;
        var sub = json.Value.GetProperty("sub").GetString() ?? "";
        var email = json.Value.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
        return new OAuthUser(sub, email);
    }

    private async Task<OAuthUser?> GitHubUserAsync(string token)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.UserAgent.ParseAdd("Nover");
        var json = await SendAsync(req);
        if (json is null) return null;
        var id = json.Value.GetProperty("id").GetInt64().ToString();
        var email = json.Value.TryGetProperty("email", out var e) && e.ValueKind == JsonValueKind.String ? e.GetString() ?? "" : "";
        return new OAuthUser(id, email);
    }

    private static async Task<JsonElement?> SendAsync(HttpRequestMessage req)
    {
        try
        {
            using var res = await Http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            return doc.RootElement.Clone();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return null;
        }
    }
}
