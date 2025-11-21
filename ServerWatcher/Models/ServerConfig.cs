namespace ServerWatcher.Models;

public class ServerConfig
{
    public string ServerName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? WebSite { get; set; }
}
