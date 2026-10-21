using System.Text.Json.Serialization;

namespace NoverApp.Data;

// Konto. Die Feldnamen passen zu den bisherigen Python-Daten (bonus_tag bleibt erhalten).
public class UserRecord
{
    public string Name { get; set; } = "";
    public string Salt { get; set; } = "";
    public string Hash { get; set; } = "";
    public string Rolle { get; set; } = "nutzer";
    public int Tokens { get; set; }

    [JsonPropertyName("bonus_tag")]
    public string BonusTag { get; set; } = "";
}

// Ein gespeichertes Frage-Antwort-Paar.
public class ChatRecord
{
    public string Zeit { get; set; } = "";
    public string Benutzer { get; set; } = "";
    public string Frage { get; set; } = "";
    public string Antwort { get; set; } = "";
    public int Kosten { get; set; }
    public bool Schwer { get; set; }
    public int? Bewertung { get; set; }
    public string? Feedback { get; set; }
}
