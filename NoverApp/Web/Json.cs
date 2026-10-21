using System.Net;
using System.Text.Json;

namespace NoverApp.Web;

// Anfrage- und Antwort-Typen der HTTP-API.
public record RegisterRequest(string Name, string Passwort);
public record LoginRequest(string Name, string Passwort);
public record ChatRequest(string Frage, bool Schwer);
public record RatingRequest(int Index, int Wert);
public record CommandRequest(string Text);
public record FeedbackRequest(int Index, string Text);

// Hilfsfunktionen fuer JSON-Antworten und -Anfragen.
public static class Json
{
    private static readonly JsonSerializerOptions Opt = new(JsonSerializerDefaults.Web);

    public static void Send(HttpListenerResponse res, int status, object body)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(body, Opt);
        res.StatusCode = status;
        res.ContentType = "application/json; charset=utf-8";
        res.ContentLength64 = bytes.Length;
        res.OutputStream.Write(bytes, 0, bytes.Length);
        res.OutputStream.Close();
    }

    public static T? Read<T>(HttpListenerRequest req) where T : class
    {
        try
        {
            using var sr = new StreamReader(req.InputStream, req.ContentEncoding);
            return JsonSerializer.Deserialize<T>(sr.ReadToEnd(), Opt);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string? Bearer(HttpListenerRequest req)
    {
        var kopf = req.Headers["Authorization"];
        return kopf is not null && kopf.StartsWith("Bearer ", StringComparison.Ordinal)
            ? kopf["Bearer ".Length..]
            : null;
    }
}
