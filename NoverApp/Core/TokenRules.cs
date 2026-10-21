using NoverApp.Config;

namespace NoverApp.Core;

// Kosten pro Frage. Die Werte kommen aus AppSettings.
public static class TokenRules
{
    public static int Kosten(bool schwer) => schwer ? AppSettings.KostenSchwer : AppSettings.KostenNormal;
}
