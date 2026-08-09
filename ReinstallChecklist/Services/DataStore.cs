using System.Text.Json;
using ReinstallChecklist.Models;

namespace ReinstallChecklist.Services;

public sealed class DataStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _temporaryPath;
    private readonly string _importedPath;
    private readonly string _legacyPath;
    private string _activePath;

    public bool UsesTemporaryRecord { get; private set; }

    public DataStore()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReinstallChecklist");
        Directory.CreateDirectory(directory);
        _temporaryPath = Path.Combine(directory, "temporary-checklist.json");
        _importedPath = Path.Combine(directory, "imported-checklist.json");
        _legacyPath = Path.Combine(directory, "checklist.json");
        _activePath = _temporaryPath;
    }

    public async Task<List<AppRecord>> LoadAsync()
    {
        _activePath = File.Exists(_importedPath) ? _importedPath : File.Exists(_legacyPath) ? _legacyPath : _temporaryPath;
        UsesTemporaryRecord = _activePath != _importedPath;
        if (!File.Exists(_activePath)) return [];
        await using var stream = File.OpenRead(_activePath);
        return await JsonSerializer.DeserializeAsync<List<AppRecord>>(stream, Options) ?? [];
    }

    public async Task SaveAsync(IEnumerable<AppRecord> records)
    {
        await using var stream = File.Create(_activePath);
        await JsonSerializer.SerializeAsync(stream, records, Options);
    }

    public async Task SaveImportedAsync(IEnumerable<AppRecord> records)
    {
        _activePath = _importedPath;
        UsesTemporaryRecord = false;
        await SaveAsync(records);
    }
}
