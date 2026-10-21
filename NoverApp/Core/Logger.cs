using NoverApp.Config;

namespace NoverApp.Core;

// Schreibt in nover.log. Fehler beim Schreiben duerfen das Programm nie stoppen.
public static class Logger
{
    private static readonly object Lock = new();

    public static void Info(string nachricht) => Schreibe("INFO", nachricht);
    public static void Warn(string nachricht) => Schreibe("WARN", nachricht);
    public static void Error(string nachricht) => Schreibe("ERROR", nachricht);

    private static void Schreibe(string stufe, string nachricht)
    {
        try
        {
            lock (Lock)
            {
                File.AppendAllText(AppSettings.LogFile,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{stufe}] {nachricht}{Environment.NewLine}");
            }
        }
        catch (IOException)
        {
            // Logging darf nie die App stoppen
        }
    }
}
