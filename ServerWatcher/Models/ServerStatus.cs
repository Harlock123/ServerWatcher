using CommunityToolkit.Mvvm.ComponentModel;

namespace ServerWatcher.Models;

public partial class ServerStatus : ObservableObject
{
    public string ServerName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [ObservableProperty]
    private ConnectionStatus status = ConnectionStatus.Unknown;

    [ObservableProperty]
    private string statusMessage = "Not tested";
}

public enum ConnectionStatus
{
    Unknown,
    Connecting,
    Success,
    Failed
}
