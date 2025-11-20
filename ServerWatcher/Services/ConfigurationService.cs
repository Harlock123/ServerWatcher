using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ServerWatcher.Models;

namespace ServerWatcher.Services;

public class ConfigurationService
{
    private const string ConfigFileName = "serverconfig.json";
    private readonly PasswordEncryptionService _encryptionService;

    public ConfigurationService()
    {
        _encryptionService = new PasswordEncryptionService();
    }

    public async Task<AppConfiguration> LoadConfigurationAsync()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file not found: {configPath}");
            }

            var json = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
                return new AppConfiguration();

            // Decrypt passwords in credential groups
            foreach (var credentialGroup in config.CredentialGroups)
            {
                if (!string.IsNullOrEmpty(credentialGroup.Password))
                {
                    try
                    {
                        credentialGroup.Password = _encryptionService.Decrypt(credentialGroup.Password);
                    }
                    catch
                    {
                        // If decryption fails, assume it's already in plain text
                        // This allows backward compatibility with existing configs
                    }
                }
            }

            return config;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load configuration: {ex.Message}", ex);
        }
    }

    public async Task SaveConfigurationAsync(AppConfiguration config)
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(configPath, json);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save configuration: {ex.Message}", ex);
        }
    }
}
