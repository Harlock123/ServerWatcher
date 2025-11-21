using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Renci.SshNet;
using ServerWatcher.Models;

namespace ServerWatcher.Services;

public class SshConnectionService
{
    private const int DefaultSshPort = 22;
    private const int ConnectionTimeoutSeconds = 10;

    public async Task<(bool Success, string Message)> TestConnectionAsync(
        string serverName,
        string username,
        string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var client = new SshClient(serverName, DefaultSshPort, username, password)
                {
                    ConnectionInfo =
                    {
                        Timeout = TimeSpan.FromSeconds(ConnectionTimeoutSeconds)
                    }
                };

                client.Connect();

                if (client.IsConnected)
                {
                    client.Disconnect();
                    return (true, "Connected successfully");
                }

                return (false, "Failed to establish connection");
            }
            catch (Renci.SshNet.Common.SshAuthenticationException)
            {
                return (false, "Authentication failed - invalid credentials");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                return (false, $"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        });
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(
        ServerConfig server,
        CredentialGroup credentials)
    {
        return await TestConnectionAsync(server.ServerName, credentials.LoginName, credentials.Password);
    }

    public async Task<(bool Success, List<RemoteFileInfo>? Files, string Message)> ListFilesAsync(
        ServerConfig server,
        CredentialGroup credentials)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(server.Path))
                {
                    return (false, null, "No path configured for this server");
                }

                using var client = new SftpClient(server.ServerName, DefaultSshPort, credentials.LoginName, credentials.Password)
                {
                    ConnectionInfo =
                    {
                        Timeout = TimeSpan.FromSeconds(ConnectionTimeoutSeconds)
                    }
                };

                client.Connect();

                if (!client.IsConnected)
                {
                    return (false, null, "Failed to establish SFTP connection");
                }

                var files = new List<RemoteFileInfo>();

                var sftpFiles = client.ListDirectory(server.Path);

                foreach (var file in sftpFiles.Where(f => f.Name != "." && f.Name != ".."))
                {
                    files.Add(new RemoteFileInfo
                    {
                        Name = file.Name,
                        ModifiedTime = file.LastWriteTime,
                        Size = file.Length,
                        IsDirectory = file.IsDirectory
                    });
                }

                client.Disconnect();

                return (true, files.OrderByDescending(f => f.IsDirectory).ThenBy(f => f.Name).ToList(),
                    $"Successfully listed {files.Count} items");
            }
            catch (Renci.SshNet.Common.SshAuthenticationException)
            {
                return (false, null, "Authentication failed - invalid credentials");
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException)
            {
                return (false, null, $"Path not found: {server.Path}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                return (false, null, $"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error listing files: {ex.Message}");
            }
        });
    }
}
