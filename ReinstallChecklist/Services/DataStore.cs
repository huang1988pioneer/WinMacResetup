using System.Text.Json;
using ReinstallChecklist.Models;

namespace ReinstallChecklist.Services;

public sealed class DataStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path;

    public DataStore()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReinstallChecklist");
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "checklist.json");
    }

    public async Task<List<AppRecord>> LoadAsync()
    {
        if (!File.Exists(_path)) return DefaultRecords.Create();
        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<List<AppRecord>>(stream, Options) ?? DefaultRecords.Create();
    }

    public async Task SaveAsync(IEnumerable<AppRecord> records)
    {
        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, records, Options);
    }
}

public static class DefaultRecords
{
    public static List<AppRecord> Create() =>
    [
        new() { Name = "Google Chrome", Category = "瀏覽器", Platforms = "Windows, macOS" },
        new() { Name = "Firefox", Category = "瀏覽器", Platforms = "Windows, macOS" },
        new() { Name = "Visual Studio Code", Category = "開發工具", Platforms = "Windows, macOS" },
        new() { Name = "Git", Category = "開發工具", Platforms = "Windows, macOS" },
        new() { Name = "7-Zip", Category = "工具程式", Platforms = "Windows" },
        new() { Name = "VLC media player", Category = "影音", Platforms = "Windows, macOS" },
        new() { Name = "Bitwarden", Category = "安全性", Platforms = "Windows, macOS" },
        new() { Name = "Microsoft 365", Category = "生產力", Platforms = "Windows, macOS", IsPaid = true },
        new() { Name = "Adobe Creative Cloud", Category = "創作", Platforms = "Windows, macOS", IsPaid = true },
        new() { Name = "Homebrew", Category = "套件管理", Platforms = "macOS" }
    ];
}
