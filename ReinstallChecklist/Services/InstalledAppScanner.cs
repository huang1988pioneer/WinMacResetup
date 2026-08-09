using Microsoft.Win32;
using System.Runtime.Versioning;

namespace ReinstallChecklist.Services;

public static class InstalledAppScanner
{
    public static Task<HashSet<string>> GetInstalledNamesAsync() => Task.Run(() =>
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (OperatingSystem.IsWindows())
        {
            foreach (var path in new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            })
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                AddRegistryNames(key, names);
            }
            using var userKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            AddRegistryNames(userKey, names);
        }
        else if (OperatingSystem.IsMacOS())
        {
            foreach (var app in Directory.EnumerateDirectories("/Applications", "*.app"))
                names.Add(Path.GetFileNameWithoutExtension(app));
        }
        return names;
    });

    [SupportedOSPlatform("windows")]
    private static void AddRegistryNames(RegistryKey? key, ISet<string> names)
    {
        if (key is null) return;
        foreach (var childName in key.GetSubKeyNames())
        {
            using var child = key.OpenSubKey(childName);
            if (child?.GetValue("DisplayName") is string name && !string.IsNullOrWhiteSpace(name)) names.Add(name);
        }
    }

    public static string? FindMatch(string checklistName, IEnumerable<string> installedNames) => installedNames.FirstOrDefault(name =>
        name.Contains(checklistName, StringComparison.OrdinalIgnoreCase) ||
        checklistName.Contains(name, StringComparison.OrdinalIgnoreCase));
}
