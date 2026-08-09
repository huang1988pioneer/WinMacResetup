using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ReinstallChecklist.Models;
using ReinstallChecklist.Services;

namespace ReinstallChecklist;

public partial class MainWindow : Window
{
    private readonly DataStore _store = new();
    private readonly ObservableCollection<AppRecord> _records = [];
    private AppRecord? _selected;
    private bool _controlsReady;

    public MainWindow()
    {
        InitializeComponent();
        _controlsReady = true;
        Opened += async (_, _) =>
        {
            foreach (var record in await _store.LoadAsync()) _records.Add(record);
            Refresh();
            StatusText.Text = _store.UsesTemporaryRecord
                ? "目前使用臨時記錄檔；匯入加密備份後會切換為正式記錄。"
                : "已使用匯入記錄檔；序號請持續透過加密備份保存。";
        };
    }

    private void Refresh()
    {
        var query = SearchBox?.Text?.Trim() ?? "";
        var filter = (FilterBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部項目";
        IEnumerable<AppRecord> visible = _records;
        visible = filter switch
        {
            "尚未安裝" => visible.Where(x => !x.IsInstalled),
            "已安裝" => visible.Where(x => x.IsInstalled),
            "付費軟體" => visible.Where(x => x.IsPaid),
            _ => visible
        };
        if (!string.IsNullOrWhiteSpace(query))
            visible = visible.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || x.Category.Contains(query, StringComparison.OrdinalIgnoreCase));
        RecordsList.ItemsSource = visible.OrderBy(x => x.IsInstalled).ThenBy(x => x.Category).ThenBy(x => x.Name).ToList();
        var done = _records.Count(x => x.IsInstalled);
        ProgressText.Text = $"{done} / {_records.Count} 已完成";
        ProgressBar.Value = _records.Count == 0 ? 0 : (double)done / _records.Count;
        ProgressHint.Text = _records.Count == done && _records.Count > 0 ? "清單已全部完成。" : $"還有 {_records.Count - done} 項待處理";
    }

    private async Task PersistAsync()
    {
        await _store.SaveAsync(_records);
        Refresh();
    }

    private async void InstalledChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: AppRecord record } box)
        {
            record.IsInstalled = box.IsChecked == true;
            record.InstalledAt = record.IsInstalled ? DateTimeOffset.Now : null;
            await PersistAsync();
        }
    }

    private void FilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_controlsReady) Refresh();
    }

    private void SearchChanged(object? sender, TextChangedEventArgs e)
    {
        if (_controlsReady) Refresh();
    }

    private async void RecordSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selected = RecordsList.SelectedItem as AppRecord;
        DetailPanel.IsVisible = _selected is not null;
        if (_selected is null) return;
        DetailHint.Text = _selected.IsInstalled ? "此項目已標記為完成。" : "完成安裝後，勾選左側方框。";
        NameInput.Text = _selected.Name; CategoryInput.Text = string.IsNullOrWhiteSpace(_selected.Category) ? "未分類" : _selected.Category;
        PlatformInput.SelectedIndex = _selected.Platforms switch { "Windows" => 0, "macOS" => 1, _ => 2 };
        PaidInput.IsChecked = _selected.IsPaid; LicenseInput.Text = _selected.LicenseKey; WebsiteInput.Text = _selected.OfficialWebsite; NotesInput.Text = _selected.Notes;
        if (!string.IsNullOrWhiteSpace(_selected.LicenseKey))
        {
            try
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard is null)
                {
                    StatusText.Text = "找不到系統剪貼簿，無法複製序號。";
                    return;
                }
                await clipboard.SetTextAsync(_selected.LicenseKey);
                StatusText.Text = $"已複製「{_selected.Name}」的序號至剪貼簿。";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"無法複製序號：{ex.Message}";
            }
        }
    }

    private async void AddClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new NewRecordDialog();
        await dialog.ShowDialog<string?>(this);
        if (dialog.Record is not { } record) return;
        _records.Add(record);
        await PersistAsync();
        RecordsList.SelectedItem = record;
        StatusText.Text = "已新增項目。";
    }

    private async void SaveDetailClick(object? sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) { StatusText.Text = "軟體名稱不能空白。"; return; }
        _selected.Name = name; _selected.Category = string.IsNullOrWhiteSpace(CategoryInput.Text) ? "未分類" : CategoryInput.Text.Trim();
        _selected.Platforms = PlatformInput.SelectedItem is ComboBoxItem item
            ? item.Content?.ToString() ?? "Windows, macOS"
            : "Windows, macOS";
        if (!TryNormalizeWebsite(WebsiteInput.Text, out var website))
        {
            StatusText.Text = "官方網站請填入有效的 http:// 或 https:// 網址。";
            WebsiteInput.Focus();
            return;
        }
        _selected.IsPaid = PaidInput.IsChecked == true; _selected.LicenseKey = LicenseInput.Text?.Trim() ?? ""; _selected.OfficialWebsite = website; _selected.Notes = NotesInput.Text?.Trim() ?? "";
        await PersistAsync(); StatusText.Text = "項目已儲存。";
    }

    private void OpenWebsiteClick(object? sender, RoutedEventArgs e)
    {
        var website = sender switch
        {
            Button { Tag: string value } => value,
            _ => WebsiteInput.Text
        };
        if (!TryNormalizeWebsite(website, out var url))
        {
            StatusText.Text = "官方網站請填入有效的 http:// 或 https:// 網址。";
            return;
        }
        if (string.IsNullOrWhiteSpace(url))
        {
            StatusText.Text = "此項目尚未設定官方網站。";
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusText.Text = $"無法開啟官方網站：{ex.Message}";
        }
    }

    private static bool TryNormalizeWebsite(string? value, out string website)
    {
        website = value?.Trim() ?? "";
        if (website.Length == 0) return true;
        if (!Uri.TryCreate(website, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) return false;
        website = uri.AbsoluteUri;
        return true;
    }

    private async void DeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _records.Remove(_selected); _selected = null; DetailPanel.IsVisible = false; DetailHint.Text = "選擇清單中的項目來編輯。";
        await PersistAsync(); StatusText.Text = "項目已刪除。";
    }

    private async void ScanInstalledClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "正在讀取目前系統的已安裝程式…";
            var installed = await InstalledAppScanner.GetInstalledNamesAsync();
            var marked = 0;
            foreach (var record in _records)
            {
                var match = InstalledAppScanner.FindMatch(record.Name, installed);
                if (match is null) continue;
                record.InstalledMatch = match;
                if (!record.IsInstalled) { record.IsInstalled = true; record.InstalledAt = DateTimeOffset.Now; marked++; }
            }
            var platform = OperatingSystem.IsMacOS() ? "macOS" : OperatingSystem.IsWindows() ? "Windows" : "Windows, macOS";
            var added = 0;
            foreach (var name in installed.OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase))
            {
                if (_records.Any(record => InstalledAppScanner.FindMatch(record.Name, [name]) is not null)) continue;
                _records.Add(new AppRecord
                {
                    Name = name,
                    Category = "系統掃描",
                    Platforms = platform,
                    IsInstalled = true,
                    InstalledAt = DateTimeOffset.Now,
                    InstalledMatch = name
                });
                added++;
            }
            await PersistAsync();
            StatusText.Text = $"系統掃描完成：標記 {marked} 個既有項目，新增 {added} 個已安裝軟體。";
        }
        catch (Exception ex) { StatusText.Text = $"掃描失敗：{ex.Message}"; }
    }

    private async void ExportClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new PasswordDialog(isExport: true); await dialog.ShowDialog<string?>(this);
        if (dialog.Password is not { } pass) return;
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "儲存加密備份", SuggestedFileName = $"reinstall-checklist-{DateTime.Now:yyyyMMdd}.resetup",
            FileTypeChoices = [new FilePickerFileType("重灌清單加密備份") { Patterns = ["*.resetup"] }]
        });
        if (file is null) return;
        try { await BackupService.ExportAsync(file.Path.LocalPath, _records, pass); StatusText.Text = "已建立 AES-256-GCM 加密備份。請妥善保管密碼。"; }
        catch (Exception ex) { StatusText.Text = $"匯出失敗：{ex.Message}"; }
    }

    private async void ImportClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "選取加密備份", AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("重灌清單加密備份") { Patterns = ["*.resetup"] }]
        });
        if (files.Count == 0) return;
        var dialog = new PasswordDialog(isExport: false); await dialog.ShowDialog<string?>(this);
        if (dialog.Password is not { } pass) return;
        try
        {
            var imported = await BackupService.ImportAsync(files[0].Path.LocalPath, pass);
            _records.Clear(); foreach (var item in imported) _records.Add(item);
            _selected = null; DetailPanel.IsVisible = false; await _store.SaveImportedAsync(_records); Refresh(); StatusText.Text = $"已安全匯入 {imported.Count} 個項目，後續變更將寫入正式記錄檔。";
        }
        catch (Exception ex) { StatusText.Text = $"匯入失敗：{ex.Message}"; }
    }
}
