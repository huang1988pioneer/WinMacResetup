using Microsoft.Win32;
using System.Runtime.Versioning;

namespace ReinstallChecklist.Services;

public static class InstalledAppScanner
{
    public static Task<Dictionary<string, string>> GetInstalledAppsAsync() => Task.Run(() =>
    {
        var apps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (OperatingSystem.IsWindows())
        {
            foreach (var path in new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            })
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                AddRegistryNames(key, apps);
            }
            using var userKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            AddRegistryNames(userKey, apps);
            AddShortcutNames(Environment.SpecialFolder.DesktopDirectory, "桌面捷徑", apps);
            AddShortcutNames(Environment.SpecialFolder.CommonDesktopDirectory, "公用桌面捷徑", apps);
            AddShortcutNames(Environment.SpecialFolder.StartMenu, "開始功能表捷徑", apps);
            AddShortcutNames(Environment.SpecialFolder.CommonStartMenu, "公用開始功能表捷徑", apps);
        }
        else if (OperatingSystem.IsMacOS())
        {
            foreach (var app in Directory.EnumerateDirectories("/Applications", "*.app"))
                apps.TryAdd(Path.GetFileNameWithoutExtension(app), "應用程式資料夾");
        }
        return apps;
    });

    [SupportedOSPlatform("windows")]
    private static void AddRegistryNames(RegistryKey? key, IDictionary<string, string> apps)
    {
        if (key is null) return;
        foreach (var childName in key.GetSubKeyNames())
        {
            using var child = key.OpenSubKey(childName);
            if (child?.GetValue("DisplayName") is string name && !string.IsNullOrWhiteSpace(name))
                apps.TryAdd(name, "Windows 已安裝程式");
        }
    }

    [SupportedOSPlatform("windows")]
    private static void AddShortcutNames(Environment.SpecialFolder folder, string source, IDictionary<string, string> apps)
    {
        var directory = Environment.GetFolderPath(folder);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return;
        try
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.lnk", SearchOption.AllDirectories))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (!string.IsNullOrWhiteSpace(name)) apps.TryAdd(name, source);
            }
        }
        catch (UnauthorizedAccessException) { }
    }

    public static string? FindMatch(string checklistName, IEnumerable<string> installedNames) => installedNames.FirstOrDefault(name =>
        name.Contains(checklistName, StringComparison.OrdinalIgnoreCase) ||
        checklistName.Contains(name, StringComparison.OrdinalIgnoreCase));
}
