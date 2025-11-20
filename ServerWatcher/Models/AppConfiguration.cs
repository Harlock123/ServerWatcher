using System.Collections.Generic;

namespace ServerWatcher.Models;

public class AppConfiguration
{
    public List<ServerConfig> Servers { get; set; } = new();
    public List<CredentialGroup> CredentialGroups { get; set; } = new();
}
