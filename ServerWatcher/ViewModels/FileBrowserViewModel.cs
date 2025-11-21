using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerWatcher.Models;
using ServerWatcher.Services;

namespace ServerWatcher.ViewModels;

public partial class FileBrowserViewModel : ViewModelBase
{
    private readonly SshConnectionService _sshService;
    private Window? _window;

    [ObservableProperty]
    private string _serverName = string.Empty;

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _statusText = "Loading files...";

    [ObservableProperty]
    private ObservableCollection<RemoteFileInfo> _files = new();

    public FileBrowserViewModel()
    {
        _sshService = new SshConnectionService();
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    public async Task LoadFilesAsync(ServerConfig server, CredentialGroup credentials)
    {
        ServerName = server.ServerName;
        Path = server.Path;
        StatusText = "Loading files...";

        var (success, files, message) = await _sshService.ListFilesAsync(server, credentials);

        if (success && files != null)
        {
            Files.Clear();
            foreach (var file in files)
            {
                Files.Add(file);
            }
            StatusText = $"Found {Files.Count} items";
        }
        else
        {
            StatusText = $"Error: {message}";
        }
    }

    [RelayCommand]
    private void Close()
    {
        _window?.Close();
    }
}
