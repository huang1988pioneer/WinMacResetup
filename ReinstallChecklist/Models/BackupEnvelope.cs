namespace ReinstallChecklist.Models;

public sealed class BackupEnvelope
{
    public int Version { get; set; } = 1;
    public string Algorithm { get; set; } = "AES-256-GCM/PBKDF2-SHA256";
    public string Salt { get; set; } = "";
    public string Nonce { get; set; } = "";
    public string Tag { get; set; } = "";
    public string Ciphertext { get; set; } = "";
}
