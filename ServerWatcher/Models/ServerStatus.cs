using CommunityToolkit.Mvvm.ComponentModel;

namespace ServerWatcher.Models;

public partial class ServerStatus : ObservableObject
{
    public string ServerName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? WebSite { get; set; }
    public string IpAddress { get; set; } = string.Empty;

    [ObservableProperty]
    private ConnectionStatus status = ConnectionStatus.Unknown;

    [ObservableProperty]
    private string statusMessage = "Not tested";

    [ObservableProperty]
    private ConnectionStatus webSiteStatus = ConnectionStatus.Unknown;

    [ObservableProperty]
    private string webSiteStatusMessage = "Not tested";
}

public enum ConnectionStatus
{
    Unknown,
    Connecting,
    Success,
    Failed
}
