namespace ReinstallChecklist.Models;

public sealed class AppRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Category { get; set; } = "一般";
    public string Platforms { get; set; } = "Windows, macOS";
    public bool IsInstalled { get; set; }
    public bool IsPaid { get; set; }
    public string LicenseKey { get; set; } = "";
    public string OfficialWebsite { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTimeOffset? InstalledAt { get; set; }
    public string InstalledMatch { get; set; } = "";
    public string InstalledSource { get; set; } = "";

    public string PlatformLabel => Platforms;
    public string StateLabel => IsInstalled ? "已完成" : "待安裝";
    public bool HasInstalledMatch => !string.IsNullOrWhiteSpace(InstalledMatch);
    public bool HasInstalledSource => !string.IsNullOrWhiteSpace(InstalledSource);
    public bool HasOfficialWebsite => !string.IsNullOrWhiteSpace(OfficialWebsite);
}
