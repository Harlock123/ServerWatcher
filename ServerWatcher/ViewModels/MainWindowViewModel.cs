using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerWatcher.Models;
using ServerWatcher.Services;
using ServerWatcher.Views;

namespace ServerWatcher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ConfigurationService _configService;
    private readonly SshConnectionService _sshService;
    private AppConfiguration? _configuration;
    private Window? _mainWindow;

    [ObservableProperty]
    private ObservableCollection<ServerStatus> _servers = new();

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private double _fontSize = 14;

    public MainWindowViewModel()
    {
        _configService = new ConfigurationService();
        _sshService = new SshConnectionService();

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            StatusText = "Loading configuration...";
            _configuration = await _configService.LoadConfigurationAsync();

            Servers.Clear();
            foreach (var server in _configuration.Servers)
            {
                Servers.Add(new ServerStatus
                {
                    ServerName = server.ServerName,
                    Group = server.Group,
                    Description = server.Description,
                    Status = ConnectionStatus.Unknown,
                    StatusMessage = "Not tested"
                });
            }

            StatusText = $"Loaded {Servers.Count} servers from configuration";
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading configuration: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task TestAllConnections()
    {
        if (_configuration == null || IsTesting)
            return;

        IsTesting = true;
        StatusText = "Testing connections...";

        var testedCount = 0;
        var successCount = 0;

        foreach (var serverStatus in Servers)
        {
            serverStatus.Status = ConnectionStatus.Connecting;
            serverStatus.StatusMessage = "Connecting...";

            var serverConfig = _configuration.Servers.FirstOrDefault(s => s.ServerName == serverStatus.ServerName);
            if (serverConfig == null)
            {
                serverStatus.Status = ConnectionStatus.Failed;
                serverStatus.StatusMessage = "Server configuration not found";
                continue;
            }

            var credentialGroup = _configuration.CredentialGroups.FirstOrDefault(g => g.GroupName == serverConfig.Group);
            if (credentialGroup == null)
            {
                serverStatus.Status = ConnectionStatus.Failed;
                serverStatus.StatusMessage = $"Credential group '{serverConfig.Group}' not found";
                continue;
            }

            var (success, message) = await _sshService.TestConnectionAsync(serverConfig, credentialGroup);

            serverStatus.Status = success ? ConnectionStatus.Success : ConnectionStatus.Failed;
            serverStatus.StatusMessage = message;

            testedCount++;
            if (success)
                successCount++;

            StatusText = $"Testing... ({testedCount}/{Servers.Count})";
        }

        StatusText = $"Complete: {successCount}/{testedCount} servers connected successfully";
        IsTesting = false;
    }

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }

    [RelayCommand]
    private async Task OpenPasswordEncryption()
    {
        if (_mainWindow == null)
            return;

        var dialog = new PasswordEncryptionWindow();
        await dialog.ShowDialog(_mainWindow);
    }
}
