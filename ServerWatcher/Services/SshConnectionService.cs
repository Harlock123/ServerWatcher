using System;
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
}
