using NoverApp.Core;

namespace NoverApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Logger.Info("Nover gestartet");
        Application.Run(new MainForm());
    }
}
