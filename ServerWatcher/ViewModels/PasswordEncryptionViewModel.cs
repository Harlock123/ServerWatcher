using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerWatcher.Services;

namespace ServerWatcher.ViewModels;

public partial class PasswordEncryptionViewModel : ViewModelBase
{
    private readonly PasswordEncryptionService _encryptionService;

    [ObservableProperty]
    private string _plainTextPassword = string.Empty;

    [ObservableProperty]
    private string _encryptedPassword = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public PasswordEncryptionViewModel()
    {
        _encryptionService = new PasswordEncryptionService();
    }

    [RelayCommand]
    private void EncryptPassword()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PlainTextPassword))
            {
                StatusMessage = "Please enter a password to encrypt";
                EncryptedPassword = string.Empty;
                return;
            }

            EncryptedPassword = _encryptionService.Encrypt(PlainTextPassword);
            StatusMessage = "Password encrypted successfully! Copy the encrypted value to your config file.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            EncryptedPassword = string.Empty;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        PlainTextPassword = string.Empty;
        EncryptedPassword = string.Empty;
        StatusMessage = string.Empty;
    }
}
