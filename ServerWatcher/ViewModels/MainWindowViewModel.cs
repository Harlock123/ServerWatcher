using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    private readonly WebsiteCheckService _websiteService;
    private AppConfiguration? _configuration;
    private Window? _mainWindow;

    [ObservableProperty]
    private ObservableCollection<ServerStatus> _servers = new();

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private double _fontSize = 12;

    public MainWindowViewModel()
    {
        _configService = new ConfigurationService();
        _sshService = new SshConnectionService();
        _websiteService = new WebsiteCheckService();

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
                var ipAddress = await ResolveIpAddressAsync(server.ServerName);

                Servers.Add(new ServerStatus
                {
                    ServerName = server.ServerName,
                    Group = server.Group,
                    Description = server.Description,
                    Path = server.Path,
                    WebSite = server.WebSite,
                    IpAddress = ipAddress,
                    Status = ConnectionStatus.Unknown,
                    StatusMessage = "Not tested",
                    WebSiteStatus = string.IsNullOrWhiteSpace(server.WebSite) ? ConnectionStatus.Unknown : ConnectionStatus.Unknown,
                    WebSiteStatusMessage = string.IsNullOrWhiteSpace(server.WebSite) ? "No website configured" : "Not tested"
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

            // Test website if configured
            if (!string.IsNullOrWhiteSpace(serverStatus.WebSite))
            {
                serverStatus.WebSiteStatus = ConnectionStatus.Connecting;
                serverStatus.WebSiteStatusMessage = "Checking...";

                var (webSuccess, webMessage) = await _websiteService.CheckWebsiteAsync(serverStatus.WebSite);
                serverStatus.WebSiteStatus = webSuccess ? ConnectionStatus.Success : ConnectionStatus.Failed;
                serverStatus.WebSiteStatusMessage = webMessage;
            }

            testedCount++;
            if (success)
                successCount++;

            StatusText = $"Testing... ({testedCount}/{Servers.Count})";
        }

        StatusText = $"Complete: {successCount}/{testedCount} servers connected successfully";
        IsTesting = false;
    }

    [RelayCommand]
    private async Task TestSingleConnection(ServerStatus serverStatus)
    {
        if (_configuration == null || serverStatus == null)
            return;

        StatusText = $"Testing {serverStatus.ServerName}...";

        serverStatus.Status = ConnectionStatus.Connecting;
        serverStatus.StatusMessage = "Connecting...";

        var serverConfig = _configuration.Servers.FirstOrDefault(s => s.ServerName == serverStatus.ServerName);
        if (serverConfig == null)
        {
            serverStatus.Status = ConnectionStatus.Failed;
            serverStatus.StatusMessage = "Server configuration not found";
            StatusText = $"Failed to test {serverStatus.ServerName}: configuration not found";
            return;
        }

        var credentialGroup = _configuration.CredentialGroups.FirstOrDefault(g => g.GroupName == serverConfig.Group);
        if (credentialGroup == null)
        {
            serverStatus.Status = ConnectionStatus.Failed;
            serverStatus.StatusMessage = $"Credential group '{serverConfig.Group}' not found";
            StatusText = $"Failed to test {serverStatus.ServerName}: credential group not found";
            return;
        }

        var (success, message) = await _sshService.TestConnectionAsync(serverConfig, credentialGroup);

        serverStatus.Status = success ? ConnectionStatus.Success : ConnectionStatus.Failed;
        serverStatus.StatusMessage = message;

        // Test website if configured
        if (!string.IsNullOrWhiteSpace(serverStatus.WebSite))
        {
            serverStatus.WebSiteStatus = ConnectionStatus.Connecting;
            serverStatus.WebSiteStatusMessage = "Checking...";

            var (webSuccess, webMessage) = await _websiteService.CheckWebsiteAsync(serverStatus.WebSite);
            serverStatus.WebSiteStatus = webSuccess ? ConnectionStatus.Success : ConnectionStatus.Failed;
            serverStatus.WebSiteStatusMessage = webMessage;
        }

        StatusText = $"Test complete: {serverStatus.ServerName} - {(success ? "Success" : "Failed")}";
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

    [RelayCommand]
    private async Task OpenFileBrowser(ServerStatus serverStatus)
    {
        if (_mainWindow == null || _configuration == null)
            return;

        var serverConfig = _configuration.Servers.FirstOrDefault(s => s.ServerName == serverStatus.ServerName);
        if (serverConfig == null)
        {
            StatusText = "Server configuration not found";
            return;
        }

        if (string.IsNullOrWhiteSpace(serverConfig.Path))
        {
            StatusText = $"No path configured for {serverStatus.ServerName}";
            return;
        }

        var credentialGroup = _configuration.CredentialGroups.FirstOrDefault(g => g.GroupName == serverConfig.Group);
        if (credentialGroup == null)
        {
            StatusText = $"Credential group '{serverConfig.Group}' not found";
            return;
        }

        var dialog = new FileBrowserWindow
        {
            DataContext = new FileBrowserViewModel()
        };

        var viewModel = (FileBrowserViewModel)dialog.DataContext;
        viewModel.SetWindow(dialog);

        dialog.Show();

        await viewModel.LoadFilesAsync(serverConfig, credentialGroup);
    }

    [RelayCommand]
    private void OpenWebsite(ServerStatus serverStatus)
    {
        if (string.IsNullOrWhiteSpace(serverStatus.WebSite))
            return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = serverStatus.WebSite,
                UseShellExecute = true
            };
            Process.Start(psi);
            StatusText = $"Opening {serverStatus.WebSite} in browser...";
        }
        catch (Exception ex)
        {
            StatusText = $"Error opening website: {ex.Message}";
        }
    }

    private async Task<string> ResolveIpAddressAsync(string hostname)
    {
        try
        {
            // If it's already an IP address, return empty string
            if (IPAddress.TryParse(hostname, out _))
            {
                return string.Empty;
            }

            // Try to resolve hostname to IP address
            var addresses = await Dns.GetHostAddressesAsync(hostname);
            if (addresses.Length > 0)
            {
                // Return the first IPv4 address if available, otherwise first address
                var ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                return ipv4?.ToString() ?? addresses[0].ToString();
            }

            return string.Empty;
        }
        catch
        {
            // If resolution fails, return empty string
            return string.Empty;
        }
    }
}
