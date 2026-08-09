using Avalonia.Controls;
using Avalonia.Layout;

namespace ReinstallChecklist;

public sealed class PasswordDialog : Window
{
    private readonly TextBox _password = new() { PasswordChar = '●', Watermark = "至少 8 個字元" };
    private readonly TextBox _confirmation = new() { PasswordChar = '●', Watermark = "再次輸入相同的加密密碼" };
    private readonly TextBlock _error = new() { Foreground = Avalonia.Media.Brushes.Firebrick, IsVisible = false, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly bool _isExport;
    public string? Password { get; private set; }

    public PasswordDialog(bool isExport)
    {
        _isExport = isExport;
        Title = isExport ? "設定加密密碼" : "確認加密密碼";
        Width = 410; Height = isExport ? 310 : 230; CanResize = false;
        var panel = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 10 };
        panel.Children.Add(new TextBlock
        {
            Text = isExport
                ? "請設定加密密碼，並再次確認。密碼不會被保存在備份檔中。"
                : "請輸入此備份檔的加密密碼，並再次確認後才會解密。",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });
        panel.Children.Add(new TextBlock { Text = "加密密碼", FontWeight = Avalonia.Media.FontWeight.SemiBold });
        panel.Children.Add(_password);
        if (isExport)
        {
            panel.Children.Add(new TextBlock { Text = "確認加密密碼", FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 3, 0, 0) });
            panel.Children.Add(_confirmation);
        }
        panel.Children.Add(_error);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8, Margin = new Avalonia.Thickness(0, 10, 0, 0) };
        var cancel = new Button { Content = "取消" }; cancel.Click += (_, _) => Close();
        var ok = new Button { Content = isExport ? "確認並加密匯出" : "確認並解密匯入" }; ok.Click += (_, _) => Submit();
        buttons.Children.Add(cancel); buttons.Children.Add(ok); panel.Children.Add(buttons); Content = panel;
    }

    private void Submit()
    {
        if (_password.Text?.Length < 8) { ShowError("加密密碼至少需要 8 個字元。"); return; }
        if (_isExport && _password.Text != _confirmation.Text) { ShowError("兩次輸入的加密密碼不一致，請重新確認。"); return; }
        Password = _password.Text; Close();
    }

    private void ShowError(string message)
    {
        _error.Text = message;
        _error.IsVisible = true;
        (_isExport ? _confirmation : _password).Focus();
    }
}
