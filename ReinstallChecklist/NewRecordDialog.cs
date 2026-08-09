using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ReinstallChecklist.Models;

namespace ReinstallChecklist;

public sealed class NewRecordDialog : Window
{
    private readonly TextBox _name = new() { Watermark = "例如：Visual Studio Code" };
    private readonly ComboBox _category = new()
    {
        ItemsSource = new[] { "未分類" },
        IsEditable = true,
        Text = "未分類"
    };
    private readonly ComboBox _platforms = new()
    {
        ItemsSource = new[] { "Windows", "macOS", "Windows, macOS" },
        SelectedIndex = 2
    };
    private readonly CheckBox _paid = new() { Content = "付費軟體／需要授權" };
    private readonly TextBox _license = new() { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 54, Watermark = "序號、帳號或授權位置" };
    private readonly TextBox _notes = new() { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 54, Watermark = "選填" };
    private readonly TextBlock _error = new() { Foreground = Brushes.Firebrick, IsVisible = false };

    public AppRecord? Record { get; private set; }

    public NewRecordDialog()
    {
        Title = "新增項目";
        Width = 470; Height = 590; MinWidth = 420; CanResize = false;
        var content = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 8 };
        content.Children.Add(new TextBlock { Text = "建立新的安裝檢查項目", FontSize = 20, FontWeight = FontWeight.SemiBold });
        content.Children.Add(new TextBlock { Text = "項目會先寫入目前使用中的記錄檔。", Foreground = Brushes.DimGray, Margin = new Avalonia.Thickness(0, 0, 0, 7) });
        AddField(content, "軟體名稱", _name);
        AddField(content, "分類", _category);
        AddField(content, "平台", _platforms);
        content.Children.Add(_paid);
        AddField(content, "序號或授權資訊", _license);
        AddField(content, "備註", _notes);
        content.Children.Add(_error);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8, Margin = new Avalonia.Thickness(0, 8, 0, 0) };
        var cancel = new Button { Content = "取消" }; cancel.Click += (_, _) => Close();
        var submit = new Button { Content = "新增項目" }; submit.Click += (_, _) => Submit();
        buttons.Children.Add(cancel); buttons.Children.Add(submit); content.Children.Add(buttons);
        Content = new ScrollViewer { Content = content };
        Opened += (_, _) => _name.Focus();
    }

    private static void AddField(Panel panel, string label, Control input)
    {
        panel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 5, 0, 0) });
        panel.Children.Add(input);
    }

    private void Submit()
    {
        var name = _name.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            _error.Text = "請輸入軟體名稱後再新增。";
            _error.IsVisible = true;
            _name.Focus();
            return;
        }
        Record = new AppRecord
        {
            Name = name,
            Category = string.IsNullOrWhiteSpace(_category.Text) ? "未分類" : _category.Text.Trim(),
            Platforms = _platforms.SelectedItem as string ?? "Windows, macOS",
            IsPaid = _paid.IsChecked == true,
            LicenseKey = _license.Text?.Trim() ?? "",
            Notes = _notes.Text?.Trim() ?? ""
        };
        Close();
    }
}
