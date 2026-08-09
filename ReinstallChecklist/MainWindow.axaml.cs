using System.Collections.ObjectModel;
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

    public MainWindow()
    {
        InitializeComponent();
        Opened += async (_, _) =>
        {
            foreach (var record in await _store.LoadAsync()) _records.Add(record);
            Refresh();
            StatusText.Text = "本機清單會自動儲存。序號請透過加密備份保存。";
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

    private void FilterChanged(object? sender, SelectionChangedEventArgs e) => Refresh();
    private void SearchChanged(object? sender, TextChangedEventArgs e) => Refresh();

    private void RecordSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selected = RecordsList.SelectedItem as AppRecord;
        DetailPanel.IsEnabled = _selected is not null;
        if (_selected is null) return;
        DetailHint.Text = _selected.IsInstalled ? "此項目已標記為完成。" : "完成安裝後，勾選左側方框。";
        NameInput.Text = _selected.Name; CategoryInput.Text = _selected.Category; PlatformInput.Text = _selected.Platforms;
        PaidInput.IsChecked = _selected.IsPaid; LicenseInput.Text = _selected.LicenseKey; NotesInput.Text = _selected.Notes;
    }

    private void AddClick(object? sender, RoutedEventArgs e)
    {
        var record = new AppRecord { Name = "新軟體項目" };
        _records.Add(record); Refresh(); RecordsList.SelectedItem = record;
    }

    private async void SaveDetailClick(object? sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) { StatusText.Text = "軟體名稱不能空白。"; return; }
        _selected.Name = name; _selected.Category = CategoryInput.Text?.Trim() ?? "一般";
        _selected.Platforms = PlatformInput.Text?.Trim() ?? "Windows, macOS";
        _selected.IsPaid = PaidInput.IsChecked == true; _selected.LicenseKey = LicenseInput.Text?.Trim() ?? ""; _selected.Notes = NotesInput.Text?.Trim() ?? "";
        await PersistAsync(); StatusText.Text = "項目已儲存。";
    }

    private async void DeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _records.Remove(_selected); _selected = null; DetailPanel.IsEnabled = false; DetailHint.Text = "選擇清單中的項目來編輯。";
        await PersistAsync(); StatusText.Text = "項目已刪除。";
    }

    private async void ScanInstalledClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "正在讀取目前系統的已安裝程式…";
            var installed = await InstalledAppScanner.GetInstalledNamesAsync();
            var matches = 0;
            foreach (var record in _records)
            {
                var match = InstalledAppScanner.FindMatch(record.Name, installed);
                if (match is null) continue;
                record.InstalledMatch = match;
                if (!record.IsInstalled) { record.IsInstalled = true; record.InstalledAt = DateTimeOffset.Now; matches++; }
            }
            await PersistAsync();
            StatusText.Text = matches > 0 ? $"系統掃描完成，新增標記 {matches} 個項目。" : "系統掃描完成，沒有找到可新增標記的項目。";
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
            _selected = null; DetailPanel.IsEnabled = false; await PersistAsync(); StatusText.Text = $"已安全匯入 {imported.Count} 個項目。";
        }
        catch (Exception ex) { StatusText.Text = $"匯入失敗：{ex.Message}"; }
    }
}
