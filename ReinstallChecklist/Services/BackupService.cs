using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ReinstallChecklist.Models;

namespace ReinstallChecklist.Services;

public static class BackupService
{
    private const int Iterations = 210_000;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task ExportAsync(string path, IEnumerable<AppRecord> records, string password)
    {
        RequirePassword(password);
        var salt = RandomNumberGenerator.GetBytes(16);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintext = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(records, JsonOptions));
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        using var aes = new AesGcm(key.GetBytes(32), tagSizeInBytes: 16);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        CryptographicOperations.ZeroMemory(plaintext);
        var envelope = new BackupEnvelope
        {
            Salt = Convert.ToBase64String(salt), Nonce = Convert.ToBase64String(nonce),
            Tag = Convert.ToBase64String(tag), Ciphertext = Convert.ToBase64String(ciphertext)
        };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(envelope, JsonOptions));
    }

    public static async Task<List<AppRecord>> ImportAsync(string path, string password)
    {
        RequirePassword(password);
        var envelope = JsonSerializer.Deserialize<BackupEnvelope>(await File.ReadAllTextAsync(path), JsonOptions)
            ?? throw new InvalidDataException("備份檔格式無效。");
        if (envelope.Version != 1 || envelope.Algorithm != "AES-256-GCM/PBKDF2-SHA256")
            throw new InvalidDataException("不支援的備份檔版本。");
        try
        {
            var salt = Convert.FromBase64String(envelope.Salt);
            var nonce = Convert.FromBase64String(envelope.Nonce);
            var tag = Convert.FromBase64String(envelope.Tag);
            var ciphertext = Convert.FromBase64String(envelope.Ciphertext);
            var plaintext = new byte[ciphertext.Length];
            using var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            using var aes = new AesGcm(key.GetBytes(32), tagSizeInBytes: 16);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            try { return JsonSerializer.Deserialize<List<AppRecord>>(plaintext, JsonOptions) ?? []; }
            finally { CryptographicOperations.ZeroMemory(plaintext); }
        }
        catch (CryptographicException)
        {
            throw new InvalidDataException("無法解密備份檔：密碼不正確，或檔案已損毀。");
        }
        catch (FormatException)
        {
            throw new InvalidDataException("備份檔內容已損毀。");
        }
    }

    private static void RequirePassword(string password)
    {
        if (password.Length < 8) throw new ArgumentException("密碼至少需要 8 個字元。");
    }
}
