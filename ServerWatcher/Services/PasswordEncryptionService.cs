using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ServerWatcher.Services;

public class PasswordEncryptionService
{
    // WARNING: In production, store this key securely (e.g., Azure Key Vault, Windows Credential Manager)
    // This is a sample key for demonstration purposes
    // Using SHA256 to ensure exactly 32 bytes for AES-256 key
    private static readonly byte[] Key = DeriveKey("ServerWatcher2024SecurePassphrase");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("InitVector16byte"); // Exactly 16 bytes for AES IV

    private static byte[] DeriveKey(string passphrase)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            var encrypted = msEncrypt.ToArray();
            return Convert.ToBase64String(encrypted);
        }
        catch (Exception ex)
        {
            throw new Exception($"Encryption failed: {ex.Message}", ex);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var msDecrypt = new MemoryStream(buffer);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw new Exception($"Decryption failed: {ex.Message}", ex);
        }
    }

    public bool IsEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        try
        {
            // Try to decode as Base64 - encrypted passwords are Base64 encoded
            var buffer = Convert.FromBase64String(value);
            return buffer.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}
