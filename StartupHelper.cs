using Microsoft.Win32;

public static class StartupHelper
{
    private const string AppName = "BiblioLinx";

    public static void SetStartup(bool enable)
    {
#if WINDOWS
        string exePath = Environment.ProcessPath ?? "";

        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run", true);

        if (enable)
        {
            key?.SetValue(AppName, exePath);
        }
        else
        {
            key?.DeleteValue(AppName, false);
        }
#endif
    }

    public static bool IsStartupEnabled()
    {
#if WINDOWS
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run");

        return key?.GetValue(AppName) != null;
#else
        return false;
#endif
    }
}