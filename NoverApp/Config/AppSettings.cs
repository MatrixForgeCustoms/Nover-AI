namespace NoverApp.Config;

// Alle Einstellungen an einer Stelle.
public static class AppSettings
{
    public static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nover");
    public static readonly string UsersFile = Path.Combine(DataDir, "nover_nutzer.json");
    public static readonly string ChatsFile = Path.Combine(DataDir, "nover_daten.json");
    public static readonly string SessionsFile = Path.Combine(DataDir, "nover_sitzungen.json");
    public static readonly string LogFile = Path.Combine(DataDir, "nover.log");
    public static readonly string WwwRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

    public const string OwnerName = "owner";
    public const int Port = 5057;
    public const int TagesBonus = 250;
    public const int KostenNormal = 25;
    public const int KostenSchwer = 75;
    public const int SitzungTage = 30;
    public const int KontextNachrichten = 6;
    public const int ApiZeitlimit = 60;

    public const string ModellSchnell = "gemini-2.5-flash";
    public const string ModellKomplex = "openai/gpt-oss-120b";
    public const string AnzeigeSchnell = "Nover";
    public const string AnzeigeKomplex = "Roms 1.3";
    public const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

    static AppSettings() => Directory.CreateDirectory(DataDir);
}
