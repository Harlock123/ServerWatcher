using System;

namespace ServerWatcher.Models;

public class RemoteFileInfo
{
    public string Name { get; set; } = string.Empty;
    public DateTime ModifiedTime { get; set; }
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
}
